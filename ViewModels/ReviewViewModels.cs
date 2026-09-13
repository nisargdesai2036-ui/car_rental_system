using System.ComponentModel.DataAnnotations;

namespace wad_project.ViewModels;

public class AddReviewViewModel
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    [Display(Name = "Rating (1 - 5 Stars)")]
    public int Rating { get; set; } = 5;

    [Required(ErrorMessage = "Please write a comment.")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Review must be between 5 and 1000 characters.")]
    [Display(Name = "Your Review")]
    public string Comment { get; set; } = string.Empty;
}
