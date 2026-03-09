using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerTests;

/// <summary>
/// Functional tests for base OpportunityManager.
/// Business rules, audit fields, permissions, workflow transitions, data transformations.
/// </summary>
public class FunctionalTests : OpportunityManagerTestFixtureBase
{
    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreateOpportunityAsync_SetsStatusToDraft()
    {
        // Arrange
        var request = new OpportunityRequest { Name = "Test", Description = "Desc", Stage = "IDENTIFY & PROFILE" };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Draft");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreateOpportunityAsync_WithStage_SetsStageCorrectly()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Test",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE"
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetAllOpportunitiesAsync_FiltersIsDeleted()
    {
        // Arrange
        var activeId = await SeedOpportunityAsync("IDENTIFY & PROFILE", false);
        await SeedSoftDeletedOpportunityAsync();

        // Act
        var result = (await Manager.GetAllOpportunitiesAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task UpdateOpportunityAsync_PreservesId()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new UpdateOpportunityRequest { Id = id, Name = "Updated", Description = "Desc" };

        // Act
        var result = await Manager.UpdateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateOverviewSectionAsync_ReloadsWithIncludes()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new OverviewSectionRequest { Name = "Reloaded", Description = "Desc" };

        // Act
        var result = await Manager.UpdateOverviewSectionAsync(id, request);

        // Assert - GetOpportunityAsync returns full model with includes
        result.Should().NotBeNull();
        result.Name.Should().Be("Reloaded");
        result.ResponsibleOrgUnit.Should().BeNull(); // May be null if not set
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhatSectionAsync_UpdatesResponsibleOrgUnit()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var orgUnitId = await SeedOrgUnitAsync();
        var request = new WhatSectionRequest
        {
            Description = "Desc",
            ResponsibleOrgUnitId = orgUnitId
        };

        // Act
        var result = await Manager.UpdateWhatSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.ResponsibleOrgUnitId.Should().Be(orgUnitId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhatSectionAsync_UpdatesProposedInitiativeType()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var initTypeId = await SeedInitiativeTypeAsync();
        var request = new WhatSectionRequest
        {
            Description = "Desc",
            ProposedInitiativeTypeId = initTypeId
        };

        // Act
        var result = await Manager.UpdateWhatSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.ProposedInitiativeTypeId.Should().Be(initTypeId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task UpdateWhySectionAsync_UpdatesExpectedBeneficiaries()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new WhySectionRequest { ExpectedBeneficiaries = "Women and children" };

        // Act
        var result = await Manager.UpdateWhySectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.ExpectedBeneficiaries.Should().Be("Women and children");
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhoSectionAsync_AddsFundingPartners()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var partnerId = await SeedPartnerAsync();
        var currencyId = await SeedCurrencyAsync();
        var request = new WhoSectionRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partnerId, Amount = 5000m, CurrencyId = currencyId }
            }
        };

        // Act
        var result = await Manager.UpdateWhoSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.FundingPartners.Should().NotBeNull().And.HaveCount(1);
        result.FundingPartners!.First().Amount.Should().Be(5000m);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhoSectionAsync_AddsClientPartners()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var partnerId = await SeedPartnerAsync();
        var request = new WhoSectionRequest
        {
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = partnerId }
            }
        };

