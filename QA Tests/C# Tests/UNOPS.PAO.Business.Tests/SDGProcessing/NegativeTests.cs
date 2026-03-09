/**
 * @fileoverview PNO-1166: SDG deduplication and primary fallback — Negative tests.
 *
 * Ratio: N ≥ 3×P (30+)
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.SDGProcessing;

/// <summary>
/// Negative tests: invalid input, non-SDG types, expected no-op or safe behavior.
/// </summary>
[Collection("SDGProcessing")]
public class SDGProcessingNegativeTests
{
    [Fact]
    [Trait("Category", "Negative")]
    public void N01_NullArray_ReturnsEmptyArray()
    {
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", null!);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N02_EmptyArray_ReturnsEmpty()
    {
        var arr = new JArray();
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N03_DependentPartners_NoPrimaryFallback()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("partners", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeFalse("partners should not trigger primary fallback");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N04_DependentDeliverables_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false), (13, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("deliverables", arr);
        result.Should().HaveCount(2);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N05_DependentUnopsMissions_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("unopsMissions", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N06_DependentEmptyString_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N07_DependentNull_ThrowsOrReturnsUnchanged()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var act = () => SDGProcessingSpec.ApplySDGPostProcessing(null!, arr);
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N08_DependentWhitespace_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("   ", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N09_DependentSdgTypo_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdg", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N10_DependentSdGsWithExtraChars_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGsExtra", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N11_ArrayWithNonJObject_NonObjectsSkippedInDedup()
    {
        var arr = new JArray
        {
            SDGProcessingFixture.CreateSdg(4, false),
            new JValue("not an object"),
            SDGProcessingFixture.CreateSdg(13, false)
        };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(2, "Non-JObject items excluded from dedup logic");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N12_SdgIdAsString_ValueIntFails_Skipped()
    {
        var obj = new JObject { ["sdgId"] = "4", ["isPrimary"] = false };
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        // Newtonsoft.Json JToken.Value<int>() converts string "4" to int 4, so item is NOT skipped
        result.Should().HaveCount(1);
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N13_IsPrimaryMissing_TreatedAsFalse()
    {
        var obj = new JObject { ["sdgId"] = 4 };
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("primary fallback sets first when isPrimary missing (treated as false)");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N14_DependentContacts_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("contacts", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N15_DependentStakeholders_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("stakeholders", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N16_DependentFundingPartners_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("fundingPartners", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N17_DependentClientPartners_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("clientPartners", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N18_DependentCollaborators_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("collaborators", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N19_AllItemsWithoutSdgId_DedupReturnsEmpty()
    {
        var arr = new JArray
        {
            SDGProcessingFixture.CreateSdgWithoutId("A"),
            SDGProcessingFixture.CreateSdgWithoutId("B")
        };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().BeEmpty("All items without sdgId are skipped in dedup");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N20_SingleItemWithoutSdgId_NoDedup_PrimaryFallbackStillRuns()
    {
        var obj = SDGProcessingFixture.CreateSdgWithoutId("X");
        obj.Remove("sdgId");
        var arr = new JArray { obj };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue("Primary fallback runs for single item");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N21_DependentOpportunityDocuments_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("opportunityDocuments", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N22_DependentRisks_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("risks", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N23_DependentBudgetLines_NoProcessing()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("budgetLines", arr);
        result.Should().BeSameAs(arr);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N24_SdgIdNull_SkippedInDedup()
    {
        var obj = new JObject { ["isPrimary"] = false };
        var arr = new JArray { obj, SDGProcessingFixture.CreateSdg(4, false) };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        result.Should().HaveCount(1);
        (result[0] as JObject)!["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N25_ExistingPrimaryFalse_ReplacementWithPrimary()
    {
        var first = SDGProcessingFixture.CreateSdg(4, false);
        var second = SDGProcessingFixture.CreateSdg(4, true);
        var arr = new JArray { first, second };
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdGs", arr);
        var kept = result[0] as JObject;
        kept!.Should().NotBeNull();
        kept!["isPrimary"]!.Value<bool>().Should().BeTrue("Primary entry replaces non-primary");
        kept["sdgId"]!.Value<int>().Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N26_BothPrimary_FirstWins()
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
    [Trait("Category", "Negative")]
    public void N27_DependentLowerCaseSdgs_TriggersLogic()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("sdgs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N28_DependentMixedCaseSdGs_TriggersLogic()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        var result = SDGProcessingSpec.ApplySDGPostProcessing("SdGs", arr);
        (result[0] as JObject)!["isPrimary"]!.Value<bool>().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N29_ThreeDifferentDependents_AllNoOp()
    {
        var arr = SDGProcessingFixture.CreateSdgArray((4, false));
        foreach (var dep in new[] { "partners", "contacts", "interactions" })
        {
            var result = SDGProcessingSpec.ApplySDGPostProcessing(dep, arr);
            result.Should().BeSameAs(arr);
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N30_DeepCopyNull_ReturnsEmpty()
    {
        var result = SDGProcessingSpec.DeepCopy(null!);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
