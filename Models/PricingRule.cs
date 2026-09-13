using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wad_project.Models;

public class PricingRule
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public VehicleType? VehicleType { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Multiplier { get; set; } = 1.0m;

    public DayOfWeek? DayOfWeek { get; set; }

    public bool IsActive { get; set; } = true;
}
