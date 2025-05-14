namespace UNOPS.PAO.UNOPSDomain.Authorization;

public class EntityPermission
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty; // e.g., "Partner", "Contact"
    public string Action { get; set; } = string.Empty; // "Create", "Read", "Update", "Delete"
    public string RoleName { get; set; } = string.Empty;
    
    // Column-level permissions (if applicable)
    public string? PropertyName { get; set; }
    
    // Row-level permission filters (if applicable)
    public string? FilterExpression { get; set; }
} 