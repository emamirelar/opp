using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models;

public class OrganizationHierarchyModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public OrganizationUnitType Type { get; set; }
    public string Description { get; set; }
    public int? ParentId { get; set; }
}

public class OrganizationHierarchyTreeModel
{
    public OrganizationHierarchyDataModel Data { get; set; }
}

public class OrganizationHierarchyDataModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public OrganizationUnitType Type { get; set; }
    public string Description { get; set; }
    public int? ParentId { get; set; }
    public List<OrganizationHierarchyDataModel> Children { get; set; } = new();
}