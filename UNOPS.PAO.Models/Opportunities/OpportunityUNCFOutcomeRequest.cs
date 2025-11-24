namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for creating/updating Opportunity UNCF Outcome
/// </summary>
public class OpportunityUNCFOutcomeRequest
{
    public int OpportunityCountryId { get; set; }
    public int UNCFOutcomeId { get; set; }
    public string? Notes { get; set; }
    public List<int>? UNCFIndicatorIds { get; set; }  // List of indicator IDs to associate
}

