using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.EdgeCases;

/// <summary>
/// Edge case tests for Contact operations
/// </summary>
public class ContactEdgeCaseTests : ManagerTestBase
{
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
        var longName = new string('A', 255);
        var contact = new Contact
        {
            Id = 1,
            Name = "Long Name Test",
            FirstName = longName,
            LastName = "Doe",
            Email = "test@test.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Length.Should().Be(255);
    }

    [Fact]
    public async Task Contact_WithUnicodeCharacters_Should_BeHandled()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "Unicode Contact",
            FirstName = "联系人 🧑",
            LastName = "Контакт",
            Email = "unicode@test.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Contain("🧑");
    }

    [Fact]
    public async Task Contact_WithEmptyOptionalFields_Should_BeCreated()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "Empty Fields Test",
            FirstName = null,
            LastName = "Doe",
            Email = "test@test.com",
            Title = "Manager",
            Phone = null,
            Mobile = null,
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Phone.Should().BeNull();
    }

    [Fact]
    public async Task GetContacts_EmptyDatabase_Should_ReturnEmpty()
    {
        // Act
        var result = await Context.Contacts.ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Contact_WithSpecialCharactersInEmail_Should_BeHandled()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john+special.chars_test@sub.example-domain.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Contain("+");
    }

    [Fact]
    public async Task SearchContacts_WithNoMatches_Should_ReturnEmpty()
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
        var result = await Context.Contacts.Where(c => c.FirstName == "NonExistent").ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeletedContact_Should_BeExcludedFromActiveQueries()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new() { Id = 1, Name = "Active User", FirstName = "Active", LastName = "User", Email = "active@test.com", Title = "Manager", Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 2, Name = "Deleted User", FirstName = "Deleted", LastName = "User", Email = "deleted@test.com", Title = "Manager", Status = EntityStatus.Active, IsDeleted = true }
        };
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var result = await Context.Contacts.Where(c => !c.IsDeleted).ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().FirstName.Should().Be("Active");
    }
}

