namespace UNOPS.PAO.Domain.Entities;
public class AiPrompt : BaseBusinessEntity
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string? Prompt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}