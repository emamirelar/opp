using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.OrganizationUnits;

public class OrganizationHierarchyModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? ParentCode { get; set; }
    public bool IsSelfManagementEnabled { get; set; }
    
    // Computed properties
    public int ChildrenCount { get; set; }
    public int EntityRelationshipCount { get; set; }
    
    // RBAC permissions
    public EntityPermissionsModel? Permissions { get; set; }
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

public class OrganizationHierarchyFilterRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public int? ParentId { get; set; }
    public string? ParentCode { get; set; }
    public string? Status { get; set; }
    public bool? IsSelfManagementEnabled { get; set; }
    public bool IncludeCounts { get; set; } = true;
}

public class OrganizationHierarchySearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public int? ParentId { get; set; }
    public string? Status { get; set; }
    public bool? IsSelfManagementEnabled { get; set; }
    public int? MinChildrenCount { get; set; }
    public int? MaxChildrenCount { get; set; }
    public int PageSize { get; set; } = 20;
    public int PageIndex { get; set; } = 1;
    public string? OrderBy { get; set; } = "Name";
    public bool Ascending { get; set; } = true;
}