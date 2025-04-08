using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class PartnerCategory : ModifiableDeletableEntity
{
    public string Description { get; set; }
}