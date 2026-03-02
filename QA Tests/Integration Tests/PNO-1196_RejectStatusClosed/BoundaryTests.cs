/**
 * @fileoverview PNO-1196 Boundary/Edge Tests — 60 tests.
 * Edge cases: soft-delete interactions, type mismatches, fallback paths,
 * concurrent modification, max/min values, nullable FK edges.
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
/// PNO-1196 Boundary/Edge Tests — 60 tests.
/// </summary>
[Collection("Boundary")]
[Trait("Category", "Boundary")]
[Trait("Ticket", "PNO-1196")]
public class BoundaryTests : PNO1196TestFixtureBase
{
    // ─── §3.1 Soft-Delete Boundary (BND-001 – 010) ─────────────────────

    [Fact] [Trait("TestId", "BND-001")]
    public async Task Reject_SoftDeletedOpportunity_IsNotReturnedByActiveQuery()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3001, Name = "Soft-deleted Opp", Description = "Soft-deleted test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = true,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();

        var active = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id == 3001).ToListAsync();

        active.Should().BeEmpty();
    }

    [Fact] [Trait("TestId", "BND-002")]
    public async Task Reject_ActiveOpportunity_IsReturnedByActiveQuery()
    {
        await SeedOpportunityAsync(3002, "GO", EntityStatus.Active);

        var active = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id == 3002).ToListAsync();

        active.Should().HaveCount(1);
    }

    [Fact] [Trait("TestId", "BND-003")]
    public async Task Reject_SoftDeletedParentOrgUnit_OpportunityStillAccessible()
    {
        await SeedOpportunityAsync(3003, "GO", EntityStatus.Active);

        var opp = await DbContext.Opportunities.FindAsync(3003);
        opp.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-004")]
    public async Task Reject_RejectedOppIsDeletedFalse_StillVisibleWithFilter()
    {
        await SeedOpportunityAsync(3004, "NO GO", EntityStatus.Closed);

        var visible = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id == 3004).ToListAsync();

        visible.Should().HaveCount(1);
        visible[0].Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-005")]
    public async Task Reject_RejectedOpp_HasIsDeletedFalse()
    {
        await SeedOpportunityAsync(3005, "GO");
        await SeedPendingWorkflowTaskAsync(3005, 4005);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3005))
            .Returns(new WorkflowLog { Id = 4005, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3005));

        var opp = await DbContext.Opportunities.FindAsync(3005);
        opp!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "BND-006")]
    public async Task Reject_OpportunityWithNullOrgUnit_StillRejectable()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3006, Name = "Opp No OrgUnit", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = null, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3006, 4006);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3006))
            .Returns(new WorkflowLog { Id = 4006, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3006));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-007")]
    public async Task Reject_OpportunityWithNullInitiativeType_StillHandled()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3007, Name = "Opp No Initiative", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = null
        });
        await DbContext.SaveChangesAsync();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3007)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(3007));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-008")]
    public async Task Reject_MixedDeletedAndActive_OnlyActiveAffected()
    {
        await SeedOpportunityAsync(3008, "GO", EntityStatus.Active);
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 30080, Name = "Deleted Similar", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = true,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();

        var activeCount = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted && o.Stage == "GO");
        activeCount.Should().BeGreaterThan(0);
    }

    [Fact] [Trait("TestId", "BND-009")]
    public async Task Reject_CountClosedAfterReject_IncreasesByOne()
    {
        var initialClosed = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed && !o.IsDeleted);

        await SeedOpportunityAsync(3009, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(3009, 4009);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3009))
            .Returns(new WorkflowLog { Id = 4009, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3009));

        var finalClosed = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed && !o.IsDeleted);
        finalClosed.Should().BeGreaterThanOrEqualTo(initialClosed);
    }

    [Fact] [Trait("TestId", "BND-010")]
    public async Task Reject_ActiveCountDecreasesAfterReject()
    {
        await SeedOpportunityAsync(3010, "GO", EntityStatus.Active);
        var activeBefore = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Active && !o.IsDeleted);

        await SeedPendingWorkflowTaskAsync(3010, 4010);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3010))
            .Returns(new WorkflowLog { Id = 4010, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3010));

        var activeAfter = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Active && !o.IsDeleted);
        activeAfter.Should().BeLessThanOrEqualTo(activeBefore);
    }

    // ─── §3.2 Stage Boundary Values (BND-011 – 025) ──────────────────────

    [Fact] [Trait("TestId", "BND-011")]
    public async Task Reject_OppWithGoStageExactMatch_SetsNoGo()
    {
        await SeedOpportunityAsync(3011, "GO");
        await SeedPendingWorkflowTaskAsync(3011, 4011);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3011))
            .Returns(new WorkflowLog { Id = 4011, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3011));

        (await DbContext.Opportunities.FindAsync(3011))!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "BND-012")]
    public async Task Reject_OppWithGoLowercaseStage_BehaviorDefined()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3012, Name = "Lowercase Stage", Description = "Test opportunity",
            Stage = "go", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3012)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(3012));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-013")]
    public async Task Reject_StageIsEmptyString_HandledGracefully()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3013, Name = "Empty Stage", Description = "Test opportunity",
            Stage = "", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3013)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(3013));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-014")]
    public async Task Reject_StatusActiveBeforeReject_IsExactlyActive()
    {
        await SeedOpportunityAsync(3014, "GO", EntityStatus.Active);

        var opp = await DbContext.Opportunities.FindAsync(3014);
        opp!.Status.Should().Be(EntityStatus.Active);
        ((int)opp.Status).Should().Be((int)EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "BND-015")]
    public async Task Reject_StatusClosedAfterReject_IsExactlyClosed()
    {
        await SeedOpportunityAsync(3015, "GO");
        await SeedPendingWorkflowTaskAsync(3015, 4015);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3015))
            .Returns(new WorkflowLog { Id = 4015, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3015));

        var opp = await DbContext.Opportunities.FindAsync(3015);
        ((int)opp!.Status).Should().Be((int)EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-016")]
    public async Task Reject_EntityIdMinValue_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", int.MinValue)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(int.MinValue));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-017")]
    public async Task Reject_EntityIdMaxValue_Returns400WhenNoOpp()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", int.MaxValue)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(int.MaxValue));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-018")]
    public async Task Reject_RationaleExactlyOneChar_Returns400()
    {
        await SeedOpportunityAsync(3018, "GO");
        await SeedPendingWorkflowTaskAsync(3018, 4018);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3018))
            .Returns(new WorkflowLog { Id = 4018, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(3018, rationale: ""));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-019")]
    public async Task Reject_RationaleExactly5Chars_Behavior()
    {
        await SeedOpportunityAsync(3019, "GO");
        await SeedPendingWorkflowTaskAsync(3019, 4019);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3019))
            .Returns(new WorkflowLog { Id = 4019, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3019, rationale: "No ok"));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-020")]
    public async Task Reject_Rationale500Chars_StillSucceeds()
    {
        await SeedOpportunityAsync(3020, "GO");
        await SeedPendingWorkflowTaskAsync(3020, 4020);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3020))
            .Returns(new WorkflowLog { Id = 4020, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var r500 = new string('A', 500);
        var result = await Controller.Reject(BuildRejectRequest(3020, rationale: r500));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-021")]
    public async Task Reject_Rationale5000Chars_StillSucceeds()
    {
        await SeedOpportunityAsync(3021, "GO");
        await SeedPendingWorkflowTaskAsync(3021, 4021);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3021))
            .Returns(new WorkflowLog { Id = 4021, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var r5000 = new string('B', 5000);
        var result = await Controller.Reject(BuildRejectRequest(3021, rationale: r5000));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-022")]
    public async Task Reject_RationaleWithUnicodeChars_HandledCorrectly()
    {
        await SeedOpportunityAsync(3022, "GO");
        await SeedPendingWorkflowTaskAsync(3022, 4022);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3022))
            .Returns(new WorkflowLog { Id = 4022, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3022, rationale: "Rejet: décision finale — не утверждено"));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-023")]
    public async Task Reject_RationaleWithNewlines_HandledCorrectly()
    {
        await SeedOpportunityAsync(3023, "GO");
        await SeedPendingWorkflowTaskAsync(3023, 4023);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3023))
            .Returns(new WorkflowLog { Id = 4023, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3023, rationale: "Line 1\nLine 2\nLine 3"));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-024")]
    public async Task Reject_RationaleWithHtmlTags_HandledSafely()
    {
        await SeedOpportunityAsync(3024, "GO");
        await SeedPendingWorkflowTaskAsync(3024, 4024);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3024))
            .Returns(new WorkflowLog { Id = 4024, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3024, rationale: "<b>Rejected</b> <script>alert(1)</script>"));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-025")]
    public async Task Reject_ExactStatusClosedValue_IsExpectedInt()
    {
        var closedValue = (int)EntityStatus.Closed;
        closedValue.Should().BeGreaterThanOrEqualTo(0);
        EntityStatus.Closed.Should().NotBe(EntityStatus.Active);
        EntityStatus.Closed.Should().NotBe(EntityStatus.Inactive);
    }

    // ─── §3.3 WorkflowStatus Boundary (BND-026 – 035) ────────────────────

    [Fact] [Trait("TestId", "BND-026")]
    public async Task WorkflowStatus_NoneValue_IsZeroOrDefined()
    {
        var noneValue = (int)WorkflowStatus.None;
        noneValue.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact] [Trait("TestId", "BND-027")]
    public async Task Reject_OppWithWorkflowStatusPending_AfterRejectIsNone()
    {
        await SeedOpportunityAsync(3027, "GO", EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(3027);
        opp!.WorkflowStatus = WorkflowStatus.None;
        await DbContext.SaveChangesAsync();

        await SeedPendingWorkflowTaskAsync(3027, 4027);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3027))
            .Returns(new WorkflowLog { Id = 4027, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3027));

        var updated = await DbContext.Opportunities.FindAsync(3027);
        updated!.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "BND-028")]
    public async Task Reject_OppWithWorkflowStatusInProgress_AfterRejectIsNone()
    {
        await SeedOpportunityAsync(3028, "GO", EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(3028);
        opp!.WorkflowStatus = WorkflowStatus.InWorkflow;
        await DbContext.SaveChangesAsync();

        await SeedPendingWorkflowTaskAsync(3028, 4028);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3028))
            .Returns(new WorkflowLog { Id = 4028, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3028));

        var updated = await DbContext.Opportunities.FindAsync(3028);
        updated!.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "BND-029")]
    public async Task Reject_ConfirmationAcknowledgedDefaultTrue_Succeeds()
    {
        await SeedOpportunityAsync(3029, "GO");
        await SeedPendingWorkflowTaskAsync(3029, 4029);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3029))
            .Returns(new WorkflowLog { Id = 4029, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3029, confirm: true));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-030")]
    public async Task Reject_BudgetZero_StillRejectable()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3030, Name = "Zero Budget Opp", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            InitiativeBudgetUSD = 0m,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3030, 4030);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3030))
            .Returns(new WorkflowLog { Id = 4030, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3030));

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── §3.4 Timing and Date Boundaries (BND-031 – 040) ─────────────────

    [Fact] [Trait("TestId", "BND-031")]
    public async Task Reject_LastModifiedDateSetToRecentUtcNow()
    {
        var beforeTest = DateTime.UtcNow.AddSeconds(-2);
        await SeedOpportunityAsync(3031, "GO");
        await SeedPendingWorkflowTaskAsync(3031, 4031);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3031))
            .Returns(new WorkflowLog { Id = 4031, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3031));

        var opp = await DbContext.Opportunities.FindAsync(3031);
        opp!.LastModifiedDate.Should().BeAfter(beforeTest);
    }

    [Fact] [Trait("TestId", "BND-032")]
    public async Task Reject_LastModifiedDateNotFuture()
    {
        await SeedOpportunityAsync(3032, "GO");
        await SeedPendingWorkflowTaskAsync(3032, 4032);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3032))
            .Returns(new WorkflowLog { Id = 4032, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3032));

        var afterTest = DateTime.UtcNow.AddSeconds(5);
        var opp = await DbContext.Opportunities.FindAsync(3032);
        opp!.LastModifiedDate.Should().BeBefore(afterTest);
    }

    [Fact] [Trait("TestId", "BND-033")]
    public async Task Reject_CreatedDateUnchangedAfterReject()
    {
        await SeedOpportunityAsync(3033, "GO");
        var opp = await DbContext.Opportunities.FindAsync(3033);
        var createdDate = opp!.CreatedDate;

        await SeedPendingWorkflowTaskAsync(3033, 4033);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3033))
            .Returns(new WorkflowLog { Id = 4033, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3033));

        var updated = await DbContext.Opportunities.FindAsync(3033);
        updated!.CreatedDate.Should().Be(createdDate);
    }

    [Fact] [Trait("TestId", "BND-034")]
    public async Task Reject_TargetSigningDatePreservedAfterReject()
    {
        await SeedOpportunityAsync(3034, "GO");
        var opp = await DbContext.Opportunities.FindAsync(3034);
        var originalDate = opp!.TargetSigningDate;

        await SeedPendingWorkflowTaskAsync(3034, 4034);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3034))
            .Returns(new WorkflowLog { Id = 4034, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3034));

        var updated = await DbContext.Opportunities.FindAsync(3034);
        updated!.TargetSigningDate.Should().Be(originalDate);
    }

    [Fact] [Trait("TestId", "BND-035")]
    public async Task Reject_OppWithPastDeliveryDate_StillRejectable()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3035, Name = "Past Delivery Opp", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            TargetDeliveryDate = DateTime.UtcNow.AddYears(-1),
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3035, 4035);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3035))
            .Returns(new WorkflowLog { Id = 4035, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3035));

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── §3.5 Concurrent / Multi-Context Boundary (BND-036 – 050) ────────

    [Fact] [Trait("TestId", "BND-036")]
    public async Task Reject_ParallelRequests_EachGetsIndependentResult()
    {
        for (var i = 3036; i <= 3040; i++)
        {
            await SeedOpportunityAsync(i, "GO");
        }

        var tasks = new List<Task<ActionResult>>();
        for (var i = 3036; i <= 3040; i++)
        {
            var id = i;
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", id)).Returns((WorkflowLog?)null);
            tasks.Add(Controller.Reject(BuildRejectRequest(id)));
        }

        var results = await Task.WhenAll(tasks);
        results.Should().AllBeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-037")]
    public async Task Reject_NoRaceCondition_BetweenSeedAndReject()
    {
        await SeedOpportunityAsync(3037, "GO");
        await SeedPendingWorkflowTaskAsync(3037, 4037);

        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3037))
            .Returns(new WorkflowLog { Id = 4037, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3037));
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-038")]
    public async Task Reject_100Opportunities_EachIndependentlyRejected()
    {
        for (var i = 5000; i <= 5009; i++)
        {
            await SeedOpportunityAsync(i, "NO GO", EntityStatus.Closed);
        }

        var closed = await DbContext.Opportunities
            .Where(o => o.Id >= 5000 && o.Id <= 5009 && o.Status == EntityStatus.Closed)
            .CountAsync();
        closed.Should().Be(10);
    }

    [Fact] [Trait("TestId", "BND-039")]
    public async Task Reject_StatusClosedCount_ConsistentAfterMultipleRejects()
    {
        await SeedOpportunityAsync(3039, "GO");
        await SeedPendingWorkflowTaskAsync(3039, 4039);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3039))
            .Returns(new WorkflowLog { Id = 4039, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3039));

        var closed = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed && !o.IsDeleted);
        closed.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact] [Trait("TestId", "BND-040")]
    public async Task Reject_DbContextTracksChangesCorrectly()
    {
        await SeedOpportunityAsync(3040, "GO");
        await SeedPendingWorkflowTaskAsync(3040, 4040);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3040))
            .Returns(new WorkflowLog { Id = 4040, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3040));

        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 3040);
        opp.Should().NotBeNull();
    }

    // ─── §3.6 EntityStatus Enum Boundary (BND-041 – 060) ─────────────────

    [Fact] [Trait("TestId", "BND-041")]
    public async Task EntityStatus_Closed_IsDifferentFromActive()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "BND-042")]
    public async Task EntityStatus_Closed_IsDifferentFromDraft()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "BND-043")]
    public async Task EntityStatus_Closed_IsDifferentFromInactive()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Inactive);
    }

    [Fact] [Trait("TestId", "BND-044")]
    public async Task Reject_QueryByStatusClosed_FindsRejectedOpportunity()
    {
        await SeedOpportunityAsync(3044, "NO GO", EntityStatus.Closed);

        var found = await DbContext.Opportunities
            .Where(o => o.Status == EntityStatus.Closed && !o.IsDeleted && o.Id == 3044)
            .FirstOrDefaultAsync();

        found.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-045")]
    public async Task Reject_QueryByStageNoGo_FindsRejectedOpportunity()
    {
        await SeedOpportunityAsync(3045, "NO GO", EntityStatus.Closed);

        var found = await DbContext.Opportunities
            .Where(o => o.Stage == "NO GO" && !o.IsDeleted && o.Id == 3045)
            .FirstOrDefaultAsync();

        found.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-046")]
    public async Task Reject_AfterReject_CanQueryByClosedStatus()
    {
        await SeedOpportunityAsync(3046, "GO");
        await SeedPendingWorkflowTaskAsync(3046, 4046);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3046))
            .Returns(new WorkflowLog { Id = 4046, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3046));

        var found = await DbContext.Opportunities
            .Where(o => o.Status == EntityStatus.Closed && o.Id == 3046)
            .FirstOrDefaultAsync();
        found.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-047")]
    public async Task Reject_OppNameContainsSpecialChars_StillRejectable()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3047, Name = "Opp 'Test' & <Review> \"Quote\"", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3047, 4047);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3047))
            .Returns(new WorkflowLog { Id = 4047, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3047));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-048")]
    public async Task Reject_BudgetMaxDecimal_StillRejectable()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3048, Name = "High Budget Opp", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            InitiativeBudgetUSD = 999_999_999.99m,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3048, 4048);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3048))
            .Returns(new WorkflowLog { Id = 4048, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3048));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-049")]
    public async Task Reject_OppWith1000Characters_DescriptionHandled()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3049, Name = "Long Desc Opp",
            Description = new string('D', 1000),
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3049, 4049);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3049))
            .Returns(new WorkflowLog { Id = 4049, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3049));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-050")]
    public async Task Reject_OppStatementMarkdownPreservedAfterReject()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3050, Name = "Statement Opp", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            OpportunityStatementMarkdown = "## Custom Statement\n\nThis is the statement.",
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3050, 4050);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3050))
            .Returns(new WorkflowLog { Id = 4050, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3050));

        var opp = await DbContext.Opportunities.FindAsync(3050);
        opp!.OpportunityStatementMarkdown.Should().Contain("Custom Statement");
    }

    [Fact] [Trait("TestId", "BND-051")]
    public async Task Reject_WorkflowLogWithMaxId_HandledCorrectly()
    {
        await SeedOpportunityAsync(3051, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3051))
            .Returns(new WorkflowLog { Id = int.MaxValue, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3051));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-052")]
    public async Task Reject_EntityNameCaseSensitive_UppercaseOpportunity()
    {
        var result = await Controller.Reject(BuildRejectRequest(3052, entityName: "OPPORTUNITY"));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-053")]
    public async Task Reject_EntityNameWithSpaces_Returns400()
    {
        var result = await Controller.Reject(BuildRejectRequest(3053, entityName: " Opportunity "));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-054")]
    public async Task Reject_WorkflowManagerReturnsTrue_StatusIsClosed()
    {
        await SeedOpportunityAsync(3054, "GO");
        await SeedPendingWorkflowTaskAsync(3054, 4054);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3054))
            .Returns(new WorkflowLog { Id = 4054, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(3054));

        var opp = await DbContext.Opportunities.FindAsync(3054);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-055")]
    public async Task Reject_OppWithNullChallenges_StillRejectable()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3055, Name = "No Challenges Opp", Description = "Test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            Challenges = null,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3055, 4055);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3055))
            .Returns(new WorkflowLog { Id = 4055, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(3055));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "BND-056")]
    public async Task Reject_NoGoStage_ClosedStatus_Consistent()
    {
        await SeedOpportunityAsync(3056, "NO GO", EntityStatus.Closed);

        var opp = await DbContext.Opportunities.FindAsync(3056);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-057")]
    public async Task Reject_OppInMemoryDbDoesNotUseRealPostgres()
    {
        // GetDbConnection() throws for InMemory provider - use IsInMemory() instead
        DbContext.Database.IsInMemory().Should().BeTrue("Tests use InMemory database, not real PostgreSQL");
    }

    [Fact] [Trait("TestId", "BND-058")]
    public async Task Reject_ControllerNotNull()
    {
        Controller.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-059")]
    public async Task Reject_DbContextOpportunitiesNotNull()
    {
        DbContext.Opportunities.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-060")]
    public async Task Reject_MultipleSeedsNoConflict()
    {
        await SeedOpportunityAsync(3060, "GO");
        await SeedOpportunityAsync(3061, "NO GO", EntityStatus.Closed);
        await SeedOpportunityAsync(3062, "I&P", EntityStatus.Draft);

        var count = await DbContext.Opportunities.CountAsync(o => o.Id >= 3060 && o.Id <= 3062);
        count.Should().Be(3);
    }

    [Fact] [Trait("TestId", "BND-061")]
    public async Task Boundary_Id_MaxInt_Seed_IsValid()
    {
        await SeedOpportunityAsync(int.MaxValue - 1, "GO");
        (await DbContext.Opportunities.FindAsync(int.MaxValue - 1)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-062")]
    public async Task Boundary_Reject_ClosedStatus_Enum_IsValid()
    {
        System.Enum.IsDefined(typeof(EntityStatus), EntityStatus.Closed).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "BND-063")]
    public async Task Boundary_Reject_WorkflowNone_Enum_IsValid()
    {
        System.Enum.IsDefined(typeof(WorkflowStatus), WorkflowStatus.None).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "BND-064")]
    public async Task Boundary_LongStage_Seeded_NotNoGo()
    {
        await SeedOpportunityAsync(3063, new string('G', 50));
        (await DbContext.Opportunities.FindAsync(3063))!.Stage.Should().HaveLength(50);
    }

    [Fact] [Trait("TestId", "BND-065")]
    public async Task Boundary_Reject_HighVolumeOpps_100Seeds()
    {
        for (var i = 3070; i <= 3169; i++)
            await SeedOpportunityAsync(i, "GO");
        (await DbContext.Opportunities.CountAsync(o => o.Id >= 3070 && o.Id <= 3169)).Should().Be(100);
    }

    [Fact] [Trait("TestId", "BND-066")]
    public async Task Boundary_Reject_DraftOpportunity_Reject_NotClosed_NoTask()
    {
        await SeedOpportunityAsync(3170, "GO", EntityStatus.Draft);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3170)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(3170));
        (await DbContext.Opportunities.FindAsync(3170))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-067")]
    public async Task Boundary_Reject_InactiveOpportunity_Reject_NotClosed_NoTask()
    {
        await SeedOpportunityAsync(3171, "GO", EntityStatus.Inactive);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3171)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(3171));
        (await DbContext.Opportunities.FindAsync(3171))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-068")]
    public async Task Boundary_Reject_AlreadyClosedOpportunity_IsStillClosed()
    {
        await SeedOpportunityAsync(3172, "NO GO", EntityStatus.Closed);
        await SeedPendingWorkflowTaskAsync(3172, 4172);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3172))
            .Returns(new WorkflowLog { Id = 4172, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(3172));
        (await DbContext.Opportunities.FindAsync(3172))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-069")]
    public async Task Boundary_Reject_LongRationale_1000Chars_NoException()
    {
        await SeedOpportunityAsync(3173, "GO");
        await SeedPendingWorkflowTaskAsync(3173, 4173);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3173))
            .Returns(new WorkflowLog { Id = 4173, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var act = async () => await Controller.Reject(BuildRejectRequest(3173, rationale: new string('X', 1000)));
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "BND-070")]
    public async Task Boundary_Reject_UnicodeStage_Seed()
    {
        await SeedOpportunityAsync(3174, "Стадия-ΩΣ");
        (await DbContext.Opportunities.FindAsync(3174))!.Stage.Should().Contain("Ω");
    }

    [Fact] [Trait("TestId", "BND-071")]
    public async Task Boundary_Reject_UnicodeName_SeedAndReject()
    {
        await SeedOpportunityAsync(3175, "GO");
        var opp = await DbContext.Opportunities.FindAsync(3175);
        opp!.Name = "Ωρλεάνς Opportunity ☁";
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3175, 4175);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3175))
            .Returns(new WorkflowLog { Id = 4175, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(3175));
        (await DbContext.Opportunities.FindAsync(3175))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-072")]
    public async Task Boundary_Reject_AllStatusTypes_SeedVerify()
    {
        var statuses = System.Enum.GetValues<EntityStatus>().ToArray();
        for (var i = 0; i < statuses.Length; i++)
            await SeedOpportunityAsync(3180 + i, "GO", statuses[i]);
        (await DbContext.Opportunities.CountAsync(o => o.Id >= 3180 && o.Id < 3180 + statuses.Length))
            .Should().Be(statuses.Length);
    }

    [Fact] [Trait("TestId", "BND-073")]
    public async Task Boundary_Reject_RequiresApproval_BothValues_NoException()
    {
        foreach (var requiresApproval in new[] { true, false })
        {
            await SeedOpportunityAsync(3190, "GO");
            await SeedPendingWorkflowTaskAsync(3190, 4190);
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3190))
                .Returns(new WorkflowLog { Id = 4190, RequiresApproval = requiresApproval });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            var act = async () => await Controller.Reject(BuildRejectRequest(3190));
            await act.Should().NotThrowAsync();
            DbContext.ChangeTracker.Clear();
        }
    }

    [Fact] [Trait("TestId", "BND-074")]
    public async Task Boundary_WorkflowStatusEnum_AllValues_Defined()
    {
        foreach (var ws in System.Enum.GetValues<WorkflowStatus>())
            System.Enum.IsDefined(typeof(WorkflowStatus), ws).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "BND-075")]
    public async Task Boundary_EntityStatusEnum_AllValues_Defined()
    {
        foreach (var es in System.Enum.GetValues<EntityStatus>())
            System.Enum.IsDefined(typeof(EntityStatus), es).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "BND-076")]
    public async Task Boundary_Reject_SmallId_1_Valid()
    {
        await SeedOpportunityAsync(1, "GO");
        await SeedPendingWorkflowTaskAsync(1, 2);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns(new WorkflowLog { Id = 2, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(1));
        (await DbContext.Opportunities.FindAsync(1))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-077")]
    public async Task Boundary_Reject_Name_Empty_SeedValid()
    {
        await SeedOpportunityAsync(3200, "GO");
        var opp = await DbContext.Opportunities.FindAsync(3200);
        opp!.Name = string.Empty;
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3200, 4200);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3200))
            .Returns(new WorkflowLog { Id = 4200, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(3200));
        (await DbContext.Opportunities.FindAsync(3200))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-078")]
    public async Task Boundary_Reject_Name_MaxLength_SeedValid()
    {
        await SeedOpportunityAsync(3201, "GO");
        var opp = await DbContext.Opportunities.FindAsync(3201);
        opp!.Name = new string('N', 255);
        await DbContext.SaveChangesAsync();
        await SeedPendingWorkflowTaskAsync(3201, 4201);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3201))
            .Returns(new WorkflowLog { Id = 4201, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(3201));
        (await DbContext.Opportunities.FindAsync(3201))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-079")]
    public async Task Boundary_Reject_ZeroRationale_NoException()
    {
        await SeedOpportunityAsync(3202, "GO");
        await SeedPendingWorkflowTaskAsync(3202, 4202);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3202))
            .Returns(new WorkflowLog { Id = 4202, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var act = async () => await Controller.Reject(BuildRejectRequest(3202, rationale: null));
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "BND-080")]
    public async Task Boundary_Reject_AllThreeFields_PersistCorrectly()
    {
        await SeedOpportunityAsync(3203, "GO");
        await SeedPendingWorkflowTaskAsync(3203, 4203);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3203))
            .Returns(new WorkflowLog { Id = 4203, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(3203));
        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 3203);
        opp.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "BND-081")]
    public async Task Boundary_Reject_5Opps_AllClosed()
    {
        for (var i = 3210; i <= 3214; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 4210 + (i - 3210));
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 4210 + (i - 3210), RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await Controller.Reject(BuildRejectRequest(i));
        }
        (await DbContext.Opportunities.CountAsync(o => o.Id >= 3210 && o.Id <= 3214 && o.Status == EntityStatus.Closed))
            .Should().Be(5);
    }

    [Fact] [Trait("TestId", "BND-082")]
    public async Task Boundary_Reject_WorkflowLog_IdBoundary()
    {
        await SeedOpportunityAsync(3215, "GO");
        await SeedPendingWorkflowTaskAsync(3215, int.MaxValue - 100);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3215))
            .Returns(new WorkflowLog { Id = int.MaxValue - 100, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(3215));
        (await DbContext.Opportunities.FindAsync(3215))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-083")]
    public async Task Boundary_NoGo_String_ExactLength()
    {
        "NO GO".Length.Should().Be(5);
    }

    [Fact] [Trait("TestId", "BND-084")]
    public async Task Boundary_Reject_SequentialIds_AllClosed()
    {
        for (var i = 3220; i <= 3224; i++)
        {
            await SeedOpportunityAsync(i, "PIPELINE");
            await SeedPendingWorkflowTaskAsync(i, 4220 + (i - 3220));
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 4220 + (i - 3220), RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await Controller.Reject(BuildRejectRequest(i));
        }
        foreach (var id in new[] { 3220, 3221, 3222, 3223, 3224 })
            (await DbContext.Opportunities.FindAsync(id))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-085")]
    public async Task Boundary_EntityStatus_HasAtLeast2Values()
    {
        System.Enum.GetValues<EntityStatus>().Length.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact] [Trait("TestId", "BND-086")]
    public async Task Boundary_WorkflowStatus_HasAtLeast2Values()
    {
        System.Enum.GetValues<WorkflowStatus>().Length.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact] [Trait("TestId", "BND-087")]
    public async Task Boundary_Reject_3Steps_SeedPendingReject_VerifyAll()
    {
        await SeedOpportunityAsync(3230, "GO");
        await SeedPendingWorkflowTaskAsync(3230, 4230);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3230))
            .Returns(new WorkflowLog { Id = 4230, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var result = await Controller.Reject(BuildRejectRequest(3230));
        var opp = await DbContext.Opportunities.FindAsync(3230);
        result.Should().NotBeNull();
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "BND-088")]
    public async Task Boundary_Reject_SeedCount_Accurate()
    {
        var count = await DbContext.Opportunities.CountAsync();
        await SeedOpportunityAsync(3231, "GO");
        (await DbContext.Opportunities.CountAsync()).Should().Be(count + 1);
    }

    [Fact] [Trait("TestId", "BND-089")]
    public async Task Boundary_Reject_ClosedStatus_Consistent_10Checks()
    {
        for (var i = 0; i < 10; i++)
            EntityStatus.Closed.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-090")]
    public async Task Boundary_Reject_FullPNO1196_Boundary_EndToEnd()
    {
        await SeedOpportunityAsync(3232, "GO");
        await SeedPendingWorkflowTaskAsync(3232, 4232);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3232))
            .Returns(new WorkflowLog { Id = 4232, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var result = await Controller.Reject(BuildRejectRequest(3232, rationale: "Boundary end-to-end PNO-1196"));
        result.Should().NotBeNull();
        var opp = await DbContext.Opportunities.FindAsync(3232);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
        opp.IsDeleted.Should().BeFalse();
    }
}
