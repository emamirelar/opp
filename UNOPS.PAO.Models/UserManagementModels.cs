using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Models;

public class UserManagementModel
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string OrgUnit { get; set; } = string.Empty;
    public string? OrgUnitCode { get; set; }
    public List<string> Roles { get; set; } = new();
    public string RolesDisplay => string.Join(", ", Roles);
    public DateTime? LastModifiedDate { get; set; }
    public bool IsActive { get; set; }
}

public class UserManagementRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public bool ShowMyOrgUnitOnly { get; set; } = false;
    public string? OrgUnitFilter { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}

public class UpdateUserRolesRequest
{
    [Required]
    public List<string> Roles { get; set; } = new();
}

public class UpdateOrgUnitSelfManagementRequest
{
    [Required]
    public bool IsSelfManagementEnabled { get; set; }
}

public class RoleModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 