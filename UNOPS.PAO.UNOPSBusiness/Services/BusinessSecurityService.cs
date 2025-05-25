namespace UNOPS.PAO.UNOPSBusiness.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models;

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
            "Partner" => ApplyPartnerFilters(query as IQueryable<UNOPSPartner>, user, userOrgUnit, currentUserId, action) as IQueryable<T>,
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
        var userOrgUnit = await GetUserOrgUnitAsync(user);
        var currentUserId = GetCurrentUserId(user);

        return entityName switch
        {
            "Contact" => await CanAccessContact(entity as UNOPSContact, user, userOrgUnit, currentUserId, action),
            "Partner" => await CanAccessPartner(entity as UNOPSPartner, user, userOrgUnit, currentUserId, action),
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
        if (user.IsInRole("PARTNER_USER"))
        {
            // Partners can see contacts where:
            // 1. They created the contact, OR
            // 2. The contact's partner office code matches their org unit
            query = query.Where(contact => 
                contact.CreatedBy == currentUserId || 
                (contact.Partner != null && 
                 contact.Partner.PartnerOffice != null && 
                 contact.Partner.PartnerOffice.Code == userOrgUnit));
        }
        else if (user.IsInRole("UNOPS_GEN_USER") || user.IsInRole("ORG_UNIT_ADMIN"))
        {
            // Internal users can see contacts in their org unit
            if (!string.IsNullOrEmpty(userOrgUnit))
            {
                query = query.Where(contact => 
                    contact.Partner != null && 
                    contact.Partner.PartnerOffice != null && 
                    contact.Partner.PartnerOffice.Code == userOrgUnit);
            }
        }

        return query;
    }

    private async Task<bool> CanAccessContact(UNOPSContact? contact, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        if (contact == null) return false;

        if (action.ToLower() == "read")
        {
            return true;
        }

        // Load partner and partner office if needed for org unit check
        if (contact.Partner == null && contact.PartnerId > 0)
        {
            contact.Partner = await _context.Partners.OfType<UNOPSPartner>()
                .Include(p => p.PartnerOffice)
                .FirstOrDefaultAsync(p => p.Id == contact.PartnerId);
        }

        // Check if partner office matches user's org unit
        bool partnerOfficeMatches = !string.IsNullOrEmpty(userOrgUnit) && 
                                   contact.Partner?.PartnerOffice?.Code == userOrgUnit;

        // For create, update, delete operations: ONLY allow if partner office matches user's org unit
        if (action.ToLower() is "create" or "update" or "delete")
        {
            return partnerOfficeMatches;
        }

        // Default to partner office match for any other actions
        return partnerOfficeMatches;
    }
    #endregion

    #region Partner Filters
    private IQueryable<UNOPSPartner> ApplyPartnerFilters(IQueryable<UNOPSPartner> query, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        if (user.IsInRole("PARTNER_USER"))
        {
            // For read operations, allow both created and org unit matches
            if (action.ToLower() == "read")
            {
                query = query.Where(partner => 
                    partner.CreatedBy == currentUserId || 
                    (partner.PartnerOffice != null && partner.PartnerOffice.Code == userOrgUnit));
            }
            else
            {
                // For create/update/delete, only allow if partner office matches user's org unit
                if (!string.IsNullOrEmpty(userOrgUnit))
                {
                    query = query.Where(partner => 
                        partner.PartnerOffice != null && partner.PartnerOffice.Code == userOrgUnit);
                }
            }
        }
        else if (user.IsInRole("UNOPS_GEN_USER") || user.IsInRole("ORG_UNIT_ADMIN"))
        {
            if (!string.IsNullOrEmpty(userOrgUnit))
            {
                query = query.Where(partner => 
                    partner.PartnerOffice != null && partner.PartnerOffice.Code == userOrgUnit);
            }
        }

        return query;
    }

    private async Task<bool> CanAccessPartner(UNOPSPartner? partner, ClaimsPrincipal user, string? userOrgUnit, int currentUserId, string action)
    {
        if (partner == null) return false;

        // Load partner office if needed
        if (partner.PartnerOffice == null && partner.PartnerOfficeId.HasValue)
        {
            partner.PartnerOffice = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(o => o.Id == partner.PartnerOfficeId.Value);
        }

        // Check if partner office matches user's org unit
        bool partnerOfficeMatches = !string.IsNullOrEmpty(userOrgUnit) && 
                                   partner.PartnerOffice?.Code == userOrgUnit;

        // For create, update, delete operations: ONLY allow if partner office matches user's org unit
        if (action.ToLower() is "create" or "update" or "delete")
        {
            return partnerOfficeMatches;
        }

        // For read operations: allow if user created it OR partner office matches
        if (action.ToLower() == "read")
        {
            bool isCreator = partner.CreatedBy == currentUserId;
            return isCreator || partnerOfficeMatches;
        }

        // Default to partner office match for any other actions
        return partnerOfficeMatches;
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
        
        // Remove UNOPS prefix if present
        if (typeName.StartsWith("UNOPS"))
        {
            return typeName.Substring(5); // Remove "UNOPS" prefix
        }
        
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
            "partner" => await CanUserAccessPartnerByIdAsync(user, entityId, action),
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
            "partner" => await GetPartnerPermissionsAsync(user, userOrgUnit),
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

        return await CanAccessContact(contact, user, await GetUserOrgUnitAsync(user), GetCurrentUserId(user), action);
    }

    private async Task<bool> CanUserAccessPartnerByIdAsync(ClaimsPrincipal user, int partnerId, string action)
    {
        var partner = await _context.Partners.OfType<UNOPSPartner>()
            .Include(p => p.PartnerOffice)
            .FirstOrDefaultAsync(p => p.Id == partnerId);

        if (partner == null) return false;

        return await CanAccessPartner(partner, user, await GetUserOrgUnitAsync(user), GetCurrentUserId(user), action);
    }

    private async Task<EntityPermissionsModel> GetContactPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        // Base permissions based on role
        bool canRead = user.IsInRole("PARTNER_USER") || user.IsInRole("UNOPS_GEN_USER") || user.IsInRole("ORG_UNIT_ADMIN");
        bool canCreate = user.IsInRole("PARTNER_USER") || user.IsInRole("ORG_UNIT_ADMIN");
        bool canUpdate = user.IsInRole("PARTNER_USER") || user.IsInRole("ORG_UNIT_ADMIN");
        bool canDelete = user.IsInRole("PARTNER_USER") || user.IsInRole("ORG_UNIT_ADMIN");

        return new EntityPermissionsModel { CanRead = canRead, CanCreate = canCreate, CanUpdate = canUpdate, CanDelete = canDelete };
    }

    private async Task<EntityPermissionsModel> GetPartnerPermissionsAsync(ClaimsPrincipal user, string? userOrgUnit)
    {
        // Base permissions based on role
        bool canRead = user.IsInRole("PARTNER_USER") || user.IsInRole("UNOPS_GEN_USER") || user.IsInRole("ORG_UNIT_ADMIN");
        bool canCreate = user.IsInRole("ORG_UNIT_ADMIN"); // Only org unit admins can create partners
        bool canUpdate = user.IsInRole("PARTNER_USER") || user.IsInRole("ORG_UNIT_ADMIN");
        bool canDelete = user.IsInRole("ORG_UNIT_ADMIN"); // Only org unit admins can delete partners

        return new EntityPermissionsModel { CanRead = canRead, CanCreate = canCreate, CanUpdate = canUpdate, CanDelete = canDelete };
    }
    #endregion
} 