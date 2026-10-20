using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Services;

namespace CustomerReviewInsights.Tests;

public class ReviewAnalysisValidationTests
{
    [Theory]
    [InlineData("Positive", "Pricing", "Price feels fair for value")]
    [InlineData("Neutral", "Usability", "Interface is okay overall")]
    [InlineData("Negative", "Bug", "App crashes opening order history")]
    [InlineData("positive", "feature request", "Please add dark mode support")]
    public void Validate_Accepts_Valid_Analysis(string sentiment, string category, string summary)
    {
        var result = ReviewAnalysisValidation.Validate(new ReviewAnalysisResult
        {
            Sentiment = sentiment,
            Category = category,
            Summary = summary
        });

        Assert.Contains(result.Sentiment, new[] { "Positive", "Neutral", "Negative" });
        Assert.Contains(result.Category, new[] { "Pricing", "Bug", "Usability", "Feature Request", "Other" });
        Assert.Equal(summary, result.Summary);
    }

    [Theory]
    [InlineData("Happy")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Rejects_Invalid_Sentiment(string sentiment)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ReviewAnalysisValidation.Validate(new ReviewAnalysisResult
            {
                Sentiment = sentiment,
                Category = "Bug",
                Summary = "App crashes often"
            }));

        Assert.Contains("sentiment", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Performance")]
    [InlineData("")]
    public void Validate_Rejects_Invalid_Category(string category)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ReviewAnalysisValidation.Validate(new ReviewAnalysisResult
            {
                Sentiment = "Negative",
                Category = category,
                Summary = "App crashes often"
            }));

        Assert.Contains("category", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_Rejects_Summary_Over_Ten_Words()
    {
        var summary = "This summary intentionally contains more than ten words in total now";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ReviewAnalysisValidation.Validate(new ReviewAnalysisResult
            {
                Sentiment = "Neutral",
                Category = "Other",
                Summary = summary
            }));

        Assert.Contains("10 words", ex.Message);
        Assert.Equal(11, ReviewAnalysisValidation.CountWords(summary));
    }

    [Fact]
    public void ParseAndValidate_Accepts_Valid_Json()
    {
        const string json = """
            {
              "sentiment": "Negative",
              "category": "Bug",
              "summary": "App crashes when opening order history"
            }
            """;

        var result = ReviewAnalysisValidation.ParseAndValidate(json);

        Assert.Equal("Negative", result.Sentiment);
        Assert.Equal("Bug", result.Category);
        Assert.Equal("App crashes when opening order history", result.Summary);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{ not-json }")]
    [InlineData("""{"sentiment":"Negative"}""")]
    public void ParseAndValidate_Rejects_Invalid_Responses(string json)
    {
        Assert.ThrowsAny<Exception>(() => ReviewAnalysisValidation.ParseAndValidate(json));
    }
}
