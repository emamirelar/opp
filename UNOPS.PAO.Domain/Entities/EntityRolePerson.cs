using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class EntityRolePerson : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Entity type (e.g., "Opportunity", "Partner")
    /// </summary>
    public required string EntityType { get; set; }
    
    /// <summary>
    /// ID of the entity record
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// Role assigned to the person for this entity
    /// </summary>
    public int EntityRoleId { get; set; }
    public virtual EntityRole? EntityRole { get; set; }
    
    /// <summary>
    /// User ID (for internal UNOPS users), nullable if external contact
    /// </summary>
    public int? UserId { get; set; }
    public virtual PAOUser? User { get; set; }
    
    /// <summary>
    /// Contact ID (for external contacts), nullable if internal user
    /// </summary>
    public int? ContactId { get; set; }
    public virtual Contact? Contact { get; set; }
    
    /// <summary>
    /// Effective start date of the role assignment
    /// </summary>
    public DateTime EffectiveDate { get; set; }
    
    /// <summary>
    /// Optional end date of the role assignment
    /// </summary>
    public DateTime? EndDate { get; set; }
}

