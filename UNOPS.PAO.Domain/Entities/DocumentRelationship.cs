using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class DocumentRelationship: ModifiableDeletableEntity
{
    public int DocumentId { get; set; }
    public virtual Document? Document { get; set; }
    public int EntityId { get; set; }
    public required string EntityType { get; set; }
    public string? Description { get; set; }
}