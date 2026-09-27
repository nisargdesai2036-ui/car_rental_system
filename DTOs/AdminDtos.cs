namespace wad_project.DTOs;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOwners { get; set; }
    public int TotalCars { get; set; }
    public int PendingOwners { get; set; }
    public int PendingCars { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}
