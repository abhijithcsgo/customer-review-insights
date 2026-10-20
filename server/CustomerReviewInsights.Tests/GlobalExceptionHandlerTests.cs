using System.Text.Json;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CustomerReviewInsights.Tests;

public class GlobalExceptionHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Unexpected_exception_returns_safe_500_api_error()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = CreateHttpContext();

        var handled = await handler.TryHandleAsync(
            context,
            new Exception("Secret connection string Server=prod;Password=hunter2"),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        var payload = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("An unexpected error occurred.", payload.Error);
        Assert.Equal("An unexpected error occurred while processing the request.", payload.Detail);
        Assert.DoesNotContain("hunter2", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection string", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ArgumentException_returns_400_with_message()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = CreateHttpContext();

        var handled = await handler.TryHandleAsync(
            context,
            new ArgumentException("Review text is required."),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var payload = JsonSerializer.Deserialize<ApiErrorResponse>(await ReadBodyAsync(context), JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Invalid review.", payload.Error);
        Assert.Equal("Review text is required.", payload.Detail);
    }

    [Fact]
    public async Task ReviewAnalysisException_returns_502_with_safe_detail()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = CreateHttpContext();

        var handled = await handler.TryHandleAsync(
            context,
            new ReviewAnalysisException("The AI analysis service timed out. Please try again."),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);

        var payload = JsonSerializer.Deserialize<ApiErrorResponse>(await ReadBodyAsync(context), JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Review analysis failed.", payload.Error);
        Assert.Equal("The AI analysis service timed out. Please try again.", payload.Detail);
    }

    [Fact]
    public async Task InvalidOperationException_returns_500_without_internal_message()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = CreateHttpContext();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("Connection string Server=prod;Password=hunter2 was rejected."),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        var payload = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("An unexpected error occurred.", payload.Error);
        Assert.Equal("An unexpected error occurred while processing the request.", payload.Detail);
        Assert.DoesNotContain("hunter2", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Connection string", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Client_abort_is_handled_without_a_500_body()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = CreateHttpContext();
        context.RequestAborted = new CancellationToken(canceled: true);

        var handled = await handler.TryHandleAsync(
            context,
            new OperationCanceledException(context.RequestAborted),
            context.RequestAborted);

        Assert.True(handled);
        Assert.NotEqual(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        Assert.Equal(string.Empty, body);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/reviews";
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
