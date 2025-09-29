using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Domain.Entities;

public class OrganizationHierarchy : ModifiableDeletableEntity
{
    public required string Code { get; set; }
    public required new string Name { get; set; }
    public OrganizationUnitType Type { get; set; }
    public required string Description { get; set; }
    public int? ParentId { get; set; }
    public bool IsSelfManagementEnabled { get; set; } = false;
    public virtual OrganizationHierarchy? Parent { get; set; }
    public virtual ICollection<OrganizationHierarchy> Children { get; set; }
    
    // Navigation property for entity relationships
    public virtual ICollection<OrganizationUnitRelationship> EntityRelationships { get; set; }

    // Computed properties for list and search operations
    [NotMapped]
    public string? ParentName { get; set; }
    
    [NotMapped]
    public string? ParentCode { get; set; }
    
    [NotMapped]
    public int ChildrenCount { get; set; }
    
    [NotMapped]
    public int EntityRelationshipCount { get; set; }

    public OrganizationHierarchy()
    {
        Children = new HashSet<OrganizationHierarchy>();
        EntityRelationships = new HashSet<OrganizationUnitRelationship>();
    }
} 