using Microsoft.EntityFrameworkCore;
using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime pickup, DateTime returnTime, int? excludeBookingId = null)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
        if (vehicle == null || vehicle.Status == VehicleStatus.UnderMaintenance)
        {
            return false;
        }

        // Overlap algorithm: (pickup < existing.ReturnDateTime) && (returnTime > existing.PickupDateTime)
        bool hasConflict = await _context.Bookings.AnyAsync(b =>
            b.VehicleId == vehicleId &&
            b.Status != BookingStatus.Cancelled &&
            (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value) &&
            pickup < b.ReturnDateTime &&
            returnTime > b.PickupDateTime);

        return !hasConflict;
    }

    public async Task<(bool Success, string Message, PriceBreakdownViewModel? Price)> CalculatePriceAsync(
        int vehicleId, RentalType rentalType, DateTime pickup, DateTime returnTime, string? promoCode = null)
    {
        // 1. Validate dates
        if (pickup < DateTime.UtcNow.AddMinutes(-5))
        {
            return (false, "Pickup date and time cannot be in the past.", null);
        }

        if (returnTime <= pickup)
        {
            return (false, "Return date and time must be after pickup date and time.", null);
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
        if (vehicle == null)
        {
            return (false, "Vehicle not found.", null);
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
        var weekendRule = await _context.PricingRules.FirstOrDefaultAsync(r =>
            r.IsActive &&
            (!r.DayOfWeek.HasValue || r.DayOfWeek.Value == pickup.DayOfWeek));

        if (weekendRule != null)
        {
            multiplier = weekendRule.Multiplier;
        }

        decimal basePrice = Math.Round(subtotal * multiplier, 2);
        decimal discountAmount = 0;
        string? appliedCode = null;

        // 4. Promo code calculation (if provided)
        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            var code = promoCode.Trim().ToUpperInvariant();
            var promo = await _context.PromoCodes.FirstOrDefaultAsync(p => p.Code.ToUpper() == code);

            if (promo == null)
            {
                return (false, $"Promo code '{promoCode}' is invalid.", null);
            }

            if (!promo.IsActive || DateTime.UtcNow < promo.ValidFrom || DateTime.UtcNow > promo.ValidTo)
            {
                return (false, $"Promo code '{promoCode}' is expired or inactive.", null);
            }

            if (basePrice < promo.MinBookingAmount)
            {
                return (false, $"Promo code requires a minimum booking of Rs.{promo.MinBookingAmount:F2}.", null);
            }

            appliedCode = promo.Code;
            discountAmount = promo.DiscountType == DiscountType.Percentage
                ? Math.Round(basePrice * (promo.DiscountValue / 100m), 2)
                : promo.DiscountValue;

            discountAmount = Math.Min(discountAmount, basePrice);
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

        return (true, "Price calculated successfully.", price);
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
        var user = await _context.Users.FindAsync(request.UserId);
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);

        if (user == null || vehicle == null)
        {
            return (false, "User or Vehicle not found.", null);
        }

        PromoCode? promo = null;
        if (!string.IsNullOrWhiteSpace(price.AppliedPromoCode))
        {
            var code = price.AppliedPromoCode.ToUpperInvariant();
            promo = await _context.PromoCodes.FirstOrDefaultAsync(p => p.Code.ToUpper() == code);
        }

        var bookingCount = await _context.Bookings.CountAsync();

        // 3. Create booking in Pending status (awaiting full payment)
        var booking = new Booking
        {
            BookingReference = $"DE-{DateTime.UtcNow:yyyyMM}-{bookingCount + 1001}",
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

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        return (true, "Booking created. Please proceed to payment to confirm.", booking);
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Include(b => b.Review)
            .Include(b => b.PromoCode)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetUserBookingsAsync(int userId)
    {
        return await _context.Bookings
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Include(b => b.Review)
            .Include(b => b.PromoCode)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return (false, "Booking not found.");
        }

        if (booking.UserId != userId)
        {
            return (false, "Unauthorized to cancel this booking.");
        }

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
        {
            return (false, $"Cannot cancel a booking that is already {booking.Status}.");
        }

        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();
        return (true, "Booking cancelled successfully.");
    }
}
