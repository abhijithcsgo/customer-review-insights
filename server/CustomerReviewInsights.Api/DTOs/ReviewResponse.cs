using CustomerReviewInsights.Api.Models;

namespace CustomerReviewInsights.Api.DTOs;

public class ReviewResponse
{
    public int Id { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static ReviewResponse FromEntity(CustomerReview review) => new()
    {
        Id = review.Id,
        ReviewText = review.ReviewText,
        Sentiment = review.Sentiment,
        Category = review.Category,
        Summary = review.Summary,
        CreatedAt = review.CreatedAt
    };
}
