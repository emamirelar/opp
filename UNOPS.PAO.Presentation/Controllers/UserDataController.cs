using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[ApiController]
[Authorize]
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
}