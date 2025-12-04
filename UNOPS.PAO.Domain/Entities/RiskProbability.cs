using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Risk probability lookup entity (aligned with oUP)
/// Values: Low, Low to medium, Medium to high, High
/// </summary>
public class RiskProbability : IBaseBusinessEntity<int>
{
    public int Id { get; set; }

    /// <summary>
    /// Probability level name (e.g., "Low", "Low to medium")
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short code for the probability (e.g., "LOW", "LOW_TO_MEDIUM")
    /// </summary>
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display label with number prefix (e.g., "1. Low", "2. Low to medium")
    /// </summary>
    [MaxLength(100)]
    public string? DisplayLabel { get; set; }

    /// <summary>
    /// Numeric value for calculations (1-4)
    /// </summary>
    public int NumericValue { get; set; }

    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Entity status
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

