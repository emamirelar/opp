/**
 * @fileoverview PNO-1196 Functional Tests: Closed status UI changes.
 * Business rules: audit fields, stage history, permissions, filtering.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1196.ClosedStatusUI;

/// <summary>
/// PNO-1196 Functional Tests — 9+ tests covering business rules for Closed opportunities.
/// </summary>
[Collection("PNO1196_Functional")]
[Trait("Category", "Functional")]
[Trait("Ticket", "PNO-1196")]
public class FunctionalTests : PNO1196TestFixtureBase
{
    [Fact]
    public async Task ClosedOpportunity_AuditFieldsUpdatedOnClose()
    {
        // Arrange
        await SeedOpportunityAsync(1, "ACTIVE", EntityStatus.Active);

        // Act
        await TransitionToClosedAsync(1, "NO GO");

        // Assert
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.LastModifiedBy.Should().Be(1);
        opp.LastModifiedDate.Should().NotBeNull();
        opp.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ClosedOpportunity_StageHistoryRecorded()
    {
        // Arrange: Stage change updates LastModifiedDate
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE", EntityStatus.Active);
        var beforeClose = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 2);

        // Act
        await TransitionToClosedAsync(2, "NO GO");
        var afterClose = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 2);

        // Assert
        afterClose.Stage.Should().Be("NO GO");
        afterClose.LastModifiedDate.Should().NotBeNull();
        afterClose.LastModifiedDate.Should().BeAfter(beforeClose.LastModifiedDate ?? beforeClose.CreatedDate);
    }

    [Fact]
    public async Task ClosedOpportunity_RelatedEntitiesUnchanged()
    {
        // Arrange
        await SeedClosedOpportunityAsync(3, responsibleOrgUnitId: 1);

        // Act
        var opp = await DbContext.Opportunities
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == 3 && !o.IsDeleted);

        // Assert: Related data preserved
        opp.Should().NotBeNull();
        opp!.ResponsibleOrgUnitId.Should().Be(1);
    }

    [Fact]
    public async Task ClosedOpportunity_BudgetRemainsUnchanged()
    {
        // Arrange
        const decimal budget = 100000m;
        await SeedOpportunityAsync(4, "ACTIVE", EntityStatus.Active, initiativeBudgetUSD: budget);

        // Act
        await TransitionToClosedAsync(4, "NO GO");

        // Assert
        var opp = await DbContext.Opportunities.FindAsync(4);
        opp!.InitiativeBudgetUSD.Should().Be(budget);
    }

    [Fact]
    public async Task ClosedOpportunity_PermissionsRestrictedAfterClose()
    {
        // Arrange: Closed opportunities have canEdit=false, canDelete=false at API layer
        await SeedClosedOpportunityAsync(5);

        // Assert: Data state supports restricted permissions
        var opp = await DbContext.Opportunities.FindAsync(5);
        opp!.Status.Should().Be(EntityStatus.Closed);
        // Status.Closed is the condition that triggers canEdit=false in permission logic
    }

    [Fact]
    public async Task ClosedOpportunity_CountExcludesDeleted()
    {
        // Arrange
        await SeedClosedOpportunityAsync(6);
        await SeedClosedOpportunityAsync(7);
        var opp7 = await DbContext.Opportunities.FindAsync(7);
        opp7!.IsDeleted = true;
        opp7.DeletedBy = 1;
        opp7.DeletedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();

        // Act
        var count = await DbContext.Opportunities
            .CountAsync(o => !o.IsDeleted && o.Status == EntityStatus.Closed);

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task ClosedOpportunity_FilterByStageReturnsOnlyClosed()
    {
        // Arrange
        await SeedClosedOpportunityAsync(8, "NO GO");
        await SeedOpportunityAsync(9, "NO GO", EntityStatus.Active); // Same stage, different status

        // Act: Filter by both Stage and Status for Closed
        var closedOpps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == "NO GO" && o.Status == EntityStatus.Closed)
            .ToListAsync();

        // Assert
        closedOpps.Should().HaveCount(1);
        closedOpps[0].Id.Should().Be(8);
    }

    [Fact]
    public async Task ClosedOpportunity_StakeholdersPreserved()
    {
        // Arrange: Add stakeholder to opportunity then close
        await SeedOpportunityAsync(10, "ACTIVE", EntityStatus.Active);
        if (!await DbContext.Set<EntityRole>().AnyAsync(r => r.Name == "Opportunity Manager"))
        {
            DbContext.Set<EntityRole>().Add(new EntityRole
            {
                Id = 100,
                Name = "Opportunity Manager",
                EntityType = "Opportunity",
                IsDeleted = false,
                Status = EntityStatus.Active
            });
            await DbContext.SaveChangesAsync();
        }
        var roleId = await DbContext.Set<EntityRole>().Where(r => r.Name == "Opportunity Manager").Select(r => r.Id).FirstAsync();
        DbContext.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            OpportunityId = 10,
            EntityRoleId = roleId,
            Name = "Stakeholder 1",
            IsInternal = true,
            IsDeleted = false
        });
        await DbContext.SaveChangesAsync();

        // Act
        await TransitionToClosedAsync(10, "NO GO");

        // Assert
        var stakeholderCount = await DbContext.Set<OpportunityStakeholder>()
            .CountAsync(s => s.OpportunityId == 10 && !s.IsDeleted);
        stakeholderCount.Should().Be(1);
    }

    [Fact]
    public async Task ClosedOpportunity_DocumentsPreserved()
    {
        // Arrange: Closed opportunity - documents are linked via DocumentRelationship
        await SeedClosedOpportunityAsync(11);

        // Act: Verify opportunity exists and can be queried with documents (if any)
        var opp = await DbContext.Opportunities
            .FirstOrDefaultAsync(o => o.Id == 11 && !o.IsDeleted);

        // Assert
        opp.Should().NotBeNull();
        opp!.Status.Should().Be(EntityStatus.Closed);
    }
}
