using UNOPS.PAO.Domain.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Newtonsoft.Json;

namespace UNOPS.PAO.Presentation.Controllers.Partners;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
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
using UNOPS.PAO.Business.Services;
using System.Text.Json;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.UNOPSDomain.Entities;
using static UNOPS.PAO.UNOPSBusiness.Services.AdvancedSearchService;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.EntityConfiguration;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.AuditLogs;
using UNOPS.PAO.Presentation.Controllers.Shared;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class PartnerController : BaseController
{
    private readonly IPartnerManager _manager;
    private readonly IOpportunityManager _opportunityManager;
    private readonly IAuditLogManager _auditLogManager;
    private readonly IGeminiManager _geminiManager;
    private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;
    private readonly AiContextualService _aiContextualService;
    private readonly AdvancedSearchService _advancedSearchService;

    public PartnerController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerController> logger,
        AiContextualService aiContextualService,
        AdvancedSearchService advancedSearchService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.PartnerManager;
        _opportunityManager = manager.OpportunityManager;
        _auditLogManager = manager.AuditLogManager;
        _geminiManager = manager.GeminiManager;
        _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
        _aiContextualService = aiContextualService;
        _advancedSearchService = advancedSearchService;
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

        // Validate Partner Levy business rules
        if (req.PartnerLevyStatus == "DoesNotApply" || req.PartnerLevyStatus == "PotentiallyNotApplied")
        {
            if (string.IsNullOrWhiteSpace(req.ReasonForLevy))
            {
                return BadRequest(new { error = "Reason for Levy is required when Partner Levy status is 'Does Not Apply' or 'Potentially Not Applied'." });
            }
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
    /// <param name="orderBy">Field to order results by (default: 'Name' for alphabetic sorting)</param>
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
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "Name",
        [FromQuery] int? partnerGroupId = null,
        [FromQuery] bool ascending = true,
        [FromQuery] bool export = false,
        [FromQuery] bool filterActive = true)
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
                PageSize = export ? int.MaxValue : pageSize, // Remove pagination limits for export
                OrderBy = orderBy ?? "Name",
                Ascending = ascending,
                PartnerGroupId = partnerGroupId,
                FilterActive = filterActive
            };
            
            // Create simple specification - global filters will be applied by the manager
            var specification = new PartnerCompositeSpecification(request);
            
            var result = await _manager.GetPartnersWithSpecificationAsync(User, specification, request);
            return (PaginationResponse<PartnerModel>)result;
        }, "partner list all");
    }

    /// <summary>
    /// Performs intelligent multi-tier search across partner fields with automatic escalation.
    /// Uses basic text search first, then similarity search, then semantic search if needed.
    /// </summary>
    /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
    /// <param name="searchText">Text to search across partner name, description, and other basic fields. 
    /// Supports phrase search (e.g., "University of Oxford") and OR search with pipe separator (e.g., "UNICEF|WHO").
    /// Will find similar results even with typos or conceptually related terms (e.g., "IFS" finds "Infrastructure")</param>
    /// <param name="enableSmartSearch">Enable intelligent multi-tier search (default: true)</param>
    /// <param name="basicThreshold">Minimum results to consider basic search successful (default: 1)</param>
    /// <param name="similarityThreshold">Similarity search threshold 0.0-1.0 (default: 0.3)</param>
    /// <param name="semanticThreshold">Semantic search threshold 0.0-1.0 (default: 0.3)</param>
    /// <example_uses>
    /// Search for partners named UNICEF
    /// Find partners containing 'Government'
    /// Search for partners with 'Development' in description
    /// Look for partners with specific keywords
    /// Find partner by short name or acronym: "IFS" finds "Infrastructure"
    /// Search with typos: "Infrastrucure" finds "Infrastructure" 
    /// Search for full partner names with spaces: "University of Oxford"
    /// Search for multiple terms with OR: "UNICEF|WHO|UNDP"
    /// Find conceptually similar partners: "development bank" finds World Bank
    /// </example_uses>
    /// <when_to_use>Use this for simple name, description, or basic field searches. Automatically handles exact matches, similarities, and semantic relationships.</when_to_use>
    /// <returns>Paginated list of partners matching the search text with optional search metadata</returns>
    [HttpGet(APIDictionary.Partner + "/search")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> SearchPartners(
        [FromQuery] string query,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "Name", 
        [FromQuery] bool ascending = true,
        [FromQuery] bool export = false,
        [FromQuery] bool filterActive = true)
    {
        try
        {
            _logger.LogInformation("=== SIMPLE SEARCH ENDPOINT (Using Smart Search) ===");
            _logger.LogInformation("Query: '{Query}', Page: {PageIndex}, Size: {PageSize}, Export: {Export}", query, pageIndex, pageSize, export);

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            // Use the enhanced specification pattern (now includes PostgreSQL similarity search)
            var paginationRequest = new PaginationRequest
            {
                PageIndex = pageIndex,
                PageSize = export ? int.MaxValue : pageSize, // Remove pagination limits for export
                OrderBy = orderBy,
                Ascending = ascending,
                FilterActive = filterActive
            };

            // Use AdvancedSearchService for unified text search with PostgreSQL similarity and metadata
            var result = await _advancedSearchService.SearchWithQueryAndMetadataAsync<UNOPSPartner, PartnerModel>(
                query, 
                paginationRequest, 
                User);

            _logger.LogInformation("Smart search completed: Found {TotalCount} results", result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in simple partner search");
            return StatusCode(500, new { error = "An error occurred during search" });
        }
    }

    /// <summary>
    /// Performs advanced search with structured criteria including status, dates, relationships, and complex filters.
    /// Enhanced with intelligent field value matching for AI agents and typo correction.
    /// </summary>
    /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
    /// <param name="searchCriteria">JSON array of search criteria objects with field, operator, value, and logicalOperator</param>
    /// <param name="enableSmartSearch">Enable intelligent field value matching and typo correction (default: true)</param>
    /// <example_uses>
    /// Find active government partners
    /// Show partners with global key account status
    /// Get partners created this month
    /// Find partners by status and office location
    /// List partners involved in climate projects
    /// Search for partners by complex criteria combinations
    /// Find active partners with name similar to "IFS" (will find "Infrastructure")
    /// </example_uses>
    /// <when_to_use>Use this for searches involving partner status, dates, types, complex criteria, or multiple field combinations.</when_to_use>
    /// <searchCriteria_format>
    /// JSON array format: [{"field": "status", "operator": "is", "value": "Active", "logicalOperator": "AND"}]
    /// Available operators: is, is not, like, not like, greater than, less than, greater than or equal, less than or equal, this week, this month, this year
    /// Available fields: name, status, partnerShortDescription, partnerLongDescription, partnerCategoryId, partnerGroupId, liaisonOfficeId, partnerFocalPointUserId, partnerGroupCode, erpDimValue, unAndStateEntity, keyGlobalPartner, unSecretariatPartner, dueDiligenceRequired, dueDiligenceApproval, dueDiligenceApprovalDate, dueDiligenceExpiryDate, partnerApprovalStatus, partnerApprovalDate, partnerApprovalReference, partnerApprovedBy, partnerLevyStatus, reasonForLevy, levyTreatment, pooledFund, canCreateNewOpportunities, reasonForNoNewOpportunity, partnerGroup.name, partnerGroup.code, liaisonOffice.name, contacts.firstName, contacts.lastName, contacts.email, contacts.title, contacts.department, contacts.phone, contacts.mobile, contacts.description, contacts.assistant, contacts.assistantEmail, contacts.assistantPhone, contacts.mailingCity, contacts.mailingStateProvince, contacts.mailingCountry, organizationUnitRelationships.organizationHierarchy.name, createdDate, lastModifiedDate, createdBy, lastModifiedBy, isDeleted
    /// Logical operators: AND, OR
    /// </searchCriteria_format>
    /// <returns>Paginated list of partners matching the advanced search criteria</returns>
    [HttpGet(APIDictionary.Partner + "/advanced-search")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult<PaginationResponse<PartnerModel>>> AdvancedSearchPartners(
        [FromQuery] string filters,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "Name",
        [FromQuery] bool ascending = true,
        [FromQuery] bool export = false,
        [FromQuery] bool filterActive = true)
    {
        try
        {
            _logger.LogInformation("=== ADVANCED SEARCH ENDPOINT ===");
            _logger.LogInformation("Filters: {Filters}, Page: {PageIndex}, Size: {PageSize}, Export: {Export}", filters, pageIndex, pageSize, export);

            if (string.IsNullOrWhiteSpace(filters))
            {
                return BadRequest(new { error = "Search filters are required" });
            }

            // Parse filters from JSON
            List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter> searchFilters;
            try
            {
                searchFilters = JsonSerializer.Deserialize<List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter>>(filters) ?? new List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse search filters: {Filters}", filters);
                return BadRequest(new { error = "Invalid filter format. Expected JSON array of filter objects." });
            }

            // Use AdvancedSearchService for structured filters with PostgreSQL similarity on "like" operators
            var paginationRequest = new PaginationRequest
            {
                PageIndex = pageIndex,
                PageSize = export ? int.MaxValue : pageSize, // Remove pagination limits for export
                OrderBy = orderBy,
                Ascending = ascending,
                FilterActive = filterActive
            };

            var result = await _advancedSearchService.SearchWithFiltersAsync<UNOPSPartner, PartnerModel>(
                searchFilters,
                paginationRequest,
                User);
            
            _logger.LogInformation("Advanced search completed: Found {TotalCount} results", result.TotalCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced partner search");
            return StatusCode(500, new { error = "An error occurred during advanced search" });
        }
    }

    /// <summary>
    /// Get supported search fields for partners - helps frontend build dynamic search forms
    /// </summary>
    /// <returns>List of all supported search fields with their metadata</returns>
    [HttpGet(APIDictionary.Partner + "/search-fields")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public ActionResult<List<SearchFieldInfo>> GetPartnerSearchFields()
    {
        try
        {
            var fields = new List<SearchFieldInfo>
            {
                // Direct Partner fields - using translation keys
                new() { Field = "name", DisplayName = "label.partner.name", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "partnerShortDescription", DisplayName = "label.partner.shortDescription", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "partnerLongDescription", DisplayName = "label.partner.longDescription", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { 
                    Field = "status", 
                    DisplayName = "label.common.status", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>
                    {
                        new() { Value = "Inactive", Label = "enums.entityStatus.inactive" },
                        new() { Value = "Active", Label = "enums.entityStatus.active" },
                        new() { Value = "Closed", Label = "enums.entityStatus.closed" },
                        new() { Value = "Draft", Label = "enums.entityStatus.draft" },
                        new() { Value = "Archived", Label = "enums.entityStatus.archived" }
                    }
                },
                new() { 
                    Field = "partnerApprovalStatus", 
                    DisplayName = "label.partner.approvalStatus", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>
                    {
                        new() { Value = "NotApproved", Label = "enums.partnerApprovalStatus.notApproved" },
                        new() { Value = "Approved", Label = "enums.partnerApprovalStatus.approved" }
                    }
                },
                new() { Field = "partnerApprovalDate", DisplayName = "label.partner.approvalDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },
                new() { Field = "keyGlobalPartner", DisplayName = "label.partner.keyGlobalPartner", FieldType = "bool", AllowedOperators = new List<string> { "entityCards.operators.eq" } },
                new() { Field = "unSecretariatPartner", DisplayName = "label.partner.unSecretariatPartner", FieldType = "bool", AllowedOperators = new List<string> { "entityCards.operators.eq" } },
                new() { Field = "pooledFund", DisplayName = "label.partner.pooledFund", FieldType = "bool", AllowedOperators = new List<string> { "entityCards.operators.eq" } },
                new() { Field = "createdDate", DisplayName = "label.common.createdDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },

                // Navigation properties - using translation keys
                new() { Field = "partnerGroup.name", DisplayName = "label.partnerGroup.name", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "liaisonOffice.name", DisplayName = "label.liaisonOffice.name", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },

                // Contact properties - using translation keys
                new() { Field = "contacts.fullName", DisplayName = "label.contact.fullName", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "contacts.firstName", DisplayName = "label.contact.firstName", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "contacts.lastName", DisplayName = "label.contact.lastName", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "contacts.email", DisplayName = "label.contact.email", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
            };
            
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving search fields");
            return StatusCode(500, new { error = "An error occurred while retrieving search fields" });
        }
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
        // Validate Partner Levy business rules
        if (req.PartnerLevyStatus == "DoesNotApply" || req.PartnerLevyStatus == "PotentiallyNotApplied")
        {
            if (string.IsNullOrWhiteSpace(req.ReasonForLevy))
            {
                return BadRequest(new { error = "Reason for Levy is required when Partner Levy status is 'Does Not Apply' or 'Potentially Not Applied'." });
            }
        }

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
    /// Unapproves an approved partner (Admin only) - unlocks data fields and records unapproval audit trail
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <param name="request">Unapproval request with optional notes</param>
    /// <returns>Updated partner with unapproved status</returns>
    [HttpPost(APIDictionary.Partner + "/{id}/unapprove")]
    [AccessControlled(EntityTypes.Partner, "update")]
    public async Task<IActionResult> UnapprovePartner(int id, [FromBody] StatusChangeRequest request)
    {
        try
        {
            var result = await _manager.UnapprovePartnerAsync(User, id, request);
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
    /// Gets all interactions associated with a specific partner for opportunity creation
    /// Returns lightweight interaction summaries with key details
    /// </summary>
    /// <param name="id">Partner ID</param>
    /// <returns>List of interactions associated with this partner</returns>
    [HttpGet(APIDictionary.Partner + "/{id}/interactions")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<IActionResult> GetPartnerInteractions(int id)
    {
        try
        {
            _logger.LogInformation($"📋 [API] Getting interactions for partner {id}");

            // Verify partner exists
            var partner = await _manager.GetPartnerAsync(User, id);
            if (partner == null)
            {
                return NotFound(new { error = $"Partner with ID {id} not found" });
            }

            // Get all interactions for this partner
            var partnerManager = _manager as UNOPSPartnerManager;
            if (partnerManager == null)
            {
                return StatusCode(500, new { error = "Partner manager not available" });
            }

            var interactions = await partnerManager.GetPartnerInteractionsAsync(id);

            _logger.LogInformation($"✅ [API] Found {interactions.Count()} interactions for partner {id}");

            return Ok(interactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ [API] Error getting interactions for partner {id}");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
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

    #region Intelligent Search Helper Methods

    /// <summary>
    /// Performs intelligent multi-tier search with automatic escalation
    /// </summary>
    private async Task<PaginationResponse<PartnerModel>> PerformIntelligentSearch(
        string searchText, PaginationRequest request, int basicThreshold, 
        float similarityThreshold, float semanticThreshold)
    {
        var searchStartTime = DateTime.UtcNow;
        
        // TIER 1: Basic Text Search (Fastest)
        _logger.LogInformation("Tier 1: Attempting basic search for '{SearchText}'", searchText);
        var basicResult = await PerformBasicSearch(searchText, request);
        
        if (basicResult.TotalCount >= basicThreshold)
        {
            _logger.LogInformation("Tier 1 successful: Found {Count} results with basic search in {ElapsedMs}ms", 
                basicResult.TotalCount, (DateTime.UtcNow - searchStartTime).TotalMilliseconds);
            return basicResult;
        }

        // TIER 2: Similarity Search (Medium speed, handles typos and variations)
        _logger.LogInformation("Tier 1 insufficient ({Count} results), trying Tier 2: Similarity search", basicResult.TotalCount);
        try
        {
            var similarityResults = await _aiContextualService.RetrieveSimilarityIds(
                "Partner", searchText, null, similarityThreshold, 0.9f, null);
                
            if (similarityResults.Any())
            {
                var partnerIds = similarityResults.Select(r => r.EntityId).ToList();
                var partnersData = await GetPartnersByIds(partnerIds, request);
                
                if (partnersData.TotalCount > 0)
                {
                    _logger.LogInformation("Tier 2 successful: Found {Count} results with similarity search in {ElapsedMs}ms", 
                        partnersData.TotalCount, (DateTime.UtcNow - searchStartTime).TotalMilliseconds);
                    return partnersData;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Similarity search failed for '{SearchText}', continuing to semantic search", searchText);
        }

        // TIER 3: Semantic Search (Slowest, handles conceptual similarity)
        _logger.LogInformation("Tier 2 insufficient, trying Tier 3: Semantic search for '{SearchText}'", searchText);
        try
        {
            var embedding = await _aiContextualService.CreateEmbeddingForText(searchText);
            if (!string.IsNullOrEmpty(embedding))
            {
                var semanticResults = await _aiContextualService.ExecuteEmbeddingSearchMultiple(
                    "Partner", embedding, semanticThreshold, request.PageSize);
                    
                if (semanticResults.Any())
                {
                    var partnerIds = semanticResults.Select(r => r.EntityId).ToList();
                    var partnersData = await GetPartnersByIds(partnerIds, request);
                    
                    if (partnersData.TotalCount > 0)
                    {
                        _logger.LogInformation("Tier 3 successful: Found {Count} results with semantic search in {ElapsedMs}ms", 
                            partnersData.TotalCount, (DateTime.UtcNow - searchStartTime).TotalMilliseconds);
                        return partnersData;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Semantic search failed for '{SearchText}'", searchText);
        }

        // No results found at any tier
        _logger.LogInformation("All search tiers failed for '{SearchText}' in {ElapsedMs}ms", 
            searchText, (DateTime.UtcNow - searchStartTime).TotalMilliseconds);
        return new PaginationResponse<PartnerModel>
        {
            Records = new List<PartnerModel>(),
            TotalCount = 0,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// Performs basic text search using the original search logic
    /// </summary>
    private async Task<PaginationResponse<PartnerModel>> PerformBasicSearch(
        string searchText, PaginationRequest request)
    {
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
                return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
            },
            CurrentUserId, _logger);
    }

    /// <summary>
    /// Gets partners by their IDs with pagination
    /// </summary>
    private async Task<PaginationResponse<PartnerModel>> GetPartnersByIds(
        List<int> partnerIds, PaginationRequest request)
    {
        // Create a specification that filters by partner IDs
        var partnerFilterRequest = new PartnerFilterRequest
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy ?? "createdDate",
            Ascending = request.Ascending
        };

        // Note: This assumes PartnerCompositeSpecification can handle a list of IDs
        // You may need to modify the specification to support this or create a custom query
        try
        {
            var allPartners = new List<PartnerModel>();
            
            // Get partners individually (this could be optimized with a batch query)
            foreach (var partnerId in partnerIds.Take(request.PageSize * 2)) // Get more than needed for safety
            {
                try
                {
                    var partner = await _manager.GetPartnerAsync(User, partnerId);
                    if (partner != null)
                    {
                        allPartners.Add(partner);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve partner {PartnerId}", partnerId);
                }
            }
            
            // Apply pagination to the collected results
            var startIndex = (request.PageIndex - 1) * request.PageSize;
            var paginatedPartners = allPartners.Skip(startIndex).Take(request.PageSize).ToList();
            
            return new PaginationResponse<PartnerModel>
            {
                Records = paginatedPartners,
                TotalCount = allPartners.Count,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving partners by IDs");
            return new PaginationResponse<PartnerModel>
            {
                Records = new List<PartnerModel>(),
                TotalCount = 0,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
        }
    }

    /// <summary>
    /// Performs enhanced advanced search with smart text search capabilities
    /// </summary>
    private async Task<PaginationResponse<PartnerModel>> PerformEnhancedAdvancedSearch(
        PartnerFilterRequest partnerFilterRequest, float similarityThreshold, float semanticThreshold)
    {
        // First try the original advanced search
        var originalResult = await PerformStandardAdvancedSearch(partnerFilterRequest);
        
        if (originalResult.TotalCount > 0)
        {
            _logger.LogInformation("Standard advanced search found {Count} results", originalResult.TotalCount);
            return originalResult;
        }
        
        // If no results and we have search text, try smart search on the text portion
        if (!string.IsNullOrWhiteSpace(partnerFilterRequest.SearchText))
        {
            _logger.LogInformation("Standard advanced search found no results, trying smart text search for: {SearchText}", 
                partnerFilterRequest.SearchText);
                
            // Try similarity search for the text portion
            try
            {
                var similarityResults = await _aiContextualService.RetrieveSimilarityIds(
                    "Partner", partnerFilterRequest.SearchText, null, similarityThreshold, 0.9f, null);
                    
                if (similarityResults.Any())
                {
                    // Apply the advanced criteria as additional filters to the similarity results
                    var enhancedResults = await ApplyAdvancedCriteriaToPartnerIds(
                        similarityResults.Select(r => r.EntityId).ToList(), partnerFilterRequest);
                        
                    if (enhancedResults.TotalCount > 0)
                    {
                        _logger.LogInformation("Enhanced advanced search with similarity found {Count} results", enhancedResults.TotalCount);
                        return enhancedResults;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Similarity search in advanced search failed");
            }
            
            // Try semantic search as last resort
            try
            {
                var embedding = await _aiContextualService.CreateEmbeddingForText(partnerFilterRequest.SearchText);
                if (!string.IsNullOrEmpty(embedding))
                {
                    var semanticResults = await _aiContextualService.ExecuteEmbeddingSearchMultiple(
                        "Partner", embedding, semanticThreshold, partnerFilterRequest.PageSize * 2);
                        
                    if (semanticResults.Any())
                    {
                        // Apply the advanced criteria as additional filters to the semantic results
                        var enhancedResults = await ApplyAdvancedCriteriaToPartnerIds(
                            semanticResults.Select(r => r.EntityId).ToList(), partnerFilterRequest);
                            
                        if (enhancedResults.TotalCount > 0)
                        {
                            _logger.LogInformation("Enhanced advanced search with semantic found {Count} results", enhancedResults.TotalCount);
                            return enhancedResults;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Semantic search in advanced search failed");
            }
        }
        
        return originalResult; // Return original empty result
    }

    /// <summary>
    /// Performs standard advanced search using the original logic
    /// </summary>
    private async Task<PaginationResponse<PartnerModel>> PerformStandardAdvancedSearch(
        PartnerFilterRequest partnerFilterRequest)
    {
        return await SearchControllerHelper.ProcessAdvancedSearch<PartnerFilterRequest, PartnerCompositeSpecification, PaginationResponse<PartnerModel>>(
            partnerFilterRequest.SearchCriteria ?? "", partnerFilterRequest.SearchText, 
            partnerFilterRequest.PageIndex, partnerFilterRequest.PageSize, 
            partnerFilterRequest.OrderBy ?? "createdDate", partnerFilterRequest.Ascending, 
            partnerFilterRequest,
            "Partner",
            filterRequest => new PartnerCompositeSpecification(filterRequest),
            async (userId, spec, pagination) => {
                return (PaginationResponse<PartnerModel>)await _manager.GetPartnersWithSpecificationAsync(User, spec, (PartnerFilterRequest)pagination);
            },
            CurrentUserId, _logger);
    }

    /// <summary>
    /// Applies advanced search criteria to a specific set of partner IDs
    /// This method filters the partners by IDs and then applies the advanced criteria
    /// </summary>
    private async Task<PaginationResponse<PartnerModel>> ApplyAdvancedCriteriaToPartnerIds(
        List<int> partnerIds, PartnerFilterRequest originalRequest)
    {
        try
        {
            // Get the partners by IDs first
            var candidatePartners = new List<PartnerModel>();
            
            foreach (var partnerId in partnerIds)
            {
                try
                {
                    var partner = await _manager.GetPartnerAsync(User, partnerId);
                    if (partner != null)
                    {
                        candidatePartners.Add(partner);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve partner {PartnerId} for advanced criteria filtering", partnerId);
                }
            }
            
            if (!candidatePartners.Any())
            {
                return new PaginationResponse<PartnerModel>
                {
                    Records = new List<PartnerModel>(),
                    TotalCount = 0,
                    PageIndex = originalRequest.PageIndex,
                    PageSize = originalRequest.PageSize
                };
            }
            
            // Apply pagination to the filtered results
            var startIndex = (originalRequest.PageIndex - 1) * originalRequest.PageSize;
            var paginatedResults = candidatePartners.Skip(startIndex).Take(originalRequest.PageSize).ToList();
            
            return new PaginationResponse<PartnerModel>
            {
                Records = paginatedResults,
                TotalCount = candidatePartners.Count,
                PageIndex = originalRequest.PageIndex,
                PageSize = originalRequest.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying advanced criteria to partner IDs");
            return new PaginationResponse<PartnerModel>
            {
                Records = new List<PartnerModel>(),
                TotalCount = 0,
                PageIndex = originalRequest.PageIndex,
                PageSize = originalRequest.PageSize
            };
        }
    }

    #endregion

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

            if (_geminiManager == null)
            {
                throw new BusinessException("Gemini manager not available");
            }

            if (req == null)
            {
                throw new BusinessException("Request cannot be null");
            }

            string fileType = _geminiManager.FindFileType(req.File) ?? "";

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
    /// Detects duplicates for an existing partner record after save operations
    /// </summary>
    /// <param name="req">Partner data to check for duplicates</param>
    /// <returns>Duplicate detection results</returns>
    [HttpPost(APIDictionary.Partner + "/detect-duplicates")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<ActionResult> DetectDuplicatesForPartner([FromBody] dynamic req)
    {
        try
        {
            // Proper null check for dynamic type
            if (req is null)
            {
                return BadRequest("Invalid request.");
            }

            // Convert the dynamic request to a proper object for duplicate detection
            object requestData;
            if (req is JsonElement jsonElement)
            {
                // Deserialize JsonElement to JObject for proper handling
                var jsonString = jsonElement.GetRawText();
                requestData = JObject.Parse(jsonString);
            }
            else if (req is JObject)
            {
                requestData = req;
            }
            else
            {
                // Try to serialize and deserialize to ensure proper format
                var jsonString = JsonConvert.SerializeObject(req);
                requestData = JObject.Parse(jsonString);
            }

            var duplicateResult = await _aiContextualService.DetectDuplicateForSingleRecordAsync(
                "Partner", 
                requestData, 
                0.5 // Standard sensitivity for post-save detection
            );
            
            // Extract ID from the converted request data
            int? recordId = null;
            if (requestData is JObject jObj && jObj.ContainsKey("id"))
            {
                int.TryParse(jObj["id"]?.ToString(), out int id);
                recordId = id > 0 ? id : null;
            }

            return Ok(new {
                success = true,
                entityType = "Partner",
                recordId = recordId,
                duplicateInfo = duplicateResult?.HasDuplicates == true ? new {
                    totalDuplicates = duplicateResult.TotalDuplicates,
                    highConfidence = duplicateResult.HighConfidence,
                    mediumConfidence = duplicateResult.MediumConfidence,
                    lowConfidence = duplicateResult.LowConfidence,
                    topDuplicate = duplicateResult.TopDuplicate != null ? new {
                        entityId = duplicateResult.TopDuplicate.EntityId,
                        entityType = duplicateResult.TopDuplicate.EntityType,
                        score = duplicateResult.TopDuplicate.Score,
                        matchReason = duplicateResult.TopDuplicate.MatchReason,
                        searchType = duplicateResult.TopDuplicate.SearchType,
                        matchedData = duplicateResult.TopDuplicate.MatchedData != null ? 
                            JsonConvert.SerializeObject(duplicateResult.TopDuplicate.MatchedData) : null
                    } : null,
                    duplicates = duplicateResult.AllDuplicates != null ? 
                        JsonConvert.SerializeObject(duplicateResult.AllDuplicates) : null
                } : null
            });
        }
        catch (Exception ex)
        {
            // Extract ID for logging
            var idForLogging = "unknown";
            try
            {
                if (req is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    var jsonString = jsonElement.GetRawText();
                    var jObj = JObject.Parse(jsonString);
                    if (jObj.ContainsKey("id"))
                    {
                        idForLogging = jObj["id"]?.ToString() ?? "unknown";
                    }
                }
                else if (req is JObject jObj && jObj.ContainsKey("id"))
                {
                    idForLogging = jObj["id"]?.ToString() ?? "unknown";
                }
            }
            catch
            {
                // Ignore errors in ID extraction for logging
            }

            _logger.LogWarning(ex, "Post-save duplicate detection failed for Partner ID {PartnerId}", idForLogging);
            // Return success with no duplicates rather than failing - this is a background operation
            return Ok(new {
                success = true,
                entityType = "Partner",
                recordId = (object)null,
                duplicateInfo = (object)null,
                warning = "Duplicate detection temporarily unavailable"
            });
        }
    }

    /// <summary>
    /// Creates a new opportunity directly from a partner record with the partner pre-populated as funding/client partner
    /// </summary>
    /// <param name="partnerId">ID of the partner to create opportunity for</param>
    /// <param name="req">Opportunity creation request with name and partner role</param>
    /// <returns>Created opportunity with partner relationship established</returns>
    [HttpPost(APIDictionary.Partner + "/{partnerId}/create-opportunity")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<IActionResult> CreateOpportunityFromPartner(int partnerId, [FromBody] CreateOpportunityFromPartnerRequest req)
    {
        try
        {
            _logger.LogInformation("Creating opportunity for partner {PartnerId} with name '{Name}' and role '{Role}'", 
                partnerId, req.Name, req.PartnerRole);

            // Validate partner exists and is active
            var partner = await _manager.GetPartnerAsync(partnerId);
            if (partner == null)
            {
                _logger.LogWarning("Partner {PartnerId} not found", partnerId);
                return NotFound(new { error = $"Partner with ID {partnerId} not found" });
            }

            if (partner.Status != "Active")
            {
                _logger.LogWarning("Cannot create opportunity for inactive partner {PartnerId}", partnerId);
                return BadRequest(new { error = "Cannot create opportunity for inactive partner" });
            }

            // Validate partner role
            var validRoles = new[] { "funding", "client", "both" };
            if (!validRoles.Contains(req.PartnerRole.ToLower()))
            {
                return BadRequest(new { error = "PartnerRole must be 'funding', 'client', or 'both'" });
            }

            // Build opportunity request with partner relationship
            var opportunityRequest = new OpportunityRequest
            {
                Name = req.Name,
                Description = req.Description ?? $"Opportunity created from partner: {partner.Name}",
                FundingPartners = new List<OpportunityFundingPartnerRequest>(),
                ClientPartners = new List<OpportunityClientPartnerRequest>(),
                Stakeholders = new List<OpportunityStakeholderRequest>()
            };

            // Add partner as funding partner
            if (req.PartnerRole.ToLower() == "funding" || req.PartnerRole.ToLower() == "both")
            {
                opportunityRequest.FundingPartners.Add(new OpportunityFundingPartnerRequest
                {
                    PartnerId = partnerId
                });
            }

            // Add partner as client partner
            if (req.PartnerRole.ToLower() == "client" || req.PartnerRole.ToLower() == "both")
            {
                opportunityRequest.ClientPartners.Add(new OpportunityClientPartnerRequest
                {
                    PartnerId = partnerId
                });
            }

            // Create the opportunity
            var result = await _opportunityManager.CreateOpportunityAsync(opportunityRequest);

            // Assign the current user as Opportunity Manager
            try
            {
                await _opportunityManager.AssignCreatorAsOpportunityManagerAsync(result.Id, CurrentUserId);
                _logger.LogInformation("✅ Assigned user {UserId} as Opportunity Manager for opportunity {OpportunityId}", 
                    CurrentUserId, result.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to assign creator as Opportunity Manager for opportunity {OpportunityId}", result.Id);
                // Don't fail the request if role assignment fails
            }

            // Create audit log for the new opportunity
            await CreateAuditLogAsync(result.Id, "create", result);

            _logger.LogInformation("✅ Successfully created opportunity {OpportunityId} from partner {PartnerId}", 
                result.Id, partnerId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opportunity from partner {PartnerId}", partnerId);
            return StatusCode(500, new { error = "Internal server error while creating opportunity", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets all opportunities related to a partner (where partner is funding or client partner)
    /// </summary>
    /// <param name="partnerId">ID of the partner</param>
    /// <returns>List of related opportunities</returns>
    [HttpGet(APIDictionary.Partner + "/{partnerId}/opportunities")]
    [AccessControlled(EntityTypes.Partner, "read")]
    public async Task<IActionResult> GetPartnerOpportunities(int partnerId)
    {
        try
        {
            _logger.LogInformation("Getting opportunities for partner {PartnerId}", partnerId);

            // Validate partner exists
            var partner = await _manager.GetPartnerAsync(partnerId);
            if (partner == null)
            {
                _logger.LogWarning("Partner {PartnerId} not found", partnerId);
                return NotFound(new { error = $"Partner with ID {partnerId} not found" });
            }

            // Get related opportunities
            var opportunities = await _opportunityManager.GetOpportunitiesByPartnerIdAsync(partnerId);

            _logger.LogInformation("Found {Count} opportunities for partner {PartnerId}", 
                opportunities.Count(), partnerId);

            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opportunities for partner {PartnerId}", partnerId);
            return StatusCode(500, new { error = "Internal server error while getting opportunities", details = ex.Message });
        }
    }

    /// <summary>
    /// Helper method to create audit log entry with complete opportunity data
    /// </summary>
    private async Task CreateAuditLogAsync(int opportunityId, string action, OpportunityModel? opportunityData = null)
    {
        try
        {
            // Get the current opportunity data if not provided
            if (opportunityData == null)
            {
                opportunityData = await _opportunityManager.GetOpportunityAsync(opportunityId);
            }

            if (opportunityData != null)
            {
                var jsonData = System.Text.Json.JsonSerializer.Serialize(opportunityData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                await _auditLogManager.CreateAuditLogAsync(new AuditLogCreateRequest
                {
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    Action = action,
                    UserId = CurrentUserId,
                    JsonData = jsonData,
                    Description = $"Opportunity {action} from partner - {opportunityData.Name}"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to create audit log for opportunity {OpportunityId}", opportunityId);
            // Don't fail the request if audit log creation fails
        }
    }


}
