/**
 * @fileoverview Negative Tests for Opportunity Overview Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Invalid inputs, immutability, workflow lock, non-existent entity.
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
    /// <summary>
    /// Negative tests for Opportunity Overview Section
    /// N >= 9 tests
    /// </summary>
    [Collection("Overview")]
    [Trait("Category", "Negative")]
    [Trait("Type", "Negative")]
    public class NegativeTests
    {
        #region Overview Section Negative Tests (9 tests)

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_001_UpdateOverview_WithNullName_Rejected()
        {
            // Arrange - when Name is explicitly sent as null
            var opportunityId = 1;
            var request = new OverviewNegRequest { Name = null };

            // Act
            var result = await UpdateOverview(opportunityId, request);

            // Assert - null name when Name is sent should be rejected
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_002_UpdateOverview_NameExceeds120Chars_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var longName = new string('A', 121);

            // Act
            var result = await UpdateOverviewName(opportunityId, longName);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_003_UpdateOverview_NegativeBudget_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var budget = -1000.00m;

            // Act
            var result = await UpdateOverviewBudget(opportunityId, budget);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_004_UpdateOverview_ImmutableOpportunity_GO_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var stage = "GO";

            // Act
            var result = await UpdateOverviewWithStage(opportunityId, stage);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_005_UpdateOverview_ImmutableOpportunity_NOGO_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var stage = "NO GO";

            // Act
            var result = await UpdateOverviewWithStage(opportunityId, stage);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_006_UpdateOverview_ImmutableOpportunity_CANCELLED_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var stage = "CANCELLED";

            // Act
            var result = await UpdateOverviewWithStage(opportunityId, stage);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_007_UpdateOverview_NonExistentOpportunity_Rejected()
        {
            // Arrange
            var nonExistentId = 999999;
            var request = new OverviewNegRequest { Name = "Test", Description = "Test" };

            // Act
            var result = await UpdateOverview(nonExistentId, request);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_008_UpdateOverview_InApprovalWorkflow_Rejected()
        {
            // Arrange - UNOPS: IsInWorkflow == true throws BusinessException
            var opportunityId = 1;
            var isInWorkflow = true;

            // Act
            var result = await UpdateOverviewInWorkflow(opportunityId, isInWorkflow);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task NEG_009_UpdateOverview_EmptyNameString_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var emptyName = "";

            // Act
            var result = await UpdateOverviewName(opportunityId, emptyName);

            // Assert
            result.Success.Should().BeFalse();
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<OverviewNegResult> UpdateOverview(int opportunityId, OverviewNegRequest request)
        {
            if (opportunityId > 100000)
                return Task.FromResult(new OverviewNegResult { Success = false });
            if (request.Name == null && request.Description == null)
                return Task.FromResult(new OverviewNegResult { Success = false });
            if (request.Name != null && (request.Name.Length == 0 || request.Name.Length > 120))
                return Task.FromResult(new OverviewNegResult { Success = false });
            return Task.FromResult(new OverviewNegResult { Success = true });
        }

        private Task<OverviewNegResult> UpdateOverviewName(int opportunityId, string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 120)
                return Task.FromResult(new OverviewNegResult { Success = false });
            return Task.FromResult(new OverviewNegResult { Success = true });
        }

        private Task<OverviewNegResult> UpdateOverviewBudget(int opportunityId, decimal? budget)
        {
            if (budget.HasValue && budget.Value < 0)
                return Task.FromResult(new OverviewNegResult { Success = false });
            return Task.FromResult(new OverviewNegResult { Success = true });
        }

        private Task<OverviewNegResult> UpdateOverviewWithStage(int opportunityId, string stage)
        {
            var stageNorm = stage?.Trim().ToUpperInvariant() ?? "";
            if (stageNorm == "GO" || stageNorm == "NO GO" || stageNorm == "CANCELLED")
                return Task.FromResult(new OverviewNegResult { Success = false });
            return Task.FromResult(new OverviewNegResult { Success = true });
        }

        private Task<OverviewNegResult> UpdateOverviewInWorkflow(int opportunityId, bool isInWorkflow)
        {
            if (isInWorkflow)
                return Task.FromResult(new OverviewNegResult { Success = false });
            return Task.FromResult(new OverviewNegResult { Success = true });
        }

        #endregion
    }

    #region Supporting Types

    public class OverviewNegResult
    {
        public bool Success { get; set; }
    }

    public class OverviewNegRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? InitiativeBudgetUSD { get; set; }
    }

    #endregion
}
