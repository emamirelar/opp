using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Users;

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

    /// <summary>
    /// Retrieves the current user's default organizational unit preference for filtering and access control.
    /// </summary>
    /// <example_uses>
    /// Get my default organizational unit setting
    /// What's my default org unit preference?
    /// Show my default office assignment
    /// Get current organizational unit filter
    /// Check my default org unit setting
    /// </example_uses>
    /// <when_to_use>Use this when loading user's default organizational unit for filtering or when determining the user's default office context.</when_to_use>
    /// <returns>Default organizational unit ID for the current user</returns>
    [HttpGet("default-org-unit")]
    public async Task<ActionResult> GetDefaultOrgUnit()
    {
        return await HandleOperationAsync(async () =>
        {
            var orgUnitId = await _userPreferenceService.GetDefaultOrgUnitIdAsync(CurrentUserId);
            return new { defaultOrgUnitId = orgUnitId };
        });
    }

    /// <summary>
    /// Updates the current user's default organizational unit preference for filtering and access control.
    /// </summary>
    /// <param name="request">Default org unit request containing the new setting</param>
    /// <param name="request.orgUnitId">Organizational unit ID to set as default (null to clear)</param>
    /// <example_uses>
    /// Set my default org unit to Headquarters
    /// Change my default office to Field Office 123
    /// Update my organizational unit preference
    /// Set default org unit filter to my office
    /// Clear my default org unit setting
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to change their default organizational unit for filtering and access control purposes.</when_to_use>
    /// <returns>Success confirmation</returns>
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