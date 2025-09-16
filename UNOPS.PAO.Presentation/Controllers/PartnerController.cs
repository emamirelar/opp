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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerController : BaseController
{
    private readonly IPartnerManager _manager;
    private readonly IGeminiManager _geminiManager;
    private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;
    private readonly AiContextualService _aiContextualService;

    public PartnerController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerController> logger,
        AiContextualService aiContextualService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.PartnerManager;
        _geminiManager = manager.GeminiManager;
        _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
        _aiContextualService = aiContextualService;
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
    /// <param name="req.organizationUnitRelationships">Associated UNOPS office relationships</param>
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
        // Validate minimum required fields for creation
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            return BadRequest(new { error = "Partner Name is required for creation" });
        }
        
        // Check for duplicates ONLY if user hasn't confirmed duplicate creation
        if (!req.ConfirmDuplicateCreation)
        {
            try
            {
                var duplicateResult = await _aiContextualService.DetectDuplicateForSingleRecordAsync(
                    "Partner", 
                    req, 
                    0.5 // Lower threshold for more sensitive detection
                );
                
                if (duplicateResult != null && duplicateResult.HasDuplicates)
                {
                    return Ok(new {
                        success = false,
                        action = "duplicateConfirmation",
                        message = "Potential duplicate partner detected. Do you want to create anyway?",
                        duplicateInfo = new {
                            totalDuplicates = duplicateResult.TotalDuplicates,
                            highConfidence = duplicateResult.HighConfidence,
                            mediumConfidence = duplicateResult.MediumConfidence,
                            lowConfidence = duplicateResult.LowConfidence,
                            topDuplicate = duplicateResult.TopDuplicate != null ? new {
                                entityId = duplicateResult.TopDuplicate.EntityId,
                                score = duplicateResult.TopDuplicate.Score,
                                matchReason = duplicateResult.TopDuplicate.MatchReason,
                                matchedData = duplicateResult.TopDuplicate.MatchedData
                            } : null
                        },
                        confirmationRequired = true,
                        originalData = req
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't block creation due to duplicate detection failure
                _logger.LogWarning($"Duplicate detection failed for partner creation: {ex.Message}");
                // Continue with creation since duplicate detection is not critical
            }
        }
        
        // Ensure partner is created in Draft status
        req.Status = "Draft";
        
        // Create the partner (either no duplicates found, or user confirmed creation)
        var result = await _manager.CreatePartnerAsync(User, req);
        if (result == null)
        {
            throw new BusinessException("Failed to create partner");
        }
        
        return StatusCode(201, new {
            success = true,
            action = "created",
            message = req.ConfirmDuplicateCreation ? 
                "Partner created successfully (duplicate confirmation acknowledged)" : 
                "Partner created successfully",
            data = result
        });
    }

    /// <summary>
    /// Retrieves all partners with basic pagination and ordering (no search criteria).
    /// </summary>
    /// <param name="pageIndex">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order results by (default: 'createdDate')</param>
    /// <param name="ascending">Sort direction - true for ascending, false for descending (default: false for newest first)</param>
    /// <example_uses>
    /// Show me all partners
    /// List all partners in the system
    /// Display the partner directory
    /// Get all partner records
    /// Browse partners
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see ALL partners without any search criteria or when asking for a general partner list.</when_to_use>
    /// <returns>Paginated list of all partners</returns>
    [HttpGet(APIDictionary.Partner)]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> ListAllPartners(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string? orderBy = "CreatedDate",
        [FromQuery] int? partnerGroupId = null,
        [FromQuery] bool ascending = false)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(pageIndex, pageSize);
        if (validationResult != null) return validationResult;
        
        return await HandleSearchOperationAsync(async () =>
        {
            // Create a basic PartnerFilterRequest with just pagination and ordering
            var request = new PartnerFilterRequest
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                OrderBy = orderBy ?? "createdDate",
                Ascending = ascending,
                PartnerGroupId = partnerGroupId
            };
            
            // Create simple specification - global filters will be applied by the manager
            var specification = new PartnerCompositeSpecification(request);
            
            var result = await _manager.GetPartnersWithSpecificationAsync(User, specification, request);
            return (PaginationResponse<PartnerModel>)result;
        }, "partner list all");
    }

    /// <summary>
    /// Performs simple text search across multiple partner fields (name, description, etc.).
    /// </summary>
    /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
    /// <param name="searchText">Text to search across partner name, description, and other basic fields. 
    /// Supports phrase search (e.g., "University of Oxford") and OR search with pipe separator (e.g., "UNICEF|WHO")</param>
    /// <example_uses>
    /// Search for partners named UNICEF
    /// Find partners containing 'Government'
    /// Search for partners with 'Development' in description
    /// Look for partners with specific keywords
    /// Find partner by short name or acronym
    /// Search for full partner names with spaces: "University of Oxford"
    /// Search for multiple terms with OR: "UNICEF|WHO|UNDP"
    /// </example_uses>
    /// <when_to_use>Use this for simple name, description, or basic field searches. NOT for complex criteria or relationship searches.</when_to_use>
    /// <returns>Paginated list of partners matching the search text</returns>
    [HttpGet(APIDictionary.Partner + "/search")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> SearchPartners(
        [FromQuery] PaginationRequest request,
        [FromQuery] string searchText)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            throw new BusinessException("Search text is required for partner search");
        }

        return await HandleSearchOperationAsync(async () =>
        {
            // Create a PartnerFilterRequest with pagination/sorting info and search text
            var partnerFilterRequest = new PartnerFilterRequest
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                OrderBy = request.OrderBy ?? "createdDate",
                Ascending = request.Ascending,
                SearchText = searchText
            };

            return await SearchControllerHelper.ProcessSimpleTextSearch<PartnerFilterRequest, PartnerCompositeSpecification, PaginationResponse<PartnerModel>>(
                searchText, request.PageIndex, request.PageSize, request.OrderBy ?? "createdDate", request.Ascending,
                partnerFilterRequest,
                "Partner",
                filterRequest => new PartnerCompositeSpecification(filterRequest),
                async (userId, spec, pagination) => {
                    // Use regular specification - global filters handled automatically by BaseRepository
                    return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
                },
                CurrentUserId, _logger);
        }, "partner simple search");
    }

    /// <summary>
    /// Performs advanced search with structured criteria including status, dates, relationships, and complex filters.
    /// </summary>
    /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
    /// <param name="searchCriteria">JSON array of search criteria objects with field, operator, value, and logicalOperator</param>
    /// <param name="searchText">Optional additional text search to combine with criteria</param>
    /// <example_uses>
    /// Find active government partners
    /// Show partners with global key account status
    /// Get partners created this month
    /// Find partners by status and office location
    /// List partners involved in climate projects
    /// Search for partners by complex criteria combinations
    /// </example_uses>
    /// <when_to_use>Use this for searches involving partner status, dates, types, complex criteria, or multiple field combinations.</when_to_use>
    /// <searchCriteria_format>
    /// JSON array format: [{"field": "status", "operator": "is", "value": "Active", "logicalOperator": "AND"}]
    /// Available operators: is, is not, like, not like, greater than, less than, greater than or equal, less than or equal, this week, this month, this year
    /// Available fields: name, status, partnerShortDescription, partnerLongDescription, partnerCategoryId, liaisonOfficeId, partnerGroupCode, keyGlobalPartner, unSecretariatPartner, partnerApprovalStatus, partnerLevyStatus, pooledFund, canCreateNewOpportunities, createdDate, lastModifiedDate
    /// Logical operators: AND, OR
    /// </searchCriteria_format>
    /// <returns>Paginated list of partners matching the advanced search criteria</returns>
    [HttpGet(APIDictionary.Partner + "/advanced-search")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> AdvancedSearchPartners(
        [FromQuery] PaginationRequest request,
        [FromQuery] string searchCriteria,
        [FromQuery] string? searchText = null)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        if (string.IsNullOrWhiteSpace(searchCriteria))
        {
            throw new BusinessException("Search criteria is required for advanced partner search");
        }

        return await HandleSearchOperationAsync(async () =>
        {
            // Create a PartnerFilterRequest with pagination/sorting info and search criteria
            var partnerFilterRequest = new PartnerFilterRequest
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                OrderBy = request.OrderBy ?? "createdDate",
                Ascending = request.Ascending,
                SearchCriteria = searchCriteria,
                SearchText = searchText,
                AdvancedSearch = true // Set this internally since we know this is an advanced search
            };

            return await SearchControllerHelper.ProcessAdvancedSearch<PartnerFilterRequest, PartnerCompositeSpecification, PaginationResponse<PartnerModel>>(
                searchCriteria, searchText, request.PageIndex, request.PageSize, request.OrderBy ?? "createdDate", request.Ascending, 
                partnerFilterRequest,
                "Partner",
                filterRequest => new PartnerCompositeSpecification(filterRequest),
                async (userId, spec, pagination) => {
                    // Use regular specification - global filters handled automatically by BaseRepository
                    return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
                },
                CurrentUserId, _logger);
        }, "partner advanced search");
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
    /// <param name="req.organizationUnitRelationships">Updated office assignments via OrganizationUnitRelationships</param>
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
    /// Retrieves all engagements for a specific partner with complete details and pagination.
    /// </summary>
    /// <param name="partnerId">Partner ID to get engagements for</param>
    /// <param name="pageIndex">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order results by (default: 'createdDate')</param>
    /// <param name="ascending">Sort direction - true for ascending, false for descending (default: false for newest first)</param>
    /// <example_uses>
    /// Show all engagements for partner 123
    /// List partner's project engagements
    /// Get engagement history for this partner
    /// Display partner collaboration records
    /// Show partner's active engagements
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see all engagements associated with a specific partner from the partner's perspective.</when_to_use>
    /// <returns>Paginated list of engagements for the specified partner</returns>
    [HttpGet(APIDictionary.Partner + "/{partnerId}/engagements")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<Engagement>>> GetPartnerEngagements(
        int partnerId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "CreatedDate",
        [FromQuery] bool ascending = false)
    {
        try
        {
            var result = await _manager.GetPartnerEngagementsAsync(User, partnerId, pageIndex, pageSize, orderBy ?? "createdDate", ascending);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner engagements for partner {PartnerId}", partnerId);
            return StatusCode(500, new { error = "An error occurred while retrieving partner engagements" });
        }
    }

    /// <summary>
    /// Retrieves all projects associated with a specific partner with complete details and pagination.
    /// </summary>
    /// <param name="partnerId">Partner ID to get projects for</param>
    /// <param name="pageIndex">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order results by (default: 'createdDate')</param>
    /// <param name="ascending">Sort direction - true for ascending, false for descending (default: false for newest first)</param>
    /// <example_uses>
    /// Show all projects for partner 123
    /// List partner's project portfolio
    /// Get project history for this partner
    /// Display partner's active projects
    /// Show partner collaboration projects
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see all projects associated with a specific partner.</when_to_use>
    /// <returns>Paginated list of projects for the specified partner</returns>
    [HttpGet(APIDictionary.Partner + "/{partnerId}/projects")]
    public async Task<ActionResult<PaginationResponse<object>>> GetPartnerProjects(
        int partnerId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "CreatedDate",
        [FromQuery] bool ascending = false)
    {
        try
        {
            var result = await _manager.GetPartnerProjectsAsync(User, partnerId, pageIndex, pageSize, orderBy ?? "createdDate", ascending);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner projects for partner {PartnerId}", partnerId);
            return StatusCode(500, new { error = "An error occurred while retrieving partner projects" });
        }
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
    /// Activates a draft partner after validating mandatory fields
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <param name="request">Activation request with optional notes</param>
    /// <returns>Updated partner with new status</returns>
    [HttpPost(APIDictionary.Partner + "/{id}/activate")]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> ActivatePartner(int id, [FromBody] ActivatePartnerRequest request)
    {
        try
        {
            var result = await _manager.ActivatePartnerAsync(User, id, request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Closes an active partner (only for NotApproved partners)
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <param name="request">Close request with optional notes</param>
    /// <returns>Updated partner with closed status</returns>
    [HttpPost(APIDictionary.Partner + "/{id}/close")]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> ClosePartner(int id, [FromBody] StatusChangeRequest request)
    {
        try
        {
            var result = await _manager.ClosePartnerAsync(User, id, request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Archives an active or closed partner (only for NotApproved partners)
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <param name="request">Archive request with optional notes</param>
    /// <returns>Updated partner with archived status</returns>
    [HttpPost(APIDictionary.Partner + "/{id}/archive")]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> ArchivePartner(int id, [FromBody] StatusChangeRequest request)
    {
        try
        {
            var result = await _manager.ArchivePartnerAsync(User, id, request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Approves an active partner (Admin only) - locks data fields and records approval audit trail
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <param name="request">Approval request with optional notes</param>
    /// <returns>Updated partner with approved status</returns>
    [HttpPost(APIDictionary.Partner + "/{id}/approve")]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> ApprovePartner(int id, [FromBody] UpdatePartnerRequest request)
    {
        try
        {
            var result = await _manager.ApprovePartnerAsync(User, id, request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
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
    [HttpGet(APIDictionary.Partner + "/by-partner-group-id/{id}")]
    // [AccessControlled(EntityTypes.Partner, "read", applyColumnFiltering: true, applyRowFiltering: true)]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> GetPartnersByPartnerGroup(int id, [FromQuery] PaginationRequest request)
    {
        try
        {
            // Ensure default ordering by createdDate if not specified
            if (string.IsNullOrEmpty(request.OrderBy))
            {
                request.OrderBy = "createdDate";
            }
            
            var result = await _manager.GetPartnersByPartnerGroupAsync(User, id, request);
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
            // Ensure default ordering by createdDate if not specified
            if (string.IsNullOrEmpty(request.OrderBy))
            {
                request.OrderBy = "createdDate";
            }
            var result = await _manager.GetPartnersByCategoryAsync(User, code, request);
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
    [HttpGet(APIDictionary.Partner + "/categories-summary")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult> GetAllPartnerCategories()
    {
        try
        {
            // Get all unique partner category codes from partner trees
            var partnerTrees = await _manager.GetPartnersAsync(User, new PaginationRequest { PageSize = int.MaxValue });
            
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
    [HttpGet(APIDictionary.Partner + "/groups-summary")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult> GetAllPartnerGroups()
    {
        try
        {
            // Get all partners to analyze groups
            var partnerTrees = await _manager.GetPartnersAsync(User, new PaginationRequest { PageSize = int.MaxValue });
            
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
    [HttpGet(APIDictionary.Partner + "/categorization-overview")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult> GetPartnerCategorizationOverview()
    {
        try
        {
            // Get all partners for analysis
            var partnerTrees = await _manager.GetPartnersAsync(User, new PaginationRequest { PageSize = int.MaxValue });
            
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

    #region AI-Powered Partner Data Processing

    /// <summary>
    /// Scans and processes uploaded files for partner data extraction using AI-powered analysis.
    /// </summary>
    /// <param name="req">File scan request containing the file to be processed</param>
    /// <param name="req.File">File to scan for partner data (required)</param>
    /// <example_uses>
    /// Scan partner contract document for data extraction
    /// Upload partner registration form for processing
    /// Analyze partner profile document with AI
    /// Extract data from partner onboarding files
    /// Process partner information from uploaded documents
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to upload and scan documents for partner data extraction using AI.</when_to_use>
    /// <returns>Extracted partner data from the scanned file</returns>
    [HttpPost(APIDictionary.Partner + "/scan-data")]
    [AccessControlled(EntityTypes.Partner, "create")]
    public async Task<ActionResult> ScanPartnerData([FromForm] GeminiFileRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req?.File == null || req?.File.Length == 0)
            {
                throw new BusinessException("No valid file detected.");
            }

            string fileType = _geminiManager.FindFileType(req.File);

            if (string.IsNullOrEmpty(fileType)) 
            {
                throw new BusinessException("File type not compatible");
            }

            string response = await _geminiManager.ScanFileForGeminiProcessing(req);

            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Prompt configuration for partner data scanning is not found.");
            }

            return response.Trim();
        });
    }

    /// <summary>
    /// Analyzes uploaded files and extracts structured partner data using AI-powered data analysis.
    /// </summary>
    /// <param name="request">Analysis request containing file and analysis parameters</param>
    /// <param name="request.entityType">Should be set to 'Partner' for partner data analysis</param>
    /// <param name="request.analysisType">Type of analysis to perform on partner data</param>
    /// <example_uses>
    /// Analyze partner documents for structured data extraction
    /// Extract partner information from uploaded forms
    /// Process partner onboarding documents with AI
    /// Convert partner files into structured database entries
    /// Analyze partner contract data for key information
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to analyze files and extract structured partner data for database import.</when_to_use>
    /// <returns>Structured partner data extracted from the analyzed file</returns>
    [HttpPost(APIDictionary.Partner + "/analyse-file")]
    [AccessControlled(EntityTypes.Partner, "create")]
    public async Task<ActionResult> AnalysePartnerData([FromBody] AnalyseFileRequest request)
    {
        return await HandleOperationAsync(async () => 
        {
            if (request == null)
            {
                throw new BusinessException("Invalid request.");
            }

            return await _geminiManager.ExtractDataAfterAnalysis(request, CurrentUserId);
        });
    }

    /// <summary>
    /// Bulk uploads multiple partner records using AI-assisted data processing and validation.
    /// </summary>
    /// <param name="req">Bulk upload request containing partner data</param>
    /// <param name="req.Type">Should be set to 'Partner' for partner bulk upload</param>
    /// <param name="req.Data">Array of partner data objects to upload</param>
    /// <param name="req.Options">Upload options and validation settings</param>
    /// <example_uses>
    /// Bulk upload 100 partner organizations from Excel
    /// Import multiple partners from CSV file
    /// Mass upload partner data with AI validation
    /// Bulk import partner records with duplicate detection
    /// Upload large partner dataset with automated processing
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to upload multiple partner records at once with AI-assisted processing.</when_to_use>
    /// <returns>Bulk upload results with success/failure status for each partner</returns>
    [HttpPost(APIDictionary.Partner + "/bulk-upload")]
    [AccessControlled(EntityTypes.Partner, "create")]
    public async Task<ActionResult> BulkUploadPartners([FromBody] BulkUploadRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.Type))
            {
                throw new BusinessException("Invalid request.");
            }

            // Ensure the request is for partner entities
            if (!req.Type.Equals("Partner", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("This endpoint only supports Partner bulk uploads.");
            }

            string response = await _geminiManager.BulkInsertRecordsAsync(req);
            return new { message = response };
        });
    }

    #endregion

    /// <summary>
    /// Describes the Partner entity structure including all field configurations
    /// </summary>
    /// <returns>Entity and field metadata for Partner</returns>
    [HttpGet(APIDictionary.Partner + "/metadata-info")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult> GetMetadataInfo()
    {
        try
        {
            var entityDetails = await _entityConfigurationManager.GetEntityConfigurationDetailsAsync(User, "Partner");
            return Ok(entityDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Partner entity description");
            return StatusCode(500, new { error = "Failed to retrieve Partner entity description" });
        }
    }

    /// <summary>
    /// Performs semantic search on partners using AI embeddings to find similar partners based on natural language queries.
    /// </summary>
    /// <param name="query">Natural language search query</param>
    /// <param name="threshold">Similarity threshold (0.0 to 1.0, default: 0.7)</param>
    /// <param name="limit">Maximum number of results to return (default: 10)</param>
    /// <example_uses>
    /// Find partners similar to UNICEF
    /// Search for government organizations in Africa
    /// Find NGOs working on healthcare
    /// Search for partners in the education sector
    /// Find organizations similar to Red Cross
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to find partners using natural language queries or semantic similarity.</when_to_use>
    /// <returns>List of similar partners with similarity scores</returns>
    [HttpGet(APIDictionary.Partner + "/deepSearch")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult> DeepSearch(
        [FromQuery] string query,
        [FromQuery] float threshold = 0.7f,
        [FromQuery] int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            if (threshold < 0.0f || threshold > 1.0f)
            {
                return BadRequest(new { error = "Threshold must be between 0.0 and 1.0" });
            }

            if (limit <= 0 || limit > 100)
            {
                return BadRequest(new { error = "Limit must be between 1 and 100" });
            }

            // Generate embedding for the search query
            var embedding = await _aiContextualService.CreateEmbeddingForText(query);
            
            // Perform semantic search
            var searchResults = await _aiContextualService.ExecuteEmbeddingSearchMultiple(
                "Partner", 
                embedding, 
                threshold, 
                limit
            );

            // Get the actual partner data for the found IDs
            var partners = new List<object>();
            foreach (var result in searchResults)
            {
                try
                {
                    var partner = await _manager.GetPartnerAsync(User, result.EntityId);
                    if (partner != null)
                    {
                        partners.Add(new
                        {
                            partner = partner,
                            similarityScore = result.Score,
                            searchType = result.SearchType
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve partner {PartnerId} from search results", result.EntityId);
                }
            }

            return Ok(new
            {
                query = query,
                threshold = threshold,
                totalResults = searchResults.Count,
                results = partners
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing deep search for partners with query: {Query}", query);
            return StatusCode(500, new { error = "An error occurred while performing the semantic search" });
        }
    }
}
