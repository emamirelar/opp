using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;
public class OrganizationUnit : ModifiableDeletableEntity
{
    public string Code { get; set; }
}