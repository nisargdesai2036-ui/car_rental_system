using System.ComponentModel.DataAnnotations;

namespace wad_project.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [Display(Name = "Mobile Number")]
    public string MobileNumber { get; set; } = string.Empty;

    [Display(Name = "Driving License Number")]
    [StringLength(30, ErrorMessage = "Driving license number cannot exceed 30 characters.")]
    public string? DrivingLicenseNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be at least 4 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Email or Mobile number is required.")]
    [Display(Name = "Email or Mobile Number")]
    public string EmailOrMobile { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class UserProfileViewModel
{
    public int Id { get; set; }

    [Display(Name = "Full Name")]
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Mobile Number")]
    public string MobileNumber { get; set; } = string.Empty;

    [Display(Name = "Driving License Number")]
    public string? DrivingLicenseNumber { get; set; }

    public DateTime CreatedAt { get; set; }
}
