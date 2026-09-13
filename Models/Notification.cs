using System.ComponentModel.DataAnnotations;

namespace wad_project.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(15)]
    public string RecipientPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.BookingConfirmation;

    public bool IsSent { get; set; } = true;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
