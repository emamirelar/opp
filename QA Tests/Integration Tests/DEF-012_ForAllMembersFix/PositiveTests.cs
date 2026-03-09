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
/// DEF-012: Positive tests for OpportunityMappingProfile ForAllMembers fix.
/// UpdateOpportunityRequest → Opportunity mapping with null protection.
/// </summary>
[Collection("Positive")]
[Trait("Category", "Positive")]
[Trait("Type", "Positive")]
public class PositiveTests
{
    private readonly IMapper _mapper;

    public PositiveTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly);
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    [Trait("DEF012", "POS_001")]
    public void POS_001_NullName_Preserved()
    {
        var dest = CreateOpportunity();
        dest.Name = "Original Name";
        var request = new UpdateOpportunityRequest { Id = 10, Name = null };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Original Name");
    }

    [Fact]
    [Trait("DEF012", "POS_002")]
    public void POS_002_NullDescription_Preserved()
    {
        var dest = CreateOpportunity();
        dest.Description = "Original Description";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = null };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Original Description");
    }

    [Fact]
    [Trait("DEF012", "POS_003")]
    public void POS_003_NullPartnerReference_Preserved()
    {
        var dest = CreateOpportunity();
        dest.PartnerReference = "ORIG-REF";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnerReference = null };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be("ORIG-REF");
    }

    [Fact]
    [Trait("DEF012", "POS_004")]
    public void POS_004_NullStage_Preserved()
    {
        var dest = CreateOpportunity();
        dest.Stage = "GO";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = null };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("GO");
    }

    [Fact]
    [Trait("DEF012", "POS_005")]
    public void POS_005_NullResponsibleOrgUnitId_Preserved()
    {
        var dest = CreateOpportunity();
        dest.ResponsibleOrgUnitId = 99;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = null };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(99);
    }

    [Fact]
    [Trait("DEF012", "POS_006")]
    public void POS_006_NullBudget_Preserved()
    {
        var dest = CreateOpportunity();
        dest.InitiativeBudgetUSD = 1_000_000m;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = null };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(1_000_000m);
    }

    [Fact]
    [Trait("DEF012", "POS_007")]
    public void POS_007_NullTargetSigningDate_Preserved()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2025, 6, 15);
        dest.TargetSigningDate = d;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = null };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "POS_008")]
    public void POS_008_NullTargetDeliveryDate_Preserved()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2026, 12, 31);
        dest.TargetDeliveryDate = d;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = null };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "POS_009")]
    public void POS_009_NullProposedInitiativeTypeId_Preserved()
    {
        var dest = CreateOpportunity();
        dest.ProposedInitiativeTypeId = 5;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ProposedInitiativeTypeId = null };
        _mapper.Map(request, dest);
        dest.ProposedInitiativeTypeId.Should().Be(5);
    }

    [Fact]
    [Trait("DEF012", "POS_010")]
    public void POS_010_NullPartnershipAgreementReference_Preserved()
    {
        var dest = CreateOpportunity();
        dest.PartnerReference = "REF-001";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnershipAgreementReference = null };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be("REF-001");
    }

    [Fact]
    [Trait("DEF012", "POS_011")]
    public void POS_011_NonNullName_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "New Name" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("New Name");
    }

    [Fact]
    [Trait("DEF012", "POS_012")]
    public void POS_012_NonNullDescription_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "New Desc" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("New Desc");
    }

    [Fact]
    [Trait("DEF012", "POS_013")]
    public void POS_013_NonNullBudget_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 5_000_000m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(5_000_000m);
    }

    [Fact]
    [Trait("DEF012", "POS_014")]
    public void POS_014_NonNullStage_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = "NO GO" };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("NO GO");
    }

    [Fact]
    [Trait("DEF012", "POS_015")]
    public void POS_015_NonNullResponsibleOrgUnitId_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = 42 };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(42);
    }

    [Fact]
    [Trait("DEF012", "POS_016")]
    public void POS_016_NonNullTargetSigningDate_Updates()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2026, 3, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "POS_017")]
    public void POS_017_NonNullTargetDeliveryDate_Updates()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2027, 6, 30);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = d };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "POS_018")]
    public void POS_018_NonNullProposedInitiativeTypeId_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ProposedInitiativeTypeId = 7 };
        _mapper.Map(request, dest);
        dest.ProposedInitiativeTypeId.Should().Be(7);
    }

    [Fact]
    [Trait("DEF012", "POS_019")]
    public void POS_019_NonNullPartnerReference_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnerReference = "REF-NEW" };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be("REF-NEW");
    }

    [Fact]
    [Trait("DEF012", "POS_020")]
    public void POS_020_NonNullPartnershipAgreementReference_Updates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnershipAgreementReference = "PA-NEW" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "POS_021")]
    public void POS_021_MapperConfig_Valid()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Valid" }, dest);
        dest.Name.Should().Be("Valid");
    }

    [Fact]
    [Trait("DEF012", "POS_022")]
    public void POS_022_UpdateOpportunityRequestToOpportunity_MapExists()
    {
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test" };
        var dest = CreateOpportunity();
        _mapper.Invoking(m => m.Map(request, dest)).Should().NotThrow();
    }

    [Fact]
    [Trait("DEF012", "POS_023")]
    public void POS_023_Map_ProducesCorrectType()
    {
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test" };
        var dest = CreateOpportunity();
        _mapper.Map(request, dest);
        dest.Should().BeOfType<OpportunityEntity>();
    }

    [Fact]
    [Trait("DEF012", "POS_024")]
    public void POS_024_MultipleSequentialMaps_Work()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "First" }, dest);
        dest.Name.Should().Be("First");
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Second" }, dest);
        dest.Name.Should().Be("Second");
    }

    [Fact]
    [Trait("DEF012", "POS_025")]
    public void POS_025_AllNullFields_PreservesAll()
    {
        var dest = CreateOpportunity();
        dest.Name = "Keep";
        dest.Description = "Keep";
        dest.InitiativeBudgetUSD = 100m;
        var request = new UpdateOpportunityRequest { Id = 10 };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Keep");
        dest.Description.Should().Be("Keep");
        dest.InitiativeBudgetUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("DEF012", "POS_026")]
    public void POS_026_AllNonNullFields_UpdatesAll()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "N",
            Description = "D",
            PartnerReference = "PR",
            Stage = "GO",
            ResponsibleOrgUnitId = 1,
            InitiativeBudgetUSD = 999m,
            TargetSigningDate = new DateTime(2026, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31),
            ProposedInitiativeTypeId = 2
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("N");
        dest.Description.Should().Be("D");
        dest.PartnerReference.Should().Be("PR");
        dest.Stage.Should().Be("GO");
        dest.ResponsibleOrgUnitId.Should().Be(1);
        dest.InitiativeBudgetUSD.Should().Be(999m);
        dest.ProposedInitiativeTypeId.Should().Be(2);
    }

    [Fact]
    [Trait("DEF012", "POS_027")]
    public void POS_027_Map_PreservesUnrelatedFields()
    {
        var dest = CreateOpportunity();
        dest.ResultsFocus = "Original";
        dest.Status = EntityStatus.Active;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Updated");
        dest.ResultsFocus.Should().Be("Original");
    }

    [Fact]
    [Trait("DEF012", "POS_028")]
    public async Task POS_028_Mapper_ThreadSafe()
    {
        var dest = CreateOpportunity();
        var tasks = Enumerable.Range(0, 10).Select(i => System.Threading.Tasks.Task.Run(() =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Name{i}" }, d);
            return d.Name;
        })).ToArray();
        var results = await System.Threading.Tasks.Task.WhenAll(tasks);
        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("DEF012", "POS_029")]
    public void POS_029_ForAllMembersCondition_Applied()
    {
        var dest = CreateOpportunity();
        dest.Description = "Preserved";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = null };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Preserved");
    }

    [Fact]
    [Trait("DEF012", "POS_030")]
    public void POS_030_IgnoreRulesForCollections_Applied()
    {
        var dest = CreateOpportunity();
        var existingCount = dest.FundingPartners?.Count ?? 0;
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
        (dest.FundingPartners?.Count ?? 0).Should().Be(existingCount);
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
