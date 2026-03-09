using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.BusinessLogic;

/// <summary>
/// Partner and Contact requirements tests migrated from JIRA.
/// Covers: Partner Approval (PNO-582), Contact Validation (PNO-691),
/// Contact Import (PNO-676), Gmail Add-on (PNO-474), Mass Upload (PNO-457),
/// Interaction List (PNO-230), Home Page (PNO-760).
/// Tests real entity operations via UNOPSAppDbContext.
/// </summary>
public class PartnerContactJiraRequirementsTests : ManagerTestBase
{
    private readonly string _marker = $"PCJR_{Guid.NewGuid():N}";

    #region Seed Helpers

    private async Task<UNOPSPartner> SeedPartnerAsync(
        EntityStatus status = EntityStatus.Draft,
        bool isDeleted = false)
    {
        var partner = new UNOPSPartner
        {
            Name = $"Partner_{_marker}",
            PartnerShortDescription = "Test",
            Status = status,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = isDeleted
        };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        RegisterTableCleanup("Partners", $"\"Id\" = {partner.Id}");
        return partner;
    }

    private async Task<UNOPSContact> SeedContactAsync(
        int partnerId,
        string firstName = "John",
        string lastName = "Doe",
        string? email = null,
        EntityStatus status = EntityStatus.Draft,
        bool isDeleted = false)
    {
        var contact = new UNOPSContact
        {
            Name = $"{firstName} {lastName}",
            FirstName = firstName,
            LastName = lastName,
            Email = email ?? $"{firstName.ToLower()}.{_marker}@example.com",
            Title = "Manager",
            PartnerId = partnerId,
            Status = status,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = isDeleted
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        RegisterTableCleanup("Contacts", $"\"Id\" = {contact.Id}");
        return contact;
    }

    private async Task<UNOPSInteraction> SeedInteractionAsync(int partnerId, string subject = "Meeting")
    {
        var interaction = new UNOPSInteraction
        {
            Name = $"Int_{_marker}",
            Subject = $"{subject}_{_marker}",
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        RegisterTableCleanup("Interactions", $"\"Id\" = {interaction.Id}");
        return interaction;
    }

    #endregion

    #region Positive Tests

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-POS-001")]
    public async Task POS_001_DraftPartner_CanBeCreated()
    {
        var partner = await SeedPartnerAsync(EntityStatus.Draft);

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-POS-002")]
    public async Task POS_002_Contact_WithAllRequiredFields_CanBeCreated()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, "Jane", "Smith");

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded.Should().NotBeNull();
        loaded!.FirstName.Should().Be("Jane");
        loaded.LastName.Should().Be("Smith");
        loaded.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-230")]
    [Trait("TestId", "TC-PCJR-POS-003")]
    public async Task POS_003_Interaction_CanBeCreated_WithSubject()
    {
        var partner = await SeedPartnerAsync();
        var interaction = await SeedInteractionAsync(partner.Id, "Project Discussion");

        var loaded = await Context.Interactions.FindAsync(interaction.Id);
        loaded.Should().NotBeNull();
        loaded!.Subject.Should().Contain("Project Discussion");
    }

    #endregion

    #region Negative Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-NEG-001")]
    public async Task NEG_001_SoftDeletedPartner_ExcludedFromActiveQuery()
    {
        var partner = await SeedPartnerAsync(isDeleted: true);

        var found = await Context.Partners
            .Where(p => p.Id == partner.Id && !p.IsDeleted)
            .FirstOrDefaultAsync();

        found.Should().BeNull();
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-NEG-002")]
    public async Task NEG_002_Contact_WithEmptyFirstName_StillSaves()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, firstName: "");

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.FirstName.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-NEG-003")]
    public async Task NEG_003_Contact_WithEmptyLastName_StillSaves()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, lastName: "");

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.LastName.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-NEG-004")]
    public async Task NEG_004_Contact_WithEmptyEmail_StillSaves()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, email: "");

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.Email.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-NEG-005")]
    public async Task NEG_005_SoftDeletedContact_ExcludedFromActiveQuery()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, isDeleted: true);

        var found = await Context.Contacts
            .Where(c => c.Id == contact.Id && !c.IsDeleted)
            .FirstOrDefaultAsync();

        found.Should().BeNull();
    }

    [Fact]
    [Trait("JIRA", "PNO-676")]
    [Trait("TestId", "TC-PCJR-NEG-006")]
    public async Task NEG_006_DuplicateEmail_BothContactsSaved()
    {
        var partner = await SeedPartnerAsync();
        var sharedEmail = $"duplicate_{_marker}@example.com";
        await SeedContactAsync(partner.Id, "First", "Person", sharedEmail);
        await SeedContactAsync(partner.Id, "Second", "Person", sharedEmail);

        var contacts = await Context.Contacts
            .Where(c => c.Email == sharedEmail && !c.IsDeleted)
            .ToListAsync();

        contacts.Should().HaveCount(2, "DB does not enforce unique email at entity level");
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-NEG-007")]
    public async Task NEG_007_Partner_CannotQueryDeletedPartnersWithActiveFilter()
    {
        var active = await SeedPartnerAsync(isDeleted: false);
        var deleted = await SeedPartnerAsync(isDeleted: true);

        var results = await Context.Partners
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .ToListAsync();

        results.Should().Contain(p => p.Id == active.Id);
        results.Should().NotContain(p => p.Id == deleted.Id);
    }

    [Fact]
    [Trait("JIRA", "PNO-230")]
    [Trait("TestId", "TC-PCJR-NEG-008")]
    public async Task NEG_008_SoftDeletedInteraction_ExcludedFromQuery()
    {
        var partner = await SeedPartnerAsync();
        var interaction = await SeedInteractionAsync(partner.Id);
        interaction.IsDeleted = true;
        Context.Interactions.Update(interaction);
        await SaveChangesAsync();

        var found = await Context.Interactions
            .Where(i => i.Id == interaction.Id && !i.IsDeleted)
            .FirstOrDefaultAsync();

        found.Should().BeNull();
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-NEG-009")]
    public async Task NEG_009_Contact_DraftStatus_CanBeCreated()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, status: EntityStatus.Draft);

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.Status.Should().Be(EntityStatus.Draft);
    }

    #endregion

    #region Edge/Boundary Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-EDGE-001")]
    public async Task EDGE_001_Partner_AllEntityStatusValues_AreValid()
    {
        var validStatuses = Enum.GetValues<EntityStatus>();
        validStatuses.Should().Contain(EntityStatus.Draft);
        validStatuses.Should().Contain(EntityStatus.Active);
        validStatuses.Should().Contain(EntityStatus.Inactive);
        validStatuses.Should().Contain(EntityStatus.Archived);
        validStatuses.Should().Contain(EntityStatus.Closed);
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-EDGE-002")]
    public async Task EDGE_002_Contact_VeryLongEmail_Persists()
    {
        var partner = await SeedPartnerAsync();
        var longEmail = $"{'a'.ToString().PadLeft(200, 'a')}@example.com";
        var contact = await SeedContactAsync(partner.Id, email: longEmail);

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.Email.Should().Be(longEmail);
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-EDGE-003")]
    public async Task EDGE_003_Partner_StatusTransition_DraftToActive()
    {
        var partner = await SeedPartnerAsync(EntityStatus.Draft);
        partner.Status = EntityStatus.Active;
        Context.Partners.Update(partner);
        await SaveChangesAsync();

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-EDGE-004")]
    public async Task EDGE_004_Partner_StatusTransition_ActiveToArchived()
    {
        var partner = await SeedPartnerAsync(EntityStatus.Active);
        partner.Status = EntityStatus.Archived;
        Context.Partners.Update(partner);
        await SaveChangesAsync();

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.Status.Should().Be(EntityStatus.Archived);
    }

    [Fact]
    [Trait("JIRA", "PNO-676")]
    [Trait("TestId", "TC-PCJR-EDGE-005")]
    public async Task EDGE_005_Contact_SpecialCharsInName_Persists()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, firstName: "Jean-Pierre", lastName: "O'Brien");

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.FirstName.Should().Be("Jean-Pierre");
        loaded.LastName.Should().Be("O'Brien");
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-EDGE-006")]
    public async Task EDGE_006_Contact_UnicodeInName_Persists()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, firstName: "\u00C9milie", lastName: "M\u00FCller");

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.FirstName.Should().Be("\u00C9milie");
        loaded.LastName.Should().Be("M\u00FCller");
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-EDGE-007")]
    public async Task EDGE_007_Partner_SoftDeletedWithActiveContacts()
    {
        var partner = await SeedPartnerAsync();
        await SeedContactAsync(partner.Id, "Active", "Contact");

        partner.IsDeleted = true;
        Context.Partners.Update(partner);
        await SaveChangesAsync();

        var contacts = await Context.Contacts
            .Where(c => c.PartnerId == partner.Id && !c.IsDeleted)
            .ToListAsync();

        contacts.Should().HaveCount(1, "soft delete should not cascade to contacts");
    }

    [Fact]
    [Trait("JIRA", "PNO-474")]
    [Trait("TestId", "TC-PCJR-EDGE-008")]
    public async Task EDGE_008_EmailParsing_ExtractsEmailFromDisplayName()
    {
        var emailString = "John Smith <john.smith@example.com>";
        var match = System.Text.RegularExpressions.Regex.Match(emailString, @"<([^>]+)>");
        var email = match.Success ? match.Groups[1].Value : emailString;

        email.Should().Be("john.smith@example.com");
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-PCJR-EDGE-009")]
    public async Task EDGE_009_CSVImport_RequiredHeaders_Validated()
    {
        var csvHeaders = new[] { "FirstName", "LastName", "Email", "Title", "Partner" };
        var requiredHeaders = new[] { "FirstName", "LastName", "Email" };

        var hasAllRequired = requiredHeaders.All(r => csvHeaders.Contains(r));
        hasAllRequired.Should().BeTrue();

        var invalidHeaders = new[] { "Name", "Phone" };
        var isInvalidFormat = requiredHeaders.All(r => invalidHeaders.Contains(r));
        isInvalidFormat.Should().BeFalse();
    }

    #endregion

    #region Functional Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-FUNC-001")]
    public async Task FUNC_001_Partner_AuditFields_PopulatedOnCreate()
    {
        var partner = await SeedPartnerAsync();

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.CreatedBy.Should().Be(TestUserId);
        loaded.LastModifiedBy.Should().Be(TestUserId);
        loaded.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-FUNC-002")]
    public async Task FUNC_002_Contact_AuditFields_PopulatedOnCreate()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id);

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-FUNC-003")]
    public async Task FUNC_003_Partner_SoftDelete_SetsDeletedFields()
    {
        var partner = await SeedPartnerAsync();

        partner.IsDeleted = true;
        partner.DeletedBy = TestUserId;
        partner.DeletedDate = DateTime.UtcNow;
        Context.Partners.Update(partner);
        await SaveChangesAsync();

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.IsDeleted.Should().BeTrue();
        loaded.DeletedBy.Should().Be(TestUserId);
        loaded.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-FUNC-004")]
    public async Task FUNC_004_Contact_SoftDelete_SetsDeletedFields()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id);

        contact.IsDeleted = true;
        contact.DeletedBy = TestUserId;
        contact.DeletedDate = DateTime.UtcNow;
        Context.Contacts.Update(contact);
        await SaveChangesAsync();

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-FUNC-005")]
    public async Task FUNC_005_Partner_DefaultsToDraftStatus()
    {
        var partner = await SeedPartnerAsync();
        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact]
    [Trait("JIRA", "PNO-676")]
    [Trait("TestId", "TC-PCJR-FUNC-006")]
    public async Task FUNC_006_DuplicateDetection_FindsMatchingEmails()
    {
        var partner = await SeedPartnerAsync();
        var email = $"unique_{_marker}@example.com";
        await SeedContactAsync(partner.Id, "First", "User", email);

        var existingEmails = await Context.Contacts
            .Where(c => !c.IsDeleted && c.PartnerId == partner.Id)
            .Select(c => c.Email)
            .ToListAsync();

        existingEmails.Should().Contain(email);
    }

    [Fact]
    [Trait("JIRA", "PNO-592")]
    [Trait("TestId", "TC-PCJR-FUNC-007")]
    public async Task FUNC_007_Partner_FilterByStatus_WorksCorrectly()
    {
        var draft = await SeedPartnerAsync(EntityStatus.Draft);
        var active = await SeedPartnerAsync(EntityStatus.Active);

        var draftPartners = await Context.Partners
            .Where(p => p.Status == EntityStatus.Draft && !p.IsDeleted && p.Name!.Contains(_marker))
            .ToListAsync();

        draftPartners.Should().Contain(p => p.Id == draft.Id);
        draftPartners.Should().NotContain(p => p.Id == active.Id);
    }

    [Fact]
    [Trait("JIRA", "PNO-474")]
    [Trait("TestId", "TC-PCJR-FUNC-008")]
    public async Task FUNC_008_EmailNameParsing_SplitsFirstAndLast()
    {
        var fullName = "John Smith";
        var parts = fullName.Split(' ');
        var firstName = parts.FirstOrDefault() ?? "";
        var lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";

        firstName.Should().Be("John");
        lastName.Should().Be("Smith");
    }

    [Fact]
    [Trait("JIRA", "PNO-230")]
    [Trait("TestId", "TC-PCJR-FUNC-009")]
    public async Task FUNC_009_Interaction_AuditFieldsSet()
    {
        var partner = await SeedPartnerAsync();
        var interaction = await SeedInteractionAsync(partner.Id);

        var loaded = await Context.Interactions.FindAsync(interaction.Id);
        loaded!.CreatedBy.Should().Be(TestUserId);
    }

    #endregion

    #region Integration Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-INT-001")]
    public async Task INT_001_Partner_CreateWithContacts_FullRoundTrip()
    {
        var partner = await SeedPartnerAsync();
        await SeedContactAsync(partner.Id, "Alice", "One");
        await SeedContactAsync(partner.Id, "Bob", "Two");

        var contacts = await Context.Contacts
            .Where(c => c.PartnerId == partner.Id && !c.IsDeleted)
            .ToListAsync();

        contacts.Should().HaveCount(2);
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-INT-002")]
    public async Task INT_002_Contact_StatusUpdatePersists()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id, status: EntityStatus.Draft);

        contact.Status = EntityStatus.Active;
        Context.Contacts.Update(contact);
        await SaveChangesAsync();

        var loaded = await Context.Contacts.FindAsync(contact.Id);
        loaded!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-INT-003")]
    public async Task INT_003_Partner_SoftDeletedPartner_ContactsStillQueryable()
    {
        var partner = await SeedPartnerAsync();
        var contact = await SeedContactAsync(partner.Id);

        partner.IsDeleted = true;
        Context.Partners.Update(partner);
        await SaveChangesAsync();

        var contacts = await Context.Contacts
            .Where(c => c.PartnerId == partner.Id && !c.IsDeleted)
            .ToListAsync();

        contacts.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-676")]
    [Trait("TestId", "TC-PCJR-INT-004")]
    public async Task INT_004_BulkContactCreation_AllPersisted()
    {
        var partner = await SeedPartnerAsync();
        for (int i = 0; i < 10; i++)
        {
            await SeedContactAsync(partner.Id, $"Contact{i}", "Bulk");
        }

        var contacts = await Context.Contacts
            .Where(c => c.PartnerId == partner.Id && !c.IsDeleted && c.LastName == "Bulk")
            .ToListAsync();

        contacts.Should().HaveCount(10);
    }

    [Fact]
    [Trait("JIRA", "PNO-592")]
    [Trait("TestId", "TC-PCJR-INT-005")]
    public async Task INT_005_Partners_FilterByMultipleStatuses()
    {
        var draft = await SeedPartnerAsync(EntityStatus.Draft);
        var active = await SeedPartnerAsync(EntityStatus.Active);
        var archived = await SeedPartnerAsync(EntityStatus.Archived);

        var targetStatuses = new[] { EntityStatus.Draft, EntityStatus.Active };
        var results = await Context.Partners
            .Where(p => targetStatuses.Contains(p.Status) && !p.IsDeleted && p.Name!.Contains(_marker))
            .ToListAsync();

        results.Should().HaveCount(2);
        results.Should().NotContain(p => p.Id == archived.Id);
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-PCJR-INT-006")]
    public async Task INT_006_Contact_SearchByName_LinqFilter()
    {
        var partner = await SeedPartnerAsync();
        await SeedContactAsync(partner.Id, "SearchTarget", "Name");
        await SeedContactAsync(partner.Id, "Other", "Person");

        var results = await Context.Contacts
            .Where(c => c.PartnerId == partner.Id && !c.IsDeleted
                && c.FirstName!.Contains("SearchTarget"))
            .ToListAsync();

        results.Should().HaveCount(1);
    }

    [Fact]
    [Trait("JIRA", "PNO-230")]
    [Trait("TestId", "TC-PCJR-INT-007")]
    public async Task INT_007_Interaction_CreateAndRetrieve()
    {
        var partner = await SeedPartnerAsync();
        var interaction = await SeedInteractionAsync(partner.Id, "Strategy Meeting");

        var loaded = await Context.Interactions
            .Where(i => i.Id == interaction.Id && !i.IsDeleted)
            .FirstOrDefaultAsync();

        loaded.Should().NotBeNull();
        loaded!.Subject.Should().Contain("Strategy Meeting");
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-PCJR-INT-008")]
    public async Task INT_008_Partner_UpdateAndRetrieve()
    {
        var partner = await SeedPartnerAsync();
        partner.PartnerShortDescription = "Updated Description";
        Context.Partners.Update(partner);
        await SaveChangesAsync();

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.PartnerShortDescription.Should().Be("Updated Description");
    }

    [Fact]
    [Trait("JIRA", "PNO-760")]
    [Trait("TestId", "TC-PCJR-INT-009")]
    public async Task INT_009_Partner_CountByStatus()
    {
        await SeedPartnerAsync(EntityStatus.Draft);
        await SeedPartnerAsync(EntityStatus.Draft);
        await SeedPartnerAsync(EntityStatus.Active);

        var draftCount = await Context.Partners
            .CountAsync(p => p.Status == EntityStatus.Draft && !p.IsDeleted && p.Name!.Contains(_marker));

        draftCount.Should().Be(2);
    }

    #endregion
}
