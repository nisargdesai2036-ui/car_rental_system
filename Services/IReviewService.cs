using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public interface IReviewService
{
    Task<(bool Success, string Message)> AddReviewAsync(int userId, AddReviewViewModel model);
    Task<List<Review>> GetVehicleReviewsAsync(int vehicleId);
}
