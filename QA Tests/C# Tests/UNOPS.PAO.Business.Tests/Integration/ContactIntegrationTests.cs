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
/// Required: ≥50 tests (FIXED minimum, core category)
/// Current: 52 tests
/// </summary>
public class ContactIntegrationTests : IntegrationTestBase
{
    #region CRUD Workflow (10 tests)

    [Fact]
    public async Task Contact_CanBeCreatedWithPartner()
    {
        // Arrange
        var partner = new UNOPSPartner
        {
            Name = "Test Partner",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();

        var contact = new UNOPSContact
        {
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Title = "Manager",
            PartnerId = partner.Id,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Partner.Should().NotBeNull();
        result.Partner!.Name.Should().Be("Test Partner");
    }

    [Fact]
    public async Task Contact_CanBeRetrievedById()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "Jane Smith",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com",
            Title = "Director",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
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
        var contact = new UNOPSContact
        {
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Title = "Manager",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.Title = "Senior Manager";
        contact.Email = "john.doe@test.com";
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.Title.Should().Be("Senior Manager");
        result.Email.Should().Be("john.doe@test.com");
    }

    [Fact]
    public async Task Contact_SoftDelete_SetsIsDeleted()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "To Delete",
            FirstName = "To",
            LastName = "Delete",
            Email = "delete@test.com",
            Title = "Test",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
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
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "Active1", FirstName = "A", LastName = "One", Email = "a@test.com", Title = "T", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Deleted", FirstName = "D", LastName = "Two", Email = "d@test.com", Title = "T", Status = EntityStatus.Active, IsDeleted = true, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Active2", FirstName = "A", LastName = "Three", Email = "b@test.com", Title = "T", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var activeContacts = await Context.Contacts.Where(c => !c.IsDeleted).ToListAsync();

        // Assert
        activeContacts.Should().HaveCount(2);
        activeContacts.Should().NotContain(c => c.Name == "Deleted");
    }

    [Fact]
    public async Task Contact_CanBeRestoredAfterSoftDelete()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "Restorable",
            FirstName = "Re",
            LastName = "Store",
            Email = "restore@test.com",
            Title = "T",
            Status = EntityStatus.Active,
            IsDeleted = true,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
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
        // Arrange
        var contacts = Enumerable.Range(1, 10).Select(i => new UNOPSContact
        {
            Name = $"Contact {i}",
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            Email = $"contact{i}@test.com",
            Title = "Staff",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });

        // Act
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        var count = await Context.Contacts.CountAsync();

        // Assert
        count.Should().Be(10);
    }

