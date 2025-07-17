namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Attributes;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerTreeController : BaseController
{
    private readonly IPartnerTreeManager _manager;

    public PartnerTreeController(
        IManagerWrapper managerWrapper, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerTreeController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.PartnerTreeManager;
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
}
