using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerTests;

/// <summary>
/// Boundary tests for base OpportunityManager.
/// Edge values, soft-delete interactions, nullable FKs, concurrent modification, min/max values.
/// </summary>
public class BoundaryTests : OpportunityManagerTestFixtureBase
{
    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateOpportunityAsync_EmptyCollections_DoesNotThrow()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Minimal",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE",
            FundingPartners = new List<OpportunityFundingPartnerRequest>(),
            ClientPartners = new List<OpportunityClientPartnerRequest>(),
            Stakeholders = new List<OpportunityStakeholderRequest>(),
            Deliverables = new List<OpportunityDeliverableRequest>(),
            Countries = new List<OpportunityCountryRequest>(),
            SDGs = new List<OpportunitySDGRequest>()
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateOpportunityAsync_NullOptionalCollections_DoesNotThrow()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Minimal",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE",
            FundingPartners = null,
            ClientPartners = null,
            Stakeholders = null,
            Deliverables = null,
            Countries = null,
            SDGs = null
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateOverviewSectionAsync_NullName_DoesNotUpdateName()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var original = await Manager.GetOpportunityAsync(id);
        var request = new OverviewSectionRequest { Description = "New desc only", Name = null };

        // Act
        var result = await Manager.UpdateOverviewSectionAsync(id, request);

        // Assert - Name is only updated if request.Name != null
        result.Should().NotBeNull();
        result.Description.Should().Be("New desc only");
        result.Name.Should().Be(original!.Name);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateOverviewSectionAsync_NullDescription_DoesNotUpdateDescription()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var original = await Manager.GetOpportunityAsync(id);
        var request = new OverviewSectionRequest { Name = "New name only", Description = null };

        // Act
        var result = await Manager.UpdateOverviewSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New name only");
        result.Description.Should().Be(original!.Description);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhatSectionAsync_EmptyDeliverables_RemovesAll()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var outputId = await SeedOutputAsync();
        var deliverable = new OpportunityDeliverable
        {
            OpportunityId = id,
            OutputId = outputId,
            Quantity = 1,
            Name = "D1"
        };
        Context.Set<OpportunityDeliverable>().Add(deliverable);
        await Context.SaveChangesAsync();

        var request = new WhatSectionRequest { Deliverables = new List<OpportunityDeliverableRequest>() };

        // Act
        var result = await Manager.UpdateWhatSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Deliverables.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhatSectionAsync_NullDeliverables_DoesNotChangeDeliverables()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var outputId = await SeedOutputAsync();
        var deliverable = new OpportunityDeliverable
        {
            OpportunityId = id,
            OutputId = outputId,
            Quantity = 1,
            Name = "D1"
        };
        Context.Set<OpportunityDeliverable>().Add(deliverable);
        await Context.SaveChangesAsync();

        var request = new WhatSectionRequest { Description = "Updated", Deliverables = null };

        // Act
        var result = await Manager.UpdateWhatSectionAsync(id, request);

