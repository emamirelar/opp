namespace UNOPS.PAO.Models.Workflow;
public class WorkflowStageModel
{
    public required string Stage { get; set; }
    public required string DisplayName { get; set; }
    public int Sequence { get; set; }
}