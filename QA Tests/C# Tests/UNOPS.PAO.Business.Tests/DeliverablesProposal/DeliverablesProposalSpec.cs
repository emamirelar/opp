/// <summary>
/// PNO-1166: Specification for Deliverables Proposal model refactor and UI template changes.
///
/// Requirements validated:
/// - REQ-1: Backend BuildDeliverableObject returns all required fields
/// - REQ-2: Frontend ProposedDeliverable interface matches backend response shape
/// - REQ-3: Old deprecated fields are NOT in the new interface
/// - REQ-4: HTML template references the new level fields (level0-level4)
/// - REQ-5: HTML template displays deliverable name as: level4 || level3 || level2 || level1 || level0 || outputName
/// - REQ-6: HTML template uses p-chip for serviceLine and level0 tags
/// - REQ-7: quantity can be null (nullable in both TS and C#)
/// </summary>

using Newtonsoft.Json.Linq;

namespace UNOPS.PAO.Business.Tests.DeliverablesProposal;

/// <summary>
/// Specification constants and helpers for PNO-1166 deliverables model refactor.
/// </summary>
public static class DeliverablesProposalSpec
{
    // ── File paths (relative to workspace root) ─────────────────────────────
    public const string TypeScriptModelPath = "UNOPS.PAO.ClientApp/src/app/features/partnerships/interactions/models/interaction-selection.model.ts";
    public const string HtmlTemplatePath = "UNOPS.PAO.ClientApp/src/app/features/partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component.html";

    // ── Backend BuildDeliverableObject required fields (REQ-1) ───────────────
    public static readonly string[] RequiredBackendFields = new[]
    {
        "outputId", "outputName", "level0", "level1", "level2", "level3", "level4",
        "definitionLevel1", "definitionLevel2", "definitionLevel3", "definitionLevel4",
        "serviceLine", "quantity"
    };

    // ── Frontend ProposedDeliverable expected fields (REQ-2) ─────────────────
    public static readonly string[] ExpectedFrontendFields = new[]
    {
        "outputId", "outputName", "level0", "level1", "level2", "level3", "level4",
        "definitionLevel1", "definitionLevel2", "definitionLevel3", "definitionLevel4",
        "serviceLine", "quantity"
    };

    // ── Deprecated fields that must NOT appear (REQ-3) ────────────────────────
    public static readonly string[] DeprecatedFields = new[]
    {
        "outputDescription", "outputGroup", "outputSubGroup", "outputServiceLine",
        "unitCode", "projectCategoryCode", "notes"
    };

    // ── HTML template level field references (REQ-4) ───────────────────────────
    public static readonly string[] LevelFieldReferences = new[]
    {
        "deliverable.level0", "deliverable.level1", "deliverable.level2",
        "deliverable.level3", "deliverable.level4"
    };

    // ── Name display order (REQ-5): level4 || level3 || level2 || level1 || level0 || outputName
    public const string ExpectedNameDisplayPattern = "deliverable.level4 || deliverable.level3 || deliverable.level2 || deliverable.level1 || deliverable.level0 || deliverable.outputName";

    /// <summary>
    /// Resolves path to a file from workspace root. Tries multiple base directories.
    /// </summary>
    public static string ResolvePath(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "..", relativePath),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", relativePath),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), relativePath),
        };
        foreach (var p in candidates)
        {
            var full = Path.GetFullPath(p);
            if (File.Exists(full))
                return full;
        }
        return Path.GetFullPath(Path.Combine(baseDir, Path.GetFileName(relativePath)));
    }

    /// <summary>
    /// Reads file content or returns empty string if not found.
    /// </summary>
    public static string ReadFileOrEmpty(string path)
        => File.Exists(path) ? File.ReadAllText(path) : string.Empty;

    /// <summary>
    /// Reads the TypeScript model file content.
    /// </summary>
    public static string ReadTypeScriptModel()
    {
        var path = ResolvePath(TypeScriptModelPath);
        return ReadFileOrEmpty(path);
    }

    /// <summary>
    /// Reads the HTML template file content.
    /// </summary>
    public static string ReadHtmlTemplate()
    {
        var path = ResolvePath(HtmlTemplatePath);
        return ReadFileOrEmpty(path);
    }

    /// <summary>
    /// Builds a JObject matching the backend BuildDeliverableObject response shape (REQ-1).
    /// </summary>
    public static JObject BuildExpectedDeliverableJObject(
        int outputId = 1,
        string outputName = "Test Output",
        string? level0 = "",
        string? level1 = "",
        string? level2 = "",
        string? level3 = "",
        string? level4 = "",
        string? definitionLevel1 = "",
        string? definitionLevel2 = "",
        string? definitionLevel3 = "",
        string? definitionLevel4 = "",
        string? serviceLine = "",
        object? quantity = null)
    {
        return new JObject
        {
            ["outputId"] = outputId,
            ["outputName"] = outputName,
            ["level0"] = level0 ?? "",
            ["level1"] = level1 ?? "",
            ["level2"] = level2 ?? "",
            ["level3"] = level3 ?? "",
            ["level4"] = level4 ?? "",
            ["definitionLevel1"] = definitionLevel1 ?? "",
            ["definitionLevel2"] = definitionLevel2 ?? "",
            ["definitionLevel3"] = definitionLevel3 ?? "",
            ["definitionLevel4"] = definitionLevel4 ?? "",
            ["serviceLine"] = serviceLine ?? "",
            ["quantity"] = quantity == null ? JValue.CreateNull() : JToken.FromObject(quantity)
        };
    }

    /// <summary>
    /// Checks if the TypeScript model contains a given field name in ProposedDeliverable.
    /// </summary>
    public static bool TypeScriptModelContainsField(string tsContent, string fieldName)
    {
        if (string.IsNullOrEmpty(tsContent)) return false;
        // ProposedDeliverable interface: look for "fieldName" or "fieldName?:" pattern
        var pattern = $@"\b{fieldName}\s*\??\s*[:;]";
        return System.Text.RegularExpressions.Regex.IsMatch(tsContent, pattern);
    }

    /// <summary>
    /// Checks if the TypeScript model contains any of the deprecated fields.
    /// </summary>
    public static bool TypeScriptModelContainsDeprecatedField(string tsContent)
    {
        return DeprecatedFields.Any(f => TypeScriptModelContainsField(tsContent, f));
    }

    /// <summary>
    /// Gets the display name for a deliverable using the fallback order (REQ-5).
    /// Treats empty strings as "empty" (same as JS/TS ||), so falls back to outputName when all levels are empty/null.
    /// </summary>
    public static string GetDisplayName(string? level4, string? level3, string? level2, string? level1, string? level0, string outputName)
    {
        return FirstNonEmpty(level4, level3, level2, level1, level0, outputName) ?? "";
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrEmpty(v))
                return v;
        }
        return null;
    }
}
