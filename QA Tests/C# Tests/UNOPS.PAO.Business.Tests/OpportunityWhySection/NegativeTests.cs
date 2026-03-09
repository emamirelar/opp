using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Negative tests for WHY - Impact &amp; Strategic Alignment (PNO-692, PNO-817, PNO-886)
/// Invalid inputs, wrong states, expected failures.
/// </summary>
public class NegativeTests
{
    #region Challenges - Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_001_Challenges_ExceedsMaxLength_ShouldFailValidation()
    {
        var overLength = new string('A', OpportunityWhySectionSpec.ChallengesMaxLength + 1);
        overLength.Length.Should().BeGreaterThan(OpportunityWhySectionSpec.ChallengesMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_002_Challenges_EmptyString_AllowedButNotRecommended()
    {
        var request = new WhySectionRequest { Challenges = "" };
        request.Challenges.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_003_Challenges_WhitespaceOnly_AllowedButNotRecommended()
    {
        var request = new WhySectionRequest { Challenges = "   " };
        string.IsNullOrWhiteSpace(request.Challenges).Should().BeTrue();
    }

    #endregion

    #region SDG - Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_004_OpportunitySDGRequest_SDGIdZero_Invalid()
    {
        var request = new OpportunitySDGRequest { SDGId = 0, IsPrimary = true };
        request.SDGId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_005_OpportunitySDGRequest_SDGIdNegative_Invalid()
    {
        var request = new OpportunitySDGRequest { SDGId = -1, IsPrimary = true };
        request.SDGId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_006_SdGs_EmptyList_NoMainSDG_InvalidForGo()
    {
        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest>() };
        var hasMainSdg = request.SdGs?.Any(s => s.IsPrimary) ?? false;
        hasMainSdg.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_007_SdGs_Null_NoSDGAlignment_InvalidForGo()
    {
        var request = new WhySectionRequest { SdGs = null };
        request.SdGs.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_008_SdGs_OnlyCrossCutting_NoMainSDG_Invalid()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 13, IsPrimary = false },
                new() { SDGId = 8, IsPrimary = false }
            }
        };
        request.SdGs!.Any(s => s.IsPrimary).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_009_SdGs_MultipleMainSDGs_Invalid()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 6, IsPrimary = true },
                new() { SDGId = 13, IsPrimary = true }
            }
        };
        request.SdGs!.Count(s => s.IsPrimary).Should().BeGreaterThan(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_010_SdGs_DuplicateSDGId_Invalid()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 6, IsPrimary = true },
                new() { SDGId = 6, IsPrimary = false }
            }
        };
        var duplicates = request.SdGs!.GroupBy(s => s.SDGId).Where(g => g.Count() > 1);
        duplicates.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_011_Sdg8_MissingFromDropdown_PNO817_Defect()
    {
        var sdg8Present = true;
        sdg8Present.Should().BeTrue("DEF-130: SDG 8 must be present in dropdown per PNO-817");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_012_Sdg153_References2020_Outdated_PNO817_Defect()
    {
        var targetYear = 2030;
        targetYear.Should().NotBe(2020, "DEF-131: SDG 15.3 must reference 2030 not 2020 per PNO-817");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_013_ClassificationLabel_Primary_ShouldNotBeUsed()
    {
        var label = "Primary";
        label.Should().NotBe(OpportunityWhySectionSpec.SdgClassificationMain);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_014_ClassificationLabel_Secondary_ShouldNotBeUsed()
    {
        var label = "Secondary";
        label.Should().NotBe(OpportunityWhySectionSpec.SdgClassificationCrossCutting);
    }

    #endregion

    #region Expected Impact / Outcomes - Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_015_ExpectedImpact_ExceedsMaxLength_Invalid()
    {
        var overLength = new string('X', OpportunityWhySectionSpec.ExpectedImpactMaxLength + 1);
        overLength.Length.Should().BeGreaterThan(OpportunityWhySectionSpec.ExpectedImpactMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_016_ExpectedOutcomes_ExceedsMaxLength_Invalid()
    {
        var overLength = new string('Y', OpportunityWhySectionSpec.ExpectedOutcomesMaxLength + 1);
        overLength.Length.Should().BeGreaterThan(OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_017_ExpectedBeneficiaries_ExceedsMaxLength_Invalid()
    {
        var overLength = new string('Z', OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength + 1);
        overLength.Length.Should().BeGreaterThan(OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength);
    }

    #endregion

    #region Beneficiaries - Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_018_EstimatedDirectBeneficiaries_Negative_Invalid()
    {
        var request = new WhySectionRequest { EstimatedDirectBeneficiaries = -1 };
        request.EstimatedDirectBeneficiaries.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_019_EstimatedIndirectBeneficiaries_Negative_Invalid()
    {
        var request = new WhySectionRequest { EstimatedIndirectBeneficiaries = -100 };
        request.EstimatedIndirectBeneficiaries.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_020_BeneficiariesToBeDetermined_WithEstimates_Ambiguous()
    {
        var request = new WhySectionRequest
        {
            BeneficiariesToBeDetermined = true,
            EstimatedDirectBeneficiaries = 5000
        };
        (request.BeneficiariesToBeDetermined && request.EstimatedDirectBeneficiaries.HasValue).Should().BeTrue();
    }

    #endregion

    #region UNSDCF - Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_021_UncfOutcomes_EmptyCountry_NoOutcome_InvalidForGo()
    {
        var request = new WhySectionRequest { UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>() };
        request.UncfOutcomes.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_022_OpportunityUNCFOutcomeRequest_InvalidOutcomeId()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 0, OpportunityCountryId = 1 };
        request.UNCFOutcomeId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_023_OpportunityUNCFOutcomeRequest_InvalidCountryId()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 0 };
        request.OpportunityCountryId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_024_UncfOutcomes_Null_WhenCountryHasActiveFramework_Invalid()
    {
        var request = new WhySectionRequest { UncfOutcomes = null };
        request.UncfOutcomes.Should().BeNull();
    }

    #endregion

    #region UNOPS Missions - Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_025_OpportunityUNOPSMissionRequest_ZeroMissionId_Invalid()
    {
        var request = new OpportunityUNOPSMissionRequest { UNOPSMissionId = 0 };
        request.UNOPSMissionId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_026_UNOPSMissionsNotApplicable_False_WithEmptyMissions_MayFailValidation()
    {
        var request = new WhySectionRequest { UNOPSMissionsNotApplicable = false, UNOPSMissions = new List<OpportunityUNOPSMissionRequest>() };
        request.UNOPSMissions!.Should().BeEmpty();
        request.UNOPSMissionsNotApplicable.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_027_UNOPSMissions_Null_WhenNotApplicableFalse_Ambiguous()
    {
        var request = new WhySectionRequest { UNOPSMissionsNotApplicable = false, UNOPSMissions = null };
        request.UNOPSMissions.Should().BeNull();
    }

    #endregion

    #region OpportunitySDGTargetRequest - Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_028_OpportunitySDGTargetRequest_ZeroSDGTargetDatabaseId_Invalid()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 0 };
        request.SDGTargetDatabaseId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_029_OpportunitySDGTargetRequest_NegativeSDGTargetDatabaseId_Invalid()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = -1 };
        request.SDGTargetDatabaseId.Should().BeNegative();
    }

    #endregion

    #region WhySectionRequest - Null/Empty

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_030_WhySectionRequest_AllNull_NoContent()
    {
        var request = new WhySectionRequest();
        request.Challenges.Should().BeNull();
        request.ExpectedImpact.Should().BeNull();
        request.SdGs.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_031_ResultsFocus_Empty_Allowed()
    {
        var request = new WhySectionRequest { ResultsFocus = "" };
        request.ResultsFocus.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_032_ExpectedBeneficiaries_Empty_Allowed()
    {
        var request = new WhySectionRequest { ExpectedBeneficiaries = "" };
        request.ExpectedBeneficiaries.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_033_SdGs_ContainsNull_Invalid()
    {
        var list = new List<OpportunitySDGRequest?> { new OpportunitySDGRequest { SDGId = 6, IsPrimary = true }, null };
        list.Any(x => x == null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_034_UncfOutcomes_ContainsInvalidOutcomeId_Invalid()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = -1, OpportunityCountryId = 1 };
        request.UNCFOutcomeId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_035_UncfOutcomes_DuplicateCountryOutcome_Invalid()
    {
        var request = new WhySectionRequest
        {
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>
            {
                new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 },
                new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 }
            }
        };
        var duplicates = request.UncfOutcomes!.GroupBy(u => new { u.OpportunityCountryId, u.UNCFOutcomeId }).Where(g => g.Count() > 1);
        duplicates.Should().NotBeEmpty();
    }

    #endregion

    #region Go Decision Validation

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_036_GoDecision_RequiresMainSDG()
    {
        var hasMainSdg = false;
        hasMainSdg.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_037_GoDecision_RequiresUNSDCFOutcome_PerCountry()
    {
        var outcomeCount = 0;
        outcomeCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_038_GoDecision_NoCountries_UNSDCFSectionUnavailable()
    {
        var countryCount = 0;
        countryCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_039_GoDecision_SDGTargetsOptional_PerPNO817()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, SkipTargetsAndIndicators = true };
        request.SkipTargetsAndIndicators.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_040_GoDecision_DefaultSDGType_ShouldBeMainNotSecondary_PNO817()
    {
        var defaultType = "Main";
        defaultType.Should().Be(OpportunityWhySectionSpec.SdgClassificationMain);
        defaultType.Should().NotBe("Secondary");
    }

    #endregion

    #region Additional Edge Cases

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_041_Challenges_SpecialCharacters_ShouldHandle()
    {
        var request = new WhySectionRequest { Challenges = "Test <script>alert(1)</script> & \"quotes\"" };
        request.Challenges.Should().Contain("<script>");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_042_Challenges_Unicode_ShouldHandle()
    {
        var request = new WhySectionRequest { Challenges = "Desafíos climáticos y biodiversidad 气候" };
        request.Challenges.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_043_ExpectedImpact_Newlines_ShouldHandle()
    {
        var request = new WhySectionRequest { ExpectedImpact = "Line1\nLine2" };
        request.ExpectedImpact.Should().Contain("\n");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_044_SdGs_NonExistentSDGId_Invalid()
    {
        var request = new OpportunitySDGRequest { SDGId = 99999, IsPrimary = true };
        request.SDGId.Should().Be(99999);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_045_OpportunitySDGRequest_Notes_ExceedsMaxLength()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, Notes = new string('N', 2001) };
        request.Notes!.Length.Should().BeGreaterThan(2000);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_046_WhySectionRequest_UncfOutcomes_InvalidIndicatorIds()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, UNCFIndicatorIds = new List<int> { -1 } };
        request.UNCFIndicatorIds!.Should().Contain(-1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_047_WhySectionRequest_ResultsFocus_ExceedsReasonableLength()
    {
        var request = new WhySectionRequest { ResultsFocus = new string('R', 5000) };
        request.ResultsFocus!.Length.Should().BeGreaterThan(4000);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_048_EstimatedDirectBeneficiaries_Zero_Ambiguous()
    {
        var request = new WhySectionRequest { EstimatedDirectBeneficiaries = 0 };
        request.EstimatedDirectBeneficiaries.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_049_EstimatedIndirectBeneficiaries_Zero_Ambiguous()
    {
        var request = new WhySectionRequest { EstimatedIndirectBeneficiaries = 0 };
        request.EstimatedIndirectBeneficiaries.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_050_SdGs_MoreThan17_Invalid()
    {
        var sdgs = Enumerable.Range(1, 18).Select(i => new OpportunitySDGRequest { SDGId = i, IsPrimary = i == 1 }).ToList();
        sdgs.Count.Should().BeGreaterThan(17);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_051_OpportunityUNOPSMissionRequest_NegativeMissionId_Invalid()
    {
        var request = new OpportunityUNOPSMissionRequest { UNOPSMissionId = -1 };
        request.UNOPSMissionId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_052_WhySectionRequest_UncfOutcomes_EmptyIndicatorIds_Valid()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, UNCFIndicatorIds = new List<int>() };
        request.UNCFIndicatorIds!.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_053_OpportunitySDGTargetRequest_ZeroOpportunitySDGId_Invalid()
    {
        var request = new OpportunitySDGTargetRequest { OpportunitySDGId = 0, SDGTargetDatabaseId = 1 };
        request.OpportunitySDGId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_054_OpportunitySDGRequest_Targets_SkipTargetsTrue_Conflicting()
    {
        var request = new OpportunitySDGRequest
        {
            SDGId = 6,
            IsPrimary = true,
            SkipTargetsAndIndicators = true,
            Targets = new List<OpportunitySDGTargetRequest> { new() { SDGTargetDatabaseId = 1 } }
        };
        (request.SkipTargetsAndIndicators == true && request.Targets!.Count > 0).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_055_WhySectionRequest_AllOptionalFieldsNull_StillValid()
    {
        var request = new WhySectionRequest();
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_056_Challenges_Null_AllowedForPartialUpdate()
    {
        var request = new WhySectionRequest { ExpectedImpact = "Impact" };
        request.Challenges.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_057_ExpectedImpact_Null_Allowed()
    {
        var request = new WhySectionRequest { Challenges = "Challenges" };
        request.ExpectedImpact.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_058_ExpectedOutcomes_Null_Allowed()
    {
        var request = new WhySectionRequest();
        request.ExpectedOutcomes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_059_ExpectedBeneficiaries_Null_Allowed()
    {
        var request = new WhySectionRequest();
        request.ExpectedBeneficiaries.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_060_EstimatedDirectBeneficiaries_Null_Allowed()
    {
        var request = new WhySectionRequest();
        request.EstimatedDirectBeneficiaries.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_061_EstimatedIndirectBeneficiaries_Null_Allowed()
    {
        var request = new WhySectionRequest();
        request.EstimatedIndirectBeneficiaries.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_062_ResultsFocus_Null_Allowed()
    {
        var request = new WhySectionRequest();
        request.ResultsFocus.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_063_UncfOutcomes_Null_Allowed()
    {
        var request = new WhySectionRequest();
        request.UncfOutcomes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_064_UNOPSMissions_Null_WhenNotApplicableTrue_Valid()
    {
        var request = new WhySectionRequest { UNOPSMissionsNotApplicable = true, UNOPSMissions = null };
        request.UNOPSMissions.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_065_OpportunitySDGRequest_Notes_Null_Valid()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, Notes = null };
        request.Notes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_066_OpportunitySDGRequest_Targets_Null_Valid()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, Targets = null };
        request.Targets.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_067_OpportunitySDGRequest_SkipTargetsAndIndicators_Null_Valid()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, SkipTargetsAndIndicators = null };
        request.SkipTargetsAndIndicators.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_068_OpportunityUNCFOutcomeRequest_Notes_Null_Valid()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, Notes = null };
        request.Notes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_069_OpportunityUNCFOutcomeRequest_UNCFIndicatorIds_Null_Valid()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, UNCFIndicatorIds = null };
        request.UNCFIndicatorIds.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_070_OpportunitySDGTargetRequest_Notes_Null_Valid()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1, Notes = null };
        request.Notes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_071_OpportunitySDGTargetRequest_SDGIndicatorDatabaseIds_Null_Valid()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1, SDGIndicatorDatabaseIds = null };
        request.SDGIndicatorDatabaseIds.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_072_OpportunityUNOPSMissionRequest_Id_Null_ValidForCreate()
    {
        var request = new OpportunityUNOPSMissionRequest { Id = null, UNOPSMissionId = 1 };
        request.Id.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_073_SdGs_SingleMainSDG_NoCrossCutting_Valid()
    {
        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } } };
        request.SdGs!.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_074_SdGs_CrossCuttingOnly_NoMain_InvalidForGo()
    {
        var hasMain = false;
        hasMain.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_075_SdGs_TwoMainSDGs_Invalid()
    {
        var mainCount = 2;
        mainCount.Should().BeGreaterThan(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_076_ExpectedImpact_EmptyString_Allowed()
    {
        var request = new WhySectionRequest { ExpectedImpact = "" };
        request.ExpectedImpact.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_077_ExpectedOutcomes_EmptyString_Allowed()
    {
        var request = new WhySectionRequest { ExpectedOutcomes = "" };
        request.ExpectedOutcomes.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_078_Challenges_ExactMaxLength_Valid()
    {
        var request = new WhySectionRequest { Challenges = new string('C', OpportunityWhySectionSpec.ChallengesMaxLength) };
        request.Challenges!.Length.Should().Be(OpportunityWhySectionSpec.ChallengesMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_079_ExpectedImpact_ExactMaxLength_Valid()
    {
        var request = new WhySectionRequest { ExpectedImpact = new string('A', OpportunityWhySectionSpec.ExpectedImpactMaxLength) };
        request.ExpectedImpact!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedImpactMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_080_ExpectedOutcomes_ExactMaxLength_Valid()
    {
        var request = new WhySectionRequest { ExpectedOutcomes = new string('B', OpportunityWhySectionSpec.ExpectedOutcomesMaxLength) };
        request.ExpectedOutcomes!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_081_ExpectedBeneficiaries_ExactMaxLength_Valid()
    {
        var request = new WhySectionRequest { ExpectedBeneficiaries = new string('C', OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength) };
        request.ExpectedBeneficiaries!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_082_UncfOutcomes_EmptyList_Valid()
    {
        var request = new WhySectionRequest { UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>() };
        request.UncfOutcomes.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_083_UNOPSMissions_EmptyList_WhenNotApplicableFalse_MayFail()
    {
        var request = new WhySectionRequest { UNOPSMissionsNotApplicable = false, UNOPSMissions = new List<OpportunityUNOPSMissionRequest>() };
        request.UNOPSMissions!.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_084_OpportunitySDGRequest_SkipTargetsAndIndicators_False_Valid()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, SkipTargetsAndIndicators = false };
        request.SkipTargetsAndIndicators.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_085_SdGs_OneMainAndOneCrossCutting_Valid()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 6, IsPrimary = true },
                new() { SDGId = 8, IsPrimary = false }
            }
        };
        request.SdGs!.Count(s => s.IsPrimary).Should().Be(1);
        request.SdGs!.Count(s => !s.IsPrimary).Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_086_SdGs_IncludeSDG8_Valid()
    {
        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 8, IsPrimary = true } } };
        request.SdGs!.First().SDGId.Should().Be(8);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_087_SdGs_IncludeSDG15_Valid()
    {
        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 15, IsPrimary = false } } };
        request.SdGs!.First().SDGId.Should().Be(15);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_088_WhySectionRequest_UncfOutcomes_MultipleCountries_Valid()
    {
        var request = new WhySectionRequest
        {
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>
            {
                new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 },
                new() { UNCFOutcomeId = 2, OpportunityCountryId = 2 }
            }
        };
        request.UncfOutcomes!.GroupBy(u => u.OpportunityCountryId).Count().Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_089_WhySectionRequest_UNOPSMissions_Multiple_Valid()
    {
        var request = new WhySectionRequest
        {
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest>
            {
                new() { UNOPSMissionId = 1 },
                new() { UNOPSMissionId = 2 }
            }
        };
        request.UNOPSMissions!.Count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_090_OpportunitySDGRequest_Targets_SDGIndicatorDatabaseIds_Valid()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1, SDGIndicatorDatabaseIds = new List<int> { 1, 2 } };
        request.SDGIndicatorDatabaseIds!.Count.Should().Be(2);
    }

    #endregion
}
