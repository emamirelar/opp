namespace UNOPS.PAO.Models;
public class AiPromptModel
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Prompt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string GenerationConfig { get; set; }
    public string ContentConfig { get; set; }
    public string? ToolsConfig { get; set; }
    public string? SafetySettings { get; set; }
    public string Project { get; set; }
    public string Location { get; set; }
    public string Model { get; set; }
}