namespace UNOPS.PAO.Models.Workflow;
public class StateAction
{
    public required string ActionName { get; set; }
    public required string NewStage { get; set; }
    public int Sequence { get; set; }
    public bool CommentRequired { get; set; }
    public Facing Facing { get; set; } = Facing.TwoFace;
}