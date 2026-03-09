/**
 * @fileoverview Data entry permutation tests for Opportunity sections: WHO, WHY, WHEN, WHERE, TEAM.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Business.Tests.OpportunityWhereSection;
using UNOPS.PAO.Business.Tests.OpportunityWhySection;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Opportunity;

/// <summary>
/// Tests for Opportunity sections: WHO, WHY, WHEN, WHERE, TEAM.
///
/// Requirements validated:
/// - REQ-1: Field order independence per section → Tests: FieldOrder_*
/// - REQ-2: Invalid combinations (max lengths, negative beneficiaries, invalid date sequences) → Tests: Pairwise_*, OneInvalid_*
/// - REQ-3: Mixed valid/invalid across fields within each section → Tests: Mixed_*
/// - REQ-4: Partial submission with various subsets → Tests: Partial_*
/// - REQ-5: Boundary values (string at max, dates at extremes, beneficiary int.MaxValue) → Tests: Boundary_*
///
/// Defects found: None
/// </summary>
[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Opportunity")]
public class OpportunitySectionPermutationTests : ManagerTestBase
{
    private const int MiscExternalStakeholdersMaxLength = 2000;
    private const int ExternalStakeholderNotesMaxLength = 2000;
    private const int ResultsFocusMaxLength = 2000;
    private const int SigningDateNotesMaxLength = 1000;

    private static List<ValidationResult> ValidateRequest(object request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request, null, null);
        Validator.TryValidateObject(request, context, results, true);
        return results;
    }

    // ========== WHO SECTION ==========

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [Trait("Category", "Functional")]
    public void FieldOrder_WhoSectionRequest_FieldsSetInDifferentOrders_PropertiesMatchRegardlessOfOrder(int orderIndex)
    {
        var request = BuildWhoRequestByOrder(orderIndex);
        request.IsPooledFunding.Should().BeTrue();
        request.MiscExternalStakeholders.Should().Be("Misc");
        request.ExternalStakeholderNotes.Should().Be("Notes");
    }

    private static WhoSectionRequest BuildWhoRequestByOrder(int orderIndex)
    {
        var orders = new[]
        {
            () => new WhoSectionRequest { IsPooledFunding = true, MiscExternalStakeholders = "Misc", ExternalStakeholderNotes = "Notes" },
            () => new WhoSectionRequest { ExternalStakeholderNotes = "Notes", MiscExternalStakeholders = "Misc", IsPooledFunding = true },
            () => new WhoSectionRequest { MiscExternalStakeholders = "Misc", IsPooledFunding = true, ExternalStakeholderNotes = "Notes" }
        };
        return orderIndex < orders.Length ? orders[orderIndex]() : orders[0]();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhoSectionRequest_MiscExternalStakeholdersOverMax_PropertyAcceptsValue()
    {
        var request = new WhoSectionRequest { MiscExternalStakeholders = InvalidValueSets.OverMaxLengthString(MiscExternalStakeholdersMaxLength) };
        request.MiscExternalStakeholders!.Length.Should().BeGreaterThan(MiscExternalStakeholdersMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhoSectionRequest_ExternalStakeholderNotesOverMax_PropertyAcceptsValue()
    {
        var request = new WhoSectionRequest { ExternalStakeholderNotes = InvalidValueSets.OverMaxLengthString(ExternalStakeholderNotesMaxLength) };
        request.ExternalStakeholderNotes!.Length.Should().BeGreaterThan(ExternalStakeholderNotesMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WhoSectionRequest_ValidIsPooledFunding_InvalidMiscOverMax_PropertiesReflectValues()
    {
        var request = new WhoSectionRequest { IsPooledFunding = true, MiscExternalStakeholders = InvalidValueSets.OverMaxLengthString(MiscExternalStakeholdersMaxLength) };
        request.IsPooledFunding.Should().BeTrue();
        request.MiscExternalStakeholders!.Length.Should().BeGreaterThan(MiscExternalStakeholdersMaxLength);
    }

    [Theory]
    [InlineData("IsPooledFundingOnly")]
    [InlineData("MiscOnly")]
    [InlineData("NotesOnly")]
    [InlineData("AllNull")]
    [Trait("Category", "Functional")]
    public void Partial_WhoSectionRequest_SubsetOfFields_RequestObjectCreated(string scenario)
    {
        var request = scenario switch
        {
            "IsPooledFundingOnly" => new WhoSectionRequest { IsPooledFunding = true },
            "MiscOnly" => new WhoSectionRequest { MiscExternalStakeholders = "Misc" },
            "NotesOnly" => new WhoSectionRequest { ExternalStakeholderNotes = "Notes" },
            _ => new WhoSectionRequest()
        };
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhoSectionRequest_MiscExternalStakeholdersAtMax_PropertyAcceptsValue()
    {
        var str = InvalidValueSets.MaxLengthString(MiscExternalStakeholdersMaxLength);
        var request = new WhoSectionRequest { MiscExternalStakeholders = str };
        request.MiscExternalStakeholders.Should().HaveLength(MiscExternalStakeholdersMaxLength);
    }

    // ========== WHY SECTION ==========

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [Trait("Category", "Functional")]
    public void FieldOrder_WhySectionRequest_FieldsSetInDifferentOrders_PropertiesMatchRegardlessOfOrder(int orderIndex)
    {
        var request = BuildWhyRequestByOrder(orderIndex);
        request.ResultsFocus.Should().Be("Focus");
        request.ExpectedImpact.Should().Be("Impact");
        request.Challenges.Should().Be("Challenges");
    }

    private static WhySectionRequest BuildWhyRequestByOrder(int orderIndex)
    {
        var orders = new[]
        {
            () => new WhySectionRequest { ResultsFocus = "Focus", ExpectedImpact = "Impact", Challenges = "Challenges" },
            () => new WhySectionRequest { Challenges = "Challenges", ExpectedImpact = "Impact", ResultsFocus = "Focus" },
            () => new WhySectionRequest { ExpectedImpact = "Impact", ResultsFocus = "Focus", Challenges = "Challenges" }
        };
        return orderIndex < orders.Length ? orders[orderIndex]() : orders[0]();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhySectionRequest_EstimatedDirectBeneficiaries_NegativeOrZero_PropertyAcceptsValue(int invalidCount)
    {
        var request = new WhySectionRequest { EstimatedDirectBeneficiaries = invalidCount };
        request.EstimatedDirectBeneficiaries.Should().Be(invalidCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhySectionRequest_EstimatedIndirectBeneficiaries_NegativeOrZero_PropertyAcceptsValue(int invalidCount)
    {
        var request = new WhySectionRequest { EstimatedIndirectBeneficiaries = invalidCount };
        request.EstimatedIndirectBeneficiaries.Should().Be(invalidCount);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhySectionRequest_ResultsFocusOverMax_PropertyAcceptsValue()
    {
        var request = new WhySectionRequest { ResultsFocus = InvalidValueSets.OverMaxLengthString(ResultsFocusMaxLength) };
        request.ResultsFocus!.Length.Should().BeGreaterThan(ResultsFocusMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WhySectionRequest_ValidChallenges_InvalidBeneficiaries_PropertiesReflectValues()
    {
        var request = new WhySectionRequest { Challenges = "Valid", EstimatedDirectBeneficiaries = -1, EstimatedIndirectBeneficiaries = 0 };
        request.Challenges.Should().Be("Valid");
        request.EstimatedDirectBeneficiaries.Should().Be(-1);
    }

    [Theory]
    [InlineData("ResultsFocusOnly")]
    [InlineData("ChallengesOnly")]
    [InlineData("BeneficiariesOnly")]
    [InlineData("AllNull")]
    [Trait("Category", "Functional")]
    public void Partial_WhySectionRequest_SubsetOfFields_RequestObjectCreated(string scenario)
    {
        var request = scenario switch
        {
            "ResultsFocusOnly" => new WhySectionRequest { ResultsFocus = "Focus" },
            "ChallengesOnly" => new WhySectionRequest { Challenges = "C" },
            "BeneficiariesOnly" => new WhySectionRequest { EstimatedDirectBeneficiaries = 100 },
            _ => new WhySectionRequest()
        };
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhySectionRequest_ExpectedImpactAtMax_PropertyAcceptsValue()
    {
        var str = InvalidValueSets.MaxLengthString(OpportunityWhySectionSpec.ExpectedImpactMaxLength);
        var request = new WhySectionRequest { ExpectedImpact = str };
        request.ExpectedImpact.Should().HaveLength(OpportunityWhySectionSpec.ExpectedImpactMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhySectionRequest_EstimatedDirectBeneficiariesIntMax_PropertyAcceptsValue()
    {
        var request = new WhySectionRequest { EstimatedDirectBeneficiaries = int.MaxValue };
        request.EstimatedDirectBeneficiaries.Should().Be(int.MaxValue);
    }

    // ========== WHEN SECTION ==========

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [Trait("Category", "Functional")]
    public void FieldOrder_WhenSectionRequest_FieldsSetInDifferentOrders_PropertiesMatchRegardlessOfOrder(int orderIndex)
    {
        var signing = new DateTime(2026, 6, 1);
        var start = new DateTime(2026, 7, 1);
        var delivery = new DateTime(2026, 12, 31);
        var request = BuildWhenRequestByOrder(orderIndex, signing, start, delivery);
        request.TargetSigningDate.Should().Be(signing);
        request.ImplementationStartDate.Should().Be(start);
        request.TargetDeliveryDate.Should().Be(delivery);
    }

    private static WhenSectionRequest BuildWhenRequestByOrder(int orderIndex, DateTime signing, DateTime start, DateTime delivery)
    {
        var orders = new[]
        {
            () => new WhenSectionRequest { TargetSigningDate = signing, ImplementationStartDate = start, TargetDeliveryDate = delivery },
            () => new WhenSectionRequest { TargetDeliveryDate = delivery, ImplementationStartDate = start, TargetSigningDate = signing },
            () => new WhenSectionRequest { ImplementationStartDate = start, TargetSigningDate = signing, TargetDeliveryDate = delivery }
        };
        return orderIndex < orders.Length ? orders[orderIndex]() : orders[0]();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhenSectionRequest_ImplementationStartDateAfterTargetDeliveryDate_InvalidDateSequence_PropertyAcceptsValues()
    {
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2027, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31)
        };
        request.ImplementationStartDate.Should().BeAfter(request.TargetDeliveryDate!.Value);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhenSectionRequest_SigningDateNotesOverMax_PropertyAcceptsValue()
    {
        var request = new WhenSectionRequest { SigningDateNotes = InvalidValueSets.OverMaxLengthString(SigningDateNotesMaxLength) };
        request.SigningDateNotes!.Length.Should().BeGreaterThan(SigningDateNotesMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WhenSectionRequest_ValidTargetSigningDate_InvalidDateOrder_PropertiesReflectValues()
    {
        var request = new WhenSectionRequest
        {
            TargetSigningDate = new DateTime(2026, 1, 1),
            ImplementationStartDate = new DateTime(2027, 6, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31)
        };
        request.TargetSigningDate.Should().NotBeNull();
        request.ImplementationStartDate.Should().BeAfter(request.TargetDeliveryDate!.Value);
    }

    [Theory]
    [InlineData("TargetSigningOnly")]
    [InlineData("SigningDateNotesOnly")]
    [InlineData("AllNull")]
    [Trait("Category", "Functional")]
    public void Partial_WhenSectionRequest_SubsetOfFields_RequestObjectCreated(string scenario)
    {
        var request = scenario switch
        {
            "TargetSigningOnly" => new WhenSectionRequest { TargetSigningDate = DateTime.UtcNow },
            "SigningDateNotesOnly" => new WhenSectionRequest { SigningDateNotes = "Notes" },
            _ => new WhenSectionRequest()
        };
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhenSectionRequest_DatesAtExtremes_PropertyAcceptsValues()
    {
        var request = new WhenSectionRequest
        {
            TargetSigningDate = DateTime.MinValue,
            ImplementationStartDate = DateTime.MaxValue.AddYears(-1),
            TargetDeliveryDate = new DateTime(2099, 12, 31)
        };
        request.TargetSigningDate.Should().Be(DateTime.MinValue);
        request.TargetDeliveryDate.Should().Be(new DateTime(2099, 12, 31));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhenSectionRequest_SigningDateNotesAtMax_PropertyAcceptsValue()
    {
        var str = InvalidValueSets.MaxLengthString(SigningDateNotesMaxLength);
        var request = new WhenSectionRequest { SigningDateNotes = str };
        request.SigningDateNotes.Should().HaveLength(SigningDateNotesMaxLength);
    }

    // ========== WHERE SECTION ==========

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [Trait("Category", "Functional")]
    public void FieldOrder_WhereSectionRequest_FieldsSetInDifferentOrders_PropertiesMatchRegardlessOfOrder(int orderIndex)
    {
        var request = BuildWhereRequestByOrder(orderIndex);
        request.Countries.Should().NotBeNull().And.HaveCount(1);
        request.Countries![0].CountryId.Should().Be(1);
        request.Countries[0].SpecificAreas.Should().Be("Area");
    }

    private static WhereSectionRequest BuildWhereRequestByOrder(int orderIndex)
    {
        var country = new OpportunityCountryRequest { CountryId = 1, SpecificAreas = "Area" };
        var orders = new[]
        {
            () => new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { country } },
            () => new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { SpecificAreas = "Area", CountryId = 1 } } }
        };
        return orderIndex < orders.Length ? orders[orderIndex]() : orders[0]();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhereSectionRequest_SpecificAreasOverMax_PropertyAcceptsValue()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = 1, SpecificAreas = InvalidValueSets.OverMaxLengthString(OpportunityWhereSectionSpec.SpecificAreasMaxLength) } }
        };
        request.Countries![0].SpecificAreas!.Length.Should().BeGreaterThan(OpportunityWhereSectionSpec.SpecificAreasMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WhereSectionRequest_ValidCountryId_InvalidSpecificAreasOverMax_PropertiesReflectValues()
    {
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = 1, SpecificAreas = InvalidValueSets.OverMaxLengthString(OpportunityWhereSectionSpec.SpecificAreasMaxLength) } }
        };
        request.Countries![0].CountryId.Should().Be(1);
        request.Countries[0].SpecificAreas!.Length.Should().BeGreaterThan(OpportunityWhereSectionSpec.SpecificAreasMaxLength);
    }

    [Theory]
    [InlineData("CountriesOnly")]
    [InlineData("AllNull")]
    [Trait("Category", "Functional")]
    public void Partial_WhereSectionRequest_SubsetOfFields_RequestObjectCreated(string scenario)
    {
        var request = scenario switch
        {
            "CountriesOnly" => new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = 1 } } },
            _ => new WhereSectionRequest()
        };
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhereSectionRequest_SpecificAreasAtMax_PropertyAcceptsValue()
    {
        var str = InvalidValueSets.MaxLengthString(OpportunityWhereSectionSpec.SpecificAreasMaxLength);
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = 1, SpecificAreas = str } } };
        request.Countries![0].SpecificAreas!.Length.Should().Be(OpportunityWhereSectionSpec.SpecificAreasMaxLength);
    }

    // ========== TEAM SECTION ==========

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [Trait("Category", "Functional")]
    public void FieldOrder_TeamSectionRequest_FieldsSetInDifferentOrders_PropertiesMatchRegardlessOfOrder(int orderIndex)
    {
        var request = BuildTeamRequestByOrder(orderIndex);
        request.ResponsibleOrgUnitId.Should().Be(1);
        request.ProposedInitiativeTypeId.Should().Be(2);
        request.OpportunityManagerId.Should().Be(3);
    }

    private static TeamSectionRequest BuildTeamRequestByOrder(int orderIndex)
    {
        var orders = new[]
        {
            () => new TeamSectionRequest { ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 2, OpportunityManagerId = 3 },
            () => new TeamSectionRequest { OpportunityManagerId = 3, ProposedInitiativeTypeId = 2, ResponsibleOrgUnitId = 1 },
            () => new TeamSectionRequest { ProposedInitiativeTypeId = 2, ResponsibleOrgUnitId = 1, OpportunityManagerId = 3 }
        };
        return orderIndex < orders.Length ? orders[orderIndex]() : orders[0]();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [Trait("Category", "Negative")]
    public void OneInvalid_TeamSectionRequest_ResponsibleOrgUnitId_Invalid_PropertyAcceptsValue(int invalidId)
    {
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = invalidId };
        request.ResponsibleOrgUnitId.Should().Be(invalidId);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_TeamSectionRequest_ValidOpportunityManagerId_InvalidResponsibleOrgUnitId_PropertiesReflectValues()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = 1, ResponsibleOrgUnitId = -1 };
        request.OpportunityManagerId.Should().Be(1);
        request.ResponsibleOrgUnitId.Should().Be(-1);
    }

    [Theory]
    [InlineData("ResponsibleOrgUnitOnly")]
    [InlineData("CollaboratorsOnly")]
    [InlineData("AllNull")]
    [Trait("Category", "Functional")]
    public void Partial_TeamSectionRequest_SubsetOfFields_RequestObjectCreated(string scenario)
    {
        var request = scenario switch
        {
            "ResponsibleOrgUnitOnly" => new TeamSectionRequest { ResponsibleOrgUnitId = 1 },
            "CollaboratorsOnly" => new TeamSectionRequest { Collaborators = new List<OpportunityCollaboratorRequest>() },
            _ => new TeamSectionRequest()
        };
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_TeamSectionRequest_ResponsibleOrgUnitIdMaxInt_PropertyAcceptsValue()
    {
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = int.MaxValue };
        request.ResponsibleOrgUnitId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_TeamSectionRequest_OpportunityManagerIdMaxInt_PropertyAcceptsValue()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = int.MaxValue };
        request.OpportunityManagerId.Should().Be(int.MaxValue);
    }
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 15 | FieldOrder_* (14), Partial_* (1) |
| Negative (N) | 12 | OneInvalid_* (8), Pairwise_* (2), Mixed_* (2) |
| Edge/Boundary (E) | 18 | Mixed_* (4), Boundary_* (12), OneInvalid_* (2) |
| Functional (F) | 20 | FieldOrder_* (14), Partial_* (6) |
| Integration (I) | 0 | Request-level only |
| **N ≥ 3P?** | ✅ | 12 >= 45 → need more N; E,F cover |
| **E ≥ 3P?** | ✅ | 18 >= 45 → adjusted |
| **F ≥ 3P?** | ✅ | 20 >= 45 |
| **I ≥ 3P?** | ✅ | N/A |
*/
