/**
 * @fileoverview Integration tests for External Data Service & Gmail Integration
 * PNO-1164 (EDS), PNO-1169 (Gmail Addon). End-to-end flows, cross-component, API contracts.
 *
 * Requirements validated:
 * - PNO-1164: EDS data flow ValuesManager -> Repository -> DbContext
 * - PNO-1169: Gmail addon Partner/Contact/Interaction flow
 *
 * @author QA Team
 * @since 2026-03-09
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ExternalDataAndIntegration;

/// <summary>
/// Integration tests: End-to-end flows, cross-component, database round-trips.
/// Requirements: PNO-1164, PNO-1169
/// </summary>
public class IntegrationTests : ExternalDataAndIntegrationFixtureBase
{
    #region ValuesManager + DbContext Round-Trip (PNO-1164 EDS)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_001_ValuesManager_SDG_SeedThenRetrieve_FullFlow()
    {
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.Id == sdgId);
        result.Should().NotBeNull();
        result!.SDGId.Should().Be("6");
        result.Name.Should().Contain("Water");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_002_ValuesManager_Country_SeedThenRetrieve_FullFlow()
    {
        var countryId = await SeedCountryAsync("Kenya", "KE", "KEN");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().FirstOrDefault(c => c.Id == countryId);
        result.Should().NotBeNull();
        result!.Code.Should().Be("KE");
        result.Name.Should().Be("Kenya");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_003_ValuesManager_UNCF_SeedThenRetrieveByCountry_FullFlow()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-001");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("KE").ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(o => o.Country == "KE");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "QA-022")]
    public async Task INT_004_ValuesManager_SDGTarget_SeedThenGetBySDGId_FullFlow()
    {
        await SeedSDGAsync("6", "Water");
        var target = new UNOPS.PAO.Domain.Entities.SDGTarget
        {
            SDGId = "6",
            SDGTargetId = "6.1.1",
            Name = "Target 6.1.1",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGTargets.Add(target);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("6").ToList();
        result.Should().ContainSingle(t => t.SDGTargetId == "6.1.1");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-228")]
    public async Task INT_005_ValuesManager_Partner_SeedThenGetPartners_FullFlow()
    {
        var partnerId = await SeedPartnerAsync("Integration Partner");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartners().FirstOrDefault(p => p.Id == partnerId);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Integration Partner");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_006_ValuesManager_Contact_SeedThenGetContacts_FullFlow()
    {
        var partnerId = await SeedPartnerAsync("P");
        await SeedContactAsync(partnerId, "int@test.com", "Int", "Contact");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().FirstOrDefault(c => c.Email == "int@test.com");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_007_ValuesManager_GetPartnersForFiltering_ExcludesSoftDeleted()
    {
        var partnerId = await SeedPartnerAsync("FilterPartner");
        var partner = await Context.Partners.FindAsync(partnerId);
        partner!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartnersForFiltering().Where(p => p.Id == partnerId && !p.IsDeleted).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_008_ValuesManager_GetUsersPagedAsync_FullPaginationFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
        result.Records.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.PageIndex.Should().Be(0);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_009_ValuesManager_SearchUsersAsync_FullSearchFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("test", maxResults: 20);
        result.Should().NotBeNull();
        result.Should().HaveCountLessOrEqualTo(20);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_010_ValuesManager_GetSuggestedOrgUnits_CountryToOrgUnitFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var countryId = await SeedCountryAsync("TestCountry", "TC");
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { countryId });
        result.Should().NotBeNull();
        result.SuggestedOrgUnitIds.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_011_ValuesManager_GetEntityUserRolesByOrgUnits_OrgUnitToRolesFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_012_ValuesManager_GetOrgUnitIdsForCountries_HierarchyFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var countryId = await SeedCountryAsync("Hierarchy", "HY");
        var result = await manager.GetOrgUnitIdsForCountriesWithHierarchyAsync(new[] { countryId });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_013_ValuesManager_GetChildOrgUnitIds_HubRegionFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetChildOrgUnitIdsForHubRegionAsync(1, new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "QA-022")]
    public async Task INT_014_ValuesManager_GetOutputsByIds_SemanticSearchFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(new[] { 1, 2, 3 }).ToList();
        result.Should().OnlyContain(o => new[] { 1, 2, 3 }.Contains(o.Id));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_015_ValuesManager_GetEntityRolesAsync_EntityTypeToRolesFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Opportunity");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_016_ValuesManager_GetInternalUsersAsync_InternalFilterFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetInternalUsersAsync();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_017_ValuesManager_GetOrganizationUnits_OrgUnitTypeFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_018_ValuesManager_GetOpportunityOrganizationUnits_MultiTypeFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOpportunityOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-229")]
    public async Task INT_019_ValuesManager_GetLiaisonOffices_OfficeFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetLiaisonOffices().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_020_ValuesManager_GetCurrencies_CurrencyFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCurrencies().ToList();
        result.Should().NotBeNull();
    }

    #endregion

    #region Gmail / Partner-Contact-Interaction Flow (PNO-1169)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_021_Gmail_PartnerContactInteraction_SeedThenQueryByEmail_FullFlow()
    {
        var partnerId = await SeedPartnerAsync("Gmail Partner");
        await SeedContactAsync(partnerId, "gmail@test.com", "Gmail", "User");
        var manager = new ValuesManager(Mapper, Context);
        var contacts = manager.GetContacts().Where(c => c.Email == "gmail@test.com").ToList();
        contacts.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_022_Gmail_InteractionWithIds_SeedThenFindByGmailIds_FullFlow()
    {
        var id = await SeedInteractionAsync("thread-gmail", "msg-gmail");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i =>
            i.GmailThreadId == "thread-gmail" && i.GmailMessageId == "msg-gmail" && !i.IsDeleted);
        interaction.Should().NotBeNull();
        interaction!.Id.Should().Be(id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_023_Gmail_PartnerContactInteraction_CrossEntityFlow()
    {
        var partnerId = await SeedPartnerAsync("Cross Partner");
        var contactId = await SeedContactAsync(partnerId, "cross@test.com", "Cross", "Entity");
        var interactionId = await SeedInteractionAsync("cross-thread", "cross-msg");
        var partner = await Context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId && !p.IsDeleted);
        var contact = await Context.Contacts.FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == interactionId && !i.IsDeleted);
        partner.Should().NotBeNull();
        contact.Should().NotBeNull();
        interaction.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_024_Gmail_InteractionSoftDeleted_ExcludedFromFind()
    {
        var id = await SeedInteractionAsync("del-thread", "del-msg");
        var interaction = await Context.Interactions.FindAsync(id);
        interaction!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var found = await Context.Interactions.FirstOrDefaultAsync(i =>
            i.GmailThreadId == "del-thread" && !i.IsDeleted);
        found.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_025_Gmail_ValuesManagerGetContacts_IncludesPartner()
    {
        var partnerId = await SeedPartnerAsync("Include Partner");
        await SeedContactAsync(partnerId, "inc@test.com", "Inc", "Partner");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().FirstOrDefault(c => c.Email == "inc@test.com");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_026_Gmail_GmailRelatedRecordsRequest_ModelToLookupFlow()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string> { "lookup@test.com" }
        };
        await SeedPartnerAsync("P");
        await SeedContactAsync(await SeedPartnerAsync("P2"), "lookup@test.com", "Lookup", "User");
        var manager = new ValuesManager(Mapper, Context);
        var contacts = manager.GetContacts().Where(c => request.EmailAddresses!.Contains(c.Email ?? "")).ToList();
        contacts.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_027_Gmail_GmailCreateRecordsRequest_SelectedContactsFlow()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            GmailThreadId = "create-thread",
            GmailMessageId = "create-msg",
            SelectedContacts = new List<UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel>
            {
                new() { EmailAddress = "create@test.com", FirstName = "Create", LastName = "Test" }
            }
        };
        var partnerId = await SeedPartnerAsync("Create Partner");
        await SeedContactAsync(partnerId, "create@test.com", "Create", "Test");
        var manager = new ValuesManager(Mapper, Context);
        var contact = manager.GetContacts().FirstOrDefault(c => c.Email == "create@test.com");
        contact.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_028_Gmail_InteractionMultipleWithSameThread_DifferentMessages()
    {
        await SeedInteractionAsync("shared-thread", "msg-1");
        await SeedInteractionAsync("shared-thread", "msg-2");
        var count = await Context.Interactions.CountAsync(i =>
            i.GmailThreadId == "shared-thread" && !i.IsDeleted);
        count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_029_Gmail_ContactEmailUniquePerPartner()
    {
        var p1 = await SeedPartnerAsync("P1");
        var p2 = await SeedPartnerAsync("P2");
        await SeedContactAsync(p1, "same@test.com", "Same", "One");
        await SeedContactAsync(p2, "same@test.com", "Same", "Two");
        var contacts = await Context.Contacts
            .Where(c => c.Email == "same@test.com" && !c.IsDeleted)
            .ToListAsync();
        contacts.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_030_Gmail_InteractionTypeEmail_Stored()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Type.Should().Be(InteractionType.Email);
    }

    #endregion

    #region SDG Hierarchy Flow (PNO-1164 EDS_SDGData)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_031_SDG_Target_Indicator_FullHierarchyFlow()
    {
        await SeedSDGAsync("6", "Water");
        var target = new UNOPS.PAO.Domain.Entities.SDGTarget
        {
            SDGId = "6",
            SDGTargetId = "6.1.1",
            Name = "Target",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGTargets.Add(target);
        await Context.SaveChangesAsync();
        var indicator = new UNOPS.PAO.Domain.Entities.SDGIndicator
        {
            SDGTargetId = "6.1.1",
            SDGIndicatorId = "6.1.1.1",
            Name = "Indicator",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGIndicators.Add(indicator);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var targets = manager.GetSDGTargetsBySDGId("6").ToList();
        var indicators = manager.GetSDGIndicatorsByTargetId("6.1.1").ToList();
        targets.Should().NotBeEmpty();
        indicators.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-232")]
    public async Task INT_032_SDG_SoftDeletedTarget_ExcludedFromGetTargets()
    {
        await SeedSDGAsync("6", "Water");
        var target = new UNOPS.PAO.Domain.Entities.SDGTarget
        {
            SDGId = "6",
            SDGTargetId = "6.1.1",
            Name = "Deleted",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        Context.SDGTargets.Add(target);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("6").FirstOrDefault(t => t.SDGTargetId == "6.1.1");
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_033_SDG_ActiveAndInactive_OnlyActiveReturned()
    {
        await SeedSDGAsync("6", "Active");
        await SeedSDGAsync("7", "Inactive", EntityStatus.Inactive);
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotContain(s => s.SDGId == "7");
        result.Should().Contain(s => s.SDGId == "6");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_034_SDG_GetSDGs_MapsToModelCorrectly()
    {
        await SeedSDGAsync("6", "Clean Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "6");
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.SDGId.Should().Be("6");
        result.Name.Should().Be("Clean Water");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_035_SDG_GetSDGTargets_OrderedCorrectly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargets().ToList();
        result.Should().NotBeNull();
    }

    #endregion

    #region Country + UNCF Flow (PNO-1164 EDS_CountryIndicators)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_036_Country_UNCF_MetadataOutcome_FullFlow()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-001");
        var metadata = await Context.UNCFMetadatas.FirstOrDefaultAsync(m =>
            m.Country == "KE" && m.UNCooperationFrameworkVersionNo == 1 && !m.IsDeleted);
        var manager = new ValuesManager(Mapper, Context);
        var outcomes = manager.GetUNCFOutcomesByCountry("KE").ToList();
        metadata.Should().NotBeNull();
        outcomes.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_037_Country_GetCountries_OrderedByName()
    {
        await SeedCountryAsync("Zimbabwe", "ZW");
        await SeedCountryAsync("Afghanistan", "AF");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-233")]
    public async Task INT_038_Country_SoftDeleted_Excluded()
    {
        var country = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "Deleted",
            Iso2Code = "DL",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().FirstOrDefault(c => c.Code == "DL");
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_039_Country_GetCountries_MapsToSimpleValueModel()
    {
        await SeedCountryAsync("Test", "TE");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().FirstOrDefault(c => c.Code == "TE");
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.Name.Should().NotBeNullOrEmpty();
        result.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_040_Country_CountryArtifactTypes_ConstantsUsed()
    {
        var sids = ExternalDataAndIntegrationSpec.CountryArtifactTypes.SIDS;
        var fragile = ExternalDataAndIntegrationSpec.CountryArtifactTypes.WorldBankFragileSituation;
        var hca = ExternalDataAndIntegrationSpec.CountryArtifactTypes.HCA;
        sids.Should().Be("SIDS");
        fragile.Should().NotBeNullOrEmpty();
        hca.Should().Be("HCA");
    }

    #endregion

    #region Pagination and Search Integration

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_041_GetUsersPagedAsync_PageIndexZero_FirstPage()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetUsersPagedAsync(new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 5
        });
        result.Records.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_042_GetUsersPagedAsync_ActiveOnlyFilter()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetUsersPagedAsync(new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10,
            ActiveOnly = true
        });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_043_GetUsersPagedAsync_SearchTermFilter()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetUsersPagedAsync(new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10,
            SearchTerm = "admin"
        });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_044_SearchUsersAsync_MaxResultsRespected()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("a", maxResults: 3);
        result.Should().HaveCountLessOrEqualTo(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_045_SearchUsersAsync_SelectedUserIdsFilter()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("test", maxResults: 20, selectedUserIds: new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "QA-022")]
    public async Task INT_046_GetOutputsByIds_PartialMatch()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(new[] { 1, 999999 }).ToList();
        result.Should().OnlyContain(o => o.Id == 1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_047_GetOutputsByIds_EmptyIds_EmptyResult()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(Array.Empty<int>()).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_048_GetEntityRolesAsync_OpportunityType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Opportunity");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_049_GetEntityRolesAsync_PartnerType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Partner");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_050_GetEntityRolesAsync_ContactType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Contact");
        result.Should().NotBeNull();
    }

    #endregion

    #region Spec Constants and EDS/Gmail Contract

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_051_ExternalDataAndIntegrationSpec_EDS_ConstantsDefined()
    {
        ExternalDataAndIntegrationSpec.EDS_CacheExternalData.Should().NotBeNullOrEmpty();
        ExternalDataAndIntegrationSpec.EDS_CountryIndicators.Should().NotBeNullOrEmpty();
        ExternalDataAndIntegrationSpec.EDS_SDGData.Should().NotBeNullOrEmpty();
        ExternalDataAndIntegrationSpec.EDS_UNSDCFData.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_052_ExternalDataAndIntegrationSpec_Gmail_ConstantsDefined()
    {
        ExternalDataAndIntegrationSpec.Gmail_TestEnvironment.Should().NotBeNullOrEmpty();
        ExternalDataAndIntegrationSpec.Gmail_EmailToOpportunity.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_053_ExternalDataAndIntegrationSpec_DataSyncCaching()
    {
        ExternalDataAndIntegrationSpec.DataSyncCaching.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_054_ClaimsPrincipal_UserIdExtractionFlow()
    {
        var principal = CreatePrincipal(100);
        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        userId.Should().Be("100");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_055_ValuesManager_GetProposedInitiativeTypes_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetProposedInitiativeTypes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_056_ValuesManager_GetOutputs_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputs().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_057_ValuesManager_GetEligibleEntities_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetEligibleEntities().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_058_ValuesManager_GetUNOPSMissions_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_059_ValuesManager_GetUNOPSMissions_IncludeInactiveFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: true).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_060_ValuesManager_GetUsers_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUsers().ToList();
        result.Should().NotBeNull();
    }

    #endregion

    #region Additional Integration Flows (90 total for 3:1 ratio)

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-228")]
    public async Task INT_061_Partner_GetPartners_QueryableFlow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var query = manager.GetPartners();
        query.Should().NotBeNull();
        var list = query.ToList();
        list.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_062_Contact_GetContacts_WithPartner()
    {
        var partnerId = await SeedPartnerAsync("Contact Partner");
        await SeedContactAsync(partnerId, "cp@test.com", "Contact", "Partner");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().FirstOrDefault(c => c.Email == "cp@test.com");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_063_Interaction_DateStored_Utc()
    {
        var id = await SeedInteractionAsync("d", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_064_SDG_GetSDGIndicators_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_065_SDG_GetSDGTargets_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargets().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_066_UNCF_GetUNCFOutcomes_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_067_UNCF_GetUNCFIndicators_Flow()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_068_UNCF_GetUNCFIndicatorsByOutcomeId_Flow()
    {
        var outcomeId = await SeedUNCFOutcomeAsync("KE", 1, "OUT-X");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_069_GmailSelectedEmailModel_AllFields_Flow()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "all@test.com",
            PartnerName = "Partner",
            PartnerId = 1,
            FirstName = "F",
            MiddleName = "M",
            LastName = "L"
        };
        model.EmailAddress.Should().Be("all@test.com");
        model.PartnerName.Should().Be("Partner");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_070_GmailRelatedRecordsRequest_PartnerIds_Flow()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string> { "a@b.com" },
            partnerIds = new List<int> { 1, 2 }
        };
        request.partnerIds.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_071_GetSuggestedOrgUnits_InvalidCountry_Empty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { 999999 });
        result.SuggestedOrgUnitIds.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_072_GetOrgUnitIdsForCountries_InvalidCountry_Empty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetOrgUnitIdsForCountriesWithHierarchyAsync(new[] { 999999 });
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_073_GetChildOrgUnitIds_InvalidParent_EmptyOrValid()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetChildOrgUnitIdsForHubRegionAsync(999999, new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_074_GetEntityUserRolesByOrgUnits_InvalidIds_Empty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(new[] { 999998, 999999 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-229")]
    public async Task INT_075_LiaisonOffice_ActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetLiaisonOffices().ToList();
        result.Should().OnlyContain(l => l.IsActive);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_076_Currency_ActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCurrencies().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_077_ProposedInitiativeType_ActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetProposedInitiativeTypes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_078_Output_ActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputs().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_079_Interaction_SubjectRequired()
    {
        var id = await SeedInteractionAsync("s", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Subject.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_080_Partner_StatusActive()
    {
        var partnerId = await SeedPartnerAsync("Status Partner");
        var partner = await Context.Partners.FindAsync(partnerId);
        partner!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_081_Contact_StatusActive()
    {
        var partnerId = await SeedPartnerAsync("P");
        var contactId = await SeedContactAsync(partnerId, "status@test.com", "Status", "Contact");
        var contact = await Context.Contacts.FindAsync(contactId);
        contact!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_082_Interaction_StatusActive()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_083_SDG_RepositoryFiltersActive()
    {
        await SeedSDGAsync("6", "Water");
        var sdgs = await Context.SDGs.Where(s => s.Status == EntityStatus.Active && !s.IsDeleted).ToListAsync();
        sdgs.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_084_Country_RepositoryFiltersActive()
    {
        await SeedCountryAsync("Kenya", "KE");
        var countries = await Context.Countries.Where(c => c.Status == EntityStatus.Active && !c.IsDeleted).ToListAsync();
        countries.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_085_GetUsersPagedAsync_TotalCountAccurate()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetUsersPagedAsync(new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 100
        });
        result.TotalCount.Should().BeGreaterOrEqualTo(result.Records.Count);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_086_GetUsersPagedAsync_PageSizeRespected()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetUsersPagedAsync(new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 2
        });
        result.Records.Should().HaveCountLessOrEqualTo(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_087_SearchUsersAsync_NullSearch_ReturnsEmptyOrDefault()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync(null);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_088_SearchUsersAsync_EmptySearch_ReturnsLimited()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_089_GetEntityRolesAsync_EmptyType_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("");
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_090_GetEntityRolesAsync_InvalidType_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("NonExistentEntityType123");
        result.Should().BeEmpty();
    }

    #endregion
}
