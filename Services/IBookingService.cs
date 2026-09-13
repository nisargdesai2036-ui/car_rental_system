using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public interface IBookingService
{
    Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime pickup, DateTime returnTime, int? excludeBookingId = null);
    Task<(bool Success, string Message, PriceBreakdownViewModel? Price)> CalculatePriceAsync(int vehicleId, RentalType rentalType, DateTime pickup, DateTime returnTime, string? promoCode = null);
    Task<(bool Success, string Message, Booking? Booking)> CreateBookingAsync(BookingRequestViewModel request);
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<List<Booking>> GetUserBookingsAsync(int userId);
    Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId);
}
