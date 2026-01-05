using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Locations;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AutoMapper;

namespace UNOPS.PAO.Business.Services
{
    public class CountryService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private const string CACHE_KEY = "COUNTRY_CACHE";
        private const string PARTNER_COUNT_CACHE_KEY = "COUNTRY_PARTNER_COUNTS_CACHE";

        public CountryService(
            AppDbContext context,
            IMemoryCache memoryCache,
            IMapper mapper)
        {
            _context = context;
            _memoryCache = memoryCache;
            _mapper = mapper;
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

        /// <summary>
        /// Performs dynamic search across country names and artifact values
        /// Returns grouped results with match context
        /// Uses IsSearchable property instead of ArtifactDataType filtering
        /// </summary>
        public async Task<CountryDynamicSearchResponse> DynamicSearchCountriesAsync(
            CountryDynamicSearchRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Validate search term
            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return new CountryDynamicSearchResponse
                {
                    TotalMatches = 0,
                    Groups = new CountrySearchGroups(),
                    AllResults = new List<CountrySearchResultModel>(),
                    Metadata = new SearchMetadata
                    {
                        SearchTerm = request.SearchTerm ?? string.Empty,
                        ArtifactTypesSearched = 0,
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                        FromCache = false
                    }
                };
            }
            
            var searchTerm = request.CaseSensitive 
                ? request.SearchTerm.Trim() 
                : request.SearchTerm.Trim().ToLowerInvariant();
            
            // Get all countries from cache
            var countries = await GetAllCountriesAsync();
            await PopulatePartnerCountsAsync(countries);
            
            // Step 1: Find countries by name
            var nameMatches = await SearchByCountryNameAsync(countries, searchTerm, request);
            
            // Step 2: Find countries by continent
            var continentMatches = await SearchByContinentAsync(countries, searchTerm, request);
            
            // Step 3: Find countries by artifact values (if enabled)
            var artifactMatches = new Dictionary<string, List<CountrySearchResultModel>>();
            var artifactTypesSearched = 0;
            
            if (request.IncludeArtifacts)
            {
                var artifactSearchResult = await SearchByArtifactValuesAsync(
                    countries, 
                    searchTerm, 
                    request);
                artifactMatches = artifactSearchResult.GroupedMatches;
                artifactTypesSearched = artifactSearchResult.TypesSearched;
            }
            
            // Step 4: Combine results and remove duplicates
            var allResults = CombineAndDeduplicateResults(
                nameMatches, 
                continentMatches, 
                artifactMatches);
            
            // Step 4: Apply result limit
            if (allResults.Count > request.MaxResults)
            {
                allResults = allResults
                    .OrderByDescending(r => r.RelevanceScore)
                    .Take(request.MaxResults)
                    .ToList();
            }
            
            stopwatch.Stop();
            
            return new CountryDynamicSearchResponse
            {
                TotalMatches = allResults.Count,
                Groups = new CountrySearchGroups
                {
                    NameMatches = nameMatches,
                    RegionMatches = new List<CountrySearchResultModel>(),
                    ContinentMatches = continentMatches,
                    ArtifactMatches = artifactMatches
                },
                AllResults = allResults,
                Metadata = new SearchMetadata
                {
                    SearchTerm = request.SearchTerm,
                    ArtifactTypesSearched = artifactTypesSearched,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    FromCache = true // Countries are from cache
                }
            };
        }

        /// <summary>
        /// Search countries by name with relevance scoring
        /// </summary>
        private async Task<List<CountrySearchResultModel>> SearchByCountryNameAsync(
            List<Country> countries,
            string searchTerm,
            CountryDynamicSearchRequest request)
        {
            var results = new List<CountrySearchResultModel>();
            var comparison = request.CaseSensitive 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;
            
            foreach (var country in countries)
            {
                var countryName = request.CaseSensitive 
                    ? country.Name 
                    : country.Name.ToLowerInvariant();
                
                bool matches = request.ExactMatch
                    ? countryName.Equals(searchTerm, comparison)
                    : countryName.Contains(searchTerm, comparison);
                
                if (matches)
                {
                    // Calculate relevance score
                    decimal relevanceScore = CalculateNameRelevanceScore(
                        country.Name, 
                        searchTerm, 
                        request.ExactMatch);
                    
                    var matchReason = new SearchMatchReason
                    {
                        MatchType = "CountryName",
                        MatchedValue = country.Name,
                        HighlightedValue = request.HighlightMatches 
                            ? HighlightMatchedText(country.Name, searchTerm) 
                            : country.Name
                    };
                    
                    results.Add(new CountrySearchResultModel
                    {
                        Country = new CountrySearchInfo
                        {
                            Id = country.Id,
                            Name = country.Name,
                            Iso2Code = country.Iso2Code,
                            Continent = country.ContinentDescription,
                            Region = country.RegionDescription
                        },
                        MatchReasons = new List<SearchMatchReason> { matchReason },
                        RelevanceScore = relevanceScore
                    });
                }
            }
            
            return results;
        }

