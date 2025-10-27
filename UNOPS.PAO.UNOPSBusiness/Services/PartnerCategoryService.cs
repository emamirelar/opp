using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PartnerCategoryService
    {
        private readonly DataRepository<UNOPSPartnerTree> _partnerTreeRepository;
        private readonly DataRepository<UNOPSPartner> _partnerRepository;
        private readonly UNOPSAppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private const string CACHE_KEY = "PARTNER_CATEGORIES_CACHE";
        private const string PARTNER_COUNT_CACHE_KEY = "PARTNER_CATEGORY_COUNTS_CACHE";

        public PartnerCategoryService(
            DataRepository<UNOPSPartnerTree> partnerTreeRepository,
            DataRepository<UNOPSPartner> partnerRepository,
            UNOPSAppDbContext context,
            IMemoryCache memoryCache)
        {
            _partnerTreeRepository = partnerTreeRepository;
            _partnerRepository = partnerRepository;
            _context = context;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets all partner categories with optional filtering and pagination
        /// </summary>
        public async Task<PaginationResponse<PartnerCategory>> GetPartnerCategoriesAsync(PartnerCategoryFilterRequest request)
        {
            var categories = await GetAllPartnerCategoriesAsync();
            
            // Apply filters
            var filteredCategories = ApplyFilters(categories, request);
            
            // Apply sorting
            filteredCategories = ApplySorting(filteredCategories, request.OrderBy, request.Ascending ?? true);
            
            // Get total count before pagination
            var totalCount = filteredCategories.Count();
            
            // Apply pagination
            var pagedCategories = filteredCategories
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            
            // Populate partner counts if requested
            if (request.IncludePartnerCounts)
            {
                await PopulatePartnerCountsAsync(pagedCategories);
            }
            
            // Populate partner groups if requested
            if (request.IncludePartnerGroups)
            {
                await PopulatePartnerGroupsAsync(pagedCategories);
            }
            
            return new PaginationResponse<PartnerCategory>
            {
                Records = pagedCategories,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Searches partner categories based on search criteria
        /// </summary>
        public async Task<PaginationResponse<PartnerCategory>> SearchPartnerCategoriesAsync(PartnerCategorySearchRequest request)
        {
            var categories = await GetAllPartnerCategoriesAsync();
            
            // Apply search filters
            var filteredCategories = ApplySearchFilters(categories, request);
            
            // Apply sorting
            filteredCategories = ApplySorting(filteredCategories, request.OrderBy, request.Ascending);
            
            // Get total count before pagination
            var totalCount = filteredCategories.Count();
            
            // Apply pagination
            var pagedCategories = filteredCategories
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            
            // Populate partner counts
            await PopulatePartnerCountsAsync(pagedCategories);
            
            // Populate partner groups if requested
            if (request.IncludePartnerGroups)
            {
                await PopulatePartnerGroupsAsync(pagedCategories);
            }
            
            return new PaginationResponse<PartnerCategory>
            {
                Records = pagedCategories,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Gets a specific partner category by ID
        /// </summary>
        public async Task<PartnerCategory?> GetPartnerCategoryByIdAsync(int id)
        {
            var categories = await GetAllPartnerCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == id);
            
            if (category != null)
            {
                await PopulatePartnerCountsAsync(new[] { category });
                await PopulatePartnerGroupsAsync(new[] { category });
            }
            
            return category;
        }

        /// <summary>
        /// Gets a specific partner category by code
        /// </summary>
        public async Task<PartnerCategory?> GetPartnerCategoryByCodeAsync(string code)
        {
            var categories = await GetAllPartnerCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Code == code || c.PartnerCategoryCode == code);
            
            if (category != null)
            {
                await PopulatePartnerCountsAsync(new[] { category });
                await PopulatePartnerGroupsAsync(new[] { category });
            }
            
            return category;
        }

        /// <summary>
        /// Gets all partner categories from cache or database
        /// </summary>
        private async Task<IEnumerable<PartnerCategory>> GetAllPartnerCategoriesAsync()
        {
            if (!_memoryCache.TryGetValue(CACHE_KEY, out IEnumerable<PartnerCategory>? categories))
            {
                var allPartnerTrees = await _partnerTreeRepository.GetAllSortedAsync("Type");
                var activePartnerTrees = allPartnerTrees.Where(pt => !pt.IsDeleted);
                
                categories = activePartnerTrees
                    .Where(pt => PartnerCategory.IsPartnerCategory(pt))
                    .Select(pt => PartnerCategory.FromPartnerTree(pt))
                    .ToList();
                
                // Set cache options
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(2));

                _memoryCache.Set(CACHE_KEY, categories, cacheOptions);
            }

            return categories ?? new List<PartnerCategory>();
        }

        /// <summary>
        /// Applies filters to the categories collection
        /// </summary>
        private IEnumerable<PartnerCategory> ApplyFilters(IEnumerable<PartnerCategory> categories, PartnerCategoryFilterRequest request)
        {
            var query = categories.AsQueryable();

            if (!string.IsNullOrEmpty(request.Name))
                query = query.Where(c => c.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Code))
                query = query.Where(c => c.Code.Contains(request.Code, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(c => c.Type == request.Type);

            if (!string.IsNullOrEmpty(request.Parent))
                query = query.Where(c => c.Parent == request.Parent);

            if (!string.IsNullOrEmpty(request.PartnerCategoryCode))
                query = query.Where(c => c.PartnerCategoryCode.Contains(request.PartnerCategoryCode, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            return query;
        }

        /// <summary>
        /// Applies search filters to the categories collection
        /// </summary>
        private IEnumerable<PartnerCategory> ApplySearchFilters(IEnumerable<PartnerCategory> categories, PartnerCategorySearchRequest request)
        {
            var query = categories.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(c => 
                    c.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.PartnerCategoryCode.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(c => c.Type == request.Type);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(c => c.Status.ToString() == request.Status);

            return query;
        }

        /// <summary>
        /// Applies sorting to the categories collection
        /// </summary>
        private IEnumerable<PartnerCategory> ApplySorting(IEnumerable<PartnerCategory> categories, string? orderBy, bool ascending)
        {
            if (string.IsNullOrEmpty(orderBy))
                orderBy = "Name";

            var query = categories.AsQueryable();

            return orderBy.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "code" => ascending ? query.OrderBy(c => c.Code) : query.OrderByDescending(c => c.Code),
                "type" => ascending ? query.OrderBy(c => c.Type) : query.OrderByDescending(c => c.Type),
                "partnercategorycode" => ascending ? query.OrderBy(c => c.PartnerCategoryCode) : query.OrderByDescending(c => c.PartnerCategoryCode),
                "status" => ascending ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
                "partnercount" => ascending ? query.OrderBy(c => c.TotalPartnerCount) : query.OrderByDescending(c => c.TotalPartnerCount),
                _ => ascending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name)
            };
        }

        /// <summary>
        /// Populates partner counts for categories
        /// </summary>
        private async Task PopulatePartnerCountsAsync(IEnumerable<PartnerCategory> categories)
        {
            var partnerCounts = await GetPartnerCountsAsync();
            
            foreach (var category in categories)
            {
                if (partnerCounts.TryGetValue(category.Id, out var count))
                {
                    category.TotalPartnerCount = count;
                }
            }
        }

        /// <summary>
        /// Gets partner counts for all categories (cached)
        /// </summary>
        private async Task<Dictionary<int, int>> GetPartnerCountsAsync()
        {
            if (!_memoryCache.TryGetValue(PARTNER_COUNT_CACHE_KEY, out Dictionary<int, int>? partnerCounts))
            {
                // Get all partner groups and their categories
                var partnerGroups = await _context.PartnerTrees
                    .Where(pt => !pt.IsDeleted)
                    .ToListAsync();

                var categories = await GetAllPartnerCategoriesAsync();
                partnerCounts = new Dictionary<int, int>();

                foreach (var category in categories)
                {
                    // Get all descendant groups for this category
                    var descendantGroupIds = await GetDescendantGroupIds(category.Code, partnerGroups);
                    
                    // Count partners in these groups
                    var partnerCount = await _context.Partners
                        .Where(p => !p.IsDeleted && p.PartnerGroupId.HasValue && descendantGroupIds.Contains(p.PartnerGroupId.Value))
                        .CountAsync();
                    
                    partnerCounts[category.Id] = partnerCount;
                }

                // Cache for 15 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

                _memoryCache.Set(PARTNER_COUNT_CACHE_KEY, partnerCounts, cacheOptions);
            }

            return partnerCounts ?? new Dictionary<int, int>();
        }

        /// <summary>
        /// Gets all descendant group IDs for a category
        /// </summary>
        private async Task<List<int>> GetDescendantGroupIds(string categoryCode, List<UNOPSPartnerTree> allPartnerTrees)
        {
            var descendants = new List<int>();
            await GetDescendantsRecursive(categoryCode, allPartnerTrees, descendants);
            return descendants;
        }

        /// <summary>
        /// Recursively gets descendant IDs
        /// </summary>
        private async Task GetDescendantsRecursive(string parentCode, List<UNOPSPartnerTree> allPartnerTrees, List<int> descendants)
        {
            var children = allPartnerTrees.Where(pt => pt.Parent == parentCode && !pt.IsDeleted).ToList();
            
            foreach (var child in children)
            {
                descendants.Add(child.Id);
                await GetDescendantsRecursive(child.Code, allPartnerTrees, descendants);
            }
        }

        /// <summary>
        /// Populates partner groups for categories
        /// </summary>
        private async Task PopulatePartnerGroupsAsync(IEnumerable<PartnerCategory> categories)
        {
            var allPartnerTrees = await _partnerTreeRepository.GetAllSortedAsync("Type");
            var activePartnerTrees = allPartnerTrees.Where(pt => !pt.IsDeleted).ToList();

            foreach (var category in categories)
            {
                var partnerGroups = new List<PartnerGroup>();
                await GetPartnerGroupsForCategory(category.Code, activePartnerTrees, partnerGroups, category);
                category.PartnerGroups = partnerGroups;
            }
        }

        /// <summary>
        /// Gets partner groups for a specific category
        /// </summary>
        private async Task GetPartnerGroupsForCategory(string categoryCode, List<UNOPSPartnerTree> allPartnerTrees, List<PartnerGroup> partnerGroups, PartnerCategory category)
        {
            var children = allPartnerTrees.Where(pt => pt.Parent == categoryCode && !pt.IsDeleted).ToList();
            
            foreach (var child in children)
            {
                if (PartnerGroup.IsPartnerGroup(child, allPartnerTrees))
                {
                    var partnerGroup = PartnerGroup.FromPartnerTree(child, allPartnerTrees, category);
                    partnerGroups.Add(partnerGroup);
                }
                
                // Recursively get child groups
                await GetPartnerGroupsForCategory(child.Code, allPartnerTrees, partnerGroups, category);
            }
        }

        /// <summary>
        /// Invalidates the cache
        /// </summary>
        public void InvalidateCache()
        {
            _memoryCache.Remove(CACHE_KEY);
            _memoryCache.Remove(PARTNER_COUNT_CACHE_KEY);
        }
    }
}
