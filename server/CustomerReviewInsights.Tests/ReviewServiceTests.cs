using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CustomerReviewInsights.Api.Data;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Models;
using CustomerReviewInsights.Api.Services;

namespace CustomerReviewInsights.Tests;

public class ReviewServiceTests
{
    [Fact]
    public async Task CreateAsync_Persists_Analyzed_Review()
    {
        await using var db = CreateDbContext();
        var analysis = new Mock<IReviewAnalysisService>();
        analysis
            .Setup(s => s.AnalyzeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReviewAnalysisResult
            {
                Sentiment = "Negative",
                Category = "Bug",
                Summary = "App crashes opening order history"
            });

        var service = new ReviewService(db, analysis.Object, NullLogger<ReviewService>.Instance);

        var created = await service.CreateAsync(new CreateReviewRequest
        {
            ReviewText = "The application crashes whenever I open order history."
        });

        Assert.True(created.Id > 0);
        Assert.Equal("Negative", created.Sentiment);
        Assert.Equal("Bug", created.Category);
        Assert.Equal(1, await db.CustomerReviews.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_Does_Not_Persist_When_Analysis_Fails()
    {
        await using var db = CreateDbContext();
        var analysis = new Mock<IReviewAnalysisService>();
        analysis
            .Setup(s => s.AnalyzeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI analysis failed."));

        var service = new ReviewService(db, analysis.Object, NullLogger<ReviewService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateReviewRequest
            {
                ReviewText = "This should not be saved."
            }));

        Assert.Equal(0, await db.CustomerReviews.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_Returns_Newest_First()
    {
        await using var db = CreateDbContext();
        db.CustomerReviews.AddRange(
            new CustomerReview
            {
                ReviewText = "Older review",
                Sentiment = "Neutral",
                Category = "Other",
                Summary = "Older summary",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new CustomerReview
            {
                ReviewText = "Newer review",
                Sentiment = "Positive",
                Category = "Usability",
                Summary = "Newer summary",
                CreatedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var analysis = new Mock<IReviewAnalysisService>();
        var service = new ReviewService(db, analysis.Object, NullLogger<ReviewService>.Instance);

        var reviews = await service.GetAllAsync();

        Assert.Equal(2, reviews.Count);
        Assert.Equal("Newer review", reviews[0].ReviewText);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
