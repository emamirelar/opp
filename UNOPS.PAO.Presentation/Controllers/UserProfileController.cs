using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.Presentation.Controllers;

/// <summary>
/// Consolidated User Profile Management Controller
/// Provides endpoints for managing user profiles, data, information, preferences, and organizational settings
/// </summary>
[Route("/")]
[ApiController]
[Authorize]
public class UserProfileController : BaseController
{
    // Dependencies from ProfileController
    private readonly ProfileManager _profileManager;
    
    // Dependencies from UserDataController  
    private readonly IUserDataManager _userDataManager;
    
    // Dependencies from UserInfoController
    private readonly IUserInfoService _userInfoService;
    private readonly UserResolverService<int> _userResolverService;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly IUserPreferenceService _userPreferenceService;

    public UserProfileController(
        // ProfileController dependencies
        ProfileManager profileManager,
        // UserDataController dependencies
        IManagerWrapper managerWrapper,
        // UserInfoController dependencies
        IUserInfoService userInfoService, 
        UserResolverService<int> userResolverService,
        UserManager<PAOIdentityUser> userManager,
        IUserPreferenceService userPreferenceService,
        // BaseController dependencies
        IAuthorizationService authorizationService,
        ILogger<UserProfileController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _profileManager = profileManager;
        _userDataManager = managerWrapper.UserDataManager;
        _userInfoService = userInfoService;
        _userResolverService = userResolverService;
        _userManager = userManager;
        _userPreferenceService = userPreferenceService;
    }

    #region ProfileController Endpoints

    /// <summary>
    /// Retrieves the current user's profile information with authorization checks.
    /// </summary>
    /// <returns>User profile with personal and professional details</returns>
    /// <example>
    /// Example uses:
    /// - "Show my profile"
    /// - "Get my profile information"
    /// - "Display profile details"
    /// - "What's in my user profile?"
    /// </example>
    /// <remarks>
    /// Use this when the user asks for their profile information or when displaying profile details in the UI.
    /// </remarks>
    /*[HttpGet(APIDictionary.Profile)]
    public async Task<ActionResult> Get()
    {
        return await HandleOperationAsync(async () =>
        {
            var email = HttpContext.User.Identity?.Name;
            var profile = _profileManager.Get(email);

            if (profile == null)
            {
                throw new BusinessException("Profile not found");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, profile, Operations.Read);

            if (!authorizationResult.Succeeded)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this profile");
            }
            
            return profile;
        });
    }*/

    /// <summary>
    /// Updates the current user's profile information.
    /// </summary>
    /// <param name="profile">Updated profile model containing new information</param>
    /// <returns>Success confirmation</returns>
    /// <example>
    /// Example uses:
    /// - "Update my profile"
    /// - "Change my profile information"
    /// - "Save profile changes"
    /// - "Modify my profile details"
    /// </example>
    /// <remarks>
    /// Use this when the user wants to update their profile information or save changes to their profile.
    /// </remarks>
    [HttpPost(APIDictionary.Profile)]
    public async Task<ActionResult> UpdateProfile([FromBody] ProfileModel profile)
    {
        return await HandleOperationAsync(async () =>
        {
            await _profileManager.Update(profile);
        });
    }

    #endregion

    #region UserInfoController Endpoints

    /// <summary>
    /// Updates current user's information and profile settings.
    /// </summary>
    /// <param name="userInfo">Updated user information object containing profile changes</param>
    /// <returns>Updated user information with confirmation</returns>
    /// <example>
    /// Example uses:
    /// - "Update my profile information"
    /// - "Change my personal details"
    /// - "Modify user account settings"
    /// - "Update contact information"
    /// - "Save profile changes"
    /// </example>
    /// <remarks>
    /// Use this when the user asks to update, modify, edit, or change their personal profile information, contact details, or account settings.
    /// </remarks>
    [HttpPut(APIDictionary.UserInfoUpdate)]
    public async Task<ActionResult<UserProfile>> UpdateUserInfo([FromBody] UserProfile userProfile)
    {
        var result = await _userInfoService.UpdateUserInfoAsync(userProfile);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves comprehensive current user information including profile, roles, permissions, organizational settings, and user preferences.
    /// Supports both authenticated user lookup and explicit email parameter for administrative purposes.
    /// </summary>
    /// <param name="email">Optional specific email to get user info for (if not provided, uses current authenticated user)</param>
    /// <returns>Complete user information including profile, roles, organizational context, permissions, and preferences</returns>
    /// <example>
    /// Example uses:
    /// - "Show my user information"
    /// - "Get my profile details"
    /// - "What are my current roles and permissions?"
    /// - "Display my organizational settings"
    /// - "Show my user preferences"
    /// - "Get current user context"
    /// - "What office am I assigned to?"
    /// - "Show my account details"
    /// </example>
    /// <remarks>
    /// Use this when the user asks for their profile information, account details, roles, permissions, organizational context, or when the system needs to load user-specific settings and preferences.
    /// </remarks>
    [HttpGet(APIDictionary.CurrentUserInfo)]
    public async Task<ActionResult<UserProfile>> GetUserProfileDetails([FromQuery] string? email = null)
    {
        string currentEmail;
        
        // Use provided email parameter if available, otherwise fall back to claims
        if (!string.IsNullOrEmpty(email))
        {
            currentEmail = email;
        }
        else
        {
            // Try multiple ways to get the current user's email from claims as fallback
            currentEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? 
                          User.FindFirst("email")?.Value ?? 
                          User.Identity?.Name ?? 
                          _userResolverService.GetUserEmail();
        }
        
        if (string.IsNullOrEmpty(currentEmail))
        {
            return Unauthorized("User not authenticated - email not found in claims or parameters");
        }

        // Get user roles - try from claims first, then database lookup
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // If no roles in claims, try to get them from database using email
        if (!userRoles.Any())
        {
            try
            {
                var aspNetUser = await _userManager.FindByEmailAsync(currentEmail);
                if (aspNetUser != null)
                {
                    userRoles = (await _userManager.GetRolesAsync(aspNetUser)).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue - we'll return empty roles
                // You might want to add proper logging here
                userRoles = new List<string>();
            }
        }

        // Check if user is PARTNER_GLOB_ADMIN
        var isPartnerGlobalAdmin = userRoles.Contains("PARTNER_GLOB_ADMIN");

        // Get user info with organization settings
        var userInfoWithOrgSettings = await _userInfoService.GetUserInfoWithOrgSettingsAsync(currentEmail);
        
        if (userInfoWithOrgSettings == null)
        {
            return NotFound($"User info not found for email {currentEmail}");
        }

        // Get the PAOUser to retrieve user preferences
        UserPreference? userPreferences = null;
        try
        {
            var aspNetUser = await _userManager.FindByEmailAsync(currentEmail);
            if (aspNetUser != null)
            {
                userPreferences = await _userPreferenceService.GetUserPreferencesAsync(aspNetUser.Id.ToString());
            }
        }
        catch (Exception ex)
        {
            userPreferences = null;
        }

        // Create response object with additional properties including user preferences
        var response = new
        {
            userInfoWithOrgSettings,
            Roles = userRoles,
            IsPartnerGlobalAdmin = isPartnerGlobalAdmin,
            // PARTNER_GLOB_ADMIN always has self-management enabled regardless of org setting
            CanManageOffice = isPartnerGlobalAdmin || 
                             (userInfoWithOrgSettings.GetType().GetProperty("IsSelfManagementEnabled")?.GetValue(userInfoWithOrgSettings) as bool? ?? false),
            UserPreferences = userPreferences
        };

        return Ok(response);
    }

    #endregion
} 