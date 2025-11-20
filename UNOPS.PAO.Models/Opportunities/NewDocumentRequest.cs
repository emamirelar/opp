namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for a new document to be persisted after opportunity creation
/// Contains GCS path, MIME type, and document type ID
/// </summary>
public class NewDocumentRequest
{
    /// <summary>
    /// GCS storage path (gs://bucket/path/file.ext)
    /// </summary>
    public required string GcsPath { get; set; }
    
    /// <summary>
    /// MIME type of the document
    /// </summary>
    public required string MimeType { get; set; }
    
    /// <summary>
    /// Document type ID (optional)
    /// </summary>
    public int? DocumentTypeId { get; set; }
}

