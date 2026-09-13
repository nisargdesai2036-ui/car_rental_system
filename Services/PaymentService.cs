using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class PaymentService : IPaymentService
{
    private readonly IDataStore _dataStore;

    public PaymentService(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<(bool Success, string Message, Payment? Payment)> ProcessPaymentAsync(MakePaymentViewModel model)
    {
        lock (_dataStore.Bookings)
        {
            var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == model.BookingId);
            if (booking == null)
            {
                return Task.FromResult<(bool, string, Payment?)>((false, "Booking not found.", null));
            }

            if (booking.Status != BookingStatus.Pending)
            {
                return Task.FromResult<(bool, string, Payment?)>((false, $"Booking is already {booking.Status}.", null));
            }

            // Simple Rule: Full payment only (must equal booking.TotalAmount)
            if (model.Amount != booking.TotalAmount)
            {
                return Task.FromResult<(bool, string, Payment?)>((false, $"Full payment of Rs.{booking.TotalAmount:F2} is required.", null));
            }

            // Create successful payment
            var payment = new Payment
            {
                Id = _dataStore.NextPaymentId(),
                BookingId = booking.Id,
                Booking = booking,
                TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}",
                PaymentMethod = model.PaymentMethod,
                Amount = model.Amount,
                Status = PaymentStatus.Successful,
                PaidAt = DateTime.UtcNow
            };

            lock (_dataStore.Payments)
            {
                _dataStore.Payments.Add(payment);
            }

            // Confirm the booking
            booking.Payment = payment;
            booking.Status = BookingStatus.Confirmed;

            return Task.FromResult<(bool, string, Payment?)>((true, "Payment successful! Your booking is confirmed.", payment));
        }
    }

    public Task<Payment?> GetPaymentByBookingIdAsync(int bookingId)
    {
        lock (_dataStore.Payments)
        {
            var payment = _dataStore.Payments.FirstOrDefault(p => p.BookingId == bookingId);
            return Task.FromResult(payment);
        }
    }
}
