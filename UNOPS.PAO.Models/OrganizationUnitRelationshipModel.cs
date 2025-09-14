namespace UNOPS.PAO.Models;

public class OrganizationUnitRelationshipModel
{
    public int OrganizationHierarchyId { get; set; }
    public OrganizationHierarchyModel? OrganizationHierarchy { get; set; }
    public int EntityId { get; set; }
    public string EntityType { get; set; }
} 