        /// <summary>
        /// Search countries by continent description with relevance scoring
        /// </summary>
        private async Task<List<CountrySearchResultModel>> SearchByContinentAsync(
            List<Country> countries,
            string searchTerm,
            CountryDynamicSearchRequest request)
        {
            var results = new List<CountrySearchResultModel>();
            var comparison = request.CaseSensitive 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;
            
            foreach (var country in countries)
            {
                if (string.IsNullOrWhiteSpace(country.ContinentDescription))
                    continue;
                
                var continentDescription = request.CaseSensitive 
                    ? country.ContinentDescription 
                    : country.ContinentDescription.ToLowerInvariant();
                
                bool matches = request.ExactMatch
                    ? continentDescription.Equals(searchTerm, comparison)
                    : continentDescription.Contains(searchTerm, comparison);
                
                if (matches)
                {
                    // Calculate relevance score
                    decimal relevanceScore = CalculateNameRelevanceScore(
                        country.ContinentDescription, 
                        searchTerm, 
                        request.ExactMatch);
                    
                    var matchReason = new SearchMatchReason
                    {
                        MatchType = "Continent",
                        MatchedValue = country.ContinentDescription,
                        HighlightedValue = request.HighlightMatches 
                            ? HighlightMatchedText(country.ContinentDescription, searchTerm) 
                            : country.ContinentDescription
                    };
                    
                    results.Add(new CountrySearchResultModel
                    {
                        Country = new CountrySearchInfo
                        {
                            Id = country.Id,
                            Name = country.Name,
                            Iso2Code = country.Iso2Code,
                            Continent = country.ContinentDescription,
                            Region = country.RegionDescription
                        },
                        MatchReasons = new List<SearchMatchReason> { matchReason },
                        RelevanceScore = relevanceScore
                    });
                }
            }
            
            return results;
        }

        /// <summary>
        /// Search countries by artifact values using IsSearchable property
        /// </summary>
        private async Task<(Dictionary<string, List<CountrySearchResultModel>> GroupedMatches, int TypesSearched)> 
            SearchByArtifactValuesAsync(
                List<Country> countries,
                string searchTerm,
                CountryDynamicSearchRequest request)
        {
            var groupedMatches = new Dictionary<string, List<CountrySearchResultModel>>();
            
            // Get all searchable artifact types for Country entity
            var searchableArtifactTypes = await _context.ArtifactTypes
                .Include(at => at.ArtifactDataType)
                .Where(at => 
                    !at.IsDeleted &&
                    at.Status == EntityStatus.Active &&
                    at.IsSearchable == true &&
                    (at.ApplicableEntityTypes == null || 
                     at.ApplicableEntityTypes.Contains("Country")))
                .ToListAsync();
            
            // Filter by specific artifact type codes if provided
            if (request.ArtifactTypeCodes != null && request.ArtifactTypeCodes.Any())
            {
                searchableArtifactTypes = searchableArtifactTypes
                    .Where(at => request.ArtifactTypeCodes.Contains(at.ArtifactTypeCode))
                    .ToList();
            }
            
            var artifactTypeIds = searchableArtifactTypes.Select(at => at.Id).ToList();
            
            // Get all entity artifacts for countries with searchable artifact types
            var entityArtifacts = await _context.EntityArtifacts
                .Include(ea => ea.ArtifactType)
                .Where(ea =>
                    !ea.IsDeleted &&
                    ea.EntityType == "Country" &&
                    artifactTypeIds.Contains(ea.ArtifactTypeId) &&
                    !string.IsNullOrEmpty(ea.ValueText))
                .ToListAsync();
            
            var comparison = request.CaseSensitive 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;
            
            // Group by artifact type
            foreach (var artifactType in searchableArtifactTypes)
            {
                var matchingArtifacts = entityArtifacts
                    .Where(ea => ea.ArtifactTypeId == artifactType.Id)
                    .ToList();
                
                var matchingCountries = new List<CountrySearchResultModel>();
                
                foreach (var artifact in matchingArtifacts)
                {
                    var valueText = request.CaseSensitive 
                        ? artifact.ValueText 
                        : artifact.ValueText?.ToLowerInvariant();
                    
                    bool matches = request.ExactMatch
                        ? valueText?.Equals(searchTerm, comparison) ?? false
                        : valueText?.Contains(searchTerm, comparison) ?? false;
                    
                    if (matches && artifact.ValueText != null)
                    {
                        var country = countries.FirstOrDefault(c => c.Id == artifact.EntityId);
                        
                        if (country != null)
                        {
                            // Calculate relevance score for artifact match
                            decimal relevanceScore = CalculateArtifactRelevanceScore(
                                artifact.ValueText, 
                                searchTerm, 
                                request.ExactMatch);
                            
                            var matchReason = new SearchMatchReason
                            {
                                MatchType = "ArtifactValue",
                                ArtifactTypeCode = artifactType.ArtifactTypeCode,
                                ArtifactTypeName = artifactType.Name,
                                Category = artifactType.Category,
                                MatchedValue = artifact.ValueText,
                                HighlightedValue = request.HighlightMatches 
                                    ? HighlightMatchedText(artifact.ValueText, searchTerm) 
                                    : artifact.ValueText
                            };
                            
                            // Check if country already in results for this artifact type
                            var existingMatch = matchingCountries
                                .FirstOrDefault(m => m.Country.Id == country.Id);
                            
                            if (existingMatch != null)
                            {
                                // Add additional match reason
                                existingMatch.MatchReasons.Add(matchReason);
                                existingMatch.RelevanceScore += relevanceScore;
                            }
                            else
                            {
                                matchingCountries.Add(new CountrySearchResultModel
                                {
                                    Country = new CountrySearchInfo
                                    {
                                        Id = country.Id,
                                        Name = country.Name,
                                        Iso2Code = country.Iso2Code,
                                        Continent = country.ContinentDescription,
                                        Region = country.RegionDescription
                                    },
                                    MatchReasons = new List<SearchMatchReason> { matchReason },
                                    RelevanceScore = relevanceScore
                                });
                            }
                        }
                    }
                }
                
                if (matchingCountries.Any())
                {
                    groupedMatches[artifactType.Name] = matchingCountries
                        .OrderByDescending(m => m.RelevanceScore)
                        .ToList();
                }
            }
            
            return (groupedMatches, searchableArtifactTypes.Count);
        }