        // Act
        var result = await Manager.UpdateWhoSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.ClientPartners.Should().NotBeNull().And.HaveCount(1);
        result.ClientPartners!.First().PartnerId.Should().Be(partnerId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhereSectionAsync_ReplacesCountries()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var country1 = await SeedCountryAsync("AA", "Country A");
        var country2 = await SeedCountryAsync("BB", "Country B");
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = country1 },
                new() { CountryId = country2 }
            }
        };

        // Act
        var result = await Manager.UpdateWhereSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Countries.Should().NotBeNull().And.HaveCount(2);
        result.Countries!.Select(c => c.CountryId).Should().Contain(new[] { country1, country2 });
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhenSectionAsync_UpdatesAllDateFields()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var signing = new DateTime(2026, 5, 1);
        var impl = new DateTime(2026, 6, 1);
        var delivery = new DateTime(2027, 5, 31);
        var request = new WhenSectionRequest
        {
            TargetSigningDate = signing,
            ImplementationStartDate = impl,
            TargetDeliveryDate = delivery
        };

        // Act
        var result = await Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.TargetSigningDate.Should().Be(signing);
        result.ImplementationStartDate.Should().Be(impl);
        result.TargetDeliveryDate.Should().Be(delivery);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task DeleteOpportunityAsync_RemovesFromGetAll()
    {
        // Arrange
        var id = await SeedOpportunityAsync();

        // Act
        await Manager.DeleteOpportunityAsync(id);
        var all = (await Manager.GetAllOpportunitiesAsync()).ToList();

        // Assert
        all.Should().NotContain(o => o.Id == id);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetRelatedItemsAsync_IncludesContactsFromPartners()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var partnerId = await SeedPartnerAsync();
        var currencyId = await SeedCurrencyAsync();
        var fp = new OpportunityFundingPartner
        {
            OpportunityId = id,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            Amount = 100m,
            Name = "FP1"
        };
        Context.Set<OpportunityFundingPartner>().Add(fp);
        var contact = new Contact
        {
            PartnerId = partnerId,
            Name = "Contact 1",
            LastName = "Test",
            Title = "Manager",
            Email = "c1@test.com",
            IsDeleted = false
        };
        Context.Set<Contact>().Add(contact);
        await Context.SaveChangesAsync();

        // Act
        var result = await Manager.GetRelatedItemsAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Contacts.Should().NotBeNull().And.HaveCount(1);
        result.Contacts!.First().Name.Should().Be("Contact 1");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task AssignExecutiveAsync_PersistsToDatabase()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        const int executiveId = 42;

        // Act
        await Manager.AssignExecutiveAsync(id, executiveId);
        var opp = await Context.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

        // Assert
        opp.Should().NotBeNull();
        opp!.ExecutiveId.Should().Be(executiveId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-221")]
    public async Task UpdateOpportunityAsync_FullReplacementOfChildCollections()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var partner1 = await SeedPartnerAsync();
        var partner2 = await SeedPartnerAsync();
        var currencyId = await SeedCurrencyAsync();
        var fp = new OpportunityFundingPartner
        {
            OpportunityId = id,
            PartnerId = partner1,
            CurrencyId = currencyId,
            Amount = 100m,
            Name = "FP1"
        };
        Context.Set<OpportunityFundingPartner>().Add(fp);
        await Context.SaveChangesAsync();

        var request = new UpdateOpportunityRequest
        {
            Id = id,
            Name = "Updated",
            Description = "Desc",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partner2, Amount = 200m, CurrencyId = currencyId }
            }
        };

        // Act
        var result = await Manager.UpdateOpportunityAsync(request);

        // Assert - Old partner removed, new partner added
        result.Should().NotBeNull();
        result.FundingPartners.Should().NotBeNull().And.HaveCount(1);
        result.FundingPartners!.First().PartnerId.Should().Be(partner2);
        result.FundingPartners.First().Amount.Should().Be(200m);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task UpdateWhySectionAsync_SDGDifferential_AddsNewKeepsExisting()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var sdg1 = await SeedSDGAsync("1", "No Poverty");
        var sdg2 = await SeedSDGAsync("2", "Zero Hunger");
        var oppSdg = new OpportunitySDG { OpportunityId = id, SDGId = sdg1, IsPrimary = true };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();

        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdg1, IsPrimary = true }, // Keep existing
                new() { SDGId = sdg2, IsPrimary = false }  // Add new
            }
        };

        // Act
        var result = await Manager.UpdateWhySectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.SDGs.Should().NotBeNull().And.HaveCount(2);
        result.SDGs!.Select(s => s.SDGDatabaseId).Should().Contain(new[] { sdg1, sdg2 });
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhySectionAsync_SDGDifferential_RemovesMissing()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var sdg1 = await SeedSDGAsync("1", "No Poverty");
        var sdg2 = await SeedSDGAsync("2", "Zero Hunger");
        var oppSdg1 = new OpportunitySDG { OpportunityId = id, SDGId = sdg1, IsPrimary = true };
        var oppSdg2 = new OpportunitySDG { OpportunityId = id, SDGId = sdg2, IsPrimary = false };
        Context.Set<OpportunitySDG>().AddRange(oppSdg1, oppSdg2);
        await Context.SaveChangesAsync();

        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdg1, IsPrimary = true } // Remove sdg2
            }
        };

        // Act
        var result = await Manager.UpdateWhySectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.SDGs.Should().NotBeNull().And.HaveCount(1);
        result.SDGs!.First().SDGDatabaseId.Should().Be(sdg1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task GetOpportunityAsync_ReturnsMappedModel()
    {
        // Arrange
        var id = await SeedOpportunityAsync();

        // Act
        var result = await Manager.GetOpportunityAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().NotBeNullOrEmpty();
        result.Description.Should().NotBeNullOrEmpty();
        result.Stage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task CreateOpportunityAsync_WithDeliverables_MapsToEntity()
    {
        // Arrange
        var outputId = await SeedOutputAsync();
        var request = new OpportunityRequest
        {
            Name = "Test",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE",
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = outputId, Quantity = 10, Notes = "Deliverable 1" }
            }
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Deliverables.Should().NotBeNull().And.HaveCount(1);
        result.Deliverables!.First().Quantity.Should().Be(10);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-058")]
    public async Task CreateOpportunityAsync_WithCountries_MapsToEntity()
    {
        // Arrange
        var countryId = await SeedCountryAsync();
        var request = new OpportunityRequest
        {
            Name = "Test",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE",
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = countryId, SpecificAreas = "Capital" }
            }
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Countries.Should().NotBeNull().And.HaveCount(1);
        result.Countries!.First().CountryId.Should().Be(countryId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Immutability_AllThreeStages_BlockModification()
    {
        // Arrange
        var goId = await SeedImmutableOpportunityAsync("GO");
        var noGoId = await SeedImmutableOpportunityAsync("NO GO");
        var cancelledId = await SeedImmutableOpportunityAsync("CANCELLED");
        var request = new OverviewSectionRequest { Name = "Hack", Description = "Attempt" };

        // Act & Assert
        var act1 = () => Manager.UpdateOverviewSectionAsync(goId, request);
        await act1.Should().ThrowAsync<BusinessException>();
        var act2 = () => Manager.UpdateOverviewSectionAsync(noGoId, request);
        await act2.Should().ThrowAsync<BusinessException>();
        var act3 = () => Manager.UpdateOverviewSectionAsync(cancelledId, request);
        await act3.Should().ThrowAsync<BusinessException>();
    }
}
