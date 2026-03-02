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
/// DEF-012: Boundary tests for OpportunityMappingProfile ForAllMembers fix.
/// </summary>
[Collection("Boundary")]
[Trait("Category", "Boundary")]
[Trait("Type", "Boundary")]
public class BoundaryTests
{
    private readonly IMapper _mapper;

    public BoundaryTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(OpportunityMappingProfile).Assembly);
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    [Trait("DEF012", "BND_001")]
    public void BND_001_EmptyStringName()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("");
    }

    [Fact]
    [Trait("DEF012", "BND_002")]
    public void BND_002_SingleCharName()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "BND_003")]
    public void BND_003_Max120CharName()
    {
        var dest = CreateOpportunity();
        var name = new string('a', 120);
        var request = new UpdateOpportunityRequest { Id = 10, Name = name };
        _mapper.Map(request, dest);
        dest.Name.Should().Be(name);
    }

    [Fact]
    [Trait("DEF012", "BND_004")]
    public void BND_004_121CharName()
    {
        var dest = CreateOpportunity();
        var name = new string('a', 121);
        var request = new UpdateOpportunityRequest { Id = 10, Name = name };
        _mapper.Map(request, dest);
        dest.Name.Should().Be(name);
    }

    [Fact]
    [Trait("DEF012", "BND_005")]
    public void BND_005_UnicodeName()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "日本語テスト" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("日本語テスト");
    }

    [Fact]
    [Trait("DEF012", "BND_006")]
    public void BND_006_NameWithSpecialChars()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test & Co. <script>" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Test & Co. <script>");
    }

    [Fact]
    [Trait("DEF012", "BND_007")]
    public void BND_007_NameWithHTML()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "<div>Test</div>" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("<div>Test</div>");
    }

    [Fact]
    [Trait("DEF012", "BND_008")]
    public void BND_008_NameWithSQLInjection()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "'; DROP TABLE Opportunity;--" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("'; DROP TABLE Opportunity;--");
    }

    [Fact]
    [Trait("DEF012", "BND_009")]
    public void BND_009_EmptyDescription()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("");
    }

    [Fact]
    [Trait("DEF012", "BND_010")]
    public void BND_010_MaxLengthDescription()
    {
        var dest = CreateOpportunity();
        var desc = new string('x', 50000);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = desc };
        _mapper.Map(request, dest);
        dest.Description.Should().Be(desc);
    }

    [Fact]
    [Trait("DEF012", "BND_011")]
    public void BND_011_DescriptionWithNewlines()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "Line1\nLine2\nLine3" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Line1\nLine2\nLine3");
    }

    [Fact]
    [Trait("DEF012", "BND_012")]
    public void BND_012_DescriptionWithTabs()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "Col1\tCol2\tCol3" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Col1\tCol2\tCol3");
    }

    [Fact]
    [Trait("DEF012", "BND_013")]
    public void BND_013_PartnerReferenceEmpty()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnerReference = "" };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be("");
    }

    [Fact]
    [Trait("DEF012", "BND_014")]
    public void BND_014_PartnerReferenceMax255()
    {
        var dest = CreateOpportunity();
        var pr = new string('r', 255);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnerReference = pr };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be(pr);
    }

    [Fact]
    [Trait("DEF012", "BND_015")]
    public void BND_015_StageAtExactMax100Chars()
    {
        var dest = CreateOpportunity();
        var stage = new string('s', 100);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = stage };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be(stage);
    }

    [Fact]
    [Trait("DEF012", "BND_016")]
    public void BND_016_BudgetZero()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 0 };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(0);
    }

    [Fact]
    [Trait("DEF012", "BND_017")]
    public void BND_017_Budget001()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 0.01m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(0.01m);
    }

    [Fact]
    [Trait("DEF012", "BND_018")]
    public void BND_018_BudgetDecimalMaxValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = decimal.MaxValue };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(decimal.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "BND_019")]
    public void BND_019_BudgetNegative001()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = -0.01m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(-0.01m);
    }

    [Fact]
    [Trait("DEF012", "BND_020")]
    public void BND_020_BudgetVeryPrecise()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 1234567.89m };
        _mapper.Map(request, dest);
        dest.InitiativeBudgetUSD.Should().Be(1234567.89m);
    }

    [Fact]
    [Trait("DEF012", "BND_021")]
    public void BND_021_ResponsibleOrgUnitIdZero()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = 0 };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(0);
    }

    [Fact]
    [Trait("DEF012", "BND_022")]
    public void BND_022_ResponsibleOrgUnitIdIntMaxValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = int.MaxValue };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "BND_023")]
    public void BND_023_ResponsibleOrgUnitIdIntMinValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = int.MinValue };
        _mapper.Map(request, dest);
        dest.ResponsibleOrgUnitId.Should().Be(int.MinValue);
    }

    [Fact]
    [Trait("DEF012", "BND_024")]
    public void BND_024_ProposedInitiativeTypeIdZero()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ProposedInitiativeTypeId = 0 };
        _mapper.Map(request, dest);
        dest.ProposedInitiativeTypeId.Should().Be(0);
    }

    [Fact]
    [Trait("DEF012", "BND_025")]
    public void BND_025_ProposedInitiativeTypeIdIntMaxValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ProposedInitiativeTypeId = int.MaxValue };
        _mapper.Map(request, dest);
        dest.ProposedInitiativeTypeId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "BND_026")]
    public void BND_026_IdZero()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = 0, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "BND_027")]
    public void BND_027_IdIntMaxValue()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = int.MaxValue, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "BND_028")]
    public void BND_028_IdIntMinValue()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = int.MinValue, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "BND_029")]
    public void BND_029_IdNegativeOne()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = -1, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "BND_030")]
    public void BND_030_IdOne()
    {
        var dest = CreateOpportunity();
        dest.Id = 1;
        var request = new UpdateOpportunityRequest { Id = 1, Name = "X" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(1);
    }

    [Fact]
    [Trait("DEF012", "BND_031")]
    public void BND_031_TargetSigningDateMinValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = DateTime.MinValue };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(DateTime.MinValue);
    }

    [Fact]
    [Trait("DEF012", "BND_032")]
    public void BND_032_TargetSigningDateMaxValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = DateTime.MaxValue };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "BND_033")]
    public void BND_033_TargetSigningDateUtcNow()
    {
        var dest = CreateOpportunity();
        var now = DateTime.UtcNow;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = now };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(now);
    }

    [Fact]
    [Trait("DEF012", "BND_034")]
    public void BND_034_TargetDeliveryDateMinValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = DateTime.MinValue };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(DateTime.MinValue);
    }

    [Fact]
    [Trait("DEF012", "BND_035")]
    public void BND_035_TargetDeliveryDateMaxValue()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = DateTime.MaxValue };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "BND_036")]
    public void BND_036_TargetDeliveryDatePast()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2000, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = d };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "BND_037")]
    public void BND_037_TargetDeliveryDateFarFuture()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2099, 12, 31);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = d };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "BND_038")]
    public void BND_038_SigningBeforeDelivery()
    {
        var dest = CreateOpportunity();
        var signing = new DateTime(2026, 1, 1);
        var delivery = new DateTime(2026, 12, 31);
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
    [Trait("DEF012", "BND_039")]
    public void BND_039_DeliveryBeforeSigning()
    {
        var dest = CreateOpportunity();
        var signing = new DateTime(2026, 6, 1);
        var delivery = new DateTime(2026, 1, 1);
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
    [Trait("DEF012", "BND_040")]
    public void BND_040_SameDates()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2026, 6, 15);
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            TargetSigningDate = d,
            TargetDeliveryDate = d
        };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
        dest.TargetDeliveryDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "BND_041")]
    public void BND_041_MidnightBoundary()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2026, 6, 15, 0, 0, 0);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "BND_042")]
    public void BND_042_Year2099()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2099, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetDeliveryDate = d };
        _mapper.Map(request, dest);
        dest.TargetDeliveryDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "BND_043")]
    public void BND_043_Year1900()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(1900, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "BND_044")]
    public void BND_044_UtcVsLocal()
    {
        var dest = CreateOpportunity();
        var utc = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = utc };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(utc);
    }

    [Fact]
    [Trait("DEF012", "BND_045")]
    public void BND_045_AllFieldsNull()
    {
        var dest = CreateOpportunity();
        dest.Name = "Keep";
        dest.Description = "Keep";
        var request = new UpdateOpportunityRequest { Id = 10 };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Keep");
        dest.Description.Should().Be("Keep");
    }

    [Fact]
    [Trait("DEF012", "BND_046")]
    public void BND_046_AllFieldsNonNull()
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
            InitiativeBudgetUSD = 100m,
            TargetSigningDate = new DateTime(2026, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31),
            ProposedInitiativeTypeId = 2
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("N");
        dest.Description.Should().Be("D");
    }

    [Fact]
    [Trait("DEF012", "BND_047")]
    public void BND_047_AlternatingNullNonNull()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "New",
            Description = null,
            PartnerReference = "NewRef",
            Stage = null
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("New");
        dest.PartnerReference.Should().Be("NewRef");
        dest.Description.Should().Be("Test Desc");
        dest.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("DEF012", "BND_048")]
    public void BND_048_AllScalarNullAllCollectionsNonNull()
    {
        var dest = CreateOpportunity();
        dest.Name = "Keep";
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Keep");
    }

    [Fact]
    [Trait("DEF012", "BND_049")]
    public void BND_049_AllScalarNonNullAllCollectionsNull()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "N",
            Description = "D",
            PartnerReference = "PR",
            Stage = "GO"
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("N");
    }

    [Fact]
    [Trait("DEF012", "BND_050")]
    public void BND_050_DestinationWithExistingData()
    {
        var dest = CreateOpportunity();
        dest.ResultsFocus = "Existing";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "New" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("New");
        dest.ResultsFocus.Should().Be("Existing");
    }

    [Fact]
    [Trait("DEF012", "BND_051")]
    public void BND_051_DestinationWithDefaultValues()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("DEF012", "BND_052")]
    public void BND_052_DestinationWithEmptyCollections()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners = new HashSet<OpportunityFundingPartner>();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "BND_053")]
    public void BND_053_MapOntoEntityWithAllMaxValues()
    {
        var dest = CreateOpportunity();
        dest.Name = new string('x', 120);
        dest.InitiativeBudgetUSD = decimal.MaxValue;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "New" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("New");
        dest.InitiativeBudgetUSD.Should().Be(decimal.MaxValue);
    }

    [Fact]
    [Trait("DEF012", "BND_054")]
    public void BND_054_MapOntoEntityWithAllMinValues()
    {
        var dest = CreateOpportunity();
        dest.InitiativeBudgetUSD = decimal.MinValue;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "New" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("New");
        dest.InitiativeBudgetUSD.Should().Be(decimal.MinValue);
    }

    [Fact]
    [Trait("DEF012", "BND_055")]
    public void BND_055_MapPreservesBaseEntityFields()
    {
        var dest = CreateOpportunity();
        dest.Status = EntityStatus.Active;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("DEF012", "BND_056")]
    public void BND_056_MapPreservesAuditFields()
    {
        var dest = CreateOpportunity();
        dest.CreatedBy = 5;
        dest.CreatedDate = new DateTime(2024, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X" };
        _mapper.Map(request, dest);
        dest.CreatedBy.Should().Be(5);
    }

    [Fact]
    [Trait("DEF012", "BND_057")]
    public void BND_057_DoubleMapSameSource()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Same" };
        _mapper.Map(request, dest);
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Same");
    }

    [Fact]
    [Trait("DEF012", "BND_058")]
    public void BND_058_MapThenRead()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Mapped" };
        _mapper.Map(request, dest);
        var name = dest.Name;
        name.Should().Be("Mapped");
    }

    [Fact]
    [Trait("DEF012", "BND_059")]
    public void BND_059_EntityAfterMultipleMaps()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "A" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "B", Description = "Desc" }, dest);
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "C" }, dest);
        dest.Name.Should().Be("C");
        dest.Description.Should().Be("Desc");
    }

    [Fact]
    [Trait("DEF012", "BND_060")]
    public void BND_060_LeapYearDate()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2024, 2, 29);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        dest.TargetSigningDate.Should().Be(d);
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
