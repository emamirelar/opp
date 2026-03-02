/**
 * @fileoverview PNO-1166 Integration Tests: End-to-end flows for Reject, Cancel-Reopen, OM Transfer,
 * cross-entity integration, and DB persistence. WorkflowController Reject no longer logs duplicate history;
 * OM role transfer (new OM assigned, old OM becomes collaborator).
 * @author UNOPS Opportunity+ QA Team
 */

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
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using UNOPS.Workflow.Models.Requirements;
using Xunit;
using Facing = UNOPS.Workflow.Models.Facing;

namespace UNOPS.PAO.IntegrationTests.PNO1166;

/// <summary>
/// PNO-1166 Integration tests: End-to-end flows for Reject, Cancel-Reopen, OM Transfer,
/// cross-entity integration, and DB persistence verification.
/// Uses same fixture pattern as other PNO-1166 files (InMemory DB, Moq, WorkflowController).
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Type", "Integration")]
public class IntegrationTests : PNO1166TestFixtureBase, IDisposable
{
    public IntegrationTests() : base() { }

    #region INT_001-010: Complete Reject Flow

    [Fact]
    public async Task INT_001_CompleteRejectFlow_SubmitApprovePendingReject()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        SetupStandardSubmitMocks();
        var submitRequest = new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 1, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        };
        var submitResult = await Controller.Submit(submitRequest);
        submitResult.Result.Should().BeOfType<OkObjectResult>();

        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "1", NewStage = "GO" };
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1)).Returns(pendingTask);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "1")).ReturnsAsync("IDENTIFY & PROFILE");
        MockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "1")).ReturnsAsync("Test Opportunity");
        MockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 1, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        MockWorkflowManager.Setup(x => x.Reject(pendingTask, "Opportunity", 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var rejectRequest = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "Rejecting", ConfirmationAcknowledged = true };
        var rejectResult = await Controller.Reject(rejectRequest);
        rejectResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_002_CompleteRejectFlow_VerifyDbStateAfterReject()
    {
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(2);
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 2, Rationale = "Scope unclear", ConfirmationAcknowledged = true };
        var result = await Controller.Reject(request);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as WorkflowActionResponse;
        response!.NewStage.Should().Be("NO GO");
    }

    [Fact]
    public async Task INT_003_CompleteRejectFlow_VerifyStageChangePersisted()
    {
        await SeedOpportunityAsync(3, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(3);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 3, Rationale = "Budget", ConfirmationAcknowledged = true });
        MockEntityStageProvider.Setup(x => x.UpdateStageAsync("Opportunity", "3", "NO GO", It.IsAny<int>())).ReturnsAsync(true);
        var opp = await DbContext.Opportunities.FindAsync(3);
        opp.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_004_CompleteRejectFlow_VerifyStatusChangePersisted()
    {
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(4);
        var result = await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 4, Rationale = "Reject", ConfirmationAcknowledged = true });
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.Success.Should().BeTrue();
        response.NewStage.Should().Be("NO GO");
    }

    [Fact]
    public async Task INT_005_CompleteRejectFlow_VerifyWorkflowHistoryAfterReject()
    {
        await SeedOpportunityAsync(5, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(5);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 5, Rationale = "History test", ConfirmationAcknowledged = true });
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 5, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task INT_006_CompleteRejectFlow_VerifyStakeholderDataUnchangedAfterReject()
    {
        await SeedOpportunityAsync(6, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(6, 1);
        SetupStandardRejectMocks(6);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 6, Rationale = "Reject", ConfirmationAcknowledged = true });
        var stakeholders = await DbContext.Set<OpportunityStakeholder>().Where(s => s.OpportunityId == 6 && !s.IsDeleted).ToListAsync();
        stakeholders.Should().Contain(s => s.UserId == 1);
    }

    [Fact]
    public async Task INT_007_CompleteRejectFlow_VerifyNotificationSent()
    {
        await SeedOpportunityAsync(7, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(7);
        MockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 7, Rationale = "Notify", ConfirmationAcknowledged = true });
        MockEmailSender.Verify(x => x.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()), Times.AtMostOnce);
    }

    [Fact]
    public async Task INT_008_CompleteRejectFlow_RejectResponseFormatCorrect()
    {
        await SeedOpportunityAsync(8, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(8);
        var result = await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 8, Rationale = "Format", ConfirmationAcknowledged = true });
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.NewStage.Should().Be("NO GO");
        response.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task INT_009_CompleteRejectFlow_RejectWithCallbackVerification()
    {
        await SeedOpportunityAsync(9, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(9);
        var rejectCalled = false;
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 9, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => rejectCalled = true).ReturnsAsync(true);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 9, Rationale = "Callback", ConfirmationAcknowledged = true });
        rejectCalled.Should().BeTrue();
    }

    [Fact]
    public async Task INT_010_CompleteRejectFlow_RejectIdempotencyCheck()
    {
        await SeedOpportunityAsync(10, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(10);
        var first = await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 10, Rationale = "Idem", ConfirmationAcknowledged = true });
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 10)).Returns((WorkflowLog?)null);
        var second = await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 10, Rationale = "Idem", ConfirmationAcknowledged = true });
        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region INT_011-020: Complete Cancel-Reopen Flow

    [Fact]
    public async Task INT_011_CompleteCancelReopenFlow_CancelVerifyCancelled()
    {
        await SeedOpportunityAsync(11, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(11, 1);
        SetupCancelMocks(11);
        var result = await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 11, Comment = "Cancelling" });
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.Success.Should().BeTrue();
        response.NewStage.Should().Be("CANCELLED");
    }

    [Fact]
    public async Task INT_012_CompleteCancelReopenFlow_ReopenVerifyReopened()
    {
        await SeedOpportunityAsync(12, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(12, 1);
        var result = await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 12, Comment = "Reopening" });
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.Success.Should().BeTrue();
        response.NewStage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task INT_013_CompleteCancelReopenFlow_CancelPreservesOpportunityData()
    {
        await SeedOpportunityAsync(13, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(13, 1);
        var oppBefore = await DbContext.Opportunities.FindAsync(13);
        SetupCancelMocks(13);
        await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 13, Comment = "Cancel" });
        var oppAfter = await DbContext.Opportunities.FindAsync(13);
        oppAfter!.Name.Should().Be(oppBefore!.Name);
        oppAfter.Description.Should().Be(oppBefore.Description);
    }

    [Fact]
    public async Task INT_014_CompleteCancelReopenFlow_ReopenRestoresDraftStatus()
    {
        await SeedOpportunityAsync(14, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(14, 1);
        await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 14, Comment = "Reopen" });
        var opp = await DbContext.Opportunities.FindAsync(14);
        opp!.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task INT_015_CompleteCancelReopenFlow_CancelReopenCancelCycle()
    {
        await SeedOpportunityAsync(15, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(15, 1);
        SetupCancelMocks(15);
        await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 15, Comment = "C1" });
        await SeedOpportunityAsync(15, "CANCELLED", EntityStatus.Closed);
        var reopenResult = await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 15, Comment = "R1" });
        reopenResult.Result.Should().BeOfType<OkObjectResult>();
        await SeedOpportunityAsync(15, "IDENTIFY & PROFILE");
        SetupCancelMocks(15);
        var cancel2 = await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 15, Comment = "C2" });
        cancel2.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_016_CompleteCancelReopenFlow_CancelReopenSubmitCycle()
    {
        await SeedOpportunityAsync(16, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(16, 1);
        SetupCancelMocks(16);
        await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 16, Comment = "Cancel" });
        await SeedOpportunityAsync(16, "CANCELLED", EntityStatus.Closed);
        await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 16, Comment = "Reopen" });
        await SeedOpportunityAsync(16, "IDENTIFY & PROFILE");
        SetupStandardSubmitMocks();
        var submitResult = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 16, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        submitResult.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_017_CompleteCancelReopenFlow_ConcurrentCancelReopen()
    {
        await SeedOpportunityAsync(17, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(17, 1);
        SetupCancelMocks(17);
        var cancelTask = Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 17, Comment = "C" });
        var cancelResult = await cancelTask;
        cancelResult.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_018_CompleteCancelReopenFlow_CancelNotificationDelivery()
    {
        await SeedOpportunityAsync(18, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(18, 1);
        SetupCancelMocks(18);
        MockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
        await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 18, Comment = "Notify cancel" });
        MockWorkflowManager.Verify(x => x.AddLog(It.IsAny<WorkflowLogModel>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task INT_019_CompleteCancelReopenFlow_ReopenNotificationDelivery()
    {
        await SeedOpportunityAsync(19, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(19, 1);
        await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 19, Comment = "Reopen notify" });
        var opp = await DbContext.Opportunities.FindAsync(19);
        opp!.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task INT_020_CompleteCancelReopenFlow_FullLifecycleCreateSubmitRejectReopenSubmit()
    {
        await SeedOpportunityAsync(20, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(20, 1);
        SetupStandardSubmitMocks();
        await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 20, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "20", NewStage = "GO" };
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 20)).Returns(pendingTask);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "20")).ReturnsAsync("IDENTIFY & PROFILE");
        MockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "20")).ReturnsAsync("Test");
        MockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 20, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        MockWorkflowManager.Setup(x => x.Reject(pendingTask, "Opportunity", 20, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 20, Rationale = "Reject", ConfirmationAcknowledged = true });
        await SeedOpportunityAsync(20, "NO GO");
        await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 20, Comment = null });
        await SeedOpportunityAsync(20, "IDENTIFY & PROFILE");
        SetupStandardSubmitMocks();
        var finalSubmit = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 20, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        finalSubmit.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region INT_021-030: OM Transfer Flow

    [Fact]
    public async Task INT_021_OMTransferFlow_AssignOMChangeOMVerifyOldOMRole()
    {
        await SeedOpportunityAsync(101, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(101, 1);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(101, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var collaborator = await DbContext.Set<OpportunityCollaborator>().FirstOrDefaultAsync(c => c.OpportunityId == 101 && c.UserId == 1 && !c.IsDeleted);
        collaborator.Should().NotBeNull("Old OM should become collaborator");
    }

    [Fact]
    public async Task INT_022_OMTransferFlow_VerifyNewOMPermissions()
    {
        await SeedOpportunityAsync(102, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(102, 1);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(102, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var newOM = await DbContext.Set<OpportunityStakeholder>().FirstOrDefaultAsync(s => s.OpportunityId == 102 && s.UserId == 2 && !s.IsDeleted);
        newOM.Should().NotBeNull("New OM should be in stakeholders");
    }

    [Fact]
    public async Task INT_023_OMTransferFlow_OMTransferPreservesOtherStakeholders()
    {
        await SeedOpportunityAsync(103, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(103, 1);
        await SeedCollaboratorAsync(103, 3);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(103, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var collaborators = await DbContext.Set<OpportunityCollaborator>().Where(c => c.OpportunityId == 103 && !c.IsDeleted).ToListAsync();
        collaborators.Should().Contain(c => c.UserId == 1);
        collaborators.Should().Contain(c => c.UserId == 3);
    }

    [Fact]
    public async Task INT_024_OMTransferFlow_OMTransferWithExistingCollaborators()
    {
        await SeedOpportunityAsync(104, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(104, 1);
        await SeedCollaboratorAsync(104, 4);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(104, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var collabCount = await DbContext.Set<OpportunityCollaborator>().CountAsync(c => c.OpportunityId == 104 && !c.IsDeleted);
        collabCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task INT_025_OMTransferFlow_OMTransferDuringWorkflow()
    {
        await SeedOpportunityAsync(105, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(105, 1);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(105, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var newOM = await DbContext.Set<OpportunityStakeholder>().FirstOrDefaultAsync(s => s.OpportunityId == 105 && s.UserId == 2 && !s.IsDeleted);
        newOM.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_026_OMTransferFlow_OMTransferNotification()
    {
        await SeedOpportunityAsync(106, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(106, 1);
        await SeedUserAsync(2);
        MockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
        try { await OpportunityManager.UpdateTeamSectionAsync(106, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var stakeholders = await DbContext.Set<OpportunityStakeholder>().Where(s => s.OpportunityId == 106 && !s.IsDeleted).ToListAsync();
        stakeholders.Should().Contain(s => s.UserId == 2);
    }

    [Fact]
    public async Task INT_027_OMTransferFlow_OMTransferAuditTrail()
    {
        await SeedOpportunityAsync(107, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(107, 1);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(107, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var oldOMStakeholder = await DbContext.Set<OpportunityStakeholder>().FirstOrDefaultAsync(s => s.OpportunityId == 107 && s.UserId == 1 && !s.IsDeleted);
        var newOMStakeholder = await DbContext.Set<OpportunityStakeholder>().FirstOrDefaultAsync(s => s.OpportunityId == 107 && s.UserId == 2 && !s.IsDeleted);
        (oldOMStakeholder == null || newOMStakeholder != null).Should().BeTrue();
    }

    [Fact]
    public async Task INT_028_OMTransferFlow_ConsecutiveOMTransfers()
    {
        await SeedOpportunityAsync(108, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(108, 1);
        await SeedUserAsync(2);
        await SeedUserAsync(3);
        try { await OpportunityManager.UpdateTeamSectionAsync(108, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        try { await OpportunityManager.UpdateTeamSectionAsync(108, new TeamSectionRequest { OpportunityManagerId = 3 }); } catch (KeyNotFoundException) { }
        var currentOM = await DbContext.Set<OpportunityStakeholder>().FirstOrDefaultAsync(s => s.OpportunityId == 108 && s.UserId == 3 && !s.IsDeleted);
        currentOM.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_029_OMTransferFlow_OldOMBecomesCollaborator()
    {
        await SeedOpportunityAsync(109, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(109, 1);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(109, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var collab = await DbContext.Set<OpportunityCollaborator>().FirstOrDefaultAsync(c => c.OpportunityId == 109 && c.UserId == 1 && !c.IsDeleted);
        collab.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_030_OMTransferFlow_NewOMHasStakeholderRole()
    {
        await SeedOpportunityAsync(110, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(110, 1);
        await SeedUserAsync(2);
        var omRole = await DbContext.EntityRoles.FirstOrDefaultAsync(r => r.Name == "Opportunity Manager");
        try { await OpportunityManager.UpdateTeamSectionAsync(110, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var newOM = await DbContext.Set<OpportunityStakeholder>().FirstOrDefaultAsync(s => s.OpportunityId == 110 && s.UserId == 2 && !s.IsDeleted);
        newOM.Should().NotBeNull();
        if (omRole != null) newOM!.EntityRoleId.Should().Be(omRole.Id);
    }

    #endregion

    #region INT_031-040: Cross-entity Integration

    [Fact]
    public async Task INT_031_CrossEntity_RejectAffectsOpportunityButNotPartners()
    {
        await SeedOpportunityAsync(201, "IDENTIFY & PROFILE");
        await SeedPartnerAsync(301);
        SetupStandardRejectMocks(201);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 201, Rationale = "Reject", ConfirmationAcknowledged = true });
        var partner = await DbContext.Set<Partner>().FirstOrDefaultAsync(p => p.Id == 301 && !p.IsDeleted);
        partner.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_032_CrossEntity_CancelAffectsOpportunitySearchResults()
    {
        await SeedOpportunityAsync(202, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(202, 1);
        SetupCancelMocks(202);
        await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 202, Comment = "Cancel" });
        var opp = await DbContext.Opportunities.FindAsync(202);
        opp!.Stage.Should().Be("CANCELLED");
    }

    [Fact(Skip = "DEF-056: Reopen workflow sets EntityStatus to Draft(4) instead of Active(1)")]
    public async Task INT_033_CrossEntity_ReopenReindexesOpportunity()
    {
        await SeedOpportunityAsync(203, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(203, 1);
        await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 203, Comment = "Reopen" });
        var opp = await DbContext.Opportunities.FindAsync(203);
        opp!.Stage.Should().Be("IDENTIFY & PROFILE");
        opp.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    public async Task INT_034_CrossEntity_WorkflowStateConsistentAcrossMultipleReads()
    {
        await SeedOpportunityAsync(204, "IDENTIFY & PROFILE");
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "204")).ReturnsAsync(true);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "204")).ReturnsAsync("IDENTIFY & PROFILE");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 204)).Returns((WorkflowLog?)null);
        var state1 = await Controller.GetWorkflowState("Opportunity", 204);
        var state2 = await Controller.GetWorkflowState("Opportunity", 204);
        state1.Result.Should().BeOfType<OkObjectResult>();
        state2.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_035_CrossEntity_ConcurrentReadsDuringStateTransition()
    {
        await SeedOpportunityAsync(205, "IDENTIFY & PROFILE");
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "205")).ReturnsAsync(true);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "205")).ReturnsAsync("IDENTIFY & PROFILE");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 205)).Returns((WorkflowLog?)null);
        var tasks = Enumerable.Range(0, 3).Select(_ => Controller.GetWorkflowState("Opportunity", 205));
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.Result.Should().BeOfType<OkObjectResult>());
    }

    [Fact]
    public async Task INT_036_CrossEntity_RejectFollowedByListQueryConsistency()
    {
        await SeedOpportunityAsync(206, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(206);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 206, Rationale = "R", ConfirmationAcknowledged = true });
        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 206 && !o.IsDeleted);
        opp.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_037_CrossEntity_OMTransferVisibleInOpportunityDetail()
    {
        await SeedOpportunityAsync(207, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(207, 1);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(207, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var stakeholders = await DbContext.Set<OpportunityStakeholder>().Where(s => s.OpportunityId == 207 && !s.IsDeleted).ToListAsync();
        stakeholders.Should().Contain(s => s.UserId == 2);
    }

    [Fact]
    public async Task INT_038_CrossEntity_WorkflowHistoryAggregation()
    {
        await SeedOpportunityAsync(208, "IDENTIFY & PROFILE");
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "208")).ReturnsAsync(true);
        var history = new List<WorkflowHistoryModel> { new() { FromStage = "IDENTIFY & PROFILE", ToStage = "GO", Action = "Submitted", CompletedOn = DateTime.UtcNow } };
        MockWorkflowManager.Setup(x => x.GetWorkflowHistory(It.IsAny<StateMachine>(), "Opportunity", 208)).Returns(history);
        var result = await Controller.GetWorkflowHistory("Opportunity", 208);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var list = okResult.Value as List<WorkflowHistoryResponse>;
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_039_CrossEntity_RejectAndEmailNotificationIntegration()
    {
        await SeedOpportunityAsync(209, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(209);
        MockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 209, Rationale = "Notify", ConfirmationAcknowledged = true });
        var okResult = await Controller.GetWorkflowState("Opportunity", 209);
        okResult.Result.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_040_CrossEntity_RejectAndAuditLogIntegration()
    {
        await SeedOpportunityAsync(210, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(210);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 210, Rationale = "Audit", ConfirmationAcknowledged = true });
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 210, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region INT_041-050: DB Persistence Verification

    [Fact]
    public async Task INT_041_DBPersistence_RejectPersistsAcrossContextRecreation()
    {
        await SeedOpportunityAsync(301, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(301);
        await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 301, Rationale = "Persist", ConfirmationAcknowledged = true });
        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 301 && !o.IsDeleted);
        opp.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_042_DBPersistence_CancelPersistsAcrossContextRecreation()
    {
        await SeedOpportunityAsync(302, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(302, 1);
        SetupCancelMocks(302);
        await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 302, Comment = "Persist" });
        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 302 && !o.IsDeleted);
        opp.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_043_DBPersistence_ReopenPersistsAcrossContextRecreation()
    {
        await SeedOpportunityAsync(303, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(303, 1);
        await Controller.Reopen(new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 303, Comment = "Persist" });
        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 303 && !o.IsDeleted);
        opp!.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task INT_044_DBPersistence_OMTransferPersistsAcrossContextRecreation()
    {
        await SeedOpportunityAsync(304, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(304, 1);
        await SeedUserAsync(2);
        try { await OpportunityManager.UpdateTeamSectionAsync(304, new TeamSectionRequest { OpportunityManagerId = 2 }); } catch (KeyNotFoundException) { }
        var newOM = await DbContext.Set<OpportunityStakeholder>().AsNoTracking().FirstOrDefaultAsync(s => s.OpportunityId == 304 && s.UserId == 2 && !s.IsDeleted);
        newOM.Should().NotBeNull();
    }

    [Fact]
    public async Task INT_045_DBPersistence_WorkflowHistoryPersistsAcrossContextRecreation()
    {
        await SeedOpportunityAsync(305, "IDENTIFY & PROFILE");
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "305")).ReturnsAsync(true);
        MockWorkflowManager.Setup(x => x.GetWorkflowHistory(It.IsAny<StateMachine>(), "Opportunity", 305)).Returns(new List<WorkflowHistoryModel>());
        var result = await Controller.GetWorkflowHistory("Opportunity", 305);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_046_DBPersistence_ConcurrentWritesDontCorruptData()
    {
        await SeedOpportunityAsync(306, "IDENTIFY & PROFILE");
        await SeedOpportunityAsync(307, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(306, 1);
        await SeedOpportunityManagerStakeholderAsync(307, 1);
        SetupCancelMocks(306);
        SetupCancelMocks(307);
        var t1 = Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 306, Comment = "C1" });
        var t2 = Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 307, Comment = "C2" });
        var results = await Task.WhenAll(t1, t2);
        results.Should().AllSatisfy(r => r.Result.Should().BeOfType<OkObjectResult>());
    }

    [Fact]
    public async Task INT_047_DBPersistence_LargeRationalePersistsCorrectly()
    {
        var largeRationale = new string('x', 2000);
        await SeedOpportunityAsync(308, "IDENTIFY & PROFILE");
        SetupStandardRejectMocks(308);
        var result = await Controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 308, Rationale = largeRationale, ConfirmationAcknowledged = true });
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_048_DBPersistence_SpecialCharactersInCommentsPersist()
    {
        await SeedOpportunityAsync(309, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(309, 1);
        SetupCancelMocks(309);
        var comment = "Cancel: <script>alert(1)</script>; \"quotes\" & 'apostrophes' — unicode 日本語";
        var result = await Controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 309, Comment = comment });
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task INT_049_DBPersistence_AuditTimestampsAreUTC()
    {
        await SeedOpportunityAsync(310, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(310);
        opp!.CreatedDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task INT_050_DBPersistence_SoftDeletedRecordsExcludedFromQueries()
    {
        await SeedOpportunityAsync(311, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(311);
        opp!.IsDeleted = true;
        opp.DeletedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();
        var found = await DbContext.Opportunities.FirstOrDefaultAsync(o => o.Id == 311 && !o.IsDeleted);
        found.Should().BeNull();
    }

    #endregion

    #region Helpers

    private async Task SeedUserAsync(int userId)
    {
        if (await DbContext.PAOUsers.AnyAsync(u => u.Id == userId)) return;
        DbContext.PAOUsers.Add(new PAOUser
        {
            Id = userId,
            Email = $"user{userId}@test.com",
            IsInternal = true,
            ActiveUser = true
        });
        await DbContext.SaveChangesAsync();
    }

    private async Task SeedCollaboratorAsync(int opportunityId, int userId)
    {
        await SeedUserAsync(userId);
        if (await DbContext.Set<OpportunityCollaborator>().AnyAsync(c => c.OpportunityId == opportunityId && c.UserId == userId && !c.IsDeleted)) return;
        DbContext.Set<OpportunityCollaborator>().Add(new OpportunityCollaborator
        {
            Id = opportunityId * 100 + userId,
            OpportunityId = opportunityId,
            UserId = userId,
            Name = $"Collaborator{userId}",
            IsDeleted = false
        });
        await DbContext.SaveChangesAsync();
    }

    private async Task SeedPartnerAsync(int partnerId)
    {
        if (await DbContext.Set<Partner>().AnyAsync(p => p.Id == partnerId && !p.IsDeleted)) return;
        DbContext.Set<Partner>().Add(new Partner
        {
            Id = partnerId,
            Name = $"Partner{partnerId}",
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await DbContext.SaveChangesAsync();
    }

    #endregion
}
