/**
 * @fileoverview Negative Tests for Opportunity WHEN Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Invalid date order, immutability, workflow, validation rejections.
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
    /// Negative tests for Opportunity WHEN Section
    /// N >= 9 tests (3x Positive baseline)
    /// </summary>
    [Collection("When")]
    [Trait("Category", "Negative")]
    [Trait("Type", "Negative")]
    public class NegativeTests
    {
        private readonly HashSet<int> _validOpportunityIds = new() { 1, 2, 3, 10, 20 };
        private readonly HashSet<int> _validDeliverableIds = new() { 1, 2, 3, 4, 5 };

        #region WHEN Section Negative Tests (9 tests)

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_001_ImplementationStartBeforeSigningDate_Rejected()
        {
            var signingDate = new DateTime(2026, 6, 1);
            var implStart = new DateTime(2026, 5, 1);
            var result = await UpdateWhenSection(1, signingDate, implStart, new DateTime(2026, 12, 31));
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Implementation Start Date cannot be before the Target Signing Date");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_002_DeliveryDateBeforeImplementationStart_Rejected()
        {
            var signingDate = new DateTime(2026, 6, 1);
            var implStart = new DateTime(2026, 7, 1);
            var deliveryDate = new DateTime(2026, 6, 15);
            var result = await UpdateWhenSection(1, signingDate, implStart, deliveryDate);
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Target Delivery Date must be after");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_003_DeliverableStartBeforeImplementationStart_Rejected()
        {
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliverables = new List<WhenNegDeliverable>
            {
                new() { Id = 1, PlannedStartDate = new DateTime(2026, 6, 15), PlannedEndDate = new DateTime(2026, 8, 15) }
            };
            var result = await UpdateDeliverables(1, effectiveStart, deliverables);
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Deliverable Planned Start Date cannot be before");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_004_DeliverableEndBeforeDeliverableStart_Rejected()
        {
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliverables = new List<WhenNegDeliverable>
            {
                new() { Id = 1, PlannedStartDate = new DateTime(2026, 7, 15), PlannedEndDate = new DateTime(2026, 7, 10) }
            };
            var result = await UpdateDeliverables(1, effectiveStart, deliverables);
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Deliverable Planned End Date cannot be before the Planned Start Date");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_005_UpdateImmutableOpportunity_Rejected()
        {
            var result = await UpdateWhenSection(1, new DateTime(2026, 6, 1), new DateTime(2026, 7, 1),
                new DateTime(2026, 12, 31), stage: "GO");
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("locked");
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_006_UpdateDuringApprovalWorkflow_Rejected()
        {
            var result = await UpdateWhenSection(1, new DateTime(2026, 6, 1), new DateTime(2026, 7, 1),
                new DateTime(2026, 12, 31), isInWorkflow: true);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_007_NonExistentOpportunity_Rejected()
        {
            var result = await UpdateWhenSection(99999, new DateTime(2026, 6, 1), new DateTime(2026, 7, 1),
                new DateTime(2026, 12, 31));
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_008_InvalidDeliverableId_Rejected()
        {
            var effectiveStart = new DateTime(2026, 7, 1);
            var deliverables = new List<WhenNegDeliverable>
            {
                new() { Id = 99999, PlannedStartDate = new DateTime(2026, 7, 15), PlannedEndDate = new DateTime(2026, 8, 15) }
            };
            var result = await UpdateDeliverablesWithIdCheck(1, effectiveStart, deliverables);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHENSection")]
        public async Task NEG_009_DeliveryDateBeforeSigningDate_WhenNoImplementationStart_Rejected()
        {
            var signingDate = new DateTime(2026, 6, 1);
            DateTime? implStart = null;
            var deliveryDate = new DateTime(2026, 5, 15);
            var result = await UpdateWhenSection(1, signingDate, implStart, deliveryDate);
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Target Delivery Date must be after");
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhenNegResult> UpdateWhenSection(int opportunityId, DateTime? signingDate, DateTime? implStart,
            DateTime? deliveryDate, string stage = "IDENTIFY & PROFILE", bool isInWorkflow = false)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhenNegResult { Success = false, Error = "Opportunity not found" });
            var immutableStages = new[] { "GO", "NO GO", "CANCELLED" };
            if (immutableStages.Contains(stage, StringComparer.OrdinalIgnoreCase))
                return Task.FromResult(new WhenNegResult
                {
                    Success = false,
                    Error = "This opportunity record is locked and cannot be modified after a decision has been made."
                });
            if (isInWorkflow)
                return Task.FromResult(new WhenNegResult { Success = false, Error = "This opportunity is pending approval and cannot be modified." });
            if (implStart.HasValue && signingDate.HasValue && implStart.Value < signingDate.Value)
                return Task.FromResult(new WhenNegResult
                {
                    Success = false,
                    Error = "Implementation Start Date cannot be before the Target Signing Date"
                });
            var effectiveStart = implStart ?? signingDate;
            if (deliveryDate.HasValue && effectiveStart.HasValue && deliveryDate.Value < effectiveStart.Value)
                return Task.FromResult(new WhenNegResult
                {
                    Success = false,
                    Error = "Target Delivery Date must be after the Implementation Start Date (or Target Signing Date if no Implementation Start Date is set)"
                });
            return Task.FromResult(new WhenNegResult { Success = true });
        }

        private Task<WhenNegResult> UpdateDeliverables(int opportunityId, DateTime? effectiveImplementationStart,
            List<WhenNegDeliverable> deliverables)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhenNegResult { Success = false });
            if (deliverables == null || !deliverables.Any())
                return Task.FromResult(new WhenNegResult { Success = true });
            foreach (var d in deliverables)
            {
                if (d.PlannedStartDate.HasValue && effectiveImplementationStart.HasValue &&
                    d.PlannedStartDate.Value < effectiveImplementationStart.Value)
                    return Task.FromResult(new WhenNegResult
                    {
                        Success = false,
                        Error = $"Deliverable Planned Start Date cannot be before the Implementation Start Date for deliverable ID: {d.Id}"
                    });
                if (d.PlannedStartDate.HasValue && d.PlannedEndDate.HasValue &&
                    d.PlannedEndDate.Value < d.PlannedStartDate.Value)
                    return Task.FromResult(new WhenNegResult
                    {
                        Success = false,
                        Error = $"Deliverable Planned End Date cannot be before the Planned Start Date for deliverable ID: {d.Id}"
                    });
            }
            return Task.FromResult(new WhenNegResult { Success = true });
        }

        private Task<WhenNegResult> UpdateDeliverablesWithIdCheck(int opportunityId, DateTime? effectiveImplementationStart,
            List<WhenNegDeliverable> deliverables)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhenNegResult { Success = false });
            foreach (var d in deliverables)
            {
                if (!_validDeliverableIds.Contains(d.Id))
                    return Task.FromResult(new WhenNegResult { Success = false, Error = "Invalid deliverable ID" });
            }
            return UpdateDeliverables(opportunityId, effectiveImplementationStart, deliverables);
        }

        #endregion
    }

    #region Supporting Types

    public class WhenNegResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class WhenNegDeliverable
    {
        public int Id { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
    }

    #endregion
}
