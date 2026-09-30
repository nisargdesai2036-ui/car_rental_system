using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.Models;
using wad_project.Services;
using wad_project.ViewModels;

namespace wad_project.Controllers;

[Authorize(Roles = "Owner,Admin")]
public class OwnerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<OwnerController> _logger;

    public OwnerController(
        ApplicationDbContext context,
        IVehicleService vehicleService,
        ILogger<OwnerController> logger)
    {
        _context = context;
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var owner = await GetCurrentAppUserAsync();
        if (owner == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await BuildOwnerDashboardAsync(owner.Id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVehicle(OwnerDashboardViewModel model)
    {
        var owner = await GetCurrentAppUserAsync();
        if (owner == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide all required vehicle details.";
            var fullModel = await BuildOwnerDashboardAsync(owner.Id);
            fullModel.NewVehicle = model.NewVehicle;
            return View("Index", fullModel);
        }

        var newV = model.NewVehicle;
        var imageUrl = string.IsNullOrWhiteSpace(newV.ImageUrl)
            ? GetDefaultVehicleImage(newV.VehicleType)
            : newV.ImageUrl.Trim();

        var vehicle = new Vehicle
        {
            Brand = newV.Brand.Trim(),
            Model = newV.Model.Trim(),
            Year = newV.Year,
            LicensePlate = newV.LicensePlate.Trim().ToUpperInvariant(),
            VehicleType = newV.VehicleType,
            SeatingCapacity = newV.SeatingCapacity,
            FuelType = newV.FuelType,
            Transmission = newV.Transmission,
            HourlyRate = newV.HourlyRate,
            DailyRate = newV.DailyRate,
            PickupLocation = newV.PickupLocation.Trim(),
            ImageUrl = imageUrl,
            Status = VehicleStatus.Pending, // Awaiting Admin Approval
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow
        };

        var (success, message) = await _vehicleService.AddVehicleAsync(vehicle);
        if (success)
        {
            TempData["SuccessMessage"] = $"Vehicle '{vehicle.Brand} {vehicle.Model}' submitted successfully! It is now pending admin approval.";
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, VehicleStatus status)
    {
        var owner = await GetCurrentAppUserAsync();
        if (owner == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == owner.Id);
        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found in your fleet.";
            return RedirectToAction(nameof(Index));
        }

        if (vehicle.Status == VehicleStatus.Pending || vehicle.Status == VehicleStatus.Rejected)
        {
            TempData["ErrorMessage"] = "Cannot modify status of a vehicle that is not yet approved by Admin.";
            return RedirectToAction(nameof(Index));
        }

        // Owner can toggle between Available, UnderMaintenance, and Inactive
        if (status == VehicleStatus.Available || status == VehicleStatus.UnderMaintenance || status == VehicleStatus.Inactive)
        {
            vehicle.Status = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Vehicle '{vehicle.Brand} {vehicle.Model}' status updated to {status}.";
        }
        else
        {
            TempData["ErrorMessage"] = "Invalid status update.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var owner = await GetCurrentAppUserAsync();
        if (owner == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == owner.Id);
        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found.";
            return RedirectToAction(nameof(Index));
        }

        vehicle.Status = VehicleStatus.Inactive;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Vehicle '{vehicle.Brand} {vehicle.Model}' has been deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<OwnerDashboardViewModel> BuildOwnerDashboardAsync(int ownerId)
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.OwnerId == ownerId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        var vehicleIds = vehicles.Select(v => v.Id).ToList();

        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Include(b => b.Payment)
            .Where(b => vehicleIds.Contains(b.VehicleId))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var totalEarnings = bookings
            .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed)
            .Sum(b => b.TotalAmount);

        return new OwnerDashboardViewModel
        {
            TotalEarnings = totalEarnings,
            TotalVehicles = vehicles.Count,
            ApprovedVehicles = vehicles.Count(v => v.Status == VehicleStatus.Approved || v.Status == VehicleStatus.Available),
            PendingVehicles = vehicles.Count(v => v.Status == VehicleStatus.Pending),
            BookedVehicles = vehicles.Count(v => v.Status == VehicleStatus.Booked),
            TotalBookings = bookings.Count,
            ActiveBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending),
            MyVehicles = vehicles,
            MyVehicleBookings = bookings
        };
    }

    private async Task<User?> GetCurrentAppUserAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

        if (string.IsNullOrEmpty(email)) return null;

        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    private static string GetDefaultVehicleImage(VehicleType type)
    {
        return type switch
        {
            VehicleType.Bike => "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?auto=format&fit=crop&w=800&q=80",
            VehicleType.Hatchback => "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=800&q=80",
            VehicleType.Sedan => "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=800&q=80",
            VehicleType.SUV => "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=800&q=80",
            VehicleType.Luxury => "https://images.unsplash.com/photo-1617788138017-80ad40651399?auto=format&fit=crop&w=800&q=80",
            _ => "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=800&q=80"
        };
    }
}
