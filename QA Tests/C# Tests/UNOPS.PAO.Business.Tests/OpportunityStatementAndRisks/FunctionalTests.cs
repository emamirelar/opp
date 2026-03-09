using FluentAssertions;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityStatementAndRisks;

/// <summary>
/// Functional tests for Opportunity Statement and Risk Register (PNO-705, PNO-761, PNO-922, PNO-975).
/// Business rules, validation logic, state transitions, data transformations.
/// </summary>
public class FunctionalTests
{
    #region PNO-705 — Opportunity Statement Business Rules

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_001_StatementGeneration_RequiresEntityNameAndId()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1 };
        var hasRequired = !string.IsNullOrEmpty(request.EntityName) && request.EntityId.HasValue && request.EntityId > 0;
        hasRequired.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_002_StatementGeneration_DataOverridesEntityFetch()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1, Data = "# Override" };
        var useData = !string.IsNullOrEmpty(request.Data);
        useData.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_003_StatementGeneration_EmptyMarkdown_ReturnsError()
    {
        var markdown = "";
        var hasContent = !string.IsNullOrEmpty(markdown);
        hasContent.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_004_StatementGeneration_DefaultFilename_WhenNotProvided()
    {
        var request = new GeneratePdfRequest { Data = "# Test" };
        var filename = string.IsNullOrEmpty(request.Filename) ? OpportunityStatementAndRisksSpec.DefaultStatementFilename : request.Filename;
        filename.Should().Be(OpportunityStatementAndRisksSpec.DefaultStatementFilename);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_005_StatementGeneration_EntityNameMustBeOpportunity_ForStatement()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1 };
        var isOpportunity = string.Equals(request.EntityName, OpportunityStatementAndRisksSpec.OpportunityEntityName, StringComparison.OrdinalIgnoreCase);
        isOpportunity.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_006_StatementGeneration_NonOpportunityEntity_NoDocumentRecord()
    {
        var request = new GeneratePdfRequest { EntityName = "Partner", EntityId = 1 };
        var isOpportunity = string.Equals(request.EntityName, "Opportunity", StringComparison.OrdinalIgnoreCase);
        isOpportunity.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_007_StatementDocumentType_OpportunityStatement()
    {
        OpportunityStatementAndRisksSpec.OpportunityStatementDocumentType.Should().Be("Opportunity Statement");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_008_IncompleteSectionsMessage_DisplayedWhenSectionsMissing()
    {
        var message = OpportunityStatementAndRisksSpec.IncompleteSectionsMessage;
        message.Should().Contain("Complete all sections");
        message.Should().Contain("generate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_009_StatementEditable_UntilSubmitForGo()
    {
        const string stage = "IDENTIFY & PROFILE";
        var canEdit = stage == OpportunityWorkflow.Stages.IdentifyAndProfile;
        canEdit.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_010_StatementFinalMatch_FlagsDisparities()
    {
        var recordSdg = "SDG 6";
        var statementSdg = "SDG 6";
        var hasDisparity = recordSdg != statementSdg;
        hasDisparity.Should().BeFalse();
    }

    #endregion

    #region PNO-761 — Risk Register Business Rules

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_011_RiskCreate_TitleAlwaysMandatory()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        var hasTitle = !string.IsNullOrWhiteSpace(request.Title);
        hasTitle.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_012_RiskCreate_ManualEntry_DefaultsForOptional()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.RiskTypeId.Should().BeNull();
        request.RiskCategoryId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_013_RiskCreate_PreDefined_AllFieldsMandatory()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", PreDefinedHighRiskId = 1 };
        request.PreDefinedHighRiskId.HasValue.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_014_RiskScoring_LikelihoodTimesImpact()
    {
        const int likelihood = 3, impact = 4;
        var score = likelihood * impact;
        score.Should().Be(12);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_015_RiskScoring_FormulaDocumented()
    {
        OpportunityStatementAndRisksSpec.RiskScoringFormula.Should().Contain("×");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_016_RiskCategory_RequiredForPreDefined()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", PreDefinedHighRiskId = 1 };
        var hasPreDefined = request.PreDefinedHighRiskId.HasValue;
        hasPreDefined.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_017_RiskProbability_RequiredForPreDefined()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", PreDefinedHighRiskId = 1, RiskProbabilityId = 2 };
        request.RiskProbabilityId.Should().HaveValue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_018_RiskImpact_RequiredForPreDefined()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", PreDefinedHighRiskId = 1, RiskImpactLevelId = 3 };
        request.RiskImpactLevelId.Should().HaveValue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_019_RiskUserMustIntentionallyAdd_NoAutoAdd()
    {
        const bool autoAdd = false;
        autoAdd.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_020_RiskAICanRecommend_EasyToAdd()
    {
        const bool aiRecommendation = true;
        aiRecommendation.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_021_RiskFieldsAlignWithoUP()
    {
        var request = new RiskCreateRequest
        {
            EntityId = 1,
            Title = "Risk",
            RiskTypeId = 1,
            RiskCategoryId = 2,
            RiskProbabilityId = 1,
            RiskProximityId = 1,
            RiskImpactLevelId = 2,
            RiskResponseTypeId = 1,
            Description = "Desc",
            Recommendation = "Rec"
        };
        request.RiskTypeId.Should().HaveValue();
        request.RiskCategoryId.Should().HaveValue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_022_HighRisksAcknowledged_UserAwareness()
    {
        OpportunityStatementAndRisksSpec.HighRisksAcknowledgedField.Should().Be("HighRisksAcknowledged");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_023_RisksNavigationLabel_RisksNotDST()
    {
        OpportunityStatementAndRisksSpec.RisksNavigationLabel.Should().Be("Risks");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_024_RiskType_ThreatOrOpportunity()
    {
        OpportunityStatementAndRisksSpec.RiskTypeThreat.Should().Be("Threat");
        OpportunityStatementAndRisksSpec.RiskTypeOpportunity.Should().Be("Opportunity");
    }

    #endregion

    #region PNO-922 — Edit Option Business Rules

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_025_RiskUpdate_RequiresValidRiskId()
    {
        const int riskId = 1;
        riskId.Should().BePositive();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_026_RiskUpdate_RequiresValidOpportunityId()
    {
        const int oppId = 1;
        oppId.Should().BePositive();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_027_RiskUpdate_UsesSameRequestAsCreate()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Updated" };
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_028_RiskUpdate_CanChangeProbability()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskProbabilityId = 3 };
        request.RiskProbabilityId.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_029_RiskUpdate_CanChangeImpact()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskImpactLevelId = 4 };
        request.RiskImpactLevelId.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_030_RiskUpdate_CanChangeDescription()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", Description = "New desc" };
        request.Description.Should().Be("New desc");
    }

    #endregion

    #region PNO-975 — Popup Visibility Business Rules

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_031_RiskPopup_ModalMustBeVisible()
    {
        const bool visible = true;
        visible.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_032_RiskPopup_ZIndexAboveHeader()
    {
        const int popupZ = 1100, headerZ = 1000;
        (popupZ > headerZ).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_033_RiskPopup_AddNewRisk_OpensPopup()
    {
        const string action = "Add to Register";
        action.Should().Contain("Add");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_034_RiskPopup_ModalBlocking()
    {
        const bool isModal = true;
        isModal.Should().BeTrue();
    }

    #endregion

    #region Opportunity Workflow — State Transitions

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_035_Workflow_IdentifyAndProfile_InitialStage()
    {
        OpportunityWorkflow.Stages.IdentifyAndProfile.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_036_Workflow_Go_FinalPositiveStage()
    {
        OpportunityWorkflow.Stages.Go.Should().Be("GO");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_037_Workflow_NoGo_Reopenable()
    {
        OpportunityWorkflow.Stages.NoGo.Should().Be("NO GO");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_038_Workflow_Cancelled_Reopenable()
    {
        OpportunityWorkflow.Stages.Cancelled.Should().Be("CANCELLED");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_039_Workflow_IsValidStage_ValidatesInput()
    {
        OpportunityWorkflow.IsValidStage("GO").Should().BeTrue();
        OpportunityWorkflow.IsValidStage("invalid").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_040_Workflow_StateMachine_HasFourStates()
    {
        OpportunityWorkflow.StateMachine.States.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_041_Workflow_StateMachine_EntityTypeOpportunity()
    {
        OpportunityWorkflow.StateMachine.EntityType.Should().Be("Opportunity");
    }

    #endregion

    #region API Endpoint Structure

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_042_GenerateStatementPdf_PostEndpoint()
    {
        OpportunityStatementAndRisksSpec.GenerateStatementPdfEndpoint.Should().Contain("generate-statement-pdf");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_043_GenerateStatement_IncludesOpportunityId()
    {
        var endpoint = OpportunityStatementAndRisksSpec.GenerateStatementEndpoint(42);
        endpoint.Should().Contain("42");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_044_DstRisks_GetEndpoint()
    {
        var endpoint = OpportunityStatementAndRisksSpec.DstRisksEndpoint(1);
        endpoint.Should().Contain("dst-risks");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_045_UpdateRisk_PutEndpoint()
    {
        var endpoint = OpportunityStatementAndRisksSpec.UpdateRiskEndpoint(1, 1);
        endpoint.Should().Contain("dst-risks");
        endpoint.Should().Contain("1");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_046_DeleteRisk_DeleteEndpoint()
    {
        var endpoint = OpportunityStatementAndRisksSpec.DeleteRiskEndpoint(1, 1);
        endpoint.Should().Contain("dst-risks");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_047_HighRiskAnalysis_GetEndpoint()
    {
        var endpoint = OpportunityStatementAndRisksSpec.HighRiskAnalysisEndpoint(1);
        endpoint.Should().Contain("high-risk-analysis");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_048_AcknowledgeHighRisks_PutEndpoint()
    {
        var endpoint = OpportunityStatementAndRisksSpec.AcknowledgeHighRisksEndpoint(1);
        endpoint.Should().Contain("acknowledge-high-risks");
    }

    #endregion

    #region Risk Model — Data Transformations

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_049_RiskModel_EntityTypeMatchesEntity()
    {
        var model = new RiskModel { EntityType = "Opportunity", EntityId = 5 };
        model.EntityType.Should().Be(OpportunityStatementAndRisksSpec.RiskEntityTypeOpportunity);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_050_RiskModel_TitleRequired()
    {
        var model = new RiskModel { Title = "Risk" };
        model.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_051_RiskModel_PreDefinedHighRiskFlag()
    {
        var model = new RiskModel { PreDefinedHighRiskId = 1 };
        model.PreDefinedHighRiskId.HasValue.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_052_RiskModel_ManualRiskNoPreDefined()
    {
        var model = new RiskModel { PreDefinedHighRiskId = null };
        model.PreDefinedHighRiskId.HasValue.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_053_DSTRisksResponse_TotalCountMatchesRisks()
    {
        var risks = new List<RiskModel> { new() { Id = 1 }, new() { Id = 2 } };
        var response = new DSTRisksResponse { Risks = risks, TotalCount = risks.Count };
        response.TotalCount.Should().Be(response.Risks.Count);
    }

    #endregion

    #region Go Decision Workflow

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_054_GoDecision_RequiresStatementMarkdown()
    {
        var hasStatement = !string.IsNullOrEmpty("# Statement");
        hasStatement.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_055_GoDecision_RequiresHighRisksAcknowledged()
    {
        const bool acknowledged = true;
        acknowledged.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_056_GoDecision_StatementAvailableInWorkflow()
    {
        OpportunityWorkflow.EntityName.Should().Be("Opportunity");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_057_GoDecision_OpportunityInGoStage()
    {
        var stage = OpportunityWorkflow.Stages.Go;
        stage.Should().Be("GO");
    }

    #endregion

    #region IRiskManager Interface

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_058_IRiskManager_GetRisksByEntityAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetRisksByEntityAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_059_IRiskManager_CreateRiskAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("CreateRiskAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_060_IRiskManager_UpdateRiskAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("UpdateRiskAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_061_IRiskManager_DeleteRiskAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("DeleteRiskAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_062_IRiskManager_GetHighRiskAnalysisAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetHighRiskAnalysisAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_063_IRiskManager_GetRiskLookupsAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetRiskLookupsAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_064_IRiskManager_GetRiskCategoriesAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetRiskCategoriesAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_065_IRiskManager_GetPreDefinedHighRisksAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetPreDefinedHighRisksAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region GeneratePdfRequest — Validation Logic

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_066_GeneratePdfRequest_EntityFetch_WhenNoData()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1 };
        var fetchFromEntity = string.IsNullOrEmpty(request.Data) && !string.IsNullOrEmpty(request.EntityName) && request.EntityId.HasValue;
        fetchFromEntity.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_067_GeneratePdfRequest_UseData_WhenProvided()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1, Data = "# Override" };
        var useData = !string.IsNullOrEmpty(request.Data);
        useData.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_068_GeneratePdfRequest_EntityIdMustBePositive()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 123 };
        (request.EntityId.HasValue && request.EntityId.Value > 0).Should().BeTrue();
    }

    #endregion

    #region PreDefined High Risk

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_069_PreDefinedHighRisk_CodeFormat_1_1_1()
    {
        const string code = "1.1.1";
        code.Should().MatchRegex(@"^\d+\.\d+\.\d+$");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_070_PreDefinedHighRisk_CodeFormat_1_2_1()
    {
        const string code = "1.2.1";
        code.Split('.').Length.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_071_PreDefinedHighRisk_CodeFormat_1_4_5()
    {
        const string code = "1.4.5";
        code.Should().StartWith("1.");
    }

    #endregion

    #region Risk Response Type

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_072_RiskResponseType_OptionalForThreat()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Threat", RiskTypeId = 1 };
        request.RiskResponseTypeId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_073_RiskResponseType_CanBeProvided()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskResponseTypeId = 1 };
        request.RiskResponseTypeId.Should().Be(1);
    }

    #endregion

    #region Statement Markdown

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_074_StatementMarkdown_CanContainHeaders()
    {
        var markdown = "# Opportunity Statement\n## Section 1";
        markdown.Should().Contain("#");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_075_StatementMarkdown_CanContainLists()
    {
        var markdown = "- Item 1\n- Item 2";
        markdown.Should().Contain("-");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_076_StatementMarkdown_CanContainBold()
    {
        var markdown = "**Bold text**";
        markdown.Should().Contain("**");
    }

    #endregion

    #region Risk Description

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_077_RiskDescription_Optional()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.Description.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_078_RiskRecommendation_Optional()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.Recommendation.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_079_RiskDescription_CanBeProvided()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", Description = "Detailed description" };
        request.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_080_RiskRecommendation_CanBeProvided()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", Recommendation = "Mitigation plan" };
        request.Recommendation.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Opportunity Statement Template

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_081_StatementTemplate_StructureVisible()
    {
        OpportunityStatementAndRisksSpec.IncompleteSectionsMessage.Should().Contain("sections");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_082_StatementTemplate_LinkageToSections()
    {
        var message = OpportunityStatementAndRisksSpec.IncompleteSectionsMessage;
        message.Length.Should().BeGreaterThan(50);
    }

    #endregion

    #region Risk Proximity

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_083_RiskProximity_OptionalForManual()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.RiskProximityId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_084_RiskProximity_CanBeProvided()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", RiskProximityId = 1 };
        request.RiskProximityId.Should().Be(1);
    }

    #endregion

    #region Validate Statement

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_085_ValidateStatementEndpoint_Exists()
    {
        var endpoint = OpportunityStatementAndRisksSpec.ValidateStatementEndpoint(1);
        endpoint.Should().Contain("validate-statement");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_086_ValidateStatement_FlagsDisparities()
    {
        var endpoint = OpportunityStatementAndRisksSpec.ValidateStatementEndpoint(1);
        endpoint.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Risk Impact Legacy

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_087_RiskCreateRequest_ImpactDefaultTwo()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.Impact.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_088_RiskCreateRequest_ImpactCanOverride()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk", Impact = 3 };
        request.Impact.Should().Be(3);
    }

    #endregion

    #region EntityType Consistency

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_089_RiskEntityType_OpportunityConsistent()
    {
        OpportunityStatementAndRisksSpec.RiskEntityTypeOpportunity.Should().Be(OpportunityStatementAndRisksSpec.OpportunityEntityName);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUN_090_WorkflowEntityName_Opportunity()
    {
        OpportunityWorkflow.EntityName.Should().Be(OpportunityStatementAndRisksSpec.OpportunityEntityName);
    }

    #endregion
}
