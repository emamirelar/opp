using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.UNOPSBusiness.Authorization;

namespace UNOPS.PAO.Presentation.Controllers.Shared
{
    /// <summary>
    /// Controller for managing saved search filters
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class SavedFilterController : BaseController
    {
        private readonly ISavedFilterService _savedFilterService;

        public SavedFilterController(
            ISavedFilterService savedFilterService,
            UserResolverService<int> userResolverService,
            IAuthorizationService authorizationService,
            ILogger<SavedFilterController> logger)
            : base(logger, authorizationService, userResolverService)
        {
            _savedFilterService = savedFilterService;
        }

        /// <summary>
        /// Create a new saved filter
        /// </summary>
        /// <param name="request">Filter creation request</param>
        /// <returns>Created filter information</returns>
        [HttpPost]
        public async Task<ActionResult> CreateSavedFilter([FromBody] CreateSavedFilterRequest request)
        {
            try
            {
                _logger.LogInformation("Creating saved filter '{FilterName}' for entity '{EntityType}'", 
                    request.Name, request.EntityType);

                var result = await _savedFilterService.CreateSavedFilterAsync(User, request);
                return Created($"api/savedfilter/{result.Id}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating saved filter");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing saved filter
        /// </summary>
        /// <param name="request">Filter update request</param>
        /// <returns>Updated filter information</returns>
        [HttpPut]
        public async Task<ActionResult> UpdateSavedFilter([FromBody] UpdateSavedFilterRequest request)
        {
            try
            {
                _logger.LogInformation("Updating saved filter {FilterId}", request.Id);

                var result = await _savedFilterService.UpdateSavedFilterAsync(User, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating saved filter {FilterId}", request.Id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a saved filter
        /// </summary>
        /// <param name="id">Filter ID to delete</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSavedFilter(int id)
        {
            try
            {
                _logger.LogInformation("Deleting saved filter {FilterId}", id);

                await _savedFilterService.DeleteSavedFilterAsync(User, id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting saved filter {FilterId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific saved filter by ID
        /// </summary>
        /// <param name="id">Filter ID</param>
        /// <returns>Filter information or NotFound</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetSavedFilter(int id)
        {
            try
            {
                var result = await _savedFilterService.GetSavedFilterAsync(User, id);
                if (result == null)
                {
                    return NotFound(new { error = $"Saved filter with ID {id} not found or access denied" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved filter {FilterId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get saved filters for the current user with optional filtering and pagination
        /// </summary>
        /// <param name="request">Search and pagination parameters</param>
        /// <returns>Paginated list of saved filters</returns>
        [HttpGet]
        public async Task<ActionResult> GetSavedFilters([FromQuery] SavedFilterSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Getting saved filters for entity type: {EntityType}", request.EntityType);

                // Validate pagination parameters
                var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
                if (validationResult != null)
                {
                    return BadRequest(new { error = "Invalid pagination parameters" });
                }

                var result = await _savedFilterService.GetSavedFiltersAsync(User, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved filters");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Apply a saved filter and get the configured filter request object
        /// </summary>
        /// <param name="id">Filter ID to apply</param>
        /// <param name="pageIndex">Page index for pagination (default: 1)</param>
        /// <param name="pageSize">Page size for pagination (default: 10)</param>
        /// <returns>Configured filter request as JSON</returns>
        [HttpGet("{id}/apply")]
        public async Task<ActionResult> ApplySavedFilter(int id, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var savedFilter = await _savedFilterService.GetSavedFilterAsync(User, id);
                if (savedFilter == null)
                {
                    return NotFound(new { error = $"Saved filter with ID {id} not found or access denied" });
                }

                // Record usage
                await _savedFilterService.RecordFilterUsageAsync(User, id);

                _logger.LogInformation("Applied saved filter {FilterId} '{FilterName}' for entity {EntityType}", 
                    id, savedFilter.Name, savedFilter.EntityType);

                // Return the filter configuration for frontend use
                return Ok(new
                {
                    filterId = id,
                    name = savedFilter.Name,
                    entityType = savedFilter.EntityType,
                    isAdvancedSearch = savedFilter.IsAdvancedSearch,
                    searchCriteria = savedFilter.SearchCriteria,
                    searchText = savedFilter.SearchText,
                    orderBy = savedFilter.OrderBy,
                    ascending = savedFilter.Ascending,
                    pageIndex = pageIndex,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying saved filter {FilterId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get usage statistics for saved filters
        /// </summary>
        /// <param name="entityType">Optional entity type filter</param>
        /// <returns>Usage statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetFilterStatistics([FromQuery] string? entityType = null)
        {
            try
            {
                var searchRequest = new SavedFilterSearchRequest
                {
                    EntityType = entityType,
                    PageSize = int.MaxValue,
                    PageIndex = 1
                };

                var filters = await _savedFilterService.GetSavedFiltersAsync(User, searchRequest);

                var statistics = new
                {
                    totalFilters = filters.TotalCount,
                    filtersByEntityType = filters.Records?
                        .GroupBy(f => f.EntityType)
                        .ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<string, int>(),
                    mostUsedFilters = filters.Records?
                        .Where(f => f.UsageCount > 0)
                        .OrderByDescending(f => f.UsageCount)
                        .Take(5)
                        .Select(f => new { f.Id, f.Name, f.EntityType, f.UsageCount })
                        .ToList()
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filter statistics");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 