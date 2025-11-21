namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Response model for ArtifactType
/// </summary>
public class ArtifactTypeResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ArtifactTypeCode { get; set; }
    public int ArtifactDataTypeId { get; set; }
    public string? ArtifactDataTypeName { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? ApplicableEntityTypes { get; set; }
    public bool IsUsedForCalculations { get; set; }
    public bool IsUsedForAI { get; set; }
    public int Order { get; set; }
    public string? Source { get; set; }
    public bool IsSearchable { get; set; }
    public bool AllowBulkUpdate { get; set; }
}

