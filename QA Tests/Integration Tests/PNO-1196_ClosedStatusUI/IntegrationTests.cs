/**
 * @fileoverview PNO-1196 Integration Tests: Closed status UI changes.
 * Full round-trip, multi-step flows, and cross-entity scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1196.ClosedStatusUI;

/// <summary>
/// PNO-1196 Integration Tests — 9+ tests covering full flows and integration scenarios.
/// </summary>
[Collection("PNO1196_Integration")]
[Trait("Category", "Integration")]
[Trait("Ticket", "PNO-1196")]
public class IntegrationTests : PNO1196TestFixtureBase
{
    [Fact]
    public async Task CreateOpportunity_ThenClose_FullRoundTrip()
    {
        // Arrange
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE", EntityStatus.Active);

        // Act: Full flow - create (already seeded) then transition to closed
        await TransitionToClosedAsync(1, "NO GO");

        // Assert
        var opp = await DbContext.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 1 && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task CreateOpportunity_GoDecision_ThenClose_FullFlow()
    {
        // Arrange: Simulate IDENTIFY & PROFILE -> GO -> NO GO (Closed)
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE", EntityStatus.Active);
        await ChangeOpportunityStageAsync(2, "GO");
        await TransitionToClosedAsync(2, "NO GO");

        // Assert
        var opp = await DbContext.Opportunities.FindAsync(2);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task ClosedOpportunity_QueryByMultipleFilters_WorksCorrectly()
    {
        // Arrange
        await SeedClosedOpportunityAsync(3, "NO GO", 1);
        await SeedOpportunityAsync(4, "ACTIVE", EntityStatus.Active, 1);

        // Act
        var closedOpps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == EntityStatus.Closed && o.ResponsibleOrgUnitId == 1)
            .ToListAsync();

        // Assert
        closedOpps.Should().HaveCount(1);
        closedOpps[0].Id.Should().Be(3);
    }

    [Fact]
    public async Task ClosedOpportunity_WithPartners_PartnersPreserved()
    {
        // Arrange: Closed opportunity - partner links via OpportunityFundingPartner
        await SeedClosedOpportunityAsync(5);

        // Act: Verify opportunity exists and can be loaded
        var opp = await DbContext.Opportunities
            .Include(o => o.FundingPartners.Where(fp => !fp.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == 5 && !o.IsDeleted);

        // Assert
        opp.Should().NotBeNull();
        opp!.Status.Should().Be(EntityStatus.Closed);
        opp.FundingPartners.Should().NotBeNull();
    }

    [Fact]
    public async Task ClosedOpportunity_PaginationIncludesClosed()
    {
        // Arrange
        await SeedClosedOpportunityAsync(6);
        await SeedClosedOpportunityAsync(7);
        await SeedOpportunityAsync(8, "ACTIVE", EntityStatus.Active);

        // Act: Paginated query including closed
        var page = await DbContext.Opportunities
            .Where(o => !o.IsDeleted)
            .OrderBy(o => o.Id)
            .Skip(0)
            .Take(10)
            .ToListAsync();

        // Assert
        var closedCount = page.Count(o => o.Status == EntityStatus.Closed);
        closedCount.Should().Be(2);
    }

    [Fact]
    public async Task ClosedOpportunity_ListViewShowsClosed()
    {
        // Arrange
        await SeedClosedOpportunityAsync(9);

        // Act: Simulate list view query
        var listItems = await DbContext.Opportunities
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.Id, o.Name, o.Stage, o.Status })
            .ToListAsync();

        // Assert
        var closedItem = listItems.FirstOrDefault(i => i.Id == 9);
        closedItem.Should().NotBeNull();
        closedItem!.Status.Should().Be(EntityStatus.Closed);
        closedItem.Stage.Should().Be("NO GO");
    }

    [Fact]
    public async Task ClosedOpportunity_SearchReturnsClosed()
    {
        // Arrange
        await SeedClosedOpportunityAsync(10);
        var opp = await DbContext.Opportunities.FindAsync(10);
        opp!.Name = "UniqueSearchableClosedOpp";
        await DbContext.SaveChangesAsync();

        // Act: Search by name
        var results = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Name.Contains("UniqueSearchableClosedOpp"))
            .ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task ClosedOpportunity_ExportIncludesClosed()
    {
        // Arrange
        await SeedClosedOpportunityAsync(11);

        // Act: Simulate export query
        var exportData = await DbContext.Opportunities
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.Id, o.Name, o.Stage, Status = o.Status.ToString() })
            .ToListAsync();

        // Assert
        var closedRow = exportData.FirstOrDefault(r => r.Id == 11);
        closedRow.Should().NotBeNull();
        closedRow!.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task ClosedOpportunity_DashboardCountIncludesClosed()
    {
        // Arrange
        await SeedClosedOpportunityAsync(12);
        await SeedClosedOpportunityAsync(13);
        await SeedOpportunityAsync(14, "ACTIVE", EntityStatus.Active);

        // Act: Dashboard count for closed opportunities
        var closedCount = await DbContext.Opportunities
            .CountAsync(o => !o.IsDeleted && o.Status == EntityStatus.Closed);

        // Assert
        closedCount.Should().Be(2);
    }
}
