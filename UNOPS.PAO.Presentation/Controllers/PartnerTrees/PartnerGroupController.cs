using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Services;
using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.PartnerTrees;

[ApiController]
[Route("api/[controller]")]
public class PartnerGroupController : BaseController
{
    private readonly PartnerGroupService _partnerGroupService;
    private readonly IMapper _mapper;

    public PartnerGroupController(
        PartnerGroupService partnerGroupService, 
        IMapper mapper,
        ILogger logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService,
        IPermissionService? permissionService = null,
        UNOPSAppDbContext? context = null,
        AiContextualService? aiService = null) 
        : base(logger, authorizationService, userResolverService, permissionService, context, aiService)
    {
        _partnerGroupService = partnerGroupService;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all partner groups with optional filtering and pagination.
    /// </summary>
    /// <example_uses>
    /// Get all partner groups with pagination
    /// Filter groups by category or name
    /// Get groups with their partners
    /// Sort groups by partner count
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to list, view, or filter partner groups.</when_to_use>
    /// <returns>Paginated list of partner groups</returns>
    [HttpGet]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerGroupModel>>> GetPartnerGroups([FromQuery] PartnerGroupFilterRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _partnerGroupService.GetPartnerGroupsAsync(request);
            
            var models = result.Records.Select(group => _mapper.Map<PartnerGroupModel>(group)).ToList();
            
            return new PaginationResponse<PartnerGroupModel>
            {
                Records = models,
                TotalCount = result.TotalCount,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
        });
    }

    /// <summary>
    /// Searches partner groups based on search criteria.
    /// </summary>
    /// <example_uses>
    /// Search groups by name or description
    /// Find groups within a specific category
    /// Search for groups with specific partner count ranges
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search or find partner groups with specific criteria.</when_to_use>
    /// <returns>Paginated search results of partner groups</returns>
    [HttpPost("search")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerGroupModel>>> SearchPartnerGroups([FromBody] PartnerGroupSearchRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _partnerGroupService.SearchPartnerGroupsAsync(request);
            
            var models = result.Records.Select(group => _mapper.Map<PartnerGroupModel>(group)).ToList();
            
            return new PaginationResponse<PartnerGroupModel>
            {
                Records = models,
                TotalCount = result.TotalCount,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
        });
    }

