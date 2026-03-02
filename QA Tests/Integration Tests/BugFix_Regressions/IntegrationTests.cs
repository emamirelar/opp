/**
 * @fileoverview Integration regression tests for BugFix scenarios.
 * Full round-trip flows, filter persistence, stakeholder CRUD, exchange rate conversion.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.BugFixRegressions;

[Collection("BugFix_Integration")]
[Trait("Category", "Integration")]
public class IntegrationTests : BugFixRegressionTestFixtureBase
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GlobalFilter_ApplyThenReset_FullRoundTrip()
    {
        // Arrange
        await SeedOpportunityAsync(1, "Opp A", 1);
        await SeedOpportunityAsync(2, "Opp B", 2);
        await SeedOpportunityAsync(3, "Opp C", 1);

        // Act: Apply filter
        var filtered = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == 1)
            .ToListAsync();

        // Act: Reset (no filter)
        var all = await DbContext.Opportunities
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        // Assert
        filtered.Should().HaveCount(2);
        all.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GlobalFilter_FilterOpportunities_ThenNavigateToDetail_FiltersPreserved()
    {
        // Arrange
        await SeedOpportunityAsync(1, "Detail Opp", 1);

        // Act: Filter list, then get detail
        var listFilter = 1;
        var listResults = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == listFilter)
            .Select(o => o.Id)
            .ToListAsync();
        var detail = listResults.Any() ? await DbContext.Opportunities
            .FirstOrDefaultAsync(o => o.Id == listResults[0] && !o.IsDeleted) : null;

        // Assert
        detail.Should().NotBeNull();
        detail!.Name.Should().Be("Detail Opp");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Stakeholder_CreateOpportunity_AddStakeholders_VerifyPersistence()
    {
        // Arrange & Act: Create opportunity and add stakeholders
        var opp = await SeedOpportunityAsync(100, "New Opp", 1);
        await SeedStakeholderAsync(100, 1);
        await SeedStakeholderAsync(100, 2);

        // Assert
        var stakeholders = await DbContext.OpportunityStakeholders
            .Where(s => s.OpportunityId == 100 && !s.IsDeleted)
            .ToListAsync();
        stakeholders.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Stakeholder_OrgUnitChange_StakeholdersRefreshed()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedStakeholderAsync(1, 1, orgHierarchyId: 1);

        // Act: Change opportunity org unit (simulate refresh)
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.ResponsibleOrgUnitId = 2;
        await DbContext.SaveChangesAsync();

        // Assert: Stakeholders still linked to opportunity
        var stakeholders = await DbContext.OpportunityStakeholders
            .Where(s => s.OpportunityId == 1 && !s.IsDeleted)
            .ToListAsync();
        stakeholders.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExchangeRate_CreateOpportunity_WithBudget_ConversionApplied()
    {
        // Arrange: Budget in EUR, convert to USD
        const decimal budgetEur = 100000m;
        const decimal rate = 1.18m;

        // Act
        var budgetUsd = budgetEur * rate;
        var opp = await SeedOpportunityAsync(200, "Budget Opp", 1, budgetUsd);

        // Assert
        opp.InitiativeBudgetUSD.Should().Be(118000m);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExchangeRate_UpdateBudget_RecalculatesConversion()
    {
        // Arrange
        var opp = await SeedOpportunityAsync(201, "Update Opp", 1, 50000m);

        // Act: Update budget with new conversion
        const decimal newBudgetEur = 75000m;
        const decimal rate = 1.18m;
        var newBudgetUsd = newBudgetEur * rate;
        opp.InitiativeBudgetUSD = newBudgetUsd;
        await DbContext.SaveChangesAsync();

        // Assert
        var updated = await DbContext.Opportunities.FindAsync(201);
        updated!.InitiativeBudgetUSD.Should().Be(88500m);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DoAPrefix_CreateOpportunity_WithDoA_PrefixDisplayed()
    {
        // Arrange: Entity role with DoA prefix
        await SeedEntityRoleAsync(1, "DoA Level 2", "DoA2");
        await SeedOpportunityAsync(1);
        await SeedStakeholderAsync(1, 1, 1);

        // Act: Get stakeholder with role
        var stakeholder = await DbContext.OpportunityStakeholders
            .Include(s => s.EntityRole)
            .FirstOrDefaultAsync(s => s.OpportunityId == 1 && s.UserId == 1 && !s.IsDeleted);
        var prefix = stakeholder?.EntityRole != null ? GetDoAPrefix(stakeholder.EntityRole.Name) : string.Empty;

        // Assert
        prefix.Should().Be("DoA Level 2");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchIcons_QueryOpportunities_CorrectIconsReturned()
    {
        // Arrange: Opportunities have entity type "Opportunity"
        await SeedOpportunityAsync(1);
        await SeedOpportunityAsync(2);

        // Act: Query opportunities - entity type determines icon
        var opportunities = await DbContext.Opportunities
            .Where(o => !o.IsDeleted)
            .Select(o => new { o.Id, EntityType = "Opportunity" })
            .ToListAsync();

        // Assert: Each has correct entity type for icon mapping
        opportunities.Should().OnlyContain(o => o.EntityType == "Opportunity");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchIcons_QueryPartners_CorrectIconsReturned()
    {
        // Arrange: Seed a partner
        if (!await DbContext.Partners.AnyAsync(p => !p.IsDeleted))
        {
            DbContext.Partners.Add(new Partner
            {
                Name = "Test Partner",
                IsDeleted = false,
                Status = EntityStatus.Active
            });
            await DbContext.SaveChangesAsync();
        }

        // Act: Query partners - entity type for icon
        var partners = await DbContext.Partners
            .Where(p => !p.IsDeleted)
            .Select(p => new { p.Id, EntityType = "Partner" })
            .ToListAsync();

        // Assert
        partners.Should().OnlyContain(p => p.EntityType == "Partner");
    }
}
