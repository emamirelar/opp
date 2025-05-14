using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
public class UserDataController : BaseController
{
    private readonly IUserDataManager _manager;

    public UserDataController(
        IManagerWrapper manager, 
        IAuthorizationService authorizationService,
        ILogger<UserDataController> logger,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.UserDataManager;
    }

    [HttpGet(APIDictionary.CurrentUserData)]
    public async Task<ActionResult> GetCurrentUserData()
    {
        return await HandleOperationAsync(async () =>
        {
            var userData = await _manager.GetCurrentUserAsync();
            if (userData == null)
            {
                throw new BusinessException("User data not found");
            }
            return userData;
        });
    }
}