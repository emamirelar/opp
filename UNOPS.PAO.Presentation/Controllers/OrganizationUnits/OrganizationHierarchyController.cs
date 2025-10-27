using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Presentation.Helpers;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Security;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using AutoMapper;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.OrganizationUnits;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class OrganizationHierarchyController : BaseController
{
    private readonly IOrganizationHierarchyManager _organizationHierarchyManager;
    private readonly OrganizationHierarchyService _organizationHierarchyService;
    private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;
    private readonly IMapper _mapper;

    public OrganizationHierarchyController(
        IOrganizationHierarchyManager organizationHierarchyManager,
        OrganizationHierarchyService organizationHierarchyService,
        IManagerWrapper manager,
        IMapper mapper,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<OrganizationHierarchyController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _organizationHierarchyManager = organizationHierarchyManager;
        _organizationHierarchyService = organizationHierarchyService;
        _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all organization hierarchies with optional filtering and pagination.
    /// </summary>
    /// <example_uses>
    /// Get all organization units with pagination
    /// Filter organization units by type or parent
    /// Get organization units with their children counts
    /// Sort organization units by children count
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to list, view, or filter organization hierarchies.</when_to_use>
    /// <returns>Paginated list of organization hierarchies</returns>
    [HttpGet("api/organizationhierarchy")]
    [AccessControlled(EntityTypes.OrganizationHierarchy, "read")]
    public async Task<ActionResult<PaginationResponse<OrganizationHierarchyModel>>> GetOrganizationHierarchies([FromQuery] OrganizationHierarchyFilterRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _organizationHierarchyService.GetOrganizationHierarchiesAsync(request);
            
            var models = result.Records.Select(hierarchy => _mapper.Map<OrganizationHierarchyModel>(hierarchy)).ToList();
            
            return new PaginationResponse<OrganizationHierarchyModel>
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
    /// Searches organization hierarchies based on search criteria.
    /// </summary>
    /// <example_uses>
    /// Search organization units by name or code
    /// Find organization units with specific children count ranges
    /// Search for organization units containing specific terms
    /// Filter by type or parent organization
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search or find organization hierarchies with specific criteria.</when_to_use>
    /// <returns>Paginated search results of organization hierarchies</returns>
    [HttpPost("api/organizationhierarchy/search")]
    [AccessControlled(EntityTypes.OrganizationHierarchy, "read")]
    public async Task<ActionResult<PaginationResponse<OrganizationHierarchyModel>>> SearchOrganizationHierarchies([FromBody] OrganizationHierarchySearchRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _organizationHierarchyService.SearchOrganizationHierarchiesAsync(request);
            
            var models = result.Records.Select(hierarchy => _mapper.Map<OrganizationHierarchyModel>(hierarchy)).ToList();
            
            return new PaginationResponse<OrganizationHierarchyModel>
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
    /// Gets a specific organization hierarchy by ID with full details.
    /// </summary>
    /// <example_uses>
    /// Show detailed organization unit information
    /// Get organization unit with children and entity relationship counts
    /// Display organization unit details in a form or modal
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to view details of a specific organization hierarchy.</when_to_use>
    /// <returns>Organization hierarchy details with computed counts</returns>
    [HttpGet("api/organizationhierarchy/{id}")]
    [AccessControlled(EntityTypes.OrganizationHierarchy, "read")]
    public async Task<ActionResult<OrganizationHierarchyModel>> GetOrganizationHierarchyByIdWithDetails(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var hierarchy = await _organizationHierarchyService.GetOrganizationHierarchyByIdAsync(id);
            
            if (hierarchy == null)
            {
                throw new BusinessException($"Organization hierarchy with ID {id} not found.");
            }
            
            return _mapper.Map<OrganizationHierarchyModel>(hierarchy);
        });
    }

    [HttpGet(APIDictionary.OrganizationHierarchy)]
    public async Task<ActionResult<IEnumerable<OrganizationHierarchyPrimeModel>>> GetOrganizationHierarchy()
    {
        // Use the new optimized format that works directly with PrimeNG
        var hierarchy = await _organizationHierarchyManager.GetOrganizationHierarchyPrime();
        return Ok(hierarchy);
    }
    
    [HttpGet(APIDictionary.OrganizationHierarchy + "/legacy")]
    public async Task<ActionResult<IEnumerable<OrganizationHierarchyTreeModel>>> GetOrganizationHierarchyLegacy()
    {
        // Keep the old format available at a different endpoint
        var hierarchy = await _organizationHierarchyManager.GetOrganizationHierarchy();
        return Ok(hierarchy);
    }

    /// <summary>
    /// Retrieves a specific organizational unit by ID with complete details including hierarchy information.
    /// </summary>
    /// <param name="id">Organizational unit ID</param>
    /// <example_uses>
    /// Show me details for org unit ID 138
    /// Get information about organizational unit 123
    /// What is org unit 456?
    /// Display org unit details for ID 789
    /// Get description of organizational unit 321
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for specific organizational unit details by ID.</when_to_use>
    /// <returns>Complete organizational unit details</returns>
    [HttpGet(APIDictionary.OrganizationHierarchy + "/{id}")]
    public async Task<ActionResult<OrganizationHierarchyModel>> GetOrganizationHierarchyById(int id)
    {
        var orgUnit = await _organizationHierarchyManager.GetOrganizationHierarchyById(id);
        if (orgUnit == null)
        {
            return NotFound($"Organizational unit with ID {id} not found");
        }
        return Ok(orgUnit);
    }

    /// <summary>
    /// Describes the OrganizationHierarchy entity structure including all field configurations
    /// </summary>
    /// <returns>Entity and field metadata for OrganizationHierarchy</returns>
    [HttpGet(APIDictionary.OrganizationHierarchy + "/metadata-info")]
    public async Task<ActionResult> GetMetadataInfo()
    {
        try
        {
            var entityDetails = await _entityConfigurationManager.GetEntityConfigurationDetailsAsync(User, "OrganizationHierarchy");
            return Ok(entityDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving OrganizationHierarchy entity description");
            return StatusCode(500, new { error = "Failed to retrieve OrganizationHierarchy entity description" });
        }
    }
} 