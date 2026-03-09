/**
 * @fileoverview Fast standalone tests for entity status and stage transition rules
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for entity status and stage transition validation logic
/// </summary>
public class EntityStatusTransitionTests
{
    public enum EntityStage
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Closed = 3,
        Cancelled = 4
    }

    public enum OpportunityStage
    {
        Identification = 0,
        Assessment = 1,
        Pipeline = 2,
        Approved = 3,
        Closed = 4,
        Cancelled = 5
    }

    private static readonly HashSet<(EntityStage From, EntityStage To)> EntityStageTransitions = new()
    {
        (EntityStage.Draft, EntityStage.Active),
        (EntityStage.Draft, EntityStage.Cancelled),
        (EntityStage.Active, EntityStage.Inactive),
        (EntityStage.Active, EntityStage.Closed),
        (EntityStage.Inactive, EntityStage.Active),
        (EntityStage.Inactive, EntityStage.Closed),
        (EntityStage.Inactive, EntityStage.Cancelled)
    };

    private static readonly HashSet<(OpportunityStage From, OpportunityStage To)> OpportunityStageTransitions = new()
    {
        (OpportunityStage.Identification, OpportunityStage.Assessment),
        (OpportunityStage.Identification, OpportunityStage.Cancelled),
        (OpportunityStage.Assessment, OpportunityStage.Pipeline),
        (OpportunityStage.Assessment, OpportunityStage.Cancelled),
        (OpportunityStage.Pipeline, OpportunityStage.Approved),
        (OpportunityStage.Pipeline, OpportunityStage.Cancelled),
        (OpportunityStage.Approved, OpportunityStage.Closed),
        (OpportunityStage.Approved, OpportunityStage.Cancelled)
    };

    private static readonly HashSet<EntityStage> EntityTerminalStages = new()
    {
        EntityStage.Closed,
        EntityStage.Cancelled
    };

    private static readonly HashSet<OpportunityStage> OpportunityTerminalStages = new()
    {
        OpportunityStage.Closed,
        OpportunityStage.Cancelled
    };

    private static bool IsEntityTransitionAllowed(EntityStage from, EntityStage to)
    {
        if (from == to) return false;
        if (EntityTerminalStages.Contains(from)) return false;
        return EntityStageTransitions.Contains((from, to));
    }

    private static bool IsOpportunityTransitionAllowed(OpportunityStage from, OpportunityStage to)
    {
        if (from == to) return false;
        if (OpportunityTerminalStages.Contains(from)) return false;
        return OpportunityStageTransitions.Contains((from, to));
    }

    // --- Valid transitions produce correct results (6 tests) ---

    [Fact]
    public void EntityStage_DraftToActive_Allowed()
    {
        IsEntityTransitionAllowed(EntityStage.Draft, EntityStage.Active).Should().BeTrue();
    }

    [Fact]
    public void EntityStage_ActiveToInactive_Allowed()
    {
        IsEntityTransitionAllowed(EntityStage.Active, EntityStage.Inactive).Should().BeTrue();
    }

    [Fact]
    public void EntityStage_InactiveToActive_Allowed()
    {
        IsEntityTransitionAllowed(EntityStage.Inactive, EntityStage.Active).Should().BeTrue();
    }

    [Fact]
    public void OpportunityStage_IdentificationToAssessment_Allowed()
    {
        IsOpportunityTransitionAllowed(OpportunityStage.Identification, OpportunityStage.Assessment).Should().BeTrue();
    }

    [Fact]
    public void OpportunityStage_PipelineToApproved_Allowed()
    {
        IsOpportunityTransitionAllowed(OpportunityStage.Pipeline, OpportunityStage.Approved).Should().BeTrue();
    }

    [Fact]
    public void OpportunityStage_ApprovedToClosed_Allowed()
    {
        IsOpportunityTransitionAllowed(OpportunityStage.Approved, OpportunityStage.Closed).Should().BeTrue();
    }

    // --- Invalid transitions are rejected (6 tests) ---

    [Fact]
    public void EntityStage_DraftToClosed_Rejected()
    {
        IsEntityTransitionAllowed(EntityStage.Draft, EntityStage.Closed).Should().BeFalse();
    }

    [Fact]
    public void EntityStage_ActiveToDraft_Rejected()
    {
        IsEntityTransitionAllowed(EntityStage.Active, EntityStage.Draft).Should().BeFalse();
    }

    [Fact]
    public void EntityStage_ClosedToActive_Rejected()
    {
        IsEntityTransitionAllowed(EntityStage.Closed, EntityStage.Active).Should().BeFalse();
    }

    [Fact]
    public void OpportunityStage_IdentificationToPipeline_Rejected()
    {
        IsOpportunityTransitionAllowed(OpportunityStage.Identification, OpportunityStage.Pipeline).Should().BeFalse();
    }

    [Fact]
    public void OpportunityStage_AssessmentToIdentification_Rejected()
    {
        IsOpportunityTransitionAllowed(OpportunityStage.Assessment, OpportunityStage.Identification).Should().BeFalse();
    }

    [Fact]
    public void OpportunityStage_ClosedToApproved_Rejected()
    {
        IsOpportunityTransitionAllowed(OpportunityStage.Closed, OpportunityStage.Approved).Should().BeFalse();
    }

    // --- Terminal states cannot transition (3 tests) ---

    [Fact]
    public void EntityStage_ClosedCannotTransition()
    {
        foreach (var to in Enum.GetValues<EntityStage>())
        {
            IsEntityTransitionAllowed(EntityStage.Closed, to).Should().BeFalse(
                $"Closed is terminal, cannot transition to {to}");
        }
    }

    [Fact]
    public void EntityStage_CancelledCannotTransition()
    {
        foreach (var to in Enum.GetValues<EntityStage>())
        {
            IsEntityTransitionAllowed(EntityStage.Cancelled, to).Should().BeFalse(
                $"Cancelled is terminal, cannot transition to {to}");
        }
    }

    [Fact]
    public void OpportunityStage_ClosedAndCancelledCannotTransition()
    {
        foreach (var from in OpportunityTerminalStages)
        {
            foreach (var to in Enum.GetValues<OpportunityStage>())
            {
                IsOpportunityTransitionAllowed(from, to).Should().BeFalse(
                    $"{from} is terminal, cannot transition to {to}");
            }
        }
    }

    // --- Self-transitions are not allowed (2 tests) ---

    [Fact]
    public void EntityStage_SelfTransitionNotAllowed()
    {
        foreach (var stage in Enum.GetValues<EntityStage>())
        {
            IsEntityTransitionAllowed(stage, stage).Should().BeFalse(
                $"self-transition from {stage} to {stage} should not be allowed");
        }
    }

    [Fact]
    public void OpportunityStage_SelfTransitionNotAllowed()
    {
        foreach (var stage in Enum.GetValues<OpportunityStage>())
        {
            IsOpportunityTransitionAllowed(stage, stage).Should().BeFalse(
                $"self-transition from {stage} to {stage} should not be allowed");
        }
    }

    // --- All stages are reachable from initial state via valid path (1 test) ---

    [Fact]
    public void OpportunityStage_AllStagesReachableFromIdentification()
    {
        var reachable = new HashSet<OpportunityStage> { OpportunityStage.Identification };
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var (from, to) in OpportunityStageTransitions)
            {
                if (reachable.Contains(from) && !reachable.Contains(to))
                {
                    reachable.Add(to);
                    changed = true;
                }
            }
        }

        var allStages = Enum.GetValues<OpportunityStage>().ToHashSet();
        reachable.Should().BeEquivalentTo(allStages,
            "all opportunity stages must be reachable from Identification via valid transitions");
    }
}
