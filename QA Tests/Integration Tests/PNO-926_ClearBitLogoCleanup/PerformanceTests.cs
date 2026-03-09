/**
 * @fileoverview PNO-926 Performance Tests — 16 response time and throughput tests.
 * Migration execution time, query performance, and memory usage validation.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Performance Tests — 16 tests for migration and query execution time.
/// </summary>
[Collection("Performance")]
[Trait("Category", "Performance")]
[Trait("Ticket", "PNO-926")]
public class PerformanceTests : PNO926TestFixtureBase
{
    [Fact] [Trait("TestId", "PERF-001")]
    public async Task Performance_SingleClearbitPartner_MigratesUnder1Second()
    {
        await SeedPartnerAsync(30001, "https://logo.clearbit.com/perf1.org");

        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact] [Trait("TestId", "PERF-002")]
    public async Task Performance_10ClearbitPartners_MigratesUnder2Seconds()
    {
        for (var i = 30010; i <= 30019; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/perf{i}.org");

        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "PERF-003")]
    public async Task Performance_50ClearbitPartners_MigratesUnder5Seconds()
    {
        for (var i = 30020; i <= 30069; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/perf{i}.org");

        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact] [Trait("TestId", "PERF-004")]
    public async Task Performance_100Partners_MixedUrls_MigratesUnder10Seconds()
    {
        for (var i = 30100; i <= 30149; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/p{i}.org");
        for (var i = 30150; i <= 30199; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact] [Trait("TestId", "PERF-005")]
    public async Task Performance_EmptyDb_MigratesUnder500Ms()
    {
        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact] [Trait("TestId", "PERF-006")]
    public async Task Performance_SecondRun_IdempotentCheck_Under500Ms()
    {
        await SeedPartnerAsync(30200, "https://logo.clearbit.com/idem.org");
        await RunClearbitCleanupMigrationAsync();

        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact] [Trait("TestId", "PERF-007")]
    public async Task Performance_SingleQuery_PartnerLookup_Under200Ms()
    {
        await SeedPartnerAsync(30201, "https://logo.clearbit.com/query-perf.org");
        await RunClearbitCleanupMigrationAsync();

        var sw = Stopwatch.StartNew();
        var p = await DbContext.Partners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 30201);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
        p.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "PERF-008")]
    public async Task Performance_GetEffectiveLogoUrl_1000Calls_Under100Ms()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
            GetEffectiveLogoUrl(null);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact] [Trait("TestId", "PERF-009")]
    public async Task Performance_SeedAndMigrate_20Partners_Under3Seconds()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 30210; i <= 30229; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/sm{i}.org");
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact] [Trait("TestId", "PERF-010")]
    public async Task Performance_NullUrls_MigrationFast_EmptyResultsIn500Ms()
    {
        for (var i = 30300; i <= 30319; i++)
            await SeedPartnerAsync(i, null);

        var sw = Stopwatch.StartNew();
        var affected = await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        affected.Should().Be(0, "null URLs should not be treated as clearbit URLs");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500),
            $"null URL migration took {sw.ElapsedMilliseconds}ms");
    }

    [Fact] [Trait("TestId", "PERF-011")]
    public async Task Performance_SafeUrls_MigrationFast_ZeroAffectedIn500Ms()
    {
        for (var i = 30320; i <= 30339; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        var sw = Stopwatch.StartNew();
        var affected = await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        affected.Should().Be(0, "safe (non-clearbit) URLs should not be affected");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500),
            $"safe URL migration took {sw.ElapsedMilliseconds}ms");
    }

    [Fact] [Trait("TestId", "PERF-012")]
    public async Task Performance_CountQuery_Under200Ms()
    {
        for (var i = 30400; i <= 30409; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/cnt{i}.org");

        var sw = Stopwatch.StartNew();
        var count = await DbContext.Partners.AsNoTracking()
            .Where(p => p.LogoUrl != null && p.LogoUrl.Contains("clearbit"))
            .CountAsync();
        sw.Stop();

        count.Should().BeGreaterThan(0);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
    }

    [Fact] [Trait("TestId", "PERF-013")]
    public async Task Performance_ToListQuery_Under500Ms()
    {
        for (var i = 30500; i <= 30519; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/list{i}.org");
        await RunClearbitCleanupMigrationAsync();

        var sw = Stopwatch.StartNew();
        var list = await DbContext.Partners.AsNoTracking()
            .Where(p => p.Id >= 30500 && p.Id <= 30519)
            .ToListAsync();
        sw.Stop();

        list.Should().HaveCount(20);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact] [Trait("TestId", "PERF-014")]
    public async Task Performance_MemoryGrowth_After30MigrationRuns_Under50Mb()
    {
        await SeedPartnerAsync(30600, "https://logo.clearbit.com/memory.org");

        GC.Collect();
        var memBefore = GC.GetTotalMemory(forceFullCollection: true);

        for (var i = 0; i < 30; i++)
            await RunClearbitCleanupMigrationAsync();

        GC.Collect();
        var memAfter = GC.GetTotalMemory(forceFullCollection: true);

        var growthMb = (memAfter - memBefore) / 1_048_576.0;
        growthMb.Should().BeLessThan(50,
            $"30 migration runs caused {growthMb:F1}MB memory growth");
    }

    [Fact] [Trait("TestId", "PERF-015")]
    public async Task Performance_FallbackLogicFor100Partners_Under200Ms()
    {
        for (var i = 30700; i <= 30799; i++)
            await SeedPartnerAsync(i, i % 2 == 0 ? null : "https://safe.org/logo.png");
        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners
            .AsNoTracking()
            .Where(p => p.Id >= 30700 && p.Id <= 30799)
            .ToListAsync();

        var sw = Stopwatch.StartNew();
        foreach (var p in partners)
            GetEffectiveLogoUrl(p.LogoUrl);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
    }

    [Fact] [Trait("TestId", "PERF-016")]
    public async Task Performance_IdempotentMigrations_10Runs_Under5Seconds()
    {
        await SeedPartnerAsync(30800, "https://logo.clearbit.com/idem10.org");

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 10; i++)
            await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }
}