        /// <summary>
        /// Combine and deduplicate results from name and artifact searches
        /// </summary>
        private List<CountrySearchResultModel> CombineAndDeduplicateResults(
            List<CountrySearchResultModel> nameMatches,
            List<CountrySearchResultModel> continentMatches,
            Dictionary<string, List<CountrySearchResultModel>> artifactMatches)
        {
            var allResults = new Dictionary<int, CountrySearchResultModel>();
            
            // Add name matches
            foreach (var match in nameMatches)
            {
                allResults[match.Country.Id] = match;
            }
            
            // Add continent matches
            foreach (var match in continentMatches)
            {
                if (allResults.ContainsKey(match.Country.Id))
                {
                    // Merge match reasons and update relevance score
                    var existing = allResults[match.Country.Id];
                    existing.MatchReasons.AddRange(match.MatchReasons);
                    existing.RelevanceScore += match.RelevanceScore;
                }
                else
                {
                    allResults[match.Country.Id] = match;
                }
            }
            
            // Add artifact matches
            foreach (var artifactGroup in artifactMatches.Values)
            {
                foreach (var match in artifactGroup)
                {
                    if (allResults.ContainsKey(match.Country.Id))
                    {
                        // Merge match reasons and update relevance score
                        var existing = allResults[match.Country.Id];
                        existing.MatchReasons.AddRange(match.MatchReasons);
                        existing.RelevanceScore += match.RelevanceScore;
                    }
                    else
                    {
                        allResults[match.Country.Id] = match;
                    }
                }
            }
            
            return allResults.Values
                .OrderByDescending(r => r.RelevanceScore)
                .ThenBy(r => r.Country.Name)
                .ToList();
        }

        /// <summary>
        /// Calculate relevance score for country name matches
        /// Higher score = more relevant
        /// </summary>
        private decimal CalculateNameRelevanceScore(
            string countryName, 
            string searchTerm, 
            bool exactMatch)
        {
            if (exactMatch)
            {
                return 100m; // Exact match gets highest score
            }
            
            decimal score = 50m; // Base score for partial match
            
            // Bonus for match at start of name
            if (countryName.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                score += 30m;
            }
            
            // Bonus based on match coverage
            decimal coverage = (decimal)searchTerm.Length / countryName.Length;
            score += coverage * 20m;
            
            return score;
        }

        /// <summary>
        /// Calculate relevance score for artifact value matches
        /// </summary>
        private decimal CalculateArtifactRelevanceScore(
            string artifactValue, 
            string searchTerm, 
            bool exactMatch)
        {
            if (exactMatch)
            {
                return 80m; // Exact artifact match (slightly lower than name match)
            }
            
            decimal score = 40m; // Base score for partial match
            
            // Bonus for match at start
            if (artifactValue.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                score += 20m;
            }
            
            // Bonus based on match coverage
            decimal coverage = (decimal)searchTerm.Length / artifactValue.Length;
            score += coverage * 15m;
            
            return score;
        }

        /// <summary>
        /// Highlight matched text within a string
        /// Returns HTML with matched portions wrapped in &lt;mark&gt; tags
        /// </summary>
        private string HighlightMatchedText(string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
            {
                return text;
            }
            
            // Use regex to find and highlight all occurrences (case-insensitive)
            var pattern = Regex.Escape(searchTerm);
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            
            return regex.Replace(text, match => $"<mark>{match.Value}</mark>");
        }
    }
}
