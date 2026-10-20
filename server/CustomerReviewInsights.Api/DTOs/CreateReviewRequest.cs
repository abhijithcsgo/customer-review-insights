using System.ComponentModel.DataAnnotations;
using CustomerReviewInsights.Api.Models;

namespace CustomerReviewInsights.Api.DTOs;

public class CreateReviewRequest
{
    [Required(ErrorMessage = "Review text is required.")]
    [MinLength(1, ErrorMessage = "Review text cannot be empty.")]
    [MaxLength(ReviewAnalysisConstants.MaxReviewLength,
        ErrorMessage = "Review text must be at most {1} characters.")]
    public string ReviewText { get; set; } = string.Empty;
}
