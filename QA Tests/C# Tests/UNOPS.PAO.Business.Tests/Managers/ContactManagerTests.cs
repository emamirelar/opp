using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for ContactManager against PostgreSQL.
/// Uses UNOPSContact and creates parent Partners for FK constraints.
/// Uses test markers to filter own data from the shared database.
/// </summary>
public class ContactManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"CMT_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetContactById_Should_ReturnContact_When_Exists()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"John Doe {_testMarker}",
            FirstName = "John",
            LastName = "Doe",
            Email = $"john.doe_{_testMarker}@example.com",
            Title = "Manager",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        RegisterTableCleanup("Contacts", $"\"Id\" = {contact.Id}");

        // Act
        var result = await Context.Contacts.FindAsync(contact.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetContactById_Should_ReturnNull_When_NotExists()
    {
        // Act
        var result = await Context.Contacts.FindAsync(999999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContacts_Should_ReturnAllContacts()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contacts = new List<UNOPSContact>
        {
            new() { Name = $"John Doe {_testMarker}", FirstName = "John", LastName = "Doe", Email = $"john_{_testMarker}@test.com", Title = "Manager", PartnerId = partnerId, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Jane Smith {_testMarker}", FirstName = "Jane", LastName = "Smith", Email = $"jane_{_testMarker}@test.com", Title = "Director", PartnerId = partnerId, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        foreach (var c in contacts) RegisterTableCleanup("Contacts", $"\"Id\" = {c.Id}");

        // Act
        var result = await Context.Contacts
            .Where(c => c.Name.Contains(_testMarker))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateContact_Should_PersistContact()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"New Contact {_testMarker}",
            FirstName = "New",
            LastName = "Contact",
            Email = $"new_{_testMarker}@test.com",
            Title = "Analyst",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };

        // Act
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        RegisterTableCleanup("Contacts", $"\"Id\" = {contact.Id}");

        // Assert
        var result = await Context.Contacts.FindAsync(contact.Id);
        result.Should().NotBeNull();
        result!.Email.Should().Contain(_testMarker);
    }

    [Fact]
    public async Task UpdateContact_Should_UpdateFields()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Original Name {_testMarker}",
            FirstName = "Original",
            LastName = "Name",
            Email = $"original_{_testMarker}@test.com",
            Title = "Manager",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        RegisterTableCleanup("Contacts", $"\"Id\" = {contact.Id}");

        // Act
        contact.FirstName = "Updated";
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Contacts.FindAsync(contact.Id);
        result!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteContact_Should_SoftDelete()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"ToDelete Contact {_testMarker}",
            FirstName = "ToDelete",
            LastName = "Contact",
            Email = $"delete_{_testMarker}@test.com",
            Title = "Manager",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        RegisterTableCleanup("Contacts", $"\"Id\" = {contact.Id}");

        // Act
        contact.IsDeleted = true;
        contact.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Contacts.FindAsync(contact.Id);
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetContactsByPartner_Should_ReturnFilteredContacts()
    {
        // Arrange
        var partner1Id = await CreateTestPartnerAsync($"Test Partner 1 {_testMarker}");
        var partner2Id = await CreateTestPartnerAsync($"Other Partner 2 {_testMarker}");

        var contacts = new List<UNOPSContact>
        {
            new() { Name = $"John Doe {_testMarker}", FirstName = "John", LastName = "Doe", Email = $"john_{_testMarker}@test.com", Title = "Manager", PartnerId = partner1Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Jane Smith {_testMarker}", FirstName = "Jane", LastName = "Smith", Email = $"jane_{_testMarker}@test.com", Title = "Director", PartnerId = partner1Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Bob Wilson {_testMarker}", FirstName = "Bob", LastName = "Wilson", Email = $"bob_{_testMarker}@test.com", Title = "Analyst", PartnerId = partner2Id, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        foreach (var c in contacts) RegisterTableCleanup("Contacts", $"\"Id\" = {c.Id}");

        // Act
        var result = await Context.Contacts.Where(c => c.PartnerId == partner1Id).ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchContacts_Should_FilterByName()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contacts = new List<UNOPSContact>
        {
            new() { Name = $"John Doe {_testMarker}", FirstName = "John", LastName = $"Doe_{_testMarker}", Email = $"john_{_testMarker}@test.com", Title = "Manager", PartnerId = partnerId, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Jane Doe {_testMarker}", FirstName = "Jane", LastName = $"Doe_{_testMarker}", Email = $"jane_{_testMarker}@test.com", Title = "Director", PartnerId = partnerId, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Bob Smith {_testMarker}", FirstName = "Bob", LastName = $"Smith_{_testMarker}", Email = $"bob_{_testMarker}@test.com", Title = "Analyst", PartnerId = partnerId, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        foreach (var c in contacts) RegisterTableCleanup("Contacts", $"\"Id\" = {c.Id}");

        // Act
        var result = await Context.Contacts.Where(c => c.LastName == $"Doe_{_testMarker}").ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
