using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;
public class UserInfo : ModifiableDeletableEntity
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? UserEmail { get; set; }
    public string? OrgUnit { get; set; }
    public int? SupervisorId { get; set; }
}