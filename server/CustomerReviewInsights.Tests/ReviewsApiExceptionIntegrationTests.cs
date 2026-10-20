using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Exceptions;
using CustomerReviewInsights.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CustomerReviewInsights.Tests;

public class ReviewsApiExceptionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly WebApplicationFactory<Program> _factory;

    public ReviewsApiExceptionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_unexpected_exception_returns_500_without_sensitive_details()
    {
        var reviewService = new Mock<IReviewService>();
        reviewService
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SqlException: Login failed for user 'sa' with password=SuperSecret123"));

        await using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IReviewService>();
                services.AddScoped(_ => reviewService.Object);
            });
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/reviews");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("An unexpected error occurred.", payload.Error);
        Assert.Equal("An unexpected error occurred while processing the request.", payload.Detail);
        Assert.DoesNotContain("SuperSecret123", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Login failed", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sa", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetById_missing_review_still_returns_404()
    {
        var reviewService = new Mock<IReviewService>();
        reviewService
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReviewResponse?)null);

        await using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IReviewService>();
                services.AddScoped(_ => reviewService.Object);
            });
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/reviews/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Review not found.", payload.Error);
    }

    [Fact]
    public async Task Create_invalid_operation_returns_502()
    {
        var reviewService = new Mock<IReviewService>();
        reviewService
            .Setup(s => s.CreateAsync(It.IsAny<CreateReviewRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ReviewAnalysisException("Gemini API key is not configured."));

        await using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IReviewService>();
                services.AddScoped(_ => reviewService.Object);
            });
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/reviews", new CreateReviewRequest
        {
            ReviewText = "The app crashes when opening orders."
        });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Review analysis failed.", payload.Error);
        Assert.Equal("Gemini API key is not configured.", payload.Detail);
    }

    [Fact]
    public async Task Create_framework_invalid_operation_returns_500_without_internal_details()
    {
        var reviewService = new Mock<IReviewService>();
        reviewService
            .Setup(s => s.CreateAsync(It.IsAny<CreateReviewRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(
                "The ConnectionString property has not been initialized. Password=SuperSecret123"));

        await using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IReviewService>();
                services.AddScoped(_ => reviewService.Object);
            });
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/reviews", new CreateReviewRequest
        {
            ReviewText = "The app crashes when opening orders."
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("An unexpected error occurred.", payload.Error);
        Assert.DoesNotContain("SuperSecret123", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
    }
}
