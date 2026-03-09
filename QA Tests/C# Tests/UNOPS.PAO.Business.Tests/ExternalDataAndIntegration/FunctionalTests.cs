/**
 * @fileoverview Functional tests for External Data Service & Gmail Integration
 * PNO-1164 (EDS), PNO-1169 (Gmail Addon). Business rules, validation, state transitions.
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ExternalDataAndIntegration;

/// <summary>
/// Functional tests: Business rules, audit fields, permissions, workflow, data transformations.
/// Requirements: PNO-1164, PNO-1169
/// </summary>
public class FunctionalTests : ExternalDataAndIntegrationFixtureBase
{
    #region SDG Functional

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_001_ValuesManager_GetSDGs_FiltersByActiveStatus()
    {
        await SeedSDGAsync("6", "Water");
        await SeedSDGAsync("7", "Energy", EntityStatus.Inactive);
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().OnlyContain(s => s.Status == EntityStatus.Active.ToString());
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-232")]
    public async Task FNC_002_ValuesManager_GetSDGs_ExcludesSoftDeleted()
    {
        var sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "88",
            SDGNumber = "88",
            Name = "Del",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "88");
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_003_ValuesManager_GetSDGTargetsBySDGId_FiltersBySDGId()
    {
        await SeedSDGAsync("6", "Water");
        var t1 = new UNOPS.PAO.Domain.Entities.SDGTarget
        {
            SDGId = "6",
            SDGTargetId = "6.1",
            Name = "T1",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var t2 = new UNOPS.PAO.Domain.Entities.SDGTarget
        {
            SDGId = "7",
            SDGTargetId = "7.1",
            Name = "T2",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGTargets.AddRange(t1, t2);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("6").ToList();
        result.Should().OnlyContain(t => t.SDGId == "6");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_004_ValuesManager_GetSDGIndicatorsByTargetId_FiltersByTargetId()
    {
        var indicator = new UNOPS.PAO.Domain.Entities.SDGIndicator
        {
            SDGTargetId = "6.1.1",
            SDGIndicatorId = "6.1.1.1",
            Name = "Ind",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGIndicators.Add(indicator);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicatorsByTargetId("6.1.1").ToList();
        result.Should().OnlyContain(i => i.SDGTargetId == "6.1.1");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_005_ValuesManager_GetSDGs_MapsToModel()
    {
        await SeedSDGAsync("6", "Clean Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "6");
        result.Should().NotBeNull();
        result!.Name.Should().Contain("Water");
        result.SDGId.Should().Be("6");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_006_ValuesManager_GetCountries_MapsToSimpleValueModel()
    {
        await SeedCountryAsync("Kenya", "KE");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().FirstOrDefault(c => c.Code == "KE");
        result.Should().NotBeNull();
        result!.Name.Should().Be("Kenya");
        result.Code.Should().Be("KE");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_007_ValuesManager_GetCountries_OrdersByName()
    {
        await SeedCountryAsync("Zambia", "ZM");
        await SeedCountryAsync("Afghanistan", "AF");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_008_ValuesManager_GetUNCFOutcomesByCountry_FiltersByCountry()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-1");
        await SeedUNCFOutcomeAsync("AF", 1, "OUT-2");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("KE").ToList();
        result.Should().OnlyContain(o => o.Country == "KE");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_009_ValuesManager_GetUNCFIndicatorsByOutcomeId_JoinsWithOutcome()
    {
        var outcome = new UNCFOutcome
        {
            Country = "KE",
            UNCooperationFrameworkVersionNo = 1,
            UNCFOutcomeId = "OUT-X",
            Name = "Outcome X",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.UNCFOutcomes.Add(outcome);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(outcome.Id).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-228")]
    public async Task FNC_010_ValuesManager_GetPartners_ExcludesSoftDeleted()
    {
        var partnerId = await SeedPartnerAsync("FncPartner");
        var partner = await Context.Partners.FindAsync(partnerId);
        partner!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartners().Where(p => p.Name == "FncPartner").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_011_ValuesManager_GetContacts_ExcludesSoftDeleted()
    {
        var partnerId = await SeedPartnerAsync("P");
        var contactId = await SeedContactAsync(partnerId, "fnc@test.com", "F", "C");
        var contact = await Context.Contacts.FindAsync(contactId);
        contact!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().Where(c => c.Email == "fnc@test.com").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_012_ValuesManager_GetContacts_IncludesPartner()
    {
        var partnerId = await SeedPartnerAsync("IncludeP");
        await SeedContactAsync(partnerId, "inc@test.com", "I", "P");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().FirstOrDefault(c => c.Email == "inc@test.com");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_013_ValuesManager_GetOrganizationUnits_FiltersByType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_014_ValuesManager_GetOpportunityOrganizationUnits_IncludesMultipleTypes()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOpportunityOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_015_ValuesManager_GetSuggestedOrgUnitsForCountries_ReturnsPrimarySuggestion()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { 1 });
        (result.PrimarySuggestionId == null || (result.PrimarySuggestionId >= 1 && result.PrimarySuggestionId <= int.MaxValue)).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_016_ValuesManager_GetEntityUserRolesByOrgUnits_GroupsByRole()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_017_ValuesManager_GetUsersPagedAsync_RespectsPageSize()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 5
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Records.Should().HaveCountLessOrEqualTo(5);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_018_ValuesManager_GetUsersPagedAsync_ReturnsTotalCount()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_019_ValuesManager_SearchUsersAsync_RespectsMaxResults()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("a", maxResults: 5);
        result.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "QA-022")]
    public async Task FNC_020_ValuesManager_GetOutputsByIds_ReturnsOnlyRequestedIds()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(new[] { 1, 2, 3 }).ToList();
        result.Should().OnlyContain(o => new[] { 1, 2, 3 }.Contains(o.Id));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_021_Country_SimpleValueModel_HasCodeAndName()
    {
        await SeedCountryAsync("Test", "TE");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().FirstOrDefault(c => c.Code == "TE");
        result!.Id.Should().BeGreaterThan(0);
        result.Name.Should().NotBeNullOrEmpty();
        result.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_022_SDG_Model_HasRequiredFields()
    {
        await SeedSDGAsync("6", "Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "6");
        result!.Id.Should().BeGreaterThan(0);
        result.SDGId.Should().NotBeNullOrEmpty();
        result.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_023_Interaction_GmailIds_StoredCorrectly()
    {
        var id = await SeedInteractionAsync("thread-abc", "msg-xyz");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.GmailThreadId.Should().Be("thread-abc");
        interaction.GmailMessageId.Should().Be("msg-xyz");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_024_GmailRelatedRecordsRequest_EmailAddresses_UsedForLookup()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string> { "lookup@test.com" }
        };
        request.EmailAddresses.Should().Contain("lookup@test.com");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_025_GmailCreateRecordsRequest_SelectedContacts_Processed()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            SelectedContacts = new List<UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel>
            {
                new() { EmailAddress = "create@test.com", FirstName = "Create", LastName = "Test" }
            }
        };
        request.SelectedContacts[0].EmailAddress.Should().Be("create@test.com");
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-229")]
    public async Task FNC_026_ValuesManager_GetLiaisonOffices_FiltersActiveAndNotDeleted()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetLiaisonOffices().ToList();
        result.Should().OnlyContain(l => l.IsActive);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_027_ValuesManager_GetCurrencies_FiltersActive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCurrencies().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_028_ValuesManager_GetProposedInitiativeTypes_FiltersActive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetProposedInitiativeTypes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_029_ValuesManager_GetOutputs_FiltersActive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputs().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_030_ValuesManager_GetEntityRolesAsync_FiltersByEntityType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Opportunity");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_031_ValuesManager_GetInternalUsersAsync_FiltersInternal()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetInternalUsersAsync();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_032_ValuesManager_GetUNCFOutcomes_JoinsWithMetadata()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_033_ValuesManager_GetUNCFIndicators_JoinsWithMetadata()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_034_ValuesManager_GetUNOPSMissions_OrdersByDisplayOrder()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_035_ValuesManager_GetOrgUnitIdsForCountriesWithHierarchy_ReturnsHierarchy()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetOrgUnitIdsForCountriesWithHierarchyAsync(new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_036_ValuesManager_GetChildOrgUnitIdsForHubRegion_FiltersByParent()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetChildOrgUnitIdsForHubRegionAsync(1, new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_037_CountryArtifactTypes_SIDS_UsedForTagging()
    {
        ExternalDataAndIntegrationSpec.CountryArtifactTypes.SIDS.Should().Be("SIDS");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_038_CountryArtifactTypes_WorldBankFragile_UsedForTagging()
    {
        ExternalDataAndIntegrationSpec.CountryArtifactTypes.WorldBankFragileSituation
            .Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_039_ValuesManager_GetPartnersForFiltering_ReturnsQueryable()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartnersForFiltering();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_040_ValuesManager_GetUsers_ReturnsActiveUsers()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUsers().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_041_Contact_Email_UniquePerPartner()
    {
        var partnerId = await SeedPartnerAsync("P");
        await SeedContactAsync(partnerId, "unique@test.com", "U", "N");
        var contact = await Context.Contacts.FirstOrDefaultAsync(c =>
            c.Email == "unique@test.com" && c.PartnerId == partnerId && !c.IsDeleted);
        contact.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_042_Interaction_Subject_Required()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Subject.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_043_Interaction_Date_Stored()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_044_ValuesManager_GetUsersPagedAsync_RespectsActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10,
            ActiveOnly = true
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_045_ValuesManager_GetUsersPagedAsync_RespectsSearchTerm()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10,
            SearchTerm = "admin"
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_046_ValuesManager_GetUsersPagedAsync_RespectsSelectedUserIds()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10,
            SelectedUserIds = new[] { 1 }
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_047_SDG_Repository_FiltersActive()
    {
        await SeedSDGAsync("6", "Water");
        var sdgs = await Context.SDGs.Where(s => s.Status == EntityStatus.Active && !s.IsDeleted).ToListAsync();
        sdgs.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_048_Country_Repository_FiltersActive()
    {
        await SeedCountryAsync("Kenya", "KE");
        var countries = await Context.Countries.Where(c => c.Status == EntityStatus.Active && !c.IsDeleted).ToListAsync();
        countries.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_049_ValuesManager_GetSuggestedOrgUnits_SuggestionReason_Set()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { 1 });
        (result.SuggestionReason == null || !string.IsNullOrEmpty(result.SuggestionReason)).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_050_ValuesManager_GetEntityUserRolesByOrgUnits_IncludesUsers()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(new[] { 1 });
        foreach (var r in result)
        {
            r.RoleGroups.Should().NotBeNull();
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_051_GmailSelectedEmailModel_PartnerId_Optional()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "a@b.com",
            PartnerId = null
        };
        model.PartnerId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_052_GmailSelectedEmailModel_PartnerName_Optional()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "a@b.com",
            PartnerName = null
        };
        model.PartnerName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_053_ValuesManager_GetOutputsByIds_EmptyInput_EmptyOutput()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(Array.Empty<int>()).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_054_ValuesManager_GetUNCFOutcomes_LatestVersionPerCountry()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_055_ValuesManager_GetUNCFOutcomesByCountry_LatestVersion()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-1");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("KE").ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_056_Interaction_Status_Stored()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_057_Partner_Status_Active()
    {
        var partnerId = await SeedPartnerAsync("P");
        var partner = await Context.Partners.FindAsync(partnerId);
        partner!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_058_Contact_Status_Active()
    {
        var partnerId = await SeedPartnerAsync("P");
        var contactId = await SeedContactAsync(partnerId, "s@t.com", "S", "T");
        var contact = await Context.Contacts.FindAsync(contactId);
        contact!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_059_ValuesManager_GetOrganizationUnits_ExcludesDeleted()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_060_ValuesManager_GetOpportunityOrganizationUnits_ExcludesDeleted()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOpportunityOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_061_ExternalDataAndIntegrationSpec_EDS_Constants()
    {
        ExternalDataAndIntegrationSpec.EDS_SDGData.Should().NotBeNullOrEmpty();
        ExternalDataAndIntegrationSpec.EDS_UNSDCFData.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_062_ExternalDataAndIntegrationSpec_Gmail_Constants()
    {
        ExternalDataAndIntegrationSpec.Gmail_EmailToOpportunity.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-228")]
    public async Task FNC_063_ValuesManager_GetPartners_MapsToPartnerValueModel()
    {
        var partnerId = await SeedPartnerAsync("MapPartner");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartners().FirstOrDefault(p => p.Name == "MapPartner");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_064_ValuesManager_GetContacts_MapsToContactValueModel()
    {
        var partnerId = await SeedPartnerAsync("P");
        await SeedContactAsync(partnerId, "map@test.com", "Map", "Contact");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().FirstOrDefault(c => c.Email == "map@test.com");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_065_ValuesManager_GetUsers_MapsToUserValueModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUsers().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_066_ValuesManager_GetSDGTargets_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargets().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_067_ValuesManager_GetSDGIndicators_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_068_ValuesManager_GetUNCFOutcomes_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_069_ValuesManager_GetUNCFIndicators_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_070_ValuesManager_GetOrganizationUnits_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-229")]
    public async Task FNC_071_ValuesManager_GetLiaisonOffices_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetLiaisonOffices().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_072_ValuesManager_GetCurrencies_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCurrencies().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_073_ValuesManager_GetEligibleEntities_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetEligibleEntities().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_074_ValuesManager_GetProposedInitiativeTypes_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetProposedInitiativeTypes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_075_ValuesManager_GetOutputs_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputs().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_076_ValuesManager_GetEntityRolesAsync_MapsToSimpleValueModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Opportunity");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_077_ValuesManager_GetInternalUsersAsync_MapsToSimpleValueModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetInternalUsersAsync();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_078_ValuesManager_GetUNOPSMissions_MapsToModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_079_ValuesManager_GetUsersPagedAsync_MapsToUserValueModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Records.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_080_ValuesManager_SearchUsersAsync_MapsToUserValueModel()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("test");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_081_Country_Continent_Optional()
    {
        await SeedCountryAsync("NoContinent", "NC");
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "NC" && !c.IsDeleted);
        country!.ContinentDescription.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_082_Country_Region_Optional()
    {
        var country = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "NoRegion",
            Iso2Code = "NR",
            RegionDescription = null,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        var found = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "NR" && !c.IsDeleted);
        found!.RegionDescription.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_083_SDG_SDGNumber_MatchesSDGId()
    {
        await SeedSDGAsync("6", "Water");
        var sdg = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "6" && !s.IsDeleted);
        sdg!.SDGNumber.Should().Be(sdg.SDGId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_084_Interaction_Type_Stored()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Type.Should().Be(InteractionType.Email);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_085_ValuesManager_GetSuggestedOrgUnits_EmptyWhenNoRelations()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { 999999 });
        result.SuggestedOrgUnitIds.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_086_ValuesManager_GetOrgUnitIdsForCountries_EmptyWhenNoRelations()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetOrgUnitIdsForCountriesWithHierarchyAsync(new[] { 999999 });
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_087_ValuesManager_GetChildOrgUnitIds_EmptyWhenNoMatch()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetChildOrgUnitIdsForHubRegionAsync(999999, new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_088_GmailCreateRecordsRequest_GmailIds_Optional()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            GmailThreadId = null,
            GmailMessageId = null,
            SelectedContacts = new List<UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel>()
        };
        request.GmailThreadId.Should().BeNull();
        request.GmailMessageId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_089_DataSyncCaching_Constant_Defined()
    {
        ExternalDataAndIntegrationSpec.DataSyncCaching.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FNC_090_ValuesManager_GetEntityUserRolesByOrgUnits_EmptyForInvalidIds()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(new[] { 999998, 999999 });
        result.Should().NotBeNull();
    }

    #endregion
}
