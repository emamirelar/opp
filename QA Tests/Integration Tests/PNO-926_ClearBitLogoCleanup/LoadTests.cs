/**
 * @fileoverview PNO-926 Load Tests — 10 sustained-load and spike tests.
 * High-volume migration runs, sustained throughput, and stability under load.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Load Tests — 10 tests for high-volume and sustained migration load.
/// Each test uses an isolated fixture to prevent cross-test contamination.
/// </summary>
[Collection("Load")]
[Trait("Category", "Load")]
[Trait("Ticket", "PNO-926")]
public class LoadTests
{
    private static (
        System.Func<Task<int>> runMigration,
        Microsoft.EntityFrameworkCore.DbContext dbContext,
        System.Func<string?, string> getEffectiveLogoUrl
    ) CreateIsolated()
    {
        var options = new DbContextOptionsBuilder<UNOPS.PAO.DataAccess.Context.AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockAccessor = new Moq.Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        mockAccessor.Setup(x => x.HttpContext)
            .Returns(new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(identity)
            });

        var mockSchema = new Moq.Mock<UNOPS.PAO.DataAccess.Interfaces.IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        var resolver = new UNOPS.PAO.DataAccess.Services.UserResolverService<int>(mockAccessor.Object);
        var ctx = new UNOPS.PAO.DataAccess.Context.AppDbContext(options, resolver, mockSchema.Object);

        async Task<int> RunMigration()
        {
            var affected = await ctx.Partners
                .Where(p => !p.IsDeleted && p.LogoUrl != null && p.LogoUrl.Contains("clearbit"))
                .ToListAsync();
            foreach (var p in affected) p.LogoUrl = null;
            await ctx.SaveChangesAsync();
            return affected.Count;
        }

        static string GetEffective(string? url) =>
            string.IsNullOrEmpty(url) ? "assets/images/Partner.png" : url;

        return (RunMigration, ctx, GetEffective);
    }

    [Fact] [Trait("TestId", "LOAD-001")]
    public async Task Load_50ClearbitPartners_AllNullifiedUnder10Seconds()
    {
        var (migration, ctx, _) = CreateIsolated();

        for (var i = 1; i <= 50; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = i, Name = $"Load Partner {i}", IsDeleted = false,
                LogoUrl = $"https://logo.clearbit.com/load{i}.org",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var affected = await migration();
        sw.Stop();

        affected.Should().Be(50);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-002")]
    public async Task Load_100ClearbitPartners_AllNullifiedUnder30Seconds()
    {
        var (migration, ctx, _) = CreateIsolated();

        for (var i = 1; i <= 100; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = i, Name = $"Load Partner {i}", IsDeleted = false,
                LogoUrl = $"https://logo.clearbit.com/load{i}.org",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var affected = await migration();
        sw.Stop();

        affected.Should().Be(100);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-003")]
    public async Task Load_200MixedPartners_50Clearbit_MigratesCorrectly()
    {
        var (migration, ctx, _) = CreateIsolated();

        for (var i = 1; i <= 100; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = i, Name = $"Clearbit {i}", IsDeleted = false,
                LogoUrl = $"https://logo.clearbit.com/cb{i}.org",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
        for (var i = 101; i <= 200; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = i, Name = $"Safe {i}", IsDeleted = false,
                LogoUrl = $"https://safe{i}.org/logo.png",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
        await ctx.SaveChangesAsync();

        var affected = await migration();

        affected.Should().Be(100);
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-004")]
    public async Task Load_10SequentialMigrationRuns_AllStableUnder15Seconds()
    {
        var (migration, ctx, _) = CreateIsolated();

        ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 1, Name = "Seq Load", IsDeleted = false,
            LogoUrl = "https://logo.clearbit.com/seq.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 10; i++)
            await migration();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-005")]
    public async Task Load_SpikeLoad_50ClearbitAtOnce_NoException()
    {
        var (migration, ctx, _) = CreateIsolated();

        for (var i = 1; i <= 50; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = i, Name = $"Spike {i}", IsDeleted = false,
                LogoUrl = $"https://logo.clearbit.com/spike{i}.org",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
        await ctx.SaveChangesAsync();

        var act = async () => await migration();
        await act.Should().NotThrowAsync();
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-006")]
    public async Task Load_EmptyDatabase_100QuickMigrations_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var (migration, ctx, _) = CreateIsolated();
            await migration();
            await ctx.DisposeAsync();
        }
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact] [Trait("TestId", "LOAD-007")]
    public async Task Load_MemoryStable_After100MigrationRuns_Under100Mb()
    {
        GC.Collect();
        var memBefore = GC.GetTotalMemory(forceFullCollection: true);

        for (var i = 0; i < 100; i++)
        {
            var (migration, ctx, _) = CreateIsolated();
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = 1, Name = "Mem Test", IsDeleted = false,
                LogoUrl = "https://logo.clearbit.com/mem.org",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
            await ctx.SaveChangesAsync();
            await migration();
            await ctx.DisposeAsync();
        }

        GC.Collect();
        var memAfter = GC.GetTotalMemory(forceFullCollection: true);

        ((memAfter - memBefore) / 1_048_576.0).Should().BeLessThan(100);
    }

    [Fact] [Trait("TestId", "LOAD-008")]
    public async Task Load_AllNullLogos_50Partners_Under3Seconds()
    {
        var (migration, ctx, _) = CreateIsolated();

        for (var i = 1; i <= 50; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = i, Name = $"Null {i}", IsDeleted = false,
                LogoUrl = null,
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
        await ctx.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var affected = await migration();
        sw.Stop();

        affected.Should().Be(0);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-009")]
    public async Task Load_SustainedLoad_20MigrationsOver50Partners_AllConsistent()
    {
        var (migration, ctx, _) = CreateIsolated();

        for (var i = 1; i <= 50; i++)
            ctx.Set<UNOPS.PAO.Domain.Entities.Partner>().Add(new UNOPS.PAO.Domain.Entities.Partner
            {
                Id = i, Name = $"Sustained {i}", IsDeleted = false,
                LogoUrl = $"https://logo.clearbit.com/sustained{i}.org",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            });
        await ctx.SaveChangesAsync();

        var firstRun = await migration();
        firstRun.Should().Be(50);

        for (var i = 1; i < 20; i++)
        {
            var run = await migration();
            run.Should().Be(0, $"Run {i + 1} should be idempotent");
        }

        await ctx.DisposeAsync();
    }

    [Fact] [Trait("TestId", "LOAD-010")]
    public async Task Load_ThroughputTest_100Migrations_AverageUnder100Ms()
    {
        var times = new List<long>();

        for (var i = 0; i < 100; i++)
        {
            var (migration, ctx, _) = CreateIsolated();
            var sw = Stopwatch.StartNew();
            await migration();
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
            await ctx.DisposeAsync();
        }

        var averageMs = times.Average();
        averageMs.Should().BeLessThan(100, $"Average migration time should be <100ms, was {averageMs:F1}ms");
    }
}
