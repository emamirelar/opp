using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Attributes;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class OpportunityController : BaseController
{
    private readonly IOpportunityManager _manager;

    public OpportunityController(
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<OpportunityController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.OpportunityManager;
    }

    /// <summary>
    /// Creates a new opportunity
    /// </summary>
    [HttpPost(APIDictionary.Opportunity)]
    [AccessControlled(EntityTypes.Opportunity, "create")]
    public async Task<ActionResult> Create([FromBody] OpportunityRequest req)
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(req.Name))
        {
            validationErrors.Add("Name is required for opportunity creation");
        }
        
        if (string.IsNullOrWhiteSpace(req.Description))
        {
            validationErrors.Add("Description is required for opportunity creation");
        }

        if (validationErrors.Any())
        {
            var errorMessage = $"Missing required fields for opportunity creation: {string.Join(", ", validationErrors)}";
            return BadRequest(new
            {
                error = errorMessage,
                missingFields = validationErrors
            });
        }

        var result = await _manager.CreateOpportunityAsync(req);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific opportunity by ID
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> Get(int id)
    {
        var result = await _manager.GetOpportunityAsync(id);

        if (result == null)
        {
            return NotFound(new { error = $"Opportunity with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all opportunities with pagination support
    /// </summary>
    [HttpGet(APIDictionary.Opportunity)]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetAllOpportunities(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "name",
        [FromQuery] bool ascending = true,
        [FromQuery] bool filterActive = true)
    {
        try
        {
            _logger.LogInformation("=== GET ALL OPPORTUNITIES ENDPOINT ===");
            _logger.LogInformation("Page: {PageIndex}, Size: {PageSize}, OrderBy: {OrderBy}", pageIndex, pageSize, orderBy);

            // Get all opportunities from manager
            var opportunities = await _manager.GetAllOpportunitiesAsync();

            // Apply ordering
            var orderedOpportunities = orderBy?.ToLower() switch
            {
                "name" => ascending ? opportunities.OrderBy(o => o.Name) : opportunities.OrderByDescending(o => o.Name),
                "createddate" => ascending ? opportunities.OrderBy(o => o.CreatedDate) : opportunities.OrderByDescending(o => o.CreatedDate),
                "lastmodifieddate" => ascending ? opportunities.OrderBy(o => o.LastModifiedDate) : opportunities.OrderByDescending(o => o.LastModifiedDate),
                _ => ascending ? opportunities.OrderBy(o => o.Name) : opportunities.OrderByDescending(o => o.Name)
            };

            // Apply pagination
            var totalCount = orderedOpportunities.Count();
            var paginatedOpportunities = orderedOpportunities
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<OpportunityModel>
            {
                Records = paginatedOpportunities,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            _logger.LogInformation("Returned {Count} opportunities out of {TotalCount}", paginatedOpportunities.Count, totalCount);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opportunities");
            return StatusCode(500, new { error = "Internal server error while fetching opportunities", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing opportunity
    /// </summary>
    [HttpPut(APIDictionary.Opportunity + "/{id}")]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateOpportunityRequest req)
    {
        if (id != req.Id)
        {
            return BadRequest(new { error = "ID mismatch between route and request body" });
        }

        var result = await _manager.UpdateOpportunityAsync(req);

        if (result == null)
        {
            return NotFound(new { error = $"Opportunity with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates the WHAT section of an opportunity (description, org unit, initiative type, deliverables)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhat)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhatSection(int id, [FromBody] WhatSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhatSectionAsync(id, req);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHAT section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHAT section", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHY section of an opportunity (strategic alignment, beneficiaries, outcomes, SDGs)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhy)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhySection(int id, [FromBody] WhySectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhySectionAsync(id, req);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHY section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHY section", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHO section of an opportunity (funding partners, client partners)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWho)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhoSection(int id, [FromBody] WhoSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhoSectionAsync(id, req);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHO section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHO section", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHERE section of an opportunity (implementation countries)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhere)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhereSection(int id, [FromBody] WhereSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhereSectionAsync(id, req);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHERE section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHERE section", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets related items (contacts, partners, interactions) for an opportunity
    /// </summary>
    [HttpGet(APIDictionary.OpportunityRelated)]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetRelatedItems(int id)
    {
        try
        {
            var result = await _manager.GetRelatedItemsAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related items for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while getting related items", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHEN section of an opportunity (timeline dates)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhen)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhenSection(int id, [FromBody] WhenSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhenSectionAsync(id, req);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHEN section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHEN section", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an opportunity
    /// </summary>
    [HttpDelete(APIDictionary.Opportunity + "/{id}")]
    [AccessControlled(EntityTypes.Opportunity, "delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _manager.DeleteOpportunityAsync(id);

        if (!result)
        {
            return NotFound(new { error = $"Opportunity with ID {id} not found" });
        }

        return Ok(new { message = "Opportunity deleted successfully", id });
    }
}

