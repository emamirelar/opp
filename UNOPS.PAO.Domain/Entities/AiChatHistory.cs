using System.Text.Json.Serialization;

namespace UNOPS.PAO.Domain.Entities;
public class AiChatHistory
{
    public int Id { get; set; }
    public string Sender { get; set; }
    public string Message { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public string Type { get; set; }
    public string? EntityType { get; set; }
    public string? RequestType { get; set; }
    public Guid? SessionId { get; set; }
    [JsonIgnore]  // Prevents circular reference in serialization
    public AiChatSession? Session { get; set; }
}