using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.ViewModels;

public class AdminDashboardViewModel
{
    // Overview KPI Metrics
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOwners { get; set; }
    public int PendingOwners { get; set; }
    public int PendingCustomers { get; set; }

    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int PendingVehicles { get; set; }
    public int BookedVehicles { get; set; }
    public int VehiclesUnderMaintenance { get; set; }

    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalRevenue { get; set; }

    // Tab Collections
    public List<Vehicle> Vehicles { get; set; } = new();
    public List<Booking> Bookings { get; set; } = new();
    public List<Booking> RecentBookings { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<PromoCode> PromoCodes { get; set; } = new();

    // Forms
    public AdminCreateVehicleViewModel NewVehicle { get; set; } = new();
    public AdminCreatePromoCodeViewModel NewPromoCode { get; set; } = new();
}

public class AdminCreateVehicleViewModel
{
    [Required(ErrorMessage = "Brand is required")]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required")]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Range(2000, 2030, ErrorMessage = "Year must be between 2000 and 2030")]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [Required(ErrorMessage = "License plate is required")]
    [StringLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    public VehicleType VehicleType { get; set; } = VehicleType.Sedan;

    [Range(1, 12, ErrorMessage = "Seating capacity must be between 1 and 12")]
    public int SeatingCapacity { get; set; } = 5;

    public FuelType FuelType { get; set; } = FuelType.Petrol;

    public TransmissionType Transmission { get; set; } = TransmissionType.Automatic;

    [Range(5, 5000, ErrorMessage = "Hourly rate must be between $5 and $5000")]
    public decimal HourlyRate { get; set; } = 25m;

    [Range(20, 20000, ErrorMessage = "Daily rate must be between $20 and $20000")]
    public decimal DailyRate { get; set; } = 120m;

    [Required(ErrorMessage = "Pickup location is required")]
    [StringLength(100)]
    public string PickupLocation { get; set; } = "Downtown Hub, New York";

    public string? ImageUrl { get; set; }
}

public class AdminCreatePromoCodeViewModel
{
    [Required(ErrorMessage = "Promo code is required")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    [Range(1, 1000, ErrorMessage = "Discount value must be greater than 0")]
    public decimal DiscountValue { get; set; } = 15m;

    [Range(0, 10000, ErrorMessage = "Min booking amount cannot be negative")]
    public decimal MinBookingAmount { get; set; } = 50m;

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow.Date;

    public DateTime ValidTo { get; set; } = DateTime.UtcNow.Date.AddMonths(3);

    public bool IsActive { get; set; } = true;
}
