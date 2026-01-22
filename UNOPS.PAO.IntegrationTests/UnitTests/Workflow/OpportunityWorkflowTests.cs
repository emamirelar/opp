using FluentAssertions;
using UNOPS.PAO.Business.Workflow;
using UNOPS.Workflow.Models;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

/// <summary>
/// Unit tests for OpportunityWorkflow state machine configuration.
/// </summary>
public class OpportunityWorkflowTests
{
    [Fact]
    public void StateMachine_ShouldHaveCorrectEntityType()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        stateMachine.EntityType.Should().Be("Opportunity");
    }

    [Fact]
    public void StateMachine_ShouldHaveThreeStates()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        stateMachine.States.Should().HaveCount(3);
    }

    [Fact]
    public void StateMachine_ShouldContainIdentifyAndProfileStage()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        stateMachine.States.Should().Contain(s => s.StageCode == "IDENTIFY & PROFILE");
    }

    [Fact]
    public void StateMachine_ShouldContainGoStage()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        stateMachine.States.Should().Contain(s => s.StageCode == "GO");
    }

    [Fact]
    public void StateMachine_ShouldContainNoGoStage()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        stateMachine.States.Should().Contain(s => s.StageCode == "NO GO");
    }

    [Fact]
    public void StateMachine_StateSequences_ShouldBeCorrect()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        var identifyState = stateMachine.States.First(s => s.StageCode == "IDENTIFY & PROFILE");
        var goState = stateMachine.States.First(s => s.StageCode == "GO");
        var noGoState = stateMachine.States.First(s => s.StageCode == "NO GO");

        identifyState.Sequence.Should().Be(1);
        goState.Sequence.Should().Be(2);
        noGoState.Sequence.Should().Be(3);
    }

    [Fact]
    public void StateMachine_AllStates_ShouldHaveInternalFacing()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        stateMachine.States.Should().AllSatisfy(state =>
        {
            state.Facing.Should().Be(Facing.Internal);
        });
    }

    [Fact]
    public void StateMachine_States_ShouldHaveDisplayNames()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;

        // Assert
        var identifyState = stateMachine.States.First(s => s.StageCode == "IDENTIFY & PROFILE");
        var goState = stateMachine.States.First(s => s.StageCode == "GO");
        var noGoState = stateMachine.States.First(s => s.StageCode == "NO GO");

        identifyState.DisplayName.Should().Be("Identify & Profile");
        goState.DisplayName.Should().Be("Go");
        noGoState.DisplayName.Should().Be("No Go");
    }

    [Fact]
    public void EntityName_ShouldBeOpportunity()
    {
        // Assert
        OpportunityWorkflow.EntityName.Should().Be("Opportunity");
    }

    [Fact]
    public void Stages_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        OpportunityWorkflow.Stages.IdentifyAndProfile.Should().Be("IDENTIFY & PROFILE");
        OpportunityWorkflow.Stages.Go.Should().Be("GO");
        OpportunityWorkflow.Stages.NoGo.Should().Be("NO GO");
    }

    [Fact]
    public void AllStages_ShouldContainAllThreeStages()
    {
        // Assert
        OpportunityWorkflow.AllStages.Should().HaveCount(3);
        OpportunityWorkflow.AllStages.Should().Contain("IDENTIFY & PROFILE");
        OpportunityWorkflow.AllStages.Should().Contain("GO");
        OpportunityWorkflow.AllStages.Should().Contain("NO GO");
    }

    [Theory]
    [InlineData("IDENTIFY & PROFILE", true)]
    [InlineData("GO", true)]
    [InlineData("NO GO", true)]
    [InlineData("Invalid Stage", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidStage_ShouldReturnCorrectResult(string? stage, bool expected)
    {
        // Act
        var result = OpportunityWorkflow.IsValidStage(stage);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StageNames_ShouldMapCorrectly()
    {
        // Arrange & Act
        var stateMachine = OpportunityWorkflow.StateMachine;
        var stageNames = stateMachine.StageNames;

        // Assert
        stageNames.Should().HaveCount(3);
        stageNames["IDENTIFY & PROFILE"].Should().Be("Identify & Profile");
        stageNames["GO"].Should().Be("Go");
        stageNames["NO GO"].Should().Be("No Go");
    }
}
