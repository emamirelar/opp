namespace UNOPS.PAO.Domain.Entities;
public class AiPrompt : BaseBusinessEntity
{
    public int? Id { get; set; }
    public string Type { get; set; }
    
    public string DataRetrievalMethod { get; set; } // Function name to call on the manager
    
    public string SystemInstructions { get; set; }
    
    public string? UserPrompt { get; set; }
    
    public string Feature { get; set; }
    
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
    
    // NEW: Caching Configuration
    public bool UseCache { get; set; } = false;
    public int CacheInvalidationMinutes { get; set; } = 60; // Default 1 hour
}