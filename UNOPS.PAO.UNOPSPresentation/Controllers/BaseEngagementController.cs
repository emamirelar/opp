using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.Presentation.Helpers;
using UNOPSAPIDictionary = UNOPS.PAO.UNOPSPresentation.Helpers.APIDictionary;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class BaseEngagementController : BaseController
{
    private readonly IBaseEngagementManager _manager;
    
    public BaseEngagementController(
        IManagerWrapper managerWrapper,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<BaseEngagementController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = ((UNOPSManagerWrapper)managerWrapper).BaseEngagementManager;
    }
    
    /// <summary>
    /// Retrieves all base engagements that the user has permission to view
    /// </summary>
    /// <param name="partnerId">Optional partner ID to filter engagements by</param>
    /// <returns>List of base engagement models with engagement and partner information</returns>
    [HttpGet(UNOPSAPIDictionary.BaseEngagements)]
    [AccessControlled(EntityTypes.BaseEngagement, "read")]
    public async Task<ActionResult> GetBaseEngagements([FromQuery] int? partnerId = null)
    {
        try
        {
            // If partnerId is provided, filter by that partner
            if (partnerId.HasValue)
            {
                var partnerEngagements = await _manager.GetByPartnerIdAsync(User, partnerId.Value);
                return Ok(partnerEngagements);
            }
            
            // Otherwise, return all engagements the user has access to
            var engagements = await _manager.GetAllAsync(User);
            return Ok(engagements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving base engagements");
            return StatusCode(500, new { error = "Failed to retrieve engagements" });
        }
    }
    
    /// <summary>
    /// Retrieves a specific base engagement by ID
    /// </summary>
    /// <param name="id">Base engagement ID</param>
    /// <returns>Base engagement model with full details including partners</returns>
    [HttpGet(UNOPSAPIDictionary.BaseEngagement + "/{id}")]
    [AccessControlled(EntityTypes.BaseEngagement, "read")]
    public async Task<ActionResult> GetBaseEngagement(int id)
    {
        try
        {
            var engagement = await _manager.GetByIdAsync(User, id);
            if (engagement == null)
            {
                return NotFound(new { error = "Engagement not found" });
            }
            return Ok(engagement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving base engagement {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve engagement" });
        }
    }
    
    /// <summary>
    /// Retrieves all base engagements associated with a specific partner
    /// </summary>
    /// <param name="partnerId">Partner ID to filter engagements by</param>
    /// <returns>List of base engagements related to the specified partner</returns>
    [HttpGet(UNOPSAPIDictionary.BaseEngagementsByPartner + "/{partnerId}/base-engagements")]
    [AccessControlled(EntityTypes.BaseEngagement, "read")]
    public async Task<ActionResult> GetBaseEngagementsByPartner(int partnerId)
    {
        try
        {
            var engagements = await _manager.GetByPartnerIdAsync(User, partnerId);
            return Ok(engagements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving base engagements for partner {PartnerId}", partnerId);
            return StatusCode(500, new { error = "Failed to retrieve engagements" });
        }
    }
    
    /// <summary>
    /// Retrieves all partner relationships for a specific engagement
    /// </summary>
    /// <param name="engagementId">Base engagement ID</param>
    /// <returns>List of partner relationship models for the engagement</returns>
    [HttpGet(UNOPSAPIDictionary.BaseEngagementPartners + "/{engagementId}/partners")]
    [AccessControlled(EntityTypes.BaseEngagement, "read")]
    public async Task<ActionResult> GetEngagementPartners(int engagementId)
    {
        try
        {
            var partners = await _manager.GetEngagementPartnersAsync(User, engagementId);
            return Ok(partners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving partners for engagement {EngagementId}", engagementId);
            return StatusCode(500, new { error = "Failed to retrieve engagement partners" });
        }
    }
}
