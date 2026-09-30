using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.Models;
using wad_project.Services;
using wad_project.ViewModels;

namespace wad_project.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAdminService _adminService;
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        IAdminService adminService,
        IVehicleService vehicleService,
        ILogger<AdminController> logger)
    {
        _context = context;
        _adminService = adminService;
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await BuildDashboardViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveVehicle(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found.";
            return RedirectToAction(nameof(Index), new { tab = "vehicles" });
        }

        vehicle.Status = VehicleStatus.Approved;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Vehicle '{vehicle.Brand} {vehicle.Model}' has been APPROVED and is now available in the public fleet!";

        return RedirectToAction(nameof(Index), new { tab = "vehicles" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectVehicle(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found.";
            return RedirectToAction(nameof(Index), new { tab = "vehicles" });
        }

        vehicle.Status = VehicleStatus.Rejected;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Vehicle '{vehicle.Brand} {vehicle.Model}' has been REJECTED.";

        return RedirectToAction(nameof(Index), new { tab = "vehicles" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyUser(int id, VerificationStatus status)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index), new { tab = "users" });
        }

        user.VerificationStatus = status;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"{user.Role} '{user.FullName}' verification updated to {status}.";

        return RedirectToAction(nameof(Index), new { tab = "users" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index), new { tab = "users" });
        }

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"User '{user.FullName}' is now {(user.IsActive ? "Active" : "Disabled")}.";

        return RedirectToAction(nameof(Index), new { tab = "users" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVehicle(AdminDashboardViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide all required vehicle details.";
            return RedirectToAction(nameof(Index), new { tab = "vehicles" });
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
            Status = VehicleStatus.Available, // Admin added vehicles are immediately available
            CreatedAt = DateTime.UtcNow
        };

        var (success, message) = await _vehicleService.AddVehicleAsync(vehicle);
        if (success)
        {
            TempData["SuccessMessage"] = $"Vehicle '{vehicle.Brand} {vehicle.Model}' created and added to fleet!";
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index), new { tab = "vehicles" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePromoCode(AdminDashboardViewModel model)
    {
        var newP = model.NewPromoCode;
        if (string.IsNullOrWhiteSpace(newP.Code))
        {
            TempData["ErrorMessage"] = "Promo code is required.";
            return RedirectToAction(nameof(Index), new { tab = "promos" });
        }

        var codeUpper = newP.Code.Trim().ToUpperInvariant();
        if (await _context.PromoCodes.AnyAsync(p => p.Code.ToUpper() == codeUpper))
        {
            TempData["ErrorMessage"] = $"Promo code '{codeUpper}' already exists.";
            return RedirectToAction(nameof(Index), new { tab = "promos" });
        }

        var promo = new PromoCode
        {
            Code = codeUpper,
            DiscountType = newP.DiscountType,
            DiscountValue = newP.DiscountValue,
            MinBookingAmount = newP.MinBookingAmount,
            ValidFrom = newP.ValidFrom.ToUniversalTime(),
            ValidTo = newP.ValidTo.ToUniversalTime(),
            IsActive = newP.IsActive
        };

        await _context.PromoCodes.AddAsync(promo);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Promo code '{promo.Code}' created successfully!";

        return RedirectToAction(nameof(Index), new { tab = "promos" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePromoCode(int id)
    {
        var promo = await _context.PromoCodes.FindAsync(id);
        if (promo == null)
        {
            TempData["ErrorMessage"] = "Promo code not found.";
            return RedirectToAction(nameof(Index), new { tab = "promos" });
        }

        _context.PromoCodes.Remove(promo);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Promo code '{promo.Code}' deleted.";

        return RedirectToAction(nameof(Index), new { tab = "promos" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePromoStatus(int id)
    {
        var promo = await _context.PromoCodes.FindAsync(id);
        if (promo == null)
        {
            TempData["ErrorMessage"] = "Promo code not found.";
            return RedirectToAction(nameof(Index), new { tab = "promos" });
        }

        promo.IsActive = !promo.IsActive;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Promo code '{promo.Code}' is now {(promo.IsActive ? "Active" : "Inactive")}.";

        return RedirectToAction(nameof(Index), new { tab = "promos" });
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

    private async Task<AdminDashboardViewModel> BuildDashboardViewModelAsync()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        var users = await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var vehicles = await _context.Vehicles.Include(v => v.Owner).OrderByDescending(v => v.CreatedAt).ToListAsync();
        var promos = await _context.PromoCodes.OrderByDescending(p => p.ValidTo).ToListAsync();
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalUsers = users.Count,
            TotalCustomers = users.Count(u => u.Role == UserRole.Customer),
            TotalOwners = users.Count(u => u.Role == UserRole.Owner),
            PendingOwners = users.Count(u => u.Role == UserRole.Owner && u.VerificationStatus == VerificationStatus.Pending),
            PendingCustomers = users.Count(u => u.Role == UserRole.Customer && u.VerificationStatus == VerificationStatus.Pending),

            TotalVehicles = vehicles.Count,
            AvailableVehicles = vehicles.Count(v => v.Status == VehicleStatus.Available || v.Status == VehicleStatus.Approved),
            PendingVehicles = vehicles.Count(v => v.Status == VehicleStatus.Pending),
            BookedVehicles = vehicles.Count(v => v.Status == VehicleStatus.Booked),
            VehiclesUnderMaintenance = vehicles.Count(v => v.Status == VehicleStatus.UnderMaintenance),

            TotalBookings = stats.TotalBookings,
            ActiveBookings = stats.ActiveBookings,
            CompletedBookings = stats.CompletedBookings,
            CancelledBookings = stats.CancelledBookings,
            PendingBookings = stats.PendingBookings,
            TotalRevenue = stats.TotalRevenue,

            Vehicles = vehicles,
            Bookings = bookings,
            Users = users,
            PromoCodes = promos
        };
    }
}
