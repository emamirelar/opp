using System;
using System.Collections.Generic;

namespace UNOPS.PAO.Models.Roles;

/// <summary>
/// Role model for role-based access control
/// </summary>
public class RoleModel
{
    // ========== ROLE IDENTIFICATION ==========
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? RoleCode { get; set; }

    // ========== ROLE DETAILS ==========
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Category { get; set; }

    // ========== ROLE SCOPE ==========
    public string? Scope { get; set; } // e.g., "Global", "OrgUnit"
    public int? OrgUnitId { get; set; }
    public string? OrgUnitName { get; set; }

    // ========== USER COUNT ==========
    public int UserCount { get; set; }

    // ========== PERMISSIONS ==========
    public List<string>? PermissionNames { get; set; }
    public int PermissionCount { get; set; }

    // ========== AUDIT ==========
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
}

/// <summary>
/// Role assignment model
/// </summary>
public class RoleAssignmentModel
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string? UserEmail { get; set; }
    public DateTime AssignedDate { get; set; }
    public int? AssignedBy { get; set; }
    public string? AssignedByName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// Role permission model
/// </summary>
public class RolePermissionModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }
    public string? PermissionCategory { get; set; }
    public string? PermissionAction { get; set; }
    public DateTime GrantedDate { get; set; }
    public int? GrantedBy { get; set; }
}

/// <summary>
/// Role create/update request model
/// </summary>
public class RoleRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? RoleCode { get; set; }
    public bool IsActive { get; set; }
    public string? Category { get; set; }
    public string? Scope { get; set; }
    public int? OrgUnitId { get; set; }
    public List<int>? PermissionIds { get; set; }
}

/// <summary>
/// User roles summary model
/// </summary>
public class UserRolesModel
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string? UserEmail { get; set; }
    public List<RoleModel> Roles { get; set; } = new();
    public int ActiveRoleCount { get; set; }
    public List<string> EffectivePermissions { get; set; } = new();
}
