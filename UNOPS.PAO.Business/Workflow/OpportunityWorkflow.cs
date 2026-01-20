using UNOPS.Workflow.Models;

namespace UNOPS.PAO.Business.Workflow;

/// <summary>
/// Defines the Opportunity workflow state machine with 3 stages.
/// Based on PRD: IDENTIFY & PROFILE → GO or NO GO, with NO GO reopenable.
/// </summary>
public static class OpportunityWorkflow
{
    /// <summary>
    /// The entity name used in workflow configuration.
    /// </summary>
    public const string EntityName = "Opportunity";

    /// <summary>
    /// Stage constants for Opportunity workflow.
    /// </summary>
    public static class Stages
    {
        /// <summary>
        /// Initial stage - identifying and profiling the opportunity.
        /// </summary>
        public const string IdentifyAndProfile = "IDENTIFY & PROFILE";

        /// <summary>
        /// Final positive stage - opportunity approved to proceed.
        /// </summary>
        public const string Go = "GO";

        /// <summary>
        /// Final negative stage - opportunity not proceeding. Can be reopened.
        /// </summary>
        public const string NoGo = "NO GO";
    }

    /// <summary>
    /// Gets the state machine definition for Opportunity entities.
    /// </summary>
    public static StateMachine StateMachine => new()
    {
        EntityType = EntityName,
        States =
        [
            new State 
            { 
                Sequence = 1, 
                StageCode = Stages.IdentifyAndProfile, 
                DisplayName = "Identify & Profile",
                Facing = Facing.Internal 
            },
            new State 
            { 
                Sequence = 2, 
                StageCode = Stages.Go, 
                DisplayName = "Go",
                Facing = Facing.Internal 
            },
            new State 
            { 
                Sequence = 3, 
                StageCode = Stages.NoGo, 
                DisplayName = "No Go",
                Facing = Facing.Internal 
            }
        ]
    };

    /// <summary>
    /// All valid stage values for validation purposes.
    /// </summary>
    public static string[] AllStages => 
    [
        Stages.IdentifyAndProfile,
        Stages.Go,
        Stages.NoGo
    ];

    /// <summary>
    /// Checks if a stage value is valid.
    /// </summary>
    public static bool IsValidStage(string? stage) => 
        !string.IsNullOrEmpty(stage) && AllStages.Contains(stage);
}
