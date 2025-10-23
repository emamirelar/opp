using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Defines rules for extracting data from one artifact type to create another artifact type
/// Used for AI processing (e.g., extract "Sustainability Metric" from "Org Strategy" document)
/// </summary>
public class ArtifactExtractionRule : ModifiableDeletableEntity
{
    public new int Id { get; set; }

    /// <summary>
    /// Source artifact type to extract from
    /// </summary>
    public int SourceArtifactTypeId { get; set; }
    public virtual ArtifactType? SourceArtifactType { get; set; }

    /// <summary>
    /// Target artifact type to create after extraction
    /// </summary>
    public int ExtractedArtifactTypeId { get; set; }
    public virtual ArtifactType? ExtractedArtifactType { get; set; }

    /// <summary>
    /// AI extraction rule/prompt
    /// Contains the instructions for AI to extract specific information
    /// </summary>
    public required string RulePrompt { get; set; }

    /// <summary>
    /// Description of what this rule extracts
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Is this rule active and should be applied?
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Execution order (if multiple rules apply to same source artifact)
    /// </summary>
    public int ExecutionOrder { get; set; }

    /// <summary>
    /// Minimum confidence score required to create extracted artifact (0.0 to 1.0)
    /// </summary>
    [MaxLength(3)]
    public decimal? MinimumConfidenceScore { get; set; }

    /// <summary>
    /// Should extraction run automatically when source artifact is created?
    /// </summary>
    public bool AutoExecute { get; set; }
}

