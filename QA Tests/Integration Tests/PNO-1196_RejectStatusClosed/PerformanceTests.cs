/**
 * @fileoverview PNO-1196 Performance Tests — 16 tests.
 * Response time, throughput, memory, and SLA validation for Reject operation.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Diagnostics;
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
/// PNO-1196 Performance Tests — 16 tests covering response time and throughput.
/// </summary>
[Collection("Performance")]
[Trait("Category", "Performance")]
[Trait("Ticket", "PNO-1196")]
public class PerformanceTests : PNO1196TestFixtureBase
{
    [Fact] [Trait("TestId", "PERF-001")]
    public async Task Reject_SingleOpp_CompletesWithin2Seconds()
    {
        await SeedOpportunityAsync(9001, "GO");
        await SeedPendingWorkflowTaskAsync(9001, 10001);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9001))
            .Returns(new WorkflowLog { Id = 10001, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var sw = Stopwatch.StartNew();
        await Controller.Reject(BuildRejectRequest(9001));
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-002")]
    public async Task Reject_NoPendingTask_CompletesWithin500Ms()
    {
        await SeedOpportunityAsync(9002, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9002)).Returns((WorkflowLog?)null);

        var sw = Stopwatch.StartNew();
        await Controller.Reject(BuildRejectRequest(9002));
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact] [Trait("TestId", "PERF-003")]
    public async Task Reject_10SequentialRejects_CompletesWithin10Seconds()
    {
        for (var i = 9010; i <= 9019; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 10000 + i);
            var id = i;
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", id))
                .Returns(new WorkflowLog { Id = 10000 + id, RequiresApproval = true });
        }
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var sw = Stopwatch.StartNew();
        for (var i = 9010; i <= 9019; i++)
            await Controller.Reject(BuildRejectRequest(i));
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact] [Trait("TestId", "PERF-004")]
    public async Task DbQuery_ClosedOpportunities_CompletesWithin1Second()
    {
        await SeedOpportunityAsync(9020, "NO GO", EntityStatus.Closed);

        var sw = Stopwatch.StartNew();
        var count = await DbContext.Opportunities
            .Where(o => o.Status == EntityStatus.Closed && !o.IsDeleted)
            .CountAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
        count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact] [Trait("TestId", "PERF-005")]
    public async Task DbSeed_100Opportunities_CompletesWithin5Seconds()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 9100; i <= 9199; i++)
        {
            DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
            {
                Id = i, Name = $"Perf Opp {i}", Stage = "GO", Description = "Performance test opportunity",
                Status = EntityStatus.Active, IsDeleted = false,
                ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
            });
        }
        await DbContext.SaveChangesAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact] [Trait("TestId", "PERF-006")]
    public async Task DbQuery_CountByStatus_CompletesWithin200Ms()
    {
        var sw = Stopwatch.StartNew();
        var closed = await DbContext.Opportunities.CountAsync(o => o.Status == EntityStatus.Closed);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
        closed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact] [Trait("TestId", "PERF-007")]
    public async Task Reject_WithLargeRationale_CompletesWithin2Seconds()
    {
        await SeedOpportunityAsync(9200, "GO");
        await SeedPendingWorkflowTaskAsync(9200, 10200);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9200))
            .Returns(new WorkflowLog { Id = 10200, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var largeRationale = string.Concat(Enumerable.Repeat("Performance test with detailed rationale explaining the rejection. ", 50));
        var sw = Stopwatch.StartNew();
        var result = await Controller.Reject(BuildRejectRequest(9200, rationale: largeRationale));
        sw.Stop();

        result.Should().BeOfType<OkObjectResult>();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-008")]
    public async Task Reject_WithWorkflowRejectTrue_TotalRoundTripUnder2Sec()
    {
        await SeedOpportunityAsync(9201, "GO");
        await SeedPendingWorkflowTaskAsync(9201, 10201);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9201))
            .Returns(new WorkflowLog { Id = 10201, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var sw = Stopwatch.StartNew();
        var result = await Controller.Reject(BuildRejectRequest(9201));
        sw.Stop();

        result.Should().BeOfType<OkObjectResult>();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-009")]
    public async Task DbQuery_FindById_CompletesWithin100Ms()
    {
        await SeedOpportunityAsync(9202, "GO");

        var sw = Stopwatch.StartNew();
        var opp = await DbContext.Opportunities.FindAsync(9202);
        sw.Stop();

        opp.Should().NotBeNull();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact] [Trait("TestId", "PERF-010")]
    public async Task DbQuery_FilterByStage_CompletesWithin1Second()
    {
        for (var i = 9300; i <= 9309; i++)
            await SeedOpportunityAsync(i, "NO GO", EntityStatus.Closed);

        var sw = Stopwatch.StartNew();
        var noGoOpps = await DbContext.Opportunities
            .Where(o => o.Stage == "NO GO" && !o.IsDeleted)
            .ToListAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
        noGoOpps.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Fact] [Trait("TestId", "PERF-011")]
    public async Task Reject_AuditFieldUpdate_NoSignificantOverhead()
    {
        await SeedOpportunityAsync(9203, "GO");
        await SeedPendingWorkflowTaskAsync(9203, 10203);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9203))
            .Returns(new WorkflowLog { Id = 10203, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var sw = Stopwatch.StartNew();
        await Controller.Reject(BuildRejectRequest(9203));
        sw.Stop();

        var opp = await DbContext.Opportunities.FindAsync(9203);
        opp!.LastModifiedBy.Should().Be(1);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-012")]
    public async Task Reject_MultipleSequentialRejects_MemoryStable()
    {
        GC.Collect();
        var memBefore = GC.GetTotalMemory(forceFullCollection: true);

        for (var i = 9400; i <= 9409; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 10400 + i);
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 10400 + i, RequiresApproval = true });
        }
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        for (var i = 9400; i <= 9409; i++)
            await Controller.Reject(BuildRejectRequest(i));

        GC.Collect();
        var memAfter = GC.GetTotalMemory(forceFullCollection: true);
        var growthMb = (memAfter - memBefore) / 1_048_576.0;
        growthMb.Should().BeLessThan(50, "10 reject operations should not cause significant memory growth");
    }

    [Fact] [Trait("TestId", "PERF-013")]
    public async Task Reject_RepeatedRejectOnSameEntity_StablePerformance()
    {
        await SeedOpportunityAsync(9500, "GO");
        await SeedPendingWorkflowTaskAsync(9500, 10500);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9500))
            .Returns(new WorkflowLog { Id = 10500, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var times = new List<long>();
        for (var i = 0; i < 5; i++)
        {
            var sw = Stopwatch.StartNew();
            await Controller.Reject(BuildRejectRequest(9500));
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        times.Max().Should().BeLessThan(2000, "no single reject should take more than 2s");
        var avg = times.Average();
        avg.Should().BeLessThan(1000, "average reject time should be under 1s");
    }

    [Fact] [Trait("TestId", "PERF-014")]
    public async Task Reject_NoWorkflowTask_5Calls_CompletesWithin1Second()
    {
        for (var i = 9210; i <= 9214; i++)
        {
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
        }

        var sw = Stopwatch.StartNew();
        for (var i = 9210; i <= 9214; i++)
            await Controller.Reject(BuildRejectRequest(i));
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact] [Trait("TestId", "PERF-015")]
    public async Task DbContext_SaveChanges_CompletesWithin500Ms()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 9215, Name = "Save Perf Test", Stage = "GO", Description = "Performance test opportunity",
            Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });

        var sw = Stopwatch.StartNew();
        await DbContext.SaveChangesAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact] [Trait("TestId", "PERF-016")]
    public async Task Reject_FullCycle_WithAllMocksConfigured_Under3Seconds()
    {
        await SeedOpportunityAsync(9216, "GO");
        await SeedPendingWorkflowTaskAsync(9216, 10216);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9216))
            .Returns(new WorkflowLog { Id = 10216, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var sw = Stopwatch.StartNew();
        var result = await Controller.Reject(BuildRejectRequest(9216,
            rationale: string.Concat(Enumerable.Repeat("Performance test rationale. ", 10))));
        sw.Stop();

        result.Should().BeOfType<OkObjectResult>();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }
}
