namespace UNOPS.PAO.Models;
public class AiChatSessionModel
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; }
}