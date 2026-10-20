namespace CustomerReviewInsights.Api.DTOs;

public class ReviewAnalysisResult
{
    public required string Sentiment { get; set; }
    public required string Category { get; set; }
    public required string Summary { get; set; }
}
