using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Security;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class OrganizationHierarchyController : BaseController
{
    private readonly IOrganizationHierarchyManager _organizationHierarchyManager;
    private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;

    public OrganizationHierarchyController(
        IOrganizationHierarchyManager organizationHierarchyManager,
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<OrganizationHierarchyController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _organizationHierarchyManager = organizationHierarchyManager;
        _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
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