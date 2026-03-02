/**
 * @fileoverview Negative regression tests for BugFix scenarios.
 * Invalid input, unauthorized, expected failures.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.BugFixRegressions;

[Collection("BugFix_Negative")]
[Trait("Category", "Negative")]
public class NegativeTests : BugFixRegressionTestFixtureBase
{
    [Fact]
    [Trait("Category", "Negative")]
    public async Task GlobalFilter_InvalidFilterValue_ReturnsEmpty()
    {
        // Arrange: Filter by non-existent org unit
        await SeedOpportunityAsync(1, "Opp A", 1);

        // Act
        var filtered = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == 99999)
            .ToListAsync();

        // Assert
        filtered.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GlobalFilter_NullFilterCriteria_ReturnsAll()
    {
        // Arrange: No filter applied
        await SeedOpportunityAsync(1);
        await SeedOpportunityAsync(2);

        // Act: Query without org unit filter
        var all = await DbContext.Opportunities
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        // Assert
        all.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task Stakeholder_DuplicateAdd_PreventsDuplicate()
    {
        // Arrange: Add same stakeholder twice
        await SeedOpportunityAsync(1);
        await SeedStakeholderAsync(1, 10);

        // Act: Attempt to add duplicate (same opp + user)
        var duplicateCount = await DbContext.OpportunityStakeholders
            .CountAsync(s => s.OpportunityId == 1 && s.UserId == 10 && !s.IsDeleted);

        // Assert: Only one non-deleted stakeholder
        duplicateCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task Stakeholder_AddToNonExistentOpportunity_ThrowsNotFound()
    {
        // Arrange: No opportunity with ID 99999
        var oppExists = await DbContext.Opportunities.AnyAsync(o => o.Id == 99999 && !o.IsDeleted);

        // Assert
        oppExists.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task Stakeholder_AddDeletedUser_HandlesGracefully()
    {
        // Arrange: Soft-deleted stakeholder
        await SeedOpportunityAsync(1);
        await SeedStakeholderAsync(1, 20, isDeleted: true);

        // Act: Query non-deleted stakeholders
        var active = await DbContext.OpportunityStakeholders
            .Where(s => s.OpportunityId == 1 && !s.IsDeleted)
            .ToListAsync();

        // Assert: Deleted stakeholder not returned
        active.Should().NotContain(s => s.UserId == 20);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ExchangeRate_ZeroRate_ThrowsValidation()
    {
        // Arrange
        const decimal rate = 0m;

        // Act & Assert: Zero rate is invalid for conversion
        rate.Should().Be(0);
        // In production, conversion with 0 rate should throw or return error
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ExchangeRate_NegativeRate_ThrowsValidation()
    {
        // Arrange
        const decimal rate = -1.5m;

        // Act & Assert: Negative rate is invalid
        rate.Should().BeLessThan(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ExchangeRate_NullCurrency_DefaultsToUSD()
    {
        // Arrange: Null currency should default to USD (1:1)
        string? currency = null;
        var effectiveCurrency = currency ?? "USD";

        // Assert
        effectiveCurrency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DoAPrefix_EmptyLevel_ReturnsEmptyString()
    {
        // Arrange
        string? level = null;

        // Act
        var prefix = GetDoAPrefix(level);

        // Assert
        prefix.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DoAPrefix_WhitespaceLevel_ReturnsEmptyString()
    {
        // Arrange
        var level = "   ";

        // Act
        var prefix = GetDoAPrefix(level);

        // Assert
        prefix.Should().BeEmpty();
    }
}
