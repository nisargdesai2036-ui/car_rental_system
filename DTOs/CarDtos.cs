using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.DTOs;

public class CreateCarDto
{
    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2030)]
    public int Year { get; set; }

    [Required]
    [StringLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    public VehicleType VehicleType { get; set; } = VehicleType.Sedan;

    [Range(1, 20)]
    public int SeatingCapacity { get; set; } = 5;

    public FuelType FuelType { get; set; } = FuelType.Petrol;

    public TransmissionType Transmission { get; set; } = TransmissionType.Manual;

    [Range(1, 100000)]
    public decimal HourlyRate { get; set; }

    [Range(1, 1000000)]
    public decimal DailyRate { get; set; }

    [Required]
    [StringLength(100)]
    public string PickupLocation { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
}

public class UpdateCarDto : CreateCarDto
{
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
}

public class CarQueryParameters
{
    public string? Location { get; set; }
    public VehicleType? Type { get; set; }
    public FuelType? FuelType { get; set; }
    public TransmissionType? Transmission { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } // price_asc, price_desc, rating
}

public class CarRecommendationRequestDto
{
    public int Passengers { get; set; } = 4;
    public decimal Budget { get; set; } = 5000;
    public int DurationDays { get; set; } = 1;
    public VehicleType? PreferredType { get; set; }
}

public class SetAvailabilityDto
{
    public bool IsAvailable { get; set; }
}

public class SetMaintenanceDto
{
    public bool IsUnderMaintenance { get; set; }
}
