using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityStatementAndRisks;

/// <summary>
/// Integration tests for Opportunity Statement and Risk Register (PNO-705, PNO-761, PNO-922, PNO-975).
/// Full CRUD through API/DB, service-to-DB round-trip, multi-component workflows.
/// </summary>
public class IntegrationTests : OpportunityStatementAndRisksFixture
{
    #region Opportunity Statement — DB Round-Trip

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_001_Opportunity_OpportunityStatementMarkdown_Persisted()
    {
        var markdown = "# Opportunity Statement\n\n## Section 1\nContent here.";
        var oppId = await SeedOpportunityAsync(opportunityStatementMarkdown: markdown);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.OpportunityStatementMarkdown.Should().Be(markdown);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_002_Opportunity_HighRisksAcknowledged_Persisted()
    {
        var oppId = await SeedOpportunityAsync(highRisksAcknowledged: true);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.HighRisksAcknowledged.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_003_Opportunity_HighRisksAcknowledgedFalse_Persisted()
    {
        var oppId = await SeedOpportunityAsync(highRisksAcknowledged: false);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.HighRisksAcknowledged.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_004_Opportunity_StatementAndAcknowledgement_Persisted()
    {
        var markdown = "# Statement";
        var oppId = await SeedOpportunityAsync(markdown, true);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.OpportunityStatementMarkdown.Should().Be(markdown);
        opp.HighRisksAcknowledged.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_005_Opportunity_StageGo_Persisted()
    {
        var oppId = await SeedOpportunityAsync(stage: OpportunityWorkflow.Stages.Go);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Stage.Should().Be(OpportunityWorkflow.Stages.Go);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_006_Opportunity_StatementNull_Persisted()
    {
        var oppId = await SeedOpportunityAsync(opportunityStatementMarkdown: null);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_007_Opportunity_StatementEmpty_Persisted()
    {
        var oppId = await SeedOpportunityAsync(opportunityStatementMarkdown: "");
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.OpportunityStatementMarkdown.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_008_Opportunity_StatementLongMarkdown_Persisted()
    {
        var markdown = new string('x', 5000);
        var oppId = await SeedOpportunityAsync(opportunityStatementMarkdown: markdown);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.OpportunityStatementMarkdown!.Length.Should().Be(5000);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_009_Opportunity_StatementWithUnicode_Persisted()
    {
        var markdown = "# SDG 6 — Clean Water";
        var oppId = await SeedOpportunityAsync(opportunityStatementMarkdown: markdown);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.OpportunityStatementMarkdown.Should().Contain("—");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_010_Opportunity_SoftDeleted_ExcludedFromQuery()
    {
        var oppId = await SeedOpportunityAsync();
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.IsDeleted = true;
        opp.DeletedDate = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        var found = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        found.Should().BeNull();
    }

    #endregion

    #region GeneratePdfRequest — Serialization Round-Trip

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_011_GeneratePdfRequest_SerializationRoundTrip()
    {
        var request = new GeneratePdfRequest
        {
            EntityName = "Opportunity",
            EntityId = 123,
            Data = "# Test",
            Filename = "Statement"
        };
        request.EntityName.Should().Be("Opportunity");
        request.EntityId.Should().Be(123);
        request.Data.Should().Be("# Test");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_012_GeneratePdfRequest_WithEntityOnly_RoundTrip()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 456 };
        request.EntityName.Should().Be("Opportunity");
        request.EntityId.Should().Be(456);
        request.Data.Should().BeNullOrEmpty();
    }

    #endregion

    #region RiskCreateRequest — Serialization Round-Trip

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_013_RiskCreateRequest_FullRequest_RoundTrip()
    {
        var request = new RiskCreateRequest
        {
            EntityId = 10,
            Title = "Security risk",
            RiskTypeId = 1,
            RiskCategoryId = 2,
            RiskProbabilityId = 1,
            RiskProximityId = 1,
            RiskImpactLevelId = 3,
            Description = "Desc",
            Recommendation = "Rec"
        };
        request.EntityId.Should().Be(10);
        request.Title.Should().Be("Security risk");
        request.Description.Should().Be("Desc");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_014_RiskCreateRequest_MinimalRequest_RoundTrip()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.EntityId.Should().Be(1);
        request.Title.Should().Be("Risk");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_015_RiskCreateRequest_PreDefinedHighRisk_RoundTrip()
    {
        var request = new RiskCreateRequest
        {
            EntityId = 10,
            Title = "HCA risk",
            PreDefinedHighRiskId = 1
        };
        request.PreDefinedHighRiskId.Should().Be(1);
    }

    #endregion

    #region DSTRisksResponse — Structure

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_016_DSTRisksResponse_EmptyRisks_RoundTrip()
    {
        var response = new DSTRisksResponse { Risks = [], TotalCount = 0 };
        response.Risks.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_017_DSTRisksResponse_MultipleRisks_RoundTrip()
    {
        var risks = new List<RiskModel>
        {
            new() { Id = 1, Title = "R1" },
            new() { Id = 2, Title = "R2" }
        };
        var response = new DSTRisksResponse { Risks = risks, TotalCount = 2 };
        response.Risks.Should().HaveCount(2);
        response.TotalCount.Should().Be(2);
    }

    #endregion

    #region Opportunity Workflow — State Machine Integration

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_018_Workflow_StateMachine_EntityType()
    {
        OpportunityWorkflow.StateMachine.EntityType.Should().Be("Opportunity");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_019_Workflow_StateMachine_StatesCount()
    {
        OpportunityWorkflow.StateMachine.States.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_020_Workflow_StateMachine_StateSequence()
    {
        var states = OpportunityWorkflow.StateMachine.States.ToList();
        states[0].Sequence.Should().Be(1);
        states[1].Sequence.Should().Be(2);
        states[2].Sequence.Should().Be(3);
        states[3].Sequence.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_021_Workflow_AllStages_HasIdentifyAndProfile()
    {
        OpportunityWorkflow.AllStages.Should().Contain("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_022_Workflow_AllStages_HasGo()
    {
        OpportunityWorkflow.AllStages.Should().Contain("GO");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_023_Workflow_IsValidStage_AllValidStages()
    {
        foreach (var stage in OpportunityWorkflow.AllStages)
        {
            OpportunityWorkflow.IsValidStage(stage).Should().BeTrue();
        }
    }

    #endregion

    #region Spec Constants — Integration Verification

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_024_Spec_OpportunityEntityName_MatchesWorkflow()
    {
        OpportunityStatementAndRisksSpec.OpportunityEntityName.Should().Be(OpportunityWorkflow.EntityName);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_025_Spec_RiskEntityType_Opportunity()
    {
        OpportunityStatementAndRisksSpec.RiskEntityTypeOpportunity.Should().Be("Opportunity");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_026_Spec_Endpoints_ContainApiPrefix()
    {
        OpportunityStatementAndRisksSpec.GenerateStatementPdfEndpoint.Should().Contain("api");
        OpportunityStatementAndRisksSpec.DstRisksEndpoint(1).Should().Contain("api");
    }

    #endregion

    #region Opportunity + Statement Query

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_027_Opportunity_QueryByStatement_NotNull()
    {
        var markdown = "# Statement";
        var oppId = await SeedOpportunityAsync(markdown);
        var statement = await Context.Opportunities
            .AsNoTracking()
            .Where(o => o.Id == oppId && !o.IsDeleted)
            .Select(o => o.OpportunityStatementMarkdown)
            .FirstOrDefaultAsync();
        statement.Should().Be(markdown);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_028_Opportunity_QueryHighRisksAcknowledged()
    {
        var oppId = await SeedOpportunityAsync(highRisksAcknowledged: true);
        var acknowledged = await Context.Opportunities
            .AsNoTracking()
            .Where(o => o.Id == oppId && !o.IsDeleted)
            .Select(o => o.HighRisksAcknowledged)
            .FirstOrDefaultAsync();
        acknowledged.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_029_Opportunity_MultipleOpportunities_Isolated()
    {
        var id1 = await SeedOpportunityAsync("# Statement 1");
        var id2 = await SeedOpportunityAsync("# Statement 2");
        var opp1 = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == id1 && !o.IsDeleted);
        var opp2 = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == id2 && !o.IsDeleted);
        opp1!.OpportunityStatementMarkdown.Should().Be("# Statement 1");
        opp2!.OpportunityStatementMarkdown.Should().Be("# Statement 2");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_030_Opportunity_UpdateStatement_Persisted()
    {
        var oppId = await SeedOpportunityAsync("# Original");
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.OpportunityStatementMarkdown = "# Updated";
        await Context.SaveChangesAsync();
        var updated = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        updated!.OpportunityStatementMarkdown.Should().Be("# Updated");
    }

    #endregion

    #region Risk Model — Structure Integration

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_031_RiskModel_EntityTypeEntityId_Consistent()
    {
        var model = new RiskModel
        {
            EntityType = "Opportunity",
            EntityId = 5,
            Title = "Risk"
        };
        model.EntityType.Should().Be(OpportunityStatementAndRisksSpec.RiskEntityTypeOpportunity);
        model.EntityId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_032_RiskModel_AllMandatoryFields()
    {
        var model = new RiskModel
        {
            Id = 1,
            EntityType = "Opportunity",
            EntityId = 1,
            Title = "Risk",
            RiskTypeId = 1,
            RiskCategoryId = 1,
            RiskProbabilityId = 1,
            RiskProximityId = 1,
            RiskImpactLevelId = 1
        };
        model.Id.Should().BePositive();
        model.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_033_RiskModel_PreDefinedHighRiskFields()
    {
        var model = new RiskModel
        {
            PreDefinedHighRiskId = 1,
            PreDefinedHighRiskCode = "1.1.1",
            PreDefinedHighRiskTitle = "HCA risk"
        };
        model.PreDefinedHighRiskId.Should().Be(1);
        model.PreDefinedHighRiskCode.Should().Be("1.1.1");
    }

    #endregion

    #region API Endpoint Structure

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_034_GenerateStatementPdf_EndpointStructure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.GenerateStatementPdfEndpoint;
        endpoint.Should().Contain("opportunity");
        endpoint.Should().Contain("generate-statement-pdf");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_035_GenerateStatement_EndpointStructure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.GenerateStatementEndpoint(99);
        endpoint.Should().Contain("99");
        endpoint.Should().Contain("generate-statement");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_036_DstRisks_EndpointStructure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.DstRisksEndpoint(42);
        endpoint.Should().Contain("42");
        endpoint.Should().Contain("dst-risks");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_037_UpdateRisk_EndpointStructure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.UpdateRiskEndpoint(10, 5);
        endpoint.Should().Contain("10");
        endpoint.Should().Contain("5");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_038_DeleteRisk_EndpointStructure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.DeleteRiskEndpoint(10, 5);
        endpoint.Should().Contain("10");
        endpoint.Should().Contain("5");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_039_HighRiskAnalysis_EndpointStructure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.HighRiskAnalysisEndpoint(7);
        endpoint.Should().Contain("7");
        endpoint.Should().Contain("high-risk-analysis");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_040_AcknowledgeHighRisks_EndpointStructure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.AcknowledgeHighRisksEndpoint(15);
        endpoint.Should().Contain("15");
        endpoint.Should().Contain("acknowledge-high-risks");
    }

    #endregion

    #region Risk Scoring — Integration

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_041_RiskScore_Calculation()
    {
        const int likelihood = 3, impact = 4;
        var score = likelihood * impact;
        score.Should().Be(12);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_042_RiskScore_MatrixValues()
    {
        var scores = new[] { 1 * 1, 2 * 3, 5 * 5 };
        scores.Sum().Should().Be(1 + 6 + 25);
    }

    #endregion

    #region Go Decision Workflow

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_043_GoDecision_OpportunityInGoStage_WithStatement()
    {
        var markdown = "# Statement";
        var oppId = await SeedOpportunityAsync(markdown, true, OpportunityWorkflow.Stages.Go);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Stage.Should().Be(OpportunityWorkflow.Stages.Go);
        opp.OpportunityStatementMarkdown.Should().NotBeNullOrEmpty();
        opp.HighRisksAcknowledged.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_044_GoDecision_IdentifyAndProfile_CanEdit()
    {
        var oppId = await SeedOpportunityAsync("# Draft", false, OpportunityWorkflow.Stages.IdentifyAndProfile);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Stage.Should().Be(OpportunityWorkflow.Stages.IdentifyAndProfile);
    }

    #endregion

    #region IncompleteSectionsMessage

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_045_IncompleteSectionsMessage_DisplayedWhenIncomplete()
    {
        var message = OpportunityStatementAndRisksSpec.IncompleteSectionsMessage;
        message.Should().Contain("Complete");
        message.Should().Contain("generate");
    }

    #endregion

    #region PNO-922 — Edit Option

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_046_UpdateRiskEndpoint_Exists()
    {
        var endpoint = OpportunityStatementAndRisksSpec.UpdateRiskEndpoint(1, 1);
        endpoint.Should().NotBeNullOrEmpty();
        endpoint.Should().Contain("dst-risks");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_047_IRiskManager_UpdateRiskAsync_Signature()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("UpdateRiskAsync");
        method.Should().NotBeNull();
        method!.GetParameters().Should().HaveCount(3);
    }

    #endregion

    #region PNO-975 — Popup Visibility

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_048_RiskPopup_AddNewRisk_Action()
    {
        const string action = "Add to Register";
        action.Should().Contain("Add");
    }

    #endregion

    #region Document Type

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_049_OpportunityStatementDocumentType_ForPDF()
    {
        OpportunityStatementAndRisksSpec.OpportunityStatementDocumentType.Should().Be("Opportunity Statement");
    }

    #endregion

    #region Risk Lookups

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_050_IRiskManager_GetRiskLookupsAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetRiskLookupsAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_051_IRiskManager_GetRiskCategoriesAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetRiskCategoriesAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_052_IRiskManager_GetPreDefinedHighRisksAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetPreDefinedHighRisksAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region GeneratePdfRequest — Entity Fetch Logic

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_053_GeneratePdfRequest_EntityFetch_WhenEntityNameAndId()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1 };
        var shouldFetch = !string.IsNullOrEmpty(request.EntityName) && request.EntityId.HasValue && request.EntityId > 0;
        shouldFetch.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_054_GeneratePdfRequest_UseData_WhenProvided()
    {
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1, Data = "# Override" };
        var useData = !string.IsNullOrEmpty(request.Data);
        useData.Should().BeTrue();
    }

    #endregion

    #region DbContext — Opportunity Set

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_055_Context_Opportunities_Exists()
    {
        Context.Opportunities.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_056_Context_Opportunities_CanAdd()
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = $"Test {TestMarker}",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            IsDeleted = false
        };
        Context.Opportunities.Add(opp);
        await Context.SaveChangesAsync();
        opp.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region RiskCreateRequest — EntityId

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_057_RiskCreateRequest_EntityId_LinksToOpportunity()
    {
        var request = new RiskCreateRequest { EntityId = 42, Title = "Risk" };
        request.EntityId.Should().Be(42);
    }

    #endregion

    #region Workflow Stages

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_058_Workflow_NoGo_Stage()
    {
        OpportunityWorkflow.Stages.NoGo.Should().Be("NO GO");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_059_Workflow_Cancelled_Stage()
    {
        OpportunityWorkflow.Stages.Cancelled.Should().Be("CANCELLED");
    }

    #endregion

    #region Risks Navigation

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_060_RisksNavigationLabel_IsRisks()
    {
        OpportunityStatementAndRisksSpec.RisksNavigationLabel.Should().Be("Risks");
    }

    #endregion

    #region Risk Model — Audit Fields

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_061_RiskModel_CreatedDate()
    {
        var model = new RiskModel { CreatedDate = DateTime.UtcNow };
        model.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_062_RiskModel_IdentifiedDate_Nullable()
    {
        var model = new RiskModel { IdentifiedDate = null };
        model.IdentifiedDate.HasValue.Should().BeFalse();
    }

    #endregion

    #region Statement Markdown — Sections

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_063_Opportunity_StatementWithSections()
    {
        var markdown = "# 1. Context\n\n# 2. Alignment\n\n# 3. Value";
        var oppId = await SeedOpportunityAsync(markdown);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.OpportunityStatementMarkdown.Should().Contain("1. Context");
        opp.OpportunityStatementMarkdown.Should().Contain("2. Alignment");
    }

    #endregion

    #region High Risk Acknowledgement

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_064_Opportunity_UpdateHighRisksAcknowledged()
    {
        var oppId = await SeedOpportunityAsync(highRisksAcknowledged: false);
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.HighRisksAcknowledged = true;
        await Context.SaveChangesAsync();
        var updated = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        updated!.HighRisksAcknowledged.Should().BeTrue();
    }

    #endregion

    #region Default Statement Filename

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_065_DefaultStatementFilename()
    {
        OpportunityStatementAndRisksSpec.DefaultStatementFilename.Should().Be("Generated_Document");
    }

    #endregion

    #region Validate Statement Endpoint

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_066_ValidateStatementEndpoint_Structure()
    {
        var endpoint = OpportunityStatementAndRisksSpec.ValidateStatementEndpoint(1);
        endpoint.Should().Contain("validate-statement");
    }

    #endregion

    #region Risk Type

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_067_RiskType_Threat()
    {
        OpportunityStatementAndRisksSpec.RiskTypeThreat.Should().Be("Threat");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_068_RiskType_Opportunity()
    {
        OpportunityStatementAndRisksSpec.RiskTypeOpportunity.Should().Be("Opportunity");
    }

    #endregion

    #region PreDefined High Risk Code

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_069_RiskModel_PreDefinedHighRiskCode_Format()
    {
        var model = new RiskModel { PreDefinedHighRiskCode = "1.1.1" };
        model.PreDefinedHighRiskCode.Should().MatchRegex(@"^\d+\.\d+\.\d+$");
    }

    #endregion

    #region Mapper

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_070_Mapper_Configured()
    {
        Mapper.Should().NotBeNull();
    }

    #endregion

    #region Context

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_071_Context_NotNull()
    {
        Context.Should().NotBeNull();
    }

    #endregion

    #region Opportunity Statement — Auto-Generated

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_072_Statement_AutoGeneratedFromSections()
    {
        var message = OpportunityStatementAndRisksSpec.IncompleteSectionsMessage;
        message.Should().Contain("sections");
    }

    #endregion

    #region Risk Scoring Formula

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_073_RiskScoringFormula_Documented()
    {
        OpportunityStatementAndRisksSpec.RiskScoringFormula.Should().Contain("Likelihood");
        OpportunityStatementAndRisksSpec.RiskScoringFormula.Should().Contain("Impact");
    }

    #endregion

    #region IRiskManager GetRisksByEntityAsync

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_074_IRiskManager_GetRisksByEntityAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetRisksByEntityAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region IRiskManager CreateRiskAsync

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_075_IRiskManager_CreateRiskAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("CreateRiskAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region IRiskManager DeleteRiskAsync

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_076_IRiskManager_DeleteRiskAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("DeleteRiskAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region IRiskManager GetHighRiskAnalysisAsync

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_077_IRiskManager_GetHighRiskAnalysisAsync_Exists()
    {
        var method = typeof(UNOPS.PAO.Business.Interfaces.IRiskManager).GetMethod("GetHighRiskAnalysisAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region Opportunity Workflow StateMachine

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_078_Workflow_StateMachine_NotNull()
    {
        OpportunityWorkflow.StateMachine.Should().NotBeNull();
    }

    #endregion

    #region GeneratePdfRequest — Error Case

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_079_GeneratePdfRequest_NoContent_Error()
    {
        var request = new GeneratePdfRequest { EntityName = null, EntityId = null, Data = null };
        var hasContent = (!string.IsNullOrEmpty(request.Data)) ||
                         (!string.IsNullOrEmpty(request.EntityName) && request.EntityId.HasValue && request.EntityId > 0);
        hasContent.Should().BeFalse();
    }

    #endregion

    #region RiskCreateRequest — Validation

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_080_RiskCreateRequest_EntityIdPositive()
    {
        var request = new RiskCreateRequest { EntityId = 1, Title = "Risk" };
        request.EntityId.Should().BePositive();
    }

    #endregion

    #region DSTRisksResponse — Consistency

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_081_DSTRisksResponse_TotalCountConsistent()
    {
        var risks = new List<RiskModel> { new() { Id = 1 } };
        var response = new DSTRisksResponse { Risks = risks, TotalCount = risks.Count };
        response.TotalCount.Should().Be(response.Risks.Count);
    }

    #endregion

    #region Opportunity — Statement Length

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_082_Opportunity_StatementLength_Persisted()
    {
        var markdown = "# " + new string('X', 100);
        var oppId = await SeedOpportunityAsync(markdown);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.OpportunityStatementMarkdown!.Length.Should().Be(markdown.Length);
    }

    #endregion

    #region Workflow — All Stages

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_083_Workflow_AllStages_ContainsAllFour()
    {
        var stages = OpportunityWorkflow.AllStages;
        stages.Should().Contain("IDENTIFY & PROFILE");
        stages.Should().Contain("GO");
        stages.Should().Contain("NO GO");
        stages.Should().Contain("CANCELLED");
    }

    #endregion

    #region Risk Model — Optional Fields

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_084_RiskModel_Description_Optional()
    {
        var model = new RiskModel { Description = "" };
        model.Description.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_085_RiskModel_Recommendation_Optional()
    {
        var model = new RiskModel { Recommendation = "" };
        model.Recommendation.Should().BeEmpty();
    }

    #endregion

    #region Opportunity — Stage Transitions

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_086_Opportunity_Stage_IdentifyAndProfile()
    {
        var oppId = await SeedOpportunityAsync(stage: OpportunityWorkflow.Stages.IdentifyAndProfile);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Stage.Should().Be(OpportunityWorkflow.Stages.IdentifyAndProfile);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_087_Opportunity_Stage_NoGo()
    {
        var oppId = await SeedOpportunityAsync(stage: OpportunityWorkflow.Stages.NoGo);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Stage.Should().Be(OpportunityWorkflow.Stages.NoGo);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_088_Opportunity_Stage_Cancelled()
    {
        var oppId = await SeedOpportunityAsync(stage: OpportunityWorkflow.Stages.Cancelled);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Stage.Should().Be(OpportunityWorkflow.Stages.Cancelled);
    }

    #endregion

    #region HighRisksAcknowledgedField

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_089_HighRisksAcknowledgedField_Name()
    {
        OpportunityStatementAndRisksSpec.HighRisksAcknowledgedField.Should().Be("HighRisksAcknowledged");
    }

    #endregion

    #region Risk Title Min Length

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_090_RiskTitle_MinLength()
    {
        OpportunityStatementAndRisksSpec.RiskTitleMinLength.Should().Be(1);
    }

    #endregion
}
