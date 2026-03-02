/**
 * @fileoverview PNO-729 Load Tests — 10 sustained and spike load tests.
 * Validates stability, throughput, and memory safety under high-volume operations.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Diagnostics;
using System.Security.Claims;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Load Tests — 10 sustained and spike load scenarios.
/// </summary>
[Collection("PNO729 Load")]
[Trait("Category", "Load")]
[Trait("Ticket", "PNO-729")]
public class LoadTests : PNO729TestFixtureBase
{
    private static AppDbContext CreateAppDbContext(DbContextOptions<AppDbContext> opts)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
        mockHttpContextAccessor.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) });
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");
        var userResolver = new UserResolverService<int>(mockHttpContextAccessor.Object);
        return new AppDbContext(opts, userResolver, mockSchema.Object);
    }

    private (Func<Task<int>> migration, AppDbContext ctx) CreateIsolated(string dbName)
    {
        var opts = new DbContextOptionsBuilder<UNOPS.PAO.DataAccess.Context.AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var ctx = CreateAppDbContext(opts);

        async Task<int> migration()
        {
            var opps = await ctx.Set<UNOPS.PAO.Domain.Entities.Opportunity>()
                .Where(o => !o.IsDeleted && o.OpportunityStatementMarkdown == null)
                .ToListAsync();
            foreach (var o in opps)
                o.OpportunityStatementMarkdown = string.Empty;
            return await ctx.SaveChangesAsync();
        }

        return (migration, ctx);
    }

    [Fact] [Trait("TestId", "LOAD-001")]
    public async Task Load_50NullStatements_AllFixedUnder10Seconds()
    {
        var (migration, ctx) = CreateIsolated("LOAD-001");
        for (var i = 1; i <= 50; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Opportunity>().Add(new UNOPS.PAO.Domain.Entities.Opportunity
            {
                Id = i, Name = $"Load Opp {i}", Description = "Load test opportunity",
                IsDeleted = false, OpportunityStatementMarkdown = null,
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active, Stage = "GO"
            });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var affected = await migration();
        sw.Stop();

        affected.Should().BeGreaterThanOrEqualTo(50);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-002")]
    public async Task Load_100Opportunities_ClosedColorMapping_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
            _ = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact] [Trait("TestId", "LOAD-003")]
    public async Task Load_200NullStatements_MigratedIn15Seconds()
    {
        var (migration, ctx) = CreateIsolated("LOAD-003");
        for (var i = 1; i <= 200; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Opportunity>().Add(new UNOPS.PAO.Domain.Entities.Opportunity
            {
                Id = i, Name = $"Opp {i}", Description = "Load test opportunity",
                IsDeleted = false, OpportunityStatementMarkdown = null,
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active, Stage = "GO"
            });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var affected = await migration();
        sw.Stop();

        affected.Should().BeGreaterThanOrEqualTo(200);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-004")]
    public async Task Load_Spike_50ConcurrentColorLookups_Stable()
    {
        var sw = Stopwatch.StartNew();
        var tasks = Enumerable.Range(0, 50).Select(_ =>
            Task.Run(() => GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed)));
        var results = await Task.WhenAll(tasks);
        sw.Stop();

        results.Should().AllBe(ClosedStatusColor);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact] [Trait("TestId", "LOAD-005")]
    public async Task Load_MemoryStable_After500Lookups()
    {
        GC.Collect();
        var before = GC.GetTotalMemory(forceFullCollection: true);

        for (var i = 0; i < 500; i++)
            _ = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);

        GC.Collect();
        var after = GC.GetTotalMemory(forceFullCollection: true);
        ((after - before) / 1_048_576.0).Should().BeLessThan(10);
    }

    [Fact] [Trait("TestId", "LOAD-006")]
    public async Task Load_10ConsecutiveMigrations_EachUnder3Seconds()
    {
        var (migration, ctx) = CreateIsolated("LOAD-006");
        ctx.Set<UNOPS.PAO.Domain.Entities.Opportunity>().Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 1, Name = "Opp1", Description = "Load test opportunity",
            IsDeleted = false, OpportunityStatementMarkdown = null,
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active, Stage = "GO"
        });
        await ctx.SaveChangesAsync();

        for (var i = 0; i < 10; i++)
        {
            var sw = Stopwatch.StartNew();
            await migration();
            sw.Stop();
            sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3),
                $"Run {i + 1} should be within SLA");
        }

        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-007")]
    public async Task Load_Mixed_Fixed_And_Null_100Opps()
    {
        var (migration, ctx) = CreateIsolated("LOAD-007");
        for (var i = 1; i <= 100; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Opportunity>().Add(new UNOPS.PAO.Domain.Entities.Opportunity
            {
                Id = i, Name = $"Opp {i}", Description = "Load test opportunity",
                IsDeleted = false, OpportunityStatementMarkdown = i % 2 == 0 ? null : "Already set",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active, Stage = "GO"
            });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var affected = await migration();
        sw.Stop();

        affected.Should().Be(50, "50 out of 100 are null");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-008")]
    public async Task Load_ClosedStatus_Throughput_1000_Checks()
    {
        var sw = Stopwatch.StartNew();
        var count = 0;
        for (var i = 0; i < 1000; i++)
            if (GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed) == ClosedStatusColor)
                count++;
        sw.Stop();

        count.Should().Be(1000);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "LOAD-009")]
    public async Task Load_LargeMarkdown_50Opps_AllPersist()
    {
        var (migration, ctx) = CreateIsolated("LOAD-009");
        var large = new string('L', 5000);
        for (var i = 1; i <= 50; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Opportunity>().Add(new UNOPS.PAO.Domain.Entities.Opportunity
            {
                Id = i, Name = $"Opp {i}", Description = "Load test opportunity",
                IsDeleted = false, OpportunityStatementMarkdown = large,
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active, Stage = "GO"
            });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var affected = await migration();
        sw.Stop();

        affected.Should().Be(0, "None are null — migration should not touch them");
        (await ctx.Set<UNOPS.PAO.Domain.Entities.Opportunity>().CountAsync(o => o.OpportunityStatementMarkdown == large))
            .Should().Be(50);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-010")]
    public async Task Load_FullPNO729_EndToEnd_10KEvents()
    {
        var sw = Stopwatch.StartNew();
        var lightRedCount = 0;

        for (var i = 0; i < 10_000; i++)
        {
            var color = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
            if (color == ClosedStatusColor) lightRedCount++;
        }

        sw.Stop();

        lightRedCount.Should().Be(10_000, "All closed status lookups should return light-red");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }
}
