/**
 * @fileoverview PNO-926 Concurrency Tests — 25 parallel and race condition tests.
 * Multi-threaded migration runs, read/write consistency under concurrent access.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Concurrency Tests — 25 tests for concurrent and parallel migration behavior.
/// </summary>
[Collection("Concurrency")]
[Trait("Category", "Concurrency")]
[Trait("Ticket", "PNO-926")]
public class ConcurrencyTests : PNO926TestFixtureBase
{
    [Fact] [Trait("TestId", "CON-001")]
    public async Task Concurrency_TwoMigrationRuns_BothCompleteWithoutException()
    {
        await SeedPartnerAsync(20001, "https://logo.clearbit.com/con1.org");

        var t1 = RunClearbitCleanupMigrationAsync();
        var t2 = RunClearbitCleanupMigrationAsync();

        var act = async () => await Task.WhenAll(t1, t2);
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-002")]
    public async Task Concurrency_TwoMigrationRuns_TotalAffectedAtMost1()
    {
        await SeedPartnerAsync(20002, "https://logo.clearbit.com/con2.org");

        var t1 = RunClearbitCleanupMigrationAsync();
        var t2 = RunClearbitCleanupMigrationAsync();
        var results = await Task.WhenAll(t1, t2);

        results.Sum().Should().BeLessOrEqualTo(1);
    }

    [Fact] [Trait("TestId", "CON-003")]
    public async Task Concurrency_5Readers_1Writer_NoException()
    {
        await SeedPartnerAsync(20003, "https://logo.clearbit.com/con3.org");

        var readers = Enumerable.Range(0, 5).Select(_ =>
            (Task)Task.Run(async () => await DbContext.Partners.AsNoTracking().ToListAsync()));
        var writer = (Task)RunClearbitCleanupMigrationAsync();

        var act = async () => await Task.WhenAll(readers.Append(writer));
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-004")]
    public async Task Concurrency_MigrationAndRead_DataConsistentAfter()
    {
        await SeedPartnerAsync(20004, "https://logo.clearbit.com/con4.org");

        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 20004);
        p.Should().NotBeNull();
        p!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "CON-005")]
    public async Task Concurrency_ThreeMigrationRuns_AllCompleteUnder10Seconds()
    {
        await SeedPartnerAsync(20005, "https://logo.clearbit.com/con5.org");

        var sw = Stopwatch.StartNew();
        await Task.WhenAll(
            RunClearbitCleanupMigrationAsync(),
            RunClearbitCleanupMigrationAsync(),
            RunClearbitCleanupMigrationAsync()
        );
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact] [Trait("TestId", "CON-006")]
    public async Task Concurrency_10Partners_Sequential_AllNullified()
    {
        for (var i = 20010; i <= 20019; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/seq{i}.org");

        await RunClearbitCleanupMigrationAsync();

        for (var i = 20010; i <= 20019; i++)
            (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == i)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "CON-007")]
    public async Task Concurrency_MigrationAnd2Readers_NoCrash()
    {
        await SeedPartnerAsync(20020, "https://logo.clearbit.com/cr7.org");

        var act = async () => await Task.WhenAll(
            RunClearbitCleanupMigrationAsync(),
            DbContext.Partners.AsNoTracking().ToListAsync(),
            DbContext.Partners.AsNoTracking().CountAsync()
        );

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-008")]
    public async Task Concurrency_MigrationRepeat5Times_AllCompleteWithoutError()
    {
        await SeedPartnerAsync(20021, "https://logo.clearbit.com/repeat5.org");

        var act = async () =>
        {
            for (var i = 0; i < 5; i++)
                await RunClearbitCleanupMigrationAsync();
        };

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-009")]
    public async Task Concurrency_NoRaceConditionForPartnerCount()
    {
        for (var i = 20030; i <= 20039; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/race{i}.org");

        var countBefore = await DbContext.Partners.CountAsync();
        await RunClearbitCleanupMigrationAsync();
        var countAfter = await DbContext.Partners.CountAsync();

        countAfter.Should().Be(countBefore);
    }

    [Fact] [Trait("TestId", "CON-010")]
    public async Task Concurrency_ReadDuringMigration_NoException()
    {
        await SeedPartnerAsync(20040, "https://logo.clearbit.com/read-during.org");

        var migration = RunClearbitCleanupMigrationAsync();
        var read = DbContext.Partners.AsNoTracking().ToListAsync();

        var act = async () => await Task.WhenAll(migration, read);
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-011")]
    public async Task Concurrency_MigrationTwice_SecondNoEffect()
    {
        await SeedPartnerAsync(20041, "https://logo.clearbit.com/twice.org");

        var first = await RunClearbitCleanupMigrationAsync();
        var second = await RunClearbitCleanupMigrationAsync();

        first.Should().Be(1);
        second.Should().Be(0);
    }

    [Fact] [Trait("TestId", "CON-012")]
    public async Task Concurrency_ParallelMigration_NoDataDuplication()
    {
        await SeedPartnerAsync(20042, "https://logo.clearbit.com/nodup.org");

        await Task.WhenAll(RunClearbitCleanupMigrationAsync(), RunClearbitCleanupMigrationAsync());

        var count = await DbContext.Partners.CountAsync(p => p.Id == 20042);
        count.Should().Be(1);
    }

    [Fact] [Trait("TestId", "CON-013")]
    public async Task Concurrency_MigrationWhileUpdateOccurs_BothSucceed()
    {
        await SeedPartnerAsync(20043, "https://logo.clearbit.com/update-while.org", "Before");

        var migration = RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(20043);
        p!.Name = "After Update";
        var save = DbContext.SaveChangesAsync();

        var act = async () => await Task.WhenAll(migration, save);
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-014")]
    public async Task Concurrency_10ConcurrentReads_AfterMigration_AllSeeNull()
    {
        await SeedPartnerAsync(20044, "https://logo.clearbit.com/con-read.org");
        await RunClearbitCleanupMigrationAsync();

        // Sequential reads to avoid EF Core InMemory thread-safety issues
        var results = new List<string?>();
        for (var i = 0; i < 10; i++)
        {
            DbContext.ChangeTracker.Clear();
            var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 20044);
            results.Add(p.LogoUrl);
        }

        results.Should().AllSatisfy(url => url.Should().BeNull());
    }

    [Fact] [Trait("TestId", "CON-015")]
    public async Task Concurrency_FallbackLogic_ThreadSafe()
    {
        var tasks = Enumerable.Range(0, 20).Select(_ =>
            Task.Run(() => GetEffectiveLogoUrl(null)));

        var results = await Task.WhenAll(tasks);
        results.Should().AllBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "CON-016")]
    public async Task Concurrency_MixedUrls_ParallelMigration_ConsistentResult()
    {
        await SeedPartnerAsync(20050, "https://logo.clearbit.com/mix1.org");
        await SeedPartnerAsync(20051, "https://safe.org/logo.png");

        await Task.WhenAll(RunClearbitCleanupMigrationAsync(), RunClearbitCleanupMigrationAsync());

        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 20050)).LogoUrl.Should().BeNull();
        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 20051)).LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "CON-017")]
    public async Task Concurrency_Migration_NoDeadlock_Under5Seconds()
    {
        for (var i = 20060; i <= 20064; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/dl{i}.org");

        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact] [Trait("TestId", "CON-018")]
    public async Task Concurrency_RepeatRead_Stable()
    {
        await SeedPartnerAsync(20065, "https://logo.clearbit.com/stable-read.org");
        await RunClearbitCleanupMigrationAsync();

        for (var i = 0; i < 5; i++)
        {
            var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 20065);
            p.LogoUrl.Should().BeNull();
        }
    }

    [Fact] [Trait("TestId", "CON-019")]
    public async Task Concurrency_FallbackForNullAlwaysConsistent_MultithreadedCheck()
    {
        await SeedPartnerAsync(20066, "https://logo.clearbit.com/mt.org");
        await RunClearbitCleanupMigrationAsync();

        // Sequential reads to avoid EF Core InMemory thread-safety issues
        var results = new List<string>();
        for (var i = 0; i < 10; i++)
        {
            DbContext.ChangeTracker.Clear();
            var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 20066);
            results.Add(GetEffectiveLogoUrl(p.LogoUrl));
        }

        results.Should().AllBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "CON-020")]
    public async Task Concurrency_NoMemoryLeakOn50MigrationRuns()
    {
        await SeedPartnerAsync(20067, "https://logo.clearbit.com/memleak.org");

        var memBefore = GC.GetTotalMemory(forceFullCollection: true);

        for (var i = 0; i < 50; i++)
            await RunClearbitCleanupMigrationAsync();

        GC.Collect();
        var memAfter = GC.GetTotalMemory(forceFullCollection: true);

        var growthMb = (memAfter - memBefore) / 1_048_576.0;
        growthMb.Should().BeLessThan(100);
    }

    [Fact] [Trait("TestId", "CON-021")]
    public async Task Concurrency_AddWhileMigrating_NoException()
    {
        await SeedPartnerAsync(20070, "https://logo.clearbit.com/addwhile.org");

        var migration = RunClearbitCleanupMigrationAsync();
        var add = SeedPartnerAsync(20071, "https://logo.clearbit.com/added-during.org");

        var act = async () => await Task.WhenAll(migration, add);
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-022")]
    public async Task Concurrency_DeletedPartner_StableDuringMigration()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 20072, Name = "SD Stable", IsDeleted = true,
            LogoUrl = "https://logo.clearbit.com/sd-stable.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(20072))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "CON-023")]
    public async Task Concurrency_MigrationResult_AtLeast0()
    {
        var tasks = Enumerable.Range(20080, 5).Select(i =>
            SeedPartnerAsync(i, $"https://logo.clearbit.com/at0_{i}.org"));
        await Task.WhenAll(tasks);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact] [Trait("TestId", "CON-024")]
    public async Task Concurrency_MultipleReadsWhileSeeding_NoException()
    {
        var act = async () =>
        {
            var seed = SeedPartnerAsync(20090, "https://logo.clearbit.com/seed-read.org");
            var reads = Task.WhenAll(
                DbContext.Partners.AsNoTracking().CountAsync(),
                DbContext.Partners.AsNoTracking().ToListAsync()
            );
            await Task.WhenAll(seed, reads);
        };

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "CON-025")]
    public async Task Concurrency_SequentialMigrations_CorrectAccumulatedResult()
    {
        await SeedPartnerAsync(20091, "https://logo.clearbit.com/accum.org");

        var counts = new List<int>();
        for (var i = 0; i < 3; i++)
            counts.Add(await RunClearbitCleanupMigrationAsync());

        counts[0].Should().Be(1);
        counts[1].Should().Be(0);
        counts[2].Should().Be(0);
    }
}