    /// <summary>
    /// Gets a specific partner group by ID with full details.
    /// </summary>
    /// <param name="id">Partner group ID</param>
    /// <param name="request">Request object containing the data</param>
    /// <example_uses>
    /// Get detailed information about a specific partner group
    /// View group with all related partners and category information
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for details about a specific partner group.</when_to_use>
    /// <returns>Partner group details with related data</returns>
    [HttpGet("{id:int}")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult<PartnerGroupModel>> GetPartnerGroup(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var group = await _partnerGroupService.GetPartnerGroupByIdAsync(id);
            
            if (group == null)
            {
                throw new BusinessException($"Partner group with ID {id} not found.");
            }
            
            return _mapper.Map<PartnerGroupModel>(group);
        });
    }

    /// <summary>
    /// Gets a specific partner group by code with full details.
    /// </summary>
    /// <example_uses>
    /// Get group details by its unique code
    /// Look up group information using group code
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for a partner group by its code identifier.</when_to_use>
    /// <returns>Partner group details with related data</returns>
    [HttpGet("by-code/{code}")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult<PartnerGroupModel>> GetPartnerGroupByCode(string code)
    {
        return await HandleOperationAsync(async () =>
        {
            var group = await _partnerGroupService.GetPartnerGroupByCodeAsync(code);
            
            if (group == null)
            {
                throw new BusinessException($"Partner group with code '{code}' not found.");
            }
            
            return _mapper.Map<PartnerGroupModel>(group);
        });
    }

    /// <summary>
    /// Gets all partner groups within a specific category.
    /// </summary>
    /// <param name="categoryId">Partner category ID</param>
    /// <param name="request">Request object containing the data</param>
    /// <example_uses>
    /// Get all groups under a specific category
    /// View category hierarchy with its groups
    /// List groups for category management
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for partner groups within a specific category.</when_to_use>
    /// <returns>List of partner groups in the specified category</returns>
    [HttpGet("by-category/{categoryId:int}")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult<IEnumerable<PartnerGroupModel>>> GetPartnerGroupsByCategory(int categoryId)
    {
        return await HandleOperationAsync(async () =>
        {
            var groups = await _partnerGroupService.GetPartnerGroupsByCategoryIdAsync(categoryId);
            var models = groups.Select(group => _mapper.Map<PartnerGroupModel>(group));
            
            return Ok(models);
        });
    }

    /// <summary>
    /// Gets summary statistics for all partner groups.
    /// </summary>
    /// <example_uses>
    /// Get overview of all partner groups with counts
    /// View group distribution and statistics
    /// Dashboard summary of partner groups
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for an overview or summary of partner groups.</when_to_use>
    /// <returns>Summary statistics of partner groups</returns>
    [HttpGet("summary")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult> GetPartnerGroupSummary()
    {
        return await HandleOperationAsync(async () =>
        {
            var request = new PartnerGroupFilterRequest 
            { 
                PageSize = int.MaxValue, 
                IncludePartnerCounts = true,
                IncludePartnerCategory = true
            };
            
            var result = await _partnerGroupService.GetPartnerGroupsAsync(request);
            
            var summary = new
            {
                TotalGroups = result.TotalCount,
                TotalPartners = result.Records.Sum(g => g.TotalPartnerCount),
                Groups = result.Records.Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Code,
                    g.Type,
                    g.Status,
                    g.PartnerCategoryName,
                    PartnerCount = g.TotalPartnerCount
                }).OrderByDescending(g => g.PartnerCount)
            };
            
            return Ok(summary);
        });
    }

    /// <summary>
    /// Gets partner groups grouped by category with statistics.
    /// </summary>
    /// <example_uses>
    /// View groups organized by their parent category
    /// Analyze group distribution by category
    /// Get category-based group breakdown
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see partner groups organized by category.</when_to_use>
    /// <returns>Groups grouped by category with statistics</returns>
    [HttpGet("by-category-summary")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult> GetPartnerGroupsByCategory()
    {
        return await HandleOperationAsync(async () =>
        {
            var request = new PartnerGroupFilterRequest 
            { 
                PageSize = int.MaxValue, 
                IncludePartnerCounts = true,
                IncludePartnerCategory = true
            };
            
            var result = await _partnerGroupService.GetPartnerGroupsAsync(request);
            
            var groupedByCategory = result.Records
                .Where(g => g.PartnerCategoryId.HasValue)
                .GroupBy(g => new { g.PartnerCategoryId, g.PartnerCategoryName, g.PartnerCategoryCode })
                .Select(g => new
                {
                    CategoryId = g.Key.PartnerCategoryId,
                    CategoryName = g.Key.PartnerCategoryName,
                    CategoryCode = g.Key.PartnerCategoryCode,
                    GroupCount = g.Count(),
                    TotalPartners = g.Sum(group => group.TotalPartnerCount),
                    Groups = g.Select(group => new
                    {
                        group.Id,
                        group.Name,
                        group.Code,
                        group.Type,
                        group.Status,
                        PartnerCount = group.TotalPartnerCount
                    }).OrderBy(group => group.Name)
                })
                .OrderBy(g => g.CategoryName);
            
            return Ok(groupedByCategory);
        });
    }

    /// <summary>
    /// Gets partner groups grouped by type with statistics.
    /// </summary>
    /// <example_uses>
    /// View groups organized by their hierarchical level
    /// Analyze group distribution by type
    /// Get type-based group breakdown
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see partner groups organized by type or level.</when_to_use>
    /// <returns>Groups grouped by type with statistics</returns>
    [HttpGet("by-type")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult> GetPartnerGroupsByType()
    {
        return await HandleOperationAsync(async () =>
        {
            var request = new PartnerGroupFilterRequest 
            { 
                PageSize = int.MaxValue, 
                IncludePartnerCounts = true 
            };
            
            var result = await _partnerGroupService.GetPartnerGroupsAsync(request);
            
            var groupedByType = result.Records
                .GroupBy(g => g.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    GroupCount = g.Count(),
                    TotalPartners = g.Sum(group => group.TotalPartnerCount),
                    Groups = g.Select(group => new
                    {
                        group.Id,
                        group.Name,
                        group.Code,
                        group.Status,
                        group.PartnerCategoryName,
                        PartnerCount = group.TotalPartnerCount
                    }).OrderBy(group => group.Name)
                })
                .OrderBy(g => g.Type);
            
            return Ok(groupedByType);
        });
    }

    /// <summary>
    /// Refreshes the partner group cache.
    /// </summary>
    /// <example_uses>
    /// Force refresh of group data after external changes
    /// Clear cached group information
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to refresh or clear partner group cache.</when_to_use>
    /// <returns>Success message</returns>
    [HttpPost("refresh-cache")]
    [AccessControlled(EntityTypes.PartnerGroup, "read")]
    public async Task<ActionResult> RefreshCache()
    {
        return await HandleOperationAsync(() =>
        {
            _partnerGroupService.InvalidateCache();
            return Task.FromResult(Ok(new { message = "Partner group cache refreshed successfully." }));
        });
    }
}
