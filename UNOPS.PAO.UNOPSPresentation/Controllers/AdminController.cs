namespace UNOPS.PAO.UNOPSPresentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Seed;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator", AuthenticationSchemes = "IAP")]
public class AdminController : ControllerBase
{
    private readonly UNOPSAppDbContext _dbContext;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;

    public AdminController(
        UNOPSAppDbContext dbContext,
        UserManager<PAOIdentityUser> userManager = null,
        RoleManager<PAOIdentityRole> roleManager = null)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("setup-admin")]
    [AllowAnonymous] // Temporarily allow anonymous access for initial setup
    public async Task<IActionResult> SetupAdminUser(string email, string password)
    {
        if (_userManager == null || _roleManager == null)
        {
            return BadRequest("User management is not available");
        }

        try
        {
            // Check if admin role exists
            if (!await _roleManager.RoleExistsAsync("Administrator"))
            {
                // Create Administrator role if it doesn't exist
                await _roleManager.CreateAsync(new PAOIdentityRole { Name = "Administrator" });
            }

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create new user if doesn't exist
                user = new PAOIdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsInternal = true
                };
                
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }

            // Assign Administrator role to user
            if (!await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                await _userManager.AddToRoleAsync(user, "Administrator");
            }

            return Ok(new { message = $"Admin user {email} setup successfully! You can now login with this account." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to setup admin user", details = ex.Message });
        }
    }

    [HttpPost("seed-permissions")]
    [Authorize(Roles = "Administrator", AuthenticationSchemes = "IAP")]
    public async Task<IActionResult> SeedEntityPermissions()
    {
        try
        {
            // Delete existing permissions if any
            var existingPermissions = await _dbContext.EntityPermissions.ToListAsync();
            if (existingPermissions.Any())
            {
                _dbContext.EntityPermissions.RemoveRange(existingPermissions);
                await _dbContext.SaveChangesAsync();
            }

            // Seed fresh permissions
            await _dbContext.SeedEntityPermissionsAsync();
            
            return Ok(new { message = "Entity permissions seeded successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to seed entity permissions", details = ex.Message });
        }
    }

    [HttpGet("check-permissions")]
    public async Task<IActionResult> CheckPermissions()
    {
        var permissions = await _dbContext.EntityPermissions.ToListAsync();
        return Ok(new { 
            count = permissions.Count,
            permissions = permissions.Select(p => new {
                p.Id,
                p.EntityName,
                p.Action,
                p.RoleName,
                p.PropertyName,
                p.FilterExpression
            }).OrderBy(p => p.EntityName).ThenBy(p => p.Action).ThenBy(p => p.RoleName).ToList()
        });
    }
} 