using System.ComponentModel.DataAnnotations;

namespace wad_project.Models;

public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
