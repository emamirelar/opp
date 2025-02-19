using UNOPS.PAO.Models.Workflow;

namespace UNOPS.PAO.Business.Workflow;
public class FundingOpportunityWorkflow
{
    public static StateMachine StateMachine
    {
        get
        {
            return new StateMachine()
            {
                States =
                [
                    new State() {
                        Sequence = 1,
                        StageCode = "Cancelled",
                        Facing = Facing.Internal,
                        Actions =
                        [
                            new StateAction { ActionName= "Reopen", NewStage = "Draft", Sequence = 1, CommentRequired = false }
                        ]
                    },
                    new State() {
                        Sequence = 2,
                        StageCode = "Draft",
                        DisplayName = "Not yet open",
                        Facing = Facing.Internal,
                        Actions =
                        [
                            new StateAction { ActionName= "Post", NewStage = "Open", Sequence = 1, CommentRequired = false },
                            new StateAction { ActionName= "Cancel", NewStage = "Cancelled", Sequence = 2, CommentRequired = false }
                        ]
                    },
                    new State() {
                        Sequence = 3,
                        StageCode = "Open",
                        Facing = Facing.TwoFace,
                        Actions =
                        [
                            new StateAction { ActionName= "Close", NewStage = "Closed", Sequence = 1, CommentRequired = false, Facing = Facing.Internal },
                            new StateAction { ActionName= "Cancel", NewStage = "Cancelled", Sequence = 2, CommentRequired = false, Facing = Facing.Internal }
                        ]
                    },
                    new State() {
                        Sequence = 4,
                        StageCode = "Closed",
                        Facing = Facing.TwoFace,
                        Actions =
                        [
                            new StateAction { ActionName= "Reopen", NewStage = "Draft", Sequence = 1, CommentRequired = false, Facing = Facing.Internal }
                        ]
                    }
                ]
            };
        }
    }
}