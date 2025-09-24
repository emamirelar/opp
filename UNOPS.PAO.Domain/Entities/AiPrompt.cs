namespace UNOPS.PAO.Domain.Entities;
public class AiPrompt : BaseBusinessEntity
{
    public new int? Id { get; set; }
    public required string Type { get; set; }
    public required string PromptFunction { get; set; } // Function name to call on the manager
    public string? Prompt { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required string GenerationConfig { get; set; }
    public required string ContentConfig { get; set; }
    public string? ToolsConfig { get; set; }
    public string? SafetySettings { get; set; }
    public required string Project { get; set; }
    public required string Location { get; set; }
    public required string Model { get; set; }
    public bool AdminCanChange { get; set; } = false;
}