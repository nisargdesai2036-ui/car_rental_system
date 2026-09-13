using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wad_project.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    [Required]
    [StringLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    public VehicleType VehicleType { get; set; } = VehicleType.Sedan;

    public int SeatingCapacity { get; set; } = 5; // e.g. 2 for bikes, 5 for sedans

    public FuelType FuelType { get; set; } = FuelType.Petrol;

    public TransmissionType Transmission { get; set; } = TransmissionType.Manual;

    [Column(TypeName = "decimal(18,2)")]
    public decimal HourlyRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DailyRate { get; set; }

    [Required]
    [StringLength(100)]
    public string PickupLocation { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    [Column(TypeName = "decimal(3,2)")]
    public decimal AverageRating { get; set; } = 0.0m;

    public int ReviewCount { get; set; } = 0;

    // Relational Navigation Properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
