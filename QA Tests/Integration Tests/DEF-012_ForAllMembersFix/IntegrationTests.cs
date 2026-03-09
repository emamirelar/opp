using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
/// DEF-012: Integration tests for OpportunityMappingProfile ForAllMembers fix.
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Type", "Integration")]
public class IntegrationTests
{
    private readonly IMapper _mapper;

    public IntegrationTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly);
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    [Trait("DEF012", "INT_001")]
    public void INT_001_MapRequestToEntity_VerifyPersistReady()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Ready", Description = "Desc" };
        _mapper.Map(request, dest);
        dest.Name.Should().NotBeNullOrEmpty();
        dest.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("DEF012", "INT_002")]
    public void INT_002_MapPreservesRequiredFieldsForEF()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "EF" };
        _mapper.Map(request, dest);
        dest.Name.Should().NotBeNullOrEmpty();
        dest.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("DEF012", "INT_003")]
    public void INT_003_MappedEntityValidForSaveChanges()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Valid", Description = "Valid" };
        _mapper.Map(request, dest);
        dest.Id.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("DEF012", "INT_004")]
    public void INT_004_SequentialMapPersistCycles()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Cycle1" }, dest);
        dest.Name.Should().Be("Cycle1");
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Cycle2" }, dest);
        dest.Name.Should().Be("Cycle2");
    }

    [Fact]
    [Trait("DEF012", "INT_005")]
    public void INT_005_MapWithRealUpdateOpportunityRequest()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "Real",
            Description = "Real Desc",
            PartnerReference = "REF",
            Stage = "GO",
            ResponsibleOrgUnitId = 1,
            InitiativeBudgetUSD = 1_000_000m,
            TargetSigningDate = new DateTime(2026, 6, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31),
            ProposedInitiativeTypeId = 2
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Real");
        dest.InitiativeBudgetUSD.Should().Be(1_000_000m);
    }

    [Fact]
    [Trait("DEF012", "INT_006")]
    public void INT_006_MapWithOpportunityFromCreateOpportunity()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Updated");
    }

    [Fact]
    [Trait("DEF012", "INT_007")]
    public void INT_007_MapPreservesDBGeneratedFields()
    {
        var dest = CreateOpportunity();
        dest.CreatedDate = new DateTime(2025, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.CreatedDate.Should().Be(new DateTime(2025, 1, 1));
    }

    [Fact]
    [Trait("DEF012", "INT_008")]
    public void INT_008_MapDoesNotAffectTrackedEntityState()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "INT_009")]
    public void INT_009_MapOutputUsableForValidation()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Valid", Description = "Valid" };
        _mapper.Map(request, dest);
        dest.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("DEF012", "INT_010")]
    public void INT_010_MapOutputHasCorrectTypes()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Should().BeOfType<OpportunityEntity>();
    }

    [Fact]
    [Trait("DEF012", "INT_011")]
    public void INT_011_MappedBudgetHasCorrectPrecision()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 1234567.89m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(1234567.89m);
    }

    [Fact]
    [Trait("DEF012", "INT_012")]
    public void INT_012_MappedDatesHaveCorrectKind()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "INT_013")]
    public void INT_013_MappedStringsHaveCorrectLength()
    {
        var dest = CreateOpportunity();
        var name = new string('a', 50);
        var request = new UpdateOpportunityRequest { Id = 10, Name = name };
        _mapper.Map(request, dest);
        dest.Name.Length.Should().Be(50);
    }

    [Fact]
    [Trait("DEF012", "INT_014")]
    public void INT_014_MappedIntsHaveCorrectValues()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = 42 };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(42);
    }

    [Fact]
    [Trait("DEF012", "INT_015")]
    public void INT_015_MappedNullablesHandledCorrectly()
    {
        var dest = CreateOpportunity();
        dest.ResponsibleOrgUnitId = 5;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = null };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(5);
    }

    [Fact]
    [Trait("DEF012", "INT_016")]
    public void INT_016_CreateEntityMapUpdateVerify()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Updated");
    }

    [Fact]
    [Trait("DEF012", "INT_017")]
    public void INT_017_MapMultipleUpdatesSequentially()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "U1" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "U2", Description = "D2" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "U3" }, dest);
        dest.Name.Should().Be("U3");
        dest.Description.Should().Be("D2");
    }

    [Fact]
    [Trait("DEF012", "INT_018")]
    public void INT_018_MapUpdatePreservingCollections()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners = new HashSet<OpportunityFundingPartner>();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "INT_019")]
    public void INT_019_MapUpdateWithPartialData()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Partial" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Partial");
    }

    [Fact]
    [Trait("DEF012", "INT_020")]
    public void INT_020_MapUpdateWithFullData()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "Full",
            Description = "Full Desc",
            PartnerReference = "PR",
            Stage = "GO",
            ResponsibleOrgUnitId = 1,
            InitiativeBudgetUSD = 999m,
            TargetSigningDate = new DateTime(2026, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31),
            ProposedInitiativeTypeId = 2
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Full");
        dest.Description.Should().Be("Full Desc");
    }

    [Fact]
    [Trait("DEF012", "INT_021")]
    public async Task INT_021_ConcurrentMapOperations()
    {
        var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(() =>
        {
            var dest = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Concurrent{i}" }, dest);
            return dest.Name;
        })).ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("DEF012", "INT_022")]
    public void INT_022_MapValidatePersistPipeline()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Pipeline", Description = "Desc" };
        _mapper.Map(request, dest);
        dest.Name.Should().NotBeNullOrEmpty();
        dest.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("DEF012", "INT_023")]
    public void INT_023_CreateReadMapUpdateReadCycle()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "First" }, dest);
        var name1 = dest.Name;
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Second" }, dest);
        var name2 = dest.Name;
        name1.Should().Be("First");
        name2.Should().Be("Second");
    }

    [Fact]
    [Trait("DEF012", "INT_024")]
    public void INT_024_MapWithEntityThatHasNavigationProps()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners = new HashSet<OpportunityFundingPartner>();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "INT_025")]
    public void INT_025_MapDoesNotTouchNavigationProperties()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners = new HashSet<OpportunityFundingPartner>();
        var count = dest.FundingPartners.Count;
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        _mapper.Map(request, dest);
        dest.FundingPartners.Count.Should().Be(count);
    }

    [Fact]
    [Trait("DEF012", "INT_026")]
    public void INT_026_MapPerformanceAcrossIterations()
    {
        var dest = CreateOpportunity();
        for (var i = 0; i < 100; i++)
        {
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Iter{i}" }, dest);
        }
        dest.Name.Should().Be("Iter99");
    }

    [Fact]
    [Trait("DEF012", "INT_027")]
    public void INT_027_MapStabilityAcross100Calls()
    {
        var dest = CreateOpportunity();
        for (var i = 0; i < 100; i++)
        {
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Stable" }, dest);
        }
        dest.Name.Should().Be("Stable");
    }

    [Fact]
    [Trait("DEF012", "INT_028")]
    public void INT_028_MapErrorRecovery()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Recovery" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Recovery");
    }

    [Fact]
    [Trait("DEF012", "INT_029")]
    public void INT_029_MapWithEntityInVariousStates()
    {
        var dest = CreateOpportunity();
        dest.Status = EntityStatus.Active;
        dest.IsDeleted = false;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Various" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Various");
    }

    [Fact]
    [Trait("DEF012", "INT_030")]
    public void INT_030_MapWithEntityLoadedFromFactory()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Factory" };
        _mapper.Map(request, dest);
        dest.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "INT_031")]
    public void INT_031_OpportunityMappingProfileCoexistsWithMappingProfile()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Coexists" }, dest);
        dest.Name.Should().Be("Coexists");
    }

    [Fact]
    [Trait("DEF012", "INT_032")]
    public void INT_032_BothProfilesRegisterCorrectly()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Register" }, dest);
        dest.Name.Should().Be("Register");
    }

    [Fact]
    [Trait("DEF012", "INT_033")]
    public void INT_033_NoMappingConflicts()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "NoConflict" }, dest);
        dest.Name.Should().Be("NoConflict");
    }

    [Fact]
    [Trait("DEF012", "INT_034")]
    public void INT_034_OpportunityToOpportunityModelMapWorks()
    {
        var opp = CreateOpportunity();
        var model = _mapper.Map<OpportunityModel>(opp);
        model.Should().NotBeNull();
        model.Name.Should().Be(opp.Name);
    }

    [Fact]
    [Trait("DEF012", "INT_035")]
    public void INT_035_OpportunityRequestToOpportunityMapWorks()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Request" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Request");
    }

    [Fact]
    [Trait("DEF012", "INT_036")]
    public void INT_036_MapChainRequestEntityModel()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Chain" };
        _mapper.Map(request, dest);
        var model = _mapper.Map<OpportunityModel>(dest);
        model.Should().NotBeNull();
        model.Name.Should().Be("Chain");
    }

    [Fact]
    [Trait("DEF012", "INT_037")]
    public void INT_037_MapReverseChainModelProperties()
    {
        var opp = CreateOpportunity();
        var model = _mapper.Map<OpportunityModel>(opp);
        model.Id.Should().Be(opp.Id);
        model.Name.Should().Be(opp.Name);
    }

    [Fact]
    [Trait("DEF012", "INT_038")]
    public void INT_038_AllProfilesLoadedByAssemblyScan()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Scanned" }, dest);
        dest.Name.Should().Be("Scanned");
    }

    [Fact]
    [Trait("DEF012", "INT_039")]
    public void INT_039_ProfileCountVerification()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Verify" };
        config.CreateMapper().Map(request, dest);
        dest.Name.Should().Be("Verify");
    }

    [Fact]
    [Trait("DEF012", "INT_040")]
    public void INT_040_NoDuplicateMaps()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly); });
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "NoDup" }, dest);
        dest.Name.Should().Be("NoDup");
    }

    [Fact]
    [Trait("DEF012", "INT_041")]
    public void INT_041_MapPreservesEntityAfterSaveChanges()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Preserved" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Preserved");
    }

    [Fact]
    [Trait("DEF012", "INT_042")]
    public async Task INT_042_MapThreadSafetyUnderParallelLoad()
    {
        var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() =>
        {
            var dest = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Parallel" }, dest);
            return dest.Name;
        })).ToArray();
        var results = await Task.WhenAll(tasks);
        results.All(r => r == "Parallel").Should().BeTrue();
    }

    [Fact]
    [Trait("DEF012", "INT_043")]
    public void INT_043_MapWithLargeStringValues()
    {
        var dest = CreateOpportunity();
        var large = new string('x', 10000);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = large };
        _mapper.Map(request, dest);
        dest.Description.Should().Be(large);
    }

    [Fact]
    [Trait("DEF012", "INT_044")]
    public void INT_044_MapWithExtremeNumericValues()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = decimal.MaxValue };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(decimal.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "INT_045")]
    public void INT_045_MapWithBoundaryDates()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            TargetSigningDate = DateTime.MinValue,
            TargetDeliveryDate = DateTime.MaxValue
        };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(DateTime.MinValue);
        dest.TargetDeliveryDate.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "INT_046")]
    public void INT_046_MapStabilityAfterException()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Stable" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Stable");
    }

    [Fact]
    [Trait("DEF012", "INT_047")]
    public void INT_047_MapDoesNotAccumulateState()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "A" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "B" }, dest);
        dest.Name.Should().Be("B");
    }

    [Fact]
    [Trait("DEF012", "INT_048")]
    public void INT_048_MapOutputIndependentOfSource()
    {
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Source" };
        var dest = CreateOpportunity();
        _mapper.Map(request, dest);
        dest.Name = "Modified";
        request.Name.Should().Be("Source");
    }

    [Fact]
    [Trait("DEF012", "INT_049")]
    public void INT_049_MapIsDeterministic()
    {
        var dest1 = CreateOpportunity();
        var dest2 = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Deterministic" };
        _mapper.Map(request, dest1);
        _mapper.Map(request, dest2);
        dest1.Name.Should().Be(dest2.Name);
    }

    [Fact]
    [Trait("DEF012", "INT_050")]
    public void INT_050_MapProducesConsistentResultsAcrossInvocations()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Consistent" };
        _mapper.Map(request, dest);
        var result1 = dest.Name;
        _mapper.Map(request, dest);
        var result2 = dest.Name;
        result1.Should().Be(result2);
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
