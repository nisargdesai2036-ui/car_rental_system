using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public interface IVehicleService
{
    Task<List<Vehicle>> GetAllVehiclesAsync();
    Task<List<Vehicle>> SearchAndFilterAsync(VehicleSearchFilterViewModel filter);
    Task<Vehicle?> GetVehicleByIdAsync(int id);
    Task<List<RecommendedVehicleViewModel>> GetRecommendationsAsync(RecommendationRequestViewModel criteria);
    Task<(bool Success, string Message)> AddVehicleAsync(Vehicle vehicle);
    Task<bool> UpdateStatusAsync(int vehicleId, VehicleStatus status);
}
