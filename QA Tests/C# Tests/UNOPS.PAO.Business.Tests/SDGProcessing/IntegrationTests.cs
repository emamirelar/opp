/**
 * @fileoverview PNO-1166: SDG deduplication and primary fallback — Integration tests.
 *
 * Ratio: I ≥ 3×P (30+)
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.SDGProcessing;

/// <summary>
/// Integration tests: full flow, realistic AI response simulation, combined scenarios.
/// </summary>
[Collection("SDGProcessing")]
public class SDGProcessingIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void I01_FullFlow_NoPrimaryThenDedup()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(4, 13);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I02_RealisticAIResponse_Goal4AndQualityEducation()
    {
        var goal4 = SDGProcessingFixture.CreateSdg(4, false, "Goal 4");
        var qualityEd = SDGProcessingFixture.CreateSdg(4, false, "Quality Education");
        var arr = new JArray { goal4, qualityEd };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I03_RealisticAIResponse_MultipleGoalsWithPrimary()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, true), (13, false), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(3);
        var primaryCount = result.OfType<JObject>().Count(o => o["isPrimary"]?.Value<bool>() ?? false);
        primaryCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I04_RealisticAIResponse_AllWithoutPrimary()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((1, false), (4, false), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var first = result[0] as JObject;
        first!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I05_MixedScenario_DuplicatesAndUnique()
    {
        var arr = SDGProcessingFixture.CreateSdgArray(
            (4, false), (4, true),
            (13, false),
            (1, false), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(3);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(4, 13, 1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I06_SequentialCalls_Independent()
    {
        var arr1 = SDGProcessingFixture.CreateSdgArray((4, false));
        var arr2 = SDGProcessingFixture.CreateSdgArray((13, false));
        var r1 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr1);
        var r2 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr2);
        (r1[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
        (r2[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(13);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I07_SameArrayTwice_SecondCallIdempotentForDedup()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var r1 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var r2 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", r1);
        r2.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I08_NonSdgThenSdg_SeparateCalls()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var r1 = SDGProcessingSpec.ApplySDGPostProcessing("partners", arr);
        r1.Should().BeSameAs(arr);
        var r2 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (r2[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I09_DeepCopyThenProcess_OriginalUnchanged()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var copy = SDGProcessingSpec.DeepCopy(arr);
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        copy.Should().HaveCount(2);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I10_AllSeventeenSdgs_OnePrimary()
    {
        var arr = new JArray();
        for (var i = 1; i <= 17; i++)
            arr.Add(SDGProcessingFixture.CreateSdg(i, i == 4));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(17);
        var primaryCount = result.OfType<JObject>().Count(o => o["isPrimary"]?.Value<bool>() ?? false);
        primaryCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I11_AllSeventeenSdgs_NoPrimary_Fallback()
    {
        var arr = new JArray();
        for (var i = 1; i <= 17; i++)
            arr.Add(SDGProcessingFixture.CreateSdg(i, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I12_DuplicateAtStart_PrimaryAtEnd()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false), (4, true));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2);
        var sdg4 = result.OfType<JObject>().First(o => o["sdgId"]?.Value<int>() == 4);
        sdg4["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I13_ThreeDuplicateGroups()
    {
        var arr = SDGProcessingFixture.CreateSdgArray(
            (4, false), (4, true),
            (13, true), (13, false),
            (1, false), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(3);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("sdgId 4: primary preferred");
        (result[1] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("sdgId 13: primary preferred");
        // sdgId 1: both false, first wins, no primary fallback (anyPrimary already true)
        (result[2] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I14_EmptyThenPopulated()
    {
        var empty = new JArray();
        var populated = SDGProcessingFixture.CreateSdgArray((4, false));
        var r1 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", empty);
        var r2 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", populated);
        r1.Should().BeEmpty();
        r2.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I15_JsonRoundTrip_StructurePreserved()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var json = result.ToString();
        var parsed = JArray.Parse(json);
        parsed.Should().HaveCount(2);
        (parsed[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I16_ExtraProperties_PreservedThroughProcessing()
    {
        var obj = SDGProcessingFixture.CreateSdg(4, false, "Quality Education");
        obj["weight"] = 0.8;
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var kept = result[0] as JObject;
        kept!["name"]!.ToString().Should().Be("Quality Education");
        kept["weight"]!.Value<double>().Should().Be(0.8);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I17_DependentCaseVariations_AllTriggerForSdGs()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        foreach (var dep in new[] { "sdGs", "SDGS", "SdGs", "sdgs" })
        {
            var copy = SDGProcessingSpec.DeepCopy(arr);
            var result = SDGProcessingSpec.ApplySDGPostProcessing(dep, copy);
            (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue($"dependent='{dep}'");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I18_RealisticOpportunitySdgs()
    {
        var arr = SDGProcessingFixture.CreateSdgArray(
            (4, true),
            (13, false),
            (8, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(3);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(4, 13, 8);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I19_RealisticWithDuplicates_ClimateAndGoal13()
    {
        var goal13 = SDGProcessingFixture.CreateSdg(13, false, "Goal 13");
        var climate = SDGProcessingFixture.CreateSdg(13, true, "Climate Action");
        var arr = new JArray { goal13, climate };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["name"]!.ToString().Should().Be("Climate Action");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I20_ComplexScenario_FiveSdgsThreeDuplicates()
    {
        var arr = SDGProcessingFixture.CreateSdgArray(
            (1, false), (4, false), (4, true), (13, false), (17, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(4);
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(1, 4, 13, 17);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I21_OrderStability_MultipleRuns()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((3, false), (7, false), (11, false));
        var r1 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", SDGProcessingSpec.DeepCopy(arr));
        var r2 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", SDGProcessingSpec.DeepCopy(arr));
        var ids1 = r1.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        var ids2 = r2.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids1.Should().Equal(ids2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I22_SingleSdgWithName()
    {
        var obj = SDGProcessingFixture.CreateSdg(4, false, "Quality Education");
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["name"]!.ToString().Should().Be("Quality Education");
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I23_NonSdgDependents_NoModification()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false));
        var copy = SDGProcessingSpec.DeepCopy(arr);
        foreach (var dep in new[] { "partners", "contacts", "deliverables", "unopsMissions" })
        {
            var result = SDGProcessingSpec.ApplySDGPostProcessing(dep, SDGProcessingSpec.DeepCopy(arr));
            result.Should().BeEquivalentTo(copy);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I24_DedupThenPrimaryFallback_OrderOfOperations()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I25_ResultEnumerable_OrderSelect()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((5, false), (10, false), (15, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var list = result.OfType<JObject>().ToList();
        list.Should().HaveCount(3);
        list[0]["sdgId"]!.Value<int>().Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I26_NullArray_ConsistentWithEmpty()
    {
        var r1 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", null!);
        var r2 = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", new JArray());
        r1.Should().BeEmpty();
        r2.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I27_StandardSdgIds_AllProcessed()
    {
        var arr = new JArray();
        foreach (var id in SDGProcessingFixture.StandardSdgIds)
            arr.Add(SDGProcessingFixture.CreateSdg(id, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(17);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I28_TwoIdenticalSdgs_OneResult()
    {
        var a = SDGProcessingFixture.CreateSdg(4, false);
        var b = SDGProcessingFixture.CreateSdg(4, false);
        var arr = new JArray { a, b };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I29_PrimaryFallback_OnlyFirstModified()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false), (1, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
        (result[1] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse();
        (result[2] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I30_FullSpecCompliance_AllRequirements()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (4, true), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("REQ-4: primary preferred");
        var ids = result.OfType<JObject>().Select(o => o["sdgId"]?.Value<int>()).ToList();
        ids.Should().ContainInOrder(new int?[] { 4, 13 }, "REQ-5: order preserved");
    }
}
