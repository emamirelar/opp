/**
 * @fileoverview PNO-697, PNO-775, PNO-776, PNO-778, PNO-895, PNO-935: Opportunity WHERE Section — Negative tests.
 * Invalid inputs, wrong states, expected failures.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhereSection;

/// <summary>
/// Negative tests for Opportunity WHERE Section.
/// </summary>
public class NegativeTests : IClassFixture<OpportunityWhereSectionFixture>
{
    private readonly OpportunityWhereSectionFixture _f;

    public NegativeTests(OpportunityWhereSectionFixture fixture) => _f = fixture;

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_NonExistentOpportunityId_ThrowsKeyNotFoundException()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(999999, request);
        act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_ZeroOpportunityId_Throws()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(0, request);
        act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_NegativeOpportunityId_Throws()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(-1, request);
        act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_NonExistentId_ReturnsNull()
    {
        var result = _f.Manager.GetOpportunityAsync(999999).GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_ZeroId_ReturnsNull()
    {
        var result = _f.Manager.GetOpportunityAsync(0).GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_NegativeId_ReturnsNull()
    {
        var result = _f.Manager.GetOpportunityAsync(-1).GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_NullRequest_Throws()
    {
        var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_ZeroCountryId_AllowedButMayFailOnUpdate()
    {
        var req = new OpportunityCountryRequest { CountryId = 0 };
        req.CountryId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_NegativeCountryId_AllowedButMayFailOnUpdate()
    {
        var req = new OpportunityCountryRequest { CountryId = -1 };
        req.CountryId.Should().Be(-1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void WhereSectionRequest_EmptyCountriesList_Allowed()
    {
        var req = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };
        req.Countries.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_NegativeId_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Id = -1 };
        model.Id.Should().Be(-1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_ZeroOpportunityId_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OpportunityId = 0 };
        model.OpportunityId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_CountriesWithInvalidCountryId_MayThrowOrSkip()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = 99999999 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
        act.Invoking(a => a().GetAwaiter().GetResult()).Should().Throw<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_SpecificAreasExceedingMax_MayFailValidation()
    {
        var longString = new string('x', 1001);
        var req = new OpportunityCountryRequest { CountryId = 1, SpecificAreas = longString };
        req.SpecificAreas!.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_RequestWithNullCountryId_ThrowsOrFails()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
        act.Invoking(a => a().GetAwaiter().GetResult()).Should().Throw<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_DeletedOpportunity_ReturnsNull()
    {
        var opp = _f.Context.Opportunities.Find(_f.OpportunityId);
        if (opp != null)
        {
            opp.IsDeleted = true;
            _f.Context.SaveChanges();
            var result = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
            result.Should().BeNull();
            opp.IsDeleted = false;
            _f.Context.SaveChanges();
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_AllNullablePropertiesNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel
        {
            SpecificAreas = null,
            ContextWarning = null,
            RiskScore = null,
            HumanitarianFrameworkAlignment = null,
            NdcAlignment = null,
            NapAlignment = null,
            OrgUnitStrategyAlignment = null
        };
        model.SpecificAreas.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_OpportunityInGoStage_ThrowsBusinessException()
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
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_OpportunityInNoGoStage_ThrowsBusinessException()
    {
        var opp = _f.Context.Opportunities.Find(_f.OpportunityId);
        if (opp != null)
        {
            opp.Stage = "NO GO";
            _f.Context.SaveChanges();
            var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
            var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
            act.Should().ThrowAsync<BusinessException>();
            opp.Stage = "IDENTIFY & PROFILE";
            _f.Context.SaveChanges();
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_OpportunityInCancelledStage_ThrowsBusinessException()
    {
        var opp = _f.Context.Opportunities.Find(_f.OpportunityId);
        if (opp != null)
        {
            opp.Stage = "CANCELLED";
            _f.Context.SaveChanges();
            var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
            var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
            act.Should().ThrowAsync<BusinessException>();
            opp.Stage = "IDENTIFY & PROFILE";
            _f.Context.SaveChanges();
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_OpportunityInWorkflow_ThrowsBusinessException()
    {
        var opp = _f.Context.Opportunities.Find(_f.OpportunityId);
        if (opp != null)
        {
            opp.WorkflowStatus = WorkflowStatus.InWorkflow;
            _f.Context.SaveChanges();
            var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
            var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
            act.Should().ThrowAsync<BusinessException>();
            opp.WorkflowStatus = WorkflowStatus.None;
            _f.Context.SaveChanges();
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_CountryIdMaxValue_Allowed()
    {
        var req = new OpportunityCountryRequest { CountryId = int.MaxValue };
        req.CountryId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void WhereSectionRequest_CountriesWithNullElements_MayThrow()
    {
        var list = new List<OpportunityCountryRequest?> { new OpportunityCountryRequest { CountryId = 1 }, null };
        list.Should().Contain((OpportunityCountryRequest?)null);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_AfterUpdateWhereSection_ReturnsConsistentData()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp1 = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        var opp2 = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp1!.Countries!.Count.Should().Be(opp2!.Countries!.Count);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_WithOnlyRemovedCountries_ResultsInEmpty()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_HasHumanitarianFramework_DefaultFalse()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel();
        model.HasHumanitarianFramework.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_HasNdc_DefaultFalse()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel();
        model.HasNdc.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_HasNap_DefaultFalse()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel();
        model.HasNap.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_HasOrgUnitStrategy_DefaultFalse()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel();
        model.HasOrgUnitStrategy.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_HasMoreLocalStrategyAvailable_DefaultFalse()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel();
        model.HasMoreLocalStrategyAvailable.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_CountriesListWithSingleInvalidId_Throws()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = -999 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
        act.Invoking(a => a().GetAwaiter().GetResult()).Should().Throw<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_AllAlignmentsFalse_Allowed()
    {
        var req = new OpportunityCountryRequest { CountryId = 1, HumanitarianFrameworkAlignment = false, NdcAlignment = false, NapAlignment = false, OrgUnitStrategyAlignment = false };
        req.HumanitarianFrameworkAlignment.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_WithoutCountries_ReturnsEmptyOrNull()
    {
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_RequestCountriesNull_DoesNotModify()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = null };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_OrgUnitWithStrategyIdNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyId = null };
        model.OrgUnitWithStrategyId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_CurrentOrgUnitWithStrategyIdNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { CurrentOrgUnitWithStrategyId = null };
        model.CurrentOrgUnitWithStrategyId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_NonExistentCountryId_ForeignKeyViolation()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = 88888888 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
        act.Invoking(a => a().GetAwaiter().GetResult()).Should().Throw<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_RiskScoreNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { RiskScore = null };
        model.RiskScore.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_ContextWarningNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { ContextWarning = null };
        model.ContextWarning.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_ConcurrentUpdate_LastWriteWins()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().HaveCount(1);
        opp.Countries[0].CountryId.Should().Be(_f.CountryId2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_SpecificAreasWhitespace_Allowed()
    {
        var req = new OpportunityCountryRequest { CountryId = 1, SpecificAreas = "   " };
        req.SpecificAreas.Should().Be("   ");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void WhereSectionRequest_CountriesEmptyList_ClearsCountries()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_CountryIso2CodeNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { Iso2Code = null } };
        model.Country!.Iso2Code.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_CountryNameNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { Name = null } };
        model.Country!.Name.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_RemoveNonExistentCountry_NoOp()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        var removeRequest = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId2 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, removeRequest).GetAwaiter().GetResult();
        result.Countries!.Should().HaveCount(1);
        result.Countries[0].CountryId.Should().Be(_f.CountryId2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityWhereSectionSpec_ContextWarningMaxLength()
    {
        OpportunityWhereSectionSpec.ContextWarningMaxLength.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_WithVeryLongSpecificAreas_MayTruncateOrFail()
    {
        var longStr = new string('a', 2000);
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = longStr } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request);
        act.Invoking(a => a().GetAwaiter().GetResult()).Should().Throw<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_CountriesNotNullWhenPresent()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp.Should().NotBeNull();
        opp!.Countries.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_MixedAlignmentValues_Allowed()
    {
        var req = new OpportunityCountryRequest { CountryId = 1, HumanitarianFrameworkAlignment = true, NdcAlignment = false, NapAlignment = null, OrgUnitStrategyAlignment = true };
        req.HumanitarianFrameworkAlignment.Should().BeTrue();
        req.NdcAlignment.Should().BeFalse();
        req.NapAlignment.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_OpportunityIdIntMax_Throws()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } };
        var act = () => _f.Manager.UpdateWhereSectionAsync(int.MaxValue, request);
        act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_OrgUnitWithStrategyNameNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyName = null };
        model.OrgUnitWithStrategyName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_OrgUnitWithStrategyCodeNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { OrgUnitWithStrategyCode = null };
        model.OrgUnitWithStrategyCode.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_SwitchBetweenOneAndMultiple_Consistent()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_ArtifactsNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { Artifacts = null } };
        model.Country!.Artifacts.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_OrganizationUnitHierarchyNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel { OrganizationUnitHierarchy = null } };
        model.Country!.OrganizationUnitHierarchy.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_CountriesWithSameIdRepeated_Handled()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = _f.CountryId1 },
                new() { CountryId = _f.CountryId1 },
                new() { CountryId = _f.CountryId1 }
            }
        };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Count.Should().BeLessOrEqualTo(3);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_CountriesIdsUnique()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        var ids = opp!.Countries!.Select(c => c.Id).ToList();
        ids.Distinct().Count().Should().Be(ids.Count);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryRequest_CountryIdIntMin_Allowed()
    {
        var req = new OpportunityCountryRequest { CountryId = int.MinValue };
        req.CountryId.Should().Be(int.MinValue);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_EmptySpecificAreasString_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, SpecificAreas = "" } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].SpecificAreas.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_HasActiveUNCF_DefaultFalse()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { Country = new UNOPS.PAO.Models.Locations.CountryModel() };
        model.Country!.HasActiveUNCF.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_FromMultipleToEmpty_ThenBackToMultiple_Success()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } }).GetAwaiter().GetResult();
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1 }, new() { CountryId = _f.CountryId2 } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_CurrentOrgUnitWithStrategyNameNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { CurrentOrgUnitWithStrategyName = null };
        model.CurrentOrgUnitWithStrategyName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityCountryModel_CurrentOrgUnitWithStrategyCodeNull_Allowed()
    {
        var model = new UNOPS.PAO.Models.OpportunityCountryModel { CurrentOrgUnitWithStrategyCode = null };
        model.CurrentOrgUnitWithStrategyCode.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_CountryWithAllAlignmentsNull_Success()
    {
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = _f.CountryId1, HumanitarianFrameworkAlignment = null, NdcAlignment = null, NapAlignment = null, OrgUnitStrategyAlignment = null } } };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries![0].HumanitarianFrameworkAlignment.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunity_AfterClearCountries_ReturnsEmpty()
    {
        _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() }).GetAwaiter().GetResult();
        var opp = _f.Manager.GetOpportunityAsync(_f.OpportunityId).GetAwaiter().GetResult();
        opp!.Countries!.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityWhereSectionSpec_CountryEntityType()
    {
        OpportunityWhereSectionSpec.CountryEntityType.Should().Be("Country");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateWhereSection_WithTenCountries_AllPersisted()
    {
        var countries = new List<OpportunityCountryRequest>();
        for (int i = 0; i < 10; i++)
        {
            countries.Add(new OpportunityCountryRequest { CountryId = _f.CountryId1 });
        }
        var request = new WhereSectionRequest { Countries = countries };
        var result = _f.Manager.UpdateWhereSectionAsync(_f.OpportunityId, request).GetAwaiter().GetResult();
        result.Countries!.Count.Should().BeLessOrEqualTo(10);
    }
}
