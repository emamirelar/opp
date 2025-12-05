using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Request model for creating or updating an EntityArtifact with a document file
/// Used for document type artifacts that are uploaded to Google Cloud Storage
/// </summary>
public class EntityArtifactDocumentRequest
{
    /// <summary>
    /// Entity type (e.g., "Country", "Opportunity", "OrgUnit", "Partner")
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// Entity ID
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Artifact Type ID
    /// </summary>
    public int ArtifactTypeId { get; set; }

    /// <summary>
    /// Artifact Type Code (used for GCS folder path)
    /// </summary>
    public string? ArtifactTypeCode { get; set; }

    /// <summary>
    /// Display name for this artifact instance
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The file to upload to Google Cloud Storage
    /// </summary>
    public IFormFile? File { get; set; }

    /// <summary>
    /// Effective date for this artifact
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Expiry date for this artifact
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Source of the artifact (e.g., "User Input", "AI Extraction")
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }
}

