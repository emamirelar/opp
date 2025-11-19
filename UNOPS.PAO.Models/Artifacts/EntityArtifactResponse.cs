namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Response model for EntityArtifact
/// </summary>
public class EntityArtifactResponse
{
    public int Id { get; set; }
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public int ArtifactTypeId { get; set; }
    public string? ArtifactTypeName { get; set; }
    public string? ArtifactTypeCode { get; set; }
    public string? DataTypeName { get; set; }
    public string? Name { get; set; }
    public string? ValueText { get; set; }
    public decimal? ValueNumber { get; set; }
    public bool? ValueBoolean { get; set; }
    public DateTime? ValueDate { get; set; }
    public string? ValueJson { get; set; }
    public int? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Source { get; set; }
    public bool IsExtracted { get; set; }
    public int? SourceArtifactId { get; set; }
    public string? Metadata { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
}

