using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Entities;

namespace UNOPS.PAO.Domain.Entities;
public class EntityUserRole : ModifiableDeletableEntity
{
    public int UserId { get; set; }
    public virtual PAOUser? User { get; set; }
    public int? RoleId { get; set; }
    public IdentityUserRole<int>? UserRole { get; set; }
    /// FK to EntityRole - defines the specific role type (e.g., Region Director, DoA1)
    /// </summary>
    public int? EntityRoleId { get; set; }
    public virtual EntityRole? EntityRole { get; set; }
    
    public int EntityId { get; set; }
    public required string EntityType { get; set; }
}