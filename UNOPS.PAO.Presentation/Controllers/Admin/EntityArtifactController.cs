using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Security;
using UNOPS.PAO.Models.Artifacts;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Managers;

namespace UNOPS.PAO.Presentation.Controllers.Admin;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class EntityArtifactController : BaseController
{
    private readonly IEntityArtifactManager _manager;
    private readonly GoogleCloudStorageService _gcsService;
    private readonly IConfiguration _configuration;

    public EntityArtifactController(
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<EntityArtifactController> logger,
        IAuthorizationService authorizationService,
        IConfiguration configuration)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.EntityArtifactManager;
        _configuration = configuration;
        _gcsService = new GoogleCloudStorageService(configuration);
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
    /// Upload a document artifact to Google Cloud Storage
    /// Documents are stored with folder path: EntityArtifacts/{ArtifactCode}/{Entity}/{EntityId}/
    /// The GCS URL is stored in ValueText instead of base64 in ValueJson
    /// </summary>
    [HttpPost(APIDictionary.EntityArtifactUploadDocument)]
    public async Task<ActionResult<EntityArtifactResponse>> UploadDocumentArtifact([FromForm] EntityArtifactDocumentRequest request)
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

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new { error = "File is required" });
        }

        try
        {
            // Get artifact type code for folder path
            var artifactTypeCode = request.ArtifactTypeCode;
            if (string.IsNullOrEmpty(artifactTypeCode))
            {
                artifactTypeCode = await _manager.GetArtifactTypeCodeAsync(request.ArtifactTypeId);
            }
            
            if (string.IsNullOrEmpty(artifactTypeCode))
            {
                artifactTypeCode = "UNKNOWN";
            }

            _logger.LogInformation("Uploading document artifact to GCS: EntityType={EntityType}, EntityId={EntityId}, ArtifactTypeCode={ArtifactTypeCode}, FileName={FileName}",
                request.EntityType, request.EntityId, artifactTypeCode, request.File.FileName);

            // Build folder path: entityartifacts/{artifactcode}/{entity}/{entityid}/
            var folderPath = $"entityartifacts/{artifactTypeCode.ToLowerInvariant()}/{request.EntityType.ToLowerInvariant()}/{request.EntityId}";

            // Generate unique filename - sanitize to avoid URL encoding issues
            var fileExtension = Path.GetExtension(request.File.FileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(request.File.FileName);
            // Replace spaces and special characters with underscores for GCS compatibility
            var sanitizedFileName = SanitizeFileName(fileNameWithoutExtension);
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueFileName = $"{sanitizedFileName}_{uniqueId}{fileExtension}";

            // Upload to GCS
            var gcsUrl = await _gcsService.UploadFileAsync(request.File, $"{folderPath}/{uniqueFileName}");

            if (string.IsNullOrEmpty(gcsUrl))
            {
                throw new BusinessException("Failed to upload file to Google Cloud Storage");
            }

            _logger.LogInformation("GCS Upload successful: {GcsUrl}", gcsUrl);

            // Create the artifact request
            var artifactRequest = new EntityArtifactRequest
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ArtifactTypeId = request.ArtifactTypeId,
                Name = request.Name,
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate,
                Source = request.Source ?? "User Input",
                Metadata = request.Metadata
            };

            // Upsert the document artifact with GCS URL in ValueText
            var artifact = await _manager.UpsertDocumentArtifactAsync(
                artifactRequest,
                gcsUrl,
                request.File.FileName,
                request.File.ContentType ?? "application/octet-stream",
                request.File.Length
            );

            return Ok(artifact);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business exception during document artifact upload: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document artifact for {EntityType} {EntityId}",
                request.EntityType, request.EntityId);
            return StatusCode(500, new { error = "Failed to upload document artifact" });
        }
    }

    /// <summary>
    /// Get a signed URL for viewing/downloading a document artifact
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactDocumentUrl)]
    public async Task<ActionResult> GetDocumentUrl(
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
            // Get the artifact
            var artifact = await _manager.GetEntityArtifactAsync(entityType, entityId, artifactTypeId);

            if (artifact == null)
            {
                return NotFound(new { error = "Artifact not found" });
            }

            // Check if this is a document artifact with a URL in ValueText
            if (string.IsNullOrEmpty(artifact.ValueText))
            {
                return BadRequest(new { error = "Artifact does not have a document URL" });
            }

            var documentUrl = artifact.ValueText;

            // If it's a gs:// URI, generate a signed URL
            if (documentUrl.StartsWith("gs://"))
            {
                var signedUrl = await _gcsService.GetSignedUrlFromGsUri(documentUrl, 60);
                return Ok(new { url = signedUrl, fileName = artifact.Name });
            }
            // If it's already an https:// URL, return it directly (or generate signed URL if it's a GCS URL)
            else if (documentUrl.StartsWith("https://storage."))
            {
                var signedUrl = await _gcsService.GenerateSignedUrlFromStorageUrl(documentUrl);
                return Ok(new { url = signedUrl, fileName = artifact.Name });
            }
            else
            {
                // Return the URL as-is
                return Ok(new { url = documentUrl, fileName = artifact.Name });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document URL for {EntityType} {EntityId} {ArtifactTypeId}",
                entityType, entityId, artifactTypeId);
            return StatusCode(500, new { error = "Failed to get document URL" });
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
    /// Get artifact types for bulk operations (filtered by AllowBulkUpdate = true)
    /// </summary>
    [HttpGet(APIDictionary.EntityArtifactBulkArtifactTypes)]
    public async Task<ActionResult<IEnumerable<ArtifactTypeResponse>>> GetBulkArtifactTypes([FromQuery] string entityType)
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
            var artifactTypes = await _manager.GetBulkUpdateArtifactTypesByEntityTypeAsync(entityType);
            return Ok(artifactTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bulk artifact types for entity type {EntityType}", entityType);
            return StatusCode(500, new { error = "Failed to retrieve bulk artifact types" });
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

    /// <summary>
    /// Sanitize filename to avoid URL encoding issues in GCS
    /// Replaces spaces and special characters with underscores
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return "document";
        }

        // Replace spaces and problematic characters with underscores
        var sanitized = fileName
            .Replace(' ', '_')
            .Replace('%', '_')
            .Replace('#', '_')
            .Replace('&', '_')
            .Replace('?', '_')
            .Replace('+', '_')
            .Replace('=', '_');

        // Remove any double underscores
        while (sanitized.Contains("__"))
        {
            sanitized = sanitized.Replace("__", "_");
        }

        // Trim underscores from start and end
        sanitized = sanitized.Trim('_');

        return string.IsNullOrEmpty(sanitized) ? "document" : sanitized;
    }
}

