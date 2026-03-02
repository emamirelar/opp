namespace UNOPS.PAO.Models.Documents;

public class GenerateGoogleDocRequest
{
    public string Data { get; set; } = string.Empty;
    public string? Filename { get; set; }
    /// <summary>
    /// Optional opportunity ID for GCS upload path (folder: opportunities/{id}).
    /// When provided, PDF is uploaded to GCS and gcsPath is included in the response.
    /// </summary>
    public int? OpportunityId { get; set; }
}


