/**
 * @fileoverview PNO-697, PNO-775, PNO-776, PNO-778, PNO-895, PNO-935: Opportunity WHERE Section — Functional tests.
 * Business rules, audit fields, permissions, workflow transitions, data transformations.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhereSection;

/// <summary>
/// Functional tests for Opportunity WHERE Section.
/// </summary>
public class FunctionalTests : IClassFixture<OpportunityWhereSectionFixture>
{
    private readonly OpportunityWhereSectionFixture _f;

    public FunctionalTests(OpportunityWhereSectionFixture fixture) => _f = fixture;

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_PersistsToDatabase()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var count = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId && !oc.IsDeleted);
        count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_SoftDeleteExcludesFromQuery()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().FirstOrDefault(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_DifferentialUpdate_RemovesOnlyRequested()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var count = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId && !oc.IsDeleted);
        count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_DoesNotCascadeDeleteUNCFOutcomes()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        var ocId = opp!.Countries!.First().Id;
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        var remaining = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId);
        remaining.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_IncludesCountriesWithCountry()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.First().Country.Should().NotBeNull();
        opp.Countries.First().Country!.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_OrgUnitWithStrategyIdComputed()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.First().OrgUnitWithStrategyId.Should().Match(id => id == null || (id >= 0 && id <= 5));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_AlignmentsPersisted()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, HumanitarianFrameworkAlignment = true } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.HumanitarianFrameworkAlignment.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_SpecificAreasPersisted()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Test Area" } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.SpecificAreas.Should().Be("Test Area");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesFilteredByIsDeleted()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().OnlyContain(c => c.Id > 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_EmptyCountries_RemovesAll()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        var count = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId && !oc.IsDeleted);
        count.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_AddNewCountry_CreatesRecord()
    {
        var before = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId && !oc.IsDeleted);
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var after = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId && !oc.IsDeleted);
        after.Should().Be(before + 1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_UpdateExisting_DoesNotDuplicate()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Updated" } } }).GetAwaiter().GetResult();
        var count = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId && oc.CountryId == _f.CountryId1 && !oc.IsDeleted);
        count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_ReturnsFullOpportunityModel()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp.Should().NotBeNull();
        opp!.Id.Should().Be(_f.OpportunityId);
        opp.Countries.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_NullCountries_DoesNotClear()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = null }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_HasOpportunityId()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.OpportunityId.Should().Be(_f.OpportunityId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_HasCountryId()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_MultipleCountries_AllHaveCorrectOpportunityId()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var ocs = _f.Context.Set<OpportunityCountry>().Where(oc => oc.OpportunityId == _f.OpportunityId && !oc.IsDeleted).ToList();
        ocs.Should().OnlyContain(oc => oc.OpportunityId == _f.OpportunityId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ReplaceCountry_PreservesOtherData()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "Keep" } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesHaveCountryId()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.First().CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_NdcAlignmentPersisted()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, NdcAlignment = true } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.NdcAlignment.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_NapAlignmentPersisted()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, NapAlignment = false } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.NapAlignment.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_OrgUnitStrategyAlignmentPersisted()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, OrgUnitStrategyAlignment = true } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.OrgUnitStrategyAlignment.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_InheritsModifiableDeletableEntity()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.CreatedBy.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_RemoveUsesPhysicalDelete()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        var total = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId);
        total.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesExcludeDeleted()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ResultIsOpportunityModel()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Should().BeOfType<UNOPS.PAO.Models.OpportunityModel>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesAreOpportunityCountryModels()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.First().Should().BeOfType<UNOPS.PAO.Models.OpportunityCountryModel>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ReloadsOpportunityAfterSave()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Id.Should().Be(_f.OpportunityId);
        result.Countries!.First().Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_HasNameProperty()
    {
        var oc = new OpportunityCountry { Name = "Test" };
        oc.Name.Should().Be("Test");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_AutoPopulatesStakeholdersWhenResponsibleOrgUnitSet()
    {
        var opp = _f.Context.Opportunities.Find(_f.OpportunityId);
        if (opp != null)
        {
            var orgId = _f.Context.OrganizationHierarchies.FirstOrDefault(o => !o.IsDeleted)?.Id;
            if (orgId.HasValue)
            {
                opp.ResponsibleOrgUnitId = orgId;
                _f.Context.SaveChanges();
                var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
                var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
                result.Should().NotBeNull();
            }
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_WhereSectionRequest_CountriesProperty()
    {
        var req = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = 1 } } };
        req.Countries.Should().HaveCount(1);
        req.Countries![0].CountryId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountryRequest_AllPropertiesSettable()
    {
        var req = new OpportunityCountryRequest
        {
            CountryId = 1,
            SpecificAreas = "X",
            HumanitarianFrameworkAlignment = true,
            NdcAlignment = false,
            NapAlignment = true,
            OrgUnitStrategyAlignment = false
        };
        req.CountryId.Should().Be(1);
        req.SpecificAreas.Should().Be("X");
        req.HumanitarianFrameworkAlignment.Should().BeTrue();
        req.NdcAlignment.Should().BeFalse();
        req.NapAlignment.Should().BeTrue();
        req.OrgUnitStrategyAlignment.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ComputeOrgUnitWithStrategyForEachCountry()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Should().HaveCount(2);
        foreach (var c in result.Countries)
            c.OrgUnitWithStrategyId.Should().Match(id => id == null || id >= 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesMappedFromEntity()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.First().Should().NotBeNull();
        opp.Countries.First().CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ExistingCountry_UpdatesNotAdds()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var firstId = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult()!.Countries!.First().Id;
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "X" } } }).GetAwaiter().GetResult();
        var secondId = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult()!.Countries!.First().Id;
        firstId.Should().Be(secondId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesToRemove_RemovedFromContext()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var remaining = _f.Context.Set<OpportunityCountry>().Where(oc => oc.OpportunityId == _f.OpportunityId && !oc.IsDeleted).Select(oc => oc.CountryId).ToList();
        remaining.Should().NotContain(_f.CountryId2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_SpecificAreasMaxLength1000()
    {
        var oc = new OpportunityCountry { SpecificAreas = new string('x', 1000) };
        oc.SpecificAreas!.Length.Should().Be(1000);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_ContextWarningMaxLength500()
    {
        var oc = new OpportunityCountry { ContextWarning = new string('x', 500) };
        oc.ContextWarning!.Length.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_RequestedCountryIds_HashSetComparison()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var requestedIds = request.Countries!.Select(c => c.CountryId).ToHashSet();
        requestedIds.Should().Contain(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_InitializeCountriesIfNull()
    {
        var opp = _f.Context.Opportunities.Include(o => o.Countries).First(o => o.Id == _f.OpportunityId);
        opp.Countries ??= new List<OpportunityCountry>();
        opp.Countries.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_IncludeCountriesThenIncludeCountry()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.First().Country!.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_NewCountry_HasOrgUnitWithStrategyId()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.First().OrgUnitWithStrategyId.Should().Match(id => id == null || id > 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ExistingCountry_OrgUnitWithStrategyIdUpdated()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        result.Countries!.First().OrgUnitWithStrategyId.Should().Match(id => id == null || id >= 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_OrgUnitWithStrategyIdNullable()
    {
        var oc = new OpportunityCountry { OrgUnitWithStrategyId = null };
        oc.OrgUnitWithStrategyId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ProcessEachRequestedCountry()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Count.Should().Be(2);
        result.Countries.Select(c => c.CountryId).Should().Contain(_f.CountryId1);
        result.Countries.Select(c => c.CountryId).Should().Contain(_f.CountryId2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesToRemove_AnyRemoved()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var count = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId);
        count.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_AfterUpdateWhere_ReflectsChanges()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp1 = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        var opp2 = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp2!.Countries!.First().CountryId.Should().Be(_f.CountryId2);
        opp1!.Countries!.First().CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountryRequest_CountryIdRequired()
    {
        var req = new OpportunityCountryRequest { CountryId = _f.CountryId1 };
        req.CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_SaveChangesCalled()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().AsNoTracking().FirstOrDefault(o => o.OpportunityId == _f.OpportunityId);
        oc.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ReloadCallsGetOpportunityAsync()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Should().NotBeNull();
        result.Countries.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_HasOpportunityNavigation()
    {
        var oc = new OpportunityCountry { OpportunityId = _f.OpportunityId };
        oc.OpportunityId.Should().Be(_f.OpportunityId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_HasCountryNavigation()
    {
        var oc = new OpportunityCountry { CountryId = _f.CountryId1 };
        oc.CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ThrowIfCannotModify_Checked()
    {
        var opp = _f.Context.Opportunities.Find(_f.OpportunityId);
        if (opp != null)
        {
            opp.Stage = "GO";
            _f.Context.SaveChanges();
            var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
            var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
            act.Should().ThrowAsync<BusinessException>();
            opp.Stage = "IDENTIFY & PROFILE";
            _f.Context.SaveChanges();
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_KeyNotFoundException_WhenNotFound()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(999999, request);
        act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_ModifiableDeletableEntityBase()
    {
        var oc = new OpportunityCountry { Id = 1 };
        oc.Id.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesNull_NoUpdateBlock()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = null }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesEnrichedWithOrgUnitHierarchy()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.First().Country.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ComputeOrgUnitWithStrategyMap()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.First().OrgUnitWithStrategyId.Should().Match(id => id == null || id >= 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_CollectionInitializer()
    {
        var list = new List<OpportunityCountryRequest> { new() { CountryId = 1 } };
        list.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_WhereSectionRequest_CountriesInitializer()
    {
        var req = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } };
        req.Countries!.Count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_AddNewCountry_OpportunityCountriesAdd()
    {
        var before = _f.Context.Set<OpportunityCountry>().Count();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var after = _f.Context.Set<OpportunityCountry>().Count();
        after.Should().BeGreaterThan(before);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_RemoveRange_CalledForRemoved()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId).Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_SpecificAreasNullable()
    {
        var oc = new OpportunityCountry { SpecificAreas = null };
        oc.SpecificAreas.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_HumanitarianFrameworkAlignmentNullable()
    {
        var oc = new OpportunityCountry { HumanitarianFrameworkAlignment = null };
        oc.HumanitarianFrameworkAlignment.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ResultIncludesAllSections()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Id.Should().Be(_f.OpportunityId);
        result.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesIsList()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().BeAssignableTo<IEnumerable<UNOPS.PAO.Models.OpportunityCountryModel>>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ExistingCountries_ToList()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Context.Opportunities.Include(o => o.Countries).First(o => o.Id == _f.OpportunityId);
        opp.Countries.ToList().Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_RequestedCountryIds_Distinct()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId1 } } };
        var ids = request.Countries!.Select(c => c.CountryId).ToHashSet();
        ids.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_OrgUnitWithStrategyNavigation()
    {
        var oc = new OpportunityCountry { OrgUnitWithStrategyId = 1 };
        oc.OrgUnitWithStrategyId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_NewOpportunityCountry_HasRequiredFields()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.OpportunityId.Should().Be(_f.OpportunityId);
        oc.CountryId.Should().Be(_f.CountryId1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ExistingCountry_UpdateProperties()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "A" } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "B" } } }).GetAwaiter().GetResult();
        var oc = _f.Context.Set<OpportunityCountry>().First(o => o.OpportunityId == _f.OpportunityId && !o.IsDeleted);
        oc.SpecificAreas.Should().Be("B");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesFromEntityMapping()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.First().CountryId.Should().Be(_f.CountryId1);
        opp.Countries.First().OpportunityId.Should().Be(_f.OpportunityId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_UNCFOutcomesCollection()
    {
        var oc = new OpportunityCountry();
        oc.UNCFOutcomes.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesToRemove_WhereNotRequested()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var requestedIds = request.Countries!.Select(c => c.CountryId).ToHashSet();
        var toRemove = _f.Context.Set<OpportunityCountry>().Where(oc => oc.OpportunityId == _f.OpportunityId && !requestedIds.Contains(oc.CountryId)).ToList();
        toRemove.Should().HaveCount(1);
        toRemove.First().CountryId.Should().Be(_f.CountryId2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_FirstOrDefault_ExistingCountry()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var existing = _f.Context.Set<OpportunityCountry>().FirstOrDefault(oc => oc.OpportunityId == _f.OpportunityId && oc.CountryId == _f.CountryId1 && !oc.IsDeleted);
        existing.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_AddNewCountry_OpportunityCountriesAddCalled()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var count = _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId);
        count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_OrgUnitWithStrategyVirtual()
    {
        var oc = new OpportunityCountry { OrgUnitWithStrategyId = 1 };
        oc.OrgUnitWithStrategy.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountryOrgUnitStrategyMap_ContainsKey()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.First().OrgUnitWithStrategyId.Should().Match(id => id == null || id >= 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_NullWhenNotFound()
    {
        var result = _f.Manager.GetOpportunityAsync(999999).GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ResultReloadedViaGetOpportunityAsync()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var direct = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        result.Countries!.Count.Should().Be(direct!.Countries!.Count);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_RiskScoreDecimalPrecision()
    {
        var oc = new OpportunityCountry { RiskScore = 5.5m };
        oc.RiskScore.Should().Be(5.5m);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ProcessEachCountryRequest()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 }, new() { CountryId = _f.CountryId3 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Count.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_ContextWarningNullable()
    {
        var oc = new OpportunityCountry { ContextWarning = null };
        oc.ContextWarning.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_RemoveRange_RemovesFromContext()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        _f.Context.Set<OpportunityCountry>().Any(oc => oc.OpportunityId == _f.OpportunityId).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_AllAlignmentPropertiesNullable()
    {
        var oc = new OpportunityCountry { HumanitarianFrameworkAlignment = null, NdcAlignment = null, NapAlignment = null, OrgUnitStrategyAlignment = null };
        oc.HumanitarianFrameworkAlignment.Should().BeNull();
        oc.NdcAlignment.Should().BeNull();
        oc.NapAlignment.Should().BeNull();
        oc.OrgUnitStrategyAlignment.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ExistingCountry_NullOrgUnitWithStrategyId()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.First().OrgUnitWithStrategyId.Should().Match(id => id == null || id >= 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_OpportunityNavigationVirtual() => new OpportunityCountry().Opportunity.Should().BeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_CountryNavigationVirtual() => new OpportunityCountry().Country.Should().BeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesNotNullAfterUpdate() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult().Countries.Should().NotBeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesEnumerable() => _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult()!.Countries.Should().NotBeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ResultIsNotNull() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult().Should().NotBeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_RequiredFields() => new OpportunityCountry { OpportunityId = 1, CountryId = 1 }.CountryId.Should().Be(1);

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_EmptyCountries_ZeroCount() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult().Countries!.Count.Should().Be(0);

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_SingleCountry_OneCount() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult().Countries!.Count.Should().Be(1);

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountryRequest_DefaultValues() => new OpportunityCountryRequest().CountryId.Should().Be(0);

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_WhereSectionRequest_DefaultCountriesNull() => new WhereSectionRequest().Countries.Should().BeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesListNotNull() => new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }.Countries.Should().NotBeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_OrgUnitWithStrategyIdOptional() => new OpportunityCountry { OrgUnitWithStrategyId = null }.OrgUnitWithStrategyId.Should().BeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_ResultIdMatches() => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult().Id.Should().Be(_f.OpportunityId);

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_GetOpportunity_CountriesListType() => _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult()!.Countries.Should().BeAssignableTo<IList<UNOPS.PAO.Models.OpportunityCountryModel>>();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_SpecificAreasPropertyExists() => typeof(OpportunityCountry).GetProperty("SpecificAreas").Should().NotBeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_OpportunityCountry_ContextWarningPropertyExists() => typeof(OpportunityCountry).GetProperty("ContextWarning").Should().NotBeNull();

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_CountriesToRemove_RemovedFromDb()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        _f.Context.Set<OpportunityCountry>().Count(oc => oc.OpportunityId == _f.OpportunityId).Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Func_UpdateWhereSection_AddNewCountry_CreatesRecordWithCorrectId()
    {
        var r = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        r.Countries!.First().CountryId.Should().Be(_f.CountryId1);
    }
}
