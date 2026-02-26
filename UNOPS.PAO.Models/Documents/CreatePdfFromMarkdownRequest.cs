namespace UNOPS.PAO.Models.Documents;

/// <summary>
/// Request model for creating a PDF from markdown content and uploading to GCS.
/// </summary>
public class CreatePdfFromMarkdownRequest
{
    /// <summary>
    /// Markdown content to convert to PDF. Supports standard markdown (headers, lists, bold, italic, code blocks).
    /// </summary>
    public string MarkdownContent { get; set; } = string.Empty;

    /// <summary>
    /// Optional folder for GCS organization (e.g., "opportunities", "partners"). Defaults to "documents".
    /// </summary>
    public string? Folder { get; set; }

    /// <summary>
    /// Optional entity ID for organizing files. Use 0 for standalone documents.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Optional file name (without extension). A timestamp and .pdf will be appended if not provided.
    /// </summary>
    public string? FileName { get; set; }
}
