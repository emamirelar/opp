using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class Document : ModifiableDeletableEntity
{
    public string Link { get; set; }
    public string? Type { get; set; }
    public ICollection<DocumentRelationship> DocumentRelationships { get; set; }
    public int? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public int? InteractionId { get; set; }
}