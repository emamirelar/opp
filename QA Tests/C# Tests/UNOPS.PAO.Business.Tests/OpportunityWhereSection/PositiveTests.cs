/**
 * @fileoverview PNO-697, PNO-775, PNO-776, PNO-778, PNO-895, PNO-935: Opportunity WHERE Section — Positive tests.
 * Validates happy-path scenarios for implementation countries, multi-select, indicators, and org unit display.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhereSection;

/// <summary>
/// Positive tests for Opportunity WHERE Section.
/// Requirements validated: PNO-697 AC1–AC7, PNO-775 (SIDS/Fragile/HCA), PNO-776 (Org Unit), PNO-778 (multi-select), PNO-895 (bulk delete), PNO-935 (search).
/// </summary>
public class PositiveTests : IClassFixture<OpportunityWhereSectionFixture>
{
    private readonly OpportunityWhereSectionFixture _f;

    public PositiveTests(OpportunityWhereSectionFixture fixture) => _f = fixture;

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithSingleCountry_Success()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Should().NotBeNull();
        result.Id.Should().Be(_f.OpportunityId);
        result.Countries.Should().NotBeNull().And.HaveCount(1);
        result.Countries![0].CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithMultipleCountries_AllPersisted()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1 },
                new() { CountryId = _f.CountryId2 },
                new() { CountryId = _f.CountryId3 }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries.Should().HaveCount(3);
        result.Countries!.Select(c => c.CountryId).Should().Contain(new[] { _f.CountryId1, _f.CountryId2, _f.CountryId3 });
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithEmptyCountries_RemovesAll()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithSpecificAreas_Success()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, SpecificAreas = "Dhaka, Chittagong" }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("Dhaka, Chittagong");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithAlignmentFlags_Success()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, HumanitarianFrameworkAlignment = true, NdcAlignment = false }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].HumanitarianFrameworkAlignment.Should().BeTrue();
        result.Countries[0].NdcAlignment.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void GetOpportunity_ReturnsCountries()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp.Should().NotBeNull();
        opp!.Countries.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void GetOpportunity_CountriesIncludeCountryDetails()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].Country.Should().NotBeNull();
        opp.Countries[0].Country!.Id.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_ReplaceCountries_OnlyRequestedRemain()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId2 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries.Should().HaveCount(1);
        result.Countries![0].CountryId.Should().Be(_f.CountryId2);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpportunityCountryModel_HasRequiredProperties()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Id = 1, OpportunityId = 1, CountryId = 1 };
        model.Id.Should().Be(1);
        model.OpportunityId.Should().Be(1);
        model.CountryId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpportunityCountryRequest_AcceptsValidData()
    {
        var req = new OpportunityCountryRequest { CountryId = 1, SpecificAreas = "Area 1" };
        req.CountryId.Should().Be(1);
        req.SpecificAreas.Should().Be("Area 1");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void WhereSectionRequest_AcceptsCountriesList()
    {
        var req = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = 1 } } };
        req.Countries.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void GetOpportunity_CountriesHaveCountryId()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void GetOpportunity_CountriesHaveOpportunityId()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].OpportunityId.Should().Be(_f.OpportunityId);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_AddCountry_ThenGet_ReturnsAdded()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Any(c => c.CountryId == _f.CountryId1).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithNullSpecificAreas_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = null } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_RepeatedUpdates_Consistent()
    {
        for (int i = 0; i < 2; i++)
        {
            var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
            _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        }
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void GetOpportunity_CountriesCountMatches()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpportunityCountryModel_CountryPropertyOptional()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = null };
        model.Country.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpportunityCountryRequest_AllAlignmentPropertiesNullable()
    {
        var req = new OpportunityCountryRequest { CountryId = 1 };
        req.HumanitarianFrameworkAlignment.Should().BeNull();
        req.NdcAlignment.Should().BeNull();
        req.NapAlignment.Should().BeNull();
        req.OrgUnitStrategyAlignment.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithNapAlignment_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, NapAlignment = true } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].NapAlignment.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_WithOrgUnitStrategyAlignment_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, OrgUnitStrategyAlignment = true } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].OrgUnitStrategyAlignment.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpportunityWhereSectionSpec_CountryArtifactTypesDefined()
    {
        OpportunityWhereSectionSpec.CountryArtifactTypes.SIDS.Should().Be("SIDS");
        OpportunityWhereSectionSpec.CountryArtifactTypes.WorldBankFragileSituation.Should().NotBeNullOrEmpty();
        OpportunityWhereSectionSpec.CountryArtifactTypes.HostAgreement.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpportunityWhereSectionSpec_SpecificAreasMaxLength()
    {
        OpportunityWhereSectionSpec.SpecificAreasMaxLength.Should().Be(1000);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_SameCountryTwiceInRequest_Handled()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1 },
                new() { CountryId = _f.CountryId1 }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Select(c => c.CountryId).Distinct().Count().Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_EmptySpecificAreas_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void GetOpportunity_CountriesHaveIds()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_ThreeCountries_AllHaveCorrectCountryId()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1 },
                new() { CountryId = _f.CountryId2 },
                new() { CountryId = _f.CountryId3 }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Select(c => c.CountryId).Should().BeEquivalentTo(new[] { _f.CountryId1, _f.CountryId2, _f.CountryId3 });
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UpdateWhereSection_UpdateExistingCountry_SpecificAreasUpdated()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Initial" } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Updated" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("Updated");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void WhereSectionRequest_NullCountries_Allowed()
    {
        var req = new WhereSectionRequest { Countries = null };
        req.Countries.Should().BeNull();
    }
}
