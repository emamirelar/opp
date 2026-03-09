using FluentAssertions;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityStatementAndRisks;

/// <summary>
/// Positive tests for Opportunity Statement and Risk Register (PNO-705, PNO-761, PNO-922, PNO-975).
/// Happy path — feature works as designed.
/// </summary>
public class PositiveTests
{
    #region PNO-705 — Opportunity Statement Spec Constants

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_001_OpportunityEntityName_MatchesSpec()
    {
        OpportunityStatementAndRisksSpec.OpportunityEntityName.Should().Be("Opportunity");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_002_OpportunityStatementDocumentType_MatchesSpec()
    {
        OpportunityStatementAndRisksSpec.OpportunityStatementDocumentType.Should().Be("Opportunity Statement");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_003_DefaultStatementFilename_IsGeneratedDocument()
    {
        OpportunityStatementAndRisksSpec.DefaultStatementFilename.Should().Be("Generated_Document");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_004_IncompleteSectionsMessage_ContainsExpectedText()
    {
        OpportunityStatementAndRisksSpec.IncompleteSectionsMessage.Should().Contain("Complete all sections");
        OpportunityStatementAndRisksSpec.IncompleteSectionsMessage.Should().Contain("generate the Opportunity Statement");
    }

    #endregion

    #region PNO-705 — GeneratePdfRequest Model

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_005_GeneratePdfRequest_WithEntityNameAndId_Valid()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 123 };
        request.EntityName.Should().Be("Opportunity");
        request.EntityId.Should().Be(123);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_006_GeneratePdfRequest_WithData_Valid()
    {
        var markdown = "# Opportunity Statement\n\nContent here.";
        var request = new GeneratePdfRequest { Data = markdown, Filename = "MyStatement" };
        request.Data.Should().Be(markdown);
        request.Filename.Should().Be("MyStatement");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_007_GeneratePdfRequest_EntityNameEntityIdAndData_Valid()
    {
        var request = new GeneratePdfRequest
        {
            EntityName = "Opportunity",
            EntityId = 456,
            Data = "# Statement",
            Filename = "Opp_456_Statement"
        };
        request.EntityName.Should().Be("Opportunity");
        request.EntityId.Should().Be(456);
        request.Data.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region PNO-761 — Risk Model & Request

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_008_RiskCreateRequest_WithTitleOnly_ValidForManualEntry()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Funding currency risk" };
        request.EntityId.Should().Be(1);
        request.Title.Should().Be("Funding currency risk");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_009_RiskCreateRequest_WithAllMandatoryFields_Valid()
    {
        var request = new RiskCreateRequest
        {
            EntityId = 10,
            Title = "Security risk in conflict zone",
            RiskTypeId = 1,
            RiskCategoryId = 5,
            RiskProbabilityId = 2,
            RiskProximityId = 1,
            RiskImpactLevelId = 3,
            Description = "Armed conflict in region",
            Recommendation = "Coordinate with UN security"
        };
        request.Title.Should().NotBeNullOrEmpty();
        request.RiskTypeId.Should().HaveValue();
        request.RiskCategoryId.Should().HaveValue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_010_RiskCreateRequest_WithPreDefinedHighRiskId_Valid()
    {
        var request = new RiskCreateRequest
        {
            EntityId = 20,
            Title = "Host country agreement risk",
            PreDefinedHighRiskId = 1
        };
        request.PreDefinedHighRiskId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_011_RiskModel_HasRequiredProperties()
    {
        var model = new RiskModel
        {
            Id = 1,
            EntityType = "Opportunity",
            EntityId = 5,
            Title = "Test Risk",
            RiskTypeId = 1,
            RiskCategoryId = 2,
            RiskProbabilityId = 1,
            RiskProximityId = 1,
            RiskImpactLevelId = 2
        };
        model.Id.Should().Be(1);
        model.EntityType.Should().Be("Opportunity");
        model.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_012_DSTRisksResponse_HasRisksAndTotalCount()
    {
        var response = new DSTRisksResponse { Risks = [new RiskModel { Id = 1, Title = "R1" }], TotalCount = 1 };
        response.Risks.Should().HaveCount(1);
        response.TotalCount.Should().Be(1);
    }

    #endregion

    #region PNO-761 — Risk Scoring & Structure

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_013_RiskScoringFormula_LikelihoodTimesImpact()
    {
        OpportunityStatementAndRisksSpec.RiskScoringFormula.Should().Contain("Likelihood");
        OpportunityStatementAndRisksSpec.RiskScoringFormula.Should().Contain("Impact");
        OpportunityStatementAndRisksSpec.RiskScoringFormula.Should().Contain("Risk Score");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_014_RiskTypeThreat_MatchesSpec()
    {
        OpportunityStatementAndRisksSpec.RiskTypeThreat.Should().Be("Threat");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_015_RiskTypeOpportunity_MatchesSpec()
    {
        OpportunityStatementAndRisksSpec.RiskTypeOpportunity.Should().Be("Opportunity");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_016_RisksNavigationLabel_IsRisksNotDST()
    {
        OpportunityStatementAndRisksSpec.RisksNavigationLabel.Should().Be("Risks");
        OpportunityStatementAndRisksSpec.RisksNavigationLabel.Should().NotBe("DST");
    }

    #endregion

    #region PNO-922 — Edit Option

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_017_IRiskManager_HasUpdateRiskAsync()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("UpdateRiskAsync");
        method.Should().NotBeNull();
        method!.Name.Should().Be("UpdateRiskAsync");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_018_IRiskManager_HasCreateRiskAsync()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("CreateRiskAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_019_IRiskManager_HasDeleteRiskAsync()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("DeleteRiskAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region Opportunity Workflow

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_020_OpportunityWorkflow_EntityName_IsOpportunity()
    {
        OpportunityWorkflow.EntityName.Should().Be("Opportunity");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_021_OpportunityWorkflow_Stages_IncludeIdentifyAndProfile()
    {
        OpportunityWorkflow.Stages.IdentifyAndProfile.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_022_OpportunityWorkflow_Stages_IncludeGo()
    {
        OpportunityWorkflow.Stages.Go.Should().Be("GO");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_023_OpportunityWorkflow_IsValidStage_AcceptsGo()
    {
        OpportunityWorkflow.IsValidStage("GO").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_024_OpportunityWorkflow_AllStages_HasFourStages()
    {
        OpportunityWorkflow.AllStages.Should().HaveCount(4);
    }

    #endregion

    #region API Endpoints

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_025_GenerateStatementPdfEndpoint_MatchesSpec()
    {
        OpportunityStatementAndRisksSpec.GenerateStatementPdfEndpoint.Should().Contain("generate-statement-pdf");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_026_GenerateStatementEndpoint_IncludesId()
    {
        var endpoint = OpportunityStatementAndRisksSpec.GenerateStatementEndpoint(99);
        endpoint.Should().Contain("99");
        endpoint.Should().Contain("generate-statement");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_027_DstRisksEndpoint_IncludesOpportunityId()
    {
        var endpoint = OpportunityStatementAndRisksSpec.DstRisksEndpoint(42);
        endpoint.Should().Contain("42");
        endpoint.Should().Contain("dst-risks");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_028_UpdateRiskEndpoint_IncludesOpportunityAndRiskId()
    {
        var endpoint = OpportunityStatementAndRisksSpec.UpdateRiskEndpoint(10, 5);
        endpoint.Should().Contain("10");
        endpoint.Should().Contain("5");
        endpoint.Should().Contain("dst-risks");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_029_HighRiskAnalysisEndpoint_IncludesOpportunityId()
    {
        var endpoint = OpportunityStatementAndRisksSpec.HighRiskAnalysisEndpoint(7);
        endpoint.Should().Contain("7");
        endpoint.Should().Contain("high-risk-analysis");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void POS_030_AcknowledgeHighRisksEndpoint_MatchesSpec()
    {
        var endpoint = OpportunityStatementAndRisksSpec.AcknowledgeHighRisksEndpoint(15);
        endpoint.Should().Contain("15");
        endpoint.Should().Contain("acknowledge-high-risks");
    }

    #endregion
}
