namespace UNOPS.PAO.UNOPSBusiness.Services;

using System.Security.Claims;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Specifications;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service to handle organizational unit filtering logic for UNOPS entities
/// </summary>
public interface IOrgUnitFilterService
{
    Task<ISpecification<UNOPSPartner>> CreatePartnerSpecificationAsync(IPartnerSearchFilter filter, ClaimsPrincipal user);
    Task<ISpecification<UNOPSContact>> CreateContactSpecificationAsync(IContactSearchFilter filter, ClaimsPrincipal user);
    Task<ISpecification<Interaction>> CreateInteractionSpecificationAsync(IInteractionSearchFilter filter, ClaimsPrincipal user);
    Task<int?> GetUserDefaultOrgUnitIdAsync(ClaimsPrincipal user);
}

public class OrgUnitFilterService : IOrgUnitFilterService
{
    private readonly IPermissionService _permissionService;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IOrgUnitHierarchyService _hierarchyService;
    private readonly UNOPSAppDbContext _context;
    private readonly ILogger<OrgUnitFilterService> _logger;

    public OrgUnitFilterService(
        IPermissionService permissionService,
        IUserPreferenceService userPreferenceService,
        IOrgUnitHierarchyService hierarchyService,
        UNOPSAppDbContext context,
        ILogger<OrgUnitFilterService> logger)
    {
        _permissionService = permissionService;
        _userPreferenceService = userPreferenceService;
        _hierarchyService = hierarchyService;
        _context = context;
        _logger = logger;
    }

    public async Task<ISpecification<UNOPSPartner>> CreatePartnerSpecificationAsync(IPartnerSearchFilter filter, ClaimsPrincipal user)
    {
        _logger.LogInformation("Creating UNOPS partner specification with OrgUnit support");
        
        // Only use OrgUnit filter if explicitly provided in the request
        int? targetOrgUnitId = filter.OrgUnitId;
        
        if (targetOrgUnitId.HasValue)
        {
            // Always include hierarchy
            var hierarchyIds = await _hierarchyService.GetDescendantIdsAsync(targetOrgUnitId.Value);
            _logger.LogInformation("OrgUnit filter applied for partner, hierarchy includes {Count} units", hierarchyIds.Count);
            
            // Get users from the org unit hierarchy
            var orgUnitUserIds = await GetOrgUnitUserIds(hierarchyIds);
            _logger.LogInformation("Found {Count} users in org unit hierarchy", orgUnitUserIds.Count);
            
            // Create composite specification with OrgUnit filter including relations
            return new UNOPSPartnerCompositeWithOrgUnitAndRelationsSpecification(filter, hierarchyIds, orgUnitUserIds);
        }
        
        // No OrgUnit filter - return standard composite specification
        return new UNOPSPartnerCompositeSpecification(filter);
    }

    public async Task<ISpecification<UNOPSContact>> CreateContactSpecificationAsync(IContactSearchFilter filter, ClaimsPrincipal user)
    {
        // Only use OrgUnit filter if explicitly provided in the request
        int? targetOrgUnitId = filter.OrgUnitId;
        
        if (targetOrgUnitId.HasValue)
        {
            // Always include hierarchy
            var hierarchyIds = await _hierarchyService.GetDescendantIdsAsync(targetOrgUnitId.Value);
            
            // Create composite specification with OrgUnit filter
            return new UNOPSContactCompositeWithOrgUnitSpecification(filter, hierarchyIds);
        }
        
        // No OrgUnit filter - return standard composite specification
        return new UNOPSContactCompositeSpecification(filter);
    }

    public async Task<ISpecification<Interaction>> CreateInteractionSpecificationAsync(IInteractionSearchFilter filter, ClaimsPrincipal user)
    {
        _logger.LogInformation("Creating interaction specification with OrgUnit support");
        
        // Only use OrgUnit filter if explicitly provided in the request
        int? targetOrgUnitId = filter.OrgUnitId;
        
        if (targetOrgUnitId.HasValue)
        {
            // Always include hierarchy
            var hierarchyIds = await _hierarchyService.GetDescendantIdsAsync(targetOrgUnitId.Value);
            _logger.LogInformation("OrgUnit filter applied for interaction, hierarchy includes {Count} units", hierarchyIds.Count);
            
            // Create composite specification with OrgUnit filter
            return new InteractionCompositeWithOrgUnitSpecification(filter, hierarchyIds);
        }
        
        // No OrgUnit filter - return standard specification
        return new InteractionCompositeSpecification(filter);
    }

    private int? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
    
    private async Task<List<string>> GetOrgUnitUserIds(List<int> orgUnitIds)
    {
        // Get organization codes for the given IDs
        var orgCodes = await _context.OrganizationHierarchies
            .Where(oh => orgUnitIds.Contains(oh.Id))
            .Select(oh => oh.Code)
            .ToListAsync();
        
        // Get users belonging to these org units
        return await _context.UserProfile
            .Where(u => u.OrgUnit != null && orgCodes.Contains(u.OrgUnit))
            .Select(u => u.UserId.ToString())
            .ToListAsync();
    }
    
    public async Task<int?> GetUserDefaultOrgUnitIdAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return null;
        }
        
        return await _userPreferenceService.GetDefaultOrgUnitIdAsync(userId.Value);
    }
}