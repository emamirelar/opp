/**
 * @fileoverview PNO-1166: SDG deduplication and primary fallback — Functional tests.
 *
 * Ratio: F ≥ 3×P (30+)
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.SDGProcessing;

/// <summary>
/// Functional tests: business rules, primary fallback before dedup, order, preference logic.
/// </summary>
[Collection("SDGProcessing")]
public class SDGProcessingFunctionalTests
{
    [Fact]
    [Trait("Category", "Functional")]
    public void F01_PrimaryFallbackRunsBeforeDedup()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var kept = result[0] as JObject;
        kept!["isPrimary"]!.Value<bool>().Should().BeTrue("Primary fallback sets first before dedup runs");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F02_DedupUsesPrimaryFallbackResult()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var first = arr[0] as JObject;
        first!["isPrimary"]!.Value<bool>().Should().BeTrue("First was mutated by primary fallback");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F03_OrderPreservation_FirstOccurrence()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((17, false), (1, false), (9, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(17, 1, 9);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F04_PrimaryPreference_WhenDuplicate()
    {
        var nonPrimary = SDGProcessingFixture.CreateSdg(4, false);
        var primary = SDGProcessingFixture.CreateSdg(4, true);
        var arr = new JArray { nonPrimary, primary };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Primary entry preferred when duplicate");
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F05_NoPrimaryPreference_FirstWins()
    {
        var first = SDGProcessingFixture.CreateSdg(4, false);
        var second = SDGProcessingFixture.CreateSdg(4, false);
        var arr = new JArray { first, second };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Primary fallback sets first when none");
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F06_AnyPrimary_NoFallback()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, true));
        var firstBefore = (arr[0] as JObject)!["isPrimary"]?.Value<bool>() ?? false;
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var firstAfter = (result[0] as JObject)!["isPrimary"]?.Value<bool>() ?? false;
        firstBefore.Should().BeFalse();
        firstAfter.Should().BeFalse("First should remain false when second has primary");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F07_CountOne_NoDedupBlock()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F08_CountTwo_DedupBlockRuns()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().NotBeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F09_NonSdg_NoPrimaryFallback()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("partners", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F10_NonSdg_NoDedup()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("partners", arr);
        result.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F11_ReplaceWhenNewIsPrimary_ExistingNotPrimary()
    {
        var existing = SDGProcessingFixture.CreateSdg(4, false);
        var newer = SDGProcessingFixture.CreateSdg(4, true);
        var arr = new JArray { existing, newer };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Primary entry replaces non-primary");
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F12_NoReplaceWhenExistingPrimary_NewNotPrimary()
    {
        var existing = SDGProcessingFixture.CreateSdg(4, true);
        var newer = SDGProcessingFixture.CreateSdg(4, false);
        var arr = new JArray { existing, newer };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Existing primary kept when new is not primary");
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F13_NoReplaceWhenBothPrimary()
    {
        var first = SDGProcessingFixture.CreateSdg(4, true);
        var second = SDGProcessingFixture.CreateSdg(4, true);
        var arr = new JArray { first, second };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("First occurrence kept when both primary");
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F14_SkipItemsWithoutSdgId()
    {
        var withId = SDGProcessingFixture.CreateSdg(4, false);
        var withoutId = new JObject { ["name"] = "X" };
        var arr = new JArray { withId, withoutId };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F15_OfTypeJObject_FiltersNonObjects()
    {
        var arr = new JArray
        {
            SDGProcessingFixture.CreateSdg(4, false),
            new JValue(42),
            SDGProcessingFixture.CreateSdg(13, false)
        };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F16_ResultIsJArray()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().BeOfType<JArray>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F17_ResultItemsAreJObject()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result[0].Should().BeOfType<JObject>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F18_OrderListPreservesInsertionOrder()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((3, false), (1, false), (2, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(3, 1, 2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F19_DictionaryById_UniqueKeys()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, true), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F20_OrderSelectById_MatchesOrderList()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((13, false), (4, false), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().Equal(13, 4, 1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F21_CaseInsensitive_SdGs()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("SDGS", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F22_CaseInsensitive_SdGsMixed()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("SdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F23_PrimaryFallback_OnlyWhenNone()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, true), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var primaryCount = result.OfType<JObject>().Count(o => o["isPrimary"]?.Value<bool>() ?? false);
        primaryCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F24_ThreeDuplicates_PrimaryWins()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, true), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F25_MultipleIds_SomeDuplicated()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((1, false), (4, false), (4, true), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(3);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(1, 4, 13);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F26_DeepCopy_Independent()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var copy = SDGProcessingSpec.DeepCopy(arr);
        copy.Should().NotBeSameAs(arr);
        copy.Should().BeEquivalentTo(arr);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F27_DeepCopy_ModifyOriginal_DoesNotAffectCopy()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var copy = SDGProcessingSpec.DeepCopy(arr);
        (arr[0] as JObject)!["isPrimary"] = true;
        (copy[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F28_ValueBoolNull_DefaultsToFalse()
    {
        var obj = new JObject { ["sdgId"] = 4 };
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Fallback sets it");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F29_ValueIntNull_SkipItem()
    {
        var obj = new JObject { ["isPrimary"] = false };
        var arr = new JArray { obj, SDGProcessingFixture.CreateSdg(4, false) };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F30_ReturnNewJArray_WhenDedup()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().NotBeSameAs(arr);
        result.Should().BeOfType<JArray>();
    }
}
