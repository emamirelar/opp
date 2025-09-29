namespace UNOPS.PAO.Domain.Infrastructure;

using UNOPS.PAO.Domain.Enums;

public class Notification : ModifiableEntity
{
    public bool IsRead { get; set; }
    public required string Headline { get; set; }
    public required string Description { get; set; }
    public required string CreatedFor { get; set; }
    public NotificationType NotificationType { get; set; }
    public int EntityId { get; set; }
}
