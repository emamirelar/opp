namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Request model for creating or updating an EntityArtifact
/// </summary>
public class EntityArtifactRequest
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
    /// Display name for this artifact instance
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Text value for string/text artifacts
    /// </summary>
    public string? ValueText { get; set; }

    /// <summary>
    /// Numeric value for number artifacts
    /// </summary>
    public decimal? ValueNumber { get; set; }

    /// <summary>
    /// Boolean value for boolean artifacts
    /// </summary>
    public bool? ValueBoolean { get; set; }

    /// <summary>
    /// Date value for date artifacts
    /// </summary>
    public DateTime? ValueDate { get; set; }

    /// <summary>
    /// JSON value for complex artifacts
    /// </summary>
    public string? ValueJson { get; set; }

    /// <summary>
    /// Document ID for document artifacts
    /// </summary>
    public int? DocumentId { get; set; }

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

