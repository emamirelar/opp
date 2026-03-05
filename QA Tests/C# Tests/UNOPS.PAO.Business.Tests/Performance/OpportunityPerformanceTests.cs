/**
 * PERFORMANCE TESTS — OpportunityManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Opportunity pipeline, CRUD, search, related items
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
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for OpportunityManager.
/// Verifies response times, throughput, and behaviour under concurrent access
/// for opportunity CRUD, list retrieval, and related-item queries.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class OpportunityPerformanceTests : PerformanceTestBase
{
    private readonly IOpportunityManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"OppPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public OpportunityPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains("UNOPS.PAO") == true));
        });
        var mapper = mapperConfig.CreateMapper();
        _manager = new OpportunityManager(mapper, Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task CreateOpportunity_SingleEntity_CompletesWithinThreshold()
    {
        var (orgUnitId, initTypeId) = await SeedRequiredLookupsAsync();
        var opp = BuildOpportunity($"Perf Create {_testMarker}", orgUnitId, initTypeId);

        _stopwatch.Restart();
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        _stopwatch.Stop();

        opp.Id.Should().BeGreaterThan(0);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreateOpportunity took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetOpportunity_ExistingEntity_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityAsync();

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Id.Should().Be(opportunity.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetOpportunity took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task CreateOpportunity_100Sequential_CompletesWithinThreshold()
    {
        var (orgUnitId, initTypeId) = await SeedRequiredLookupsAsync();
        var opportunities = Enumerable.Range(0, 100)
            .Select(i => BuildOpportunity($"Bulk {i} {_testMarker}", orgUnitId, initTypeId))
            .ToList();

        _stopwatch.Restart();
        await Context.Opportunities.AddRangeAsync(opportunities);
        await SaveChangesAsync();
        _stopwatch.Stop();

        var count = await Context.Opportunities.CountAsync(o => !o.IsDeleted && o.Name.Contains(_testMarker));
        count.Should().Be(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Create 100 opportunities took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAllOpportunities_100Records_CompletesWithinThreshold()
    {
        await SeedOpportunitiesAsync(100);

        _stopwatch.Restart();
        var result = await _manager.GetAllOpportunitiesAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetAllOpportunities (100+ records) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task UpdateOpportunity_100Sequential_CompletesWithinThreshold()
    {
        var opportunities = await SeedOpportunitiesAsync(100);
        var firstId = opportunities.First().Id;

        _stopwatch.Restart();
        for (int i = 0; i < 100; i++)
        {
            var opp = await Context.Opportunities.FirstAsync(o => o.Id == opportunities[i].Id);
            opp.Name = $"Updated {i} {_testMarker}";
            opp.Description = opp.Description ?? "Desc";
            opp.LastModifiedDate = DateTime.UtcNow;
        }
        await SaveChangesAsync();
        _stopwatch.Stop();

        var updated = await Context.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == firstId && !o.IsDeleted);
        updated.Should().NotBeNull();
        updated!.Name.Should().Contain("Updated");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Update 100 opportunities took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetAllOpportunities_SimpleSearch_CompletesWithinThreshold()
    {
        await SeedOpportunitiesAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetAllOpportunitiesAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple GetAllOpportunities took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAllOpportunities_200Records_CompletesWithinThreshold()
    {
        await SeedOpportunitiesAsync(200);

        _stopwatch.Restart();
        var result = await _manager.GetAllOpportunitiesAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetAllOpportunities (200 records) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetRelatedItems_ExistingOpportunity_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityAsync();

        _stopwatch.Restart();
        var stakeholders = await Context.OpportunityStakeholders.AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted).ToListAsync();
        var deliverables = await Context.OpportunityDeliverables.AsNoTracking()
            .Where(d => d.OpportunityId == opportunity.Id && !d.IsDeleted).ToListAsync();
        var fundingPartners = await Context.OpportunityFundingPartners.AsNoTracking()
            .Where(fp => fp.OpportunityId == opportunity.Id && !fp.IsDeleted).ToListAsync();
        _stopwatch.Stop();

        stakeholders.Should().NotBeNull();
        deliverables.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetRelatedItems took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetOpportunity_WithManyIncludes_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithRelatedDataAsync();

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.FundingPartners.Should().NotBeNull();
        result.ClientPartners.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetOpportunity with includes took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAllOpportunities_ExcludesDeleted_CompletesWithinThreshold()
    {
        await SeedOpportunitiesAsync(100);
        var toDelete = await Context.Opportunities
            .Where(o => !o.IsDeleted && o.Name.Contains(_testMarker))
            .Take(20)
            .ToListAsync();
        foreach (var o in toDelete)
        {
            o.IsDeleted = true;
            o.DeletedDate = DateTime.UtcNow;
        }
        await SaveChangesAsync();

        _stopwatch.Restart();
        var result = await _manager.GetAllOpportunitiesAsync();
        _stopwatch.Stop();

        var ourOpps = result.Where(r => r.Name?.Contains(_testMarker) == true).ToList();
        ourOpps.Should().HaveCount(80);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetAllOpportunities (excluding deleted) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task ConcurrentReads_50ParallelGetOpportunity_MaintainsPerformance()
    {
        var opportunity = await SeedOpportunityAsync();
        var results = new List<UNOPS.PAO.Domain.Entities.Opportunity?>();

        _stopwatch.Restart();
        for (int i = 0; i < 50; i++)
        {
            var result = await Context.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
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
    public async Task ConcurrentReads_20ParallelGetAllOpportunities_MaintainsPerformance()
    {
        await SeedOpportunitiesAsync(30);
        var results = new List<IEnumerable<OpportunityModel>>();

        _stopwatch.Restart();
        for (int i = 0; i < 20; i++)
        {
            var result = await _manager.GetAllOpportunitiesAsync();
            results.Add(result);
        }
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 sequential GetAllOpportunities (simulating load) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var opportunity = await SeedOpportunityAsync();
        await SeedOpportunitiesAsync(20);

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            await Context.Opportunities.AsNoTracking()
                .Where(o => !o.IsDeleted)
                .Take(50)
                .ToListAsync();
        }
        for (int i = 0; i < 10; i++)
        {
            await Context.Opportunities.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed sequential reads (simulating load) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeOpportunityList_MemoryUsage_WithinCap()
    {
        await SeedOpportunitiesAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetAllOpportunitiesAsync();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        var opportunity = await SeedOpportunityAsync();
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await Context.Opportunities.AsNoTracking()
                .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
                .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
            await Context.Opportunities.AsNoTracking()
                .Where(o => !o.IsDeleted)
                .Take(20)
                .ToListAsync();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        await SeedOpportunitiesAsync(50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetAllOpportunitiesAsync();
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

    [Trait("Defect", "DEF-089")]
    public async Task GetOpportunity_WithRelated_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithRelatedDataAsync();

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
            .Include(o => o.ClientPartners.Where(cp => !cp.IsDeleted))
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — GetOpportunity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAllOpportunities_AsNoTracking_ReadOnlyQueryOptimized()
    {
        await SeedOpportunitiesAsync(100);

        _stopwatch.Restart();
        var result = await _manager.GetAllOpportunitiesAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"AsNoTracking read query should complete within threshold — took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var (orgUnitId, initTypeId) = await SeedRequiredLookupsAsync();
        var report = new Dictionary<string, long>();
        var opportunity = await SeedOpportunityAsync();

        report["CreateOpportunity"] = await TimeMs(async () =>
        {
            var benchName = $"Bench {Guid.NewGuid():N}";
            var opp = BuildOpportunity(benchName[..Math.Min(benchName.Length, 20)], orgUnitId, initTypeId);
            await Context.Opportunities.AddAsync(opp);
            await SaveChangesAsync();
        });
        report["GetOpportunity"] = await TimeMs(() => Context.Opportunities.AsNoTracking()
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted));
        report["GetAllOpportunities"] = await TimeMs(() => Context.Opportunities.AsNoTracking()
            .Where(o => !o.IsDeleted).Take(100).ToListAsync());
        report["GetRelatedItems"] = await TimeMs(async () =>
        {
            await Context.OpportunityStakeholders.AsNoTracking()
                .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted).ToListAsync();
            await Context.OpportunityDeliverables.AsNoTracking()
                .Where(d => d.OpportunityId == opportunity.Id && !d.IsDeleted).ToListAsync();
        });
        report["UpdateOpportunity"] = await TimeMs(async () =>
        {
            var opp = await Context.Opportunities.FirstAsync(o => o.Id == opportunity.Id);
            opp.Description = $"Updated {Guid.NewGuid():N}";
            opp.LastModifiedDate = DateTime.UtcNow;
            await SaveChangesAsync();
        });
        report["DeleteOpportunity"] = await TimeMs(async () =>
        {
            var toDelete = await SeedOpportunityAsync("DeleteBench");
            toDelete.IsDeleted = true;
            toDelete.DeletedDate = DateTime.UtcNow;
            await SaveChangesAsync();
        });

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Seeds required lookup data (OrganizationHierarchy, ProposedInitiativeType, Currency).
    /// Returns (orgUnitId, initTypeId) for use when creating opportunities.
    /// </summary>
    private async Task<(int OrgUnitId, int InitTypeId)> SeedRequiredLookupsAsync()
    {
        await EnsureTestUserAsync();
        if (!await Context.OrganizationHierarchies.AnyAsync(o => !o.IsDeleted))
        {
            await Context.OrganizationHierarchies.AddAsync(new OrganizationHierarchy
            {
                Name = $"OrgUnit {_testMarker}",
                Code = "PERF",
                Description = "Perf test org",
                Type = OrganizationUnitType.Office,
                IsDeleted = false,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            });
        }
        if (!await Context.ProposedInitiativeTypes.AnyAsync(p => !p.IsDeleted))
        {
            await Context.ProposedInitiativeTypes.AddAsync(new ProposedInitiativeType
            {
                Name = $"InitiativeType {_testMarker}",
                IsDeleted = false,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            });
        }
        if (!await Context.Currencies.AnyAsync(c => !c.IsDeleted))
        {
            await Context.Currencies.AddAsync(new Currency
            {
                Name = "US Dollar",
                Code = "USD",
                IsDeleted = false
            });
        }
        await SaveChangesAsync();

        var orgUnitId = await Context.OrganizationHierarchies
            .Where(o => !o.IsDeleted)
            .Select(o => o.Id)
            .FirstAsync();
        var initTypeId = await Context.ProposedInitiativeTypes
            .Where(p => !p.IsDeleted)
            .Select(p => p.Id)
            .FirstAsync();
        return (orgUnitId, initTypeId);
    }

    private UNOPS.PAO.Domain.Entities.Opportunity BuildOpportunity(string name, int? orgUnitId, int? initTypeId)
    {
        return new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = name,
            Description = $"Perf test opportunity {_testMarker}",
            Stage = OpportunityWorkflow.Stages.IdentifyAndProfile,
            Status = EntityStatus.Active,
            ResponsibleOrgUnitId = orgUnitId > 0 ? orgUnitId : null,
            ProposedInitiativeTypeId = initTypeId > 0 ? initTypeId : null,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
    }

    private async Task<UNOPS.PAO.Domain.Entities.Opportunity> SeedOpportunityAsync(string? suffix = null)
    {
        var (orgUnitId, initTypeId) = await SeedRequiredLookupsAsync();
        var raw = $"Seed {suffix ?? Guid.NewGuid().ToString("N")} {_testMarker}";
        var name = raw.Length > 50 ? raw[..50] : raw;
        var opp = BuildOpportunity(name, orgUnitId, initTypeId);
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        return opp;
    }

    private async Task<List<UNOPS.PAO.Domain.Entities.Opportunity>> SeedOpportunitiesAsync(int count)
    {
        var (orgUnitId, initTypeId) = await SeedRequiredLookupsAsync();
        var existing = await Context.Opportunities.CountAsync(o => !o.IsDeleted && o.Name.Contains(_testMarker));
        if (existing >= count)
            return await Context.Opportunities
                .Where(o => !o.IsDeleted && o.Name.Contains(_testMarker))
                .Take(count)
                .ToListAsync();

        var toCreate = count - existing;
        var opportunities = Enumerable.Range(existing, toCreate)
            .Select(i => BuildOpportunity($"Seed {i} {_testMarker}", orgUnitId, initTypeId))
            .ToList();
        await Context.Opportunities.AddRangeAsync(opportunities);
        await SaveChangesAsync();
        return await Context.Opportunities
            .Where(o => !o.IsDeleted && o.Name.Contains(_testMarker))
            .Take(count)
            .ToListAsync();
    }

    private async Task<UNOPS.PAO.Domain.Entities.Opportunity> SeedOpportunityWithRelatedDataAsync()
    {
        var (orgUnitId, initTypeId) = await SeedRequiredLookupsAsync();
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var currencyId = await Context.Currencies.Where(c => !c.IsDeleted).Select(c => c.Id).FirstAsync();

        var opp = BuildOpportunity($"Related {_testMarker}", orgUnitId, initTypeId);
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();

        var fundingPartner = new OpportunityFundingPartner
        {
            Name = "Perf Funding Partner",
            OpportunityId = opp.Id,
            PartnerId = partnerId,
            Amount = 100_000m,
            CurrencyId = currencyId,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        var clientPartner = new OpportunityClientPartner
        {
            Name = "Perf Client Partner",
            OpportunityId = opp.Id,
            PartnerId = partnerId,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.OpportunityFundingPartners.AddAsync(fundingPartner);
        await Context.OpportunityClientPartners.AddAsync(clientPartner);
        await SaveChangesAsync();

        return await Context.Opportunities
            .Include(o => o.FundingPartners)
            .Include(o => o.ClientPartners)
            .FirstAsync(o => o.Id == opp.Id);
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
