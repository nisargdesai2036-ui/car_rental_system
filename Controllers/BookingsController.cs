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
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingService _bookingService;

    public BookingsController(ApplicationDbContext context, IBookingService bookingService)
    {
        _context = context;
        _bookingService = bookingService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BookingResponseDto>>>> GetAllBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Include(b => b.PromoCode)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<BookingResponseDto>>.Ok(bookings.Select(MapToBookingDto).ToList()));
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<BookingResponseDto>>>> GetMyBookings()
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<List<BookingResponseDto>>.Fail("User is not authenticated."));
        }

        var bookings = await _bookingService.GetUserBookingsAsync(user.Id);
        return Ok(ApiResponse<List<BookingResponseDto>>.Ok(bookings.Select(MapToBookingDto).ToList()));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpGet("/api/owner/bookings")]
    public async Task<ActionResult<ApiResponse<List<BookingResponseDto>>>> GetOwnerVehicleBookings()
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<List<BookingResponseDto>>.Fail("User is not authenticated."));
        }

        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Include(b => b.PromoCode)
            .Where(b => b.Vehicle != null && b.Vehicle.OwnerId == user.Id)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<BookingResponseDto>>.Ok(bookings.Select(MapToBookingDto).ToList()));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpGet("/api/owner/bookings/{id:int}")]
    public async Task<ActionResult<ApiResponse<BookingResponseDto>>> GetOwnerVehicleBookingById(int id)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<BookingResponseDto>.Fail("User is not authenticated."));
        }

        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Include(b => b.PromoCode)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound(ApiResponse<BookingResponseDto>.Fail($"Booking with ID {id} not found."));
        }

        if (user.Role != UserRole.Admin && (booking.Vehicle == null || booking.Vehicle.OwnerId != user.Id))
        {
            return StatusCode(403, ApiResponse<BookingResponseDto>.Fail("You do not have permission to view this booking."));
        }

        return Ok(ApiResponse<BookingResponseDto>.Ok(MapToBookingDto(booking)));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BookingResponseDto>>> GetBookingById(int id)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<BookingResponseDto>.Fail("User is not authenticated."));
        }

        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound(ApiResponse<BookingResponseDto>.Fail($"Booking with ID {id} not found."));
        }

        // Prevent IDOR: Customer can only view their own booking
        if (user.Role != UserRole.Admin && booking.UserId != user.Id && (booking.Vehicle?.OwnerId != user.Id))
        {
            return StatusCode(403, ApiResponse<BookingResponseDto>.Fail("You do not have permission to view this booking."));
        }

        return Ok(ApiResponse<BookingResponseDto>.Ok(MapToBookingDto(booking)));
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingResponseDto>>> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<BookingResponseDto>.Fail("Invalid booking data."));
        }

        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<BookingResponseDto>.Fail("Authentication required to create a booking."));
        }

        // Customer role and verification check
        if (user.Role == UserRole.Customer && user.VerificationStatus != VerificationStatus.Verified)
        {
            return StatusCode(403, ApiResponse<BookingResponseDto>.Fail("Your account verification is pending approval. You must be verified to make a booking."));
        }

        // Check if car is approved & available
        var vehicle = await _context.Vehicles.FindAsync(dto.CarId);
        if (vehicle == null || (vehicle.Status != VehicleStatus.Approved && vehicle.Status != VehicleStatus.Available))
        {
            return BadRequest(ApiResponse<BookingResponseDto>.Fail("Selected vehicle is not available for rental."));
        }

        var req = new ViewModels.BookingRequestViewModel
        {
            UserId = user.Id,
            VehicleId = dto.CarId,
            RentalType = dto.RentalType,
            PickupDateTime = dto.PickupDateTime,
            ReturnDateTime = dto.ReturnDateTime,
            PickupLocation = dto.PickupLocation,
            PromoCode = dto.PromoCode
        };

        var (success, message, booking) = await _bookingService.CreateBookingAsync(req);
        if (!success || booking == null)
        {
            return BadRequest(ApiResponse<BookingResponseDto>.Fail(message));
        }

        // Add auto notification
        _context.Notifications.Add(new Notification
        {
            UserId = user.Id,
            RecipientPhone = booking.User?.MobileNumber ?? "9876543210",
            Message = $"Your booking {booking.BookingReference} has been reserved. Please complete payment to confirm.",
            Type = NotificationType.BookingConfirmation,
            IsSent = true,
            SentAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var populated = await _bookingService.GetBookingByIdAsync(booking.Id);
        return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, ApiResponse<BookingResponseDto>.Ok(MapToBookingDto(populated ?? booking), message));
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponse>> CancelBooking(int id)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound(ApiResponse.Fail($"Booking with ID {id} not found."));
        }

        // Customer can only cancel their own booking, Admin can cancel any
        if (user.Role != UserRole.Admin && booking.UserId != user.Id)
        {
            return StatusCode(403, ApiResponse.Fail("You cannot cancel another customer's booking."));
        }

        if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
        {
            return BadRequest(ApiResponse.Fail($"Cannot cancel a booking with status {booking.Status}."));
        }

        booking.Status = BookingStatus.Cancelled;

        // Notification
        _context.Notifications.Add(new Notification
        {
            UserId = booking.UserId,
            RecipientPhone = "9876543210",
            Message = $"Your booking {booking.BookingReference} has been cancelled.",
            Type = NotificationType.BookingCancelled,
            IsSent = true,
            SentAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Booking cancelled successfully."));
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

    private static BookingResponseDto MapToBookingDto(Booking b) => new()
    {
        Id = b.Id,
        BookingReference = b.BookingReference,
        UserId = b.UserId,
        UserName = b.User?.FullName ?? "Unknown",
        UserEmail = b.User?.Email ?? string.Empty,
        CarId = b.VehicleId,
        CarName = b.Vehicle != null ? $"{b.Vehicle.Brand} {b.Vehicle.Model}" : "Car",
        LicensePlate = b.Vehicle?.LicensePlate ?? string.Empty,
        RentalType = b.RentalType,
        PickupDateTime = b.PickupDateTime,
        ReturnDateTime = b.ReturnDateTime,
        PickupLocation = b.PickupLocation,
        Duration = b.RentalDuration,
        BasePrice = b.BasePrice,
        DiscountAmount = b.DiscountAmount,
        TotalAmount = b.TotalAmount,
        PromoCode = b.PromoCode?.Code,
        BookingStatus = b.Status,
        PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
        CreatedAt = b.CreatedAt
    };
}
