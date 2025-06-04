namespace UNOPS.PAO.Models;

public class AiPromptModel
{
    public int? Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PromptFunction { get; set; } = string.Empty;
    public string? Prompt { get; set; }
    public string? Description { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string GenerationConfig { get; set; } = string.Empty;
    public string ContentConfig { get; set; } = string.Empty;
    public string? ToolsConfig { get; set; }
    public string? SafetySettings { get; set; }
    public string Project { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}

public class AiPromptFilterRequest : PaginationRequest
{
    public string? SearchText { get; set; }
}