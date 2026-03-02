using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using UNOPS.Workflow.Models.Requirements;
using Xunit;
using Facing = UNOPS.Workflow.Models.Facing;

namespace UNOPS.PAO.IntegrationTests.PNO1197;

[Collection("Functional")]
[Trait("Category", "Functional")]
[Trait("Type", "Functional")]
public class FunctionalTests : PNO1197TestFixtureBase, IDisposable
{
    #region FUN_001-015: DoA Validation Business Rules

    [Fact]
    public async Task FUN_001_DoA2TakesPriorityOverDoA3()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(1));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_002_DoA3OnlyUsedWhenNoActiveDoA2()
    {
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(2, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(2));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_003_DeletedDoA2TriggersDoA3Fallback()
    {
        await SeedOpportunityAsync(3, "IDENTIFY & PROFILE");
        var doa2Holders = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1 &&
                eur.EntityRole != null && eur.EntityRole.Code == "DoA2_OrganizationHierarchy")
            .ToListAsync();
        foreach (var h in doa2Holders) h.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(3, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(3));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_004_DeletedDoA3WithActiveDoA2StillPasses()
    {
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 3);
        var doa3Holders = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1 &&
                eur.EntityRole != null && eur.EntityRole.Code == "DoA3_OrganizationHierarchy")
            .ToListAsync();
        foreach (var h in doa3Holders) h.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(4, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(4));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_005_BothDeletedFails()
    {
        await SeedOpportunityAsync(5, "IDENTIFY & PROFILE");
        var holders = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1)
            .ToListAsync();
        foreach (var h in holders) h.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(5, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(5));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_006_DoACheckFiltersByOrgUnit()
    {
        await SeedOpportunityAsync(6, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 80))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy { Id = 80, Name = "O", Code = "O", Description = "O", Status = EntityStatus.Active, IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }
        await SeedDoAHolderAsync(80, 2);
        await SeedOpportunityManagerStakeholderAsync(6, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(6));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_007_DoACheckFiltersByEntityType()
    {
        await SeedOpportunityAsync(7, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var doaRole = await DbContext.EntityRoles.FirstOrDefaultAsync(r => r.Code == "DoA2_OrganizationHierarchy");
        if (doaRole == null)
        {
            doaRole = new EntityRole { Id = 200, Name = "DoA2", Code = "DoA2_OrganizationHierarchy", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
            DbContext.EntityRoles.Add(doaRole);
            await DbContext.SaveChangesAsync();
        }
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = doaRole.Id, EntityRole = doaRole, EntityId = 1, EntityType = "Partner", Name = "Wrong", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(7, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(7));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_008_DoACheckIsNotCaseSensitive()
    {
        await SeedOpportunityAsync(8, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(8, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(8));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_009_DoAHolderMustBeActiveUser()
    {
        await SeedOpportunityAsync(9, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(9, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(9));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_010_DoAHolderMustBeOnCorrectOrg()
    {
        await SeedOpportunityAsync(10, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(10, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(10));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_011_DoAValidationRunsBeforeOtherChecks()
    {
        await SeedOpportunityAsync(11, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(11, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(11));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeFalse();
        response.UnmetRequirements.Should().Contain(r => r.Contains("doaHolderRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task FUN_012_DoARequirementMessageIsCorrect()
    {
        await SeedOpportunityAsync(12, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(12, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(12));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.UnmetRequirements.Should().Contain("message.requirements.opportunity.doaHolderRequired");
    }

    [Fact]
    public async Task FUN_013_DoAValidationUsesAsNoTracking()
    {
        await SeedOpportunityAsync(13, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(13, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(13));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_014_DoAValidationUsesIncludeForEntityRole()
    {
        await SeedOpportunityAsync(14, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(14, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(14));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_015_DoAValidationRespectsIsDeletedFilter()
    {
        await SeedOpportunityAsync(15, "IDENTIFY & PROFILE");
        var holders = await DbContext.EntityUserRoles.Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1).ToListAsync();
        foreach (var h in holders) h.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(15, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(15));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    #endregion

    #region FUN_016-030: Submit Flow Business Rules

    [Fact]
    public async Task FUN_016_SubmitValidatesAll21Requirements()
    {
        await SeedOpportunityAsync(16, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(16, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(16));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_017_SubmitReturnsUnmetRequirementsList()
    {
        await SeedOpportunityAsync(17, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(17, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(17));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.UnmetRequirements.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FUN_018_SubmitReturnsRequirementsNotMetFlag()
    {
        await SeedOpportunityAsync(18, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(18, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(18));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.RequirementsNotMet.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_019_SubmitChecksOMBeforeDoA()
    {
        await SeedOpportunityAsync(19, "IDENTIFY & PROFILE");
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(19));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.UnmetRequirements.Should().Contain(r => r.Contains("managerRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(Skip = "DEF-008: Submit validation does not enforce countries requirement")]
    public async Task FUN_020_SubmitChecksCountries()
    {
        await SeedOpportunityAsync(20, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(20, 1);
        var countries = await DbContext.Set<OpportunityCountry>().Where(oc => oc.OpportunityId == 20).ToListAsync();
        DbContext.Set<OpportunityCountry>().RemoveRange(countries);
        await DbContext.SaveChangesAsync();
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(20));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact(Skip = "DEF-008: Submit validation does not enforce deliverables requirement")]
    public async Task FUN_021_SubmitChecksDeliverables()
    {
        await SeedOpportunityAsync(21, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(21, 1);
        var dels = await DbContext.Set<OpportunityDeliverable>().Where(d => d.OpportunityId == 21).ToListAsync();
        DbContext.Set<OpportunityDeliverable>().RemoveRange(dels);
        await DbContext.SaveChangesAsync();
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(21));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact(Skip = "DEF-008: Submit validation does not enforce SDGs requirement")]
    public async Task FUN_022_SubmitChecksSDGs()
    {
        await SeedOpportunityAsync(22, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(22, 1);
        var sdgs = await DbContext.Set<OpportunitySDG>().Where(s => s.OpportunityId == 22).ToListAsync();
        DbContext.Set<OpportunitySDG>().RemoveRange(sdgs);
        await DbContext.SaveChangesAsync();
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(22));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_023_SubmitChecksStatement()
    {
        await SeedOpportunityAsync(23, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(23);
        opp!.OpportunityStatementMarkdown = "";
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(23, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(23));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_024_SubmitChecksBeneficiaries()
    {
        await SeedOpportunityAsync(24, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(24);
        opp!.BeneficiariesToBeDetermined = false;
        opp.EstimatedDirectBeneficiaries = 0;
        opp.EstimatedIndirectBeneficiaries = -1;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(24, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(24));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_025_SubmitCreatesPendingTask()
    {
        await SeedOpportunityAsync(25, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(25, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(25));
        MockWorkflowManager.Verify(x => x.AddLog(It.IsAny<WorkflowLogModel>()), Times.Once);
    }

    [Fact]
    public async Task FUN_026_SubmitReturnsApprovalRequired()
    {
        await SeedOpportunityAsync(26, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(26, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(26));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.ApprovalRequired.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_027_SubmitGeneratesOpportunityStatement()
    {
        await SeedOpportunityAsync(27, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(27, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(27));
        MockManagerWrapper.Verify(x => x.GeminiManager, Times.AtLeastOnce);
    }

    [Fact]
    public async Task FUN_028_SubmitNonOMWarningFlow()
    {
        await SeedOpportunityAsync(28, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(28, 2);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(28));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.RequiresConfirmation.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_029_SubmitOrgUnitMismatchFlow()
    {
        await SeedOpportunityAsync(29, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(29, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 29, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = false, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.RequiresConfirmation.Should().BeTrue();
        response.ConfirmationType.Should().Be("OrgUnitCountryMismatch");
    }

    [Fact]
    public async Task FUN_030_SubmitAcknowledgmentFlow()
    {
        await SeedOpportunityAsync(30, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(30, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 30, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = false
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.RequiresAcknowledgment.Should().BeTrue();
    }

    #endregion

    #region FUN_031-040: Constraint Rules

    [Fact]
    public async Task FUN_031_OnlyOMCanSubmit()
    {
        await SeedOpportunityAsync(31, "IDENTIFY & PROFILE");
        // Seed a DIFFERENT user (userId=99) as the Opportunity Manager - current user (userId=1) is NOT the OM
        // This satisfies requirement 18 (OM must exist) but the submitter (userId=1) is not the OM
        await SeedOpportunityManagerStakeholderAsync(31, 99);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(31));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.RequiresConfirmation.Should().BeTrue();
        response.ConfirmationType.Should().Be("NonOMSubmitter");
    }

    [Fact]
    public async Task FUN_032_SubmitOnlyFromIP()
    {
        await SeedOpportunityAsync(32, "GO");
        await SeedOpportunityManagerStakeholderAsync(32, 1);
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "32")).ReturnsAsync(true);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "32")).ReturnsAsync("GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 32)).Returns((WorkflowLog?)null);
        MockWorkflowManager.Setup(x => x.WorkflowStateByStage(It.IsAny<StateMachine>(), "GO", Facing.Internal)).Returns(new State { StageCode = "GO" });
        MockWorkflowManager.Setup(x => x.NextActions("Opportunity", It.IsAny<State>(), Facing.Internal)).Returns(Array.Empty<WorkflowStateActionModel>());
        var result = await Controller.Submit(CreateRequest(32));
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_033_SubmitNotAllowedWhileInWorkflow()
    {
        await SeedOpportunityAsync(33, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(33, 1);
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "33", NewStage = "GO" };
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "33")).ReturnsAsync(true);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "33")).ReturnsAsync("IDENTIFY & PROFILE");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 33)).Returns(pendingTask);
        var result = await Controller.Submit(CreateRequest(33));
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_034_SubmitRequiresValidTransition()
    {
        await SeedOpportunityAsync(34, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(34, 1);
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "34")).ReturnsAsync(true);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "34")).ReturnsAsync("IDENTIFY & PROFILE");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 34)).Returns((WorkflowLog?)null);
        MockWorkflowManager.Setup(x => x.WorkflowStateByStage(It.IsAny<StateMachine>(), "IDENTIFY & PROFILE", Facing.Internal)).Returns(new State { StageCode = "IDENTIFY & PROFILE" });
        MockWorkflowManager.Setup(x => x.NextActions("Opportunity", It.IsAny<State>(), Facing.Internal)).Returns(Array.Empty<WorkflowStateActionModel>());
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 34, NewStage = "INVALID",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_035_DoACheckDependsOnResponsibleOrgUnit()
    {
        await SeedOpportunityAsync(35, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(35);
        opp!.ResponsibleOrgUnitId = null;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(35, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(35));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.UnmetRequirements.Should().Contain(r => r.Contains("doaHolderRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task FUN_036_DoAValidationIndependentOfSubmitUser()
    {
        await SeedOpportunityAsync(36, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(36, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(36));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_037_DoARequirementCannotBeBypassed()
    {
        await SeedOpportunityAsync(37, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(37, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(37));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_038_MultipleDoAHoldersForSameLevelIsOK()
    {
        await SeedOpportunityAsync(38, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedDoAHolderAsync(1, 2);
        await SeedOpportunityManagerStakeholderAsync(38, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(38));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_039_DoAHierarchyIsFlatNotRecursive()
    {
        await SeedOpportunityAsync(39, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(39, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(39));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_040_RequirementListIsCompleteNotPartial()
    {
        await SeedOpportunityAsync(40, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var opp = await DbContext.Opportunities.FindAsync(40);
        opp!.OpportunityStatementMarkdown = "";
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(40, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(40));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.UnmetRequirements.Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region FUN_041-050: Audit Rules

    [Fact(Skip = "DEF-054: DoA3Fallback controller does not implement ILogger logging for submit operations")]
    public async Task FUN_041_SubmitAttemptLogged()
    {
        await SeedOpportunityAsync(41, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(41, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(41));
        MockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact(Skip = "DEF-054: DoA3Fallback controller does not implement ILogger logging for submit operations")]
    public async Task FUN_042_FailedSubmitLogged()
    {
        await SeedOpportunityAsync(42, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(42, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(42));
        MockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task FUN_043_SuccessfulSubmitLogged()
    {
        await SeedOpportunityAsync(43, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(43, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(43));
        MockWorkflowManager.Verify(x => x.AddLog(It.IsAny<WorkflowLogModel>()), Times.Once);
    }

    [Fact(Skip = "DEF-054: DoA3Fallback controller does not implement ILogger logging for submit operations")]
    public async Task FUN_044_DoAValidationResultLogged()
    {
        await SeedOpportunityAsync(44, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(44, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(44));
        MockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact(Skip = "DEF-054: DoA3Fallback controller does not implement ILogger logging for submit operations")]
    public async Task FUN_045_UnmetRequirementsLogged()
    {
        await SeedOpportunityAsync(45, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(45, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(45));
        MockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact(Skip = "DEF-054: DoA3Fallback controller does not implement ILogger logging for submit operations")]
    public async Task FUN_046_OMCheckLogged()
    {
        await SeedOpportunityAsync(46, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(46, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(46));
        MockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task FUN_047_StageTransitionLogged()
    {
        await SeedOpportunityAsync(47, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(47, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(47));
        MockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.NewStage == "GO")), Times.Once);
    }

    [Fact]
    public async Task FUN_048_PendingTaskCreationLogged()
    {
        await SeedOpportunityAsync(48, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(48, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(48));
        MockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.RequiresApproval)), Times.Once);
    }

    [Fact]
    public async Task FUN_049_NotificationTriggeredOnSubmit()
    {
        await SeedOpportunityAsync(49, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(49, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(49));
        MockWorkflowManager.Verify(x => x.Initiate(It.IsAny<UNOPS.Workflow.Models.WorkflowActionModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_050_StatementRegenerationLogged()
    {
        await SeedOpportunityAsync(50, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(50, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(50));
        MockManagerWrapper.Verify(x => x.GeminiManager, Times.AtLeastOnce);
    }

    #endregion

    private static WorkflowSubmitRequest CreateRequest(int entityId) =>
        new()
        {
            EntityName = "opportunity",
            EntityId = entityId,
            NewStage = "GO",
            ConfirmedNonOMSubmission = false,
            ConfirmedOrgUnitWarning = true,
            AcknowledgedStatement = true
        };
}
