using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class UserManagementController : BaseController
{
    private readonly IUserManagementManager _manager;

    public UserManagementController(
        IManagerWrapper managerWrapper,
        ILogger<UserManagementController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.UserManagementManager;
    }

    /// <summary>
    /// Get paginated list of users with filtering and search capabilities
    /// </summary>
    /// <param name="request">User management request with pagination and filters</param>
    /// <returns>Paginated list of users</returns>
    [HttpPost(APIDictionary.UserManagementUsers)]
    [AccessControlled(EntityTypes.UserManagement, "read")]
    public async Task<ActionResult<PaginationResponse<UserManagementModel>>> GetUsers([FromBody] UserManagementRequest request)
    {
        try
        {
            var result = await _manager.GetUsersAsync(User, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user details by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User details</returns>
    [HttpGet(APIDictionary.UserManagementUsers + "/{userId}")]
    [AccessControlled(EntityTypes.UserManagement, "read")]
    public async Task<ActionResult<UserManagementModel>> GetUser(string userId)
    {
        try
        {
            var result = await _manager.GetUserByIdAsync(User, userId);
            if (result == null)
            {
                return NotFound("User not found");
            }
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user details");
        }
    }

    /// <summary>
    /// Update user roles
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update roles request</param>
    /// <returns>Updated user details</returns>
    [HttpPut(APIDictionary.UserManagementUsers + "/{userId}/roles")]
    [AccessControlled(EntityTypes.UserManagement, "update")]
    public async Task<ActionResult<UserManagementModel>> UpdateUserRoles(string userId, [FromBody] UpdateUserRolesRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _manager.UpdateUserRolesAsync(User, userId, request);
            if (result == null)
            {
                return NotFound("User not found");
            }

            _logger.LogInformation("User {UserId} roles updated by {CurrentUserId}", userId, CurrentUserId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid request: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error updating user roles for user {UserId}", userId);
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating user roles for user {UserId}", userId);
            return StatusCode(500, "An error occurred while updating user roles");
        }
    }

    /// <summary>
    /// Get available roles that the current user can assign
    /// </summary>
    /// <returns>List of available roles</returns>
    [HttpGet(APIDictionary.UserManagementRoles)]
    [AccessControlled(EntityTypes.UserManagement, "read")]
    public async Task<ActionResult<IEnumerable<RoleModel>>> GetAvailableRoles()
    {
        try
        {
            var result = await _manager.GetAvailableRolesAsync(User);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available roles");
            return StatusCode(500, "An error occurred while retrieving available roles");
        }
    }

    /// <summary>
    /// Get current user's organization unit for filtering
    /// </summary>
    /// <returns>Current user's org unit</returns>
    [HttpGet(APIDictionary.UserManagementCurrentUserOrgUnit)]
    [AccessControlled(EntityTypes.UserManagement, "read")]
    public async Task<ActionResult<string>> GetCurrentUserOrgUnit()
    {
        try
        {
            // This would typically come from a service that gets user info
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("User email not found");
            }

            // For now, return a placeholder - this should be implemented based on your user service
            return Ok("Sample Org Unit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user org unit");
            return StatusCode(500, "An error occurred while retrieving org unit information");
        }
    }

    /// <summary>
    /// Get organization unit self-management setting
    /// </summary>
    /// <param name="orgUnitCode">Organization unit code</param>
    /// <returns>Self-management setting</returns>
    [HttpGet(APIDictionary.UserManagement + "/org-units/{orgUnitCode}/self-management")]
    [AccessControlled(EntityTypes.UserManagement, "read")]
    public async Task<ActionResult> GetOrgUnitSelfManagement(string orgUnitCode)
    {
        try
        {
            var result = await _manager.GetOrgUnitSelfManagementAsync(User, orgUnitCode);
            return Ok(new { isSelfManagementEnabled = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid request: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving org unit self-management for {OrgUnitCode}", orgUnitCode);
            return StatusCode(500, "An error occurred while retrieving organization settings");
        }
    }

    /// <summary>
    /// Update organization unit self-management setting
    /// </summary>
    /// <param name="orgUnitCode">Organization unit code</param>
    /// <param name="request">Self-management setting request</param>
    /// <returns>Success response</returns>
    [HttpPut(APIDictionary.UserManagement + "/org-units/{orgUnitCode}/self-management")]
    [AccessControlled(EntityTypes.UserManagement, "update")]
    public async Task<ActionResult> UpdateOrgUnitSelfManagement(string orgUnitCode, [FromBody] UpdateOrgUnitSelfManagementRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _manager.UpdateOrgUnitSelfManagementAsync(User, orgUnitCode, request);

            _logger.LogInformation("Organization unit {OrgUnitCode} self-management updated to {IsSelfManagementEnabled} by {CurrentUserId}", 
                orgUnitCode, request.IsSelfManagementEnabled, CurrentUserId);
            
            return Ok(new { message = "Organization self-management setting updated successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid request: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating org unit self-management for {OrgUnitCode}", orgUnitCode);
            return StatusCode(500, "An error occurred while updating organization settings");
        }
    }
} 