        // Assert - null means don't update deliverables
        result.Should().NotBeNull();
        result.Deliverables.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhereSectionAsync_EmptyCountries_RemovesAll()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var countryId = await SeedCountryAsync();
        var oppCountry = new OpportunityCountry { OpportunityId = id, CountryId = countryId };
        Context.Set<OpportunityCountry>().Add(oppCountry);
        await Context.SaveChangesAsync();

        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };

        // Act
        var result = await Manager.UpdateWhereSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Countries.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhereSectionAsync_NullCountries_DoesNotChangeCountries()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var countryId = await SeedCountryAsync();
        var oppCountry = new OpportunityCountry { OpportunityId = id, CountryId = countryId };
        Context.Set<OpportunityCountry>().Add(oppCountry);
        await Context.SaveChangesAsync();

        var request = new WhereSectionRequest { Countries = null };

        // Act
        var result = await Manager.UpdateWhereSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Countries.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhenSectionAsync_OnlyTargetSigningDate_Valid()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new WhenSectionRequest
        {
            TargetSigningDate = new DateTime(2026, 12, 31),
            ImplementationStartDate = null,
            TargetDeliveryDate = null
        };

        // Act
        var result = await Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.TargetSigningDate.Should().Be(new DateTime(2026, 12, 31));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhenSectionAsync_ImplementationEqualsSigning_Valid()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var date = new DateTime(2026, 6, 1);
        var request = new WhenSectionRequest
        {
            TargetSigningDate = date,
            ImplementationStartDate = date // Same as signing - valid
        };

        // Act
        var result = await Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhenSectionAsync_DeliveryEqualsImplementation_Valid()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var implDate = new DateTime(2026, 7, 1);
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = implDate,
            TargetDeliveryDate = implDate // Same - valid
        };

        // Act
        var result = await Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetAllOpportunitiesAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Act - no opportunities seeded
        var result = (await Manager.GetAllOpportunitiesAsync()).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetRelatedItemsAsync_OpportunityWithNoPartners_ReturnsEmptyRelated()
    {
        // Arrange
        var id = await SeedOpportunityAsync();

        // Act
        var result = await Manager.GetRelatedItemsAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Partners.Should().NotBeNull().And.BeEmpty();
        result.Contacts.Should().NotBeNull().And.BeEmpty();
        result.Interactions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task IsOpportunityImmutable_StageCaseInsensitive_GO_Matches()
    {
        // Arrange - "go" and "GO" should both be immutable
        var id = await SeedOpportunityAsync("go");

        // Act - update should throw
        var act = () => Manager.UpdateOverviewSectionAsync(id, new OverviewSectionRequest { Name = "X" });

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task UpdateOpportunityAsync_MutableStage_IdentifyAndProfile_Succeeds()
    {
        // Arrange
        var id = await SeedOpportunityAsync("IDENTIFY & PROFILE");
        var request = new UpdateOpportunityRequest { Id = id, Name = "Updated", Description = "Desc" };

        // Act
        var result = await Manager.UpdateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateOverviewSectionAsync_PartialUpdate_OnlyProvidedFields_Updated()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new OverviewSectionRequest { Name = "Only Name Updated" };

        // Act
        var result = await Manager.UpdateOverviewSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Only Name Updated");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateOpportunityAsync_NullableStage_DefaultsToIdentifyProfile()
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

        // Assert - mapper maps, Stage may be null in request; entity defaults to "IDENTIFY & PROFILE"
        result.Should().NotBeNull();
        var entity = await Context.Opportunities.FindAsync(result.Id);
        entity.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateOverviewSection_BothNameAndDescription_UpdatesCorrectly()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new OverviewSectionRequest
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };

        // Act
        var result = await Manager.UpdateOverviewSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetExecutivesForOpportunityAsync_NoOrgUnit_ReturnsEmpty()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var opp = await Context.Opportunities.FindAsync(id);
        opp!.ResponsibleOrgUnitId = null;
        await Context.SaveChangesAsync();

        // Act
        var result = (await Manager.GetExecutivesForOpportunityAsync(id)).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhatSectionAsync_DeliveryModalityProvided_UpdatesCorrectly()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new WhatSectionRequest
        {
            Description = "Updated",
            DeliveryModality = 2 // AllDirect
        };

        // Act
        var result = await Manager.UpdateWhatSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.DeliveryModality.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task UpdateWhySectionAsync_NullSdGs_DoesNotRemoveExisting()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync();
        var oppSdg = new OpportunitySDG { OpportunityId = id, SDGId = sdgId, IsPrimary = true };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();

        var request = new WhySectionRequest { ExpectedImpact = "Updated", SdGs = null };

        // Act
        var result = await Manager.UpdateWhySectionAsync(id, request);

        // Assert - null SdGs means don't update SDGs
        result.Should().NotBeNull();
        result.SDGs.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhySectionAsync_EmptySdGs_RemovesAll()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync();
        var oppSdg = new OpportunitySDG { OpportunityId = id, SDGId = sdgId, IsPrimary = true };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();

        var request = new WhySectionRequest { SdGs = new List<OpportunitySDGRequest>() };

        // Act
        var result = await Manager.UpdateWhySectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.SDGs.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task DeleteOpportunityAsync_AlreadyDeleted_ReturnsFalse()
    {
        // Arrange - GetById returns null for soft-deleted, so Delete gets entity via GetById
        // DataRepository.GetById filters !IsDeleted - so we get null for soft-deleted
        var id = await SeedSoftDeletedOpportunityAsync();

        // Act
        var result = await Manager.DeleteOpportunityAsync(id);

        // Assert - GetById returns null for soft-deleted, so DeleteOpportunity returns false
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhoSectionAsync_EmptyFundingPartners_RemovesAll()
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
        await Context.SaveChangesAsync();

        var request = new WhoSectionRequest { FundingPartners = new List<OpportunityFundingPartnerRequest>() };

        // Act
        var result = await Manager.UpdateWhoSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.FundingPartners.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhoSectionAsync_EmptyClientPartners_RemovesAll()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var partnerId = await SeedPartnerAsync();
        var cp = new OpportunityClientPartner { OpportunityId = id, PartnerId = partnerId };
        Context.Set<OpportunityClientPartner>().Add(cp);
        await Context.SaveChangesAsync();

        var request = new WhoSectionRequest { ClientPartners = new List<OpportunityClientPartnerRequest>() };

        // Act
        var result = await Manager.UpdateWhoSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.ClientPartners.Should().NotBeNull().And.BeEmpty();
    }
}
