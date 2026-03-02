namespace UNOPS.PAO.Models.AI;

public class TestPromptRequest
{
    // Core request - the AiPrompt table will define the rest
    public string Type { get; set; } = null!; // Maps to AiPrompt.Type (e.g., "partner_summary")
    public int? Id { get; set; }     // Entity ID to analyze (optional - for Entity ID mode)
    public string? TestData { get; set; } // Test data (optional - for Test Data mode)
    
    // NEW: Enhanced prompt structure
    public string? DataRetrievalMethod { get; set; } // Override the data retrieval method
    public string? SystemInstructions { get; set; } // Override system instructions
    public string? UserPrompt { get; set; } // Override user prompt
    
    // Optional overrides for testing (if not provided, uses values from AiPrompt table)
    public string? Model { get; set; }
    public string? Project { get; set; }
    public string? Location { get; set; }
    public double? Temperature { get; set; }
    public double? TopP { get; set; }
    public int? MaxOutputTokens { get; set; }
    public bool? GoogleSearch { get; set; }
    public string? SafetySettings { get; set; }
    
    // LEGACY: Keep for backward compatibility
    public string? Prompt { get; set; } // Override the prompt from the database (maps to SystemInstructions)
} 