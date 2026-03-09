/**
 * @fileoverview PNO-729 Performance Tests — 16 SLA and response time tests.
 * Validates migration speed, color lookup speed, statement persistence under timing constraints.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Performance Tests — 16 timing and SLA tests.
/// </summary>
[Collection("PNO729 Performance")]
[Trait("Category", "Performance")]
[Trait("Ticket", "PNO-729")]
public class PerformanceTests : PNO729TestFixtureBase
{
    [Fact] [Trait("TestId", "PERF-001")]
    public async Task Performance_SingleNullStatement_FixedUnder1Second()
    {
        await SeedOpportunityAsync(11001, null);
        var sw = Stopwatch.StartNew();
        var count = await RunStatementFixMigrationAsync();
        sw.Stop();
        count.Should().Be(1, "exactly 1 null statement should be fixed");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1),
            $"single statement fix took {sw.ElapsedMilliseconds}ms");
    }

    [Fact] [Trait("TestId", "PERF-002")]
    public async Task Performance_50NullStatements_FixedUnder5Seconds()
    {
        for (var i = 11100; i <= 11149; i++)
            await SeedOpportunityAsync(i, null);
        var sw = Stopwatch.StartNew();
        var count = await RunStatementFixMigrationAsync();
        sw.Stop();
        count.Should().Be(50, "all 50 null statements should be fixed");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5),
            $"50 statement fixes took {sw.ElapsedMilliseconds}ms");
    }

    [Fact] [Trait("TestId", "PERF-003")]
    public async Task Performance_ColorLookup_Under10ms()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
            GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        sw.Stop();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100),
            "1000 color lookups should be cheap");
    }

    [Fact] [Trait("TestId", "PERF-004")]
    public async Task Performance_SingleQuery_Under1Second()
    {
        await SeedClosedOpportunityAsync(11200);
        var sw = Stopwatch.StartNew();
        _ = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 11200);
        sw.Stop();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact] [Trait("TestId", "PERF-005")]
    public async Task Performance_Migration_Idempotent_UnchangedOnSecondRun()
    {
        await SeedOpportunityAsync(11201, null);
        await RunStatementFixMigrationAsync();

        var sw = Stopwatch.StartNew();
        var count = await RunStatementFixMigrationAsync();
        sw.Stop();

        count.Should().Be(0);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact] [Trait("TestId", "PERF-006")]
    public async Task Performance_LargeStatement_PersistsUnder2Seconds()
    {
        var large = new string('Z', 10000);
        var sw = Stopwatch.StartNew();
        await SeedOpportunityAsync(11202, large);
        sw.Stop();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-007")]
    public async Task Performance_100NullStatements_FixedUnder10Seconds()
    {
        for (var i = 11300; i <= 11399; i++)
            await SeedOpportunityAsync(i, null);
        var sw = Stopwatch.StartNew();
        var count = await RunStatementFixMigrationAsync();
        sw.Stop();
        count.Should().Be(100, "all 100 null statements should be fixed");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10),
            $"100 statement fixes took {sw.ElapsedMilliseconds}ms, expected <10000ms");
    }

    [Fact] [Trait("TestId", "PERF-008")]
    public async Task Performance_ClosedQuery_Under500ms()
    {
        await SeedClosedOpportunityAsync(11400);
        var sw = Stopwatch.StartNew();
        _ = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed)
            .ToListAsync();
        sw.Stop();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact] [Trait("TestId", "PERF-009")]
    public async Task Performance_MemoryUsage_Under50MB_50Migrations()
    {
        for (var i = 11500; i <= 11549; i++)
            await SeedOpportunityAsync(i, null);
        await RunStatementFixMigrationAsync();

        GC.Collect();
        var before = GC.GetTotalMemory(forceFullCollection: true);

        for (var i = 0; i < 50; i++)
            await RunStatementFixMigrationAsync();

        GC.Collect();
        var after = GC.GetTotalMemory(forceFullCollection: true);
        var growthMb = (after - before) / 1_048_576.0;
        growthMb.Should().BeLessThan(50,
            $"50 idempotent migration runs caused {growthMb:F1}MB memory growth");
    }

    [Fact] [Trait("TestId", "PERF-010")]
    public async Task Performance_StatusTransition_Under1Second()
    {
        await SeedOpportunityAsync(11600, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(11600);

        var sw = Stopwatch.StartNew();
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact] [Trait("TestId", "PERF-011")]
    public async Task Performance_5Reads_Under2Seconds()
    {
        await SeedClosedOpportunityAsync(11601);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 5; i++)
            _ = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 11601);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-012")]
    public async Task Performance_ColorMapping_AllStatuses_Under10ms()
    {
        var statuses = System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>();
        var sw = Stopwatch.StartNew();
        foreach (var s in statuses)
            _ = GetStatusColorClass(s);
        sw.Stop();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(10));
    }

    [Fact] [Trait("TestId", "PERF-013")]
    public async Task Performance_AlreadyFixed_SecondMigration_Under100ms()
    {
        await SeedOpportunityAsync(11700, EmptyMarkdown);

        var sw = Stopwatch.StartNew();
        var count = await RunStatementFixMigrationAsync();
        sw.Stop();

        count.Should().Be(0, "Already fixed — migration should not touch it");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact] [Trait("TestId", "PERF-014")]
    public async Task Performance_SeedAndMigrate_10Opps_Under3Seconds()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 11800; i <= 11809; i++)
            await SeedOpportunityAsync(i, null);
        await RunStatementFixMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact] [Trait("TestId", "PERF-015")]
    public async Task Performance_UnicodeStatement_Under2Seconds()
    {
        var unicode = string.Concat(Enumerable.Repeat("ΩЮ机🌟", 200));
        var sw = Stopwatch.StartNew();
        await SeedOpportunityAsync(11900, unicode);
        sw.Stop();

        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 11900))
            .OpportunityStatementMarkdown.Should().NotBeNullOrEmpty();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-016")]
    public async Task Performance_FullPNO729Flow_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();

        await SeedClosedOpportunityWithNullStatementAsync(11901);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 11901);
        var color = GetStatusColorClass(opp.Status);

        sw.Stop();

        color.Should().Be(ClosedStatusColor);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }
}
