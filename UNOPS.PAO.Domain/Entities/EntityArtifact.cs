using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Stores artifacts associated with any entity (Country, Opportunity, OrgUnit, Partner, etc.)
/// Can store documents, metrics, text data, or extracted information
/// </summary>
public class EntityArtifact : ModifiableDeletableEntity
{
    public new int Id { get; set; }

    /// <summary>
    /// Entity type (e.g., "Opportunity", "Country", "OrgUnit", "Partner")
    /// </summary>
    [MaxLength(100)]
    public required string EntityType { get; set; }

    /// <summary>
    /// Entity ID (e.g., OpportunityId = 1, CountryId = 123)
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// FK to ArtifactType
    /// </summary>
    public int ArtifactTypeId { get; set; }
    public virtual ArtifactType? ArtifactType { get; set; }

    /// <summary>
    /// Display name for this specific artifact instance
    /// </summary>
    [MaxLength(500)]
    public new string? Name { get; set; }

    /// <summary>
    /// Artifact value - stored based on ArtifactDataType
    /// For string/text values
    /// </summary>
    public string? ValueText { get; set; }

    /// <summary>
    /// For numeric values
    /// </summary>
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? ValueNumber { get; set; }

    /// <summary>
    /// For boolean values
    /// </summary>
    public bool? ValueBoolean { get; set; }

    /// <summary>
    /// For date values
    /// </summary>
    public DateTime? ValueDate { get; set; }

    /// <summary>
    /// For JSON/structured data, arrays, maps
    /// </summary>
    public string? ValueJson { get; set; }

    /// <summary>
    /// For document references (FK to Document table or file path)
    /// </summary>
    public int? DocumentId { get; set; }
    public virtual Document? Document { get; set; }

    /// <summary>
    /// Effective date for this artifact (when it becomes applicable)
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Expiry date for this artifact (when it's no longer valid)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Source of the artifact (e.g., "User Upload", "AI Extraction", "External API", "System Calculated", "User Input")
    /// </summary>
    [MaxLength(255)]
    public string? Source { get; set; }

    /// <summary>
    /// Was this artifact extracted from another artifact?
    /// </summary>
    public bool IsExtracted { get; set; }

    /// <summary>
    /// If extracted, reference to source artifact (for audit/tracing)
    /// </summary>
    public int? SourceArtifactId { get; set; }
    public virtual EntityArtifact? SourceArtifact { get; set; }

    /// <summary>
    /// Additional metadata as JSON (for flexible extension)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Confidence score for AI-extracted artifacts (0.0 to 1.0)
    /// </summary>
    [Column(TypeName = "decimal(3, 2)")]
    public decimal? ConfidenceScore { get; set; }

    // Navigation property for artifacts extracted from this one
    public virtual ICollection<EntityArtifact> ExtractedArtifacts { get; set; } = new HashSet<EntityArtifact>();
}

