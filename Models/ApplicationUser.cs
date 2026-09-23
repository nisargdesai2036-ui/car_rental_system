using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace wad_project.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? DrivingLicenseNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
