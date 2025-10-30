namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// DTO for artifact values embedded in entity models
/// Keyed by ArtifactTypeCode for easy access
/// </summary>
public class EntityArtifactModel
{
    /// <summary>
    /// Artifact type code (e.g., "ContextAndChallenges", "Scope")
    /// </summary>
    public required string ArtifactTypeCode { get; set; }
    
    /// <summary>
    /// Artifact type name for display
    /// </summary>
    public string? ArtifactTypeName { get; set; }
    
    /// <summary>
    /// Category (Strategy, Assessment, Target, etc.)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Data type name (string, number, date, etc.)
    /// </summary>
    public string? DataType { get; set; }
    
    /// <summary>
    /// The actual value (could be text, number, date, json, etc.)
    /// Frontend should cast based on DataType
    /// </summary>
    public object? Value { get; set; }
    
    /// <summary>
    /// For document type artifacts
    /// </summary>
    public int? DocumentId { get; set; }
    
    /// <summary>
    /// Optional metadata
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Effective date
    /// </summary>
    public DateTime? EffectiveDate { get; set; }
    
    /// <summary>
    /// Expiry date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// Source of the data
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Was this extracted by AI?
    /// </summary>
    public bool IsExtracted { get; set; }
    
    /// <summary>
    /// Confidence score for AI-extracted data
    /// </summary>
    public decimal? ConfidenceScore { get; set; }
}

