using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityStakeholder : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Parent opportunity
    /// </summary>
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Type of stakeholder (Internal or External)
    /// </summary>
    public required string StakeholderType { get; set; } // "Internal" or "External"
    
    /// <summary>
    /// User ID for internal stakeholders (UNOPS personnel)
    /// </summary>
    public int? UserId { get; set; }
    public virtual PAOUser? User { get; set; }
    
    /// <summary>
    /// Stakeholder's role - FK to EntityRole
    /// </summary>
    public int? EntityRoleId { get; set; }
    public virtual EntityRole? EntityRole { get; set; }
}

