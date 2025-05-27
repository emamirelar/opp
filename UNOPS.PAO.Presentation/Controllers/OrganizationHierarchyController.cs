using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[ApiController]
public class OrganizationHierarchyController : ControllerBase
{
    private readonly IOrganizationHierarchyManager _organizationHierarchyManager;

    public OrganizationHierarchyController(IOrganizationHierarchyManager organizationHierarchyManager)
    {
        _organizationHierarchyManager = organizationHierarchyManager;
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
} 