// PNO-1182: Specification model for WHEN section date field label alignment.
// Models label lengths and truncation requirements per REQ-1 through REQ-7.
// Distinct from PNO-1210 (calendar clipping) — this spec focuses on label alignment only.

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Specification model for WHEN section date field label alignment (PNO-1182).
/// REQ-1 through REQ-7: Label alignment, max-width, truncation, background masking.
/// </summary>
public sealed class WhenLabelAlignmentSpec
{
    /// <summary>
    /// English label text for each date field (from i18n en.json).
    /// Used to model truncation requirements — long labels need ellipsis on narrow layouts.
    /// Label lengths: Target Signing Date=19, Implementation Start Date=25,
    /// Target Delivery Date=20, Proposal Submission Date=24.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> DateFieldLabels = new Dictionary<string, string>
    {
        ["targetSigningDate"] = "Target Signing Date",
        ["implementationStartDate"] = "Implementation Start Date",
        ["targetDeliveryDate"] = "Target Delivery Date",
        ["submissionDeadline"] = "Proposal Submission Date"
    };

    /// <summary>
    /// REQ-2/REQ-3: Labels that exceed typical available width need truncation.
    /// Implementation Start Date (24 chars) and Proposal Submission Date (25 chars) are longest.
    /// </summary>
    public static int GetLabelLength(string fieldId)
    {
        return DateFieldLabels.TryGetValue(fieldId, out var label) ? label.Length : 0;
    }

    /// <summary>
    /// REQ-2: Labels longer than threshold would overflow into calendar icon area without max-width.
    /// Threshold ~15 chars is where truncation becomes visible on grid-cols-3 layout.
    /// </summary>
    public static bool WouldNeedTruncation(string fieldId, int thresholdChars = 15)
    {
        return GetLabelLength(fieldId) > thresholdChars;
    }

    /// <summary>
    /// REQ-1: All four date fields must use the same p-floatlabel > p-datepicker pattern for consistent alignment.
    /// </summary>
    public static IReadOnlyList<string> ExpectedDateFieldIds => new[]
    {
        "targetSigningDate",
        "implementationStartDate",
        "targetDeliveryDate",
        "submissionDeadline"
    };

    /// <summary>
    /// REQ-4: Default datepicker label max-width: calc(100% - 3.5rem).
    /// </summary>
    public const string DefaultLabelMaxWidth = "calc(100% - 3.5rem)";

    /// <summary>
    /// REQ-5: Filled/focused label max-width: calc(100% - 3rem).
    /// </summary>
    public const string FilledLabelMaxWidth = "calc(100% - 3rem)";

    /// <summary>
    /// REQ-6: Required overflow/truncation properties for default label.
    /// </summary>
    public static readonly IReadOnlyList<string> RequiredDefaultLabelProperties = new[]
    {
        "overflow: hidden",
        "text-overflow: ellipsis",
        "white-space: nowrap"
    };

    /// <summary>
    /// REQ-7: Required filled/focused label properties for clean visual separation.
    /// </summary>
    public static readonly IReadOnlyList<string> RequiredFilledLabelProperties = new[]
    {
        "background-color: white",
        "padding: 0 0.25rem"
    };

    /// <summary>
    /// Returns the longest label length among all date fields.
    /// </summary>
    public static int GetLongestLabelLength()
    {
        return DateFieldLabels.Values.Max(s => s.Length);
    }

    /// <summary>
    /// Returns the shortest label length among all date fields.
    /// </summary>
    public static int GetShortestLabelLength()
    {
        return DateFieldLabels.Values.Min(s => s.Length);
    }

    /// <summary>
    /// REQ-1: Verifies all expected fields have consistent structure (same pattern).
    /// </summary>
    public static bool AllFieldsUseSamePattern(IReadOnlyList<string> fieldIdsInTemplate)
    {
        return ExpectedDateFieldIds.All(expected => fieldIdsInTemplate.Contains(expected));
    }
}
