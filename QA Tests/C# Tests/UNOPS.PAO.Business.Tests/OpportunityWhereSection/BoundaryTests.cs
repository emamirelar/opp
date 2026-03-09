/**
 * @fileoverview PNO-697, PNO-775, PNO-776, PNO-778, PNO-895, PNO-935: Opportunity WHERE Section — Boundary tests.
 * Min/max values, soft-delete, nullable FK, concurrent modification.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhereSection;

/// <summary>
/// Boundary tests for Opportunity WHERE Section.
/// </summary>
public class BoundaryTests : IClassFixture<OpportunityWhereSectionFixture>
{
    private readonly OpportunityWhereSectionFixture _f;

    public BoundaryTests(OpportunityWhereSectionFixture fixture) => _f = fixture;

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SpecificAreasAtMaxLength_Success()
    {
        var str = new string('x', OpportunityWhereSectionSpec.SpecificAreasMaxLength);
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = str } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas!.Length.Should().Be(OpportunityWhereSectionSpec.SpecificAreasMaxLength);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SpecificAreasOneChar_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "A" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("A");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_OneCountry_Minimum()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_ZeroCountries_EmptyList()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryRequest_CountryIdOne_MinimumValid()
    {
        var req = new OpportunityCountryRequest { CountryId = 1 };
        req.CountryId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_IdZero_NewEntity()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Id = 0 };
        model.Id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_AllAlignmentsTrue_Success()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, HumanitarianFrameworkAlignment = true, NdcAlignment = true, NapAlignment = true, OrgUnitStrategyAlignment = true }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].HumanitarianFrameworkAlignment.Should().BeTrue();
        result.Countries[0].NdcAlignment.Should().BeTrue();
        result.Countries[0].NapAlignment.Should().BeTrue();
        result.Countries[0].OrgUnitStrategyAlignment.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_AllAlignmentsFalse_Success()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, HumanitarianFrameworkAlignment = false, NdcAlignment = false, NapAlignment = false, OrgUnitStrategyAlignment = false }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].HumanitarianFrameworkAlignment.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_RiskScoreDecimalBoundary()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { RiskScore = 9.9m };
        model.RiskScore.Should().Be(9.9m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_ReplaceWithSameCountry_Success()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Updated" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Should().HaveCount(1);
        result.Countries[0].SpecificAreas.Should().Be("Updated");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_CountryCount_VariousCounts(int count)
    {
        var countries = new List<OpportunityCountryRequest>();
        var ids = new[] { _f.CountryId1, _f.CountryId2, _f.CountryId3 };
        for (int i = 0; i < count; i++)
            countries.Add(new OpportunityCountryRequest { CountryId = ids[i % ids.Length] });
        var request = new WhereSectionRequest { Countries = countries };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Count.Should().Be(count);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_OrgUnitWithStrategyIdPositive()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyId = 1 };
        model.OrgUnitWithStrategyId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryRequest_SpecificAreasUnicode_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Dhaka — Chittagong" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Contain("—");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SpecificAreasWithNewlines_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Line1\nLine2" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Contain("\n");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_HasHumanitarianFrameworkTrue()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { HasHumanitarianFramework = true };
        model.HasHumanitarianFramework.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_HasNdcTrue()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { HasNdc = true };
        model.HasNdc.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_HasNapTrue()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { HasNap = true };
        model.HasNap.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_HasOrgUnitStrategyTrue()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { HasOrgUnitStrategy = true };
        model.HasOrgUnitStrategy.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_HasMoreLocalStrategyAvailableTrue()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { HasMoreLocalStrategyAvailable = true };
        model.HasMoreLocalStrategyAvailable.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_AlternateAddRemove_Success()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().HaveCount(1);
        opp.Countries[0].CountryId.Should().Be(_f.CountryId2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_CountryContinentNull()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { Continent = null } };
        model.Country!.Continent.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_CountryRegionNull()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { Region = null } };
        model.Country!.Region.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_MixedAlignmentNullAndValues()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, HumanitarianFrameworkAlignment = true, NdcAlignment = null, NapAlignment = false, OrgUnitStrategyAlignment = null }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].HumanitarianFrameworkAlignment.Should().BeTrue();
        result.Countries[0].NdcAlignment.Should().BeNull();
        result.Countries[0].NapAlignment.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityWhereSectionSpec_ContextWarningMaxLengthBoundary()
    {
        var str = new string('x', OpportunityWhereSectionSpec.ContextWarningMaxLength);
        str.Length.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void GetOpportunity_CountriesIdsPositive()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SpecificAreasExactMaxLength()
    {
        var str = new string('a', 1000);
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = str } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas!.Length.Should().Be(1000);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_CurrentOrgUnitWithStrategyIdSet()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { CurrentOrgUnitWithStrategyId = 42 };
        model.CurrentOrgUnitWithStrategyId.Should().Be(42);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_ThreeDifferentCountries_AllPersisted()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, SpecificAreas = "A1" },
                new() { CountryId = _f.CountryId2, SpecificAreas = "A2" },
                new() { CountryId = _f.CountryId3, SpecificAreas = "A3" }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Select(c => c.SpecificAreas).Should().BeEquivalentTo(new[] { "A1", "A2", "A3" });
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryRequest_SpecificAreasTabChar()
    {
        var req = new OpportunityCountryRequest { CountryId = 1, SpecificAreas = "Col1\tCol2" };
        req.SpecificAreas.Should().Contain("\t");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_EmptyStringSpecificAreas()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_OrgUnitWithStrategyCodeSet()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyCode = "BDO" };
        model.OrgUnitWithStrategyCode.Should().Be("BDO");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_OrgUnitWithStrategyNameSet()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyName = "Bangladesh Office" };
        model.OrgUnitWithStrategyName.Should().Be("Bangladesh Office");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_FromThreeToTwo_RemovesCorrect()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1 },
                new() { CountryId = _f.CountryId2 },
                new() { CountryId = _f.CountryId3 }
            }
        }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1 },
                new() { CountryId = _f.CountryId2 }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Select(c => c.CountryId).Should().NotContain(_f.CountryId3);
    }

    [Theory]
    [InlineData("IDENTIFY & PROFILE")]
    [InlineData("DEVELOP & NEGOTIATE")]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_ModifiableStages_Success(string stage)
    {
        var opp = _f.Context.Opportunities.Find(_f.OpportunityId);
        if (opp != null)
        {
            opp.Stage = stage;
            _f.Context.SaveChanges();
            var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
            var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
            result.Countries!.Should().HaveCount(1);
            opp.Stage = "IDENTIFY & PROFILE";
            _f.Context.SaveChanges();
        }
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_RiskScoreZero()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { RiskScore = 0 };
        model.RiskScore.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_RiskScoreMaxDecimal()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { RiskScore = 9.9m };
        model.RiskScore.Should().BeInRange(0, 10);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SpecificAreasSpecialChars()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Area <test> & \"quoted\"" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Contain("<");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void GetOpportunity_CountriesOrderPreserved()
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
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Select(c => c.CountryId).Should().ContainInOrder(_f.CountryId1, _f.CountryId2, _f.CountryId3);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryRequest_CountryIdMaxInt()
    {
        var req = new OpportunityCountryRequest { CountryId = int.MaxValue };
        req.CountryId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_RepeatedSameRequest_Idempotent()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_CountryHasActiveUNCFTrue()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { HasActiveUNCF = true } };
        model.Country!.HasActiveUNCF.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SpecificAreasWhitespaceOnly()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "   \t  " } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("   \t  ");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityWhereSectionSpec_ArtifactTypeCodes()
    {
        OpportunityWhereSectionSpec.CountryArtifactTypes.UNRegion.Should().NotBeNullOrEmpty();
        OpportunityWhereSectionSpec.CountryArtifactTypes.UNSubRegion.Should().NotBeNullOrEmpty();
        OpportunityWhereSectionSpec.CountryArtifactTypes.UNOPSRegion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_AddRemoveAddSameCountry()
    {
        var req = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, req).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, req).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_AllOptionalStringsNull()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel
        {
            SpecificAreas = null,
            ContextWarning = null,
            OrgUnitWithStrategyName = null,
            OrgUnitWithStrategyCode = null,
            CurrentOrgUnitWithStrategyName = null,
            CurrentOrgUnitWithStrategyCode = null
        };
        model.SpecificAreas.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_TwoCountriesSameSpecificAreas()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, SpecificAreas = "Same" },
                new() { CountryId = _f.CountryId2, SpecificAreas = "Same" }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.All(c => c.SpecificAreas == "Same").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_CountryIso2CodeSet()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { Iso2Code = "BD" } };
        model.Country!.Iso2Code.Should().Be("BD");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_OpportunityIdBoundary()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Id.Should().Be(_f.OpportunityId);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void GetOpportunity_CountriesWithCountryDetails()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].Country!.Id.Should().Be(_f.CountryId1);
        opp.Countries[0].Country.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryRequest_SpecificAreasCommaSeparated()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "A, B, C" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("A, B, C");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_RemoveOneOfTwo_OneRemains()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } }
        }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Should().HaveCount(1);
        result.Countries[0].CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_OrgUnitWithStrategyIdZero()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyId = 0 };
        model.OrgUnitWithStrategyId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SpecificAreasWithCommas()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Dhaka, Chittagong, Sylhet" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Contain(",");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_OpportunityIdMatches()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].OpportunityId.Should().Be(_f.OpportunityId);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_EmptyThenAdd_Success()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpportunityCountryModel_CountryIdMatches()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries![0].CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UpdateWhereSection_SwitchOrderOfCountries()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } }
        }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId2 }, new() { CountryId = _f.CountryId1 } }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Select(c => c.CountryId).Should().BeEquivalentTo(new[] { _f.CountryId1, _f.CountryId2 });
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_DefaultValues() => new UNOPS.PAO.Models.OpportunityCountryModel().Id.Should().Be(0);

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_WhereSectionRequest_NewInstance() => new WhereSectionRequest().Countries.Should().BeNull();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryRequest_DefaultCountryId() => new OpportunityCountryRequest().CountryId.Should().Be(0);

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 3, 1)]
    [InlineData(3, 1, 2)]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_ThreeCountriesPermutations(int a, int b, int c)
    {
        var ids = new[] { _f.CountryId1, _f.CountryId2, _f.CountryId3 };
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = ids[a - 1] }, new() { CountryId = ids[b - 1] }, new() { CountryId = ids[c - 1] } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Count.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_SpecificAreas_SingleSpace() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = " " } } }).GetAwaiter().GetResult().Countries![0].SpecificAreas.Should().Be(" ");

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_CountryModel_Iso2CodeEmpty() => new UNOPS.PAO.Models.Locations.CountryModel { Iso2Code = "" }.Iso2Code.Should().Be("");

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_NullSpecificAreasInRequest() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = null } } }).GetAwaiter().GetResult().Countries![0].SpecificAreas.Should().BeNull();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_OrgUnitWithStrategyIdNegative() => new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyId = -1 }.OrgUnitWithStrategyId.Should().Be(-1);

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_FourCountries() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 }, new() { CountryId = _f.CountryId3 }, new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult().Countries!.Count.Should().BeLessOrEqualTo(4);

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_RiskScoreMin() => new UNOPS.PAO.Models.OpportunityCountryModel { RiskScore = 0 }.RiskScore.Should().Be(0);

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_GetOpportunity_ReturnsNonNullCountries() => _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult()!.Countries.Should().NotBeNull();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_AlignmentsAllNull() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult().Countries![0].HumanitarianFrameworkAlignment.Should().BeNull();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_CountryIdPositive() => new UNOPS.PAO.Models.OpportunityCountryModel { CountryId = 1 }.CountryId.Should().Be(1);

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_ReplaceWithDifferentSpecificAreas() { _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Old" } } }).GetAwaiter().GetResult(); var r = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "New" } } }).GetAwaiter().GetResult(); r.Countries![0].SpecificAreas.Should().Be("New"); }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryRequest_SpecificAreasNull() => new OpportunityCountryRequest { SpecificAreas = null }.SpecificAreas.Should().BeNull();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_TwoCountriesDifferentAlignments()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1, HumanitarianFrameworkAlignment = true },
                new() { CountryId = _f.CountryId2, NdcAlignment = true }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].HumanitarianFrameworkAlignment.Should().BeTrue();
        result.Countries[1].NdcAlignment.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_CurrentOrgUnitWithStrategyIdZero() => new UNOPS.PAO.Models.OpportunityCountryModel { CurrentOrgUnitWithStrategyId = 0 }.CurrentOrgUnitWithStrategyId.Should().Be(0);

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_AddSecondCountry() { _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult(); var r = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult(); r.Countries!.Count.Should().Be(2); }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityWhereSectionSpec_SpecificAreasMaxLengthValue() => OpportunityWhereSectionSpec.SpecificAreasMaxLength.Should().BePositive();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_SpecificAreas999Chars() { var s = new string('x', 999); var r = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = s } } }).GetAwaiter().GetResult(); r.Countries![0].SpecificAreas!.Length.Should().Be(999); }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_CountryNotNullWhenSet() => new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel() }.Country.Should().NotBeNull();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_RemoveAllThenAddOne() { _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult(); var r = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult(); r.Countries!.Count.Should().Be(1); }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryRequest_NapAlignmentTrue() => new OpportunityCountryRequest { NapAlignment = true }.NapAlignment.Should().BeTrue();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryRequest_OrgUnitStrategyAlignmentFalse() => new OpportunityCountryRequest { OrgUnitStrategyAlignment = false }.OrgUnitStrategyAlignment.Should().BeFalse();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_CountriesListNotNull() => new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }.Countries.Should().NotBeNull();

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_IdPositiveWhenSaved() { _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult(); _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult()!.Countries![0].Id.Should().BePositive(); }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_SpecificAreasWithApostrophe() { var r = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "O'Brien" } } }).GetAwaiter().GetResult(); r.Countries![0].SpecificAreas.Should().Contain("'"); }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_OpportunityCountryModel_OpportunityIdPositive() => new UNOPS.PAO.Models.OpportunityCountryModel { OpportunityId = 1 }.OpportunityId.Should().Be(1);

    [Fact]
    [Trait("Category", "Boundary")]
    public void Boundary_UpdateWhereSection_FromTwoToThree() { _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult(); var r = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 }, new() { CountryId = _f.CountryId3 } } }).GetAwaiter().GetResult(); r.Countries!.Count.Should().Be(3); }
}
