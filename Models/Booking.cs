using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wad_project.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string BookingReference { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public RentalType RentalType { get; set; } = RentalType.Daily;

    public DateTime PickupDateTime { get; set; }

    public DateTime ReturnDateTime { get; set; }

    [Required]
    [StringLength(100)]
    public string PickupLocation { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RentalDuration { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0.0m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; } // BasePrice - DiscountAmount

    public int? PromoCodeId { get; set; }
    public PromoCode? PromoCode { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relational Navigations
    public Payment? Payment { get; set; }
    public Review? Review { get; set; }
}
