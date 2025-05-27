namespace UNOPS.PAO.UNOPSBusiness.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Entities;

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

    public BusinessSecurityService(UNOPSAppDbContext context, ILogger<BusinessSecurityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string?> GetUserOrgUnitAsync(ClaimsPrincipal user)
    {
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                       user.FindFirst("email")?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("User email not found in claims");
            return null;
        }

        var userInfo = await _context.UserInfos
            .Where(u => u.UserEmail.ToLower() == userEmail.ToLower() && !u.IsDeleted)
            .Select(u => u.OrgUnit)
            .FirstOrDefaultAsync();

        return userInfo;
    }

    public async Task<IQueryable<T>> ApplyRowFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action = "read") where T : class
    {
        // Currently all users can read all entities.
        if (action.ToLower() == "read")
        {
            return query;
        }
        // Early return for administrators - they see everything
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return query;
        }

        var entityName = GetEntityName<T>();
        var userOrgUnit = await GetUserOrgUnitAsync(user);
        var currentUserId = GetCurrentUserId(user);

        return entityName switch
        {
            "Contact" => ApplyContactFilters(query as IQueryable<UNOPSContact>, user, userOrgUnit, currentUserId, action) as IQueryable<T>,
            "Partner" => ApplyBasePartnerFilters(query as IQueryable<Partner>, user, userOrgUnit, currentUserId, action) as IQueryable<T>,
            "UNOPSPartner" => ApplyPartnerFilters(query as IQueryable<UNOPSPartner>, user, userOrgUnit, currentUserId, action) as IQueryable<T>,
            _ => query // No specific filters, return original query
        } ?? query;
    }

    public async Task<bool> CanUserAccessEntityAsync<T>(T entity, ClaimsPrincipal user, string action = "read") where T : class
    {
        // Early return for administrators
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return true;
        }

        var entityName = GetEntityName<T>();

        // If action is not read and user has only UNOPS_GEN_USER role, deny access for non-interaction entities
        if (action.ToLower() != "read" && entityName != "Interaction" && entityName != "UNOPSInteraction")
        {
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            
            // Check if user has only UNOPS_GEN_USER role (and no other roles)
            if (userRoles.Count == 1 && userRoles.Contains("UNOPS_GEN_USER"))
            {
                return false;
            }
        }

        var userOrgUnit = await GetUserOrgUnitAsync(user);
        var currentUserId = GetCurrentUserId(user);

        return entityName switch
        {
            "Contact" => await CanAccessContactCommon(entity as Contact, user, userOrgUnit, currentUserId, action),
            "UNOPSContact" => await CanAccessContactCommon(entity as Contact, user, userOrgUnit, currentUserId, action),
            "Partner" => await CanAccessPartnerCommon(entity as Partner, user, userOrgUnit, currentUserId, action),
            "UNOPSPartner" => await CanAccessPartnerCommon(entity as Partner, user, userOrgUnit, currentUserId, action),
            "Interaction" => await CanAccessInteractionCommon(entity as Interaction, user, userOrgUnit, currentUserId, action),
            "UNOPSInteraction" => await CanAccessInteractionCommon(entity as Interaction, user, userOrgUnit, currentUserId, action),
            _ => true // Default to allow if no specific rule
        };
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

    #region Contact Filters
    private IQueryable<UNOPSContact> ApplyContactFilters(IQueryable<UNOPSContact> query, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        if (user.IsInRole("PARTNER_USER") || user.IsInRole("ORG_UNIT_ADMIN"))
        {
            query = query.Where(contact => (contact.Partner != null && 
                 contact.Partner.PartnerOffice != null && 
                 contact.Partner.PartnerOffice.Code == userOrgUnit));
        }

        return query;
    }

    private async Task<bool> CanAccessContactCommon(Contact? contact, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        if (contact == null) return false;

        if (action.ToLower() == "read")
        {
            return true;
        }

        // Cast to UNOPSContact if needed for partner access
        var unopsContact = contact as UNOPSContact;
        if (unopsContact == null) return false;

        // Load partner and partner office if needed for org unit check
        if (unopsContact.Partner == null && unopsContact.PartnerId > 0)
        {
            unopsContact.Partner = await _context.Partners.OfType<UNOPSPartner>()
                .Include(p => p.PartnerOffice)
                .FirstOrDefaultAsync(p => p.Id == unopsContact.PartnerId);
        }

        // Check if partner office matches user's org unit
        bool partnerOfficeMatches = !string.IsNullOrEmpty(userOrgUnit) && 
                                   unopsContact.Partner?.PartnerOffice?.Code == userOrgUnit;

        return partnerOfficeMatches;
    }
    #endregion

    #region Partner Filters
    private IQueryable<Partner> ApplyBasePartnerFilters(IQueryable<Partner> query, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        return ApplyCommonPartnerFilters(query, user, userOrgUnit, action);
    }

    private IQueryable<UNOPSPartner> ApplyPartnerFilters(IQueryable<UNOPSPartner> query, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        return ApplyCommonPartnerFilters(query, user, userOrgUnit, action).Cast<UNOPSPartner>();
    }

    private IQueryable<T> ApplyCommonPartnerFilters<T>(IQueryable<T> query, ClaimsPrincipal user, string? userOrgUnit, string action) where T : Partner
    {
        if (action.ToLower() == "read" || user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return query;
        }
        
        // Apply org unit filtering
        if (!string.IsNullOrEmpty(userOrgUnit))
        {
            query = query.Where(partner => 
                partner.PartnerOffice != null && partner.PartnerOffice.Code == userOrgUnit);
        }

        return query;
    }

    private async Task<bool> CanAccessPartnerCommon(Partner? partner, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        if (partner == null) return false;

        // For read operations: allow if user created it OR partner office matches
        if (action.ToLower() == "read")
        {
            return true;
        }

        // Cast to UNOPSPartner if needed for partner office access
        var unopsPartner = partner as UNOPSPartner;
        if (unopsPartner == null) return false;

        // Load partner office if needed
        if (unopsPartner.PartnerOffice == null && unopsPartner.PartnerOfficeId.HasValue)
        {
            unopsPartner.PartnerOffice = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(o => o.Id == unopsPartner.PartnerOfficeId.Value);
        }

        // Check if partner office matches user's org unit
        bool partnerOfficeMatches = !string.IsNullOrEmpty(userOrgUnit) && 
                                   unopsPartner.PartnerOffice?.Code == userOrgUnit;

        return partnerOfficeMatches;
    }
    #endregion

    #region Interaction Filters
    private async Task<bool> CanAccessInteractionCommon(Interaction? interaction, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        if (interaction == null) return false;

        if (action.ToLower() == "read")
        {
            return true;
        }

        // Get user roles
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Apply role-specific rules
        if (userRoles.Contains("PARTNER_GLOB_ADMIN") || (!userRoles.Contains("UNOPS_GEN_USER") && action.ToLower() == "create"))
        {
            return true;
        }

        // Cast to UNOPSInteraction if needed for access to properties
        var unopsInteraction = interaction as UNOPSInteraction;
        if (unopsInteraction == null) return false;

        // Load interaction details if needed
        if (unopsInteraction.OrgUnit == null && unopsInteraction.OrgUnitId.HasValue)
        {
            unopsInteraction.OrgUnit = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(o => o.Id == unopsInteraction.OrgUnitId.Value);
        }

        // Load interaction users if needed for user-based access
        var interactionUsers = await _context.InteractionUsers
            .Where(iu => iu.InteractionId == unopsInteraction.Id)
            .ToListAsync();

        // Check if current user is in the interaction users
        bool isUserInInteraction = interactionUsers.Any(iu => iu.UserId == currentUserId);

        // Check if interaction org unit matches user's org unit
        bool orgUnitMatches = !string.IsNullOrEmpty(userOrgUnit) && 
                             unopsInteraction.OrgUnit?.Code == userOrgUnit;

        // Apply role-specific rules
        if (userRoles.Contains("UNOPS_GEN_USER"))
        {
            if (action.ToLower() == "update")
            {
                // Can edit when interaction org unit = user's org unit OR user is in InteractionUsers
                return orgUnitMatches || isUserInInteraction;
            }
        }
        else
        {
            if (action.ToLower() == "update")
            {
                // Can update when interaction org unit = user's org unit OR user is in InteractionUsers
                return orgUnitMatches || isUserInInteraction;
            }
            else if (action.ToLower() == "delete")
            {
                // Can delete when interaction org unit = user's org unit
                return orgUnitMatches;
            }
        }

        // Default deny for other roles or actions
        return false;
    }
    #endregion

    private int GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         user.FindFirst("sub")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Gets the entity name for permission checking, normalizing UNOPS-prefixed entities to their base names
    /// </summary>
    private string GetEntityName<T>()
    {
        var typeName = typeof(T).Name;
        
        return typeName;
    }

    public async Task<bool> CanUserAccessEntityAsync(ClaimsPrincipal user, string entityName, int entityId, string action = "read")
    {
        // Early return for administrators
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return true;
        }

        return entityName.ToLower() switch
        {
            "contact" => await CanUserAccessContactByIdAsync(user, entityId, action),
            "unopscontact" => await CanUserAccessContactByIdAsync(user, entityId, action),
            "partner" => await CanUserAccessPartnerByIdAsync(user, entityId, action),
            "unopspartner" => await CanUserAccessPartnerByIdAsync(user, entityId, action),
            "interaction" => await CanUserAccessInteractionByIdAsync(user, entityId, action),
            "unopsinteraction" => await CanUserAccessInteractionByIdAsync(user, entityId, action),
            _ => true // Default to allow if no specific rule
        };
    }

    public async Task<EntityPermissionsModel> GetEntityPermissionsAsync(ClaimsPrincipal user, string entityName)
    {
        // For administrators, all permissions are true
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return new EntityPermissionsModel { CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true };
        }

        var userOrgUnit = await GetUserOrgUnitAsync(user);

        return entityName.ToLower() switch
        {
            "contact" => await GetContactPermissionsAsync(user, userOrgUnit),
            "unopscontact" => await GetContactPermissionsAsync(user, userOrgUnit),
            "partner" => await GetPartnerPermissionsAsync(user, userOrgUnit),
            "unopspartner" => await GetPartnerPermissionsAsync(user, userOrgUnit),
            "interaction" => await GetInteractionPermissionsAsync(user, userOrgUnit),
            "unopsinteraction" => await GetInteractionPermissionsAsync(user, userOrgUnit),
            _ => new EntityPermissionsModel { CanRead = true, CanCreate = false, CanUpdate = false, CanDelete = false }
        };
    }

    #region Entity-specific permission methods
    private async Task<bool> CanUserAccessContactByIdAsync(ClaimsPrincipal user, int contactId, string action)
    {
        var contact = await _context.Contacts.OfType<UNOPSContact>()
            .Include(c => c.Partner)
                .ThenInclude(p => p.PartnerOffice)
            .FirstOrDefaultAsync(c => c.Id == contactId);

        if (contact == null) return false;

        return await CanAccessContactCommon(contact, user, await GetUserOrgUnitAsync(user), GetCurrentUserId(user), action);
    }

    private async Task<bool> CanUserAccessPartnerByIdAsync(ClaimsPrincipal user, int partnerId, string action)
    {
        var partner = await _context.Partners.OfType<UNOPSPartner>()
            .Include(p => p.PartnerOffice)
            .FirstOrDefaultAsync(p => p.Id == partnerId);

        if (partner == null) return false;

        return await CanAccessPartnerCommon(partner, user, await GetUserOrgUnitAsync(user), GetCurrentUserId(user), action);
    }

    private async Task<bool> CanUserAccessInteractionByIdAsync(ClaimsPrincipal user, int interactionId, string action)
    {
        var interaction = await _context.Interactions.OfType<UNOPSInteraction>()
            .Include(i => i.Contact)
                .ThenInclude(c => c.Partner)
                    .ThenInclude(p => p.PartnerOffice)
            .FirstOrDefaultAsync(i => i.Id == interactionId);

        if (interaction == null) return false;

        return await CanAccessInteractionCommon(interaction, user, await GetUserOrgUnitAsync(user), GetCurrentUserId(user), action);
    }

    private async Task<EntityPermissionsModel> GetEntityPermissionsFromDatabaseAsync(ClaimsPrincipal user, string entityName)
    {
        // Get user roles
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // If no roles found, return no permissions
        if (!userRoles.Any())
        {
            return new EntityPermissionsModel { CanRead = false, CanCreate = false, CanUpdate = false, CanDelete = false };
        }

        // Query EntityPermissions table for the specified entity with user's roles
        var permissions = await _context.EntityPermissions
            .Where(ep => ep.Entity == entityName && userRoles.Contains(ep.Role))
            .ToListAsync();

        // If no permissions found for any role, return default no access
        if (!permissions.Any())
        {
            return new EntityPermissionsModel { CanRead = false, CanCreate = false, CanUpdate = false, CanDelete = false };
        }

        // Aggregate permissions across all roles (use OR logic - if any role allows, then allow)
        return new EntityPermissionsModel
        {
            CanRead = permissions.Any(p => p.CanRead),
            CanCreate = permissions.Any(p => p.CanCreate),
            CanUpdate = permissions.Any(p => p.CanUpdate),
            CanDelete = permissions.Any(p => p.CanDelete)
        };
    }

    private async Task<EntityPermissionsModel> GetContactPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        // Note: Row-level filtering (org unit checking) is still applied in the 
        // ApplyContactFilters and CanAccessContact methods
        return await GetEntityPermissionsFromDatabaseAsync(user, "Contact");
    }

    private async Task<EntityPermissionsModel> GetPartnerPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        // Note: Row-level filtering (org unit checking) is still applied in the 
        // ApplyPartnerFilters and CanAccessPartner methods
        return await GetEntityPermissionsFromDatabaseAsync(user, "Partner");
    }

    private async Task<EntityPermissionsModel> GetInteractionPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        // Note: Row-level filtering (org unit checking) is still applied in the 
        // ApplyInteractionFilters and CanAccessInteraction methods
        return await GetEntityPermissionsFromDatabaseAsync(user, "Interaction");
    }
    #endregion
} 