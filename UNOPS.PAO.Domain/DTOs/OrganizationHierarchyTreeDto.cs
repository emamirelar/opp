using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Domain.DTOs;

/// <summary>
/// DTO for organization hierarchy tree nodes that matches the structure needed by PrimeNG organization chart
/// </summary>
public class OrganizationHierarchyTreeDto
{
    /// <summary>
    /// Whether the node is expanded in the tree
    /// </summary>
    public bool Expanded { get; set; } = true;

    /// <summary>
    /// Type of node (always 'person' for compatibility with PrimeNG chart)
    /// </summary>
    public string Type { get; set; } = "person";

    /// <summary>
    /// The data contained in this node
    /// </summary>
    public required OrganizationHierarchyNodeData Data { get; set; }

    /// <summary>
    /// Child nodes
    /// </summary>
    public List<OrganizationHierarchyTreeDto> Children { get; set; } = new List<OrganizationHierarchyTreeDto>();
}

/// <summary>
/// Data structure for organization hierarchy node
/// </summary>
public class OrganizationHierarchyNodeData
{
    /// <summary>
    /// ID of the organization unit
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the organization unit
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Code of the organization unit
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Type of the organization unit
    /// </summary>
    public OrganizationUnitType Type { get; set; }

    /// <summary>
    /// Description of the organization unit
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// ID of the parent organization unit (null for root nodes)
    /// </summary>
    public int? ParentId { get; set; }
} 