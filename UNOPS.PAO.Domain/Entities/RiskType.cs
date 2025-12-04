using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Risk type lookup entity (aligned with oUP)
/// Values: Threat, Opportunity
/// </summary>
public class RiskType : IBaseBusinessEntity<int>
{
    public int Id { get; set; }

    /// <summary>
    /// Risk type name (e.g., "Threat", "Opportunity")
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short code for the risk type (e.g., "THREAT", "OPPORTUNITY")
    /// </summary>
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the risk type
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether ResponseType is mandatory for this risk type
    /// True for Opportunity, False for Threat
    /// </summary>
    public bool IsResponseTypeMandatory { get; set; }

    /// <summary>
    /// Entity status
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

