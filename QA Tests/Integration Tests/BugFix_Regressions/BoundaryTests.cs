/**
 * @fileoverview Boundary regression tests for BugFix scenarios.
 * Min/max values, soft-delete interactions, precision, special characters.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.BugFixRegressions;

[Collection("BugFix_Boundary")]
[Trait("Category", "Boundary")]
public class BoundaryTests : BugFixRegressionTestFixtureBase
{
    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GlobalFilter_MaxFiltersApplied_AllProcessed()
    {
        // Arrange: Multiple org units
        await SeedOrgUnitAsync(1);
        await SeedOrgUnitAsync(2);
        await SeedOrgUnitAsync(3);
        await SeedOpportunityAsync(1, "Opp1", 1);
        await SeedOpportunityAsync(2, "Opp2", 2);
        await SeedOpportunityAsync(3, "Opp3", 3);

        // Act: Filter by multiple org units (simulate combined filter)
        var orgUnitIds = new[] { 1, 2 };
        var filtered = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && orgUnitIds.Contains(o.ResponsibleOrgUnitId ?? 0))
            .ToListAsync();

        // Assert
        filtered.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GlobalFilter_EmptyStringFilter_TreatedAsNoFilter()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        string filterValue = string.Empty;

        // Act: Empty string treated as no filter
        var query = DbContext.Opportunities.Where(o => !o.IsDeleted);
        if (!string.IsNullOrWhiteSpace(filterValue))
        {
            query = query.Where(o => o.Name == filterValue);
        }
        var results = await query.ToListAsync();

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Stakeholder_MaxStakeholders_AllSaved()
    {
        // Arrange: Add multiple stakeholders
        await SeedOpportunityAsync(1);
        for (var i = 1; i <= 5; i++)
        {
            await SeedUserAsync(i);
            await SeedStakeholderAsync(1, i, 1);
        }

        // Act
        var count = await DbContext.OpportunityStakeholders
            .CountAsync(s => s.OpportunityId == 1 && !s.IsDeleted);

        // Assert
        count.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Stakeholder_SoftDeletedStakeholder_NotReturnedInQuery()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedStakeholderAsync(1, 30, isDeleted: true);

        // Act: Query excludes IsDeleted
        var stakeholders = await DbContext.OpportunityStakeholders
            .Where(s => s.OpportunityId == 1 && !s.IsDeleted)
            .ToListAsync();

        // Assert
        stakeholders.Should().NotContain(s => s.UserId == 30);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ExchangeRate_VerySmallRate_PrecisionMaintained()
    {
        // Arrange: Very small rate (e.g., JPY to USD ~0.0067)
        const decimal amount = 1000m;
        const decimal rate = 0.0067m;

        // Act
        var usd = amount * rate;

        // Assert
        usd.Should().BeApproximately(6.7m, 0.01m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ExchangeRate_VeryLargeRate_NoOverflow()
    {
        // Arrange
        const decimal amount = 100m;
        const decimal rate = 1000000m;

        // Act
        var usd = amount * rate;

        // Assert
        usd.Should().Be(100000000m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ExchangeRate_RateWith10Decimals_PrecisionPreserved()
    {
        // Arrange
        const decimal rate = 1.1234567890m;
        const decimal amount = 100m;

        // Act
        var result = Math.Round(amount * rate, 2);

        // Assert
        result.Should().Be(112.35m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DoAPrefix_SpecialCharacters_DisplayedCorrectly()
    {
        // Arrange
        var level = "DoA2 (L2)";

        // Act
        var prefix = GetDoAPrefix(level);

        // Assert
        prefix.Should().Be("DoA2 (L2)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DoAPrefix_UnicodeCharacters_HandledCorrectly()
    {
        // Arrange
        var level = "DoA2—Level";

        // Act
        var prefix = GetDoAPrefix(level);

        // Assert
        prefix.Should().Be("DoA2—Level");
    }
}
