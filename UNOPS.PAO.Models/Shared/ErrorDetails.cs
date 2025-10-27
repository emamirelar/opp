namespace UNOPS.PAO.Models.Shared;

using System.Text.Json;

public class ErrorDetails
{
    public ErrorDetails(string message, string StackTrace = "")
    {
        Message = message;
        this.StackTrace = StackTrace;
    }

    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}