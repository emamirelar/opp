using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for ContactManager
/// </summary>
public class ContactManagerTests : ManagerTestBase
{
    [Fact]
    public async Task GetContactById_Should_ReturnContact_When_Exists()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetContactById_Should_ReturnNull_When_NotExists()
    {
        // Act
        var result = await Context.Contacts.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContacts_Should_ReturnAllContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new() { Id = 1, Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "john@test.com", Title = "Manager", Status = EntityStatus.Active },
            new() { Id = 2, Name = "Jane Smith", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", Title = "Director", Status = EntityStatus.Active }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateContact_Should_PersistContact()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "New Contact",
            FirstName = "New",
            LastName = "Contact",
            Email = "new@test.com",
            Title = "Analyst",
            Status = EntityStatus.Active
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Assert
        var result = await Context.Contacts.FindAsync(1);
        result.Should().NotBeNull();
        result!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateContact_Should_UpdateFields()
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

        // Act
        contact.FirstName = "Updated";
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Contacts.FindAsync(1);
        result!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteContact_Should_SoftDelete()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "ToDelete Contact",
            FirstName = "ToDelete",
            LastName = "Contact",
            Email = "delete@test.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        contact.IsDeleted = true;
        contact.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Contacts.FindAsync(1);
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetContactsByPartner_Should_ReturnFilteredContacts()
    {
        // Arrange
        var partner = new Partner { Id = 1, Name = "Test Partner", Status = EntityStatus.Active };
        await Context.Partners.AddAsync(partner);
        
        var contacts = new List<Contact>
        {
            new() { Id = 1, Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "john@test.com", Title = "Manager", PartnerId = 1, Status = EntityStatus.Active },
            new() { Id = 2, Name = "Jane Smith", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", Title = "Director", PartnerId = 1, Status = EntityStatus.Active },
            new() { Id = 3, Name = "Bob Wilson", FirstName = "Bob", LastName = "Wilson", Email = "bob@test.com", Title = "Analyst", PartnerId = 2, Status = EntityStatus.Active }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.Where(c => c.PartnerId == 1).ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchContacts_Should_FilterByName()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new() { Id = 1, Name = "John Doe", FirstName = "John", LastName = "Doe", Email = "john@test.com", Title = "Manager", Status = EntityStatus.Active },
            new() { Id = 2, Name = "Jane Doe", FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", Title = "Director", Status = EntityStatus.Active },
            new() { Id = 3, Name = "Bob Smith", FirstName = "Bob", LastName = "Smith", Email = "bob@test.com", Title = "Analyst", Status = EntityStatus.Active }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.Where(c => c.LastName == "Doe").ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}

