using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using System.Text.Json;
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Business.Services
{
    /// <summary>
    /// Service for managing saved search filters
    /// </summary>
    public class SavedFilterService : ISavedFilterService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SavedFilterService> _logger;

        public SavedFilterService(
            AppDbContext context,
            IMapper mapper,
            ILogger<SavedFilterService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SavedFilterModel> CreateSavedFilterAsync(ClaimsPrincipal user, CreateSavedFilterRequest request)
        {
            var userId = GetUserId(user);
            _logger.LogInformation("Creating saved filter for user {UserId}, entity type: {EntityType}", userId, request.EntityType);

            // Validate search criteria if it's an advanced search
            if (request.IsAdvancedSearch && !string.IsNullOrEmpty(request.SearchCriteria))
            {
                try
                {
                    // Basic JSON validation
                    JsonDocument.Parse(request.SearchCriteria);
                }
                catch (JsonException)
                {
                    throw new BusinessException("Invalid search criteria JSON format");
                }
            }

            var entity = new SavedFilter
            {
                Name = request.Name,
                Description = request.Description,
                EntityType = request.EntityType,
                UserId = userId,
                SearchCriteria = request.SearchCriteria,
                SearchText = request.SearchText,
                IsAdvancedSearch = request.IsAdvancedSearch,
                OrderByField = request.OrderBy,
                Ascending = request.Ascending,
                UsageCount = 0
            };

            // Set audit data
            entity.SetCreateAuditData(GetUserIdAsInt(user));
            entity.SetUpdateAuditData(GetUserIdAsInt(user));

            _context.SavedFilters.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created saved filter with ID {FilterId}", entity.Id);
            return _mapper.Map<SavedFilterModel>(entity);
        }

        public async Task<SavedFilterModel> UpdateSavedFilterAsync(ClaimsPrincipal user, UpdateSavedFilterRequest request)
        {
            var userId = GetUserId(user);
            _logger.LogInformation("Updating saved filter {FilterId} for user {UserId}", request.Id, userId);

            var entity = await _context.SavedFilters
                .FirstOrDefaultAsync(f => f.Id == request.Id && f.UserId == userId);

            if (entity == null)
            {
                throw new BusinessException($"Saved filter with ID {request.Id} not found or access denied");
            }

            // Validate search criteria if it's an advanced search
            if (request.IsAdvancedSearch && !string.IsNullOrEmpty(request.SearchCriteria))
            {
                try
                {
                    // Basic JSON validation
                    JsonDocument.Parse(request.SearchCriteria);
                }
                catch (JsonException)
                {
                    throw new BusinessException("Invalid search criteria JSON format");
                }
            }

            // Update entity properties
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.SearchCriteria = request.SearchCriteria;
            entity.SearchText = request.SearchText;
            entity.IsAdvancedSearch = request.IsAdvancedSearch;
            entity.OrderByField = request.OrderBy;
            entity.Ascending = request.Ascending;
            entity.SetUpdateAuditData(GetUserIdAsInt(user));

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated saved filter {FilterId}", entity.Id);
            return _mapper.Map<SavedFilterModel>(entity);
        }

        public async Task DeleteSavedFilterAsync(ClaimsPrincipal user, int filterId)
        {
            var userId = GetUserId(user);
            _logger.LogInformation("Deleting saved filter {FilterId} for user {UserId}", filterId, userId);

            var entity = await _context.SavedFilters
                .FirstOrDefaultAsync(f => f.Id == filterId && f.UserId == userId);

            if (entity == null)
            {
                throw new BusinessException($"Saved filter with ID {filterId} not found or access denied");
            }

            _context.SavedFilters.Remove(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted saved filter {FilterId}", filterId);
        }

        public async Task<SavedFilterModel?> GetSavedFilterAsync(ClaimsPrincipal user, int filterId)
        {
            var userId = GetUserId(user);

            var entity = await _context.SavedFilters
                .FirstOrDefaultAsync(f => f.Id == filterId && f.UserId == userId);

            return entity != null ? _mapper.Map<SavedFilterModel>(entity) : null;
        }

        public async Task<PaginationResponse<SavedFilterModel>> GetSavedFiltersAsync(ClaimsPrincipal user, SavedFilterSearchRequest searchRequest)
        {
            var userId = GetUserId(user);
            _logger.LogInformation("Getting saved filters for user {UserId}, entity type: {EntityType}", 
                userId, searchRequest.EntityType);

            var query = _context.SavedFilters.AsQueryable();

            // Filter by user's own filters only
            query = query.Where(f => f.UserId == userId);

            // Filter by entity type if specified
            if (!string.IsNullOrEmpty(searchRequest.EntityType))
            {
                query = query.Where(f => f.EntityType == searchRequest.EntityType);
            }

            // Filter by search text
            if (!string.IsNullOrEmpty(searchRequest.SearchText))
            {
                var searchTerm = searchRequest.SearchText.ToLower();
                query = query.Where(f => f.Name.ToLower().Contains(searchTerm) || 
                                        (f.Description != null && f.Description.ToLower().Contains(searchTerm)));
            }

            // Order by usage count (most used first), then by last used date, then by name
            query = query.OrderByDescending(f => f.UsageCount)
                         .ThenByDescending(f => f.LastUsedDate)
                         .ThenBy(f => f.Name);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((searchRequest.PageIndex - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ToListAsync();

            var models = _mapper.Map<List<SavedFilterModel>>(items);

            return new PaginationResponse<SavedFilterModel>
            {
                Records = models,
                TotalCount = totalCount,
                PageIndex = searchRequest.PageIndex,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
            };
        }

        public async Task RecordFilterUsageAsync(ClaimsPrincipal user, int filterId)
        {
            var userId = GetUserId(user);

            var entity = await _context.SavedFilters
                .FirstOrDefaultAsync(f => f.Id == filterId && f.UserId == userId);

            if (entity != null)
            {
                entity.UsageCount++;
                entity.LastUsedDate = DateTime.UtcNow;
                entity.SetUpdateAuditData(GetUserIdAsInt(user));

                await _context.SaveChangesAsync();
                _logger.LogDebug("Recorded usage for filter {FilterId}", filterId);
            }
        }

        public async Task<TFilterRequest?> ApplySavedFilterAsync<TFilterRequest>(ClaimsPrincipal user, int filterId, int pageIndex = 1, int pageSize = 10) 
            where TFilterRequest : new()
        {
            var savedFilter = await GetSavedFilterAsync(user, filterId);
            if (savedFilter == null)
            {
                return default;
            }

            // Record usage
            await RecordFilterUsageAsync(user, filterId);

            // Create filter request using reflection
            var filterRequest = new TFilterRequest();
            SetPropertyIfExists(filterRequest, "AdvancedSearch", savedFilter.IsAdvancedSearch);
            SetPropertyIfExists(filterRequest, "SearchCriteria", savedFilter.SearchCriteria);
            SetPropertyIfExists(filterRequest, "SearchText", savedFilter.SearchText);
            SetPropertyIfExists(filterRequest, "PageIndex", pageIndex);
            SetPropertyIfExists(filterRequest, "PageSize", pageSize);
            SetPropertyIfExists(filterRequest, "OrderBy", savedFilter.OrderBy);
            SetPropertyIfExists(filterRequest, "Ascending", savedFilter.Ascending);

            _logger.LogInformation("Applied saved filter {FilterId} '{FilterName}' for entity {EntityType}", 
                filterId, savedFilter.Name, savedFilter.EntityType);

            return filterRequest;
        }

        private static string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier) 
                   ?? user.FindFirstValue("preferred_username") 
                   ?? user.Identity?.Name 
                   ?? throw new BusinessException("User ID not found in claims");
        }

        private static int GetUserIdAsInt(ClaimsPrincipal user)
        {
            var userIdString = GetUserId(user);
            if (int.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            
            // For non-numeric user IDs, use a hash
            return userIdString.GetHashCode();
        }

        private static void SetPropertyIfExists<T>(T obj, string propertyName, object? value)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property != null && property.CanWrite && value != null)
            {
                property.SetValue(obj, value);
            }
        }
    }
} 