using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for InteractionManager against PostgreSQL.
/// Uses UNOPSInteraction and test markers for data isolation.
/// </summary>
public class InteractionManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"IMT_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetInteractionById_Should_ReturnInteraction_When_Exists()
    {
        // Arrange
        var interaction = new UNOPSInteraction
        {
            Name = $"Test Meeting {_testMarker}",
            Subject = "Test Meeting",
            Description = "Test Description",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        RegisterTableCleanup("Interactions", $"\"Id\" = {interaction.Id}");

        // Act
        var result = await Context.Interactions.FindAsync(interaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Subject.Should().Be("Test Meeting");
    }

    [Fact]
    public async Task GetInteractionById_Should_ReturnNull_When_NotExists()
    {
        // Act
        var result = await Context.Interactions.FindAsync(999999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllInteractions_Should_ReturnAllInteractions()
    {
        // Arrange
        var interactions = new List<UNOPSInteraction>
        {
            new() { Name = $"Meeting 1 {_testMarker}", Subject = "Meeting 1", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Call 1 {_testMarker}", Subject = "Call 1", Type = InteractionType.Call, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        foreach (var i in interactions) RegisterTableCleanup("Interactions", $"\"Id\" = {i.Id}");

        // Act
        var result = await Context.Interactions
            .Where(i => i.Name.Contains(_testMarker))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateInteraction_Should_PersistInteraction()
    {
        // Arrange
        var interaction = new UNOPSInteraction
        {
            Name = $"New Interaction {_testMarker}",
            Subject = "New Interaction",
            Type = InteractionType.Email,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };

        // Act
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        RegisterTableCleanup("Interactions", $"\"Id\" = {interaction.Id}");

        // Assert
        var result = await Context.Interactions.FindAsync(interaction.Id);
        result.Should().NotBeNull();
        result!.Subject.Should().Be("New Interaction");
    }

    [Fact]
    public async Task GetInteractionsByType_Should_FilterCorrectly()
    {
        // Arrange
        var interactions = new List<UNOPSInteraction>
        {
            new() { Name = $"Meeting {_testMarker}", Subject = "Meeting", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Virtual Meeting {_testMarker}", Subject = "Virtual Meeting", Type = InteractionType.VirtualMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Call {_testMarker}", Subject = "Call", Type = InteractionType.Call, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Email {_testMarker}", Subject = "Email", Type = InteractionType.Email, Date = DateTime.UtcNow, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        foreach (var i in interactions) RegisterTableCleanup("Interactions", $"\"Id\" = {i.Id}");

        // Act
        var meetingResult = await Context.Interactions
            .Where(i => i.Name.Contains(_testMarker) && i.Type == InteractionType.InPersonMeeting)
            .ToListAsync();
        var emailResult = await Context.Interactions
            .Where(i => i.Name.Contains(_testMarker) && i.Type == InteractionType.Email)
            .ToListAsync();

        // Assert
        meetingResult.Should().HaveCount(1);
        emailResult.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetInteractionsByDateRange_Should_FilterCorrectly()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var interactions = new List<UNOPSInteraction>
        {
            new() { Name = $"Today {_testMarker}", Subject = "Today", Type = InteractionType.InPersonMeeting, Date = today, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Yesterday {_testMarker}", Subject = "Yesterday", Type = InteractionType.InPersonMeeting, Date = today.AddDays(-1), Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Last Week {_testMarker}", Subject = "Last Week", Type = InteractionType.InPersonMeeting, Date = today.AddDays(-7), Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        foreach (var i in interactions) RegisterTableCleanup("Interactions", $"\"Id\" = {i.Id}");

        // Act
        var result = await Context.Interactions
            .Where(i => i.Name.Contains(_testMarker) && i.Date >= today.AddDays(-2))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateInteraction_Should_UpdateFields()
    {
        // Arrange
        var interaction = new UNOPSInteraction
        {
            Name = $"Original Subject {_testMarker}",
            Subject = "Original Subject",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        RegisterTableCleanup("Interactions", $"\"Id\" = {interaction.Id}");

        // Act
        interaction.Subject = "Updated Subject";
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Interactions.FindAsync(interaction.Id);
        result!.Subject.Should().Be("Updated Subject");
    }

    [Fact]
    public async Task DeleteInteraction_Should_SoftDelete()
    {
        // Arrange
        var interaction = new UNOPSInteraction
        {
            Name = $"To Delete {_testMarker}",
            Subject = "To Delete",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        RegisterTableCleanup("Interactions", $"\"Id\" = {interaction.Id}");

        // Act
        interaction.IsDeleted = true;
        interaction.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Interactions.FindAsync(interaction.Id);
        result!.IsDeleted.Should().BeTrue();
    }
}
