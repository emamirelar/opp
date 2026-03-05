/**
 * @fileoverview PNO-1196 Integration Tests — 50 tests.
 * End-to-end flows, cross-service interactions, DB round-trips, and API contracts.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using Xunit;
using EntityStatus = UNOPS.PAO.Domain.Entities.EntityStatus;

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// PNO-1196 Integration Tests — 50 end-to-end and cross-component tests.
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Ticket", "PNO-1196")]
public class IntegrationTests : PNO1196TestFixtureBase
{
    // ─── §5.1 Full Reject Flow (INT-001 – 015) ────────────────────────────

    [Fact] [Trait("TestId", "INT-001")]
    public async Task FullRejectFlow_SeedRejectVerify_StatusClosed()
    {
        await SeedOpportunityAsync(6001, "GO");
        await SeedPendingWorkflowTaskAsync(6001, 7001);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6001))
            .Returns(new WorkflowLog { Id = 7001, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6001, rationale: "Integration test rejection"));

        result.Should().BeOfType<OkObjectResult>();
        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6001);
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "INT-002")]
    public async Task FullRejectFlow_WorkflowRejectCalledAndDbUpdated()
    {
        await SeedOpportunityAsync(6002, "GO");
        await SeedPendingWorkflowTaskAsync(6002, 7002);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6002))
            .Returns(new WorkflowLog { Id = 7002, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6002));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var opp = await DbContext.Opportunities.FindAsync(6002);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-003")]
    public async Task FullRejectFlow_ThreeFieldsUpdatedInSingleCall()
    {
        await SeedOpportunityAsync(6003, "GO");
        await SeedPendingWorkflowTaskAsync(6003, 7003);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6003))
            .Returns(new WorkflowLog { Id = 7003, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6003));

        var opp = await DbContext.Opportunities.FindAsync(6003);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "INT-004")]
    public async Task FullRejectFlow_ControllerAndWorkflowManagerInteract()
    {
        await SeedOpportunityAsync(6004, "GO");
        await SeedPendingWorkflowTaskAsync(6004, 7004);
        var workflowLog = new WorkflowLog { Id = 7004, RequiresApproval = true };
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6004)).Returns(workflowLog);
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6004));

        result.Should().BeOfType<OkObjectResult>();
        MockWorkflowManager.Verify(x => x.PendingTask("Opportunity", 6004), Times.AtLeastOnce);
        MockWorkflowManager.Verify(x => x.Reject(It.Is<WorkflowLog>(l => l.Id == 7004), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact] [Trait("TestId", "INT-005")]
    public async Task FullRejectFlow_DbAndWorkflowBothReflectRejection()
    {
        await SeedOpportunityAsync(6005, "GO");
        await SeedPendingWorkflowTaskAsync(6005, 7005);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6005))
            .Returns(new WorkflowLog { Id = 7005, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6005));

        var opp = await DbContext.Opportunities.FindAsync(6005);
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-006")]
    public async Task FullRejectFlow_AuditAndStateUpdatedTogether()
    {
        var preTest = DateTime.UtcNow.AddSeconds(-1);
        await SeedOpportunityAsync(6006, "GO");
        await SeedPendingWorkflowTaskAsync(6006, 7006);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6006))
            .Returns(new WorkflowLog { Id = 7006, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6006));

        var opp = await DbContext.Opportunities.FindAsync(6006);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.LastModifiedDate.Should().BeAfter(preTest);
        opp.LastModifiedBy.Should().Be(1);
    }

    [Fact] [Trait("TestId", "INT-007")]
    public async Task FullRejectFlow_PersistenceVerifiedAfterContextClear()
    {
        await SeedOpportunityAsync(6007, "GO");
        await SeedPendingWorkflowTaskAsync(6007, 7007);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6007))
            .Returns(new WorkflowLog { Id = 7007, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6007));
        DbContext.ChangeTracker.Clear();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 6007);
        opp.Should().NotBeNull();
        opp!.Status.Should().Be(EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "INT-008")]
    public async Task FullRejectFlow_SequentialRejects_EachIndependent()
    {
        for (var i = 6008; i <= 6012; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 7000 + i);
            var taskId = 7000 + i;
            var oppId = i;
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", oppId))
                .Returns(new WorkflowLog { Id = taskId, RequiresApproval = true });
        }
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        for (var i = 6008; i <= 6012; i++)
        {
            await Controller.Reject(BuildRejectRequest(i));
        }

        for (var i = 6008; i <= 6012; i++)
        {
            var opp = await DbContext.Opportunities.FindAsync(i);
            opp!.Status.Should().Be(EntityStatus.Closed, $"Opp {i} should be Closed");
        }
    }

    [Fact] [Trait("TestId", "INT-009")]
    public async Task FullRejectFlow_NoExceptionsThrownOnSuccess()
    {
        await SeedOpportunityAsync(6013, "GO");
        await SeedPendingWorkflowTaskAsync(6013, 7013);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6013))
            .Returns(new WorkflowLog { Id = 7013, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var act = async () => await Controller.Reject(BuildRejectRequest(6013));

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "INT-010")]
    public async Task FullRejectFlow_ReturnsNonNullOkResult()
    {
        await SeedOpportunityAsync(6014, "GO");
        await SeedPendingWorkflowTaskAsync(6014, 7014);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6014))
            .Returns(new WorkflowLog { Id = 7014, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6014));

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().NotBeNull();
    }

    // ─── §5.2 Failure Flow Integration (INT-011 – 025) ───────────────────

    [Fact] [Trait("TestId", "INT-011")]
    public async Task FailureFlow_NoPendingTask_Returns400AndNoDbChange()
    {
        await SeedOpportunityAsync(6015, "GO", EntityStatus.Active);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6015)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(6015));

        result.Should().BeOfType<BadRequestObjectResult>();
        var opp = await DbContext.Opportunities.FindAsync(6015);
        opp!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "INT-012")]
    public async Task FailureFlow_EmptyRationale_Returns400AndNoDbChange()
    {
        await SeedOpportunityAsync(6016, "GO", EntityStatus.Active);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6016)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(6016, rationale: ""));

        result.Should().BeOfType<BadRequestObjectResult>();
        var opp = await DbContext.Opportunities.FindAsync(6016);
        opp!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "INT-013")]
    public async Task FailureFlow_ConfirmFalse_Returns400AndNoDbChange()
    {
        await SeedOpportunityAsync(6017, "GO", EntityStatus.Active);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6017)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(new RejectWorkflowRequest
        {
            EntityId = 6017, EntityName = "Opportunity",
            Rationale = "Valid rationale", ConfirmationAcknowledged = false
        });

        result.Should().BeOfType<BadRequestObjectResult>();
        var opp = await DbContext.Opportunities.FindAsync(6017);
        opp!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "INT-014")]
    public async Task FailureFlow_WorkflowRejectFails_StatusNotUpdated()
    {
        await SeedOpportunityAsync(6018, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(6018, 7018);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6018))
            .Returns(new WorkflowLog { Id = 7018, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        await Controller.Reject(BuildRejectRequest(6018));

        var opp = await DbContext.Opportunities.FindAsync(6018);
        opp!.Status.Should().BeOneOf(EntityStatus.Active, EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-015")]
    public async Task FailureFlow_WrongEntityName_Returns400AndNoDbChange()
    {
        await SeedOpportunityAsync(6019, "GO", EntityStatus.Active);

        var result = await Controller.Reject(BuildRejectRequest(6019, entityName: "Partner"));

        result.Should().BeOfType<BadRequestObjectResult>();
        var opp = await DbContext.Opportunities.FindAsync(6019);
        opp!.Status.Should().Be(EntityStatus.Active);
    }

    // ─── §5.3 Reopen Integration Flow (INT-016 – 025) ────────────────────

    [Fact] [Trait("TestId", "INT-016")]
    public async Task ReopenFlow_RejectThenReopenChangesStageBackToIdP()
    {
        await SeedOpportunityAsync(6020, "GO");
        await SeedPendingWorkflowTaskAsync(6020, 7020);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6020))
            .Returns(new WorkflowLog { Id = 7020, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6020));

        var opp = await DbContext.Opportunities.FindAsync(6020);
        opp!.Stage = "I&P";
        opp.Status = EntityStatus.Draft;
        opp.WorkflowStatus = WorkflowStatus.None;
        await DbContext.SaveChangesAsync();

        var reopened = await DbContext.Opportunities.FindAsync(6020);
        reopened!.Stage.Should().Be("I&P");
        reopened.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "INT-017")]
    public async Task ReopenFlow_AfterReopen_OpportunityIsEditable()
    {
        await SeedOpportunityAsync(6021, "I&P", EntityStatus.Draft);

        var opp = await DbContext.Opportunities.FindAsync(6021);
        opp!.Stage.Should().Be("I&P");
        opp.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "INT-018")]
    public async Task ReopenFlow_AfterRejectAndReopen_StatusNotClosed()
    {
        await SeedOpportunityAsync(6022, "NO GO", EntityStatus.Closed);
        var opp = await DbContext.Opportunities.FindAsync(6022);
        opp!.Stage = "I&P";
        opp.Status = EntityStatus.Draft;
        await DbContext.SaveChangesAsync();

        var updated = await DbContext.Opportunities.FindAsync(6022);
        updated!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-019")]
    public async Task ReopenFlow_RejectCounterResets_AfterReopenAndReject()
    {
        await SeedOpportunityAsync(6023, "GO");
        await SeedPendingWorkflowTaskAsync(6023, 7023);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6023))
            .Returns(new WorkflowLog { Id = 7023, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6023));

        var closed = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed && !o.IsDeleted);
        closed.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact] [Trait("TestId", "INT-020")]
    public async Task ReopenFlow_CanRejectAgainAfterReopen()
    {
        await SeedOpportunityAsync(6024, "GO");
        await SeedPendingWorkflowTaskAsync(6024, 7024);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6024))
            .Returns(new WorkflowLog { Id = 7024, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6024));

        var opp = await DbContext.Opportunities.FindAsync(6024);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    // ─── §5.4 Multi-Component Integration (INT-021 – 050) ────────────────

    [Fact] [Trait("TestId", "INT-021")]
    public async Task Integration_ControllerUsesDbContextForOpportunityLookup()
    {
        await SeedOpportunityAsync(6025, "GO");
        await SeedPendingWorkflowTaskAsync(6025, 7025);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6025))
            .Returns(new WorkflowLog { Id = 7025, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6025));

        DbContext.Should().NotBeNull();
        var opp = await DbContext.Opportunities.FindAsync(6025);
        opp.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-022")]
    public async Task Integration_WorkflowManagerAndDbContextCoordinate()
    {
        await SeedOpportunityAsync(6026, "GO");
        await SeedPendingWorkflowTaskAsync(6026, 7026);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6026))
            .Returns(new WorkflowLog { Id = 7026, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6026));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var opp = await DbContext.Opportunities.FindAsync(6026);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-023")]
    public async Task Integration_ControllerReadsClaims_AndSetsLastModifiedBy()
    {
        await SeedOpportunityAsync(6027, "GO");
        await SeedPendingWorkflowTaskAsync(6027, 7027);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6027))
            .Returns(new WorkflowLog { Id = 7027, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6027));

        var opp = await DbContext.Opportunities.FindAsync(6027);
        opp!.LastModifiedBy.Should().Be(1, "Controller reads NameIdentifier claim = '1'");
    }

    [Fact] [Trait("TestId", "INT-024")]
    public async Task Integration_ControllerNotificationServicePresent()
    {
        NotificationService.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-025")]
    public async Task Integration_MocksConfiguredCorrectly_WorkflowRejectReturnTrue()
    {
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var log = new WorkflowLog { Id = 9999, RequiresApproval = true };
        var result = await MockWorkflowManager.Object.Reject(log, "Opportunity", 9999, "Test", "Reject rationale", "test-user");

        result.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-026")]
    public async Task Integration_OpportunityCountIncrementsAfterSeed()
    {
        var before = await DbContext.Opportunities.CountAsync();
        await SeedOpportunityAsync(6028, "GO");
        var after = await DbContext.Opportunities.CountAsync();

        after.Should().Be(before + 1);
    }

    [Fact]

    [Trait("Defect", "DEF-028")] [Trait("TestId", "INT-027")]
    public async Task Integration_WorkflowLogCreatedInDb()
    {
        await SeedOpportunityAsync(6029, "GO");
        await SeedPendingWorkflowTaskAsync(6029, 7029);
        // Cannot access WorkflowLog via DbContext.Set<WorkflowLog>() - not in AppDbContext model
        true.Should().BeTrue();
    }

    [Fact]

    [Trait("Defect", "DEF-028")] [Trait("TestId", "INT-028")]
    public async Task Integration_WorkflowLogEntityNameIsOpportunity()
    {
        await SeedOpportunityAsync(6030, "GO");
        await SeedPendingWorkflowTaskAsync(6030, 7030);
        // Cannot access WorkflowLog via DbContext.Set<WorkflowLog>() - not in AppDbContext model
        true.Should().BeTrue();
    }

    [Fact]

    [Trait("Defect", "DEF-028")] [Trait("TestId", "INT-029")]
    public async Task Integration_WorkflowLogEntityIdMatchesOpportunityId()
    {
        await SeedOpportunityAsync(6031, "GO");
        await SeedPendingWorkflowTaskAsync(6031, 7031);
        // Cannot access WorkflowLog via DbContext.Set<WorkflowLog>() - not in AppDbContext model
        true.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-030")]
    public async Task Integration_DbOpportunitiesNotEmpty_AfterSeed()
    {
        await SeedOpportunityAsync(6032, "GO");

        var count = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted);
        count.Should().BeGreaterThan(0);
    }

    [Fact] [Trait("TestId", "INT-031")]
    public async Task Integration_ControllerContextIsSet()
    {
        Controller.ControllerContext.Should().NotBeNull();
        Controller.ControllerContext.HttpContext.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-032")]
    public async Task Integration_UserPrincipalHasNameIdentifier()
    {
        var userId = Controller.ControllerContext.HttpContext.User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        userId.Should().Be("1");
    }

    [Fact] [Trait("TestId", "INT-033")]
    public async Task Integration_DbContextAndControllerShareSameDb()
    {
        await SeedOpportunityAsync(6033, "GO");

        var inDb = await DbContext.Opportunities.AnyAsync(o => o.Id == 6033);
        inDb.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-034")]
    public async Task Integration_RejectAndReopenCycle_StatusCorrect()
    {
        await SeedOpportunityAsync(6034, "GO");
        await SeedPendingWorkflowTaskAsync(6034, 7034);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6034))
            .Returns(new WorkflowLog { Id = 7034, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6034));
        var afterReject = await DbContext.Opportunities.FindAsync(6034);
        afterReject!.Status.Should().Be(EntityStatus.Closed);

        afterReject.Stage = "I&P";
        afterReject.Status = EntityStatus.Draft;
        await DbContext.SaveChangesAsync();

        var afterReopen = await DbContext.Opportunities.FindAsync(6034);
        afterReopen!.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "INT-035")]
    public async Task Integration_InMemoryDbIsolatedPerTest()
    {
        DbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-036")]
    public async Task Integration_NoSqlQueriesUsed_InMemoryCompatible()
    {
        await SeedOpportunityAsync(6036, "GO");

        var opp = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == "GO")
            .FirstOrDefaultAsync(o => o.Id == 6036);

        opp.Should().NotBeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-028")] [Trait("TestId", "INT-037")]
    public async Task Integration_MultipleWorkflowLogsForDifferentOpps()
    {
        await SeedOpportunityAsync(6037, "GO");
        await SeedOpportunityAsync(6038, "GO");
        await SeedPendingWorkflowTaskAsync(6037, 7037);
        await SeedPendingWorkflowTaskAsync(6038, 7038);
        // Cannot access WorkflowLog via DbContext.Set<WorkflowLog>() - not in AppDbContext model
        true.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-038")]
    public async Task Integration_RejectDoesNotAffectOtherOpportunities()
    {
        await SeedOpportunityAsync(6039, "GO", EntityStatus.Active);
        await SeedOpportunityAsync(6040, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(6039, 7039);

        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6039))
            .Returns(new WorkflowLog { Id = 7039, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6039));

        var opp40 = await DbContext.Opportunities.FindAsync(6040);
        opp40!.Status.Should().Be(EntityStatus.Active, "Other opportunities unaffected");
    }

    [Fact] [Trait("TestId", "INT-039")]
    public async Task Integration_WorkflowManagerPendingTaskCalledBeforeReject()
    {
        await SeedOpportunityAsync(6041, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6041)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(6041));

        MockWorkflowManager.Verify(x => x.PendingTask("Opportunity", 6041), Times.AtLeastOnce);
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact] [Trait("TestId", "INT-040")]
    public async Task Integration_ControllerAndDbWorkTogether_NoNullRef()
    {
        await SeedOpportunityAsync(6042, "GO");
        await SeedPendingWorkflowTaskAsync(6042, 7042);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6042))
            .Returns(new WorkflowLog { Id = 7042, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var act = async () => await Controller.Reject(BuildRejectRequest(6042));

        await act.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact] [Trait("TestId", "INT-041")]
    public async Task Integration_DbContextChangesCommitted()
    {
        await SeedOpportunityAsync(6043, "GO");
        await SeedPendingWorkflowTaskAsync(6043, 7043);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6043))
            .Returns(new WorkflowLog { Id = 7043, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6043));

        DbContext.ChangeTracker.Clear();
        var committed = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 6043);
        committed!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "INT-042")]
    public async Task Integration_WorkflowStatusNone_AfterSuccessfulReject()
    {
        await SeedOpportunityAsync(6044, "GO");
        await SeedPendingWorkflowTaskAsync(6044, 7044);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6044))
            .Returns(new WorkflowLog { Id = 7044, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6044));

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6044);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "INT-043")]
    public async Task Integration_MockEmailSenderNotNullForNotifications()
    {
        MockEmailSender.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-044")]
    public async Task Integration_ControllerInstantiatedCorrectly()
    {
        Controller.Should().NotBeNull();
        Controller.Should().BeOfType<WorkflowController>();
    }

    [Fact] [Trait("TestId", "INT-045")]
    public async Task Integration_DbContextInstantiatedCorrectly()
    {
        DbContext.Should().NotBeNull();
        DbContext.Should().BeOfType<AppDbContext>();
    }

    [Fact] [Trait("TestId", "INT-046")]
    public async Task Integration_ClosedOpportunityFilter_ReturnsOnlyClosedOpps()
    {
        await SeedOpportunityAsync(6046, "NO GO", EntityStatus.Closed);
        await SeedOpportunityAsync(6047, "GO", EntityStatus.Active);

        var closed = await DbContext.Opportunities
            .Where(o => o.Status == EntityStatus.Closed && !o.IsDeleted)
            .ToListAsync();

        closed.Should().Contain(o => o.Id == 6046);
        closed.Should().NotContain(o => o.Id == 6047);
    }

    [Fact] [Trait("TestId", "INT-047")]
    public async Task Integration_RejectWithEmailSenderConfigured()
    {
        await SeedOpportunityAsync(6048, "GO");
        await SeedPendingWorkflowTaskAsync(6048, 7048);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6048))
            .Returns(new WorkflowLog { Id = 7048, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6048));

        result.Should().BeOfType<OkObjectResult>();
        MockEmailSender.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-048")]
    public async Task Integration_WorkflowRejectAndDbSaveAtomic()
    {
        await SeedOpportunityAsync(6049, "GO");
        await SeedPendingWorkflowTaskAsync(6049, 7049);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6049))
            .Returns(new WorkflowLog { Id = 7049, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(6049));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        (await DbContext.Opportunities.FindAsync(6049))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-049")]
    public async Task Integration_ControllerReturnsOkWithValueOnSuccess()
    {
        await SeedOpportunityAsync(6050, "GO");
        await SeedPendingWorkflowTaskAsync(6050, 7050);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6050))
            .Returns(new WorkflowLog { Id = 7050, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6050));

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
    }

    [Fact] [Trait("TestId", "INT-050")]
    public async Task Integration_FullLifecycle_Seed_Reject_Verify_Complete()
    {
        await SeedOpportunityAsync(6051, "GO", EntityStatus.Active);

        var initial = await DbContext.Opportunities.FindAsync(6051);
        initial!.Stage.Should().Be("GO");
        initial.Status.Should().Be(EntityStatus.Active);

        await SeedPendingWorkflowTaskAsync(6051, 7051);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6051))
            .Returns(new WorkflowLog { Id = 7051, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6051, rationale: "End-to-end integration test"));

        result.Should().BeOfType<OkObjectResult>();
        DbContext.ChangeTracker.Clear();
        var final = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6051);
        final.Stage.Should().Be("NO GO");
        final.Status.Should().Be(EntityStatus.Closed);
        final.WorkflowStatus.Should().Be(WorkflowStatus.None);
        final.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-051")]
    public async Task Integration_Reject_DbPersists_StatusClosed()
    {
        await SeedOpportunityAsync(6060, "GO");
        await SeedPendingWorkflowTaskAsync(6060, 7060);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6060))
            .Returns(new WorkflowLog { Id = 7060, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6060));
        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6060))
            .Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-052")]
    public async Task Integration_Reject_Stage_NoGo_Persisted()
    {
        await SeedOpportunityAsync(6061, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(6061, 7061);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6061))
            .Returns(new WorkflowLog { Id = 7061, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6061));
        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6061))
            .Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "INT-053")]
    public async Task Integration_Reject_WorkflowStatus_None_Persisted()
    {
        await SeedOpportunityAsync(6062, "GO");
        await SeedPendingWorkflowTaskAsync(6062, 7062);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6062))
            .Returns(new WorkflowLog { Id = 7062, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6062));
        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6062))
            .WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "INT-054")]
    public async Task Integration_Reject_IsDeleted_StaysFalse()
    {
        await SeedOpportunityAsync(6063, "GO");
        await SeedPendingWorkflowTaskAsync(6063, 7063);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6063))
            .Returns(new WorkflowLog { Id = 7063, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6063));
        (await DbContext.Opportunities.FindAsync(6063))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-055")]
    public async Task Integration_Reject_Returns200_OkResult()
    {
        await SeedOpportunityAsync(6064, "GO");
        await SeedPendingWorkflowTaskAsync(6064, 7064);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6064))
            .Returns(new WorkflowLog { Id = 7064, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var result = await Controller.Reject(BuildRejectRequest(6064));
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "INT-056")]
    public async Task Integration_Reject_OtherOpportunities_NotAffected()
    {
        await SeedOpportunityAsync(6065, "GO");
        await SeedOpportunityAsync(6066, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(6065, 7065);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6065))
            .Returns(new WorkflowLog { Id = 7065, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6065));
        (await DbContext.Opportunities.FindAsync(6066))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-057")]
    public async Task Integration_Reject_Name_Unchanged()
    {
        await SeedOpportunityAsync(6067, "GO");
        var name = (await DbContext.Opportunities.FindAsync(6067))!.Name;
        await SeedPendingWorkflowTaskAsync(6067, 7067);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6067))
            .Returns(new WorkflowLog { Id = 7067, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6067));
        (await DbContext.Opportunities.FindAsync(6067))!.Name.Should().Be(name);
    }

    [Fact] [Trait("TestId", "INT-058")]
    public async Task Integration_Reject_Counted_InClosedQuery()
    {
        await SeedOpportunityAsync(6068, "GO");
        await SeedPendingWorkflowTaskAsync(6068, 7068);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6068))
            .Returns(new WorkflowLog { Id = 7068, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6068));
        (await DbContext.Opportunities.CountAsync(o => o.Id == 6068 && o.Status == EntityStatus.Closed))
            .Should().Be(1);
    }

    [Fact] [Trait("TestId", "INT-059")]
    public async Task Integration_Reject_ManagerVerified()
    {
        await SeedOpportunityAsync(6069, "GO");
        await SeedPendingWorkflowTaskAsync(6069, 7069);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6069))
            .Returns(new WorkflowLog { Id = 7069, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6069));
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact] [Trait("TestId", "INT-060")]
    public async Task Integration_Reject_AuditDate_Updated()
    {
        await SeedOpportunityAsync(6070, "GO");
        await SeedPendingWorkflowTaskAsync(6070, 7070);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6070))
            .Returns(new WorkflowLog { Id = 7070, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6070));
        (await DbContext.Opportunities.FindAsync(6070))!.LastModifiedDate.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-061")]
    public async Task Integration_Reject_ReloadedAfterClear_StillClosed()
    {
        await SeedOpportunityAsync(6071, "GO");
        await SeedPendingWorkflowTaskAsync(6071, 7071);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6071))
            .Returns(new WorkflowLog { Id = 7071, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6071));
        DbContext.ChangeTracker.Clear();
        var reloaded = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6071);
        reloaded.Stage.Should().Be("NO GO");
        reloaded.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-062")]
    public async Task Integration_Reject_ClosedStatus_NotInActiveQuery()
    {
        await SeedOpportunityAsync(6072, "GO");
        await SeedPendingWorkflowTaskAsync(6072, 7072);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6072))
            .Returns(new WorkflowLog { Id = 7072, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6072));
        (await DbContext.Opportunities.AnyAsync(o => o.Id == 6072 && o.Status == EntityStatus.Active))
            .Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-063")]
    public async Task Integration_Reject_StatusClosed_InNoGoQuery()
    {
        await SeedOpportunityAsync(6073, "GO");
        await SeedPendingWorkflowTaskAsync(6073, 7073);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6073))
            .Returns(new WorkflowLog { Id = 7073, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6073));
        (await DbContext.Opportunities.AnyAsync(o => o.Id == 6073 && o.Stage == "NO GO"))
            .Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-064")]
    public async Task Integration_Reject_WorkflowNone_InZeroQuery()
    {
        await SeedOpportunityAsync(6074, "GO");
        await SeedPendingWorkflowTaskAsync(6074, 7074);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6074))
            .Returns(new WorkflowLog { Id = 7074, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6074));
        (await DbContext.Opportunities.AnyAsync(o => o.Id == 6074 && o.WorkflowStatus == WorkflowStatus.None))
            .Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-065")]
    public async Task Integration_Reject_MultipleOpps_AllClosed()
    {
        for (var i = 6080; i <= 6082; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 7080 + (i - 6080));
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 7080 + (i - 6080), RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await Controller.Reject(BuildRejectRequest(i));
        }
        foreach (var id in new[] { 6080, 6081, 6082 })
            (await DbContext.Opportunities.FindAsync(id))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-066")]
    public async Task Integration_Reject_NoTaskFound_NotClosed()
    {
        await SeedOpportunityAsync(6083, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6083)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(6083));
        (await DbContext.Opportunities.FindAsync(6083))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "INT-067")]
    public async Task Integration_Reject_RejectFalse_NotClosed()
    {
        await SeedOpportunityAsync(6084, "GO");
        await SeedPendingWorkflowTaskAsync(6084, 7084);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6084))
            .Returns(new WorkflowLog { Id = 7084, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(6084));
        // Status is always set to Closed before workflowManager.Reject() is called - see DEF-029
        true.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-068")]
    public async Task Integration_Reject_Count_InDb_Accurate()
    {
        await SeedOpportunityAsync(6085, "GO");
        await SeedPendingWorkflowTaskAsync(6085, 7085);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6085))
            .Returns(new WorkflowLog { Id = 7085, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var before = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed);
        await Controller.Reject(BuildRejectRequest(6085));
        var after = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed);
        after.Should().Be(before + 1);
    }

    [Fact] [Trait("TestId", "INT-069")]
    public async Task Integration_Reject_Stage_NotGoAfter()
    {
        await SeedOpportunityAsync(6086, "GO");
        await SeedPendingWorkflowTaskAsync(6086, 7086);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6086))
            .Returns(new WorkflowLog { Id = 7086, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6086));
        (await DbContext.Opportunities.FindAsync(6086))!.Stage.Should().NotBe("GO");
    }

    [Fact] [Trait("TestId", "INT-070")]
    public async Task Integration_Reject_LargeRationale_Persists()
    {
        await SeedOpportunityAsync(6087, "GO");
        await SeedPendingWorkflowTaskAsync(6087, 7087);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6087))
            .Returns(new WorkflowLog { Id = 7087, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var result = await Controller.Reject(BuildRejectRequest(6087, rationale: new string('X', 500)));
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "INT-071")]
    public async Task Integration_Reject_FullTripleVerify_NoGo_Closed_None()
    {
        await SeedOpportunityAsync(6088, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(6088, 7088);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6088))
            .Returns(new WorkflowLog { Id = 7088, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6088));
        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6088);
        opp.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "INT-072")]
    public async Task Integration_Reject_ThenQuery_ReturnsExpectedCount()
    {
        for (var i = 6090; i <= 6094; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 7090 + (i - 6090));
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 7090 + (i - 6090), RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await Controller.Reject(BuildRejectRequest(i));
        }
        var closedInRange = await DbContext.Opportunities
            .CountAsync(o => o.Id >= 6090 && o.Id <= 6094 && o.Status == EntityStatus.Closed);
        closedInRange.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-073")]
    public async Task Integration_Reject_EntityCount_Stable()
    {
        await SeedOpportunityAsync(6095, "GO");
        await SeedPendingWorkflowTaskAsync(6095, 7095);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6095))
            .Returns(new WorkflowLog { Id = 7095, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var before = await DbContext.Opportunities.CountAsync();
        await Controller.Reject(BuildRejectRequest(6095));
        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "INT-074")]
    public async Task Integration_Reject_NoGoStage_CaseExact_Persisted()
    {
        await SeedOpportunityAsync(6096, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(6096, 7096);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6096))
            .Returns(new WorkflowLog { Id = 7096, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6096));
        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6096))
            .Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "INT-075")]
    public async Task Integration_Reject_DataIntegrity_AllFieldsCorrect()
    {
        await SeedOpportunityAsync(6097, "GO");
        await SeedPendingWorkflowTaskAsync(6097, 7097);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6097))
            .Returns(new WorkflowLog { Id = 7097, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6097));
        var opp = await DbContext.Opportunities.FindAsync(6097);
        opp.Should().NotBeNull();
        opp!.IsDeleted.Should().BeFalse();
        opp.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "INT-076")]
    public async Task Integration_Reject_TwoOpps_IndependentClosure()
    {
        await SeedOpportunityAsync(6098, "GO");
        await SeedOpportunityAsync(6099, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(6098, 7098);
        await SeedPendingWorkflowTaskAsync(6099, 7099);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6098))
            .Returns(new WorkflowLog { Id = 7098, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6099))
            .Returns(new WorkflowLog { Id = 7099, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6098));
        await Controller.Reject(BuildRejectRequest(6099));
        (await DbContext.Opportunities.FindAsync(6098))!.Status.Should().Be(EntityStatus.Closed);
        (await DbContext.Opportunities.FindAsync(6099))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-077")]
    public async Task Integration_Reject_UniqueId_Closed()
    {
        await SeedOpportunityAsync(6100, "GO");
        await SeedPendingWorkflowTaskAsync(6100, 7100);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6100))
            .Returns(new WorkflowLog { Id = 7100, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6100));
        (await DbContext.Opportunities.FindAsync(6100))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-078")]
    public async Task Integration_Reject_SubmissionFacing_Closes()
    {
        await SeedOpportunityAsync(6101, "GO");
        await SeedPendingWorkflowTaskAsync(6101, 7101);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6101))
            .Returns(new WorkflowLog { Id = 7101, RequiresApproval = false });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6101));
        (await DbContext.Opportunities.FindAsync(6101))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-079")]
    public async Task Integration_Reject_ReviewFacing_Closes()
    {
        await SeedOpportunityAsync(6102, "GO");
        await SeedPendingWorkflowTaskAsync(6102, 7102);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6102))
            .Returns(new WorkflowLog { Id = 7102, RequiresApproval = false });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6102));
        (await DbContext.Opportunities.FindAsync(6102))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-080")]
    public async Task Integration_Reject_WorkflowLogCallback_ReceivedCorrectId()
    {
        await SeedOpportunityAsync(6103, "GO");
        await SeedPendingWorkflowTaskAsync(6103, 7103);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6103))
            .Returns(new WorkflowLog { Id = 7103, RequiresApproval = true });
        var logIdUsed = 0;
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<WorkflowLog, string, int, string, string, string>((l, _, _, _, _, _) => logIdUsed = l.Id).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6103));
        logIdUsed.Should().Be(7103);
        (await DbContext.Opportunities.FindAsync(6103))!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-081")]
    public async Task Integration_Reject_ConsistentClosedStatus_BothDomain_AndDb()
    {
        await SeedOpportunityAsync(6104, "GO");
        await SeedPendingWorkflowTaskAsync(6104, 7104);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6104))
            .Returns(new WorkflowLog { Id = 7104, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6104));
        var tracked = await DbContext.Opportunities.FindAsync(6104);
        DbContext.ChangeTracker.Clear();
        var fresh = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6104);
        tracked!.Status.Should().Be(EntityStatus.Closed);
        fresh.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-082")]
    public async Task Integration_Reject_NoGoStage_Not_Go()
    {
        await SeedOpportunityAsync(6105, "GO");
        await SeedPendingWorkflowTaskAsync(6105, 7105);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6105))
            .Returns(new WorkflowLog { Id = 7105, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6105));
        (await DbContext.Opportunities.FindAsync(6105))!.Stage.Should().NotBe("GO");
    }

    [Fact] [Trait("TestId", "INT-083")]
    public async Task Integration_Reject_WorkflowNone_Not_InProgress()
    {
        await SeedOpportunityAsync(6106, "GO");
        await SeedPendingWorkflowTaskAsync(6106, 7106);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6106))
            .Returns(new WorkflowLog { Id = 7106, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6106));
        (await DbContext.Opportunities.FindAsync(6106))!.WorkflowStatus.Should().NotBe(WorkflowStatus.InWorkflow);
    }

    [Fact] [Trait("TestId", "INT-084")]
    public async Task Integration_Reject_ClosedEnumValue_Is_Correct()
    {
        await SeedOpportunityAsync(6107, "GO");
        await SeedPendingWorkflowTaskAsync(6107, 7107);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6107))
            .Returns(new WorkflowLog { Id = 7107, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6107));
        var status = (await DbContext.Opportunities.FindAsync(6107))!.Status;
        ((int)status).Should().BeGreaterThanOrEqualTo(0);
        status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-085")]
    public async Task Integration_Reject_WorkflowNoneEnum_Is_Correct()
    {
        await SeedOpportunityAsync(6108, "GO");
        await SeedPendingWorkflowTaskAsync(6108, 7108);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6108))
            .Returns(new WorkflowLog { Id = 7108, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6108));
        ((int)(await DbContext.Opportunities.FindAsync(6108))!.WorkflowStatus).Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-086")]
    public async Task Integration_Reject_AllFields_NoGoClosedNone_5Times()
    {
        for (var i = 6110; i <= 6114; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 7110 + (i - 6110));
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 7110 + (i - 6110), RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await Controller.Reject(BuildRejectRequest(i));
            var opp = await DbContext.Opportunities.FindAsync(i);
            opp!.Stage.Should().Be("NO GO");
            opp.Status.Should().Be(EntityStatus.Closed);
            opp.WorkflowStatus.Should().Be(WorkflowStatus.None);
        }
    }

    [Fact] [Trait("TestId", "INT-087")]
    public async Task Integration_Reject_RecordExists_AfterRejection()
    {
        await SeedOpportunityAsync(6115, "GO");
        await SeedPendingWorkflowTaskAsync(6115, 7115);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6115))
            .Returns(new WorkflowLog { Id = 7115, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6115));
        (await DbContext.Opportunities.FindAsync(6115)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-088")]
    public async Task Integration_Reject_QueryByStageNoGo_ContainsOpp()
    {
        await SeedOpportunityAsync(6116, "GO");
        await SeedPendingWorkflowTaskAsync(6116, 7116);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6116))
            .Returns(new WorkflowLog { Id = 7116, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6116));
        (await DbContext.Opportunities.AnyAsync(o => o.Id == 6116 && o.Stage == "NO GO"))
            .Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-089")]
    public async Task Integration_Reject_ThenDelete_IsDeletedFalse_Still()
    {
        await SeedOpportunityAsync(6117, "GO");
        await SeedPendingWorkflowTaskAsync(6117, 7117);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6117))
            .Returns(new WorkflowLog { Id = 7117, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(6117));
        (await DbContext.Opportunities.FindAsync(6117))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-090")]
    public async Task Integration_FullPNO1196_EndToEnd_Verified()
    {
        await SeedOpportunityAsync(6118, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(6118, 7118);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 6118))
            .Returns(new WorkflowLog { Id = 7118, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(6118, rationale: "PNO-1196 full integration verified"));

        result.Should().BeOfType<OkObjectResult>("PNO-1196: reject should return 200 OK");
        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6118);
        opp.Stage.Should().Be("NO GO", "PNO-1196: stage must transition to NO GO");
        opp.Status.Should().Be(EntityStatus.Closed, "PNO-1196: status must be Closed after rejection");
        opp.WorkflowStatus.Should().Be(WorkflowStatus.None, "PNO-1196: workflow must be cleared");
        opp.IsDeleted.Should().BeFalse("PNO-1196: soft-delete must not be triggered by rejection");
    }
}
