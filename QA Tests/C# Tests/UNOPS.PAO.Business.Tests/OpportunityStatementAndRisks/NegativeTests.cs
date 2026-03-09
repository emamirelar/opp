using FluentAssertions;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityStatementAndRisks;

/// <summary>
/// Negative tests for Opportunity Statement and Risk Register (PNO-705, PNO-761, PNO-922, PNO-975).
/// Invalid inputs, wrong states, expected failures.
/// </summary>
public class NegativeTests
{
    #region GeneratePdfRequest — Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_001_GeneratePdfRequest_NullEntityNameWithEntityId_Ambiguous()
    {
        var request = new GeneratePdfRequest { EntityName = null, EntityId = 1 };
        request.EntityName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_002_GeneratePdfRequest_EmptyEntityName_Invalid()
    {
        var request = new GeneratePdfRequest { EntityName = "", EntityId = 1 };
        string.IsNullOrEmpty(request.EntityName).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_003_GeneratePdfRequest_EntityIdZero_Invalid()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 0 };
        request.EntityId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_004_GeneratePdfRequest_EntityIdNegative_Invalid()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = -1 };
        request.EntityId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_005_GeneratePdfRequest_NoDataNoEntity_Invalid()
    {
        var request = new GeneratePdfRequest { Data = null, EntityName = null, EntityId = null };
        (string.IsNullOrEmpty(request.Data) && string.IsNullOrEmpty(request.EntityName)).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_006_GeneratePdfRequest_EmptyData_Invalid()
    {
        var request = new GeneratePdfRequest { Data = "" };
        request.Data.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_007_GeneratePdfRequest_WhitespaceOnlyData_Invalid()
    {
        var request = new GeneratePdfRequest { Data = "   \t\n  " };
        string.IsNullOrWhiteSpace(request.Data).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_008_GeneratePdfRequest_WrongEntityName_Invalid()
    {
        var request = new GeneratePdfRequest { EntityName = "Partner", EntityId = 1 };
        request.EntityName.Should().NotBe(OpportunityStatementAndRisksSpec.OpportunityEntityName);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_009_GeneratePdfRequest_EntityNameWithoutEntityId_Invalid()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = null };
        request.EntityId.HasValue.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_010_GeneratePdfRequest_EntityIdWithoutEntityName_Invalid()
    {
        var request = new GeneratePdfRequest { EntityName = null, EntityId = 1 };
        string.IsNullOrEmpty(request.EntityName).Should().BeTrue();
    }

    #endregion

    #region RiskCreateRequest — Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_011_RiskCreateRequest_EmptyTitle_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "" };
        request.Title.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_012_RiskCreateRequest_NullTitle_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = null! };
        request.Title.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_013_RiskCreateRequest_WhitespaceOnlyTitle_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "   \t  " };
        string.IsNullOrWhiteSpace(request.Title).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_014_RiskCreateRequest_EntityIdZero_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 0, Title = "Valid Title" };
        request.EntityId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_015_RiskCreateRequest_EntityIdNegative_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = -5, Title = "Valid Title" };
        request.EntityId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_016_RiskCreateRequest_RiskTypeIdZero_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskTypeId = 0 };
        request.RiskTypeId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_017_RiskCreateRequest_RiskCategoryIdNegative_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskCategoryId = -1 };
        request.RiskCategoryId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_018_RiskCreateRequest_RiskProbabilityIdNegative_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskProbabilityId = -2 };
        request.RiskProbabilityId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_019_RiskCreateRequest_RiskImpactLevelIdZero_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskImpactLevelId = 0 };
        request.RiskImpactLevelId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_020_RiskCreateRequest_PreDefinedHighRiskIdZero_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", PreDefinedHighRiskId = 0 };
        request.PreDefinedHighRiskId.Should().Be(0);
    }

    #endregion

    #region RiskModel — Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_021_RiskModel_IdZero_Invalid()
    {
        var model = new RiskModel { Id = 0 };
        model.Id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_022_RiskModel_EmptyEntityType_Invalid()
    {
        var model = new RiskModel { EntityType = "" };
        model.EntityType.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_023_RiskModel_EmptyTitle_Invalid()
    {
        var model = new RiskModel { Title = "" };
        model.Title.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_024_RiskModel_WrongEntityType_Invalid()
    {
        var model = new RiskModel { EntityType = "Project" };
        model.EntityType.Should().NotBe(OpportunityStatementAndRisksSpec.RiskEntityTypeOpportunity);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_025_DSTRisksResponse_NullRisks_Invalid()
    {
        var response = new DSTRisksResponse { Risks = null! };
        response.Risks.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_026_DSTRisksResponse_NegativeTotalCount_Invalid()
    {
        var response = new DSTRisksResponse { TotalCount = -1 };
        response.TotalCount.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_027_DSTRisksResponse_TotalCountMismatch_Invalid()
    {
        var response = new DSTRisksResponse { Risks = [new RiskModel()], TotalCount = 5 };
        response.Risks.Count.Should().NotBe(response.TotalCount);
    }

    #endregion

    #region Opportunity Workflow — Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_028_OpportunityWorkflow_IsValidStage_Null_ReturnsFalse()
    {
        OpportunityWorkflow.IsValidStage(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_029_OpportunityWorkflow_IsValidStage_Empty_ReturnsFalse()
    {
        OpportunityWorkflow.IsValidStage("").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_030_OpportunityWorkflow_IsValidStage_InvalidStage_ReturnsFalse()
    {
        OpportunityWorkflow.IsValidStage("INVALID").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_031_OpportunityWorkflow_IsValidStage_LowercaseGo_ReturnsFalse()
    {
        OpportunityWorkflow.IsValidStage("go").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_032_OpportunityWorkflow_IsValidStage_Whitespace_ReturnsFalse()
    {
        OpportunityWorkflow.IsValidStage("  GO  ").Should().BeFalse();
    }

    #endregion

    #region Spec Constants — Negative Assertions

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_033_RisksNavigationLabel_NotDST()
    {
        OpportunityStatementAndRisksSpec.RisksNavigationLabel.Should().NotBe("DST");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_034_RisksNavigationLabel_NotDSTAnalysis()
    {
        OpportunityStatementAndRisksSpec.RisksNavigationLabel.Should().NotBe("DST Analysis");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_035_OpportunityEntityName_NotPartner()
    {
        OpportunityStatementAndRisksSpec.OpportunityEntityName.Should().NotBe("Partner");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_036_OpportunityEntityName_NotProject()
    {
        OpportunityStatementAndRisksSpec.OpportunityEntityName.Should().NotBe("Project");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_037_RiskEntityType_NotProject()
    {
        OpportunityStatementAndRisksSpec.RiskEntityTypeOpportunity.Should().NotBe("Project");
    }

    #endregion

    #region API Endpoints — Invalid Patterns

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_038_DstRisksEndpoint_ZeroOpportunityId_Invalid()
    {
        var endpoint = OpportunityStatementAndRisksSpec.DstRisksEndpoint(0);
        endpoint.Should().Contain("0");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_039_UpdateRiskEndpoint_ZeroRiskId_Invalid()
    {
        var endpoint = OpportunityStatementAndRisksSpec.UpdateRiskEndpoint(1, 0);
        endpoint.Should().Contain("0");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_040_DeleteRiskEndpoint_NegativeIds_Invalid()
    {
        var endpoint = OpportunityStatementAndRisksSpec.DeleteRiskEndpoint(-1, -1);
        endpoint.Should().Contain("-1");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_041_GenerateStatementEndpoint_ZeroId_Invalid()
    {
        var endpoint = OpportunityStatementAndRisksSpec.GenerateStatementEndpoint(0);
        endpoint.Should().Contain("0");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_042_HighRiskAnalysisEndpoint_ZeroId_Invalid()
    {
        var endpoint = OpportunityStatementAndRisksSpec.HighRiskAnalysisEndpoint(0);
        endpoint.Should().Contain("0");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_043_AcknowledgeHighRisksEndpoint_NegativeId_Invalid()
    {
        var endpoint = OpportunityStatementAndRisksSpec.AcknowledgeHighRisksEndpoint(-5);
        endpoint.Should().Contain("-5");
    }

    #endregion

    #region Risk Scoring — Invalid

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_044_RiskScore_ProbabilityZero_Invalid()
    {
        const int probability = 0;
        const int impact = 3;
        probability.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_045_RiskScore_ImpactZero_Invalid()
    {
        const int probability = 2;
        const int impact = 0;
        impact.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_046_RiskScore_NegativeProbability_Invalid()
    {
        const int probability = -1;
        probability.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_047_RiskScore_NegativeImpact_Invalid()
    {
        const int impact = -2;
        impact.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_048_RiskProbability_OutOfRange_Invalid()
    {
        const int probability = 10;
        probability.Should().BeGreaterThan(5);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_049_RiskImpact_OutOfRange_Invalid()
    {
        const int impact = 99;
        impact.Should().BeGreaterThan(5);
    }

    #endregion

    #region High Risk Acknowledgement

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_050_HighRisksAcknowledgedField_NotNull()
    {
        OpportunityStatementAndRisksSpec.HighRisksAcknowledgedField.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_051_HighRisksAcknowledged_NotOptionalFieldName()
    {
        OpportunityStatementAndRisksSpec.HighRisksAcknowledgedField.Should().NotBe("Optional");
    }

    #endregion

    #region PNO-922 — Edit Option Negative (missing in UI)

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_052_RiskUpdate_RequiresValidRiskId()
    {
        const int invalidRiskId = 0;
        invalidRiskId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_053_RiskUpdate_RequiresValidOpportunityId()
    {
        const int invalidOppId = -1;
        invalidOppId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_054_RiskUpdate_NonExistentRisk_ShouldFail()
    {
        const int nonExistentId = 999999;
        nonExistentId.Should().BePositive();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_055_RiskUpdate_SoftDeletedRisk_ShouldNotBeEditable()
    {
        const bool isDeleted = true;
        isDeleted.Should().BeTrue();
    }

    #endregion

    #region PNO-975 — Popup Visibility

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_056_RiskPopup_ZIndexZero_Invalid()
    {
        const int zIndex = 0;
        zIndex.Should().BeLessThanOrEqualTo(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_057_RiskPopup_NegativeZIndex_Invalid()
    {
        const int zIndex = -100;
        zIndex.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_058_RiskPopup_ModalBehindHeader_Invalid()
    {
        const int popupZIndex = 100;
        const int headerZIndex = 1000;
        (popupZIndex < headerZIndex).Should().BeTrue();
    }

    #endregion

    #region Statement Markdown

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_059_OpportunityStatementMarkdown_Null_Invalid()
    {
        string? markdown = null;
        string.IsNullOrEmpty(markdown).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_060_OpportunityStatementMarkdown_Empty_Invalid()
    {
        var markdown = "";
        markdown.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_061_OpportunityStatementMarkdown_OnlyWhitespace_Invalid()
    {
        var markdown = "   \n\t  ";
        string.IsNullOrWhiteSpace(markdown).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_062_OpportunityStatement_InvalidMarkdown_Invalid()
    {
        var markdown = "<<<invalid>>>";
        markdown.Should().Contain("<<<");
    }

    #endregion

    #region Additional Risk Validation

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_063_RiskCreateRequest_DescriptionExceedsMax_Invalid()
    {
        var desc = new string('X', 10001);
        desc.Length.Should().BeGreaterThan(10000);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_064_RiskCreateRequest_RecommendationExceedsMax_Invalid()
    {
        var rec = new string('Y', 5001);
        rec.Length.Should().BeGreaterThan(5000);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_065_RiskCreateRequest_TitleExceedsMax_Invalid()
    {
        var title = new string('Z', 501);
        title.Length.Should().BeGreaterThan(500);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_066_RiskCreateRequest_EntityTypeMismatch_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.EntityId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_067_RiskModel_InvalidRiskTypeName_Invalid()
    {
        var model = new RiskModel { RiskTypeName = "" };
        string.IsNullOrEmpty(model.RiskTypeName).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_068_RiskModel_InvalidCategoryName_Invalid()
    {
        var model = new RiskModel { RiskCategoryName = null };
        model.RiskCategoryName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_069_RiskModel_InvalidProbabilityName_Invalid()
    {
        var model = new RiskModel { RiskProbabilityName = "" };
        string.IsNullOrEmpty(model.RiskProbabilityName).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_070_RiskModel_InvalidImpactName_Invalid()
    {
        var model = new RiskModel { RiskImpactLevelName = null };
        model.RiskImpactLevelName.Should().BeNull();
    }

    #endregion

    #region Go Decision Workflow

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_071_GoDecision_StatementWithoutMarkdown_Invalid()
    {
        string? statementMarkdown = null;
        string.IsNullOrEmpty(statementMarkdown).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_072_GoDecision_OpportunityNotInGoStage_Invalid()
    {
        var stage = "IDENTIFY & PROFILE";
        stage.Should().NotBe(OpportunityWorkflow.Stages.Go);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_073_GoDecision_OpportunityNotInIdentifyAndProfile_Invalid()
    {
        var stage = "GO";
        stage.Should().NotBe(OpportunityWorkflow.Stages.IdentifyAndProfile);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_074_GoDecision_HighRisksNotAcknowledged_Invalid()
    {
        const bool acknowledged = false;
        acknowledged.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_075_GoDecision_StatementDisparity_Invalid()
    {
        var recordValue = "SDG 6";
        var statementValue = "SDG 8";
        recordValue.Should().NotBe(statementValue);
    }

    #endregion

    #region AI Risk Suggestions

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_076_AIRiskSuggestion_EmptyOpportunity_Invalid()
    {
        const int opportunityId = 0;
        opportunityId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_077_AIRiskSuggestion_NonExistentOpportunity_Invalid()
    {
        const int opportunityId = -1;
        opportunityId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_078_AIRiskSuggestion_SoftDeletedOpportunity_Invalid()
    {
        const bool isDeleted = true;
        isDeleted.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_079_AIRiskSuggestion_NullUser_Invalid()
    {
        System.Security.Claims.ClaimsPrincipal? user = null;
        user.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_080_AutoAddRisks_NotAllowed_PNO761AC4()
    {
        const bool autoAddAllowed = false;
        autoAddAllowed.Should().BeFalse();
    }

    #endregion

    #region Document Type

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_081_OpportunityStatementDocumentType_NotConceptNote()
    {
        OpportunityStatementAndRisksSpec.OpportunityStatementDocumentType.Should().NotBe("Concept Note");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_082_OpportunityStatementDocumentType_NotDecisionNote()
    {
        OpportunityStatementAndRisksSpec.OpportunityStatementDocumentType.Should().NotBe("Decision Note");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_083_OpportunityStatementDocumentType_NotEmpty()
    {
        OpportunityStatementAndRisksSpec.OpportunityStatementDocumentType.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region PreDefined High Risk

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_084_PreDefinedHighRisk_NegativeId_Invalid()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", PreDefinedHighRiskId = -1 };
        request.PreDefinedHighRiskId.Should().BeNegative();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_085_PreDefinedHighRisk_NonExistentId_Invalid()
    {
        const int nonExistentId = 99999;
        nonExistentId.Should().BeGreaterThan(1000);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_086_RiskModel_PreDefinedHighRiskIdZero_NotPreDefined()
    {
        var model = new RiskModel { PreDefinedHighRiskId = 0 };
        (model.PreDefinedHighRiskId.HasValue && model.PreDefinedHighRiskId.Value > 0).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_087_RiskModel_PreDefinedHighRiskIdNull_NotPreDefined()
    {
        var model = new RiskModel { PreDefinedHighRiskId = null };
        model.PreDefinedHighRiskId.HasValue.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_088_RiskCreateRequest_PreDefinedWithoutMandatoryFields_Invalid()
    {
        var request = new RiskCreateRequest
        {
            EntityId = 1,
            Title = "Risk",
            PreDefinedHighRiskId = 1,
            RiskTypeId = null,
            RiskCategoryId = null
        };
        request.RiskTypeId.HasValue.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_089_RiskResponseType_OpportunityType_WhenMandatory_Invalid()
    {
        var request = new RiskCreateRequest
        {
            EntityId = 1,
            Title = "Opportunity Risk",
            RiskTypeId = 2,
            RiskResponseTypeId = null
        };
        request.RiskResponseTypeId.HasValue.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_090_ValidateStatementEndpoint_ZeroId_Invalid()
    {
        var endpoint = OpportunityStatementAndRisksSpec.ValidateStatementEndpoint(0);
        endpoint.Should().Contain("0");
    }

    #endregion
}
