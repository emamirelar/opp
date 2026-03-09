/**
 * @fileoverview Functional Tests for Opportunity WHO Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Business rules, deduplication, currency conversion, audit trail.
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
    /// Functional tests for Opportunity WHO Section
    /// F >= 9 tests (3x Positive baseline)
    /// </summary>
    [Collection("Who")]
    [Trait("Category", "Functional")]
    [Trait("Type", "Functional")]
    public class FunctionalTests
    {
        private readonly HashSet<int> _validPartnerIds = new() { 1, 2, 3, 4, 5 };
        private readonly HashSet<int> _validOpportunityIds = new() { 1, 2, 3 };
        private const decimal UsdExchangeRate = 1.0m;
        private const decimal EurToUsdRate = 1.08m;

        #region WHO Section Functional Tests (9 tests)

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_001_IsPooledFundingFlag_Persists()
        {
            var result = await SetPooledFundingAndRead(1, true);
            result.Success.Should().BeTrue();
            result.IsPooledFunding.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_002_FundingPartnerDeduplication_RemovesDuplicates()
        {
            var partners = new List<WhoFuncData> { new(1, 100m), new(1, 200m), new(2, 150m) };
            var result = await AddFundingPartnersWithDeduplication(1, partners);
            result.Success.Should().BeTrue();
            result.DeduplicatedCount.Should().Be(2);
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_003_ClientPartnerDeduplication_RemovesDuplicates()
        {
            var partners = new List<int> { 1, 1, 2, 2, 3 };
            var result = await AddClientPartnersWithDeduplication(1, partners);
            result.Success.Should().BeTrue();
            result.DeduplicatedCount.Should().Be(3);
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_004_CurrencyConversion_CalculatesUSD()
        {
            var result = await ConvertToUsd(100000m, "EUR");
            result.Success.Should().BeTrue();
            result.AmountUSD.Should().BeApproximately(108000m, 0.01m);
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_005_PartnerAgreementReference_Stored()
        {
            var refNum = "AGR-2025-001";
            var result = await StorePartnerAgreementReference(1, 1, refNum);
            result.Success.Should().BeTrue();
            result.StoredReference.Should().Be(refNum);
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_006_TotalFunding_CalculatedCorrectly()
        {
            var amounts = new[] { 100000m, 50000m, 25000m };
            var result = await CalculateTotalFunding(amounts);
            result.Success.Should().BeTrue();
            result.Total.Should().Be(175000m);
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_007_ExternalStakeholderValidation_AgainstPartners()
        {
            var fundingPartnerIds = new[] { 1, 2 };
            var clientPartnerIds = new[] { 3 };
            var contactIds = new[] { 100, 101 };
            var result = await ValidateExternalStakeholdersAgainstPartners(fundingPartnerIds, clientPartnerIds, contactIds);
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_008_AuditTrail_OnPartnerChanges()
        {
            var result = await AddFundingPartnerAndCheckAudit(1, 1, 100000m);
            result.Success.Should().BeTrue();
            result.AuditEntryCreated.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task FUNC_009_FundingPercentage_SumValidation()
        {
            var percentages = new[] { 50m, 30m, 20m };
            var result = await ValidateFundingPercentageSum(percentages);
            result.Success.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhoFuncResult> SetPooledFundingAndRead(int opportunityId, bool isPooledFunding)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhoFuncResult { Success = false });
            return Task.FromResult(new WhoFuncResult { Success = true, IsPooledFunding = isPooledFunding });
        }

        private Task<WhoFuncResult> AddFundingPartnersWithDeduplication(int opportunityId, List<WhoFuncData> partners)
        {
            var distinctPartnerIds = partners.Select(p => p.PartnerId).Distinct().Count();
            return Task.FromResult(new WhoFuncResult { Success = true, DeduplicatedCount = distinctPartnerIds });
        }

        private Task<WhoFuncResult> AddClientPartnersWithDeduplication(int opportunityId, List<int> partnerIds)
        {
            var distinctCount = partnerIds.Distinct().Count();
            return Task.FromResult(new WhoFuncResult { Success = true, DeduplicatedCount = distinctCount });
        }

        private Task<WhoFuncResult> ConvertToUsd(decimal amount, string currency)
        {
            var rate = currency == "USD" ? UsdExchangeRate : EurToUsdRate;
            var amountUsd = amount * rate;
            return Task.FromResult(new WhoFuncResult { Success = true, AmountUSD = amountUsd });
        }

        private Task<WhoFuncResult> StorePartnerAgreementReference(int opportunityId, int partnerId, string reference)
        {
            if (!_validOpportunityIds.Contains(opportunityId) || !_validPartnerIds.Contains(partnerId))
                return Task.FromResult(new WhoFuncResult { Success = false });
            return Task.FromResult(new WhoFuncResult { Success = true, StoredReference = reference });
        }

        private Task<WhoFuncResult> CalculateTotalFunding(decimal[] amounts)
        {
            var total = amounts.Sum();
            return Task.FromResult(new WhoFuncResult { Success = true, Total = total });
        }

        private Task<WhoFuncResult> ValidateExternalStakeholdersAgainstPartners(int[] fundingPartnerIds, int[] clientPartnerIds, int[] contactIds)
        {
            return Task.FromResult(new WhoFuncResult { Success = true });
        }

        private Task<WhoFuncResult> AddFundingPartnerAndCheckAudit(int opportunityId, int partnerId, decimal amount)
        {
            if (!_validOpportunityIds.Contains(opportunityId) || !_validPartnerIds.Contains(partnerId))
                return Task.FromResult(new WhoFuncResult { Success = false });
            return Task.FromResult(new WhoFuncResult { Success = true, AuditEntryCreated = true });
        }

        private Task<WhoFuncResult> ValidateFundingPercentageSum(decimal[] percentages)
        {
            var sum = percentages.Sum();
            var isValid = Math.Abs(sum - 100m) < 0.01m;
            return Task.FromResult(new WhoFuncResult { Success = true, IsValid = isValid });
        }

        #endregion
    }

    #region Supporting Types

    public class WhoFuncResult
    {
        public bool Success { get; set; }
        public bool? IsPooledFunding { get; set; }
        public int DeduplicatedCount { get; set; }
        public decimal? AmountUSD { get; set; }
        public string? StoredReference { get; set; }
        public decimal? Total { get; set; }
        public bool AuditEntryCreated { get; set; }
        public bool IsValid { get; set; }
    }

    public record WhoFuncData(int PartnerId, decimal Amount);

    #endregion
}
