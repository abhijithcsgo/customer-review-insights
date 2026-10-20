using System.Text.Json;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Models;

namespace CustomerReviewInsights.Api.Services;

public static class ReviewAnalysisValidation
{
    public static ReviewAnalysisResult ParseAndValidate(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("AI response was empty.");
        }

        ReviewAnalysisResult? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<ReviewAnalysisResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("AI response was not valid JSON.", ex);
        }

        if (parsed is null)
        {
            throw new InvalidOperationException("AI response could not be deserialized.");
        }

        return Validate(parsed);
    }

    public static ReviewAnalysisResult Validate(ReviewAnalysisResult result)
    {
        if (string.IsNullOrWhiteSpace(result.Sentiment))
        {
            throw new InvalidOperationException("AI response is missing sentiment.");
        }

        if (!ReviewAnalysisConstants.AllowedSentiments.Contains(result.Sentiment))
        {
            throw new InvalidOperationException($"AI returned an unsupported sentiment: '{result.Sentiment}'.");
        }

        if (string.IsNullOrWhiteSpace(result.Category))
        {
            throw new InvalidOperationException("AI response is missing category.");
        }

        if (!ReviewAnalysisConstants.AllowedCategories.Contains(result.Category))
        {
            throw new InvalidOperationException($"AI returned an unsupported category: '{result.Category}'.");
        }

        if (string.IsNullOrWhiteSpace(result.Summary))
        {
            throw new InvalidOperationException("AI response is missing summary.");
        }

        var trimmedSummary = result.Summary.Trim();
        if (trimmedSummary.Length > ReviewAnalysisConstants.MaxSummaryLength)
        {
            throw new InvalidOperationException(
                $"AI summary exceeds {ReviewAnalysisConstants.MaxSummaryLength} characters.");
        }

        var wordCount = CountWords(trimmedSummary);
        if (wordCount > ReviewAnalysisConstants.MaxSummaryWords)
        {
            throw new InvalidOperationException(
                $"AI summary must contain at most {ReviewAnalysisConstants.MaxSummaryWords} words (received {wordCount}).");
        }

        return new ReviewAnalysisResult
        {
            Sentiment = ReviewAnalysisConstants.NormalizeSentiment(result.Sentiment),
            Category = ReviewAnalysisConstants.NormalizeCategory(result.Category),
            Summary = trimmedSummary
        };
    }

    public static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return text
            .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }
}
