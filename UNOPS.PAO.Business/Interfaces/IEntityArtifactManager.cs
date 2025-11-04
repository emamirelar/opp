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
    /// Get all artifacts for a specific entity
    /// </summary>
    Task<IEnumerable<EntityArtifactResponse>> GetEntityArtifactsAsync(string entityType, int entityId);
}

