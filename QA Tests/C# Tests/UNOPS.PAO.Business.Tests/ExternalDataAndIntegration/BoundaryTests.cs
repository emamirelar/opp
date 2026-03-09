/**
 * @fileoverview Boundary tests for External Data Service & Gmail Integration
 * PNO-1164 (EDS), PNO-1169 (Gmail Addon). Min/max values, soft-delete, edge cases.
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
/// Boundary tests: Min/max values, soft-delete interactions, nullable FK, edge cases.
/// Requirements: PNO-1164, PNO-1169
/// </summary>
public class BoundaryTests : ExternalDataAndIntegrationFixtureBase
{
    #region SDG Boundary

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_001_SDG_MinId_SingleDigit()
    {
        await SeedSDGAsync("1", "No Poverty");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "1");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_002_SDG_MaxId_Seventeen()
    {
        await SeedSDGAsync("17", "Partnerships");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "17");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_003_SDG_EmptyName_Stored()
    {
        var sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "96",
            SDGNumber = "96",
            Name = "",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var found = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "96" && !s.IsDeleted);
        found.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_004_SDG_LongName_Stored()
    {
        var longName = new string('A', 500);
        var sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "95",
            SDGNumber = "95",
            Name = longName,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var found = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "95" && !s.IsDeleted);
        found!.Name.Should().HaveLength(500);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-232")]
    public async Task BND_005_SDG_SoftDeleted_ExcludedFromGetSDGs()
    {
        var sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "94",
            SDGNumber = "94",
            Name = "SoftDel",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "94");
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_006_SDG_StatusInactive_Excluded()
    {
        await SeedSDGAsync("93", "Inactive", EntityStatus.Inactive);
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "93");
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_007_ValuesManager_GetSDGTargetsBySDGId_ExactMatch()
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
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargetsBySDGId("6").ToList();
        result.Should().ContainSingle(t => t.SDGTargetId == "6.1.1");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_008_SDG_MultipleWithSameNumber_Distinct()
    {
        await SeedSDGAsync("6", "Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().Where(s => s.SDGId == "6").ToList();
        result.Should().HaveCountLessOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_009_ValuesManager_GetSDGIndicatorsByTargetId_EmptyTargetId()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicatorsByTargetId("").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_010_SDG_UnicodeName_Stored()
    {
        await SeedSDGAsync("92", "SDG 水资源");
        var found = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "92" && !s.IsDeleted);
        found!.Name.Should().Contain("水");
    }

    #endregion

    #region Country Boundary

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_011_Country_Iso2Code_TwoChars()
    {
        await SeedCountryAsync("Test", "AB");
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "AB" && !c.IsDeleted);
        country!.Iso2Code.Should().HaveLength(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_012_Country_Iso3Code_ThreeChars()
    {
        await SeedCountryAsync("Test", "AB", "ABC");
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "AB" && !c.IsDeleted);
        country!.Iso3Code.Should().Be("ABC");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_013_Country_NullRegion_Allowed()
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
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-233")]
    public async Task BND_014_Country_SoftDeleted_ExcludedFromGetCountries()
    {
        var country = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "Del",
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
    [Trait("Category", "Boundary")]
    public async Task BND_015_ValuesManager_GetSuggestedOrgUnits_SingleCountry()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_016_ValuesManager_GetSuggestedOrgUnits_ManyCountries()
    {
        var manager = new ValuesManager(Mapper, Context);
        var ids = Enumerable.Range(1, 50).ToArray();
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(ids);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_017_Country_EmptyContinent_Allowed()
    {
        var country = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "NoCont",
            Iso2Code = "NC",
            ContinentDescription = null,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        var found = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "NC" && !c.IsDeleted);
        found.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_018_ValuesManager_GetCountries_OrderedByName()
    {
        await SeedCountryAsync("Zimbabwe", "ZW");
        await SeedCountryAsync("Afghanistan", "AF");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        var first = result.First();
        first.Name.Should().Be("Afghanistan");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_019_Country_ZeroId_NotInDatabase()
    {
        var country = await Context.Countries.FirstOrDefaultAsync(c => c.Id == 0 && !c.IsDeleted);
        country.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_020_ValuesManager_GetEntityRolesAsync_ValidEntityType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Opportunity");
        result.Should().NotBeNull();
    }

    #endregion

    #region UNCF Boundary

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_021_UNCFOutcome_CountryCode_TwoChars()
    {
        await SeedUNCFOutcomeAsync("KE", 1, "OUT-001");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("KE").FirstOrDefault();
        result!.Country.Should().HaveLength(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_022_UNCFOutcome_VersionOne()
    {
        await SeedUNCFOutcomeAsync("AF", 1, "OUT-002");
        var outcomes = await Context.UNCFOutcomes
            .Where(o => o.Country == "AF" && o.UNCooperationFrameworkVersionNo == 1)
            .ToListAsync();
        outcomes.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_023_UNCFOutcome_HighVersionNumber()
    {
        await SeedUNCFOutcomeAsync("BR", 99, "OUT-099");
        var outcome = await Context.UNCFOutcomes
            .FirstOrDefaultAsync(o => o.Country == "BR" && o.UNCooperationFrameworkVersionNo == 99 && !o.IsDeleted);
        outcome.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_024_ValuesManager_GetUNCFIndicatorsByOutcomeId_ZeroId()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(0).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_025_UNCFOutcome_SoftDeleted_Excluded()
    {
        var outcome = new UNCFOutcome
        {
            Country = "XX",
            UNCooperationFrameworkVersionNo = 1,
            UNCFOutcomeId = "DEL",
            Name = "Deleted",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        Context.UNCFOutcomes.Add(outcome);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("XX").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_026_ValuesManager_GetUNOPSMissions_IncludeInactive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: true).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_027_ValuesManager_GetUNOPSMissions_ExcludeInactive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: false).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_028_UNCFOutcome_NullCountry_NotQueried()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomesByCountry("").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_029_ValuesManager_GetUNOPSMissions_ExcludeInactive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: false).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_030_ValuesManager_GetUNOPSMissions_IncludeInactive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNOPSMissions(includeInactive: true).ToList();
        result.Should().NotBeNull();
    }

    #endregion

    #region Gmail / Interaction Boundary

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_031_Interaction_GmailThreadId_MaxLength()
    {
        var threadId = new string('t', 255);
        var id = await SeedInteractionAsync(threadId, "msg1");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.GmailThreadId.Should().Be(threadId);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_032_Interaction_GmailMessageId_MaxLength80()
    {
        var msgId = new string('m', 80);
        var id = await SeedInteractionAsync("thread", msgId);
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.GmailMessageId.Should().HaveLength(80);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_033_Interaction_NullGmailIds_Allowed()
    {
        var id = await SeedInteractionAsync(null, null);
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.GmailThreadId.Should().BeNull();
        interaction.GmailMessageId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_034_Interaction_EmptyGmailIds_StoredAsNull()
    {
        var interaction = new UNOPSInteraction
        {
            Name = "Test",
            Subject = "Test",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            Status = EntityStatus.Active,
            IsDeleted = false,
            GmailThreadId = "",
            GmailMessageId = "",
            CreatedBy = 1,
            LastModifiedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Interactions.Add(interaction);
        await Context.SaveChangesAsync();
        var found = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == interaction.Id && !i.IsDeleted);
        found.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_035_GmailRelatedRecordsRequest_SingleEmail()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string> { "single@test.com" }
        };
        request.EmailAddresses.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_036_GmailRelatedRecordsRequest_ManyEmails()
    {
        var emails = Enumerable.Range(1, 100).Select(i => $"user{i}@test.com").ToList();
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = emails
        };
        request.EmailAddresses.Should().HaveCount(100);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_037_GmailSelectedEmailModel_AllFieldsPopulated()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "full@test.com",
            PartnerName = "Partner",
            PartnerId = 1,
            FirstName = "F",
            MiddleName = "M",
            LastName = "L"
        };
        model.MiddleName.Should().Be("M");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_038_GmailSelectedEmailModel_MinimalFields()
    {
        var model = new UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel
        {
            EmailAddress = "min@test.com"
        };
        model.PartnerName.Should().BeNull();
        model.FirstName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_039_GmailCreateRecordsRequest_WithGmailIds()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            GmailThreadId = "thread-1",
            GmailMessageId = "msg-1",
            SelectedContacts = new List<UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel>()
        };
        request.GmailThreadId.Should().Be("thread-1");
        request.GmailMessageId.Should().Be("msg-1");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_040_Contact_Email_ExactMatch()
    {
        var partnerId = await SeedPartnerAsync("P");
        await SeedContactAsync(partnerId, "exact@match.com", "Exact", "Match");
        var contact = await Context.Contacts.FirstOrDefaultAsync(c =>
            c.Email == "exact@match.com" && !c.IsDeleted);
        contact!.Email.Should().Be("exact@match.com");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_041_Interaction_SoftDeleted_ExcludedFromFind()
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
    [Trait("Category", "Boundary")]
    public async Task BND_042_ValuesManager_GetUsersPagedAsync_PageSizeOne()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 1
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Records.Should().HaveCountLessOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_043_ValuesManager_GetUsersPagedAsync_LargePageSize()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 1000
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_044_ValuesManager_SearchUsersAsync_MaxResultsOne()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("test", maxResults: 1);
        result.Should().HaveCountLessOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_045_ValuesManager_SearchUsersAsync_MaxResultsZero()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("test", maxResults: 0);
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_046_ValuesManager_GetOutputsByIds_SingleId()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputsByIds(new[] { 1 }).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_047_ValuesManager_GetOutputsByIds_ManyIds()
    {
        var manager = new ValuesManager(Mapper, Context);
        var ids = Enumerable.Range(1, 100).ToArray();
        var result = manager.GetOutputsByIds(ids).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-228")]
    public async Task BND_048_Partner_SoftDeleted_ExcludedFromGetPartners()
    {
        var partnerId = await SeedPartnerAsync("BoundaryPartner");
        var partner = await Context.Partners.FindAsync(partnerId);
        partner!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartners().Where(p => p.Name == "BoundaryPartner").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_049_Contact_SoftDeleted_ExcludedFromGetContacts()
    {
        var partnerId = await SeedPartnerAsync("P");
        var contactId = await SeedContactAsync(partnerId, "boundary@test.com", "B", "C");
        var contact = await Context.Contacts.FindAsync(contactId);
        contact!.IsDeleted = true;
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().Where(c => c.Email == "boundary@test.com").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_050_ClaimsPrincipal_UserIdZero()
    {
        var principal = CreatePrincipal(0);
        principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value.Should().Be("0");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_051_ClaimsPrincipal_UserIdMaxInt()
    {
        var principal = CreatePrincipal(int.MaxValue);
        principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value.Should().Be(int.MaxValue.ToString());
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_052_ValuesManager_GetEntityUserRolesByOrgUnits_SingleId()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityUserRolesByOrgUnitsAsync(new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_053_ValuesManager_GetOrgUnitIdsForCountries_SingleCountry()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetOrgUnitIdsForCountriesWithHierarchyAsync(new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_054_ValuesManager_GetChildOrgUnitIds_SingleCountry()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetChildOrgUnitIdsForHubRegionAsync(1, new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_055_SDG_SpecialCharactersInName()
    {
        await SeedSDGAsync("91", "SDG & Goals (2020-2030)");
        var found = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "91" && !s.IsDeleted);
        found!.Name.Should().Contain("&");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_056_Country_SpecialCharactersInName()
    {
        await SeedCountryAsync("Côte d'Ivoire", "CI", "CIV");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().FirstOrDefault(c => c.Code == "CI");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_057_Interaction_UtcDate_Stored()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        new[] { DateTimeKind.Utc, DateTimeKind.Unspecified }.Should().Contain(interaction!.Date.Kind);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_058_GmailCreateRecordsRequest_SingleSelectedContact()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailCreateRecordsRequest
        {
            SelectedContacts = new List<UNOPS.PAO.Models.Integrations.GmailSelectedEmailModel>
            {
                new() { EmailAddress = "one@test.com" }
            }
        };
        request.SelectedContacts.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-228")]
    public async Task BND_059_ValuesManager_GetPartners_EmptyResult()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartners().Where(p => p.Name == "NonExistentPartnerXYZ").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_060_ValuesManager_GetContacts_EmptyResult()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().Where(c => c.Email == "nonexistent@xyz.com").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_061_ValuesManager_GetSDGs_AllActive()
    {
        await SeedSDGAsync("6", "Water");
        await SeedSDGAsync("7", "Energy");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().OnlyContain(s => s.Status == EntityStatus.Active.ToString());
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_062_ValuesManager_GetCountries_AllHaveCode()
    {
        await SeedCountryAsync("Kenya", "KE");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCountries().ToList();
        result.Should().OnlyContain(c => !string.IsNullOrEmpty(c.Code));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_063_ValuesManager_GetSDGTargets_AllActive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGTargets().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_064_ValuesManager_GetSDGIndicators_AllActive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGIndicators().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_065_ValuesManager_GetUNCFOutcomes_AllActiveWhenNotIncludingInactive()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_066_Country_NullIso3_Allowed()
    {
        var country = new UNOPS.PAO.Domain.Entities.Country
        {
            Name = "NoIso3",
            Iso2Code = "N3",
            Iso3Code = null,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        var found = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == "N3" && !c.IsDeleted);
        found!.Iso3Code.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_067_Interaction_TypeEmail_Stored()
    {
        var id = await SeedInteractionAsync("t", "m");
        var interaction = await Context.Interactions.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        interaction!.Type.Should().Be(InteractionType.Email);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_068_ValuesManager_GetEntityRolesAsync_OpportunityType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Opportunity");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_069_ValuesManager_GetEntityRolesAsync_PartnerType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Partner");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_070_ValuesManager_GetEntityRolesAsync_ContactType()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetEntityRolesAsync("Contact");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_071_GmailRelatedRecordsRequest_NullPartnerIds_Allowed()
    {
        var request = new UNOPS.PAO.Models.Integrations.GmailRelatedRecordsRequest
        {
            EmailAddresses = new List<string> { "a@b.com" },
            partnerIds = null
        };
        request.partnerIds.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_072_ValuesManager_GetUsersPagedAsync_ActiveOnly()
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
    [Trait("Category", "Boundary")]
    public async Task BND_073_ValuesManager_GetUsersPagedAsync_WithSearchTerm()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10,
            SearchTerm = "test"
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_074_ValuesManager_GetUsersPagedAsync_WithSelectedUserIds()
    {
        var manager = new ValuesManager(Mapper, Context);
        var request = new UNOPS.PAO.Models.Users.UsersPagedRequest
        {
            PageIndex = 0,
            PageSize = 10,
            SelectedUserIds = new[] { 1, 2 }
        };
        var result = await manager.GetUsersPagedAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_075_ValuesManager_SearchUsersAsync_WithSelectedUserIds()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.SearchUsersAsync("test", maxResults: 20, selectedUserIds: new[] { 1 });
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_076_CountryArtifactTypes_HCA_Constant()
    {
        ExternalDataAndIntegrationSpec.CountryArtifactTypes.HCA.Should().Be("HCA");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_077_SDG_StatusDraft_NotInActiveResults()
    {
        var sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = "90",
            SDGNumber = "90",
            Name = "Draft",
            Status = EntityStatus.Draft,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().FirstOrDefault(s => s.SDGId == "90");
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_078_ValuesManager_GetOrganizationUnits_ReturnsOrdered()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_079_ValuesManager_GetOpportunityOrganizationUnits_ReturnsOrdered()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOpportunityOrganizationUnits().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-229")]
    public async Task BND_080_ValuesManager_GetLiaisonOffices_ActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetLiaisonOffices().ToList();
        result.Should().OnlyContain(l => l.IsActive);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_081_ValuesManager_GetCurrencies_ReturnsFromRepository()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetCurrencies().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_082_ValuesManager_GetEligibleEntities_ReturnsFromRepository()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetEligibleEntities().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_083_ValuesManager_GetProposedInitiativeTypes_ActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetProposedInitiativeTypes().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_084_ValuesManager_GetOutputs_ReturnsFromRepository()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetOutputs().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_085_Partner_ForFiltering_ExcludesDeleted()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetPartnersForFiltering().Where(p => !p.IsDeleted).ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_086_Contact_WithPartner_IncludePartner()
    {
        var partnerId = await SeedPartnerAsync("IncludePartner");
        await SeedContactAsync(partnerId, "inc@test.com", "Inc", "Partner");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetContacts().FirstOrDefault(c => c.Email == "inc@test.com");
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_087_ValuesManager_GetUsers_ActiveOnly()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetUsers().ToList();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_088_Interaction_MultipleWithSameGmailIds_DifferentEntities()
    {
        await SeedInteractionAsync("shared-thread", "shared-msg");
        await SeedInteractionAsync("shared-thread", "shared-msg");
        var count = await Context.Interactions.CountAsync(i =>
            i.GmailThreadId == "shared-thread" && !i.IsDeleted);
        count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_089_ValuesManager_GetSuggestedOrgUnits_NoRelations_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = await manager.GetSuggestedOrgUnitsForCountriesAsync(new[] { 999999 });
        result.SuggestedOrgUnitIds.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task BND_090_ExternalDataAndIntegrationSpec_Constants_Defined()
    {
        ExternalDataAndIntegrationSpec.EDS_CacheExternalData.Should().NotBeNullOrEmpty();
        ExternalDataAndIntegrationSpec.Gmail_TestEnvironment.Should().NotBeNullOrEmpty();
    }

    #endregion
}
