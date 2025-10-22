using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models.OrganizationUnits;

/// <summary>
/// Model designed specifically for PrimeNG organization chart component
/// </summary>
public class OrganizationHierarchyPrimeModel
{
    /// <summary>
    /// Whether the node is expanded in the chart
    /// </summary>
    public bool Expanded { get; set; } = true;
    
    /// <summary>
    /// Type of node (always 'person' for PrimeNG)
    /// </summary>
    public string Type { get; set; } = "person";
    
    /// <summary>
    /// Data for this node
    /// </summary>
    public OrganizationHierarchyPrimeDataModel Data { get; set; }
    
    /// <summary>
    /// Child nodes
    /// </summary>
    public List<OrganizationHierarchyPrimeModel> Children { get; set; } = new();
}

/// <summary>
/// Data model for PrimeNG organization chart node
/// </summary>
public class OrganizationHierarchyPrimeDataModel
{
    /// <summary>
    /// ID of the organization unit
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the organization unit
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Code of the organization unit
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// Type of the organization unit (0=Organization, 1=Business Group, 2=Country Office, 3=Unit)
    /// </summary>
    public OrganizationUnitType Type { get; set; }
    
    /// <summary>
    /// Description of the organization unit
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// ID of the parent organization unit
    /// </summary>
    public int? ParentId { get; set; }
} 