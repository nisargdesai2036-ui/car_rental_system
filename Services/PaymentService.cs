using Microsoft.EntityFrameworkCore;
using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, Payment? Payment)> ProcessPaymentAsync(MakePaymentViewModel model)
    {
        var booking = await _context.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == model.BookingId);

        if (booking == null)
        {
            return (false, "Booking not found.", null);
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return (false, $"Booking is already {booking.Status}.", null);
        }

        // Rule: Full payment only (must equal booking.TotalAmount)
        if (model.Amount != booking.TotalAmount)
        {
            return (false, $"Full payment of Rs.{booking.TotalAmount:F2} is required.", null);
        }

        // Create successful payment
        var payment = new Payment
        {
            BookingId = booking.Id,
            Booking = booking,
            TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}",
            PaymentMethod = model.PaymentMethod,
            Amount = model.Amount,
            Status = PaymentStatus.Successful,
            PaidAt = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment);

        // Confirm the booking
        booking.Payment = payment;
        booking.Status = BookingStatus.Confirmed;

        await _context.SaveChangesAsync();

        return (true, "Payment successful! Your booking is confirmed.", payment);
    }

    public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
    }
}
