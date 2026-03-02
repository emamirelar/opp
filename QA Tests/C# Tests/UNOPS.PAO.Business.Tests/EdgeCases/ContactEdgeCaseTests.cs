using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.EdgeCases;

/// <summary>
/// Edge case tests for Contact operations against PostgreSQL.
/// Uses UNOPSContact (TPH derived type) and creates parent Partners for FK constraints.
/// Tests use unique markers to filter own data from the shared database.
/// </summary>
public class ContactEdgeCaseTests : ManagerTestBase
{
    private readonly string _testMarker = $"ECT_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetContactById_WithZeroId_Should_ReturnNull()
    {
        // Act
        var result = await Context.Contacts.FindAsync(0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContactById_WithNegativeId_Should_ReturnNull()
    {
        // Act
        var result = await Context.Contacts.FindAsync(-1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Contact_WithVeryLongName_Should_BeHandled()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var longName = new string('A', 255);
        var contact = new UNOPSContact
        {
            Name = $"Long Name Test {_testMarker}",
            FirstName = longName,
            LastName = "Doe",
            Email = $"longname_{_testMarker}@test.com",
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
        result!.FirstName!.Length.Should().Be(255);
    }

    [Fact]
    public async Task Contact_WithUnicodeCharacters_Should_BeHandled()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Unicode Contact {_testMarker}",
            FirstName = "联系人 🧑",
            LastName = "Контакт",
            Email = $"unicode_{_testMarker}@test.com",
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
        result!.FirstName.Should().Contain("🧑");
    }

    [Fact]
    public async Task Contact_WithEmptyOptionalFields_Should_BeCreated()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Empty Fields Test {_testMarker}",
            FirstName = null,
            LastName = "Doe",
            Email = $"empty_{_testMarker}@test.com",
            Title = "Manager",
            PartnerId = partnerId,
            Phone = null,
            Mobile = null,
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
        result!.Phone.Should().BeNull();
    }

    [Fact]
    public async Task GetContacts_EmptyDatabase_Should_ReturnFilteredEmpty()
    {
        // Act - Query for contacts with a marker that doesn't exist
        var result = await Context.Contacts
            .Where(c => c.Name == "NONEXISTENT_MARKER_THAT_WILL_NEVER_MATCH")
            .ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Contact_WithSpecialCharactersInEmail_Should_BeHandled()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"John Doe {_testMarker}",
            FirstName = "John",
            LastName = "Doe",
            Email = $"john+special.chars_test@sub.example-domain.com",
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
        result!.Email.Should().Contain("+");
    }

    [Fact]
    public async Task SearchContacts_WithNoMatches_Should_ReturnEmpty()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"John Doe {_testMarker}",
            FirstName = "John",
            LastName = "Doe",
            Email = $"john_{_testMarker}@test.com",
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
        var result = await Context.Contacts
            .Where(c => c.FirstName == "NonExistent_ZZZZZ_Marker")
            .ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeletedContact_Should_BeExcludedFromActiveQueries()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var activeContact = new UNOPSContact
        {
            Name = $"Active User {_testMarker}",
            FirstName = $"Active_{_testMarker}",
            LastName = "User",
            Email = $"active_{_testMarker}@test.com",
            Title = "Manager",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            IsDeleted = false,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        var deletedContact = new UNOPSContact
        {
            Name = $"Deleted User {_testMarker}",
            FirstName = $"Deleted_{_testMarker}",
            LastName = "User",
            Email = $"deleted_{_testMarker}@test.com",
            Title = "Manager",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            IsDeleted = true,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddRangeAsync(activeContact, deletedContact);
        await SaveChangesAsync();
        RegisterTableCleanup("Contacts", $"\"Id\" IN ({activeContact.Id}, {deletedContact.Id})");

        // Act - Filter by test marker AND not deleted
        var result = await Context.Contacts
            .Where(c => !c.IsDeleted && c.Name.Contains(_testMarker))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().FirstName.Should().StartWith("Active_");
    }
}
