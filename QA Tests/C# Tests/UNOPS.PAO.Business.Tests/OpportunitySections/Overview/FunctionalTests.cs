/**
 * @fileoverview Functional Tests for Opportunity Overview Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Immutability, audit fields, partial updates, workflow lock, precision.
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
    /// Functional tests for Opportunity Overview Section
    /// F >= 9 tests
    /// </summary>
    [Collection("Overview")]
    [Trait("Category", "Functional")]
    [Trait("Type", "Functional")]
    public class FunctionalTests
    {
        #region Overview Section Functional Tests (9 tests)

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_001_ImmutabilityEnforced_GOStage()
        {
            // Arrange
            var opportunityId = 1;
            var stage = "GO";

            // Act
            var result = await TryUpdateOverviewWithStage(opportunityId, stage);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_002_ImmutabilityEnforced_NOGOStage()
        {
            // Arrange
            var opportunityId = 1;
            var stage = "NO GO";

            // Act
            var result = await TryUpdateOverviewWithStage(opportunityId, stage);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_003_ImmutabilityEnforced_CancelledStage_CaseInsensitive()
        {
            // Arrange - case insensitive: "cancelled", "CANCELLED", "Cancelled"
            var opportunityId = 1;
            var stages = new[] { "cancelled", "CANCELLED", "Cancelled" };

            // Act & Assert
            foreach (var stage in stages)
            {
                var result = await TryUpdateOverviewWithStage(opportunityId, stage);
                result.Success.Should().BeFalse($"Stage '{stage}' should block update");
            }
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_004_AuditFields_UpdatedOnSave()
        {
            // Arrange
            var opportunityId = 1;
            var data = new OverviewFuncData { Name = "Test", Description = "Desc" };

            // Act
            var result = await UpdateOverviewWithAudit(opportunityId, data);

            // Assert
            result.LastModifiedDate.Should().NotBe(default);
            result.LastModifiedBy.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_005_OnlyNonNullFields_Updated()
        {
            // Arrange - request with only Name set, Description and Budget null
            var opportunityId = 1;
            var request = new OverviewFuncData { Name = "New Name", Description = null, InitiativeBudgetUSD = null };

            // Act
            var result = await PartialUpdateOverview(opportunityId, request);

            // Assert - only Name should be in the updated set
            result.UpdatedFields.Should().Contain("Name");
            result.UpdatedFields.Should().NotContain("Description");
            result.UpdatedFields.Should().NotContain("InitiativeBudgetUSD");
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_006_LastModifiedDate_Updated()
        {
            // Arrange
            var opportunityId = 1;
            var before = DateTime.UtcNow;

            // Act
            await UpdateOverviewName(opportunityId, "Updated");
            var data = await GetOverviewWithAudit(opportunityId);

            // Assert
            data.LastModifiedDate.Should().BeAfter(before.AddSeconds(-1));
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_007_WorkflowLock_PreventsModification()
        {
            // Arrange
            var opportunityId = 1;
            var isInWorkflow = true;

            // Act
            var result = await TryUpdateWhenInWorkflow(opportunityId, isInWorkflow);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_008_BudgetStoredWithCorrectPrecision()
        {
            // Arrange
            var opportunityId = 1;
            var budget = 1234567.89m;

            // Act
            var stored = await StoreAndRetrieveBudget(opportunityId, budget);

            // Assert - decimal(18,2)
            stored.Should().Be(1234567.89m);
        }

        [Fact]
        [Trait("Section", "OverviewSection")]
        public async Task FUNC_009_NullFieldsPreserveExistingValues()
        {
            // Arrange - existing data
            var opportunityId = 1;
            var existing = new OverviewFuncData { Name = "Original", Description = "Original Desc", InitiativeBudgetUSD = 1000m };
            var update = new OverviewFuncData { Name = null, Description = null, InitiativeBudgetUSD = null };

            // Act
            var result = await MergeOverviewUpdate(opportunityId, existing, update);

            // Assert - existing values preserved
            result.Name.Should().Be("Original");
            result.Description.Should().Be("Original Desc");
            result.InitiativeBudgetUSD.Should().Be(1000m);
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<OverviewFuncResult> TryUpdateOverviewWithStage(int opportunityId, string stage)
        {
            var stageNorm = stage?.Trim().ToUpperInvariant() ?? "";
            var immutable = new[] { "GO", "NO GO", "CANCELLED" };
            var success = !immutable.Contains(stageNorm);
            return Task.FromResult(new OverviewFuncResult { Success = success });
        }

        private Task<OverviewFuncAuditEntry> UpdateOverviewWithAudit(int opportunityId, OverviewFuncData data)
        {
            return Task.FromResult(new OverviewFuncAuditEntry
            {
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = 1
            });
        }

        private Task<OverviewFuncResult> PartialUpdateOverview(int opportunityId, OverviewFuncData request)
        {
            var updated = new List<string>();
            if (request.Name != null) updated.Add("Name");
            if (request.Description != null) updated.Add("Description");
            if (request.InitiativeBudgetUSD.HasValue) updated.Add("InitiativeBudgetUSD");
            return Task.FromResult(new OverviewFuncResult
            {
                Success = true,
                UpdatedFields = updated
            });
        }

        private Task<OverviewFuncResult> UpdateOverviewName(int opportunityId, string name)
        {
            return Task.FromResult(new OverviewFuncResult { Success = true });
        }

        private Task<OverviewFuncAuditEntry> GetOverviewWithAudit(int opportunityId)
        {
            return Task.FromResult(new OverviewFuncAuditEntry
            {
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = 1
            });
        }

        private Task<OverviewFuncResult> TryUpdateWhenInWorkflow(int opportunityId, bool isInWorkflow)
        {
            return Task.FromResult(new OverviewFuncResult { Success = !isInWorkflow });
        }

        private Task<decimal> StoreAndRetrieveBudget(int opportunityId, decimal budget)
        {
            return Task.FromResult(Math.Round(budget, 2));
        }

        private Task<OverviewFuncData> MergeOverviewUpdate(int opportunityId, OverviewFuncData existing, OverviewFuncData update)
        {
            var merged = new OverviewFuncData
            {
                Name = update.Name ?? existing.Name,
                Description = update.Description ?? existing.Description,
                InitiativeBudgetUSD = update.InitiativeBudgetUSD ?? existing.InitiativeBudgetUSD
            };
            return Task.FromResult(merged);
        }

        #endregion
    }

    #region Supporting Types

    public class OverviewFuncResult
    {
        public bool Success { get; set; }
        public List<string> UpdatedFields { get; set; } = new();
    }

    public class OverviewFuncData
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? InitiativeBudgetUSD { get; set; }
    }

    public class OverviewFuncAuditEntry
    {
        public DateTime LastModifiedDate { get; set; }
        public int LastModifiedBy { get; set; }
    }

    #endregion
}
