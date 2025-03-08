namespace UNOPS.PAO.Models;
public class AiScreenMappingModel
{
    public int Id { get; set; }
    public string Type { get; set; }  // Identifier of the type of response required
    public string TableName { get; set; }   // Main table name (e.g., "Partners", "Contacts")
    public string? RelatedEntity { get; set; }
    public string? RelatedEntityKey { get; set; }
    public string? QueryConditions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}