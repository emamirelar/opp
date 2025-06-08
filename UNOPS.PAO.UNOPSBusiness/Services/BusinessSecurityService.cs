namespace UNOPS.PAO.UNOPSBusiness.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Domain.Enums;

public interface IBusinessSecurityService
{
    Task<string?> GetUserOrgUnitAsync(ClaimsPrincipal user);
    Task<IQueryable<T>> ApplyRowFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action = "read") where T : class;
    Task<bool> CanUserAccessEntityAsync<T>(T entity, ClaimsPrincipal user, string action = "read") where T : class;
    Task<object> GetEntityPermissionsAsync<T>(T entity, ClaimsPrincipal user) where T : class;
    
    // New overloads for PermissionsController
    Task<bool> CanUserAccessEntityAsync(ClaimsPrincipal user, string entityName, int entityId, string action = "read");
    Task<EntityPermissionsModel> GetEntityPermissionsAsync(ClaimsPrincipal user, string entityName);
}

public class BusinessSecurityService : IBusinessSecurityService
{
    private readonly UNOPSAppDbContext _context;
    private readonly ILogger<BusinessSecurityService> _logger;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly IGenericRowFilterService _genericRowFilterService;

    public BusinessSecurityService(
        UNOPSAppDbContext context, 
        ILogger<BusinessSecurityService> logger, 
        UserManager<PAOIdentityUser> userManager,
        IGenericRowFilterService genericRowFilterService)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _genericRowFilterService = genericRowFilterService;
    }

    public async Task<string?> GetUserOrgUnitAsync(ClaimsPrincipal user)
    {
        _logger.LogInformation("DEBUG - GetUserOrgUnitAsync called");
        
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                       user.FindFirst("email")?.Value;
        
        _logger.LogInformation("DEBUG - User email from claims: {Email}", userEmail);
        
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("DEBUG - User email not found in claims");
            return null;
        }

        try
        {   
            // For external users, look up their assigned org unit
            // This would typically involve a database lookup
            _logger.LogInformation("DEBUG - User is external, looking up org unit in database");
            
            // Check if there's a UserInfos table lookup (restore original logic if it exists)
            var userInfo = await _context.UserInfos
                .Where(u => u.UserEmail.ToLower() == userEmail.ToLower() && !u.IsDeleted)
                .Select(u => u.OrgUnit)
                .FirstOrDefaultAsync();
                
            _logger.LogInformation("DEBUG - UserInfo lookup result: {OrgUnit}", userInfo);
            
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DEBUG - Error getting user org unit for email: {Email}", userEmail);
            return null;
        }
    }

    public async Task<IQueryable<T>> ApplyRowFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action = "read") where T : class
    {
        // Early return for administrators - they see everything
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return query;
        }

        // Use the generic row filter service for data-driven filtering
        return await _genericRowFilterService.ApplyRowFiltersAsync(query, user, action);
    }

    public async Task<bool> CanUserAccessEntityAsync<T>(T entity, ClaimsPrincipal user, string action = "read") where T : class
    {
        // Use the generic row filter service for data-driven filtering
        // Note: Removed hard-coded PARTNER_GLOB_ADMIN bypass to ensure all users go through row filters
        return await _genericRowFilterService.CanUserAccessEntityAsync(entity, user, action);
    }

    public async Task<object> GetEntityPermissionsAsync<T>(T entity, ClaimsPrincipal user) where T : class
    {
        // For administrators, all permissions are true
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return new
            {
                canRead = true,
                canUpdate = true,
                canDelete = true,
                canCreate = true
            };
        }

        return new
        {
            canRead = await CanUserAccessEntityAsync(entity, user, "read"),
            canUpdate = await CanUserAccessEntityAsync(entity, user, "update"),
            canDelete = await CanUserAccessEntityAsync(entity, user, "delete"),
            canCreate = await CanUserAccessEntityAsync(entity, user, "create")
        };
    }

    // Keep the existing methods for backward compatibility and fallback
    private int GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         user.FindFirst("sub")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetEntityName<T>()
    {
        var typeName = typeof(T).Name;
        return typeName;
    }

    // New overloads for PermissionsController
    public async Task<bool> CanUserAccessEntityAsync(ClaimsPrincipal user, string entityName, int entityId, string action = "read")
    {
        // Use the generic row filter service for data-driven filtering
        // Note: Removed hard-coded PARTNER_GLOB_ADMIN bypass to ensure all users go through row filters
        
        return entityName.ToLower() switch
        {
            "contact" => await CanUserAccessContactByIdAsync(user, entityId, action),
            "unopscontact" => await CanUserAccessContactByIdAsync(user, entityId, action),
            "partner" => await CanUserAccessPartnerByIdAsync(user, entityId, action),
            "unopspartner" => await CanUserAccessPartnerByIdAsync(user, entityId, action),
            "interaction" => await CanUserAccessInteractionByIdAsync(user, entityId, action),
            "unopsinteraction" => await CanUserAccessInteractionByIdAsync(user, entityId, action),
            "partnertree" => await CanUserAccessPartnerTreeAsync(user, action),
            _ => true // Default to allow if no specific rule
        };
    }

    public async Task<EntityPermissionsModel> GetEntityPermissionsAsync(ClaimsPrincipal user, string entityName)
    {
        _logger.LogInformation("DEBUG - GetEntityPermissionsAsync called for entity: {EntityName}", entityName);
        
        // For administrators, all permissions are true
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            _logger.LogInformation("DEBUG - User has PARTNER_GLOB_ADMIN role, returning full permissions");
            return new EntityPermissionsModel { CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true };
        }

        var userOrgUnit = await GetUserOrgUnitAsync(user);
        _logger.LogInformation("DEBUG - User org unit: {UserOrgUnit}", userOrgUnit);

        var result = entityName.ToLower() switch
        {
            "contact" => await GetContactPermissionsAsync(user, userOrgUnit),
            "unopscontact" => await GetContactPermissionsAsync(user, userOrgUnit),
            "partner" => await GetPartnerPermissionsAsync(user, userOrgUnit),
            "unopspartner" => await GetPartnerPermissionsAsync(user, userOrgUnit),
            "interaction" => await GetInteractionPermissionsAsync(user, userOrgUnit),
            "unopsinteraction" => await GetInteractionPermissionsAsync(user, userOrgUnit),
            "partnertree" => await GetPartnerTreePermissionsAsync(user, userOrgUnit),
            "usermanagement" => await GetEntityPermissionsFromDatabaseAsync(user, "UserManagement"),
            "aipromptmanagement" => await GetEntityPermissionsFromDatabaseAsync(user, "AiPromptManagement"),
            "entitymanager" => await GetEntityPermissionsFromDatabaseAsync(user, "EntityManager"),
            _ => new EntityPermissionsModel { CanRead = true, CanCreate = false, CanUpdate = false, CanDelete = false }
        };
        
        _logger.LogInformation("DEBUG - Final permissions for {EntityName}: CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}", 
            entityName, result.CanRead, result.CanCreate, result.CanUpdate, result.CanDelete);
            
        return result;
    }

    #region Entity-specific permission methods
    private async Task<bool> CanUserAccessContactByIdAsync(ClaimsPrincipal user, int contactId, string action)
    {
        var contact = await _context.Contacts.OfType<UNOPSContact>()
            .Include(c => c.Partner)
                .ThenInclude(p => p.PartnerOffice)
            .FirstOrDefaultAsync(c => c.Id == contactId);

        if (contact == null) return false;

        return await _genericRowFilterService.CanUserAccessEntityAsync(contact, user, action);
    }

    private async Task<bool> CanUserAccessPartnerByIdAsync(ClaimsPrincipal user, int partnerId, string action)
    {
        var partner = await _context.Partners.OfType<UNOPSPartner>()
            .Include(p => p.PartnerOffice)
            .FirstOrDefaultAsync(p => p.Id == partnerId);

        if (partner == null) return false;

        return await _genericRowFilterService.CanUserAccessEntityAsync(partner, user, action);
    }

    private async Task<bool> CanUserAccessInteractionByIdAsync(ClaimsPrincipal user, int interactionId, string action)
    {
        var interaction = await _context.Interactions.OfType<UNOPSInteraction>()
            .Include(i => i.Contact)
                .ThenInclude(c => c.Partner)
                    .ThenInclude(p => p.PartnerOffice)
            .FirstOrDefaultAsync(i => i.Id == interactionId);

        if (interaction == null) return false;

        return await _genericRowFilterService.CanUserAccessEntityAsync(interaction, user, action);
    }

    private async Task<EntityPermissionsModel> GetEntityPermissionsFromDatabaseAsync(ClaimsPrincipal user, string entityName)
    {
        _logger.LogInformation("DEBUG - GetEntityPermissionsFromDatabaseAsync called for entity: {EntityName}", entityName);
        
        // Get user roles from claims first
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        _logger.LogInformation("DEBUG - User roles from claims: {Roles}", string.Join(", ", userRoles));

        // FALLBACK: If no roles found in claims, get them directly from the database
        if (!userRoles.Any())
        {
            _logger.LogWarning("DEBUG - No roles found in claims, attempting to get roles from database");
            
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("DEBUG - Fallback lookup using Email: {Email}, UserId: {UserId}", userEmail, userId);
            
            try
            {
                PAOIdentityUser dbUser = null;
                
                // Try to find user by ID first, then by email
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var userIdInt))
                {
                    dbUser = await _userManager.FindByIdAsync(userId);
                    _logger.LogInformation("DEBUG - User found by ID: {UserFound}", dbUser != null);
                }
                
                if (dbUser == null && !string.IsNullOrEmpty(userEmail))
                {
                    dbUser = await _userManager.FindByEmailAsync(userEmail);
                    _logger.LogInformation("DEBUG - User found by email: {UserFound}", dbUser != null);
                }
                
                if (dbUser != null)
                {
                    userRoles = (await _userManager.GetRolesAsync(dbUser)).ToList();
                    _logger.LogInformation("DEBUG - Roles from database: {Roles}", string.Join(", ", userRoles));
                }
                else
                {
                    _logger.LogWarning("DEBUG - User not found in database with email {Email} or ID {UserId}", userEmail, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DEBUG - Error during fallback role lookup for user {Email}/{UserId}", userEmail, userId);
            }
        }

        // If still no roles found, return no permissions
        if (!userRoles.Any())
        {
            _logger.LogWarning("DEBUG - No roles found for user even after database fallback, returning no permissions");
            return new EntityPermissionsModel { CanRead = false, CanCreate = false, CanUpdate = false, CanDelete = false };
        }

        // Query EntityPermissions table for the specified entity with user's roles
        _logger.LogInformation("DEBUG - Querying EntityPermissions table for entity: {EntityName} with roles: {Roles}", 
            entityName, string.Join(", ", userRoles));
            
        var permissions = await _context.EntityPermissions
            .Where(ep => ep.Entity == entityName && userRoles.Contains(ep.Role))
            .ToListAsync();

        _logger.LogInformation("DEBUG - Found {Count} permission records in database", permissions.Count);
        
        foreach (var permission in permissions)
        {
            _logger.LogInformation("DEBUG - Permission record: Entity={Entity}, Role={Role}, CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}", 
                permission.Entity, permission.Role, permission.CanRead, permission.CanCreate, permission.CanUpdate, permission.CanDelete);
        }

        // If no permissions found for any role, return default no access
        if (!permissions.Any())
        {
            _logger.LogWarning("DEBUG - No permissions found in database for entity {EntityName} with roles {Roles}, returning no access", 
                entityName, string.Join(", ", userRoles));
            return new EntityPermissionsModel { CanRead = false, CanCreate = false, CanUpdate = false, CanDelete = false };
        }

        // Aggregate permissions across all roles (use OR logic - if any role allows, then allow)
        var result = new EntityPermissionsModel
        {
            CanRead = permissions.Any(p => p.CanRead),
            CanCreate = permissions.Any(p => p.CanCreate),
            CanUpdate = permissions.Any(p => p.CanUpdate),
            CanDelete = permissions.Any(p => p.CanDelete)
        };
        
        _logger.LogInformation("DEBUG - Aggregated permissions result: CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}", 
            result.CanRead, result.CanCreate, result.CanUpdate, result.CanDelete);
            
        return result;
    }

    private async Task<EntityPermissionsModel> GetContactPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        return await GetEntityPermissionsFromDatabaseAsync(user, "Contact");
    }

    private async Task<EntityPermissionsModel> GetPartnerPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        return await GetEntityPermissionsFromDatabaseAsync(user, "Partner");
    }

    private async Task<EntityPermissionsModel> GetInteractionPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        return await GetEntityPermissionsFromDatabaseAsync(user, "Interaction");
    }

    private async Task<EntityPermissionsModel> GetPartnerTreePermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        return await GetEntityPermissionsFromDatabaseAsync(user, "PartnerTree");
    }

    private async Task<bool> CanUserAccessPartnerTreeAsync(ClaimsPrincipal user, string action)
    {
        // Get permissions from database
        var permissions = await GetPartnerTreePermissionsAsync(user, null);
        
        return action.ToLower() switch
        {
            "read" => permissions.CanRead,
            "create" => permissions.CanCreate,
            "update" => permissions.CanUpdate,
            "delete" => permissions.CanDelete,
            _ => false
        };
    }
    #endregion
} 