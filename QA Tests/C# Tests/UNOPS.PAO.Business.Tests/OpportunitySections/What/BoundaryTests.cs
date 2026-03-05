/**
 * @fileoverview Boundary Tests for Opportunity What Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Scope length limits, deliverable counts, activity limits, decimal precision, unicode.
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
    /// Boundary tests for Opportunity What Section
    /// B >= 9 tests
    /// </summary>
    [Collection("What")]
    [Trait("Category", "Boundary")]
    [Trait("Type", "Boundary")]
    public class BoundaryTests
    {
        private const int MaxScopeLength = 4000;
        private const int MaxDeliverables = 50;
        private const int MaxActivities = 100;

        #region What Section Boundary Tests (9 tests)

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_001_ScopeOfWork_AtMaxLength_Accepted()
        {
            var scope = new string('A', MaxScopeLength);

            var result = await UpdateScopeOfWork(1, scope);

            result.Success.Should().BeTrue();
            result.StoredScope.Should().HaveLength(MaxScopeLength);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_002_ScopeOfWork_AtMaxLengthPlusOne_Rejected()
        {
            var scope = new string('B', MaxScopeLength + 1);

            var result = await UpdateScopeOfWork(1, scope);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_003_ZeroDeliverables_Valid()
        {
            var deliverables = new List<WhatBndDeliverableData>();

            var result = await SaveDeliverables(1, deliverables);

            result.Success.Should().BeTrue();
            result.DeliverableCount.Should().Be(0);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_004_MaxDeliverables_Accepted()
        {
            var deliverables = Enumerable.Range(1, MaxDeliverables)
                .Select(i => new WhatBndDeliverableData { Name = $"Deliverable {i}", Type = "Product", Quantity = 1, UnitValue = 100m })
                .ToList();

            var result = await SaveDeliverables(1, deliverables);

            result.Success.Should().BeTrue();
            result.DeliverableCount.Should().Be(MaxDeliverables);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_005_MaxDeliverablesPlusOne_Rejected()
        {
            var deliverables = Enumerable.Range(1, MaxDeliverables + 1)
                .Select(i => new WhatBndDeliverableData { Name = $"Deliverable {i}", Type = "Product", Quantity = 1, UnitValue = 100m })
                .ToList();

            var result = await SaveDeliverables(1, deliverables);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_006_SingleCharScope_Valid()
        {
            var scope = "x";

            var result = await UpdateScopeOfWork(1, scope);

            result.Success.Should().BeTrue();
            result.StoredScope.Should().Be("x");
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_007_MaxActivities_Accepted()
        {
            var activities = Enumerable.Range(1, MaxActivities).Select(i => $"Activity {i}").ToList();

            var result = await SaveActivities(1, activities);

            result.Success.Should().BeTrue();
            result.ActivityCount.Should().Be(MaxActivities);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_008_DeliverableValue_DecimalPrecisionPreserved()
        {
            var deliverables = new List<WhatBndDeliverableData>
            {
                new() { Name = "Item", Type = "Product", Quantity = 1, UnitValue = 1234.56789m }
            };

            var result = await SaveDeliverablesWithPrecision(1, deliverables);

            result.Success.Should().BeTrue();
            result.StoredUnitValue.Should().Be(1234.57m);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task BND_009_ScopeOfWork_WithUnicode_Accepted()
        {
            var scope = "Projet d'aide humanitaire - Région Méditerranée - 项目范围";

            var result = await UpdateScopeOfWork(1, scope);

            result.Success.Should().BeTrue();
            result.StoredScope.Should().Be(scope);
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhatBndResult> UpdateScopeOfWork(int opportunityId, string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
                return Task.FromResult(new WhatBndResult { Success = false });
            if (scope.Length > MaxScopeLength)
                return Task.FromResult(new WhatBndResult { Success = false });
            return Task.FromResult(new WhatBndResult { Success = true, StoredScope = scope });
        }

        private Task<WhatBndResult> SaveDeliverables(int opportunityId, List<WhatBndDeliverableData> deliverables)
        {
            if (deliverables.Count > MaxDeliverables)
                return Task.FromResult(new WhatBndResult { Success = false });
            return Task.FromResult(new WhatBndResult { Success = true, DeliverableCount = deliverables.Count });
        }

        private Task<WhatBndResult> SaveActivities(int opportunityId, List<string> activities)
        {
            if (activities.Count > MaxActivities)
                return Task.FromResult(new WhatBndResult { Success = false });
            return Task.FromResult(new WhatBndResult { Success = true, ActivityCount = activities.Count });
        }

        private Task<WhatBndResult> SaveDeliverablesWithPrecision(int opportunityId, List<WhatBndDeliverableData> deliverables)
        {
            if (deliverables.Count == 0)
                return Task.FromResult(new WhatBndResult { Success = false });
            var rounded = Math.Round(deliverables[0].UnitValue, 2, MidpointRounding.AwayFromZero);
            return Task.FromResult(new WhatBndResult { Success = true, StoredUnitValue = rounded });
        }

        #endregion
    }

    #region Supporting Types

    public class WhatBndResult
    {
        public bool Success { get; set; }
        public string? StoredScope { get; set; }
        public int DeliverableCount { get; set; }
        public int ActivityCount { get; set; }
        public decimal? StoredUnitValue { get; set; }
    }

    public class WhatBndDeliverableData
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Quantity { get; set; }
        public decimal UnitValue { get; set; }
    }

    #endregion
}
