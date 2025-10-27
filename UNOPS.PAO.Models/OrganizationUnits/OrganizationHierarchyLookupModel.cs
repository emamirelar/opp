namespace UNOPS.PAO.Models.OrganizationUnits;

/// <summary>
/// Lookup model for OrganizationHierarchy - simplified version for dropdown/lookup scenarios
/// </summary>
public class OrganizationHierarchyLookupModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? ParentCode { get; set; }
    public bool IsSelfManagementEnabled { get; set; }
    
    // For dropdown display
    public string DisplayName => $"{Code} - {Name}";
    
    // For hierarchical display
    public string HierarchicalDisplayName => ParentName != null ? $"{ParentName} > {Name}" : Name;
}