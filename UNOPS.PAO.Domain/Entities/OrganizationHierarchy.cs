using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Domain.Entities;

public class OrganizationHierarchy : ModifiableDeletableEntity
{
    public string Code { get; set; }
    public string Name { get; set; }
    public OrganizationUnitType Type { get; set; }
    public string Description { get; set; }
    public int? ParentId { get; set; }
    public virtual OrganizationHierarchy Parent { get; set; }
    public virtual ICollection<OrganizationHierarchy> Children { get; set; }

    public OrganizationHierarchy()
    {
        Children = new HashSet<OrganizationHierarchy>();
    }
} 