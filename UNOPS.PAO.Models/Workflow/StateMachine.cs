namespace UNOPS.PAO.Models.Workflow;
public class StateMachine
{
    public string? Stage { get; set; }
    public required State[] States { get; set; }

    public State? StateAction
    {
        get
        {
            return this.States.FirstOrDefault(x => x.StageCode == this.Stage);
        }
    }

    //public string? StageName
    //{
    //    get
    //    {
    //        var stateAction = this.States.FirstOrDefault(x => x.Stage == this.Stage);

    //        if (stateAction == null)
    //        {
    //            return string.Empty;
    //        }

    //        return string.IsNullOrEmpty(stateAction.DisplayName) ? stateAction.Stage : stateAction.DisplayName;
    //    }
    //}

    //public StateAction[] NextActions
    //{
    //    get
    //    {
    //        var stateAction = this.States.FirstOrDefault(x => x.Stage == this.Stage);

    //        return stateAction == null
    //            ? []
    //            : [.. stateAction.Actions];
    //    }
    //}
}