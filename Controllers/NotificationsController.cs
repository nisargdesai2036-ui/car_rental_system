using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.DTOs;
using wad_project.Models;

namespace wad_project.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetMyNotifications()
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse<List<NotificationResponseDto>>.Fail("Authentication required."));
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.SentAt)
            .Take(20)
            .Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                Type = n.Type,
                IsSent = n.IsSent,
                SentAt = n.SentAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<NotificationResponseDto>>.Ok(notifications));
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
    {
        var user = await GetCurrentAppUserAsync();
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
        {
            return NotFound(ApiResponse.Fail($"Notification with ID {id} not found."));
        }

        // Prevent IDOR: only notification recipient can mark it read
        if (user.Role != UserRole.Admin && notification.UserId != user.Id)
        {
            return StatusCode(403, ApiResponse.Fail("You cannot modify another user's notification."));
        }

        notification.IsSent = true;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Notification marked as read."));
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
}
