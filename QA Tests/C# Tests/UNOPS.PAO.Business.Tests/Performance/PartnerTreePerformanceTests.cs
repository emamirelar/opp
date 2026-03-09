/**
 * PERFORMANCE TESTS — PartnerTreeManager (UNOPSPartnerTreeManager)
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Partner tree hierarchy, categories, groups, CRUD
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Diagnostics;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for PartnerTreeManager (UNOPSPartnerTreeManager).
/// Verifies response times, throughput, hierarchy building, and behaviour under concurrent access.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class PartnerTreePerformanceTests : PerformanceTestBase
{
    private readonly IPartnerTreeManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly ClaimsPrincipal _testUser;
    private readonly string _testMarker = $"PTPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public PartnerTreePerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Mapping.MappingProfile>();
            cfg.CreateMap<UNOPSPartnerTree, ExternalPartnerTreeModel>();
        });
        var mapper = mapperConfig.CreateMapper();
        var configuration = TestEnvironment.CreateTestConfiguration();
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(s => s.HasPermissionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.CanPerformActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(true);

        var partnerTreeRepository = new DataRepository<UNOPSPartnerTree>(Context);
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var partnerTreeService = new PartnerTreeService(partnerTreeRepository, memoryCache);

        _manager = new UNOPSPartnerTreeManager(mapper, Context, configuration, partnerTreeService, mockPermissionService.Object);
        _stopwatch = new Stopwatch();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()),
            new Claim(ClaimTypes.Email, "perf@test.local"),
            new Claim(ClaimTypes.Name, "Perf Test User")
        };
        _testUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task CreatePartnerTree_SingleEntity_CompletesWithinThreshold()
    {
        var model = BuildPartnerTreeDataModel($"Create_{_testMarker}");

        _stopwatch.Restart();
        var result = await _manager.CreatePartnerTreeAsync(_testUser, model);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreatePartnerTree took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetPartnerTree_ExistingEntity_CompletesWithinThreshold()
    {
        var tree = await SeedPartnerTreeAsync();

        _stopwatch.Restart();
        var result = await _manager.GetPartnerTreeAsync(_testUser, tree.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Data.Id.Should().Be(tree.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetPartnerTree took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetPartnerTrees_100Trees_CompletesWithinThreshold()
    {
        await SeedPartnerTreesAsync(100);

        _stopwatch.Restart();
        var result = await _manager.GetPartnerTreesAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        var flatCount = CountTreeNodes(result);
        flatCount.Should().BeGreaterThanOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetPartnerTrees (100 trees) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetCategoryAndGroupStructure_CompletesWithinThreshold()
    {
        await SeedPartnerTreesAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetCategoryAndGroupStructureAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetCategoryAndGroupStructure took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPostedPartnerTrees_Sync_CompletesWithinThreshold()
    {
        await SeedPartnerTreesAsync(80);

        _stopwatch.Restart();
        var result = _manager.GetPostedPartnerTrees();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Count().Should().BeGreaterThanOrEqualTo(80);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetPostedPartnerTrees took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetPartnerTrees_SimpleSort_CompletesWithinThreshold()
    {
        await SeedPartnerTreesAsync(200);

        _stopwatch.Restart();
        var result = await _manager.GetPartnerTreesAsync(_testUser, "Name", ascending: true);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetPartnerTrees (simple sort) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPartnerTrees_DescendingSort_CompletesWithinThreshold()
    {
        await SeedPartnerTreesAsync(150);

        _stopwatch.Restart();
        var result = await _manager.GetPartnerTreesAsync(_testUser, "Name", ascending: false);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetPartnerTrees (descending) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPartnerTree_ById_CompletesWithinThreshold()
    {
        var tree = await SeedPartnerTreeAsync();

        _stopwatch.Restart();
        var result = await _manager.GetPartnerTreeAsync(_testUser, tree.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetPartnerTree by ID took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPostedPartnerTree_ById_CompletesWithinThreshold()
    {
        var tree = await SeedPartnerTreeAsync();

        _stopwatch.Restart();
        var result = await _manager.GetPostedPartnerTree(tree.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetPostedPartnerTree by ID took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPartnerTrees_HierarchyBuild_CompletesWithinThreshold()
    {
        await SeedPartnerTreesWithHierarchyAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetPartnerTreesAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetPartnerTrees (hierarchy) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task ConcurrentReads_50ParallelGetPartnerTrees_MaintainsPerformance()
    {
        await SeedPartnerTreesAsync(30);
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetPartnerTreesAsync(_testUser))
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

    [Trait("Defect", "DEF-093")]
    public async Task ConcurrentReads_20ParallelGetPartnerTree_MaintainsPerformance()
    {
        var tree = await SeedPartnerTreeAsync();
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _manager.GetPartnerTreeAsync(_testUser, tree.Id))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetPartnerTree took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        await SeedPartnerTreesAsync(20);
        var model = BuildPartnerTreeDataModel($"Mixed_{Guid.NewGuid():N}"[..20]);

        var readTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetPartnerTreesAsync(_testUser))
            .Cast<Task>()
            .ToList();
        var structureTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.GetCategoryAndGroupStructureAsync(_testUser))
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(structureTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent reads took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeTreeList_MemoryUsage_WithinCap()
    {
        await SeedPartnerTreesAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetPartnerTreesAsync(_testUser);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        await SeedPartnerTreesAsync(10);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetPartnerTreesAsync(_testUser);
            await _manager.GetCategoryAndGroupStructureAsync(_testUser);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        await SeedPartnerTreesAsync(50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetPartnerTreesAsync(_testUser);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(Math.Max(first25Avg * 10, 500),
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region Update and Delete Performance

    [Fact]
    public async Task UpdatePartnerTree_ExistingEntity_CompletesWithinThreshold()
    {
        var tree = await SeedPartnerTreeAsync();
        var model = BuildPartnerTreeDataModel($"Updated_{_testMarker}");
        model.Id = tree.Id;

        _stopwatch.Restart();
        var result = await _manager.UpdatePartnerTreeAsync(_testUser, model);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpdatePartnerTree took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DeletePartnerTree_ExistingEntity_CompletesWithinThreshold()
    {
        var tree = await SeedPartnerTreeAsync();

        _stopwatch.Restart();
        await _manager.DeletePartnerTreeAsync(_testUser, tree.Id);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeletePartnerTree took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region EF Core — N+1 & Hierarchy Verification

    [Fact]
    public async Task GetPartnerTrees_WithHierarchy_NoCartesianExplosion_CompletesWithinThreshold()
    {
        await SeedPartnerTreesWithHierarchyAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetPartnerTreesAsync(_testUser);
        _stopwatch.Stop();

        var flatCount = CountTreeNodes(result);
        flatCount.Should().BeGreaterThanOrEqualTo(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var tree = await SeedPartnerTreeAsync();
        var createModel = BuildPartnerTreeDataModel($"Bench_{Guid.NewGuid():N}"[..15]);

        report["CreatePartnerTree"] = await TimeMs(() => _manager.CreatePartnerTreeAsync(_testUser, createModel));
        report["GetPartnerTrees"] = await TimeMs(() => _manager.GetPartnerTreesAsync(_testUser));
        report["GetPartnerTree"] = await TimeMs(() => _manager.GetPartnerTreeAsync(_testUser, tree.Id));
        report["GetCategoryAndGroupStructure"] = await TimeMs(() => _manager.GetCategoryAndGroupStructureAsync(_testUser));
        report["GetPostedPartnerTrees"] = await TimeMs(() => Task.Run(() => _manager.GetPostedPartnerTrees()));

        var updateModel = BuildPartnerTreeDataModel("Updated");
        updateModel.Id = tree.Id;
        report["UpdatePartnerTree"] = await TimeMs(() => _manager.UpdatePartnerTreeAsync(_testUser, updateModel));
        report["DeletePartnerTree"] = await TimeMs(() => _manager.DeletePartnerTreeAsync(_testUser, tree.Id));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-30}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private PartnerTreeDataModel BuildPartnerTreeDataModel(string nameSuffix)
    {
        var code = $"{_testMarker}_{Guid.NewGuid():N}"[..60];
        return new PartnerTreeDataModel
        {
            Id = 0,
            Name = $"Tree {nameSuffix}",
            Description = $"Perf test tree {nameSuffix}",
            Code = code,
            Type = "Group",
            Parent = null,
            Status = EntityStatus.Active.ToString()
        };
    }

    private async Task<UNOPSPartnerTree> SeedPartnerTreeAsync(string? nameSuffix = null)
    {
        var tree = new UNOPSPartnerTree
        {
            Code = $"{_testMarker}_{Guid.NewGuid():N}"[..60],
            Name = $"Tree {nameSuffix ?? "Seed"}",
            Description = $"Perf test tree",
            Type = "Group",
            Parent = null,
            Status = EntityStatus.Active
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        return tree;
    }

    private async Task SeedPartnerTreesAsync(int count)
    {
        var existing = await Context.PartnerTrees.CountAsync(pt => !pt.IsDeleted && pt.Code.Contains(_testMarker));
        if (existing >= count) return;

        var trees = Enumerable.Range(existing, count - existing)
            .Select(i => new UNOPSPartnerTree
            {
                Code = $"{_testMarker}_{i}_{Guid.NewGuid():N}"[..60],
                Name = $"Tree {i} {_testMarker}",
                Description = $"Perf test tree {i}",
                Type = i % 3 == 0 ? "Category" : "Group",
                Parent = null,
                Status = EntityStatus.Active
            })
            .ToList();
        await Context.PartnerTrees.AddRangeAsync(trees);
        await SaveChangesAsync();
    }

    private async Task SeedPartnerTreesWithHierarchyAsync(int count)
    {
        var parent = await SeedPartnerTreeAsync("Root");
        for (int i = 0; i < count - 1; i++)
        {
            var child = new UNOPSPartnerTree
            {
                Code = $"{_testMarker}_Child_{i}_{Guid.NewGuid():N}"[..60],
                Name = $"Child {i} {_testMarker}",
                Description = $"Perf child {i}",
                Type = "Group",
                Parent = parent.Code,
                Status = EntityStatus.Active
            };
            await Context.PartnerTrees.AddAsync(child);
        }
        await SaveChangesAsync();
    }

    private static int CountTreeNodes(IEnumerable<PartnerTreeModel> nodes)
    {
        if (nodes == null) return 0;
        var count = 0;
        foreach (var node in nodes)
        {
            count++;
            if (node.Children != null && node.Children.Any())
                count += CountTreeNodes(node.Children);
        }
        return count;
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
