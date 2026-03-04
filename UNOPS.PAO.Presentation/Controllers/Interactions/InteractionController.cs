using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDomain.Entities;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Newtonsoft.Json;
using static UNOPS.PAO.UNOPSBusiness.Services.AdvancedSearchService;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Interactions
{
    [Route("/")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class InteractionController : BaseController
    {
        private readonly IInteractionManager _manager;
        private readonly IContactManager _contactManager;
        private readonly ISecureSpecificationFactory _secureSpecificationFactory;
        private readonly IGeminiManager _geminiManager;
        private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;
        private readonly AiContextualService _aiContextualService;
        private readonly AdvancedSearchService _advancedSearchService;

        public InteractionController(
            IManagerWrapper manager, 
            UserResolverService<int> userResolverService,
            IAuthorizationService authorizationService,
            ISecureSpecificationFactory secureSpecificationFactory,
            ILogger<InteractionController> logger,
            AiContextualService aiContextualService,
            AdvancedSearchService advancedSearchService)
            : base(logger, authorizationService, userResolverService)
        {
            _manager = manager.InteractionManager;
            _contactManager = manager.ContactManager;
            _secureSpecificationFactory = secureSpecificationFactory;
            _geminiManager = manager.GeminiManager;
            _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
            _aiContextualService = aiContextualService;
            _advancedSearchService = advancedSearchService;
        }
        
        /// <summary>
        /// Auto-populates EmailAddresses and PartnerIds from ContactIds if they are not provided.
        /// This ensures interactions always have the relevant partner and email information based on selected contacts.
        /// </summary>
        private async Task AutoPopulateFromContactsAsync(InteractionRequest req)
        {
            if (req.ContactIds == null || !req.ContactIds.Any())
            {
                return; // No contacts to populate from
            }
            
            var contactEmails = new List<string>();
            var contactPartnerIds = new List<int>();
            
            foreach (var contactId in req.ContactIds)
            {
                try
                {
                    var contact = await _contactManager.GetContactAsync(contactId);
                    if (contact != null)
                    {
                        // Collect email if available
                        if (!string.IsNullOrWhiteSpace(contact.Email))
                        {
                            contactEmails.Add(contact.Email);
                        }
                        
                        // Collect partnerId if available (Partner is a navigation property)
                        if (contact.Partner?.Id > 0)
                        {
                            contactPartnerIds.Add(contact.Partner.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve contact {ContactId} for auto-population", contactId);
                }
            }
            
            // Auto-populate EmailAddresses if empty
            if ((req.EmailAddresses == null || !req.EmailAddresses.Any()) && contactEmails.Any())
            {
                req.EmailAddresses = contactEmails.Distinct().ToList();
                _logger.LogInformation("Auto-populated {Count} email addresses from contacts", req.EmailAddresses.Count);
            }
            
            // Auto-populate PartnerIds if empty
            if ((req.PartnerIds == null || !req.PartnerIds.Any()) && contactPartnerIds.Any())
            {
                req.PartnerIds = contactPartnerIds.Distinct().ToList();
                _logger.LogInformation("Auto-populated {Count} partner IDs from contacts", req.PartnerIds.Count);
            }
        }
        
        /// <summary>
        /// Normalizes DateTime values in the request to UTC format.
        /// PostgreSQL requires DateTime values to be in UTC for 'timestamp with time zone' columns.
        /// </summary>
        private void NormalizeDateTimeToUtc(InteractionRequest req)
        {
            if (req.Date.Kind == DateTimeKind.Unspecified)
            {
                // Treat unspecified as UTC to avoid PostgreSQL errors
                req.Date = DateTime.SpecifyKind(req.Date, DateTimeKind.Utc);
                _logger.LogDebug("Normalized interaction Date from Unspecified to UTC: {Date}", req.Date);
            }
            else if (req.Date.Kind == DateTimeKind.Local)
            {
                // Convert local time to UTC
                req.Date = req.Date.ToUniversalTime();
                _logger.LogDebug("Converted interaction Date from Local to UTC: {Date}", req.Date);
            }
        }

        /// <summary>
        /// Creates a new interaction record with complete details including participants, type, and associated entities.
        /// Request includes: type (Meeting/Email/Call/Conference), subject, description, startDate, endDate, location, status, participants, partners.
        /// </summary>
        /// <param name="req">Interaction creation request with type, subject, description, dates, location, status, participants, and partners</param>
        /// <example_uses>
        /// Create a new meeting with UNICEF on project planning
        /// Record email interaction with partner contacts
        /// Add conference call with multiple stakeholders
        /// Log face-to-face meeting at headquarters
        /// Create virtual meeting interaction record
        /// </example_uses>
        /// <when_to_use>Use this when the user asks to create, add, record, or log a new interaction, meeting, call, or communication.</when_to_use>
        /// <returns>Created interaction with ID and metadata</returns>
        [HttpPost(APIDictionary.Interaction)]
        [AccessControlled(EntityTypes.Interaction, "create")]
        public async Task<ActionResult> Create([FromBody] InteractionRequest req)
        {
            // Validate model state first
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate required fields for interaction creation
            var validationErrors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(req.Subject))
            {
                validationErrors.Add("Subject is required for interaction creation");
            }
            
            // Validate that at least one participant is specified
            // Note: If ContactIds are provided, PartnerIds and EmailAddresses will be auto-populated from contacts
            var hasParticipants = (req.ContactIds != null && req.ContactIds.Any()) || 
                                  (req.PartnerIds != null && req.PartnerIds.Any()) ||
                                  (req.UserIds != null && req.UserIds.Any()) ||
                                  (req.EmailAddresses != null && req.EmailAddresses.Any());
            
            if (!hasParticipants)
            {
                validationErrors.Add("At least one participant is required (ContactIds, PartnerIds, UserIds, or EmailAddresses). Tip: If you provide ContactIds, PartnerIds and EmailAddresses will be auto-populated from the contacts.");
            }
            
            // Validate PartnerIds exist (if provided)
            if (req.PartnerIds != null && req.PartnerIds.Any())
            {
                foreach (var partnerId in req.PartnerIds)
                {
                    if (partnerId <= 0)
                    {
                        validationErrors.Add($"Invalid PartnerId: {partnerId}. Partner IDs must be positive integers from the system.");
                    }
                }
            }
            
            // Validate ContactIds exist (if provided)
            if (req.ContactIds != null && req.ContactIds.Any())
            {
                foreach (var contactId in req.ContactIds)
                {
                    if (contactId <= 0)
                    {
                        validationErrors.Add($"Invalid ContactId: {contactId}. Contact IDs must be positive integers from the system.");
                    }
                }
            }
            
            // Return validation errors if any
            if (validationErrors.Any())
            {
                var errorMessage = $"Validation failed for interaction creation: {string.Join("; ", validationErrors)}";
                _logger.LogWarning("Interaction creation validation failed: {Errors}", errorMessage);
                return BadRequest(new {
                    success = false,
                    error = errorMessage,
                    validationErrors = validationErrors,
                    requiredFields = new[] { "Subject" },
                    optionalButRecommended = new[] { "ContactIds", "PartnerIds", "Description", "Date", "Type", "Location" },
                    hint = "Ensure Subject is provided and at least one participant (ContactIds, PartnerIds, UserIds, or EmailAddresses) is specified. If you provide ContactIds, PartnerIds and EmailAddresses will be automatically populated from the contacts' data."
                });
            }
            
            // Auto-populate EmailAddresses and PartnerIds from ContactIds if not provided
            await AutoPopulateFromContactsAsync(req);
            
            // Normalize DateTime to UTC for PostgreSQL compatibility
            NormalizeDateTimeToUtc(req);

            // Check for duplicates ONLY if user hasn't confirmed duplicate creation
            if (!req.ConfirmDuplicateCreation)
            {
                try
                {
                    var duplicateResult = await _aiContextualService.DetectDuplicateForSingleRecordAsync(
                        "Interaction", 
                        req, 
                        0.7 // Field match threshold
                    );
                    
                    if (duplicateResult != null && duplicateResult.HasDuplicates)
                    {
                        return Ok(new {
                            success = false,
                            action = "duplicateConfirmation",
                            message = "Potential duplicate interaction detected. Do you want to create anyway?",
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
                    _logger.LogWarning($"Duplicate detection failed for interaction creation: {ex.Message}");
                    // Continue with creation since duplicate detection is not critical
                }
            }

            return await HandleOperationAsync(async () =>
            {
                var result = await _manager.CreateInteractionAsync(req);
                if (result == null)
                {
                    throw new BusinessException("Failed to create interaction");
                }
                
                return new {
                    success = true,
                    action = "created",
                    message = req.ConfirmDuplicateCreation ? 
                        "Interaction created successfully (duplicate confirmation acknowledged)" : 
                        "Interaction created successfully",
                    data = result
                };
            }, 201);
        }

        /// <summary>
        /// Retrieves all interactions with basic pagination and ordering (no search criteria).
        /// </summary>
        /// <param name="pageIndex">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20)</param>
        /// <param name="orderBy">Field to order results by (optional)</param>
        /// <param name="ascending">Sort direction - true for ascending, false for descending (default: true)</param>
        /// <param name="partnerId">Optional partner ID to filter interactions by specific partner</param>
        /// <param name="contactId">Optional contact ID to filter interactions by specific contact</param>
        /// <example_uses>
        /// Show me all interactions
        /// List all interactions in the system
        /// Display the interaction history
        /// Get all interaction records
        /// Browse interactions
        /// Show interactions for partner 123
        /// Show interactions for contact 456
        /// </example_uses>
        /// <when_to_use>Use this when the user wants to see ALL interactions without any search criteria or when asking for a general interaction list. Can be filtered by partner or contact.</when_to_use>
        /// <returns>Paginated list of all interactions, optionally filtered by partner or contact</returns>
        [HttpGet(APIDictionary.Interaction)]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> ListAllInteractions(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? orderBy = "Subject",
            [FromQuery] bool ascending = true,
            [FromQuery] int? partnerId = null,
            [FromQuery] int? contactId = null,
            [FromQuery] bool export = false,
            [FromQuery] bool filterActive = true)
        {
            // Validate model state first
            var modelValidationResult = ValidateModelState();
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            // Validate pagination parameters
            var paginationValidationResult = ValidatePaginationParameters(pageIndex, pageSize);
            if (paginationValidationResult != null) 
            {
                return paginationValidationResult;
            }

            return await HandleOperationAsync(async () =>
            {
                // Create a basic InteractionFilterRequest with just pagination and ordering
                var request = new InteractionFilterRequest
                {
                    PageIndex = pageIndex,
                    PageSize = export ? int.MaxValue : pageSize, // Remove pagination limits for export
                    OrderBy = orderBy ?? "Subject",
                    Ascending = ascending,
                    PartnerId = partnerId,
                    ContactId = contactId,
                    FilterActive = filterActive
                };
                
                // Return all interactions with secure pagination
                var result = await SecureSearchControllerHelper.ProcessSecureListingAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                    request,
                    "Interaction",
                    User,
                    _secureSpecificationFactory.CreateInteractionSpecificationAsync,
                    async (userId, spec, pagination) => await _manager.GetInteractionsWithSpecification(userId, spec, (InteractionFilterRequest)pagination),
                    CurrentUserId,
                    _logger);
                
                return result;
            });
        }

        /// <summary>
        /// Performs simple text search across multiple interaction fields (subject, description, etc.).
        /// </summary>
        /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
        /// <param name="query">Text to search across interaction subject, description, and other basic fields</param>
        /// <param name="partnerId">Optional partner ID to filter by partner</param>
        /// <param name="contactId">Optional contact ID to filter by contact</param>
        /// <param name="export">Whether to export all results without pagination</param>
        /// <param name="filterActive">Whether to apply global filters, default: true</param>
        /// <example_uses>
        /// Search for interactions about project
        /// Find interactions containing 'meeting notes'
        /// Search for interactions with 'UNICEF' mentioned
        /// Look for interactions about specific topics
        /// Find interactions by keywords
        /// </example_uses>
        /// <when_to_use>Use this for simple keyword searches across interaction content. NOT for relationship-based searches.</when_to_use>
        /// <returns>Paginated list of interactions matching the search text</returns>
        [HttpGet(APIDictionary.Interaction + "/search")]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> SearchInteractions(
            [FromQuery] PaginationRequest request,
            [FromQuery] string query,
            [FromQuery] int? partnerId = null,
            [FromQuery] int? contactId = null,
            [FromQuery] bool export = false,
            [FromQuery] bool filterActive = true)
        {
            // Validate model state first
            var modelValidationResult = ValidateModelState();
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            // Validate pagination parameters
            var paginationValidationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
            if (paginationValidationResult != null) 
            {
                return paginationValidationResult;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new BusinessException("Search text is required for interaction search");
            }

            // Use the enhanced search pattern (now includes PostgreSQL similarity search)
            var paginationRequest = new PaginationRequest
            {
                PageIndex = request.PageIndex,
                PageSize = export ? int.MaxValue : request.PageSize, // Remove pagination limits for export
                OrderBy = request.OrderBy ?? "Subject",
                Ascending = request.Ascending ?? true,
                FilterActive = filterActive
            };

            // Create unified search request with query and filters for entity-specific search
            var additionalFilters = new List<UNOPS.PAO.UNOPSBusiness.Services.SearchFilter>();

        // Apply base entity filtering first if partnerId or contactId is provided
        if (partnerId.HasValue || contactId.HasValue)
        {
            _logger.LogInformation("Adding entity filters to search - PartnerId: {PartnerId}, ContactId: {ContactId}", partnerId, contactId);
            
            // Add the entity filters to the search filters list
            if (partnerId.HasValue)
            {
                additionalFilters.Add(new UNOPS.PAO.UNOPSBusiness.Services.SearchFilter
                {
                    field = "InteractionPartners.Any(ip => ip.PartnerId == " + partnerId.Value + ")",
                    @operator = "eq",
                    value = "true",
                    logicalOperator = "AND",
                    fieldType = "bool"
                });
            }
            
            if (contactId.HasValue)
            {
                additionalFilters.Add(new UNOPS.PAO.UNOPSBusiness.Services.SearchFilter
                {
                    field = "InteractionContacts.Any(ic => ic.ContactId == " + contactId.Value + ")",
                    @operator = "eq",
                    value = "true",
                    logicalOperator = "AND",
                    fieldType = "bool"
                });
            }
        }

        _logger.LogInformation("Using AdvancedSearchService for search with {FilterCount} entity filters", additionalFilters.Count);

            var searchRequest = new UNOPS.PAO.UNOPSBusiness.Services.UnifiedSearchRequest
            {
                Query = query,
                Filters = additionalFilters,
                PageIndex = paginationRequest.PageIndex,
                PageSize = paginationRequest.PageSize,
                OrderBy = paginationRequest.OrderBy,
                Ascending = paginationRequest.Ascending ?? true,
                FilterActive = paginationRequest.FilterActive
            };

            // Use AdvancedSearchService for unified text search with PostgreSQL similarity and metadata
            var result = await _advancedSearchService.SearchWithQueryAndMetadataAsync<UNOPSInteraction, InteractionModel>(
                query, 
                paginationRequest, 
                User);

            _logger.LogInformation("Interaction search completed: Found {TotalCount} results for query: {Query}, export: {Export}", result.TotalCount, query, export);

            return Ok(result);
        }

        /// <summary>
        /// Performs advanced search with structured criteria including relationships with partners, contacts, dates, and complex filters.
        /// Enhanced with intelligent field value matching for AI agents and typo correction.
        /// </summary>
        /// <param name="filters">JSON array of search criteria objects with field, operator, value, and logicalOperator</param>
        /// <param name="pageIndex">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="ascending">Sort direction</param>
        /// <param name="partnerId">Optional partner ID to filter by partner</param>
        /// <param name="contactId">Optional contact ID to filter by contact</param>
        /// <param name="export">Whether to export all results without pagination</param>
        /// <param name="filterActive">Whether to apply global filters, default: true</param>
        /// <example_uses>
        /// Find interactions with UNICEF partners
        /// Show meetings with John Smith contact
        /// Get email interactions this week
        /// Find conference calls from Finance department contacts
        /// List interactions by type and date range
        /// Search for interactions by complex criteria combinations
        /// </example_uses>
        /// <when_to_use>Use this for searches involving partner relationships, contact details, dates, interaction types, or multiple field combinations.</when_to_use>
        /// <searchCriteria_format>
        /// JSON array format: [{"field": "partner.name", "operator": "like", "value": "UNICEF", "logicalOperator": "AND"}]
        /// Available operators: is, is not, like, not like, greater than, less than, greater than or equal, less than or equal, this week, this month, this year
        /// Available fields: type, date, subject, description, contact.firstName, contact.lastName, partner.name, partner.status
        /// Logical operators: AND, OR
        /// </searchCriteria_format>
        /// <returns>Paginated list of interactions matching the advanced search criteria</returns>
    [HttpGet(APIDictionary.Interaction + "/advanced-search")]
    [AccessControlled(EntityTypes.Interaction, "read")]
    public async Task<ActionResult> AdvancedSearchInteractions(
        [FromQuery] string filters,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "Subject",
        [FromQuery] bool ascending = true,
        [FromQuery] int? partnerId = null,
        [FromQuery] int? contactId = null,
        [FromQuery] bool export = false,
        [FromQuery] bool filterActive = true)
    {
        _logger.LogInformation("=== INTERACTION ADVANCED SEARCH ENDPOINT ENTRY ===");
        
        try
        {
            _logger.LogInformation("=== INTERACTION ADVANCED SEARCH ENDPOINT ===");
            _logger.LogInformation("Filters: {Filters}, Page: {PageIndex}, Size: {PageSize}", filters, pageIndex, pageSize);
            _logger.LogInformation("PartnerId: {PartnerId}, ContactId: {ContactId}, Export: {Export}, FilterActive: {FilterActive}", partnerId, contactId, export, filterActive);

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

            // Apply base entity filtering first if partnerId or contactId is provided
            if (partnerId.HasValue || contactId.HasValue)
            {
                _logger.LogInformation("Adding entity filters to advanced search - PartnerId: {PartnerId}, ContactId: {ContactId}", partnerId, contactId);
                
                // Add the entity filters to the existing search filters
                if (partnerId.HasValue)
                {
                    searchFilters.Add(new UNOPS.PAO.UNOPSBusiness.Services.SearchFilter
                    {
                        field = "InteractionPartners.Any(ip => ip.PartnerId == " + partnerId.Value + ")",
                        @operator = "eq",
                        value = "true",
                        logicalOperator = "AND",
                        fieldType = "bool"
                    });
                }
                
                if (contactId.HasValue)
                {
                    searchFilters.Add(new UNOPS.PAO.UNOPSBusiness.Services.SearchFilter
                    {
                        field = "InteractionContacts.Any(ic => ic.ContactId == " + contactId.Value + ")",
                        @operator = "eq",
                        value = "true",
                        logicalOperator = "AND",
                        fieldType = "bool"
                    });
                }
            }

            _logger.LogInformation("Using AdvancedSearchService with {FilterCount} total filters", searchFilters.Count);

            // Use AdvancedSearchService for structured filters with PostgreSQL similarity on "like" operators
            var paginationRequest = new PaginationRequest
            {
                PageIndex = pageIndex,
                PageSize = export ? int.MaxValue : pageSize, // Remove pagination limits for export
                OrderBy = orderBy ?? "Subject",
                Ascending = ascending,
                FilterActive = filterActive
            };

            var result = await _advancedSearchService.SearchWithFiltersAsync<UNOPSInteraction, InteractionModel>(
                searchFilters,
                paginationRequest,
                User);
            
            _logger.LogInformation("Advanced interaction search completed: Found {TotalCount} results", result.TotalCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced interaction search - PartnerId: {PartnerId}, ContactId: {ContactId}, Filters: {Filters}", partnerId, contactId, filters);
            _logger.LogError("Exception details: {ExceptionType} - {ExceptionMessage}", ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerExceptionType} - {InnerExceptionMessage}", ex.InnerException.GetType().Name, ex.InnerException.Message);
            }
            return StatusCode(500, new { error = "Internal server error during interaction search", details = ex.Message });
        }
    }

        /// <summary>
        /// Get supported search fields for interactions - helps frontend build dynamic search forms
        /// </summary>
        /// <returns>List of all supported search fields with their metadata</returns>
        [HttpGet(APIDictionary.SingularInteraction  + "/search-fields")]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public ActionResult<List<SearchFieldInfo>> GetInteractionSearchFields()
        {
            try
            {
                var fields = _manager.GetInteractionSearchFields();
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interaction search fields");
                return StatusCode(500, new { error = "An error occurred while retrieving search fields" });
            }
        }

        /// <summary>
        /// Retrieves a specific interaction by ID with complete details including participants, documents, and permissions.
        /// </summary>
        /// <param name="id">Interaction ID</param>
        /// <example_uses>
        /// Show me details for interaction ID 123
        /// Get full information about meeting 456
        /// Display interaction record 789
        /// Get complete interaction details
        /// Show meeting with all participants and documents
        /// </example_uses>
        /// <when_to_use>Use this when the user asks for specific interaction details by ID or when you need complete interaction information.</when_to_use>
        /// <returns>Complete interaction details with participants and related information</returns>
        [HttpGet(APIDictionary.Interaction + "/{id}")]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> Get(int id)
        {
            return await HandleOperationAsync(async () =>
            {
                // Use the secure method that includes permissions in the interaction model
                var interaction = await _manager.GetInteractionAsync(User, id);
                if (interaction == null)
                {
                    throw new BusinessException($"Interaction with ID {id} not found");
                }

                return interaction;
            });
        }

        /// <summary>
        /// Updates an existing interaction's information including details, participants, scheduling, and metadata.
        /// </summary>
        /// <param name="req">Interaction update request (id, subject, description, type, startDate, endDate, location, status, participants)</param>
        /// <example_uses>
        /// Update meeting 123's time to 2 PM
        /// Change interaction 456's location to virtual
        /// Modify meeting description and agenda
        /// Update participant list for conference call
        /// Change meeting status to completed
        /// </example_uses>
        /// <when_to_use>Use this when the user asks to update, modify, edit, or change interaction information.</when_to_use>
        /// <returns>Success confirmation</returns>
        [HttpPut(APIDictionary.Interaction)]
        [AccessControlled(EntityTypes.Interaction, "update")]
        public async Task<ActionResult> Update([FromBody] UpdateInteractionRequest req)
        {
            // Auto-populate EmailAddresses and PartnerIds from ContactIds if not provided
            await AutoPopulateFromContactsAsync(req);
            
            // Normalize DateTime to UTC for PostgreSQL compatibility
            NormalizeDateTimeToUtc(req);
            
            return await HandleOperationAsync(async () =>
            {
                await _manager.UpdateInteractionAsync(CurrentUserId, req);
            });
        }

        /// <summary>
        /// Soft deletes an interaction from the system (marks as deleted rather than permanent removal).
        /// </summary>
        /// <param name="id">Interaction ID to delete</param>
        /// <example_uses>
        /// Delete interaction ID 123
        /// Remove meeting 456 from the system
        /// Cancel and delete upcoming meeting
        /// Remove completed interaction record
        /// Soft delete interaction entry
        /// </example_uses>
        /// <when_to_use>Use this when the user asks to delete, remove, cancel, or eliminate an interaction.</when_to_use>
        /// <returns>No content on successful deletion</returns>
        [HttpDelete(APIDictionary.Interaction + "/{id}")]
        [AccessControlled(EntityTypes.Interaction, "delete")]
        public async Task<ActionResult> Delete(int id)
        {
            return await HandleOperationAsync(async () =>
            {
                await _manager.DeleteInteractionAsync(CurrentUserId, id);
            });
        }

        /// <summary>
        /// Retrieves the current user's permissions for a specific interaction (read, update, delete).
        /// </summary>
        /// <param name="id">Interaction ID to check permissions for</param>
        /// <example_uses>
        /// Check my permissions for interaction 123
        /// What can I do with meeting 456?
        /// Get access rights for this interaction
        /// Verify interaction permissions before editing
        /// Can I update this meeting?
        /// </example_uses>
        /// <when_to_use>Use this when you need to check user permissions before performing operations or showing UI elements for interaction management.</when_to_use>
        /// <returns>Permission object with CanRead, CanUpdate, CanDelete flags</returns>
        [HttpGet(APIDictionary.Interaction + "/{id}/permissions")]
        public async Task<ActionResult> PermissionsGet(int id)
        {
            return await HandleOperationAsync(async () =>
            {
                var interaction = await _manager.GetInteraction(CurrentUserId, id);
                if (interaction == null)
                {
                    throw new BusinessException($"Interaction with ID {id} not found");
                }

                // Create entity for permission checking
                // var interactionEntity = new UNOPSDomain.Entities.UNOPSInteraction
                // {
                //     Id = interaction.Id
                //     // Note: Contact relationships are now handled through InteractionContacts junction table
                // };

                // return await _businessSecurityService.GetEntityPermissionsAsync(interactionEntity, User);
                
                // Return default permissions for now
                return new { CanRead = true, CanUpdate = true, CanDelete = true };
            });
        }

        #region AI-Powered Interaction Data Processing

        /// <summary>
        /// Scans and processes uploaded files for interaction data extraction using AI-powered analysis.
        /// </summary>
        /// <param name="req">File scan request with File property (required)</param>
        /// <example_uses>
        /// Scan meeting notes for interaction details
        /// Upload email threads for processing
        /// Analyze call transcripts with AI
        /// Extract data from communication logs
        /// Process interaction records from uploaded files
        /// </example_uses>
        /// <when_to_use>Use this when the user wants to upload and scan documents for interaction data extraction using AI.</when_to_use>
        /// <returns>Extracted interaction data from the scanned file</returns>
        [HttpPost(APIDictionary.Interaction + "/scan-data")]
        [AccessControlled(EntityTypes.Interaction, "create")]
        public async Task<ActionResult> ScanInteractionData([FromForm] GeminiFileRequest req) 
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
                    throw new BusinessException("Prompt configuration for interaction data scanning is not found.");
                }

                return response.Trim();
            });
        }

        /// <summary>
        /// Analyzes uploaded files and extracts structured interaction data using AI-powered data analysis.
        /// </summary>
        /// <param name="request">Analysis request (entityType: 'Interaction', analysisType)</param>
        /// <example_uses>
        /// Analyze meeting transcripts for structured data extraction
        /// Extract interaction information from uploaded logs
        /// Process communication documents with AI
        /// Convert interaction files into structured database entries
        /// Analyze call records for key information
        /// </example_uses>
        /// <when_to_use>Use this when the user wants to analyze files and extract structured interaction data for database import.</when_to_use>
        /// <returns>Structured interaction data extracted from the analyzed file</returns>
        [HttpPost(APIDictionary.Interaction + "/analyse-file")]
        [AccessControlled(EntityTypes.Interaction, "create")]
        public async Task<ActionResult> AnalyseInteractionData([FromBody] AnalyseFileRequest request)
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
        /// Bulk uploads multiple interaction records using AI-assisted data processing and validation.
        /// </summary>
        /// <param name="req">Bulk upload request (Type: 'Interaction', Data, Options)</param>
        /// <example_uses>
        /// Bulk upload 200 interactions from Excel
        /// Import multiple interactions from CSV file
        /// Mass upload interaction data with AI validation
        /// Bulk import interaction records with duplicate detection
        /// Upload large interaction dataset with automated processing
        /// </example_uses>
        /// <when_to_use>Use this when the user wants to upload multiple interaction records at once with AI-assisted processing.</when_to_use>
        /// <returns>Bulk upload results with success/failure status for each interaction</returns>
        [HttpPost(APIDictionary.Interaction + "/bulk-upload")]
        [AccessControlled(EntityTypes.Interaction, "create")]
        public async Task<ActionResult> BulkUploadInteractions([FromBody] BulkUploadRequest req) 
        {
            return await HandleOperationAsync(async () => 
            {
                if (req == null || string.IsNullOrEmpty(req.Type))
                {
                    throw new BusinessException("Invalid request.");
                }

                // Ensure the request is for interaction entities
                if (!req.Type.Equals("Interaction", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessException("This endpoint only supports Interaction bulk uploads.");
                }

                string response = await _geminiManager.BulkInsertRecordsAsync(req);
                return new { message = response };
            });
        }

        #endregion

        /// <summary>
        /// Describes the Interaction entity structure including all field configurations
        /// </summary>
        /// <returns>Entity and field metadata for Interaction</returns>
        [HttpGet(APIDictionary.Interaction + "/metadata-info")]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> GetMetadataInfo()
        {
            try
            {
                var entityDetails = await _entityConfigurationManager.GetEntityConfigurationDetailsAsync(User, "Interaction");
                return Ok(entityDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Interaction entity description");
                return StatusCode(500, new { error = "Failed to retrieve Interaction entity description" });
            }
        }

        /// <summary>
        /// Performs semantic search on interactions using AI embeddings to find similar interactions based on natural language queries.
        /// </summary>
        /// <param name="query">Natural language search query</param>
        /// <param name="threshold">Similarity threshold (0.0 to 1.0, default: 0.7)</param>
        /// <param name="limit">Maximum number of results to return (default: 10)</param>
        /// <example_uses>
        /// Find interactions similar to project planning meetings
        /// Search for email communications about healthcare
        /// Find conference calls about technical issues
        /// Search for meetings in the education sector
        /// Find interactions similar to contract negotiations
        /// </example_uses>
        /// <when_to_use>Use this when the user wants to find interactions using natural language queries or semantic similarity.</when_to_use>
        /// <returns>List of similar interactions with similarity scores</returns>
        [HttpGet(APIDictionary.Interaction + "/deepSearch")]
        [AccessControlled(EntityTypes.Interaction, "read")]
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
                    "Interaction", 
                    embedding, 
                    threshold, 
                    limit
                );

                // Get the actual interaction data for the found IDs
                var interactions = new List<object>();
                foreach (var result in searchResults)
                {
                    try
                    {
                        var interaction = await _manager.GetInteractionAsync(User, result.EntityId);
                        if (interaction != null)
                        {
                            interactions.Add(new
                            {
                                interaction = interaction,
                                similarityScore = result.Score,
                                searchType = result.SearchType
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retrieve interaction {InteractionId} from search results", result.EntityId);
                    }
                }

                return Ok(new
                {
                    query = query,
                    threshold = threshold,
                    totalResults = searchResults.Count,
                    results = interactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing deep search for interactions with query: {Query}", query);
                return StatusCode(500, new { error = "An error occurred while performing the semantic search" });
            }
        }

        /// <summary>
        /// Detects duplicates for an existing interaction record after save operations
        /// </summary>
        /// <param name="req">Interaction data to check for duplicates</param>
        /// <returns>Duplicate detection results</returns>
        [HttpPost(APIDictionary.Interaction + "/detect-duplicates")]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> DetectDuplicatesForInteraction([FromBody] dynamic req)
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
                "Interaction", 
                requestData, 
                0.7 // Standard sensitivity for post-save detection
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
                    entityType = "Interaction",
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

                _logger.LogWarning(ex, "Post-save duplicate detection failed for Interaction ID {InteractionId}", idForLogging);
                // Return success with no duplicates rather than failing - this is a background operation
                return Ok(new {
                    success = true,
                    entityType = "Interaction",
                    recordId = (object?)null,
                    duplicateInfo = (object?)null,
                    warning = "Duplicate detection temporarily unavailable"
                });
            }
        }

        /// <summary>
        /// Get interactions with optional search query and pagination - Brief version for fast listing
        /// Fast endpoint for listing interactions with basic details
        /// </summary>
        /// <param name="query">Optional search query text</param>
        /// <param name="pageIndex">Page number (1-based), default: 1</param>
        /// <param name="pageSize">Items per page, default: 50</param>
        /// <param name="orderBy">Field to order by, default: CreatedDate</param>
        /// <param name="ascending">Sort direction, default: false (descending)</param>
        /// <param name="filterActive">Whether to apply global filters, default: true</param>
        /// <returns>Paginated list of interactions with search metadata</returns>
        [HttpGet(APIDictionary.InteractionsBrief)]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<IActionResult> GetInteractionsBrief(
            [FromQuery] string? query = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? orderBy = "CreatedDate",
            [FromQuery] bool? ascending = false,
            [FromQuery] bool filterActive = true)
        {
            return await HandleOperationAsync(async () =>
            {
                var startTime = DateTime.UtcNow;
                _logger.LogInformation("Getting interactions. Query: '{Query}', Page: {PageIndex}, PageSize: {PageSize}, OrderBy: {OrderBy}, Ascending: {Ascending}, FilterActive: {FilterActive}",
                    query, pageIndex, pageSize, orderBy, ascending, filterActive);

                PaginationResponse<InteractionModel> result;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    // Use AdvancedSearchService for text search
                    var pagination = new PaginationRequest
                    {
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        OrderBy = orderBy,
                        Ascending = ascending,
                        FilterActive = filterActive
                    };

                    result = await _advancedSearchService.SearchWithQueryAsync<UNOPSInteraction, InteractionModel>(
                        query,
                        pagination,
                        User);
                }
                else
                {
                    // No search query - get all interactions (filtered by access control)
                    var pagination = new PaginationRequest
                    {
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        OrderBy = orderBy,
                        Ascending = ascending,
                        FilterActive = filterActive
                    };

                    result = await _manager.GetInteractionsAsync(User, pagination);
                }

                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                result.ExecutionTimeMs = executionTime;

                _logger.LogInformation("Returned {Count} interactions in {ExecutionTime}ms", result.Records?.Count ?? 0, executionTime);
                return (object)result;
            });
        }
    }
} 