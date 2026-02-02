using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityStakeholder : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    public int EntityRoleId { get; set; }
    public virtual EntityRole? EntityRole { get; set; }
    
    public bool IsInternal { get; set; } = true;
    
    [MaxLength(50)]
    public string? StakeholderType { get; set; } // "Internal" or "External"
    
    public int? UserId { get; set; }
    public virtual PAOUser? User { get; set; }
    
    /// <summary>
    /// FK to OrganizationHierarchy - used for auto-populated stakeholders from EntityUserRoles.
    /// When set, this stakeholder represents all users with the EntityRoleId for this OrgUnit.
    /// </summary>
    public int? OrganizationHierarchyId { get; set; }
    public virtual OrganizationHierarchy? OrganizationHierarchy { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Indicates if this stakeholder was auto-populated from EntityUserRoles.
    /// Auto-populated stakeholders cannot be edited or removed by users.
    /// </summary>
    [NotMapped]
    public bool IsAutoPopulated => OrganizationHierarchyId.HasValue;
}

