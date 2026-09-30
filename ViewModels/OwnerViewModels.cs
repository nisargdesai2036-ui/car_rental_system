using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.ViewModels;

public class OwnerDashboardViewModel
{
    public decimal TotalEarnings { get; set; }
    public int TotalVehicles { get; set; }
    public int ApprovedVehicles { get; set; }
    public int PendingVehicles { get; set; }
    public int BookedVehicles { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }

    public List<Vehicle> MyVehicles { get; set; } = new();
    public List<Booking> MyVehicleBookings { get; set; } = new();

    public CreateOwnerVehicleViewModel NewVehicle { get; set; } = new();
}

public class CreateOwnerVehicleViewModel
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
