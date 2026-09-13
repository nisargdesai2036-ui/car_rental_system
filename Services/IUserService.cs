using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public interface IUserService
{
    Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model);
    Task<User?> GetUserByIdAsync(int id);
    Task<List<User>> GetAllUsersAsync();
    Task<bool> ToggleUserStatusAsync(int userId);
}
