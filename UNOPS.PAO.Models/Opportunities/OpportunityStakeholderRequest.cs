namespace UNOPS.PAO.Models;

public class OpportunityStakeholderRequest
{
    /// <summary>
    /// User ID - required for manually added stakeholders (e.g., Opportunity Manager).
    /// Not required for auto-populated stakeholders from EntityUserRoles.
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Entity Role ID - always required.
    /// </summary>
    public int EntityRoleId { get; set; }
    
    /// <summary>
    /// Organization Hierarchy ID - used for auto-populated stakeholders from EntityUserRoles.
    /// When set, represents all users with the EntityRoleId for this OrgUnit.
    /// </summary>
    public int? OrganizationHierarchyId { get; set; }
    
    public string? Notes { get; set; }
}
