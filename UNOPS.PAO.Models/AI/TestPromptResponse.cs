namespace UNOPS.PAO.Models.AI;

public class TestPromptResponse
{
    public bool Success { get; set; }
    public string Response { get; set; }
    public string Error { get; set; }
    public string? DataRetrievalResult { get; set; } // JSON data retrieved by the data retrieval method
} 