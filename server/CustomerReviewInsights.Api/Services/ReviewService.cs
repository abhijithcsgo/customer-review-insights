using Microsoft.EntityFrameworkCore;
using CustomerReviewInsights.Api.Data;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Models;

namespace CustomerReviewInsights.Api.Services;

public class ReviewService(
    AppDbContext dbContext,
    IReviewAnalysisService analysisService,
    ILogger<ReviewService> logger) : IReviewService
{
    public async Task<IReadOnlyList<ReviewResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var reviews = await dbContext.CustomerReviews
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(ReviewResponse.FromEntity).ToList();
    }

    public async Task<ReviewResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var review = await dbContext.CustomerReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return review is null ? null : ReviewResponse.FromEntity(review);
    }

    public async Task<ReviewResponse> CreateAsync(
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var reviewText = request.ReviewText.Trim();
        if (string.IsNullOrWhiteSpace(reviewText))
        {
            throw new ArgumentException("Review text is required.");
        }

        if (reviewText.Length > ReviewAnalysisConstants.MaxReviewLength)
        {
            throw new ArgumentException(
                $"Review text must be at most {ReviewAnalysisConstants.MaxReviewLength} characters.");
        }

        var analysis = await analysisService.AnalyzeAsync(reviewText, cancellationToken);

        var entity = new CustomerReview
        {
            ReviewText = reviewText,
            Sentiment = analysis.Sentiment,
            Category = analysis.Category,
            Summary = analysis.Summary,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.CustomerReviews.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist customer review.");
            throw new InvalidOperationException("Unable to save the review. Please try again later.");
        }

        return ReviewResponse.FromEntity(entity);
    }
}
