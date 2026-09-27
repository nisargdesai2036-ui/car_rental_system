using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [Phone]
    public string MobileNumber { get; set; } = string.Empty;

    [StringLength(30)]
    public string? DrivingLicenseNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 4)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "Email or mobile number is required.")]
    public string EmailOrMobile { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string MobileNumber { get; set; } = string.Empty;

    [StringLength(30)]
    public string? DrivingLicenseNumber { get; set; }
}

public class RegisterOwnerDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [Phone]
    public string MobileNumber { get; set; } = string.Empty;

    [StringLength(30)]
    public string? DrivingLicenseNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 4)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? DrivingLicenseNumber { get; set; }
    public UserRole Role { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
