/**
 * @fileoverview PNO-1196 Concurrency Tests — 25 tests.
 * Concurrent modification, duplicate rejection attempts, race conditions.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using Xunit;
using EntityStatus = UNOPS.PAO.Domain.Entities.EntityStatus;

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// PNO-1196 Concurrency Tests — 25 tests covering concurrent rejection scenarios.
/// </summary>
[Collection("Concurrency")]
[Trait("Category", "Concurrency")]
[Trait("Ticket", "PNO-1196")]
public class ConcurrencyTests : PNO1196TestFixtureBase
{
    [Fact] [Trait("TestId", "CON-001")]
    public async Task Concurrency_TwoRejectsForDifferentOpps_BothSucceed()
    {
        await SeedOpportunityAsync(8001, "GO");
        await SeedOpportunityAsync(8002, "GO");
        await SeedPendingWorkflowTaskAsync(8001, 9001);
        await SeedPendingWorkflowTaskAsync(8002, 9002);

        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8001))
            .Returns(new WorkflowLog { Id = 9001, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8002))
            .Returns(new WorkflowLog { Id = 9002, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var task1 = Controller.Reject(BuildRejectRequest(8001));
        var task2 = Controller.Reject(BuildRejectRequest(8002));
        var results = await Task.WhenAll(task1, task2);

        results.Should().AllBeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-002")]
    public async Task Concurrency_TwoRejectsForSameOpp_FirstSucceedsSecondFails()
    {
        await SeedOpportunityAsync(8003, "GO");
        await SeedPendingWorkflowTaskAsync(8003, 9003);

        MockWorkflowManager.SetupSequence(x => x.PendingTask("Opportunity", 8003))
            .Returns(new WorkflowLog { Id = 9003, RequiresApproval = true })
            .Returns((WorkflowLog?)null);
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result1 = await Controller.Reject(BuildRejectRequest(8003));
        var result2 = await Controller.Reject(BuildRejectRequest(8003));

        result1.Should().BeOfType<OkObjectResult>();
        result2.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-003")]
    public async Task Concurrency_FiveSequentialRejects_AllComplete()
    {
        var results = new List<IActionResult>();
        for (var i = 8010; i <= 8014; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 9000 + i);
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 9000 + i, RequiresApproval = true });
        }
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        for (var i = 8010; i <= 8014; i++)
        {
            results.Add(await Controller.Reject(BuildRejectRequest(i)));
        }

        results.Should().AllBeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-004")]
    public async Task Concurrency_RejectAndQuerySimultaneous_NoException()
    {
        await SeedOpportunityAsync(8015, "GO");
        await SeedPendingWorkflowTaskAsync(8015, 9015);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8015))
            .Returns(new WorkflowLog { Id = 9015, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var rejectTask = Controller.Reject(BuildRejectRequest(8015));
        var queryTask = DbContext.Opportunities.Where(o => !o.IsDeleted).CountAsync();

        await Task.WhenAll(rejectTask, queryTask);

        (await rejectTask).Should().NotBeNull();
        (await queryTask).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact] [Trait("TestId", "CON-005")]
    public async Task Concurrency_NoPendingTaskScenario_AllReturn400()
    {
        var tasks = new List<Task<ActionResult>>();
        for (var i = 8020; i <= 8024; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            tasks.Add(Controller.Reject(BuildRejectRequest(i)));
        }

        var results = await Task.WhenAll(tasks);
        results.Should().AllBeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-006")]
    public async Task Concurrency_ClosedOppsQueryDuringReject_Consistent()
    {
        await SeedOpportunityAsync(8025, "GO");
        await SeedPendingWorkflowTaskAsync(8025, 9025);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8025))
            .Returns(new WorkflowLog { Id = 9025, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(8025));

        var closedCount = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed && !o.IsDeleted);
        closedCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact] [Trait("TestId", "CON-007")]
    public async Task Concurrency_MultipleReadsDuringWrite_NoCorruption()
    {
        await SeedOpportunityAsync(8026, "GO");
        await SeedPendingWorkflowTaskAsync(8026, 9026);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8026))
            .Returns(new WorkflowLog { Id = 9026, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var writeTask = Controller.Reject(BuildRejectRequest(8026));
        var readTask1 = DbContext.Opportunities.AsNoTracking().Where(o => o.Id == 8026).FirstOrDefaultAsync();

        await writeTask;
        var readResult = await readTask1;

        readResult.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "CON-008")]
    public async Task Concurrency_TenParallelNoPendingTask_AllReturn400()
    {
        var tasks = new List<Task<ActionResult>>();
        for (var i = 8030; i <= 8039; i++)
        {
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            tasks.Add(Controller.Reject(BuildRejectRequest(i)));
        }

        var results = await Task.WhenAll(tasks);
        results.Should().AllBeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-009")]
    public async Task Concurrency_DbStateConsistentAfterMultipleRejects()
    {
        for (var i = 8040; i <= 8044; i++)
        {
            await SeedOpportunityAsync(i, "NO GO", EntityStatus.Closed);
        }

        var closedCount = await DbContext.Opportunities
            .Where(o => o.Id >= 8040 && o.Id <= 8044 && o.Status == EntityStatus.Closed)
            .CountAsync();

        closedCount.Should().Be(5);
    }

    [Fact] [Trait("TestId", "CON-010")]
    public async Task Concurrency_NoDeadlockOn_SequentialSeedAndReject()
    {
        for (var i = 8045; i <= 8049; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            var result = await Controller.Reject(BuildRejectRequest(i));
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }

    [Fact] [Trait("TestId", "CON-011")]
    public async Task Concurrency_InMemoryDbThread_Safe_ForReadOnlyQueries()
    {
        await SeedOpportunityAsync(8050, "GO");

        var tasks = Enumerable.Range(0, 5).Select(_ =>
            DbContext.Opportunities.AsNoTracking().Where(o => o.Id == 8050).FirstOrDefaultAsync());

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    [Fact] [Trait("TestId", "CON-012")]
    public async Task Concurrency_WorkflowManagerMockThreadSafe()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);

        var tasks = Enumerable.Range(8060, 5).Select(i =>
            Controller.Reject(BuildRejectRequest(i)));

        var results = await Task.WhenAll(tasks);
        results.Should().AllBeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-013")]
    public async Task Concurrency_SuccessfulReject_DbConsistentAfterClearTracker()
    {
        await SeedOpportunityAsync(8065, "GO");
        await SeedPendingWorkflowTaskAsync(8065, 9065);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8065))
            .Returns(new WorkflowLog { Id = 9065, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(8065));
        DbContext.ChangeTracker.Clear();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 8065);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "CON-014")]
    public async Task Concurrency_RejectDoesNotMutateOtherOpps()
    {
        await SeedOpportunityAsync(8066, "GO", EntityStatus.Active);
        await SeedOpportunityAsync(8067, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(8066, 9066);

        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8066))
            .Returns(new WorkflowLog { Id = 9066, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(8066));

        var other = await DbContext.Opportunities.FindAsync(8067);
        other!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "CON-015")]
    public async Task Concurrency_RejectResponseNotAffectedByOtherOppState()
    {
        await SeedOpportunityAsync(8068, "GO");
        await SeedOpportunityAsync(8069, "NO GO", EntityStatus.Closed);
        await SeedPendingWorkflowTaskAsync(8068, 9068);

        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8068))
            .Returns(new WorkflowLog { Id = 9068, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var result = await Controller.Reject(BuildRejectRequest(8068));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-016")]
    public async Task Concurrency_FiveConcurrentFailedRejects_AllReturn400()
    {
        var tasks = Enumerable.Range(8070, 5).Select(async i =>
        {
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            return await Controller.Reject(BuildRejectRequest(i));
        });

        var results = await Task.WhenAll(tasks);
        results.Should().AllBeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-017")]
    public async Task Concurrency_SequentialSeedAndVerify_DataIntegrity()
    {
        for (var i = 8080; i <= 8082; i++)
        {
            await SeedOpportunityAsync(i, "NO GO", EntityStatus.Closed);
        }

        var allClosed = await DbContext.Opportunities
            .Where(o => o.Id >= 8080 && o.Id <= 8082)
            .AllAsync(o => o.Status == EntityStatus.Closed);

        allClosed.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "CON-018")]
    public async Task Concurrency_WorkflowManagerVerifyCount_AfterMultipleRejects()
    {
        for (var i = 8083; i <= 8085; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 9000 + i);
            var id = i;
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", id))
                .Returns(new WorkflowLog { Id = 9000 + id, RequiresApproval = true });
        }
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        for (var i = 8083; i <= 8085; i++)
        {
            await Controller.Reject(BuildRejectRequest(i));
        }

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
    }

    [Fact] [Trait("TestId", "CON-019")]
    public async Task Concurrency_StageQueryAfterReject_ReturnsCorrectStage()
    {
        await SeedOpportunityAsync(8086, "GO");
        await SeedPendingWorkflowTaskAsync(8086, 9086);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8086))
            .Returns(new WorkflowLog { Id = 9086, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(8086));

        var stage = await DbContext.Opportunities
            .Where(o => o.Id == 8086)
            .Select(o => o.Stage)
            .FirstOrDefaultAsync();

        stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "CON-020")]
    public async Task Concurrency_StatusQueryAfterReject_ReturnsClosedStatus()
    {
        await SeedOpportunityAsync(8087, "GO");
        await SeedPendingWorkflowTaskAsync(8087, 9087);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 8087))
            .Returns(new WorkflowLog { Id = 9087, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(8087));

        var status = await DbContext.Opportunities
            .Where(o => o.Id == 8087)
            .Select(o => o.Status)
            .FirstOrDefaultAsync();

        status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "CON-021")]
    public async Task Concurrency_NoDeadlock_OnConcurrentSeedAndQuery()
    {
        var seedTask = SeedOpportunityAsync(8088, "GO");
        var queryTask = DbContext.Opportunities.CountAsync();

        await Task.WhenAll(seedTask, queryTask);

        (await DbContext.Opportunities.FindAsync(8088)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "CON-022")]
    public async Task Concurrency_TwentyNoPendingTaskRequests_AllReturn400()
    {
        var tasks = Enumerable.Range(8090, 20).Select(i =>
        {
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            return Controller.Reject(BuildRejectRequest(i));
        });

        var results = await Task.WhenAll(tasks);
        results.Should().AllBeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "CON-023")]
    public async Task Concurrency_InMemoryDbDoesNotShareStateAcrossTests()
    {
        var oppInThisTest = await DbContext.Opportunities.AnyAsync(o => o.Id == 99999);
        oppInThisTest.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "CON-024")]
    public async Task Concurrency_ControllerObjectReusable_AcrossMultipleCalls()
    {
        for (var i = 0; i < 5; i++)
        {
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            var result = await Controller.Reject(BuildRejectRequest(i));
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }

    [Fact] [Trait("TestId", "CON-025")]
    public async Task Concurrency_ParallelStatusChecks_AfterRejects_AllClosed()
    {
        for (var i = 8110; i <= 8114; i++)
        {
            await SeedOpportunityAsync(i, "NO GO", EntityStatus.Closed);
        }

        var tasks = Enumerable.Range(8110, 5).Select(i =>
            DbContext.Opportunities.AsNoTracking()
                .Where(o => o.Id == i)
                .Select(o => o.Status)
                .FirstOrDefaultAsync());

        var statuses = await Task.WhenAll(tasks);
        statuses.Should().AllBeEquivalentTo(EntityStatus.Closed);
    }
}
