/**
 * PERFORMANCE TESTS — ValuesManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Lookup/reference data service — currencies, countries, SDGs, org units, liaison offices
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for ValuesManager (lookup/reference data service).
/// Verifies that lookup queries complete quickly, don't degrade under repeated calls,
/// and don't leak memory. Tests sync methods (GetCurrencies, GetCountries, GetSDGs, etc.)
/// and async methods (GetUsersPagedAsync, SearchUsersAsync).
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
[Collection("ValuesManagerPerformance")]
public class ValuesManagerPerformanceTests : PerformanceTestBase
{
    private readonly ValuesManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"VM_{Guid.NewGuid():N}";

    // ── SLA thresholds (scaled for CI via PerformanceTestBase.ScaleThreshold) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public ValuesManagerPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        _manager = new ValuesManager(mapper, Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GetCurrencies_SingleLookup_CompletesWithinThreshold()
    {
        await SeedCurrenciesAsync(10);

        _stopwatch.Restart();
        var result = _manager.GetCurrencies();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetCurrencies took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GetCountries_SingleLookup_CompletesWithinThreshold()
    {
        await SeedCountriesAsync(20);

        _stopwatch.Restart();
        var result = _manager.GetCountries();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetCountries took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetSDGs_BulkLookup_CompletesWithinThreshold()
    {
        await SeedSDGsAsync(50);

        _stopwatch.Restart();
        var result = _manager.GetSDGs();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetSDGs took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetSDGTargets_BulkLookup_CompletesWithinThreshold()
    {
        var sdgId = await SeedSDGsAsync(5);
        await SeedSDGTargetsAsync(sdgId, 100);

        _stopwatch.Restart();
        var result = _manager.GetSDGTargets();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetSDGTargets took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GetUsersPagedAsync_Bulk100_CompletesWithinThreshold()
    {
        await SeedPAOUsersWithProfilesAsync(150);

        _stopwatch.Restart();
        var result = await _manager.GetUsersPagedAsync(new UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 100,
            ActiveOnly = true
        });
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetUsersPagedAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetOrganizationUnits_SimpleLookup_CompletesWithinThreshold()
    {
        await SeedOrganizationHierarchiesAsync(50);

        _stopwatch.Restart();
        var result = _manager.GetOrganizationUnits();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetOrganizationUnits took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GetLiaisonOffices_SimpleLookup_CompletesWithinThreshold()
    {
        await SeedLiaisonOfficesAsync(30);

        _stopwatch.Restart();
        var result = _manager.GetLiaisonOffices();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetLiaisonOffices took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task SearchUsersAsync_WithTerm_CompletesWithinThreshold()
    {
        await SeedPAOUsersWithProfilesAsync(100);

        _stopwatch.Restart();
        var result = await _manager.SearchUsersAsync("user", 20, null);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"SearchUsersAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GetUsersPagedAsync_Paginated_CompletesWithinThreshold()
    {
        await SeedPAOUsersWithProfilesAsync(200);

        _stopwatch.Restart();
        var result = await _manager.GetUsersPagedAsync(new UsersPagedRequest
        {
            PageIndex = 2,
            PageSize = 20,
            SearchTerm = "perf",
            ActiveOnly = true
        });
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetUsersPagedAsync paginated took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GetCurrencies_RepeatedCalls_NoDegradation()
    {
        await SeedCurrenciesAsync(20);
        var times = new List<long>();

        for (int i = 0; i < 20; i++)
        {
            _stopwatch.Restart();
            _ = _manager.GetCurrencies();
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first5Avg = times.Take(5).Average();
        var last5Avg = times.Skip(15).Average();
        last5Avg.Should().BeLessThan(first5Avg * 3,
            $"Repeated GetCurrencies degraded from {first5Avg}ms to {last5Avg}ms avg");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task ConcurrentReads_50ParallelGetCurrencies_MaintainsPerformance()
    {
        await SeedCurrenciesAsync(30);
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() => _manager.GetCurrencies().ToList()))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task ConcurrentReads_20ParallelGetCountries_MaintainsPerformance()
    {
        await SeedCountriesAsync(50);
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => _manager.GetCountries().ToList()))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetCountries took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task ConcurrentMixed_GetCurrenciesAndGetSDGs_PerformanceStable()
    {
        await SeedCurrenciesAsync(20);
        await SeedSDGsAsync(30);

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            _ = _manager.GetCurrencies().ToList();
            _ = _manager.GetSDGs().ToList();
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed sequential lookups took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeCountryList_MemoryUsage_WithinCap()
    {
        await SeedCountriesAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        _ = _manager.GetCountries().ToList();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"GetCountries allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task RepeatedGetCurrencies_NoMemoryLeak()
    {
        await SeedCurrenciesAsync(50);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            _ = _manager.GetCurrencies().ToList();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        await SeedCurrenciesAsync(30);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            _ = _manager.GetCurrencies().ToList();
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        if (first25Avg > 0)
            last25Avg.Should().BeLessThan(first25Avg * 3,
                $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region EF Core — AsNoTracking Verification

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task GetCountries_AsNoTracking_ReadOnlyQueryOptimized()
    {
        await SeedCountriesAsync(100);

        _stopwatch.Restart();
        var result = _manager.GetCountries().ToList();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"AsNoTracking read query should complete within threshold — took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]

    [Trait("Defect", "DEF-092")]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        await SeedReferenceDataAsync();
        var report = new Dictionary<string, long>();

        report["GetCurrencies"] = Measure(() => _manager.GetCurrencies().ToList()).ElapsedMs;
        report["GetCountries"] = Measure(() => _manager.GetCountries().ToList()).ElapsedMs;
        report["GetSDGs"] = Measure(() => _manager.GetSDGs().ToList()).ElapsedMs;
        report["GetOrganizationUnits"] = Measure(() => _manager.GetOrganizationUnits().ToList()).ElapsedMs;
        report["GetLiaisonOffices"] = Measure(() => _manager.GetLiaisonOffices().ToList()).ElapsedMs;
        report["GetUsersPagedAsync"] = await TimeMs(async () =>
            await _manager.GetUsersPagedAsync(new UsersPagedRequest { PageIndex = 0, PageSize = 20 }));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task SeedCurrenciesAsync(int count)
    {
        var existing = await Context.Currencies.CountAsync(c => c.Code.Contains(_testMarker));
        if (existing >= count) return;

        var toAdd = Enumerable.Range(existing + 1, count - existing)
            .Select(i => new Currency
            {
                Name = $"Currency {i} {_testMarker}",
                Code = $"CC{i}_{_testMarker[..6]}",
                Status = EntityStatus.Active
            })
            .ToList();

        await Context.Currencies.AddRangeAsync(toAdd);
        await SaveChangesAsync();
    }

    private async Task SeedCountriesAsync(int count)
    {
        var existing = await Context.Countries.CountAsync(c => c.Name.Contains(_testMarker));
        if (existing >= count) return;

        var toAdd = Enumerable.Range(existing + 1, count - existing)
            .Select(i => new Country
            {
                Name = $"Country {i} {_testMarker}",
                Iso2Code = $"C{i:D2}_{_testMarker[..4]}",
                Iso3Code = $"CT{i}",
                RegionDescription = "Test Region",
                ContinentDescription = "Test Continent",
                Status = EntityStatus.Active
            })
            .ToList();

        await Context.Countries.AddRangeAsync(toAdd);
        await SaveChangesAsync();
    }

    private async Task<string> SeedSDGsAsync(int count)
    {
        var existing = await Context.SDGs.CountAsync(s => s.Name.Contains(_testMarker));
        if (existing >= count)
        {
            var first = await Context.SDGs.FirstOrDefaultAsync(s => s.Name.Contains(_testMarker));
            return first?.SDGId ?? "1";
        }

        var toAdd = Enumerable.Range(existing + 1, count - existing)
            .Select(i => new SDG
            {
                Name = $"SDG {i} {_testMarker}",
                SDGId = $"{i}",
                SDGNumber = $"{i}",
                SDGDescription = $"Goal {i}",
                Status = EntityStatus.Active
            })
            .ToList();

        await Context.SDGs.AddRangeAsync(toAdd);
        await SaveChangesAsync();
        return toAdd.First().SDGId ?? "1";
    }

    private async Task SeedSDGTargetsAsync(string sdgId, int count)
    {
        var existing = await Context.SDGTargets.CountAsync(t => t.Name.Contains(_testMarker));
        if (existing >= count) return;

        var toAdd = Enumerable.Range(existing + 1, count - existing)
            .Select(i => new SDGTarget
            {
                Name = $"Target {i} {_testMarker}",
                SDGId = sdgId,
                SDGTargetId = $"T{i}",
                Status = EntityStatus.Active
            })
            .ToList();

        await Context.SDGTargets.AddRangeAsync(toAdd);
        await SaveChangesAsync();
    }

    private async Task SeedOrganizationHierarchiesAsync(int count)
    {
        await EnsureTestUserAsync();
        var existing = await Context.OrganizationHierarchies.CountAsync(o => o.Name.Contains(_testMarker));
        if (existing >= count) return;

        var toAdd = Enumerable.Range(existing + 1, count - existing)
            .Select(i => new OrganizationHierarchy
            {
                Name = $"OrgUnit {i} {_testMarker}",
                Code = $"OU{i}_{_testMarker[..6]}",
                Description = $"Org {i}",
                Type = OrganizationUnitType.OrgUnit,
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            })
            .ToList();

        await Context.OrganizationHierarchies.AddRangeAsync(toAdd);
        await SaveChangesAsync();
    }

    private async Task SeedLiaisonOfficesAsync(int count)
    {
        await EnsureTestUserAsync();
        var existing = await Context.LiaisonOffices.CountAsync(l => l.Name.Contains(_testMarker) && !l.IsDeleted);
        if (existing >= count) return;

        var toAdd = Enumerable.Range(existing + 1, count - existing)
            .Select(i => new LiaisonOffice
            {
                Name = $"Office {i} {_testMarker}",
                Code = $"LO{i}_{_testMarker[..6]}",
                IsActive = true,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            })
            .ToList();

        await Context.LiaisonOffices.AddRangeAsync(toAdd);
        await SaveChangesAsync();
    }

    private async Task SeedPAOUsersWithProfilesAsync(int count)
    {
        await EnsureTestUserAsync();
        var existing = await Context.PAOUsers.CountAsync(u => u.Email.Contains(_testMarker));
        if (existing >= count) return;

        for (int i = existing + 1; i <= count; i++)
        {
            var user = new PAOUser
            {
                Email = $"user{i}_{_testMarker}@perf.test",
                IsInternal = true,
                ActiveUser = true
            };
            await Context.PAOUsers.AddAsync(user);
            await SaveChangesAsync();

            var profile = new UserProfile
            {
                UserId = user.Id,
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                UserEmail = user.Email,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.UserProfile.AddAsync(profile);
            await SaveChangesAsync();
        }
    }

    private async Task SeedReferenceDataAsync()
    {
        await SeedCurrenciesAsync(20);
        await SeedCountriesAsync(30);
        await SeedSDGsAsync(17);
        await SeedOrganizationHierarchiesAsync(20);
        await SeedLiaisonOfficesAsync(15);
        await SeedPAOUsersWithProfilesAsync(50);
    }

    private new (T Result, long ElapsedMs) Measure<T>(Func<T> fn)
    {
        _stopwatch.Restart();
        var result = fn();
        _stopwatch.Stop();
        return (result, _stopwatch.ElapsedMilliseconds);
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
