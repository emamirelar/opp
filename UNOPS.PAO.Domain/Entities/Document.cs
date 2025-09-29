using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class Document : ModifiableDeletableEntity
{
    public required string Link { get; set; }
    public string? Type { get; set; }
    public virtual ICollection<DocumentRelationship> DocumentRelationships { get; set; } = new HashSet<DocumentRelationship>();
    public int? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public int? InteractionId { get; set; }
}