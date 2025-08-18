namespace UNOPS.PAO.Models;

public class LiaisonOfficeModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    
    // For dropdown purposes
    public string DisplayName => $"{Code} - {Name}";
}
