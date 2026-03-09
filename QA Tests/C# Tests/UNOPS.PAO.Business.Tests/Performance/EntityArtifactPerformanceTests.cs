/**
 * PERFORMANCE TESTS — EntityArtifactManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Entity artifacts, artifact types, artifact CRUD, bulk operations
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
using UNOPS.PAO.Models.Artifacts;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for EntityArtifactManager (IEntityArtifactManager).
/// Verifies response times for GetAvailableEntityTypesAsync, GetArtifactTypesByEntityTypeAsync,
/// GetEntityArtifactAsync, UpsertEntityArtifactAsync, GetEntityArtifactsAsync, GetArtifactTypeCodeAsync.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
[Collection("EntityArtifactPerformance")]
public class EntityArtifactPerformanceTests : PerformanceTestBase
{
    private readonly IEntityArtifactManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"EART_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public EntityArtifactPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        _manager = new EntityArtifactManager(mapper, Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task GetEntityArtifactAsync_ExistingArtifact_CompletesWithinThreshold()
    {
        var (entityType, entityId, artifactTypeId) = await SeedEntityArtifactAsync();

        _stopwatch.Restart();
        var result = await _manager.GetEntityArtifactAsync(entityType, entityId, artifactTypeId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.EntityType.Should().Be(entityType);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetEntityArtifactAsync took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task UpsertEntityArtifactAsync_CreateNew_CompletesWithinThreshold()
    {
        var (artifactTypeId, entityId) = await SeedArtifactTypeAndPartnerAsync();

        var request = new EntityArtifactRequest
        {
            EntityType = "Partner",
            EntityId = entityId,
            ArtifactTypeId = artifactTypeId,
            ValueText = $"Perf value {_testMarker}",
            Source = "PerfTest"
        };

        _stopwatch.Restart();
        var result = await _manager.UpsertEntityArtifactAsync(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpsertEntityArtifactAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetEntityArtifactsAsync_100Artifacts_CompletesWithinThreshold()
    {
        var (entityType, entityId, _) = await SeedEntityArtifactsAsync(100);

        _stopwatch.Restart();
        var result = await _manager.GetEntityArtifactsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetEntityArtifactsAsync (100) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetArtifactTypesByEntityTypeAsync_MultipleTypes_CompletesWithinThreshold()
    {
        await SeedArtifactTypesForEntityAsync("Opportunity", 50);

        _stopwatch.Restart();
        var result = await _manager.GetArtifactTypesByEntityTypeAsync("Opportunity");
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetArtifactTypesByEntityTypeAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAvailableEntityTypesAsync_CompletesWithinThreshold()
    {
        await SeedArtifactTypesForEntityAsync("Country", 10);
        await SeedArtifactTypesForEntityAsync("Partner", 10);

        _stopwatch.Restart();
        var result = await _manager.GetAvailableEntityTypesAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetAvailableEntityTypesAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetEntityArtifactsAsync_SimpleFilter_CompletesWithinThreshold()
    {
        var (entityType, entityId, _) = await SeedEntityArtifactsAsync(200);

        _stopwatch.Restart();
        var result = await _manager.GetEntityArtifactsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetEntityArtifactsAsync simple filter took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityArtifactAsync_ByEntityTypeAndId_CompletesWithinThreshold()
    {
        var (entityType, entityId, artifactTypeId) = await SeedEntityArtifactAsync();

        _stopwatch.Restart();
        var result = await _manager.GetEntityArtifactAsync(entityType, entityId, artifactTypeId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetEntityArtifactAsync by keys took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetArtifactTypeCodeAsync_Lookup_CompletesWithinThreshold()
    {
        var artifactTypeId = await SeedArtifactTypeAsync("CODE_LOOKUP");

        _stopwatch.Restart();
        var result = await _manager.GetArtifactTypeCodeAsync(artifactTypeId);
        _stopwatch.Stop();

        result.Should().NotBeNullOrEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetArtifactTypeCodeAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityArtifactsAsync_MultipleEntities_CompletesWithinThreshold()
    {
        var (type1, id1, _) = await SeedEntityArtifactsAsync(30, "Partner");
        var (_, id2, _) = await SeedEntityArtifactsForSecondPartnerAsync(30);

        _stopwatch.Restart();
        var result1 = await _manager.GetEntityArtifactsAsync(type1, id1);
        var result2 = await _manager.GetEntityArtifactsAsync("Partner", id2);
        _stopwatch.Stop();

        result1.Should().HaveCount(30);
        result2.Should().HaveCount(30);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Multi-entity GetEntityArtifactsAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityArtifactsAsync_ExcludesSoftDeleted_CompletesWithinThreshold()
    {
        var (entityType, entityId, artifactIds) = await SeedEntityArtifactsWithSomeDeletedAsync(100, 20);

        _stopwatch.Restart();
        var result = await _manager.GetEntityArtifactsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(80);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetEntityArtifactsAsync excluding deleted took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-069")]
    public async Task ConcurrentReads_50ParallelGetEntityArtifact_MaintainsPerformance()
    {
        var (entityType, entityId, artifactTypeId) = await SeedEntityArtifactAsync();
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetEntityArtifactAsync(entityType, entityId, artifactTypeId))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r != null);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-069")]
    public async Task ConcurrentReads_20ParallelGetEntityArtifacts_MaintainsPerformance()
    {
        var (entityType, entityId, _) = await SeedEntityArtifactsAsync(50);
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _manager.GetEntityArtifactsAsync(entityType, entityId))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r.Count() == 50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetEntityArtifactsAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-069")]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var (entityType, entityId, artifactTypeId) = await SeedEntityArtifactAsync();
        var (newArtifactTypeId, _) = await SeedArtifactTypeAndPartnerAsync();

        var readTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetEntityArtifactAsync(entityType, entityId, artifactTypeId))
            .Cast<Task>()
            .ToList();
        var writeTasks = Enumerable.Range(0, 3)
            .Select(i => _manager.UpsertEntityArtifactAsync(new EntityArtifactRequest
            {
                EntityType = "Partner",
                EntityId = entityId,
                ArtifactTypeId = newArtifactTypeId,
                ValueText = $"Mixed_{i}_{_testMarker}",
                Source = "PerfTest"
            }))
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(writeTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent read/write took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeArtifactList_MemoryUsage_WithinCap()
    {
        var (entityType, entityId, _) = await SeedEntityArtifactsAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetEntityArtifactsAsync(entityType, entityId);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"GetEntityArtifactsAsync allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedGetEntityArtifacts_NoMemoryLeak()
    {
        var (entityType, entityId, _) = await SeedEntityArtifactsAsync(20);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetEntityArtifactsAsync(entityType, entityId);
            await _manager.GetAvailableEntityTypesAsync();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var (entityType, entityId, _) = await SeedEntityArtifactsAsync(50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetEntityArtifactsAsync(entityType, entityId);
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

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]
    public async Task GetEntityArtifacts_WithArtifactType_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var (entityType, entityId, _) = await SeedEntityArtifactsAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetEntityArtifactsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        result.Should().OnlyContain(r => r.ArtifactTypeName != null || r.ArtifactTypeId > 0);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var (entityType, entityId, artifactTypeId) = await SeedEntityArtifactAsync();
        var report = new Dictionary<string, long>();

        report["GetAvailableEntityTypesAsync"] = await TimeMs(() => _manager.GetAvailableEntityTypesAsync());
        report["GetArtifactTypesByEntityTypeAsync"] = await TimeMs(() => _manager.GetArtifactTypesByEntityTypeAsync(entityType));
        report["GetEntityArtifactAsync"] = await TimeMs(() => _manager.GetEntityArtifactAsync(entityType, entityId, artifactTypeId));
        report["GetEntityArtifactsAsync"] = await TimeMs(() => _manager.GetEntityArtifactsAsync(entityType, entityId));
        report["GetArtifactTypeCodeAsync"] = await TimeMs(() => _manager.GetArtifactTypeCodeAsync(artifactTypeId));
        report["UpsertEntityArtifactAsync"] = await TimeMs(() => _manager.UpsertEntityArtifactAsync(new EntityArtifactRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            ArtifactTypeId = artifactTypeId,
            ValueText = $"Bench_{_testMarker}",
            Source = "PerfTest"
        }));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-35}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<int> SeedArtifactDataTypeAsync(string name = "text")
    {
        var existing = await Context.ArtifactDataTypes
            .FirstOrDefaultAsync(adt => adt.Name == name && !adt.IsDeleted);
        if (existing != null) return existing.Id;

        var dataType = new ArtifactDataType
        {
            Name = name,
            Description = $"Test: {name}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.ArtifactDataTypes.AddAsync(dataType);
        await SaveChangesAsync();
        return dataType.Id;
    }

    private async Task<int> SeedArtifactTypeAsync(string codeSuffix, string dataTypeName = "text")
    {
        var code = $"{codeSuffix}_{_testMarker[..8]}";
        var existing = await Context.ArtifactTypes
            .FirstOrDefaultAsync(at => at.ArtifactTypeCode == code && !at.IsDeleted);
        if (existing != null) return existing.Id;

        var dataTypeId = await SeedArtifactDataTypeAsync(dataTypeName);
        var artifactType = new ArtifactType
        {
            Name = $"AT {codeSuffix} {_testMarker}",
            ArtifactTypeCode = code,
            ArtifactDataTypeId = dataTypeId,
            ApplicableEntityTypes = "Partner,Opportunity,Country",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.ArtifactTypes.AddAsync(artifactType);
        await SaveChangesAsync();
        return artifactType.Id;
    }

    private async Task SeedArtifactTypesForEntityAsync(string entityType, int count)
    {
        var existing = await Context.ArtifactTypes.CountAsync(at =>
            at.ApplicableEntityTypes != null && at.ApplicableEntityTypes.Contains(entityType) &&
            at.Name!.Contains(_testMarker));
        if (existing >= count) return;

        for (int i = existing + 1; i <= count; i++)
        {
            await SeedArtifactTypeAsync($"ET_{entityType}_{i}");
        }
    }

    private async Task<(int ArtifactTypeId, int EntityId)> SeedArtifactTypeAndPartnerAsync()
    {
        await EnsureTestUserAsync();
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var artifactTypeId = await SeedArtifactTypeAsync($"SEED_{_testMarker[..8]}");
        return (artifactTypeId, partnerId);
    }

    private async Task<(string EntityType, int EntityId, int ArtifactTypeId)> SeedEntityArtifactAsync(string entityType = "Partner")
    {
        var (artifactTypeId, entityId) = await SeedArtifactTypeAndPartnerAsync();
        var artifact = new EntityArtifact
        {
            EntityType = entityType,
            EntityId = entityId,
            ArtifactTypeId = artifactTypeId,
            Name = $"Artifact {_testMarker}",
            ValueText = "Perf value",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();
        return (entityType, entityId, artifactTypeId);
    }

    private async Task<(string EntityType, int EntityId, int ArtifactTypeId)> SeedEntityArtifactsAsync(
        int count,
        string entityType = "Partner")
    {
        var (artifactTypeId, entityId) = await SeedArtifactTypeAndPartnerAsync();
        var artifacts = Enumerable.Range(1, count)
            .Select(i => new EntityArtifact
            {
                EntityType = entityType,
                EntityId = entityId,
                ArtifactTypeId = artifactTypeId,
                Name = $"Artifact {i} {_testMarker}",
                ValueText = $"Value {i}",
                Status = EntityStatus.Active,
                IsDeleted = false
            })
            .ToList();
        await Context.EntityArtifacts.AddRangeAsync(artifacts);
        await SaveChangesAsync();
        return (entityType, entityId, artifactTypeId);
    }

    private async Task<(string EntityType, int EntityId, int ArtifactTypeId)> SeedEntityArtifactsForSecondPartnerAsync(int count)
    {
        await EnsureTestUserAsync();
        var partnerId = await CreateTestPartnerAsync($"Partner2_{_testMarker}");
        var artifactTypeId = await SeedArtifactTypeAsync($"SECOND_{_testMarker[..8]}");
        var artifacts = Enumerable.Range(1, count)
            .Select(i => new EntityArtifact
            {
                EntityType = "Partner",
                EntityId = partnerId,
                ArtifactTypeId = artifactTypeId,
                Name = $"Artifact2 {i} {_testMarker}",
                ValueText = $"Value {i}",
                Status = EntityStatus.Active,
                IsDeleted = false
            })
            .ToList();
        await Context.EntityArtifacts.AddRangeAsync(artifacts);
        await SaveChangesAsync();
        return ("Partner", partnerId, artifactTypeId);
    }

    private async Task<(string EntityType, int EntityId, List<int> ArtifactIds)> SeedEntityArtifactsWithSomeDeletedAsync(
        int total, int deleteCount)
    {
        var (artifactTypeId, entityId) = await SeedArtifactTypeAndPartnerAsync();
        var entityType = "Partner";
        var artifacts = Enumerable.Range(1, total)
            .Select(i => new EntityArtifact
            {
                EntityType = "Partner",
                EntityId = entityId,
                ArtifactTypeId = artifactTypeId,
                Name = $"Artifact {i} {_testMarker}",
                ValueText = $"Value {i}",
                Status = EntityStatus.Active,
                IsDeleted = false
            })
            .ToList();
        await Context.EntityArtifacts.AddRangeAsync(artifacts);
        await SaveChangesAsync();

        var toDelete = artifacts.Take(deleteCount).ToList();
        foreach (var a in toDelete)
        {
            a.IsDeleted = true;
            a.DeletedDate = DateTime.UtcNow;
            a.DeletedBy = TestUserId;
        }
        await SaveChangesAsync();

        return (entityType, entityId, artifacts.Select(a => a.Id).ToList());
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
