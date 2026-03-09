// PNO-914: Specification model for Session Timeout / Connection Lost.
// Mirrors TypeScript ErrorParser logic (error.model.ts) and session configuration requirements.
// AC-1: Users should NOT get frequent "Connection lost" popups during normal use
// AC-2: Session/idle timeout should be properly managed (IAP expiry extended)
// AC-3: Application should handle network disconnection gracefully
// AC-4: Session refresh should happen transparently before expiry
// AC-5: Error messages should be clear and actionable (not generic "Connection Lost" for session issues)

namespace UNOPS.PAO.Business.Tests.Infrastructure;

/// <summary>
/// C# specification mirror of TypeScript ErrorParser (error.model.ts).
/// Validates HTTP error parsing rules for PNO-914 AC-3, AC-5.
/// </summary>
public sealed class ErrorParserSpec
{
    /// <summary>
    /// AC-5: Status 0 (network error) maps to translation keys for Connection Lost.
    /// </summary>
    public const string NetworkErrorTitleKey = "error.networkError.title";
    public const string NetworkErrorDetailKey = "error.networkError.detail";

    /// <summary>
    /// Parses an HTTP error into structured AppError (mirrors TypeScript ErrorParser.parse).
    /// </summary>
    public static AppErrorSpec Parse(HttpErrorSpec err, string? context = null)
    {
        var baseError = new AppErrorSpec
        {
            Status = err.Status,
            Url = err.Url,
            Context = context,
            Timestamp = DateTime.UtcNow
        };

        // Network errors (no connection) - AC-3, AC-5
        if (err.Status == 0)
        {
            return baseError with
            {
                Title = NetworkErrorTitleKey,
                Detail = NetworkErrorDetailKey
            };
        }

        // Server errors (500+)
        if (err.Status >= 500)
        {
            var detail = err.ErrorObject?.Detail;
            return baseError with
            {
                Title = err.ErrorObject?.Title ?? "Server Error",
                Detail = string.IsNullOrEmpty(detail) ? "An unexpected server error occurred. Please try again later." : detail,
                StackTrace = err.ErrorObject?.StackTrace
            };
        }

        // Client errors (400-499) with ProblemDetails format
        if (err.ErrorObject != null)
        {
            // Validation errors format
            if (err.ErrorObject.Errors != null && err.ErrorObject.Errors.Count > 0)
            {
                var detail = string.Join("\n",
                    err.ErrorObject.Errors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"));
                return baseError with
                {
                    Title = err.ErrorObject.Title ?? "Validation Error",
                    Detail = detail,
                    ValidationErrors = err.ErrorObject.Errors
                };
            }

            // ProblemDetails format
            if (!string.IsNullOrEmpty(err.ErrorObject.Title))
            {
                return baseError with
                {
                    Title = err.ErrorObject.Title,
                    Detail = err.ErrorObject.Detail ?? "An error occurred while processing your request."
                };
            }

            // Simple error object format { error: "message" }
            if (!string.IsNullOrEmpty(err.ErrorObject.Error))
            {
                return baseError with
                {
                    Title = $"Error {err.Status}",
                    Detail = err.ErrorObject.Error,
                    MissingFields = err.ErrorObject.MissingFields
                };
            }

            // Fallback for other error objects
            return baseError with
            {
                Title = $"Error {err.Status}",
                Detail = err.ErrorObject.Message ?? err.Message ?? "An unexpected error occurred."
            };
        }

        // Fallback for non-object errors
        return baseError with
        {
            Title = $"Error {err.Status}",
            Detail = string.IsNullOrEmpty(err.Message) ? "An unexpected error occurred." : err.Message
        };
    }
}

/// <summary>
/// Simplified HTTP error representation for spec testing.
/// </summary>
public sealed record HttpErrorSpec(
    int Status,
    string? Url = null,
    string Message = "",
    ErrorObjectSpec? ErrorObject = null);

/// <summary>
/// Simplified error object (mirrors TypeScript err.error structure).
/// </summary>
public sealed record ErrorObjectSpec(
    string? Title = null,
    string? Detail = null,
    string? Error = null,
    string? Message = null,
    string? StackTrace = null,
    Dictionary<string, string[]>? Errors = null,
    string[]? MissingFields = null);

/// <summary>
/// Parsed app error (mirrors TypeScript AppError interface).
/// </summary>
public sealed record AppErrorSpec
{
    public int Status { get; init; }
    public string Title { get; init; } = "";
    public string Detail { get; init; } = "";
    public DateTime Timestamp { get; init; }
    public string? Url { get; init; }
    public string? Context { get; init; }
    public string? StackTrace { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }
    public string[]? MissingFields { get; init; }
}

/// <summary>
/// Session configuration specification for PNO-914 AC-2.
/// </summary>
public static class SessionConfigSpec
{
    /// <summary>
    /// AC-2: expiryInMinutes should be at least 480 (8 hours) for adequate session length.
    /// </summary>
    public const int MinimumExpiryMinutes = 480;

    /// <summary>
    /// IAP JWT ClockSkew should be reasonable (≤5 minutes) per IAPVerificationMiddleware.
    /// </summary>
    public const int MaxClockSkewMinutes = 5;

    /// <summary>
    /// Public key cache refresh interval (hours) in IAPVerificationMiddleware.
    /// </summary>
    public const int PublicKeyCacheRefreshHours = 1;

    /// <summary>
    /// Default health check path when not configured.
    /// </summary>
    public const string DefaultHealthCheckPath = "/health";
}
