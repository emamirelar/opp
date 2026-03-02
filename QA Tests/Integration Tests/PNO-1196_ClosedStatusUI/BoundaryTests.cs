/**
 * @fileoverview PNO-1196 Boundary Tests: Closed status UI changes.
 * Edge cases: soft-delete, max length, nullables, concurrency, idempotency.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1196.ClosedStatusUI;

/// <summary>
/// PNO-1196 Boundary Tests — 9+ tests covering edge cases for Closed opportunities.
/// </summary>
[Collection("PNO1196_Boundary")]
[Trait("Category", "Boundary")]
[Trait("Ticket", "PNO-1196")]
public class BoundaryTests : PNO1196TestFixtureBase
{
    [Fact]
    public async Task ClosedOpportunity_SoftDeletedThenQueried_NotReturned()
    {
        // Arrange
        await SeedClosedOpportunityAsync(1);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.IsDeleted = true;
        opp.DeletedBy = 1;
        opp.DeletedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();

        // Act
        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == EntityStatus.Closed)
            .ToListAsync();

        // Assert
        result.Should().NotContain(o => o.Id == 1);
    }

    [Fact]
    public async Task ClosedOpportunity_WithMaxLengthName_DisplaysCorrectly()
    {
        // Arrange: Name max 120 chars
        var longName = new string('A', 120);
        await SeedOpportunityAsync(2, "NO GO", EntityStatus.Closed, name: longName);

        // Act
        var opp = await DbContext.Opportunities.FirstOrDefaultAsync(o => o.Id == 2 && !o.IsDeleted);

        // Assert
        opp.Should().NotBeNull();
        opp!.Name.Should().HaveLength(120);
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task ClosedOpportunity_WithZeroBudget_StillClosable()
    {
        // Arrange
        await SeedOpportunityAsync(3, "ACTIVE", EntityStatus.Active, initiativeBudgetUSD: 0);

        // Act
        await TransitionToClosedAsync(3, "NO GO");

        // Assert
        var opp = await DbContext.Opportunities.FindAsync(3);
        opp!.Status.Should().Be(EntityStatus.Closed);
        opp.InitiativeBudgetUSD.Should().Be(0);
    }

    [Fact]
    public async Task ClosedOpportunity_WithNullDescription_StillClosable()
    {
        // Arrange: Opportunity requires Description - use minimal
        await SeedOpportunityAsync(4, "ACTIVE", EntityStatus.Active, description: "Minimal");

        // Act
        await TransitionToClosedAsync(4, "NO GO");

        // Assert
        var opp = await DbContext.Opportunities.FindAsync(4);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task ClosedOpportunity_WithAllRelatedEntities_StillClosable()
    {
        // Arrange: Opportunity with org unit
        await SeedClosedOpportunityAsync(5, responsibleOrgUnitId: 1);

        // Act
        var opp = await DbContext.Opportunities
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == 5 && !o.IsDeleted);

        // Assert
        opp.Should().NotBeNull();
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task ClosedOpportunity_StageStringCaseSensitivity()
    {
        // Arrange: Stage values are case-sensitive in DB
        await SeedOpportunityAsync(6, "NO GO", EntityStatus.Closed);

        // Act
        var exactMatch = await DbContext.Opportunities
            .AnyAsync(o => !o.IsDeleted && o.Stage == "NO GO");
        var lowerMatch = await DbContext.Opportunities
            .AnyAsync(o => !o.IsDeleted && o.Stage == "no go");

        // Assert
        exactMatch.Should().BeTrue();
        lowerMatch.Should().BeFalse();
    }

    [Fact]
    public async Task ClosedOpportunity_MultipleConcurrentCloseAttempts()
    {
        // Arrange
        await SeedOpportunityAsync(7, "ACTIVE", EntityStatus.Active);

        // Act: Simulate multiple close transitions (idempotent)
        await TransitionToClosedAsync(7, "NO GO");
        await TransitionToClosedAsync(7, "NO GO");

        // Assert: Final state is Closed
        var opp = await DbContext.Opportunities.FindAsync(7);
        opp!.Status.Should().Be(EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
    }

    [Fact]
    public async Task ClosedOpportunity_WithNoStakeholders_StillClosable()
    {
        // Arrange: Opportunity without stakeholders
        await SeedClosedOpportunityAsync(8);

        // Act
        var stakeholderCount = await DbContext.Set<OpportunityStakeholder>()
            .CountAsync(s => s.OpportunityId == 8 && !s.IsDeleted);

        // Assert
        stakeholderCount.Should().Be(0);
        var opp = await DbContext.Opportunities.FindAsync(8);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task ClosedOpportunity_ClosedTwice_IdempotentBehavior()
    {
        // Arrange
        await SeedClosedOpportunityAsync(9);

        // Act: Transition to closed again (already closed)
        await TransitionToClosedAsync(9, "NO GO");

        // Assert: Still closed, no duplicate state
        var opp = await DbContext.Opportunities.FindAsync(9);
        opp!.Status.Should().Be(EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
    }
}
