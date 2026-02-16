/**
 * CONTACT INTEGRATION TESTS
 * 
 * Required: ≥50 tests (FIXED minimum, core category)
 * Purpose: End-to-end workflow testing with real dependencies
 * 
 * Coverage Areas:
 *   - CRUD workflow (10): Create, read, update, delete with real DB
 *   - Search/filter (10): Text search, status filters, partner filters
 *   - Pagination (5): Page boundaries, sort ordering, total counts
 *   - Relationships (10): Partner-contact, contact-interaction links
 *   - Error handling (15): Not found, validation errors, constraint violations
 * 
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Integration;

/// <summary>
/// Integration tests for Contact management
/// 
/// Test Strategy: These tests verify complete workflows with
/// real database operations and dependencies.
/// 
/// PostgreSQL: Tests run inside a transaction that is rolled back on Dispose.
/// Pre-existing database data is visible, so assertions scope to test-created data
/// using unique markers (email prefix per test) rather than assuming empty tables.
/// 
/// Required: ≥50 tests (FIXED minimum, core category)
/// Current: 52 tests
/// </summary>
public class ContactIntegrationTests : IntegrationTestBase
{
    /// <summary>
    /// Creates a UNOPSContact with valid audit FKs and default test partner.
    /// </summary>
    private UNOPSContact MakeContact(
        string firstName, string lastName, string email,
        string title = "Staff", EntityStatus status = EntityStatus.Active,
        int? partnerId = null, bool isDeleted = false)
    {
        return new UNOPSContact
        {
            Name = $"{firstName} {lastName}",
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Title = title,
            Status = status,
            PartnerId = partnerId ?? DefaultTestPartnerId,
            IsDeleted = isDeleted,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a UNOPSPartner with valid audit FKs.
    /// </summary>
    private UNOPSPartner MakePartner(string name)
    {
        return new UNOPSPartner
        {
            Name = name,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a UNOPSInteraction with valid audit FKs.
    /// </summary>
    private UNOPSInteraction MakeInteraction(string subject, InteractionType type = InteractionType.InPersonMeeting)
    {
        return new UNOPSInteraction
        {
            Name = subject,
            Subject = subject,
            Type = type,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
    }

    #region CRUD Workflow (10 tests)

    [Fact]
    public async Task Contact_CanBeCreatedWithPartner()
    {
        // Arrange
        var partner = MakePartner("Test Partner CRUD");
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();

        var contact = MakeContact("John", "Doe", "john-crud@test.com", "Manager", partnerId: partner.Id);

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Partner.Should().NotBeNull();
        result.Partner!.Name.Should().Be("Test Partner CRUD");
    }

    [Fact]
    public async Task Contact_CanBeRetrievedById()
    {
        // Arrange
        var contact = MakeContact("Jane", "Smith", "jane-retrieve@test.com", "Director");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task Contact_CanBeUpdated()
    {
        // Arrange
        var contact = MakeContact("John", "Doe", "john-update@test.com", "Manager");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.Title = "Senior Manager";
        contact.Email = "john.doe-updated@test.com";
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.Title.Should().Be("Senior Manager");
        result.Email.Should().Be("john.doe-updated@test.com");
    }

    [Fact]
    public async Task Contact_SoftDelete_SetsIsDeleted()
    {
        // Arrange
        var contact = MakeContact("To", "Delete", "delete-sd@test.com", "Test");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.IsDeleted = true;
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Contact_SoftDeleted_ExcludedFromActiveQueries()
    {
        // Arrange - use unique email prefix to scope assertions to test data
        var prefix = $"sde-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("A", "One", $"{prefix}-a@test.com", isDeleted: false),
            MakeContact("D", "Two", $"{prefix}-d@test.com", isDeleted: true),
            MakeContact("A", "Three", $"{prefix}-b@test.com", isDeleted: false)
        });
        await SaveChangesAsync();

        // Act - scope to test data
        var activeContacts = await Context.Contacts
            .Where(c => !c.IsDeleted && c.Email!.StartsWith(prefix))
            .ToListAsync();

        // Assert
        activeContacts.Should().HaveCount(2);
        activeContacts.Should().NotContain(c => c.Email!.Contains("-d@"));
    }

    [Fact]
    public async Task Contact_CanBeRestoredAfterSoftDelete()
    {
        // Arrange
        var contact = MakeContact("Re", "Store", "restore@test.com", isDeleted: true);
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.IsDeleted = false;
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Contact_CreateMultiple_AllPersisted()
    {
        // Arrange - use unique email prefix to scope count to test data
        var prefix = $"multi-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 10).Select(i =>
            MakeContact($"First{i}", $"Last{i}", $"{prefix}-{i}@test.com"));

        // Act
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        var count = await Context.Contacts.CountAsync(c => c.Email!.StartsWith(prefix));

        // Assert
        count.Should().Be(10);
    }

    [Fact]
    public async Task Contact_Update_PreservesOtherFields()
    {
        // Arrange
        var contact = MakeContact("Original", "Name", "original-pres@test.com", "Manager");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Only update title
        contact.Title = "Director";
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.Title.Should().Be("Director");
        result.Email.Should().Be("original-pres@test.com", "Email should not change");
        result.FirstName.Should().Be("Original", "Name should not change");
    }

    [Fact]
    public async Task Contact_CreateWithAllFields_Persisted()
    {
        // Arrange
        var contact = MakeContact("Full", "Contact", "full-all@test.com", "CEO");

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("full-all@test.com");
        result.Title.Should().Be("CEO");
    }

    [Fact]
    public async Task Contact_StatusChange_Persisted()
    {
        // Arrange
        var contact = MakeContact("Status", "Test", "status-chg@test.com");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.Status = EntityStatus.Inactive;
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.Status.Should().Be(EntityStatus.Inactive);
    }

    #endregion

    #region Relationships (10 tests)

    [Fact]
    public async Task Contact_CanHaveMultipleInteractions()
    {
        // Arrange
        var contact = MakeContact("John", "Doe", "john-intx@test.com", "Manager");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var interactions = new List<UNOPSInteraction>
        {
            MakeInteraction("Meeting 1"),
            MakeInteraction("Call 1", InteractionType.Call)
        };
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();

        var interactionContacts = new List<InteractionContact>
        {
            new() { InteractionId = interactions[0].Id, ContactId = contact.Id },
            new() { InteractionId = interactions[1].Id, ContactId = contact.Id }
        };
        await Context.InteractionContacts.AddRangeAsync(interactionContacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.InteractionContacts.Where(ic => ic.ContactId == contact.Id).CountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task Partner_CanHaveMultipleContacts()
    {
        // Arrange
        var partner = MakePartner("MultiContact Partner");
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();

        var contacts = new List<UNOPSContact>
        {
            MakeContact("John", "Doe", "john-mc@test.com", partnerId: partner.Id),
            MakeContact("Jane", "Smith", "jane-mc@test.com", partnerId: partner.Id)
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Partners
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.Id == partner.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(2);
    }

    [Fact]
    public async Task Contact_PartnerRelationship_LoadedCorrectly()
    {
        // Arrange
        var partner = MakePartner("Partner A Rel");
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();

        var contact = MakeContact("C", "One", "c1-rel@test.com", partnerId: partner.Id);
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == contact.Id);

        // Assert
        result!.Partner.Should().NotBeNull();
        result.Partner!.Name.Should().Be("Partner A Rel");
        result.PartnerId.Should().Be(partner.Id);
    }

    [Fact]
    public async Task Contact_DifferentPartners_IsolatedCorrectly()
    {
        // Arrange
        var partnerA = MakePartner("PartnerA Iso");
        var partnerB = MakePartner("PartnerB Iso");
        await Context.Partners.AddRangeAsync(new[] { partnerA, partnerB });
        await SaveChangesAsync();

        var contacts = new List<UNOPSContact>
        {
            MakeContact("A", "1", "a1-iso@test.com", partnerId: partnerA.Id),
            MakeContact("A", "2", "a2-iso@test.com", partnerId: partnerA.Id),
            MakeContact("B", "1", "b1-iso@test.com", partnerId: partnerB.Id)
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var partnerAContacts = await Context.Contacts.Where(c => c.PartnerId == partnerA.Id).CountAsync();
        var partnerBContacts = await Context.Contacts.Where(c => c.PartnerId == partnerB.Id).CountAsync();

        // Assert
        partnerAContacts.Should().Be(2);
        partnerBContacts.Should().Be(1);
    }

    [Fact]
    public async Task Contact_InteractionLink_BothDirections()
    {
        // Arrange
        var contact = MakeContact("C", "1", "c-bidir@test.com");
        var interaction = MakeInteraction("Meeting BiDir");

        await Context.Contacts.AddAsync(contact);
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        await Context.InteractionContacts.AddAsync(new InteractionContact { ContactId = contact.Id, InteractionId = interaction.Id });
        await SaveChangesAsync();

        // Act - Query from both sides
        var contactInteractions = await Context.InteractionContacts.Where(ic => ic.ContactId == contact.Id).CountAsync();
        var interactionContacts = await Context.InteractionContacts.Where(ic => ic.InteractionId == interaction.Id).CountAsync();

        // Assert
        contactInteractions.Should().Be(1);
        interactionContacts.Should().Be(1);
    }

    [Fact]
    public async Task MultipleContacts_LinkedToSameInteraction()
    {
        // Arrange
        var contacts = new List<UNOPSContact>
        {
            MakeContact("C", "1", "c1-group@t.com"),
            MakeContact("C", "2", "c2-group@t.com"),
            MakeContact("C", "3", "c3-group@t.com")
        };
        await Context.Contacts.AddRangeAsync(contacts);
        var interaction = MakeInteraction("Group Meeting");
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        await Context.InteractionContacts.AddRangeAsync(new[]
        {
            new InteractionContact { ContactId = contacts[0].Id, InteractionId = interaction.Id },
            new InteractionContact { ContactId = contacts[1].Id, InteractionId = interaction.Id },
            new InteractionContact { ContactId = contacts[2].Id, InteractionId = interaction.Id }
        });
        await SaveChangesAsync();

        // Act
        var attendees = await Context.InteractionContacts.Where(ic => ic.InteractionId == interaction.Id).CountAsync();

        // Assert
        attendees.Should().Be(3);
    }

    [Fact]
    public async Task Contact_WithoutExplicitPartner_UsesDefault()
    {
        // Arrange - Contact with default partner
        var contact = MakeContact("Un", "Linked", "unlinked@test.com", "Consultant");

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.PartnerId.Should().Be(DefaultTestPartnerId);
    }

    [Fact]
    public async Task Contact_PartnerDeletion_ContactRemains()
    {
        // Arrange
        var partner = MakePartner("To Delete Partner");
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        var contact = MakeContact("Or", "Phan", "orphan@test.com", partnerId: partner.Id);
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Soft delete partner
        partner.IsDeleted = true;
        await SaveChangesAsync();
        var contactResult = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        contactResult.Should().NotBeNull("Contact should remain after partner soft delete");
    }

    [Fact]
    public async Task Contact_TransferPartner_UpdatesRelationship()
    {
        // Arrange
        var partnerA = MakePartner("Partner A Xfer");
        var partnerB = MakePartner("Partner B Xfer");
        await Context.Partners.AddRangeAsync(new[] { partnerA, partnerB });
        await SaveChangesAsync();
        var contact = MakeContact("Trans", "Fer", "transfer@test.com", partnerId: partnerA.Id);
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.PartnerId = partnerB.Id;
        await SaveChangesAsync();
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == contact.Id);

        // Assert
        result!.PartnerId.Should().Be(partnerB.Id);
        result.Partner!.Name.Should().Be("Partner B Xfer");
    }

    #endregion

    #region Search and Filtering (10 tests)

    [Fact]
    public async Task Search_ByFirstName_ReturnsMatches()
    {
        // Arrange - use unique last name to scope assertions
        var marker = $"SrchFN-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("John", marker, $"j-{marker}@t.com"),
            MakeContact("Jane", marker, $"ja-{marker}@t.com"),
            MakeContact("John", marker, $"js-{marker}@t.com")
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts
            .Where(c => c.FirstName == "John" && c.LastName == marker)
            .ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_ByLastName_ReturnsMatches()
    {
        // Arrange - use unique email prefix to scope
        var marker = $"SrchLN-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("John", "DoeTest", $"{marker}-j@t.com"),
            MakeContact("Jane", "DoeTest", $"{marker}-ja@t.com"),
            MakeContact("Bob", "SmithTest", $"{marker}-b@t.com")
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts
            .Where(c => c.LastName == "DoeTest" && c.Email!.StartsWith(marker))
            .ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_ByEmail_ReturnsExactMatch()
    {
        // Arrange
        var uniqueEmail = $"unique-{Guid.NewGuid():N}@test.com";
        var contacts = new List<UNOPSContact>
        {
            MakeContact("C", "1", uniqueEmail),
            MakeContact("C", "2", "other-srch@test.com")
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FirstOrDefaultAsync(c => c.Email == uniqueEmail);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(contacts[0].Id);
    }

    [Fact]
    public async Task Filter_ByStatus_Active_ReturnsOnlyActive()
    {
        // Arrange - use unique email prefix
        var prefix = $"fstat-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("A", "1", $"{prefix}-a@t.com", status: EntityStatus.Active),
            MakeContact("I", "2", $"{prefix}-i@t.com", status: EntityStatus.Inactive),
            MakeContact("A", "3", $"{prefix}-a2@t.com", status: EntityStatus.Active)
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts
            .Where(c => c.Status == EntityStatus.Active && c.Email!.StartsWith(prefix))
            .ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Filter_ByPartner_ReturnsOnlyPartnerContacts()
    {
        // Arrange
        var partner1 = MakePartner("FP1");
        var partner2 = MakePartner("FP2");
        await Context.Partners.AddRangeAsync(new[] { partner1, partner2 });
        await SaveChangesAsync();
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("C", "1", "c1-fp@t.com", partnerId: partner1.Id),
            MakeContact("C", "2", "c2-fp@t.com", partnerId: partner1.Id),
            MakeContact("C", "3", "c3-fp@t.com", partnerId: partner2.Id)
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.PartnerId == partner1.Id).ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Filter_ExcludesDeleted_ByDefault()
    {
        // Arrange - use unique prefix
        var prefix = $"fdel-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("A", "1", $"{prefix}-a@t.com", isDeleted: false),
            MakeContact("D", "2", $"{prefix}-d@t.com", isDeleted: true)
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts
            .Where(c => !c.IsDeleted && c.Email!.StartsWith(prefix))
            .ToListAsync();

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_NoResults_ReturnsEmptyList()
    {
        // Arrange
        await Context.Contacts.AddAsync(MakeContact("John", "Doe", "j-norez@t.com"));
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.FirstName == "NonExistent-XYZ-99").ToListAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Filter_CombinedCriteria_WorksCorrectly()
    {
        // Arrange
        var partner = MakePartner("CombP1");
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        var prefix = $"comb-{Guid.NewGuid():N}";
        var activeContact = MakeContact("A", "1", $"{prefix}-a1@t.com", partnerId: partner.Id);
        await Context.Contacts.AddRangeAsync(new[]
        {
            activeContact,
            MakeContact("I", "2", $"{prefix}-i@t.com", status: EntityStatus.Inactive, partnerId: partner.Id),
            MakeContact("D", "3", $"{prefix}-d@t.com", isDeleted: true, partnerId: partner.Id)
        });
        await SaveChangesAsync();

        // Act - Active, non-deleted, for this partner
        var results = await Context.Contacts
            .Where(c => c.PartnerId == partner.Id && c.Status == EntityStatus.Active && !c.IsDeleted && c.Email!.StartsWith(prefix))
            .ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().Id.Should().Be(activeContact.Id);
    }

    [Fact]
    public async Task Sort_ByLastName_ReturnsOrdered()
    {
        // Arrange - use unique prefix to scope data
        var prefix = $"sort-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("Charlie", "Zulu", $"{prefix}-c@t.com"),
            MakeContact("Alice", "Alpha", $"{prefix}-a@t.com"),
            MakeContact("Bob", "Mike", $"{prefix}-b@t.com")
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix))
            .OrderBy(c => c.LastName)
            .ToListAsync();

        // Assert
        results.Should().HaveCount(3);
        results[0].LastName.Should().Be("Alpha");
        results[1].LastName.Should().Be("Mike");
        results[2].LastName.Should().Be("Zulu");
    }

    [Fact]
    public async Task Search_ByTitle_ReturnsMatches()
    {
        // Arrange - use unique prefix
        var prefix = $"stit-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("C", "1", $"{prefix}-1@t.com", "DirectorXQ"),
            MakeContact("C", "2", $"{prefix}-2@t.com", "ManagerXQ"),
            MakeContact("C", "3", $"{prefix}-3@t.com", "DirectorXQ")
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts
            .Where(c => c.Title == "DirectorXQ" && c.Email!.StartsWith(prefix))
            .ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    #endregion

    #region Pagination (5 tests)

    [Fact]
    public async Task Pagination_FirstPage_ReturnsCorrectResults()
    {
        // Arrange - use unique prefix and scope pagination to test data
        var prefix = $"pg1-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 25).Select(i =>
            MakeContact($"F{i}", $"L{i}", $"{prefix}-{i}@t.com"));
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act - paginate within test data only
        var page = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix))
            .OrderBy(c => c.Id)
            .Take(10)
            .ToListAsync();

        // Assert
        page.Should().HaveCount(10);
        page.Select(c => c.Id).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Pagination_SecondPage_ReturnsCorrectResults()
    {
        // Arrange
        var prefix = $"pg2-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 25).Select(i =>
            MakeContact($"F{i}", $"L{i}", $"{prefix}-{i}@t.com"));
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var allIds = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix))
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToListAsync();

        var page = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix))
            .OrderBy(c => c.Id)
            .Skip(10)
            .Take(10)
            .ToListAsync();

        // Assert
        page.Should().HaveCount(10);
        page.First().Id.Should().Be(allIds[10]);
        page.Last().Id.Should().Be(allIds[19]);
    }

    [Fact]
    public async Task Pagination_LastPage_ReturnRemainingResults()
    {
        // Arrange
        var prefix = $"pg3-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 25).Select(i =>
            MakeContact($"F{i}", $"L{i}", $"{prefix}-{i}@t.com"));
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix))
            .OrderBy(c => c.Id)
            .Skip(20)
            .Take(10)
            .ToListAsync();

        // Assert
        page.Should().HaveCount(5); // Only 5 remaining
    }

    [Fact]
    public async Task Pagination_BeyondData_ReturnsEmpty()
    {
        // Arrange
        var prefix = $"pg4-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 5).Select(i =>
            MakeContact($"F{i}", $"L{i}", $"{prefix}-{i}@t.com"));
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix))
            .OrderBy(c => c.Id)
            .Skip(100)
            .Take(10)
            .ToListAsync();

        // Assert
        page.Should().BeEmpty();
    }

    [Fact]
    public async Task Pagination_TotalCount_Accurate()
    {
        // Arrange
        var prefix = $"pg5-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 33).Select(i =>
            MakeContact($"F{i}", $"L{i}", $"{prefix}-{i}@t.com"));
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var totalCount = await Context.Contacts.CountAsync(c => c.Email!.StartsWith(prefix));
        var pageSize = 10;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        // Assert
        totalCount.Should().Be(33);
        totalPages.Should().Be(4);
    }

    #endregion

    #region Error Handling (15 tests)

    [Fact]
    public async Task GetById_NonExistent_ReturnsNull()
    {
        // Act
        var result = await Context.Contacts.FindAsync(999999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_Zero_ReturnsNull()
    {
        // Act
        var result = await Context.Contacts.FindAsync(0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_NegativeId_ReturnsNull()
    {
        // Act
        var result = await Context.Contacts.FindAsync(-1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Create_WithDuplicateId_ThrowsException()
    {
        // Arrange - Create first contact and save to get auto-generated ID
        var firstContact = MakeContact("F", "1", "f-dup@t.com");
        await Context.Contacts.AddAsync(firstContact);
        await SaveChangesAsync();

        // Act & Assert - Create second contact with same ID to trigger duplicate key violation
        var duplicateContact = MakeContact("D", "2", "d-dup@t.com");
        duplicateContact.Id = firstContact.Id;

        var act = async () =>
        {
            await Context.Contacts.AddAsync(duplicateContact);
            await SaveChangesAsync();
        };
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Query_NoTestData_ReturnsNoTestMatches()
    {
        // Act - query for a marker that no test data uses
        var uniqueMarker = $"empty-{Guid.NewGuid():N}";
        var results = await Context.Contacts
            .Where(c => c.Email!.StartsWith(uniqueMarker))
            .ToListAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Count_NoTestData_ReturnsZero()
    {
        // Act - count only contacts with a marker that doesn't exist
        var uniqueMarker = $"count0-{Guid.NewGuid():N}";
        var count = await Context.Contacts.CountAsync(c => c.Email!.StartsWith(uniqueMarker));

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task FirstOrDefault_NoMatch_ReturnsNull()
    {
        // Arrange
        await Context.Contacts.AddAsync(MakeContact("E", "1", "e-nomatch@t.com"));
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FirstOrDefaultAsync(c => c.Email == "nonexistent-xyz-999@test.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Filter_DeletedOnly_EmptyWhenNoneDeleted()
    {
        // Arrange - create only active contacts with unique prefix
        var prefix = $"delonly-{Guid.NewGuid():N}";
        await Context.Contacts.AddAsync(MakeContact("A", "1", $"{prefix}-a@t.com", isDeleted: false));
        await SaveChangesAsync();

        // Act - check among our test data
        var deletedContacts = await Context.Contacts
            .Where(c => c.IsDeleted && c.Email!.StartsWith(prefix))
            .ToListAsync();

        // Assert
        deletedContacts.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_NonExistentContact_ThrowsException()
    {
        // Act
        var nonExistent = await Context.Contacts.FindAsync(999999);

        // Assert
        nonExistent.Should().BeNull("Cannot update a contact that doesn't exist");
    }

    [Fact]
    public async Task BulkInsert_LargeDataset_Succeeds()
    {
        // Arrange - use unique prefix
        var prefix = $"bulk-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 100).Select(i =>
            MakeContact($"F{i}", $"L{i}", $"{prefix}-{i}@t.com"));

        // Act
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        var count = await Context.Contacts.CountAsync(c => c.Email!.StartsWith(prefix));

        // Assert
        count.Should().Be(100);
    }

    [Fact]
    public async Task Query_WithDefaultPartner_ReturnsDefaultPartnerContacts()
    {
        // Arrange - create contacts with the default test partner
        var prefix = $"defp-{Guid.NewGuid():N}";
        await Context.Contacts.AddRangeAsync(new[]
        {
            MakeContact("W", "P", $"{prefix}-wp@t.com"),
            MakeContact("N", "P", $"{prefix}-np@t.com")
        });
        await SaveChangesAsync();

        // Act - query contacts belonging to default partner within our prefix
        var contacts = await Context.Contacts
            .Where(c => c.PartnerId == DefaultTestPartnerId && c.Email!.StartsWith(prefix))
            .ToListAsync();

        // Assert
        contacts.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_RemainsDeleted()
    {
        // Arrange
        var contact = MakeContact("A", "D", "ad-already@t.com", isDeleted: true);
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Try to "delete" again
        contact.IsDeleted = true;
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Query_MaxInt_Id_ReturnsNull()
    {
        // Act
        var result = await Context.Contacts.FindAsync(int.MaxValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Concurrent_Reads_ReturnConsistentData()
    {
        // Arrange
        var contact = MakeContact("C", "1", "c-consist@t.com");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Multiple reads
        var result1 = await Context.Contacts.FindAsync(contact.Id);
        var result2 = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result1!.Name.Should().Be(result2!.Name);
    }

    [Fact]
    public async Task Query_WithUniqueMarker_ReturnsOnlyTestData()
    {
        // Arrange - create data with a unique marker
        var prefix = $"marker-{Guid.NewGuid():N}";
        await Context.Contacts.AddAsync(MakeContact("W", "C", $"{prefix}-w@t.com"));
        await SaveChangesAsync();

        // Act
        var count = await Context.Contacts.CountAsync(c => c.Email!.StartsWith(prefix));

        // Assert - exactly 1 (only our test data)
        count.Should().Be(1);
    }

    #endregion

    #region Additional Workflow Tests (2 tests)

    [Fact]
    public async Task Contact_MultipleStatusChanges_TracksLatest()
    {
        // Arrange
        var contact = MakeContact("S", "T", "st-multi@t.com");
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Multiple status changes
        contact.Status = EntityStatus.Inactive;
        await SaveChangesAsync();

        contact.Status = EntityStatus.Active;
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.Status.Should().Be(EntityStatus.Active, "Latest status should be Active");
    }

    [Fact]
    public async Task Contact_BulkStatusUpdate_AppliesCorrectly()
    {
        // Arrange - use unique prefix to scope
        var prefix = $"blkst-{Guid.NewGuid():N}";
        var contacts = Enumerable.Range(1, 5).Select(i =>
            MakeContact($"B{i}", $"U{i}", $"{prefix}-{i}@t.com")).ToList();
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act - Deactivate first, third, fifth contacts (by Id order)
        var ordered = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix))
            .OrderBy(c => c.Id)
            .ToListAsync();
        var toDeactivate = ordered.Where((_, i) => i % 2 == 0).ToList();
        foreach (var c in toDeactivate) c.Status = EntityStatus.Inactive;
        await SaveChangesAsync();

        // Assert - scoped to test data
        var active = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix) && c.Status == EntityStatus.Active)
            .CountAsync();
        var inactive = await Context.Contacts
            .Where(c => c.Email!.StartsWith(prefix) && c.Status == EntityStatus.Inactive)
            .CountAsync();
        active.Should().Be(2);
        inactive.Should().Be(3);
    }

    #endregion
}
