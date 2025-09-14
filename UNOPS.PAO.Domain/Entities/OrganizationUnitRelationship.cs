using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class OrganizationUnitRelationship : ModifiableDeletableEntity
{
    public int OrganizationHierarchyId { get; set; }
    public OrganizationHierarchy OrganizationHierarchy { get; set; }
    public int EntityId { get; set; }
    public string EntityType { get; set; }
} 