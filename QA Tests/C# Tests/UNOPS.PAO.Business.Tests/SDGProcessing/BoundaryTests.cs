/**
 * @fileoverview PNO-1166: SDG deduplication and primary fallback — Boundary tests.
 *
 * Ratio: E ≥ 3×P (30+)
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.SDGProcessing;

/// <summary>
/// Boundary/edge tests: count boundaries, sdgId edges, mixed content, REQ-6.
/// </summary>
[Collection("SDGProcessing")]
public class SDGProcessingBoundaryTests
{
    [Fact]
    [Trait("Category", "Boundary")]
    public void B01_CountZero_ReturnsEmpty()
    {
        var arr = new JArray();
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B02_CountOne_NoDedup_PrimaryFallbackApplies()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B03_CountTwo_SameSdgId_DedupTriggers()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B04_SdgIdZero_Accepted()
    {
        var obj = new JObject { ["sdgId"] = 0, ["isPrimary"] = false };
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B05_SdgId17_Valid()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((17, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(17);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B06_MixedWithAndWithoutSdgId_OnlyWithIdInResult_REQ6()
    {
        var arr = new JArray
        {
            SDGProcessingFixture.CreateSdg(4, false),
            SDGProcessingFixture.CreateSdgWithoutId("X"),
            SDGProcessingFixture.CreateSdg(13, false)
        };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2, "REQ-6: Items without sdgId skipped");
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(4, 13);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B07_AllWithoutSdgId_EmptyResult_REQ6()
    {
        var arr = new JArray
        {
            SDGProcessingFixture.CreateSdgWithoutId("A"),
            SDGProcessingFixture.CreateSdgWithoutId("B")
        };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().BeEmpty("REQ-6: All without sdgId skipped in dedup");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B08_FirstWithoutSdgId_SecondWithId_PrimaryOnSecond()
    {
        var arr = new JArray
        {
            SDGProcessingFixture.CreateSdgWithoutId("A"),
            SDGProcessingFixture.CreateSdg(4, false)
        };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
        // Spec: Primary fallback sets objectsArray[0]; that item has no sdgId so is skipped in dedup.
        // The remaining item (sdgId=4) never receives primary.
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B09_SdgIdNegative_SkippedOrAccepted()
    {
        var obj = new JObject { ["sdgId"] = -1, ["isPrimary"] = false };
        var arr = new JArray { obj, SDGProcessingFixture.CreateSdg(4, false) };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().Contain(-1);
        ids.Should().Contain(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B10_CountExactlyTwo_DifferentIds_NoDedup()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B11_CountExactlyTwo_SameId_DedupToOne()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, true));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B12_ThreeItems_AllSameSdgId_OneResult()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false), (4, true));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B13_Order_FirstOccurrencePreserved()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((13, false), (4, false), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(13, 4, 1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B14_DependentSdGs_ExactMatch()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B15_DeepCopy_ModifiesCopyNotOriginal()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var copy = SDGProcessingSpec.DeepCopy(arr);
        copy[0]!["isPrimary"] = true;
        (arr[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse("Original unchanged");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B16_PrimaryFallback_ModifiesInPlace()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var firstRef = arr[0];
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result[0].Should().BeSameAs(firstRef);
        (arr[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Array mutated in place");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B17_Dedup_ReturnsNewArray()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().NotBeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B18_NoDedup_ReturnsSameArray()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B19_FiveUniqueSdgs_AllPreserved()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((1, false), (4, true), (8, false), (13, false), (17, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(5);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B20_FiveWithTwoDuplicates_ThreeResult()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((1, false), (4, false), (4, true), (13, false), (17, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B21_LastItemPrimary_PreferredOverFirst()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false), (4, true));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var kept = result[0] as JObject;
        kept!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B22_FirstItemPrimary_Kept()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, true), (4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var kept = result[0] as JObject;
        kept!["isPrimary"]!.Value<bool>().Should().BeTrue();
        kept!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B23_SdgIdMaxInt_Accepted()
    {
        var obj = new JObject { ["sdgId"] = int.MaxValue, ["isPrimary"] = false };
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B24_ExtraProperties_Preserved()
    {
        var obj = SDGProcessingFixture.CreateSdg(4, false);
        obj["name"] = "Quality Education";
        obj["description"] = "Ensure inclusive education";
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var kept = result[0] as JObject;
        kept!["name"]!.ToString().Should().Be("Quality Education");
        kept["description"]!.ToString().Should().Be("Ensure inclusive education");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B25_EmptyDependent_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B26_DependentSdGsWithLeadingSpace_NoMatch()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing(" sdGs", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B27_DependentSdGsWithTrailingSpace_NoMatch()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs ", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B28_MultipleDuplicatesDifferentIds_OrderPreserved()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, true), (13, false), (13, true), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(4, 13, 1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B29_AllSeventeenSdgs_NoDedup()
    {
        var arr = new JArray();
        for (var i = 1; i <= 17; i++)
            arr.Add(SDGProcessingFixture.CreateSdg(i, i == 4));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(17);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B30_DeepCopyEmpty_ReturnsEmpty()
    {
        var arr = new JArray();
        var copy = SDGProcessingSpec.DeepCopy(arr);
        copy.Should().NotBeNull();
        copy.Should().BeEmpty();
    }
}
