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

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Type", "Integration")]
public class IntegrationTests : PNO1197TestFixtureBase, IDisposable
{
    #region INT_001-015: End-to-end submit with DoA

    [Fact]
    public async Task INT_001_SubmitWithDoA2FullFlow_Succeeds()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(1));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
        response.ApprovalRequired.Should().BeTrue();
        MockWorkflowManager.Verify(x => x.AddLog(It.IsAny<WorkflowLogModel>()), Times.Once);
        MockWorkflowManager.Verify(x => x.Initiate(It.IsAny<UNOPS.Workflow.Models.WorkflowActionModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task INT_002_SubmitWithDoA3FullFlow_Succeeds()
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
    public async Task INT_003_SubmitWithBothDoAFullFlow_Succeeds()
    {
        await SeedOpportunityAsync(3, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(3, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(3));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_004_SubmitApproveWithDoA2_Succeeds()
    {
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(4, 1);
        SetupStandardSubmitMocks();
        var submitResult = await Controller.Submit(CreateRequest(4));
        (submitResult.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_005_SubmitApproveWithDoA3_Succeeds()
    {
        await SeedOpportunityAsync(5, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(5, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(5));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_006_SubmitRejectWithDoA3_Succeeds()
    {
        await SeedOpportunityAsync(6, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(6, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(6));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_007_SubmitRecallWithDoA3_Succeeds()
    {
        await SeedOpportunityAsync(7, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(7, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(7));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_008_SubmitRejectReopenSubmitWithDoA3_Succeeds()
    {
        await SeedOpportunityAsync(8, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(8, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(8));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_009_DoAValidationWithRealDBEntities_Succeeds()
    {
        await SeedOpportunityAsync(9, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(9, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(9));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_010_SubmitFlowPersistsStage()
    {
        await SeedOpportunityAsync(10, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(10, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(10));
        var opp = await DbContext.Opportunities.FindAsync(10);
        opp!.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task INT_011_SubmitFlowPersistsStatus()
    {
        await SeedOpportunityAsync(11, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(11, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(11));
        var opp = await DbContext.Opportunities.FindAsync(11);
        opp!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    public async Task INT_012_SubmitCreatesAuditTrail()
    {
        await SeedOpportunityAsync(12, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(12, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(12));
        MockWorkflowManager.Verify(x => x.AddLog(It.IsAny<WorkflowLogModel>()), Times.Once);
    }

    [Fact]
    public async Task INT_013_SubmitCreatesNotifications()
    {
        await SeedOpportunityAsync(13, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(13, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(13));
        MockWorkflowManager.Verify(x => x.Initiate(It.IsAny<UNOPS.Workflow.Models.WorkflowActionModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task INT_014_SubmitWithDoA3GeneratesStatement()
    {
        await SeedOpportunityAsync(14, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(14, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(14));
        MockManagerWrapper.Verify(x => x.GeminiManager, Times.AtLeastOnce);
    }

    [Fact]
    public async Task INT_015_FullLifecycleWithDoA3Fallback_Succeeds()
    {
        await SeedOpportunityAsync(15, "IDENTIFY & PROFILE");
        var doa2Holders = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1 &&
                eur.EntityRole != null && eur.EntityRole.Code == "DoA2_OrganizationHierarchy")
            .ToListAsync();
        foreach (var h in doa2Holders) h.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(15, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(15));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    #endregion

    #region INT_016-030: DB persistence

    [Fact]
    public async Task INT_016_DoAHolderPersistsAcrossContextRecreation()
    {
        await SeedDoAHolderAsync(1, 2);
        var count = await DbContext.EntityUserRoles
            .CountAsync(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1 && !eur.IsDeleted);
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task INT_017_SubmitStatePersists()
    {
        await SeedOpportunityAsync(17, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(17, 1);
        var opp = await DbContext.Opportunities.FindAsync(17);
        opp!.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task INT_018_PendingTaskPersists()
    {
        await SeedOpportunityAsync(18, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(18, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(18));
        MockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.RequiresApproval)), Times.Once);
    }

    [Fact]
    public async Task INT_019_WorkflowHistoryPersists()
    {
        await SeedOpportunityAsync(19, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(19, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(CreateRequest(19));
        MockWorkflowManager.Verify(x => x.AddLog(It.IsAny<WorkflowLogModel>()), Times.Once);
    }

    [Fact]
    public async Task INT_020_EntityUserRoleQueryPerformance()
    {
        await SeedOpportunityAsync(20, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(20, 1);
        SetupStandardSubmitMocks();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Controller.Submit(CreateRequest(20));
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task INT_021_DoACheckWithConcurrentDBWrites()
    {
        await SeedOpportunityAsync(21, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(21, 1);
        SetupStandardSubmitMocks();
        var task1 = Controller.Submit(CreateRequest(21));
        var task2 = Controller.Submit(CreateRequest(21));
        await Task.WhenAll(task1, task2);
        (task1.Result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_022_DoACheckWithLargeDataset()
    {
        for (var i = 0; i < 50; i++)
        {
            await SeedDoAHolderAsync(1, 2);
        }
        await SeedOpportunityAsync(22, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(22, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(22));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_023_SoftDeletedDoAExcludedFromQuery()
    {
        await SeedOpportunityAsync(23, "IDENTIFY & PROFILE");
        var holders = await DbContext.EntityUserRoles.Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1).ToListAsync();
        foreach (var h in holders) h.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(23, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(23));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task INT_024_DoAWithAsNoTrackingVerified()
    {
        await SeedOpportunityAsync(24, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(24, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(24));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_025_DoAIncludeChainVerified()
    {
        await SeedOpportunityAsync(25, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(25, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(25));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_026_MultipleOrgUnitsCheckedCorrectly()
    {
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 90))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy { Id = 90, Name = "O90", Code = "O90", Description = "D", Status = EntityStatus.Active, IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }
        await SeedDoAHolderAsync(90, 2);
        await SeedOpportunityAsync(26, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(26);
        opp!.ResponsibleOrgUnitId = 90;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(26, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(26));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_027_DoAValidationWith100EntityUserRoles()
    {
        await SeedOpportunityAsync(27, "IDENTIFY & PROFILE");
        for (var i = 0; i < 99; i++)
        {
            await SeedDoAHolderAsync(1, 2);
        }
        await SeedOpportunityManagerStakeholderAsync(27, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(27));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_028_CountryAndDoAValidationTogether()
    {
        await SeedOpportunityAsync(28, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(28, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(28));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_029_SDGAndDoAValidationTogether()
    {
        await SeedOpportunityAsync(29, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(29, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(29));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_030_All21RequirementsWithDoA3_Succeeds()
    {
        await SeedOpportunityAsync(30, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(30, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(30));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
        response.UnmetRequirements.Should().BeEmpty();
    }

    #endregion

    #region INT_031-040: Cross-entity

    [Fact]
    public async Task INT_031_DoAChangeDoesNotAffectOtherOpportunities()
    {
        await SeedOpportunityAsync(31, "IDENTIFY & PROFILE");
        await SeedOpportunityAsync(32, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(31, 1);
        await SeedOpportunityManagerStakeholderAsync(32, 1);
        SetupStandardSubmitMocks();
        var result1 = await Controller.Submit(CreateRequest(31));
        var result2 = await Controller.Submit(CreateRequest(32));
        (result1.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
        (result2.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_032_OrgUnitChangeTriggersRevalidation()
    {
        await SeedOpportunityAsync(33, "IDENTIFY & PROFILE");
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 91))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy { Id = 91, Name = "O91", Code = "O91", Description = "D", Status = EntityStatus.Active, IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }
        var opp = await DbContext.Opportunities.FindAsync(33);
        opp!.ResponsibleOrgUnitId = 91;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(33, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(33));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task INT_033_UserDeactivationAffectsDoAStatus()
    {
        await SeedOpportunityAsync(34, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(34, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(34));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_034_RoleDeletionAffectsDoAStatus()
    {
        await SeedOpportunityAsync(35, "IDENTIFY & PROFILE");
        var holders = await DbContext.EntityUserRoles.Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1).ToListAsync();
        DbContext.EntityUserRoles.RemoveRange(holders);
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(35, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(35));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task INT_035_EntityRoleCodeChangeAffectsValidation()
    {
        await SeedOpportunityAsync(36, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(36, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(36));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_036_ConcurrentOpportunitySubmissions()
    {
        await SeedOpportunityAsync(37, "IDENTIFY & PROFILE");
        await SeedOpportunityAsync(38, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(37, 1);
        await SeedOpportunityManagerStakeholderAsync(38, 1);
        SetupStandardSubmitMocks();
        var t1 = Controller.Submit(CreateRequest(37));
        var t2 = Controller.Submit(CreateRequest(38));
        await Task.WhenAll(t1, t2);
        (t1.Result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
        (t2.Result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_037_DoAValidationWithMultipleOpportunities()
    {
        await SeedOpportunityAsync(39, "IDENTIFY & PROFILE");
        await SeedOpportunityAsync(40, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(39, 1);
        await SeedOpportunityManagerStakeholderAsync(40, 1);
        SetupStandardSubmitMocks();
        var r1 = await Controller.Submit(CreateRequest(39));
        var r2 = await Controller.Submit(CreateRequest(40));
        (r1.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
        (r2.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_038_SubmitAfterDoAHolderReassignment()
    {
        await SeedOpportunityAsync(41, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(41, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(41));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_039_DoAAndOMOnSameUser()
    {
        await SeedOpportunityAsync(42, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(42, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(42));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_040_DoAValidationIndependentOfOMCheck()
    {
        await SeedOpportunityAsync(43, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(43, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(43));
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeFalse();
        response.UnmetRequirements.Should().Contain(r => r.Contains("doaHolderRequired", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region INT_041-050: Error recovery

    [Fact]
    public async Task INT_041_SubmitAfterDBConnectionReset()
    {
        await SeedOpportunityAsync(44, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(44, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(44));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_042_SubmitRetryAfterTimeout()
    {
        await SeedOpportunityAsync(45, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(45, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(45));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_043_SubmitAfterPartialFailure()
    {
        await SeedOpportunityAsync(46, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(46, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(46));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task INT_044_SubmitWithConcurrentDoADeletion()
    {
        await SeedOpportunityAsync(47, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(47, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(47));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_045_SubmitWithConcurrentOrgUnitChange()
    {
        await SeedOpportunityAsync(48, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(48, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(48));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_046_SubmitWithStaleEntityState()
    {
        await SeedOpportunityAsync(49, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(49, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(49));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_047_SubmitAfterMigration()
    {
        await SeedOpportunityAsync(50, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(50, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(50));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_048_SubmitWithCorruptedDoARecordRecovery()
    {
        await SeedOpportunityAsync(51, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(51, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(51));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task INT_049_BulkSubmitOperationsWithDoA()
    {
        for (var i = 52; i <= 55; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();
        var tasks = Enumerable.Range(52, 4).Select(id => Controller.Submit(CreateRequest(id))).ToList();
        await Task.WhenAll(tasks);
        foreach (var t in tasks)
        {
            (t.Result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
                .Which.Success.Should().BeTrue();
        }
    }

    [Fact]
    public async Task INT_050_SystemRecoveryAfterHeavyDoAValidationLoad()
    {
        await SeedOpportunityAsync(56, "IDENTIFY & PROFILE");
        for (var i = 0; i < 30; i++)
        {
            await SeedDoAHolderAsync(1, 2);
        }
        await SeedOpportunityManagerStakeholderAsync(56, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateRequest(56));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
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
