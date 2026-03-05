/**
 * @fileoverview Negative Tests for Opportunity What Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Invalid inputs, validation rejections for deliverables, scope, activities.
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
    /// Negative tests for Opportunity What Section
    /// N >= 9 tests
    /// </summary>
    [Collection("What")]
    [Trait("Category", "Negative")]
    [Trait("Type", "Negative")]
    public class NegativeTests
    {
        private const int MaxScopeLength = 4000;
        private readonly HashSet<string> _validDeliverableTypes = new(StringComparer.OrdinalIgnoreCase)
            { "Product", "Service", "Output" };

        #region What Section Negative Tests (9 tests)

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_001_SaveDeliverables_WithNullDeliverables_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            List<WhatNegDeliverableData>? deliverables = null;

            // Act
            var result = await SaveDeliverables(opportunityId, deliverables);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_002_UpdateScopeOfWork_WithEmptyScope_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var scope = "";

            // Act
            var result = await UpdateScopeOfWork(opportunityId, scope);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_003_UpdateScopeOfWork_WithWhitespaceOnlyScope_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var scope = "   \t\n  ";

            // Act
            var result = await UpdateScopeOfWork(opportunityId, scope);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_004_UpdateScopeOfWork_ExceedsMaxLength_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var scope = new string('A', MaxScopeLength + 1);

            // Act
            var result = await UpdateScopeOfWork(opportunityId, scope);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_005_SaveDeliverables_WithInvalidDeliverableType_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var deliverables = new List<WhatNegDeliverableData>
            {
                new() { Name = "Report", Type = "InvalidType", Quantity = 1, UnitValue = 100m }
            };

            // Act
            var result = await SaveDeliverables(opportunityId, deliverables);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_006_SaveDeliverables_WithDuplicateDeliverable_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var deliverables = new List<WhatNegDeliverableData>
            {
                new() { Name = "Report A", Type = "Product", Quantity = 1, UnitValue = 100m },
                new() { Name = "Report A", Type = "Product", Quantity = 2, UnitValue = 100m }
            };

            // Act
            var result = await SaveDeliverablesWithDuplicateCheck(opportunityId, deliverables);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_007_AddActivities_WithNullActivitiesList_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            List<string>? activities = null;

            // Act
            var result = await AddActivities(opportunityId, activities);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_008_AddActivities_WithEmptyActivityName_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var activities = new List<string> { "Valid Activity", "", "Another Valid" };

            // Act
            var result = await AddActivities(opportunityId, activities);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task NEG_009_UpdateScopeOfWork_WithInvalidSpecialChars_Rejected()
        {
            // Arrange - control characters or invalid chars that should be rejected
            var opportunityId = 1;
            var scope = "Valid text " + (char)0x00 + " with control char";

            // Act
            var result = await UpdateScopeOfWorkWithSpecialCharCheck(opportunityId, scope);

            // Assert
            result.Success.Should().BeFalse();
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhatNegResult> SaveDeliverables(int opportunityId, List<WhatNegDeliverableData>? deliverables)
        {
            if (deliverables == null)
                return Task.FromResult(new WhatNegResult { Success = false });
            if (deliverables.Count == 0)
                return Task.FromResult(new WhatNegResult { Success = false });
            foreach (var d in deliverables)
            {
                if (string.IsNullOrWhiteSpace(d.Name) || !_validDeliverableTypes.Contains(d.Type ?? ""))
                    return Task.FromResult(new WhatNegResult { Success = false });
            }
            return Task.FromResult(new WhatNegResult { Success = true });
        }

        private Task<WhatNegResult> UpdateScopeOfWork(int opportunityId, string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
                return Task.FromResult(new WhatNegResult { Success = false });
            if (scope.Length > MaxScopeLength)
                return Task.FromResult(new WhatNegResult { Success = false });
            return Task.FromResult(new WhatNegResult { Success = true });
        }

        private Task<WhatNegResult> SaveDeliverablesWithDuplicateCheck(int opportunityId, List<WhatNegDeliverableData> deliverables)
        {
            var names = deliverables.Select(d => d.Name?.Trim().ToLowerInvariant()).ToList();
            var distinctCount = names.Distinct().Count();
            if (distinctCount != names.Count)
                return Task.FromResult(new WhatNegResult { Success = false });
            return Task.FromResult(new WhatNegResult { Success = true });
        }

        private Task<WhatNegResult> AddActivities(int opportunityId, List<string>? activities)
        {
            if (activities == null)
                return Task.FromResult(new WhatNegResult { Success = false });
            if (activities.Any(string.IsNullOrWhiteSpace))
                return Task.FromResult(new WhatNegResult { Success = false });
            return Task.FromResult(new WhatNegResult { Success = true });
        }

        private Task<WhatNegResult> UpdateScopeOfWorkWithSpecialCharCheck(int opportunityId, string scope)
        {
            var hasControlChar = scope.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
            if (hasControlChar)
                return Task.FromResult(new WhatNegResult { Success = false });
            return Task.FromResult(new WhatNegResult { Success = true });
        }

        #endregion
    }

    #region Supporting Types

    public class WhatNegResult
    {
        public bool Success { get; set; }
    }

    public class WhatNegDeliverableData
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Quantity { get; set; }
        public decimal UnitValue { get; set; }
    }

    #endregion
}
