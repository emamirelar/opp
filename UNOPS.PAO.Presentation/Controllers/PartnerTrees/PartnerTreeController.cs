namespace UNOPS.PAO.Presentation.Controllers.PartnerTrees;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using System;
using System.Linq;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Shared;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerTreeController : BaseController
{
    private readonly IPartnerTreeManager _manager;
    private readonly IManagerWrapper _managerWrapper;
    private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;

    public PartnerTreeController(
        IManagerWrapper managerWrapper, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerTreeController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _managerWrapper = managerWrapper;
        _manager = managerWrapper.PartnerTreeManager;
        _entityConfigurationManager = ((UNOPSManagerWrapper)managerWrapper).EntityConfigurationManager;
    }

    /// <summary>
    /// Creates a new partner tree node for organizing partner hierarchies and classifications.
    /// </summary>
    /// <param name="req">Partner tree data model with hierarchy information</param>
    /// <param name="req.name">Partner tree node name (required)</param>
    /// <param name="req.code">Unique code for the tree node (required)</param>
    /// <param name="req.description">Description of the partner category/group</param>
    /// <param name="req.parentCode">Parent node code for hierarchy</param>
    /// <param name="req.level">Tree level/depth</param>
    /// <param name="req.isCategory">Whether this is a category (true) or group (false)</param>
    /// <example_uses>
    /// Create a new partner category for Government
    /// Add a partner group under UN Agencies
    /// Create NGO subcategory classification
    /// Add new partner hierarchy node
    /// Set up partner organization structure
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to create, add, or set up new partner categories, groups, or hierarchy structures.</when_to_use>
    /// <returns>Created partner tree node with ID and metadata</returns>
    [HttpPost(APIDictionary.PartnerTree)]
    [AccessControlled(EntityTypes.PartnerTree, "create")]
    // Internal call: Create a Partner Tree
    public async Task<ActionResult> Create([FromBody] PartnerTreeDataModel req)
    { 
        return await HandleOperationAsync(async () =>
        {
            var result = await _manager.CreatePartnerTreeAsync(User, req);
            if (result == null)
            {
                throw new BusinessException("Failed to create partner tree");
            }
            return result;
        }, 201);
    }

    /// <summary>
    /// Retrieves all partner tree nodes (categories and groups) with sorting and access control.
    /// </summary>
    /// <param name="sortBy">Field to sort by (default: "Name")</param>
    /// <param name="ascending">Sort direction (default: true for ascending)</param>
    /// <example_uses>
    /// Show all partner categories and groups
    /// List partner hierarchy structure
    /// Get partner classification tree
    /// Show partner organization taxonomy
    /// List all partner categories sorted by name
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see partner categories, groups, hierarchy, or classification structure.</when_to_use>
    /// <returns>List of partner tree nodes with hierarchy information</returns>
    [HttpGet(APIDictionary.PartnerTree)]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    // Internal call: get partner tree created by logged-in user
    public async Task<ActionResult> GetAll([FromQuery] string sortBy = "Name", [FromQuery] bool ascending = true)
    {
        try
        {
            // Use the new secure method that includes row filtering and permissions
            var result = await _manager.GetPartnerTreesAsync(User, sortBy, ascending);
            return Ok(result);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business exception occurred: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Retrieves a specific partner tree node by ID with complete hierarchy details and permissions.
    /// </summary>
    /// <param name="id">Partner tree node ID</param>
    /// <example_uses>
    /// Show me details for partner category ID 123
    /// Get information about partner group 456
    /// Display hierarchy node 789
    /// Show complete partner tree node details
    /// Get partner classification details
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for specific partner tree node details by ID or when you need complete hierarchy information.</when_to_use>
    /// <returns>Complete partner tree node details with hierarchy information</returns>
    [HttpGet(APIDictionary.PartnerTree + "/{id}")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    // Internal call: Partner Tree details
    public async Task<ActionResult> Get(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            // Use the new secure method that checks entity-level access
            var partnerTree = await _manager.GetPartnerTreeAsync(User, id);
            if (partnerTree == null)
            {
                throw new BusinessException($"Partner Tree with ID {id} not found");
            }
            return partnerTree;
        });
    }

    /// <summary>
    /// Updates multiple partner tree nodes (categories and groups) with new hierarchy information and properties.
    /// </summary>
    /// <param name="req">Array of partner tree data models to update</param>
    /// <param name="req[].id">Partner tree node ID to update (required)</param>
    /// <param name="req[].name">Updated node name</param>
    /// <param name="req[].code">Updated code</param>
    /// <param name="req[].description">Updated description</param>
    /// <param name="req[].parentCode">Updated parent node code</param>
    /// <param name="req[].level">Updated tree level</param>
    /// <example_uses>
    /// Update partner category names and descriptions
    /// Reorganize partner tree hierarchy
    /// Modify partner group classifications
    /// Update multiple tree nodes at once
    /// Restructure partner organization taxonomy
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to update, modify, edit, or reorganize partner tree structure or classifications.</when_to_use>
    /// <returns>List of updated partner tree nodes</returns>
    [HttpPut(APIDictionary.PartnerTree)]
    [AccessControlled(EntityTypes.PartnerTree, "update")]
    // Internal call: update Partner Tree
    public async Task<ActionResult> Update([FromBody] PartnerTreeDataModel[] req)
    {
        return await HandleOperationAsync(async () =>
        {
            List<PartnerTreeModel> updatedTrees = new List<PartnerTreeModel>();
            
            foreach (var item in req)
            {
                // Use the new secure method that checks entity-level permissions
                var updatedTree = await _manager.UpdatePartnerTreeAsync(User, item);
                if (updatedTree != null)
                {
                    updatedTrees.Add(updatedTree);
                }
            }

            return updatedTrees;
        });
    }

    /// <summary>
    /// Soft deletes a partner tree node from the hierarchy (marks as deleted rather than permanent removal).
    /// </summary>
    /// <param name="id">Partner tree node ID to delete</param>
    /// <example_uses>
    /// Delete partner category ID 123
    /// Remove partner group 456 from hierarchy
    /// Delete obsolete partner classification
    /// Remove unused tree node
    /// Clean up partner taxonomy structure
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to delete, remove, or eliminate a partner category, group, or tree node.</when_to_use>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete(APIDictionary.PartnerTree + "/{id}")]
    [AccessControlled(EntityTypes.PartnerTree, "delete")]
    // Internal call: delete partner tree
    public async Task<ActionResult> Delete(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            // Use the new secure method that checks entity-level permissions
            await _manager.DeletePartnerTreeAsync(User, id);
        });
    }

    /// <summary>
    /// Retrieves the current user's permissions for a specific partner tree node (read, update, delete).
    /// </summary>
    /// <param name="id">Partner tree node ID to check permissions for</param>
    /// <example_uses>
    /// Check my permissions for partner category 123
    /// What can I do with partner group 456?
    /// Get access rights for this tree node
    /// Verify tree permissions before editing
    /// Can I modify this partner classification?
    /// </example_uses>
    /// <when_to_use>Use this when you need to check user permissions before performing operations or showing UI elements for partner tree management.</when_to_use>
    /// <returns>Permission object with CanRead, CanUpdate, CanDelete flags</returns>
    [HttpGet(APIDictionary.PartnerTree + "/{id}/permissions")]
    public async Task<ActionResult> PermissionsGet(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var partnerTree = await _manager.GetPartnerTreeAsync(User, id);
            if (partnerTree == null)
            {
                throw new BusinessException($"Partner Tree with ID {id} not found");
            }
            
            // Return permissions for this partner tree
            var permissions = await GetEntityPermissionsAsync("PartnerTree", partnerTree);
            
            return permissions;
        });
    }

    /// <summary>
    /// Retrieves the complete partner category and group structure as a hierarchical tree for organizational navigation.
    /// </summary>
    /// <example_uses>
    /// Show partner hierarchy structure
    /// Get complete partner taxonomy tree
    /// Display partner organization chart
    /// Show category and group relationships
    /// Get partner classification structure
    /// Load partner tree for navigation
    /// </example_uses>
    /// <when_to_use>Use this when the user needs to see the complete partner organizational structure, hierarchy, or when building navigation trees.</when_to_use>
    /// <returns>Hierarchical partner tree structure with categories and groups</returns>
    [HttpGet(APIDictionary.PartnerTree + "-structure")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    public async Task<ActionResult> GetCategoryAndGroupStructure()
    {
        var result = await _manager.GetCategoryAndGroupStructureAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of partners filtered by specific partner group code with access control and sorting.
    /// </summary>
    /// <param name="code">Partner group code to filter by (e.g., 'GOV', 'NGO', 'UNAGENCY')</param>
    /// <param name="request">Pagination request containing page size and index</param>
    /// <param name="request.pageIndex">Page number (1-based)</param>
    /// <param name="request.pageSize">Number of items per page</param>
    /// <param name="request.orderBy">Field to order results by</param>
    /// <param name="request.ascending">Sort direction (true for ascending)</param>
    /// <example_uses>
    /// Show all government partners (GOV group)
    /// List all NGO partners with pagination
    /// Get UN agency partners sorted by name
    /// Find partners in a specific partner group classification
    /// Show commercial partners (COMM group) with 20 per page
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to filter or search partners by partner group, organization type, or institutional classification.</when_to_use>
    /// <returns>Paginated list of partners belonging to the specified partner group</returns>
    [HttpGet(APIDictionary.PartnerTree + "/by-partner-group-id/{id}")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerGroup(int id, [FromQuery] PaginationRequest request)
    {
        try
        {
            // Use the partner manager through the wrapper since PartnerTreeManager focuses on tree structure
            var result = await _managerWrapper.PartnerManager.GetPartnersByPartnerGroupAsync(User, id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {       
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a paginated list of partners filtered by specific partner category code with access control and sorting.
    /// </summary>
    /// <param name="code">Partner category code to filter by (e.g., 'NATIONAL', 'INTERNATIONAL', 'BILATERAL')</param>
    /// <param name="request">Pagination request containing page size and index</param>
    /// <param name="request.pageIndex">Page number (1-based)</param>
    /// <param name="request.pageSize">Number of items per page</param>
    /// <param name="request.orderBy">Field to order results by</param>
    /// <param name="request.ascending">Sort direction (true for ascending)</param>
    /// <example_uses>
    /// Show all national partners in this category
    /// List international partners with pagination
    /// Get bilateral partners sorted by name
    /// Find partners in a specific operational category
    /// Show multilateral partners with 15 per page
    /// Filter partners by geographic or operational scope
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to filter or search partners by partner category, operational scope, or geographic classification.</when_to_use>
    /// <returns>Paginated list of partners belonging to the specified partner category</returns>
    [HttpGet(APIDictionary.PartnerTree + "/by-partner-category-code/{code}")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerCategory(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            // Use the partner manager through the wrapper since PartnerTreeManager focuses on tree structure
            var result = await _managerWrapper.PartnerManager.GetPartnersByCategoryAsync(User, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all partner categories with their partner counts for statistical analysis and diagram generation.
    /// </summary>
    /// <example_uses>
    /// Get all partner categories with counts
    /// Show partner distribution by category
    /// Generate partner category statistics
    /// Create partner category breakdown chart
    /// Display partner classification overview
    /// Draw diagram of partner categories with counts
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see partner distribution across categories, generate statistics, or create visual diagrams of partner categorization.</when_to_use>
    /// <returns>List of partner categories with their respective partner counts</returns>
    [HttpGet(APIDictionary.PartnerTree + "/categories-summary")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    public async Task<ActionResult> GetAllPartnerCategories()
    {
        try
        {
            // Use the partner manager through the wrapper since PartnerTreeManager focuses on tree structure
            // Get all unique partner category codes from partner trees
            var partnerTrees = await _managerWrapper.PartnerManager.GetPartnersAsync(User, new PaginationRequest { PageSize = int.MaxValue });
            
            // Group by partner category and count
            var categoryStats = partnerTrees.Records
                .Where(p => !string.IsNullOrEmpty(p.PartnerCategoryCode))
                .GroupBy(p => new { p.PartnerCategoryCode, p.PartnerCategoryName })
                .Select(g => new
                {
                    code = g.Key.PartnerCategoryCode,
                    name = g.Key.PartnerCategoryName ?? g.Key.PartnerCategoryCode,
                    partnerCount = g.Count(),
                    description = $"{g.Key.PartnerCategoryName ?? g.Key.PartnerCategoryCode} partners"
                })
                .OrderBy(x => x.name)
                .ToList();

            return Ok(new
            {
                totalCategories = categoryStats.Count,
                totalPartners = categoryStats.Sum(x => x.partnerCount),
                categories = categoryStats
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all partner groups with their partner counts for statistical analysis and diagram generation.
    /// </summary>
    /// <example_uses>
    /// Get all partner groups with counts
    /// Show partner distribution by group
    /// Generate partner group statistics
    /// Create partner group breakdown chart
    /// Display partner group overview
    /// Draw diagram of partner groups with counts
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see partner distribution across groups, generate statistics, or create visual diagrams of partner grouping.</when_to_use>
    /// <returns>List of partner groups with their respective partner counts</returns>
    [HttpGet(APIDictionary.PartnerTree + "/groups-summary")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    public async Task<ActionResult> GetAllPartnerGroups()
    {
        try
        {
            // Use the partner manager through the wrapper since PartnerTreeManager focuses on tree structure
            // Get all partners to analyze groups
            var partnerTrees = await _managerWrapper.PartnerManager.GetPartnersAsync(User, new PaginationRequest { PageSize = int.MaxValue });
            
            // Group by partner group and count
            var groupStats = partnerTrees.Records
                .Where(p => p.PartnerGroupId.HasValue)
                .GroupBy(p => new { p.PartnerGroupId, p.PartnerGroupName })
                .Select(g => new
                {
                    id = g.Key.PartnerGroupId,
                    name = g.Key.PartnerGroupName ?? $"Group {g.Key.PartnerGroupId}",
                    partnerCount = g.Count(),
                    description = $"{g.Key.PartnerGroupName ?? $"Group {g.Key.PartnerGroupId}"} partners"
                })
                .OrderBy(x => x.name)
                .ToList();

            return Ok(new
            {
                totalGroups = groupStats.Count,
                totalPartners = groupStats.Sum(x => x.partnerCount),
                groups = groupStats
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves complete partner categorization overview with both categories and groups including partner counts for comprehensive analysis and diagram generation.
    /// </summary>
    /// <example_uses>
    /// Get complete partner categorization overview
    /// Show partner distribution across categories and groups
    /// Generate comprehensive partner statistics
    /// Create partner organization chart
    /// Display complete partner taxonomy with counts
    /// Draw diagram showing partner categories and groups with distribution
    /// </example_uses>
    /// <when_to_use>Use this when the user wants a complete overview of partner organization, comprehensive statistics, or to create detailed diagrams showing both categories and groups.</when_to_use>
    /// <returns>Complete partner categorization data with categories, groups, and their respective partner counts</returns>
    [HttpGet(APIDictionary.PartnerTree + "/categorization-overview")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    public async Task<ActionResult> GetPartnerCategorizationOverview()
    {
        try
        {
            // Use the partner manager through the wrapper since PartnerTreeManager focuses on tree structure
            // Get all partners for analysis
            var partnerTrees = await _managerWrapper.PartnerManager.GetPartnersAsync(User, new PaginationRequest { PageSize = int.MaxValue });
            
            // Group by categories
            var categoryStats = partnerTrees.Records
                .Where(p => !string.IsNullOrEmpty(p.PartnerCategoryCode))
                .GroupBy(p => new { p.PartnerCategoryCode, p.PartnerCategoryName })
                .Select(g => new
                {
                    code = g.Key.PartnerCategoryCode,
                    name = g.Key.PartnerCategoryName ?? g.Key.PartnerCategoryCode,
                    partnerCount = g.Count(),
                    partners = g.Select(p => new { p.Id, p.Name }).ToList()
                })
                .OrderBy(x => x.name)
                .ToList();

            // Group by groups
            var groupStats = partnerTrees.Records
                .Where(p => p.PartnerGroupId.HasValue)
                .GroupBy(p => new { p.PartnerGroupId, p.PartnerGroupName })
                .Select(g => new
                {
                    id = g.Key.PartnerGroupId,
                    name = g.Key.PartnerGroupName ?? $"Group {g.Key.PartnerGroupId}",
                    partnerCount = g.Count(),
                    partners = g.Select(p => new { p.Id, p.Name }).ToList()
                })
                .OrderBy(x => x.name)
                .ToList();

            return Ok(new
            {
                summary = new
                {
                    totalPartners = partnerTrees.TotalCount,
                    totalCategories = categoryStats.Count,
                    totalGroups = groupStats.Count
                },
                categories = categoryStats,
                groups = groupStats,
                metadata = new
                {
                    generatedAt = DateTime.UtcNow,
                    description = "Complete partner categorization overview with categories, groups, and partner counts"
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Describes the PartnerTree entity structure including all field configurations
    /// </summary>
    /// <returns>Entity and field metadata for PartnerTree</returns>
    [HttpGet(APIDictionary.PartnerTree + "/describe")]
    [AccessControlled(EntityTypes.PartnerTree, "read")]
    public async Task<ActionResult> Describe()
    {
        try
        {
            var entityDetails = await _entityConfigurationManager.GetEntityConfigurationDetailsAsync(User, "PartnerTree");
            return Ok(entityDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving PartnerTree entity description");
            return StatusCode(500, new { error = "Failed to retrieve PartnerTree entity description" });
        }
    }
}
