namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Request model for downloading bulk import template CSV
/// </summary>
public class BulkTemplateDownloadRequest
{
    /// <summary>
    /// Entity type for which to generate template
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// List of artifact type IDs to include as columns in the template
    /// Order matters - will be used as column order
    /// </summary>
    public required List<int> ArtifactTypeIds { get; set; }
}

