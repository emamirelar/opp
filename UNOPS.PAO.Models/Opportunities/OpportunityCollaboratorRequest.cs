namespace UNOPS.PAO.Models;

/// <summary>
/// Request model for adding/updating opportunity collaborators
/// </summary>
public class OpportunityCollaboratorRequest
{
    /// <summary>
    /// The user ID of the collaborator to add
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// List of expertise IDs to assign to this collaborator.
    /// Indicates the specific expertise/capacity in which the collaborator is related to the opportunity.
    /// </summary>
    public List<int>? ExpertiseIds { get; set; }
}

/// <summary>
/// Request model for updating the team section (includes collaborators)
/// </summary>
public class OpportunityDevelopmentTeamRequest
{
    /// <summary>
    /// Opportunity Manager user ID (required)
    /// </summary>
    public int? OpportunityManagerId { get; set; }
    
    /// <summary>
    /// List of collaborator user IDs
    /// </summary>
    public List<int>? CollaboratorIds { get; set; }
    
    /// <summary>
    /// Org Unit responsible for opportunity development (required)
    /// </summary>
    public int? ResponsibleOrgUnitId { get; set; }
}
