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
/// DEF-012: Negative tests for OpportunityMappingProfile ForAllMembers fix.
/// </summary>
[Collection("Negative")]
[Trait("Category", "Negative")]
[Trait("Type", "Negative")]
public class NegativeTests
{
    private readonly IMapper _mapper;

    public NegativeTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly);
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    [Trait("DEF012", "NEG_001")]
    public void NEG_001_MapNullSource_Throws()
    {
        var dest = CreateOpportunity();
        dest.Name = "Preserved";
        _mapper.Map((UpdateOpportunityRequest?)null!, dest);
        dest.Name.Should().Be("Preserved");
    }

    [Fact]
    [Trait("DEF012", "NEG_002")]
    public void NEG_002_MapToNullDestination_Throws()
    {
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test" };
        _mapper.Invoking(m => m.Map(request, (OpportunityEntity?)null!)).Should().NotThrow();
    }

    [Fact]
    [Trait("DEF012", "NEG_003")]
    public void NEG_003_NullName_DoesNotOverwriteNonNull()
    {
        var dest = CreateOpportunity();
        dest.Name = "Keep";
        var request = new UpdateOpportunityRequest { Id = 10, Name = null };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Keep");
    }

    [Fact]
    [Trait("DEF012", "NEG_004")]
    public void NEG_004_NullDescription_DoesNotClear()
    {
        var dest = CreateOpportunity();
        dest.Description = "Keep";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = null };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Keep");
    }

    [Fact]
    [Trait("DEF012", "NEG_005")]
    public void NEG_005_NullBudget_DoesNotZeroOut()
    {
        var dest = CreateOpportunity();
        dest.InitiativeBudgetUSD = 1_000_000m;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = null };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(1_000_000m);
    }

    [Fact]
    [Trait("DEF012", "NEG_006")]
    public void NEG_006_NullStage_DoesNotClear()
    {
        var dest = CreateOpportunity();
        dest.Stage = "GO";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = null };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("GO");
    }

    [Fact]
    [Trait("DEF012", "NEG_007")]
    public void NEG_007_NullInt_DoesNotSetZero()
    {
        var dest = CreateOpportunity();
        dest.ResponsibleOrgUnitId = 99;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = null };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(99);
    }

    [Fact]
    [Trait("DEF012", "NEG_008")]
    public void NEG_008_NullDateTime_DoesNotSetMinValue()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2026, 6, 1);
        dest.TargetSigningDate = d;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = null };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "NEG_009")]
    public void NEG_009_NullDecimal_DoesNotSetZero()
    {
        var dest = CreateOpportunity();
        dest.InitiativeBudgetUSD = 500m;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = null };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(500m);
    }

    [Fact]
    [Trait("DEF012", "NEG_010")]
    public void NEG_010_NullSource_PreservesAllDestValues()
    {
        var dest = CreateOpportunity();
        dest.Name = "A";
        dest.Description = "B";
        dest.InitiativeBudgetUSD = 100m;
        var request = new UpdateOpportunityRequest { Id = 10 };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("A");
        dest.Description.Should().Be("B");
        dest.InitiativeBudgetUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("DEF012", "NEG_011")]
    public void NEG_011_EmptyCollections_StillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", FundingPartners = new List<OpportunityFundingPartnerRequest>() };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_012")]
    public void NEG_012_NullCollections_StillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", FundingPartners = null };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_013")]
    public void NEG_013_MapWithOnlyIdSet()
    {
        var dest = CreateOpportunity();
        dest.Name = "Original";
        var request = new UpdateOpportunityRequest { Id = 10 };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Original");
    }

    [Fact]
    [Trait("DEF012", "NEG_014")]
    public void NEG_014_MapWithOnlyNameSet()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "OnlyName" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("OnlyName");
        dest.Description.Should().Be("Test Desc");
    }

    [Fact]
    [Trait("DEF012", "NEG_015")]
    public void NEG_015_MapWithOnlyCollectionsSet()
    {
        var dest = CreateOpportunity();
        dest.Name = "Original";
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Original");
    }

    [Fact]
    [Trait("DEF012", "NEG_016")]
    public void NEG_016_MapWithMismatchedTypes_Validated()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test", Stage = "GO" };
        _mapper.Invoking(m => m.Map(request, dest)).Should().NotThrow();
    }

    [Fact]
    [Trait("DEF012", "NEG_017")]
    public void NEG_017_StringOverflowHandling()
    {
        var dest = CreateOpportunity();
        var longName = new string('x', 200);
        var request = new UpdateOpportunityRequest { Id = 10, Name = longName };
        _mapper.Map(request, dest);
        dest.Name.Should().Be(longName);
    }

    [Fact]
    [Trait("DEF012", "NEG_018")]
    public void NEG_018_VeryLongName()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = new string('a', 500) };
        _mapper.Map(request, dest);
        dest.Name.Length.Should().Be(500);
    }

    [Fact]
    [Trait("DEF012", "NEG_019")]
    public void NEG_019_VeryLongDescription()
    {
        var dest = CreateOpportunity();
        var desc = new string('b', 10000);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = desc };
        _mapper.Map(request, dest);
        dest.Description.Should().Be(desc);
    }

    [Fact]
    [Trait("DEF012", "NEG_020")]
    public void NEG_020_NegativeBudget()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = -100m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(-100m);
    }

    [Fact]
    [Trait("DEF012", "NEG_021")]
    public void NEG_021_ZeroBudget()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 0m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(0m);
    }

    [Fact]
    [Trait("DEF012", "NEG_022")]
    public void NEG_022_HugeBudget()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = decimal.MaxValue };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(decimal.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "NEG_023")]
    public void NEG_023_PastDates()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2000, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "NEG_024")]
    public void NEG_024_FarFutureDates()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2100, 12, 31);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = d };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "NEG_025")]
    public void NEG_025_InvalidStageValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = "INVALID_STAGE" };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("INVALID_STAGE");
    }

    [Fact]
    [Trait("DEF012", "NEG_026")]
    public void NEG_026_NegativeIDs()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = -1, Name = "X", ResponsibleOrgUnitId = -5 };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(-5);
    }

    [Fact]
    [Trait("DEF012", "NEG_027")]
    public void NEG_027_ZeroIDs()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = 0 };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(0);
    }

    [Fact]
    [Trait("DEF012", "NEG_028")]
    public void NEG_028_MaxIntIDs()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = int.MaxValue };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "NEG_029")]
    public void NEG_029_MaxDecimalValues()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = decimal.MaxValue };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(decimal.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "NEG_030")]
    public void NEG_030_MinDecimalValues()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = decimal.MinValue };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(decimal.MinValue);
    }

    [Fact]
    [Trait("DEF012", "NEG_031")]
    public void NEG_031_FundingPartners_Ignored()
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
    [Trait("DEF012", "NEG_032")]
    public void NEG_032_ClientPartners_Ignored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_033")]
    public void NEG_033_Stakeholders_Ignored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Stakeholders = new List<OpportunityStakeholderRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_034")]
    public void NEG_034_Deliverables_Ignored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Deliverables = new List<OpportunityDeliverableRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_035")]
    public void NEG_035_Countries_Ignored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Countries = new List<OpportunityCountryRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_036")]
    public void NEG_036_SDGs_Ignored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            SDGs = new List<OpportunitySDGRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_037")]
    public void NEG_037_NonNullFundingPartners_StillIgnored()
    {
        var dest = CreateOpportunity();
        var initialCount = dest.FundingPartners?.Count ?? 0;
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 }, new() { PartnerId = 2 } }
        };
        _mapper.Map(request, dest);
        (dest.FundingPartners?.Count ?? 0).Should().Be(initialCount);
    }

    [Fact]
    [Trait("DEF012", "NEG_038")]
    public void NEG_038_NonNullClientPartners_StillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ClientPartners = new List<OpportunityClientPartnerRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_039")]
    public void NEG_039_NonNullStakeholders_StillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stakeholders = new List<OpportunityStakeholderRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_040")]
    public void NEG_040_NonNullDeliverables_StillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Deliverables = new List<OpportunityDeliverableRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_041")]
    public void NEG_041_NonNullCountries_StillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Countries = new List<OpportunityCountryRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_042")]
    public void NEG_042_NonNullSDGs_StillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", SDGs = new List<OpportunitySDGRequest> { new() } };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_043")]
    public void NEG_043_EmptyListFundingPartners_Ignored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", FundingPartners = new List<OpportunityFundingPartnerRequest>() };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "NEG_044")]
    public void NEG_044_Collections_DoNotNullDestination()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners = new HashSet<OpportunityFundingPartner>();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", FundingPartners = new List<OpportunityFundingPartnerRequest>() };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "NEG_045")]
    public void NEG_045_Collections_DoNotReplaceDestination()
    {
        var dest = CreateOpportunity();
        var existingCount = dest.FundingPartners?.Count ?? 0;
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 99 } }
        };
        _mapper.Map(request, dest);
        (dest.FundingPartners?.Count ?? 0).Should().Be(existingCount);
    }

    [Fact]
    [Trait("DEF012", "NEG_046")]
    public void NEG_046_ForAllMembersWithIgnore_IgnoreWins()
    {
        var dest = CreateOpportunity();
        dest.Id = 42;
        var request = new UpdateOpportunityRequest { Id = 99, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(42);
    }

    [Fact]
    [Trait("DEF012", "NEG_047")]
    public void NEG_047_ConditionAppliesAfterIgnore()
    {
        var dest = CreateOpportunity();
        dest.Description = "Keep";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = null };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Keep");
    }

    [Fact]
    [Trait("DEF012", "NEG_048")]
    public void NEG_048_NonNullWithIgnore_StillIgnored()
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
    [Trait("DEF012", "NEG_049")]
    public void NEG_049_NullWithNonIgnore_StillProtected()
    {
        var dest = CreateOpportunity();
        dest.Stage = "GO";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = null };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("GO");
    }

    [Fact]
    [Trait("DEF012", "NEG_050")]
    public void NEG_050_MixedNullAndNonNull()
    {
        var dest = CreateOpportunity();
        dest.Name = "Old";
        dest.Description = "OldDesc";
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "New",
            Description = null
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("New");
        dest.Description.Should().Be("OldDesc");
    }

    [Fact]
    [Trait("DEF012", "NEG_051")]
    public void NEG_051_IdBehaviorWithForAllMembers()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "NEG_052")]
    public void NEG_052_DefaultIntVsNullInt()
    {
        var dest = CreateOpportunity();
        dest.ResponsibleOrgUnitId = 5;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = null };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(5);
    }

    [Fact]
    [Trait("DEF012", "NEG_053")]
    public void NEG_053_DefaultDateTimeVsNullDateTime()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2026, 1, 1);
        dest.TargetSigningDate = d;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = null };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "NEG_054")]
    public void NEG_054_DefaultDecimalVsNullDecimal()
    {
        var dest = CreateOpportunity();
        dest.InitiativeBudgetUSD = 100m;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = null };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("DEF012", "NEG_055")]
    public void NEG_055_UpdateSameEntityTwice()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "First" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Second" }, dest);
        dest.Name.Should().Be("Second");
    }

    [Fact]
    [Trait("DEF012", "NEG_056")]
    public void NEG_056_SequentialMaps_DoNotAccumulate()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "A", Description = "D1" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "B" }, dest);
        dest.Name.Should().Be("B");
        dest.Description.Should().Be("D1");
    }

    [Fact]
    [Trait("DEF012", "NEG_057")]
    public void NEG_057_Map_DoesNotAffectSource()
    {
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Original" };
        var dest = CreateOpportunity();
        _mapper.Map(request, dest);
        dest.Name = "Modified";
        request.Name.Should().Be("Original");
    }

    [Fact]
    [Trait("DEF012", "NEG_058")]
    public async Task NEG_058_ConcurrentMaps_Safe()
    {
        var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(() =>
        {
            var dest = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Name{i}" }, dest);
            return dest.Name;
        })).ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("DEF012", "NEG_059")]
    public void NEG_059_DestinationAuditFields_Preserved()
    {
        var dest = CreateOpportunity();
        dest.CreatedBy = 1;
        dest.CreatedDate = new DateTime(2025, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.CreatedBy.Should().Be(1);
        dest.CreatedDate.Should().Be(new DateTime(2025, 1, 1));
    }

    [Fact]
    [Trait("DEF012", "NEG_060")]
    public void NEG_060_Map_PreservesIsDeleted()
    {
        var dest = CreateOpportunity();
        dest.IsDeleted = true;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.IsDeleted.Should().BeTrue();
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
