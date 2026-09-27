using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.DTOs;
using wad_project.Models;
using wad_project.Services;

namespace wad_project.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public PaymentsController(ApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    [HttpGet("{bookingId:int}")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> GetPaymentByBookingId(int bookingId)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.Fail("Authentication required."));
        }

        var booking = await _context.Bookings.Include(b => b.Vehicle).FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail($"Booking with ID {bookingId} not found."));
        }

        // Customer can only view their own payment; Owner can view vehicle payments; Admin can view all
        if (user.Role != UserRole.Admin && booking.UserId != user.Id && (booking.Vehicle == null || booking.Vehicle.OwnerId != user.Id))
        {
            return StatusCode(403, ApiResponse<PaymentResponseDto>.Fail("You do not have permission to view this payment information."));
        }

        var payment = await _paymentService.GetPaymentByBookingIdAsync(bookingId);
        if (payment == null)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail($"No payment record found for booking ID {bookingId}."));
        }

        return Ok(ApiResponse<PaymentResponseDto>.Ok(MapToPaymentDto(payment)));
    }

    [HttpPost("{bookingId:int}/pay")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> ProcessSimulatedPayment(
        int bookingId, [FromBody] MakePaymentDto dto)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.Fail("Authentication required."));
        }

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return NotFound(ApiResponse<PaymentResponseDto>.Fail($"Booking with ID {bookingId} not found."));
        }

        // Only the customer who placed the booking (or Admin) can pay
        if (user.Role != UserRole.Admin && booking.UserId != user.Id)
        {
            return StatusCode(403, ApiResponse<PaymentResponseDto>.Fail("You cannot pay for another customer's booking."));
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return BadRequest(ApiResponse<PaymentResponseDto>.Fail($"Booking is already {booking.Status}."));
        }

        if (dto.SimulateFailure)
        {
            var failedPayment = new Payment
            {
                BookingId = booking.Id,
                TransactionId = $"TXN-FAIL-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                PaymentMethod = dto.PaymentMethod,
                Amount = booking.TotalAmount,
                Status = PaymentStatus.Failed,
                PaidAt = DateTime.UtcNow
            };
            _context.Payments.Add(failedPayment);
            await _context.SaveChangesAsync();

            return BadRequest(ApiResponse<PaymentResponseDto>.Fail("Simulated payment failed. Card declined or transaction rejected."));
        }

        var (success, message, payment) = await _paymentService.ProcessPaymentAsync(new ViewModels.MakePaymentViewModel
        {
            BookingId = booking.Id,
            PaymentMethod = dto.PaymentMethod,
            Amount = booking.TotalAmount
        });

        if (!success || payment == null)
        {
            return BadRequest(ApiResponse<PaymentResponseDto>.Fail(message));
        }

        // Add payment alert notification
        _context.Notifications.Add(new Notification
        {
            UserId = booking.UserId,
            RecipientPhone = "9876543210",
            Message = $"Payment of Rs.{payment.Amount:F2} successful (Txn: {payment.TransactionId}). Booking {booking.BookingReference} confirmed!",
            Type = NotificationType.PaymentAlert,
            IsSent = true,
            SentAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<PaymentResponseDto>.Ok(MapToPaymentDto(payment), "Simulated payment processed successfully. Booking confirmed!"));
    }

    private async Task<User?> GetCurrentAppUserAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

        if (!string.IsNullOrEmpty(email))
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int parsedId))
        {
            return await _context.Users.FindAsync(parsedId);
        }

        return null;
    }

    private static PaymentResponseDto MapToPaymentDto(Payment p) => new()
    {
        Id = p.Id,
        BookingId = p.BookingId,
        TransactionId = p.TransactionId,
        PaymentMethod = p.PaymentMethod,
        Amount = p.Amount,
        Status = p.Status,
        PaidAt = p.PaidAt
    };
}
