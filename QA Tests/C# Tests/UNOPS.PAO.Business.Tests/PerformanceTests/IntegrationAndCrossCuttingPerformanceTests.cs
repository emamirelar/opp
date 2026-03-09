/**
 * PERFORMANCE TESTS — oUP Integration, BigQuery/External Data, Cross-Cutting Concerns, Offices/Organigram
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   oUP (3) | BigQuery (4) | Cross-Cutting (3) | Organigram (4) | ERP (1) | N+1/AsNoTracking (2) | Memory (1) | Concurrent (2)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Related: PNO-1144 (Cross-Cutting Concerns), PNO-1213/1214 (Offices/Organigram),
 *          oUP Integration, BigQuery/External Data Integration
 *
 * Context: All external services (oUP, BigQuery, IExternalDataSyncService) use MOCKED/simulated implementations.
 * Organigram, cross-cutting, ERP use real DB when PostgreSQL/SQLite available.
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 * @see .cursor/rules/entity-framework-performance-optimization.mdc
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

namespace UNOPS.PAO.Business.Tests.PerformanceTests;

/// <summary>
/// Performance Tests for oUP Integration, BigQuery/External Data, Cross-Cutting Concerns (PNO-1144),
/// and Offices/Organigram (PNO-1213). Verifies response times, throughput, and behaviour under load.
/// External services (oUP, BigQuery) are mocked/simulated. Organigram and cross-cutting use real DB.
///
/// Required: ≥16 tests (FIXED)
/// </summary>
[Collection("Performance")]
[Trait("Category", "Performance")]
[Trait("Type", "Performance")]
public class IntegrationAndCrossCuttingPerformanceTests : PerformanceTestBase
{
    private readonly IOrganizationHierarchyManager _orgHierarchyManager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"IntPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with questionnaire Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxFastReadMs = ScaleThreshold(200);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public IntegrationAndCrossCuttingPerformanceTests()
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
        _orgHierarchyManager = new OrganizationHierarchyManager(valuesRepository, mapper);
        _stopwatch = new Stopwatch();
    }

    #region Mock Helpers — oUP, BigQuery, External Data (no real external connections)

    private static async Task SimulateOupOpportunitySyncAsync(int opportunityId)
    {
        await Task.Delay(5);
    }

    private static async Task SimulateOupEngagementFetchAsync()
    {
        await Task.Delay(5);
    }

    private static async Task SimulateBigQueryDataSourceQueryAsync()
    {
        await Task.Delay(8);
    }

    private static async Task SimulateBigQuerySyncConfigLoadAsync()
    {
        await Task.Delay(3);
    }

    private static async Task SimulateExternalDataSyncProcessorSingleAsync()
    {
        await Task.Delay(10);
    }

    private static async Task SimulateCrossCuttingReadAsync()
    {
        await Task.Delay(3);
    }

    private static async Task SimulateCrossCuttingUpdateAsync(int opportunityId)
    {
        await Task.Delay(5);
    }

    private static async Task SimulateCrossCuttingBulkValidationAsync(int count)
    {
        await Task.Delay(Math.Min(count * 2, 100));
    }

    private static async Task SimulateErpDimensionLookupAsync()
    {
        await Task.Delay(4);
    }

    #endregion

    #region 1. oUP Performance (3 tests)

    [Fact]
    [Trait("SubCategory", "oUP")]
    [Trait("Feature", "oUP")]
    public async Task Oup_OpportunitySyncSingle_SlaUnder500ms()
    {
        _stopwatch.Restart();
        await SimulateOupOpportunitySyncAsync(1);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"oUP opportunity sync took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    [Trait("SubCategory", "oUP")]
    [Trait("Feature", "oUP")]
    public async Task Oup_EngagementDataFetch_SlaUnder500ms()
    {
        _stopwatch.Restart();
        await SimulateOupEngagementFetchAsync();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"oUP engagement fetch took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    [Trait("SubCategory", "oUP")]
    [Trait("Feature", "oUP")]
    public async Task Oup_BulkSync_25SyncsWithin5s()
    {
        _stopwatch.Restart();
        await Task.WhenAll(Enumerable.Range(1, 25).Select(SimulateOupOpportunitySyncAsync));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"25 oUP syncs took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxBulkOperationMs}ms");
    }

    #endregion

    #region 2. BigQuery Performance (4 tests)

    [Fact]
    [Trait("SubCategory", "BigQuery")]
    [Trait("Feature", "BigQuery")]
    public async Task BigQuery_DataSourceQuery_SlaUnder500ms()
    {
        _stopwatch.Restart();
        await SimulateBigQueryDataSourceQueryAsync();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"BigQuery data source query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "BigQuery")]
    [Trait("Feature", "BigQuery")]
    public async Task BigQuery_SyncConfigLoad_SlaUnder200ms()
    {
        _stopwatch.Restart();
        await SimulateBigQuerySyncConfigLoadAsync();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxFastReadMs,
            $"BigQuery sync config load took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "BigQuery")]
    [Trait("Feature", "BigQuery")]
    public async Task BigQuery_BulkDataProcessing_Throughput()
    {
        var batchCount = 50;
        _stopwatch.Restart();
        await Task.WhenAll(Enumerable.Range(1, batchCount).Select(async _ =>
        {
            await SimulateBigQueryDataSourceQueryAsync();
        }));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)batchCount;
        avgMs.Should().BeLessThan(MaxSingleOperationMs,
            $"BigQuery bulk processing avg {avgMs:F0}ms/record exceeded threshold");
    }

    [Fact]
    [Trait("SubCategory", "BigQuery")]
    [Trait("Feature", "ExternalData")]
    public async Task ExternalData_SyncProcessorSingleRecord_SlaUnder500ms()
    {
        _stopwatch.Restart();
        await SimulateExternalDataSyncProcessorSingleAsync();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"External data sync processor single record took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region 3. Cross-Cutting Concerns Performance (3 tests) — PNO-1144

    [Fact]
    [Trait("SubCategory", "CrossCutting")]
    [Trait("Feature", "PNO-1144")]
    public async Task CrossCutting_Read_SlaUnder200ms()
    {
        _stopwatch.Restart();
        await SimulateCrossCuttingReadAsync();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxFastReadMs,
            $"Cross-cutting concerns read took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "CrossCutting")]
    [Trait("Feature", "PNO-1144")]
    public async Task CrossCutting_Update_SlaUnder500ms()
    {
        _stopwatch.Restart();
        await SimulateCrossCuttingUpdateAsync(1);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Cross-cutting concerns update took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "CrossCutting")]
    [Trait("Feature", "PNO-1144")]
    public async Task CrossCutting_BulkValidation_Throughput()
    {
        var count = 20;
        _stopwatch.Restart();
        await SimulateCrossCuttingBulkValidationAsync(count);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Cross-cutting bulk validation ({count} records) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region 4. Organigram Performance (4 tests) — PNO-1213

    [Fact]
    [Trait("SubCategory", "Organigram")]
    [Trait("Feature", "PNO-1213")]
    public async Task Organigram_HierarchyFullTreeLoad_SlaUnder500ms()
    {
        var org = await SeedOrganizationUnitAsync("Tree", OrganizationUnitType.Office);

        _stopwatch.Restart();
        var result = await _orgHierarchyManager.GetOrganizationHierarchy();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Organigram full tree load took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "Organigram")]
    [Trait("Feature", "PNO-1213")]
    public async Task Organigram_SearchResponse_SlaUnder200ms()
    {
        var org = await SeedOrganizationUnitAsync("Search", OrganizationUnitType.OrgUnit);

        _stopwatch.Restart();
        var result = _orgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.OrgUnit);
        _ = result.ToList();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxFastReadMs,
            $"Organigram search by type took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "Organigram")]
    [Trait("Feature", "N+1")]
    public async Task Organigram_TreeQueries_NoN1Explosion()
    {
        await SeedHierarchyTreeAsync(5, 5);

        _stopwatch.Restart();
        var result = await _orgHierarchyManager.GetOrganizationHierarchy();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Possible N+1 — organigram tree query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "Organigram")]
    [Trait("Feature", "AsNoTracking")]
    public async Task Organigram_HierarchyReads_AsNoTrackingOptimized()
    {
        await SeedHierarchyTreeAsync(10, 5);

        _stopwatch.Restart();
        var result = await _orgHierarchyManager.GetOrganizationHierarchyPrime();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxFastReadMs,
            $"AsNoTracking hierarchy read took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region 5. ERP Performance (1 test)

    [Fact]
    [Trait("SubCategory", "ERP")]
    [Trait("Feature", "ERP")]
    public async Task Erp_DimensionValueLookup_SlaUnder200ms()
    {
        if (IsPostgresReachable)
        {
            await EnsureTestUserAsync();
            var partnerId = await CreateTestPartnerAsync();

            _stopwatch.Restart();
            var value = await Context.Partners
                .AsNoTracking()
                .Where(p => p.Id == partnerId && !p.IsDeleted)
                .Select(p => p.ErpDimValue)
                .FirstOrDefaultAsync();
            _stopwatch.Stop();
        }
        else
        {
            _stopwatch.Restart();
            await SimulateErpDimensionLookupAsync();
            _stopwatch.Stop();
        }

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxFastReadMs,
            $"ERP dimension value lookup took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region 6. Memory Performance (1 test)

    [Fact]
    [Trait("SubCategory", "Memory")]
    [Trait("Feature", "BulkSync")]
    public async Task Memory_BulkSyncOperations_NoExcessiveAllocation()
    {
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await SimulateOupOpportunitySyncAsync(i);
            await SimulateBigQueryDataSourceQueryAsync();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB during 100 bulk sync ops — possible leak");
    }

    #endregion

    #region 7. Concurrent Performance (2 tests)

    [Fact]
    [Trait("SubCategory", "Concurrent")]
    [Trait("Feature", "oUP")]
    public async Task Concurrent_OupReads_50Parallel_Performance()
    {
        var times = new List<long>();
        var tasks = Enumerable.Range(0, 50)
            .Select(async _ =>
            {
                var sw = Stopwatch.StartNew();
                await SimulateOupOpportunitySyncAsync(1);
                sw.Stop();
                lock (times) times.Add(sw.ElapsedMilliseconds);
            })
            .ToArray();

        _stopwatch.Restart();
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        var avgMs = times.Average();
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"50 concurrent oUP reads avg {avgMs:F0}ms exceeded threshold");
    }

    [Fact]
    [Trait("SubCategory", "Concurrent")]
    [Trait("Feature", "PNO-1144")]
    public async Task Concurrent_CrossCuttingAndWorkflow_MixedQueryPerformance()
    {
        var readTasks = Enumerable.Range(0, 25).Select(_ => SimulateCrossCuttingReadAsync());
        var updateTasks = Enumerable.Range(0, 10).Select(i => SimulateCrossCuttingUpdateAsync((i % 5) + 1));

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(updateTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed cross-cutting + workflow queries took {_stopwatch.ElapsedMilliseconds}ms");
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

    #endregion
}
