using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using System;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Presentation;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Specifications;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Entities;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerController : BaseController
{
    private readonly IPartnerManager _manager;
    private readonly IOrgUnitFilterService _orgUnitFilterService;

    public PartnerController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerController> logger,
        IOrgUnitFilterService orgUnitFilterService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.PartnerManager;
        _orgUnitFilterService = orgUnitFilterService;
    }

    /// <summary>
    /// Creates a new partner organization with complete details including address, contact info, and organizational metadata.
    /// </summary>
    /// <param name="req">Partner creation request with required fields</param>
    /// <param name="req.name">Partner organization name (required)</param>
    /// <param name="req.shortName">Short/abbreviated name (required)</param>
    /// <param name="req.status">Partner status (defaults to 'Active')</param>
    /// <param name="req.website">Partner website URL</param>
    /// <param name="req.phone">Partner phone number</param>
    /// <param name="req.address1Street">Street address line 1</param>
    /// <param name="req.address1City">City</param>
    /// <param name="req.address1Country">Country</param>
    /// <param name="req.partnerOfficeId">Associated UNOPS office ID</param>
    /// <param name="req.partnerGroupCode">Partner group classification code</param>
    /// <param name="req.globalKeyAccount">Whether this is a global key account</param>
    /// <param name="req.unSecretariatEntity">Whether this is a UN Secretariat entity</param>
    /// <param name="req.pooledFund">Pooled fund involvement</param>
    /// <param name="req.ddRequired">Due diligence requirement status</param>
    /// <example_uses>
    /// Create a new partner called UNICEF
    /// Add a new organization with website www.redcross.org
    /// Register a new government partner from Bangladesh
    /// Set up a partner organization with contact details
    /// Create a global key account partner
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to add, create, register, or set up a new partner.</when_to_use>
    /// <returns>Created partner with ID and metadata</returns>
    [HttpPost(APIDictionary.Partner)]
    [AccessControlled(EntityTypes.Partner, "create")]
    public async Task<IActionResult> Create([FromBody] PartnerRequest req)
    {
        var result = await _manager.CreatePartnerAsync(User, req);
        if (result == null)
        {
            throw new BusinessException("Failed to create partner");
        }
        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    /// <summary>
    /// Retrieves a list of partners with advanced filtering, pagination, search capabilities, and access control.
    /// </summary>
    /// <param name="request">Partner filter request containing search and pagination parameters</param>
    /// <param name="request.pageIndex">Page number (1-based)</param>
    /// <param name="request.pageSize">Number of items per page</param>
    /// <param name="request.searchText">Text to search across partner fields</param>
    /// <param name="request.orderBy">Field to order results by</param>
    /// <param name="request.ascending">Sort direction (true for ascending)</param>
    /// <param name="request.status">Filter by partner status</param>
    /// <param name="request.name">Filter by partner name</param>
    /// <param name="request.partnerOfficeId">Filter by partner office ID</param>
    /// <param name="request.globalKeyAccount">Filter by global key account status</param>
    /// <param name="request.orgUnitId">Filter by organizational unit ID for access control</param>
    /// <param name="advancedSearch">Enable advanced search mode for complex entity relationship searches</param>
    /// <param name="searchCriteria">JSON string containing advanced search filters with nested entity criteria. Required when advancedSearch=true</param>
    /// <param name="searchText">Text to search across partner fields (override for request.searchText)</param>
    /// <example_uses>
    /// Show me all partners
    /// List partners containing 'UNICEF' in the name
    /// Find partners with global key account status
    /// Show active partners from my office
    /// Search for government partners
    /// Get partners sorted by name
    /// Find partners in specific organizational unit
    /// Find partners with specific contacts (use advancedSearch=true, searchCriteria with contact.name)
    /// Get partners involved in certain interactions (use advancedSearch=true, searchCriteria with interaction.type)
    /// Find partners with documents containing keywords (advancedSearch=true)
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search, list, filter, or browse partners. Use advancedSearch=true for relationship-based searches involving contacts, interactions, or documents.</when_to_use>
    /// <advanced_search_guidance>
    /// **CRITICAL: When to use advancedSearch=true:**
    /// - User searches for partners BY CONTACT: "partners with contact John Smith", "partners having contact X"
    /// - User searches for partners BY INTERACTION: "partners involved in meeting Y", "partners from interaction Z"
    /// - User searches for partners BY DOCUMENT: "partners with document about climate"
    /// - User searches for partners BY OPPORTUNITY: "partners in opportunity X"
    /// - Any search involving related entities (contacts, interactions, documents, opportunities)
    /// 
             /// **searchCriteria JSON format with operators (CRITICAL - Must use this exact format):**
         /// searchCriteria must be a JSON array of SearchCriteria objects with field, operator, value, and logicalOperator
         /// 
         /// **Available Operators:**
         /// - "is" (exact match), "is not" (not equal), "like" (contains), "not like" (does not contain)
         /// - ">", "<", ">=", "<=" (comparisons), "after", "before", "between" (dates)
         /// 
         /// **Logical Operators:** "AND", "OR"
         /// 
         /// **Examples:**
         /// - Find partners with contact "John Smith": 
         ///   searchCriteria=[{"field": "contact.firstName", "operator": "like", "value": "John", "logicalOperator": "AND"}, {"field": "contact.lastName", "operator": "like", "value": "Smith"}]
         /// - Find partners involved in climate meetings: 
         ///   searchCriteria=[{"field": "interaction.subject", "operator": "like", "value": "climate", "logicalOperator": "AND"}, {"field": "interaction.type", "operator": "is", "value": "Meeting"}]
         /// - Find partners with documents about infrastructure: 
         ///   searchCriteria=[{"field": "document.description", "operator": "like", "value": "infrastructure"}]
         /// - Find active partners NOT like "Asian": 
         ///   searchCriteria=[{"field": "name", "operator": "not like", "value": "Asian", "logicalOperator": "AND"}, {"field": "status", "operator": "is", "value": "Active"}]
    /// 
    /// **Simple search (advancedSearch=false) for:**
    /// - Partner name/description text search: "partners named UNICEF"
    /// - Basic filtering: "active partners", "government partners"
    /// - Status/type searches: "global key account partners"
    /// </advanced_search_guidance>
    /// <returns>Paginated list of partners with metadata</returns>
    [HttpGet(APIDictionary.Partner)]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetAll(
        [FromQuery] PartnerFilterRequest request, 
        [FromQuery] bool advancedSearch = false, 
        [FromQuery] string? searchCriteria = null,
        [FromQuery] string? searchText = null)
    {
        
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        // Handle search using the new helper methods
        return await HandleSearchOperationAsync(async () =>
        {
            if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
            {
                // For advanced search, we need to parse the search criteria and combine with org unit filter
                // Since SearchControllerHelper expects a synchronous factory, we'll create the spec inline
                return await SearchControllerHelper.ProcessAdvancedSearch<PartnerFilterRequest, PartnerCompositeSpecification, PaginationResponse<PartnerModel>>(
                    searchCriteria, searchText ?? request.SearchText, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending, 
                    request, // Use request as pagination request since PartnerFilterRequest extends PaginationRequest
                    "Partner",
                    filterRequest => new PartnerCompositeSpecification(filterRequest),
                    async (userId, spec, pagination) => {
                        // If OrgUnitId is specified, use the OrgUnitFilterService to create a proper specification
                        if (pagination is PartnerFilterRequest partnerPagination && partnerPagination.OrgUnitId.HasValue)
                        {
                            var orgUnitSpec = await _orgUnitFilterService.CreatePartnerSpecificationAsync(partnerPagination, User);
                            var adaptedSpec = new PartnerSpecificationAdapter(orgUnitSpec);
                            return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, adaptedSpec, partnerPagination);
                        }
                        return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
                    },
                    CurrentUserId, _logger);
            }
            
            // Handle simple text search
            if (!string.IsNullOrWhiteSpace(searchText) || !string.IsNullOrWhiteSpace(request.SearchText))
            {
                var textToSearch = searchText ?? request.SearchText;
                return await SearchControllerHelper.ProcessSimpleTextSearch<PartnerFilterRequest, PartnerCompositeSpecification, PaginationResponse<PartnerModel>>(
                    textToSearch!, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending,
                    request,
                    "Partner",
                    filterRequest => new PartnerCompositeSpecification(filterRequest),
                    async (userId, spec, pagination) => {
                        // If OrgUnitId is specified, use the OrgUnitFilterService to create a proper specification
                        if (pagination is PartnerFilterRequest partnerPagination && partnerPagination.OrgUnitId.HasValue)
                        {
                            var orgUnitSpec = await _orgUnitFilterService.CreatePartnerSpecificationAsync(partnerPagination, User);
                            var adaptedSpec = new PartnerSpecificationAdapter(orgUnitSpec);
                            return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, adaptedSpec, partnerPagination);
                        }
                        return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
                    },
                    CurrentUserId, _logger);
            }
            
            // For no search parameters, return all partners with pagination
            _logger.LogInformation("Retrieving all partners with pagination. OrgUnitId filter: {OrgUnitId}", request.OrgUnitId);
            
            // Use OrgUnitFilterService to create the appropriate specification
            var unosPartnerSpec = await _orgUnitFilterService.CreatePartnerSpecificationAsync(request, User);
            var specification = new PartnerSpecificationAdapter(unosPartnerSpec);
            
            var result = await _manager.GetPartnersWithSpecificationAsync(User, specification, request);
            return (PaginationResponse<PartnerModel>)result;
        }, "partner search");
    }

    /// <summary>
    /// Retrieves a specific partner by ID with complete details including documents, contacts, and office information.
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <example_uses>
    /// Show me details for partner ID 123
    /// Get full information about partner 456
    /// Display partner record 789
    /// Get complete partner profile
    /// Show partner with all related data
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for specific partner details by ID or when you need complete partner information.</when_to_use>
    /// <returns>Complete partner details with related information</returns>
    [HttpGet(APIDictionary.Partner + "/{id}")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<IActionResult> Get(int id)
    {
        var partner = await _manager.GetPartnerAsync(User, id);
        if (partner == null)
        {
            return NotFound();
        }

        // Return partner data directly using JsonResult to avoid the wrapper metadata
        return new JsonResult(partner);
    }

    /// <summary>
    /// Updates an existing partner's information including contact details, status, and organizational metadata.
    /// </summary>
    /// <param name="req">Partner update request containing modified fields</param>
    /// <param name="req.id">Partner ID to update (required)</param>
    /// <param name="req.name">Updated partner name</param>
    /// <param name="req.shortName">Updated short name</param>
    /// <param name="req.status">Updated status</param>
    /// <param name="req.website">Updated website</param>
    /// <param name="req.phone">Updated phone number</param>
    /// <param name="req.globalKeyAccount">Updated key account status</param>
    /// <param name="req.partnerOfficeId">Updated office assignment</param>
    /// <example_uses>
    /// Update partner 123's name to New UNICEF
    /// Change partner 456's status to Inactive
    /// Update the website for partner 789
    /// Modify partner contact information
    /// Change partner office assignment
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to update, modify, edit, or change partner information.</when_to_use>
    /// <returns>Updated partner data</returns>
    [HttpPut(APIDictionary.Partner)]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> Update([FromBody] UpdatePartnerRequest req)
    {
        var result = await _manager.UpdatePartnerAsync(User, req);
        if (result == null)
        {
            return NotFound(); // Partner not found or user doesn't have permission
        }
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a partner from the system (marks as deleted rather than permanent removal).
    /// </summary>
    /// <param name="id">Partner ID to delete</param>
    /// <example_uses>
    /// Delete partner ID 123
    /// Remove partner 456 from the system
    /// Deactivate partner organization
    /// Soft delete partner record
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to delete, remove, or eliminate a partner.</when_to_use>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete(APIDictionary.Partner + "/{id}")]
    [AccessControlled(EntityTypes.Partner, "delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _manager.DeletePartnerAsync(User, id);
        if (!success)
        {
            return NotFound(); // Partner not found or user doesn't have permission
        }
        return NoContent();
    }

    /// <summary>
    /// Retrieves the current user's permissions for a specific partner (read, update, delete).
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <example_uses>
    /// Check my permissions for partner 123
    /// What can I do with partner 456?
    /// Get access rights for this partner
    /// Verify partner permissions
    /// </example_uses>
    /// <when_to_use>Use this when you need to check user permissions before performing operations or showing UI elements.</when_to_use>
    /// <returns>Permission object with CanRead, CanUpdate, CanDelete flags</returns>
    [HttpGet(APIDictionary.Partner + "/{id}/permissions")]
    public async Task<IActionResult> PermissionsGet(int id)
    {
        var partner = await _manager.GetPartnerAsync(User, id);
        if (partner == null)
        {
            return NotFound();
        }

        // Return permissions for this partner
        var permissions = new { CanRead = true, CanUpdate = true, CanDelete = true };
        
        return Ok(permissions);
    }

    /// <summary>
    /// Uploads and associates a logo image with a partner (max 1MB, JPEG/PNG/WEBP only).
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <param name="file">Image file (max 1MB)</param>
    /// <example_uses>
    /// Upload a logo for partner 123
    /// Add organization logo to partner record
    /// Set partner brand image
    /// Update partner visual identity
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to add or update a partner's logo/image.</when_to_use>
    /// <returns>Success confirmation or error details</returns>
    [HttpPost(APIDictionary.Partner + "/{id}/logo")]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded");
        }

        // Check file size (1MB max)
        if (file.Length > 1024 * 1024)
        {
            return BadRequest("File size exceeds maximum limit of 1MB");
        }

        // Validate file type
        var validImageTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!validImageTypes.Contains(file.ContentType))
        {
            return BadRequest("Invalid file type. Only JPEG, PNG, and WEBP files are allowed.");
        }

        var result = await _manager.UpdatePartnerLogoAsync(id, file);
        return Ok(new { imageUrl = result });
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
    [HttpGet(APIDictionary.Partner + "/by-partner-group-code/{code}")]
    // [AccessControlled(EntityTypes.Partner, "read", applyColumnFiltering: true, applyRowFiltering: true)]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerGroup(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _manager.GetPartnersByPartnerGroupAsync(User, code, request);
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
    [HttpGet(APIDictionary.Partner + "/by-partner-category-code/{code}")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerCategory(string code, [FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _manager.GetPartnersByCategoryAsync(User, code, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
