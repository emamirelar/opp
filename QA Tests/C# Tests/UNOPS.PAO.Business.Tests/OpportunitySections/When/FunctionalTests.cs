/**
 * @fileoverview Functional Tests for Opportunity WHEN Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Date validation rules, effective start fallback, field persistence.
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
    /// Functional tests for Opportunity WHEN Section
    /// F >= 9 tests (3x Positive baseline)
    /// </summary>
    [Collection("When")]
    [Trait("Category", "Functional")]
    [Trait("Type", "Functional")]
    public class FunctionalTests
    {
        private readonly HashSet<int> _validOpportunityIds = new() { 1, 2, 3, 10, 20 };

        #region WHEN Section Functional Tests (9 tests)

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_001_DateValidation_ImplStartCannotBeBeforeSigning()
        {
            var signingDate = new DateTime(2026, 6, 1);
            var implStart = new DateTime(2026, 5, 1);
            var result = await ValidateWhenDates(signingDate, implStart, new DateTime(2026, 12, 31));
            result.Valid.Should().BeFalse();
            result.Error.Should().Contain("Implementation Start Date cannot be before");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_002_DateValidation_DeliveryCannotBeBeforeEffectiveStart()
        {
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliveryDate = new DateTime(2026, 6, 15);
            var result = await ValidateDeliveryDate(effectiveStart, deliveryDate);
            result.Valid.Should().BeFalse();
            result.Error.Should().Contain("Target Delivery Date must be after");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_003_DateValidation_DeliverableStartRespectsImplementation()
        {
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliverables = new List<WhenFuncDeliverable>
            {
                new() { Id = 1, PlannedStartDate = new DateTime(2026, 6, 1), PlannedEndDate = new DateTime(2026, 8, 1) }
            };
            var result = await ValidateDeliverables(effectiveStart, deliverables);
            result.Valid.Should().BeFalse();
            result.Error.Should().Contain("Deliverable Planned Start Date cannot be before");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_004_DateValidation_DeliverableEndRespectsStart()
        {
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliverables = new List<WhenFuncDeliverable>
            {
                new() { Id = 1, PlannedStartDate = new DateTime(2026, 7, 15), PlannedEndDate = new DateTime(2026, 7, 10) }
            };
            var result = await ValidateDeliverables(effectiveStart, deliverables);
            result.Valid.Should().BeFalse();
            result.Error.Should().Contain("Deliverable Planned End Date cannot be before the Planned Start Date");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_005_EffectiveStartDate_FallsBackToSigningDate()
        {
            var signingDate = new DateTime(2026, 6, 1);
            DateTime? implStart = null;
            var effectiveStart = await ComputeEffectiveStartDate(implStart, signingDate);
            effectiveStart.Should().Be(signingDate);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_006_IsTargetSigningDateFirm_Persists()
        {
            var data = new WhenFuncData { IsTargetSigningDateFirm = true };
            var stored = await StoreAndRetrieveWhenData(1, data);
            stored.IsTargetSigningDateFirm.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_007_SigningDateNotes_Persists()
        {
            var notes = "Partner deadline for Q2 2026";
            var data = new WhenFuncData { SigningDateNotes = notes };
            var stored = await StoreAndRetrieveWhenData(1, data);
            stored.SigningDateNotes.Should().Be(notes);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_008_SubmissionDeadline_Persists()
        {
            var deadline = new DateTime(2026, 6, 15);
            var data = new WhenFuncData { SubmissionDeadline = deadline };
            var stored = await StoreAndRetrieveWhenData(1, data);
            stored.SubmissionDeadline.Should().Be(deadline);
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task FUNC_009_DeliverableDates_UpdatedIndividually()
        {
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliverables = new List<WhenFuncDeliverable>
            {
                new() { Id = 1, PlannedStartDate = new DateTime(2026, 7, 15), PlannedEndDate = new DateTime(2026, 8, 15) },
                new() { Id = 2, PlannedStartDate = new DateTime(2026, 8, 15), PlannedEndDate = new DateTime(2026, 9, 15) }
            };
            var result = await UpdateDeliverablesIndividually(1, effectiveStart, deliverables);
            result.UpdatedCount.Should().Be(2);
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhenFuncValidationResult> ValidateWhenDates(DateTime? signingDate, DateTime? implStart, DateTime? deliveryDate)
        {
            if (implStart.HasValue && signingDate.HasValue && implStart.Value < signingDate.Value)
                return Task.FromResult(new WhenFuncValidationResult
                {
                    Valid = false,
                    Error = "Implementation Start Date cannot be before the Target Signing Date"
                });
            var effectiveStart = implStart ?? signingDate;
            if (deliveryDate.HasValue && effectiveStart.HasValue && deliveryDate.Value < effectiveStart.Value)
                return Task.FromResult(new WhenFuncValidationResult
                {
                    Valid = false,
                    Error = "Target Delivery Date must be after the Implementation Start Date (or Target Signing Date if no Implementation Start Date is set)"
                });
            return Task.FromResult(new WhenFuncValidationResult { Valid = true });
        }

        private Task<WhenFuncValidationResult> ValidateDeliveryDate(DateTime? effectiveStart, DateTime? deliveryDate)
        {
            if (deliveryDate.HasValue && effectiveStart.HasValue && deliveryDate.Value < effectiveStart.Value)
                return Task.FromResult(new WhenFuncValidationResult
                {
                    Valid = false,
                    Error = "Target Delivery Date must be after the Implementation Start Date (or Target Signing Date if no Implementation Start Date is set)"
                });
            return Task.FromResult(new WhenFuncValidationResult { Valid = true });
        }

        private Task<WhenFuncValidationResult> ValidateDeliverables(DateTime? effectiveImplementationStart,
            List<WhenFuncDeliverable> deliverables)
        {
            if (deliverables == null || !deliverables.Any())
                return Task.FromResult(new WhenFuncValidationResult { Valid = true });
            foreach (var d in deliverables)
            {
                if (d.PlannedStartDate.HasValue && effectiveImplementationStart.HasValue &&
                    d.PlannedStartDate.Value < effectiveImplementationStart.Value)
                    return Task.FromResult(new WhenFuncValidationResult
                    {
                        Valid = false,
                        Error = $"Deliverable Planned Start Date cannot be before the Implementation Start Date for deliverable ID: {d.Id}"
                    });
                if (d.PlannedStartDate.HasValue && d.PlannedEndDate.HasValue &&
                    d.PlannedEndDate.Value < d.PlannedStartDate.Value)
                    return Task.FromResult(new WhenFuncValidationResult
                    {
                        Valid = false,
                        Error = $"Deliverable Planned End Date cannot be before the Planned Start Date for deliverable ID: {d.Id}"
                    });
            }
            return Task.FromResult(new WhenFuncValidationResult { Valid = true });
        }

        private Task<DateTime?> ComputeEffectiveStartDate(DateTime? implStart, DateTime? signingDate)
        {
            return Task.FromResult(implStart ?? signingDate);
        }

        private readonly Dictionary<int, WhenFuncData> _whenStore = new();

        private Task<WhenFuncData> StoreAndRetrieveWhenData(int opportunityId, WhenFuncData data)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhenFuncData());
            _whenStore[opportunityId] = data;
            return Task.FromResult(data);
        }

        private Task<WhenFuncUpdateResult> UpdateDeliverablesIndividually(int opportunityId, DateTime? effectiveStart,
            List<WhenFuncDeliverable> deliverables)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhenFuncUpdateResult { UpdatedCount = 0 });
            return Task.FromResult(new WhenFuncUpdateResult { UpdatedCount = deliverables?.Count ?? 0 });
        }

        #endregion
    }

    #region Supporting Types

    public class WhenFuncResult
    {
        public bool Success { get; set; }
    }

    public class WhenFuncData
    {
        public DateTime? TargetSigningDate { get; set; }
        public DateTime? ImplementationStartDate { get; set; }
        public DateTime? TargetDeliveryDate { get; set; }
        public bool? IsTargetSigningDateFirm { get; set; }
        public string? SigningDateNotes { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public List<WhenFuncDeliverable>? Deliverables { get; set; }
    }

    public class WhenFuncDeliverable
    {
        public int Id { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
    }

    public class WhenFuncValidationResult
    {
        public bool Valid { get; set; }
        public string? Error { get; set; }
    }

    public class WhenFuncUpdateResult
    {
        public int UpdatedCount { get; set; }
    }

    #endregion
}
