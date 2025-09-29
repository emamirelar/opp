using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class WorkflowLog : ModifiableDeletableEntity
{
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public string? Stage { get; set; }
    public required string NewStage { get; set; }
    public required string Comment { get; set; }
}