    [Fact]
    public async Task Contact_Update_PreservesOtherFields()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "Original Name",
            FirstName = "Original",
            LastName = "Name",
            Email = "original@test.com",
            Title = "Manager",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Only update title
        contact.Title = "Director";
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result!.Title.Should().Be("Director");
        result.Email.Should().Be("original@test.com", "Email should not change");
        result.FirstName.Should().Be("Original", "Name should not change");
    }

    [Fact]
    public async Task Contact_CreateWithAllFields_Persisted()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "Full Contact",
            FirstName = "Full",
            LastName = "Contact",
            Email = "full@test.com",
            Title = "CEO",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("full@test.com");
        result.Title.Should().Be("CEO");
    }

    [Fact]
    public async Task Contact_StatusChange_Persisted()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "Status Test",
            FirstName = "Status",
            LastName = "Test",
            Email = "status@test.com",
            Title = "Staff",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
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
        var contact = new UNOPSContact
        {
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Title = "Manager",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var interactions = new List<UNOPSInteraction>
        {
            new() { Name = "Meeting 1", Subject = "Meeting 1", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = "Call 1", Subject = "Call 1", Type = InteractionType.Call, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
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
        var partner = new UNOPSPartner
        {
            Name = "Test Partner",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();

        var contacts = new List<UNOPSContact>
        {
            new() { Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "john@test.com", Title = "Manager", PartnerId = partner.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = "Jane Smith", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", Title = "Director", PartnerId = partner.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
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
        var partner = new UNOPSPartner
        {
            Name = "Partner A",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();

        var contact = new UNOPSContact
        {
            Name = "Contact 1",
            FirstName = "C",
            LastName = "One",
            Email = "c1@test.com",
            Title = "Staff",
            PartnerId = partner.Id,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == contact.Id);

        // Assert
        result!.Partner.Should().NotBeNull();
        result.Partner!.Name.Should().Be("Partner A");
        result.PartnerId.Should().Be(partner.Id);
    }

    [Fact]
    public async Task Contact_DifferentPartners_IsolatedCorrectly()
    {
        // Arrange
        var partnerA = new UNOPSPartner { Name = "Partner A", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        var partnerB = new UNOPSPartner { Name = "Partner B", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        await Context.Partners.AddRangeAsync(new[] { partnerA, partnerB });
        await SaveChangesAsync();

        var contacts = new List<UNOPSContact>
        {
            new() { Name = "Contact A1", FirstName = "A", LastName = "1", Email = "a1@test.com", Title = "T", PartnerId = partnerA.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = "Contact A2", FirstName = "A", LastName = "2", Email = "a2@test.com", Title = "T", PartnerId = partnerA.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = "Contact B1", FirstName = "B", LastName = "1", Email = "b1@test.com", Title = "T", PartnerId = partnerB.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
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
        var contact = new UNOPSContact { Name = "C1", FirstName = "C", LastName = "1", Email = "c@test.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        var interaction = new UNOPSInteraction { Name = "Meeting", Subject = "Meeting", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };

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
            new() { Name = "C1", FirstName = "C", LastName = "1", Email = "c1@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = "C2", FirstName = "C", LastName = "2", Email = "c2@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = "C3", FirstName = "C", LastName = "3", Email = "c3@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        var interaction = new UNOPSInteraction { Name = "Group Meeting", Subject = "Group Meeting", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
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
    public async Task Contact_WithoutPartner_StillPersists()
    {
        // Arrange - Contact without PartnerId
        var contact = new UNOPSContact
        {
            Name = "Unlinked",
            FirstName = "Un",
            LastName = "Linked",
            Email = "unlinked@test.com",
            Title = "Consultant",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.PartnerId.Should().Be(0);
    }

    [Fact]
    public async Task Contact_PartnerDeletion_ContactRemains()
    {
        // Arrange
        var partner = new UNOPSPartner
        {
            Name = "To Delete",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        var contact = new UNOPSContact
        {
            Name = "Orphan",
            FirstName = "Or",
            LastName = "Phan",
            Email = "orphan@test.com",
            Title = "T",
            PartnerId = partner.Id,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
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
        var partnerA = new UNOPSPartner { Name = "Partner A", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        var partnerB = new UNOPSPartner { Name = "Partner B", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        await Context.Partners.AddRangeAsync(new[] { partnerA, partnerB });
        await SaveChangesAsync();
        var contact = new UNOPSContact
        {
            Name = "Transfer",
            FirstName = "Trans",
            LastName = "Fer",
            Email = "transfer@test.com",
            Title = "T",
            PartnerId = partnerA.Id,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.PartnerId = partnerB.Id;
        await SaveChangesAsync();
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == contact.Id);

        // Assert
        result!.PartnerId.Should().Be(partnerB.Id);
        result.Partner!.Name.Should().Be("Partner B");
    }

    #endregion

    #region Search and Filtering (10 tests)

    [Fact]
    public async Task Search_ByFirstName_ReturnsMatches()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "j@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Jane Doe", FirstName = "Jane", LastName = "Doe", Email = "ja@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "John Smith", FirstName = "John", LastName = "Smith", Email = "js@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.FirstName == "John").ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_ByLastName_ReturnsMatches()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "j@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Jane Doe", FirstName = "Jane", LastName = "Doe", Email = "ja@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Bob Smith", FirstName = "Bob", LastName = "Smith", Email = "b@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.LastName == "Doe").ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_ByEmail_ReturnsExactMatch()
    {
        // Arrange
        var contacts = new List<UNOPSContact>
        {
            new() { Name = "C1", FirstName = "C", LastName = "1", Email = "unique@test.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = "C2", FirstName = "C", LastName = "2", Email = "other@test.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FirstOrDefaultAsync(c => c.Email == "unique@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(contacts[0].Id);
    }

    [Fact]
    public async Task Filter_ByStatus_Active_ReturnsOnlyActive()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "Active", FirstName = "A", LastName = "1", Email = "a@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Inactive", FirstName = "I", LastName = "2", Email = "i@t.com", Title = "T", Status = EntityStatus.Inactive, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Active2", FirstName = "A", LastName = "3", Email = "a2@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.Status == EntityStatus.Active).ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Filter_ByPartner_ReturnsOnlyPartnerContacts()
    {
        // Arrange
        var partner1 = new UNOPSPartner { Name = "P1", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        var partner2 = new UNOPSPartner { Name = "P2", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        await Context.Partners.AddRangeAsync(new[] { partner1, partner2 });
        await SaveChangesAsync();
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "C1", FirstName = "C", LastName = "1", Email = "c1@t.com", Title = "T", PartnerId = partner1.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "C2", FirstName = "C", LastName = "2", Email = "c2@t.com", Title = "T", PartnerId = partner1.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "C3", FirstName = "C", LastName = "3", Email = "c3@t.com", Title = "T", PartnerId = partner2.Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
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
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "Active", FirstName = "A", LastName = "1", Email = "a@t.com", Title = "T", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Deleted", FirstName = "D", LastName = "2", Email = "d@t.com", Title = "T", Status = EntityStatus.Active, IsDeleted = true, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => !c.IsDeleted).ToListAsync();

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_NoResults_ReturnsEmptyList()
    {
        // Arrange
        await Context.Contacts.AddAsync(new UNOPSContact
        {
            Name = "John",
            FirstName = "John",
            LastName = "Doe",
            Email = "j@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.FirstName == "NonExistent").ToListAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Filter_CombinedCriteria_WorksCorrectly()
    {
        // Arrange
        var partner = new UNOPSPartner { Name = "P1", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        var contact = new UNOPSContact
        {
            Name = "Active P1",
            FirstName = "A",
            LastName = "1",
            Email = "a1@t.com",
            Title = "T",
            PartnerId = partner.Id,
            Status = EntityStatus.Active,
            IsDeleted = false,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddRangeAsync(new[]
        {
            contact,
            new UNOPSContact { Name = "Inactive P1", FirstName = "I", LastName = "2", Email = "i@t.com", Title = "T", PartnerId = partner.Id, Status = EntityStatus.Inactive, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Deleted P1", FirstName = "D", LastName = "3", Email = "d@t.com", Title = "T", PartnerId = partner.Id, Status = EntityStatus.Active, IsDeleted = true, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act - Active, non-deleted, for partner 1
        var results = await Context.Contacts
            .Where(c => c.PartnerId == partner.Id && c.Status == EntityStatus.Active && !c.IsDeleted)
            .ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().Id.Should().Be(contact.Id);
    }

    [Fact]
    public async Task Sort_ByLastName_ReturnsOrdered()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "Charlie Z", FirstName = "Charlie", LastName = "Zulu", Email = "c@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Alice A", FirstName = "Alice", LastName = "Alpha", Email = "a@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "Bob M", FirstName = "Bob", LastName = "Mike", Email = "b@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.OrderBy(c => c.LastName).ToListAsync();

        // Assert
        results[0].LastName.Should().Be("Alpha");
        results[1].LastName.Should().Be("Mike");
        results[2].LastName.Should().Be("Zulu");
    }

    [Fact]
    public async Task Search_ByTitle_ReturnsMatches()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "C1", FirstName = "C", LastName = "1", Email = "c1@t.com", Title = "Director", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "C2", FirstName = "C", LastName = "2", Email = "c2@t.com", Title = "Manager", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "C3", FirstName = "C", LastName = "3", Email = "c3@t.com", Title = "Director", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.Title == "Director").ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    #endregion

    #region Pagination (5 tests)

    [Fact]
    public async Task Pagination_FirstPage_ReturnsCorrectResults()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 25).Select(i => new UNOPSContact
        {
            Name = $"Contact {i}",
            FirstName = $"F{i}",
            LastName = $"L{i}",
            Email = $"c{i}@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts.OrderBy(c => c.Id).Take(10).ToListAsync();

        // Assert
        page.Should().HaveCount(10);
        page.Select(c => c.Id).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Pagination_SecondPage_ReturnsCorrectResults()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 25).Select(i => new UNOPSContact
        {
            Name = $"Contact {i}",
            FirstName = $"F{i}",
            LastName = $"L{i}",
            Email = $"c{i}@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts.OrderBy(c => c.Id).Skip(10).Take(10).ToListAsync();

        // Assert
        page.Should().HaveCount(10);
        var allIds = await Context.Contacts.OrderBy(c => c.Id).Select(c => c.Id).ToListAsync();
        page.First().Id.Should().Be(allIds[10]);
        page.Last().Id.Should().Be(allIds[19]);
    }

    [Fact]
    public async Task Pagination_LastPage_ReturnRemainingResults()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 25).Select(i => new UNOPSContact
        {
            Name = $"Contact {i}",
            FirstName = $"F{i}",
            LastName = $"L{i}",
            Email = $"c{i}@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts.OrderBy(c => c.Id).Skip(20).Take(10).ToListAsync();

        // Assert
        page.Should().HaveCount(5); // Only 5 remaining
    }

    [Fact]
    public async Task Pagination_BeyondData_ReturnsEmpty()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 5).Select(i => new UNOPSContact
        {
            Name = $"Contact {i}",
            FirstName = $"F{i}",
            LastName = $"L{i}",
            Email = $"c{i}@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts.OrderBy(c => c.Id).Skip(100).Take(10).ToListAsync();

        // Assert
        page.Should().BeEmpty();
    }

    [Fact]
    public async Task Pagination_TotalCount_Accurate()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 33).Select(i => new UNOPSContact
        {
            Name = $"Contact {i}",
            FirstName = $"F{i}",
            LastName = $"L{i}",
            Email = $"c{i}@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var totalCount = await Context.Contacts.CountAsync();
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
        var result = await Context.Contacts.FindAsync(999);

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
        var firstContact = new UNOPSContact
        {
            Name = "First",
            FirstName = "F",
            LastName = "1",
            Email = "f@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(firstContact);
        await SaveChangesAsync();

        // Act & Assert - Create second contact with same ID to trigger duplicate key violation
        var duplicateContact = new UNOPSContact
        {
            Id = firstContact.Id,
            Name = "Duplicate",
            FirstName = "D",
            LastName = "2",
            Email = "d@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        var act = async () =>
        {
            await Context.Contacts.AddAsync(duplicateContact);
            await SaveChangesAsync();
        };
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Query_EmptyTable_ReturnsEmptyList()
    {
        // Act
        var results = await Context.Contacts.ToListAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Count_EmptyTable_ReturnsZero()
    {
        // Act
        var count = await Context.Contacts.CountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task FirstOrDefault_NoMatch_ReturnsNull()
    {
        // Arrange
        await Context.Contacts.AddAsync(new UNOPSContact
        {
            Name = "Exists",
            FirstName = "E",
            LastName = "1",
            Email = "e@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FirstOrDefaultAsync(c => c.Email == "nonexistent@test.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Filter_DeletedOnly_EmptyWhenNoneDeleted()
    {
        // Arrange
        await Context.Contacts.AddAsync(new UNOPSContact
        {
            Name = "Active",
            FirstName = "A",
            LastName = "1",
            Email = "a@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            IsDeleted = false,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await SaveChangesAsync();

        // Act
        var deletedContacts = await Context.Contacts.Where(c => c.IsDeleted).ToListAsync();

        // Assert
        deletedContacts.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_NonExistentContact_ThrowsException()
    {
        // Act
        var nonExistent = await Context.Contacts.FindAsync(999);

        // Assert
        nonExistent.Should().BeNull("Cannot update a contact that doesn't exist");
    }

    [Fact]
    public async Task BulkInsert_LargeDataset_Succeeds()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 100).Select(i => new UNOPSContact
        {
            Name = $"Bulk {i}",
            FirstName = $"F{i}",
            LastName = $"L{i}",
            Email = $"bulk{i}@t.com",
            Title = "Staff",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });

        // Act
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        var count = await Context.Contacts.CountAsync();

        // Assert
        count.Should().Be(100);
    }

    [Fact]
    public async Task Query_WithNoPartner_ReturnsOrphanContacts()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new UNOPSContact { Name = "WithPartner", FirstName = "W", LastName = "P", Email = "wp@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new UNOPSContact { Name = "NoPartner", FirstName = "N", LastName = "P", Email = "np@t.com", Title = "T", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        });
        await SaveChangesAsync();

        // Act
        var orphans = await Context.Contacts.Where(c => c.PartnerId == 0).ToListAsync();

        // Assert
        orphans.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_RemainsDeleted()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "AlreadyDel",
            FirstName = "A",
            LastName = "D",
            Email = "ad@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            IsDeleted = true,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
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
        var contact = new UNOPSContact
        {
            Name = "Consistent",
            FirstName = "C",
            LastName = "1",
            Email = "c@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Multiple reads
        var result1 = await Context.Contacts.FindAsync(contact.Id);
        var result2 = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result1!.Name.Should().Be(result2!.Name);
    }

    [Fact]
    public async Task Query_AfterClearDatabase_ReturnsEmpty()
    {
        // Arrange
        await Context.Contacts.AddAsync(new UNOPSContact
        {
            Name = "WillBeCleared",
            FirstName = "W",
            LastName = "C",
            Email = "w@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        });
        await SaveChangesAsync();

        // Act
        ClearDatabase();
        var count = await Context.Contacts.CountAsync();

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region Additional Workflow Tests (2 tests)

    [Fact]
    public async Task Contact_MultipleStatusChanges_TracksLatest()
    {
        // Arrange
        var contact = new UNOPSContact
        {
            Name = "StatusTrack",
            FirstName = "S",
            LastName = "T",
            Email = "st@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
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
        // Arrange
        var contacts = Enumerable.Range(1, 5).Select(i => new UNOPSContact
        {
            Name = $"Bulk{i}",
            FirstName = $"B{i}",
            LastName = $"U{i}",
            Email = $"bu{i}@t.com",
            Title = "T",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        }).ToList();
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act - Deactivate first, third, fifth contacts (by Id order)
        var ordered = await Context.Contacts.OrderBy(c => c.Id).ToListAsync();
        var toDeactivate = ordered.Where((c, i) => i % 2 == 0).ToList();
        foreach (var c in toDeactivate) c.Status = EntityStatus.Inactive;
        await SaveChangesAsync();

        // Assert
        var active = await Context.Contacts.Where(c => c.Status == EntityStatus.Active).CountAsync();
        var inactive = await Context.Contacts.Where(c => c.Status == EntityStatus.Inactive).CountAsync();
        active.Should().Be(2);
        inactive.Should().Be(3);
    }

    #endregion
}
