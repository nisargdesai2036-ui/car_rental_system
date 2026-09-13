using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class ReviewService : IReviewService
{
    private readonly IDataStore _dataStore;

    public ReviewService(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<(bool Success, string Message)> AddReviewAsync(int userId, AddReviewViewModel model)
    {
        lock (_dataStore.Bookings)
        {
            var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == model.BookingId);
            if (booking == null)
            {
                return Task.FromResult((false, "Booking not found."));
            }

            // 1. Must be the customer who made the booking
            if (booking.UserId != userId)
            {
                return Task.FromResult((false, "You can only review vehicles for your own bookings."));
            }

            // 2. Booking must be completed
            if (booking.Status != BookingStatus.Completed)
            {
                return Task.FromResult((false, "You can only review a vehicle after your booking is completed."));
            }

            lock (_dataStore.Reviews)
            {
                // 3. Prevent duplicate reviews for the same booking
                if (_dataStore.Reviews.Any(r => r.BookingId == model.BookingId))
                {
                    return Task.FromResult((false, "You have already reviewed this booking."));
                }

                var review = new Review
                {
                    Id = _dataStore.NextReviewId(),
                    UserId = userId,
                    User = booking.User,
                    VehicleId = booking.VehicleId,
                    Vehicle = booking.Vehicle,
                    BookingId = booking.Id,
                    Booking = booking,
                    Rating = model.Rating,
                    Comment = model.Comment.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _dataStore.Reviews.Add(review);
                booking.Review = review;
            }

            // 4. Update vehicle rating and review count
            lock (_dataStore.Vehicles)
            {
                var vehicle = _dataStore.Vehicles.FirstOrDefault(v => v.Id == booking.VehicleId);
                if (vehicle != null)
                {
                    lock (_dataStore.Reviews)
                    {
                        var reviews = _dataStore.Reviews.Where(r => r.VehicleId == vehicle.Id).ToList();
                        vehicle.ReviewCount = reviews.Count;
                        vehicle.AverageRating = Math.Round((decimal)reviews.Average(r => r.Rating), 2);
                    }
                }
            }

            return Task.FromResult((true, "Thank you! Your review has been submitted successfully."));
        }
    }

    public Task<List<Review>> GetVehicleReviewsAsync(int vehicleId)
    {
        lock (_dataStore.Reviews)
        {
            var list = _dataStore.Reviews
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return Task.FromResult(list);
        }
    }
}
