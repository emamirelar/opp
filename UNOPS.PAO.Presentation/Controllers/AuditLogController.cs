using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers;

/// <summary>
/// Controller for managing audit log operations
/// </summary>
[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class AuditLogController : BaseController
{
    private readonly IAuditLogManager _auditLogManager;
    private readonly int _currentUserId;

    public AuditLogController(
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<AuditLogController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _auditLogManager = manager.AuditLogManager;
        _currentUserId = userResolverService.GetCurrentUserId();
    }

    /// <summary>
    /// Gets the latest audit log for a specific entity
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., 'Opportunity')</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>Latest audit log entry with JSON data</returns>
    [HttpGet(APIDictionary.AuditLogLatest)]
    public async Task<ActionResult> GetLatestAuditLog(
        [FromQuery] string entityType,
        [FromQuery] int entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            return BadRequest(new { error = "EntityType is required" });
        }

        if (entityId <= 0)
        {
            return BadRequest(new { error = "Valid EntityId is required" });
        }

        var auditLog = await _auditLogManager.GetLatestAuditLogAsync(entityType, entityId);

        if (auditLog == null)
        {
            return NotFound(new { error = $"No audit log found for {entityType} with ID {entityId}" });
        }

        return Ok(auditLog);
    }
}

