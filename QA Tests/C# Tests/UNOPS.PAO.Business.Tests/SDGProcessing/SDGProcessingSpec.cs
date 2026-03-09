/**
 * @fileoverview Specification helpers for SDG deduplication and primary fallback logic.
 * Replicates the post-processing in AiContextualService.BuildOpportunityCollectionObjects (PNO-1166).
 *
 * Requirements validated:
 * - REQ-1: When no SDG has isPrimary=true, the first SDG should be set as primary
 * - REQ-2: When at least one SDG has isPrimary=true, no changes to primary flags
 * - REQ-3: Duplicate SDGs (same sdgId) should be deduplicated
 * - REQ-4: When deduplicating, the entry with isPrimary=true should be preferred
 * - REQ-5: Deduplication should preserve first-occurrence order
 * - REQ-6: SDGs without sdgId should be skipped during deduplication
 * - REQ-7: Non-SDG dependent types should NOT trigger this logic
 *
 * @author UNOPS Opportunity+ QA Team
 */

using Newtonsoft.Json.Linq;

namespace UNOPS.PAO.Business.Tests.SDGProcessing;

/// <summary>
/// PNO-1166: SDG post-processing specification.
/// Pure functions replicating the SDG logic from BuildOpportunityCollectionObjects.
/// </summary>
public static class SDGProcessingSpec
{
    /// <summary>
    /// Applies SDG-specific post-processing: primary fallback and deduplication.
    /// Matches the logic in AiContextualService.BuildOpportunityCollectionObjects (lines 3274-3310).
    /// </summary>
    /// <param name="dependent">The dependent type (e.g. "sdGs", "partners").</param>
    /// <param name="objectsArray">Input JArray of SDG objects. May be mutated for primary fallback.</param>
    /// <returns>Processed JArray (possibly new instance after deduplication).</returns>
    public static JArray ApplySDGPostProcessing(string dependent, JArray objectsArray)
    {
        if (objectsArray == null)
            return new JArray();

        if (!dependent.Equals("sdGs", StringComparison.OrdinalIgnoreCase))
            return objectsArray;

        // Primary fallback: if no SDG has isPrimary=true, set first as primary
        if (objectsArray.Count >= 1)
        {
            var anyPrimary = objectsArray.OfType<JObject>().Any(o => o["isPrimary"]?.Value<bool>() ?? false);
            if (!anyPrimary)
            {
                var first = objectsArray[0] as JObject;
                if (first != null)
                    first["isPrimary"] = true;
            }
        }

        // Deduplicate SDGs by sdgId
        if (objectsArray.Count > 1)
        {
            var byId = new Dictionary<int, JObject>();
            var order = new List<int>();
            foreach (var item in objectsArray.OfType<JObject>())
            {
                var sdgId = item["sdgId"]?.Value<int>();
                if (!sdgId.HasValue) continue;
                var isPrimary = item["isPrimary"]?.Value<bool>() ?? false;
                if (!byId.TryGetValue(sdgId.Value, out var existing))
                {
                    byId[sdgId.Value] = item;
                    order.Add(sdgId.Value);
                }
                else if (isPrimary && !(existing["isPrimary"]?.Value<bool>() ?? false))
                {
                    byId[sdgId.Value] = item;
                }
            }
            return new JArray(order.Select(id => byId[id]));
        }

        return objectsArray;
    }

    /// <summary>
    /// Creates a deep copy of a JArray so tests can compare before/after without mutation side effects.
    /// </summary>
    public static JArray DeepCopy(JArray source)
    {
        if (source == null) return new JArray();
        return (JArray)source.DeepClone();
    }
}
