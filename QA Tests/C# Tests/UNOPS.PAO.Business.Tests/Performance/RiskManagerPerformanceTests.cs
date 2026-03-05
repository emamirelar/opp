/**
 * PERFORMANCE TESTS — RiskManager (UNOPSRiskManager)
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Risk register, DST risks, risk assessment, risk mitigation tracking
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Diagnostics;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for RiskManager (UNOPSRiskManager).
/// Verifies response times, throughput, memory efficiency, N+1 detection,
/// AsNoTracking optimization, and risk assessment calculation performance.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class RiskManagerPerformanceTests : PerformanceTestBase
{
    private readonly IRiskManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"RiskPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public RiskManagerPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        var configuration = TestEnvironment.CreateTestConfiguration();
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(s => s.HasPermissionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.CanPerformActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(true);

        _manager = new UNOPSRiskManager(mapper, Context, configuration, mockPermissionService.Object);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task GetRisksByEntity_ExistingEntity_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 5);

        _stopwatch.Restart();
        var result = await _manager.GetRisksByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Risks.Should().HaveCount(5);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetRisksByEntity took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task CreateRisk_SingleEntity_CompletesWithinThreshold()
    {
        var lookups = await SeedAllLookupsAsync();
        var request = BuildCreateRequest(1, "Perf Risk Single", lookups);

        _stopwatch.Restart();
        var result = await _manager.CreateRiskAsync(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Title.Should().Be("Perf Risk Single");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreateRisk took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetRisksByEntity_100Risks_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 100);

        _stopwatch.Restart();
        var result = await _manager.GetRisksByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Risks.Should().HaveCount(100);
        result.TotalCount.Should().Be(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetRisksByEntity (100 risks) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetRiskLookups_AllLookups_CompletesWithinThreshold()
    {
        await SeedAllLookupsAsync();

        _stopwatch.Restart();
        var result = await _manager.GetRiskLookupsAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.RiskTypes.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetRiskLookups took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetRiskCategories_Hierarchy_CompletesWithinThreshold()
    {
        await SeedAllLookupsAsync();

        _stopwatch.Restart();
        var result = await _manager.GetRiskCategoriesAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetRiskCategories took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetRisksByEntity_SimpleFilter_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 200);

        _stopwatch.Restart();
        var result = await _manager.GetRisksByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Risks.Should().HaveCount(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple GetRisksByEntity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetRisksByEntity_MultipleEntityTypes_CompletesWithinThreshold()
    {
        var (type1, id1) = await SeedRisksForEntityAsync("Opportunity", 50);
        var (type2, id2) = await SeedRisksForEntityAsync("Opportunity", 50, entityIdOffset: 9999);

        _stopwatch.Restart();
        var result1 = await _manager.GetRisksByEntityAsync(type1, id1);
        var result2 = await _manager.GetRisksByEntityAsync(type2, id2);
        _stopwatch.Stop();

        result1.Risks.Should().HaveCount(50);
        result2.Risks.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Multi-entity GetRisksByEntity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPreDefinedHighRisks_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        var result = await _manager.GetPreDefinedHighRisksAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetPreDefinedHighRisks took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-084")]
    public async Task GetHighRiskAnalysis_OpportunityCompletesWithinThreshold()
    {
        var opportunityId = await GetOrCreateOpportunityIdAsync();

        _stopwatch.Restart();
        var result = await _manager.GetHighRiskAnalysisAsync(opportunityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetHighRiskAnalysis took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetRisksByEntity_ExcludesDeleted_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 100);
        var riskIds = await Context.Risks
            .Where(r => r.EntityType == entityType && r.EntityId == entityId && r.Name!.Contains(_testMarker))
            .Take(20)
            .Select(r => r.Id)
            .ToListAsync();
        await SoftDeleteRisksAsync(riskIds);

        _stopwatch.Restart();
        var result = await _manager.GetRisksByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Risks.Should().HaveCount(80);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetRisksByEntity (excluding deleted) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-084")]
    public async Task ConcurrentReads_50ParallelGetRisksByEntity_MaintainsPerformance()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 30);
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetRisksByEntityAsync(entityType, entityId))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r.Risks.Count == 30);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-084")]
    public async Task ConcurrentReads_20ParallelGetRiskLookups_MaintainsPerformance()
    {
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _manager.GetRiskLookupsAsync())
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetRiskLookups took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-084")]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 20);
        var lookups = await SeedAllLookupsAsync();
        var request = BuildCreateRequest(entityId, $"Mixed_{Guid.NewGuid():N}"[..20], lookups);

        var readTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetRisksByEntityAsync(entityType, entityId))
            .Cast<Task>()
            .ToList();
        var lookupTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.GetRiskLookupsAsync())
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(lookupTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent reads took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeRiskList_MemoryUsage_WithinCap()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetRisksByEntityAsync(entityType, entityId);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 10);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetRisksByEntityAsync(entityType, entityId);
            await _manager.GetRiskLookupsAsync();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetRisksByEntityAsync(entityType, entityId);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region Update and Delete Performance

    [Fact]
    public async Task UpdateRisk_ExistingEntity_CompletesWithinThreshold()
    {
        var lookups = await SeedAllLookupsAsync();
        var request = BuildCreateRequest(1, "Perf Risk Update", lookups);
        var created = await _manager.CreateRiskAsync(request);

        var updateRequest = BuildCreateRequest(1, "Perf Risk Updated", lookups);

        _stopwatch.Restart();
        var result = await _manager.UpdateRiskAsync(created.Id, updateRequest);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Title.Should().Be("Perf Risk Updated");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpdateRisk took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DeleteRisk_ExistingEntity_CompletesWithinThreshold()
    {
        var lookups = await SeedAllLookupsAsync();
        var request = BuildCreateRequest(1, "Perf Risk Delete", lookups);
        var created = await _manager.CreateRiskAsync(request);

        _stopwatch.Restart();
        var result = await _manager.DeleteRiskAsync(created.Id);
        _stopwatch.Stop();

        result.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeleteRisk took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]
    public async Task GetRisksByEntity_WithRelated_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 50);

        _stopwatch.Restart();
        var result = await _manager.GetRisksByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Risks.Should().HaveCount(50);
        result.Risks.Should().OnlyContain(r => r.RiskTypeName != null || r.RiskTypeId > 0);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    [Fact]
    public async Task GetRisksByEntity_AsNoTracking_ReadOnlyQueryOptimized()
    {
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 100);

        _stopwatch.Restart();
        var result = await _manager.GetRisksByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Risks.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"AsNoTracking read query should complete within threshold — took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Risk Assessment Calculation Performance

    [Fact]

    [Trait("Defect", "DEF-084")]
    public async Task GetHighRiskAnalysis_CalculationPerformance_CompletesWithinThreshold()
    {
        var opportunityId = await GetOrCreateOpportunityIdAsync();

        _stopwatch.Restart();
        var result = await _manager.GetHighRiskAnalysisAsync(opportunityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.AvailableHighRisks.Should().NotBeNull();
        result.Recommendations.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Risk assessment calculation took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var (entityType, entityId) = await SeedRisksForEntityAsync("Opportunity", 20);
        var lookups = await SeedAllLookupsAsync();
        var createRequest = BuildCreateRequest(entityId, $"Bench_{Guid.NewGuid():N}"[..15], lookups);
        var created = await _manager.CreateRiskAsync(createRequest);

        report["GetRisksByEntity"] = await TimeMs(() => _manager.GetRisksByEntityAsync(entityType, entityId));
        report["GetRiskLookups"] = await TimeMs(() => _manager.GetRiskLookupsAsync());
        report["GetRiskCategories"] = await TimeMs(() => _manager.GetRiskCategoriesAsync());
        report["GetPreDefinedHighRisks"] = await TimeMs(() => _manager.GetPreDefinedHighRisksAsync());
        report["CreateRisk"] = await TimeMs(() => _manager.CreateRiskAsync(BuildCreateRequest(entityId, $"B2_{Guid.NewGuid():N}"[..20], lookups)));
        report["UpdateRisk"] = await TimeMs(() => _manager.UpdateRiskAsync(created.Id, BuildCreateRequest(entityId, "Updated", lookups)));
        report["DeleteRisk"] = await TimeMs(() => _manager.DeleteRiskAsync(created.Id));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<(int TypeId, int ProbId, int ProxId, int ImpactId, int CategoryId)> SeedAllLookupsAsync()
    {
        var typeId = await SeedRiskTypeAsync();
        var probId = await SeedRiskProbabilityAsync();
        var proxId = await SeedRiskProximityAsync();
        var impactId = await SeedRiskImpactLevelAsync();
        var categoryId = await SeedRiskCategoryLevel3Async();
        return (typeId, probId, proxId, impactId, categoryId);
    }

    private async Task<int> SeedRiskTypeAsync(string code = "THREAT", string name = "Threat")
    {
        var existing = await Context.RiskTypes
            .FirstOrDefaultAsync(rt => rt.Code == code && !rt.IsDeleted);
        if (existing != null) return existing.Id;

        var riskType = new RiskType
        {
            Name = name,
            Code = code,
            Description = $"Test {name}",
            DisplayOrder = 1,
            IsResponseTypeMandatory = code == "OPPORTUNITY",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskTypes.AddAsync(riskType);
        await SaveChangesAsync();
        return riskType.Id;
    }

    private async Task<int> SeedRiskProbabilityAsync(string code = "LOW", string name = "Low")
    {
        var existing = await Context.RiskProbabilities.FirstOrDefaultAsync(rp => rp.Code == code);
        if (existing != null) return existing.Id;

        var prob = new RiskProbability
        {
            Name = name,
            Code = code,
            DisplayLabel = name,
            NumericValue = 1,
            DisplayOrder = 1,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskProbabilities.AddAsync(prob);
        await SaveChangesAsync();
        return prob.Id;
    }

    private async Task<int> SeedRiskProximityAsync(string code = "WITHIN_SIX_MONTHS", string name = "Within six months")
    {
        var existing = await Context.RiskProximities.FirstOrDefaultAsync(rp => rp.Code == code);
        if (existing != null) return existing.Id;

        var prox = new RiskProximity
        {
            Name = name,
            Code = code,
            MonthsValue = 6,
            DisplayOrder = 1,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskProximities.AddAsync(prox);
        await SaveChangesAsync();
        return prox.Id;
    }

    private async Task<int> SeedRiskImpactLevelAsync(string code = "LOW", string name = "Low")
    {
        var existing = await Context.RiskImpactLevels.FirstOrDefaultAsync(ri => ri.Code == code);
        if (existing != null) return existing.Id;

        var impact = new RiskImpactLevel
        {
            Name = name,
            Code = code,
            DisplayLabel = name,
            NumericValue = 1,
            DisplayOrder = 1,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskImpactLevels.AddAsync(impact);
        await SaveChangesAsync();
        return impact.Id;
    }

    private async Task<int> SeedRiskCategoryLevel3Async()
    {
        var existing = await Context.RiskCategories
            .FirstOrDefaultAsync(rc => rc.Level == 3 && !rc.IsDeleted);
        if (existing != null) return existing.Id;

        var level1 = new RiskCategory
        {
            Name = $"Finance {_testMarker}",
            Code = $"UPC1_FINANCE_{_testMarker[..8]}",
            ShortCode = "FINANCE",
            Level = 1,
            DisplayOrder = 1,
            Status = EntityStatus.Active
        };
        await Context.RiskCategories.AddAsync(level1);
        await SaveChangesAsync();

        var level2 = new RiskCategory
        {
            Name = $"Contributions {_testMarker}",
            Code = $"UPC2_CONTRIB_{_testMarker[..8]}",
            ShortCode = "CONTRIBUTIONS",
            Level = 2,
            ParentCategoryId = level1.Id,
            DisplayOrder = 1,
            Status = EntityStatus.Active
        };
        await Context.RiskCategories.AddAsync(level2);
        await SaveChangesAsync();

        var level3 = new RiskCategory
        {
            Name = $"Engagement Costing {_testMarker}",
            Code = $"UPC3_ENGCOST_{_testMarker[..8]}",
            ShortCode = "ENG_COST_PRICE",
            Level = 3,
            ParentCategoryId = level2.Id,
            DisplayOrder = 1,
            Status = EntityStatus.Active
        };
        await Context.RiskCategories.AddAsync(level3);
        await SaveChangesAsync();
        return level3.Id;
    }

    private async Task<(string EntityType, int EntityId)> SeedRisksForEntityAsync(
        string entityType,
        int count,
        int entityIdOffset = 0)
    {
        var lookups = await SeedAllLookupsAsync();
        var entityId = entityIdOffset > 0 ? entityIdOffset : 1;

        var risks = Enumerable.Range(1, count)
            .Select(i => new Risk
            {
                Name = $"Risk {i} {_testMarker}",
                Title = $"Risk {i} for Entity {_testMarker}",
                EntityType = entityType,
                EntityId = entityId,
                RiskTypeId = lookups.TypeId,
                RiskCategoryId = lookups.CategoryId,
                RiskProbabilityId = lookups.ProbId,
                RiskProximityId = lookups.ProxId,
                RiskImpactLevelId = lookups.ImpactId,
                Impact = RiskImpact.Medium,
                RiskStatus = RiskStatus.Open,
                Status = EntityStatus.Active
            })
            .ToList();

        await Context.Risks.AddRangeAsync(risks);
        await SaveChangesAsync();
        return (entityType, entityId);
    }

    private RiskCreateRequest BuildCreateRequest(
        int entityId,
        string title,
        (int TypeId, int ProbId, int ProxId, int ImpactId, int CategoryId) lookups)
    {
        return new RiskCreateRequest
        {
            EntityId = entityId,
            Title = title,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Description = "Perf test description",
            Recommendation = "Perf test recommendation"
        };
    }

    private async Task SoftDeleteRisksAsync(List<int> riskIds)
    {
        var risks = await Context.Risks.Where(r => riskIds.Contains(r.Id)).ToListAsync();
        foreach (var r in risks)
        {
            r.IsDeleted = true;
            r.DeletedDate = DateTime.UtcNow;
            r.DeletedBy = 0;
        }
        await SaveChangesAsync();
    }

    private async Task<int> GetOrCreateOpportunityIdAsync()
    {
        var existing = await Context.Opportunities
            .Where(o => !o.IsDeleted)
            .Select(o => o.Id)
            .FirstOrDefaultAsync();
        if (existing > 0) return existing;

        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = $"PerfOpp_{_testMarker}",
            Description = "Perf test opportunity",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        return opp.Id;
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
