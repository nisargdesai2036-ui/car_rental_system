using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.DTOs;
using wad_project.Models;
using wad_project.Services;

namespace wad_project.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;

    public AdminApiController(
        ApplicationDbContext context,
        IAdminService adminService,
        IUserService userService)
    {
        _context = context;
        _adminService = adminService;
        _userService = userService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboard()
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var stats = await _adminService.GetDashboardStatsAsync();
        var totalCustomers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
        var totalOwners = await _context.Users.CountAsync(u => u.Role == UserRole.Owner);
        var pendingOwners = await _context.Users.CountAsync(u => u.Role == UserRole.Owner && u.VerificationStatus == VerificationStatus.Pending);
        var pendingCars = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Pending);

        var dto = new AdminDashboardDto
        {
            TotalUsers = stats.TotalUsers,
            TotalCustomers = totalCustomers,
            TotalOwners = totalOwners,
            TotalCars = stats.TotalVehicles,
            PendingOwners = pendingOwners,
            PendingCars = pendingCars,
            TotalBookings = stats.TotalBookings,
            ActiveBookings = stats.ActiveBookings,
            CompletedBookings = stats.CompletedBookings,
            CancelledBookings = stats.CancelledBookings,
            PendingBookings = stats.PendingBookings,
            TotalRevenue = stats.TotalRevenue
        };

        return Ok(ApiResponse<AdminDashboardDto>.Ok(dto));
    }

    // -------------------------------------------------------------
    // OWNER VERIFICATION
    // -------------------------------------------------------------

    [HttpGet("owners/pending")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetPendingOwners()
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var owners = await _context.Users
            .Where(u => u.Role == UserRole.Owner && u.VerificationStatus == VerificationStatus.Pending)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                DrivingLicenseNumber = u.DrivingLicenseNumber,
                Role = u.Role,
                VerificationStatus = u.VerificationStatus,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserDto>>.Ok(owners));
    }

    [HttpPut("owners/{userId:int}/approve")]
    public async Task<ActionResult<ApiResponse>> ApproveOwner(int userId)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Owner);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail($"Owner with user ID {userId} not found."));
        }

        user.VerificationStatus = VerificationStatus.Verified;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Owner '{user.FullName}' has been approved and verified successfully."));
    }

    [HttpPut("owners/{userId:int}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectOwner(int userId)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Owner);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail($"Owner with user ID {userId} not found."));
        }

        user.VerificationStatus = VerificationStatus.Rejected;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Owner '{user.FullName}' verification has been rejected."));
    }

    // -------------------------------------------------------------
    // CUSTOMER VERIFICATION
    // -------------------------------------------------------------

    [HttpGet("customers/pending")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetPendingCustomers()
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var customers = await _context.Users
            .Where(u => u.Role == UserRole.Customer && u.VerificationStatus == VerificationStatus.Pending)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                DrivingLicenseNumber = u.DrivingLicenseNumber,
                Role = u.Role,
                VerificationStatus = u.VerificationStatus,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserDto>>.Ok(customers));
    }

    [HttpPut("customers/{userId:int}/approve")]
    public async Task<ActionResult<ApiResponse>> ApproveCustomer(int userId)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Customer);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail($"Customer with user ID {userId} not found."));
        }

        user.VerificationStatus = VerificationStatus.Verified;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Customer '{user.FullName}' verification approved successfully."));
    }

    [HttpPut("customers/{userId:int}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectCustomer(int userId)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Customer);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail($"Customer with user ID {userId} not found."));
        }

        user.VerificationStatus = VerificationStatus.Rejected;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Customer '{user.FullName}' verification rejected."));
    }

    // -------------------------------------------------------------
    // VEHICLE / CAR APPROVALS
    // -------------------------------------------------------------

    [HttpGet("cars/pending")]
    public async Task<ActionResult<ApiResponse<List<Vehicle>>>> GetPendingCars()
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var pendingCars = await _context.Vehicles
            .Include(v => v.Owner)
            .Where(v => v.Status == VehicleStatus.Pending)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<Vehicle>>.Ok(pendingCars));
    }

    [HttpPut("cars/{id:int}/approve")]
    public async Task<ActionResult<ApiResponse>> ApproveCar(int id)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var car = await _context.Vehicles.FindAsync(id);
        if (car == null)
        {
            return NotFound(ApiResponse.Fail($"Car with ID {id} not found."));
        }

        car.Status = VehicleStatus.Approved;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Car '{car.Brand} {car.Model}' approved and listed as available for booking."));
    }

    [HttpPut("cars/{id:int}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectCar(int id)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var car = await _context.Vehicles.FindAsync(id);
        if (car == null)
        {
            return NotFound(ApiResponse.Fail($"Car with ID {id} not found."));
        }

        car.Status = VehicleStatus.Rejected;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Car '{car.Brand} {car.Model}' has been rejected."));
    }

    // -------------------------------------------------------------
    // USERS MANAGEMENT
    // -------------------------------------------------------------

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var users = await _userService.GetAllUsersAsync();
        var dtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            MobileNumber = u.MobileNumber,
            DrivingLicenseNumber = u.DrivingLicenseNumber,
            Role = u.Role,
            VerificationStatus = u.VerificationStatus,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();

        return Ok(ApiResponse<List<UserDto>>.Ok(dtos));
    }

    [HttpGet("users/{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail($"User with ID {id} not found."));
        }

        var dto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            DrivingLicenseNumber = user.DrivingLicenseNumber,
            Role = user.Role,
            VerificationStatus = user.VerificationStatus,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(ApiResponse<UserDto>.Ok(dto));
    }

    [HttpPut("users/{id:int}/status")]
    public async Task<ActionResult<ApiResponse>> ToggleUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail($"User with ID {id} not found."));
        }

        user.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"User active status updated to {dto.IsActive}."));
    }

    // -------------------------------------------------------------
    // BOOKINGS MANAGEMENT
    // -------------------------------------------------------------

    [HttpGet("bookings")]
    public async Task<ActionResult<ApiResponse<List<BookingResponseDto>>>> GetAllAdminBookings()
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Include(b => b.PromoCode)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookingResponseDto
            {
                Id = b.Id,
                BookingReference = b.BookingReference,
                UserId = b.UserId,
                UserName = b.User != null ? b.User.FullName : "Customer",
                UserEmail = b.User != null ? b.User.Email : string.Empty,
                CarId = b.VehicleId,
                CarName = b.Vehicle != null ? $"{b.Vehicle.Brand} {b.Vehicle.Model}" : "Car",
                LicensePlate = b.Vehicle != null ? b.Vehicle.LicensePlate : string.Empty,
                RentalType = b.RentalType,
                PickupDateTime = b.PickupDateTime,
                ReturnDateTime = b.ReturnDateTime,
                PickupLocation = b.PickupLocation,
                Duration = b.RentalDuration,
                BasePrice = b.BasePrice,
                DiscountAmount = b.DiscountAmount,
                TotalAmount = b.TotalAmount,
                PromoCode = b.PromoCode != null ? b.PromoCode.Code : null,
                BookingStatus = b.Status,
                PaymentStatus = b.Payment != null ? b.Payment.Status : PaymentStatus.Pending,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<BookingResponseDto>>.Ok(bookings));
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<ApiResponse<BookingResponseDto>>> GetAdminBookingById(int id)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var b = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Include(b => b.PromoCode)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (b == null)
        {
            return NotFound(ApiResponse<BookingResponseDto>.Fail($"Booking with ID {id} not found."));
        }

        var dto = new BookingResponseDto
        {
            Id = b.Id,
            BookingReference = b.BookingReference,
            UserId = b.UserId,
            UserName = b.User != null ? b.User.FullName : "Customer",
            UserEmail = b.User != null ? b.User.Email : string.Empty,
            CarId = b.VehicleId,
            CarName = b.Vehicle != null ? $"{b.Vehicle.Brand} {b.Vehicle.Model}" : "Car",
            LicensePlate = b.Vehicle != null ? b.Vehicle.LicensePlate : string.Empty,
            RentalType = b.RentalType,
            PickupDateTime = b.PickupDateTime,
            ReturnDateTime = b.ReturnDateTime,
            PickupLocation = b.PickupLocation,
            Duration = b.RentalDuration,
            BasePrice = b.BasePrice,
            DiscountAmount = b.DiscountAmount,
            TotalAmount = b.TotalAmount,
            PromoCode = b.PromoCode != null ? b.PromoCode.Code : null,
            BookingStatus = b.Status,
            PaymentStatus = b.Payment != null ? b.Payment.Status : PaymentStatus.Pending,
            CreatedAt = b.CreatedAt
        };

        return Ok(ApiResponse<BookingResponseDto>.Ok(dto));
    }

    [HttpPut("bookings/{id:int}/status")]
    public async Task<ActionResult<ApiResponse>> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound(ApiResponse.Fail($"Booking with ID {id} not found."));
        }

        booking.Status = dto.Status;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Booking status updated to {dto.Status}."));
    }

    [HttpPut("bookings/{id:int}/cancel")]
    public async Task<ActionResult<ApiResponse>> AdminCancelBooking(int id)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound(ApiResponse.Fail($"Booking with ID {id} not found."));
        }

        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Booking {booking.BookingReference} cancelled by Admin."));
    }

    private async Task<ActionResult?> EnsureAdminAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null || user.Role != UserRole.Admin)
        {
            return StatusCode(403, ApiResponse.Fail("Admin authorization required."));
        }

        return null;
    }
}
