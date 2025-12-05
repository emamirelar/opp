using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// PreDefined High Risk entity - represents the High Risk Checklist items from oUP (EAC questions)
/// These are standard risks that can be auto-detected or manually selected based on opportunity data.
/// Data sourced from risk_questions.csv
/// </summary>
public class PreDefinedHighRisk : ModifiableDeletableEntity<int, int>
{
    /// <summary>
    /// Risk category short code (e.g., "LEGAL_REGUL_FRWRK_OP", "REGIONAL_LOCAL_INSTABILIT")
    /// Links to RiskCategory.ShortCode for the Level 3 category
    /// </summary>
    [MaxLength(50)]
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Level 1 grouping number (1, 2, 3, or 4)
    /// </summary>
    public int Level1 { get; set; }

    /// <summary>
    /// Level 2 code (e.g., "1.1", "1.2", "2.1")
    /// </summary>
    [MaxLength(10)]
    public string Level2Code { get; set; } = string.Empty;

    /// <summary>
    /// Original oUP question ID for mapping back to legacy system
    /// </summary>
    public int OupQuestionId { get; set; }

    /// <summary>
    /// Risk code (e.g., "1.1.1", "1.2.1", "3.2.1")
    /// </summary>
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display code - may differ from Code (e.g., Code="2.2.1" but DisplayCode="2.4.1")
    /// </summary>
    [MaxLength(20)]
    public string DisplayCode { get; set; } = string.Empty;

    /// <summary>
    /// Full description of the high risk item (the EAC question text)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Short title for display (derived from description)
    /// </summary>
    [MaxLength(255)]
    public string ShortTitle { get; set; } = string.Empty;

    /// <summary>
    /// Whether this high risk can be automatically detected from opportunity data
    /// </summary>
    public bool IsAutoDetectable { get; set; }

    /// <summary>
    /// Type of detection rule if auto-detectable
    /// e.g., "COUNTRY_FRAGILE", "PARTNER_DRAFT", "NON_USD_CURRENCY"
    /// </summary>
    [MaxLength(50)]
    public string? DetectionRuleType { get; set; }

    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// FK to RiskCategory (Level 3 category)
    /// </summary>
    public int? RiskCategoryId { get; set; }

    /// <summary>
    /// Navigation property to RiskCategory
    /// </summary>
    [ForeignKey(nameof(RiskCategoryId))]
    public virtual RiskCategory? RiskCategory { get; set; }
}

