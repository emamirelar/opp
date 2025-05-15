namespace UNOPS.PAO.UNOPSPresentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Seed;

[Route("api/[controller]")]
[Authorize(Roles = "Administrator", AuthenticationSchemes = "IAP")]
public class AdminController : BaseController
{
    private readonly UNOPSAppDbContext _dbContext;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;

    public AdminController(
        UNOPSAppDbContext dbContext,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        ILogger<AdminController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("setup-admin")]
    [AllowAnonymous] // Temporarily allow anonymous access for initial setup
    public async Task<ActionResult> SetupAdminUser(string email, string password)
    {
        return await HandleOperationAsync(async () =>
        {
            if (_userManager == null || _roleManager == null)
            {
                throw new BusinessException("User management is not available");
            }

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
                    throw new BusinessException("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Assign Administrator role to user
            if (!await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                await _userManager.AddToRoleAsync(user, "Administrator");
            }

            return new { message = $"Admin user {email} setup successfully! You can now login with this account." };
        });
    }

    [HttpPost("seed-permissions")]
    [Authorize(Roles = "Administrator", AuthenticationSchemes = "IAP")]
    public async Task<ActionResult> SeedEntityPermissions()
    {
        return await HandleOperationAsync(async () =>
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
            
            return new { message = "Entity permissions seeded successfully!" };
        });
    }

    [HttpGet("check-permissions")]
    public async Task<ActionResult> CheckPermissions()
    {
        return await HandleOperationAsync(async () =>
        {
            var permissions = await _dbContext.EntityPermissions.ToListAsync();
            return new { 
                count = permissions.Count,
                permissions = permissions.Select(p => new {
                    p.Id,
                    p.EntityName,
                    p.Action,
                    p.RoleName,
                    p.PropertyName,
                    p.FilterExpression
                }).OrderBy(p => p.EntityName).ThenBy(p => p.Action).ThenBy(p => p.RoleName).ToList()
            };
        });
    }
} 