namespace UNOPS.PAO.Models.Notifications;
using UNOPS.PAO.Models.Shared;
public class NotificationFilterModel : PaginationRequest
{
    public string? SearchQuery { get; set; }
}
