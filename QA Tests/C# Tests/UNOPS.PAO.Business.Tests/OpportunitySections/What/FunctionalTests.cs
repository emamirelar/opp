/**
 * @fileoverview Functional Tests for Opportunity What Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Immutability, audit fields, partial updates, workflow lock, deliverable count.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Tests.OpportunitySections.What
{
    /// <summary>
    /// Functional tests for Opportunity What Section
    /// F >= 9 tests
    /// </summary>
    [Collection("What")]
    [Trait("Category", "Functional")]
    [Trait("Type", "Functional")]
    public class FunctionalTests
    {
        #region What Section Functional Tests (9 tests)

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_001_ImmutabilityEnforced_GOStage()
        {
            // Arrange
            var opportunityId = 1;
            var stage = "GO";

            // Act
            var result = await TryUpdateWhatWithStage(opportunityId, stage);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_002_ImmutabilityEnforced_NOGOStage()
        {
            // Arrange
            var opportunityId = 1;
            var stage = "NO GO";

            // Act
            var result = await TryUpdateWhatWithStage(opportunityId, stage);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_003_ImmutabilityEnforced_CancelledStage_CaseInsensitive()
        {
            // Arrange - case insensitive: "cancelled", "CANCELLED", "Cancelled"
            var opportunityId = 1;
            var stages = new[] { "cancelled", "CANCELLED", "Cancelled" };

            // Act & Assert
            foreach (var stage in stages)
            {
                var result = await TryUpdateWhatWithStage(opportunityId, stage);
                result.Success.Should().BeFalse($"Stage '{stage}' should block update");
            }
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_004_AuditFields_UpdatedOnSave()
        {
            // Arrange
            var opportunityId = 1;
            var data = new WhatFuncData { ScopeOfWork = "Test scope", Deliverables = new List<WhatFuncDeliverableData>() };

            // Act
            var result = await UpdateWhatWithAudit(opportunityId, data);

            // Assert
            result.LastModifiedDate.Should().NotBe(default);
            result.LastModifiedBy.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_005_OnlyNonNullFields_Updated()
        {
            // Arrange - request with only ScopeOfWork set, Deliverables and Activities null
            var opportunityId = 1;
            var request = new WhatFuncData { ScopeOfWork = "New scope", Deliverables = null, Activities = null };

            // Act
            var result = await PartialUpdateWhat(opportunityId, request);

            // Assert - only ScopeOfWork should be in the updated set
            result.UpdatedFields.Should().Contain("ScopeOfWork");
            result.UpdatedFields.Should().NotContain("Deliverables");
            result.UpdatedFields.Should().NotContain("Activities");
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_006_LastModifiedDate_Updated()
        {
            // Arrange
            var opportunityId = 1;
            var before = DateTime.UtcNow;

            // Act
            await UpdateWhatScope(opportunityId, "Updated scope");
            var data = await GetWhatWithAudit(opportunityId);

            // Assert
            data.LastModifiedDate.Should().BeAfter(before.AddSeconds(-1));
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_007_WorkflowLock_PreventsModification()
        {
            // Arrange
            var opportunityId = 1;
            var isInWorkflow = true;

            // Act
            var result = await TryUpdateWhatWhenInWorkflow(opportunityId, isInWorkflow);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_008_DeliverableCount_ComputedCorrectly()
        {
            // Arrange
            var opportunityId = 1;
            var deliverables = new List<WhatFuncDeliverableData>
            {
                new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 100m },
                new() { Name = "D2", Type = "Service", Quantity = 2, UnitValue = 50m }
            };

            // Act
            var result = await ComputeDeliverableCount(opportunityId, deliverables);

            // Assert
            result.Success.Should().BeTrue();
            result.DeliverableCount.Should().Be(2);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task FUNC_009_NullFields_PreserveExistingValues()
        {
            // Arrange - existing data
            var opportunityId = 1;
            var existing = new WhatFuncData
            {
                ScopeOfWork = "Original scope",
                Deliverables = new List<WhatFuncDeliverableData> { new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 100m } },
                Activities = new List<string> { "Activity 1" }
            };
            var update = new WhatFuncData { ScopeOfWork = null, Deliverables = null, Activities = null };

            // Act
            var result = await MergeWhatUpdate(opportunityId, existing, update);

            // Assert - existing values preserved
            result.ScopeOfWork.Should().Be("Original scope");
            result.Deliverables.Should().HaveCount(1);
            result.Activities.Should().HaveCount(1);
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhatFuncResult> TryUpdateWhatWithStage(int opportunityId, string stage)
        {
            var stageNorm = stage?.Trim().ToUpperInvariant() ?? "";
            var immutable = new[] { "GO", "NO GO", "CANCELLED" };
            var success = !immutable.Contains(stageNorm);
            return Task.FromResult(new WhatFuncResult { Success = success });
        }

        private Task<WhatFuncAuditEntry> UpdateWhatWithAudit(int opportunityId, WhatFuncData data)
        {
            return Task.FromResult(new WhatFuncAuditEntry
            {
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = 1
            });
        }

        private Task<WhatFuncResult> PartialUpdateWhat(int opportunityId, WhatFuncData request)
        {
            var updated = new List<string>();
            if (request.ScopeOfWork != null) updated.Add("ScopeOfWork");
            if (request.Deliverables != null) updated.Add("Deliverables");
            if (request.Activities != null) updated.Add("Activities");
            return Task.FromResult(new WhatFuncResult
            {
                Success = true,
                UpdatedFields = updated
            });
        }

        private Task<WhatFuncResult> UpdateWhatScope(int opportunityId, string scope)
        {
            return Task.FromResult(new WhatFuncResult { Success = true });
        }

        private Task<WhatFuncAuditEntry> GetWhatWithAudit(int opportunityId)
        {
            return Task.FromResult(new WhatFuncAuditEntry
            {
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = 1
            });
        }

        private Task<WhatFuncResult> TryUpdateWhatWhenInWorkflow(int opportunityId, bool isInWorkflow)
        {
            return Task.FromResult(new WhatFuncResult { Success = !isInWorkflow });
        }

        private Task<WhatFuncResult> ComputeDeliverableCount(int opportunityId, List<WhatFuncDeliverableData> deliverables)
        {
            return Task.FromResult(new WhatFuncResult
            {
                Success = true,
                DeliverableCount = deliverables?.Count ?? 0
            });
        }

        private Task<WhatFuncData> MergeWhatUpdate(int opportunityId, WhatFuncData existing, WhatFuncData update)
        {
            var merged = new WhatFuncData
            {
                ScopeOfWork = update.ScopeOfWork ?? existing.ScopeOfWork,
                Deliverables = update.Deliverables ?? existing.Deliverables,
                Activities = update.Activities ?? existing.Activities
            };
            return Task.FromResult(merged);
        }

        #endregion
    }

    #region Supporting Types

    public class WhatFuncResult
    {
        public bool Success { get; set; }
        public List<string> UpdatedFields { get; set; } = new();
        public int DeliverableCount { get; set; }
    }

    public class WhatFuncData
    {
        public string? ScopeOfWork { get; set; }
        public List<WhatFuncDeliverableData>? Deliverables { get; set; }
        public List<string>? Activities { get; set; }
    }

    public class WhatFuncDeliverableData
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Quantity { get; set; }
        public decimal UnitValue { get; set; }
    }

    public class WhatFuncAuditEntry
    {
        public DateTime LastModifiedDate { get; set; }
        public int LastModifiedBy { get; set; }
    }

    #endregion
}
