using Microsoft.EntityFrameworkCore;
using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        var phone = model.MobileNumber.Trim();

        // Business Rule 1: Check for duplicate email
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
        {
            return (false, "An account with this email address already exists.", null);
        }

        // Business Rule 2: Check for duplicate mobile number
        if (await _context.Users.AnyAsync(u => u.MobileNumber == phone))
        {
            return (false, "An account with this mobile number already exists.", null);
        }

        var newUser = new User
        {
            FullName = model.FullName.Trim(),
            Email = email,
            MobileNumber = phone,
            DrivingLicenseNumber = model.DrivingLicenseNumber.Trim().ToUpperInvariant(),
            Password = model.Password,
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        return (true, "Registration successful.", newUser);
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model)
    {
        var input = model.EmailOrMobile.Trim();

        // Business Rule 3: Find user by email or phone
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == input.ToLower() ||
            u.MobileNumber == input);

        if (user == null)
        {
            return (false, "Invalid email/mobile or password.", null);
        }

        // Business Rule 4: Verify account is active
        if (!user.IsActive)
        {
            return (false, "Your account is deactivated. Please contact support.", null);
        }

        // Business Rule 5: Verify password matches
        if (user.Password != model.Password)
        {
            return (false, "Invalid email/mobile or password.", null);
        }

        return (true, "Login successful.", user);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.OrderBy(u => u.Id).ToListAsync();
    }

    public async Task<bool> ToggleUserStatusAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }
}
