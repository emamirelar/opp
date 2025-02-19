using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Entities;

namespace UNOPS.PAO.Domain.Entities;
public class EntityUserRole : ModifiableDeletableEntity
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public IdentityUserRole<int> UserRole { get; set; }
    public int EntityId { get; set; }
    public string EntityType { get; set; }
}