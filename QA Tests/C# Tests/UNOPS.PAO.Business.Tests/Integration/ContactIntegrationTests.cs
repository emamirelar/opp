using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Integration;

/// <summary>
/// Integration tests for Contact management
/// </summary>
public class ContactIntegrationTests : IntegrationTestBase
{
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

        // Create interaction-contact relationships
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
}

