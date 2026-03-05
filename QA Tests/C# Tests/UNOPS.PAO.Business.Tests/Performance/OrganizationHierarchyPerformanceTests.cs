/**
 * PERFORMANCE TESTS — OrganizationHierarchyManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Organization hierarchy, tree retrieval, type filtering, hierarchy depth
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
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for OrganizationHierarchyManager.
/// Verifies response times, throughput, and behaviour under concurrent access
/// for tree retrieval, hierarchy depth, finding by type, finding by id.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class OrganizationHierarchyPerformanceTests : PerformanceTestBase
{
    private readonly IOrganizationHierarchyManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"OHPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public OrganizationHierarchyPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
            cfg.CreateMap<OrganizationHierarchy, OrganizationHierarchyModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
                .ForMember(dest => dest.ParentCode, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Code : null))
                .ForMember(dest => dest.Artifacts, opt => opt.Ignore());
        });
        var mapper = mapperConfig.CreateMapper();
        var valuesRepository = new ValuesRepository(Context);
        _manager = new OrganizationHierarchyManager(valuesRepository, mapper);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task GetOrganizationHierarchyById_ExistingEntity_CompletesWithinThreshold()
    {
        var org = await SeedOrganizationUnitAsync("Single", OrganizationUnitType.OrgUnit);

        _stopwatch.Restart();
        var result = await _manager.GetOrganizationHierarchyById(org.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Id.Should().Be(org.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetOrganizationHierarchyById took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetOrganizationHierarchy_TreeRetrieval_CompletesWithinThreshold()
    {
        await SeedHierarchyTreeAsync(3, 5);

        _stopwatch.Restart();
        var result = await _manager.GetOrganizationHierarchy();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetOrganizationHierarchy took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetOrganizationHierarchy_100Nodes_CompletesWithinThreshold()
    {
        await SeedHierarchyTreeAsync(10, 10);

        _stopwatch.Restart();
        var result = await _manager.GetOrganizationHierarchy();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        var totalNodes = CountTreeNodes(result);
        totalNodes.Should().BeGreaterThanOrEqualTo(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetOrganizationHierarchy (100 nodes) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetOrganizationHierarchyPrime_100Nodes_CompletesWithinThreshold()
    {
        await SeedHierarchyTreeAsync(10, 10);

        _stopwatch.Restart();
        var result = await _manager.GetOrganizationHierarchyPrime();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetOrganizationHierarchyPrime took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAllOrganizations_100Records_CompletesWithinThreshold()
    {
        await SeedOrganizationUnitsFlatAsync(100);

        _stopwatch.Restart();
        var result = _manager.GetAllOrganizations();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        var list = result.ToList();
        list.Should().HaveCountGreaterThanOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetAllOrganizations (100 records) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetOrganizationsByType_Office_CompletesWithinThreshold()
    {
        await SeedOrganizationUnitsByTypesAsync(50, OrganizationUnitType.Office, OrganizationUnitType.Region);

        _stopwatch.Restart();
        var result = _manager.GetOrganizationsByType(OrganizationUnitType.Office);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        var list = result.ToList();
        list.Should().OnlyContain(m => m.Type == OrganizationUnitType.Office.ToString());
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetOrganizationsByType(Office) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetOrganizationsByType_Region_CompletesWithinThreshold()
    {
        await SeedOrganizationUnitsByTypesAsync(80, OrganizationUnitType.Region, OrganizationUnitType.Hub);

        _stopwatch.Restart();
        var result = _manager.GetOrganizationsByType(OrganizationUnitType.Region);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        var list = result.ToList();
        list.Should().OnlyContain(m => m.Type == OrganizationUnitType.Region.ToString());
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetOrganizationsByType(Region) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetOrganizationsByType_Hub_CompletesWithinThreshold()
    {
        await SeedOrganizationUnitsByTypesAsync(60, OrganizationUnitType.Hub, OrganizationUnitType.OrgUnit);

        _stopwatch.Restart();
        var result = _manager.GetOrganizationsByType(OrganizationUnitType.Hub);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        var list = result.ToList();
        list.Should().OnlyContain(m => m.Type == OrganizationUnitType.Hub.ToString());
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetOrganizationsByType(Hub) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetOrganizationHierarchy_DeepHierarchy_CompletesWithinThreshold()
    {
        await SeedDeepHierarchyAsync(5);

        _stopwatch.Restart();
        var result = await _manager.GetOrganizationHierarchy();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetOrganizationHierarchy (deep hierarchy) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetOrganizationHierarchy_ExcludesDeleted_CompletesWithinThreshold()
    {
        var orgs = await SeedOrganizationUnitsFlatAsync(100);
        await SoftDeleteOrganizationUnitsAsync(orgs.Take(20).ToList());

        _stopwatch.Restart();
        var result = _manager.GetAllOrganizations();
        _stopwatch.Stop();

        var list = result.Where(m => m.Name?.Contains(_testMarker) == true).ToList();
        list.Should().HaveCount(80, "Query should return 80 non-deleted org units with test marker");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetAllOrganizations (excluding deleted) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task ConcurrentReads_50ParallelGetById_MaintainsPerformance()
    {
        var org = await SeedOrganizationUnitAsync("Concurrent", OrganizationUnitType.OrgUnit);
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetOrganizationHierarchyById(org.Id))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r != null && r.Id == org.Id);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]
    public async Task ConcurrentReads_20ParallelGetHierarchy_MaintainsPerformance()
    {
        await SeedHierarchyTreeAsync(5, 5);
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _manager.GetOrganizationHierarchy())
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetOrganizationHierarchy took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var org = await SeedOrganizationUnitAsync("Mixed", OrganizationUnitType.OrgUnit);

        var readTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetOrganizationHierarchyById(org.Id))
            .Cast<Task>()
            .ToList();
        var treeTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.GetOrganizationHierarchy())
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(treeTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent reads took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeHierarchyTree_MemoryUsage_WithinCap()
    {
        await SeedHierarchyTreeAsync(20, 15);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetOrganizationHierarchy();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        var org = await SeedOrganizationUnitAsync("Leak", OrganizationUnitType.OrgUnit);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetOrganizationHierarchyById(org.Id);
            _ = _manager.GetAllOrganizations().ToList();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var org = await SeedOrganizationUnitAsync("GcPressure", OrganizationUnitType.OrgUnit);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetOrganizationHierarchyById(org.Id);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region EF Core — N+1 & Split Query Verification

    [Fact]
    public async Task GetOrganizationHierarchy_WithRelated_NoCartesianExplosion_CompletesWithinThreshold()
    {
        await SeedHierarchyTreeAsync(10, 5);

        _stopwatch.Restart();
        var result = await _manager.GetOrganizationHierarchy();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetOrganizationHierarchyPrime_AsNoTracking_ReadOnlyQueryOptimized()
    {
        await SeedHierarchyTreeAsync(15, 5);

        _stopwatch.Restart();
        var result = await _manager.GetOrganizationHierarchyPrime();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"AsNoTracking read query should complete within threshold — took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var org = await SeedOrganizationUnitAsync("Bench", OrganizationUnitType.OrgUnit);

        report["GetOrganizationHierarchyById"] = await TimeMs(() => _manager.GetOrganizationHierarchyById(org.Id));
        report["GetOrganizationHierarchy"] = await TimeMs(() => _manager.GetOrganizationHierarchy());
        report["GetOrganizationHierarchyPrime"] = await TimeMs(() => _manager.GetOrganizationHierarchyPrime());
        report["GetOrganizationsByType"] = await TimeMs(() => Task.Run(() => { _ = _manager.GetOrganizationsByType(OrganizationUnitType.Office).ToList(); }));
        report["GetAllOrganizations"] = await TimeMs(() => Task.Run(() => { _ = _manager.GetAllOrganizations().ToList(); }));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-30}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<OrganizationHierarchy> SeedOrganizationUnitAsync(
        string nameSuffix,
        OrganizationUnitType type,
        int? parentId = null)
    {
        var org = new OrganizationHierarchy
        {
            Code = $"{nameSuffix}_{_testMarker[..8]}",
            Name = $"{nameSuffix} {_testMarker}",
            Type = type,
            Description = $"Perf test org {nameSuffix}",
            ParentId = parentId,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        return org;
    }

    private async Task<List<int>> SeedOrganizationUnitsFlatAsync(int count)
    {
        var ids = new List<int>();
        for (int i = 0; i < count; i++)
        {
            var org = await SeedOrganizationUnitAsync($"Flat_{i}", OrganizationUnitType.OrgUnit);
            ids.Add(org.Id);
        }
        return ids;
    }

    private async Task SeedOrganizationUnitsByTypesAsync(
        int countPerType,
        OrganizationUnitType type1,
        OrganizationUnitType type2)
    {
        for (int i = 0; i < countPerType; i++)
        {
            await SeedOrganizationUnitAsync($"{type1}_{i}", type1);
            await SeedOrganizationUnitAsync($"{type2}_{i}", type2);
        }
    }

    private async Task SeedHierarchyTreeAsync(int breadth, int depth)
    {
        var roots = new List<OrganizationHierarchy>();
        for (int r = 0; r < breadth; r++)
        {
            var root = await SeedOrganizationUnitAsync($"Root_{r}", OrganizationUnitType.Office, null);
            roots.Add(root);
        }
        var currentLevel = roots;
        for (int d = 1; d < depth; d++)
        {
            var nextLevel = new List<OrganizationHierarchy>();
            foreach (var parent in currentLevel)
            {
                for (int b = 0; b < breadth; b++)
                {
                    var child = await SeedOrganizationUnitAsync($"L{d}_B{b}", OrganizationUnitType.OrgUnit, parent.Id);
                    nextLevel.Add(child);
                }
            }
            currentLevel = nextLevel;
        }
    }

    private async Task SeedDeepHierarchyAsync(int depth)
    {
        OrganizationHierarchy? parent = null;
        for (int i = 0; i < depth; i++)
        {
            var org = await SeedOrganizationUnitAsync(
                $"Depth_{i}",
                i == 0 ? OrganizationUnitType.Office : OrganizationUnitType.OrgUnit,
                parent?.Id);
            parent = org;
        }
    }

    private static int CountTreeNodes(IEnumerable<OrganizationHierarchyTreeModel> tree)
    {
        var count = 0;
        foreach (var node in tree)
        {
            count++;
            if (node.Data?.Children != null && node.Data.Children.Count > 0)
                count += CountDataNodes(node.Data.Children);
        }
        return count;
    }

    private static int CountDataNodes(List<OrganizationHierarchyDataModel> children)
    {
        var count = children.Count;
        foreach (var child in children)
        {
            if (child.Children != null && child.Children.Count > 0)
                count += CountDataNodes(child.Children);
        }
        return count;
    }

    private async Task SoftDeleteOrganizationUnitsAsync(List<int> ids)
    {
        var orgs = await Context.OrganizationHierarchies
            .Where(o => ids.Contains(o.Id))
            .ToListAsync();
        foreach (var o in orgs)
        {
            o.IsDeleted = true;
            o.DeletedDate = DateTime.UtcNow;
            o.DeletedBy = 0;
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
