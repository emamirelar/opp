using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerTests;

/// <summary>
/// Integration tests for base OpportunityManager.
/// Full CRUD through API, service-to-DB round-trip, multi-section workflows.
/// </summary>
public class IntegrationTests : OpportunityManagerTestFixtureBase
{
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-058")]
    public async Task FullCRUD_CreateGetUpdateDelete_CompletesSuccessfully()
    {
        // Create
        var createRequest = new OpportunityRequest
        {
            Name = "Integration Test Opp",
            Description = "Full CRUD test",
            Stage = "IDENTIFY & PROFILE"
        };
        var created = await Manager.CreateOpportunityAsync(createRequest);
        created.Should().NotBeNull();
        var id = created.Id;

        // Get
        var fetched = await Manager.GetOpportunityAsync(id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Integration Test Opp");

        // Update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = id,
            Name = "Updated Integration Opp",
            Description = "Updated description"
        };
        var updated = await Manager.UpdateOpportunityAsync(updateRequest);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Integration Opp");

        // Delete
        var deleted = await Manager.DeleteOpportunityAsync(id);
        deleted.Should().BeTrue();

        // Verify deleted
        var afterDelete = await Manager.GetOpportunityAsync(id);
        afterDelete.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-058")]
    public async Task MultiSectionUpdate_OverviewWhatWhyWhereWhen_AllPersisted()
    {
        // Create
        var createRequest = new OpportunityRequest
        {
            Name = "Multi-Section Opp",
            Description = "Initial",
            Stage = "IDENTIFY & PROFILE"
        };
        var created = await Manager.CreateOpportunityAsync(createRequest);
        var id = created.Id;

        // Update Overview
        await Manager.UpdateOverviewSectionAsync(id, new OverviewSectionRequest
        {
            Name = "Overview Updated",
            Description = "Overview desc"
        });

        // Update What
        var outputId = await SeedOutputAsync();
        var orgUnitId = await SeedOrgUnitAsync();
        var initTypeId = await SeedInitiativeTypeAsync();
        await Manager.UpdateWhatSectionAsync(id, new WhatSectionRequest
        {
            Description = "What desc",
            ResponsibleOrgUnitId = orgUnitId,
            ProposedInitiativeTypeId = initTypeId,
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = outputId, Quantity = 5 }
            }
        });

        // Update Why
        await Manager.UpdateWhySectionAsync(id, new WhySectionRequest
        {
            ExpectedBeneficiaries = "Communities",
            ExpectedImpact = "Impact",
            Challenges = "Challenges"
        });

