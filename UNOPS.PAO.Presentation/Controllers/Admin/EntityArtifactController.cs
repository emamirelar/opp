using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Identity.Security;
using UNOPS.PAO.Models.Artifacts;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers.Admin;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class EntityArtifactController : BaseController
{
    private readonly IEntityArtifactManager _manager;

    public EntityArtifactController(
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<EntityArtifactController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.EntityArtifactManager;
    }

    /// <summary>
    /// Get all available entity types from ArtifactType configuration
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactEntityTypes)]
    public async Task<ActionResult<IEnumerable<EntityTypeOption>>> GetEntityTypes()
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        try
        {
            var entityTypes = await _manager.GetAvailableEntityTypesAsync();
            return Ok(entityTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity types");
            return StatusCode(500, new { error = "Failed to retrieve entity types" });
        }
    }

    /// <summary>
    /// Get artifact types filtered by entity type
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactTypes)]
    public async Task<ActionResult<IEnumerable<ArtifactTypeResponse>>> GetArtifactTypes([FromQuery] string entityType)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(entityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        try
        {
            var artifactTypes = await _manager.GetArtifactTypesByEntityTypeAsync(entityType);
            return Ok(artifactTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving artifact types for entity type {EntityType}", entityType);
            return StatusCode(500, new { error = "Failed to retrieve artifact types" });
        }
    }

    /// <summary>
    /// Get entity records for dropdown (e.g., list of countries, partners, etc.)
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactRecords)]
    public async Task<ActionResult<IEnumerable<EntityRecordOption>>> GetEntityRecords(
        [FromQuery] string entityType,
        [FromQuery] string? searchTerm = null)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(entityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        try
        {
            var records = await _manager.GetEntityRecordsAsync(entityType, searchTerm);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity records for type {EntityType}", entityType);
            return StatusCode(500, new { error = "Failed to retrieve entity records" });
        }
    }

    /// <summary>
    /// Get existing artifact value for a specific entity and artifact type
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactGet)]
    public async Task<ActionResult<EntityArtifactResponse>> GetEntityArtifact(
        [FromQuery] string entityType,
        [FromQuery] int entityId,
        [FromQuery] int artifactTypeId)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(entityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        if (entityId <= 0)
        {
            return BadRequest(new { error = "Entity ID must be greater than 0" });
        }

        if (artifactTypeId <= 0)
        {
            return BadRequest(new { error = "Artifact type ID must be greater than 0" });
        }

        try
        {
            var artifact = await _manager.GetEntityArtifactAsync(entityType, entityId, artifactTypeId);
            
            // Return OK with null if no artifact exists yet (user will create a new one)
            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving artifact for {EntityType} {EntityId} with artifact type {ArtifactTypeId}", 
                entityType, entityId, artifactTypeId);
            return StatusCode(500, new { error = "Failed to retrieve artifact" });
        }
    }

    /// <summary>
    /// Upsert (create or update) an entity artifact
    /// </summary>
    [HttpPost(APIDictionary.EntityArtifactUpsert)]
    public async Task<ActionResult<EntityArtifactResponse>> UpsertEntityArtifact([FromBody] EntityArtifactRequest request)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(request.EntityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        if (request.EntityId <= 0)
        {
            return BadRequest(new { error = "Entity ID must be greater than 0" });
        }

        if (request.ArtifactTypeId <= 0)
        {
            return BadRequest(new { error = "Artifact type ID must be greater than 0" });
        }

        try
        {
            var artifact = await _manager.UpsertEntityArtifactAsync(request);
            return Ok(artifact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting artifact for {EntityType} {EntityId}", 
                request.EntityType, request.EntityId);
            return StatusCode(500, new { error = "Failed to save artifact" });
        }
    }

    /// <summary>
    /// Get all artifacts for a specific entity
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactList)]
    public async Task<ActionResult<IEnumerable<EntityArtifactResponse>>> GetEntityArtifacts(
        [FromQuery] string entityType,
        [FromQuery] int entityId)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(entityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        if (entityId <= 0)
        {
            return BadRequest(new { error = "Entity ID must be greater than 0" });
        }

        try
        {
            var artifacts = await _manager.GetEntityArtifactsAsync(entityType, entityId);
            return Ok(artifacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving artifacts for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, new { error = "Failed to retrieve artifacts" });
        }
    }

    /// <summary>
    /// Get unique identifier example for bulk import template
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactBulkUniqueIdExample)]
    public async Task<ActionResult<EntityUniqueIdExampleResponse>> GetBulkUniqueIdExample([FromQuery] string entityType)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(entityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        try
        {
            var example = await _manager.GetUniqueIdExampleAsync(entityType);
            return Ok(example);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unique ID example for {EntityType}", entityType);
            return StatusCode(500, new { error = "Failed to retrieve unique ID example" });
        }
    }

    /// <summary>
    /// Download CSV template for bulk import
    /// </summary>
    [HttpPost(APIDictionary.EntityArtifactBulkTemplateDownload)]
    public async Task<IActionResult> DownloadBulkTemplate([FromBody] BulkTemplateDownloadRequest request)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(request.EntityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        if (request.ArtifactTypeIds == null || !request.ArtifactTypeIds.Any())
        {
            return BadRequest(new { error = "At least one artifact type is required" });
        }

        try
        {
            var csvBytes = await _manager.GenerateBulkTemplateAsync(request);
            var fileName = $"EntityArtifact_BulkImport_{request.EntityType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            
            return File(csvBytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk template for {EntityType}", request.EntityType);
            return StatusCode(500, new { error = "Failed to generate bulk template" });
        }
    }

    /// <summary>
    /// Bulk upsert entity artifacts from CSV data
    /// </summary>
    [HttpPost(APIDictionary.EntityArtifactBulkUpsert)]
    public async Task<ActionResult<BulkEntityArtifactResponse>> BulkUpsertEntityArtifacts([FromBody] BulkEntityArtifactRequest request)
    {
        // Check role authorization
        var authResult = await CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN);
        if (authResult != null)
        {
            return authResult;
        }

        if (string.IsNullOrEmpty(request.EntityType))
        {
            return BadRequest(new { error = "Entity type is required" });
        }

        if (request.Rows == null || !request.Rows.Any())
        {
            return BadRequest(new { error = "No rows provided for import" });
        }

        if (request.ColumnToArtifactTypeMapping == null || !request.ColumnToArtifactTypeMapping.Any())
        {
            return BadRequest(new { error = "Column to artifact type mapping is required" });
        }

        try
        {
            var result = await _manager.BulkUpsertEntityArtifactsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk upsert for {EntityType}", request.EntityType);
            return StatusCode(500, new { error = "Failed to process bulk upsert" });
        }
    }
}

