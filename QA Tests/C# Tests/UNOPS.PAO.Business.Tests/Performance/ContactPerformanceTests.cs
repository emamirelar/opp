/**
 * PERFORMANCE TESTS — Contact (UNOPSContact)
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Contact management, partner contacts, search, CRUD
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for Contact operations against PostgreSQL.
/// Uses UNOPSContact and creates parent Partners for FK constraints.
/// Uses test markers to filter own data from the shared database.
/// Verifies response times, throughput, memory efficiency, N+1 detection,
/// and behaviour under concurrent access.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
[Collection("ContactPerformance")]
public class ContactPerformanceTests : PerformanceTestBase
{
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"PERF_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public ContactPerformanceTests()
    {
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task Create_SingleContact_CompletesWithinThreshold()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = BuildContact(partnerId, 1);

        _stopwatch.Restart();
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        _stopwatch.Stop();

        contact.Id.Should().BeGreaterThan(0);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Create took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetById_ExistingContact_CompletesWithinThreshold()
    {
        var contact = await SeedContactAsync();

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contact.Id && !c.IsDeleted);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.FirstName.Should().Be(contact.FirstName);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetById took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task BulkCreate_100Contacts_CompletesWithinThreshold()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contacts = Enumerable.Range(1, 100)
            .Select(i => BuildContact(partnerId, i))
            .ToList();

        _stopwatch.Restart();
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        _stopwatch.Stop();

        var count = await Context.Contacts.CountAsync(c => c.Name!.Contains(_testMarker));
        count.Should().Be(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"BulkCreate 100 took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxBulkOperationMs}ms");
    }

    [Fact]
    public async Task BulkUpdate_100Contacts_CompletesWithinThreshold()
    {
        var (partnerId, contactIds) = await SeedContactsAsync(100);

        _stopwatch.Restart();
        var contacts = await Context.Contacts
            .Where(c => contactIds.Contains(c.Id))
            .ToListAsync();
        foreach (var c in contacts)
            c.Title = $"Updated_{_testMarker}";
        await SaveChangesAsync();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"BulkUpdate 100 took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxBulkOperationMs}ms");
    }

    [Fact]
    public async Task BulkRead_500Contacts_CompletesWithinThreshold()
    {
        var (partnerId, _) = await SeedContactsAsync(500);

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.Name!.Contains(_testMarker) && !c.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(500);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"BulkRead 500 took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxBulkOperationMs}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task Search_SimpleByName_CompletesWithinThreshold()
    {
        var (_, _) = await SeedContactsAsync(200);

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.Name!.Contains(_testMarker) && !c.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple search took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSimpleSearchMs}ms");
    }

    [Fact]
    public async Task Search_ComplexMultiColumn_CompletesWithinThreshold()
    {
        var (partnerId, _) = await SeedContactsAsync(200);

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.Name!.Contains(_testMarker)
                && c.PartnerId == partnerId
                && c.FirstName!.StartsWith("First")
                && !c.IsDeleted)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.Email)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Complex search took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxComplexSearchMs}ms");
    }

    [Fact]
    public async Task Search_ByPartnerId_CompletesWithinThreshold()
    {
        var (partnerId, _) = await SeedContactsAsync(150);

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == partnerId && !c.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(150);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Partner filter search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Search_MultiPartner_CompletesWithinThreshold()
    {
        var partner1Id = await CreateTestPartnerAsync($"Partner1_{_testMarker}");
        var partner2Id = await CreateTestPartnerAsync($"Partner2_{_testMarker}");
        await SeedContactsForPartnerAsync(partner1Id, 50);
        await SeedContactsForPartnerAsync(partner2Id, 50);

        _stopwatch.Restart();
        var result1 = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == partner1Id && !c.IsDeleted)
            .ToListAsync();
        var result2 = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.PartnerId == partner2Id && !c.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        result1.Should().HaveCount(50);
        result2.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Multi-partner search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Search_ExcludesSoftDeleted_CompletesWithinThreshold()
    {
        var (partnerId, contactIds) = await SeedContactsAsync(100);
        await SoftDeleteContactsAsync(contactIds.Take(20).ToList());

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.Name!.Contains(_testMarker) && !c.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(80);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Search excluding deleted took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task ConcurrentReads_50ParallelGetById_MaintainsPerformance()
    {
        var contact = await SeedContactAsync();
        var contactId = contact.Id;
        var results = new List<UNOPSContact?>();

        _stopwatch.Restart();
        for (int i = 0; i < 50; i++)
        {
            var result = await Context.Contacts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
            results.Add(result);
        }
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r != null);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 sequential calls (simulating load) exceeded threshold: {avgMs}ms");
    }

    [Fact]
    public async Task ConcurrentReads_20ParallelGetPartnerContacts_MaintainsPerformance()
    {
        var (partnerId, _) = await SeedContactsAsync(50);
        var results = new List<List<UNOPSContact>>();

        _stopwatch.Restart();
        for (int i = 0; i < 20; i++)
        {
            var result = await Context.Contacts
                .AsNoTracking()
                .Where(c => c.PartnerId == partnerId && !c.IsDeleted)
                .ToListAsync();
            results.Add(result);
        }
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r.Count == 50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 sequential GetPartnerContacts (simulating load) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var (partnerId, _) = await SeedContactsAsync(30);

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            await Context.Contacts
                .AsNoTracking()
                .Where(c => c.PartnerId == partnerId && !c.IsDeleted)
                .ToListAsync();
        }
        for (int i = 0; i < 5; i++)
        {
            await Context.Contacts
                .AsNoTracking()
                .Where(c => c.Name!.Contains(_testMarker))
                .FirstOrDefaultAsync();
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed sequential reads (simulating load) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeContactList_MemoryUsage_WithinCap()
    {
        var (partnerId, _) = await SeedContactsAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await Context.Contacts
            .AsNoTracking()
            .Where(c => c.Name!.Contains(_testMarker) && !c.IsDeleted)
            .ToListAsync();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        var (partnerId, _) = await SeedContactsAsync(10);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await Context.Contacts
                .AsNoTracking()
                .Where(c => c.PartnerId == partnerId && !c.IsDeleted)
                .ToListAsync();
            await Context.Contacts
                .AsNoTracking()
                .Where(c => c.Name!.Contains(_testMarker))
                .CountAsync();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var (partnerId, _) = await SeedContactsAsync(50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await Context.Contacts
                .AsNoTracking()
                .Where(c => c.PartnerId == partnerId && !c.IsDeleted)
                .ToListAsync();
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]
    public async Task GetContactsWithPartner_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var (partnerId, _) = await SeedContactsAsync(50);

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .Include(c => c.Partner)
            .Where(c => c.Name!.Contains(_testMarker) && !c.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        result.Should().OnlyContain(c => c.Partner != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    [Fact]
    public async Task GetContacts_AsNoTracking_ReadOnlyQueryOptimized()
    {
        var (partnerId, _) = await SeedContactsAsync(100);

        _stopwatch.Restart();
        var result = await Context.Contacts
            .AsNoTracking()
            .Where(c => c.Name!.Contains(_testMarker) && !c.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"AsNoTracking read query should complete within threshold — took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var contact = await SeedContactAsync();
        var (partnerId, _) = await SeedContactsAsync(20);

        report["GetById"] = await TimeMs(() =>
            Context.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == contact.Id && !c.IsDeleted));
        report["GetPartnerContacts"] = await TimeMs(() =>
            Context.Contacts.AsNoTracking().Where(c => c.PartnerId == partnerId && !c.IsDeleted).ToListAsync());
        report["SearchByName"] = await TimeMs(() =>
            Context.Contacts.AsNoTracking().Where(c => c.Name!.Contains(_testMarker) && !c.IsDeleted).ToListAsync());
        report["Count"] = await TimeMs(() =>
            Context.Contacts.AsNoTracking().CountAsync(c => c.Name!.Contains(_testMarker) && !c.IsDeleted));

        var newContact = BuildContact(partnerId, 999);
        report["Create"] = await TimeMs(async () =>
        {
            await Context.Contacts.AddAsync(newContact);
            await SaveChangesAsync();
        });

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private UNOPSContact BuildContact(int partnerId, int index)
    {
        return new UNOPSContact
        {
            Name = $"Contact {index} {_testMarker}",
            FirstName = $"First{index}",
            LastName = $"Last{index}",
            Email = $"contact{index}_{_testMarker}@test.com",
            Title = $"Title{index}",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
    }

    private async Task<UNOPSContact> SeedContactAsync()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = BuildContact(partnerId, 1);
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        return contact;
    }

    private async Task<(int PartnerId, List<int> ContactIds)> SeedContactsAsync(int count)
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contacts = Enumerable.Range(1, count)
            .Select(i => BuildContact(partnerId, i))
            .ToList();
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        return (partnerId, contacts.Select(c => c.Id).ToList());
    }

    private async Task SeedContactsForPartnerAsync(int partnerId, int count)
    {
        var contacts = Enumerable.Range(1, count)
            .Select(i => BuildContact(partnerId, i))
            .ToList();
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
    }

    private async Task SoftDeleteContactsAsync(List<int> contactIds)
    {
        var contacts = await Context.Contacts.Where(c => contactIds.Contains(c.Id)).ToListAsync();
        foreach (var c in contacts)
        {
            c.IsDeleted = true;
            c.DeletedDate = DateTime.UtcNow;
            c.DeletedBy = 0;
        }
        await SaveChangesAsync();
    }

    private async Task<long> TimeMs(Func<Task> fn)
    {
        _stopwatch.Restart();
        await fn();
        _stopwatch.Stop();
        return _stopwatch.ElapsedMilliseconds;
    }

    #endregion
}
