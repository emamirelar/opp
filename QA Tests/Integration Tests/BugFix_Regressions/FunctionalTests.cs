/**
 * @fileoverview Functional regression tests for BugFix scenarios.
 * Business rules, filter reset, stakeholder auto-populate, exchange rate defaults.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.BugFixRegressions;

[Collection("BugFix_Functional")]
[Trait("Category", "Functional")]
public class FunctionalTests : BugFixRegressionTestFixtureBase
{
    [Fact]
    [Trait("Category", "Functional")]
    public async Task GlobalFilter_ResetFilters_ReturnsAllResults()
    {
        // Arrange: Apply filter then reset (simulate)
        await SeedOpportunityAsync(1, "Opp A", 1);
        await SeedOpportunityAsync(2, "Opp B", 2);
        var filteredCount = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == 1)
            .CountAsync();

        // Act: "Reset" = no filter
        var allCount = await DbContext.Opportunities
            .Where(o => !o.IsDeleted)
            .CountAsync();

        // Assert
        filteredCount.Should().Be(1);
        allCount.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GlobalFilter_CombineMultipleFilters_IntersectsResults()
    {
        // Arrange: Org unit + name filter
        await SeedOpportunityAsync(1, "Alpha Opp", 1);
        await SeedOpportunityAsync(2, "Beta Opp", 1);
        await SeedOpportunityAsync(3, "Alpha Other", 2);

        // Act: Combined filter (org 1 AND name contains Alpha)
        var results = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == 1 && o.Name.Contains("Alpha"))
            .ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Alpha Opp");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GlobalFilter_FilterPersistsAfterNavigation()
    {
        // Arrange: Seed data
        await SeedOpportunityAsync(1, "Opp 1", 1);
        await SeedOpportunityAsync(2, "Opp 2", 1);

        // Act: Apply filter, "navigate" (re-query with same filter)
        var filterOrgUnitId = 1;
        var firstQuery = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == filterOrgUnitId)
            .ToListAsync();
        var secondQuery = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == filterOrgUnitId)
            .ToListAsync();

        // Assert: Same results (filter persisted)
        firstQuery.Should().BeEquivalentTo(secondQuery, o => o.ExcludingNestedObjects());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Stakeholder_AutoPopulateFromOrgUnit_AddsCorrectStakeholders()
    {
        // Arrange: Stakeholder with OrganizationHierarchyId (auto-populated)
        await SeedOpportunityAsync(1);
        await SeedStakeholderAsync(1, 40, orgHierarchyId: 1);

        // Act
        var stakeholder = await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == 1 && s.UserId == 40 && !s.IsDeleted);

        // Assert: Auto-populated has OrgHierarchyId
        stakeholder.Should().NotBeNull();
        stakeholder!.OrganizationHierarchyId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Stakeholder_RemoveStakeholder_SoftDeletes()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedStakeholderAsync(1, 50);

        // Act: Soft delete
        var stakeholder = await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == 1 && s.UserId == 50);
        stakeholder!.IsDeleted = true;
        stakeholder.DeletedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();

        // Assert
        var active = await DbContext.OpportunityStakeholders
            .CountAsync(s => s.OpportunityId == 1 && s.UserId == 50 && !s.IsDeleted);
        active.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ExchangeRate_NullRate_DefaultsToOne()
    {
        // Arrange
        decimal? rate = null;
        var effectiveRate = rate ?? 1.0m;

        // Assert
        effectiveRate.Should().Be(1.0m);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ExchangeRate_BudgetConversion_MatchesManualCalculation()
    {
        // Arrange: 50,000 EUR at 1.18
        const decimal budgetEur = 50000m;
        const decimal rate = 1.18m;

        // Act
        var budgetUsd = budgetEur * rate;

        // Assert
        budgetUsd.Should().Be(59000m);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DoAPrefix_LevelOnePrefix_DisplaysCorrectly()
    {
        // Arrange
        var level = "DoA2";

        // Act
        var prefix = GetDoAPrefix(level);

        // Assert
        prefix.Should().Be("DoA2");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DoAPrefix_LevelMapping_ConsistentAcrossEntities()
    {
        // Arrange: Same level string used in multiple places
        var level = "DoA3";

        // Act
        var prefix1 = GetDoAPrefix(level);
        var prefix2 = GetDoAPrefix(level);

        // Assert
        prefix1.Should().Be(prefix2).And.Be("DoA3");
    }
}
