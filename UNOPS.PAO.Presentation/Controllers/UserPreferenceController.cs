using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.Extensions.Logging;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("api/user-preferences")]
[Authorize(AuthenticationSchemes = "IAP")]
public class UserPreferenceController : BaseController
{
    private readonly IUserPreferenceService _userPreferenceService;

    public UserPreferenceController(
        IUserPreferenceService userPreferenceService,
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<UserPreferenceController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _userPreferenceService = userPreferenceService;
    }

    [HttpGet("default-org-unit")]
    public async Task<ActionResult> GetDefaultOrgUnit()
    {
        return await HandleOperationAsync(async () =>
        {
            var orgUnitId = await _userPreferenceService.GetDefaultOrgUnitIdAsync(CurrentUserId);
            return new { defaultOrgUnitId = orgUnitId };
        });
    }

    [HttpPut("default-org-unit")]
    public async Task<ActionResult> SetDefaultOrgUnit([FromBody] DefaultOrgUnitRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            await _userPreferenceService.UpdateDefaultOrgUnitAsync(CurrentUserId, request.OrgUnitId);
            return Ok();
        });
    }
}

public class DefaultOrgUnitRequest
{
    public int? OrgUnitId { get; set; }
}