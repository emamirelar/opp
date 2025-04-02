using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;
public class DocumentType : ModifiableDeletableEntity
{
    public string EntityType { get; set; }
}
