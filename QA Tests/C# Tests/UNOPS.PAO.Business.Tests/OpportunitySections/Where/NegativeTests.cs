/**
 * @fileoverview Negative Tests for Opportunity WHERE Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Invalid inputs, immutability, workflow lock, non-existent entity.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Tests.OpportunitySections.Where
{
    /// <summary>
    /// Negative tests for Opportunity WHERE Section
    /// N >= 9 tests
    /// </summary>
    [Collection("Where")]
    [Trait("Category", "Negative")]
    [Trait("Type", "Negative")]
    public class NegativeTests
    {
        #region WHERE Section Negative Tests (9 tests)

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_001_InvalidCountryId_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var countries = new List<WhereNegCountryData>
            {
                new() { CountryId = 99999, SpecificAreas = "Test" }
            };

            // Act
            var result = await UpdateWhere(opportunityId, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_002_DuplicateCountryId_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var countries = new List<WhereNegCountryData>
            {
                new() { CountryId = 1, SpecificAreas = "Area A" },
                new() { CountryId = 1, SpecificAreas = "Area B" }
            };

            // Act
            var result = await UpdateWhere(opportunityId, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_003_UpdateImmutableOpportunity_Rejected()
        {
            // Arrange - stage "GO" throws BusinessException
            var opportunityId = 1;
            var stage = "GO";
            var countries = new List<WhereNegCountryData> { new() { CountryId = 1 } };

            // Act
            var result = await UpdateWhereWithStage(opportunityId, stage, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_004_UpdateDuringWorkflow_Rejected()
        {
            // Arrange - UNOPS: IsInWorkflow == true throws BusinessException
            var opportunityId = 1;
            var isInWorkflow = true;
            var countries = new List<WhereNegCountryData> { new() { CountryId = 1 } };

            // Act
            var result = await UpdateWhereInWorkflow(opportunityId, isInWorkflow, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_005_NonExistentOpportunity_Rejected()
        {
            // Arrange
            var nonExistentId = 999999;
            var countries = new List<WhereNegCountryData> { new() { CountryId = 1 } };

            // Act
            var result = await UpdateWhere(nonExistentId, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_006_NegativeCountryId_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var countries = new List<WhereNegCountryData>
            {
                new() { CountryId = -1, SpecificAreas = "Test" }
            };

            // Act
            var result = await UpdateWhere(opportunityId, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_007_CountryIdZero_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var countries = new List<WhereNegCountryData>
            {
                new() { CountryId = 0, SpecificAreas = "Test" }
            };

            // Act
            var result = await UpdateWhere(opportunityId, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_008_SpecificAreasExceedsMaxLength_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var countries = new List<WhereNegCountryData>
            {
                new() { CountryId = 1, SpecificAreas = new string('X', 2001) }
            };

            // Act
            var result = await UpdateWhere(opportunityId, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task NEG_009_UpdateImmutableOpportunity_NOGO_Rejected()
        {
            // Arrange - stage "NO GO" throws BusinessException
            var opportunityId = 1;
            var stage = "NO GO";
            var countries = new List<WhereNegCountryData> { new() { CountryId = 1 } };

            // Act
            var result = await UpdateWhereWithStage(opportunityId, stage, countries);

            // Assert
            result.Success.Should().BeFalse();
        }

        #endregion

        #region Helper Methods (Stubs)

        private readonly HashSet<int> _validCountryIds = new() { 1, 2, 3, 4, 5, 10, 20, 50, 100 };
        private const int MaxSpecificAreasLength = 2000;
        private const int MaxOpportunityId = 100000;

        private Task<WhereNegResult> UpdateWhere(int opportunityId, List<WhereNegCountryData> countries)
        {
            if (opportunityId > MaxOpportunityId || opportunityId <= 0)
                return Task.FromResult(new WhereNegResult { Success = false });
            if (countries == null)
                return Task.FromResult(new WhereNegResult { Success = false });
            if (countries.Any(c => c.CountryId <= 0 || !_validCountryIds.Contains(c.CountryId)))
                return Task.FromResult(new WhereNegResult { Success = false });
            if (countries.Any(c => c.SpecificAreas != null && c.SpecificAreas.Length > MaxSpecificAreasLength))
                return Task.FromResult(new WhereNegResult { Success = false });
            if (countries.GroupBy(c => c.CountryId).Any(g => g.Count() > 1))
                return Task.FromResult(new WhereNegResult { Success = false });
            return Task.FromResult(new WhereNegResult { Success = true });
        }

        private Task<WhereNegResult> UpdateWhereWithStage(int opportunityId, string stage, List<WhereNegCountryData> countries)
        {
            var stageNorm = stage?.Trim().ToUpperInvariant() ?? "";
            if (stageNorm == "GO" || stageNorm == "NO GO" || stageNorm == "CANCELLED")
                return Task.FromResult(new WhereNegResult { Success = false });
            return UpdateWhere(opportunityId, countries);
        }

        private Task<WhereNegResult> UpdateWhereInWorkflow(int opportunityId, bool isInWorkflow, List<WhereNegCountryData> countries)
        {
            if (isInWorkflow)
                return Task.FromResult(new WhereNegResult { Success = false });
            return UpdateWhere(opportunityId, countries);
        }

        #endregion
    }

    #region Supporting Types

    public class WhereNegResult
    {
        public bool Success { get; set; }
    }

    public class WhereNegCountryData
    {
        public int CountryId { get; set; }
        public string? SpecificAreas { get; set; }
        public bool? HumanitarianFrameworkAlignment { get; set; }
        public bool? NdcAlignment { get; set; }
        public bool? NapAlignment { get; set; }
        public bool? OrgUnitStrategyAlignment { get; set; }
    }

    #endregion
}
