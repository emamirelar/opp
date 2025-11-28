namespace UNOPS.PAO.Models;

/// <summary>
/// Model for OpportunityUNCFIndicator - links opportunities to UNCF Indicators
/// </summary>
public class OpportunityUNCFIndicatorModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int OpportunityUNCFOutcomeId { get; set; }
    public int UNCFIndicatorId { get; set; }  // The integer FK for database relations
    public string? UNCFIndicatorExternalId { get; set; }  // The string identifier from external system
    public string? UNCFIndicatorName { get; set; }
    public string? Notes { get; set; }
    
    /// <summary>
    /// Indicates if this UNCF Indicator is currently inactive (outside its active date range)
    /// </summary>
    public bool IsInactive { get; set; }
    
    /// <summary>
    /// Indicates if a newer version of this UNCF Indicator is available
    /// </summary>
    public bool HasNewerVersion { get; set; }
}

