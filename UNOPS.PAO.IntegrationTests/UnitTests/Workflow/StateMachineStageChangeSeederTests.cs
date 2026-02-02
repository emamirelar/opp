using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Seeders;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

/// <summary>
/// Unit tests for StateMachineStageChangeSeeder.
/// Uses InMemory database to test seeding logic.
/// Tests all 5 transitions: Go, No Go, Reopen from No Go, Cancel, and Reopen from Cancelled.
/// </summary>
public class StateMachineStageChangeSeederTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly WorkflowDbContext _workflowContext;

    public StateMachineStageChangeSeederTests()
    {
        // Setup in-memory database for WorkflowDbContext
        var options = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase(databaseName: $"WorkflowTest_{Guid.NewGuid()}")
            .Options;

        _workflowContext = new WorkflowDbContext(options);

        // Setup service collection
        var services = new ServiceCollection();
        services.AddSingleton(_workflowContext);
        services.AddSingleton<ILogger<WorkflowDbContext>>(new Mock<ILogger<WorkflowDbContext>>().Object);

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _workflowContext.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldCreateFiveTransitions()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transitions = await _workflowContext.StateMachineStageChanges.ToListAsync();
        transitions.Should().HaveCount(5);
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldCreateIdentifyToGoTransition()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transition = await _workflowContext.StateMachineStageChanges
            .FirstOrDefaultAsync(x => 
                x.EntityName == "Opportunity" &&
                x.FromStage == "IDENTIFY & PROFILE" && 
                x.ToStage == "GO");

        transition.Should().NotBeNull();
        transition!.ApprovalRequired.Should().BeTrue();
        transition.CommentRequired.Should().BeTrue();
        transition.Name.Should().Be("Submit for Go");
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldCreateIdentifyToNoGoTransition()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transition = await _workflowContext.StateMachineStageChanges
            .FirstOrDefaultAsync(x => 
                x.EntityName == "Opportunity" &&
                x.FromStage == "IDENTIFY & PROFILE" && 
                x.ToStage == "NO GO");

        transition.Should().NotBeNull();
        transition!.ApprovalRequired.Should().BeTrue();
        transition.CommentRequired.Should().BeTrue();
        transition.Name.Should().Be("Submit for No Go");
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldCreateReopenFromNoGoTransition()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transition = await _workflowContext.StateMachineStageChanges
            .FirstOrDefaultAsync(x => 
                x.EntityName == "Opportunity" &&
                x.FromStage == "NO GO" && 
                x.ToStage == "IDENTIFY & PROFILE");

        transition.Should().NotBeNull();
        transition!.ApprovalRequired.Should().BeFalse();
        transition.CommentRequired.Should().BeFalse();
        transition.CommentOptional.Should().BeTrue();
        transition.Name.Should().Be("Reopen");
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldCreateCancelTransition()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transition = await _workflowContext.StateMachineStageChanges
            .FirstOrDefaultAsync(x => 
                x.EntityName == "Opportunity" &&
                x.FromStage == "IDENTIFY & PROFILE" && 
                x.ToStage == "CANCELLED");

        transition.Should().NotBeNull();
        transition!.ApprovalRequired.Should().BeFalse();
        transition.CommentRequired.Should().BeTrue();
        transition.CommentOptional.Should().BeFalse();
        transition.Name.Should().Be("Cancel");
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldCreateReopenFromCancelledTransition()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transition = await _workflowContext.StateMachineStageChanges
            .FirstOrDefaultAsync(x => 
                x.EntityName == "Opportunity" &&
                x.FromStage == "CANCELLED" && 
                x.ToStage == "IDENTIFY & PROFILE");

        transition.Should().NotBeNull();
        transition!.ApprovalRequired.Should().BeFalse();
        transition.CommentRequired.Should().BeTrue();
        transition.CommentOptional.Should().BeFalse();
        transition.Name.Should().Be("Reopen");
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldBeIdempotent()
    {
        // Act - Run seeder twice
        await _serviceProvider.SeedStateMachineStageChangesAsync();
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert - Should still have exactly 5 transitions
        var transitions = await _workflowContext.StateMachineStageChanges.ToListAsync();
        transitions.Should().HaveCount(5);
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_TransitionsWithApproval_ShouldHaveCorrectFlags()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transitionsRequiringApproval = await _workflowContext.StateMachineStageChanges
            .Where(x => x.ApprovalRequired)
            .ToListAsync();

        transitionsRequiringApproval.Should().HaveCount(2);
        transitionsRequiringApproval.Should().AllSatisfy(t =>
        {
            t.FromStage.Should().Be("IDENTIFY & PROFILE");
            t.CommentRequired.Should().BeTrue();
        });
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_AllTransitions_ShouldBeInternal()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transitions = await _workflowContext.StateMachineStageChanges.ToListAsync();
        transitions.Should().AllSatisfy(t =>
        {
            t.Internal.Should().BeTrue();
            t.External.Should().BeFalse();
        });
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_AllTransitions_ShouldHaveActiveStatus()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transitions = await _workflowContext.StateMachineStageChanges.ToListAsync();
        transitions.Should().AllSatisfy(t =>
        {
            t.Status.Should().Be(EntityStatus.Active);
        });
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_AllTransitions_ShouldHaveCorrectEntityName()
    {
        // Act
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert
        var transitions = await _workflowContext.StateMachineStageChanges.ToListAsync();
        transitions.Should().AllSatisfy(t =>
        {
            t.EntityName.Should().Be(OpportunityWorkflow.EntityName);
        });
    }

    [Fact]
    public async Task SeedStateMachineStageChangesAsync_ShouldReactivateDeletedTransitions()
    {
        // Arrange - First seed, then soft-delete a transition
        await _serviceProvider.SeedStateMachineStageChangesAsync();
        
        var transition = await _workflowContext.StateMachineStageChanges
            .FirstAsync(x => x.ToStage == "GO");
        transition.IsDeleted = true;
        await _workflowContext.SaveChangesAsync();

        // Act - Run seeder again
        await _serviceProvider.SeedStateMachineStageChangesAsync();

        // Assert - Transition should be reactivated
        var reactivated = await _workflowContext.StateMachineStageChanges
            .FirstAsync(x => x.ToStage == "GO");
        reactivated.IsDeleted.Should().BeFalse();
    }
}
