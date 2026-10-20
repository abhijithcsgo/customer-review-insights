namespace CustomerReviewInsights.Api.Exceptions;

/// <summary>
/// Expected failure from the AI analysis provider or from AI output that failed validation.
/// The message is written for API clients and must not include secrets, SQL, or raw provider payloads.
/// </summary>
public sealed class ReviewAnalysisException : Exception
{
    public ReviewAnalysisException(string message)
        : base(message)
    {
    }

    public ReviewAnalysisException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
