using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class UserDataController : ControllerBase
{
    private IUserDataManager manager;
    private IAuthorizationService authorizationService;

    public UserDataController(IManagerWrapper manager, IAuthorizationService authorizationService)
    {
        this.manager = manager.UserDataManager;
        this.authorizationService = authorizationService;
    }

    [HttpGet(APIDictionary.CurrentUserData)]
    public async Task<IActionResult> GetCurrentUserData()
    {
        var userData = await manager.GetCurrentUserAsync();
        if (userData == null)
        {
            return BadRequest();
        }
        return Ok(userData);
    }
    
    [HttpGet(APIDictionary.UserInfo)]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            // If no email is provided, try to get the current user's email from claims
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("Email is required");
            }
            email = userEmail;
        }
        
        var userData = await manager.GetUserByEmailAsync(email);
        if (userData == null)
        {
            // User not found in database
            return NotFound();
        }
        
        return Ok(userData);
    }
}