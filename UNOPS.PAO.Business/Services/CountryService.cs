using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;

namespace UNOPS.PAO.Business.Services
{
    public class CountryService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private const string CACHE_KEY = "COUNTRY_CACHE";
        private const string PARTNER_COUNT_CACHE_KEY = "COUNTRY_PARTNER_COUNTS_CACHE";

        public CountryService(
            AppDbContext context,
            IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets all countries with optional filtering and pagination
        /// </summary>
        public async Task<PaginationResponse<Country>> GetCountriesAsync(CountryFilterRequest request)
        {
            var countries = await GetAllCountriesAsync();
            
            // Apply filters
            var filteredCountries = ApplyFilters(countries, request);
            
            // Apply sorting
            filteredCountries = ApplySorting(filteredCountries, request.OrderBy, request.Ascending ?? true);
            
            // Get total count before pagination
            var totalCount = filteredCountries.Count();
            
            // Apply pagination
            var pagedCountries = filteredCountries
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Populate partner counts if requested
            if (request.IncludeCounts)
            {
                await PopulatePartnerCountsAsync(pagedCountries);
            }

            return new PaginationResponse<Country>
            {
                Records = pagedCountries,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Searches countries based on search criteria
        /// </summary>
        public async Task<PaginationResponse<Country>> SearchCountriesAsync(CountrySearchRequest request)
        {
            var countries = await GetAllCountriesAsync();
            
            // Apply search filters
            var filteredCountries = ApplySearchFilters(countries, request);
            
            // Apply sorting
            filteredCountries = ApplySorting(filteredCountries, request.OrderBy, request.Ascending);
            
            // Get total count before pagination
            var totalCount = filteredCountries.Count();
            
            // Apply pagination
            var pagedCountries = filteredCountries
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Populate partner counts
            await PopulatePartnerCountsAsync(pagedCountries);

            return new PaginationResponse<Country>
            {
                Records = pagedCountries,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        /// <summary>
        /// Gets a specific country by ID
        /// </summary>
        public async Task<Country?> GetCountryByIdAsync(int id)
        {
            var countries = await GetAllCountriesAsync();
            var country = countries.FirstOrDefault(c => c.Id == id);
            
            if (country != null)
            {
                await PopulatePartnerCountsAsync(new List<Country> { country });
            }
            
            return country;
        }

        private async Task<List<Country>> GetAllCountriesAsync()
        {
            return await _memoryCache.GetOrCreateAsync(CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                
                var countries = await _context.Countries
                    .Where(c => c.Status == EntityStatus.Active)
                    .ToListAsync();
                
                return countries;
            });
        }

        private async Task PopulatePartnerCountsAsync(List<Country> countries)
        {
            var partnerCounts = await _memoryCache.GetOrCreateAsync(PARTNER_COUNT_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                
                // Get partner counts by country through LiaisonOffice
                var counts = await _context.Partners
                    .Where(p => !p.IsDeleted && p.LiaisonOffice != null && !string.IsNullOrEmpty(p.LiaisonOffice.Country))
                    .GroupBy(p => p.LiaisonOffice.Country)
                    .Select(g => new { Country = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Country!, x => x.Count);
                
                return counts;
            });

            var liaisonOfficeCounts = await _context.LiaisonOffices
                .Where(lo => lo.IsActive && !lo.IsDeleted && !string.IsNullOrEmpty(lo.Country))
                .GroupBy(lo => lo.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Country, x => x.Count);

            foreach (var country in countries)
            {
                country.PartnerCount = partnerCounts.GetValueOrDefault(country.Name, 0);
                country.LiaisonOfficeCount = liaisonOfficeCounts.GetValueOrDefault(country.Name, 0);
            }
        }

        private IEnumerable<Country> ApplyFilters(List<Country> countries, CountryFilterRequest request)
        {
            var filtered = countries.AsEnumerable();

            if (!string.IsNullOrEmpty(request.Name))
                filtered = filtered.Where(c => c.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Iso2Code))
                filtered = filtered.Where(c => c.Iso2Code.Contains(request.Iso2Code, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                filtered = filtered.Where(c => c.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            return filtered;
        }

        private IEnumerable<Country> ApplySearchFilters(List<Country> countries, CountrySearchRequest request)
        {
            var filtered = countries.AsEnumerable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                filtered = filtered.Where(c => 
                    c.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Iso2Code.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(request.Status))
                filtered = filtered.Where(c => c.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (request.MinPartnerCount.HasValue)
                filtered = filtered.Where(c => c.PartnerCount >= request.MinPartnerCount.Value);

            if (request.MaxPartnerCount.HasValue)
                filtered = filtered.Where(c => c.PartnerCount <= request.MaxPartnerCount.Value);

            return filtered;
        }

        private IEnumerable<Country> ApplySorting(IEnumerable<Country> countries, string? orderBy, bool ascending)
        {
            return orderBy?.ToLower() switch
            {
                "name" => ascending ? countries.OrderBy(c => c.Name) : countries.OrderByDescending(c => c.Name),
                "iso2code" => ascending ? countries.OrderBy(c => c.Iso2Code) : countries.OrderByDescending(c => c.Iso2Code),
                "status" => ascending ? countries.OrderBy(c => c.Status) : countries.OrderByDescending(c => c.Status),
                "partnercount" => ascending ? countries.OrderBy(c => c.PartnerCount) : countries.OrderByDescending(c => c.PartnerCount),
                _ => ascending ? countries.OrderBy(c => c.Name) : countries.OrderByDescending(c => c.Name)
            };
        }
    }
}
