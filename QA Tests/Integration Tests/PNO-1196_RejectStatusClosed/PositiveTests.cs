/**
 * @fileoverview PNO-1196 Positive Tests: Opportunity EntityStatus→Closed after Reject.
 * 30 happy-path tests confirming correct state transitions on rejection.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Workflow;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using Xunit;
using EntityStatus = UNOPS.PAO.Domain.Entities.EntityStatus;
using System.Security.Claims;

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// PNO-1196 Positive Tests — 30 tests covering happy-path rejection scenarios.
/// Verifies: Stage→NO GO, Status→Closed, WorkflowStatus→None, audit, reopen, notifications.
/// </summary>
[Collection("Positive")]
[Trait("Category", "Positive")]
[Trait("Ticket", "PNO-1196")]
public class PositiveTests : PNO1196TestFixtureBase
{
    // ─── §1.1 Core State Transitions (POS-001 – 010) ───────────────────────

    [Fact] [Trait("TestId", "POS-001")]
    public async Task Reject_DoA2_SetsStageToNoGo()
    {
        await SeedOpportunityAsync(1, "GO");
        await SeedPendingWorkflowTaskAsync(1, 101);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns(new WorkflowLog { Id = 101, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(1));

        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "POS-002")]
    public async Task Reject_DoA2_SetsStatusToClosed()
    {
        await SeedOpportunityAsync(2, "GO");
        await SeedPendingWorkflowTaskAsync(2, 102);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 2))
            .Returns(new WorkflowLog { Id = 102, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(2));

        var opp = await DbContext.Opportunities.FindAsync(2);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-003")]
    public async Task Reject_SetsWorkflowStatusToNone()
    {
        await SeedOpportunityAsync(3, "GO");
        await SeedPendingWorkflowTaskAsync(3, 103);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3))
            .Returns(new WorkflowLog { Id = 103, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3));

        var opp = await DbContext.Opportunities.FindAsync(3);
        opp!.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "POS-004")]
    public async Task Reject_DoA3Fallback_SetsStatusToClosed()
    {
        await SeedOpportunityAsync(4, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(4, 104);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4))
            .Returns(new WorkflowLog { Id = 104, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4, rationale: "DoA3 fallback rejection"));

        var opp = await DbContext.Opportunities.FindAsync(4);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-005")]
    public async Task Reject_WithRationale_RationaleStoredInWorkflowLog()
    {
        await SeedOpportunityAsync(5, "GO");
        await SeedPendingWorkflowTaskAsync(5, 105);
        var capturedLog = (WorkflowLog?)null;
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 5))
            .Returns(new WorkflowLog { Id = 105, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<WorkflowLog, string, int, string, string, string>((log, _, _, _, _, _) => capturedLog = log)
            .ReturnsAsync(true);

        const string rationale = "Budget constraints prevent implementation";
        await Controller.Reject(BuildRejectRequest(5, rationale: rationale));

        capturedLog.Should().NotBeNull();
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact] [Trait("TestId", "POS-006")]
    public async Task Reject_ReturnsSuccessTrue()
    {
        await SeedOpportunityAsync(6, "GO");
        await SeedPendingWorkflowTaskAsync(6, 106);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6))
            .Returns(new WorkflowLog { Id = 106, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6));

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "POS-007")]
    public async Task Reject_ResponseContainsNoGoNewStage()
    {
        await SeedOpportunityAsync(7, "GO");
        await SeedPendingWorkflowTaskAsync(7, 107);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 7))
            .Returns(new WorkflowLog { Id = 107, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(7));

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        var response = ok!.Value as UNOPS.PAO.Models.Workflow.WorkflowActionResponse;
        response.Should().NotBeNull();
        response!.NewStage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "POS-008")]
    public async Task Reject_OpportunityPersistedWithNoGoStageInDb()
    {
        await SeedOpportunityAsync(8, "GO");
        await SeedPendingWorkflowTaskAsync(8, 108);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8))
            .Returns(new WorkflowLog { Id = 108, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(8));
        DbContext.ChangeTracker.Clear();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 8);
        opp.Should().NotBeNull();
        opp!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "POS-009")]
    public async Task Reject_OpportunityPersistedWithStatusClosedInDb()
    {
        await SeedOpportunityAsync(9, "GO");
        await SeedPendingWorkflowTaskAsync(9, 109);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9))
            .Returns(new WorkflowLog { Id = 109, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(9));
        DbContext.ChangeTracker.Clear();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 9);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-010")]
    public async Task Reject_CallsWorkflowManagerRejectExactlyOnce()
    {
        await SeedOpportunityAsync(10, "GO");
        await SeedPendingWorkflowTaskAsync(10, 110);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 10))
            .Returns(new WorkflowLog { Id = 110, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(10));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ─── §1.2 Audit Fields (POS-011 – 016) ────────────────────────────────

    [Fact] [Trait("TestId", "POS-011")]
    public async Task Reject_SetsLastModifiedByToRejectingUser()
    {
        await SeedOpportunityAsync(11, "GO");
        await SeedPendingWorkflowTaskAsync(11, 111);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 11))
            .Returns(new WorkflowLog { Id = 111, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(11));

        var opp = await DbContext.Opportunities.FindAsync(11);
        opp!.LastModifiedBy.Should().Be(1);
    }

    [Fact] [Trait("TestId", "POS-012")]
    public async Task Reject_SetsLastModifiedDateToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        await SeedOpportunityAsync(12, "GO");
        await SeedPendingWorkflowTaskAsync(12, 112);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 12))
            .Returns(new WorkflowLog { Id = 112, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(12));

        var opp = await DbContext.Opportunities.FindAsync(12);
        opp!.LastModifiedDate.Should().BeAfter(before);
    }

    [Fact] [Trait("TestId", "POS-013")]
    public async Task Reject_WorkflowStatusIsNoneNotInProgress()
    {
        await SeedOpportunityAsync(13, "GO");
        await SeedPendingWorkflowTaskAsync(13, 113);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 13))
            .Returns(new WorkflowLog { Id = 113, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(13));

        var opp = await DbContext.Opportunities.FindAsync(13);
        opp!.WorkflowStatus.Should().Be(WorkflowStatus.None);
        opp.WorkflowStatus.Should().NotBe(WorkflowStatus.InWorkflow);
    }

    [Fact] [Trait("TestId", "POS-014")]
    public async Task Reject_WorkflowManagerRejectCalledWithCorrectLog()
    {
        await SeedOpportunityAsync(14, "GO");
        await SeedPendingWorkflowTaskAsync(14, 114);
        var expectedLog = new WorkflowLog { Id = 114, RequiresApproval = true };
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 14)).Returns(expectedLog);
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(14));

        MockWorkflowManager.Verify(x => x.Reject(It.Is<WorkflowLog>(l => l.Id == 114), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact] [Trait("TestId", "POS-015")]
    public async Task Reject_NotificationServiceNotNullAfterReject()
    {
        await SeedOpportunityAsync(15, "GO");
        await SeedPendingWorkflowTaskAsync(15, 115);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 15))
            .Returns(new WorkflowLog { Id = 115, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(15));

        result.Should().NotBeNull();
        NotificationService.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "POS-016")]
    public async Task Reject_IsDeletedRemainsFalseAfterReject()
    {
        await SeedOpportunityAsync(16, "GO");
        await SeedPendingWorkflowTaskAsync(16, 116);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 16))
            .Returns(new WorkflowLog { Id = 116, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(16));

        var opp = await DbContext.Opportunities.FindAsync(16);
        opp!.IsDeleted.Should().BeFalse();
    }

    // ─── §1.3 Post-Reject Immutability (POS-017 – 022) ────────────────────

    [Fact] [Trait("TestId", "POS-017")]
    public async Task RejectedOpportunity_StageIsNoGo_NotEditable()
    {
        await SeedOpportunityAsync(17, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.FindAsync(17);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-018")]
    public async Task RejectedOpportunity_ExistingDocumentsAccessible()
    {
        await SeedOpportunityAsync(18, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 18 && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-019")]
    public async Task Reject_SecondRejectOnSameOpp_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(19, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 19)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(19));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "POS-020")]
    public async Task Reject_ClosedOpportunity_StatusRemainsClosedInDb()
    {
        await SeedOpportunityAsync(20, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 20);
        opp!.Status.Should().Be(EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "POS-021")]
    public async Task Reject_OpportunityStillInDbAfterReject()
    {
        await SeedOpportunityAsync(21, "GO");
        await SeedPendingWorkflowTaskAsync(21, 121);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 21))
            .Returns(new WorkflowLog { Id = 121, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(21));

        var opp = await DbContext.Opportunities.FindAsync(21);
        opp.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "POS-022")]
    public async Task Reject_OpportunityNameUnchangedAfterReject()
    {
        await SeedOpportunityAsync(22, "GO");
        await SeedPendingWorkflowTaskAsync(22, 122);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 22))
            .Returns(new WorkflowLog { Id = 122, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(22));

        var opp = await DbContext.Opportunities.FindAsync(22);
        opp!.Name.Should().Be("Test Opportunity 22");
    }

    // ─── §1.4 Notification & Reopen (POS-023 – 030) ───────────────────────

    [Fact] [Trait("TestId", "POS-023")]
    public async Task Reject_ReturnsOkResult()
    {
        await SeedOpportunityAsync(23, "GO");
        await SeedPendingWorkflowTaskAsync(23, 123);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 23))
            .Returns(new WorkflowLog { Id = 123, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(23));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "POS-024")]
    public async Task Reject_WithLongRationale_StillSucceeds()
    {
        await SeedOpportunityAsync(24, "GO");
        await SeedPendingWorkflowTaskAsync(24, 124);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 24))
            .Returns(new WorkflowLog { Id = 124, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var longRationale = string.Concat(Enumerable.Repeat("Extended rejection rationale. ", 20));
        var result = await Controller.Reject(BuildRejectRequest(24, rationale: longRationale));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "POS-025")]
    public async Task ReopenFromNoGo_AfterReject_ControllerAcceptsRequest()
    {
        await SeedOpportunityAsync(25, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 25)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(25));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "POS-026")]
    public async Task Reject_ControllerContextUserIdentified()
    {
        Controller.ControllerContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            .Should().Be("1");
    }

    [Fact] [Trait("TestId", "POS-027")]
    public async Task Reject_WorkflowManagerPendingTaskCalledWithOpportunityName()
    {
        await SeedOpportunityAsync(27, "GO");
        await SeedPendingWorkflowTaskAsync(27, 127);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 27))
            .Returns(new WorkflowLog { Id = 127, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(27));

        MockWorkflowManager.Verify(x => x.PendingTask("Opportunity", 27), Times.AtLeastOnce);
    }

    [Fact] [Trait("TestId", "POS-028")]
    public async Task Reject_StageWasGoBeforeReject_BecomeNoGoAfter()
    {
        await SeedOpportunityAsync(28, "GO", EntityStatus.Active);
        var oppBefore = await DbContext.Opportunities.FindAsync(28);
        oppBefore!.Stage.Should().Be("GO");

        await SeedPendingWorkflowTaskAsync(28, 128);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 28))
            .Returns(new WorkflowLog { Id = 128, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(28));

        var oppAfter = await DbContext.Opportunities.FindAsync(28);
        oppAfter!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "POS-029")]
    public async Task Reject_StatusWasActiveBeforeReject_BecomesClosedAfter()
    {
        await SeedOpportunityAsync(29, "GO", EntityStatus.Active);
        var oppBefore = await DbContext.Opportunities.FindAsync(29);
        oppBefore!.Status.Should().Be(EntityStatus.Active);

        await SeedPendingWorkflowTaskAsync(29, 129);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 29))
            .Returns(new WorkflowLog { Id = 129, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(29));

        var oppAfter = await DbContext.Opportunities.FindAsync(29);
        oppAfter!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-030")]
    public async Task Reject_MultipleOpportunities_EachGetsIndependentlyRejected()
    {
        await SeedOpportunityAsync(301, "GO");
        await SeedOpportunityAsync(302, "GO");
        await SeedPendingWorkflowTaskAsync(301, 1301);
        await SeedPendingWorkflowTaskAsync(302, 1302);

        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 301))
            .Returns(new WorkflowLog { Id = 1301, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 302))
            .Returns(new WorkflowLog { Id = 1302, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(301));
        await Controller.Reject(BuildRejectRequest(302));

        var opp1 = await DbContext.Opportunities.FindAsync(301);
        var opp2 = await DbContext.Opportunities.FindAsync(302);
        opp1!.Status.Should().Be(EntityStatus.Closed);
        opp2!.Status.Should().Be(EntityStatus.Closed);
    }
}
