using Microsoft.EntityFrameworkCore;
using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> AddReviewAsync(int userId, AddReviewViewModel model)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .FirstOrDefaultAsync(b => b.Id == model.BookingId);

        if (booking == null)
        {
            return (false, "Booking not found.");
        }

        // 1. Must be the customer who made the booking
        if (booking.UserId != userId)
        {
            return (false, "You can only review vehicles for your own bookings.");
        }

        // 2. Booking must be completed
        if (booking.Status != BookingStatus.Completed)
        {
            return (false, "You can only review a vehicle after your booking is completed.");
        }

        // 3. Prevent duplicate reviews for the same booking
        if (await _context.Reviews.AnyAsync(r => r.BookingId == model.BookingId))
        {
            return (false, "You have already reviewed this booking.");
        }

        var review = new Review
        {
            UserId = userId,
            VehicleId = booking.VehicleId,
            BookingId = booking.Id,
            Rating = model.Rating,
            Comment = model.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Reviews.AddAsync(review);
        booking.Review = review;
        await _context.SaveChangesAsync();

        // 4. Update vehicle rating and review count
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == booking.VehicleId);
        if (vehicle != null)
        {
            var reviews = await _context.Reviews.Where(r => r.VehicleId == vehicle.Id).ToListAsync();
            vehicle.ReviewCount = reviews.Count;
            vehicle.AverageRating = reviews.Count > 0 ? Math.Round((decimal)reviews.Average(r => r.Rating), 2) : 0;
            await _context.SaveChangesAsync();
        }

        return (true, "Thank you! Your review has been submitted successfully.");
    }

    public async Task<List<Review>> GetVehicleReviewsAsync(int vehicleId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
