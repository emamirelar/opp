using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Risk category hierarchy entity (3 levels) aligned with oUP
/// Level 1: Top-level categories (e.g., Finance, Partners &amp; stakeholders, People, Process/Operations)
/// Level 2: Sub-categories (e.g., Contributions, Fraud and ethics, etc.)
/// Level 3: Leaf categories - users select from this level when creating risks
/// </summary>
public class RiskCategory : ModifiableDeletableEntity<int, int>
{
    /// <summary>
    /// Unique code for the category (e.g., "UPC1_FINANCE", "UPC2_CONTRIBUTIONS", "UPC3_ENG_COST_PRICE")
    /// </summary>
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Short code without prefix (e.g., "FINANCE", "CONTRIBUTIONS", "ENG_COST_PRICE")
    /// Used for mapping to oUP and PreDefinedHighRisk
    /// </summary>
    [MaxLength(50)]
    public string ShortCode { get; set; } = string.Empty;

    /// <summary>
    /// Category hierarchy level (1, 2, or 3)
    /// Level 3 = leaf level (selectable by users)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Parent category ID (null for Level 1 categories)
    /// </summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// Parent category short code (for seeding purposes)
    /// </summary>
    [MaxLength(50)]
    public string? ParentShortCode { get; set; }

    /// <summary>
    /// Navigation property to parent category
    /// </summary>
    [ForeignKey(nameof(ParentCategoryId))]
    public virtual RiskCategory? ParentCategory { get; set; }

    /// <summary>
    /// Navigation property to child categories
    /// </summary>
    public virtual ICollection<RiskCategory> ChildCategories { get; set; } = new HashSet<RiskCategory>();

    /// <summary>
    /// Display order within the same level/parent
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Full path display name (e.g., "Finance > Contributions > Engagement costing and pricing")
    /// Computed/cached for display purposes
    /// </summary>
    [NotMapped]
    public string FullPath
    {
        get
        {
            if (ParentCategory == null)
                return Name;
            return $"{ParentCategory.FullPath} > {Name}";
        }
    }

    /// <summary>
    /// Whether this is a leaf category (Level 3) that can be selected
    /// </summary>
    [NotMapped]
    public bool IsSelectable => Level == 3;
}

