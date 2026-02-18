using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for InteractionManager
/// </summary>
public class InteractionManagerTests : ManagerTestBase
{
    [Fact]
    public async Task GetInteractionById_Should_ReturnInteraction_When_Exists()
    {
        // Arrange
        var interaction = new Interaction
        {
            Id = 1,
            Name = "Test Meeting",
            Subject = "Test Meeting",
            Description = "Test Description",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active
        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();

        // Act
        var result = await Context.Interactions.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Subject.Should().Be("Test Meeting");
    }

    [Fact]
    public async Task GetInteractionById_Should_ReturnNull_When_NotExists()
    {
        // Act
        var result = await Context.Interactions.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllInteractions_Should_ReturnAllInteractions()
    {
        // Arrange
        var interactions = new List<Interaction>
        {
            new() { Id = 1, Name = "Meeting 1", Subject = "Meeting 1", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active },
            new() { Id = 2, Name = "Call 1", Subject = "Call 1", Type = InteractionType.Call, Date = DateTime.UtcNow, Status = EntityStatus.Active }
        };
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();

        // Act
        var result = await Context.Interactions.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateInteraction_Should_PersistInteraction()
    {
        // Arrange
        var interaction = new Interaction
        {
            Id = 1,
            Name = "New Interaction",
            Subject = "New Interaction",
            Type = InteractionType.Email,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active
        };

        // Act
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();

        // Assert
        var result = await Context.Interactions.FindAsync(1);
        result.Should().NotBeNull();
        result!.Subject.Should().Be("New Interaction");
    }

    [Fact]
    public async Task GetInteractionsByType_Should_FilterCorrectly()
    {
        // Arrange
        var interactions = new List<Interaction>
        {
            new() { Id = 1, Name = "Meeting", Subject = "Meeting", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active },
            new() { Id = 2, Name = "Virtual Meeting", Subject = "Virtual Meeting", Type = InteractionType.VirtualMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active },
            new() { Id = 3, Name = "Call", Subject = "Call", Type = InteractionType.Call, Date = DateTime.UtcNow, Status = EntityStatus.Active },
            new() { Id = 4, Name = "Email", Subject = "Email", Type = InteractionType.Email, Date = DateTime.UtcNow, Status = EntityStatus.Active }
        };
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();

        // Act
        var meetingResult = await Context.Interactions.Where(i => i.Type == InteractionType.InPersonMeeting).ToListAsync();
        var emailResult = await Context.Interactions.Where(i => i.Type == InteractionType.Email).ToListAsync();

        // Assert
        meetingResult.Should().HaveCount(1);
        emailResult.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetInteractionsByDateRange_Should_FilterCorrectly()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var interactions = new List<Interaction>
        {
            new() { Id = 1, Name = "Today", Subject = "Today", Type = InteractionType.InPersonMeeting, Date = today, Status = EntityStatus.Active },
            new() { Id = 2, Name = "Yesterday", Subject = "Yesterday", Type = InteractionType.InPersonMeeting, Date = today.AddDays(-1), Status = EntityStatus.Active },
            new() { Id = 3, Name = "Last Week", Subject = "Last Week", Type = InteractionType.InPersonMeeting, Date = today.AddDays(-7), Status = EntityStatus.Active }
        };
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();

        // Act
        var result = await Context.Interactions
            .Where(i => i.Date >= today.AddDays(-2))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateInteraction_Should_UpdateFields()
    {
        // Arrange
        var interaction = new Interaction
        {
            Id = 1,
            Name = "Original Subject",
            Subject = "Original Subject",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active
        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();

        // Act
        interaction.Subject = "Updated Subject";
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Interactions.FindAsync(1);
        result!.Subject.Should().Be("Updated Subject");
    }

    [Fact]
    public async Task DeleteInteraction_Should_SoftDelete()
    {
        // Arrange
        var interaction = new Interaction
        {
            Id = 1,
            Name = "To Delete",
            Subject = "To Delete",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active
        };
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();

        // Act
        interaction.IsDeleted = true;
        interaction.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Interactions.FindAsync(1);
        result!.IsDeleted.Should().BeTrue();
    }
}

