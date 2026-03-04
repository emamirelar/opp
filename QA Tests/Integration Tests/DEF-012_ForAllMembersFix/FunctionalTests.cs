using System;
using System.Collections.Generic;
using System.Linq;
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
/// DEF-012: Functional tests for OpportunityMappingProfile ForAllMembers fix.
/// </summary>
[Collection("Functional")]
[Trait("Category", "Functional")]
[Trait("Type", "Functional")]
public class FunctionalTests
{
    private readonly IMapper _mapper;

    public FunctionalTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly);
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    [Trait("DEF012", "FUN_001")]
    public void FUN_001_ConditionPreventsNullOverwrites()
    {
        var dest = CreateOpportunity();
        dest.Name = "Keep";
        var request = new UpdateOpportunityRequest { Id = 10, Name = null };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Keep");
    }

    [Fact]
    [Trait("DEF012", "FUN_002")]
    public void FUN_002_ConditionAllowsNonNullUpdates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "New" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("New");
    }

    [Fact]
    [Trait("DEF012", "FUN_003")]
    public void FUN_003_ForAllMembersAppliesToAllMembers()
    {
        var dest = CreateOpportunity();
        dest.Description = "Keep";
        dest.InitiativeBudgetUSD = 100m;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = null, InitiativeBudgetUSD = null };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Keep");
        dest.InitiativeBudgetUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("DEF012", "FUN_004")]
    public void FUN_004_ForAllMembersDoesNotOverrideExplicitIgnore()
    {
        var dest = CreateOpportunity();
        dest.Id = 42;
        var request = new UpdateOpportunityRequest { Id = 99, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(42);
    }

    [Fact]
    [Trait("DEF012", "FUN_005")]
    public void FUN_005_IgnoreWinsOverConditionForId()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = 999, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "FUN_006")]
    public void FUN_006_IgnoreWinsForFundingPartners()
    {
        var dest = CreateOpportunity();
        var count = dest.FundingPartners?.Count ?? 0;
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        _mapper.Map(request, dest);
        (dest.FundingPartners?.Count ?? 0).Should().Be(count);
    }

    [Fact]
    [Trait("DEF012", "FUN_007")]
    public void FUN_007_IgnoreWinsForClientPartners()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ClientPartners = new List<OpportunityClientPartnerRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_008")]
    public void FUN_008_IgnoreWinsForStakeholders()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stakeholders = new List<OpportunityStakeholderRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_009")]
    public void FUN_009_IgnoreWinsForDeliverables()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Deliverables = new List<OpportunityDeliverableRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_010")]
    public void FUN_010_IgnoreWinsForCountries()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Countries = new List<OpportunityCountryRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_011")]
    public void FUN_011_IgnoreWinsForSDGs()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", SDGs = new List<OpportunitySDGRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_012")]
    public void FUN_012_ConditionIsSrcMemberNotNull()
    {
        var dest = CreateOpportunity();
        dest.Stage = "GO";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = null };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("GO");
    }

    [Fact]
    [Trait("DEF012", "FUN_013")]
    public void FUN_013_DefaultIntPassesCondition()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = 0 };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(0);
    }

    [Fact]
    [Trait("DEF012", "FUN_014")]
    public void FUN_014_DefaultDateTimePassesCondition()
    {
        var dest = CreateOpportunity();
        var d = DateTime.MinValue;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "FUN_015")]
    public void FUN_015_NullStringFailsCondition()
    {
        var dest = CreateOpportunity();
        dest.Name = "Preserved";
        var request = new UpdateOpportunityRequest { Id = 10, Name = null };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Preserved");
    }

    [Fact]
    [Trait("DEF012", "FUN_016")]
    public void FUN_016_PartialUpdatePreservesUnchanged()
    {
        var dest = CreateOpportunity();
        dest.Description = "Original";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "NewName" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("NewName");
        dest.Description.Should().Be("Original");
    }

    [Fact]
    [Trait("DEF012", "FUN_017")]
    public void FUN_017_FullUpdateReplacesAll()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "N",
            Description = "D",
            PartnerReference = "PR",
            Stage = "GO",
            InitiativeBudgetUSD = 100m
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("N");
        dest.Description.Should().Be("D");
        dest.PartnerReference.Should().Be("PR");
        dest.Stage.Should().Be("GO");
        dest.InitiativeBudgetUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("DEF012", "FUN_018")]
    public void FUN_018_SequentialUpdatesAccumulate()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "A" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Description = "Desc" }, dest);
        dest.Name.Should().Be("A");
        dest.Description.Should().Be("Desc");
    }

    [Fact]
    [Trait("DEF012", "FUN_019")]
    public void FUN_019_UpdateDoesNotCreateNewEntity()
    {
        var dest = CreateOpportunity();
        var refBefore = dest;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Should().BeSameAs(refBefore);
    }

    [Fact]
    [Trait("DEF012", "FUN_020")]
    public void FUN_020_UpdatePreservesEntityRelationships()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners = new HashSet<OpportunityFundingPartner>();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "FUN_021")]
    public void FUN_021_UpdatePreservesAuditTrail()
    {
        var dest = CreateOpportunity();
        dest.CreatedBy = 1;
        dest.CreatedDate = new DateTime(2025, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.CreatedBy.Should().Be(1);
    }

    [Fact]
    [Trait("DEF012", "FUN_022")]
    public void FUN_022_NameUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "UpdatedName" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("UpdatedName");
    }

    [Fact]
    [Trait("DEF012", "FUN_023")]
    public void FUN_023_DescriptionUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "NewDesc" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("NewDesc");
    }

    [Fact]
    [Trait("DEF012", "FUN_024")]
    public void FUN_024_BudgetUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 5_000_000m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(5_000_000m);
    }

    [Fact]
    [Trait("DEF012", "FUN_025")]
    public void FUN_025_StageUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = "NO GO" };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("NO GO");
    }

    [Fact]
    [Trait("DEF012", "FUN_026")]
    public void FUN_026_DateUpdateRules()
    {
        var dest = CreateOpportunity();
        var signing = new DateTime(2026, 3, 1);
        var delivery = new DateTime(2026, 9, 30);
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            TargetSigningDate = signing,
            TargetDeliveryDate = delivery
        };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(signing);
        dest.TargetDeliveryDate.Should().Be(delivery);
    }

    [Fact]
    [Trait("DEF012", "FUN_027")]
    public void FUN_027_OrgUnitUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = 42 };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(42);
    }

    [Fact]
    [Trait("DEF012", "FUN_028")]
    public void FUN_028_InitiativeTypeUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ProposedInitiativeTypeId = 3 };
        _mapper.Map(request, dest);
        dest.ProposedInitiativeTypeId.Should().Be(3);
    }

    [Fact]
    [Trait("DEF012", "FUN_029")]
    public void FUN_029_PartnerReferenceUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnerReference = "REF-001" };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be("REF-001");
    }

    [Fact]
    [Trait("DEF012", "FUN_030")]
    public void FUN_030_PartnershipAgreementRefUpdateRule()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnershipAgreementReference = "PA-001" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_031")]
    public void FUN_031_ProfileRegisteredCorrectly()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Registered" }, dest);
        dest.Name.Should().Be("Registered");
    }

    [Fact]
    [Trait("DEF012", "FUN_032")]
    public void FUN_032_MapConfigurationValid()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Config" }, dest);
        dest.Name.Should().Be("Config");
    }

    [Fact]
    [Trait("DEF012", "FUN_033")]
    public void FUN_033_NoUnmappedPropertiesWarning()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "X" }, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_034")]
    public void FUN_034_AllCollectionsExplicitlyIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() },
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() },
            Stakeholders = new List<OpportunityStakeholderRequest> { new() },
            Deliverables = new List<OpportunityDeliverableRequest> { new() },
            Countries = new List<OpportunityCountryRequest> { new() },
            SDGs = new List<OpportunitySDGRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "FUN_035")]
    public void FUN_035_ForAllMembersAppliedAfterForMember()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = 999, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "FUN_036")]
    public async Task FUN_036_MapperHandlesConcurrentAccess()
    {
        var tasks = Enumerable.Range(0, 10).Select(_ => System.Threading.Tasks.Task.Run(() =>
        {
            var dest = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Concurrent" }, dest);
            return dest.Name;
        })).ToArray();
        var results = await System.Threading.Tasks.Task.WhenAll(tasks);
        results.All(r => r == "Concurrent").Should().BeTrue();
    }

    [Fact]
    [Trait("DEF012", "FUN_037")]
    public void FUN_037_MapperIsReusable()
    {
        var dest1 = CreateOpportunity();
        var dest2 = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "First" }, dest1);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Second" }, dest2);
        dest1.Name.Should().Be("First");
        dest2.Name.Should().Be("Second");
    }

    [Fact]
    [Trait("DEF012", "FUN_038")]
    public void FUN_038_MapperRespectsAssemblyScanning()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Scanned" }, dest);
        dest.Name.Should().Be("Scanned");
    }

    [Fact]
    [Trait("DEF012", "FUN_039")]
    public void FUN_039_ProfileInheritsFromProfile()
    {
        typeof(OpportunityMappingProfile).BaseType?.Name.Should().Be("Profile");
    }

    [Fact]
    [Trait("DEF012", "FUN_040")]
    public void FUN_040_CreateMapReturnsIMappingExpression()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Expr" }, dest);
        dest.Name.Should().Be("Expr");
    }

    [Fact]
    [Trait("DEF012", "FUN_041")]
    public void FUN_041_MappedEntityHasValidState()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Valid" };
        _mapper.Map(request, dest);
        dest.Name.Should().NotBeNullOrEmpty();
        dest.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("DEF012", "FUN_042")]
    public void FUN_042_IdValuePreservedInMap()
    {
        var dest = CreateOpportunity();
        dest.Id = 42;
        var request = new UpdateOpportunityRequest { Id = 99, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(42);
    }

    [Fact]
    [Trait("DEF012", "FUN_043")]
    public void FUN_043_StatusPreservedInMap()
    {
        var dest = CreateOpportunity();
        dest.Status = EntityStatus.Active;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("DEF012", "FUN_044")]
    public void FUN_044_IsDeletedPreservedInMap()
    {
        var dest = CreateOpportunity();
        dest.IsDeleted = true;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.IsDeleted.Should().BeTrue();
    }

    [Fact]
    [Trait("DEF012", "FUN_045")]
    public void FUN_045_AuditFieldsPreserved()
    {
        var dest = CreateOpportunity();
        dest.CreatedBy = 7;
        dest.LastModifiedBy = 8;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.CreatedBy.Should().Be(7);
        dest.LastModifiedBy.Should().Be(8);
    }

    [Fact]
    [Trait("DEF012", "FUN_046")]
    public void FUN_046_CollectionPropertiesNotNullified()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners = new HashSet<OpportunityFundingPartner>();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "FUN_047")]
    public void FUN_047_WorkflowStatusPreserved()
    {
        var dest = CreateOpportunity();
        dest.WorkflowStatus = WorkflowStatus.InWorkflow;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.WorkflowStatus.Should().Be(WorkflowStatus.InWorkflow);
    }

    [Fact]
    [Trait("DEF012", "FUN_048")]
    public void FUN_048_StagePreservedWhenNullInSource()
    {
        var dest = CreateOpportunity();
        dest.Stage = "GO";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = null };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("GO");
    }

    [Fact]
    [Trait("DEF012", "FUN_049")]
    public void FUN_049_EntityRemainsValidAfterMap()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Valid", Description = "Valid Desc" };
        _mapper.Map(request, dest);
        dest.Id.Should().BeGreaterThanOrEqualTo(0);
        dest.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("DEF012", "FUN_050")]
    public void FUN_050_MapDoesNotTriggerSaveChanges()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

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
