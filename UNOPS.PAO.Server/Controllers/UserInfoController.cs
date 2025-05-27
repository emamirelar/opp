using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation.Helpers;
using System.Security.Claims;

namespace UNOPS.PAO.Server.Controllers;

[Route("/")]
[ApiController]
[Authorize]
public class UserInfoController : ControllerBase
{
    private readonly IUserInfoService _userInfoService;
    private readonly UserResolverService<int> _userResolverService;

    public UserInfoController(IUserInfoService userInfoService, UserResolverService<int> userResolverService)
    {
        _userInfoService = userInfoService;
        _userResolverService = userResolverService;
    }

    [HttpGet(APIDictionary.CurrentUserInfo)]
    public async Task<ActionResult<UserInfo>> GetCurrentUserInfo()
    {
            // Try multiple ways to get the current user's email, similar to other working controllers
        var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? 
                          User.FindFirst("email")?.Value ?? 
                          User.Identity?.Name ?? 
                          _userResolverService.GetUserEmail();
        
        if (string.IsNullOrEmpty(currentEmail))
        {
            return Unauthorized("User not authenticated - email not found in claims");
        }

        var userInfo = await _userInfoService.GetUserInfoByEmailAsync(currentEmail);
        
        if (userInfo == null)
        {
            return NotFound($"User info not found for email {currentEmail}");
        }

        return Ok(userInfo);
    }
} 