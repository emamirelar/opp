using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Entities;
using System.Collections.Generic;
using AutoMapper;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Dedicated service for dashboard data retrieval with user-specific filtering
/// This service keeps dashboard logic separate from core entity APIs
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly UNOPSAppDbContext _context;
    private readonly ILogger<DashboardService> _logger;
    private readonly IMapper _mapper;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IOrgUnitHierarchyService _hierarchyService;

    public DashboardService(
        UNOPSAppDbContext context, 
        ILogger<DashboardService> logger, 
        IMapper mapper,
        IUserPreferenceService userPreferenceService,
        IOrgUnitHierarchyService hierarchyService)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _userPreferenceService = userPreferenceService;
        _hierarchyService = hierarchyService;
    }

    /// <summary>
    /// Gets partners that are related to the current user (created by or last modified by)
    /// and excludes Draft status partners
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetMyPartnersAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard partners request");
            return new PaginationResponse<PartnerModel> { Records = new List<PartnerModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting dashboard partners for user {UserId}", userId.Value);

        var query = _context.Set<UNOPSPartner>()
            .Include(p => p.PartnerGroup)
            .Include(p => p.Projects)
            .Where(p => (p.CreatedBy == userId.Value || p.LastModifiedBy == userId.Value) 
                       && p.Status != "Draft")
            .OrderByDescending(p => p.LastModifiedDate ?? p.CreatedDate);

        var totalCount = await query.CountAsync();
        var entities = await query.Take(pageSize).ToListAsync();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<PartnerModel>>(entities);

        _logger.LogInformation("Found {Count} dashboard partners for user {UserId}", records.Count, userId.Value);

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
    /// and excludes Draft status contacts
    /// </summary>
    public async Task<PaginationResponse<ContactModel>> GetMyContactsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard contacts request");
            return new PaginationResponse<ContactModel> { Records = new List<ContactModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting dashboard contacts for user {UserId}", userId.Value);

        var query = _context.Set<UNOPSContact>()
            .Include(c => c.Partner)
            .Where(c => (c.CreatedBy == userId.Value || c.LastModifiedBy == userId.Value) 
                       && c.Status != "Draft")
            .OrderByDescending(c => c.LastModifiedDate ?? c.CreatedDate);

        var totalCount = await query.CountAsync();
        var entities = await query.Take(pageSize).ToListAsync();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<ContactModel>>(entities);

        _logger.LogInformation("Found {Count} dashboard contacts for user {UserId}", records.Count, userId.Value);

        return new PaginationResponse<ContactModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets draft partners that are related to the current user (created by or last modified by)
    /// </summary>
    public async Task<PaginationResponse<PartnerModel>> GetMyDraftPartnersAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for draft partners request");
            return new PaginationResponse<PartnerModel> { Records = new List<PartnerModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting draft partners for user {UserId}", userId.Value);

        var query = _context.Set<UNOPSPartner>()
            .Include(p => p.PartnerGroup)
            .Where(p => (p.CreatedBy == userId.Value || p.LastModifiedBy == userId.Value) 
                       && p.Status == "Draft")
            .OrderByDescending(p => p.CreatedDate);

        var totalCount = await query.CountAsync();
        var entities = await query.Take(pageSize).ToListAsync();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<PartnerModel>>(entities);

        _logger.LogInformation("Found {Count} draft partners for user {UserId}", records.Count, userId.Value);

        return new PaginationResponse<PartnerModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets draft contacts that are related to the current user (created by or last modified by)
    /// </summary>
    public async Task<PaginationResponse<ContactModel>> GetMyDraftContactsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for draft contacts request");
            return new PaginationResponse<ContactModel> { Records = new List<ContactModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting draft contacts for user {UserId}", userId.Value);

        var query = _context.Set<UNOPSContact>()
            .Include(c => c.Partner)
            .Where(c => (c.CreatedBy == userId.Value || c.LastModifiedBy == userId.Value) 
                       && c.Status == "Draft")
            .OrderByDescending(c => c.CreatedDate);

        var totalCount = await query.CountAsync();
        var entities = await query.Take(pageSize).ToListAsync();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<ContactModel>>(entities);

        _logger.LogInformation("Found {Count} draft contacts for user {UserId}", records.Count, userId.Value);

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
    /// and excludes Draft status interactions
    /// </summary>
    public async Task<PaginationResponse<InteractionModel>> GetMyInteractionsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for dashboard interactions request");
            return new PaginationResponse<InteractionModel> { Records = new List<InteractionModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting dashboard interactions for user {UserId}", userId.Value);

        var query = _context.Set<Interaction>()
            .Include(i => i.InteractionContacts)
            .Include(i => i.InteractionPartners)
            .Where(i => (i.CreatedBy == userId.Value || i.LastModifiedBy == userId.Value) 
                       && i.Status != EntityStatus.Draft)
            .OrderByDescending(i => i.LastModifiedDate ?? i.CreatedDate);

        var totalCount = await query.CountAsync();
        var entities = await query.Take(pageSize).ToListAsync();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<InteractionModel>>(entities);

        _logger.LogInformation("Found {Count} dashboard interactions for user {UserId}", records.Count, userId.Value);

        return new PaginationResponse<InteractionModel>
        {
            Records = records,
            TotalCount = totalCount,
            PageIndex = 1,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets draft interactions that are related to the current user (created by or last modified by)
    /// </summary>
    public async Task<PaginationResponse<InteractionModel>> GetMyDraftInteractionsAsync(ClaimsPrincipal user, int pageSize = 1000)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for draft interactions request");
            return new PaginationResponse<InteractionModel> { Records = new List<InteractionModel>(), TotalCount = 0 };
        }

        _logger.LogInformation("Getting draft interactions for user {UserId}", userId.Value);

        var query = _context.Set<Interaction>()
            .Include(i => i.InteractionContacts)
            .Include(i => i.InteractionPartners)
            .Where(i => (i.CreatedBy == userId.Value || i.LastModifiedBy == userId.Value) 
                       && i.Status == EntityStatus.Draft)
            .OrderByDescending(i => i.CreatedDate);

        var totalCount = await query.CountAsync();
        var entities = await query.Take(pageSize).ToListAsync();
        
        // Map entities to models to avoid circular references
        var records = _mapper.Map<List<InteractionModel>>(entities);

        _logger.LogInformation("Found {Count} draft interactions for user {UserId}", records.Count, userId.Value);

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
    /// combined and sorted by last modified date, filtered by user's global org unit filter
    /// </summary>
    public async Task<List<RecentUpdateModel>> GetOrgUnitRecentUpdatesAsync(ClaimsPrincipal user, int pageSize = 10)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
        {
            _logger.LogWarning("No valid user ID found for org unit recent updates request");
            return new List<RecentUpdateModel>();
        }

        _logger.LogInformation("Getting org unit recent updates for user {UserId}", userId.Value);

        try
        {
            // Get user's global filters to check for org unit filtering
            var globalFilters = await _userPreferenceService.GetGlobalFiltersAsync(userId.ToString());
            List<int>? orgUnitIds = null;
            
            if (globalFilters?.OrgUnitId.HasValue == true)
            {
                // Get descendant org unit IDs for hierarchical filtering
                orgUnitIds = await _hierarchyService.GetDescendantIdsAsync(globalFilters.OrgUnitId.Value);
                _logger.LogInformation("Applying org unit filter for {OrgUnitId}, including {Count} descendant units", 
                    globalFilters.OrgUnitId.Value, orgUnitIds.Count);
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

            var recentPartners = await partnerQuery
                .OrderByDescending(p => p.LastModifiedDate)
                .Take(20)
                .Select(p => new RecentUpdateModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Type = "Partner",
                    LastModifiedDate = p.LastModifiedDate,
                    LastModifiedBy = p.LastModifiedBy.ToString(),
                    Status = p.Status.ToString(),
                    EntityData = null
                })
                .ToListAsync();

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

            var recentContacts = await contactQuery
                .OrderByDescending(c => c.LastModifiedDate)
                .Take(20)
                .Select(c => new RecentUpdateModel
                {
                    Id = c.Id,
                    Name = c.FirstName + " " + c.LastName,
                    Type = "Contact",
                    LastModifiedDate = c.LastModifiedDate,
                    LastModifiedBy = c.LastModifiedBy.ToString(),
                    Status = c.Status.ToString(),
                    EntityData = null
                })
                .ToListAsync();

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

            var recentInteractions = await interactionQuery
                .OrderByDescending(i => i.LastModifiedDate)
                .Take(20)
                .Select(i => new RecentUpdateModel
                {
                    Id = i.Id,
                    Name = i.Subject ?? "Untitled Interaction",
                    Type = "Interaction",
                    LastModifiedDate = i.LastModifiedDate,
                    LastModifiedBy = i.LastModifiedBy.ToString(),
                    Status = i.Status.ToString(),
                    EntityData = null
                })
                .ToListAsync();

            allUpdates.AddRange(recentInteractions);

            // Combine all updates, sort by most recent, and take the requested page size
            var sortedUpdates = allUpdates
                .Where(u => u.LastModifiedDate.HasValue)
                .OrderByDescending(u => u.LastModifiedDate)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Found {Count} org unit recent updates (filtered: {Filtered})", 
                sortedUpdates.Count, orgUnitIds != null);
            return sortedUpdates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving org unit recent updates for user {UserId}", userId.Value);
            return new List<RecentUpdateModel>();
        }
    }
}
