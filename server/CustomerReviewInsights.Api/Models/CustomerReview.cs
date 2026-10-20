namespace CustomerReviewInsights.Api.Models;

public class CustomerReview
{
    public int Id { get; set; }

    public required string ReviewText { get; set; }

    public required string Sentiment { get; set; }

    public required string Category { get; set; }

    public required string Summary { get; set; }

    public DateTime CreatedAt { get; set; }
}
