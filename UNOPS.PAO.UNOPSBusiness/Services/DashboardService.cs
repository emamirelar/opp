using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Entities;
using System.Collections.Generic;
using System.Collections;
using AutoMapper;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDomain.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Dedicated service for dashboard data retrieval with user-specific filtering and RBAC support
/// This service keeps dashboard logic separate from core entity APIs
/// </summary>
public class DashboardService : BaseUNOPSManager, IDashboardService
{
    private readonly ILogger<DashboardService> _logger;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IOrgUnitHierarchyService _hierarchyService;

    public DashboardService(
        UNOPSAppDbContext context, 
        ILogger<DashboardService> logger, 
        IMapper mapper,
        IConfiguration configuration,
        IUserPreferenceService userPreferenceService,
        IOrgUnitHierarchyService hierarchyService,
        IPermissionService permissionService = null,
        IHttpContextAccessor httpContextAccessor = null)
        : base(mapper, context, configuration, null, "Dashboard", permissionService, httpContextAccessor)
    {
        _logger = logger;
        _userPreferenceService = userPreferenceService;
        _hierarchyService = hierarchyService;
    }

    /// <summary>
    /// Helper method to apply access control filters with a specific entity name
    /// since the dashboard service handles multiple entity types
    /// </summary>
    private async Task<IEnumerable<T>> ApplyAccessControlFiltersWithEntityName<T>(IQueryable<T> query, ClaimsPrincipal user, string action, string entityName) where T : class
    {
        if (_permissionService == null)
        {
            _logger.LogWarning("No permission service available for RBAC filtering - returning empty list");
            return new List<T>();
        }

        try
        {
            var result = await _permissionService.ApplyAccessControlFiltersAsync(query, user, action, entityName);
            
            // Cast the result back to the expected type, following the pattern from BaseUNOPSManager
            if (result is List<T> typedList)
            {
                return typedList;
            }
            
            // If it's some other enumerable, convert it
            if (result is IEnumerable<T> enumerable)
            {
                return enumerable.ToList();
            }
            
            // Fallback: return empty list
            _logger.LogWarning("RBAC filtering returned unexpected type {Type} for entity {EntityName}", 
                result?.GetType().Name ?? "null", entityName);
            return new List<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying RBAC filters for entity {EntityName}", entityName);
            return new List<T>();
        }
    }

