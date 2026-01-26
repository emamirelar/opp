namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the Team section of an opportunity
/// Includes responsible org unit, initiative type, and internal stakeholders (UNOPS Team & Internal Stakeholders)
/// </summary>
public class TeamSectionRequest
{
    /// <summary>
    /// Responsible organization unit ID (required for Opportunity Development Team)
    /// </summary>
    public int? ResponsibleOrgUnitId { get; set; }

    /// <summary>
    /// Proposed initiative type ID
    /// </summary>
    public int? ProposedInitiativeTypeId { get; set; }

    /// <summary>
    /// Opportunity Manager user ID (required for Opportunity Development Team)
    /// </summary>
    public int? OpportunityManagerId { get; set; }

    /// <summary>
    /// List of collaborator user IDs (Opportunity Development Team)
    /// </summary>
    public List<int>? CollaboratorIds { get; set; }

    /// <summary>
    /// List of internal team members and stakeholders
    /// </summary>
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }
    
    /// <summary>
    /// List of SME (Subject Matter Expert) selections for the opportunity.
    /// These are saved to EntityUserRoles table.
    /// </summary>
    public List<SMESelectionRequest>? SMESelections { get; set; }
}

