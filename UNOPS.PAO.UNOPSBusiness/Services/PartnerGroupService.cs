using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PartnerGroupService
    {
        private readonly DataRepository<UNOPSPartnerTree> _partnerTreeRepository;
        private readonly DataRepository<UNOPSPartner> _partnerRepository;
        private readonly UNOPSAppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private const string CACHE_KEY = "PARTNER_GROUPS_CACHE";
        private const string PARTNER_COUNT_CACHE_KEY = "PARTNER_GROUP_COUNTS_CACHE";

        public PartnerGroupService(
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
        /// Gets all partner groups with optional filtering and pagination
        /// </summary>
        public async Task<PaginationResponse<PartnerGroup>> GetPartnerGroupsAsync(PartnerGroupFilterRequest request)
        {
            var groups = await GetAllPartnerGroupsAsync();
            
            // Apply filters
            var filteredGroups = ApplyFilters(groups, request);
            
            // Apply sorting
            filteredGroups = ApplySorting(filteredGroups, request.OrderBy, request.Ascending ?? true);
            
            // Get total count before pagination
            var totalCount = filteredGroups.Count();
            
            // Apply pagination
            var pagedGroups = filteredGroups
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            
            // Populate partner counts if requested
            if (request.IncludePartnerCounts)
            {
                await PopulatePartnerCountsAsync(pagedGroups);
            }
            
            // Populate partner category if requested
            if (request.IncludePartnerCategory)
            {
                await PopulatePartnerCategoriesAsync(pagedGroups);
            }
            
            // Populate partners if requested
            if (request.IncludePartners)
            {
                await PopulatePartnersAsync(pagedGroups);
            }
            
            return new PaginationResponse<PartnerGroup>
            {
                Records = pagedGroups,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Searches partner groups based on search criteria
        /// </summary>
        public async Task<PaginationResponse<PartnerGroup>> SearchPartnerGroupsAsync(PartnerGroupSearchRequest request)
        {
            var groups = await GetAllPartnerGroupsAsync();
            
            // Apply search filters
            var filteredGroups = ApplySearchFilters(groups, request);
            
            // Apply sorting
            filteredGroups = ApplySorting(filteredGroups, request.OrderBy, request.Ascending);
            
            // Get total count before pagination
            var totalCount = filteredGroups.Count();
            
            // Apply pagination
            var pagedGroups = filteredGroups
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            
            // Populate partner counts
            await PopulatePartnerCountsAsync(pagedGroups);
            
            // Populate partner category if requested
            if (request.IncludePartnerCategory)
            {
                await PopulatePartnerCategoriesAsync(pagedGroups);
            }
            
            // Populate partners if requested
            if (request.IncludePartners)
            {
                await PopulatePartnersAsync(pagedGroups);
            }
            
            return new PaginationResponse<PartnerGroup>
            {
                Records = pagedGroups,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Gets a specific partner group by ID
        /// </summary>
        public async Task<PartnerGroup?> GetPartnerGroupByIdAsync(int id)
        {
            var groups = await GetAllPartnerGroupsAsync();
            var group = groups.FirstOrDefault(g => g.Id == id);
            
            if (group != null)
            {
                await PopulatePartnerCountsAsync(new[] { group });
                await PopulatePartnerCategoriesAsync(new[] { group });
                await PopulatePartnersAsync(new[] { group });
            }
            
            return group;
        }

        /// <summary>
        /// Gets a specific partner group by code
        /// </summary>
        public async Task<PartnerGroup?> GetPartnerGroupByCodeAsync(string code)
        {
            var groups = await GetAllPartnerGroupsAsync();
            var group = groups.FirstOrDefault(g => g.Code == code || g.PartnerGroupCode == code);
            
            if (group != null)
            {
                await PopulatePartnerCountsAsync(new[] { group });
                await PopulatePartnerCategoriesAsync(new[] { group });
                await PopulatePartnersAsync(new[] { group });
            }
            
            return group;
        }

        /// <summary>
        /// Gets partner groups by category ID
        /// </summary>
        public async Task<IEnumerable<PartnerGroup>> GetPartnerGroupsByCategoryIdAsync(int categoryId)
        {
            var groups = await GetAllPartnerGroupsAsync();
            var categoryGroups = groups.Where(g => g.PartnerCategoryId == categoryId).ToList();
            
            await PopulatePartnerCountsAsync(categoryGroups);
            
            return categoryGroups;
        }

        /// <summary>
        /// Gets all partner groups from cache or database
        /// </summary>
        private async Task<IEnumerable<PartnerGroup>> GetAllPartnerGroupsAsync()
        {
            if (!_memoryCache.TryGetValue(CACHE_KEY, out IEnumerable<PartnerGroup>? groups))
            {
                var allPartnerTrees = await _partnerTreeRepository.GetAllSortedAsync("Type");
                var activePartnerTrees = allPartnerTrees.Where(pt => !pt.IsDeleted).ToList();
                
                var partnerGroups = new List<PartnerGroup>();
                
                foreach (var partnerTree in activePartnerTrees)
                {
                    if (PartnerGroup.IsPartnerGroup(partnerTree, activePartnerTrees))
                    {
                        // Find the parent category for this group
                        var parentCategory = PartnerGroup.FindParentCategory(partnerTree, activePartnerTrees);
                        var partnerGroup = PartnerGroup.FromPartnerTree(partnerTree, activePartnerTrees, parentCategory);
                        partnerGroups.Add(partnerGroup);
                    }
                }
                
                groups = partnerGroups;
                
                // Set cache options
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(2));

                _memoryCache.Set(CACHE_KEY, groups, cacheOptions);
            }

            return groups ?? new List<PartnerGroup>();
        }

        /// <summary>
        /// Applies filters to the groups collection
        /// </summary>
        private IEnumerable<PartnerGroup> ApplyFilters(IEnumerable<PartnerGroup> groups, PartnerGroupFilterRequest request)
        {
            var query = groups.AsQueryable();

            if (!string.IsNullOrEmpty(request.Name))
                query = query.Where(g => g.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Code))
                query = query.Where(g => g.Code.Contains(request.Code, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(g => g.Type == request.Type);

            if (!string.IsNullOrEmpty(request.Parent))
                query = query.Where(g => g.Parent == request.Parent);

            if (!string.IsNullOrEmpty(request.PartnerGroupCode))
                query = query.Where(g => g.PartnerGroupCode.Contains(request.PartnerGroupCode, StringComparison.OrdinalIgnoreCase));

            if (request.PartnerCategoryId.HasValue)
                query = query.Where(g => g.PartnerCategoryId == request.PartnerCategoryId.Value);

            if (!string.IsNullOrEmpty(request.PartnerCategoryCode))
                query = query.Where(g => g.PartnerCategoryCode != null && g.PartnerCategoryCode.Contains(request.PartnerCategoryCode, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(g => g.Status.ToString() == request.Status);

            return query;
        }

        /// <summary>
        /// Applies search filters to the groups collection
        /// </summary>
        private IEnumerable<PartnerGroup> ApplySearchFilters(IEnumerable<PartnerGroup> groups, PartnerGroupSearchRequest request)
        {
            var query = groups.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(g => 
                    g.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    g.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    g.Code.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    g.PartnerGroupCode.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (g.PartnerCategoryName != null && g.PartnerCategoryName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(g => g.Type == request.Type);

            if (request.PartnerCategoryId.HasValue)
                query = query.Where(g => g.PartnerCategoryId == request.PartnerCategoryId.Value);

            if (!string.IsNullOrEmpty(request.PartnerCategoryCode))
                query = query.Where(g => g.PartnerCategoryCode != null && g.PartnerCategoryCode.Contains(request.PartnerCategoryCode, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(g => g.Status.ToString() == request.Status);

            if (request.MinPartnerCount.HasValue)
                query = query.Where(g => g.TotalPartnerCount >= request.MinPartnerCount.Value);

            if (request.MaxPartnerCount.HasValue)
                query = query.Where(g => g.TotalPartnerCount <= request.MaxPartnerCount.Value);

            return query;
        }

        /// <summary>
        /// Applies sorting to the groups collection
        /// </summary>
        private IEnumerable<PartnerGroup> ApplySorting(IEnumerable<PartnerGroup> groups, string? orderBy, bool ascending)
        {
            if (string.IsNullOrEmpty(orderBy))
                orderBy = "Name";

            var query = groups.AsQueryable();

            return orderBy.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(g => g.Name) : query.OrderByDescending(g => g.Name),
                "code" => ascending ? query.OrderBy(g => g.Code) : query.OrderByDescending(g => g.Code),
                "type" => ascending ? query.OrderBy(g => g.Type) : query.OrderByDescending(g => g.Type),
                "partnergroupcode" => ascending ? query.OrderBy(g => g.PartnerGroupCode) : query.OrderByDescending(g => g.PartnerGroupCode),
                "partnercategoryname" => ascending ? query.OrderBy(g => g.PartnerCategoryName ?? "") : query.OrderByDescending(g => g.PartnerCategoryName ?? ""),
                "status" => ascending ? query.OrderBy(g => g.Status) : query.OrderByDescending(g => g.Status),
                "partnercount" => ascending ? query.OrderBy(g => g.TotalPartnerCount) : query.OrderByDescending(g => g.TotalPartnerCount),
                _ => ascending ? query.OrderBy(g => g.Name) : query.OrderByDescending(g => g.Name)
            };
        }

        /// <summary>
        /// Populates partner counts for groups
        /// </summary>
        private async Task PopulatePartnerCountsAsync(IEnumerable<PartnerGroup> groups)
        {
            var partnerCounts = await GetPartnerCountsAsync();
            
            foreach (var group in groups)
            {
                if (partnerCounts.TryGetValue(group.Id, out var count))
                {
                    group.TotalPartnerCount = count;
                }
            }
        }

        /// <summary>
        /// Gets partner counts for all groups (cached)
        /// </summary>
        private async Task<Dictionary<int, int>> GetPartnerCountsAsync()
        {
            if (!_memoryCache.TryGetValue(PARTNER_COUNT_CACHE_KEY, out Dictionary<int, int>? partnerCounts))
            {
                partnerCounts = new Dictionary<int, int>();

                // Get direct partner counts for each group
                var groupPartnerCounts = await _context.Partners
                    .Where(p => !p.IsDeleted && p.PartnerGroupId.HasValue)
                    .GroupBy(p => p.PartnerGroupId.Value)
                    .Select(g => new { GroupId = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var groupCount in groupPartnerCounts)
                {
                    partnerCounts[groupCount.GroupId] = groupCount.Count;
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
        /// Populates partner categories for groups
        /// </summary>
        private async Task PopulatePartnerCategoriesAsync(IEnumerable<PartnerGroup> groups)
        {
            var allPartnerTrees = await _partnerTreeRepository.GetAllSortedAsync("Type");
            var activePartnerTrees = allPartnerTrees.Where(pt => !pt.IsDeleted).ToList();

            foreach (var group in groups.Where(g => g.PartnerCategoryId.HasValue))
            {
                var categoryTree = activePartnerTrees.FirstOrDefault(pt => pt.Id == group.PartnerCategoryId.Value);
                if (categoryTree != null && PartnerCategory.IsPartnerCategory(categoryTree))
                {
                    group.PartnerCategory = PartnerCategory.FromPartnerTree(categoryTree);
                }
            }
        }

        /// <summary>
        /// Populates partners for groups
        /// </summary>
        private async Task PopulatePartnersAsync(IEnumerable<PartnerGroup> groups)
        {
            var groupIds = groups.Select(g => g.Id).ToList();
            
            var partners = await _context.Partners
                .Where(p => !p.IsDeleted && p.PartnerGroupId.HasValue && groupIds.Contains(p.PartnerGroupId.Value))
                .ToListAsync();

            foreach (var group in groups)
            {
                group.Partners = partners.Where(p => p.PartnerGroupId == group.Id).Cast<Partner>().ToList();
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
