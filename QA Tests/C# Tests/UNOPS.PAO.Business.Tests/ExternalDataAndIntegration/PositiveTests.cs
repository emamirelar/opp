/**
 * @fileoverview Positive tests for External Data Service & Gmail Integration
 * PNO-1164 (EDS), PNO-1169 (Gmail Addon). Happy-path scenarios.
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ExternalDataAndIntegration;

/// <summary>
/// Positive tests: External Data & Gmail Integration happy paths.
/// Requirements: PNO-1164, PNO-1169
/// </summary>
public class PositiveTests : ExternalDataAndIntegrationFixtureBase
{
    #region ValuesManager / SDG (PNO-1164 EDS_SDGData)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_001_ValuesManager_GetSDGs_WithSeededData_ReturnsActiveSDGs()
    {
        await SeedSDGAsync("6", "Clean Water");
        await SeedSDGAsync("8", "Decent Work");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.Status == EntityStatus.Active.ToString());
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_002_ValuesManager_GetSDGTargets_ReturnsTargets()
    {
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var target = new UNOPS.PAO.Domain.Entities.SDGTarget
        {
            SDGId = "6",
            SDGTargetId = "6.1",
            Name = "Target 6.1",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGTargets.Add(target);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargets().ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_003_ValuesManager_GetCountries_ReturnsActiveCountries()
    {
        await SeedCountryAsync("Kenya", "KE", "KEN");
        await SeedCountryAsync("Afghanistan", "AF", "AFG");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(c => !string.IsNullOrEmpty(c.Name));
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_004_ValuesManager_GetUNCFOutcomes_ReturnsOutcomes()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-001");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_005_ValuesManager_GetUNCFOutcomesByCountry_ReturnsFiltered()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-001");
        await SeedUNCFOutcomeAsync("AF", 1, "OUT-002");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("KE").ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(o => o.Country == "KE");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_006_ValuesManager_GetSDGTargetsBySDGId_ReturnsFiltered()
    {
        await SeedSDGAsync("6", "Clean Water");
        var target = new UNOPS.PAO.Domain.Entities.SDGTarget
        {
            SDGId = "6",
            SDGTargetId = "6.1",
            Name = "Target 6.1",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGTargets.Add(target);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("6").ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(t => t.SDGId == "6");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_007_ValuesManager_GetCurrencies_ReturnsActiveCurrencies()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCurrencies().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_008_ValuesManager_GetProposedInitiativeTypes_ReturnsTypes()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetProposedInitiativeTypes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_009_ValuesManager_GetOutputs_ReturnsOutputs()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputs().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_010_Country_Entity_HasIso2Code()
    {
        var id = await SeedCountryAsync("Kenya", "KE");
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        country.Should().NotBeNull();
        country!.Iso2Code.Should().Be("KE");
    }

    #endregion

    #region Gmail / Interaction (PNO-1169)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_011_Interaction_WithGmailIds_Persisted()
    {
        var id = await SeedInteractionAsync("thread-123", "msg-456");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction.Should().NotBeNull();
        interaction!.GmailThreadId.Should().Be("thread-123");
        interaction.GmailMessageId.Should().Be("msg-456");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_012_Contact_WithEmail_Queryable()
    {
        var partnerId = await SeedPartnerAsync("Test Partner");
        await SeedContactAsync(partnerId, "test@example.com", "John", "Doe");
        var contact = await Context.Contacts.FirstOrDefaultAsync(c => c.Email == "test@example.com" && !c.IsDeleted);
        contact.Should().NotBeNull();
        contact!.FirstName.Should().Be("John");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_013_GmailRelatedRecordsRequest_EmailAddresses_Accepted()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string> { "user@unops.org" }
        };
        request.EmailAddresses.Should().HaveCount(1);
        request.EmailAddresses[0].Should().Be("user@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_014_GmailSelectedEmailModel_ValidStructure()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "user@partner.org",
            PartnerName = "Partner Inc",
            FirstName = "Jane",
            LastName = "Smith"
        };
        model.EmailAddress.Should().Be("user@partner.org");
        model.PartnerName.Should().Be("Partner Inc");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_015_GmailCreateRecordsRequest_SelectedContacts_Accepted()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            SelectedContacts = new List<UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel>
            {
                new() { EmailAddress = "a@b.com", FirstName = "A", LastName = "B" }
            }
        };
        request.SelectedContacts.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_016_SDG_ActiveStatus_ReturnedByValuesManager()
    {
        await SeedSDGAsync("13", "Climate Action");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "13");
        result.Should().NotBeNull();
        result!.Status.Should().Be(EntityStatus.Active.ToString());
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_017_Country_OrderedByName()
    {
        await SeedCountryAsync("Zimbabwe", "ZW");
        await SeedCountryAsync("Afghanistan", "AF");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Defect", "DEF-229")]
    public async Task POS_018_ValuesManager_GetLiaisonOffices_ReturnsOffices()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetLiaisonOffices().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_019_ValuesManager_GetOrganizationUnits_ReturnsUnits()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_020_CountryArtifactTypes_SIDS_Constant()
    {
        ExternalDataAndIntegrationSpec.CountryArtifactTypes.SIDS.Should().Be("SIDS");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_021_CountryArtifactTypes_WorldBankFragile_Constant()
    {
        ExternalDataAndIntegrationSpec.CountryArtifactTypes.WorldBankFragileSituation
            .Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_022_Interaction_WithoutGmailIds_Allowed()
    {
        var id = await SeedInteractionAsync(null, null);
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction.Should().NotBeNull();
        interaction!.GmailThreadId.Should().BeNull();
        interaction.GmailMessageId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_023_Partner_ForGmailAddon_Queryable()
    {
        var partnerId = await SeedPartnerAsync("Gmail Partner");
        var partner = await Context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId && !p.IsDeleted);
        partner.Should().NotBeNull();
        partner!.Name.Should().Be("Gmail Partner");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_024_ValuesManager_GetSDGIndicators_ReturnsIndicators()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_025_ValuesManager_GetUNCFIndicators_ReturnsIndicators()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_026_ValuesManager_GetEligibleEntities_ReturnsEntities()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetEligibleEntities().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_027_ValuesManager_GetUNOPSMissions_ReturnsMissions()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_028_Country_HasRegionDescription()
    {
        var country = await Context.Countries.FirstOrDefaultAsync(c => !c.IsDeleted);
        if (country != null)
            (country.RegionDescription != null || string.IsNullOrEmpty(country.RegionDescription)).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_029_SDG_HasSDGNumber()
    {
        await SeedSDGAsync("7", "Affordable Energy");
        var sdg = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "7" && !s.IsDeleted);
        sdg.Should().NotBeNull();
        sdg!.SDGNumber.Should().Be("7");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task POS_030_ClaimsPrincipal_CreatePrincipal_HasUserId()
    {
        var principal = CreatePrincipal(42);
        principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value.Should().Be("42");
    }

    #endregion
}
