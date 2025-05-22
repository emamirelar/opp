namespace UNOPS.PAO.UNOPSBusiness.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;

public class PermissionService : IPermissionService
{
    private readonly UNOPSAppDbContext _context;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;
    private readonly IMemoryCache _cache;

    public PermissionService(
        UNOPSAppDbContext context,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        IMemoryCache cache)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _cache = cache;
    }
    
    private async Task<bool> CheckPermissionAsync(string entityName, ClaimsPrincipal user, object? entity, string operation)
    {
        // Get user ID
        var userId = _userManager.GetUserId(user);
        if (userId == null) return false;
        
        // Get user roles
        var identityUser = await _userManager.FindByIdAsync(userId);
        if (identityUser == null) return false;
        
        var userRoles = await _userManager.GetRolesAsync(identityUser);
        
        // Check if any role has the requested permission for this entity
        var permissions = await _context.EntityPermissions
            .Where(p => p.Entity == entityName && userRoles.Contains(p.Role))
            .ToListAsync();
            
        foreach (var permission in permissions)
        {
            if ((operation == "read" && permission.CanRead) ||
                (operation == "create" && permission.CanCreate) ||
                (operation == "update" && permission.CanUpdate) ||
                (operation == "delete" && permission.CanDelete))
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Implementation of IPermissionService interface method
    public async Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object? entity = null)
    {
        // Map the action to the appropriate CRUD operation
        return action.ToLowerInvariant() switch
        {
            "read" => await CheckPermissionAsync(entityName, user, entity, "read"),
            "create" => await CheckPermissionAsync(entityName, user, entity, "create"),
            "update" => await CheckPermissionAsync(entityName, user, entity, "update"),
            "delete" => await CheckPermissionAsync(entityName, user, entity, "delete"),
            _ => false // Unknown actions are not allowed
        };
    }
} 