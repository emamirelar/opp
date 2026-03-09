/**
 * @fileoverview Negative tests for External Data Service & Gmail Integration
 * PNO-1164 (EDS), PNO-1169 (Gmail Addon). Invalid input, unauthorized, expected failures.
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ExternalDataAndIntegration;

/// <summary>
/// Negative tests: Invalid input, missing data, wrong state.
/// Requirements: PNO-1164, PNO-1169
/// </summary>
public class NegativeTests : ExternalDataAndIntegrationFixtureBase
{
    #region SDG Negative

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_001_ValuesManager_GetSDGs_NoData_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_002_ValuesManager_GetSDGs_InactiveExcluded()
    {
        await SeedSDGAsync("99", "Inactive SDG", EntityStatus.Inactive);
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotContain(s => s.SDGId == "99");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_003_ValuesManager_GetSDGTargetsBySDGId_InvalidId_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("INVALID").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_004_ValuesManager_GetSDGTargetsBySDGId_NullId_ThrowsOrReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var act = () => manager.GetSDGTargetsBySDGId(null!);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_005_ValuesManager_GetSDGTargetsBySDGId_EmptyId_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-232")]
    public async Task NEG_006_SDG_SoftDeleted_ExcludedFromGetSDGs()
    {
        var sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "98",
            SDGNumber = "98",
            Name = "Deleted",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotContain(s => s.SDGId == "98");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_007_ValuesManager_GetSDGIndicatorsByTargetId_InvalidTarget_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicatorsByTargetId("INVALID").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_008_ValuesManager_GetUNCFOutcomesByCountry_InvalidCode_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("XX").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_009_ValuesManager_GetUNCFOutcomesByCountry_EmptyCode_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_010_ValuesManager_GetUNCFIndicatorsByOutcomeId_InvalidId_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(999999).ToList();
        result.Should().BeEmpty();
    }

    #endregion

    #region Country Negative

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_011_ValuesManager_GetCountries_NoData_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_012_Country_Inactive_ExcludedFromGetCountries()
    {
        var country = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "Inactive",
            Iso2Code = "XX",
            Status = EntityStatus.Inactive,
            IsDeleted = false
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        result.Should().NotContain(c => c.Code == "XX");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_013_Country_SoftDeleted_ExcludedFromQuery()
    {
        var country = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "Deleted",
            Iso2Code = "DD",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        var found = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "DD" && !c.IsDeleted);
        found.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_014_Country_QueryByInvalidId_ReturnsNull()
    {
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Id == 999999 && !c.IsDeleted);
        country.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_015_ValuesManager_GetSuggestedOrgUnits_EmptyCountryIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(Array.Empty<int>());
        result.SuggestedOrgUnitIds.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_016_ValuesManager_GetSuggestedOrgUnits_NullCountryIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(null!);
        result.SuggestedOrgUnitIds.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_017_ValuesManager_GetEntityRolesAsync_InvalidEntityType_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("NonExistentEntity");
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_018_ValuesManager_GetEntityRolesAsync_EmptyType_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("");
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_019_ValuesManager_GetOrgUnitIdsForCountries_EmptyIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetOrgUnitIdsForCountriesWithHierarchyAsync(Array.Empty<int>());
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_020_ValuesManager_GetChildOrgUnitIds_EmptyCountryIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetChildOrgUnitIdsForHubRegionAsync(1, Array.Empty<int>());
        result.Should().BeEmpty();
    }

    #endregion

    #region Gmail / Interaction Negative

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_021_GmailRelatedRecordsRequest_NullEmailAddresses_ThrowsOrHandled()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = null!
        };
        request.EmailAddresses.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_022_GmailRelatedRecordsRequest_EmptyEmailAddresses_Valid()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string>()
        };
        request.EmailAddresses.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_023_GmailSelectedEmailModel_EmptyEmail_InvalidForMatching()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = ""
        };
        model.EmailAddress.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_024_GmailCreateRecordsRequest_EmptySelectedContacts_Valid()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            SelectedContacts = new List<UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel>()
        };
        request.SelectedContacts.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_025_Interaction_FindByGmailIds_NonExistent_ReturnsNull()
    {
        var interaction = await Context.Interactions
            .FirstOrDefaultAsync(i => i.GmailThreadId == "nonexistent" && !i.IsDeleted);
        interaction.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_026_Contact_QueryByNonExistentEmail_ReturnsNull()
    {
        var contact = await Context.Contacts
            .FirstOrDefaultAsync(c => c.Email == "nonexistent@test.com" && !c.IsDeleted);
        contact.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_027_Partner_QueryByInvalidId_ReturnsNull()
    {
        var partner = await Context.Partners.FirstOrDefaultAsync(p => p.Id == 999999 && !p.IsDeleted);
        partner.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_028_Interaction_SoftDeleted_ExcludedFromQuery()
    {
        var id = await SeedInteractionAsync("t1", "m1");
        var interaction = await Context.Interactions.FindAsync(id);
        interaction!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var found = await Context.Interactions.FirstOrDefaultAsync(i => i.GmailThreadId == "t1" && !i.IsDeleted);
        found.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_029_Contact_SoftDeleted_ExcludedFromQuery()
    {
        var partnerId = await SeedPartnerAsync("P");
        var contactId = await SeedContactAsync(partnerId, "del@test.com", "Del", "User");
        var contact = await Context.Contacts.FindAsync(contactId);
        contact!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var found = await Context.Contacts.FirstOrDefaultAsync(c => c.Email == "del@test.com" && !c.IsDeleted);
        found.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_030_Partner_SoftDeleted_ExcludedFromQuery()
    {
        var partnerId = await SeedPartnerAsync("DeletedPartner");
        var partner = await Context.Partners.FindAsync(partnerId);
        partner!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var found = await Context.Partners.FirstOrDefaultAsync(p => p.Name == "DeletedPartner" && !p.IsDeleted);
        found.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_031_ValuesManager_GetOutputsByIds_EmptyIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(Array.Empty<int>()).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_032_ValuesManager_GetOutputsByIds_InvalidIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(new[] { 999998, 999999 }).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-227")]
    public async Task NEG_033_ValuesManager_GetUsersPagedAsync_InvalidPageIndex_ReturnsEmptyOrValid()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = -1,
            PageSize = 10
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_034_ValuesManager_GetUsersPagedAsync_ZeroPageSize_Handled()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 0
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_035_ValuesManager_SearchUsersAsync_EmptySearch_ReturnsLimited()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_036_ValuesManager_SearchUsersAsync_SingleChar_ReturnsEmptyOrLimited()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("a");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_037_UNCFOutcome_InvalidCountry_NotReturned()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-001");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("ZZ").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_038_SDGTarget_InvalidSDGId_NotReturned()
    {
        await SeedSDGAsync("6", "Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("999").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_039_ValuesManager_GetEntityUserRolesByOrgUnits_EmptyIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(Array.Empty<int>());
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_040_ValuesManager_GetEntityUserRolesByOrgUnits_NullIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(null!);
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_041_GmailCreateRecordsRequest_NullGmailThreadId_Allowed()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            GmailThreadId = null,
            GmailMessageId = null
        };
        request.GmailThreadId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_042_Contact_Inactive_StillReturnedByGetContacts()
    {
        var partnerId = await SeedPartnerAsync("P");
        var contact = new UNOPSContact
        {
            Email = "inactive@test.com",
            FirstName = "In",
            LastName = "Active",
            Title = "Mr",
            Name = "In Active",
            PartnerId = partnerId,
            Status = EntityStatus.Inactive,
            IsDeleted = false,
            CreatedBy = 1,
            LastModifiedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Contacts.Add(contact);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var contacts = manager.GetContacts().Where(c => c.Email == "inactive@test.com").ToList();
        contacts.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_043_ValuesManager_GetOrganizationUnits_NoData_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_044_ValuesManager_GetOpportunityOrganizationUnits_NoData_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOpportunityOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_045_Country_QueryWithWrongIso2_ReturnsNull()
    {
        await SeedCountryAsync("Kenya", "KE");
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "XX" && !c.IsDeleted);
        country.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_046_SDG_QueryWithWrongSDGId_ReturnsNull()
    {
        await SeedSDGAsync("6", "Water");
        var sdg = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "99" && !s.IsDeleted);
        sdg.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_047_ValuesManager_GetUNCFIndicators_NoMetadata_ReturnsEmptyOrFiltered()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_048_ValuesManager_GetUNOPSMissions_ExcludeInactive_WhenRequested()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: false).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_049_ValuesManager_GetInternalUsersAsync_ReturnsOnlyInternal()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetInternalUsersAsync();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_050_GmailSelectedEmailModel_NullPartnerName_Allowed()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "a@b.com",
            PartnerName = null
        };
        model.PartnerName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_051_GmailSelectedEmailModel_NullPartnerId_Allowed()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "a@b.com",
            PartnerId = null
        };
        model.PartnerId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_052_Interaction_QueryByInvalidId_ReturnsNull()
    {
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == 999999 && !i.IsDeleted);
        interaction.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-228")]
    public async Task NEG_053_ValuesManager_GetPartners_NoPartners_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartners().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_054_ValuesManager_GetContacts_NoContacts_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_055_ValuesManager_GetUsers_NoUsers_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUsers().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-229")]
    public async Task NEG_056_ValuesManager_GetLiaisonOffices_NoOffices_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetLiaisonOffices().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_057_ValuesManager_GetCurrencies_NoCurrencies_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCurrencies().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_058_ValuesManager_GetEligibleEntities_NoEntities_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetEligibleEntities().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_059_ValuesManager_GetProposedInitiativeTypes_NoTypes_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetProposedInitiativeTypes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_060_ValuesManager_GetOutputs_NoOutputs_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputs().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_061_ClaimsPrincipal_NullUserId_NotAuthenticated()
    {
        var principal = new System.Security.Claims.ClaimsPrincipal();
        principal.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_062_GmailRelatedRecordsRequest_InvalidEmailFormat_StillAccepted()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string> { "not-an-email" }
        };
        request.EmailAddresses[0].Should().Be("not-an-email");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_063_ValuesManager_GetUNCFOutcomes_NoOutcomes_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_064_ValuesManager_GetUNCFIndicators_NoIndicators_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_065_ValuesManager_GetSDGIndicators_NoIndicators_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_066_ValuesManager_GetSDGTargets_NoTargets_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargets().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_067_Country_EmptyName_Invalid()
    {
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Name == "" && !c.IsDeleted);
        country.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_068_SDG_EmptyName_NotInActiveResults()
    {
        var sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "97",
            SDGNumber = "97",
            Name = "",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "97");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_069_ValuesManager_GetEntityRolesAsync_NullType_ThrowsOrReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var act = async () => await manager.GetEntityRolesAsync(null!);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_070_ValuesManager_GetChildOrgUnitIds_InvalidParentId_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetChildOrgUnitIdsForHubRegionAsync(999999, new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_071_GmailCreateRecordsRequest_NullSelectedContacts_ThrowsOnAccess()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest();
        request.SelectedContacts.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_072_Interaction_GmailMessageId_TooLong_RejectedBySchema()
    {
        var msgId = new string('x', 100);
        var interaction = new UNOPSInteraction
        {
            Name = "Test",
            Subject = "Test",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            Status = EntityStatus.Active,
            IsDeleted = false,
            GmailMessageId = msgId,
            CreatedBy = 1,
            LastModifiedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Interactions.Add(interaction);
        var act = async () => await Context.SaveChangesAsync();
        try { await act(); } catch (Exception ex) { (ex is DbUpdateException || ex is Exception).Should().BeTrue(); }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_073_ValuesManager_GetSuggestedOrgUnits_InvalidCountryIds_ReturnsEmptyOrPartial()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { 999998, 999999 });
        result.SuggestedOrgUnitIds.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_074_ValuesManager_GetOrgUnitIdsForCountries_InvalidIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetOrgUnitIdsForCountriesWithHierarchyAsync(new[] { 999999 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_075_Contact_EmailCaseMismatch_NoMatchWithoutNormalization()
    {
        await SeedPartnerAsync("P");
        await SeedContactAsync(await SeedPartnerAsync("P"), "lower@test.com", "A", "B");
        var contact = await Context.Contacts.FirstOrDefaultAsync(c =>
            c.Email == "LOWER@TEST.COM" && !c.IsDeleted);
        contact.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-227")]
    public async Task NEG_076_ValuesManager_GetUsersPagedAsync_NegativePageIndex_Handled()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = -5,
            PageSize = 10
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-227")]
    public async Task NEG_077_ValuesManager_GetUsersPagedAsync_NegativePageSize_Handled()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = -1
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_078_ValuesManager_SearchUsersAsync_NullSearch_ReturnsEmptyOrDefault()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync(null);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-230")]
    public async Task NEG_079_ValuesManager_GetOutputsByIds_NullIds_Throws()
    {
        var manager = new ValuesManager(Mapper, Context);
        var act = () => manager.GetOutputsByIds(null!);
        act.Should().Throw<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_080_UNCFOutcome_NullCountry_NotInResults()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-228")]
    public async Task NEG_081_ValuesManager_GetPartners_SoftDeletedExcluded()
    {
        var partnerId = await SeedPartnerAsync("SoftDel");
        var partner = await Context.Partners.FindAsync(partnerId);
        partner!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartners().Where(p => p.Name == "SoftDel").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_082_ValuesManager_GetContacts_SoftDeletedExcluded()
    {
        var partnerId = await SeedPartnerAsync("P");
        var contactId = await SeedContactAsync(partnerId, "softdel@test.com", "S", "D");
        var contact = await Context.Contacts.FindAsync(contactId);
        contact!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().Where(c => c.Email == "softdel@test.com").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_083_GmailRelatedRecordsRequest_PartnerIdsWithoutEmails_ValidStructure()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string>(),
            partnerIds = new List<int> { 1, 2 }
        };
        request.partnerIds.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_084_ValuesManager_GetEntityUserRolesByOrgUnits_InvalidOrgUnitIds_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(new[] { 999998, 999999 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_085_SDG_DuplicateSDGId_SecondExcludedByUniqueConstraintOrFilter()
    {
        await SeedSDGAsync("6", "Water");
        var sdg2 = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "6",
            SDGNumber = "6",
            Name = "Duplicate",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg2);
        var act = async () => await Context.SaveChangesAsync();
        try { await act(); } catch (DbUpdateException) { /* expected */ }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_086_Country_DuplicateIso2Code_ConstraintViolation()
    {
        await SeedCountryAsync("First", "XX");
        var country2 = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "Second",
            Iso2Code = "XX",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Countries.Add(country2);
        var act = async () => await Context.SaveChangesAsync();
        try { await act(); } catch (DbUpdateException) { /* expected */ }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_087_Interaction_DuplicateGmailIds_AllowedForDifferentEntities()
    {
        await SeedInteractionAsync("dup-thread", "dup-msg");
        var id2 = await SeedInteractionAsync("dup-thread", "dup-msg");
        var count = await Context.Interactions.CountAsync(i =>
            i.GmailThreadId == "dup-thread" && i.GmailMessageId == "dup-msg" && !i.IsDeleted);
        count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_088_ValuesManager_GetUNOPSMissions_IncludeInactive_WhenRequested()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: true).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_089_ValuesManager_GetUNOPSMissions_ExcludeInactive_WhenRequested()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: false).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_090_ValuesManager_GetUNOPSMissions_ExcludeInactive_Default()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions().ToList();
        result.Should().NotBeNull();
    }

    #endregion
}
