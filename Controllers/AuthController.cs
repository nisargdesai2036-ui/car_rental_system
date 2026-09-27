using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wad_project.DTOs;
using wad_project.Models;
using wad_project.Services;

namespace wad_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        IUserService userService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _userService = userService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<UserDto>.Fail(errors));
        }

        var (success, message, user) = await _userService.RegisterAsync(new ViewModels.RegisterViewModel
        {
            FullName = dto.FullName,
            Email = dto.Email,
            MobileNumber = dto.MobileNumber,
            DrivingLicenseNumber = dto.DrivingLicenseNumber,
            Password = dto.Password,
            ConfirmPassword = dto.ConfirmPassword
        });

        if (!success || user == null)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(message));
        }

        // Ensure Identity user and role
        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        if (identityUser == null)
        {
            identityUser = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.MobileNumber,
                DrivingLicenseNumber = user.DrivingLicenseNumber,
                VerificationStatus = user.VerificationStatus,
                IsActive = user.IsActive
            };
            var createResult = await _userManager.CreateAsync(identityUser, user.Password);
            if (!createResult.Succeeded)
            {
                await _userManager.CreateAsync(identityUser, "DriveEase@1234");
            }
        }

        if (!await _userManager.IsInRoleAsync(identityUser, "Customer"))
        {
            await _userManager.AddToRoleAsync(identityUser, "Customer");
        }

        await _signInManager.SignInAsync(identityUser, isPersistent: true);

        var userDto = MapToUserDto(user);
        return CreatedAtAction(nameof(GetMe), ApiResponse<UserDto>.Ok(userDto, "Customer registration successful. Pending verification."));
    }

    [HttpPost("register-owner")]
    public async Task<ActionResult<ApiResponse<UserDto>>> RegisterOwner([FromBody] RegisterOwnerDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<UserDto>.Fail(errors));
        }

        var (success, message, user) = await _userService.RegisterOwnerAsync(new ViewModels.RegisterViewModel
        {
            FullName = dto.FullName,
            Email = dto.Email,
            MobileNumber = dto.MobileNumber,
            DrivingLicenseNumber = dto.DrivingLicenseNumber,
            Password = dto.Password,
            ConfirmPassword = dto.ConfirmPassword
        });

        if (!success || user == null)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(message));
        }

        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        if (identityUser == null)
        {
            identityUser = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.MobileNumber,
                DrivingLicenseNumber = user.DrivingLicenseNumber,
                VerificationStatus = user.VerificationStatus,
                IsActive = user.IsActive
            };
            var createResult = await _userManager.CreateAsync(identityUser, user.Password);
            if (!createResult.Succeeded)
            {
                await _userManager.CreateAsync(identityUser, "DriveEase@1234");
            }
        }

        if (!await _userManager.IsInRoleAsync(identityUser, "Owner"))
        {
            await _userManager.AddToRoleAsync(identityUser, "Owner");
        }

        await _signInManager.SignInAsync(identityUser, isPersistent: true);

        var userDto = MapToUserDto(user);
        return CreatedAtAction(nameof(GetMe), ApiResponse<UserDto>.Ok(userDto, "Owner registration submitted. Pending admin verification."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Invalid login payload."));
        }

        var (success, message, user) = await _userService.LoginAsync(new ViewModels.LoginViewModel
        {
            EmailOrMobile = dto.EmailOrMobile,
            Password = dto.Password
        });

        if (!success || user == null)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(message));
        }

        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        if (identityUser == null)
        {
            identityUser = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.MobileNumber,
                DrivingLicenseNumber = user.DrivingLicenseNumber,
                IsActive = user.IsActive
            };
            var createResult = await _userManager.CreateAsync(identityUser, user.Password);
            if (!createResult.Succeeded)
            {
                await _userManager.CreateAsync(identityUser, "DriveEase@1234");
            }
        }

        await _signInManager.SignInAsync(identityUser, isPersistent: true);

        return Ok(ApiResponse<UserDto>.Ok(MapToUserDto(user), "Login successful."));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe()
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("User is not authenticated."));
        }

        return Ok(ApiResponse<UserDto>.Ok(MapToUserDto(user)));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("User is not authenticated."));
        }

        user.FullName = dto.FullName.Trim();
        user.MobileNumber = dto.MobileNumber.Trim();
        user.DrivingLicenseNumber = dto.DrivingLicenseNumber?.Trim().ToUpperInvariant();

        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        if (identityUser != null)
        {
            identityUser.FullName = user.FullName;
            identityUser.PhoneNumber = user.MobileNumber;
            identityUser.DrivingLicenseNumber = user.DrivingLicenseNumber;
            await _userManager.UpdateAsync(identityUser);
        }

        return Ok(ApiResponse<UserDto>.Ok(MapToUserDto(user), "Profile updated successfully."));
    }

    private async Task<User?> GetCurrentAppUserAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

        if (string.IsNullOrEmpty(email))
        {
            return null;
        }

        var allUsers = await _userService.GetAllUsersAsync();
        return allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    private static UserDto MapToUserDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        MobileNumber = u.MobileNumber,
        DrivingLicenseNumber = u.DrivingLicenseNumber,
        Role = u.Role,
        VerificationStatus = u.VerificationStatus,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
