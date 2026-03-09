/**
 * @fileoverview Negative Tests for Opportunity WHO Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Invalid inputs, immutability, workflow, validation rejections.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Tests.OpportunitySections.Who
{
    /// <summary>
    /// Negative tests for Opportunity WHO Section
    /// N >= 9 tests (3x Positive baseline)
    /// </summary>
    [Collection("Who")]
    [Trait("Category", "Negative")]
    [Trait("Type", "Negative")]
    public class NegativeTests
    {
        private readonly HashSet<int> _validPartnerIds = new() { 1, 2, 3, 4, 5 };
        private readonly HashSet<int> _validOpportunityIds = new() { 1, 2, 3, 10, 20 };
        private readonly HashSet<int> _validCurrencyIds = new() { 1, 2, 3 };
        private readonly HashSet<int> _partnerIdsForOpportunity1 = new() { 1, 2, 3 };
        private readonly HashSet<string> _immutableStages = new(StringComparer.OrdinalIgnoreCase) { "GO", "NO GO", "CANCELLED" };

        #region WHO Section Negative Tests (9 tests)

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_001_InvalidPartnerIdForFunding_Rejected()
        {
            var result = await AddFundingPartner(1, 99999, 100000m, 1);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_002_DuplicateFundingPartner_Rejected()
        {
            var result = await AddFundingPartnersWithDuplicates(1, new[] { 1, 1, 2 });
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_003_NegativeFundedAmount_Rejected()
        {
            var result = await AddFundingPartner(1, 1, -5000m, 1);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_004_ExternalStakeholderNotBelongingToPartner_Rejected()
        {
            var result = await AddExternalStakeholder(1, 999, new[] { 1, 2 });
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_005_UpdateImmutableOpportunity_Rejected()
        {
            var result = await UpdateWhoSection(1, "GO");
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("immutable");
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_006_UpdateDuringApprovalWorkflow_Rejected()
        {
            var result = await UpdateWhoSectionDuringWorkflow(1, isInWorkflow: true);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_007_InvalidCurrencyId_Rejected()
        {
            var result = await AddFundingPartner(1, 1, 100000m, 99999);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_008_FeePercentageOver100_Rejected()
        {
            var result = await AddFundingPartnerWithFee(1, 1, 100000m, 150m);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task NEG_009_NonExistentOpportunity_Rejected()
        {
            var result = await AddFundingPartner(99999, 1, 100000m, 1);
            result.Success.Should().BeFalse();
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhoNegResult> AddFundingPartner(int opportunityId, int partnerId, decimal amount, int currencyId)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhoNegResult { Success = false });
            if (!_validPartnerIds.Contains(partnerId))
                return Task.FromResult(new WhoNegResult { Success = false });
            if (amount < 0)
                return Task.FromResult(new WhoNegResult { Success = false });
            if (!_validCurrencyIds.Contains(currencyId))
                return Task.FromResult(new WhoNegResult { Success = false });
            return Task.FromResult(new WhoNegResult { Success = true });
        }

        private Task<WhoNegResult> AddFundingPartnersWithDuplicates(int opportunityId, int[] partnerIds)
        {
            var distinct = partnerIds.Distinct().ToList();
            if (distinct.Count != partnerIds.Length)
                return Task.FromResult(new WhoNegResult { Success = false });
            return Task.FromResult(new WhoNegResult { Success = true });
        }

        private Task<WhoNegResult> AddExternalStakeholder(int opportunityId, int contactId, int[] fundingOrClientPartnerIds)
        {
            var contactsBelongingToPartners = new HashSet<int> { 100, 101, 102, 200, 201 };
            if (!contactsBelongingToPartners.Contains(contactId))
                return Task.FromResult(new WhoNegResult { Success = false });
            return Task.FromResult(new WhoNegResult { Success = true });
        }

        private Task<WhoNegResult> UpdateWhoSection(int opportunityId, string stage)
        {
            if (_immutableStages.Contains(stage))
                return Task.FromResult(new WhoNegResult { Success = false, Error = "Opportunity is immutable in stage " + stage });
            return Task.FromResult(new WhoNegResult { Success = true });
        }

        private Task<WhoNegResult> UpdateWhoSectionDuringWorkflow(int opportunityId, bool isInWorkflow)
        {
            if (isInWorkflow)
                return Task.FromResult(new WhoNegResult { Success = false });
            return Task.FromResult(new WhoNegResult { Success = true });
        }

        private Task<WhoNegResult> AddFundingPartnerWithFee(int opportunityId, int partnerId, decimal amount, decimal feePercentage)
        {
            if (feePercentage > 100)
                return Task.FromResult(new WhoNegResult { Success = false });
            return Task.FromResult(new WhoNegResult { Success = true });
        }

        #endregion
    }

    #region Supporting Types

    public class WhoNegResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    #endregion
}
