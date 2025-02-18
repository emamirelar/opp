using UNOPS.PAO.Models.Workflow;

namespace UNOPS.PAO.Business.Workflow;
public class ProposalWorkflow
{
    public static StateMachine StateMachine
    {
        get
        {
            var draft = new State()
            {
                StageCode = "Draft",
                Sequence = 1,
                Facing = Facing.External,
                Actions =
                [
                    new StateAction { ActionName= "Submit", NewStage = "Submitted", Sequence = 1, CommentRequired = false, Facing = Facing.External }
                ]
            };

            var submitted = new State()
            {
                StageCode = "Submitted",
                Sequence = 2,
                Facing = Facing.TwoFace,
                Actions = 
                [
                    new StateAction { ActionName= "Reject", NewStage = "Disqualified", Sequence = 1, CommentRequired = true, Facing = Facing.Internal },
                    new StateAction { ActionName= "Recommend", NewStage = "Recommended", Sequence = 2, CommentRequired = true, Facing = Facing.Internal }
                    
                ]
            };

            var disqualified = new State()
            {
                StageCode = "Disqualified",
                Sequence = 3,
                Facing = Facing.Internal,
                Actions = [],
                ExternalState = submitted
            };

            var recommended = new State()
            {
                StageCode = "Recommended",
                Sequence = 4,
                Facing = Facing.Internal,
                Actions = [],
                ExternalState = submitted
            };

            return new StateMachine()
            {
                States =
                [
                    draft,
                    submitted,
                    disqualified,
                    recommended
                ]
            };
        }
    }
}