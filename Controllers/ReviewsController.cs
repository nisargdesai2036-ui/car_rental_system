using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.DTOs;
using wad_project.Models;
using wad_project.Services;

namespace wad_project.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IReviewService _reviewService;

    public ReviewsController(ApplicationDbContext context, IReviewService reviewService)
    {
        _context = context;
        _reviewService = reviewService;
    }

    [AllowAnonymous]
    [HttpGet("/api/cars/{carId:int}/reviews")]
    public async Task<ActionResult<ApiResponse<List<ReviewResponseDto>>>> GetCarReviews(int carId)
    {
        var reviews = await _reviewService.GetVehicleReviewsAsync(carId);
        return Ok(ApiResponse<List<ReviewResponseDto>>.Ok(reviews.Select(MapToReviewDto).ToList()));
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse>> AddReview([FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Rating between 1-5 and comment are required."));
        }

        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("Authentication required to submit a review."));
        }

        var booking = await _context.Bookings.FindAsync(dto.BookingId);
        if (booking == null)
        {
            return NotFound(ApiResponse.Fail($"Booking with ID {dto.BookingId} not found."));
        }

        // Only the customer who completed the booking can review
        if (user.Role != UserRole.Admin && booking.UserId != user.Id)
        {
            return StatusCode(403, ApiResponse.Fail("You can only review a vehicle from your own completed booking."));
        }

        if (booking.Status != BookingStatus.Completed)
        {
            return BadRequest(ApiResponse.Fail("Reviews can only be submitted after the booking has been completed."));
        }

        var (success, message) = await _reviewService.AddReviewAsync(user.Id, new ViewModels.AddReviewViewModel
        {
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment
        });

        if (!success)
        {
            return BadRequest(ApiResponse.Fail(message));
        }

        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<ReviewResponseDto>.Fail("Authentication required."));
        }

        var review = await _context.Reviews.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        if (review == null)
        {
            return NotFound(ApiResponse<ReviewResponseDto>.Fail($"Review with ID {id} not found."));
        }

        if (user.Role != UserRole.Admin && review.UserId != user.Id)
        {
            return StatusCode(403, ApiResponse<ReviewResponseDto>.Fail("You do not have permission to edit this review."));
        }

        review.Rating = dto.Rating;
        review.Comment = dto.Comment.Trim();

        // Recalculate car average rating
        var car = await _context.Vehicles.FindAsync(review.VehicleId);
        if (car != null)
        {
            var allReviews = await _context.Reviews.Where(r => r.VehicleId == car.Id).ToListAsync();
            car.AverageRating = allReviews.Count > 0 ? Math.Round((decimal)allReviews.Average(r => r.Rating), 2) : 0;
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<ReviewResponseDto>.Ok(MapToReviewDto(review), "Review updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteReview(int id)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound(ApiResponse.Fail($"Review with ID {id} not found."));
        }

        if (user.Role != UserRole.Admin && review.UserId != user.Id)
        {
            return StatusCode(403, ApiResponse.Fail("You do not have permission to delete this review."));
        }

        var carId = review.VehicleId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        // Update car statistics
        var car = await _context.Vehicles.FindAsync(carId);
        if (car != null)
        {
            var remaining = await _context.Reviews.Where(r => r.VehicleId == car.Id).ToListAsync();
            car.ReviewCount = remaining.Count;
            car.AverageRating = remaining.Count > 0 ? Math.Round((decimal)remaining.Average(r => r.Rating), 2) : 0;
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse.Ok("Review deleted successfully."));
    }

    private async Task<User?> GetCurrentAppUserAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

        if (!string.IsNullOrEmpty(email))
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int parsedId))
        {
            return await _context.Users.FindAsync(parsedId);
        }

        return null;
    }

    private static ReviewResponseDto MapToReviewDto(Review r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        UserName = r.User?.FullName ?? "Customer",
        CarId = r.VehicleId,
        BookingId = r.BookingId,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };
}
