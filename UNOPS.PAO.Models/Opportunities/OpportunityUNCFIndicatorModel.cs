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
}

