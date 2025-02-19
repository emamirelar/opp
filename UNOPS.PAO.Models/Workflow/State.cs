namespace UNOPS.PAO.Models.Workflow;
public class State
{
    public required string StageCode { get; set; }
    public string? DisplayName { get; set; }
    public int Sequence { get; set; }
    public required StateAction[] Actions { get; set; }
    public Facing Facing { get; set; } = Facing.TwoFace;
    public State? ExternalState { get; set; }
    public State? InternalState { get; set; }
    public string Name => !string.IsNullOrEmpty(DisplayName) ? DisplayName : StageCode;
}