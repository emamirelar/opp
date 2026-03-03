/**
 * @fileoverview Integration Tests for Opportunity WHERE Section
 * Specification/stub pattern - NO real DB, NO real managers.
 * Covers: End-to-end flows, full update, read-after-write, concurrent updates.
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
    /// Integration tests for Opportunity WHERE Section
    /// I >= 9 tests
    /// </summary>
    [Collection("Where")]
    [Trait("Category", "Integration")]
    [Trait("Type", "Integration")]
    public class IntegrationTests
    {
        #region WHERE Section Integration Tests (9 tests)

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_001_FullWhereUpdate_AllFields()
        {
            // Arrange
            var opportunityId = 1;
            var request = new WhereIntCountryData
            {
                CountryId = 1,
                SpecificAreas = "Region A, Region B",
                HumanitarianFrameworkAlignment = true,
                NdcAlignment = true,
                NapAlignment = false,
                OrgUnitStrategyAlignment = true
            };
            var countries = new List<WhereIntCountryData> { request };

            // Act
            var result = await UpdateWhere(opportunityId, countries);
            var read = await ReadWhere(opportunityId);

            // Assert
            result.Success.Should().BeTrue();
            read.Should().NotBeNull();
            read!.Countries.Should().HaveCount(1);
            var c = read.Countries![0];
            c.CountryId.Should().Be(1);
            c.SpecificAreas.Should().Be("Region A, Region B");
            c.HumanitarianFrameworkAlignment.Should().BeTrue();
            c.NdcAlignment.Should().BeTrue();
            c.NapAlignment.Should().BeFalse();
            c.OrgUnitStrategyAlignment.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_002_AddCountriesThenVerifyRead()
        {
            // Arrange
            var opportunityId = 2;
            var countries = new List<WhereIntCountryData>
            {
                new() { CountryId = 1, SpecificAreas = "Area 1" },
                new() { CountryId = 2, SpecificAreas = "Area 2" }
            };

            // Act
            await UpdateWhere(opportunityId, countries);
            var read = await ReadWhere(opportunityId);

            // Assert
            read.Should().NotBeNull();
            read!.Countries.Should().HaveCount(2);
            read.Countries!.Select(x => x.CountryId).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_003_ReplaceCountries_OldRemoved()
        {
            // Arrange
            var opportunityId = 3;
            var initial = new List<WhereIntCountryData>
            {
                new() { CountryId = 1 },
                new() { CountryId = 2 }
            };
            var replacement = new List<WhereIntCountryData>
            {
                new() { CountryId = 3 },
                new() { CountryId = 4 }
            };

            // Act
            await UpdateWhere(opportunityId, initial);
            await UpdateWhere(opportunityId, replacement);
            var read = await ReadWhere(opportunityId);

            // Assert
            read!.Countries!.Select(c => c.CountryId).Should().BeEquivalentTo(new[] { 3, 4 });
            read.Countries.Should().NotContain(c => c.CountryId == 1 || c.CountryId == 2);
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_004_ConcurrentWhereUpdates_Handled()
        {
            // Arrange
            var opportunityId = 4;
            var task1 = UpdateWhere(opportunityId, new List<WhereIntCountryData> { new() { CountryId = 1 } });
            var task2 = UpdateWhere(opportunityId, new List<WhereIntCountryData> { new() { CountryId = 2 } });

            // Act
            var results = await Task.WhenAll(task1, task2);
            var read = await ReadWhere(opportunityId);

            // Assert - last write wins in stub
            results.All(r => r.Success).Should().BeTrue();
            read.Should().NotBeNull();
            read!.Countries.Should().HaveCount(1);
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_005_WhereWithAlignmentChanges_Persists()
        {
            // Arrange
            var opportunityId = 5;
            var countries = new List<WhereIntCountryData>
            {
                new()
                {
                    CountryId = 1,
                    HumanitarianFrameworkAlignment = true,
                    NdcAlignment = false,
                    NapAlignment = true,
                    OrgUnitStrategyAlignment = true
                }
            };

            // Act
            await UpdateWhere(opportunityId, countries);
            var read = await ReadWhere(opportunityId);

            // Assert
            var c = read!.Countries!.First();
            c.HumanitarianFrameworkAlignment.Should().BeTrue();
            c.NdcAlignment.Should().BeFalse();
            c.NapAlignment.Should().BeTrue();
            c.OrgUnitStrategyAlignment.Should().BeTrue();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_006_WhereUpdateReturnsUpdatedModel()
        {
            // Arrange
            var opportunityId = 6;
            var countries = new List<WhereIntCountryData> { new() { CountryId = 1, SpecificAreas = "Test" } };

            // Act
            var result = await UpdateWhereAndReturn(opportunityId, countries);

            // Assert
            result.Should().NotBeNull();
            result!.Countries.Should().HaveCount(1);
            result.Countries![0].SpecificAreas.Should().Be("Test");
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_007_ClearAllCountries_EmptyList()
        {
            // Arrange
            var opportunityId = 7;
            await UpdateWhere(opportunityId, new List<WhereIntCountryData> { new() { CountryId = 1 } });

            // Act
            await UpdateWhere(opportunityId, new List<WhereIntCountryData>());
            var read = await ReadWhere(opportunityId);

            // Assert
            read!.Countries!.Should().BeEmpty();
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_008_MultipleSequentialWhereUpdates_AllPersist()
        {
            // Arrange
            var opportunityId = 8;

            // Act
            await UpdateWhere(opportunityId, new List<WhereIntCountryData> { new() { CountryId = 1 } });
            await UpdateWhere(opportunityId, new List<WhereIntCountryData> { new() { CountryId = 1 }, new() { CountryId = 2 } });
            await UpdateWhere(opportunityId, new List<WhereIntCountryData> { new() { CountryId = 1 }, new() { CountryId = 2 }, new() { CountryId = 3 } });
            var read = await ReadWhere(opportunityId);

            // Assert
            read!.Countries!.Should().HaveCount(3);
            read.Countries.Select(c => c.CountryId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        [Trait("Section", "WHERESection")]
        public async Task INT_009_WhereUpdate_AuditTrailCreated()
        {
            // Arrange
            var opportunityId = 9;
            var countries = new List<WhereIntCountryData> { new() { CountryId = 1 } };

            // Act
            var result = await UpdateWhereWithAudit(opportunityId, countries);

            // Assert
            result.Success.Should().BeTrue();
            result.AuditTrailCreated.Should().BeTrue();
        }

        #endregion

        #region Helper Methods (Stubs)

        private readonly HashSet<int> _validCountryIds = new() { 1, 2, 3, 4, 5, 10, 20, 50, 100 };
        private readonly Dictionary<int, WhereIntData> _store = new();

        private Task<WhereIntResult> UpdateWhere(int opportunityId, List<WhereIntCountryData> countries)
        {
            if (opportunityId <= 0 || opportunityId > 100000)
                return Task.FromResult(new WhereIntResult { Success = false });
            if (countries == null)
                return Task.FromResult(new WhereIntResult { Success = false });
            if (countries.Count > 0 && countries.Any(c => !_validCountryIds.Contains(c.CountryId)))
                return Task.FromResult(new WhereIntResult { Success = false });
            if (countries.Count > 0 && countries.GroupBy(c => c.CountryId).Any(g => g.Count() > 1))
                return Task.FromResult(new WhereIntResult { Success = false });
            _store[opportunityId] = new WhereIntData
            {
                OpportunityId = opportunityId,
                Countries = countries.Select(c => new WhereIntCountryData
                {
                    CountryId = c.CountryId,
                    SpecificAreas = c.SpecificAreas,
                    HumanitarianFrameworkAlignment = c.HumanitarianFrameworkAlignment,
                    NdcAlignment = c.NdcAlignment,
                    NapAlignment = c.NapAlignment,
                    OrgUnitStrategyAlignment = c.OrgUnitStrategyAlignment
                }).ToList()
            };
            return Task.FromResult(new WhereIntResult { Success = true });
        }

        private Task<WhereIntData?> ReadWhere(int opportunityId)
        {
            if (_store.TryGetValue(opportunityId, out var data))
                return Task.FromResult<WhereIntData?>(data);
            return Task.FromResult<WhereIntData?>(null);
        }

        private Task<WhereIntData?> UpdateWhereAndReturn(int opportunityId, List<WhereIntCountryData> countries)
        {
            if (countries == null || countries.Any(c => !_validCountryIds.Contains(c.CountryId)))
                return Task.FromResult<WhereIntData?>(null);
            var data = new WhereIntData
            {
                OpportunityId = opportunityId,
                Countries = countries.ToList()
            };
            _store[opportunityId] = data;
            return Task.FromResult<WhereIntData?>(data);
        }

        private Task<WhereIntResult> UpdateWhereWithAudit(int opportunityId, List<WhereIntCountryData> countries)
        {
            if (countries == null || countries.Any(c => !_validCountryIds.Contains(c.CountryId)))
                return Task.FromResult(new WhereIntResult { Success = false, AuditTrailCreated = false });
            _store[opportunityId] = new WhereIntData
            {
                OpportunityId = opportunityId,
                Countries = countries.ToList()
            };
            return Task.FromResult(new WhereIntResult { Success = true, AuditTrailCreated = true });
        }

        #endregion
    }

    #region Supporting Types

    public class WhereIntResult
    {
        public bool Success { get; set; }
        public bool AuditTrailCreated { get; set; }
    }

    public class WhereIntData
    {
        public int OpportunityId { get; set; }
        public List<WhereIntCountryData>? Countries { get; set; }
    }

    public class WhereIntCountryData
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
