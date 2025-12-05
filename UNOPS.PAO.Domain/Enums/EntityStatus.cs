namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// General entity status enum used across the system
/// Note: For Risk-specific status, use UNOPS.PAO.Domain.Enums.RiskStatus
/// </summary>
public enum EntityStatus
{
    Inactive,
    Active,
    OnHold,
    Closed,
    Draft,
    Archived,
    /// <summary>
    /// Open status - used for risks and other entities requiring an open state
    /// </summary>
    Open
}