    /// <summary>
    /// Gets partners that are related to the current user (created by or last modified by)
    /// and excludes Draft status partners with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetMyPartnersAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard partners request");
            return new PaginationResponse<PartnerModel> { Records = new List<PartnerModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting dashboard partners for user {UserId} with RBAC filtering", userId.Value);

        var query = _context.Set<UNOPSPartner>()
            .Include(p => p.PartnerGroup)
            .Include(p => p.Projects)
            .Where(p => (p.CreatedBy == userId.Value || p.LastModifiedBy == userId.Value) 
                       && p.Status != Domain.Entities.EntityStatus.Draft)
            .OrderByDescending(p => p.LastModifiedDate ?? p.CreatedDate);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Partner");
        
        var partnerArray = filteredData.ToArray();
        var totalCount = partnerArray.Length;
        var paginatedEntities = partnerArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<PartnerModel>>(paginatedEntities);

        _logger.LogInformation("Found {Count} dashboard partners for user {UserId} after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<PartnerModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets contacts that are related to the current user (created by or last modified by)
    /// and excludes Draft status contacts with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<ContactModel>> GetMyContactsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard contacts request");
            return new PaginationResponse<ContactModel> { Records = new List<ContactModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting dashboard contacts for user {UserId} with RBAC filtering", userId.Value);

        var query = _context.Set<UNOPSContact>()
            .Include(c => c.Partner)
            .Where(c => (c.CreatedBy == userId.Value || c.LastModifiedBy == userId.Value) 
                       && c.Status != "Draft")
            .OrderByDescending(c => c.LastModifiedDate ?? c.CreatedDate);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Contact");
        
        var contactArray = filteredData.ToArray();
        var totalCount = contactArray.Length;
        var paginatedEntities = contactArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<ContactModel>>(paginatedEntities);

        _logger.LogInformation("Found {Count} dashboard contacts for user {UserId} after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<ContactModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets draft partners that are related to the current user (created by or last modified by) with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetMyDraftPartnersAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for draft partners request");
            return new PaginationResponse<PartnerModel> { Records = new List<PartnerModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting draft partners for user {UserId} with RBAC filtering", userId.Value);

        var query = _context.Set<UNOPSPartner>()
            .Include(p => p.PartnerGroup)
            .Where(p => (p.CreatedBy == userId.Value || p.LastModifiedBy == userId.Value) 
                       && p.Status == Domain.Entities.EntityStatus.Draft)
            .OrderByDescending(p => p.CreatedDate);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Partner");
        
        var partnerArray = filteredData.ToArray();
        var totalCount = partnerArray.Length;
        var paginatedEntities = partnerArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<PartnerModel>>(paginatedEntities);

        _logger.LogInformation("Found {Count} draft partners for user {UserId} after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<PartnerModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets draft contacts that are related to the current user (created by or last modified by) with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<ContactModel>> GetMyDraftContactsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for draft contacts request");
            return new PaginationResponse<ContactModel> { Records = new List<ContactModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting draft contacts for user {UserId} with RBAC filtering", userId.Value);

        var query = _context.Set<UNOPSContact>()
            .Include(c => c.Partner)
            .Where(c => (c.CreatedBy == userId.Value || c.LastModifiedBy == userId.Value) 
                       && c.Status == "Draft")
            .OrderByDescending(c => c.CreatedDate);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Contact");
        
        var contactArray = filteredData.ToArray();
        var totalCount = contactArray.Length;
        var paginatedEntities = contactArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<ContactModel>>(paginatedEntities);

        _logger.LogInformation("Found {Count} draft contacts for user {UserId} after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<ContactModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets interactions that are related to the current user (created by or last modified by)
    /// and excludes Draft status interactions with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<InteractionModel>> GetMyInteractionsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard interactions request");
            return new PaginationResponse<InteractionModel> { Records = new List<InteractionModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting dashboard interactions for user {UserId} with RBAC filtering", userId.Value);

        var query = _context.Set<Interaction>()
            .Include(i => i.InteractionContacts)
            .Include(i => i.InteractionPartners)
            .Where(i => (i.CreatedBy == userId.Value || i.LastModifiedBy == userId.Value) 
                       && i.Status != EntityStatus.Draft)
            .OrderByDescending(i => i.LastModifiedDate ?? i.CreatedDate);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Interaction");
        
        var interactionArray = filteredData.ToArray();
        var totalCount = interactionArray.Length;
        var paginatedEntities = interactionArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<InteractionModel>>(paginatedEntities);

        _logger.LogInformation("Found {Count} dashboard interactions for user {UserId} after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<InteractionModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets draft interactions that are related to the current user (created by or last modified by) with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<InteractionModel>> GetMyDraftInteractionsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for draft interactions request");
            return new PaginationResponse<InteractionModel> { Records = new List<InteractionModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting draft interactions for user {UserId} with RBAC filtering", userId.Value);

        var query = _context.Set<Interaction>()
            .Include(i => i.InteractionContacts)
            .Include(i => i.InteractionPartners)
            .Where(i => (i.CreatedBy == userId.Value || i.LastModifiedBy == userId.Value) 
                       && i.Status == EntityStatus.Draft)
            .OrderByDescending(i => i.CreatedDate);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Interaction");
        
        var interactionArray = filteredData.ToArray();
        var totalCount = interactionArray.Length;
        var paginatedEntities = interactionArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<InteractionModel>>(paginatedEntities);

        _logger.LogInformation("Found {Count} draft interactions for user {UserId} after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<InteractionModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Extracts the current user ID from the claims principal
    /// </summary>
    private int? GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Gets recent updates from all entity types (Partners, Contacts, Interactions) 
    /// combined and sorted by last modified date, filtered by user's global org unit filter and RBAC
    /// </summary>
    public async Task<OrgUnitRecentUpdatesResponse> GetOrgUnitRecentUpdatesAsync(ClaimsPrincipal user, int pageSize = 10)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for org unit recent updates request");
            return new OrgUnitRecentUpdatesResponse();
        }

        _logger.LogInformation("Getting org unit recent updates for user {UserId} with RBAC filtering", userId.Value);

        try
        {
            // Get user's global filters to check for org unit filtering
            var globalFilters = await _userPreferenceService.GetGlobalFiltersAsync(userId.ToString());
            List<int>? orgUnitIds = null;
            string orgUnitName = "your organization unit";
            int? orgUnitId = null;
            
            if (globalFilters?.OrgUnitId.HasValue == true)
            {
                orgUnitId = globalFilters.OrgUnitId.Value;
                
                // Get descendant org unit IDs for hierarchical filtering
                orgUnitIds = await _hierarchyService.GetDescendantIdsAsync(globalFilters.OrgUnitId.Value);
                _logger.LogInformation("Applying org unit filter for {OrgUnitId}, including {Count} descendant units", 
                    globalFilters.OrgUnitId.Value, orgUnitIds.Count);

                // Logic to get the Org Unit Name directly from database
                var orgUnit = await _context.Set<OrganizationHierarchy>()
                    .Where(oh => oh.Id == orgUnitId.Value)
                    .Select(oh => new { oh.Name })
                    .FirstOrDefaultAsync();
                
                if (orgUnit != null && !string.IsNullOrEmpty(orgUnit.Name))
                {
                    orgUnitName = orgUnit.Name;
                }
                else
                {
                    orgUnitName = $"Org Unit {orgUnitId.Value}";
                }
            }

            var allUpdates = new List<RecentUpdateModel>();

            // Get recent partners with org unit filtering
            var partnerQuery = _context.Set<UNOPSPartner>()
                .Where(p => p.LastModifiedDate.HasValue);

            if (orgUnitIds != null && orgUnitIds.Any())
            {
                // Filter partners by org unit relationships
                var validPartnerIds = await _context.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();

                partnerQuery = partnerQuery.Where(p => validPartnerIds.Contains(p.Id));
            }

            // Apply RBAC filtering for partners
            var filteredPartners = await ApplyAccessControlFiltersWithEntityName(
                partnerQuery.OrderByDescending(p => p.LastModifiedDate).Take(20), 
                user, "read", "Partner");
            
            var recentPartners = filteredPartners.Select(p => new RecentUpdateModel
            {
                Id = p.Id,
                Name = p.Name,
                Type = "Partner",
                LastModifiedDate = p.LastModifiedDate,
                LastModifiedBy = p.LastModifiedBy,
                Status = p.Status.ToString(),
                EntityData = null
            }).ToList();

            allUpdates.AddRange(recentPartners);

            // Get recent contacts with org unit filtering (via partner relationships)
            var contactQuery = _context.Set<UNOPSContact>()
                .Where(c => c.LastModifiedDate.HasValue);

            if (orgUnitIds != null && orgUnitIds.Any())
            {
                // Filter contacts by their partner's org unit relationships
                var validPartnerIds = await _context.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Partner" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();

                contactQuery = contactQuery.Where(c => validPartnerIds.Contains(c.PartnerId));
            }

            // Apply RBAC filtering for contacts
            var filteredContacts = await ApplyAccessControlFiltersWithEntityName(
                contactQuery.OrderByDescending(c => c.LastModifiedDate).Take(20), 
                user, "read", "Contact");
            
            var recentContacts = filteredContacts.Select(c => new RecentUpdateModel
            {
                Id = c.Id,
                Name = c.FirstName + " " + c.LastName,
                Type = "Contact",
                LastModifiedDate = c.LastModifiedDate,
                LastModifiedBy = c.LastModifiedBy,
                Status = c.Status.ToString(),
                EntityData = null
            }).ToList();

            allUpdates.AddRange(recentContacts);

            // Get recent interactions with org unit filtering (directly by interaction org unit relationships)
            var interactionQuery = _context.Set<Interaction>()
                .Where(i => i.LastModifiedDate.HasValue);

            if (orgUnitIds != null && orgUnitIds.Any())
            {
                // Filter interactions directly by their org unit relationships
                var validInteractionIds = await _context.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Interaction" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();

                interactionQuery = interactionQuery.Where(i => validInteractionIds.Contains(i.Id));
            }

            // Apply RBAC filtering for interactions
            var filteredInteractions = await ApplyAccessControlFiltersWithEntityName(
                interactionQuery.OrderByDescending(i => i.LastModifiedDate).Take(20), 
                user, "read", "Interaction");
            
            var recentInteractions = filteredInteractions.Select(i => new RecentUpdateModel
            {
                Id = i.Id,
                Name = i.Subject ?? "Untitled Interaction",
                Type = "Interaction",
                LastModifiedDate = i.LastModifiedDate,
                LastModifiedBy = i.LastModifiedBy,
                Status = i.Status.ToString(),
                EntityData = null
            }).ToList();

            allUpdates.AddRange(recentInteractions);

            // Combine all updates, sort by most recent, and take the requested page size
            var sortedUpdates = allUpdates
                .Where(u => u.LastModifiedDate.HasValue)
                .OrderByDescending(u => u.LastModifiedDate)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Found {Count} org unit recent updates after RBAC filtering (org unit filtered: {Filtered})", 
                sortedUpdates.Count, orgUnitIds != null);
                
            return new OrgUnitRecentUpdatesResponse
            {
                Updates = sortedUpdates,
                OrgUnitName = orgUnitName,
                OrgUnitId = orgUnitId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving org unit recent updates for user {UserId}", userId.Value);
            return new OrgUnitRecentUpdatesResponse();
        }
    }

    /// <summary>
    /// Dashboard service handles multiple entity types, so this method is not applicable.
    /// Use the specific dashboard methods instead (GetMyPartnersAsync, GetMyContactsAsync, etc.)
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        // Dashboard service aggregates data from multiple entity types
        // Individual entity access should go through their respective managers
        throw new NotSupportedException(
            "Dashboard service handles multiple entity types. " +
            "Use GetMyPartnersAsync, GetMyContactsAsync, GetMyInteractionsAsync, or GetOrgUnitRecentUpdatesAsync instead.");
    }
}
