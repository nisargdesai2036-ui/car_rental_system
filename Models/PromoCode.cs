using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wad_project.Models;

public class PromoCode
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinBookingAmount { get; set; } = 0m;

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;
}
