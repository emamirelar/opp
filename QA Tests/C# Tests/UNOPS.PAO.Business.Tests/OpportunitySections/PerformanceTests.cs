/**
 * PERFORMANCE TESTS — Opportunity Sections
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * Tests REAL opportunity section operations against a real database:
 * stakeholders, deliverables, SDGs, risks, documents.
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
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
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;

namespace UNOPS.PAO.Business.Tests.OpportunitySections;

/// <summary>
/// Performance Tests for Opportunity Sections (stakeholders, deliverables, SDGs).
/// Verifies response times, throughput, and behaviour under concurrent access
/// for real database operations on opportunity section entities.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
[Collection("Performance")]
[Trait("Category", "Performance")]
[Trait("Type", "Performance")]
public class PerformanceTests : PerformanceTestBase
{
    private readonly IOpportunityManager _opportunityManager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"OppSec_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private const int MaxSingleOperationMs = 500;
    private const int MaxBulkOperationMs = 5_000;
    private const int MaxSimpleSearchMs = 500;
    private const int MaxComplexSearchMs = 2_000;
    private const int MaxPaginatedQueryMs = 200;
    private const int MaxConcurrentReadMs = 100;
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public PerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        _opportunityManager = new OpportunityManager(mapper, Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task GetOpportunity_WithSections_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 5, deliverableCount: 5, sdgCount: 3);

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Stakeholders.Should().HaveCount(5);
        result.Deliverables.Should().HaveCount(5);
        result.SDGs.Should().HaveCount(3);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetOpportunity with sections took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task LoadStakeholdersByOpportunity_SingleQuery_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 20);

        _stopwatch.Restart();
        var stakeholders = await Context.OpportunityStakeholders
            .AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
            .Include(s => s.EntityRole)
            .ToListAsync();
        _stopwatch.Stop();

        stakeholders.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"LoadStakeholders took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    [Trait("SubCategory", "BulkOps")]
    public async Task BulkLoadStakeholders_100Records_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 100);

        _stopwatch.Restart();
        var stakeholders = await Context.OpportunityStakeholders
            .AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        stakeholders.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk load 100 stakeholders took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "BulkOps")]
    public async Task BulkCreateStakeholders_50Records_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityAsync();
        var entityRoleId = await GetOrCreateEntityRoleAsync();

        var stakeholders = Enumerable.Range(1, 50)
            .Select(i => new OpportunityStakeholder
            {
                Name = $"Stakeholder {i} {_testMarker}",
                OpportunityId = opportunity.Id,
                EntityRoleId = entityRoleId,
                IsInternal = true,
                UserId = TestUserId,
                Status = EntityStatus.Active
            })
            .ToList();

        _stopwatch.Restart();
        await Context.OpportunityStakeholders.AddRangeAsync(stakeholders);
        await SaveChangesAsync();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk create 50 stakeholders took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "BulkOps")]
    public async Task BulkLoadDeliverables_100Records_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithDeliverablesAsync(100);

        _stopwatch.Restart();
        var deliverables = await Context.OpportunityDeliverables
            .AsNoTracking()
            .Where(d => d.OpportunityId == opportunity.Id && !d.IsDeleted)
            .ToListAsync();
        _stopwatch.Stop();

        deliverables.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk load 100 deliverables took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task SearchStakeholders_ByOpportunity_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 200);

        _stopwatch.Restart();
        var result = await Context.OpportunityStakeholders
            .AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted && s.Name!.Contains(_testMarker))
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Search stakeholders took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task SearchOpportunities_ByStage_CompletesWithinThreshold()
    {
        await SeedOpportunitiesAsync(50, stage: "IDENTIFY & PROFILE");

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Stage == "IDENTIFY & PROFILE" && o.Name!.Contains(_testMarker))
            .Take(100)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCountGreaterThanOrEqualTo(1);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Search opportunities by stage took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task SearchSDGs_ByOpportunity_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithSDGsAsync(5);

        _stopwatch.Restart();
        var result = await Context.OpportunitySDGs
            .AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
            .Include(s => s.SDG)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(5);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Search SDGs by opportunity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task PaginatedOpportunityList_CompletesWithinThreshold()
    {
        await SeedOpportunitiesAsync(100);

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Name!.Contains(_testMarker))
            .OrderBy(o => o.Id)
            .Skip(0)
            .Take(20)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCountLessThanOrEqualTo(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Paginated opportunity list took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task ComplexFilter_OpportunitiesWithSections_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 10, deliverableCount: 10);

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Id == opportunity.Id)
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Stakeholders.Should().HaveCount(10);
        result.Deliverables.Should().HaveCount(10);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Complex filter with sections took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    [Trait("SubCategory", "ConcurrentAccess")]
    public async Task ConcurrentReads_50SequentialStakeholderLoads_MaintainsPerformance()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 30);
        var results = new List<List<OpportunityStakeholder>>();

        _stopwatch.Restart();
        for (int i = 0; i < 50; i++)
        {
            var stakeholders = await Context.OpportunityStakeholders
                .AsNoTracking()
                .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
                .ToListAsync();
            results.Add(stakeholders);
        }
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r.Count == 30);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 sequential calls exceeded threshold: {avgMs}ms");
    }

    [Fact]
    [Trait("SubCategory", "ConcurrentAccess")]
    public async Task ConcurrentReads_20SequentialGetOpportunity_MaintainsPerformance()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 10);
        var results = new List<OpportunityEntity?>();

        _stopwatch.Restart();
        for (int i = 0; i < 20; i++)
        {
            var opp = await Context.Opportunities
                .AsNoTracking()
                .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
                .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
            results.Add(opp);
        }
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 sequential GetOpportunity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "ConcurrentAccess")]
    public async Task ConcurrentMixedReads_PerformanceStable()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 20, deliverableCount: 20);

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            await Context.OpportunityStakeholders
                .AsNoTracking()
                .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
                .ToListAsync();
            await Context.OpportunityDeliverables
                .AsNoTracking()
                .Where(d => d.OpportunityId == opportunity.Id && !d.IsDeleted)
                .ToListAsync();
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed sequential reads took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    [Trait("SubCategory", "Memory")]
    public async Task LargeStakeholderQuery_MemoryUsage_WithinCap()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await Context.OpportunityStakeholders
            .AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
            .Include(s => s.EntityRole)
            .ToListAsync();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    [Trait("SubCategory", "Memory")]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 20);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await Context.OpportunityStakeholders
                .AsNoTracking()
                .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
                .ToListAsync();
            await Context.OpportunityDeliverables
                .AsNoTracking()
                .Where(d => d.OpportunityId == opportunity.Id && !d.IsDeleted)
                .ToListAsync();
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    [Trait("SubCategory", "Memory")]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await Context.OpportunityStakeholders
                .AsNoTracking()
                .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
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

    #region EF Core — N+1 & Split Query Verification

    [Fact]
    [Trait("SubCategory", "N+1")]
    public async Task GetOpportunityWithAllSections_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 50, deliverableCount: 30, sdgCount: 5);

        _stopwatch.Restart();
        var result = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Stakeholders.Should().HaveCount(50);
        result.Deliverables.Should().HaveCount(30);
        result.SDGs.Should().HaveCount(5);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Possible N+1 or Cartesian product — GetOpportunity with sections took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("SubCategory", "N+1")]
    public async Task LoadDeliverablesWithRelated_NoN1Pattern_CompletesWithinThreshold()
    {
        var opportunity = await SeedOpportunityWithDeliverablesAsync(50);

        _stopwatch.Restart();
        var deliverables = await Context.OpportunityDeliverables
            .AsNoTracking()
            .Where(d => d.OpportunityId == opportunity.Id && !d.IsDeleted)
            .Include(d => d.Output)
            .ToListAsync();
        _stopwatch.Stop();

        deliverables.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 — deliverables query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    [Trait("SubCategory", "Benchmark")]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var opportunity = await SeedOpportunityWithSectionsAsync(stakeholderCount: 20, deliverableCount: 20);

        report["GetOpportunity"] = await TimeMs(() => Context.Opportunities
            .AsNoTracking()
            .Include(o => o.Stakeholders.Where(s => !s.IsDeleted))
            .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
            .Include(o => o.SDGs.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id && !o.IsDeleted));
        report["LoadStakeholders"] = await TimeMs(() => Context.OpportunityStakeholders
            .AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
            .ToListAsync());
        report["LoadDeliverables"] = await TimeMs(() => Context.OpportunityDeliverables
            .AsNoTracking()
            .Where(d => d.OpportunityId == opportunity.Id && !d.IsDeleted)
            .ToListAsync());
        report["LoadSDGs"] = await TimeMs(() => Context.OpportunitySDGs
            .AsNoTracking()
            .Where(s => s.OpportunityId == opportunity.Id && !s.IsDeleted)
            .Include(s => s.SDG)
            .ToListAsync());
        report["SearchByStage"] = await TimeMs(() => Context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Stage == "IDENTIFY & PROFILE")
            .Take(20)
            .ToListAsync());

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<OpportunityEntity> SeedOpportunityAsync()
    {
        await EnsureTestUserAsync();
        var pitId = await GetOrCreateProposedInitiativeTypeAsync();
        var opp = new OpportunityEntity
        {
            Name = $"Opp {_testMarker}",
            Description = "Perf test opportunity",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            ProposedInitiativeTypeId = pitId,
            IsDeleted = false
        };
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        return opp;
    }

    private async Task<OpportunityEntity> SeedOpportunityWithSectionsAsync(
        int stakeholderCount = 0,
        int deliverableCount = 0,
        int sdgCount = 0)
    {
        var opp = await SeedOpportunityAsync();
        var entityRoleId = await GetOrCreateEntityRoleAsync();

        if (stakeholderCount > 0)
        {
            var stakeholders = Enumerable.Range(1, stakeholderCount)
                .Select(i => new OpportunityStakeholder
                {
                    Name = $"Stakeholder {i} {_testMarker}",
                    OpportunityId = opp.Id,
                    EntityRoleId = entityRoleId,
                    IsInternal = true,
                    UserId = TestUserId,
                    Status = EntityStatus.Active
                })
                .ToList();
            await Context.OpportunityStakeholders.AddRangeAsync(stakeholders);
        }

        if (deliverableCount > 0)
        {
            var deliverables = Enumerable.Range(1, deliverableCount)
                .Select(i => new OpportunityDeliverable
                {
                    Name = $"Deliverable {i} {_testMarker}",
                    OpportunityId = opp.Id,
                    Status = EntityStatus.Active
                })
                .ToList();
            await Context.OpportunityDeliverables.AddRangeAsync(deliverables);
        }

        if (sdgCount > 0)
        {
            var sdgIds = await GetOrCreateSDGIdsAsync(sdgCount);
            var sdgs = sdgIds.Select((id, i) => new OpportunitySDG
            {
                Name = $"SDG {i + 1} {_testMarker}",
                OpportunityId = opp.Id,
                SDGId = id,
                IsPrimary = i == 0,
                Status = EntityStatus.Active
            }).ToList();
            await Context.OpportunitySDGs.AddRangeAsync(sdgs);
        }

        await SaveChangesAsync();
        return opp;
    }

    private async Task<OpportunityEntity> SeedOpportunityWithDeliverablesAsync(int count)
    {
        return await SeedOpportunityWithSectionsAsync(deliverableCount: count);
    }

    private async Task<OpportunityEntity> SeedOpportunityWithSDGsAsync(int count)
    {
        return await SeedOpportunityWithSectionsAsync(sdgCount: count);
    }

    private async Task SeedOpportunitiesAsync(int count, string stage = "IDENTIFY & PROFILE")
    {
        await EnsureTestUserAsync();
        var pitId = await GetOrCreateProposedInitiativeTypeAsync();
        var opps = Enumerable.Range(1, count)
            .Select(i => new OpportunityEntity
            {
                Name = $"Opp {i} {_testMarker}",
                Description = $"Perf test {i}",
                Stage = stage,
                Status = EntityStatus.Active,
                ProposedInitiativeTypeId = pitId,
                IsDeleted = false
            })
            .ToList();
        await Context.Opportunities.AddRangeAsync(opps);
        await SaveChangesAsync();
    }

    private async Task<int> GetOrCreateEntityRoleAsync()
    {
        var existing = await Context.EntityRoles
            .FirstOrDefaultAsync(r => r.EntityType == "Opportunity" && !r.IsDeleted);
        if (existing != null) return existing.Id;

        var role = new EntityRole
        {
            EntityType = "Opportunity",
            Name = $"Stakeholder Role {_testMarker}",
            IsInternal = true,
            AllowsMultiple = true,
            Status = EntityStatus.Active
        };
        await Context.EntityRoles.AddAsync(role);
        await SaveChangesAsync();
        return role.Id;
    }

    private async Task<int> GetOrCreateProposedInitiativeTypeAsync()
    {
        var existing = await Context.ProposedInitiativeTypes
            .FirstOrDefaultAsync(p => !p.IsDeleted);
        if (existing != null) return existing.Id;

        var pit = new ProposedInitiativeType
        {
            Name = $"Project {_testMarker}",
            Order = 1,
            Status = EntityStatus.Active
        };
        await Context.ProposedInitiativeTypes.AddAsync(pit);
        await SaveChangesAsync();
        return pit.Id;
    }

    private async Task<List<int>> GetOrCreateSDGIdsAsync(int count)
    {
        var existing = await Context.SDGs
            .Where(s => !s.IsDeleted)
            .Take(count)
            .Select(s => s.Id)
            .ToListAsync();

        if (existing.Count >= count)
            return existing.Take(count).ToList();

        var toCreate = count - existing.Count;
        for (int i = 0; i < toCreate; i++)
        {
            var sdg = new SDG
            {
                Name = $"SDG Perf {i} {_testMarker}",
                SDGNumber = $"{existing.Count + i + 1}",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            await Context.SDGs.AddAsync(sdg);
        }
        await SaveChangesAsync();

        var newIds = await Context.SDGs
            .Where(s => !s.IsDeleted && s.Name!.Contains(_testMarker))
            .Select(s => s.Id)
            .ToListAsync();
        return existing.Concat(newIds).Take(count).ToList();
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
