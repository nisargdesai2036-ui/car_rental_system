using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.ViewModels;

public class BookingRequestViewModel
{
    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [Display(Name = "Rental Plan")]
    public RentalType RentalType { get; set; } = RentalType.Daily;

    [Required(ErrorMessage = "Pickup date and time is required.")]
    [Display(Name = "Pickup Date & Time")]
    public DateTime PickupDateTime { get; set; } = DateTime.UtcNow.AddHours(2);

    [Required(ErrorMessage = "Return date and time is required.")]
    [Display(Name = "Return Date & Time")]
    public DateTime ReturnDateTime { get; set; } = DateTime.UtcNow.AddDays(1);

    [Required(ErrorMessage = "Pickup location is required.")]
    [Display(Name = "Pickup Location")]
    public string PickupLocation { get; set; } = string.Empty;

    [Display(Name = "Promo Code (Optional)")]
    public string? PromoCode { get; set; }
}

public class PriceBreakdownViewModel
{
    public RentalType RentalType { get; set; }
    public decimal Duration { get; set; }
    public decimal UnitRate { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DynamicMultiplier { get; set; } = 1.0m;
    public string? AppliedPromoCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
