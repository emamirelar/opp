using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Business.Services
{
    public class OrganizationHierarchyService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private const string CACHE_KEY = "ORGANIZATION_HIERARCHY_CACHE";
        private const string CHILDREN_COUNT_CACHE_KEY = "ORGANIZATION_HIERARCHY_CHILDREN_COUNTS_CACHE";
        private const string ENTITY_RELATIONSHIP_COUNT_CACHE_KEY = "ORGANIZATION_HIERARCHY_ENTITY_RELATIONSHIP_COUNTS_CACHE";

        public OrganizationHierarchyService(
            AppDbContext context,
            IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets all organization hierarchies with optional filtering and pagination
        /// </summary>
        public async Task<PaginationResponse<OrganizationHierarchy>> GetOrganizationHierarchiesAsync(OrganizationHierarchyFilterRequest request)
        {
            var orgHierarchies = await GetAllOrganizationHierarchiesAsync();
            
            // Apply filters
            var filteredHierarchies = ApplyFilters(orgHierarchies, request);
            
            // Apply sorting
            filteredHierarchies = ApplySorting(filteredHierarchies, request.OrderBy, request.Ascending ?? true);
            
            // Get total count before pagination
            var totalCount = filteredHierarchies.Count();
            
            // Apply pagination
            var pagedHierarchies = filteredHierarchies
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Populate counts if requested
            if (request.IncludeCounts)
            {
                await PopulateCountsAsync(pagedHierarchies);
            }

            return new PaginationResponse<OrganizationHierarchy>
            {
                Records = pagedHierarchies,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Searches organization hierarchies based on search criteria
        /// </summary>
        public async Task<PaginationResponse<OrganizationHierarchy>> SearchOrganizationHierarchiesAsync(OrganizationHierarchySearchRequest request)
        {
            var orgHierarchies = await GetAllOrganizationHierarchiesAsync();
            
            // Apply search filters
            var filteredHierarchies = ApplySearchFilters(orgHierarchies, request);
            
            // Apply sorting
            filteredHierarchies = ApplySorting(filteredHierarchies, request.OrderBy, request.Ascending);
            
            // Get total count before pagination
            var totalCount = filteredHierarchies.Count();
            
            // Apply pagination
            var pagedHierarchies = filteredHierarchies
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Populate counts
            await PopulateCountsAsync(pagedHierarchies);

            return new PaginationResponse<OrganizationHierarchy>
            {
                Records = pagedHierarchies,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Gets a specific organization hierarchy by ID
        /// </summary>
        public async Task<OrganizationHierarchy?> GetOrganizationHierarchyByIdAsync(int id)
        {
            var orgHierarchies = await GetAllOrganizationHierarchiesAsync();
            var hierarchy = orgHierarchies.FirstOrDefault(oh => oh.Id == id);
            
            if (hierarchy != null)
            {
                await PopulateCountsAsync(new List<OrganizationHierarchy> { hierarchy });
            }
            
            return hierarchy;
        }

        private async Task<List<OrganizationHierarchy>> GetAllOrganizationHierarchiesAsync()
        {
            return await _memoryCache.GetOrCreateAsync(CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                
                var orgHierarchies = await _context.OrganizationHierarchies
                    .Where(oh => !oh.IsDeleted && oh.Status == EntityStatus.Active)
                    .Include(oh => oh.Parent)
                    .ToListAsync();

                // Populate parent information
                foreach (var hierarchy in orgHierarchies)
                {
                    if (hierarchy.Parent != null)
                    {
                        hierarchy.ParentName = hierarchy.Parent.Name;
                        hierarchy.ParentCode = hierarchy.Parent.Code;
                    }
                }
                
                return orgHierarchies;
            });
        }

        private async Task PopulateCountsAsync(List<OrganizationHierarchy> orgHierarchies)
        {
            var childrenCounts = await _memoryCache.GetOrCreateAsync(CHILDREN_COUNT_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                
                // Get children counts by parent ID
                var counts = await _context.OrganizationHierarchies
                    .Where(oh => !oh.IsDeleted && oh.ParentId.HasValue)
                    .GroupBy(oh => oh.ParentId.Value)
                    .Select(g => new { ParentId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.ParentId, x => x.Count);
                
                return counts;
            });

            var entityRelationshipCounts = await _memoryCache.GetOrCreateAsync(ENTITY_RELATIONSHIP_COUNT_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                
                // Get entity relationship counts by organization hierarchy ID
                var counts = await _context.OrganizationUnitRelationships
                    .GroupBy(our => our.OrganizationHierarchyId)
                    .Select(g => new { OrganizationHierarchyId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.OrganizationHierarchyId, x => x.Count);
                
                return counts;
            });

            foreach (var hierarchy in orgHierarchies)
            {
                hierarchy.ChildrenCount = childrenCounts.GetValueOrDefault(hierarchy.Id, 0);
                hierarchy.EntityRelationshipCount = entityRelationshipCounts.GetValueOrDefault(hierarchy.Id, 0);
            }
        }

        private IEnumerable<OrganizationHierarchy> ApplyFilters(List<OrganizationHierarchy> orgHierarchies, OrganizationHierarchyFilterRequest request)
        {
            var filtered = orgHierarchies.AsEnumerable();

            if (!string.IsNullOrEmpty(request.Name))
                filtered = filtered.Where(oh => oh.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Code))
                filtered = filtered.Where(oh => oh.Code.Contains(request.Code, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Type))
                filtered = filtered.Where(oh => oh.Type.ToString().Equals(request.Type, StringComparison.OrdinalIgnoreCase));

            if (request.ParentId.HasValue)
                filtered = filtered.Where(oh => oh.ParentId == request.ParentId.Value);

            if (!string.IsNullOrEmpty(request.ParentCode))
                filtered = filtered.Where(oh => !string.IsNullOrEmpty(oh.ParentCode) && oh.ParentCode.Contains(request.ParentCode, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                filtered = filtered.Where(oh => oh.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (request.IsSelfManagementEnabled.HasValue)
                filtered = filtered.Where(oh => oh.IsSelfManagementEnabled == request.IsSelfManagementEnabled.Value);

            return filtered;
        }

        private IEnumerable<OrganizationHierarchy> ApplySearchFilters(List<OrganizationHierarchy> orgHierarchies, OrganizationHierarchySearchRequest request)
        {
            var filtered = orgHierarchies.AsEnumerable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                filtered = filtered.Where(oh => 
                    oh.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    oh.Code.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(oh.Description) && oh.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(request.Type))
                filtered = filtered.Where(oh => oh.Type.ToString().Equals(request.Type, StringComparison.OrdinalIgnoreCase));

            if (request.ParentId.HasValue)
                filtered = filtered.Where(oh => oh.ParentId == request.ParentId.Value);

            if (!string.IsNullOrEmpty(request.Status))
                filtered = filtered.Where(oh => oh.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (request.IsSelfManagementEnabled.HasValue)
                filtered = filtered.Where(oh => oh.IsSelfManagementEnabled == request.IsSelfManagementEnabled.Value);

            return filtered;
        }

        private IEnumerable<OrganizationHierarchy> ApplySorting(IEnumerable<OrganizationHierarchy> orgHierarchies, string? orderBy, bool ascending)
        {
            return orderBy?.ToLower() switch
            {
                "name" => ascending ? orgHierarchies.OrderBy(oh => oh.Name) : orgHierarchies.OrderByDescending(oh => oh.Name),
                "code" => ascending ? orgHierarchies.OrderBy(oh => oh.Code) : orgHierarchies.OrderByDescending(oh => oh.Code),
                "type" => ascending ? orgHierarchies.OrderBy(oh => oh.Type) : orgHierarchies.OrderByDescending(oh => oh.Type),
                "parentname" => ascending ? orgHierarchies.OrderBy(oh => oh.ParentName) : orgHierarchies.OrderByDescending(oh => oh.ParentName),
                "status" => ascending ? orgHierarchies.OrderBy(oh => oh.Status) : orgHierarchies.OrderByDescending(oh => oh.Status),
                "childrencount" => ascending ? orgHierarchies.OrderBy(oh => oh.ChildrenCount) : orgHierarchies.OrderByDescending(oh => oh.ChildrenCount),
                "entityrelationshipcount" => ascending ? orgHierarchies.OrderBy(oh => oh.EntityRelationshipCount) : orgHierarchies.OrderByDescending(oh => oh.EntityRelationshipCount),
                _ => ascending ? orgHierarchies.OrderBy(oh => oh.Name) : orgHierarchies.OrderByDescending(oh => oh.Name)
            };
        }
    }
}
