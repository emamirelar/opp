namespace UNOPS.PAO.Models.Workflow;

public class WorkflowActionModel
{
    public required string EntityName { get; set; }
    public int Id { get; set; }
    public required string NewStage { get; set; }
    public required string Comment { get; set; }
}