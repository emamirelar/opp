namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the Team section of an opportunity
/// Includes responsible org unit and initiative type (UNOPS Team & Internal Stakeholders)
/// </summary>
public class TeamSectionRequest
{
    /// <summary>
    /// Responsible organization unit ID
    /// </summary>
    public int? ResponsibleOrgUnitId { get; set; }

    /// <summary>
    /// Proposed initiative type ID
    /// </summary>
    public int? ProposedInitiativeTypeId { get; set; }
}

