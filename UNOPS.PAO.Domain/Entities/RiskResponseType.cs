using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Risk response type lookup entity (aligned with oUP)
/// Values vary by RiskType:
/// - THREAT: Accept, Avoid, Reduce, Share, Transfer
/// - OPPORTUNITY: Accept, Enhance, Exploit, Share, Transfer
/// </summary>
public class RiskResponseType : IBaseBusinessEntity<int>
{
    public int Id { get; set; }

    /// <summary>
    /// Response type name (e.g., "Accept", "Reduce", "Enhance")
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short code for the response type (e.g., "ACCEPT", "REDUCE")
    /// </summary>
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the response type
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this response type is valid for Threat risks
    /// </summary>
    public bool ValidForThreat { get; set; }

    /// <summary>
    /// Whether this response type is valid for Opportunity risks
    /// </summary>
    public bool ValidForOpportunity { get; set; }

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

