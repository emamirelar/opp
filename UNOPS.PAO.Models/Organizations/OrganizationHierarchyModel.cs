using System;
using System.Collections.Generic;

namespace UNOPS.PAO.Models.Organizations;

/// <summary>
/// Organization model (base)
/// </summary>
public class OrganizationModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

/// <summary>
/// Organization hierarchy shorthand model
/// </summary>
public class OrgHierarchyModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public int Level { get; set; }
    public List<OrgHierarchyModel>? Children { get; set; }
}

/// <summary>
/// Organization hierarchy model for organizational structure
/// </summary>
public class OrganizationHierarchyModel
{
    // ========== ORGANIZATION IDENTIFICATION ==========
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
    public string? ShortName { get; set; }

    // ========== HIERARCHY ==========
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; } // Hierarchical path (e.g., "/1/5/12")
    public int? DisplayOrder { get; set; }

    // ========== ORGANIZATION DETAILS ==========
    public string? Type { get; set; } // Division, Department, Unit, Team
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? Status { get; set; }

    // ========== LEADERSHIP ==========
    public int? HeadUserId { get; set; }
    public string? HeadName { get; set; }
    public string? HeadEmail { get; set; }

    // ========== CHILDREN ==========
    public List<OrganizationHierarchyModel>? Children { get; set; }
    public int ChildCount { get; set; }
    public int TotalDescendants { get; set; }

    // ========== STAFF ==========
    public int DirectStaffCount { get; set; }
    public int TotalStaffCount { get; set; }

    // ========== AUDIT ==========
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
}

/// <summary>
/// Organization unit model (flattened)
/// </summary>
public class OrgUnitModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
    public string? ShortName { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }
    public string? Type { get; set; }
    public bool IsActive { get; set; }
    public int? HeadUserId { get; set; }
    public string? HeadName { get; set; }
    public int DirectStaffCount { get; set; }
}

/// <summary>
/// Organization structure model (tree view)
/// </summary>
public class OrgStructureModel
{
    public int RootOrgId { get; set; }
    public string RootOrgName { get; set; }
    public int TotalUnits { get; set; }
    public int MaxDepth { get; set; }
    public List<OrganizationHierarchyModel> Tree { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

/// <summary>
/// Organization create/update request
/// </summary>
public class OrganizationRequest
{
    public string Name { get; set; }
    public string? Code { get; set; }
    public string? ShortName { get; set; }
    public int? ParentId { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int? HeadUserId { get; set; }
    public int? DisplayOrder { get; set; }
}

/// <summary>
/// Organization hierarchy path model
/// </summary>
public class OrgHierarchyPathModel
{
    public int OrgUnitId { get; set; }
    public string OrgUnitName { get; set; }
    public List<OrgUnitModel> Ancestors { get; set; } = new();
    public List<OrgUnitModel> Descendants { get; set; } = new();
    public string FullPath { get; set; }
    public int Depth { get; set; }
}

/// <summary>
/// Organization staff assignment model
/// </summary>
public class OrgStaffModel
{
    public int Id { get; set; }
    public int OrgUnitId { get; set; }
    public string OrgUnitName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? Role { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime AssignedDate { get; set; }
    public int? AssignedBy { get; set; }
}

/// <summary>
/// Organization statistics model
/// </summary>
public class OrgStatsModel
{
    public int OrgUnitId { get; set; }
    public string OrgUnitName { get; set; }
    public int DirectStaffCount { get; set; }
    public int TotalStaffCount { get; set; }
    public int ChildUnitCount { get; set; }
    public int TotalDescendantUnits { get; set; }
    public int PartnersAssigned { get; set; }
    public int OpportunitiesManaged { get; set; }
    public DateTime? StatsGeneratedDate { get; set; }
}
