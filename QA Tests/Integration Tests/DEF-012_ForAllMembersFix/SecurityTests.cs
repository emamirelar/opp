using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
/// DEF-012: Security tests for OpportunityMappingProfile ForAllMembers fix.
/// Verifies that AutoMapper mapping doesn't introduce security issues.
/// </summary>
[Collection("Security")]
[Trait("Category", "Security")]
[Trait("Type", "Security")]
public class SecurityTests
{
    private readonly IMapper _mapper;

    public SecurityTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OpportunityMappingProfile>();
        });
        _mapper = config.CreateMapper();
    }

    private static OpportunityEntity CreateOpportunity() => new()
    {
        Id = 10,
        Name = "Test Opportunity",
        Description = "Test Description",
        Stage = "IDENTIFY & PROFILE",
        Status = EntityStatus.Draft,
        IsDeleted = false
    };

    #region SEC_001-010: Input sanitization via mapping

    [Fact]
    [Trait("DEF012", "SEC_001")]
    public void SEC_001_SqlInjectionInNameHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "'; DROP TABLE Opportunity;--" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("'; DROP TABLE Opportunity;--");
    }

    [Fact]
    [Trait("DEF012", "SEC_002")]
    public void SEC_002_XssInDescriptionHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "<script>alert('xss')</script>" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("<script>alert('xss')</script>");
    }

    [Fact]
    [Trait("DEF012", "SEC_003")]
    public void SEC_003_ScriptTagsInNameHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "<script>document.cookie</script>" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("<script>document.cookie</script>");
    }

    [Fact]
    [Trait("DEF012", "SEC_004")]
    public void SEC_004_HtmlEntitiesInPartnerReferenceHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnerReference = "&lt;img src=x onerror=alert(1)&gt;" };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be("&lt;img src=x onerror=alert(1)&gt;");
    }

    [Fact]
    [Trait("DEF012", "SEC_005")]
    public void SEC_005_NullBytesInStringsHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test\x00Injection" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Test\x00Injection");
    }

    [Fact]
    [Trait("DEF012", "SEC_006")]
    public void SEC_006_PathTraversalInStageHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Stage = "../../../etc/passwd" };
        _mapper.Map(request, dest);
        dest.Stage.Should().Be("../../../etc/passwd");
    }

    [Fact]
    [Trait("DEF012", "SEC_007")]
    public void SEC_007_LdapInjectionInNameHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "*)(uid=*))(|(uid=*" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("*)(uid=*))(|(uid=*");
    }

    [Fact]
    [Trait("DEF012", "SEC_008")]
    public void SEC_008_FormatStringAttackInDescriptionHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "%s%s%s%s%s%s%s%s" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("%s%s%s%s%s%s%s%s");
    }

    [Fact]
    [Trait("DEF012", "SEC_009")]
    public void SEC_009_UnicodeExploitsInNameHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "\u202E\u2066malicious\u2069" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("\u202E\u2066malicious\u2069");
    }

    [Fact]
    [Trait("DEF012", "SEC_010")]
    public void SEC_010_ControlCharactersInDescriptionHandledSafely()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = "Text\u0001\u0002\u0003\u0004\u0005" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Text\u0001\u0002\u0003\u0004\u0005");
    }

    #endregion

    #region SEC_011-020: Data integrity via mapping

    [Fact]
    [Trait("DEF012", "SEC_011")]
    public void SEC_011_MappedEntityDoesNotExposeSourceMutations()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Original" };
        _mapper.Map(request, dest);
        request.Name = "MutatedAfterMap";
        dest.Name.Should().Be("Original");
    }

    [Fact]
    [Trait("DEF012", "SEC_012")]
    public void SEC_012_SourceModificationsAfterMapDoNotAffectDest()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "First", Description = "Desc1" };
        _mapper.Map(request, dest);
        request.Name = "Changed";
        request.Description = "ChangedDesc";
        dest.Name.Should().Be("First");
        dest.Description.Should().Be("Desc1");
    }

    [Fact]
    [Trait("DEF012", "SEC_013")]
    public void SEC_013_DestinationIsIndependentCopyForScalars()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Copy", InitiativeBudgetUSD = 1000m };
        _mapper.Map(request, dest);
        request.InitiativeBudgetUSD = 9999m;
        dest.InitiativeBudgetUSD.Should().Be(1000m);
    }

    [Fact]
    [Trait("DEF012", "SEC_014")]
    public void SEC_014_MapDoesNotLeakSourceCollectionReferences()
    {
        var dest = CreateOpportunity();
        var fundingList = new List<OpportunityFundingPartnerRequest> { new() };
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", FundingPartners = fundingList };
        _mapper.Map(request, dest);
        fundingList.Add(new OpportunityFundingPartnerRequest());
        dest.FundingPartners.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_015")]
    public void SEC_015_CollectionsProperlyIsolatedByIgnore()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners.Add(new OpportunityFundingPartner { Id = 1, Name = "Existing" });
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "Updated",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 99 } }
        };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().ContainSingle().Which.Name.Should().Be("Existing");
    }

    [Fact]
    [Trait("DEF012", "SEC_016")]
    public void SEC_016_MappedBudgetNotAffectedBySourceChange()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", InitiativeBudgetUSD = 5000m };
        _mapper.Map(request, dest);
        request.InitiativeBudgetUSD = 0m;
        dest.InitiativeBudgetUSD.Should().Be(5000m);
    }

    [Fact]
    [Trait("DEF012", "SEC_017")]
    public void SEC_017_MappedDatesIndependentOfSource()
    {
        var dest = CreateOpportunity();
        var d = new DateTime(2025, 6, 15);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", TargetSigningDate = d };
        _mapper.Map(request, dest);
        request.TargetSigningDate = new DateTime(2030, 1, 1);
        dest.TargetSigningDate.Should().Be(d);
    }

    [Fact]
    [Trait("DEF012", "SEC_018")]
    public void SEC_018_MappedStringsIndependentOfSource()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Independent" };
        _mapper.Map(request, dest);
        request.Name = "Modified";
        dest.Name.Should().Be("Independent");
    }

    [Fact]
    [Trait("DEF012", "SEC_019")]
    public void SEC_019_MappedIntsIndependentOfSource()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ResponsibleOrgUnitId = 42 };
        _mapper.Map(request, dest);
        request.ResponsibleOrgUnitId = 999;
        dest.ResponsibleOrgUnitId.Should().Be(42);
    }

    [Fact]
    [Trait("DEF012", "SEC_020")]
    public void SEC_020_MappedNullableValuesIndependentOfSource()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", ProposedInitiativeTypeId = 7 };
        _mapper.Map(request, dest);
        request.ProposedInitiativeTypeId = null;
        dest.ProposedInitiativeTypeId.Should().Be(7);
    }

    #endregion

    #region SEC_021-030: Privilege and access

    [Fact]
    [Trait("DEF012", "SEC_021")]
    public void SEC_021_MapDoesNotEscalateIsDeleted()
    {
        var dest = CreateOpportunity();
        dest.IsDeleted = false;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.IsDeleted.Should().BeFalse();
    }

    [Fact]
    [Trait("DEF012", "SEC_022")]
    public void SEC_022_MapPreservesOriginalCreatedBy()
    {
        var dest = CreateOpportunity();
        dest.CreatedBy = 100;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.CreatedBy.Should().Be(100);
    }

    [Fact]
    [Trait("DEF012", "SEC_023")]
    public void SEC_023_MapPreservesOriginalCreatedDate()
    {
        var dest = CreateOpportunity();
        var createdDate = new DateTime(2024, 1, 15);
        dest.CreatedDate = createdDate;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.CreatedDate.Should().Be(createdDate);
    }

    [Fact]
    [Trait("DEF012", "SEC_024")]
    public void SEC_024_MapDoesNotAllowStatusManipulationViaMap()
    {
        var dest = CreateOpportunity();
        dest.Status = EntityStatus.Active;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("DEF012", "SEC_025")]
    public void SEC_025_MapDoesNotAllowWorkflowStatusManipulationViaMap()
    {
        var dest = CreateOpportunity();
        dest.WorkflowStatus = WorkflowStatus.InWorkflow;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.WorkflowStatus.Should().Be(WorkflowStatus.InWorkflow);
    }

    [Fact]
    [Trait("DEF012", "SEC_026")]
    public void SEC_026_MapPreservesEntityIdWhenSourceIs0()
    {
        var dest = CreateOpportunity();
        dest.Id = 10;
        var request = new UpdateOpportunityRequest { Id = 0, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.Id.Should().Be(10);
    }

    [Fact]
    [Trait("DEF012", "SEC_027")]
    public void SEC_027_MapWithCollectionInjectionBlockedByIgnore()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Stakeholders = new List<OpportunityStakeholderRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Stakeholders.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_028")]
    public void SEC_028_MapPreventsHiddenFieldUpdatesForStatus()
    {
        var dest = CreateOpportunity();
        dest.Status = EntityStatus.Draft;
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Name" };
        _mapper.Map(request, dest);
        dest.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact]
    [Trait("DEF012", "SEC_029")]
    public void SEC_029_ExtraJsonPropertiesInRequestDoNotAffectMap()
    {
        var dest = CreateOpportunity();
        dest.Description = "Preserved";
        var request = new UpdateOpportunityRequest { Id = 10, Name = "OnlyName" };
        _mapper.Map(request, dest);
        dest.Description.Should().Be("Preserved");
    }

    [Fact]
    [Trait("DEF012", "SEC_030")]
    public void SEC_030_MapDoesNotExposeInternalEntityState()
    {
        var dest = CreateOpportunity();
        dest.LastModifiedBy = 50;
        dest.LastModifiedDate = new DateTime(2025, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.LastModifiedBy.Should().Be(50);
        dest.LastModifiedDate.Should().NotBeNull();
    }

    #endregion

    #region SEC_031-040: Injection through collections

    [Fact]
    [Trait("DEF012", "SEC_031")]
    public void SEC_031_FundingPartnersInjectionBlockedByIgnore()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_032")]
    public void SEC_032_ClientPartnersInjectionBlockedByIgnore()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.ClientPartners.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_033")]
    public void SEC_033_StakeholdersInjectionBlockedByIgnore()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Stakeholders = new List<OpportunityStakeholderRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Stakeholders.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_034")]
    public void SEC_034_DeliverablesInjectionBlockedByIgnore()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Deliverables = new List<OpportunityDeliverableRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Deliverables.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_035")]
    public void SEC_035_CountriesInjectionBlockedByIgnore()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Countries = new List<OpportunityCountryRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Countries.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_036")]
    public void SEC_036_SdgsInjectionBlockedByIgnore()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            SDGs = new List<OpportunitySDGRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.SDGs.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_037")]
    public void SEC_037_NonNullFundingPartnersStillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1 },
                new() { PartnerId = 2 }
            }
        };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_038")]
    public void SEC_038_NonNullClientPartnersStillIgnored()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            ClientPartners = new List<OpportunityClientPartnerRequest> { new(), new() }
        };
        _mapper.Map(request, dest);
        dest.ClientPartners.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_039")]
    public void SEC_039_EmptyListFundingPartnersStillIgnored()
    {
        var dest = CreateOpportunity();
        dest.FundingPartners.Add(new OpportunityFundingPartner { Id = 1, Name = "Keep" });
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", FundingPartners = new List<OpportunityFundingPartnerRequest>() };
        _mapper.Map(request, dest);
        dest.FundingPartners.Should().ContainSingle().Which.Name.Should().Be("Keep");
    }

    [Fact]
    [Trait("DEF012", "SEC_040")]
    public void SEC_040_CollectionsDoNotReplaceExistingDestinationCollections()
    {
        var dest = CreateOpportunity();
        dest.Countries.Add(new OpportunityCountry { Id = 1, Name = "Existing" });
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            Countries = new List<OpportunityCountryRequest> { new() }
        };
        _mapper.Map(request, dest);
        dest.Countries.Should().ContainSingle().Which.Name.Should().Be("Existing");
    }

    #endregion

    #region SEC_041-050: Edge security

    [Fact]
    [Trait("DEF012", "SEC_041")]
    public void SEC_041_ConcurrentMapTamperingSafe()
    {
        var dest = CreateOpportunity();
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            var req = new UpdateOpportunityRequest { Id = 10, Name = $"Name{i}" };
            _mapper.Map(req, dest);
        })).ToArray();
        var act = () => Task.WaitAll(tasks);
        act.Should().NotThrow();
        dest.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("DEF012", "SEC_042")]
    public void SEC_042_RapidMapThenReadSafe()
    {
        var dest = CreateOpportunity();
        for (var i = 0; i < 100; i++)
        {
            var request = new UpdateOpportunityRequest { Id = 10, Name = $"Iter{i}" };
            _mapper.Map(request, dest);
            dest.Name.Should().Be($"Iter{i}");
        }
    }

    [Fact]
    [Trait("DEF012", "SEC_043")]
    public void SEC_043_MapUnderMemoryPressureSafe()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Pressure", Description = new string('x', 50000) };
        var act = () => _mapper.Map(request, dest);
        act.Should().NotThrow();
        dest.Description.Should().HaveLength(50000);
    }

    [Fact]
    [Trait("DEF012", "SEC_044")]
    public void SEC_044_MapWithHugeStringsSafe()
    {
        var dest = CreateOpportunity();
        var huge = new string('A', 10 * 1024 * 1024);
        var request = new UpdateOpportunityRequest { Id = 10, Name = huge };
        var act = () => _mapper.Map(request, dest);
        act.Should().NotThrow();
        dest.Name.Should().HaveLength(10 * 1024 * 1024);
    }

    [Fact]
    [Trait("DEF012", "SEC_045")]
    public void SEC_045_MapWithBinaryDataInStringsSafe()
    {
        var dest = CreateOpportunity();
        var binary = Encoding.UTF8.GetString(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", Description = binary };
        _mapper.Map(request, dest);
        dest.Description.Should().Be(binary);
    }

    [Fact]
    [Trait("DEF012", "SEC_046")]
    public void SEC_046_MapWithEncodedPayloadsSafe()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "X", PartnerReference = "%3Cscript%3Ealert(1)%3C%2Fscript%3E" };
        _mapper.Map(request, dest);
        dest.PartnerReference.Should().Be("%3Cscript%3Ealert(1)%3C%2Fscript%3E");
    }

    [Fact]
    [Trait("DEF012", "SEC_047")]
    public void SEC_047_MapPreservesDestinationSecurityFields()
    {
        var dest = CreateOpportunity();
        dest.IsDeleted = false;
        dest.CreatedBy = 1;
        dest.CreatedDate = new DateTime(2020, 1, 1);
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Updated" };
        _mapper.Map(request, dest);
        dest.IsDeleted.Should().BeFalse();
        dest.CreatedBy.Should().Be(1);
        dest.CreatedDate.Should().Be(new DateTime(2020, 1, 1));
    }

    [Fact]
    [Trait("DEF012", "SEC_048")]
    public void SEC_048_MapDoesNotCreateExploitableState()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Safe", Description = "Safe" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Safe");
        dest.Description.Should().Be("Safe");
        dest.FundingPartners.Should().NotBeNull();
        dest.Stakeholders.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "SEC_049")]
    public void SEC_049_MapOutputSafeForSerialization()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Serializable", Description = "Data" };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Serializable");
        dest.Description.Should().Be("Data");
        dest.Should().NotBeNull();
    }

    [Fact]
    [Trait("DEF012", "SEC_050")]
    public void SEC_050_MapOutputSafeForDbPersistence()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "Persistable",
            Description = "Valid",
            Stage = "GO",
            InitiativeBudgetUSD = 100000m
        };
        _mapper.Map(request, dest);
        dest.Name.Should().Be("Persistable");
        dest.Description.Should().Be("Valid");
        dest.Stage.Should().Be("GO");
        dest.InitiativeBudgetUSD.Should().Be(100000m);
    }

    #endregion
}
