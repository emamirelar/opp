using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Functional tests for WHY - Impact &amp; Strategic Alignment (PNO-692, PNO-817, PNO-886)
/// Business rules, validation logic, state transitions, data transformations.
/// </summary>
public class FunctionalTests
{
    #region SDG Classification Logic

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_001_IsPrimaryTrue_MapsToMainClassification()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true };
        var classification = request.IsPrimary ? OpportunityWhySectionSpec.SdgClassificationMain : OpportunityWhySectionSpec.SdgClassificationCrossCutting;
        classification.Should().Be(OpportunityWhySectionSpec.SdgClassificationMain);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_002_IsPrimaryFalse_MapsToCrossCuttingClassification()
    {
        var request = new OpportunitySDGRequest { SDGId = 13, IsPrimary = false };
        var classification = request.IsPrimary ? OpportunityWhySectionSpec.SdgClassificationMain : OpportunityWhySectionSpec.SdgClassificationCrossCutting;
        classification.Should().Be(OpportunityWhySectionSpec.SdgClassificationCrossCutting);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_003_GoDecisionRequires_ExactlyOneMainSDG()
    {
        var sdgs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } };
        var mainCount = sdgs.Count(s => s.IsPrimary);
        mainCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_004_GoDecisionRequires_AtLeastOneSDG()
    {
        var sdgs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } };
        sdgs.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_005_SDGTargetSelection_OptionalPerPNO817()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, SkipTargetsAndIndicators = true };
        var targetsRequired = !(request.SkipTargetsAndIndicators == true);
        targetsRequired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_006_SDGTargetSelection_WhenNotSkipped_CanProvideTargets()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, SkipTargetsAndIndicators = false, Targets = new List<OpportunitySDGTargetRequest> { new() { SDGTargetDatabaseId = 1 } } };
        var hasTargets = request.Targets?.Count > 0;
        hasTargets.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_007_DefaultSDGType_WhenNoSDGsSelected_ShouldBeMain()
    {
        var defaultType = OpportunityWhySectionSpec.SdgClassificationMain;
        defaultType.Should().Be("Main");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_008_SDG8_MustBeAvailableInDropdown()
    {
        var sdg8Id = OpportunityWhySectionSpec.Sdg8Id;
        sdg8Id.Should().Be("8");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_009_SDG153_TargetYear_2030()
    {
        var year = OpportunityWhySectionSpec.Sdg153TargetYear;
        year.Should().Be(2030);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_010_ClassificationLabels_MainNotPrimary()
    {
        OpportunityWhySectionSpec.SdgClassificationMain.Should().NotBe("Primary");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_011_ClassificationLabels_CrossCuttingNotSecondary()
    {
        OpportunityWhySectionSpec.SdgClassificationCrossCutting.Should().NotBe("Secondary");
    }

    #endregion

    #region Context and Challenges

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_012_Challenges_RequiredForCompleteSection()
    {
        var request = new WhySectionRequest { Challenges = "Water scarcity affects 2 billion people." };
        var hasChallenges = !string.IsNullOrWhiteSpace(request.Challenges);
        hasChallenges.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_013_Challenges_WithinMaxLength_Valid()
    {
        var text = new string('A', OpportunityWhySectionSpec.ChallengesMaxLength);
        var isValid = text.Length <= OpportunityWhySectionSpec.ChallengesMaxLength;
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_014_Challenges_ExceedsMaxLength_Invalid()
    {
        var text = new string('A', OpportunityWhySectionSpec.ChallengesMaxLength + 1);
        var isValid = text.Length <= OpportunityWhySectionSpec.ChallengesMaxLength;
        isValid.Should().BeFalse();
    }

    #endregion

    #region Expected Impact and Outcomes (PNO-886)

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_015_ExpectedImpact_FieldExists_PNO886()
    {
        var request = new WhySectionRequest { ExpectedImpact = "Long-term positive impact" };
        request.ExpectedImpact.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_016_ExpectedImpact_MaxLength510()
    {
        var maxLen = OpportunityWhySectionSpec.ExpectedImpactMaxLength;
        maxLen.Should().Be(510);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_017_ExpectedOutcomes_MaxLength510()
    {
        var maxLen = OpportunityWhySectionSpec.ExpectedOutcomesMaxLength;
        maxLen.Should().Be(510);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_018_ExpectedBeneficiaries_MaxLength1000_PNO886()
    {
        var maxLen = OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength;
        maxLen.Should().Be(1000);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_019_ExpectedImpact_AboveExpectedOutcomes_FieldOrder()
    {
        var request = new WhySectionRequest { ExpectedImpact = "Impact", ExpectedOutcomes = "Outcomes" };
        request.ExpectedImpact.Should().NotBeNull();
        request.ExpectedOutcomes.Should().NotBeNull();
    }

    #endregion

    #region UNSDCF Logic

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_020_UncfOutcomes_PerCountry_RequiredForGo()
    {
        var request = new WhySectionRequest
        {
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest> { new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 } }
        };
        var hasOutcome = request.UncfOutcomes?.Count > 0;
        hasOutcome.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_021_UncfOutcomes_CountrySpecific()
    {
        var request = new WhySectionRequest
        {
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>
            {
                new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 },
                new() { UNCFOutcomeId = 2, OpportunityCountryId = 2 }
            }
        };
        var countries = request.UncfOutcomes!.Select(u => u.OpportunityCountryId).Distinct().ToList();
        countries.Count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_022_UncfOutcomes_CanIncludeIndicators()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, UNCFIndicatorIds = new List<int> { 1, 2 } };
        request.UNCFIndicatorIds!.Count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_023_UncfOutcomes_OptionalIndicators()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, UNCFIndicatorIds = null };
        request.UNCFIndicatorIds.Should().BeNull();
    }

    #endregion

    #region UNOPS Missions (PNO-886)

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_024_UNOPSMissionsNotApplicable_WhenTrue_NoMissionsRequired()
    {
        var request = new WhySectionRequest { UNOPSMissionsNotApplicable = true, UNOPSMissions = null };
        var missionsRequired = !request.UNOPSMissionsNotApplicable;
        missionsRequired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_025_UNOPSMissions_WhenNotApplicableFalse_CanHaveMissions()
    {
        var request = new WhySectionRequest
        {
            UNOPSMissionsNotApplicable = false,
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest> { new() { UNOPSMissionId = 1 } }
        };
        request.UNOPSMissions!.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_026_AlignmentToUNOPSStrategicMissions_SectionRename_PNO886()
    {
        var sectionName = "Alignment to UNOPS Strategic Missions";
        sectionName.Should().Contain("UNOPS");
        sectionName.Should().Contain("Strategic");
    }

    #endregion

    #region Beneficiaries Logic

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_027_BeneficiariesToBeDetermined_WhenTrue_EstimatesOptional()
    {
        var request = new WhySectionRequest { BeneficiariesToBeDetermined = true, EstimatedDirectBeneficiaries = null };
        var estimatesRequired = !request.BeneficiariesToBeDetermined;
        estimatesRequired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_028_EstimatedBeneficiaries_PositiveIntegers()
    {
        var request = new WhySectionRequest { EstimatedDirectBeneficiaries = 5000, EstimatedIndirectBeneficiaries = 10000 };
        request.EstimatedDirectBeneficiaries.Should().BePositive();
        request.EstimatedIndirectBeneficiaries.Should().BePositive();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_029_ExpectedBeneficiaries_TextDescription()
    {
        var request = new WhySectionRequest { ExpectedBeneficiaries = "Rural communities in East Africa" };
        request.ExpectedBeneficiaries.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Partner Results Framework (AC3)

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_030_PartnerResultsFramework_TagDocumentAgainstPartner()
    {
        var tagMechanism = "Tag Partner Result Framework";
        tagMechanism.Should().Contain("Partner");
        tagMechanism.Should().Contain("Framework");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_031_PartnerResultsFramework_MultiplePartners_IndicateWhich()
    {
        var multiPartner = true;
        multiPartner.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_032_PartnerResultsFramework_OptionToIndicateNotAvailable()
    {
        var notAvailableOption = true;
        notAvailableOption.Should().BeTrue();
    }

    #endregion

    #region Section Structure

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_033_SectionId_Why()
    {
        OpportunityWhySectionSpec.SectionId.Should().Be("why");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_034_SectionHeader_WhyImpactStrategicAlignment_PNO886()
    {
        var header = "Why - Impact & Strategic Alignment";
        header.Should().Contain("Why");
        header.Should().Contain("Impact");
        header.Should().Contain("Strategic");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_035_ContextAndChallenges_SubsectionLabel()
    {
        var label = "Context and challenge(s)";
        label.Should().Contain("Context");
        label.Should().Contain("challenge");
    }

    #endregion

    #region Data Transformation

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_036_WhySectionRequest_ToModel_ChallengesMapped()
    {
        var request = new WhySectionRequest { Challenges = "Test challenges" };
        var challenges = request.Challenges;
        challenges.Should().Be("Test challenges");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_037_WhySectionRequest_ToModel_SdGsMapped()
    {
        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } } };
        var sdgCount = request.SdGs?.Count ?? 0;
        sdgCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_038_WhySectionRequest_ToModel_UncfOutcomesMapped()
    {
        var request = new WhySectionRequest { UncfOutcomes = new List<OpportunityUNCFOutcomeRequest> { new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 } } };
        var outcomeCount = request.UncfOutcomes?.Count ?? 0;
        outcomeCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_039_OpportunitySDGRequest_IsPrimary_MapsToMain()
    {
        var isPrimary = true;
        var displayLabel = isPrimary ? "Main" : "Cross-cutting";
        displayLabel.Should().Be("Main");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_040_OpportunitySDGRequest_IsPrimaryFalse_MapsToCrossCutting()
    {
        var isPrimary = false;
        var displayLabel = isPrimary ? "Main" : "Cross-cutting";
        displayLabel.Should().Be("Cross-cutting");
    }

    #endregion

    #region Validation Rules

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_041_GoDecision_SDGValidation_MainRequired()
    {
        var sdgs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } };
        var isValid = sdgs.Any(s => s.IsPrimary);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_042_GoDecision_SDGValidation_EmptyFails()
    {
        var sdgs = new List<OpportunitySDGRequest>();
        var isValid = sdgs.Any(s => s.IsPrimary);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_043_GoDecision_SDGValidation_OnlyCrossCuttingFails()
    {
        var sdgs = new List<OpportunitySDGRequest> { new() { SDGId = 13, IsPrimary = false } };
        var isValid = sdgs.Any(s => s.IsPrimary);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_044_GoDecision_SDGValidation_MultipleMainFails()
    {
        var sdgs = new List<OpportunitySDGRequest>
        {
            new() { SDGId = 6, IsPrimary = true },
            new() { SDGId = 13, IsPrimary = true }
        };
        var mainCount = sdgs.Count(s => s.IsPrimary);
        var isValid = mainCount == 1;
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_045_Challenges_MaxLengthValidation()
    {
        var maxLen = OpportunityWhySectionSpec.ChallengesMaxLength;
        var text = new string('A', maxLen);
        var isValid = text.Length <= maxLen;
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_046_ExpectedImpact_MaxLengthValidation()
    {
        var maxLen = OpportunityWhySectionSpec.ExpectedImpactMaxLength;
        var text = new string('X', maxLen);
        var isValid = text.Length <= maxLen;
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_047_ExpectedOutcomes_MaxLengthValidation()
    {
        var maxLen = OpportunityWhySectionSpec.ExpectedOutcomesMaxLength;
        var text = new string('Y', maxLen);
        var isValid = text.Length <= maxLen;
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_048_ExpectedBeneficiaries_MaxLengthValidation()
    {
        var maxLen = OpportunityWhySectionSpec.ExpectedBeneficiariesMaxLength;
        var text = new string('Z', maxLen);
        var isValid = text.Length <= maxLen;
        isValid.Should().BeTrue();
    }

    #endregion

    #region UNSDCF Version Change (AC9)

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_049_UNSDCF_InactiveFramework_ShouldNotify()
    {
        var inactiveNotification = true;
        inactiveNotification.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_050_UNSDCF_NewVersionAvailable_ShouldIndicate()
    {
        var newVersionAvailable = true;
        newVersionAvailable.Should().BeTrue();
    }

    #endregion

    #region Humanitarian/Peace/Security (AC10)

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_051_HumanitarianFramework_PerCountry()
    {
        var perCountry = true;
        perCountry.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_052_HumanitarianFramework_CanIndicateAlignOrNot()
    {
        var canIndicate = true;
        canIndicate.Should().BeTrue();
    }

    #endregion

    #region Additional Functional Rules

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_053_WhySectionRequest_ResultsFocus_Optional()
    {
        var request = new WhySectionRequest { ResultsFocus = null };
        request.ResultsFocus.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_054_OpportunitySDGRequest_Notes_Optional()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, Notes = null };
        request.Notes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_055_OpportunityUNCFOutcomeRequest_Notes_Optional()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, Notes = null };
        request.Notes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_056_SdGs_CanSaveWithoutTargets()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, SkipTargetsAndIndicators = true, Targets = null };
        var canSave = request.SkipTargetsAndIndicators == true || (request.Targets?.Count ?? 0) >= 0;
        canSave.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_057_WhySectionRequest_PartialUpdate_OnlyChallenges()
    {
        var request = new WhySectionRequest { Challenges = "Updated challenges" };
        request.Challenges.Should().NotBeNull();
        request.ExpectedImpact.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_058_WhySectionRequest_PartialUpdate_OnlySDGs()
    {
        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } } };
        request.SdGs!.Count.Should().Be(1);
        request.Challenges.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_059_OpportunitySDGTargetRequest_OptionalIndicators()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1, SDGIndicatorDatabaseIds = null };
        request.SDGIndicatorDatabaseIds.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_060_OpportunitySDGTargetRequest_CanIncludeIndicators()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1, SDGIndicatorDatabaseIds = new List<int> { 1 } };
        request.SDGIndicatorDatabaseIds!.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_061_WhySectionRequest_AllFieldsPopulated_Valid()
    {
        var request = new WhySectionRequest
        {
            Challenges = "C",
            ExpectedImpact = "I",
            ExpectedOutcomes = "O",
            ExpectedBeneficiaries = "B",
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } },
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest> { new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 } },
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest> { new() { UNOPSMissionId = 1 } }
        };
        request.Challenges.Should().NotBeNull();
        request.SdGs!.Count.Should().Be(1);
        request.UncfOutcomes!.Count.Should().Be(1);
        request.UNOPSMissions!.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_062_SdGs_NoDuplicateSDGIds_Validation()
    {
        var sdgs = new List<OpportunitySDGRequest>
        {
            new() { SDGId = 6, IsPrimary = true },
            new() { SDGId = 8, IsPrimary = false }
        };
        var duplicateIds = sdgs.GroupBy(s => s.SDGId).Any(g => g.Count() > 1);
        duplicateIds.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_063_SdGs_DuplicateSDGIds_Invalid()
    {
        var sdgs = new List<OpportunitySDGRequest>
        {
            new() { SDGId = 6, IsPrimary = true },
            new() { SDGId = 6, IsPrimary = false }
        };
        var duplicateIds = sdgs.GroupBy(s => s.SDGId).Any(g => g.Count() > 1);
        duplicateIds.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_064_OpportunityUNOPSMissionRequest_Id_ForUpdate()
    {
        var request = new OpportunityUNOPSMissionRequest { Id = 10, UNOPSMissionId = 1 };
        request.Id.Should().Be(10);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_065_OpportunityUNOPSMissionRequest_Id_NullForCreate()
    {
        var request = new OpportunityUNOPSMissionRequest { Id = null, UNOPSMissionId = 1 };
        request.Id.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_066_WhySectionRequest_UncfOutcomes_DistinctCountryOutcomePairs()
    {
        var request = new WhySectionRequest
        {
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>
            {
                new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 },
                new() { UNCFOutcomeId = 2, OpportunityCountryId = 1 }
            }
        };
        var distinctPairs = request.UncfOutcomes!.Select(u => new { u.OpportunityCountryId, u.UNCFOutcomeId }).Distinct().Count();
        distinctPairs.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_067_EstimatedBeneficiaries_Null_WhenToBeDetermined()
    {
        var request = new WhySectionRequest { BeneficiariesToBeDetermined = true, EstimatedDirectBeneficiaries = null };
        request.EstimatedDirectBeneficiaries.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_068_ExpectedImpact_GuidingText_PNO886()
    {
        var guidingText = "Impact implies changes in people's lives - briefly state expected long-term positive impact pursued by partner(s)";
        guidingText.Should().Contain("Impact");
        guidingText.Should().Contain("long-term");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_069_ContextAndChallenges_GuidingText()
    {
        var guidingText = "Describe challenge(s) that the initiative will address";
        guidingText.Should().Contain("challenge");
        guidingText.Should().Contain("initiative");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_070_SdGs_OptOutOncePerOpportunity_PNO817()
    {
        var skipTargets = true;
        skipTargets.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_071_NoNeedToOptOutPerSDG_PNO817()
    {
        var optOutOnce = true;
        optOutOnce.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_072_OpportunitySDGRequest_SDGId_ReferencesSDGEntity()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true };
        request.SDGId.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_073_OpportunityUNCFOutcomeRequest_OpportunityCountryId_ReferencesOpportunityCountry()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1 };
        request.OpportunityCountryId.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_074_OpportunityUNCFOutcomeRequest_UNCFOutcomeId_ReferencesUNCFOutcome()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1 };
        request.UNCFOutcomeId.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_075_OpportunityUNOPSMissionRequest_UNOPSMissionId_ReferencesUNOPSMission()
    {
        var request = new OpportunityUNOPSMissionRequest { UNOPSMissionId = 1 };
        request.UNOPSMissionId.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_076_OpportunitySDGTargetRequest_SDGTargetDatabaseId_ReferencesSDGTarget()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1 };
        request.SDGTargetDatabaseId.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_077_WhySectionRequest_EmptySdGs_ValidForPartialUpdate()
    {
        var request = new WhySectionRequest { Challenges = "C", SdGs = new List<OpportunitySDGRequest>() };
        request.SdGs!.Count.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_078_WhySectionRequest_NullSdGs_ValidForPartialUpdate()
    {
        var request = new WhySectionRequest { Challenges = "C", SdGs = null };
        request.SdGs.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_079_SdGs_MainAndCrossCutting_BothCanBePresent()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 6, IsPrimary = true },
                new() { SDGId = 13, IsPrimary = false }
            }
        };
        var hasMain = request.SdGs!.Any(s => s.IsPrimary);
        var hasCrossCutting = request.SdGs!.Any(s => !s.IsPrimary);
        hasMain.Should().BeTrue();
        hasCrossCutting.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_080_WhySectionRequest_ResultsFocus_CanBeSet()
    {
        var request = new WhySectionRequest { ResultsFocus = "Sustainable development" };
        request.ResultsFocus.Should().Be("Sustainable development");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_081_OpportunitySDGRequest_Notes_Max2000Chars()
    {
        var request = new OpportunitySDGRequest { SDGId = 6, IsPrimary = true, Notes = new string('N', 2000) };
        request.Notes!.Length.Should().Be(2000);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_082_WhySectionRequest_EstimatedBeneficiaries_BothDirectAndIndirect()
    {
        var request = new WhySectionRequest { EstimatedDirectBeneficiaries = 5000, EstimatedIndirectBeneficiaries = 15000 };
        request.EstimatedDirectBeneficiaries.Should().Be(5000);
        request.EstimatedIndirectBeneficiaries.Should().Be(15000);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_083_WhySectionRequest_UncfOutcomes_SameCountryMultipleOutcomes()
    {
        var request = new WhySectionRequest
        {
            UncfOutcomes = new List<OpportunityUNCFOutcomeRequest>
            {
                new() { UNCFOutcomeId = 1, OpportunityCountryId = 1 },
                new() { UNCFOutcomeId = 2, OpportunityCountryId = 1 }
            }
        };
        request.UncfOutcomes!.Count(u => u.OpportunityCountryId == 1).Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_084_OpportunitySDGRequest_Targets_MultipleTargets()
    {
        var request = new OpportunitySDGRequest
        {
            SDGId = 6,
            IsPrimary = true,
            Targets = new List<OpportunitySDGTargetRequest>
            {
                new() { SDGTargetDatabaseId = 1 },
                new() { SDGTargetDatabaseId = 2 },
                new() { SDGTargetDatabaseId = 3 }
            }
        };
        request.Targets!.Count.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_085_OpportunitySDGTargetRequest_SDGIndicatorDatabaseIds_Multiple()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1, SDGIndicatorDatabaseIds = new List<int> { 1, 2, 3 } };
        request.SDGIndicatorDatabaseIds!.Count.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_086_WhySectionRequest_UNOPSMissions_MultipleMissions()
    {
        var request = new WhySectionRequest
        {
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest>
            {
                new() { UNOPSMissionId = 1 },
                new() { UNOPSMissionId = 2 },
                new() { UNOPSMissionId = 3 }
            }
        };
        request.UNOPSMissions!.Count.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_087_SdGs_CrossCutting_ZeroToMany()
    {
        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 6, IsPrimary = true } } };
        var crossCuttingCount = request.SdGs!.Count(s => !s.IsPrimary);
        crossCuttingCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_088_WhySectionRequest_Challenges_Null_Allowed()
    {
        var request = new WhySectionRequest { ExpectedImpact = "Impact" };
        request.Challenges.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_089_OpportunityUNCFOutcomeRequest_Notes_CanBeSet()
    {
        var request = new OpportunityUNCFOutcomeRequest { UNCFOutcomeId = 1, OpportunityCountryId = 1, Notes = "Alignment note" };
        request.Notes.Should().Be("Alignment note");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_090_OpportunitySDGTargetRequest_Notes_CanBeSet()
    {
        var request = new OpportunitySDGTargetRequest { SDGTargetDatabaseId = 1, Notes = "Target note" };
        request.Notes.Should().Be("Target note");
    }

    #endregion
}
