using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using wad_project.Models;

namespace wad_project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Services.IVehicleService _vehicleService;
    private readonly Services.IBookingService _bookingService;
    private readonly Services.IUserService _userService;

    public HomeController(
        ILogger<HomeController> logger, 
        Services.IVehicleService vehicleService,
        Services.IBookingService bookingService,
        Services.IUserService userService)
    {
        _logger = logger;
        _vehicleService = vehicleService;
        _bookingService = bookingService;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var vehicles = await _vehicleService.GetAllVehiclesAsync();
        return View(vehicles);
    }

    [HttpGet]
    public async Task<IActionResult> Book(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Book", "Home", new { id }) });
        }

        var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null || (vehicle.Status != VehicleStatus.Approved && vehicle.Status != VehicleStatus.Available))
        {
            TempData["ErrorMessage"] = "The selected vehicle is currently not available for booking.";
            return RedirectToAction(nameof(Explore));
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out int userId);

        var bookingModel = new ViewModels.BookingRequestViewModel
        {
            VehicleId = vehicle.Id,
            UserId = userId,
            RentalType = RentalType.Daily,
            PickupDateTime = DateTime.Now.AddHours(2),
            ReturnDateTime = DateTime.Now.AddDays(1).AddHours(2),
            PickupLocation = vehicle.PickupLocation
        };

        ViewData["Vehicle"] = vehicle;
        return View(bookingModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(ViewModels.BookingRequestViewModel model)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        var vehicle = await _vehicleService.GetVehicleByIdAsync(model.VehicleId);
        if (vehicle == null)
        {
            TempData["ErrorMessage"] = "Vehicle not found.";
            return RedirectToAction(nameof(Explore));
        }

        ViewData["Vehicle"] = vehicle;

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            model.UserId = userId;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, message, booking) = await _bookingService.CreateBookingAsync(model);
        if (!success || booking == null)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Booking reserved successfully! Reference: {booking.BookingReference}. Total: ₹{booking.TotalAmount:N0}";
        return RedirectToAction("Bookings", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> Explore(ViewModels.VehicleSearchFilterViewModel filter)
    {
        var vehicles = await _vehicleService.SearchAndFilterAsync(filter);
        // Only return Approved & Available vehicles for public exploration
        var availableVehicles = vehicles
            .Where(v => v.Status == VehicleStatus.Approved || v.Status == VehicleStatus.Available)
            .ToList();

        ViewData["Filter"] = filter;
        return View(availableVehicles);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
