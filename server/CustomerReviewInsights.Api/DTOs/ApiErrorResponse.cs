namespace CustomerReviewInsights.Api.DTOs;

public class ApiErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public IDictionary<string, string[]>? ValidationErrors { get; set; }
}
