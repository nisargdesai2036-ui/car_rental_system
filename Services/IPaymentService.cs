using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public interface IPaymentService
{
    Task<(bool Success, string Message, Payment? Payment)> ProcessPaymentAsync(MakePaymentViewModel model);
    Task<Payment?> GetPaymentByBookingIdAsync(int bookingId);
}
