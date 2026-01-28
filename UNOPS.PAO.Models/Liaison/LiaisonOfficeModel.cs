using System;
using System.Collections.Generic;

namespace UNOPS.PAO.Models.Liaison;

/// <summary>
/// Liaison office model for regional coordination
/// </summary>
public class LiaisonOfficeModel
{
    // ========== OFFICE IDENTIFICATION ==========
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
    public string? ShortName { get; set; }

    // ========== LOCATION ==========
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? TimeZone { get; set; }

    // ========== CONTACT INFORMATION ==========
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    // ========== OFFICE DETAILS ==========
    public int? HeadOfOfficeUserId { get; set; }
    public string? HeadOfOfficeName { get; set; }
    public string? HeadOfOfficeEmail { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? Status { get; set; }

    // ========== STAFF INFORMATION ==========
    public int StaffCount { get; set; }
    public int ActiveUserCount { get; set; }

    // ========== OPERATIONAL ==========
    public DateTime? OperationalSince { get; set; }
    public int? PartnersAssigned { get; set; }
    public int? OpportunitiesManaged { get; set; }
    public int? ContactsManaged { get; set; }

    // ========== AUDIT ==========
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
}

/// <summary>
/// Liaison office user assignment model
/// </summary>
public class LiaisonUserModel
{
    public int Id { get; set; }
    public int LiaisonOfficeId { get; set; }
    public string? LiaisonOfficeName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? Role { get; set; } // Head, Staff, Coordinator
    public DateTime AssignedDate { get; set; }
    public int? AssignedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime? InactiveDate { get; set; }
}

/// <summary>
/// Liaison office assignment model
/// </summary>
public class LiaisonAssignmentModel
{
    public int Id { get; set; }
    public int LiaisonOfficeId { get; set; }
    public string LiaisonOfficeName { get; set; }
    public string EntityType { get; set; } // Partner, Contact, Opportunity
    public int EntityId { get; set; }
    public string? EntityName { get; set; }
    public DateTime AssignedDate { get; set; }
    public int? AssignedBy { get; set; }
    public string? AssignedByName { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Liaison office create/update request
/// </summary>
public class LiaisonOfficeRequest
{
    public string Name { get; set; }
    public string? Code { get; set; }
    public string? ShortName { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? TimeZone { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public int? HeadOfOfficeUserId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Liaison office statistics model
/// </summary>
public class LiaisonOfficeStatsModel
{
    public int LiaisonOfficeId { get; set; }
    public string LiaisonOfficeName { get; set; }
    public int TotalStaff { get; set; }
    public int ActiveStaff { get; set; }
    public int TotalPartners { get; set; }
    public int ActivePartners { get; set; }
    public int TotalOpportunities { get; set; }
    public int ActiveOpportunities { get; set; }
    public int TotalContacts { get; set; }
    public DateTime? StatsGeneratedDate { get; set; }
}
