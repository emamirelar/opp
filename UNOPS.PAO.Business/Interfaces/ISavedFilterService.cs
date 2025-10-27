using System.Security.Claims;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Business.Interfaces
{
    /// <summary>
    /// Service interface for managing saved search filters
    /// </summary>
    public interface ISavedFilterService
    {
        /// <summary>
        /// Create a new saved filter for the current user
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="request">Filter creation request</param>
        /// <returns>Created filter model</returns>
        Task<SavedFilterModel> CreateSavedFilterAsync(ClaimsPrincipal user, CreateSavedFilterRequest request);

        /// <summary>
        /// Update an existing saved filter
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="request">Filter update request</param>
        /// <returns>Updated filter model</returns>
        Task<SavedFilterModel> UpdateSavedFilterAsync(ClaimsPrincipal user, UpdateSavedFilterRequest request);

        /// <summary>
        /// Delete a saved filter
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="filterId">ID of the filter to delete</param>
        Task DeleteSavedFilterAsync(ClaimsPrincipal user, int filterId);

        /// <summary>
        /// Get a specific saved filter by ID
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="filterId">Filter ID</param>
        /// <returns>Filter model or null if not found/accessible</returns>
        Task<SavedFilterModel?> GetSavedFilterAsync(ClaimsPrincipal user, int filterId);

        /// <summary>
        /// Get saved filters for the current user with optional filtering
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="searchRequest">Search and pagination parameters</param>
        /// <returns>Paginated list of saved filters</returns>
        Task<PaginationResponse<SavedFilterModel>> GetSavedFiltersAsync(ClaimsPrincipal user, SavedFilterSearchRequest searchRequest);

        /// <summary>
        /// Record that a filter has been used (increment usage count and update last used date)
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="filterId">Filter ID</param>
        Task RecordFilterUsageAsync(ClaimsPrincipal user, int filterId);

        /// <summary>
        /// Apply a saved filter to create a filter request object
        /// </summary>
        /// <typeparam name="TFilterRequest">Type of filter request to create</typeparam>
        /// <param name="user">Current user</param>
        /// <param name="filterId">Filter ID to apply</param>
        /// <param name="pageIndex">Page index for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Configured filter request object</returns>
        Task<TFilterRequest?> ApplySavedFilterAsync<TFilterRequest>(ClaimsPrincipal user, int filterId, int pageIndex = 1, int pageSize = 10) where TFilterRequest : new();
    }
} 