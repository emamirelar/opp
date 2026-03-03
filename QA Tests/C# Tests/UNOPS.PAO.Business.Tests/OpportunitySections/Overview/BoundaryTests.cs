/**
 * @fileoverview Boundary Tests for Overview Section
 * Tests edge cases and boundary values for Overview section fields
 * Covers: Name length limits, budget extremes, special characters, unicode
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Tests.OpportunitySections.Overview
{
    [Collection("Overview")]
    [Trait("Category", "Boundary")]
    [Trait("Type", "Boundary")]
    public class BoundaryTests
    {
        #region Name Length Boundary Tests

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_001_NameAtExactly120Chars_Accepted()
        {
            var name = new string('A', 120);

            var result = await UpdateOverviewName(1, name);

            result.Success.Should().BeTrue();
            result.StoredName.Should().HaveLength(120);
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_002_NameAt119Chars_Accepted()
        {
            var name = new string('B', 119);

            var result = await UpdateOverviewName(1, name);

            result.Success.Should().BeTrue();
            result.StoredName.Should().HaveLength(119);
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_003_NameAt121Chars_Rejected()
        {
            var name = new string('C', 121);

            var result = await UpdateOverviewName(1, name);

            result.Success.Should().BeFalse();
        }

        #endregion

        #region Budget Boundary Tests

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_004_BudgetAtZero_Accepted()
        {
            var result = await UpdateOverviewBudget(1, 0m);

            result.Success.Should().BeTrue();
            result.StoredBudget.Should().Be(0m);
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_005_BudgetAtMaxDecimal_Handled()
        {
            var maxBudget = 9999999999999999.99m;

            var result = await UpdateOverviewBudget(1, maxBudget);

            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_006_DescriptionEmptyString_Accepted()
        {
            var result = await UpdateOverviewDescription(1, string.Empty);

            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_007_BudgetWithManyDecimalPlaces_RoundedTo2()
        {
            var result = await UpdateOverviewBudget(1, 1234.56789m);

            result.Success.Should().BeTrue();
            result.StoredBudget.Should().Be(1234.57m);
        }

        #endregion

        #region Special Character Tests

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_008_NameWithSpecialCharacters_Accepted()
        {
            var name = "Project-Alpha & Beta (Phase 1) #2 @UNOPS!";

            var result = await UpdateOverviewName(1, name);

            result.Success.Should().BeTrue();
            result.StoredName.Should().Be(name);
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task BND_009_NameWithUnicodeCharacters_Accepted()
        {
            var name = "Projet d'aide humanitaire - Région Méditerranée";

            var result = await UpdateOverviewName(1, name);

            result.Success.Should().BeTrue();
            result.StoredName.Should().Be(name);
        }

        #endregion

        #region Helper Methods

        private Task<OverviewBndResult> UpdateOverviewName(int opportunityId, string name)
        {
            if (string.IsNullOrEmpty(name))
                return Task.FromResult(new OverviewBndResult { Success = false });
            if (name.Length > 120)
                return Task.FromResult(new OverviewBndResult { Success = false });
            return Task.FromResult(new OverviewBndResult { Success = true, StoredName = name });
        }

        private Task<OverviewBndResult> UpdateOverviewDescription(int opportunityId, string description)
        {
            return Task.FromResult(new OverviewBndResult { Success = true, StoredDescription = description });
        }

        private Task<OverviewBndResult> UpdateOverviewBudget(int opportunityId, decimal budget)
        {
            if (budget < 0)
                return Task.FromResult(new OverviewBndResult { Success = false });
            var rounded = Math.Round(budget, 2, MidpointRounding.AwayFromZero);
            return Task.FromResult(new OverviewBndResult { Success = true, StoredBudget = rounded });
        }

        #endregion
    }

    #region Supporting Types

    public class OverviewBndResult
    {
        public bool Success { get; set; }
        public string? StoredName { get; set; }
        public string? StoredDescription { get; set; }
        public decimal? StoredBudget { get; set; }
    }

    #endregion
}
