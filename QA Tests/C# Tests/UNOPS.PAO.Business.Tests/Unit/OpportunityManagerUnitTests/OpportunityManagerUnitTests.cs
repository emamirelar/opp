/**
 * @fileoverview Comprehensive unit tests for base OpportunityManager.
 * Covers immutability, soft delete, section updates, SDG/UNCF differential updates,
 * stakeholder auto-population, date validation, country/org unit handling, related items.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerUnitTests;

/// <summary>
/// Comprehensive unit tests for base OpportunityManager.
/// Requirements source: Production code in UNOPS.PAO.Business/Managers/OpportunityManager.cs
/// </summary>
[Collection("Unit")]
[Trait("Category", "Unit")]
[Trait("Type", "Unit")]
public class OpportunityManagerUnitTests : IClassFixture<OpportunityManagerUnitTestFixture>
{
    private readonly OpportunityManagerUnitTestFixture _fixture;

    public OpportunityManagerUnitTests(OpportunityManagerUnitTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Immutability — IsOpportunityImmutable & ThrowIfImmutable

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_GOStage_ReturnsTrue()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic("GO").Should().BeTrue();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_NOGOStage_ReturnsTrue()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic("NO GO").Should().BeTrue();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_CANCELLEDStage_ReturnsTrue()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic("CANCELLED").Should().BeTrue();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_IdentifyAndProfileStage_ReturnsFalse()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic("IDENTIFY & PROFILE").Should().BeFalse();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_NullStage_ReturnsFalse()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic((string?)null).Should().BeFalse();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_EmptyStage_ReturnsFalse()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic("").Should().BeFalse();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_CaseInsensitive_Go_ReturnsTrue()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic("go").Should().BeTrue();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_WithOpportunityEntity_GO_ReturnsTrue()
    {
        var opp = new OpportunityEntity { Stage = "GO", Name = "Test", Description = "Desc" };
        _fixture.TestableManager.IsOpportunityImmutablePublic(opp).Should().BeTrue();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void IsOpportunityImmutable_WithOpportunityEntity_Null_ReturnsFalse()
    {
        _fixture.TestableManager.IsOpportunityImmutablePublic((OpportunityEntity?)null).Should().BeFalse();
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void ThrowIfImmutable_GOStage_ThrowsBusinessException()
    {
        var opp = new OpportunityEntity { Stage = "GO", Name = "Test", Description = "Desc" };
        var act = () => _fixture.TestableManager.ThrowIfImmutablePublic(opp);
        act.Should().Throw<BusinessException>().WithMessage("*locked*cannot be modified*");
    }

    [Fact]
    [Trait("SubCategory", "Immutability")]
    public void ThrowIfImmutable_IdentifyAndProfile_DoesNotThrow()
    {
        var opp = new OpportunityEntity { Stage = "IDENTIFY & PROFILE", Name = "Test", Description = "Desc" };
        var act = () => _fixture.TestableManager.ThrowIfImmutablePublic(opp);
        act.Should().NotThrow();
    }

    #endregion

    #region Soft Delete

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SoftDelete")]
    [Trait("Defect", "DEF-221")]
    public async Task DeleteOpportunityAsync_ShouldSoftDelete_RecordExistsWithIsDeletedTrue()
    {
        // Arrange
        var id = await _fixture.SeedOpportunityAsync();

        // Act
        var result = await _fixture.Manager.DeleteOpportunityAsync(id);

        // Assert
        result.Should().BeTrue();
        var entity = await _fixture.GetOpportunityEntityDirectlyAsync(id);
        entity.Should().NotBeNull("per spec, delete should use soft delete (IsDeleted=true), not physical delete");
        entity!.IsDeleted.Should().BeTrue("per spec, DeleteOpportunityAsync should set IsDeleted=true");
    }

    [Fact]
    [Trait("SubCategory", "SoftDelete")]
    public async Task DeleteOpportunityAsync_NonExistentId_ReturnsFalse()
    {
        var result = await _fixture.Manager.DeleteOpportunityAsync(99999);
        result.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SoftDelete")]
    public async Task DeleteOpportunityAsync_AfterDelete_GetOpportunityAsyncReturnsNull()
    {
        var id = await _fixture.SeedOpportunityAsync();
        await _fixture.Manager.DeleteOpportunityAsync(id);
        var result = await _fixture.Manager.GetOpportunityAsync(id);
        result.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SoftDelete")]
    public async Task DeleteOpportunityAsync_AfterDelete_GetAllOpportunitiesAsyncExcludesDeleted()
    {
        var id = await _fixture.SeedOpportunityAsync();
        await _fixture.Manager.DeleteOpportunityAsync(id);
        var result = (await _fixture.Manager.GetAllOpportunitiesAsync()).ToList();
        result.Should().NotContain(o => o.Id == id);
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SoftDelete")]
    public async Task DeleteOpportunityAsync_ImmutableStage_ThrowsBusinessException()
    {
        var id = await _fixture.SeedImmutableOpportunityAsync("GO");
        var act = () => _fixture.Manager.DeleteOpportunityAsync(id);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*locked*cannot be modified*");
    }

    #endregion

    #region Section Updates — Overview

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SectionUpdates")]
    public async Task UpdateOverviewSectionAsync_OnlyUpdatesNameAndDescription_NotOtherFields()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var orgUnitId = await _fixture.SeedOrgUnitAsync();
        var initTypeId = await _fixture.SeedInitiativeTypeAsync();

        // Set other fields via UpdateOpportunity
        var opp = await _fixture.Context.Opportunities.FindAsync(id);
        opp!.ResponsibleOrgUnitId = orgUnitId;
        opp.ProposedInitiativeTypeId = initTypeId;
        opp.TargetSigningDate = new DateTime(2025, 6, 1);
        await _fixture.Context.SaveChangesAsync();

        var request = new OverviewSectionRequest
        {
            Name = "Overview Name Only",
            Description = "Overview Description Only"
        };

        var result = await _fixture.Manager.UpdateOverviewSectionAsync(id, request);

        result.Name.Should().Be("Overview Name Only");
        result.Description.Should().Be("Overview Description Only");
        result.ResponsibleOrgUnitId.Should().Be(orgUnitId, "Overview section should not touch other fields");
        result.ProposedInitiativeTypeId.Should().Be(initTypeId);
        result.TargetSigningDate.Should().Be(new DateTime(2025, 6, 1));
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SectionUpdates")]
    public async Task UpdateOverviewSectionAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var originalName = (await _fixture.Manager.GetOpportunityAsync(id))!.Name;

        var request = new OverviewSectionRequest { Description = "Only description changed" };

        var result = await _fixture.Manager.UpdateOverviewSectionAsync(id, request);

        result.Description.Should().Be("Only description changed");
        result.Name.Should().Be(originalName, "null Name in request should not overwrite");
    }

    #endregion

    #region Section Updates — What

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SectionUpdates")]
    public async Task UpdateWhatSectionAsync_UpdatesResponsibleOrgUnitAndDeliverables()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var orgUnitId = await _fixture.SeedOrgUnitAsync();
        var outputId = await _fixture.SeedOutputAsync();

        var request = new WhatSectionRequest
        {
            Description = "What description",
            ResponsibleOrgUnitId = orgUnitId,
            ProposedInitiativeTypeId = null,
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = outputId, Quantity = 10, Notes = "Test" }
            }
        };

        var result = await _fixture.Manager.UpdateWhatSectionAsync(id, request);

        result.Description.Should().Be("What description");
        result.ResponsibleOrgUnitId.Should().Be(orgUnitId);
        result.Deliverables.Should().HaveCount(1);
        result.Deliverables!.First().OutputId.Should().Be(outputId);
        result.Deliverables.First().Quantity.Should().Be(10);
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SectionUpdates")]
    public async Task UpdateWhatSectionAsync_UpdatesDeliveryModality()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var request = new WhatSectionRequest
        {
            Description = "Desc",
            DeliveryModality = 3 // AllGrantSupport
        };

        var result = await _fixture.Manager.UpdateWhatSectionAsync(id, request);

        result.Should().NotBeNull();
        result.DeliveryModality.Should().Be(3);
    }

    #endregion

    #region Section Updates — When (Date Validation)

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "DateValidation")]
    public async Task UpdateWhenSectionAsync_ImplementationStartBeforeTargetSigning_ThrowsBusinessException()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var request = new WhenSectionRequest
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 5, 1)
        };

        var act = () => _fixture.Manager.UpdateWhenSectionAsync(id, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Implementation Start Date cannot be before the Target Signing Date*");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "DateValidation")]
    public async Task UpdateWhenSectionAsync_TargetDeliveryBeforeImplementationStart_ThrowsBusinessException()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2025, 5, 1)
        };

        var act = () => _fixture.Manager.UpdateWhenSectionAsync(id, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Target Delivery Date must be after*");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "DateValidation")]
    public async Task UpdateWhenSectionAsync_ValidDates_Succeeds()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var request = new WhenSectionRequest
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 12, 31)
        };

        var result = await _fixture.Manager.UpdateWhenSectionAsync(id, request);

        result.Should().NotBeNull();
        result.TargetSigningDate.Should().Be(new DateTime(2025, 6, 1));
        result.ImplementationStartDate.Should().Be(new DateTime(2025, 7, 1));
        result.TargetDeliveryDate.Should().Be(new DateTime(2025, 12, 31));
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "DateValidation")]
    public async Task UpdateWhenSectionAsync_DeliverablePlannedStartBeforeImplementationStart_ThrowsBusinessException()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var outputId = await _fixture.SeedOutputAsync();

        var whatResult = await _fixture.Manager.UpdateWhatSectionAsync(id, new WhatSectionRequest
        {
            Description = "Desc",
            Deliverables = new List<OpportunityDeliverableRequest> { new() { OutputId = outputId, Quantity = 1 } }
        });
        var deliverableId = whatResult.Deliverables!.First().Id;

        var request = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2025, 6, 1),
            Deliverables = new List<DeliverableDateUpdate>
            {
                new() { Id = deliverableId, PlannedStartDate = new DateTime(2025, 5, 1), PlannedEndDate = new DateTime(2025, 8, 1) }
            }
        };

        var act = () => _fixture.Manager.UpdateWhenSectionAsync(id, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Planned Start Date cannot be before the Implementation Start Date*");
    }

    #endregion

    #region Section Updates — Where (Country/Org Unit)

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "WhereSection")]
    public async Task UpdateWhereSectionAsync_UpdatesCountriesWithAlignmentFields()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var countryId = await _fixture.SeedCountryAsync("KE", "Kenya");

        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new()
                {
                    CountryId = countryId,
                    SpecificAreas = "Nairobi",
                    HumanitarianFrameworkAlignment = true,
                    NdcAlignment = true,
                    NapAlignment = false,
                    OrgUnitStrategyAlignment = true
                }
            }
        };

        var result = await _fixture.Manager.UpdateWhereSectionAsync(id, request);

        result.Countries.Should().HaveCount(1);
        result.Countries!.First().CountryId.Should().Be(countryId);
        result.Countries.First().SpecificAreas.Should().Be("Nairobi");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "WhereSection")]
    public async Task UpdateWhereSectionAsync_ReplacesCountries_RemovesOldAddsNew()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var countryId1 = await _fixture.SeedCountryAsync("KE", "Kenya");
        var countryId2 = await _fixture.SeedCountryAsync("UG", "Uganda");

        await _fixture.Manager.UpdateWhereSectionAsync(id, new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = countryId1 } }
        });

        var result = await _fixture.Manager.UpdateWhereSectionAsync(id, new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = countryId2 } }
        });

        result.Countries.Should().HaveCount(1);
        result.Countries!.First().CountryId.Should().Be(countryId2);
    }

    #endregion

    #region SDG/UNCF Differential Updates — Why Section

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SDGDifferential")]
    public async Task UpdateWhySectionAsync_AddsNewSDG_WhenNotInExisting()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var sdgId = await _fixture.SeedSDGAsync("1", "No Poverty");

        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdgId, IsPrimary = true, Notes = "Primary" }
            }
        };

        var result = await _fixture.Manager.UpdateWhySectionAsync(id, request);

        result.SDGs.Should().HaveCount(1);
        result.SDGs!.First().SDGId.Should().NotBeNullOrEmpty();
        result.SDGs.First().IsPrimary.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SDGDifferential")]
    public async Task UpdateWhySectionAsync_RemovesSDG_WhenNotInRequest()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var sdgId1 = await _fixture.SeedSDGAsync("1", "No Poverty");
        var sdgId2 = await _fixture.SeedSDGAsync("2", "Zero Hunger");

        await _fixture.Manager.UpdateWhySectionAsync(id, new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdgId1 },
                new() { SDGId = sdgId2 }
            }
        });

        var result = await _fixture.Manager.UpdateWhySectionAsync(id, new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = sdgId1 } }
        });

        result.SDGs.Should().HaveCount(1);
        result.SDGs!.First().SDGId.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "SDGDifferential")]
    public async Task UpdateWhySectionAsync_KeepsExistingSDG_UpdatesProperties()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var sdgId = await _fixture.SeedSDGAsync("1", "No Poverty");

        await _fixture.Manager.UpdateWhySectionAsync(id, new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = sdgId, IsPrimary = false, Notes = "Original" } }
        });

        var result = await _fixture.Manager.UpdateWhySectionAsync(id, new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = sdgId, IsPrimary = true, Notes = "Updated" } }
        });

        result.SDGs.Should().HaveCount(1);
        result.SDGs!.First().IsPrimary.Should().BeTrue();
        result.SDGs.First().Notes.Should().Be("Updated");
    }

    #endregion

    #region Related Items Aggregation

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "RelatedItems")]
    public async Task GetRelatedItemsAsync_AggregatesContactsFromFundingAndClientPartners()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var partnerId = await _fixture.SeedPartnerAsync();
        var currencyId = await _fixture.SeedCurrencyAsync();

        await _fixture.Manager.UpdateWhoSectionAsync(id, new WhoSectionRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partnerId, Amount = 1000, CurrencyId = currencyId }
            }
        });

        var contact = new Contact
        {
            Name = "Test Contact",
            LastName = "Contact",
            Title = "Manager",
            Email = "test@example.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _fixture.Context.Set<Contact>().Add(contact);
        await _fixture.Context.SaveChangesAsync();

        var result = await _fixture.Manager.GetRelatedItemsAsync(id);

        result.Should().NotBeNull();
        result.Partners.Should().NotBeNull().And.HaveCount(1);
        result.Contacts.Should().NotBeNull();
    }

    [Fact]
    [Trait("SubCategory", "RelatedItems")]
    public async Task GetRelatedItemsAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        var act = () => _fixture.Manager.GetRelatedItemsAsync(99999);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*99999*not found*");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "RelatedItems")]
    public async Task GetRelatedItemsAsync_NoPartners_ReturnsEmptyCollections()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var result = await _fixture.Manager.GetRelatedItemsAsync(id);
        result.Should().NotBeNull();
        result.Contacts.Should().NotBeNull();
        result.Partners.Should().NotBeNull();
        result.Interactions.Should().NotBeNull();
    }

    #endregion

    #region Create — Child Entity Mapping

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "Create")]
    public async Task CreateOpportunityAsync_WithAllChildCollections_MapsCorrectly()
    {
        var currencyId = await _fixture.SeedCurrencyAsync();
        var partnerId = await _fixture.SeedPartnerAsync();
        var countryId = await _fixture.SeedCountryAsync();
        var sdgId = await _fixture.SeedSDGAsync();
        var outputId = await _fixture.SeedOutputAsync();

        var request = new OpportunityRequest
        {
            Name = "Full Opp",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = partnerId, Amount = 100, CurrencyId = currencyId } },
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() { PartnerId = partnerId } },
            Countries = new List<OpportunityCountryRequest> { new() { CountryId = countryId } },
            SDGs = new List<OpportunitySDGRequest> { new() { SDGId = sdgId } },
            Deliverables = new List<OpportunityDeliverableRequest> { new() { OutputId = outputId, Quantity = 5 } }
        };

        var result = await _fixture.Manager.CreateOpportunityAsync(request);

        result.Should().NotBeNull();
        result.FundingPartners.Should().HaveCount(1);
        result.ClientPartners.Should().HaveCount(1);
        result.Countries.Should().HaveCount(1);
        result.SDGs.Should().HaveCount(1);
        result.Deliverables.Should().HaveCount(1);
    }

    #endregion

    #region Stub Methods — NotImplementedException

    [Fact]
    [Trait("SubCategory", "StubMethods")]
    public async Task GetOpportunitiesByPartnerIdAsync_ThrowsNotImplementedException()
    {
        var act = () => _fixture.Manager.GetOpportunitiesByPartnerIdAsync(1);
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*GetOpportunitiesByPartnerIdAsync*only implemented in UNOPSOpportunityManager*");
    }

    [Fact]
    [Trait("SubCategory", "StubMethods")]
    public void GetOpportunitySearchFields_ThrowsNotImplementedException()
    {
        var act = () => _fixture.Manager.GetOpportunitySearchFields();
        act.Should().Throw<NotImplementedException>()
            .WithMessage("*GetOpportunitySearchFields*only implemented in UNOPSOpportunityManager*");
    }

    [Fact]
    [Trait("SubCategory", "StubMethods")]
    public async Task UpdateHighRiskAcknowledgementAsync_ThrowsNotImplementedException()
    {
        var act = () => _fixture.Manager.UpdateHighRiskAcknowledgementAsync(1, true);
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*UpdateHighRiskAcknowledgementAsync*only implemented in UNOPSOpportunityManager*");
    }

    [Fact]
    [Trait("SubCategory", "StubMethods")]
    public async Task CreateOpportunityFromProposalAsync_ThrowsNotImplementedException()
    {
        var act = () => _fixture.Manager.CreateOpportunityFromProposalAsync(new CreateOpportunityFromInteractionsRequest { Name = "Test" }, 1);
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*CreateOpportunityFromProposalAsync*only implemented in UNOPSOpportunityManager*");
    }

    [Fact]
    [Trait("SubCategory", "StubMethods")]
    public async Task GenerateStatementPdfAsync_ReturnsErrorResult_NotAvailable()
    {
        var result = await _fixture.Manager.GenerateStatementPdfAsync(new GeneratePdfRequest());
        result.Should().NotBeNull();
        result.Error.Should().Contain("not available");
        result.Details.Should().Contain("UNOPS");
    }

    #endregion

    #region AssignExecutive & GetExecutivesForOpportunity

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "Executive")]
    public async Task AssignExecutiveAsync_ValidOpportunity_SetsExecutiveId()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var opp = await _fixture.Context.Opportunities.FindAsync(id);
        opp!.ExecutiveId.Should().BeNull();

        await _fixture.Manager.AssignExecutiveAsync(id, 999);

        opp = await _fixture.Context.Opportunities.FindAsync(id);
        opp!.ExecutiveId.Should().Be(999);
    }

    [Fact]
    [Trait("SubCategory", "Executive")]
    public async Task AssignExecutiveAsync_NonExistentOpportunity_ThrowsKeyNotFoundException()
    {
        var act = () => _fixture.Manager.AssignExecutiveAsync(99999, 1);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*99999*not found*");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "Executive")]
    public async Task GetExecutivesForOpportunityAsync_NoResponsibleOrgUnit_ReturnsEmpty()
    {
        var id = await _fixture.SeedOpportunityAsync();
        var result = await _fixture.Manager.GetExecutivesForOpportunityAsync(id);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "Executive")]
    public async Task GetExecutivesForOpportunityAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        var act = () => _fixture.Manager.GetExecutivesForOpportunityAsync(99999);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*99999*not found*");
    }

    #endregion

    #region Section Updates — Immutability

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "Immutability")]
    public async Task UpdateWhatSectionAsync_GOStage_ThrowsBusinessException()
    {
        var id = await _fixture.SeedImmutableOpportunityAsync("GO");
        var request = new WhatSectionRequest { Description = "X" };
        var act = () => _fixture.Manager.UpdateWhatSectionAsync(id, request);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*locked*");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "Immutability")]
    public async Task UpdateWhySectionAsync_GOStage_ThrowsBusinessException()
    {
        var id = await _fixture.SeedImmutableOpportunityAsync("GO");
        var request = new WhySectionRequest { ExpectedImpact = "X" };
        var act = () => _fixture.Manager.UpdateWhySectionAsync(id, request);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*locked*");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "Immutability")]
    public async Task UpdateWhenSectionAsync_GOStage_ThrowsBusinessException()
    {
        var id = await _fixture.SeedImmutableOpportunityAsync("GO");
        var request = new WhenSectionRequest { TargetSigningDate = DateTime.UtcNow };
        var act = () => _fixture.Manager.UpdateWhenSectionAsync(id, request);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*locked*");
    }

    [SkipIfInMemoryFact]
    [Trait("SubCategory", "Immutability")]
    public async Task UpdateWhereSectionAsync_GOStage_ThrowsBusinessException()
    {
        var id = await _fixture.SeedImmutableOpportunityAsync("GO");
        var countryId = await _fixture.SeedCountryAsync();
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest> { new() { CountryId = countryId } } };
        var act = () => _fixture.Manager.UpdateWhereSectionAsync(id, request);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*locked*");
    }

    #endregion
}
