namespace UNOPS.PAO.Domain.Entities;
public class AiPrompt : BaseBusinessEntity
{
    public int? Id { get; set; }
    public string Type { get; set; }
    public string PromptFunction { get; set; } // Function name to call on the manager
    public string? Prompt { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string GenerationConfig { get; set; }
    public string ContentConfig { get; set; }
    public string? ToolsConfig { get; set; }
    public string? SafetySettings { get; set; }
    public string Project { get; set; }
    public string Location { get; set; }
    public string Model { get; set; }
}