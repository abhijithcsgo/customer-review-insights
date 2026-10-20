using System.ComponentModel.DataAnnotations;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Models;

namespace CustomerReviewInsights.Tests;

public class CreateReviewRequestValidationTests
{
    [Fact]
    public void CreateReviewRequest_Requires_ReviewText()
    {
        var request = new CreateReviewRequest { ReviewText = string.Empty };
        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase)
            || r.ErrorMessage!.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateReviewRequest_Rejects_Too_Long_Review()
    {
        var request = new CreateReviewRequest
        {
            ReviewText = new string('a', ReviewAnalysisConstants.MaxReviewLength + 1)
        };

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage!.Contains("5000"));
    }

    [Fact]
    public void CreateReviewRequest_Accepts_Valid_Review()
    {
        var request = new CreateReviewRequest
        {
            ReviewText = "The application crashes whenever I open order history."
        };

        var results = Validate(request);

        Assert.Empty(results);
    }

    private static IList<ValidationResult> Validate(CreateReviewRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }
}
