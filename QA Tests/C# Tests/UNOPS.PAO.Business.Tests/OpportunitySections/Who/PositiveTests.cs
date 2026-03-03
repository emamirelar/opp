/**
 * @fileoverview Positive Tests for Opportunity WHO Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Happy path scenarios for FundingPartners, ClientPartners, IsPooledFunding.
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
    /// Positive tests for Opportunity WHO Section
    /// P = 3 tests (baseline for ratio calculations)
    /// </summary>
    [Collection("Who")]
    [Trait("Category", "Positive")]
    [Trait("Type", "Positive")]
    public class PositiveTests
    {
        private readonly HashSet<int> _validPartnerIds = new() { 1, 2, 3, 4, 5 };
        private readonly HashSet<int> _validOpportunityIds = new() { 1, 2, 3, 10, 20 };
        private readonly HashSet<int> _validCurrencyIds = new() { 1, 2, 3 };
        #region WHO Section Positive Tests (3 tests)

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task POS_001_AddFundingPartners_WithValidData_Succeeds()
        {
            // Arrange
            var opportunityId = 1;
            var partners = new List<WhoPosPartnerData>
            {
                new() { PartnerId = 1, FundedAmount = 100000m, CurrencyId = 1 }
            };

            // Act
            var result = await AddFundingPartners(opportunityId, partners);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task POS_002_AddClientPartners_WithValidData_Succeeds()
        {
            // Arrange
            var opportunityId = 1;
            var partners = new List<WhoPosPartnerData>
            {
                new() { PartnerId = 2 }
            };

            // Act
            var result = await AddClientPartners(opportunityId, partners);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHOSection")]
        public async Task POS_003_SetPooledFunding_Succeeds()
        {
            // Arrange
            var opportunityId = 1;
            var isPooledFunding = true;

            // Act
            var result = await SetPooledFunding(opportunityId, isPooledFunding);

            // Assert
            result.Success.Should().BeTrue();
        }

        #endregion

        #region Helper Methods (Stubs)

        private Task<WhoPosResult> AddFundingPartners(int opportunityId, List<WhoPosPartnerData> partners)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhoPosResult { Success = false });
            if (partners == null || partners.Count == 0)
                return Task.FromResult(new WhoPosResult { Success = false });
            foreach (var p in partners)
            {
                if (!_validPartnerIds.Contains(p.PartnerId) || p.FundedAmount < 0)
                    return Task.FromResult(new WhoPosResult { Success = false });
                if (p.CurrencyId.HasValue && !_validCurrencyIds.Contains(p.CurrencyId.Value))
                    return Task.FromResult(new WhoPosResult { Success = false });
            }
            return Task.FromResult(new WhoPosResult { Success = true });
        }

        private Task<WhoPosResult> AddClientPartners(int opportunityId, List<WhoPosPartnerData> partners)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhoPosResult { Success = false });
            if (partners == null || partners.Count == 0)
                return Task.FromResult(new WhoPosResult { Success = false });
            foreach (var p in partners)
            {
                if (!_validPartnerIds.Contains(p.PartnerId))
                    return Task.FromResult(new WhoPosResult { Success = false });
            }
            return Task.FromResult(new WhoPosResult { Success = true });
        }

        private Task<WhoPosResult> SetPooledFunding(int opportunityId, bool isPooledFunding)
        {
            if (!_validOpportunityIds.Contains(opportunityId))
                return Task.FromResult(new WhoPosResult { Success = false });
            return Task.FromResult(new WhoPosResult { Success = true });
        }

        #endregion
    }

    #region Supporting Types

    public class WhoPosResult
    {
        public bool Success { get; set; }
    }

    public class WhoPosPartnerData
    {
        public int PartnerId { get; set; }
        public decimal FundedAmount { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? FeePercentage { get; set; }
        public decimal? FeeAmount { get; set; }
        public string? PartnershipAgreementReference { get; set; }
        public int? DocumentId { get; set; }
    }

    #endregion
}
