using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.ViewModels;

public class MakePaymentViewModel
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.UPI;

    [Required]
    [Display(Name = "Payable Amount")]
    public decimal Amount { get; set; }
}
