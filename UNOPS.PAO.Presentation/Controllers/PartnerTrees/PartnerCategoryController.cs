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
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.PartnerTrees;

[ApiController]
[Route("api/[controller]")]
public class PartnerCategoryController : BaseController
{
    private readonly PartnerCategoryService _partnerCategoryService;
    private readonly IMapper _mapper;

    public PartnerCategoryController(
        PartnerCategoryService partnerCategoryService, 
        IMapper mapper,
        ILogger logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService,
        IPermissionService? permissionService = null,
        UNOPSAppDbContext? context = null,
        AiContextualService? aiService = null) 
        : base(logger, authorizationService, userResolverService, permissionService, context, aiService)
    {
        _partnerCategoryService = partnerCategoryService;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all partner categories with optional filtering and pagination.
    /// </summary>
    /// <example_uses>
    /// Get all partner categories with pagination
    /// Filter categories by name or type
    /// Get categories with their partner groups
    /// Sort categories by partner count
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to list, view, or filter partner categories.</when_to_use>
    /// <returns>Paginated list of partner categories</returns>
    [HttpGet]
    [AccessControlled(EntityTypes.PartnerCategory, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerCategoryModel>>> GetPartnerCategories([FromQuery] PartnerCategoryFilterRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _partnerCategoryService.GetPartnerCategoriesAsync(request);
            
            var models = result.Records.Select(category => _mapper.Map<PartnerCategoryModel>(category)).ToList();
            
            return new PaginationResponse<PartnerCategoryModel>
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
    /// Searches partner categories based on search criteria.
    /// </summary>
    /// <example_uses>
    /// Search categories by name or description
    /// Find categories with specific partner count ranges
    /// Search for categories containing specific terms
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search or find partner categories with specific criteria.</when_to_use>
    /// <returns>Paginated search results of partner categories</returns>
    [HttpPost("search")]
    [AccessControlled(EntityTypes.PartnerCategory, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerCategoryModel>>> SearchPartnerCategories([FromBody] PartnerCategorySearchRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _partnerCategoryService.SearchPartnerCategoriesAsync(request);
            
            var models = result.Records.Select(category => _mapper.Map<PartnerCategoryModel>(category)).ToList();
            
            return new PaginationResponse<PartnerCategoryModel>
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
    /// Gets a specific partner category by ID with full details.
    /// </summary>
    /// <param name="id">Partner category ID</param>
    /// <param name="request">Request object containing the data</param>
    /// <example_uses>
    /// Get detailed information about a specific partner category
    /// View category with all related partner groups and counts
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for details about a specific partner category.</when_to_use>
    /// <returns>Partner category details with related data</returns>
    [HttpGet("{id:int}")]
    [AccessControlled(EntityTypes.PartnerCategory, "read")]
    public async Task<ActionResult<PartnerCategoryModel>> GetPartnerCategory(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var category = await _partnerCategoryService.GetPartnerCategoryByIdAsync(id);
            
            if (category == null)
            {
                throw new BusinessException($"Partner category with ID {id} not found.");
            }
            
            return _mapper.Map<PartnerCategoryModel>(category);
        });
    }

    /// <summary>
    /// Gets a specific partner category by code with full details.
    /// </summary>
    /// <example_uses>
    /// Get category details by its unique code
    /// Look up category information using category code
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for a partner category by its code identifier.</when_to_use>
    /// <returns>Partner category details with related data</returns>
    [HttpGet("by-code/{code}")]
    [AccessControlled(EntityTypes.PartnerCategory, "read")]
    public async Task<ActionResult<PartnerCategoryModel>> GetPartnerCategoryByCode(string code)
    {
        return await HandleOperationAsync(async () =>
        {
            var category = await _partnerCategoryService.GetPartnerCategoryByCodeAsync(code);
            
            if (category == null)
            {
                throw new BusinessException($"Partner category with code '{code}' not found.");
            }
            
            return _mapper.Map<PartnerCategoryModel>(category);
        });
    }

    /// <summary>
    /// Gets summary statistics for all partner categories.
    /// </summary>
    /// <example_uses>
    /// Get overview of all partner categories with counts
    /// View category distribution and statistics
    /// Dashboard summary of partner categorization
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for an overview or summary of partner categories.</when_to_use>
    /// <returns>Summary statistics of partner categories</returns>
    [HttpGet("summary")]
    [AccessControlled(EntityTypes.PartnerCategory, "read")]
    public async Task<ActionResult> GetPartnerCategorySummary()
    {
        return await HandleOperationAsync(async () =>
        {
            var request = new PartnerCategoryFilterRequest 
            { 
                PageSize = int.MaxValue, 
                IncludePartnerCounts = true 
            };
            
            var result = await _partnerCategoryService.GetPartnerCategoriesAsync(request);
            
            var summary = new
            {
                TotalCategories = result.TotalCount,
                TotalPartners = result.Records.Sum(c => c.TotalPartnerCount),
                Categories = result.Records.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Code,
                    c.Type,
                    c.Status,
                    PartnerCount = c.TotalPartnerCount,
                    GroupCount = c.PartnerGroupCount
                }).OrderByDescending(c => c.PartnerCount)
            };
            
            return Ok(summary);
        });
    }

    /// <summary>
    /// Gets partner categories grouped by type with statistics.
    /// </summary>
    /// <example_uses>
    /// View categories organized by their hierarchical level
    /// Analyze category distribution by type
    /// Get type-based category breakdown
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see partner categories organized by type or level.</when_to_use>
    /// <returns>Categories grouped by type with statistics</returns>
    [HttpGet("by-type")]
    [AccessControlled(EntityTypes.PartnerCategory, "read")]
    public async Task<ActionResult> GetPartnerCategoriesByType()
    {
        return await HandleOperationAsync(async () =>
        {
            var request = new PartnerCategoryFilterRequest 
            { 
                PageSize = int.MaxValue, 
                IncludePartnerCounts = true 
            };
            
            var result = await _partnerCategoryService.GetPartnerCategoriesAsync(request);
            
            var groupedByType = result.Records
                .GroupBy(c => c.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    CategoryCount = g.Count(),
                    TotalPartners = g.Sum(c => c.TotalPartnerCount),
                    Categories = g.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Code,
                        c.Status,
                        PartnerCount = c.TotalPartnerCount
                    }).OrderBy(c => c.Name)
                })
                .OrderBy(g => g.Type);
            
            return Ok(groupedByType);
        });
    }

    /// <summary>
    /// Refreshes the partner category cache.
    /// </summary>
    /// <example_uses>
    /// Force refresh of category data after external changes
    /// Clear cached category information
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to refresh or clear partner category cache.</when_to_use>
    /// <returns>Success message</returns>
    [HttpPost("refresh-cache")]
    [AccessControlled(EntityTypes.PartnerCategory, "read")]
    public async Task<ActionResult> RefreshCache()
    {
        return await HandleOperationAsync(() =>
        {
            _partnerCategoryService.InvalidateCache();
            return Task.FromResult(Ok(new { message = "Partner category cache refreshed successfully." }));
        });
    }
}
