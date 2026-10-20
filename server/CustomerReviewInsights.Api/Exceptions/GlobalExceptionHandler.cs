using System.Text.Json;
using CustomerReviewInsights.Api.DTOs;
using Microsoft.AspNetCore.Diagnostics;

namespace CustomerReviewInsights.Api.Exceptions;

/// <summary>
/// Central handler for unhandled exceptions. Returns <see cref="ApiErrorResponse"/> JSON
/// without exposing stack traces, connection strings, or other sensitive details.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Client disconnects are not server failures. Do not log them as errors or write a 500 body.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request canceled by the client for {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path.Value);
            return true;
        }

        // Only application-authored messages are returned to the client.
        // ArgumentException covers review-text validation. ReviewAnalysisException covers Gemini.
        // Framework exceptions such as InvalidOperationException and database failures stay on the 500 path
        // so their messages (connection strings, SQL, file paths) are never copied into the response.
        var (statusCode, error, detail, logException) = exception switch
        {
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Invalid review.",
                string.IsNullOrWhiteSpace(argEx.Message)
                    ? "The request is invalid."
                    : argEx.Message,
                false),
            ReviewAnalysisException analysisEx => (
                StatusCodes.Status502BadGateway,
                "Review analysis failed.",
                string.IsNullOrWhiteSpace(analysisEx.Message)
                    ? "The AI analysis service could not complete the request."
                    : analysisEx.Message,
                false),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "An unexpected error occurred while processing the request.",
                true)
        };

        if (logException)
        {
            logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. StatusCode={StatusCode}",
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                statusCode);
        }
        else
        {
            // Expected failures are already described by a safe message. Do not log the exception object again;
            // the analysis service logs provider status and timing without the API key or review text.
            logger.LogWarning(
                "Handled {ExceptionType} for {Method} {Path}. StatusCode={StatusCode}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                statusCode);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        var payload = new ApiErrorResponse
        {
            Error = error,
            Detail = detail
        };

        await httpContext.Response.WriteAsJsonAsync(payload, SerializerOptions, cancellationToken);
        return true;
    }
}
