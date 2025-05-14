namespace UNOPS.PAO.Presentation.Controllers.External;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class ProfileController : ControllerBase
{
    private UserResolverService<int> userResolverService;
    private ProfileManager profileManager;

    public ProfileController(ProfileManager profileManager, UserResolverService<int> userResolverService)
    {
        this.userResolverService = userResolverService;
        this.profileManager = profileManager;
    }

    [HttpGet(APIDictionary.ExternalProfile)]
    public ActionResult Get()
    {
        var profile = profileManager.Get(userResolverService.GetUserName());

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPost(APIDictionary.ExternalProfile)]
    public async Task UpdateProfile([FromBody] ProfileModel profile)
    {
        await profileManager.Update(profile);

    }
}
