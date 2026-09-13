using wad_project.ViewModels;

namespace wad_project.Services;

public interface IAdminService
{
    Task<AdminDashboardViewModel> GetDashboardStatsAsync();
}
