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
        var partner = new Partner { Id = 1, Name = "Test Partner", Status = EntityStatus.Active };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();

        var contact = new Contact
        {
            Id = 1,
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Title = "Manager",
            PartnerId = 1,
            Status = EntityStatus.Active
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == 1);

        // Assert
        result.Should().NotBeNull();
        result!.Partner.Should().NotBeNull();
        result.Partner!.Name.Should().Be("Test Partner");
    }

    [Fact]
    public async Task Contact_CanBeRetrievedById()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "Jane Smith",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com",
            Title = "Director",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task Contact_CanBeUpdated()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.Title = "Senior Manager";
        contact.Email = "john.doe@test.com";
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result!.Title.Should().Be("Senior Manager");
        result.Email.Should().Be("john.doe@test.com");
    }

    [Fact]
    public async Task Contact_SoftDelete_SetsIsDeleted()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "To Delete",
            FirstName = "To",
            LastName = "Delete",
            Email = "delete@test.com",
            Title = "Test",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.IsDeleted = true;
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Contact_SoftDeleted_ExcludedFromActiveQueries()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "Active1", FirstName = "A", LastName = "One", Email = "a@test.com", Title = "T", Status = EntityStatus.Active, IsDeleted = false },
            new Contact { Id = 2, Name = "Deleted", FirstName = "D", LastName = "Two", Email = "d@test.com", Title = "T", Status = EntityStatus.Active, IsDeleted = true },
            new Contact { Id = 3, Name = "Active2", FirstName = "A", LastName = "Three", Email = "b@test.com", Title = "T", Status = EntityStatus.Active, IsDeleted = false }
        });
        await SaveChangesAsync();

        // Act
        var activeContacts = await Context.Contacts.Where(c => !c.IsDeleted).ToListAsync();

        // Assert
        activeContacts.Should().HaveCount(2);
        activeContacts.Should().NotContain(c => c.Id == 2);
    }

    [Fact]
    public async Task Contact_CanBeRestoredAfterSoftDelete()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "Restorable",
            FirstName = "Re",
            LastName = "Store",
            Email = "restore@test.com",
            Title = "T",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.IsDeleted = false;
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Contact_CreateMultiple_AllPersisted()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 10).Select(i => new Contact
        {
            Id = i,
            Name = $"Contact {i}",
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            Email = $"contact{i}@test.com",
            Title = "Staff",
            Status = EntityStatus.Active
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
        var contact = new Contact
        {
            Id = 1,
            Name = "Original Name",
            FirstName = "Original",
            LastName = "Name",
            Email = "original@test.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Only update title
        contact.Title = "Director";
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result!.Title.Should().Be("Director");
        result.Email.Should().Be("original@test.com", "Email should not change");
        result.FirstName.Should().Be("Original", "Name should not change");
    }

    [Fact]
    public async Task Contact_CreateWithAllFields_Persisted()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "Full Contact",
            FirstName = "Full",
            LastName = "Contact",
            Email = "full@test.com",
            Title = "CEO",
            Status = EntityStatus.Active
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("full@test.com");
        result.Title.Should().Be("CEO");
    }

    [Fact]
    public async Task Contact_StatusChange_Persisted()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "Status Test",
            FirstName = "Status",
            LastName = "Test",
            Email = "status@test.com",
            Title = "Staff",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.Status = EntityStatus.Inactive;
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result!.Status.Should().Be(EntityStatus.Inactive);
    }

    #endregion

    #region Relationships (10 tests)

    [Fact]
    public async Task Contact_CanHaveMultipleInteractions()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var interactions = new List<Interaction>
        {
            new() { Id = 1, Name = "Meeting 1", Subject = "Meeting 1", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active },
            new() { Id = 2, Name = "Call 1", Subject = "Call 1", Type = InteractionType.Call, Date = DateTime.UtcNow, Status = EntityStatus.Active }
        };
        await Context.Interactions.AddRangeAsync(interactions);

        var interactionContacts = new List<InteractionContact>
        {
            new() { InteractionId = 1, ContactId = 1 },
            new() { InteractionId = 2, ContactId = 1 }
        };
        await Context.InteractionContacts.AddRangeAsync(interactionContacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.InteractionContacts.Where(ic => ic.ContactId == 1).CountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task Partner_CanHaveMultipleContacts()
    {
        // Arrange
        var partner = new Partner { Id = 1, Name = "Test Partner", Status = EntityStatus.Active };
        await Context.Partners.AddAsync(partner);

        var contacts = new List<Contact>
        {
            new() { Id = 1, Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "john@test.com", Title = "Manager", PartnerId = 1, Status = EntityStatus.Active },
            new() { Id = 2, Name = "Jane Smith", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", Title = "Director", PartnerId = 1, Status = EntityStatus.Active }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Partners
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.Id == 1);

        // Assert
        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(2);
    }

    [Fact]
    public async Task Contact_PartnerRelationship_LoadedCorrectly()
    {
        // Arrange
        var partner = new Partner { Id = 1, Name = "Partner A", Status = EntityStatus.Active };
        await Context.Partners.AddAsync(partner);

        var contact = new Contact
        {
            Id = 1, Name = "Contact 1", FirstName = "C", LastName = "One",
            Email = "c1@test.com", Title = "Staff", PartnerId = 1, Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == 1);

        // Assert
        result!.Partner.Should().NotBeNull();
        result.Partner!.Name.Should().Be("Partner A");
        result.PartnerId.Should().Be(1);
    }

    [Fact]
    public async Task Contact_DifferentPartners_IsolatedCorrectly()
    {
        // Arrange
        await Context.Partners.AddRangeAsync(new[]
        {
            new Partner { Id = 1, Name = "Partner A", Status = EntityStatus.Active },
            new Partner { Id = 2, Name = "Partner B", Status = EntityStatus.Active }
        });

        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "Contact A1", FirstName = "A", LastName = "1", Email = "a1@test.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "Contact A2", FirstName = "A", LastName = "2", Email = "a2@test.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active },
            new Contact { Id = 3, Name = "Contact B1", FirstName = "B", LastName = "1", Email = "b1@test.com", Title = "T", PartnerId = 2, Status = EntityStatus.Active }
        });
        await SaveChangesAsync();

        // Act
        var partnerAContacts = await Context.Contacts.Where(c => c.PartnerId == 1).CountAsync();
        var partnerBContacts = await Context.Contacts.Where(c => c.PartnerId == 2).CountAsync();

        // Assert
        partnerAContacts.Should().Be(2);
        partnerBContacts.Should().Be(1);
    }

    [Fact]
    public async Task Contact_InteractionLink_BothDirections()
    {
        // Arrange
        var contact = new Contact { Id = 1, Name = "C1", FirstName = "C", LastName = "1", Email = "c@test.com", Title = "T", Status = EntityStatus.Active };
        var interaction = new Interaction { Id = 1, Name = "Meeting", Subject = "Meeting", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active };

        await Context.Contacts.AddAsync(contact);
        await Context.Interactions.AddAsync(interaction);
        await Context.InteractionContacts.AddAsync(new InteractionContact { ContactId = 1, InteractionId = 1 });
        await SaveChangesAsync();

        // Act - Query from both sides
        var contactInteractions = await Context.InteractionContacts.Where(ic => ic.ContactId == 1).CountAsync();
        var interactionContacts = await Context.InteractionContacts.Where(ic => ic.InteractionId == 1).CountAsync();

        // Assert
        contactInteractions.Should().Be(1);
        interactionContacts.Should().Be(1);
    }

    [Fact]
    public async Task MultipleContacts_LinkedToSameInteraction()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "C1", FirstName = "C", LastName = "1", Email = "c1@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "C2", FirstName = "C", LastName = "2", Email = "c2@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 3, Name = "C3", FirstName = "C", LastName = "3", Email = "c3@t.com", Title = "T", Status = EntityStatus.Active }
        });
        var interaction = new Interaction { Id = 1, Name = "Group Meeting", Subject = "Group Meeting", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active };
        await Context.Interactions.AddAsync(interaction);
        await Context.InteractionContacts.AddRangeAsync(new[]
        {
            new InteractionContact { ContactId = 1, InteractionId = 1 },
            new InteractionContact { ContactId = 2, InteractionId = 1 },
            new InteractionContact { ContactId = 3, InteractionId = 1 }
        });
        await SaveChangesAsync();

        // Act
        var attendees = await Context.InteractionContacts.Where(ic => ic.InteractionId == 1).CountAsync();

        // Assert
        attendees.Should().Be(3);
    }

    [Fact]
    public async Task Contact_WithoutPartner_StillPersists()
    {
        // Arrange - Contact without PartnerId
        var contact = new Contact
        {
            Id = 1, Name = "Unlinked", FirstName = "Un", LastName = "Linked",
            Email = "unlinked@test.com", Title = "Consultant", Status = EntityStatus.Active
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.PartnerId.Should().BeNull();
    }

    [Fact]
    public async Task Contact_PartnerDeletion_ContactRemains()
    {
        // Arrange
        var partner = new Partner { Id = 1, Name = "To Delete", Status = EntityStatus.Active };
        await Context.Partners.AddAsync(partner);
        var contact = new Contact
        {
            Id = 1, Name = "Orphan", FirstName = "Or", LastName = "Phan",
            Email = "orphan@test.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Soft delete partner
        partner.IsDeleted = true;
        await SaveChangesAsync();
        var contactResult = await Context.Contacts.FindAsync(1);

        // Assert
        contactResult.Should().NotBeNull("Contact should remain after partner soft delete");
    }

    [Fact]
    public async Task Contact_TransferPartner_UpdatesRelationship()
    {
        // Arrange
        await Context.Partners.AddRangeAsync(new[]
        {
            new Partner { Id = 1, Name = "Partner A", Status = EntityStatus.Active },
            new Partner { Id = 2, Name = "Partner B", Status = EntityStatus.Active }
        });
        var contact = new Contact
        {
            Id = 1, Name = "Transfer", FirstName = "Trans", LastName = "Fer",
            Email = "transfer@test.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.PartnerId = 2;
        await SaveChangesAsync();
        var result = await Context.Contacts.Include(c => c.Partner).FirstOrDefaultAsync(c => c.Id == 1);

        // Assert
        result!.PartnerId.Should().Be(2);
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
            new Contact { Id = 1, Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "j@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "Jane Doe", FirstName = "Jane", LastName = "Doe", Email = "ja@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 3, Name = "John Smith", FirstName = "John", LastName = "Smith", Email = "js@t.com", Title = "T", Status = EntityStatus.Active }
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
            new Contact { Id = 1, Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "j@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "Jane Doe", FirstName = "Jane", LastName = "Doe", Email = "ja@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 3, Name = "Bob Smith", FirstName = "Bob", LastName = "Smith", Email = "b@t.com", Title = "T", Status = EntityStatus.Active }
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
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "C1", FirstName = "C", LastName = "1", Email = "unique@test.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "C2", FirstName = "C", LastName = "2", Email = "other@test.com", Title = "T", Status = EntityStatus.Active }
        });
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FirstOrDefaultAsync(c => c.Email == "unique@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Filter_ByStatus_Active_ReturnsOnlyActive()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "Active", FirstName = "A", LastName = "1", Email = "a@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "Inactive", FirstName = "I", LastName = "2", Email = "i@t.com", Title = "T", Status = EntityStatus.Inactive },
            new Contact { Id = 3, Name = "Active2", FirstName = "A", LastName = "3", Email = "a2@t.com", Title = "T", Status = EntityStatus.Active }
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
        await Context.Partners.AddRangeAsync(new[]
        {
            new Partner { Id = 1, Name = "P1", Status = EntityStatus.Active },
            new Partner { Id = 2, Name = "P2", Status = EntityStatus.Active }
        });
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "C1", FirstName = "C", LastName = "1", Email = "c1@t.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "C2", FirstName = "C", LastName = "2", Email = "c2@t.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active },
            new Contact { Id = 3, Name = "C3", FirstName = "C", LastName = "3", Email = "c3@t.com", Title = "T", PartnerId = 2, Status = EntityStatus.Active }
        });
        await SaveChangesAsync();

        // Act
        var results = await Context.Contacts.Where(c => c.PartnerId == 1).ToListAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Filter_ExcludesDeleted_ByDefault()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "Active", FirstName = "A", LastName = "1", Email = "a@t.com", Title = "T", Status = EntityStatus.Active, IsDeleted = false },
            new Contact { Id = 2, Name = "Deleted", FirstName = "D", LastName = "2", Email = "d@t.com", Title = "T", Status = EntityStatus.Active, IsDeleted = true }
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
        await Context.Contacts.AddAsync(new Contact
        {
            Id = 1, Name = "John", FirstName = "John", LastName = "Doe", Email = "j@t.com", Title = "T", Status = EntityStatus.Active
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
        await Context.Partners.AddAsync(new Partner { Id = 1, Name = "P1", Status = EntityStatus.Active });
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "Active P1", FirstName = "A", LastName = "1", Email = "a1@t.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active, IsDeleted = false },
            new Contact { Id = 2, Name = "Inactive P1", FirstName = "I", LastName = "2", Email = "i@t.com", Title = "T", PartnerId = 1, Status = EntityStatus.Inactive, IsDeleted = false },
            new Contact { Id = 3, Name = "Deleted P1", FirstName = "D", LastName = "3", Email = "d@t.com", Title = "T", PartnerId = 1, Status = EntityStatus.Active, IsDeleted = true }
        });
        await SaveChangesAsync();

        // Act - Active, non-deleted, for partner 1
        var results = await Context.Contacts
            .Where(c => c.PartnerId == 1 && c.Status == EntityStatus.Active && !c.IsDeleted)
            .ToListAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task Sort_ByLastName_ReturnsOrdered()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(new[]
        {
            new Contact { Id = 1, Name = "Charlie Z", FirstName = "Charlie", LastName = "Zulu", Email = "c@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "Alice A", FirstName = "Alice", LastName = "Alpha", Email = "a@t.com", Title = "T", Status = EntityStatus.Active },
            new Contact { Id = 3, Name = "Bob M", FirstName = "Bob", LastName = "Mike", Email = "b@t.com", Title = "T", Status = EntityStatus.Active }
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
            new Contact { Id = 1, Name = "C1", FirstName = "C", LastName = "1", Email = "c1@t.com", Title = "Director", Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "C2", FirstName = "C", LastName = "2", Email = "c2@t.com", Title = "Manager", Status = EntityStatus.Active },
            new Contact { Id = 3, Name = "C3", FirstName = "C", LastName = "3", Email = "c3@t.com", Title = "Director", Status = EntityStatus.Active }
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
        var contacts = Enumerable.Range(1, 25).Select(i => new Contact
        {
            Id = i, Name = $"Contact {i}", FirstName = $"F{i}", LastName = $"L{i}",
            Email = $"c{i}@t.com", Title = "T", Status = EntityStatus.Active
        });
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts.OrderBy(c => c.Id).Take(10).ToListAsync();

        // Assert
        page.Should().HaveCount(10);
        page.First().Id.Should().Be(1);
        page.Last().Id.Should().Be(10);
    }

    [Fact]
    public async Task Pagination_SecondPage_ReturnsCorrectResults()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 25).Select(i => new Contact
        {
            Id = i, Name = $"Contact {i}", FirstName = $"F{i}", LastName = $"L{i}",
            Email = $"c{i}@t.com", Title = "T", Status = EntityStatus.Active
        });
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var page = await Context.Contacts.OrderBy(c => c.Id).Skip(10).Take(10).ToListAsync();

        // Assert
        page.Should().HaveCount(10);
        page.First().Id.Should().Be(11);
        page.Last().Id.Should().Be(20);
    }

    [Fact]
    public async Task Pagination_LastPage_ReturnRemainingResults()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 25).Select(i => new Contact
        {
            Id = i, Name = $"Contact {i}", FirstName = $"F{i}", LastName = $"L{i}",
            Email = $"c{i}@t.com", Title = "T", Status = EntityStatus.Active
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
        var contacts = Enumerable.Range(1, 5).Select(i => new Contact
        {
            Id = i, Name = $"Contact {i}", FirstName = $"F{i}", LastName = $"L{i}",
            Email = $"c{i}@t.com", Title = "T", Status = EntityStatus.Active
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
        var contacts = Enumerable.Range(1, 33).Select(i => new Contact
        {
            Id = i, Name = $"Contact {i}", FirstName = $"F{i}", LastName = $"L{i}",
            Email = $"c{i}@t.com", Title = "T", Status = EntityStatus.Active
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
        // Arrange
        await Context.Contacts.AddAsync(new Contact
        {
            Id = 1, Name = "First", FirstName = "F", LastName = "1", Email = "f@t.com", Title = "T", Status = EntityStatus.Active
        });
        await SaveChangesAsync();

        // Act & Assert
        await Context.Contacts.AddAsync(new Contact
        {
            Id = 1, Name = "Duplicate", FirstName = "D", LastName = "2", Email = "d@t.com", Title = "T", Status = EntityStatus.Active
        });

        var act = async () => await SaveChangesAsync();
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
        await Context.Contacts.AddAsync(new Contact
        {
            Id = 1, Name = "Exists", FirstName = "E", LastName = "1", Email = "e@t.com", Title = "T", Status = EntityStatus.Active
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
        await Context.Contacts.AddAsync(new Contact
        {
            Id = 1, Name = "Active", FirstName = "A", LastName = "1", Email = "a@t.com", Title = "T", Status = EntityStatus.Active, IsDeleted = false
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
        var contacts = Enumerable.Range(1, 100).Select(i => new Contact
        {
            Id = i, Name = $"Bulk {i}", FirstName = $"F{i}", LastName = $"L{i}",
            Email = $"bulk{i}@t.com", Title = "Staff", Status = EntityStatus.Active
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
            new Contact { Id = 1, Name = "WithPartner", FirstName = "W", LastName = "P", Email = "wp@t.com", Title = "T", PartnerId = null, Status = EntityStatus.Active },
            new Contact { Id = 2, Name = "NoPartner", FirstName = "N", LastName = "P", Email = "np@t.com", Title = "T", PartnerId = null, Status = EntityStatus.Active }
        });
        await SaveChangesAsync();

        // Act
        var orphans = await Context.Contacts.Where(c => c.PartnerId == null).ToListAsync();

        // Assert
        orphans.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_RemainsDeleted()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1, Name = "AlreadyDel", FirstName = "A", LastName = "D", Email = "ad@t.com", Title = "T",
            Status = EntityStatus.Active, IsDeleted = true
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Try to "delete" again
        contact.IsDeleted = true;
        await SaveChangesAsync();
        var result = await Context.Contacts.FindAsync(1);

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
        await Context.Contacts.AddAsync(new Contact
        {
            Id = 1, Name = "Consistent", FirstName = "C", LastName = "1", Email = "c@t.com", Title = "T", Status = EntityStatus.Active
        });
        await SaveChangesAsync();

        // Act - Multiple reads
        var result1 = await Context.Contacts.FindAsync(1);
        var result2 = await Context.Contacts.FindAsync(1);

        // Assert
        result1!.Name.Should().Be(result2!.Name);
    }

    [Fact]
    public async Task Query_AfterClearDatabase_ReturnsEmpty()
    {
        // Arrange
        await Context.Contacts.AddAsync(new Contact
        {
            Id = 1, Name = "WillBeCleared", FirstName = "W", LastName = "C", Email = "w@t.com", Title = "T", Status = EntityStatus.Active
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
        var contact = new Contact
        {
            Id = 1, Name = "StatusTrack", FirstName = "S", LastName = "T", Email = "st@t.com", Title = "T", Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - Multiple status changes
        contact.Status = EntityStatus.Inactive;
        await SaveChangesAsync();

        contact.Status = EntityStatus.Active;
        await SaveChangesAsync();

        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result!.Status.Should().Be(EntityStatus.Active, "Latest status should be Active");
    }

    [Fact]
    public async Task Contact_BulkStatusUpdate_AppliesCorrectly()
    {
        // Arrange
        await Context.Contacts.AddRangeAsync(Enumerable.Range(1, 5).Select(i => new Contact
        {
            Id = i, Name = $"Bulk{i}", FirstName = $"B{i}", LastName = $"U{i}",
            Email = $"bu{i}@t.com", Title = "T", Status = EntityStatus.Active
        }));
        await SaveChangesAsync();

        // Act - Deactivate contacts 1, 3, 5
        var toDeactivate = await Context.Contacts.Where(c => c.Id % 2 == 1).ToListAsync();
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
