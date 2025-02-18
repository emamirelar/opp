namespace UNOPS.PAO.Domain.Infrastructure;

using UNOPS.PAO.Domain.Enums;

public class Notification : ModifiableEntity
{
    public bool IsRead { get; set; }
    public string Headline { get; set; }
    public string Description { get; set; }
    public string CreatedFor { get; set; }
    public NotificationType NotificationType { get; set; }
    public int EntityId { get; set; }
}
