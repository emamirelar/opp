using System.Text.Json.Serialization;

namespace UNOPS.PAO.Domain.Entities;
public class AiChatSession
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = "Active";
    //[JsonIgnore]  // Prevents circular reference in serialization
    public virtual ICollection<AiChatHistory>? Chats { get; set; }
}