using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.DataAccess.Services;
using System.Threading.Tasks;
using System.Linq;

namespace UNOPS.PAO.UNOPSPresentation.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<PAOIdentityUser> _userManager;
        private readonly RoleManager<PAOIdentityRole> _roleManager;
        private readonly ILogger<RoleController> _logger;

        public RoleController(
            UserManager<PAOIdentityUser> userManager,
            RoleManager<PAOIdentityRole> roleManager,
            ILogger<RoleController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new { id = r.Id, name = r.Name })
                .ToListAsync();
            
            return Ok(roles);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { 
                email = user.Email,
                roles = roles 
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserRoles([FromBody] string[] roles)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Ensure SecurityStamp is set (required for role operations)
                if (string.IsNullOrEmpty(user.SecurityStamp))
                {
                    _logger.LogInformation($"SecurityStamp is null for user {user.Email}, updating it");
                    var updateStampResult = await _userManager.UpdateSecurityStampAsync(user);
                    if (!updateStampResult.Succeeded)
                    {
                        var errors = string.Join(", ", updateStampResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to update security stamp: {errors}");
                        return BadRequest(new { 
                            message = "Failed to update user security stamp", 
                            errors = updateStampResult.Errors.Select(e => e.Description) 
                        });
                    }
                    _logger.LogInformation($"SecurityStamp updated successfully for user {user.Email}");
                }

                // Validate input roles
                if (roles == null)
                {
                    roles = new string[0];
                }

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation($"Current roles for user {user.Email}: {string.Join(", ", currentRoles)}");
                _logger.LogInformation($"New roles to assign: {string.Join(", ", roles)}");

                // Remove current roles only if there are any
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to remove current roles: {errors}");
                        return BadRequest(new { 
                            message = "Failed to remove current roles", 
                            errors = removeResult.Errors.Select(e => e.Description) 
                        });
                    }
                    _logger.LogInformation($"Successfully removed roles: {string.Join(", ", currentRoles)}");
                }

                // Add new roles only if there are any
                if (roles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, roles);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to add new roles: {errors}");
                        return BadRequest(new { 
                            message = "Failed to add new roles", 
                            errors = addResult.Errors.Select(e => e.Description) 
                        });
                    }
                    _logger.LogInformation($"Successfully added roles: {string.Join(", ", roles)}");
                }

                _logger.LogInformation($"Roles updated successfully for user {user.Email}");
                return Ok(new { message = "Roles updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user roles");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }
    }
} 