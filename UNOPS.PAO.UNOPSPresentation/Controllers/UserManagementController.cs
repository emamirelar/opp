using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSPresentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Authorization;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class UserManagementController : BaseController
{
    private readonly IUserManagementManager _manager;
    private readonly IPermissionService _permissionService;

    public UserManagementController(
        IManagerWrapper managerWrapper,
        ILogger<UserManagementController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService,
        IPermissionService permissionService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.UserManagementManager;
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get paginated list of users with filtering and search capabilities
    /// </summary>
    /// <param name="request">User management request with pagination and filters</param>
    /// <returns>Paginated list of users</returns>
    [HttpPost(APIDictionary.UserManagementUsers)]
    public async Task<ActionResult<PaginationResponse<UserManagementModel>>> GetUsers([FromBody] UserManagementRequest request)
    {
        try
        {
            // Check permissions using PermissionService
            var hasAccess = await _permissionService.CanPerformActionAsync("UserManagement", "read", User);
            if (!hasAccess)
            {
                _logger.LogWarning("User {User} denied access to user management", User.Identity?.Name);
                return Forbid("Access denied. Only Partnership Global Admins and Org Unit Admins can access user management.");
            }

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
    public async Task<ActionResult<UserManagementModel>> GetUser(int userId)
    {
        try
        {
            // Check permissions using PermissionService
            var hasAccess = await _permissionService.CanPerformActionAsync("UserManagement", "read", User);
            if (!hasAccess)
            {
                _logger.LogWarning("User {User} denied access to user management", User.Identity?.Name);
                return Forbid("Access denied. Only Partnership Global Admins and Org Unit Admins can access user management.");
            }

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
    public async Task<ActionResult<UserManagementModel>> UpdateUserRoles(int userId, [FromBody] UpdateUserRolesRequest request)
    {
        try
        {
            // Check permissions using PermissionService
            var hasAccess = await _permissionService.CanPerformActionAsync("UserManagement", "update", User);
            if (!hasAccess)
            {
                _logger.LogWarning("User {User} denied access to update user roles", User.Identity?.Name);
                return Forbid("Access denied. Only Partnership Global Admins and Org Unit Admins can update user roles.");
            }

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
    public async Task<ActionResult<IEnumerable<RoleModel>>> GetAvailableRoles()
    {
        try
        {
            // Check permissions using PermissionService
            var hasAccess = await _permissionService.CanPerformActionAsync("UserManagement", "read", User);
            if (!hasAccess)
            {
                _logger.LogWarning("User {User} denied access to user management roles", User.Identity?.Name);
                return Forbid("Access denied. Only Partnership Global Admins and Org Unit Admins can access user management.");
            }

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
    public async Task<ActionResult<string>> GetCurrentUserOrgUnit()
    {
        try
        {
            // Check permissions using PermissionService
            var hasAccess = await _permissionService.CanPerformActionAsync("UserManagement", "read", User);
            if (!hasAccess)
            {
                _logger.LogWarning("User {User} denied access to user management", User.Identity?.Name);
                return Forbid("Access denied. Only Partnership Global Admins and Org Unit Admins can access user management.");
            }

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
} 