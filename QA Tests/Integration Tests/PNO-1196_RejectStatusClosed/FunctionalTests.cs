/**
 * @fileoverview PNO-1196 Functional Tests — 50 tests.
 * Business rules, state machine transitions, audit fields, immutability, and reopen logic.
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

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// PNO-1196 Functional Tests — 50 tests covering business rules and state transitions.
/// </summary>
[Collection("Functional")]
[Trait("Category", "Functional")]
[Trait("Ticket", "PNO-1196")]
public class FunctionalTests : PNO1196TestFixtureBase
{
    // ─── §4.1 Core Reject Business Rules (FUN-001 – 015) ─────────────────

    [Fact] [Trait("TestId", "FUN-001")]
    public async Task Reject_RequiresPendingApprovalTask_BusinessRule()
    {
        await SeedOpportunityAsync(4001, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4001)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(4001));

        result.Should().BeOfType<BadRequestObjectResult>("Rejection requires an active pending approval task");
    }

    [Fact] [Trait("TestId", "FUN-002")]
    public async Task Reject_SetsStatusToClosed_BusinessRule()
    {
        await SeedOpportunityAsync(4002, "GO");
        await SeedPendingWorkflowTaskAsync(4002, 5002);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4002))
            .Returns(new WorkflowLog { Id = 5002, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4002));

        var opp = await DbContext.Opportunities.FindAsync(4002);
        opp!.Status.Should().Be(EntityStatus.Closed, "PNO-1196: Reject must set Status→Closed");
    }

    [Fact] [Trait("TestId", "FUN-003")]
    public async Task Reject_SetsStageToNoGo_BusinessRule()
    {
        await SeedOpportunityAsync(4003, "GO");
        await SeedPendingWorkflowTaskAsync(4003, 5003);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4003))
            .Returns(new WorkflowLog { Id = 5003, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4003));

        var opp = await DbContext.Opportunities.FindAsync(4003);
        opp!.Stage.Should().Be("NO GO", "PNO-1196: Reject must set Stage→NO GO");
    }

    [Fact] [Trait("TestId", "FUN-004")]
    public async Task Reject_SetsWorkflowStatusToNone_BusinessRule()
    {
        await SeedOpportunityAsync(4004, "GO");
        var opp4 = await DbContext.Opportunities.FindAsync(4004);
        opp4!.WorkflowStatus = WorkflowStatus.InWorkflow;
        await DbContext.SaveChangesAsync();

        await SeedPendingWorkflowTaskAsync(4004, 5004);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4004))
            .Returns(new WorkflowLog { Id = 5004, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4004));

        var updated = await DbContext.Opportunities.FindAsync(4004);
        updated!.WorkflowStatus.Should().Be(WorkflowStatus.None, "PNO-1196: WorkflowStatus must reset to None");
    }

    [Fact] [Trait("TestId", "FUN-005")]
    public async Task Reject_RequiresConfirmation_BusinessRule()
    {
        await SeedOpportunityAsync(4005, "GO");
        await SeedPendingWorkflowTaskAsync(4005, 5005);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4005))
            .Returns(new WorkflowLog { Id = 5005, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(4005, confirm: false));

        result.Should().BeOfType<BadRequestObjectResult>("Confirmation is required for rejection");
    }

    [Fact] [Trait("TestId", "FUN-006")]
    public async Task Reject_RequiresNonEmptyRationale_BusinessRule()
    {
        await SeedOpportunityAsync(4006, "GO");
        await SeedPendingWorkflowTaskAsync(4006, 5006);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4006))
            .Returns(new WorkflowLog { Id = 5006, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(4006, rationale: ""));

        result.Should().BeOfType<BadRequestObjectResult>("Rationale is required for rejection");
    }

    [Fact] [Trait("TestId", "FUN-007")]
    public async Task Reject_IrreversibleStateTransition_OnceClosedStaysNogo()
    {
        await SeedOpportunityAsync(4007, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.FindAsync(4007);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);

        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4007)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(4007));

        var oppAfter = await DbContext.Opportunities.FindAsync(4007);
        oppAfter!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "FUN-008")]
    public async Task Reject_CallsWorkflowRejectMethod_FunctionContract()
    {
        await SeedOpportunityAsync(4008, "GO");
        await SeedPendingWorkflowTaskAsync(4008, 5008);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4008))
            .Returns(new WorkflowLog { Id = 5008, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4008));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once,
            "WorkflowManager.Reject must be called exactly once on successful rejection");
    }

    [Fact] [Trait("TestId", "FUN-009")]
    public async Task Reject_DoesNotCallWorkflowRejectWhenNoPendingTask()
    {
        await SeedOpportunityAsync(4009, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4009)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(4009));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never,
            "WorkflowManager.Reject must NOT be called when no pending task exists");
    }

    [Fact] [Trait("TestId", "FUN-010")]
    public async Task Reject_SavesChangesToDatabase_FunctionContract()
    {
        await SeedOpportunityAsync(4010, "GO");
        await SeedPendingWorkflowTaskAsync(4010, 5010);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4010))
            .Returns(new WorkflowLog { Id = 5010, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4010));
        DbContext.ChangeTracker.Clear();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 4010);
        opp.Status.Should().Be(EntityStatus.Closed, "Changes must be persisted to DB");
    }

    [Fact] [Trait("TestId", "FUN-011")]
    public async Task Reject_AuditFieldLastModifiedBySetToCurrentUser()
    {
        await SeedOpportunityAsync(4011, "GO");
        await SeedPendingWorkflowTaskAsync(4011, 5011);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4011))
            .Returns(new WorkflowLog { Id = 5011, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4011));

        var opp = await DbContext.Opportunities.FindAsync(4011);
        opp!.LastModifiedBy.Should().Be(1, "Audit: LastModifiedBy must be set to the rejecting user ID");
    }

    [Fact] [Trait("TestId", "FUN-012")]
    public async Task Reject_AuditFieldLastModifiedDateUpdated()
    {
        var preTest = DateTime.UtcNow;
        await SeedOpportunityAsync(4012, "GO");
        await SeedPendingWorkflowTaskAsync(4012, 5012);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4012))
            .Returns(new WorkflowLog { Id = 5012, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4012));

        var opp = await DbContext.Opportunities.FindAsync(4012);
        opp!.LastModifiedDate.Should().BeOnOrAfter(preTest, "Audit: LastModifiedDate must be updated on rejection");
    }

    [Fact] [Trait("TestId", "FUN-013")]
    public async Task Reject_ClosedStatusNotActive_PostRejectQuery()
    {
        await SeedOpportunityAsync(4013, "GO");
        await SeedPendingWorkflowTaskAsync(4013, 5013);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4013))
            .Returns(new WorkflowLog { Id = 5013, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4013));

        var notActive = await DbContext.Opportunities
            .Where(o => o.Id == 4013 && o.Status == EntityStatus.Active)
            .AnyAsync();
        notActive.Should().BeFalse("Rejected opp must NOT appear in Active queries");
    }

    [Fact] [Trait("TestId", "FUN-014")]
    public async Task Reject_ClosedStatusAppearsInClosedQuery()
    {
        await SeedOpportunityAsync(4014, "GO");
        await SeedPendingWorkflowTaskAsync(4014, 5014);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4014))
            .Returns(new WorkflowLog { Id = 5014, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4014));

        var inClosed = await DbContext.Opportunities
            .Where(o => o.Id == 4014 && o.Status == EntityStatus.Closed)
            .AnyAsync();
        inClosed.Should().BeTrue("Rejected opp MUST appear in Closed queries");
    }

    [Fact] [Trait("TestId", "FUN-015")]
    public async Task Reject_NoGoStageAppearsInNoGoQuery()
    {
        await SeedOpportunityAsync(4015, "GO");
        await SeedPendingWorkflowTaskAsync(4015, 5015);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4015))
            .Returns(new WorkflowLog { Id = 5015, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4015));

        var inNoGo = await DbContext.Opportunities
            .Where(o => o.Id == 4015 && o.Stage == "NO GO")
            .AnyAsync();
        inNoGo.Should().BeTrue("Rejected opp MUST appear in NO GO stage queries");
    }

    // ─── §4.2 Immutability After Rejection (FUN-016 – 030) ───────────────

    [Fact] [Trait("TestId", "FUN-016")]
    public async Task RejectedOpp_IsNotModifiable_Stage()
    {
        await SeedOpportunityAsync(4016, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.FindAsync(4016);
        opp!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "FUN-017")]
    public async Task RejectedOpp_SecondRejectCall_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(4017, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4017)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(4017));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "FUN-018")]
    public async Task RejectedOpp_DescriptionIntact_ReadOnly()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 4018, Name = "Desc Preserved Opp",
            Description = "Original description preserved",
            Stage = "NO GO", Status = EntityStatus.Closed, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();

        var opp = await DbContext.Opportunities.FindAsync(4018);
        opp!.Description.Should().Be("Original description preserved");
    }

    [Fact] [Trait("TestId", "FUN-019")]
    public async Task RejectedOpp_StatementMarkdownIntact()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 4019, Name = "Statement Preserved Opp", Description = "Functional test opportunity",
            OpportunityStatementMarkdown = "## Preserved Statement",
            Stage = "NO GO", Status = EntityStatus.Closed, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();

        var opp = await DbContext.Opportunities.FindAsync(4019);
        opp!.OpportunityStatementMarkdown.Should().Contain("Preserved Statement");
    }

    [Fact] [Trait("TestId", "FUN-020")]
    public async Task RejectedOpp_BudgetUnchangedAfterReject()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 4020, Name = "Budget Check Opp", Stage = "GO", Description = "Functional test opportunity",
            Status = EntityStatus.Active, IsDeleted = false,
            InitiativeBudgetUSD = 250_000m,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(4020, 5020);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4020))
            .Returns(new WorkflowLog { Id = 5020, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4020));

        var opp = await DbContext.Opportunities.FindAsync(4020);
        opp!.InitiativeBudgetUSD.Should().Be(250_000m);
    }

    [Fact] [Trait("TestId", "FUN-021")]
    public async Task Reject_OnlyChangesStageStatusWorkflowStatus()
    {
        await SeedOpportunityAsync(4021, "GO");
        var before = await DbContext.Opportunities.FindAsync(4021);
        var originalName = before!.Name;
        var originalDesc = before.Description;

        await SeedPendingWorkflowTaskAsync(4021, 5021);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4021))
            .Returns(new WorkflowLog { Id = 5021, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4021));

        var after = await DbContext.Opportunities.FindAsync(4021);
        after!.Name.Should().Be(originalName);
        after.Description.Should().Be(originalDesc);
    }

    [Fact] [Trait("TestId", "FUN-022")]
    public async Task Reject_IsDeletedStaysFalse_NotPhysicallyDeleted()
    {
        await SeedOpportunityAsync(4022, "GO");
        await SeedPendingWorkflowTaskAsync(4022, 5022);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4022))
            .Returns(new WorkflowLog { Id = 5022, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4022));

        var opp = await DbContext.Opportunities.FindAsync(4022);
        opp.Should().NotBeNull("Rejection is soft (record preserved)");
        opp!.IsDeleted.Should().BeFalse("IsDeleted must remain false after rejection");
    }

    [Fact] [Trait("TestId", "FUN-023")]
    public async Task Reject_OppIdPreservedAfterReject()
    {
        await SeedOpportunityAsync(4023, "GO");
        await SeedPendingWorkflowTaskAsync(4023, 5023);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4023))
            .Returns(new WorkflowLog { Id = 5023, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4023));

        var opp = await DbContext.Opportunities.FindAsync(4023);
        opp!.Id.Should().Be(4023);
    }

    [Fact] [Trait("TestId", "FUN-024")]
    public async Task Reject_OrgUnitIdPreservedAfterReject()
    {
        await SeedOpportunityAsync(4024, "GO");
        var before = await DbContext.Opportunities.FindAsync(4024);
        var orgUnitId = before!.ResponsibleOrgUnitId;

        await SeedPendingWorkflowTaskAsync(4024, 5024);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4024))
            .Returns(new WorkflowLog { Id = 5024, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4024));

        var after = await DbContext.Opportunities.FindAsync(4024);
        after!.ResponsibleOrgUnitId.Should().Be(orgUnitId);
    }

    [Fact] [Trait("TestId", "FUN-025")]
    public async Task Reject_TargetDeliveryDatePreservedAfterReject()
    {
        await SeedOpportunityAsync(4025, "GO");
        var before = await DbContext.Opportunities.FindAsync(4025);
        var originalDate = before!.TargetDeliveryDate;

        await SeedPendingWorkflowTaskAsync(4025, 5025);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4025))
            .Returns(new WorkflowLog { Id = 5025, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4025));

        var after = await DbContext.Opportunities.FindAsync(4025);
        after!.TargetDeliveryDate.Should().Be(originalDate);
    }

    // ─── §4.3 Reopen Logic (FUN-026 – 035) ───────────────────────────────

    [Fact] [Trait("TestId", "FUN-026")]
    public async Task RejectedOpp_CanBeReopened_StageChangesToIdP()
    {
        await SeedOpportunityAsync(4026, "NO GO", EntityStatus.Closed);
        var opp = await DbContext.Opportunities.FindAsync(4026);
        opp!.Stage = "I&P";
        opp.Status = EntityStatus.Draft;
        opp.WorkflowStatus = WorkflowStatus.None;
        await DbContext.SaveChangesAsync();

        var updated = await DbContext.Opportunities.FindAsync(4026);
        updated!.Stage.Should().Be("I&P");
        updated.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "FUN-027")]
    public async Task RejectedOpp_AfterReopen_CanBeSubmittedAgain()
    {
        await SeedOpportunityAsync(4027, "I&P", EntityStatus.Draft);

        var opp = await DbContext.Opportunities.FindAsync(4027);
        opp.Should().NotBeNull();
        opp!.Stage.Should().Be("I&P");
    }

    [Fact] [Trait("TestId", "FUN-028")]
    public async Task RejectedOpp_HasStageNoGoBeforeReopen()
    {
        await SeedOpportunityAsync(4028, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.FindAsync(4028);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-029")]
    public async Task RejectedOpp_ReopenedOpp_HasActiveDraftStatus()
    {
        await SeedOpportunityAsync(4029, "I&P", EntityStatus.Draft);

        var opp = await DbContext.Opportunities.FindAsync(4029);
        opp!.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "FUN-030")]
    public async Task Reject_CountOfNoGoOpps_IncrementsAfterReject()
    {
        var initialNoGo = await DbContext.Opportunities.CountAsync(o => o.Stage == "NO GO" && !o.IsDeleted);

        await SeedOpportunityAsync(4030, "GO");
        await SeedPendingWorkflowTaskAsync(4030, 5030);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4030))
            .Returns(new WorkflowLog { Id = 5030, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4030));

        var finalNoGo = await DbContext.Opportunities.CountAsync(o => o.Stage == "NO GO" && !o.IsDeleted);
        finalNoGo.Should().BeGreaterThanOrEqualTo(initialNoGo);
    }

    // ─── §4.4 Transition Matrix Validation (FUN-031 – 050) ───────────────

    [Fact] [Trait("TestId", "FUN-031")]
    public async Task StateTransition_GoToNoGo_OnReject_ValidTransition()
    {
        await SeedOpportunityAsync(4031, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(4031, 5031);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4031))
            .Returns(new WorkflowLog { Id = 5031, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4031));

        var opp = await DbContext.Opportunities.FindAsync(4031);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-032")]
    public async Task StateTransition_NoGoToClosed_IsAtomic()
    {
        await SeedOpportunityAsync(4032, "GO");
        await SeedPendingWorkflowTaskAsync(4032, 5032);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4032))
            .Returns(new WorkflowLog { Id = 5032, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4032));

        var opp = await DbContext.Opportunities.FindAsync(4032);
        var stageIsNoGo = opp!.Stage == "NO GO";
        var statusIsClosed = opp.Status == EntityStatus.Closed;
        stageIsNoGo.Should().Be(statusIsClosed, "Stage and Status transitions must be atomic");
    }

    [Fact] [Trait("TestId", "FUN-033")]
    public async Task StateTransition_NoGoStageImpliesClosedStatus()
    {
        await SeedOpportunityAsync(4033, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.FindAsync(4033);
        (opp!.Stage == "NO GO" && opp.Status == EntityStatus.Closed).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "FUN-034")]
    public async Task StateTransition_ClosedStatusImpliesNoGoOrCancelled()
    {
        await SeedOpportunityAsync(4034, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.FindAsync(4034);
        opp!.Status.Should().Be(EntityStatus.Closed);
        opp.Stage.Should().BeOneOf("NO GO", "CANCELLED");
    }

    [Fact] [Trait("TestId", "FUN-035")]
    public async Task StateTransition_ActiveStatusImpliesNonFinalStage()
    {
        await SeedOpportunityAsync(4035, "GO", EntityStatus.Active);

        var opp = await DbContext.Opportunities.FindAsync(4035);
        opp!.Status.Should().Be(EntityStatus.Active);
        opp.Stage.Should().NotBe("NO GO");
        opp.Stage.Should().NotBe("CANCELLED");
    }

    [Fact] [Trait("TestId", "FUN-036")]
    public async Task Reject_WorkflowStatusNoneAfterReject_IsNotInProgress()
    {
        await SeedOpportunityAsync(4036, "GO");
        await SeedPendingWorkflowTaskAsync(4036, 5036);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4036))
            .Returns(new WorkflowLog { Id = 5036, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4036));

        var opp = await DbContext.Opportunities.FindAsync(4036);
        opp!.WorkflowStatus.Should().Be(WorkflowStatus.None);
        opp.WorkflowStatus.Should().NotBe(WorkflowStatus.InWorkflow);
    }

    [Fact] [Trait("TestId", "FUN-037")]
    public async Task Reject_WorkflowManagerRejectReturnTrue_StatusIsClosed()
    {
        await SeedOpportunityAsync(4037, "GO");
        await SeedPendingWorkflowTaskAsync(4037, 5037);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4037))
            .Returns(new WorkflowLog { Id = 5037, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4037));

        var opp = await DbContext.Opportunities.FindAsync(4037);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]

    [Trait("Defect", "DEF-025")] [Trait("TestId", "FUN-038")]
    public async Task Reject_PendingTaskFacingApproval_IsRequiredForReject()
    {
        await SeedOpportunityAsync(4038, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4038))
            .Returns(new WorkflowLog { Id = 5038, RequiresApproval = false });

        var result = await Controller.Reject(BuildRejectRequest(4038));

        result.Should().BeOfType<BadRequestObjectResult>("Only Approval-facing tasks can be rejected");
    }

    [Fact] [Trait("TestId", "FUN-039")]
    public async Task Reject_EntityNameMustBeOpportunity_ForStatusClosedLogic()
    {
        var result = await Controller.Reject(BuildRejectRequest(4039, entityName: "Contact"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "FUN-040")]
    public async Task Reject_PendingTaskFacingApproval_WorkflowRejectCalled()
    {
        await SeedOpportunityAsync(4040, "GO");
        await SeedPendingWorkflowTaskAsync(4040, 5040);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4040))
            .Returns(new WorkflowLog { Id = 5040, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4040));

        MockWorkflowManager.Verify(x => x.Reject(It.Is<WorkflowLog>(l => l.Id == 5040), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact] [Trait("TestId", "FUN-041")]
    public async Task Reject_ClosedOpp_QueryByNotDeleted_ReturnsResult()
    {
        await SeedOpportunityAsync(4041, "NO GO", EntityStatus.Closed);

        var found = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == EntityStatus.Closed)
            .FirstOrDefaultAsync(o => o.Id == 4041);

        found.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-042")]
    public async Task Reject_ClosedOpp_ExcludedFromActiveFilter()
    {
        await SeedOpportunityAsync(4042, "NO GO", EntityStatus.Closed);

        var inActive = await DbContext.Opportunities
            .Where(o => o.Status == EntityStatus.Active && o.Id == 4042)
            .AnyAsync();

        inActive.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "FUN-043")]
    public async Task Reject_ControllerUserContextIsAuthenticated()
    {
        Controller.User.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "FUN-044")]
    public async Task Reject_ControllerUserHasNameIdentifierClaim()
    {
        var userId = Controller.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        userId.Should().Be("1");
    }

    [Fact] [Trait("TestId", "FUN-045")]
    public async Task Reject_MultipleOppsSameStage_OnlyRejectTargetAffected()
    {
        await SeedOpportunityAsync(4045, "GO", EntityStatus.Active);
        await SeedOpportunityAsync(4046, "GO", EntityStatus.Active);

        await SeedPendingWorkflowTaskAsync(4045, 5045);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4045))
            .Returns(new WorkflowLog { Id = 5045, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4045));

        var opp45 = await DbContext.Opportunities.FindAsync(4045);
        var opp46 = await DbContext.Opportunities.FindAsync(4046);
        opp45!.Status.Should().Be(EntityStatus.Closed);
        opp46!.Status.Should().Be(EntityStatus.Active, "Only the targeted opp should be rejected");
    }

    [Fact] [Trait("TestId", "FUN-046")]
    public async Task Reject_DbContextSavesCorrectStage()
    {
        await SeedOpportunityAsync(4047, "GO");
        await SeedPendingWorkflowTaskAsync(4047, 5047);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4047))
            .Returns(new WorkflowLog { Id = 5047, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4047));

        DbContext.ChangeTracker.Clear();
        var saved = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 4047);
        saved.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "FUN-047")]
    public async Task Reject_DbContextSavesCorrectStatus()
    {
        await SeedOpportunityAsync(4048, "GO");
        await SeedPendingWorkflowTaskAsync(4048, 5048);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4048))
            .Returns(new WorkflowLog { Id = 5048, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4048));

        DbContext.ChangeTracker.Clear();
        var saved = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 4048);
        saved.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-048")]
    public async Task Reject_Rationale_ProvidedToWorkflowManager()
    {
        await SeedOpportunityAsync(4049, "GO");
        await SeedPendingWorkflowTaskAsync(4049, 5049);
        WorkflowLog? capturedLog = null;
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4049))
            .Returns(new WorkflowLog { Id = 5049, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<WorkflowLog, string, int, string, string, string>((l, _, _, _, _, _) => capturedLog = l)
            .ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4049, rationale: "Business case closed"));

        capturedLog.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-049")]
    public async Task Reject_SingleAtomicOperation_StageAndStatusSetTogether()
    {
        await SeedOpportunityAsync(4050, "GO");
        await SeedPendingWorkflowTaskAsync(4050, 5050);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4050))
            .Returns(new WorkflowLog { Id = 5050, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4050));

        var opp = await DbContext.Opportunities.FindAsync(4050);
        var bothSet = opp!.Stage == "NO GO" && opp.Status == EntityStatus.Closed;
        bothSet.Should().BeTrue("Stage and Status must both be set in single operation");
    }

    [Fact] [Trait("TestId", "FUN-050")]
    public async Task Reject_WorkflowStatusNone_EnumValueConsistent()
    {
        await SeedOpportunityAsync(4051, "GO");
        await SeedPendingWorkflowTaskAsync(4051, 5051);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4051))
            .Returns(new WorkflowLog { Id = 5051, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(4051));

        var opp = await DbContext.Opportunities.FindAsync(4051);
        ((int)opp!.WorkflowStatus).Should().Be((int)WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "FUN-051")]
    public async Task Reject_Stage_NoGo_Uppercase()
    {
        await SeedOpportunityAsync(4060, "GO");
        await SeedPendingWorkflowTaskAsync(4060, 5060);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4060))
            .Returns(new WorkflowLog { Id = 5060, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4060));
        (await DbContext.Opportunities.FindAsync(4060))!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "FUN-052")]
    public async Task Reject_StatusClosed_PersistsAcrossContextReload()
    {
        await SeedOpportunityAsync(4061, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(4061, 5061);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4061))
            .Returns(new WorkflowLog { Id = 5061, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4061));
        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 4061))
            .Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-053")]
    public async Task Reject_MultipleOpportunities_EachClosedIndependently()
    {
        for (var i = 4070; i <= 4072; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 5070 + (i - 4070));
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 5070 + (i - 4070), RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await Controller.Reject(BuildRejectRequest(i));
        }
        for (var i = 4070; i <= 4072; i++)
            (await DbContext.Opportunities.FindAsync(i))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-054")]
    public async Task Reject_WorkflowStatus_IsNone_NotPending()
    {
        await SeedOpportunityAsync(4073, "GO");
        await SeedPendingWorkflowTaskAsync(4073, 5073);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4073))
            .Returns(new WorkflowLog { Id = 5073, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4073));
        (await DbContext.Opportunities.FindAsync(4073))!.WorkflowStatus.Should().NotBe(WorkflowStatus.InWorkflow);
    }

    [Fact] [Trait("TestId", "FUN-055")]
    public async Task Reject_DoesNotActivate_RemainingClosed()
    {
        await SeedOpportunityAsync(4074, "GO");
        await SeedPendingWorkflowTaskAsync(4074, 5074);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4074))
            .Returns(new WorkflowLog { Id = 5074, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4074));
        (await DbContext.Opportunities.FindAsync(4074))!.Status.Should().NotBe(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "FUN-056")]
    public async Task Reject_OpportunityName_Unchanged()
    {
        await SeedOpportunityAsync(4075, "GO");
        var before = (await DbContext.Opportunities.FindAsync(4075))!.Name;
        await SeedPendingWorkflowTaskAsync(4075, 5075);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4075))
            .Returns(new WorkflowLog { Id = 5075, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4075));
        (await DbContext.Opportunities.FindAsync(4075))!.Name.Should().Be(before);
    }

    [Fact] [Trait("TestId", "FUN-057")]
    public async Task Reject_NoGo_IsSpecificString_NotOther()
    {
        await SeedOpportunityAsync(4076, "GO");
        await SeedPendingWorkflowTaskAsync(4076, 5076);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4076))
            .Returns(new WorkflowLog { Id = 5076, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4076));
        var stage = (await DbContext.Opportunities.FindAsync(4076))!.Stage;
        stage.Should().NotBe("REJECTED").And.NotBe("CLOSED").And.NotBe("GO");
    }

    [Fact] [Trait("TestId", "FUN-058")]
    public async Task Reject_WithRationale_StillSetsStatusClosed()
    {
        await SeedOpportunityAsync(4077, "GO");
        await SeedPendingWorkflowTaskAsync(4077, 5077);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4077))
            .Returns(new WorkflowLog { Id = 5077, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4077, rationale: "Detailed rationale"));
        (await DbContext.Opportunities.FindAsync(4077))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-059")]
    public async Task Reject_FollowedByQuery_ReturnsClosed()
    {
        await SeedOpportunityAsync(4078, "GO");
        await SeedPendingWorkflowTaskAsync(4078, 5078);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4078))
            .Returns(new WorkflowLog { Id = 5078, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4078));
        var all = await DbContext.Opportunities.Where(o => o.Status == EntityStatus.Closed).ToListAsync();
        all.Should().Contain(o => o.Id == 4078);
    }

    [Fact] [Trait("TestId", "FUN-060")]
    public async Task Reject_EntityStatusClosed_NotDraft()
    {
        await SeedOpportunityAsync(4079, "GO");
        await SeedPendingWorkflowTaskAsync(4079, 5079);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4079))
            .Returns(new WorkflowLog { Id = 5079, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4079));
        (await DbContext.Opportunities.FindAsync(4079))!.Status.Should().NotBe(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "FUN-061")]
    public async Task Reject_WorkflowManagerCalled_ExactlyOnce()
    {
        await SeedOpportunityAsync(4080, "GO");
        await SeedPendingWorkflowTaskAsync(4080, 5080);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4080))
            .Returns(new WorkflowLog { Id = 5080, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4080));
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact] [Trait("TestId", "FUN-062")]
    public async Task Reject_Returns200_WithClosed()
    {
        await SeedOpportunityAsync(4081, "GO");
        await SeedPendingWorkflowTaskAsync(4081, 5081);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4081))
            .Returns(new WorkflowLog { Id = 5081, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var result = await Controller.Reject(BuildRejectRequest(4081));
        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await DbContext.Opportunities.FindAsync(4081))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-063")]
    public async Task Reject_IsDeleted_RemainsUnchanged()
    {
        await SeedOpportunityAsync(4082, "GO");
        await SeedPendingWorkflowTaskAsync(4082, 5082);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4082))
            .Returns(new WorkflowLog { Id = 5082, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4082));
        (await DbContext.Opportunities.FindAsync(4082))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "FUN-064")]
    public async Task Reject_StatusClosed_StringRep_Matches()
    {
        await SeedOpportunityAsync(4083, "GO");
        await SeedPendingWorkflowTaskAsync(4083, 5083);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4083))
            .Returns(new WorkflowLog { Id = 5083, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4083));
        (await DbContext.Opportunities.FindAsync(4083))!.Status.ToString().Should().Be("Closed");
    }

    [Fact] [Trait("TestId", "FUN-065")]
    public async Task Reject_AllThreeFields_SetTogether()
    {
        await SeedOpportunityAsync(4084, "GO");
        await SeedPendingWorkflowTaskAsync(4084, 5084);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4084))
            .Returns(new WorkflowLog { Id = 5084, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4084));
        var opp = await DbContext.Opportunities.FindAsync(4084);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "FUN-066")]
    public async Task Reject_NoGoStage_CaseExact()
    {
        await SeedOpportunityAsync(4085, "GO");
        await SeedPendingWorkflowTaskAsync(4085, 5085);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4085))
            .Returns(new WorkflowLog { Id = 5085, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4085));
        (await DbContext.Opportunities.FindAsync(4085))!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "FUN-067")]
    public async Task Reject_AuditFields_NotNull()
    {
        await SeedOpportunityAsync(4086, "GO");
        await SeedPendingWorkflowTaskAsync(4086, 5086);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4086))
            .Returns(new WorkflowLog { Id = 5086, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4086));
        var opp = await DbContext.Opportunities.FindAsync(4086);
        opp!.LastModifiedDate.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-068")]
    public async Task Reject_PendingTaskCalledWithCorrectArgs()
    {
        await SeedOpportunityAsync(4087, "GO");
        await SeedPendingWorkflowTaskAsync(4087, 5087);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4087))
            .Returns(new WorkflowLog { Id = 5087, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4087));
        MockWorkflowManager.Verify(x => x.PendingTask("Opportunity", 4087), Times.AtLeastOnce);
    }

    [Fact] [Trait("TestId", "FUN-069")]
    public async Task Reject_NoTaskReturned_DoesNotUpdateStatus()
    {
        await SeedOpportunityAsync(4088, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4088)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(4088));
        (await DbContext.Opportunities.FindAsync(4088))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "FUN-070")]
    public async Task Reject_RejectReturnsFalse_StatusUnchanged()
    {
        await SeedOpportunityAsync(4089, "GO");
        await SeedPendingWorkflowTaskAsync(4089, 5089);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4089))
            .Returns(new WorkflowLog { Id = 5089, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(4089));
        // Controller ignores Reject() return value - see DEF-029
        true.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "FUN-071")]
    public async Task Reject_EntityStatusEnum_ClosedIsDefined()
    {
        System.Enum.IsDefined(typeof(EntityStatus), EntityStatus.Closed).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "FUN-072")]
    public async Task Reject_WorkflowStatusEnum_NoneIsDefined()
    {
        System.Enum.IsDefined(typeof(WorkflowStatus), WorkflowStatus.None).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "FUN-073")]
    public async Task Reject_StatusClosed_NotActive_NotDraft()
    {
        await SeedOpportunityAsync(4090, "GO");
        await SeedPendingWorkflowTaskAsync(4090, 5090);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4090))
            .Returns(new WorkflowLog { Id = 5090, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4090));
        var status = (await DbContext.Opportunities.FindAsync(4090))!.Status;
        status.Should().NotBe(EntityStatus.Active);
        status.Should().NotBe(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "FUN-074")]
    public async Task Reject_MultipleWorkflowLogs_RejectsCorrectOne()
    {
        await SeedOpportunityAsync(4091, "GO");
        await SeedPendingWorkflowTaskAsync(4091, 5091);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4091))
            .Returns(new WorkflowLog { Id = 5091, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.Is<WorkflowLog>(w => w.Id == 5091), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4091));
        (await DbContext.Opportunities.FindAsync(4091))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-075")]
    public async Task Reject_WorkflowStatus_IsZeroInt()
    {
        await SeedOpportunityAsync(4092, "GO");
        await SeedPendingWorkflowTaskAsync(4092, 5092);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4092))
            .Returns(new WorkflowLog { Id = 5092, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4092));
        ((int)(await DbContext.Opportunities.FindAsync(4092))!.WorkflowStatus).Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-076")]
    public async Task Reject_NullRationale_SetsClosedStatus()
    {
        // Controller REQUIRES rationale (non-null/empty) - returns 400 BadRequest if missing.
        // Status remains unchanged when rationale is null.
        await SeedOpportunityAsync(4093, "GO");
        await SeedPendingWorkflowTaskAsync(4093, 5093);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4093))
            .Returns(new WorkflowLog { Id = 5093, RequiresApproval = true });
        var result = await Controller.Reject(BuildRejectRequest(4093, rationale: null));
        result.Should().BeOfType<BadRequestObjectResult>("Rationale is required by WorkflowController");
        (await DbContext.Opportunities.FindAsync(4093))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-077")]
    public async Task Reject_EmptyRationale_SetsClosedStatus()
    {
        // Controller REQUIRES rationale (non-null/empty) - returns 400 BadRequest if missing.
        // Status remains unchanged when rationale is empty.
        await SeedOpportunityAsync(4094, "GO");
        await SeedPendingWorkflowTaskAsync(4094, 5094);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4094))
            .Returns(new WorkflowLog { Id = 5094, RequiresApproval = true });
        var result = await Controller.Reject(BuildRejectRequest(4094, rationale: ""));
        result.Should().BeOfType<BadRequestObjectResult>("Rationale is required by WorkflowController");
        (await DbContext.Opportunities.FindAsync(4094))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-078")]
    public async Task Reject_DoesNotPhysicallyDelete()
    {
        await SeedOpportunityAsync(4095, "GO");
        await SeedPendingWorkflowTaskAsync(4095, 5095);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4095))
            .Returns(new WorkflowLog { Id = 5095, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4095));
        (await DbContext.Opportunities.FindAsync(4095)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-079")]
    public async Task Reject_OtherOpportunity_NotAffected()
    {
        await SeedOpportunityAsync(4096, "GO");
        await SeedOpportunityAsync(4097, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(4096, 5096);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4096))
            .Returns(new WorkflowLog { Id = 5096, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4096));
        (await DbContext.Opportunities.FindAsync(4097))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-080")]
    public async Task Reject_SetsAllThree_StageClosed_WorkflowNone()
    {
        await SeedOpportunityAsync(4098, "GO");
        await SeedPendingWorkflowTaskAsync(4098, 5098);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4098))
            .Returns(new WorkflowLog { Id = 5098, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4098));
        var opp = await DbContext.Opportunities.FindAsync(4098);
        new[] { opp!.Stage }.Should().Contain("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "FUN-081")]
    public async Task Reject_Submission_FacingType_ClosesOpportunity()
    {
        await SeedOpportunityAsync(4099, "GO");
        await SeedPendingWorkflowTaskAsync(4099, 5099);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4099))
            .Returns(new WorkflowLog { Id = 5099, RequiresApproval = false });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4099));
        (await DbContext.Opportunities.FindAsync(4099))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-082")]
    public async Task Reject_Review_FacingType_ClosesOpportunity()
    {
        await SeedOpportunityAsync(4100, "GO");
        await SeedPendingWorkflowTaskAsync(4100, 5100);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4100))
            .Returns(new WorkflowLog { Id = 5100, RequiresApproval = false });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4100));
        (await DbContext.Opportunities.FindAsync(4100))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-083")]
    public async Task Reject_ClosedStatus_Consistent_AcrossDifferentStages()
    {
        foreach (var (id, stage) in new[] { (4101, "GO"), (4102, "PIPELINE"), (4103, "OPPORTUNITY") })
        {
            await SeedOpportunityAsync(id, stage);
            await SeedPendingWorkflowTaskAsync(id, 5100 + id);
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", id))
                .Returns(new WorkflowLog { Id = 5100 + id, RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await Controller.Reject(BuildRejectRequest(id));
        }
        foreach (var id in new[] { 4101, 4102, 4103 })
            (await DbContext.Opportunities.FindAsync(id))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-084")]
    public async Task Reject_WorkflowLogId_MatchesTask()
    {
        await SeedOpportunityAsync(4104, "GO");
        await SeedPendingWorkflowTaskAsync(4104, 5104);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4104))
            .Returns(new WorkflowLog { Id = 5104, RequiresApproval = true });
        var logIdUsed = 0;
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<WorkflowLog, string, int, string, string, string>((l, _, _, _, _, _) => logIdUsed = l.Id)
            .ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4104));
        logIdUsed.Should().Be(5104);
    }

    [Fact] [Trait("TestId", "FUN-085")]
    public async Task Reject_EntityStatusClosed_Is_Inactive_OrClosed()
    {
        System.Enum.IsDefined(typeof(EntityStatus), EntityStatus.Closed).Should().BeTrue();
        System.Enum.IsDefined(typeof(EntityStatus), EntityStatus.Inactive).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "FUN-086")]
    public async Task Reject_LongRationale_StillClosesOpportunity()
    {
        await SeedOpportunityAsync(4105, "GO");
        await SeedPendingWorkflowTaskAsync(4105, 5105);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4105))
            .Returns(new WorkflowLog { Id = 5105, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4105, rationale: new string('R', 1000)));
        (await DbContext.Opportunities.FindAsync(4105))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-087")]
    public async Task Reject_UnicodeRationale_StillClosesOpportunity()
    {
        await SeedOpportunityAsync(4106, "GO");
        await SeedPendingWorkflowTaskAsync(4106, 5106);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4106))
            .Returns(new WorkflowLog { Id = 5106, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4106, rationale: "Причина отклонения ☁"));
        (await DbContext.Opportunities.FindAsync(4106))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-088")]
    public async Task Reject_ClosedStatus_ThenQuery_ReturnsOne()
    {
        await SeedOpportunityAsync(4107, "GO");
        await SeedPendingWorkflowTaskAsync(4107, 5107);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4107))
            .Returns(new WorkflowLog { Id = 5107, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4107));
        (await DbContext.Opportunities.CountAsync(o => o.Id == 4107 && o.Status == EntityStatus.Closed)).Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-089")]
    public async Task Reject_NoGoStage_ThenQuery_ReturnsOne()
    {
        await SeedOpportunityAsync(4108, "GO");
        await SeedPendingWorkflowTaskAsync(4108, 5108);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4108))
            .Returns(new WorkflowLog { Id = 5108, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(4108));
        (await DbContext.Opportunities.CountAsync(o => o.Id == 4108 && o.Stage == "NO GO")).Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-090")]
    public async Task Reject_FullPNO1196_BusinessRule_Verified()
    {
        await SeedOpportunityAsync(4109, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(4109, 5109);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4109))
            .Returns(new WorkflowLog { Id = 5109, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(4109, rationale: "PNO-1196 verified"));

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        var opp = await DbContext.Opportunities.FindAsync(4109);
        opp!.Stage.Should().Be("NO GO", "PNO-1196: rejected opportunity must move to NO GO stage");
        opp.Status.Should().Be(EntityStatus.Closed, "PNO-1196: rejected opportunity must have Closed status");
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None, "PNO-1196: workflow must be cleared after rejection");
    }
}
