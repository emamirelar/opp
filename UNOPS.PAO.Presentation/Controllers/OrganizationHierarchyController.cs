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
} 