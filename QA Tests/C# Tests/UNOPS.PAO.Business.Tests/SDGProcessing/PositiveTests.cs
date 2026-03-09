/**
 * @fileoverview PNO-1166: SDG deduplication and primary fallback — Positive tests.
 *
 * Ratio: P=10
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.SDGProcessing;

/// <summary>
/// Positive (happy-path) tests for SDG post-processing.
/// Requirements: REQ-1, REQ-2, REQ-3, REQ-4, REQ-5, REQ-7
/// </summary>
[Collection("SDGProcessing")]
public class SDGProcessingPositiveTests
{
    private readonly SDGProcessingFixture _fixture;

    public SDGProcessingPositiveTests(SDGProcessingFixture fixture) => _fixture = fixture;

    [Fact]
    [Trait("Category", "Positive")]
    public void P01_NoPrimary_FirstSdgSetAsPrimary_REQ1()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        result.Should().NotBeNull();
        var first = result[0] as JObject;
        first.Should().NotBeNull();
        (first!["isPrimary"]?.Value<bool>() ?? false).Should().BeTrue("REQ-1: First SDG should be set as primary when none marked");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P02_AtLeastOnePrimary_NoChangesToPrimaryFlags_REQ2()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, true), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        var primaryCount = result.OfType<JObject>().Count(o => o["isPrimary"]?.Value<bool>() ?? false);
        primaryCount.Should().Be(1, "REQ-2: Exactly one primary when one was already marked");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P03_DuplicateSdgIds_Deduplicated_REQ3()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        result.Should().HaveCount(2, "REQ-3: Duplicate sdgId=4 should be deduplicated");
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(4, 13);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P04_DuplicateWithPrimary_PreferredEntryKept_REQ4()
    {
        var first = SDGProcessingFixture.CreateSdg(4, false);
        var second = SDGProcessingFixture.CreateSdg(4, true);
        var arr = new JArray { first, second };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        result.Should().HaveCount(1);
        var kept = result[0] as JObject;
        kept!["isPrimary"]!.Value<bool>().Should().BeTrue("REQ-4: Entry with isPrimary=true should be preferred");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P05_Deduplication_PreservesFirstOccurrenceOrder_REQ5()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((13, false), (4, false), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(new int?[] { 13, 4, 1 }, "REQ-5: Order should follow first occurrence");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P06_NonSdgDependent_ReturnsUnchanged_REQ7()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false));
        var copy = SDGProcessingSpec.DeepCopy(arr);
        var result = SDGProcessingSpec.ApplySDGPostProcessing("partners", arr);

        result.Should().BeSameAs(arr, "REQ-7: Non-SDG dependent should return same array");
        result.Should().BeEquivalentTo(copy, "REQ-7: Content should be unchanged");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P07_SingleSdg_NoPrimary_GetsPrimary_REQ1()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("REQ-1: Single SDG without primary gets primary");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P08_CaseInsensitive_SdGs_TriggersLogic()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("SDGS", arr);

        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("sdGs comparison is case-insensitive");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P09_MultipleUniqueSdgs_NoDedup_AllPreserved()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((1, false), (4, true), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        result.Should().HaveCount(3);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(1, 4, 13);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P10_ThreeDuplicates_PrimaryInMiddle_Preferred()
    {
        var arr = new JArray
        {
            SDGProcessingFixture.CreateSdg(4, false),
            SDGProcessingFixture.CreateSdg(4, true),
            SDGProcessingFixture.CreateSdg(4, false)
        };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);

        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Middle entry with isPrimary should be kept");
    }
}
