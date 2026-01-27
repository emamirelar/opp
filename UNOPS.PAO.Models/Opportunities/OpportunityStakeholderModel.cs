namespace UNOPS.PAO.Models;

public class OpportunityStakeholderModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int EntityRoleId { get; set; }
    public string? EntityRoleName { get; set; }
    public string? EntityRoleCode { get; set; }
    public bool IsInternal { get; set; }
    public string? StakeholderType { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// Standardized position title from the personnel record
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Organization Hierarchy ID - used for auto-populated stakeholders from EntityUserRoles.
    /// </summary>
    public int? OrganizationHierarchyId { get; set; }
    
    /// <summary>
    /// Organization Hierarchy Name - the name of the org unit for auto-populated stakeholders.
    /// </summary>
    public string? OrganizationHierarchyName { get; set; }
    
    /// <summary>
    /// Indicates if this stakeholder was auto-populated from EntityUserRoles.
    /// Auto-populated stakeholders cannot be edited or removed by users.
    /// </summary>
    public bool IsAutoPopulated { get; set; }
    
    public string? Notes { get; set; }
}
