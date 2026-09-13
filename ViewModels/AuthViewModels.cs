using System.ComponentModel.DataAnnotations;

namespace wad_project.ViewModels;

/// <summary>
/// Model used for user registration with DataAnnotations for Razor asp-validation-for tag helpers.
/// </summary>
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

    [Required(ErrorMessage = "Driving License Number is required for self-drive rental.")]
    [StringLength(30, ErrorMessage = "Driving license number cannot exceed 30 characters.")]
    [Display(Name = "Driving License Number")]
    public string DrivingLicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Model used for user login with DataAnnotations for Razor asp-validation-for tag helpers.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Email or Mobile number is required.")]
    [Display(Name = "Email or Mobile Number")]
    public string EmailOrMobile { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
