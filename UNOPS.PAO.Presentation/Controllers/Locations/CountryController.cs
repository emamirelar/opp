using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Locations;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Locations;

[ApiController]
[Route("api/[controller]")]
public class CountryController : BaseController
{
    private readonly CountryService _countryService;
    private readonly IMapper _mapper;

    public CountryController(
        CountryService countryService,
        IMapper mapper,
        ILogger<CountryController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _countryService = countryService;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all countries with optional filtering and pagination.
    /// </summary>
    /// <example_uses>
    /// Get all countries with pagination
    /// Filter countries by name or region
    /// Get countries with their partner counts
    /// Sort countries by partner count
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to list, view, or filter countries.</when_to_use>
    /// <returns>Paginated list of countries</returns>
    [HttpGet]
    [AccessControlled(EntityTypes.Country, "read")]
    public async Task<ActionResult<PaginationResponse<CountryModel>>> GetCountries([FromQuery] CountryFilterRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _countryService.GetCountriesAsync(request);
            
            var models = result.Records.Select(country => _mapper.Map<CountryModel>(country)).ToList();
            
            return new PaginationResponse<CountryModel>
            {
                Records = models,
                TotalCount = result.TotalCount,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
        });
    }

    /// <summary>
    /// Searches countries based on search criteria.
    /// </summary>
    /// <example_uses>
    /// Search countries by name or ISO code
    /// Find countries with specific partner count ranges
    /// Search for countries containing specific terms
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search or find countries with specific criteria.</when_to_use>
    /// <returns>Paginated search results of countries</returns>
    [HttpPost("search")]
    [AccessControlled(EntityTypes.Country, "read")]
    public async Task<ActionResult<PaginationResponse<CountryModel>>> SearchCountries([FromBody] CountrySearchRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _countryService.SearchCountriesAsync(request);
            
            var models = result.Records.Select(country => _mapper.Map<CountryModel>(country)).ToList();
            
            return new PaginationResponse<CountryModel>
            {
                Records = models,
                TotalCount = result.TotalCount,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
        });
    }

    /// <summary>
    /// Gets a specific country by ID with full details.
    /// </summary>
    /// <example_uses>
    /// Show detailed country information
    /// Get country with partner and liaison office counts
    /// Display country details in a form or modal
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to view details of a specific country.</when_to_use>
    /// <returns>Country details with computed counts</returns>
    [HttpGet("{id}")]
    [AccessControlled(EntityTypes.Country, "read")]
    public async Task<ActionResult<CountryModel>> GetCountryById(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var country = await _countryService.GetCountryByIdAsync(id);
            
            if (country == null)
            {
                throw new BusinessException($"Country with ID {id} not found.");
            }
            
            return _mapper.Map<CountryModel>(country);
        });
    }

    /// <summary>
    /// Performs dynamic search across country names and artifact values.
    /// Returns grouped results with match context for enhanced user experience.
    /// Uses IsSearchable property on ArtifactTypes to determine which artifacts to search.
    /// </summary>
    /// <example_uses>
    /// Search countries by name and artifact values
    /// Find countries with specific strategic documents
    /// Locate countries by searchable index values
    /// Search across multiple artifact types
    /// </example_uses>
    /// <when_to_use>
    /// Use this when the user needs advanced country search with artifact matching.
    /// Ideal for searching countries by metrics, strategies, or other searchable attributes.
    /// </when_to_use>
    /// <returns>Grouped search results with match context and relevance scores</returns>
    [HttpPost("dynamic-search")]
    [AccessControlled(EntityTypes.Country, "read")]
    public async Task<ActionResult<CountryDynamicSearchResponse>> DynamicSearchCountries(
        [FromBody] CountryDynamicSearchRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _countryService.DynamicSearchCountriesAsync(request);
            return result;
        });
    }
}
