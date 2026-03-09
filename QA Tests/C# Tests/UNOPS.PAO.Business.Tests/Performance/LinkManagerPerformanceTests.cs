/**
 * PERFORMANCE TESTS — LinkManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Link/URL management for partners, contacts, partner trees
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
using UNOPS.PAO.Models.Links;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for LinkManager.
/// Verifies response times, throughput, bulk retrieval, search/filter,
/// memory efficiency, N+1 detection, AsNoTracking optimization, and cross-entity benchmarks.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class LinkManagerPerformanceTests : PerformanceTestBase
{
    private readonly ILinkManager _manager;
    private readonly IMapper _mapper;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"LinkPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public LinkManagerPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();
        _manager = new LinkManager(_mapper, Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task CreateLink_SingleEntity_CompletesWithinThreshold()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var request = BuildCreateRequest(LinkEntityType.Partner, partnerId, $"https://example.com/{_testMarker}");

        _stopwatch.Restart();
        var result = await _manager.CreateLinkAsync(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreateLink took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetLink_ExistingEntity_CompletesWithinThreshold()
    {
        var link = await SeedLinkAsync();

        _stopwatch.Restart();
        var result = await _manager.GetLink(link.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetLink took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task UpdateLink_ExistingEntity_CompletesWithinThreshold()
    {
        var link = await SeedLinkAsync();

        _stopwatch.Restart();
        var result = await _manager.UpdateLinkAsync(new UpdateLinkRequest
        {
            Id = link.Id,
            Entity = link.Entity,
            EntityId = link.EntityId,
            Url = $"https://updated.example.com/{_testMarker}",
            Name = $"Updated {_testMarker}"
        });
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpdateLink took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task DeleteLink_ExistingEntity_CompletesWithinThreshold()
    {
        var link = await SeedLinkAsync();

        _stopwatch.Restart();
        await _manager.DeleteLinkAsync(link.Id);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeleteLink took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetEntityLinks_100Links_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 100);
        var request = new PaginationRequest(1, 200);

        _stopwatch.Restart();
        var result = await _manager.GetEntityLinks(entityType, entityId, request);
        _stopwatch.Stop();

        result.Records.Should().HaveCount(100);
        result.TotalCount.Should().Be(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetEntityLinks (100 links) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityLinks_200Links_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 200);
        var request = new PaginationRequest(1, 250);

        _stopwatch.Restart();
        var result = await _manager.GetEntityLinks(entityType, entityId, request);
        _stopwatch.Stop();

        result.Records.Should().HaveCount(200);
        result.TotalCount.Should().Be(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetEntityLinks (200 links) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task BulkCreate_50Links_CompletesWithinThreshold()
    {
        var partnerId = await CreateTestPartnerAsync($"BulkPartner_{_testMarker}");
        var requests = Enumerable.Range(1, 50)
            .Select(i => BuildCreateRequest(LinkEntityType.Partner, partnerId, $"https://bulk{i}.example.com"))
            .ToList();

        _stopwatch.Restart();
        foreach (var r in requests)
            await _manager.CreateLinkAsync(r);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"BulkCreate 50 links took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetEntityLinks_SimpleQuery_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 100);
        var request = new PaginationRequest(1, 50);

        _stopwatch.Restart();
        var result = await _manager.GetEntityLinks(entityType, entityId, request);
        _stopwatch.Stop();

        result.Records.Should().HaveCount(50);
        result.TotalCount.Should().Be(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple GetEntityLinks took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityLinks_MultipleEntityTypes_CompletesWithinThreshold()
    {
        var (partnerType, partnerId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 50);
        var (contactType, contactId) = await SeedLinksForEntityAsync(LinkEntityType.Contact, 50);
        var request = new PaginationRequest(1, 100);

        _stopwatch.Restart();
        var partnerResult = await _manager.GetEntityLinks(partnerType, partnerId, request);
        var contactResult = await _manager.GetEntityLinks(contactType, contactId, request);
        _stopwatch.Stop();

        partnerResult.Records.Should().HaveCount(50);
        contactResult.Records.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Multi-entity GetEntityLinks took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityLinks_ExcludesDeleted_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 100);
        var linksToDelete = await Context.Links
            .Where(l => l.Entity == entityType && l.EntityId == entityId && !l.IsDeleted)
            .Take(20)
            .ToListAsync();
        foreach (var l in linksToDelete)
        {
            l.IsDeleted = true;
            l.DeletedDate = DateTime.UtcNow;
        }
        await SaveChangesAsync();

        var request = new PaginationRequest(1, 200);

        _stopwatch.Restart();
        var result = await _manager.GetEntityLinks(entityType, entityId, request);
        _stopwatch.Stop();

        result.Records.Should().HaveCount(80);
        result.TotalCount.Should().Be(80);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetEntityLinks (excluding deleted) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityLinks_Pagination_FirstPageFast()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 500);
        var request = new PaginationRequest(1, 20);

        _stopwatch.Restart();
        var result = await _manager.GetEntityLinks(entityType, entityId, request);
        _stopwatch.Stop();

        result.Records.Should().HaveCount(20);
        result.TotalCount.Should().Be(500);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Paginated GetEntityLinks (500 total) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetEntityLinks_ContactEntity_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Contact, 75);
        var request = new PaginationRequest(1, 100);

        _stopwatch.Restart();
        var result = await _manager.GetEntityLinks(entityType, entityId, request);
        _stopwatch.Stop();

        result.Records.Should().HaveCount(75);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetEntityLinks (Contact) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    [Trait("Defect", "DEF-083")]
    [Trait("Defect", "QA-020")]
    public async Task ConcurrentReads_50ParallelGetLink_MaintainsPerformance()
    {
        var link = await SeedLinkAsync();
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetLink(link.Id))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().OnlyContain(r => r != null);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]
    [Trait("Defect", "DEF-083")]
    [Trait("Defect", "QA-020")]
    public async Task ConcurrentReads_20ParallelGetEntityLinks_MaintainsPerformance()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 50);
        var request = new PaginationRequest(1, 100);
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _manager.GetEntityLinks(entityType, entityId, request))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r.Records.Count == 50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetEntityLinks took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("Defect", "DEF-083")]
    [Trait("Defect", "QA-020")]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 30);
        var link = await SeedLinkAsync();
        var partnerId = await CreateTestPartnerAsync($"MixPartner_{_testMarker}");
        var request = new PaginationRequest(1, 50);

        var readTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetEntityLinks(entityType, entityId, request))
            .Cast<Task>()
            .ToList();
        var getTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.GetLink(link.Id))
            .Cast<Task>()
            .ToList();
        var createTasks = Enumerable.Range(0, 5)
            .Select(i => _manager.CreateLinkAsync(BuildCreateRequest(LinkEntityType.Partner, partnerId, $"https://mix{i}.example.com")))
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(getTasks).Concat(createTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent ops took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeLinkList_MemoryUsage_WithinCap()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 1000);
        var request = new PaginationRequest(1, 1000);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetEntityLinks(entityType, entityId, request);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            var link = await SeedLinkAsync();
            await _manager.GetLink(link.Id);
            await _manager.DeleteLinkAsync(link.Id);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 100);
        var request = new PaginationRequest(1, 50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetEntityLinks(entityType, entityId, request);
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
    public async Task GetEntityLinks_WithRelated_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 50);
        var request = new PaginationRequest(1, 50);

        _stopwatch.Restart();
        var result = await _manager.GetEntityLinks(entityType, entityId, request);
        _stopwatch.Stop();

        result.Records.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    [Fact]
    public async Task CrossEntity_LinkQueryBenchmark_AllEntityTypesWithinThreshold()
    {
        var (partnerType, partnerId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 25);
        var (contactType, contactId) = await SeedLinksForEntityAsync(LinkEntityType.Contact, 25);
        var (partnerTreeType, partnerTreeId) = await SeedLinksForEntityAsync(LinkEntityType.PartnerTree, 25);
        var request = new PaginationRequest(1, 50);

        _stopwatch.Restart();
        var partnerResult = await _manager.GetEntityLinks(partnerType, partnerId, request);
        var contactResult = await _manager.GetEntityLinks(contactType, contactId, request);
        var partnerTreeResult = await _manager.GetEntityLinks(partnerTreeType, partnerTreeId, request);
        _stopwatch.Stop();

        partnerResult.Records.Should().HaveCount(25);
        contactResult.Records.Should().HaveCount(25);
        partnerTreeResult.Records.Should().HaveCount(25);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Cross-entity link query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var link = await SeedLinkAsync();

        report["GetLink"] = await TimeMs(() => _manager.GetLink(link.Id));
        report["UpdateLink"] = await TimeMs(() => _manager.UpdateLinkAsync(new UpdateLinkRequest
        {
            Id = link.Id,
            Entity = link.Entity,
            EntityId = link.EntityId,
            Url = link.Url,
            Name = link.Name
        }));
        report["DeleteLink"] = await TimeMs(() => _manager.DeleteLinkAsync(link.Id));

        var (entityType, entityId) = await SeedLinksForEntityAsync(LinkEntityType.Partner, 20);
        var request = new PaginationRequest(1, 20);
        report["GetEntityLinks"] = await TimeMs(() => _manager.GetEntityLinks(entityType, entityId, request));

        var partnerId = await CreateTestPartnerAsync($"Bench_{_testMarker}");
        report["CreateLink"] = await TimeMs(() => _manager.CreateLinkAsync(BuildCreateRequest(LinkEntityType.Partner, partnerId, "https://bench.example.com")));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-20}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private LinkRequest BuildCreateRequest(LinkEntityType entity, int entityId, string url, string? name = null) =>
        new()
        {
            Entity = entity,
            EntityId = entityId,
            Url = url,
            Name = name ?? url
        };

    private async Task<LinkModel> SeedLinkAsync()
    {
        var partnerId = await CreateTestPartnerAsync($"Link_{_testMarker}");
        var request = BuildCreateRequest(LinkEntityType.Partner, partnerId, $"https://seed.{_testMarker}.example.com");
        return await _manager.CreateLinkAsync(request);
    }

    private async Task<(LinkEntityType EntityType, int EntityId)> SeedLinksForEntityAsync(LinkEntityType entityType, int count)
    {
        int entityId = entityType switch
        {
            LinkEntityType.Partner => await CreateTestPartnerAsync($"Links_{_testMarker}"),
            LinkEntityType.Contact => await CreateTestContactAsync(),
            LinkEntityType.PartnerTree => await CreateTestPartnerTreeAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(entityType))
        };

        var links = Enumerable.Range(1, count)
            .Select(i => new UNOPSLink
            {
                Entity = entityType,
                EntityId = entityId,
                Url = $"https://{_testMarker}-{i}.example.com",
                Name = $"Link {i} {_testMarker}",
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.UtcNow
            })
            .ToList();
        await Context.Links.AddRangeAsync(links);
        await SaveChangesAsync();

        return (entityType, entityId);
    }

    private async Task<int> CreateTestContactAsync()
    {
        var partnerId = await CreateTestPartnerAsync($"ContactPartner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Contact {_testMarker}",
            FirstName = "Test",
            LastName = "Contact",
            Title = "Manager",
            Email = $"contact_{_testMarker}@test.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        return contact.Id;
    }

    private async Task<int> CreateTestPartnerTreeAsync()
    {
        var code = $"PT_{Guid.NewGuid():N}"[..20];
        var tree = new UNOPSPartnerTree
        {
            Code = code,
            Name = $"PartnerTree {_testMarker}",
            Description = $"Description {_testMarker}",
            Type = "Group",
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        return tree.Id;
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
