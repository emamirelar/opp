namespace UNOPS.PAO.Models;

/// <summary>
/// Request model for creating/updating an Opportunity's UNOPS Mission alignment
/// </summary>
public class OpportunityUNOPSMissionRequest
{
    public int? Id { get; set; }
    public required int UNOPSMissionId { get; set; }
}

