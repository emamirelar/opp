using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.DataAccess.Services;
using System.Threading.Tasks;

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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove all current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest(new { message = "Failed to remove current roles" });
            }

            // Add new roles
            var addResult = await _userManager.AddToRolesAsync(user, roles);
            if (!addResult.Succeeded)
            {
                return BadRequest(new { message = "Failed to add new roles" });
            }

            return Ok(new { message = "Roles updated successfully" });
        }
    }
} 