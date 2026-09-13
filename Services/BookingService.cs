using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class BookingService : IBookingService
{
    private readonly IDataStore _dataStore;

    public BookingService(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime pickup, DateTime returnTime, int? excludeBookingId = null)
    {
        lock (_dataStore.Vehicles)
        {
            var vehicle = _dataStore.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null || vehicle.Status == VehicleStatus.UnderMaintenance)
            {
                return Task.FromResult(false);
            }
        }

        lock (_dataStore.Bookings)
        {
            // Simple overlap algorithm: (pickup < existing.ReturnDateTime) && (returnTime > existing.PickupDateTime)
            bool hasConflict = _dataStore.Bookings.Any(b =>
                b.VehicleId == vehicleId &&
                b.Status != BookingStatus.Cancelled &&
                (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value) &&
                pickup < b.ReturnDateTime &&
                returnTime > b.PickupDateTime);

            return Task.FromResult(!hasConflict);
        }
    }

    public Task<(bool Success, string Message, PriceBreakdownViewModel? Price)> CalculatePriceAsync(
        int vehicleId, RentalType rentalType, DateTime pickup, DateTime returnTime, string? promoCode = null)
    {
        // 1. Validate dates
        if (pickup < DateTime.UtcNow.AddMinutes(-5))
        {
            return Task.FromResult<(bool, string, PriceBreakdownViewModel?)>((false, "Pickup date and time cannot be in the past.", null));
        }

        if (returnTime <= pickup)
        {
            return Task.FromResult<(bool, string, PriceBreakdownViewModel?)>((false, "Return date and time must be after pickup date and time.", null));
        }

        Vehicle? vehicle;
        lock (_dataStore.Vehicles)
        {
            vehicle = _dataStore.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
        }

        if (vehicle == null)
        {
            return Task.FromResult<(bool, string, PriceBreakdownViewModel?)>((false, "Vehicle not found.", null));
        }

        // 2. Calculate duration and base rate
        decimal duration;
        decimal unitRate;
        decimal subtotal;

        if (rentalType == RentalType.Hourly)
        {
            var totalHours = (decimal)(returnTime - pickup).TotalHours;
            duration = Math.Max(1, Math.Ceiling(totalHours));
            unitRate = vehicle.HourlyRate;
            subtotal = duration * unitRate;
        }
        else
        {
            var totalDays = (decimal)(returnTime - pickup).TotalDays;
            duration = Math.Max(1, Math.Ceiling(totalDays));
            unitRate = vehicle.DailyRate;
            subtotal = duration * unitRate;
        }

        // 3. Simple Weekend dynamic surge (+15% if Saturday or Sunday)
        decimal multiplier = 1.0m;
        lock (_dataStore.PricingRules)
        {
            var weekendRule = _dataStore.PricingRules.FirstOrDefault(r =>
                r.IsActive &&
                (!r.DayOfWeek.HasValue || r.DayOfWeek.Value == pickup.DayOfWeek));

            if (weekendRule != null)
            {
                multiplier = weekendRule.Multiplier;
            }
        }

        decimal basePrice = Math.Round(subtotal * multiplier, 2);
        decimal discountAmount = 0;
        string? appliedCode = null;

        // 4. Promo code calculation (if provided)
        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            var code = promoCode.Trim().ToUpperInvariant();
            lock (_dataStore.PromoCodes)
            {
                var promo = _dataStore.PromoCodes.FirstOrDefault(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

                if (promo == null)
                {
                    return Task.FromResult<(bool, string, PriceBreakdownViewModel?)>((false, $"Promo code '{promoCode}' is invalid.", null));
                }

                if (!promo.IsActive || DateTime.UtcNow < promo.ValidFrom || DateTime.UtcNow > promo.ValidTo)
                {
                    return Task.FromResult<(bool, string, PriceBreakdownViewModel?)>((false, $"Promo code '{promoCode}' is expired or inactive.", null));
                }

                if (basePrice < promo.MinBookingAmount)
                {
                    return Task.FromResult<(bool, string, PriceBreakdownViewModel?)>((false, $"Promo code requires a minimum booking of Rs.{promo.MinBookingAmount:F2}.", null));
                }

                appliedCode = promo.Code;
                discountAmount = promo.DiscountType == DiscountType.Percentage
                    ? Math.Round(basePrice * (promo.DiscountValue / 100m), 2)
                    : promo.DiscountValue;

                discountAmount = Math.Min(discountAmount, basePrice);
            }
        }

        decimal totalAmount = Math.Max(0, basePrice - discountAmount);

        var price = new PriceBreakdownViewModel
        {
            RentalType = rentalType,
            Duration = duration,
            UnitRate = unitRate,
            BasePrice = basePrice,
            DynamicMultiplier = multiplier,
            AppliedPromoCode = appliedCode,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount
        };

        return Task.FromResult<(bool, string, PriceBreakdownViewModel?)>((true, "Price calculated successfully.", price));
    }

    public async Task<(bool Success, string Message, Booking? Booking)> CreateBookingAsync(BookingRequestViewModel request)
    {
        // 1. Availability check
        bool isAvailable = await IsVehicleAvailableAsync(request.VehicleId, request.PickupDateTime, request.ReturnDateTime);
        if (!isAvailable)
        {
            return (false, "This vehicle is not available for the requested time range.", null);
        }

        // 2. Price calculation
        var priceResult = await CalculatePriceAsync(request.VehicleId, request.RentalType, request.PickupDateTime, request.ReturnDateTime, request.PromoCode);
        if (!priceResult.Success || priceResult.Price == null)
        {
            return (false, priceResult.Message, null);
        }

        var price = priceResult.Price;
        User? user;
        Vehicle? vehicle;
        PromoCode? promo = null;

        lock (_dataStore.Users)
        {
            user = _dataStore.Users.FirstOrDefault(u => u.Id == request.UserId);
        }

        lock (_dataStore.Vehicles)
        {
            vehicle = _dataStore.Vehicles.FirstOrDefault(v => v.Id == request.VehicleId);
        }

        if (user == null || vehicle == null)
        {
            return (false, "User or Vehicle not found.", null);
        }

        if (!string.IsNullOrWhiteSpace(price.AppliedPromoCode))
        {
            lock (_dataStore.PromoCodes)
            {
                promo = _dataStore.PromoCodes.FirstOrDefault(p => p.Code == price.AppliedPromoCode);
            }
        }

        // 3. Create booking in Pending status (awaiting full payment)
        var booking = new Booking
        {
            Id = _dataStore.NextBookingId(),
            BookingReference = $"DE-{DateTime.UtcNow:yyyyMM}-{_dataStore.Bookings.Count + 1001}",
            UserId = user.Id,
            User = user,
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            RentalType = request.RentalType,
            PickupDateTime = request.PickupDateTime,
            ReturnDateTime = request.ReturnDateTime,
            PickupLocation = request.PickupLocation.Trim(),
            RentalDuration = price.Duration,
            BasePrice = price.BasePrice,
            DiscountAmount = price.DiscountAmount,
            TotalAmount = price.TotalAmount,
            PromoCodeId = promo?.Id,
            PromoCode = promo,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        lock (_dataStore.Bookings)
        {
            _dataStore.Bookings.Add(booking);
        }

        return (true, "Booking created. Please proceed to payment to confirm.", booking);
    }

    public Task<Booking?> GetBookingByIdAsync(int id)
    {
        lock (_dataStore.Bookings)
        {
            var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == id);
            return Task.FromResult(booking);
        }
    }

    public Task<List<Booking>> GetUserBookingsAsync(int userId)
    {
        lock (_dataStore.Bookings)
        {
            var list = _dataStore.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
            return Task.FromResult(list);
        }
    }

    public Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId)
    {
        lock (_dataStore.Bookings)
        {
            var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null)
            {
                return Task.FromResult((false, "Booking not found."));
            }

            if (booking.UserId != userId)
            {
                return Task.FromResult((false, "Unauthorized to cancel this booking."));
            }

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            {
                return Task.FromResult((false, $"Cannot cancel a booking that is already {booking.Status}."));
            }

            booking.Status = BookingStatus.Cancelled;
            return Task.FromResult((true, "Booking cancelled successfully."));
        }
    }
}
