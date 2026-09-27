using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.DTOs;

public class CreateBookingDto
{
    [Required]
    public int CarId { get; set; }

    public RentalType RentalType { get; set; } = RentalType.Daily;

    [Required]
    public DateTime PickupDateTime { get; set; }

    [Required]
    public DateTime ReturnDateTime { get; set; }

    [Required]
    [StringLength(100)]
    public string PickupLocation { get; set; } = string.Empty;

    public string? PromoCode { get; set; }
}

public class BookingResponseDto
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public RentalType RentalType { get; set; }
    public DateTime PickupDateTime { get; set; }
    public DateTime ReturnDateTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public decimal Duration { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PromoCode { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateBookingStatusDto
{
    [Required]
    public BookingStatus Status { get; set; }
}
