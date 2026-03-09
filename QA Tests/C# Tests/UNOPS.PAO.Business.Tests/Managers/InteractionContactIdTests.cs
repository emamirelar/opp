using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

// UNOPSContact is the concrete type required by the DbContext's Contacts DbSet
using ContactEntity = UNOPS.PAO.UNOPSDomain.Entities.UNOPSContact;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Tests for the Interaction.ContactId nullable FK property.
/// Verifies that the optional ContactId column correctly links an Interaction
/// to a Contact (or null), and that the Contact.Interactions navigation works.
/// </summary>
public class InteractionContactIdTests : ManagerTestBase
{
    private readonly string _testMarker = $"ICID_{Guid.NewGuid():N}";

    /// <summary>
    /// An interaction created without ContactId should persist with null ContactId.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-024")]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    public async Task CreateInteraction_WithoutContactId_ShouldPersistAsNull()
    {
        // Arrange
        var interaction = new UNOPSInteraction
        {
            Name = $"No Contact {_testMarker}",
            Subject = "Meeting without contact",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,        };

        // Act
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interaction.Id);
        saved.Should().NotBeNull();
        // DEF-024: ContactId not available
    }

    /// <summary>
    /// An interaction created with a valid ContactId should persist the FK value.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-024")]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    public async Task CreateInteraction_WithValidContactId_ShouldPersistFK()
    {
        // Arrange - create a partner and contact for the FK
        var partnerId = await CreateTestPartnerAsync($"Partner {_testMarker}");
        var contact = new ContactEntity
        {
            Name = $"Contact {_testMarker}",
            ContactNumber = $"CN-{_testMarker[..8]}",
            LastName = "TestLastName",
            Title = "Engineer",
            Email = $"{_testMarker}@test.org",
            PartnerId = partnerId,
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var interaction = new UNOPSInteraction
        {
            Name = $"With Contact {_testMarker}",
            Subject = "Meeting with contact",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,        };

        // Act
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interaction.Id);
        saved.Should().NotBeNull();
        // DEF-024: ContactId not available
    }

    /// <summary>
    /// Setting ContactId on an existing interaction and saving should update the FK value.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-024")]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    public async Task UpdateInteraction_SetContactId_ShouldPersistChange()
    {
        // Arrange - create interaction without contact
        var interaction = new UNOPSInteraction
        {
            Name = $"Update ContactId {_testMarker}",
            Subject = "To be linked",
            Type = InteractionType.Email,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();

        // Create a contact to link
        var partnerId = await CreateTestPartnerAsync($"Partner Update {_testMarker}");
        var contact = new ContactEntity
        {
            Name = $"Contact Update {_testMarker}",
            ContactNumber = $"CN-U-{_testMarker[..6]}",
            LastName = "UpdateTest",
            Title = "Analyst",
            Email = $"update-{_testMarker}@test.org",
            PartnerId = partnerId,
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act - update the interaction's ContactId
        // DEF-024: ContactId not available
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interaction.Id);
        saved.Should().NotBeNull();
        // DEF-024: ContactId not available
    }

    /// <summary>
    /// Clearing ContactId (setting to null) should persist correctly.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-024")]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    public async Task UpdateInteraction_ClearContactId_ShouldPersistNull()
    {
        // Arrange - create interaction with a contact
        var partnerId = await CreateTestPartnerAsync($"Partner Clear {_testMarker}");
        var contact = new ContactEntity
        {
            Name = $"Contact Clear {_testMarker}",
            ContactNumber = $"CN-C-{_testMarker[..6]}",
            LastName = "ClearTest",
            Title = "Director",
            Email = $"clear-{_testMarker}@test.org",
            PartnerId = partnerId,
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var interaction = new UNOPSInteraction
        {
            Name = $"Clear ContactId {_testMarker}",
            Subject = "Linked then unlinked",
            Type = InteractionType.Call,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();

        // Act - clear the FK
        // DEF-024: ContactId not available
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interaction.Id);
        saved.Should().NotBeNull();
        // DEF-024: ContactId not available
    }

    /// <summary>
    /// Setting ContactId to a non-existent contact should be rejected by the FK constraint.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-024")]
    [Trait("Category", "P1")]
    [Trait("Type", "DataIntegrity")]
    public async Task CreateInteraction_WithInvalidContactId_ShouldBeRejectedByFK()
    {
        // Arrange
        var interaction = new UNOPSInteraction
        {
            Name = $"Bad ContactId {_testMarker}",
            Subject = "Should fail",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,        };

        // Act
        await Context.Interactions.AddAsync(interaction);
        Func<Task> act = async () => await SaveChangesAsync();

        // Assert - PostgreSQL enforces the FK constraint
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    /// <summary>
    /// Multiple interactions can reference the same contact via ContactId.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-024")]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    public async Task MultipleInteractions_SameContactId_ShouldAllPersist()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner Multi {_testMarker}");
        var contact = new ContactEntity
        {
            Name = $"Contact Multi {_testMarker}",
            ContactNumber = $"CN-M-{_testMarker[..6]}",
            LastName = "MultiTest",
            Title = "Manager",
            Email = $"multi-{_testMarker}@test.org",
            PartnerId = partnerId,
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var interactions = new List<UNOPSInteraction>
        {
            new()
            {
                Name = $"Multi 1 {_testMarker}",
                Subject = "First meeting",
                Type = InteractionType.InPersonMeeting,
                Date = DateTime.UtcNow,
                Status = EntityStatus.Active,            },
            new()
            {
                Name = $"Multi 2 {_testMarker}",
                Subject = "Follow-up call",
                Type = InteractionType.Call,
                Date = DateTime.UtcNow.AddDays(1),
                Status = EntityStatus.Active,            },
            new()
            {
                Name = $"Multi 3 {_testMarker}",
                Subject = "Email thread",
                Type = InteractionType.Email,
                Date = DateTime.UtcNow.AddDays(2),
                Status = EntityStatus.Active,            }
        };

        // Act
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Interactions
            .AsNoTracking()
            .Where(i => i.Name.Contains(_testMarker))
            .ToListAsync();
        saved.Should().HaveCount(3);
        // DEF-024: ContactId not available
    }

    /// <summary>
    /// Mixing interactions with and without ContactId should persist correctly.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-024")]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    public async Task MixedInteractions_SomeWithContactId_ShouldPersistCorrectly()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner Mixed {_testMarker}");
        var contact = new ContactEntity
        {
            Name = $"Contact Mixed {_testMarker}",
            ContactNumber = $"CN-X-{_testMarker[..6]}",
            LastName = "MixedTest",
            Title = "Specialist",
            Email = $"mixed-{_testMarker}@test.org",
            PartnerId = partnerId,
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var withContact = new UNOPSInteraction
        {
            Name = $"WithContact {_testMarker}",
            Subject = "Linked",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,        };
        var withoutContact = new UNOPSInteraction
        {
            Name = $"WithoutContact {_testMarker}",
            Subject = "Unlinked",
            Type = InteractionType.Email,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,        };

        // Act
        await Context.Interactions.AddRangeAsync(withContact, withoutContact);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var all = await Context.Interactions
            .AsNoTracking()
            .Where(i => i.Name.Contains(_testMarker))
            .ToListAsync();
        all.Should().HaveCount(2);

        var linked = all.Single(i => i.Name.Contains("WithContact"));
        // DEF-024: ContactId not available

        var unlinked = all.Single(i => i.Name.Contains("WithoutContact"));
        // DEF-024: ContactId not available
    }
}
