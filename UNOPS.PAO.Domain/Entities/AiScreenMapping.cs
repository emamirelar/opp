namespace UNOPS.PAO.Domain.Entities;
public class AiScreenMapping : BaseBusinessEntity
{
    public string Type { get; set; } // Identifier of the type of response required
    public string TableName { get; set; } // Main table name (eg, Partners, Contacts)
    public string ComparisonKey { get; set; } = "Id";  // Comparison Key could vary per RelatedEntity
    public string? RelatedEntity { get; set; }
    public string? RelatedEntityKey { get; set; }
    public string? QueryConditions { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}