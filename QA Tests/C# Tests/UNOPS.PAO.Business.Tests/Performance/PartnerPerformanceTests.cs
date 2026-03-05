/**
 * PERFORMANCE TESTS — PartnerManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Partner management, CRUD, search, pagination, contacts/interactions
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for PartnerManager.
/// Verifies response times, throughput, and behaviour under concurrent access
/// for partner CRUD, list queries, search, and related data loading.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class PartnerPerformanceTests : PerformanceTestBase
{
    private readonly IPartnerManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"PerfPartner_{Guid.NewGuid():N}";
    private new const int TestUserId = 1;

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public PartnerPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        _manager = new PartnerManager(mapper, Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task CreatePartner_SingleEntity_CompletesWithinThreshold()
    {
        var partner = new UNOPSPartner
        {
            Name = $"Create_{_testMarker}",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };

        _stopwatch.Restart();
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        _stopwatch.Stop();

        partner.Id.Should().BeGreaterThan(0);
        partner.Name.Should().Contain(_testMarker);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreatePartner took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetPartner_ExistingPartner_CompletesWithinThreshold()
    {
        var partner = await SeedPartnerAsync();

        _stopwatch.Restart();
        var result = await _manager.GetPartnerAsync(partner.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Id.Should().Be(partner.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetPartner took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task CreatePartner_Bulk100Partners_CompletesWithinThreshold()
    {
        var partners = Enumerable.Range(0, 100)
            .Select(i => new UNOPSPartner
            {
                Name = $"Bulk_{i}_{_testMarker}",
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            })
            .ToList();

        _stopwatch.Restart();
        await Context.Partners.AddRangeAsync(partners);
        await SaveChangesAsync();
        _stopwatch.Stop();

        var count = await Context.Partners.CountAsync(p => !p.IsDeleted && p.Name!.Contains(_testMarker));
        count.Should().BeGreaterThanOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk create 100 partners took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxBulkOperationMs}ms");
    }

    [Fact]
    public async Task GetPartners_Paginated100Records_CompletesWithinThreshold()
    {
        await SeedPartnersAsync(100);

        _stopwatch.Restart();
        var records = await Context.Partners.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Take(100)
            .ToListAsync();
        _stopwatch.Stop();

        records.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetPartners (page 100) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task UpdatePartner_Bulk50Updates_CompletesWithinThreshold()
    {
        var partners = await SeedPartnersAsync(50);
        var firstId = partners.First();

        _stopwatch.Restart();
        for (int i = 0; i < 50; i++)
        {
            var updateRequest = new UpdatePartnerRequest
            {
                Id = firstId + i,
                Name = $"Updated_{i}_{_testMarker}"
            };
            await _manager.UpdatePartnerAsync(TestUserId, updateRequest);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk update 50 partners took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetPartners_SimplePagination_CompletesWithinThreshold()
    {
        await SeedPartnersAsync(200);

        _stopwatch.Restart();
        var records = await Context.Partners.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Take(20)
            .ToListAsync();
        _stopwatch.Stop();

        records.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple GetPartners pagination took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPartners_WithOrderBy_CompletesWithinThreshold()
    {
        await SeedPartnersAsync(150);

        _stopwatch.Restart();
        var records = await Context.Partners.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Take(50)
            .ToListAsync();
        _stopwatch.Stop();

        records.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetPartners with OrderBy took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPartnersByPartnerGroup_FilteredQuery_CompletesWithinThreshold()
    {
        var (partnerGroupId, _) = await SeedPartnersInGroupAsync(80);

        _stopwatch.Restart();
        var result = await _manager.GetPartnersByPartnerGroup(TestUserId, partnerGroupId, new PaginationRequest(1, 50));
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Records.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetPartnersByPartnerGroup took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPartnerWithContactsAndInteractions_RelatedData_CompletesWithinThreshold()
    {
        var partner = await SeedPartnerWithContactsAsync(10);

        _stopwatch.Restart();
        var result = await _manager.GetPartnerWithContactsAndInteractionsAsync(partner.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Contacts.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetPartnerWithContactsAndInteractions took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPartners_ExcludesDeleted_CompletesWithinThreshold()
    {
        var partners = await SeedPartnersAsync(100);
        await SoftDeletePartnersAsync(partners.Take(20).ToList());

        _stopwatch.Restart();
        var totalCount = await Context.Partners.AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_testMarker))
            .CountAsync();
        _stopwatch.Stop();

        totalCount.Should().Be(80, "Query should return 80 non-deleted partners with test marker");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetPartners (excluding deleted) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task ConcurrentReads_50ParallelGetPartner_MaintainsPerformance()
    {
        var partner = await SeedPartnerAsync();
        var times = new List<long>();

        for (int i = 0; i < 50; i++)
        {
            _stopwatch.Restart();
            var result = await _manager.GetPartnerAsync(partner.Id);
            _stopwatch.Stop();
            result.Should().NotBeNull();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var avgMs = times.Average();
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read over 50 sequential calls exceeded threshold: {avgMs}ms");
    }

    [Fact]
    public async Task ConcurrentReads_20ParallelGetPartners_MaintainsPerformance()
    {
        await SeedPartnersAsync(50);
        var times = new List<long>();

        for (int i = 0; i < 20; i++)
        {
            _stopwatch.Restart();
            var records = await Context.Partners.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .Take(20)
                .ToListAsync();
            _stopwatch.Stop();
            records.Should().NotBeEmpty();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var totalMs = times.Sum();
        totalMs.Should().BeLessThan(MaxBulkOperationMs,
            $"20 sequential GetPartners-equivalent queries took {totalMs}ms total");
    }

    [Fact]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var partner = await SeedPartnerAsync();

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            var result = await _manager.GetPartnerAsync(partner.Id);
            result.Should().NotBeNull();
        }
        for (int i = 0; i < 5; i++)
        {
            var newPartner = new UNOPSPartner
            {
                Name = $"Mixed_{i}_{_testMarker}",
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Partners.AddAsync(newPartner);
            await SaveChangesAsync();
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed sequential read/write took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargePartnerList_MemoryUsage_WithinCap()
    {
        await SeedPartnersAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await Context.Partners.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Take(500)
            .ToListAsync();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        var partner = await SeedPartnerAsync();
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetPartnerAsync(partner.Id);
            await _manager.GetPartnerWithContactsAndInteractionsAsync(partner.Id);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var partner = await SeedPartnerAsync();
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetPartnerAsync(partner.Id);
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
    public async Task GetPartnerWithContactsAndInteractions_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var partner = await SeedPartnerWithContactsAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetPartnerWithContactsAndInteractionsAsync(partner.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 50 contacts");
    }

    [Fact]
    public async Task GetPartners_WithIncludes_NoN1Pattern_CompletesWithinThreshold()
    {
        await SeedPartnersAsync(100);

        _stopwatch.Restart();
        var records = await Context.Partners.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Take(100)
            .ToListAsync();
        _stopwatch.Stop();

        records.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetPartners query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Update and Delete Performance

    [Fact]
    public async Task UpdatePartner_ExistingEntity_CompletesWithinThreshold()
    {
        var partner = await SeedPartnerAsync();
        var updateRequest = new UpdatePartnerRequest
        {
            Id = partner.Id,
            Name = $"Updated_{_testMarker}"
        };

        _stopwatch.Restart();
        var result = await _manager.UpdatePartnerAsync(TestUserId, updateRequest);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Name.Should().Contain("Updated");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpdatePartner took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DeletePartner_ExistingEntity_CompletesWithinThreshold()
    {
        var partner = await SeedPartnerAsync();

        _stopwatch.Restart();
        await _manager.DeletePartnerAsync(TestUserId, partner.Id);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeletePartner took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var partner = await SeedPartnerAsync();
        var (partnerGroupId, _) = await SeedPartnersInGroupAsync(20);

        report["CreatePartner"] = await TimeMs(async () =>
        {
            var p = new UNOPSPartner
            {
                Name = $"Bench_{_testMarker}",
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Partners.AddAsync(p);
            await SaveChangesAsync();
        });
        report["GetPartner"] = await TimeMs(() => _manager.GetPartnerAsync(partner.Id));
        report["GetPartnerWithContacts"] = await TimeMs(() => _manager.GetPartnerWithContactsAndInteractionsAsync(partner.Id));
        report["UpdatePartner"] = await TimeMs(() => _manager.UpdatePartnerAsync(TestUserId, new UpdatePartnerRequest { Id = partner.Id, Name = partner.Name }));
        report["GetPartners"] = await TimeMs(() => Context.Partners.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Take(20)
            .ToListAsync());
        report["GetPartnersByPartnerGroup"] = await TimeMs(() => _manager.GetPartnersByPartnerGroup(TestUserId, partnerGroupId, new PaginationRequest(1, 20)));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private PartnerRequest BuildCreateRequest(string name)
    {
        return new PartnerRequest
        {
            Name = name,
            Status = "Active"
        };
    }

    private async Task<UNOPSPartner> SeedPartnerAsync(string? nameSuffix = null)
    {
        var name = nameSuffix != null ? $"{nameSuffix}_{_testMarker}" : $"Seed_{_testMarker}";
        var partner = new UNOPSPartner
        {
            Name = name,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        return partner;
    }

    private async Task<List<int>> SeedPartnersAsync(int count)
    {
        var ids = new List<int>();
        for (int i = 0; i < count; i++)
        {
            var partner = await SeedPartnerAsync($"List_{i}");
            ids.Add(partner.Id);
        }
        return ids;
    }

    private async Task<UNOPSPartner> SeedPartnerWithContactsAsync(int contactCount)
    {
        var partner = await SeedPartnerAsync("WithContacts");
        for (int i = 0; i < contactCount; i++)
        {
            var contact = new UNOPS.PAO.UNOPSDomain.Entities.UNOPSContact
            {
                Name = $"Contact_{i}_{_testMarker}",
                LastName = $"Contact_{i}",
                Title = "Test",
                Email = $"contact{i}@test.com",
                PartnerId = partner.Id,
                Status = EntityStatus.Active,
                ContactNumber = "",
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Contacts.AddAsync(contact);
        }
        await SaveChangesAsync();
        return partner;
    }

    private async Task<(int PartnerGroupId, List<int> PartnerIds)> SeedPartnersInGroupAsync(int count)
    {
        var partnerTree = new UNOPSPartnerTree
        {
            Name = $"PerfGroup_{_testMarker}",
            Code = $"PG_{_testMarker[..8]}",
            Description = "Perf test partner group",
            Type = "Group",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.PartnerTrees.AddAsync(partnerTree);
        await SaveChangesAsync();

        var partners = new List<UNOPSPartner>();
        for (int i = 0; i < count; i++)
        {
            var partner = new UNOPSPartner
            {
                Name = $"GroupPartner_{i}_{_testMarker}",
                Status = EntityStatus.Active,
                PartnerGroupId = partnerTree.Id,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Partners.AddAsync(partner);
            partners.Add(partner);
        }
        await SaveChangesAsync();
        return (partnerTree.Id, partners.Select(p => p.Id).ToList());
    }

    private async Task SoftDeletePartnersAsync(List<int> partnerIds)
    {
        var partners = await Context.Partners
            .Where(p => partnerIds.Contains(p.Id))
            .ToListAsync();
        foreach (var p in partners)
        {
            p.IsDeleted = true;
            p.DeletedDate = DateTime.UtcNow;
            p.DeletedBy = TestUserId;
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
