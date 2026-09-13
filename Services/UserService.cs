using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class UserService : IUserService
{
    private readonly IDataStore _dataStore;

    public UserService(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        var phone = model.MobileNumber.Trim();

        lock (_dataStore.Users)
        {
            // Business Rule 1: Check for duplicate email (not checkable on the view alone)
            if (_dataStore.Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult<(bool, string, User?)>((false, "An account with this email address already exists.", null));
            }

            // Business Rule 2: Check for duplicate mobile number (not checkable on the view alone)
            if (_dataStore.Users.Any(u => u.MobileNumber == phone))
            {
                return Task.FromResult<(bool, string, User?)>((false, "An account with this mobile number already exists.", null));
            }

            var newUser = new User
            {
                Id = _dataStore.NextUserId(),
                FullName = model.FullName.Trim(),
                Email = email,
                MobileNumber = phone,
                DrivingLicenseNumber = model.DrivingLicenseNumber.Trim().ToUpperInvariant(),
                Password = model.Password, // Simple, direct password storage
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dataStore.Users.Add(newUser);
            return Task.FromResult<(bool, string, User?)>((true, "Registration successful.", newUser));
        }
    }

    public Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model)
    {
        var input = model.EmailOrMobile.Trim();

        lock (_dataStore.Users)
        {
            // Business Rule 3: Find user by email or phone
            var user = _dataStore.Users.FirstOrDefault(u =>
                u.Email.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                u.MobileNumber == input);

            if (user == null)
            {
                return Task.FromResult<(bool, string, User?)>((false, "Invalid email/mobile or password.", null));
            }

            // Business Rule 4: Verify account is active
            if (!user.IsActive)
            {
                return Task.FromResult<(bool, string, User?)>((false, "Your account is deactivated. Please contact support.", null));
            }

            // Business Rule 5: Verify password matches
            if (user.Password != model.Password)
            {
                return Task.FromResult<(bool, string, User?)>((false, "Invalid email/mobile or password.", null));
            }

            return Task.FromResult<(bool, string, User?)>((true, "Login successful.", user));
        }
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        lock (_dataStore.Users)
        {
            var user = _dataStore.Users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }
    }

    public Task<List<User>> GetAllUsersAsync()
    {
        lock (_dataStore.Users)
        {
            return Task.FromResult(_dataStore.Users.ToList());
        }
    }

    public Task<bool> ToggleUserStatusAsync(int userId)
    {
        lock (_dataStore.Users)
        {
            var user = _dataStore.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return Task.FromResult(false);

            user.IsActive = !user.IsActive;
            return Task.FromResult(true);
        }
    }
}
