using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class AdminService : IAdminService
{
    private readonly IDataStore _dataStore;

    public AdminService(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<AdminDashboardViewModel> GetDashboardStatsAsync()
    {
        var vm = new AdminDashboardViewModel();

        lock (_dataStore.Users)
        {
            vm.TotalUsers = _dataStore.Users.Count(u => u.Role == UserRole.Customer);
        }

        lock (_dataStore.Vehicles)
        {
            vm.TotalVehicles = _dataStore.Vehicles.Count;
            vm.AvailableVehicles = _dataStore.Vehicles.Count(v => v.Status == VehicleStatus.Available);
            vm.VehiclesUnderMaintenance = _dataStore.Vehicles.Count(v => v.Status == VehicleStatus.UnderMaintenance);
        }

        lock (_dataStore.Bookings)
        {
            vm.TotalBookings = _dataStore.Bookings.Count;
            vm.ActiveBookings = _dataStore.Bookings.Count(b => b.Status == BookingStatus.Confirmed);
            vm.CompletedBookings = _dataStore.Bookings.Count(b => b.Status == BookingStatus.Completed);
            vm.CancelledBookings = _dataStore.Bookings.Count(b => b.Status == BookingStatus.Cancelled);
            vm.PendingBookings = _dataStore.Bookings.Count(b => b.Status == BookingStatus.Pending);

            vm.RecentBookings = _dataStore.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToList();
        }

        lock (_dataStore.Payments)
        {
            vm.TotalRevenue = _dataStore.Payments
                .Where(p => p.Status == PaymentStatus.Successful)
                .Sum(p => p.Amount);
        }

        return Task.FromResult(vm);
    }
}