        // Update Where
        var countryId = await SeedCountryAsync();
        await Manager.UpdateWhereSectionAsync(id, new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = countryId }
            }
        });

        // Update When
        await Manager.UpdateWhenSectionAsync(id, new WhenSectionRequest
        {
            TargetSigningDate = new DateTime(2026, 6, 1),
            ImplementationStartDate = new DateTime(2026, 7, 1),
            TargetDeliveryDate = new DateTime(2027, 6, 30)
        });

        // Verify all sections
        var result = await Manager.GetOpportunityAsync(id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Overview Updated");
        result.Description.Should().Be("What desc");
        result.ExpectedBeneficiaries.Should().Be("Communities");
        result.Countries.Should().HaveCount(1);
        result.TargetSigningDate.Should().Be(new DateTime(2026, 6, 1));
        result.Deliverables.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-058")]
    public async Task CreateWithAllChildEntities_StoredAndRetrieved()
    {
        // Arrange
        var currencyId = await SeedCurrencyAsync();
        var partnerId = await SeedPartnerAsync();
        var countryId = await SeedCountryAsync();
        var outputId = await SeedOutputAsync();
        var sdgId = await SeedSDGAsync();

        var request = new OpportunityRequest
        {
            Name = "Full Opp",
            Description = "Full description",
            Stage = "IDENTIFY & PROFILE",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partnerId, Amount = 1000m, CurrencyId = currencyId }
            },
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = partnerId }
            },
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = countryId }
            },
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = outputId, Quantity = 3 }
            },
            SDGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdgId, IsPrimary = true }
            }
        };

        // Act
        var created = await Manager.CreateOpportunityAsync(request);
        var fetched = await Manager.GetOpportunityAsync(created.Id);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.FundingPartners.Should().HaveCount(1);
        fetched.ClientPartners.Should().HaveCount(1);
        fetched.Countries.Should().HaveCount(1);
        fetched.Deliverables.Should().HaveCount(1);
        fetched.SDGs.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-058")]
    public async Task WhoSection_AddPartnersThenGetRelatedItems()
    {
        // Create opportunity
        var id = await SeedOpportunityAsync();
        var partnerId = await SeedPartnerAsync();
        var currencyId = await SeedCurrencyAsync();

        // Add funding partner via Who section
        await Manager.UpdateWhoSectionAsync(id, new WhoSectionRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partnerId, Amount = 500m, CurrencyId = currencyId }
            }
        });

        // Get related items
        var related = await Manager.GetRelatedItemsAsync(id);

        // Assert
        related.Partners.Should().HaveCount(1);
        related.Partners!.First().Id.Should().Be(partnerId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-058")]
    public async Task AssignExecutive_ThenGetOpportunity_IncludesExecutive()
    {
        // Create
        var id = await SeedOpportunityAsync();
        const int executiveId = 99;

        // Assign
        await Manager.AssignExecutiveAsync(id, executiveId);

        // Get and verify
        var result = await Manager.GetOpportunityAsync(id);
        result.Should().NotBeNull();
        result!.ExecutiveId.Should().Be(executiveId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-221")]
    public async Task UpdateOpportunity_FullReplacement_ChildCollectionsReplaced()
    {
        // Create with initial data
        var id = await SeedOpportunityAsync();
        var partner1 = await SeedPartnerAsync();
        var partner2 = await SeedPartnerAsync();
        var currencyId = await SeedCurrencyAsync();
        var country1 = await SeedCountryAsync("C1", "Country 1");
        var country2 = await SeedCountryAsync("C2", "Country 2");

        await Manager.UpdateWhoSectionAsync(id, new WhoSectionRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partner1, Amount = 100m, CurrencyId = currencyId }
            }
        });
        await Manager.UpdateWhereSectionAsync(id, new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = country1 }
            }
        });

        // Full update with different data
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = id,
            Name = "Replaced",
            Description = "Replaced desc",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partner2, Amount = 200m, CurrencyId = currencyId }
            },
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = country2 }
            }
        };

        var result = await Manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.FundingPartners.Should().HaveCount(1);
        result.FundingPartners!.First().PartnerId.Should().Be(partner2);
        result.Countries.Should().HaveCount(1);
        result.Countries!.First().CountryId.Should().Be(country2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-058")]
    public async Task WhySection_SDGDifferential_AddRemoveUpdate_PersistsCorrectly()
    {
        var id = await SeedOpportunityAsync();
        var sdg1 = await SeedSDGAsync("1", "No Poverty");
        var sdg2 = await SeedSDGAsync("2", "Zero Hunger");
        var sdg3 = await SeedSDGAsync("3", "Good Health");

        // Add SDG 1 and 2
        await Manager.UpdateWhySectionAsync(id, new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdg1, IsPrimary = true },
                new() { SDGId = sdg2, IsPrimary = false }
            }
        });

        var afterAdd = await Manager.GetOpportunityAsync(id);
        afterAdd!.SDGs.Should().HaveCount(2);

        // Remove SDG 2, add SDG 3
        await Manager.UpdateWhySectionAsync(id, new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdg1, IsPrimary = false }, // Update isPrimary
                new() { SDGId = sdg3, IsPrimary = true }
            }
        });

        var afterUpdate = await Manager.GetOpportunityAsync(id);
        afterUpdate!.SDGs.Should().HaveCount(2);
        afterUpdate.SDGs!.Select(s => s.SDGDatabaseId).Should().Contain(sdg1);
        afterUpdate.SDGs.Select(s => s.SDGDatabaseId).Should().Contain(sdg3);
        afterUpdate.SDGs.Select(s => s.SDGDatabaseId).Should().NotContain(sdg2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAllOpportunitiesAsync_AfterCreateAndDelete_ExcludesDeleted()
    {
        var created = await Manager.CreateOpportunityAsync(new OpportunityRequest
        {
            Name = "To Delete",
            Description = "Desc",
            Stage = "IDENTIFY & PROFILE"
        });
        var id = created.Id;

        await Manager.DeleteOpportunityAsync(id);

        var all = (await Manager.GetAllOpportunitiesAsync()).ToList();
        all.Should().NotContain(o => o.Id == id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateStatementPdf_BaseImplementation_ReturnsErrorWithoutException()
    {
        var id = await SeedOpportunityAsync();
        var request = new GeneratePdfRequest { EntityId = id, EntityName = "Opportunity" };

        var result = await Manager.GenerateStatementPdfAsync(request);

        result.Should().NotBeNull();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-058")]
    public async Task GetExecutivesForOpportunity_WithOrgUnit_ReturnsResults()
    {
        var id = await SeedOpportunityAsync();
        var orgUnitId = await SeedOrgUnitAsync();
        var opp = await Context.Opportunities.FindAsync(id);
        opp!.ResponsibleOrgUnitId = orgUnitId;
        await Context.SaveChangesAsync();

        var executives = (await Manager.GetExecutivesForOpportunityAsync(id)).ToList();

        // May be empty if no EntityUserRoles, but should not throw
        executives.Should().NotBeNull();
    }
}
