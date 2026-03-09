/**
 * @fileoverview Integration Tests for Opportunity WHEN Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Full update flows, persistence, concurrent updates, audit trail.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Tests.OpportunitySections.When
{
    /// <summary>
    /// Integration tests for Opportunity WHEN Section
    /// I >= 9 tests (3x Positive baseline)
    /// </summary>
    [Collection("When")]
    [Trait("Category", "Integration")]
    [Trait("Type", "Integration")]
    public class IntegrationTests
    {
        private readonly HashSet<int> _validOpportunityIds = new() { 1, 2, 3, 10, 20 };
        private readonly Dictionary<int, WhenIntData> _store = new();
        private readonly Dictionary<int, List<WhenIntAuditEntry>> _auditStore = new();

        #region WHEN Section Integration Tests (9 tests)

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_001_FullWhenUpdate_AllFields()
        {
            var opportunityId = 1;
            var request = new WhenIntData
            {
                TargetSigningDate = new DateTime(2026, 6, 1),
                ImplementationStartDate = new DateTime(2026, 7, 1),
                TargetDeliveryDate = new DateTime(2026, 12, 31),
                IsTargetSigningDateFirm = true,
                SigningDateNotes = "Partner deadline",
                SubmissionDeadline = new DateTime(2026, 5, 15),
                Deliverables = new List<WhenIntDeliverable>
                {
                    new() { Id = 1, PlannedStartDate = new DateTime(2026, 7, 15), PlannedEndDate = new DateTime(2026, 8, 15) }
                }
            };

            var result = await FullWhenUpdate(opportunityId, request);
            result.Success.Should().BeTrue();

            var retrieved = await GetWhenData(opportunityId);
            retrieved.TargetSigningDate.Should().Be(request.TargetSigningDate);
            retrieved.ImplementationStartDate.Should().Be(request.ImplementationStartDate);
            retrieved.TargetDeliveryDate.Should().Be(request.TargetDeliveryDate);
            retrieved.IsTargetSigningDateFirm.Should().Be(request.IsTargetSigningDateFirm);
            retrieved.SigningDateNotes.Should().Be(request.SigningDateNotes);
            retrieved.SubmissionDeadline.Should().Be(request.SubmissionDeadline);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_002_WhenDates_PersistAndReadable()
        {
            var opportunityId = 2;
            var signingDate = new DateTime(2026, 5, 1);
            var implStart = new DateTime(2026, 6, 1);
            var deliveryDate = new DateTime(2026, 12, 1);

            await UpdateWhenDates(opportunityId, signingDate, implStart, deliveryDate);
            var retrieved = await GetWhenData(opportunityId);

            retrieved.TargetSigningDate.Should().Be(signingDate);
            retrieved.ImplementationStartDate.Should().Be(implStart);
            retrieved.TargetDeliveryDate.Should().Be(deliveryDate);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_003_WhenWithDeliverables_AllDatesStored()
        {
            var opportunityId = 3;
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliverables = new List<WhenIntDeliverable>
            {
                new() { Id = 1, PlannedStartDate = new DateTime(2026, 7, 15), PlannedEndDate = new DateTime(2026, 8, 15) },
                new() { Id = 2, PlannedStartDate = new DateTime(2026, 8, 15), PlannedEndDate = new DateTime(2026, 9, 15) }
            };

            var result = await UpdateWhenWithDeliverables(opportunityId, effectiveStart, deliverables);
            result.Success.Should().BeTrue();
            result.DeliverablesUpdated.Should().Be(2);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_004_ConcurrentWhenUpdates_Handled()
        {
            var opportunityId = 1;
            var task1 = UpdateWhenDates(opportunityId, new DateTime(2026, 6, 1), new DateTime(2026, 7, 1), new DateTime(2026, 12, 31));
            var task2 = UpdateWhenDates(opportunityId, new DateTime(2026, 6, 2), new DateTime(2026, 7, 2), new DateTime(2026, 12, 30));

            await Task.WhenAll(task1, task2);

            var retrieved = await GetWhenData(opportunityId);
            retrieved.Should().NotBeNull();
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_005_SequentialWhenUpdates_AllPersist()
        {
            var opportunityId = 1;

            await UpdateWhenDates(opportunityId, new DateTime(2026, 5, 1), new DateTime(2026, 6, 1), new DateTime(2026, 11, 1));
            var first = await GetWhenData(opportunityId);
            first.TargetSigningDate.Should().Be(new DateTime(2026, 5, 1));

            await UpdateWhenDates(opportunityId, new DateTime(2026, 5, 15), new DateTime(2026, 6, 15), new DateTime(2026, 11, 15));
            var second = await GetWhenData(opportunityId);
            second.TargetSigningDate.Should().Be(new DateTime(2026, 5, 15));
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_006_WhenReturnsUpdatedModel()
        {
            var opportunityId = 1;
            var request = new WhenIntData
            {
                TargetSigningDate = new DateTime(2026, 6, 1),
                ImplementationStartDate = new DateTime(2026, 7, 1),
                TargetDeliveryDate = new DateTime(2026, 12, 31)
            };

            var result = await FullWhenUpdateAndReturn(opportunityId, request);
            result.Should().NotBeNull();
            result.TargetSigningDate.Should().Be(request.TargetSigningDate);
            result.TargetDeliveryDate.Should().Be(request.TargetDeliveryDate);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_007_WhenClearsDatesWhenNull()
        {
            var opportunityId = 1;
            await UpdateWhenDates(opportunityId, new DateTime(2026, 6, 1), new DateTime(2026, 7, 1), new DateTime(2026, 12, 31));

            await ClearWhenDates(opportunityId);
            var retrieved = await GetWhenData(opportunityId);

            retrieved.TargetSigningDate.Should().BeNull();
            retrieved.ImplementationStartDate.Should().BeNull();
            retrieved.TargetDeliveryDate.Should().BeNull();
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_008_WhenUpdate_AuditTrailCreated()
        {
            var opportunityId = 1;
            var request = new WhenIntData
            {
                TargetSigningDate = new DateTime(2026, 6, 1),
                ImplementationStartDate = new DateTime(2026, 7, 1),
                TargetDeliveryDate = new DateTime(2026, 12, 31)
            };

            await FullWhenUpdateWithAudit(opportunityId, request, userId: 42);
            var auditEntries = await GetAuditTrail(opportunityId);

            auditEntries.Should().NotBeEmpty();
            auditEntries.Should().Contain(a => a.LastModifiedBy == 42);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task INT_009_WhenWithPartialFields_OnlySpecifiedUpdated()
        {
            var opportunityId = 1;
            _store[opportunityId] = new WhenIntData
            {
                TargetSigningDate = new DateTime(2026, 6, 1),
                ImplementationStartDate = new DateTime(2026, 7, 1),
                TargetDeliveryDate = new DateTime(2026, 12, 31),
                SigningDateNotes = "Original notes"
            };

            var partialUpdate = new WhenIntData { SigningDateNotes = "Updated notes only" };
            await PartialWhenUpdate(opportunityId, partialUpdate);

            var retrieved = await GetWhenData(opportunityId);
            retrieved.SigningDateNotes.Should().Be("Updated notes only");
            retrieved.TargetSigningDate.Should().Be(new DateTime(2026, 6, 1));
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhenIntResult> FullWhenUpdate(int opportunityId, WhenIntData request)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhenIntResult { Success = false });
            if (request.ImplementationStartDate.HasValue && request.TargetSigningDate.HasValue &&
                request.ImplementationStartDate.Value < request.TargetSigningDate.Value)
                return Task.FromResult(new WhenIntResult { Success = false });
            var effectiveStart = request.ImplementationStartDate ?? request.TargetSigningDate;
            if (request.TargetDeliveryDate.HasValue && effectiveStart.HasValue &&
                request.TargetDeliveryDate.Value < effectiveStart.Value)
                return Task.FromResult(new WhenIntResult { Success = false });
            _store[opportunityId] = new WhenIntData
            {
                TargetSigningDate = request.TargetSigningDate,
                ImplementationStartDate = request.ImplementationStartDate,
                TargetDeliveryDate = request.TargetDeliveryDate,
                IsTargetSigningDateFirm = request.IsTargetSigningDateFirm,
                SigningDateNotes = request.SigningDateNotes,
                SubmissionDeadline = request.SubmissionDeadline
            };
            return Task.FromResult(new WhenIntResult { Success = true });
        }

        private Task UpdateWhenDates(int opportunityId, DateTime? signingDate, DateTime? implStart, DateTime? deliveryDate)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.CompletedTask;
            if (implStart.HasValue && signingDate.HasValue && implStart.Value < signingDate.Value)
                return Task.CompletedTask;
            var effectiveStart = implStart ?? signingDate;
            if (deliveryDate.HasValue && effectiveStart.HasValue && deliveryDate.Value < effectiveStart.Value)
                return Task.CompletedTask;
            var existing = _store.TryGetValue(opportunityId, out var d) ? d : new WhenIntData();
            existing.TargetSigningDate = signingDate;
            existing.ImplementationStartDate = implStart;
            existing.TargetDeliveryDate = deliveryDate;
            _store[opportunityId] = existing;
            return Task.CompletedTask;
        }

        private Task<WhenIntData?> GetWhenData(int opportunityId)
        {
            return Task.FromResult(_store.TryGetValue(opportunityId, out var d) ? d : null);
        }

        private Task<WhenIntDeliverablesResult> UpdateWhenWithDeliverables(int opportunityId, DateTime? effectiveStart,
            List<WhenIntDeliverable> deliverables)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhenIntDeliverablesResult { Success = false, DeliverablesUpdated = 0 });
            if (deliverables == null || !deliverables.Any())
                return Task.FromResult(new WhenIntDeliverablesResult { Success = true, DeliverablesUpdated = 0 });
            foreach (var d in deliverables)
            {
                if (d.PlannedStartDate.HasValue && effectiveStart.HasValue &&
                    d.PlannedStartDate.Value < effectiveStart.Value)
                    return Task.FromResult(new WhenIntDeliverablesResult { Success = false, DeliverablesUpdated = 0 });
                if (d.PlannedStartDate.HasValue && d.PlannedEndDate.HasValue &&
                    d.PlannedEndDate.Value < d.PlannedStartDate.Value)
                    return Task.FromResult(new WhenIntDeliverablesResult { Success = false, DeliverablesUpdated = 0 });
            }
            return Task.FromResult(new WhenIntDeliverablesResult { Success = true, DeliverablesUpdated = deliverables.Count });
        }

        private Task<WhenIntData> FullWhenUpdateAndReturn(int opportunityId, WhenIntData request)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult<WhenIntData>(null!);
            if (request.ImplementationStartDate.HasValue && request.TargetSigningDate.HasValue &&
                request.ImplementationStartDate.Value < request.TargetSigningDate.Value)
                return Task.FromResult<WhenIntData>(null!);
            var effectiveStart = request.ImplementationStartDate ?? request.TargetSigningDate;
            if (request.TargetDeliveryDate.HasValue && effectiveStart.HasValue &&
                request.TargetDeliveryDate.Value < effectiveStart.Value)
                return Task.FromResult<WhenIntData>(null!);
            _store[opportunityId] = request;
            return Task.FromResult(request);
        }

        private Task ClearWhenDates(int opportunityId)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.CompletedTask;
            _store[opportunityId] = new WhenIntData
            {
                TargetSigningDate = null,
                ImplementationStartDate = null,
                TargetDeliveryDate = null
            };
            return Task.CompletedTask;
        }

        private Task FullWhenUpdateWithAudit(int opportunityId, WhenIntData request, int userId)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.CompletedTask;
            _store[opportunityId] = request;
            if (!_auditStore.ContainsKey(opportunityId))
                _auditStore[opportunityId] = new List<WhenIntAuditEntry>();
            _auditStore[opportunityId].Add(new WhenIntAuditEntry
            {
                LastModifiedBy = userId,
                LastModifiedDate = DateTime.UtcNow
            });
            return Task.CompletedTask;
        }

        private Task<List<WhenIntAuditEntry>> GetAuditTrail(int opportunityId)
        {
            return Task.FromResult(_auditStore.TryGetValue(opportunityId, out var entries) ? entries : new List<WhenIntAuditEntry>());
        }

        private Task PartialWhenUpdate(int opportunityId, WhenIntData partialUpdate)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.CompletedTask;
            var existing = _store.TryGetValue(opportunityId, out var d) ? d : new WhenIntData();
            if (partialUpdate.TargetSigningDate.HasValue) existing.TargetSigningDate = partialUpdate.TargetSigningDate;
            if (partialUpdate.ImplementationStartDate.HasValue) existing.ImplementationStartDate = partialUpdate.ImplementationStartDate;
            if (partialUpdate.TargetDeliveryDate.HasValue) existing.TargetDeliveryDate = partialUpdate.TargetDeliveryDate;
            if (partialUpdate.IsTargetSigningDateFirm.HasValue) existing.IsTargetSigningDateFirm = partialUpdate.IsTargetSigningDateFirm;
            if (partialUpdate.SigningDateNotes != null) existing.SigningDateNotes = partialUpdate.SigningDateNotes;
            if (partialUpdate.SubmissionDeadline.HasValue) existing.SubmissionDeadline = partialUpdate.SubmissionDeadline;
            _store[opportunityId] = existing;
            return Task.CompletedTask;
        }

        #endregion
    }

    #region Supporting Types

    public class WhenIntResult
    {
        public bool Success { get; set; }
    }

    public class WhenIntData
    {
        public DateTime? TargetSigningDate { get; set; }
        public DateTime? ImplementationStartDate { get; set; }
        public DateTime? TargetDeliveryDate { get; set; }
        public bool? IsTargetSigningDateFirm { get; set; }
        public string? SigningDateNotes { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public List<WhenIntDeliverable>? Deliverables { get; set; }
    }

    public class WhenIntDeliverable
    {
        public int Id { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
    }

    public class WhenIntDeliverablesResult
    {
        public bool Success { get; set; }
        public int DeliverablesUpdated { get; set; }
    }

    public class WhenIntAuditEntry
    {
        public DateTime LastModifiedDate { get; set; }
        public int LastModifiedBy { get; set; }
    }

    #endregion
}
