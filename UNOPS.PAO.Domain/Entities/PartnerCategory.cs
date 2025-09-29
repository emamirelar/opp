using System.Collections.Generic;
using System;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Read-only entity representing Partner Categories derived from PartnerTree
/// Categories are PartnerTree nodes that meet specific business rules:
/// - Level_1 nodes that are NOT in specialCategoryCodes (MULTILATERAL, GOVERNMENT)
/// - Level_2 nodes that are children of specialCategoryCodes
/// </summary>
[NotMapped] // This is a read-only entity, not mapped to a database table
public class PartnerCategory : IBaseBusinessEntity<int>
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
    public required string PartnerCategoryCode { get; set; }
    
    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<PartnerGroup> PartnerGroups { get; set; } = new List<PartnerGroup>();
    
    // Computed properties
    [NotMapped]
    public int PartnerGroupCount => PartnerGroups?.Count ?? 0;
    
    [NotMapped]
    public int TotalPartnerCount { get; set; } // Will be populated by service
    
    /// <summary>
    /// Determines if a PartnerTree node qualifies as a PartnerCategory
    /// </summary>
    public static bool IsPartnerCategory(PartnerTree partnerTree)
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
    /// Creates a PartnerCategory from a PartnerTree node
    /// </summary>
    public static PartnerCategory FromPartnerTree(PartnerTree partnerTree)
    {
        if (!IsPartnerCategory(partnerTree))
            throw new InvalidOperationException($"PartnerTree with code '{partnerTree.Code}' does not qualify as a PartnerCategory");
            
        return new PartnerCategory
        {
            Id = partnerTree.Id,
            Name = partnerTree.Name,
            Status = partnerTree.Status,
            Description = partnerTree.Description,
            Code = partnerTree.Code,
            Type = partnerTree.Type,
            Parent = partnerTree.Parent,
            PartnerCategoryCode = partnerTree.PartnerCategoryCode ?? partnerTree.Code
        };
    }
}
