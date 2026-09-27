using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.DTOs;
using wad_project.Models;
using wad_project.Services;

namespace wad_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IVehicleService _vehicleService;

    public CarsController(ApplicationDbContext context, IVehicleService vehicleService)
    {
        _context = context;
        _vehicleService = vehicleService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Vehicle>>>> GetAllCars([FromQuery] CarQueryParameters query)
    {
        var filter = new ViewModels.VehicleSearchFilterViewModel
        {
            SearchLocation = query.Location,
            VehicleType = query.Type,
            FuelType = query.FuelType,
            Transmission = query.Transmission,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            SortBy = query.SortBy
        };

        var cars = await _vehicleService.SearchAndFilterAsync(filter);
        // Only return Approved & Available vehicles for public/customer listing
        var availableCars = cars.Where(c => c.Status == VehicleStatus.Approved || c.Status == VehicleStatus.Available).ToList();
        return Ok(ApiResponse<List<Vehicle>>.Ok(availableCars, $"Found {availableCars.Count} available cars."));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<Vehicle>>>> GetMyCars()
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<List<Vehicle>>.Fail("User is not authenticated."));
        }

        var cars = await _context.Vehicles
            .Where(v => v.OwnerId == user.Id)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<Vehicle>>.Ok(cars));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<Vehicle>>> GetCarById(int id)
    {
        var car = await _vehicleService.GetVehicleByIdAsync(id);
        if (car == null)
        {
            return NotFound(ApiResponse<Vehicle>.Fail($"Car with ID {id} not found."));
        }

        return Ok(ApiResponse<Vehicle>.Ok(car));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Vehicle>>> AddCar([FromBody] CreateCarDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<Vehicle>.Fail("Invalid car details."));
        }

        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<Vehicle>.Fail("Authentication required to add a car."));
        }

        if (user.Role == UserRole.Owner && user.VerificationStatus != VerificationStatus.Verified)
        {
            return StatusCode(403, ApiResponse<Vehicle>.Fail("Your owner account is pending verification. You cannot list vehicles yet."));
        }

        var vehicle = new Vehicle
        {
            Brand = dto.Brand.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            LicensePlate = dto.LicensePlate.Trim().ToUpperInvariant(),
            VehicleType = dto.VehicleType,
            SeatingCapacity = dto.SeatingCapacity,
            FuelType = dto.FuelType,
            Transmission = dto.Transmission,
            HourlyRate = dto.HourlyRate,
            DailyRate = dto.DailyRate,
            PickupLocation = dto.PickupLocation.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? "/images/vehicles/default.png" : dto.ImageUrl.Trim(),
            Status = user.Role == UserRole.Admin ? VehicleStatus.Approved : VehicleStatus.Pending,
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        var (success, message) = await _vehicleService.AddVehicleAsync(vehicle);
        if (!success)
        {
            return BadRequest(ApiResponse<Vehicle>.Fail(message));
        }

        var msg = user.Role == UserRole.Admin 
            ? "Car added and approved successfully." 
            : "Car submitted successfully. It is now pending admin approval.";

        return CreatedAtAction(nameof(GetCarById), new { id = vehicle.Id }, ApiResponse<Vehicle>.Ok(vehicle, msg));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<Vehicle>>> UpdateCar(int id, [FromBody] UpdateCarDto dto)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<Vehicle>.Fail("Authentication required."));
        }

        var car = await _context.Vehicles.FindAsync(id);
        if (car == null)
        {
            return NotFound(ApiResponse<Vehicle>.Fail($"Car with ID {id} not found."));
        }

        // Ownership enforcement: Owner can only edit their own vehicle. Admin can edit any.
        if (user.Role != UserRole.Admin && (user.Role != UserRole.Owner || car.OwnerId != user.Id))
        {
            return StatusCode(403, ApiResponse<Vehicle>.Fail("You do not have permission to modify this vehicle."));
        }

        car.Brand = dto.Brand.Trim();
        car.Model = dto.Model.Trim();
        car.Year = dto.Year;
        car.LicensePlate = dto.LicensePlate.Trim().ToUpperInvariant();
        car.VehicleType = dto.VehicleType;
        car.SeatingCapacity = dto.SeatingCapacity;
        car.FuelType = dto.FuelType;
        car.Transmission = dto.Transmission;
        car.HourlyRate = dto.HourlyRate;
        car.DailyRate = dto.DailyRate;
        car.PickupLocation = dto.PickupLocation.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl)) car.ImageUrl = dto.ImageUrl.Trim();
        
        // Owners cannot self-approve; only Admin can set Approved status directly
        if (user.Role == UserRole.Admin)
        {
            car.Status = dto.Status;
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<Vehicle>.Ok(car, "Car updated successfully."));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteCar(int id)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var car = await _context.Vehicles.FindAsync(id);
        if (car == null)
        {
            return NotFound(ApiResponse.Fail($"Car with ID {id} not found."));
        }

        // Check ownership
        if (user.Role != UserRole.Admin && (user.Role != UserRole.Owner || car.OwnerId != user.Id))
        {
            return StatusCode(403, ApiResponse.Fail("You do not have permission to delete this vehicle."));
        }

        car.Status = VehicleStatus.Inactive;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Car deactivated successfully."));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPut("{id:int}/availability")]
    public async Task<ActionResult<ApiResponse>> SetAvailability(int id, [FromBody] SetAvailabilityDto dto)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var car = await _context.Vehicles.FindAsync(id);
        if (car == null)
        {
            return NotFound(ApiResponse.Fail($"Car with ID {id} not found."));
        }

        if (user.Role != UserRole.Admin && (user.Role != UserRole.Owner || car.OwnerId != user.Id))
        {
            return StatusCode(403, ApiResponse.Fail("You do not have permission to modify availability for this vehicle."));
        }

        car.Status = dto.IsAvailable ? VehicleStatus.Available : VehicleStatus.Booked;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Car availability set to {(dto.IsAvailable ? "Available" : "Unavailable")}."));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPut("{id:int}/maintenance")]
    public async Task<ActionResult<ApiResponse>> SetMaintenance(int id, [FromBody] SetMaintenanceDto dto)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var car = await _context.Vehicles.FindAsync(id);
        if (car == null)
        {
            return NotFound(ApiResponse.Fail($"Car with ID {id} not found."));
        }

        if (user.Role != UserRole.Admin && (user.Role != UserRole.Owner || car.OwnerId != user.Id))
        {
            return StatusCode(403, ApiResponse.Fail("You do not have permission to mark maintenance for this vehicle."));
        }

        car.Status = dto.IsUnderMaintenance ? VehicleStatus.UnderMaintenance : VehicleStatus.Available;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"Car maintenance status set to {(dto.IsUnderMaintenance ? "Under Maintenance" : "Available")}."));
    }

    [AllowAnonymous]
    [HttpGet("recommended")]
    public async Task<ActionResult<ApiResponse<List<ViewModels.RecommendedVehicleViewModel>>>> GetRecommendedCars(
        [FromQuery] CarRecommendationRequestDto request)
    {
        var criteria = new ViewModels.RecommendationRequestViewModel
        {
            Passengers = request.Passengers,
            Budget = request.Budget,
            DurationDays = request.DurationDays,
            PreferredType = request.PreferredType
        };

        var recommendations = await _vehicleService.GetRecommendationsAsync(criteria);
        return Ok(ApiResponse<List<ViewModels.RecommendedVehicleViewModel>>.Ok(recommendations, "Recommendations generated successfully."));
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
}
