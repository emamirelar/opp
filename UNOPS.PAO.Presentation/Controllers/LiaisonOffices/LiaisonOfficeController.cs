using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models.LiaisonOffices;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.LiaisonOffices;

[ApiController]
[Route("api/[controller]")]
public class LiaisonOfficeController : BaseController
{
    private readonly LiaisonOfficeService _liaisonOfficeService;
    private readonly IMapper _mapper;

    public LiaisonOfficeController(
        LiaisonOfficeService liaisonOfficeService,
        IMapper mapper,
        ILogger<LiaisonOfficeController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _liaisonOfficeService = liaisonOfficeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all liaison offices with optional filtering and pagination.
    /// </summary>
    /// <example_uses>
    /// Get all liaison offices with pagination
    /// Filter liaison offices by region or country
    /// Get liaison offices with their partner counts
    /// Sort liaison offices by partner count
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to list, view, or filter liaison offices.</when_to_use>
    /// <returns>Paginated list of liaison offices</returns>
    [HttpGet]
    [AccessControlled(EntityTypes.LiaisonOffice, "read")]
    public async Task<ActionResult<PaginationResponse<LiaisonOfficeModel>>> GetLiaisonOffices([FromQuery] LiaisonOfficeFilterRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _liaisonOfficeService.GetLiaisonOfficesAsync(request);
            
            var models = result.Records.Select(office => _mapper.Map<LiaisonOfficeModel>(office)).ToList();
            
            return new PaginationResponse<LiaisonOfficeModel>
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
    /// Searches liaison offices based on search criteria.
    /// </summary>
    /// <example_uses>
    /// Search liaison offices by name or code
    /// Find liaison offices with specific partner count ranges
    /// Search for liaison offices containing specific terms
    /// Filter by region or country
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search or find liaison offices with specific criteria.</when_to_use>
    /// <returns>Paginated search results of liaison offices</returns>
    [HttpPost("search")]
    [AccessControlled(EntityTypes.LiaisonOffice, "read")]
    public async Task<ActionResult<PaginationResponse<LiaisonOfficeModel>>> SearchLiaisonOffices([FromBody] LiaisonOfficeSearchRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _liaisonOfficeService.SearchLiaisonOfficesAsync(request);
            
            var models = result.Records.Select(office => _mapper.Map<LiaisonOfficeModel>(office)).ToList();
            
            return new PaginationResponse<LiaisonOfficeModel>
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
    /// Gets a specific liaison office by ID with full details.
    /// </summary>
    /// <example_uses>
    /// Show detailed liaison office information
    /// Get liaison office with partner counts
    /// Display liaison office details in a form or modal
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to view details of a specific liaison office.</when_to_use>
    /// <returns>Liaison office details with computed counts</returns>
    [HttpGet("{id}")]
    [AccessControlled(EntityTypes.LiaisonOffice, "read")]
    public async Task<ActionResult<LiaisonOfficeModel>> GetLiaisonOfficeById(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var office = await _liaisonOfficeService.GetLiaisonOfficeByIdAsync(id);
            
            if (office == null)
            {
                throw new BusinessException($"Liaison office with ID {id} not found.");
            }
            
            return _mapper.Map<LiaisonOfficeModel>(office);
        });
    }
}
