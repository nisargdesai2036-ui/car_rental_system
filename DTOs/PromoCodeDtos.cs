using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.DTOs;

public class CreatePromoCodeDto
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    [Range(1, 100000)]
    public decimal DiscountValue { get; set; }

    [Range(0, 1000000)]
    public decimal MinBookingAmount { get; set; } = 0m;

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;
}
