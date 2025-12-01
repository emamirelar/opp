namespace UNOPS.PAO.Models;

/// <summary>
/// Model representing an Opportunity's alignment to a UNOPS Mission
/// </summary>
public class OpportunityUNOPSMissionModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int UNOPSMissionId { get; set; }
    
    /// <summary>
    /// The UNOPS Mission details
    /// </summary>
    public UNOPSMissionModel? UNOPSMission { get; set; }
}

