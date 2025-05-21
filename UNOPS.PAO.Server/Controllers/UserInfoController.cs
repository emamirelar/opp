using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation.Helpers;

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
        var currentEmail = _userResolverService.GetUserEmail();
        if (currentEmail == null)
        {
            return Unauthorized("User not authenticated");
        }

        var userInfo = await _userInfoService.GetUserInfoByEmailAsync(currentEmail);
        
        if (userInfo == null)
        {
            return NotFound($"User info not found for email {currentEmail}");
        }

        return Ok(userInfo);
    }
} 