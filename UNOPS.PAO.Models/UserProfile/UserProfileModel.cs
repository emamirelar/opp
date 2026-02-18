using System;
using System.Collections.Generic;

namespace UNOPS.PAO.Models.UserProfile;

/// <summary>
/// User profile model for personal information and settings
/// </summary>
public class UserProfileModel
{
    // ========== USER IDENTIFICATION ==========
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? DisplayName { get; set; }

    // ========== PERSONAL INFORMATION ==========
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }

    // ========== ORGANIZATION ==========
    public int? OrgUnitId { get; set; }
    public string? OrgUnitName { get; set; }
    public int? LiaisonOfficeId { get; set; }
    public string? LiaisonOfficeName { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }

    // ========== PROFILE DETAILS ==========
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Languages { get; set; }
    public string? Skills { get; set; }

    // ========== PREFERENCES ==========
    public string? PreferredLanguage { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool InAppNotificationsEnabled { get; set; }

    // ========== ACTIVITY ==========
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int? LoginCount { get; set; }
    public bool IsActive { get; set; }

    // ========== AUDIT ==========
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
}

/// <summary>
/// User preferences model
/// </summary>
public class UserPreferencesModel
{
    public int UserId { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Theme { get; set; } // Light, Dark, Auto
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? TimeZone { get; set; }
    public int? DefaultOrgUnitId { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool InAppNotificationsEnabled { get; set; }
    public bool DesktopNotificationsEnabled { get; set; }
    public Dictionary<string, object>? CustomPreferences { get; set; }
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// User settings model
/// </summary>
public class UserSettingsModel
{
    public int UserId { get; set; }
    public string SettingKey { get; set; }
    public string? SettingValue { get; set; }
    public string? SettingType { get; set; } // String, Boolean, Integer, JSON
    public string? Category { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

/// <summary>
/// Update user profile request model
/// </summary>
public class UpdateUserProfileRequest
{
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Languages { get; set; }
    public string? Skills { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public bool? EmailNotificationsEnabled { get; set; }
    public bool? InAppNotificationsEnabled { get; set; }
}
