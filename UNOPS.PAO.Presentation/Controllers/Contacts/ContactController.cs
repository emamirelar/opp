using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Net;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Specifications;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDomain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static UNOPS.PAO.UNOPSBusiness.Services.AdvancedSearchService;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Contacts;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Security;
using System.Text.Json.Nodes;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Presentation.Controllers.Shared;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class ContactController : BaseController
{
    private readonly IContactManager _manager;
    private readonly ISecureSpecificationFactory _secureSpecificationFactory;
    private readonly IGeminiManager _geminiManager;
    private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;
    private readonly AiContextualService _aiContextualService;
    private readonly AdvancedSearchService _advancedSearchService;

    public ContactController(
        IManagerWrapper manager, 
        ISecureSpecificationFactory secureSpecificationFactory,
        UserResolverService<int> userResolverService, 
        ILogger<ContactController> logger,
        IAuthorizationService authorizationService,
        AiContextualService aiContextualService,
        AdvancedSearchService advancedSearchService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.ContactManager;
        _secureSpecificationFactory = secureSpecificationFactory;
        _geminiManager = manager.GeminiManager;
        _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
        _aiContextualService = aiContextualService;
        _advancedSearchService = advancedSearchService;
    }

    /// <summary>
    /// Creates a new contact with comprehensive personal and professional details.
    /// </summary>
    /// <param name="req">Contact creation request with required fields including firstName, lastName, email, partnerId, title, salutation, middleName, suffix, department, phone, mobile, status, mailingStreet, mailingCity, mailingCountry</param>
    /// <example_uses>
    /// Create a contact named John Doe with email john@unicef.org
    /// Add a new program manager contact for partner 123
    /// Register Dr. Jane Smith as the technical lead
    /// Create contact with full address information
    /// Add executive contact to organization
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to add, create, register, or set up a new contact.</when_to_use>
    /// <returns>Created contact with ID and metadata</returns>
    [HttpPost(APIDictionary.Contact)]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> Create([FromBody] ContactRequest req)
    {
        // Validate mandatory fields for contact creation
        var validationErrors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(req.LastName))
        {
            validationErrors.Add("LastName is required for contact creation");
        }
        
        if (string.IsNullOrWhiteSpace(req.Title))
        {
            validationErrors.Add("Title is required for contact creation");
        }
        
        if (string.IsNullOrWhiteSpace(req.Email))
        {
            validationErrors.Add("Email is required for contact creation");
        }
        else if (!IsValidEmail(req.Email))
        {
            validationErrors.Add("Email format is invalid");
        }
        
        if (req.PartnerId <= 0)
        {
            validationErrors.Add("PartnerId is required and must be a valid partner ID");
        }
        
        // Return validation errors if any
        if (validationErrors.Any())
        {
            var errorMessage = $"Validation failed for contact creation: {string.Join("; ", validationErrors)}";
            _logger.LogWarning("Contact creation validation failed: {Errors}", errorMessage);
            return BadRequest(new { 
                success = false,
                error = errorMessage,
                validationErrors = validationErrors,
                requiredFields = new[] { "LastName", "Title", "Email", "PartnerId" },
                optionalButRecommended = new[] { "FirstName", "MiddleName", "Phone", "Mobile", "Department", "MailingCity", "MailingCountry" },
                hint = "Ensure LastName, Title, Email, and PartnerId are provided. PartnerId must be a valid partner ID from the system - search for partners first if needed."
            });
        }
        
        // Check for duplicates ONLY if user hasn't confirmed duplicate creation
        if (!req.ConfirmDuplicateCreation)
        {
            try
            {
                var duplicateResult = await _aiContextualService.DetectDuplicateForSingleRecordAsync(
                    "Contact", 
                    req,
                    fieldMatchThreshold: 0.5      // Standard sensitivity for field-based detection
                );
                
                if (duplicateResult.HasDuplicates)
                {
                    // Return duplicate confirmation response
                    return Ok(new {
                        success = false,
                        action = "duplicateConfirmation",
                        message = "Potential duplicate contact(s) found. Do you want to create anyway?",
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
                        originalData = req  // Return original data for re-submission with confirmation
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't block creation due to duplicate detection failure
                _logger.LogWarning($"Duplicate detection failed for contact creation: {ex.Message}");
                // Continue with creation since duplicate detection is not critical
            }
        }
        
        // Create the contact (either no duplicates found, or user confirmed creation)
        var result = await _manager.CreateContactAsync(req);
        if (result == null)
        {
            throw new BusinessException("Failed to create contact");
        }
        
        return StatusCode(201, new {
            success = true,
            action = "created",
            message = req.ConfirmDuplicateCreation ? 
                "Contact created successfully (duplicate confirmation acknowledged)" : 
                "Contact created successfully",
            data = result
        });
    }

    /// <summary>
    /// Retrieves all contacts with basic pagination and ordering, optionally filtered by partner.
    /// </summary>
    /// <param name="pageIndex">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order results by (optional)</param>
    /// <param name="ascending">Sort direction - true for ascending, false for descending (default: true)</param>
    /// <param name="partnerId">Optional partner ID to filter contacts by specific partner</param>
    /// <param name="export">Whether to export results as file instead of returning JSON (default: false)</param>
    /// <param name="filterActive">Whether to apply global filters, default: true</param>
    /// <example_uses>
    /// Show me all contacts
    /// List all contacts in the system
    /// Display the contact directory
    /// Get all contact records
    /// Browse contacts
    /// Show contacts for partner ID 123
    /// List all contacts from UNICEF partner
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see ALL contacts without search criteria, or when asking for contacts filtered by a specific partner.</when_to_use>
    /// <returns>Paginated list of all contacts, optionally filtered by partner</returns>
    [HttpGet(APIDictionary.Contact)]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> ListAllContacts(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "FirstName",
        [FromQuery] bool ascending = true,
        [FromQuery] int? partnerId = null,
        [FromQuery] bool export = false,
        [FromQuery] bool filterActive = true)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(pageIndex, pageSize);
        if (validationResult != null) return validationResult;
        
        return await HandleSearchOperationAsync(async () =>
        {
            // Create a basic ContactFilterRequest with pagination, ordering, and optional partner filter
            var request = new ContactFilterRequest
            {
                PageIndex = pageIndex,
                PageSize = export ? int.MaxValue : pageSize, // Remove pagination limits for export
                OrderBy = orderBy ?? "FirstName",
                Ascending = ascending,
                PartnerId = partnerId,
                FilterActive = filterActive
            };
            
            _logger.LogInformation($"[CONTROLLER DEBUG] ContactFilterRequest - PartnerId: {request.PartnerId}");
            
            // Create simple specification - global filters will be applied by the manager
            var specification = new ContactCompositeSpecification(request);
            
            var result = await _manager.GetContactsWithSpecificationAsync(User, specification, request);
            return (PaginationResponse<ContactModel>)result;
        }, "contact list all");
    }

    /// <summary>
    /// Performs simple text search across multiple contact fields (name, email, title, etc.).
    /// </summary>
    /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
    /// <param name="query">Text to search across contact name, email, title, and other basic fields</param>
    /// <param name="partnerId">Optional partner ID to filter contacts by partner</param>
    /// <param name="export">Whether to export all results without pagination</param>
    /// <param name="filterActive">Whether to apply global filters, default: true</param>
    /// <example_uses>
    /// Search for contacts named John
    /// Find contacts with @unicef.org email
    /// Search for contacts containing 'Smith'
    /// Find contact with phone number 555-1234
    /// Look for contacts with title 'Manager'
    /// </example_uses>
    /// <when_to_use>Use this for simple name, email, title, or basic field searches. NOT for partner relationship searches.</when_to_use>
    /// <returns>Paginated list of contacts matching the search text</returns>
    [HttpGet(APIDictionary.Contact + "/search")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> SearchContacts(
        [FromQuery] PaginationRequest request,
        [FromQuery] string query,
        [FromQuery] int? partnerId = null,
        [FromQuery] bool export = false,
        [FromQuery] bool filterActive = true)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new BusinessException("Search text is required for contact search");
        }

        // Use the enhanced search pattern (now includes PostgreSQL similarity search)
        var paginationRequest = new PaginationRequest
        {
            PageIndex = request.PageIndex,
            PageSize = export ? int.MaxValue : request.PageSize, // Remove pagination limits for export
            OrderBy = request.OrderBy ?? "FirstName",
            Ascending = request.Ascending ?? true,
            FilterActive = filterActive
        };

        // Apply base entity filtering first if partnerId is provided
        if (partnerId.HasValue)
        {
            // Add partnerId as a filter to the search request
            var baseFilters = new List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter>
            {
                new UNOPS.PAO.UNOPSBusiness.Services.SearchFilter
                {
                    field = "PartnerId",
                    @operator = "eq",
                    value = partnerId.Value.ToString(),
                    logicalOperator = "AND",
                    fieldType = "int"
                }
            };

            // Create unified search request with query and base entity filters
            var searchRequest = new UNOPS.PAO.UNOPSBusiness.Services.UnifiedSearchRequest
            {
                Query = query,
                Filters = baseFilters,
                PageIndex = paginationRequest.PageIndex,
                PageSize = paginationRequest.PageSize,
                OrderBy = paginationRequest.OrderBy,
                Ascending = paginationRequest.Ascending ?? true,
                FilterActive = paginationRequest.FilterActive
            };

            // Use AdvancedSearchService for unified text search with entity pre-filtering
            var searchResult = await _advancedSearchService.SearchAsync<UNOPSContact, ContactModel>(
                searchRequest,
                User);

            return Ok(searchResult);
        }

        // Use AdvancedSearchService for unified text search with PostgreSQL similarity and metadata
        var result = await _advancedSearchService.SearchWithQueryAndMetadataAsync<UNOPSContact, ContactModel>(
            query, 
            paginationRequest, 
            User);

        _logger.LogInformation("Contact search completed: Found {TotalCount} results for query: {Query}, export: {Export}", result.TotalCount, query, export);

        return Ok(result);
    }

    /// <summary>
    /// Performs advanced search with structured criteria including relationships with partners, departments, and complex filters.
    /// Enhanced with intelligent field value matching for AI agents and typo correction.
    /// </summary>
    /// <param name="filters">JSON array of search criteria objects with field, operator, value, and logicalOperator</param>
    /// <param name="pageIndex">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order by (optional)</param>
    /// <param name="ascending">Sort direction (default: true)</param>
    /// <param name="partnerId">Optional partner ID to filter contacts by partner</param>
    /// <param name="export">Whether to export all results without pagination</param>
    /// <param name="filterActive">Whether to apply global filters, default: true</param>
    /// <example_uses>
    /// Find contacts from UNICEF partner organization
    /// Show contacts in Finance department created this month
    /// Get contacts working at Asian Infrastructure partners
    /// Find contacts where partner status is Active
    /// List contacts from climate-related interactions
    /// Search for contacts by department and creation date
    /// </example_uses>
    /// <when_to_use>Use this for searches involving partner relationships, departments, dates, status, or any complex multi-field criteria.</when_to_use>
    /// <searchCriteria_format>
    /// JSON array format: [{"field": "partner.name", "operator": "like", "value": "UNICEF", "logicalOperator": "AND"}]
    /// Available operators: is, is not, like, not like, greater than, less than, greater than or equal, less than or equal, this week, this month, this year
    /// Available fields: firstName, lastName, email, title, department, partner.name, partner.status, createdDate, modifiedDate
    /// Logical operators: AND, OR
    /// </searchCriteria_format>
    /// <returns>Paginated list of contacts matching the advanced search criteria</returns>
    [HttpGet(APIDictionary.Contact + "/advanced-search")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> AdvancedSearchContacts(
        [FromQuery] string filters,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "FirstName",
        [FromQuery] bool ascending = true,
        [FromQuery] int? partnerId = null,
        [FromQuery] bool export = false,
        [FromQuery] bool filterActive = true)
    {
        try
        {
            _logger.LogInformation("=== CONTACT ADVANCED SEARCH ENDPOINT ===");
            _logger.LogInformation("Filters: {Filters}, Page: {PageIndex}, Size: {PageSize}", filters, pageIndex, pageSize);

            if (string.IsNullOrWhiteSpace(filters))
            {
                return BadRequest(new { error = "Search filters are required" });
            }

            // Parse filters from JSON
            List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter> searchFilters;
            try
            {
                searchFilters = System.Text.Json.JsonSerializer.Deserialize<List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter>>(filters) ?? new List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter>();
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse search filters: {Filters}", filters);
                return BadRequest(new { error = "Invalid filter format. Expected JSON array of filter objects." });
            }

            // Apply base entity filtering first if partnerId is provided
            if (partnerId.HasValue)
            {
                // Add the entity filter to the existing search filters
                searchFilters.Add(new UNOPS.PAO.UNOPSBusiness.Services.SearchFilter
                {
                    field = "PartnerId",
                    @operator = "eq",
                    value = partnerId.Value.ToString(),
                    logicalOperator = "AND",
                    fieldType = "int"
                });
            }

            // Use AdvancedSearchService for structured filters with PostgreSQL similarity on "like" operators
            var paginationRequest = new PaginationRequest
            {
                PageIndex = pageIndex,
                PageSize = export ? int.MaxValue : pageSize, // Remove pagination limits for export
                OrderBy = orderBy ?? "FirstName",
                Ascending = ascending,
                FilterActive = filterActive
            };

            var result = await _advancedSearchService.SearchWithFiltersAsync<UNOPSContact, ContactModel>(
                searchFilters,
                paginationRequest,
                User);
            
            _logger.LogInformation("Advanced contact search completed: Found {TotalCount} results", result.TotalCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced contact search");
            return StatusCode(500, new { error = "Internal server error during contact search", details = ex.Message });
        }
    }

    /// <summary>
    /// Get supported search fields for contacts - helps frontend build dynamic search forms
    /// </summary>
    /// <returns>List of all supported search fields with their metadata</returns>
    [HttpGet(APIDictionary.Contact + "/search-fields")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public ActionResult<List<SearchFieldInfo>> GetContactSearchFields()
    {
        try
        {
            var fields = _manager.GetContactSearchFields();
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact search fields");
            return StatusCode(500, new { error = "An error occurred while retrieving search fields" });
        }
    }

    /// <summary>
    /// Retrieves a specific contact by ID with complete details including partner information and all contact methods.
    /// </summary>
    /// <param name="id">Contact ID</param>
    /// <example_uses>
    /// Show me details for contact ID 123
    /// Get full information about contact 456
    /// Display contact record 789
    /// Get complete contact profile
    /// Show contact with partner information
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for specific contact details by ID or when you need complete contact information.</when_to_use>
    /// <returns>Complete contact details with related information</returns>
    [HttpGet(APIDictionary.Contact + "/{id}")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> Get(int id)
    {
        // Use the new secure method that checks entity-level access
        var contact = await _manager.GetContactAsync(User, id);
        if (contact == null)
        {
            return NotFound();
        }
        return Ok(contact);
    }

    /// <summary>
    /// Updates an existing contact's information including personal details, contact methods, and professional information.
    /// </summary>
    /// <param name="req">Contact update request containing modified fields including id, firstName, lastName, email, title, phone, mobile, department, status</param>
    /// <example_uses>
    /// Update contact 123's email to newemail@unicef.org
    /// Change contact 456's title to Senior Manager
    /// Update phone number for contact 789
    /// Modify contact's department information
    /// Change contact status to Inactive
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to update, modify, edit, or change contact information.</when_to_use>
    /// <returns>Updated contact data</returns>
    [HttpPut(APIDictionary.Contact)]
    [AccessControlled(EntityTypes.Contact, "update")]
    public async Task<ActionResult> Update([FromBody] UpdateContactRequest req)
    {
        // Use the new secure method that checks entity-level permissions
        var result = await _manager.UpdateContactAsync(User, req);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a contact from the system (marks as deleted rather than permanent removal).
    /// </summary>
    /// <param name="id">Contact ID to delete</param>
    /// <example_uses>
    /// Delete contact ID 123
    /// Remove contact 456 from the system
    /// Deactivate contact record
    /// Soft delete contact John Doe
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to delete, remove, or eliminate a contact.</when_to_use>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete(APIDictionary.Contact + "/{id}")]
    [AccessControlled(EntityTypes.Contact, "delete")]
    public async Task<ActionResult> Delete(int id)
    {
        // Use the new secure method that checks entity-level permissions
        await _manager.DeleteContactAsync(User, id);
        return NoContent();
    }


    /// <summary>
    /// Retrieves all contacts associated with a specific partner organization with access control.
    /// </summary>
    /// <param name="partnerId">Partner organization ID to get contacts for</param>
    /// <example_uses>
    /// Show all contacts for partner organization 123
    /// Get contact list for UNICEF partner
    /// Find all people working at partner ID 456
    /// List contacts belonging to a specific organization
    /// Get partner's contact directory
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see contacts for a specific partner, organization, or wants to view who works at a particular partner.</when_to_use>
    /// <returns>List of contacts belonging to the specified partner organization</returns>
    [HttpGet(APIDictionary.PartnerContacts)]
    [AccessControlled(EntityTypes.Contact, "read")]
    public Task<ActionResult> PartnerContacts(int partnerId)
    {
        return Task.FromResult<ActionResult>(Ok(_manager.GetPartnerContacts(partnerId)));
    }

    /// <summary>
    /// Retrieves the current user's permissions for a specific contact (read, update, delete).
    /// </summary>
    /// <param name="id">Contact ID to check permissions for</param>
    /// <example_uses>
    /// Check my permissions for contact 123
    /// What can I do with contact 456?
    /// Get access rights for this contact
    /// Verify contact permissions before editing
    /// Can I update this contact's information?
    /// </example_uses>
    /// <when_to_use>Use this when you need to check user permissions before performing operations or showing UI elements for contact management.</when_to_use>
    /// <returns>Permission object with CanRead, CanUpdate, CanDelete flags</returns>
    [HttpGet(APIDictionary.Contact + "/{id}/permissions")]
    public async Task<ActionResult> PermissionsGet(int id)
    {
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound(new { error = $"Contact with ID {id} not found" });
        }
        
        // Return permissions for this contact
        var permissions = await GetEntityPermissionsAsync("Contact", contact);
        
        return Ok(permissions);
    }

    /// <summary>
    /// Uploads and associates a profile picture with a contact (max 1MB, JPEG/PNG/WEBP only).
    /// </summary>
    /// <param name="id">Contact ID to upload profile picture for</param>
    /// <param name="file">Image file (max 1MB, JPEG/PNG/WEBP formats only)</param>
    /// <example_uses>
    /// Upload a profile picture for contact 123
    /// Add photo to contact John Doe
    /// Set contact profile image
    /// Update contact's profile picture
    /// Add headshot to contact record
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to add or update a contact's profile picture or photo.</when_to_use>
    /// <returns>Success confirmation with image URL or error details</returns>
    [HttpPost(APIDictionary.Contact + "/{id}/profile-picture")]
    [AccessControlled(EntityTypes.Contact, "update")]
    public async Task<ActionResult> UploadProfilePicture(int id, IFormFile file)
    {
        
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("No file was uploaded");
        }

        // Check file size (1MB max)
        if (file.Length > 1024 * 1024)
        {
            throw new BusinessException("File size exceeds maximum limit of 1MB");
        }

        // Validate file type
        var validImageTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!validImageTypes.Contains(file.ContentType))
        {
            throw new BusinessException("Invalid file type. Only JPEG, PNG, and WEBP files are allowed.");
        }

        var result = await _manager.UpdateContactProfilePictureAsync(id, file);
        return Ok(new { imageUrl = result });
    }

    #region AI-Powered Contact Data Processing

    /// <summary>
    /// Scans and processes uploaded files for contact data extraction using AI-powered analysis.
    /// </summary>
    /// <param name="req">File scan request containing the file to be processed</param>
    /// <example_uses>
    /// Scan business cards for contact information
    /// Upload contact forms for processing
    /// Analyze contact documents with AI
    /// Extract data from contact lists
    /// Process contact information from uploaded files
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to upload and scan documents for contact data extraction using AI.</when_to_use>
    /// <returns>Extracted contact data from the scanned file</returns>
    [HttpPost(APIDictionary.Contact + "/scan-data")]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> ScanContactData([FromForm] GeminiFileRequest req) 
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
                throw new BusinessException("Prompt configuration for contact data scanning is not found.");
            }

            return response.Trim();
        });
    }

    /// <summary>
    /// Analyzes uploaded files and extracts structured contact data using AI-powered data analysis.
    /// </summary>
    /// <param name="request">Analysis request containing file and analysis parameters</param>
    /// <example_uses>
    /// Analyze contact directories for structured data extraction
    /// Extract contact information from uploaded forms
    /// Process contact documents with AI
    /// Convert contact files into structured database entries
    /// Analyze business card data for key information
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to analyze files and extract structured contact data for database import.</when_to_use>
    /// <returns>Structured contact data extracted from the analyzed file</returns>
    [HttpPost(APIDictionary.Contact + "/analyse-file")]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> AnalyseContactData([FromBody] AnalyseFileRequest request)
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
    /// Bulk uploads multiple contact records using AI-assisted data processing and validation.
    /// </summary>
    /// <param name="req">Bulk upload request containing contact data</param>
    /// <example_uses>
    /// Bulk upload 500 contacts from Excel
    /// Import multiple contacts from CSV file
    /// Mass upload contact data with AI validation
    /// Bulk import contact records with duplicate detection
    /// Upload large contact dataset with automated processing
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to upload multiple contact records at once with AI-assisted processing.</when_to_use>
    /// <returns>Bulk upload results with success/failure status for each contact</returns>
    [HttpPost(APIDictionary.Contact + "/bulk-upload")]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> BulkUploadContacts([FromBody] BulkUploadRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.Type))
            {
                throw new BusinessException("Invalid request.");
            }

            // Ensure the request is for contact entities
            if (!req.Type.Equals("Contact", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("This endpoint only supports Contact bulk uploads.");
            }

            string response = await _geminiManager.BulkInsertRecordsAsync(req);
            return new { message = response };
        });
    }

    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// Validates email format using a simple regex pattern
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid, false otherwise</returns>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        try
        {
            // Use .NET's built-in email validation
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion

    /// <summary>
    /// Describes the Contact entity structure including all field configurations
    /// </summary>
    /// <returns>Entity and field metadata for Contact</returns>
    [HttpGet(APIDictionary.Contact + "/metadata-info")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> GetMetadataInfo()
    {
        try
        {
            var entityDetails = await _entityConfigurationManager.GetEntityConfigurationDetailsAsync(User, "Contact");
            return Ok(entityDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Contact entity description");
            return StatusCode(500, new { error = "Failed to retrieve Contact entity description" });
        }
    }

    /// <summary>
    /// Detects duplicates for an existing contact record after save operations
    /// </summary>
    /// <param name="req">Contact data to check for duplicates</param>
    /// <returns>Duplicate detection results</returns>
    [HttpPost(APIDictionary.Contact + "/detect-duplicates")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> DetectDuplicatesForContact([FromBody] dynamic req)
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
                "Contact", 
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
                entityType = "Contact",
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

            _logger.LogWarning(ex, "Post-save duplicate detection failed for Contact ID {ContactId}", idForLogging);
            // Return success with no duplicates rather than failing - this is a background operation
            return Ok(new {
                success = true,
                entityType = "Contact",
                recordId = (object?)null,
                duplicateInfo = (object?)null,
                warning = "Duplicate detection temporarily unavailable"
            });
        }
    }

}
