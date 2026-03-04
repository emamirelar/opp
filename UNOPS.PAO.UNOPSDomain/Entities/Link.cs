using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

public class Link : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    public string Entity { get; set; } = string.Empty;  // "Contact" or "Partner"
    public int EntityId { get; set; }  // ID of the Contact or Partner
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
} 