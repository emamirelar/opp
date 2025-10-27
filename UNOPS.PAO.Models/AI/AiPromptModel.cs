namespace UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Shared;

public class AiPromptModel
{
    public int? Id { get; set; }
    public string Type { get; set; } = string.Empty;
    
    // NEW: DataRetrievalMethod (replaces PromptFunction)
    public string DataRetrievalMethod { get; set; } = string.Empty;
    
    // NEW: SystemInstructions (replaces Prompt)
    public string SystemInstructions { get; set; } = string.Empty;
    
    // NEW: UserPrompt - separate from system instructions
    public string? UserPrompt { get; set; }
    
    // NEW: Feature column to categorize/group prompts
    public string Feature { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public string? Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string GenerationConfig { get; set; } = string.Empty;
    public string ContentConfig { get; set; } = string.Empty;
    public string? ToolsConfig { get; set; }
    public string? SafetySettings { get; set; }
    public string Project { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    
    // NEW: Caching Configuration
    public bool UseCache { get; set; } = false;
    public int CacheInvalidationMinutes { get; set; } = 60;
    
    // LEGACY: Keep old properties for backward compatibility during migration
    [Obsolete("Use SystemInstructions instead")]
    public string? Prompt { get; set; }
    
    [Obsolete("Use DataRetrievalMethod instead")]
    public string? PromptFunction { get; set; } = string.Empty;
}

public class AiPromptFilterRequest : PaginationRequest
{
    public string? SearchText { get; set; }
}

public class GeminiModelUpgradeResult
{
    public bool Success { get; set; }
    public int UpdatedCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? LatestModel { get; set; }
    public bool AlreadyLatest { get; set; }
}