/**
 * @fileoverview Fixture providing SDG test data builders for PNO-1166 tests.
 *
 * @author UNOPS Opportunity+ QA Team
 */

using Newtonsoft.Json.Linq;

namespace UNOPS.PAO.Business.Tests.SDGProcessing;

/// <summary>
/// Shared fixture for SDG processing tests. Provides factory methods for SDG JObjects.
/// </summary>
public sealed class SDGProcessingFixture
{
    /// <summary>
    /// Creates a JObject representing an SDG with sdgId and optional isPrimary.
    /// </summary>
    public static JObject CreateSdg(int sdgId, bool isPrimary = false, string? name = null)
    {
        var obj = new JObject
        {
            ["sdgId"] = sdgId,
            ["isPrimary"] = isPrimary
        };
        if (name != null)
            obj["name"] = name;
        return obj;
    }

    /// <summary>
    /// Creates an SDG object without sdgId (for boundary tests).
    /// </summary>
    public static JObject CreateSdgWithoutId(string name = "Unknown")
    {
        return new JObject
        {
            ["name"] = name,
            ["isPrimary"] = false
        };
    }

    /// <summary>
    /// Creates a JArray of SDGs from (sdgId, isPrimary) tuples.
    /// </summary>
    public static JArray CreateSdgArray(params (int sdgId, bool isPrimary)[] items)
    {
        var arr = new JArray();
        foreach (var (sdgId, isPrimary) in items)
            arr.Add(CreateSdg(sdgId, isPrimary));
        return arr;
    }

    /// <summary>
    /// Standard SDG IDs used in tests (UN SDG goals 1-17).
    /// </summary>
    public static readonly int[] StandardSdgIds = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
}

[CollectionDefinition("SDGProcessing")]
public class SDGProcessingCollection : ICollectionFixture<SDGProcessingFixture> { }
