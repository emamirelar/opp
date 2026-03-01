namespace UNOPS.PAO.Models.Documents;

/// <summary>
/// Result of generating a PDF and uploading to GCS.
/// </summary>
public class GeneratePdfResult
{
    /// <summary>
    /// GCS path (gs://bucket/folder/entityId/filename.pdf) when successful.
    /// </summary>
    public string? GcsPath { get; set; }

    /// <summary>
    /// Error message when conversion or upload fails.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Additional error details.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// True when PDF was generated and uploaded successfully.
    /// </summary>
    public bool Success => !string.IsNullOrEmpty(GcsPath);
}
