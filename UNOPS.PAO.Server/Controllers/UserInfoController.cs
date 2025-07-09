using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;

namespace UNOPS.PAO.Server.Controllers;

[Route("/")]
[ApiController]
[Authorize]
public class UserInfoController : ControllerBase
{
    private readonly IUserInfoService _userInfoService;
    private readonly UserResolverService<int> _userResolverService;
    private readonly UserManager<PAOIdentityUser> _userManager;

    public UserInfoController(
        IUserInfoService userInfoService, 
        UserResolverService<int> userResolverService,
        UserManager<PAOIdentityUser> userManager)
    {
        _userInfoService = userInfoService;
        _userResolverService = userResolverService;
        _userManager = userManager;
    }

    [HttpPut(APIDictionary.UserInfoUpdate)]
    public async Task<ActionResult<UserInfo>> UpdateUserInfo([FromBody] UserInfo userInfo)
    {
        var result = await _userInfoService.UpdateUserInfoAsync(userInfo);
        return Ok(result);
    }

    [HttpGet(APIDictionary.CurrentUserInfo)]
    public async Task<ActionResult<UserInfo>> GetCurrentUserInfo([FromQuery] string? email = null)
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

        // Create response object with additional properties
        var response = new
        {
            userInfoWithOrgSettings,
            Roles = userRoles,
            IsPartnerGlobalAdmin = isPartnerGlobalAdmin,
            // PARTNER_GLOB_ADMIN always has self-management enabled regardless of org setting
            CanManageOffice = isPartnerGlobalAdmin || 
                             (userInfoWithOrgSettings.GetType().GetProperty("IsSelfManagementEnabled")?.GetValue(userInfoWithOrgSettings) as bool? ?? false)
        };

        return Ok(response);
    }
} 