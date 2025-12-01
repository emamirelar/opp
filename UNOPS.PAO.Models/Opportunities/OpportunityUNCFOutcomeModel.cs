namespace UNOPS.PAO.Models;

/// <summary>
/// Model for OpportunityUNCFOutcome - links opportunities to UNCF Outcomes via country
/// </summary>
public class OpportunityUNCFOutcomeModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int OpportunityCountryId { get; set; }
    public int UNCFOutcomeId { get; set; }  // The integer FK for database relations
    public string? UNCFOutcomeExternalId { get; set; }  // The string identifier from external system
    public string? UNCFOutcomeName { get; set; }
    public int? VersionNo { get; set; }
    public string? Country { get; set; }  // ISO2 code
    public string? Notes { get; set; }
    public List<OpportunityUNCFIndicatorModel> Indicators { get; set; } = new List<OpportunityUNCFIndicatorModel>();
    
    /// <summary>
    /// Indicates if this UNCF Outcome is currently inactive (outside its active date range)
    /// </summary>
    public bool IsInactive { get; set; }
    
    /// <summary>
    /// Indicates if a newer version of this UNCF Outcome is available
    /// </summary>
    public bool HasNewerVersion { get; set; }
}

