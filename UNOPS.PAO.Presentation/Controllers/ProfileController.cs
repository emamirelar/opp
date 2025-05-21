namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;

[Route("/")]
public class ProfileController : BaseController
{
    private readonly ProfileManager _profileManager;

    public ProfileController(
        ProfileManager profileManager, 
        IAuthorizationService authorizationService,
        ILogger<ProfileController> logger,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _profileManager = profileManager;
    }

    [HttpGet(APIDictionary.Profile)]
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
    }

    [HttpPost(APIDictionary.Profile)]
    public async Task<ActionResult> UpdateProfile([FromBody] ProfileModel profile)
    {
        return await HandleOperationAsync(async () =>
        {
            await _profileManager.Update(profile);
        });
    }
}
