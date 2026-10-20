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
        // Known client/service conditions are mapped to non-500 statuses.
        // Unexpected exceptions always become a safe 500.
        var (statusCode, error, detail) = exception switch
        {
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Invalid review.",
                string.IsNullOrWhiteSpace(argEx.Message)
                    ? "The request is invalid."
                    : argEx.Message),
            InvalidOperationException invEx => (
                StatusCodes.Status502BadGateway,
                "Review analysis failed.",
                string.IsNullOrWhiteSpace(invEx.Message)
                    ? "The operation could not be completed."
                    : invEx.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "An unexpected error occurred while processing the request.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
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
            logger.LogWarning(
                exception,
                "Handled application exception for {Method} {Path}. StatusCode={StatusCode}",
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
