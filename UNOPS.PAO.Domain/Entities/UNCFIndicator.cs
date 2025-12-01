using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// UN Cooperation Framework (UNCF) Indicator entity
/// Data synced from External Data Service (ERP Database) - Read Only
/// Represents indicators that measure outcomes defined in UN Cooperation Frameworks
/// </summary>
public class UNCFIndicator : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// UNCF Indicator Name (computed from indicators field or description)
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [MaxLength(1000)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (read-only entities default to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// UNCF Indicator ID - External system identifier
    /// </summary>
    [MaxLength(255)]
    public string? UNCFIndicatorId { get; set; }
    
    /// <summary>
    /// Unit of measurement for the indicator (e.g., "%", "Number", etc.)
    /// </summary>
    [MaxLength(255)]
    public string? Unit { get; set; }
    
    /// <summary>
    /// Additional description for the indicator
    /// Note: Can be very long (up to 2,823 chars in source data)
    /// </summary>
    [MaxLength(3000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Indicator Start Date - When this indicator measurement period begins
    /// </summary>
    public DateTime? UNCFIndicatorStartDate { get; set; }
    
    /// <summary>
    /// Indicator End Date - When this indicator measurement period ends
    /// </summary>
    public DateTime? UNCFIndicatorEndDate { get; set; }
    
    /// <summary>
    /// Indicators text - The actual indicator description/text
    /// </summary>
    [MaxLength(2000)]
    public string? Indicators { get; set; }
    
    /// <summary>
    /// Baseline value for the indicator
    /// Note: Can be long (up to 768 chars in source data)
    /// </summary>
    [MaxLength(1000)]
    public string? Baseline { get; set; }
    
    /// <summary>
    /// Narrative text providing context or additional information
    /// Note: Can be very long (up to 6,402 chars in source data)
    /// </summary>
    [MaxLength(7000)]
    public string? Narrative { get; set; }
    
    /// <summary>
    /// UN Cooperation Framework Version Number
    /// Together with UNCFOutcomeExternalId, forms a composite reference to the parent UNCFOutcome
    /// Matches: UNCFOutcome.UNCooperationFrameworkVersionNo
    /// </summary>
    public int? UNCooperationFrameworkVersionNo { get; set; }
    
    /// <summary>
    /// UNCF Outcome ID - Reference to parent UNCFOutcome.UNCFOutcomeId
    /// Together with UNCooperationFrameworkVersionNo, uniquely identifies the parent outcome
    /// Note: UNCFOutcomeId alone is NOT unique - must use both fields as composite key
    /// Example: outcome_id="309" + version_no=1 → UNCFOutcome
    /// </summary>
    [MaxLength(255)]
    public string? UNCFOutcomeExternalId { get; set; }
    
    /// <summary>
    /// Country code (ISO 2-letter code) - Reference to Country.Iso2Code
    /// Matches the pattern in UNCFOutcome for country references
    /// </summary>
    [MaxLength(5)]
    public string? Country { get; set; }
    
    /// <summary>
    /// UNCF Indicator Last Updated Date - When this indicator was last updated
    /// </summary>
    public DateTime? UNCFIndicatorLastUpdatedDate { get; set; }
}

