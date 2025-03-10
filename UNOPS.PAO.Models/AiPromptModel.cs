namespace UNOPS.PAO.Models;
public class AiPromptModel
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Prompt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}