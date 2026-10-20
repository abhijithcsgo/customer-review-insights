using CustomerReviewInsights.Api.DTOs;

namespace CustomerReviewInsights.Api.Services;

public interface IReviewAnalysisService
{
    Task<ReviewAnalysisResult> AnalyzeAsync(string reviewText, CancellationToken cancellationToken = default);
}
