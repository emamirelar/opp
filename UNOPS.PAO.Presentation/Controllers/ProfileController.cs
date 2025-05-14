namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class ProfileController : ControllerBase
{
    private ProfileManager profileManager;
    private IAuthorizationService authorizationService;

    public ProfileController(ProfileManager profileManager, IAuthorizationService authorizationService)
    {
        this.profileManager = profileManager;
        this.authorizationService = authorizationService;
    }

    [HttpGet(APIDictionary.Profile)]
    public async Task<IActionResult> Get()
    {
        var email = HttpContext.User.Identity?.Name;
        var profile = profileManager.Get(email);

        if (profile == null)
        {
            return NotFound();
        }

        var authorizationResult = await this.authorizationService.AuthorizeAsync(User, profile, Operations.Read);

        if (authorizationResult.Succeeded)
        {
            return Ok(profile);
        }
        else
        {
            return Forbid();
        }
    }

    [HttpPost(APIDictionary.Profile)]
    public async Task UpdateProfile([FromBody] ProfileModel profile)
    {
        await profileManager.Update(profile);

    }
}
