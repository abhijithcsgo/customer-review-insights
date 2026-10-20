namespace CustomerReviewInsights.Api.Models;

public static class ReviewAnalysisConstants
{
    public const int MaxReviewLength = 5000;
    public const int MaxSummaryWords = 10;
    public const int MaxSummaryLength = 200;

    public static readonly HashSet<string> AllowedSentiments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Positive",
        "Neutral",
        "Negative"
    };

    public static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pricing",
        "Bug",
        "Usability",
        "Feature Request",
        "Other"
    };

    public static string NormalizeSentiment(string sentiment)
    {
        return AllowedSentiments.First(s => s.Equals(sentiment, StringComparison.OrdinalIgnoreCase));
    }

    public static string NormalizeCategory(string category)
    {
        return AllowedCategories.First(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));
    }
}
