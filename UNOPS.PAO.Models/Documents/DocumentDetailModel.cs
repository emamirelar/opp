namespace UNOPS.PAO.Models.Documents;

/// <summary>
/// Simplified document model for display purposes
/// </summary>
public class DocumentDetailModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? StoragePath { get; set; }
    public string? Link { get; set; }
}

