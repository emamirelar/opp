/**
 * @fileoverview PNO-1196 Positive Tests: Closed status UI changes.
 * Happy-path tests for opportunity transition to Closed, persistence, and querying.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1196.ClosedStatusUI;

/// <summary>
/// PNO-1196 Positive Tests — 3 tests covering happy-path Closed status scenarios.
/// </summary>
[Collection("PNO1196_Positive")]
[Trait("Category", "Positive")]
[Trait("Ticket", "PNO-1196")]
public class PositiveTests : PNO1196TestFixtureBase
{
    [Fact]
    public async Task OpportunityClosed_StagePersistedCorrectly()
    {
        // Arrange
        await SeedOpportunityAsync(1, "ACTIVE", EntityStatus.Active);

        // Act
        await TransitionToClosedAsync(1, "NO GO");

        // Assert
        var opp = await DbContext.Opportunities.FirstOrDefaultAsync(o => o.Id == 1 && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task OpportunityClosed_StatusPersistedCorrectly()
    {
        // Arrange: Create and close opportunity
        await SeedOpportunityAsync(2, "GO", EntityStatus.Active);
        await TransitionToClosedAsync(2, "NO GO");

        // Act: Reload from DB
        var opp = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 2 && !o.IsDeleted);

        // Assert: Status is Closed and persisted correctly
        opp.Should().NotBeNull();
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task OpportunityClosed_CanBeQueriedByStage()
    {
        // Arrange
        await SeedClosedOpportunityAsync(3, "NO GO");
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE", EntityStatus.Active);

        // Act
        var closedOpps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == "NO GO" && o.Status == EntityStatus.Closed)
            .ToListAsync();

        // Assert
        closedOpps.Should().HaveCount(1);
        closedOpps[0].Id.Should().Be(3);
    }
}
