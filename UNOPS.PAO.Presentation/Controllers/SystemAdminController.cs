namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;

[Route("/")]
[ApiController]
public class SystemAdminController : ControllerBase
{
    private readonly ISystemAdminManager systemAdminManager;
    public SystemAdminController(IManagerWrapper manager)
    {
        this.systemAdminManager = manager.SystemAdminManager;
    }

    [HttpGet(APIDictionary.SystemAdmin + "/migrations/run")]
    [PermissionAuthorize(PermissionNames.CanRunMigrations)]
    public async Task<IActionResult> RunMigrations()
    {
        await systemAdminManager.RunMigrations();

        return Ok();
    }
}
