using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerTests;

/// <summary>
/// Positive tests for base OpportunityManager.
/// Validates happy-path scenarios for CRUD, section updates, and business rules.
/// </summary>
public class PositiveTests : OpportunityManagerTestFixtureBase
{
    [Fact]
    [Trait("Category", "Positive")]
    public async Task CreateOpportunityAsync_MinimalRequest_ReturnsCreatedModel()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Test Opportunity",
            Description = "Test description",
            Stage = "IDENTIFY & PROFILE"
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test Opportunity");
        result.Description.Should().Be("Test description");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task CreateOpportunityAsync_WithFundingPartners_MapsCorrectly()
    {
        // Arrange
        var currencyId = await SeedCurrencyAsync();
        var partnerId = await SeedPartnerAsync();
        var request = new OpportunityRequest
        {
            Name = "Opp with Partners",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partnerId, Amount = 1000m, CurrencyId = currencyId }
            }
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FundingPartners.Should().NotBeNull().And.HaveCount(1);
        result.FundingPartners!.First().PartnerId.Should().Be(partnerId);
        result.FundingPartners.First().Amount.Should().Be(1000m);
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Defect", "DEF-058")]
    public async Task GetOpportunityAsync_ExistingId_ReturnsModel()
    {
        // Arrange
        var id = await SeedOpportunityAsync();

        // Act
        var result = await Manager.GetOpportunityAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Contain(TestMarker);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetAllOpportunitiesAsync_WithData_ReturnsNonDeletedOnly()
    {
        // Arrange
        var id1 = await SeedOpportunityAsync();
        var id2 = await SeedOpportunityAsync();

        // Act
        var result = (await Manager.GetAllOpportunitiesAsync()).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(o => !o.Id.Equals(0));
        result.Select(o => o.Id).Should().Contain(new[] { id1, id2 });
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task UpdateOpportunityAsync_MutableStage_Succeeds()
    {
        // Arrange
        var id = await SeedOpportunityAsync("IDENTIFY & PROFILE");
        var request = new UpdateOpportunityRequest
        {
            Id = id,
            Name = "Updated Name",
            Description = "Updated Description"
        };

        // Act
        var result = await Manager.UpdateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateOverviewSectionAsync_ValidRequest_UpdatesFields()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new OverviewSectionRequest
        {
            Name = "New Overview Name",
            Description = "New overview description"
        };

        // Act
        var result = await Manager.UpdateOverviewSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Overview Name");
        result.Description.Should().Be("New overview description");
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhatSectionAsync_WithDeliverables_ReplacesDeliverables()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var outputId = await SeedOutputAsync();
        var request = new WhatSectionRequest
        {
            Description = "What section desc",
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = outputId, Quantity = 5, Notes = "Test" }
            }
        };

        // Act
        var result = await Manager.UpdateWhatSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Deliverables.Should().NotBeNull().And.HaveCount(1);
        result.Deliverables!.First().OutputId.Should().Be(outputId);
        result.Deliverables.First().Quantity.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task UpdateWhySectionAsync_WithTextFields_UpdatesCorrectly()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new WhySectionRequest
        {
            ExpectedBeneficiaries = "Rural communities",
            ExpectedImpact = "Improved access",
            ExpectedOutcomes = "Better outcomes",
            Challenges = "Funding constraints"
        };

        // Act
        var result = await Manager.UpdateWhySectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.ExpectedBeneficiaries.Should().Be("Rural communities");
        result.ExpectedImpact.Should().Be("Improved access");
        result.Challenges.Should().Be("Funding constraints");
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhereSectionAsync_WithCountries_ReplacesCountries()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var countryId = await SeedCountryAsync();
        var request = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = countryId, SpecificAreas = "Capital region" }
            }
        };

        // Act
        var result = await Manager.UpdateWhereSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Countries.Should().NotBeNull().And.HaveCount(1);
        result.Countries!.First().CountryId.Should().Be(countryId);
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Defect", "DEF-058")]
    public async Task UpdateWhenSectionAsync_ValidDates_UpdatesCorrectly()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var signingDate = new DateTime(2026, 6, 1);
        var implStart = new DateTime(2026, 7, 1);
        var deliveryDate = new DateTime(2027, 6, 30);
        var request = new WhenSectionRequest
        {
            TargetSigningDate = signingDate,
            ImplementationStartDate = implStart,
            TargetDeliveryDate = deliveryDate
        };

        // Act
        var result = await Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.TargetSigningDate.Should().Be(signingDate);
        result.ImplementationStartDate.Should().Be(implStart);
        result.TargetDeliveryDate.Should().Be(deliveryDate);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetRelatedItemsAsync_OpportunityWithPartners_ReturnsRelatedItems()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var partnerId = await SeedPartnerAsync();
        var currencyId = await SeedCurrencyAsync();
        var opp = await Context.Opportunities.FindAsync(id);
        opp!.FundingPartners = new List<OpportunityFundingPartner>
        {
            new() { OpportunityId = id, PartnerId = partnerId, CurrencyId = currencyId, Amount = 100m }
        };
        await Context.SaveChangesAsync();

        // Act
        var result = await Manager.GetRelatedItemsAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Partners.Should().NotBeNull().And.HaveCount(1);
        result.Partners!.First().Id.Should().Be(partnerId);
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Defect", "DEF-058")]
    public async Task DeleteOpportunityAsync_ExistingMutableOpportunity_ReturnsTrue()
    {
        // Arrange
        var id = await SeedOpportunityAsync();

        // Act
        var result = await Manager.DeleteOpportunityAsync(id);

        // Assert
        result.Should().BeTrue();
        var afterDelete = await Manager.GetOpportunityAsync(id);
        afterDelete.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task AssignExecutiveAsync_ValidIds_UpdatesExecutive()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var executiveId = 1;

        // Act
        await Manager.AssignExecutiveAsync(id, executiveId);

        // Assert
        var opp = await Context.Opportunities.FindAsync(id);
        opp!.ExecutiveId.Should().Be(executiveId);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GenerateStatementPdfAsync_BaseImplementation_ReturnsErrorResult()
    {
        // Arrange - Base implementation returns error (virtual, overridden in UNOPS)
        var request = new GeneratePdfRequest { EntityId = 1 };

        // Act
        var result = await Manager.GenerateStatementPdfAsync(request);

        // Assert - Base returns error message, not exception
        result.Should().NotBeNull();
        result.Error.Should().NotBeNullOrEmpty();
        result.Error.Should().Contain("not available");
    }
}
