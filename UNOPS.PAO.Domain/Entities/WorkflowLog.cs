using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class WorkflowLog : ModifiableDeletableEntity
{
    public string EntityName { get; set; }
    public string EntityId { get; set; }
    public string? Stage { get; set; }
    public string NewStage { get; set; }
    public string Comment { get; set; }
}