namespace UNOPS.PAO.Models;

/// <summary>
/// Model for opportunity collaborator - a team member with edit permissions.
/// Part of the Opportunity Development Team.
/// </summary>
public class OpportunityCollaboratorModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    
    /// <summary>
    /// The user ID of the collaborator
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Full name of the collaborator
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Email of the collaborator
    /// </summary>
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// Standardized position title from the personnel record
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Date when the collaborator was added
    /// </summary>
    public DateTime? AddedDate { get; set; }
    
    /// <summary>
    /// User who added this collaborator
    /// </summary>
    public int? AddedBy { get; set; }
    
    /// <summary>
    /// Name of the user who added this collaborator
    /// </summary>
    public string? AddedByName { get; set; }
    
    /// <summary>
    /// List of expertise areas for this collaborator.
    /// Indicates the specific expertise/capacity in which the collaborator is related to the opportunity.
    /// </summary>
    public List<CollaboratorExpertiseModel> Expertises { get; set; } = new();
}
