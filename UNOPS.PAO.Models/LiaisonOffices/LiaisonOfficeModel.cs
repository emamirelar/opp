namespace UNOPS.PAO.Models.LiaisonOffices;
using UNOPS.PAO.Models.Shared;

public class LiaisonOfficeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    
    // Computed properties
    public int PartnerCount { get; set; }
    
    // RBAC permissions
    public EntityPermissionsModel? Permissions { get; set; }
    
    // For dropdown purposes
    public string DisplayName => $"{Code} - {Name}";
}

public class LiaisonOfficeFilterRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool IncludeCounts { get; set; } = true;
}

public class LiaisonOfficeSearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public int? MinPartnerCount { get; set; }
    public int? MaxPartnerCount { get; set; }
    public int PageSize { get; set; } = 20;
    public int PageIndex { get; set; } = 1;
    public string? OrderBy { get; set; } = "Name";
    public bool Ascending { get; set; } = true;
}
