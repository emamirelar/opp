using UNOPS.PAO.Models.Artifacts;

namespace UNOPS.PAO.Business.Interfaces;

public interface IEntityArtifactManager
{
    /// <summary>
    /// Get all available entity types from ArtifactType.ApplicableEntityTypes
    /// </summary>
    Task<IEnumerable<EntityTypeOption>> GetAvailableEntityTypesAsync();

    /// <summary>
    /// Get artifact types filtered by entity type
    /// </summary>
    Task<IEnumerable<ArtifactTypeResponse>> GetArtifactTypesByEntityTypeAsync(string entityType);

    /// <summary>
    /// Get artifact types filtered by entity type and AllowBulkUpdate flag (for bulk operations)
    /// </summary>
    Task<IEnumerable<ArtifactTypeResponse>> GetBulkUpdateArtifactTypesByEntityTypeAsync(string entityType);

    /// <summary>
    /// Get records for a specific entity type (for EntityID dropdown)
    /// </summary>
    Task<IEnumerable<EntityRecordOption>> GetEntityRecordsAsync(string entityType, string? searchTerm = null);

    /// <summary>
    /// Get existing artifact value for entity + artifact type
    /// </summary>
    Task<EntityArtifactResponse?> GetEntityArtifactAsync(string entityType, int entityId, int artifactTypeId);

    /// <summary>
    /// Upsert (create or update) an entity artifact
    /// </summary>
    Task<EntityArtifactResponse> UpsertEntityArtifactAsync(EntityArtifactRequest request);

    /// <summary>
    /// Upsert (create or update) a document type entity artifact
    /// Document URL is stored in ValueText instead of base64 in ValueJson
    /// </summary>
    /// <param name="request">The artifact request with document metadata</param>
    /// <param name="documentUrl">The GCS URL where the document is stored</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="mimeType">MIME type of the file</param>
    /// <param name="fileSize">Size of the file in bytes</param>
    Task<EntityArtifactResponse> UpsertDocumentArtifactAsync(
        EntityArtifactRequest request, 
        string documentUrl, 
        string fileName, 
        string mimeType, 
        long fileSize);

    /// <summary>
    /// Get the artifact type code for an artifact type ID
    /// </summary>
    Task<string?> GetArtifactTypeCodeAsync(int artifactTypeId);

    /// <summary>
    /// Get all artifacts for a specific entity
    /// </summary>
    Task<IEnumerable<EntityArtifactResponse>> GetEntityArtifactsAsync(string entityType, int entityId);

    /// <summary>
    /// Get unique identifier example for bulk import template
    /// </summary>
    Task<EntityUniqueIdExampleResponse> GetUniqueIdExampleAsync(string entityType);

    /// <summary>
    /// Generate CSV template for bulk import
    /// </summary>
    Task<byte[]> GenerateBulkTemplateAsync(BulkTemplateDownloadRequest request);

    /// <summary>
    /// Process bulk upsert of entity artifacts
    /// </summary>
    Task<BulkEntityArtifactResponse> BulkUpsertEntityArtifactsAsync(BulkEntityArtifactRequest request);
}

