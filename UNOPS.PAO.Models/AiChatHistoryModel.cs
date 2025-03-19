using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models;
public class AiChatHistoryModel
{
    public int Id { get; set; }
    public string Sender { get; set; }
    public string Message { get; set; }
    public string? RawUserMessage { get; set; } // Only user input
    public DateTime TimeStamp { get; set; }
    public string Type { get; set; }
    public string? EntityType { get; set; }
    public string? RequestType { get; set; }
    public Guid? SessionId { get; set; }
    public AiChatSessionModel? Session { get; set; }
}