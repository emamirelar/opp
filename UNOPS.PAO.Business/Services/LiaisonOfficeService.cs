using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Models.LiaisonOffices;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Business.Services
{
    public class LiaisonOfficeService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private const string CACHE_KEY = "LIAISON_OFFICE_CACHE";
        private const string PARTNER_COUNT_CACHE_KEY = "LIAISON_OFFICE_PARTNER_COUNTS_CACHE";

        public LiaisonOfficeService(
            AppDbContext context,
            IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets all liaison offices with optional filtering and pagination
        /// </summary>
        public async Task<PaginationResponse<LiaisonOffice>> GetLiaisonOfficesAsync(LiaisonOfficeFilterRequest request)
        {
            var liaisonOffices = await GetAllLiaisonOfficesAsync();
            
            // Apply filters
            var filteredOffices = ApplyFilters(liaisonOffices, request);
            
            // Apply sorting
            filteredOffices = ApplySorting(filteredOffices, request.OrderBy, request.Ascending ?? true);
            
            // Get total count before pagination
            var totalCount = filteredOffices.Count();
            
            // Apply pagination
            var pagedOffices = filteredOffices
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Populate partner counts if requested
            if (request.IncludeCounts)
            {
                await PopulatePartnerCountsAsync(pagedOffices);
            }

            return new PaginationResponse<LiaisonOffice>
            {
                Records = pagedOffices,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Searches liaison offices based on search criteria
        /// </summary>
        public async Task<PaginationResponse<LiaisonOffice>> SearchLiaisonOfficesAsync(LiaisonOfficeSearchRequest request)
        {
            var liaisonOffices = await GetAllLiaisonOfficesAsync();
            
            // Apply search filters
            var filteredOffices = ApplySearchFilters(liaisonOffices, request);
            
            // Apply sorting
            filteredOffices = ApplySorting(filteredOffices, request.OrderBy, request.Ascending);
            
            // Get total count before pagination
            var totalCount = filteredOffices.Count();
            
            // Apply pagination
            var pagedOffices = filteredOffices
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Populate partner counts
            await PopulatePartnerCountsAsync(pagedOffices);

            return new PaginationResponse<LiaisonOffice>
            {
                Records = pagedOffices,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Gets a specific liaison office by ID
        /// </summary>
        public async Task<LiaisonOffice?> GetLiaisonOfficeByIdAsync(int id)
        {
            var liaisonOffices = await GetAllLiaisonOfficesAsync();
            var office = liaisonOffices.FirstOrDefault(lo => lo.Id == id);
            
            if (office != null)
            {
                await PopulatePartnerCountsAsync(new List<LiaisonOffice> { office });
            }
            
            return office;
        }

        private async Task<List<LiaisonOffice>> GetAllLiaisonOfficesAsync()
        {
            return await _memoryCache.GetOrCreateAsync(CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                
                var liaisonOffices = await _context.LiaisonOffices
                    .Where(lo => lo.IsActive && !lo.IsDeleted)
                    .ToListAsync();
                
                return liaisonOffices;
            });
        }

        private async Task PopulatePartnerCountsAsync(List<LiaisonOffice> liaisonOffices)
        {
            var partnerCounts = await _memoryCache.GetOrCreateAsync(PARTNER_COUNT_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                
                // Get partner counts by liaison office ID
                var counts = await _context.Partners
                    .Where(p => !p.IsDeleted && p.LiaisonOfficeId.HasValue)
                    .GroupBy(p => p.LiaisonOfficeId.Value)
                    .Select(g => new { LiaisonOfficeId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.LiaisonOfficeId, x => x.Count);
                
                return counts;
            });

            foreach (var office in liaisonOffices)
            {
                office.PartnerCount = partnerCounts.GetValueOrDefault(office.Id, 0);
            }
        }

        private IEnumerable<LiaisonOffice> ApplyFilters(List<LiaisonOffice> liaisonOffices, LiaisonOfficeFilterRequest request)
        {
            var filtered = liaisonOffices.AsEnumerable();

            if (!string.IsNullOrEmpty(request.Name))
                filtered = filtered.Where(lo => lo.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Code))
                filtered = filtered.Where(lo => lo.Code.Contains(request.Code, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Region))
                filtered = filtered.Where(lo => !string.IsNullOrEmpty(lo.Region) && lo.Region.Contains(request.Region, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Country))
                filtered = filtered.Where(lo => !string.IsNullOrEmpty(lo.Country) && lo.Country.Contains(request.Country, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                filtered = filtered.Where(lo => lo.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (request.IsActive.HasValue)
                filtered = filtered.Where(lo => lo.IsActive == request.IsActive.Value);

            return filtered;
        }

        private IEnumerable<LiaisonOffice> ApplySearchFilters(List<LiaisonOffice> liaisonOffices, LiaisonOfficeSearchRequest request)
        {
            var filtered = liaisonOffices.AsEnumerable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                filtered = filtered.Where(lo => 
                    lo.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    lo.Code.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(lo.Description) && lo.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(request.Region))
                filtered = filtered.Where(lo => !string.IsNullOrEmpty(lo.Region) && lo.Region.Contains(request.Region, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Country))
                filtered = filtered.Where(lo => !string.IsNullOrEmpty(lo.Country) && lo.Country.Contains(request.Country, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                filtered = filtered.Where(lo => lo.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (request.IsActive.HasValue)
                filtered = filtered.Where(lo => lo.IsActive == request.IsActive.Value);

            return filtered;
        }

        private IEnumerable<LiaisonOffice> ApplySorting(IEnumerable<LiaisonOffice> liaisonOffices, string? orderBy, bool ascending)
        {
            return orderBy?.ToLower() switch
            {
                "name" => ascending ? liaisonOffices.OrderBy(lo => lo.Name) : liaisonOffices.OrderByDescending(lo => lo.Name),
                "code" => ascending ? liaisonOffices.OrderBy(lo => lo.Code) : liaisonOffices.OrderByDescending(lo => lo.Code),
                "region" => ascending ? liaisonOffices.OrderBy(lo => lo.Region) : liaisonOffices.OrderByDescending(lo => lo.Region),
                "country" => ascending ? liaisonOffices.OrderBy(lo => lo.Country) : liaisonOffices.OrderByDescending(lo => lo.Country),
                "status" => ascending ? liaisonOffices.OrderBy(lo => lo.Status) : liaisonOffices.OrderByDescending(lo => lo.Status),
                "partnercount" => ascending ? liaisonOffices.OrderBy(lo => lo.PartnerCount) : liaisonOffices.OrderByDescending(lo => lo.PartnerCount),
                _ => ascending ? liaisonOffices.OrderBy(lo => lo.Name) : liaisonOffices.OrderByDescending(lo => lo.Name)
            };
        }
    }
}
