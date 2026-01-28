using System;
using System.Collections.Generic;

namespace UNOPS.PAO.Models.Permissions;

/// <summary>
/// Permission model for access control
/// </summary>
public class PermissionModel
{
    // ========== PERMISSION IDENTIFICATION ==========
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? PermissionCode { get; set; }

    // ========== PERMISSION DETAILS ==========
    public string? Category { get; set; } // e.g., "Partners", "Contacts", "Opportunities"
    public string? Module { get; set; }
    public string? Action { get; set; } // e.g., "View", "Create", "Edit", "Delete"
    public bool IsSystemPermission { get; set; }
    public bool IsActive { get; set; }

    // ========== PERMISSION SCOPE ==========
    public string? Scope { get; set; } // e.g., "Global", "OrgUnit", "Own"
    public int? DisplayOrder { get; set; }

    // ========== AUDIT ==========
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
}

/// <summary>
/// Permission check request model
/// </summary>
public class PermissionCheckModel
{
    public int UserId { get; set; }
    public string PermissionName { get; set; }
    public string? ResourceType { get; set; }
    public int? ResourceId { get; set; }
    public int? OrgUnitId { get; set; }
}

/// <summary>
/// Permission check response model
/// </summary>
public class PermissionCheckResponse
{
    public bool HasPermission { get; set; }
    public string? Reason { get; set; }
    public List<string>? GrantedPermissions { get; set; }
}

/// <summary>
/// Permission assignment model
/// </summary>
public class PermissionAssignmentModel
{
    public int Id { get; set; }
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime AssignedDate { get; set; }
    public int? AssignedBy { get; set; }
}

/// <summary>
/// User permissions summary model
/// </summary>
public class UserPermissionsModel
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public List<PermissionModel> DirectPermissions { get; set; } = new();
    public List<PermissionModel> RolePermissions { get; set; } = new();
    public List<string> EffectivePermissions { get; set; } = new();
}
