using wad_project.Models;

namespace wad_project.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int VehiclesUnderMaintenance { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<Booking> RecentBookings { get; set; } = new();
}
