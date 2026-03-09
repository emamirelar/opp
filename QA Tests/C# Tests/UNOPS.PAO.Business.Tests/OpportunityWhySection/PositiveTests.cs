using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Positive tests for WHY - Impact &amp; Strategic Alignment (PNO-692, PNO-817, PNO-886)
///
/// Requirements validated:
/// - AC1: Section exists
/// - AC2: Context and challenge(s) field
/// - AC4: SDG Alignment (Main + cross-cutting)
/// - AC5: Optional targets/indicators
/// - PNO-886: Expected Impact, Expected Outcomes
/// </summary>
public class PositiveTests
{
    #region AC1 - Section Exists

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_001_SectionId_WhySection_MatchesSpec()
    {
        OpportunityWhySectionSpec.SectionId.Should().Be("why");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_002_WhySectionRequest_CanBeInstantiated()
    {
        var request = new WhySectionRequest();
        request.Should().NotBeNull();
    }

    #endregion

    #region AC2 - Context and Challenge(s)

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_003_WhySectionRequest_Challenges_AcceptsValidText()
    {
        var request = new WhySectionRequest { Challenges = "Water scarcity and climate variability affect agricultural productivity." };
        request.Challenges.Should().NotBeNullOrEmpty();
        request.Challenges.Should().Contain("Water scarcity");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_004_WhySectionRequest_Challenges_WithinMaxLength()
    {
        var text = new string('A', OpportunityWhySectionSpec.ChallengesMaxLength);
        var request = new WhySectionRequest { Challenges = text };
        request.Challenges!.Length.Should().BeLessThanOrEqualTo(OpportunityWhySectionSpec.ChallengesMaxLength);
    }

    #endregion

    #region AC4/AC5 - SDG Alignment

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_005_OpportunitySDGRequest_IsPrimaryTrue_RepresentsMainSDG()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true };
        request.IsPrimary.Should().BeTrue();
        request.SDGId.Should().Be(6);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_006_OpportunitySDGRequest_IsPrimaryFalse_RepresentsCrossCuttingSDG()
    {
        var request = new OpportunitySDGRequest { SDGId = 13, IsPrimary = false };
        request.IsPrimary.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_007_OpportunitySDGRequest_SkipTargetsAndIndicators_OptionalPerPNO817()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, SkipTargetsAndIndicators = true };
        request.SkipTargetsAndIndicators.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_008_WhySectionRequest_SdGs_AcceptsMainAndCrossCutting()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 6, IsPrimary = true },
                new() { SDGId = 13, IsPrimary = false },
                new() { SDGId = 8, IsPrimary = false }
            }
        };
        request.SdGs.Should().HaveCount(3);
        request.SdGs!.Count(s => s.IsPrimary).Should().Be(1);
        request.SdGs!.Count(s => !s.IsPrimary).Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_009_Sdg8_MustBePresentInDropdown_PNO817()
    {
        OpportunityWhySectionSpec.Sdg8Id.Should().Be("8");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_010_Sdg153_TargetYear_Is2030Not2020_PNO817()
    {
        OpportunityWhySectionSpec.Sdg153TargetYear.Should().Be(2030);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_011_SdgClassification_Main_NotPrimary_PNO817()
    {
        OpportunityWhySectionSpec.SdgClassificationMain.Should().Be("Main");
        OpportunityWhySectionSpec.SdgClassificationMain.Should().NotBe("Primary");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_012_SdgClassification_CrossCutting_NotSecondary_PNO817()
    {
        OpportunityWhySectionSpec.SdgClassificationCrossCutting.Should().Be("Cross-cutting");
        OpportunityWhySectionSpec.SdgClassificationCrossCutting.Should().NotBe("Secondary");
    }

    #endregion

    #region PNO-886 - Expected Impact, Expected Outcomes

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_013_WhySectionRequest_ExpectedImpact_AcceptsValidText()
    {
        var request = new WhySectionRequest { ExpectedImpact = "Impact implies changes in people's lives - briefly state expected long-term positive impact." };
        request.ExpectedImpact.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_014_WhySectionRequest_ExpectedOutcomes_AcceptsValidText()
    {
        var request = new WhySectionRequest { ExpectedOutcomes = "Improved water access for 50,000 households." };
        request.ExpectedOutcomes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_015_ExpectedImpact_MaxLength_510_PNO886()
    {
        var text = new string('X', OpportunityWhySectionSpec.ExpectedImpactMaxLength);
        var request = new WhySectionRequest { ExpectedImpact = text };
        request.ExpectedImpact!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedImpactMaxLength);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_016_ExpectedOutcomes_MaxLength_510()
    {
        var text = new string('Y', OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
        var request = new WhySectionRequest { ExpectedOutcomes = text };
        request.ExpectedOutcomes!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_017_ExpectedBeneficiaries_MaxLength_1000_PNO886()
    {
        var text = new string('Z', OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength);
        var request = new WhySectionRequest { ExpectedBeneficiaries = text };
        request.ExpectedBeneficiaries!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength);
    }

    #endregion

    #region UNSDCF, UNOPS Missions

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_018_WhySectionRequest_UncfOutcomes_AcceptsList()
    {
        var request = new WhySectionRequest
        {
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>
            {
                new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 }
            }
        };
        request.UncfOutcomes.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_019_WhySectionRequest_UNOPSMissions_AcceptsList()
    {
        var request = new WhySectionRequest
        {
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest>
            {
                new() { UNOPSMissionId = 1 }
            }
        };
        request.UNOPSMissions.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_020_WhySectionRequest_UNOPSMissionsNotApplicable_AcceptsTrue()
    {
        var request = new WhySectionRequest { UNOPSMissionsNotApplicable = true };
        request.UNOPSMissionsNotApplicable.Should().BeTrue();
    }

    #endregion

    #region Beneficiaries

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_021_WhySectionRequest_EstimatedDirectBeneficiaries_AcceptsPositive()
    {
        var request = new WhySectionRequest { EstimatedDirectBeneficiaries = 50000 };
        request.EstimatedDirectBeneficiaries.Should().Be(50000);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_022_WhySectionRequest_EstimatedIndirectBeneficiaries_AcceptsPositive()
    {
        var request = new WhySectionRequest { EstimatedIndirectBeneficiaries = 100000 };
        request.EstimatedIndirectBeneficiaries.Should().Be(100000);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_023_WhySectionRequest_BeneficiariesToBeDetermined_AcceptsTrue()
    {
        var request = new WhySectionRequest { BeneficiariesToBeDetermined = true };
        request.BeneficiariesToBeDetermined.Should().BeTrue();
    }

    #endregion

    #region Full Request

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_024_WhySectionRequest_FullValidRequest_AllFieldsPopulated()
    {
        var request = new WhySectionRequest
        {
            Challenges = "Context and challenges text",
            ExpectedImpact = "Expected impact text",
            ExpectedOutcomes = "Expected outcomes text",
            ExpectedBeneficiaries = "Rural communities",
            EstimatedDirectBeneficiaries = 10000,
            BeneficiariesToBeDetermined = false,
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } },
            ResultsFocus = "Water and sanitation"
        };
        request.Challenges.Should().NotBeNullOrEmpty();
        request.ExpectedImpact.Should().NotBeNullOrEmpty();
        request.SdGs.Should().HaveCount(1);
        request.SdGs![0].IsPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_025_OpportunitySDGRequest_WithTargets_AcceptsOptionalTargets()
    {
        var request = new OpportunitySDGRequest
        {
            SDGId = 6,
            IsPrimary = true,
            Targets = new List<OpportunitySDGTargetRequest> { new() { SDGTargetDatabaseId = 1 } }
        };
        request.Targets.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_026_OpportunitySDGRequest_Notes_AcceptsOptionalNotes()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, Notes = "Alignment with national water strategy" };
        request.Notes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_027_WhySectionRequest_ResultsFocus_AcceptsText()
    {
        var request = new WhySectionRequest { ResultsFocus = "Sustainable development and climate resilience" };
        request.ResultsFocus.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_028_WhySectionRequest_EmptySdGs_ValidForPartialUpdate()
    {
        var request = new WhySectionRequest { Challenges = "Test", SdGs = new List<OpportunitySDGRequest>() };
        request.SdGs.Should().BeEmpty();
        request.Challenges.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_029_WhySectionRequest_NullOptionalFields_Valid()
    {
        var request = new WhySectionRequest { Challenges = "Required" };
        request.ExpectedImpact.Should().BeNull();
        request.ExpectedOutcomes.Should().BeNull();
        request.SdGs.Should().BeNull();
        request.UncfOutcomes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_030_MultipleCrossCuttingSDGs_ValidPerAC4()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 6, IsPrimary = true },
                new() { SDGId = 8, IsPrimary = false },
                new() { SDGId = 13, IsPrimary = false },
                new() { SDGId = 15, IsPrimary = false }
            }
        };
        request.SdGs!.Count(s => s.IsPrimary).Should().Be(1);
        request.SdGs!.Count(s => !s.IsPrimary).Should().Be(3);
    }

    #endregion
}
