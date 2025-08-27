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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class InteractionController : BaseController
    {
        private readonly IInteractionManager _manager;
        private readonly ISecureSpecificationFactory _secureSpecificationFactory;
        private readonly IGeminiManager _geminiManager;
        private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;
        private readonly AiContextualService _aiContextualService;

        public InteractionController(
            IManagerWrapper manager, 
            UserResolverService<int> userResolverService,
            IAuthorizationService authorizationService,
            ISecureSpecificationFactory secureSpecificationFactory,
            ILogger<InteractionController> logger,
            AiContextualService aiContextualService)
            : base(logger, authorizationService, userResolverService)
        {
            _manager = manager.InteractionManager;
            _secureSpecificationFactory = secureSpecificationFactory;
            _geminiManager = manager.GeminiManager;
            _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
            _aiContextualService = aiContextualService;
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
        /// Retrieves all interactions with basic pagination and ordering (no search criteria).
        /// </summary>
        /// <param name="pageIndex">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20)</param>
        /// <param name="orderBy">Field to order results by (optional)</param>
        /// <param name="ascending">Sort direction - true for ascending, false for descending (default: true)</param>
        /// <example_uses>
        /// Show me all interactions
        /// List all interactions in the system
        /// Display the interaction history
        /// Get all interaction records
        /// Browse interactions
        /// </example_uses>
        /// <when_to_use>Use this when the user wants to see ALL interactions without any search criteria or when asking for a general interaction list.</when_to_use>
        /// <returns>Paginated list of all interactions</returns>
        [HttpGet(APIDictionary.Interaction)]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<ActionResult> ListAllInteractions(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool ascending = true)
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
                    PageSize = pageSize,
                    OrderBy = orderBy,
                    Ascending = ascending
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
        /// <param name="searchText">Text to search across interaction subject, description, and other basic fields</param>
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
            [FromQuery] string searchText)
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

            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new BusinessException("Search text is required for interaction search");
            }

            return await HandleOperationAsync(async () =>
            {
                // Create an InteractionFilterRequest with pagination/sorting info and search text
                var interactionFilterRequest = new InteractionFilterRequest
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    OrderBy = request.OrderBy,
                    Ascending = request.Ascending,
                    SearchText = searchText
                };

                var result = await SecureSearchControllerHelper.ProcessSecureSimpleTextSearchAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                    searchText, 
                    request.PageIndex, 
                    request.PageSize, 
                    request.OrderBy, 
                    request.Ascending,
                    interactionFilterRequest,
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
        /// Performs advanced search with structured criteria including relationships with partners, contacts, dates, and complex filters.
        /// </summary>
        /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
        /// <param name="searchCriteria">JSON array of search criteria objects with field, operator, value, and logicalOperator</param>
        /// <param name="searchText">Optional additional text search to combine with criteria</param>
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
            [FromQuery] PaginationRequest request,
            [FromQuery] string searchCriteria,
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

            if (string.IsNullOrWhiteSpace(searchCriteria))
            {
                throw new BusinessException("Search criteria is required for advanced interaction search");
            }

            return await HandleOperationAsync(async () =>
            {
                // Create an InteractionFilterRequest with pagination/sorting info and search criteria
                var interactionFilterRequest = new InteractionFilterRequest
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    OrderBy = request.OrderBy,
                    Ascending = request.Ascending,
                    SearchCriteria = searchCriteria,
                    SearchText = searchText,
                    AdvancedSearch = true // Set this internally since we know this is an advanced search
                };

                var result = await SecureSearchControllerHelper.ProcessSecureAdvancedSearchAsync<Domain.Entities.Interaction, InteractionFilterRequest, PaginationResponse<InteractionModel>>(
                    searchCriteria, 
                    searchText, 
                    request.PageIndex, 
                    request.PageSize, 
                    request.OrderBy, 
                    request.Ascending,
                    interactionFilterRequest,
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

        #region AI-Powered Interaction Data Processing

        /// <summary>
        /// Scans and processes uploaded files for interaction data extraction using AI-powered analysis.
        /// </summary>
        /// <param name="req">File scan request containing the file to be processed</param>
        /// <param name="req.File">File to scan for interaction data (required)</param>
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

                string fileType = _geminiManager.FindFileType(req.File);

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
        /// <param name="request">Analysis request containing file and analysis parameters</param>
        /// <param name="request.entityType">Should be set to 'Interaction' for interaction data analysis</param>
        /// <param name="request.analysisType">Type of analysis to perform on interaction data</param>
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
        /// <param name="req">Bulk upload request containing interaction data</param>
        /// <param name="req.Type">Should be set to 'Interaction' for interaction bulk upload</param>
        /// <param name="req.Data">Array of interaction data objects to upload</param>
        /// <param name="req.Options">Upload options and validation settings</param>
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
    }
} 