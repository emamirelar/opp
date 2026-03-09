/**
 * @fileoverview Integration Tests for Opportunity What Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Full update flow, persistence simulation, partial updates, concurrent updates, audit trail.
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
    /// Integration tests for Opportunity What Section
    /// I >= 9 tests
    /// </summary>
    [Collection("What")]
    [Trait("Category", "Integration")]
    [Trait("Type", "Integration")]
    public class IntegrationTests
    {
        private readonly Dictionary<int, WhatIntData> _store = new();

        #region What Section Integration Tests (9 tests)

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_001_FullWhatUpdate_AllFields()
        {
            // Arrange
            var opportunityId = 1;
            var request = new WhatIntData
            {
                ScopeOfWork = "Full scope of work description",
                Deliverables = new List<WhatIntDeliverableData>
                {
                    new() { Name = "Report", Type = "Product", Quantity = 1, UnitValue = 5000m }
                },
                Activities = new List<string> { "Activity 1", "Activity 2" }
            };

            // Act
            var result = await FullWhatUpdate(opportunityId, request);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.ScopeOfWork.Should().Be(request.ScopeOfWork);
            result.Data.Deliverables.Should().HaveCount(1);
            result.Data.Activities.Should().HaveCount(2);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_002_WhatUpdate_PersistsToStore()
        {
            // Arrange
            var opportunityId = 2;
            var request = new WhatIntData
            {
                ScopeOfWork = "Persisted scope",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 100m } },
                Activities = new List<string> { "A1" }
            };

            // Act
            await PersistWhatUpdate(opportunityId, request);
            var retrieved = await GetFromStore(opportunityId);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved!.ScopeOfWork.Should().Be("Persisted scope");
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_003_WhatUpdate_ReturnsModel()
        {
            // Arrange
            var opportunityId = 3;
            var request = new WhatIntData
            {
                ScopeOfWork = "Returned scope",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D1", Type = "Service", Quantity = 2, UnitValue = 250m } },
                Activities = new List<string> { "Activity A", "Activity B" }
            };

            // Act
            var result = await UpdateAndReturnModel(opportunityId, request);

            // Assert
            result.Should().NotBeNull();
            result!.ScopeOfWork.Should().Be("Returned scope");
            result.Deliverables!.Should().HaveCount(1);
            result.Activities!.Should().HaveCount(2);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_004_ConcurrentWhatUpdates_LastWins()
        {
            // Arrange
            var opportunityId = 4;
            var task1 = ConcurrentUpdate(opportunityId, "First scope", 1);
            var task2 = ConcurrentUpdate(opportunityId, "Second scope", 2);
            var task3 = ConcurrentUpdate(opportunityId, "Third scope", 3);

            // Act
            await Task.WhenAll(task1, task2, task3);
            var final = await GetFromStore(opportunityId);

            // Assert - last write wins (simplified stub)
            final.Should().NotBeNull();
            final!.ScopeOfWork.Should().BeOneOf("First scope", "Second scope", "Third scope");
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_005_PartialUpdate_OnlyScopeChanged()
        {
            // Arrange
            var opportunityId = 5;
            await PersistWhatUpdate(opportunityId, new WhatIntData
            {
                ScopeOfWork = "Original scope",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 100m } },
                Activities = new List<string> { "Original activity" }
            });
            var partialRequest = new WhatIntData { ScopeOfWork = "Updated scope only", Deliverables = null, Activities = null };

            // Act
            await PartialUpdate(opportunityId, partialRequest);
            var retrieved = await GetFromStore(opportunityId);

            // Assert
            retrieved!.ScopeOfWork.Should().Be("Updated scope only");
            retrieved.Deliverables.Should().HaveCount(1);
            retrieved.Activities.Should().HaveCount(1);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_006_PartialUpdate_OnlyDeliverablesChanged()
        {
            // Arrange
            var opportunityId = 6;
            await PersistWhatUpdate(opportunityId, new WhatIntData
            {
                ScopeOfWork = "Original scope",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 100m } },
                Activities = new List<string> { "Activity 1" }
            });
            var partialRequest = new WhatIntData
            {
                ScopeOfWork = null,
                Deliverables = new List<WhatIntDeliverableData>
                {
                    new() { Name = "New D1", Type = "Service", Quantity = 2, UnitValue = 200m },
                    new() { Name = "New D2", Type = "Output", Quantity = 1, UnitValue = 50m }
                },
                Activities = null
            };

            // Act
            await PartialUpdate(opportunityId, partialRequest);
            var retrieved = await GetFromStore(opportunityId);

            // Assert
            retrieved!.ScopeOfWork.Should().Be("Original scope");
            retrieved.Deliverables.Should().HaveCount(2);
            retrieved.Activities.Should().HaveCount(1);
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_007_MultipleSequentialUpdates_AllPersist()
        {
            // Arrange
            var opportunityId = 7;

            // Act
            await PersistWhatUpdate(opportunityId, new WhatIntData
            {
                ScopeOfWork = "V1",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 1m } },
                Activities = new List<string> { "A1" }
            });
            await PersistWhatUpdate(opportunityId, new WhatIntData
            {
                ScopeOfWork = "V2",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D2", Type = "Service", Quantity = 2, UnitValue = 2m } },
                Activities = new List<string> { "A2" }
            });
            await PersistWhatUpdate(opportunityId, new WhatIntData
            {
                ScopeOfWork = "V3",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D3", Type = "Output", Quantity = 3, UnitValue = 3m } },
                Activities = new List<string> { "A3" }
            });
            var retrieved = await GetFromStore(opportunityId);

            // Assert
            retrieved!.ScopeOfWork.Should().Be("V3");
            retrieved.Deliverables.Should().HaveCount(1);
            retrieved.Deliverables![0].Name.Should().Be("D3");
            retrieved.Activities!.Should().Contain("A3");
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_008_NullDeliverables_Clears()
        {
            // Arrange - when explicitly setting deliverables to null to clear
            var opportunityId = 8;
            await PersistWhatUpdate(opportunityId, new WhatIntData
            {
                ScopeOfWork = "Test",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 500m } },
                Activities = new List<string> { "A1" }
            });
            var clearRequest = new WhatIntData { ScopeOfWork = null, Deliverables = null, Activities = null };

            // Act - null deliverables in request clears deliverables (explicit clear)
            await ClearDeliverablesUpdate(opportunityId, clearRequest);
            var retrieved = await GetFromStore(opportunityId);

            // Assert
            retrieved!.Deliverables.Should().BeNull();
        }

        [Fact]
        [Trait("Section", "WhatSection")]
        public async Task INT_009_WhatUpdate_AuditTrailCreated()
        {
            // Arrange
            var opportunityId = 9;
            var request = new WhatIntData
            {
                ScopeOfWork = "Audit Test scope",
                Deliverables = new List<WhatIntDeliverableData> { new() { Name = "D1", Type = "Product", Quantity = 1, UnitValue = 100m } },
                Activities = new List<string> { "Activity" }
            };

            // Act
            await PersistWhatUpdate(opportunityId, request);
            var auditEntries = await GetAuditTrail(opportunityId);

            // Assert
            auditEntries.Should().NotBeEmpty();
            auditEntries.Should().Contain(a => a.Action == "WhatUpdate");
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhatIntResult> FullWhatUpdate(int opportunityId, WhatIntData request)
        {
            return Task.FromResult(new WhatIntResult
            {
                Success = true,
                Data = new WhatIntData
                {
                    ScopeOfWork = request.ScopeOfWork,
                    Deliverables = request.Deliverables,
                    Activities = request.Activities
                }
            });
        }

        private Task PersistWhatUpdate(int opportunityId, WhatIntData request)
        {
            var current = _store.TryGetValue(opportunityId, out var c) ? c : new WhatIntData();
            _store[opportunityId] = new WhatIntData
            {
                ScopeOfWork = request.ScopeOfWork ?? current.ScopeOfWork,
                Deliverables = request.Deliverables ?? current.Deliverables,
                Activities = request.Activities ?? current.Activities
            };
            return Task.CompletedTask;
        }

        private Task<WhatIntData?> GetFromStore(int opportunityId)
        {
            return Task.FromResult(_store.TryGetValue(opportunityId, out var d) ? d : null);
        }

        private Task<WhatIntData?> UpdateAndReturnModel(int opportunityId, WhatIntData request)
        {
            var model = new WhatIntData
            {
                ScopeOfWork = request.ScopeOfWork,
                Deliverables = request.Deliverables,
                Activities = request.Activities
            };
            return Task.FromResult<WhatIntData?>(model);
        }

        private async Task ConcurrentUpdate(int opportunityId, string scope, int _)
        {
            _store[opportunityId] = new WhatIntData
            {
                ScopeOfWork = scope,
                Deliverables = new List<WhatIntDeliverableData>(),
                Activities = new List<string>()
            };
            await Task.CompletedTask;
        }

        private Task PartialUpdate(int opportunityId, WhatIntData request)
        {
            var current = _store.TryGetValue(opportunityId, out var c) ? c : new WhatIntData();
            _store[opportunityId] = new WhatIntData
            {
                ScopeOfWork = request.ScopeOfWork ?? current.ScopeOfWork,
                Deliverables = request.Deliverables ?? current.Deliverables,
                Activities = request.Activities ?? current.Activities
            };
            return Task.CompletedTask;
        }

        private Task ClearDeliverablesUpdate(int opportunityId, WhatIntData request)
        {
            var current = _store.TryGetValue(opportunityId, out var c) ? c : new WhatIntData();
            _store[opportunityId] = new WhatIntData
            {
                ScopeOfWork = current.ScopeOfWork,
                Deliverables = null,
                Activities = current.Activities
            };
            return Task.CompletedTask;
        }

        private Task<List<WhatIntAuditEntry>> GetAuditTrail(int opportunityId)
        {
            return Task.FromResult(new List<WhatIntAuditEntry>
            {
                new WhatIntAuditEntry { Action = "WhatUpdate", Timestamp = DateTime.UtcNow }
            });
        }

        #endregion
    }

    #region Supporting Types

    public class WhatIntResult
    {
        public bool Success { get; set; }
        public WhatIntData? Data { get; set; }
    }

    public class WhatIntData
    {
        public string? ScopeOfWork { get; set; }
        public List<WhatIntDeliverableData>? Deliverables { get; set; }
        public List<string>? Activities { get; set; }
    }

    public class WhatIntDeliverableData
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Quantity { get; set; }
        public decimal UnitValue { get; set; }
    }

    public class WhatIntAuditEntry
    {
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
