namespace UNOPS.PAO.UNOPSDomain.Authorization;

public class EntityPermission
{
    public int Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool CanRead { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public string? PropertyFilter { get; set; }
    public string? RowFilter { get; set; }
} 