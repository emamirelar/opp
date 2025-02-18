namespace UNOPS.PAO.Models.Workflow;
public class WorkflowStateModel
{
    public required string Stage { get; set; }
    public required string DisplayName { get; set; }
    public required string Comment { get; set; }
    public required StateAction[] NextActions { get; set; }

}