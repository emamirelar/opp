using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Defines types of artifacts that can be associated with entities
/// Examples: Org Strategy, UN Cooperation Framework, Corruption Index, Sustainability Metric
/// </summary>
public class ArtifactType : ModifiableDeletableEntity
{
    public new int Id { get; set; }

    /// <summary>
    /// Artifact type name (e.g., "Org Strategy", "UN Coop Framework", "Corruption Index")
    /// </summary>
    [MaxLength(255)]
    public new required string Name { get; set; }

    /// <summary>
    /// Unique code for the artifact type (e.g., "ORG_STRATEGY", "UNSDCF", "CORRUPTION_IDX")
    /// Must be unique across all artifact types
    /// </summary>
    [MaxLength(100)]
    public required string ArtifactTypeCode { get; set; }

    /// <summary>
    /// FK to ArtifactDataType (defines what type of data this artifact stores)
    /// </summary>
    public int ArtifactDataTypeId { get; set; }
    public virtual ArtifactDataType? ArtifactDataType { get; set; }

    /// <summary>
    /// Description of the artifact type
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping (e.g., "Strategy", "Metric", "Document", "Assessment", "Target")
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Applicable entity types (comma-separated: "Country,Opportunity,OrgUnit,Partner")
    /// </summary>
    [MaxLength(500)]
    public string? ApplicableEntityTypes { get; set; }

    /// <summary>
    /// Can this artifact be used in calculations?
    /// </summary>
    public bool IsUsedForCalculations { get; set; }

    /// <summary>
    /// Can this artifact be processed by AI?
    /// </summary>
    public bool IsUsedForAI { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Source of the artifact data (e.g., "World Bank", "UN", "Internal")
    /// </summary>
    [MaxLength(255)]
    public string? Source { get; set; }

    /// <summary>
    /// Can this artifact be used in search operations?
    /// </summary>
    public bool IsSearchable { get; set; }

    /// <summary>
    /// Can this artifact be updated in bulk operations?
    /// </summary>
    public bool AllowBulkUpdate { get; set; }

    // Navigation properties
    public virtual ICollection<EntityArtifact> EntityArtifacts { get; set; } = new HashSet<EntityArtifact>();
    public virtual ICollection<ArtifactExtractionRule> SourceExtractionRules { get; set; } = new HashSet<ArtifactExtractionRule>();
    public virtual ICollection<ArtifactExtractionRule> TargetExtractionRules { get; set; } = new HashSet<ArtifactExtractionRule>();
}

