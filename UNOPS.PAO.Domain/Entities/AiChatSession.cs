using System.Text.Json.Serialization;

namespace UNOPS.PAO.Domain.Entities;
public class AiChatSession
{
    public required string Id { get; set; }
    public double LastUpdated { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = "Active";
    public string Title { get; set; } = "New Chat";
    public bool AiGenerateTitle { get; set; } = true;

    public bool Archived { get; set; } = false;

    public bool Starred { get; set; } = false;
}
