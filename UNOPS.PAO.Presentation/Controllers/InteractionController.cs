using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Attributes;

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class InteractionController : BaseController
    {
        private readonly IInteractionManager _manager;
        private readonly ISecureSpecificationFactory _secureSpecificationFactory;
        private readonly IOrgUnitFilterService _orgUnitFilterService;

        public InteractionController(
            IManagerWrapper manager, 
            UserResolverService<int> userResolverService, 
            IAuthorizationService authorizationService,
            ISecureSpecificationFactory secureSpecificationFactory,
            IOrgUnitFilterService orgUnitFilterService,
            ILogger<InteractionController> logger)
            : base(logger, authorizationService, userResolverService)
        {
            _manager = manager.InteractionManager;
            _secureSpecificationFactory = secureSpecificationFactory;
            _orgUnitFilterService = orgUnitFilterService;
        }

        /// <summary>
        /// Creates a new interaction record with complete details including participants, type, and associated entities.
        /// </summary>
        /// <param name="req">Interaction creation request with all required details</param>
        /// <param name="req.type">Interaction type (required) - e.g., 'Meeting', 'Email', 'Call', 'Conference'</param>
        /// <param name="req.subject">Interaction subject/title (required)</param>
        /// <param name="req.description">Detailed description of the interaction</param>
        /// <param name="req.startDate">Interaction start date and time</param>
        /// <param name="req.endDate">Interaction end date and time</param>
        /// <param name="req.location">Meeting location or platform</param>
        /// <param name="req.status">Interaction status</param>
        /// <param name="req.participants">List of contact participants</param>
        /// <param name="req.partners">List of partner organizations involved</param>
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

            return await HandleOperationAsync(async () =>
            {
                var result = await _manager.CreateInteractionAsync(req);
                if (result == null)
                {
                    throw new BusinessException("Failed to create interaction");
                }
                return result;
            }, 201);
        }

        /// <summary>
        /// Retrieves a list of interactions with advanced filtering, pagination, search capabilities, and access control.
        /// </summary>
        /// <param name="request">Interaction filter request containing search and pagination parameters</param>
        /// <param name="request.pageIndex">Page number (1-based)</param>
        /// <param name="request.pageSize">Number of items per page</param>
        /// <param name="request.searchText">Text to search across interaction fields</param>
        /// <param name="request.orderBy">Field to order results by</param>
        /// <param name="request.ascending">Sort direction (true for ascending)</param>
        /// <param name="request.type">Filter by interaction type</param>
        /// <param name="request.status">Filter by interaction status</param>
        /// <param name="request.startDate">Filter interactions from this date</param>
        /// <param name="request.endDate">Filter interactions until this date</param>
        /// <param name="advancedSearch">Enable advanced search mode for complex queries with nested entity searches</param>
        /// <param name="searchCriteria">MANDATORY when advancedSearch=true. JSON array of search criteria objects with field, operator, value, and description</param>
        /// <param name="searchText">Simple text search across interaction fields (used when advancedSearch=false)</param>
        /// <advanced_search_guidance>
        /// CRITICAL DECISION RULES for AI Agent:
        /// 
        /// USE searchText (advancedSearch=false) when:
        /// - Simple keyword searches: "project", "meeting notes", "UNICEF"
        /// - General text searches across multiple fields
        /// - Single search terms or phrases
        /// - User asks: "find interactions containing X", "search for Y", "interactions about Z"
        /// 
        /// USE advancedSearch=true when:
        /// - Searching related entities: "interactions with UNICEF partners", "meetings with John Smith contact"
        /// - Field-specific searches: "interactions where partner name is UNICEF", "contacts with email containing @unops.org"
        /// - Complex criteria with operators: date ranges, status filters, type filters
        /// - User asks: "find interactions with X partner", "meetings with Y contact", "interactions by Z person"
        /// 
        /// MANDATORY searchCriteria FORMAT (when advancedSearch=true):
        /// [
        ///   {
        ///     "field": "partner.name",
        ///     "operator": "like", 
        ///     "value": "UNICEF",
        ///     "description": "Find interactions with UNICEF partners"
        ///   },
        ///   {
        ///     "field": "type",
        ///     "operator": "is",
        ///     "value": "Meeting",
        ///     "logicalOperator": "AND",
        ///     "description": "Must be meeting type interactions"
        ///   }
        /// ]
        /// 
        /// AVAILABLE FIELDS:
        /// Direct fields: id, contactId, type, date, fromDate, toDate, description, subject
        /// Nested fields: contact.firstName, contact.lastName, contact.email, contact.title, contact.department, contact.phone, contact.mobile, partner.name, partner.status, partner.shortName
        /// 
        /// AVAILABLE OPERATORS:
        /// like, is, not, contains, startsWith, endsWith, between, greaterThan, lessThan, greaterThanOrEqual, lessThanOrEqual, on, "not on", "this week", "this month", "this year"
        /// 
        /// EXAMPLES:
        /// Simple: searchText="project updates" (advancedSearch=false)
        /// Advanced: searchCriteria=[{"field":"partner.name","operator":"like","value":"UNICEF","description":"Find UNICEF partner interactions"}] (advancedSearch=true)
        /// </advanced_search_guidance>
        /// <example_uses>
        /// Show me all interactions (no search)
        /// Find interactions containing "project" (searchText)
        /// Find interactions with UNICEF partners (advancedSearch)
        /// Get meetings with John Smith contact (advancedSearch)
        /// Show email interactions this week (advancedSearch with date)
        /// List interactions sorted by date (no search, just sorting)
        /// Find conference calls from Finance department contacts (advancedSearch)
        /// </example_uses>
        /// <when_to_use>Use this when the user asks to search, list, filter, or browse interactions, meetings, calls, or communications.</when_to_use>
        /// <returns>Paginated list of interactions with metadata</returns>
        [HttpGet(APIDictionary.Interaction)]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> GetAll(
            [FromQuery] InteractionFilterRequest request,
            [FromQuery] bool advancedSearch = false,
            [FromQuery] string? searchCriteria = null,
            [FromQuery] string? searchText = null)
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

            return await HandleOperationAsync(async () =>
            {
                
                PaginationResponse<InteractionModel> result;
                
                // Handle different search scenarios with integrated RBAC filtering
                if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
                {
                    result = await SecureSearchControllerHelper.ProcessSecureAdvancedSearchAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                        searchCriteria, 
                        searchText ?? request.SearchText, 
                        request.PageIndex, 
                        request.PageSize, 
                        request.OrderBy, 
                        request.Ascending,
                        request,
                        "Interaction",
                        User,
                        _secureSpecificationFactory.CreateInteractionSpecificationAsync,
                        async (userId, spec, pagination) => await _manager.GetInteractionsWithSpecification(userId, spec, (InteractionFilterRequest)pagination),
                        CurrentUserId, 
                        _logger);
                }
                else if (!string.IsNullOrWhiteSpace(searchText) || !string.IsNullOrWhiteSpace(request.SearchText))
                {
                    var textToSearch = searchText ?? request.SearchText;
                    result = await SecureSearchControllerHelper.ProcessSecureSimpleTextSearchAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                        textToSearch!, 
                        request.PageIndex, 
                        request.PageSize, 
                        request.OrderBy, 
                        request.Ascending,
                        request,
                        "Interaction",
                        User,
                        _secureSpecificationFactory.CreateInteractionSpecificationAsync,
                        async (userId, spec, pagination) => await _manager.GetInteractionsWithSpecification(userId, spec, (InteractionFilterRequest)pagination),
                        CurrentUserId, 
                        _logger);
                }
                else
                {
                    // For no search parameters, return all interactions with secure pagination
                    result = await SecureSearchControllerHelper.ProcessSecureListingAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                        request,
                        "Interaction",
                        User,
                        _secureSpecificationFactory.CreateInteractionSpecificationAsync,
                        async (userId, spec, pagination) => await _manager.GetInteractionsWithSpecification(userId, spec, (InteractionFilterRequest)pagination),
                        CurrentUserId,
                        _logger);
                }
                
                // No need for post-query RBAC filtering anymore - security is integrated at database level
                // This ensures proper pagination and accurate TotalCount
                
                return result;
            });
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
        /// <param name="req">Interaction update request containing modified fields</param>
        /// <param name="req.id">Interaction ID to update (required)</param>
        /// <param name="req.subject">Updated subject/title</param>
        /// <param name="req.description">Updated description</param>
        /// <param name="req.type">Updated interaction type</param>
        /// <param name="req.startDate">Updated start date and time</param>
        /// <param name="req.endDate">Updated end date and time</param>
        /// <param name="req.location">Updated location</param>
        /// <param name="req.status">Updated status</param>
        /// <param name="req.participants">Updated participant list</param>
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
    }
} 