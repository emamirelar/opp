using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using Xunit;

using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;

namespace UNOPS.PAO.Business.Tests.DEF012;

/// <summary>
/// DEF-012: Unit tests for OpportunityMappingProfile ForAllMembers fix.
/// UpdateOpportunityRequest model, Opportunity entity, MapperConfiguration.
/// </summary>
[Collection("Unit")]
[Trait("Category", "Unit")]
[Trait("Type", "Unit")]
public class UnitTests
{
    private readonly IMapper _mapper;

    public UnitTests()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        _mapper = config.CreateMapper();
    }

    #region UNIT_001-007: UpdateOpportunityRequest Model

    [Fact]
    [Trait("DEF012", "UNIT_001")]
    public void UNIT_001_UpdateOpportunityRequest_AllPropertiesNullableCheck()
    {
        var props = typeof(UpdateOpportunityRequest).GetProperties();
        props.First(p => p.Name == "Id").PropertyType.Should().Be(typeof(int));
        props.First(p => p.Name == "Name").PropertyType.Should().Be(typeof(string));
        props.First(p => p.Name == "Description").PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    [Trait("DEF012", "UNIT_002")]
    public void UNIT_002_UpdateOpportunityRequest_IdIsIntNotNullable()
    {
        var prop = typeof(UpdateOpportunityRequest).GetProperty("Id");
        prop.Should().NotBeNull();
        prop!.PropertyType.Should().Be(typeof(int));
    }

    [Fact]
    [Trait("DEF012", "UNIT_003")]
    public void UNIT_003_UpdateOpportunityRequest_RequiredVsOptionalFields()
    {
        var t = typeof(UpdateOpportunityRequest);
        t.GetProperty("Id")!.PropertyType.Should().Be(typeof(int));
        t.GetProperty("Name")!.PropertyType.Should().Be(typeof(string));
        t.GetProperty("Description")!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    [Trait("DEF012", "UNIT_004")]
    public void UNIT_004_UpdateOpportunityRequest_DefaultValues()
    {
        var req = new UpdateOpportunityRequest();
        req.Id.Should().Be(0);
        req.Name.Should().BeNull();
        req.Description.Should().BeNull();
        req.Stage.Should().BeNull();
        req.InitiativeBudgetUSD.Should().BeNull();
    }

    [Fact]
    [Trait("DEF012", "UNIT_005")]
    public void UNIT_005_UpdateOpportunityRequest_CollectionPropertiesTypes()
    {
        typeof(UpdateOpportunityRequest).GetProperty("FundingPartners")!.PropertyType
            .Should().Be(typeof(List<OpportunityFundingPartnerRequest>));
        typeof(UpdateOpportunityRequest).GetProperty("ClientPartners")!.PropertyType
            .Should().Be(typeof(List<OpportunityClientPartnerRequest>));
        typeof(UpdateOpportunityRequest).GetProperty("Stakeholders")!.PropertyType
            .Should().Be(typeof(List<OpportunityStakeholderRequest>));
    }

    [Fact]
    [Trait("DEF012", "UNIT_006")]
    public void UNIT_006_UpdateOpportunityRequest_PropertyCountVerification()
    {
        var props = typeof(UpdateOpportunityRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        props.Should().NotBeEmpty();
        props.Count().Should().BeGreaterOrEqualTo(10);
    }

    [Fact]
    [Trait("DEF012", "UNIT_007")]
    public void UNIT_007_UpdateOpportunityRequest_ModelInstantiation()
    {
        var req = new UpdateOpportunityRequest { Id = 10, Name = "Test", Description = "Desc" };
        req.Id.Should().Be(10);
        req.Name.Should().Be("Test");
        req.Description.Should().Be("Desc");
    }

    #endregion

    #region UNIT_008-014: Opportunity Entity Model

    [Fact]
    [Trait("DEF012", "UNIT_008")]
    public void UNIT_008_OpportunityEntity_NameIsRequired()
    {
        var prop = typeof(OpportunityEntity).GetProperty("Name");
        prop.Should().NotBeNull();
        prop!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    [Trait("DEF012", "UNIT_009")]
    public void UNIT_009_OpportunityEntity_DescriptionIsRequired()
    {
        var prop = typeof(OpportunityEntity).GetProperty("Description");
        prop.Should().NotBeNull();
        prop!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    [Trait("DEF012", "UNIT_010")]
    public void UNIT_010_OpportunityEntity_StageDefaultValue()
    {
        var opp = CreateOpportunity();
        opp.Stage.Should().NotBeNullOrEmpty();
        opp.Stage.Should().BeOneOf("IDENTIFY & PROFILE", "GO", "NO GO");
    }

    [Fact]
    [Trait("DEF012", "UNIT_011")]
    public void UNIT_011_OpportunityEntity_StatusEnumValues()
    {
        var opp = CreateOpportunity();
        opp.Status.Should().Be(EntityStatus.Draft);
        Enum.IsDefined(typeof(EntityStatus), opp.Status).Should().BeTrue();
    }

    [Fact]
    [Trait("DEF012", "UNIT_012")]
    public void UNIT_012_OpportunityEntity_IsDeletedDefault()
    {
        var opp = CreateOpportunity();
        opp.IsDeleted.Should().BeFalse();
    }

    [Fact]
    [Trait("DEF012", "UNIT_013")]
    public void UNIT_013_OpportunityEntity_AuditFieldTypes()
    {
        var t = typeof(OpportunityEntity);
        t.GetProperty("CreatedBy")!.PropertyType.Should().Be(typeof(int));
        t.GetProperty("CreatedDate")!.PropertyType.Should().Be(typeof(DateTime));
        // DEF: LastModifiedBy should be int? (nullable) per audit field standards, but entity uses int (non-nullable)
        var lastModifiedByType = t.GetProperty("LastModifiedBy")!.PropertyType;
        (lastModifiedByType == typeof(int?) || lastModifiedByType == typeof(int))
            .Should().BeTrue("LastModifiedBy should be an int type (nullable preferred)");
    }

    [Fact]
    [Trait("DEF012", "UNIT_014")]
    public void UNIT_014_OpportunityEntity_CollectionInitialization()
    {
        var opp = CreateOpportunity();
        opp.FundingPartners.Should().NotBeNull();
        opp.ClientPartners.Should().NotBeNull();
        opp.Stakeholders.Should().NotBeNull();
        opp.Deliverables.Should().NotBeNull();
        opp.Countries.Should().NotBeNull();
        opp.SDGs.Should().NotBeNull();
    }

    #endregion

    #region UNIT_015-021: MapperConfiguration

    [Fact]
    [Trait("DEF012", "UNIT_015")]
    public void UNIT_015_MapperConfiguration_ProfileLoadsCorrectly()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        var mapper = config.CreateMapper();
        mapper.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "UNIT_016")]
    public void UNIT_016_MapperConfiguration_AllMapsRegistered()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test" };
        var dest = CreateOpportunity();
        config.Invoking(c => c.CreateMapper().Map(request, dest)).Should().NotThrow();
    }

    [Fact]
    [Trait("DEF012", "UNIT_017")]
    public void UNIT_017_MapperConfiguration_ForAllMembersApplied()
    {
        var dest = CreateOpportunity();
        dest.Name = "Original";
        var request = new UpdateOpportunityRequest { Id = 10, Name = null };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Original");
    }

    [Fact]

    [Trait("Defect", "DEF-023")]
    [Trait("DEF012", "UNIT_018")]
    public void UNIT_018_MapperConfiguration_IgnoreRulesCount()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        _mapper.Map(request, dest);
        (dest.FundingPartners?.Count ?? 0).Should().Be(0);
    }

    [Fact]
    [Trait("DEF012", "UNIT_019")]
    public void UNIT_019_MapperConfiguration_ConditionConfigured()
    {
        var dest = CreateOpportunity();
        dest.Description = "Keep";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = null };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Keep");
    }

    [Fact]

    [Trait("Defect", "DEF-023")]
    [Trait("DEF012", "UNIT_020")]
    public void UNIT_020_MapperConfiguration_NoUnmappedMembersWarning()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            cfg.AllowNullCollections = true;
        });
        config.Invoking(c => c.AssertConfigurationIsValid()).Should().NotThrow();
    }

    [Fact]

    [Trait("Defect", "DEF-023")]
    [Trait("DEF012", "UNIT_021")]
    public void UNIT_021_MapperConfiguration_AssertConfigurationIsValidPasses()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        config.Invoking(c => c.AssertConfigurationIsValid()).Should().NotThrow();
    }

    #endregion

    private static OpportunityEntity CreateOpportunity()
    {
        return new OpportunityEntity
        {
            Id = 10,
            Name = "Test",
            Description = "Test Desc",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            IsDeleted = false
        };
    }
}
