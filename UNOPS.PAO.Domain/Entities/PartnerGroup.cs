using System.Collections.Generic;
using System;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Linq;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Read-only entity representing Partner Groups derived from PartnerTree
/// Groups are PartnerTree nodes that meet specific business rules:
/// - Must have a parent (not root level)
/// - Parent must be a Category OR Parent must be a Group (recursive)
/// </summary>
[NotMapped] // This is a read-only entity, not mapped to a database table
public class PartnerGroup : IBaseBusinessEntity<int>
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public EntityStatus Status { get; set; }
    
    [MaxLength(500)]
    public required string Description { get; set; }
    
    [MaxLength(50)]
    public required string Code { get; set; }
    
    [MaxLength(20)]
    public required string Type { get; set; }
    
    [MaxLength(50)]
    public string? Parent { get; set; }
    
    [MaxLength(50)]
    public required string PartnerGroupCode { get; set; }
    
    // Category relationship
    public int? PartnerCategoryId { get; set; }
    
    [MaxLength(50)]
    public string? PartnerCategoryCode { get; set; }
    
    [MaxLength(255)]
    public string? PartnerCategoryName { get; set; }
    
    // Navigation properties
    [JsonIgnore]
    public virtual PartnerCategory? PartnerCategory { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Partner> Partners { get; set; } = new List<Partner>();
    
    // Computed properties
    [NotMapped]
    public int PartnerCount => Partners?.Count ?? 0;
    
    [NotMapped]
    public int TotalPartnerCount { get; set; } // Will be populated by service including sub-groups
    
    /// <summary>
    /// Determines if a PartnerTree node qualifies as a PartnerGroup
    /// </summary>
    public static bool IsPartnerGroup(PartnerTree partnerTree, IEnumerable<PartnerTree> allPartnerTrees)
    {
        if (partnerTree == null || string.IsNullOrEmpty(partnerTree.Parent)) return false;
        
        var partnerTreesList = allPartnerTrees.ToList();
        var parentPartnerTree = partnerTreesList.FirstOrDefault(pt => pt.Code == partnerTree.Parent);
        if (parentPartnerTree == null) return false;

        // Rule: is a child of a category OR is a child of a group (recursive)
        return IsPartnerCategory(parentPartnerTree) || IsPartnerGroup(parentPartnerTree, partnerTreesList);
    }
    
    /// <summary>
    /// Helper method to check if a PartnerTree is a category (replicates PartnerCategory logic)
    /// </summary>
    private static bool IsPartnerCategory(PartnerTree partnerTree)
    {
        if (partnerTree == null) return false;
        
        // Rule 1: Level_1 and not in specialCategoryCodes
        if (partnerTree.Type == "Level_1" && !PartnerTree.specialCategoryCodes.Contains(partnerTree.Code))
        {
            return true;
        }

        // Rule 2: Level_2 and parent is in specialCategoryCodes
        if (partnerTree.Type == "Level_2" && !string.IsNullOrEmpty(partnerTree.Parent) && 
            PartnerTree.specialCategoryCodes.Contains(partnerTree.Parent))
        {
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Creates a PartnerGroup from a PartnerTree node
    /// </summary>
    public static PartnerGroup FromPartnerTree(PartnerTree partnerTree, IEnumerable<PartnerTree> allPartnerTrees, PartnerCategory? category = null)
    {
        if (!IsPartnerGroup(partnerTree, allPartnerTrees))
            throw new InvalidOperationException($"PartnerTree with code '{partnerTree.Code}' does not qualify as a PartnerGroup");
            
        var partnerGroup = new PartnerGroup
        {
            Id = partnerTree.Id,
            Name = partnerTree.Name,
            Status = partnerTree.Status,
            Description = partnerTree.Description,
            Code = partnerTree.Code,
            Type = partnerTree.Type,
            Parent = partnerTree.Parent,
            PartnerGroupCode = partnerTree.PartnerGroupCode ?? partnerTree.Code
        };
        
        // Set category information if provided
        if (category != null)
        {
            partnerGroup.PartnerCategoryId = category.Id;
            partnerGroup.PartnerCategoryCode = category.PartnerCategoryCode;
            partnerGroup.PartnerCategoryName = category.Name;
            partnerGroup.PartnerCategory = category;
        }
        
        return partnerGroup;
    }
    
    /// <summary>
    /// Finds the parent category for this group by traversing up the hierarchy
    /// </summary>
    public static PartnerCategory? FindParentCategory(PartnerTree partnerTree, IEnumerable<PartnerTree> allPartnerTrees)
    {
        if (partnerTree == null || string.IsNullOrEmpty(partnerTree.Parent)) return null;
        
        var partnerTreesList = allPartnerTrees.ToList();
        var parent = partnerTreesList.FirstOrDefault(pt => pt.Code == partnerTree.Parent);
        
        if (parent == null) return null;
        
        // If parent is a category, return it
        if (IsPartnerCategory(parent))
        {
            return PartnerCategory.FromPartnerTree(parent);
        }
        
        // If parent is a group, recursively find its category
        return FindParentCategory(parent, partnerTreesList);
    }
}
