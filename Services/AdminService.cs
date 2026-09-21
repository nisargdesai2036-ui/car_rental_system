using Microsoft.EntityFrameworkCore;
using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
    {
        var totalUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
        var totalVehicles = await _context.Vehicles.CountAsync();
        var availableVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Available);
        var vehiclesUnderMaintenance = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.UnderMaintenance);

        var totalBookings = await _context.Bookings.CountAsync();
        var activeBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);
        var completedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Completed);
        var cancelledBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);
        var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);

        var recentBookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .ToListAsync();

        var totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Successful)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        return new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalVehicles = totalVehicles,
            AvailableVehicles = availableVehicles,
            VehiclesUnderMaintenance = vehiclesUnderMaintenance,
            TotalBookings = totalBookings,
            ActiveBookings = activeBookings,
            CompletedBookings = completedBookings,
            CancelledBookings = cancelledBookings,
            PendingBookings = pendingBookings,
            RecentBookings = recentBookings,
            TotalRevenue = totalRevenue
        };
    }
}
