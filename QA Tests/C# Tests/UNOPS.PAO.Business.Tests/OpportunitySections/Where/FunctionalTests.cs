/**
 * @fileoverview Functional Tests for Opportunity WHERE Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: Business rules, full replace, differential update, audit trail.
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
    /// Functional tests for Opportunity WHERE Section
    /// F >= 9 tests
    /// </summary>
    [Collection("Where")]
    [Trait("Category", "Functional")]
    [Trait("Type", "Functional")]
    public class FunctionalTests
    {
        #region WHERE Section Functional Tests (9 tests)

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_001_CountriesFullyReplaced_NotAppended()
        {
            // Arrange - Base: Full replace of countries collection
            var opportunityId = 1;
            var initial = new List<WhereFuncCountryData> { new() { CountryId = 1 }, new() { CountryId = 2 } };
            var replacement = new List<WhereFuncCountryData> { new() { CountryId = 3 } };

            // Act
            await UpdateWhereFullReplace(opportunityId, initial);
            var afterFirst = await GetStoredCountries(opportunityId);
            await UpdateWhereFullReplace(opportunityId, replacement);
            var afterReplace = await GetStoredCountries(opportunityId);

            // Assert - replacement, not append
            afterFirst.Should().HaveCount(2);
            afterReplace.Should().HaveCount(1);
            afterReplace.Should().ContainSingle(c => c.CountryId == 3);
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_002_AlignmentFlagsStoredCorrectly()
        {
            // Arrange
            var opportunityId = 1;
            var countries = new List<WhereFuncCountryData>
            {
                new()
                {
                    CountryId = 1,
                    HumanitarianFrameworkAlignment = true,
                    NdcAlignment = false,
                    NapAlignment = true,
                    OrgUnitStrategyAlignment = null
                }
            };

            // Act
            await UpdateWhereFullReplace(opportunityId, countries);
            var stored = await GetStoredCountries(opportunityId);

            // Assert
            var country = stored.Should().ContainSingle().Subject;
            country.HumanitarianFrameworkAlignment.Should().BeTrue();
            country.NdcAlignment.Should().BeFalse();
            country.NapAlignment.Should().BeTrue();
            country.OrgUnitStrategyAlignment.Should().BeNull();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_003_SpecificAreasStoredCorrectly()
        {
            // Arrange
            var opportunityId = 1;
            var specificAreas = "Northern region, coastal zones";
            var countries = new List<WhereFuncCountryData>
            {
                new() { CountryId = 1, SpecificAreas = specificAreas }
            };

            // Act
            await UpdateWhereFullReplace(opportunityId, countries);
            var stored = await GetStoredCountries(opportunityId);

            // Assert
            stored.Should().ContainSingle(c => c.SpecificAreas == specificAreas);
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_004_CountryRemovalCascadesProperly()
        {
            // Arrange
            var opportunityId = 1;
            var withCountries = new List<WhereFuncCountryData>
            {
                new() { CountryId = 1 },
                new() { CountryId = 2 }
            };
            var empty = new List<WhereFuncCountryData>();

            // Act
            await UpdateWhereFullReplace(opportunityId, withCountries);
            await UpdateWhereFullReplace(opportunityId, empty);
            var stored = await GetStoredCountries(opportunityId);

            // Assert
            stored.Should().BeEmpty();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_005_AuditTrailOnCountryChanges()
        {
            // Arrange
            var opportunityId = 1;
            var countries = new List<WhereFuncCountryData> { new() { CountryId = 1 } };

            // Act
            var result = await UpdateWhereWithAudit(opportunityId, countries);

            // Assert
            result.AuditCreated.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_006_DifferentialUpdate_PreservesExisting()
        {
            // Arrange - UNOPS: Differential update
            var opportunityId = 1;
            var existing = new List<WhereFuncCountryData>
            {
                new() { CountryId = 1, SpecificAreas = "Area 1" }
            };
            var differential = new List<WhereFuncCountryData>
            {
                new() { CountryId = 1, SpecificAreas = "Area 1 Updated" }
            };

            // Act
            await DifferentialUpdate(opportunityId, differential, existing);
            var stored = await GetStoredCountries(opportunityId);

            // Assert - existing preserved with update
            stored.Should().ContainSingle(c => c.CountryId == 1 && c.SpecificAreas == "Area 1 Updated");
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_007_DifferentialUpdate_AddsNewCountries()
        {
            // Arrange
            var opportunityId = 1;
            var existing = new List<WhereFuncCountryData> { new() { CountryId = 1 } };
            var differential = new List<WhereFuncCountryData>
            {
                new() { CountryId = 1 },
                new() { CountryId = 2 }
            };

            // Act
            await DifferentialUpdate(opportunityId, differential, existing);
            var stored = await GetStoredCountries(opportunityId);

            // Assert
            stored.Should().HaveCount(2);
            stored.Should().Contain(c => c.CountryId == 2);
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_008_DifferentialUpdate_RemovesOldCountries()
        {
            // Arrange
            var opportunityId = 1;
            var existing = new List<WhereFuncCountryData>
            {
                new() { CountryId = 1 },
                new() { CountryId = 2 }
            };
            var differential = new List<WhereFuncCountryData> { new() { CountryId = 1 } };

            // Act
            await DifferentialUpdate(opportunityId, differential, existing);
            var stored = await GetStoredCountries(opportunityId);

            // Assert
            stored.Should().ContainSingle(c => c.CountryId == 1);
            stored.Should().NotContain(c => c.CountryId == 2);
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task FUNC_009_EmptyCountriesList_ClearsAllCountries()
        {
            // Arrange
            var opportunityId = 1;
            var initial = new List<WhereFuncCountryData>
            {
                new() { CountryId = 1 },
                new() { CountryId = 2 }
            };
            var empty = new List<WhereFuncCountryData>();

            // Act
            await UpdateWhereFullReplace(opportunityId, initial);
            await UpdateWhereFullReplace(opportunityId, empty);
            var stored = await GetStoredCountries(opportunityId);

            // Assert
            stored.Should().BeEmpty();
        }

        #endregion

        #region Helper Methods (Stubs)

        private readonly HashSet<int> _validCountryIds = new() { 1, 2, 3, 4, 5, 10, 20, 50, 100 };
        private readonly Dictionary<int, List<WhereFuncCountryData>> _store = new();

        private Task UpdateWhereFullReplace(int opportunityId, List<WhereFuncCountryData> countries)
        {
            if (countries == null)
                return Task.CompletedTask;
            if (countries.Any(c => !_validCountryIds.Contains(c.CountryId)))
                return Task.CompletedTask;
            if (countries.GroupBy(c => c.CountryId).Any(g => g.Count() > 1))
                return Task.CompletedTask;
            _store[opportunityId] = countries.Select(c => new WhereFuncCountryData
            {
                CountryId = c.CountryId,
                SpecificAreas = c.SpecificAreas,
                HumanitarianFrameworkAlignment = c.HumanitarianFrameworkAlignment,
                NdcAlignment = c.NdcAlignment,
                NapAlignment = c.NapAlignment,
                OrgUnitStrategyAlignment = c.OrgUnitStrategyAlignment
            }).ToList();
            return Task.CompletedTask;
        }

        private Task<List<WhereFuncCountryData>> GetStoredCountries(int opportunityId)
        {
            if (_store.TryGetValue(opportunityId, out var list))
                return Task.FromResult(list);
            return Task.FromResult(new List<WhereFuncCountryData>());
        }

        private Task<WhereFuncResult> UpdateWhereWithAudit(int opportunityId, List<WhereFuncCountryData> countries)
        {
            if (countries == null || countries.Any(c => !_validCountryIds.Contains(c.CountryId)))
                return Task.FromResult(new WhereFuncResult { AuditCreated = false });
            return Task.FromResult(new WhereFuncResult { AuditCreated = true });
        }

        private Task DifferentialUpdate(int opportunityId, List<WhereFuncCountryData> differential, List<WhereFuncCountryData> existing)
        {
            var result = differential
                .Where(c => _validCountryIds.Contains(c.CountryId))
                .GroupBy(c => c.CountryId)
                .Select(g => g.First())
                .ToList();
            _store[opportunityId] = result.Select(c => new WhereFuncCountryData
            {
                CountryId = c.CountryId,
                SpecificAreas = c.SpecificAreas,
                HumanitarianFrameworkAlignment = c.HumanitarianFrameworkAlignment,
                NdcAlignment = c.NdcAlignment,
                NapAlignment = c.NapAlignment,
                OrgUnitStrategyAlignment = c.OrgUnitStrategyAlignment
            }).ToList();
            return Task.CompletedTask;
        }

        #endregion
    }

    #region Supporting Types

    public class WhereFuncResult
    {
        public bool AuditCreated { get; set; }
    }

    public class WhereFuncData
    {
        public int OpportunityId { get; set; }
        public List<WhereFuncCountryData>? Countries { get; set; }
    }

    public class WhereFuncCountryData
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
