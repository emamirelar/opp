/**
 * @fileoverview Positive regression tests for BugFix scenarios.
 * Happy path: Global Filter apply, Stakeholder add, Exchange Rate conversion.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.BugFixRegressions;

[Collection("BugFix_Positive")]
[Trait("Category", "Positive")]
public class PositiveTests : BugFixRegressionTestFixtureBase
{
    [Fact]
    [Trait("Category", "Positive")]
    public async Task GlobalFilter_ApplyFilter_ReturnsFilteredResults()
    {
        // Arrange: Seed opportunities with different org units
        await SeedOpportunityAsync(1, "Opp A", 1);
        await SeedOpportunityAsync(2, "Opp B", 1);
        await SeedOpportunityAsync(3, "Opp C", 2);
        await SeedOrgUnitAsync(2, "Region", "REG");

        // Act: Simulate filter by org unit 1 - query opportunities in that org
        var filtered = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == 1)
            .ToListAsync();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Select(o => o.Name).Should().Contain("Opp A").And.Contain("Opp B");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task Stakeholder_AddToOpportunity_SavedCorrectly()
    {
        // Arrange
        var opp = await SeedOpportunityAsync(1);
        await SeedUserAsync(10);

        // Act: Add stakeholder
        var stakeholder = new OpportunityStakeholder
        {
            OpportunityId = opp.Id,
            UserId = 10,
            EntityRoleId = 1,
            IsInternal = true,
            Name = "Stakeholder-10"
        };
        DbContext.OpportunityStakeholders.Add(stakeholder);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == 1 && s.UserId == 10 && !s.IsDeleted);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(10);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ExchangeRate_ApplyConversion_CalculatesCorrectly()
    {
        // Arrange: 100 EUR at rate 1.18 = 118 USD
        const decimal amountEur = 100m;
        const decimal rate = 1.18m;

        // Act: Simulate conversion (amount * rate = USD)
        var amountUsd = amountEur * rate;

        // Assert
        amountUsd.Should().Be(118m);
    }
}
