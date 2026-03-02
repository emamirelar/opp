namespace UNOPS.PAO.Domain.Infrastructure;

public class ErrorLog
{
    public ErrorLog()
    {
    }

    public ErrorLog(string message, string? stackTrace, int statusCode, string? url, string? userEmail)
    {
        Message = message;
        StackTrace = stackTrace;
        StatusCode = statusCode;
        UserEmail = userEmail;
        Date = DateTime.UtcNow;
        Url = url;
    }

    public int Id { get; }
    public string Message { get; } = string.Empty;
    public string? Url { get; }
    public DateTime Date { get; }
    public string? StackTrace { get; }
    public string? UserEmail { get; }
    public int StatusCode { get; }
}