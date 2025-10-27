using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.Users;

public class UserManagementModel
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string OrgUnit { get; set; } = string.Empty;
    public string? OrgUnitCode { get; set; }
    public string? OrgUnitDescription { get; set; }
    public List<string> Roles { get; set; } = new();
    public string RolesDisplay => string.Join(", ", Roles);
    public DateTime? LastModifiedDate { get; set; }
    public bool IsActive { get; set; }
}

public class UserManagementRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public List<string>? RoleFilter { get; set; }
    public bool ShowMyOrgUnitOnly { get; set; } = false;
    public List<int>? OrgUnitFilter { get; set; }
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

public class OrgUnitModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ResolveUsersRequest
{
    public List<int> UserIds { get; set; } = new List<int>();
}

public class ResolveRolesRequest
{
    public List<int> RoleIds { get; set; } = new List<int>();
} 