using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Dashboard;
using UNOPS.PAO.Models.Shared;

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
        IPermissionService? permissionService = null,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(mapper, context, configuration, null!, "Dashboard", permissionService, httpContextAccessor)
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
    /// including ALL statuses (Active, Draft, etc.) with RBAC filtering
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

        // Include ALL statuses (including Draft) in My Workspace
        var query = _context.Set<UNOPSPartner>()
            .Include(p => p.PartnerGroup)
            .Where(p => p.CreatedBy == userId.Value || p.LastModifiedBy == userId.Value)
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
    /// including ALL statuses (Active, Draft, etc.) with RBAC filtering
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

        // Include ALL statuses (including Draft) in My Workspace
        var query = _context.Set<UNOPSContact>()
            .Include(c => c.Partner)
            .Where(c => c.CreatedBy == userId.Value || c.LastModifiedBy == userId.Value)
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
                       && c.Status == EntityStatus.Draft)
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
    /// including ALL statuses (Active, Draft, etc.) with RBAC filtering
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

        // Include ALL statuses (including Draft) in My Workspace
        var query = _context.Set<Interaction>()
            .Include(i => i.InteractionContacts)
            .Include(i => i.InteractionPartners)
            .Where(i => i.CreatedBy == userId.Value || i.LastModifiedBy == userId.Value)
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
    /// Gets opportunities where the current user is a stakeholder, creator, or last modifier
    /// including ALL statuses (Active, Draft, etc.) with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<OpportunityModel>> GetMyOpportunitiesAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard opportunities request");
            return new PaginationResponse<OpportunityModel> { Records = new List<OpportunityModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting dashboard opportunities for user {UserId} (stakeholder/creator/modifier) with RBAC filtering", userId.Value);

        // Get opportunities where user is a stakeholder, including their role information
        var userStakeholderRoles = await _context.Set<OpportunityStakeholder>()
            .Include(os => os.EntityRole)
            .Where(os => os.UserId == userId.Value)
            .Select(os => new { os.OpportunityId, RoleName = os.EntityRole != null ? os.EntityRole.Name : null })
            .ToListAsync();

        var opportunityIdsFromStakeholders = userStakeholderRoles
            .Select(os => os.OpportunityId)
            .Distinct()
            .ToList();
        
        _logger.LogInformation("DEBUG: User {UserId} is stakeholder on {Count} opportunities: [{Ids}]", 
            userId.Value, 
            opportunityIdsFromStakeholders.Count,
            string.Join(", ", opportunityIdsFromStakeholders.Take(10)));

        // Create a lookup dictionary for roles by opportunity ID
        var rolesByOpportunityId = userStakeholderRoles
            .Where(os => !string.IsNullOrEmpty(os.RoleName))
            .GroupBy(os => os.OpportunityId)
            .ToDictionary(
                g => g.Key, 
                g => string.Join(", ", g.Select(x => x.RoleName).Distinct())
            );

        // Check how many opportunities user created or modified (including drafts)
        var createdByUser = await _context.Set<Opportunity>()
            .Where(o => o.CreatedBy == userId.Value)
            .CountAsync();
        var modifiedByUser = await _context.Set<Opportunity>()
            .Where(o => o.LastModifiedBy == userId.Value)
            .CountAsync();
        
        _logger.LogInformation("DEBUG: User {UserId} created {Created} opportunities, modified {Modified} opportunities (all statuses)", 
            userId.Value, createdByUser, modifiedByUser);

        // Query opportunities where user is stakeholder, creator, or last modifier
        // Include ALL statuses (including Draft) in My Workspace
        var query = _context.Set<Opportunity>()
            .Include(o => o.FundingPartners)
            .Include(o => o.ClientPartners)
            .Where(o => opportunityIdsFromStakeholders.Contains(o.Id) 
                        || o.CreatedBy == userId.Value 
                        || o.LastModifiedBy == userId.Value)
            .OrderByDescending(o => o.LastModifiedDate ?? o.CreatedDate);

        // Count before RBAC filtering
        var countBeforeRbac = await query.CountAsync();
        _logger.LogInformation("DEBUG: Found {Count} opportunities BEFORE RBAC filtering", countBeforeRbac);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Opportunity");
        
        _logger.LogInformation("DEBUG: Found {Count} opportunities AFTER RBAC filtering", filteredData.Count());
        
        var opportunityArray = filteredData.ToArray();
        var totalCount = opportunityArray.Length;
        var paginatedEntities = opportunityArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<OpportunityModel>>(paginatedEntities);

        // Add role information to each opportunity model
        foreach (var record in records)
        {
            if (rolesByOpportunityId.TryGetValue(record.Id, out var roleName))
            {
                // Store the role name in a property that can be used by the frontend
                // Note: OpportunityModel needs to have a UserRole or similar property
                record.UserRole = roleName;
            }
        }

        _logger.LogInformation("Found {Count} dashboard opportunities for user {UserId} (stakeholder/creator/modifier) after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<OpportunityModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets draft opportunities where the current user is a stakeholder, creator, or last modifier with RBAC filtering
    /// </summary>
    public async Task<PaginationResponse<OpportunityModel>> GetMyDraftOpportunitiesAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for draft opportunities request");
            return new PaginationResponse<OpportunityModel> { Records = new List<OpportunityModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting draft opportunities for user {UserId} (stakeholder/creator/modifier) with RBAC filtering", userId.Value);

        // Get opportunities where user is a stakeholder, including their role information
        var userStakeholderRoles = await _context.Set<OpportunityStakeholder>()
            .Include(os => os.EntityRole)
            .Where(os => os.UserId == userId.Value)
            .Select(os => new { os.OpportunityId, RoleName = os.EntityRole != null ? os.EntityRole.Name : null })
            .ToListAsync();

        var opportunityIdsFromStakeholders = userStakeholderRoles
            .Select(os => os.OpportunityId)
            .Distinct()
            .ToList();

        // Create a lookup dictionary for roles by opportunity ID
        var rolesByOpportunityId = userStakeholderRoles
            .Where(os => !string.IsNullOrEmpty(os.RoleName))
            .GroupBy(os => os.OpportunityId)
            .ToDictionary(
                g => g.Key, 
                g => string.Join(", ", g.Select(x => x.RoleName).Distinct())
            );

        // Query draft opportunities where user is stakeholder, creator, or last modifier
        var query = _context.Set<Opportunity>()
            .Where(o => (opportunityIdsFromStakeholders.Contains(o.Id) 
                        || o.CreatedBy == userId.Value 
                        || o.LastModifiedBy == userId.Value)
                       && o.Status == EntityStatus.Draft)
            .OrderByDescending(o => o.CreatedDate);

        // Apply RBAC access control filters before counting and pagination
        var filteredData = await ApplyAccessControlFiltersWithEntityName(query, user, "read", "Opportunity");
        
        var opportunityArray = filteredData.ToArray();
        var totalCount = opportunityArray.Length;
        var paginatedEntities = opportunityArray.Take(pageSize).ToList();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<OpportunityModel>>(paginatedEntities);

        // Add role information to each opportunity model
        foreach (var record in records)
        {
            if (rolesByOpportunityId.TryGetValue(record.Id, out var roleName))
            {
                record.UserRole = roleName;
            }
        }

        _logger.LogInformation("Found {Count} draft opportunities for user {UserId} (stakeholder/creator/modifier) after RBAC filtering", records.Count, userId.Value);

        return new PaginationResponse<OpportunityModel>
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

            // Build user name lookup dictionary from UserProfile
            var userProfiles = await _context.Set<UserProfile>()
                .Select(up => new { up.UserId, up.FirstName, up.LastName })
                .ToListAsync();

            var userNameLookup = userProfiles.ToDictionary(
                up => up.UserId,
                up => {
                    if (!string.IsNullOrEmpty(up.FirstName) && !string.IsNullOrEmpty(up.LastName))
                        return $"{up.FirstName} {up.LastName}".Trim();
                    else if (!string.IsNullOrEmpty(up.FirstName))
                        return up.FirstName;
                    else if (!string.IsNullOrEmpty(up.LastName))
                        return up.LastName;
                    else
                        return $"User {up.UserId}";
                }
            );

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

            var recentPartners = filteredPartners.Select(p =>
            {
                var userId = p.LastModifiedBy != 0 ? p.LastModifiedBy : p.CreatedBy;
                return new RecentUpdateModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Type = "Partner",
                    LastModifiedDate = p.LastModifiedDate,
                    LastModifiedBy = userId,
                    LastModifiedByName = userNameLookup.ContainsKey(userId)
                        ? userNameLookup[userId]
                        : $"User {userId}",
                    Status = p.Status.ToString(),
                    EntityData = null
                };
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

            var recentContacts = filteredContacts.Select(c =>
            {
                var userId = c.LastModifiedBy != 0 ? c.LastModifiedBy : c.CreatedBy;
                return new RecentUpdateModel
                {
                    Id = c.Id,
                    Name = c.FirstName + " " + c.LastName,
                    Type = "Contact",
                    LastModifiedDate = c.LastModifiedDate,
                    LastModifiedBy = userId,
                    LastModifiedByName = userNameLookup.ContainsKey(userId)
                        ? userNameLookup[userId]
                        : $"User {userId}",
                    Status = c.Status.ToString(),
                    EntityData = null
                };
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

            var recentInteractions = filteredInteractions.Select(i =>
            {
                var userId = i.LastModifiedBy != 0 ? i.LastModifiedBy : i.CreatedBy;
                return new RecentUpdateModel
                {
                    Id = i.Id,
                    Name = i.Subject ?? "Untitled Interaction",
                    Type = "Interaction",
                    LastModifiedDate = i.LastModifiedDate,
                    LastModifiedBy = userId,
                    LastModifiedByName = userNameLookup.ContainsKey(userId)
                        ? userNameLookup[userId]
                        : $"User {userId}",
                    Status = i.Status.ToString(),
                    EntityData = null
                };
            }).ToList();

            allUpdates.AddRange(recentInteractions);

            // Get recent opportunities with org unit filtering
            var opportunityQuery = _context.Set<Opportunity>()
                .Where(o => o.LastModifiedDate.HasValue);

            if (orgUnitIds != null && orgUnitIds.Any())
            {
                // Filter opportunities by org unit relationships
                var validOpportunityIds = await _context.Set<OrganizationUnitRelationship>()
                    .Where(orgRel => 
                        orgRel.EntityType == "Opportunity" && 
                        orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => orgRel.EntityId)
                    .ToListAsync();

                opportunityQuery = opportunityQuery.Where(o => validOpportunityIds.Contains(o.Id));
            }

            // Apply RBAC filtering for opportunities
            var filteredOpportunities = await ApplyAccessControlFiltersWithEntityName(
                opportunityQuery.OrderByDescending(o => o.LastModifiedDate).Take(20),
                user, "read", "Opportunity");

            var recentOpportunities = filteredOpportunities.Select(o =>
            {
                var userId = o.LastModifiedBy != 0 ? o.LastModifiedBy : o.CreatedBy;
                return new RecentUpdateModel
                {
                    Id = o.Id,
                    Name = o.Name ?? "Untitled Opportunity",
                    Type = "Opportunity",
                    LastModifiedDate = o.LastModifiedDate,
                    LastModifiedBy = userId,
                    LastModifiedByName = userNameLookup.ContainsKey(userId)
                        ? userNameLookup[userId]
                        : $"User {userId}",
                    Status = o.Status.ToString(),
                    EntityData = null
                };
            }).ToList();

            allUpdates.AddRange(recentOpportunities);

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
    /// Gets all dashboard data in a single optimized request.
    /// Uses lightweight projections and .AsNoTracking() for high performance.
    /// Executes queries sequentially (simpler and fast enough with projections).
    /// </summary>
    public async Task<DashboardCombinedResponse> GetAllDashboardDataAsync(ClaimsPrincipal user, int pageSize = 50, int recentUpdatesPageSize = 10)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard request");
            return new DashboardCombinedResponse();
        }

        _logger.LogInformation("Getting dashboard data for user {UserId}", userId.Value);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var response = new DashboardCombinedResponse();

        try
        {
            // ==========================================
            // BATCH 1: Get all user's opportunity stakeholder info in ONE query
            // ==========================================
            var userStakeholderInfo = await _context.Set<OpportunityStakeholder>()
                .AsNoTracking()
                .Where(os => os.UserId == userId.Value)
                .Select(os => new 
                { 
                    os.OpportunityId, 
                    RoleName = os.EntityRole != null ? os.EntityRole.Name : null 
                })
                .ToListAsync();

            var opportunityIdsFromStakeholders = userStakeholderInfo
                .Select(os => os.OpportunityId)
                .Distinct()
                .ToList();

            var rolesByOpportunityId = userStakeholderInfo
                .Where(os => !string.IsNullOrEmpty(os.RoleName))
                .GroupBy(os => os.OpportunityId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(x => x.RoleName).Distinct())
                );

            // ==========================================
            // BATCH 2: Execute all entity queries with projections sequentially
            // Using projections (~90% smaller than full entities)
            // Sequential execution is simpler and fast enough with projections
            // ==========================================

            // Partners (all statuses for My Workspace)
            response.MyPartners = await _context.Set<UNOPSPartner>()
                .AsNoTracking()
                .Where(p => p.CreatedBy == userId.Value || p.LastModifiedBy == userId.Value)
                .OrderByDescending(p => p.LastModifiedDate ?? p.CreatedDate)
                .Take(pageSize)
                .Select(p => new DashboardPartnerModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status.ToString(),
                    CreatedDate = p.CreatedDate,
                    LastModifiedDate = p.LastModifiedDate
                })
                .ToListAsync();

            // Draft Partners
            response.DraftPartners = await _context.Set<UNOPSPartner>()
                .AsNoTracking()
                .Where(p => (p.CreatedBy == userId.Value || p.LastModifiedBy == userId.Value) 
                           && p.Status == Domain.Entities.EntityStatus.Draft)
                .OrderByDescending(p => p.CreatedDate)
                .Take(pageSize)
                .Select(p => new DashboardPartnerModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status.ToString(),
                    CreatedDate = p.CreatedDate,
                    LastModifiedDate = p.LastModifiedDate
                })
                .ToListAsync();

            // Contacts (all statuses for My Workspace)
            response.MyContacts = await _context.Set<UNOPSContact>()
                .AsNoTracking()
                .Where(c => c.CreatedBy == userId.Value || c.LastModifiedBy == userId.Value)
                .OrderByDescending(c => c.LastModifiedDate ?? c.CreatedDate)
                .Take(pageSize)
                .Select(c => new DashboardContactModel
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    CreatedDate = c.CreatedDate,
                    LastModifiedDate = c.LastModifiedDate
                })
                .ToListAsync();

            // Draft Contacts
            response.DraftContacts = await _context.Set<UNOPSContact>()
                .AsNoTracking()
                .Where(c => (c.CreatedBy == userId.Value || c.LastModifiedBy == userId.Value) 
                           && c.Status == EntityStatus.Draft)
                .OrderByDescending(c => c.CreatedDate)
                .Take(pageSize)
                .Select(c => new DashboardContactModel
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    CreatedDate = c.CreatedDate,
                    LastModifiedDate = c.LastModifiedDate
                })
                .ToListAsync();

            // Interactions (all statuses for My Workspace)
            response.MyInteractions = await _context.Set<Interaction>()
                .AsNoTracking()
                .Where(i => i.CreatedBy == userId.Value || i.LastModifiedBy == userId.Value)
                .OrderByDescending(i => i.LastModifiedDate ?? i.CreatedDate)
                .Take(pageSize)
                .Select(i => new DashboardInteractionModel
                {
                    Id = i.Id,
                    Type = i.Type.ToString(),
                    Subject = i.Subject,
                    Description = i.Description,
                    Date = i.Date,
                    Status = i.Status.ToString(),
                    CreatedDate = i.CreatedDate,
                    LastModifiedDate = i.LastModifiedDate
                })
                .ToListAsync();

            // Draft Interactions
            response.DraftInteractions = await _context.Set<Interaction>()
                .AsNoTracking()
                .Where(i => (i.CreatedBy == userId.Value || i.LastModifiedBy == userId.Value) 
                           && i.Status == EntityStatus.Draft)
                .OrderByDescending(i => i.CreatedDate)
                .Take(pageSize)
                .Select(i => new DashboardInteractionModel
                {
                    Id = i.Id,
                    Type = i.Type.ToString(),
                    Subject = i.Subject,
                    Description = i.Description,
                    Date = i.Date,
                    Status = i.Status.ToString(),
                    CreatedDate = i.CreatedDate,
                    LastModifiedDate = i.LastModifiedDate
                })
                .ToListAsync();

            // Opportunities (all statuses for My Workspace)
            response.MyOpportunities = await _context.Set<Opportunity>()
                .AsNoTracking()
                .Where(o => opportunityIdsFromStakeholders.Contains(o.Id) 
                           || o.CreatedBy == userId.Value 
                           || o.LastModifiedBy == userId.Value)
                .OrderByDescending(o => o.LastModifiedDate ?? o.CreatedDate)
                .Take(pageSize)
                .Select(o => new DashboardOpportunityModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    Status = o.Status.ToString(),
                    Stage = o.Stage, // Use Stage property instead of WorkflowStage navigation
                    CreatedDate = o.CreatedDate,
                    LastModifiedDate = o.LastModifiedDate
                })
                .ToListAsync();

            // Draft Opportunities
            response.DraftOpportunities = await _context.Set<Opportunity>()
                .AsNoTracking()
                .Where(o => (opportunityIdsFromStakeholders.Contains(o.Id) 
                           || o.CreatedBy == userId.Value 
                           || o.LastModifiedBy == userId.Value)
                           && o.Status == EntityStatus.Draft)
                .OrderByDescending(o => o.CreatedDate)
                .Take(pageSize)
                .Select(o => new DashboardOpportunityModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    Status = o.Status.ToString(),
                    Stage = o.Stage, // Use Stage property instead of WorkflowStage navigation
                    CreatedDate = o.CreatedDate,
                    LastModifiedDate = o.LastModifiedDate
                })
                .ToListAsync();

            // Add user roles to opportunities
            foreach (var opp in response.MyOpportunities)
            {
                if (rolesByOpportunityId.TryGetValue(opp.Id, out var roleName))
                {
                    opp.UserRole = roleName;
                }
            }
            foreach (var opp in response.DraftOpportunities)
            {
                if (rolesByOpportunityId.TryGetValue(opp.Id, out var roleName))
                {
                    opp.UserRole = roleName;
                }
            }

            // ==========================================
            // BATCH 3: Get recent updates (optimized)
            // ==========================================
            var recentUpdatesResult = await GetOrgUnitRecentUpdatesOptimizedAsync(
                userId.Value.ToString(), 
                recentUpdatesPageSize);
            
            response.OrgUnitRecentUpdates = recentUpdatesResult.Updates;
            response.OrgUnitName = recentUpdatesResult.OrgUnitName;
            response.OrgUnitId = recentUpdatesResult.OrgUnitId;

            stopwatch.Stop();
            _logger.LogInformation(
                "Dashboard data loaded in {ElapsedMs}ms for user {UserId}: " +
                "{Partners}/{DraftPartners} partners, {Contacts}/{DraftContacts} contacts, " +
                "{Interactions}/{DraftInteractions} interactions, {Opportunities}/{DraftOpportunities} opportunities, " +
                "{RecentUpdates} recent updates",
                stopwatch.ElapsedMilliseconds,
                userId.Value,
                response.MyPartners.Count, response.DraftPartners.Count,
                response.MyContacts.Count, response.DraftContacts.Count,
                response.MyInteractions.Count, response.DraftInteractions.Count,
                response.MyOpportunities.Count, response.DraftOpportunities.Count,
                response.OrgUnitRecentUpdates.Count);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data for user {UserId}", userId.Value);
            throw;
        }
    }

    /// <summary>
    /// Optimized recent updates query that doesn't load all UserProfiles into memory.
    /// Uses a single query per entity type with efficient user name lookups.
    /// </summary>
    private async Task<DashboardOrgUnitRecentUpdatesResponse> GetOrgUnitRecentUpdatesOptimizedAsync(
        string userIdString, 
        int pageSize)
    {
        var response = new DashboardOrgUnitRecentUpdatesResponse();
        
        try
        {
            // Get user's global filters for org unit filtering
            var globalFilters = await _userPreferenceService.GetGlobalFiltersAsync(userIdString);
            List<int>? orgUnitIds = null;

            if (globalFilters?.OrgUnitId.HasValue == true)
            {
                response.OrgUnitId = globalFilters.OrgUnitId.Value;
                orgUnitIds = await _hierarchyService.GetDescendantIdsAsync(globalFilters.OrgUnitId.Value);

                // Get org unit name efficiently
                var orgUnitName = await _context.Set<OrganizationHierarchy>()
                    .AsNoTracking()
                    .Where(oh => oh.Id == globalFilters.OrgUnitId.Value)
                    .Select(oh => oh.Name)
                    .FirstOrDefaultAsync();

                response.OrgUnitName = !string.IsNullOrEmpty(orgUnitName) 
                    ? orgUnitName 
                    : $"Org Unit {globalFilters.OrgUnitId.Value}";
            }

            var allUpdates = new List<DashboardRecentUpdateModel>();

            // Get valid entity IDs if org unit filtering is enabled
            HashSet<int>? validPartnerIds = null;
            HashSet<int>? validContactPartnerIds = null;
            HashSet<int>? validInteractionIds = null;
            HashSet<int>? validOpportunityIds = null;

            if (orgUnitIds != null && orgUnitIds.Any())
            {
                // Batch query for all org unit relationships
                var orgUnitRelationships = await _context.Set<OrganizationUnitRelationship>()
                    .AsNoTracking()
                    .Where(orgRel => orgUnitIds.Contains(orgRel.OrganizationHierarchyId))
                    .Select(orgRel => new { orgRel.EntityType, orgRel.EntityId })
                    .ToListAsync();

                validPartnerIds = orgUnitRelationships
                    .Where(r => r.EntityType == "Partner")
                    .Select(r => r.EntityId)
                    .ToHashSet();

                validContactPartnerIds = validPartnerIds; // Contacts filtered by partner
                
                validInteractionIds = orgUnitRelationships
                    .Where(r => r.EntityType == "Interaction")
                    .Select(r => r.EntityId)
                    .ToHashSet();

                validOpportunityIds = orgUnitRelationships
                    .Where(r => r.EntityType == "Opportunity")
                    .Select(r => r.EntityId)
                    .ToHashSet();
            }

            // Collect all user IDs we need to look up (more efficient than loading all UserProfiles)
            var userIdsToLookup = new HashSet<int>();

            // Partners - with projection
            var partnersQuery = _context.Set<UNOPSPartner>()
                .AsNoTracking()
                .Where(p => p.LastModifiedDate.HasValue);

            if (validPartnerIds != null)
            {
                partnersQuery = partnersQuery.Where(p => validPartnerIds.Contains(p.Id));
            }

            var recentPartners = await partnersQuery
                .OrderByDescending(p => p.LastModifiedDate)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.LastModifiedDate,
                    ModifiedBy = p.LastModifiedBy != 0 ? p.LastModifiedBy : p.CreatedBy,
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            foreach (var p in recentPartners)
            {
                userIdsToLookup.Add(p.ModifiedBy);
            }

            // Contacts - with projection
            var contactsQuery = _context.Set<UNOPSContact>()
                .AsNoTracking()
                .Where(c => c.LastModifiedDate.HasValue);

            if (validContactPartnerIds != null)
            {
                contactsQuery = contactsQuery.Where(c => validContactPartnerIds.Contains(c.PartnerId));
            }

            var recentContacts = await contactsQuery
                .OrderByDescending(c => c.LastModifiedDate)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    Name = (!string.IsNullOrEmpty(c.FirstName) && !string.IsNullOrEmpty(c.LastName))
                        ? $"{c.FirstName} {c.LastName}".Trim()
                        : (!string.IsNullOrEmpty(c.FirstName) ? c.FirstName : c.LastName ?? "Unnamed Contact"),
                    c.LastModifiedDate,
                    ModifiedBy = c.LastModifiedBy != 0 ? c.LastModifiedBy : c.CreatedBy,
                    Status = c.Status.ToString()
                })
                .ToListAsync();

            foreach (var c in recentContacts)
            {
                userIdsToLookup.Add(c.ModifiedBy);
            }

            // Interactions - with projection
            var interactionsQuery = _context.Set<Interaction>()
                .AsNoTracking()
                .Where(i => i.LastModifiedDate.HasValue);

            if (validInteractionIds != null)
            {
                interactionsQuery = interactionsQuery.Where(i => validInteractionIds.Contains(i.Id));
            }

            var recentInteractions = await interactionsQuery
                .OrderByDescending(i => i.LastModifiedDate)
                .Take(pageSize)
                .Select(i => new
                {
                    i.Id,
                    Name = i.Subject ?? "Untitled Interaction",
                    i.LastModifiedDate,
                    ModifiedBy = i.LastModifiedBy != 0 ? i.LastModifiedBy : i.CreatedBy,
                    Status = i.Status.ToString()
                })
                .ToListAsync();

            foreach (var i in recentInteractions)
            {
                userIdsToLookup.Add(i.ModifiedBy);
            }

            // Opportunities - with projection
            var opportunitiesQuery = _context.Set<Opportunity>()
                .AsNoTracking()
                .Where(o => o.LastModifiedDate.HasValue);

            if (validOpportunityIds != null)
            {
                opportunitiesQuery = opportunitiesQuery.Where(o => validOpportunityIds.Contains(o.Id));
            }

            var recentOpportunities = await opportunitiesQuery
                .OrderByDescending(o => o.LastModifiedDate)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    Name = o.Name ?? "Untitled Opportunity",
                    o.LastModifiedDate,
                    ModifiedBy = o.LastModifiedBy != 0 ? o.LastModifiedBy : o.CreatedBy,
                    Status = o.Status.ToString()
                })
                .ToListAsync();

            foreach (var o in recentOpportunities)
            {
                userIdsToLookup.Add(o.ModifiedBy);
            }

            // SINGLE query for user names (instead of loading ALL UserProfiles)
            var userNameLookup = await _context.Set<UserProfile>()
                .AsNoTracking()
                .Where(up => userIdsToLookup.Contains(up.UserId))
                .Select(up => new { up.UserId, up.FirstName, up.LastName })
                .ToDictionaryAsync(
                    up => up.UserId,
                    up => BuildUserName(up.FirstName, up.LastName, up.UserId)
                );

            // Build final results
            foreach (var p in recentPartners)
            {
                allUpdates.Add(new DashboardRecentUpdateModel
                {
                    Id = p.Id,
                    Name = p.Name ?? "Unnamed Partner",
                    Type = "Partner",
                    LastModifiedDate = p.LastModifiedDate,
                    LastModifiedBy = p.ModifiedBy,
                    LastModifiedByName = userNameLookup.TryGetValue(p.ModifiedBy, out var name) ? name : $"User {p.ModifiedBy}",
                    Status = p.Status
                });
            }

            foreach (var c in recentContacts)
            {
                var contactName = !string.IsNullOrWhiteSpace(c.Name) ? c.Name.Trim() : "Unnamed Contact";
                allUpdates.Add(new DashboardRecentUpdateModel
                {
                    Id = c.Id,
                    Name = contactName,
                    Type = "Contact",
                    LastModifiedDate = c.LastModifiedDate,
                    LastModifiedBy = c.ModifiedBy,
                    LastModifiedByName = userNameLookup.TryGetValue(c.ModifiedBy, out var name) ? name : $"User {c.ModifiedBy}",
                    Status = c.Status
                });
            }

            foreach (var i in recentInteractions)
            {
                allUpdates.Add(new DashboardRecentUpdateModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Type = "Interaction",
                    LastModifiedDate = i.LastModifiedDate,
                    LastModifiedBy = i.ModifiedBy,
                    LastModifiedByName = userNameLookup.TryGetValue(i.ModifiedBy, out var name) ? name : $"User {i.ModifiedBy}",
                    Status = i.Status
                });
            }

            foreach (var o in recentOpportunities)
            {
                allUpdates.Add(new DashboardRecentUpdateModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    Type = "Opportunity",
                    LastModifiedDate = o.LastModifiedDate,
                    LastModifiedBy = o.ModifiedBy,
                    LastModifiedByName = userNameLookup.TryGetValue(o.ModifiedBy, out var name) ? name : $"User {o.ModifiedBy}",
                    Status = o.Status
                });
            }

            // Sort and limit
            response.Updates = allUpdates
                .Where(u => u.LastModifiedDate.HasValue)
                .OrderByDescending(u => u.LastModifiedDate)
                .Take(pageSize)
                .ToList();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimized org unit recent updates");
            return response;
        }
    }

    private static string BuildUserName(string? firstName, string? lastName, int userId)
    {
        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            return $"{firstName} {lastName}".Trim();
        if (!string.IsNullOrEmpty(firstName))
            return firstName;
        if (!string.IsNullOrEmpty(lastName))
            return lastName;
        return $"User {userId}";
    }

    /// <summary>
    /// Dashboard service handles multiple entity types, so this method is not applicable.
    /// Use the specific dashboard methods instead (GetMyPartnersAsync, GetMyContactsAsync, etc.)
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal? user = null)
    {
        // Dashboard service aggregates data from multiple entity types
        // Individual entity access should go through their respective managers
        throw new NotSupportedException(
            "Dashboard service handles multiple entity types. " +
            "Use GetMyPartnersAsync, GetMyContactsAsync, GetMyInteractionsAsync, or GetOrgUnitRecentUpdatesAsync instead.");
    }
}
