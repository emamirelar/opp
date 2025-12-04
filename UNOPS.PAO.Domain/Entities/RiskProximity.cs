using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Risk proximity (time horizon) lookup entity (aligned with oUP)
/// Values: Within one month, Within three months, Within six months, One year and beyond
/// </summary>
public class RiskProximity : IBaseBusinessEntity<int>
{
    public int Id { get; set; }

    /// <summary>
    /// Proximity name (e.g., "Within one month", "One year and beyond")
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short code for the proximity (e.g., "WITHIN_ONE_MONTH", "ONE_YEAR_AND_BEYOND")
    /// </summary>
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Number of months this proximity represents (approximate)
    /// e.g., 1 for "Within one month", 12 for "One year and beyond"
    /// </summary>
    public int? MonthsValue { get; set; }

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

