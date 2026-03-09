/// <summary>
/// Specification model for Opportunity WHAT - Products &amp; Services section.
/// PNO-700: WHAT section exists, deliverables tree, output selection, delivery modality, beneficiaries, estimated value.
/// PNO-864: Product/service selection UX (manual search, AI search, multi-select).
/// </summary>

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

/// <summary>
/// Specification model for Opportunity WHAT section.
/// PNO-700 AC1: Section "WHAT - Products and Services" exists on opportunity record.
/// PNO-700 AC4: Delivery modality (1=NotYetKnown, 2=AllDirect, 3=AllGrantSupport, 4=Mixed).
/// PNO-864: Manual search, AI search, multi-select outputs with quantities.
/// </summary>
public sealed class OpportunityWhatSectionSpec
{
    /// <summary>Delivery modality: 1=NotYetKnown, 2=AllDirect, 3=AllGrantSupport, 4=Mixed</summary>
    public int? DeliveryModality { get; set; }

    /// <summary>Responsible org unit ID</summary>
    public int? ResponsibleOrgUnitId { get; set; }

    /// <summary>Proposed initiative type ID</summary>
    public int? ProposedInitiativeTypeId { get; set; }

    /// <summary>Deliverables (outputs) selected for this opportunity</summary>
    public List<OpportunityWhatDeliverableSpec> Deliverables { get; set; } = new();

    /// <summary>PNO-700 AC4: Valid delivery modality values</summary>
    public static readonly int[] ValidDeliveryModalityValues = { 1, 2, 3, 4 };

    /// <summary>PNO-864: Minimum search query length for tree search</summary>
    public const int MinTreeSearchLength = 2;

    /// <summary>PNO-864: Minimum search query length for AI semantic search</summary>
    public const int MinAiSearchLength = 3;

    /// <summary>PNO-700 AC4: Validates delivery modality is one of the allowed values</summary>
    public bool IsDeliveryModalityValid()
    {
        return DeliveryModality.HasValue && ValidDeliveryModalityValues.Contains(DeliveryModality.Value);
    }

    /// <summary>PNO-864: Validates tree search query meets minimum length</summary>
    public static bool IsTreeSearchQueryValid(string? query)
    {
        return !string.IsNullOrWhiteSpace(query) && query.Trim().Length >= MinTreeSearchLength;
    }

    /// <summary>PNO-864: Validates AI search query meets minimum length</summary>
    public static bool IsAiSearchQueryValid(string? query)
    {
        return !string.IsNullOrWhiteSpace(query) && query.Trim().Length >= MinAiSearchLength;
    }

    /// <summary>PNO-864: Validates no duplicate output IDs in deliverables</summary>
    public bool HasDuplicateOutputIds()
    {
        var outputIds = Deliverables.Where(d => d.OutputId.HasValue).Select(d => d.OutputId!.Value).ToList();
        return outputIds.Count != outputIds.Distinct().Count();
    }

    /// <summary>PNO-864: Validates at least one deliverable selected when adding</summary>
    public bool HasDeliverables()
    {
        return Deliverables != null && Deliverables.Count > 0;
    }

    /// <summary>PNO-864: Validates output is selectable (terminal node in tree)</summary>
    public static bool IsOutputTerminalAtLevel(string? level0, string? level1, string? level2, string? level3, string? level4, int level)
    {
        var levels = new[] { level0, level1, level2, level3, level4 };
        if (level < 0 || level >= levels.Length || string.IsNullOrEmpty(levels[level]))
            return false;
        if (level == 4)
            return true;
        return string.IsNullOrEmpty(levels[level + 1]);
    }

    /// <summary>PNO-864: Validates quantity is non-negative when provided</summary>
    public static bool IsQuantityValid(int? quantity)
    {
        return !quantity.HasValue || quantity.Value >= 0;
    }
}

/// <summary>
/// Deliverable specification for WHAT section.
/// Maps to OpportunityDeliverable with output hierarchy (level0-level4).
/// </summary>
public sealed class OpportunityWhatDeliverableSpec
{
    public int? OutputId { get; set; }
    public string? OutputName { get; set; }
    public string? Level0 { get; set; }
    public string? Level1 { get; set; }
    public string? Level2 { get; set; }
    public string? Level3 { get; set; }
    public string? Level4 { get; set; }
    public string? ServiceLine { get; set; }
    public int? Quantity { get; set; }
    public bool? ProcurementComponent { get; set; }
}
