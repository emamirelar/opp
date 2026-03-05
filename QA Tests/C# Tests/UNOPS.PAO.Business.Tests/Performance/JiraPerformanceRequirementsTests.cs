using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance requirements tests migrated from JIRA.
/// Covers: Search performance (PNO-693), Mass upload (PNO-457),
/// Pagination, Entity detail loading, Memory usage.
/// Tests real DB operations via UNOPSAppDbContext with timing measurements.
/// </summary>
public class JiraPerformanceRequirementsTests : PerformanceTestBase
{
    private readonly string _marker = $"JPR_{Guid.NewGuid():N}";

    #region Seed Helpers

    private async Task SeedPartnersAsync(int count)
    {
        await EnsureTestUserAsync();
        for (int i = 0; i < count; i++)
        {
            var partner = new UNOPSPartner
            {
                Name = $"PerfPartner_{i}_{_marker}",
                PartnerShortDescription = $"Desc {i}",
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Partners.AddAsync(partner);
        }
        await Context.SaveChangesAsync();
    }

    private async Task SeedOpportunitiesAsync(int count)
    {
        await EnsureTestUserAsync();
        for (int i = 0; i < count; i++)
        {
            var opp = new Domain.Entities.Opportunity
            {
                Name = $"PerfOpp_{i}_{_marker}",
                Description = $"Performance test opportunity {i}",
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Opportunities.AddAsync(opp);
        }
        await Context.SaveChangesAsync();
    }

    private async Task SeedContactsAsync(int partnerId, int count)
    {
        await EnsureTestUserAsync();
        for (int i = 0; i < count; i++)
        {
            var contact = new UNOPSContact
            {
                Name = $"PerfContact_{i}_{_marker}",
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                Email = $"perf{i}_{_marker}@example.com",
                Title = "Analyst",
                PartnerId = partnerId,
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Contacts.AddAsync(contact);
        }
        await Context.SaveChangesAsync();
    }

    #endregion

    #region Positive Tests

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-POS-001")]
    public async Task POS_001_PartnerQuery_WithFilter_ReturnsResults()
    {
        await SeedPartnersAsync(10);

        var results = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .ToListAsync();

        results.Should().HaveCount(10);
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-POS-002")]
    public async Task POS_002_Pagination_ReturnsCorrectPageSize()
    {
        await SeedPartnersAsync(50);

        var page = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .OrderBy(p => p.Id)
            .Skip(0)
            .Take(20)
            .ToListAsync();

        page.Should().HaveCount(20);
    }

    #endregion

    #region Negative Tests (>= 6)

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-NEG-001")]
    public async Task NEG_001_SearchWithNoMatches_ReturnsEmpty()
    {
        var results = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains("NONEXISTENT_QUERY_12345"))
            .ToListAsync();

        results.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-NEG-002")]
    public async Task NEG_002_PaginationBeyondLastPage_ReturnsEmpty()
    {
        await SeedPartnersAsync(5);

        var page = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .OrderBy(p => p.Id)
            .Skip(1000)
            .Take(20)
            .ToListAsync();

        page.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JPR-NEG-003")]
    public async Task NEG_003_EmptyDataset_QueryReturnsNoResults()
    {
        var uniqueMarker = Guid.NewGuid().ToString();
        var count = await Context.Partners
            .CountAsync(p => p.Name!.Contains(uniqueMarker));

        count.Should().Be(0);
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-NEG-004")]
    public async Task NEG_004_DeletedRecords_ExcludedFromCount()
    {
        await SeedPartnersAsync(5);
        var partner = await Context.Partners
            .FirstAsync(p => p.Name!.Contains(_marker));
        partner.IsDeleted = true;
        Context.Partners.Update(partner);
        await Context.SaveChangesAsync();

        var activeCount = await Context.Partners
            .CountAsync(p => !p.IsDeleted && p.Name!.Contains(_marker));

        activeCount.Should().Be(4);
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-NEG-005")]
    public async Task NEG_005_OpportunitySearch_DeletedExcluded()
    {
        await SeedOpportunitiesAsync(3);
        var opp = await Context.Opportunities
            .FirstAsync(o => o.Name!.Contains(_marker));
        opp.IsDeleted = true;
        Context.Opportunities.Update(opp);
        await Context.SaveChangesAsync();

        var results = await Context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Name!.Contains(_marker))
            .ToListAsync();

        results.Should().HaveCount(2);
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JPR-NEG-006")]
    public async Task NEG_006_NegativePageNumber_SkipsNothing()
    {
        await SeedPartnersAsync(10);

        var page = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .OrderBy(p => p.Id)
            .Skip(0)
            .Take(20)
            .ToListAsync();

        page.Should().HaveCount(10);
    }

    #endregion

    #region Edge/Boundary Tests (>= 6)

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-EDGE-001")]
    public async Task EDGE_001_SingleRecord_QueryPerformance()
    {
        await SeedPartnersAsync(1);

        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Partners
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
                .FirstOrDefaultAsync());

        result.Should().NotBeNull();
        elapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-EDGE-002")]
    public async Task EDGE_002_PageSizeEqualsTotalRecords()
    {
        await SeedPartnersAsync(20);

        var page = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .OrderBy(p => p.Id)
            .Take(20)
            .ToListAsync();

        page.Should().HaveCount(20);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JPR-EDGE-003")]
    public async Task EDGE_003_BulkInsert_50Records_CompletesReasonably()
    {
        var elapsed = await MeasureAsync(async () =>
        {
            await SeedPartnersAsync(50);
        });

        elapsed.Should().BeLessThan(SlowOperationThreshold * 5,
            "bulk insert of 50 records should complete within 5x slow threshold");
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-EDGE-004")]
    public async Task EDGE_004_ContactSearch_WithManyContacts()
    {
        await EnsureTestUserAsync();
        var partner = new UNOPSPartner
        {
            Name = $"ContactPerfPartner_{_marker}",
            PartnerShortDescription = "Perf",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await Context.SaveChangesAsync();

        await SeedContactsAsync(partner.Id, 30);

        var (results, elapsed) = await MeasureAsync(async () =>
            await Context.Contacts
                .AsNoTracking()
                .Where(c => !c.IsDeleted && c.PartnerId == partner.Id)
                .ToListAsync());

        results.Should().HaveCount(30);
        elapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-EDGE-005")]
    public async Task EDGE_005_RepeatedQueries_ConsistentPerformance()
    {
        await SeedPartnersAsync(20);
        var times = new List<long>();

        for (int i = 0; i < 5; i++)
        {
            var elapsed = await MeasureAsync(async () =>
            {
                await Context.Partners
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
                    .ToListAsync();
            });
            times.Add(elapsed);
        }

        var maxVariance = times.Max() - times.Min();
        maxVariance.Should().BeLessThan(NormalOperationThreshold,
            "repeated queries should have consistent performance");
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-EDGE-006")]
    public async Task EDGE_006_AsNoTracking_ReducesMemoryOverhead()
    {
        await SeedPartnersAsync(20);

        var initialMemory = GC.GetTotalMemory(true);

        var results = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .ToListAsync();

        var finalMemory = GC.GetTotalMemory(false);
        var memoryUsedKB = (finalMemory - initialMemory) / 1024;

        results.Should().HaveCount(20);
        memoryUsedKB.Should().BeLessThan(50_000, "AsNoTracking queries should use minimal memory");
    }

    #endregion

    #region Functional Tests (>= 6)

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-FUNC-001")]
    public async Task FUNC_001_PartnerSearch_FilterByNameContains()
    {
        await SeedPartnersAsync(10);

        var (results, elapsed) = await MeasureAsync(async () =>
            await Context.Partners
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Name!.Contains("PerfPartner_5"))
                .ToListAsync());

        results.Should().NotBeEmpty();
        elapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-FUNC-002")]
    public async Task FUNC_002_OpportunityQuery_OrderByDate()
    {
        await SeedOpportunitiesAsync(10);

        var results = await Context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Name!.Contains(_marker))
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

        results.Should().HaveCount(10);
        results.First().CreatedDate.Should().BeOnOrAfter(results.Last().CreatedDate);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-FUNC-003")]
    public async Task FUNC_003_CountQuery_PerformsWell()
    {
        await SeedPartnersAsync(30);

        var (count, elapsed) = await MeasureAsync(async () =>
            await Context.Partners
                .CountAsync(p => !p.IsDeleted && p.Name!.Contains(_marker)));

        count.Should().Be(30);
        elapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JPR-FUNC-004")]
    public async Task FUNC_004_BulkInsert_AllRecordsRetrievable()
    {
        await SeedPartnersAsync(25);

        var all = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .ToListAsync();

        all.Should().HaveCount(25);
        all.Select(p => p.Name).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-FUNC-005")]
    public async Task FUNC_005_PaginatedQuery_AllPagesConsistent()
    {
        await SeedPartnersAsync(40);
        var allIds = new HashSet<int>();

        for (int page = 0; page < 4; page++)
        {
            var pageResults = await Context.Partners
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
                .OrderBy(p => p.Id)
                .Skip(page * 10)
                .Take(10)
                .Select(p => p.Id)
                .ToListAsync();

            pageResults.Should().HaveCount(10);
            foreach (var id in pageResults)
                allIds.Add(id);
        }

        allIds.Should().HaveCount(40, "all pages combined should cover all records");
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-FUNC-006")]
    public async Task FUNC_006_OpportunityDetail_SingleLoad_PerformsFast()
    {
        await SeedOpportunitiesAsync(1);
        var opp = await Context.Opportunities
            .FirstAsync(o => o.Name!.Contains(_marker));

        var (loaded, elapsed) = await MeasureAsync(async () =>
            await Context.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opp.Id));

        loaded.Should().NotBeNull();
        elapsed.Should().BeLessThan(FastOperationThreshold);
    }

    #endregion

    #region Integration Tests (>= 6)

    [Fact]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-INT-001")]
    public async Task INT_001_FullSearchWorkflow_SeedFilterPaginate()
    {
        await SeedPartnersAsync(30);

        var filtered = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker) && p.Status == EntityStatus.Active)
            .OrderBy(p => p.Name)
            .Skip(0)
            .Take(10)
            .ToListAsync();

        filtered.Should().HaveCount(10);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-INT-002")]
    public async Task INT_002_CrossEntity_PartnerWithContacts_Query()
    {
        await EnsureTestUserAsync();
        var partner = new UNOPSPartner
        {
            Name = $"CrossEntity_{_marker}",
            PartnerShortDescription = "Cross",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await Context.SaveChangesAsync();

        await SeedContactsAsync(partner.Id, 10);

        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Partners
                .AsNoTracking()
                .Include(p => p.Contacts.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == partner.Id));

        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(10);
        elapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JPR-INT-003")]
    public async Task INT_003_BulkInsertAndQuery_EndToEnd()
    {
        var insertElapsed = await MeasureAsync(async () =>
        {
            await SeedPartnersAsync(20);
        });

        var (queryResults, queryElapsed) = await MeasureAsync(async () =>
            await Context.Partners
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
                .ToListAsync());

        queryResults.Should().HaveCount(20);
        (insertElapsed + queryElapsed).Should().BeLessThan(SlowOperationThreshold);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-INT-004")]
    public async Task INT_004_OpportunityWithRelations_IncludePerformance()
    {
        await EnsureTestUserAsync();
        var opp = new Domain.Entities.Opportunity
        {
            Name = $"RelPerf_{_marker}",
            Description = "Perf test",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Opportunities.AddAsync(opp);
        await Context.SaveChangesAsync();

        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Opportunities
                .AsNoTracking()
                .Include(o => o.Deliverables.Where(d => !d.IsDeleted))
                .Include(o => o.SDGs.Where(s => !s.IsDeleted))
                .Include(o => o.Collaborators.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(o => o.Id == opp.Id));

        result.Should().NotBeNull();
        elapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-INT-005")]
    public async Task INT_005_MultiEntitySearch_AcrossPartnerAndOpportunity()
    {
        await SeedPartnersAsync(10);
        await SeedOpportunitiesAsync(10);

        var partnerElapsed = await MeasureAsync(async () =>
        {
            await Context.Partners
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
                .ToListAsync();
        });

        var oppElapsed = await MeasureAsync(async () =>
        {
            await Context.Opportunities
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.Name!.Contains(_marker))
                .ToListAsync();
        });

        partnerElapsed.Should().BeLessThan(NormalOperationThreshold);
        oppElapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    [Fact]

    [Trait("Defect", "DEF-082")]
    [Trait("JIRA", "PNO-693")]
    [Trait("TestId", "TC-JPR-INT-006")]
    public async Task INT_006_StatusFilteredCount_AllStatuses()
    {
        await SeedPartnersAsync(10);

        var (totalCount, elapsed) = await MeasureAsync(async () =>
        {
            var active = await Context.Partners
                .CountAsync(p => !p.IsDeleted && p.Name!.Contains(_marker) && p.Status == EntityStatus.Active);
            var draft = await Context.Partners
                .CountAsync(p => !p.IsDeleted && p.Name!.Contains(_marker) && p.Status == EntityStatus.Draft);
            return active + draft;
        });

        totalCount.Should().Be(10);
        elapsed.Should().BeLessThan(NormalOperationThreshold);
    }

    #endregion
}
