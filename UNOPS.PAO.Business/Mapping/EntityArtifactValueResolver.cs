using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Artifacts;

namespace UNOPS.PAO.Business.Mapping;

/// <summary>
/// AutoMapper value resolver to automatically load EntityArtifacts for any entity
/// This resolver queries the EntityArtifact table and maps to EntityArtifactModel
/// </summary>
public class EntityArtifactValueResolver : IValueResolver<object, object, List<EntityArtifactModel>>
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public EntityArtifactValueResolver(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public List<EntityArtifactModel> Resolve(object source, object destination, List<EntityArtifactModel> destMember, ResolutionContext context)
    {
        // Extract entity type and ID from the source object
        var entityType = GetEntityType(source);
        var entityId = GetEntityId(source);

        if (string.IsNullOrEmpty(entityType))
        {
            return new List<EntityArtifactModel>();
        }

        // Query EntityArtifacts for this entity
        // Filter by effective date: artifact must be effective (EffectiveDate is null or in the past)
        // Filter by status: only include active artifacts
        var now = DateTime.UtcNow;
        var artifacts = _context.EntityArtifacts
            .Include(a => a.ArtifactType)
            .ThenInclude(at => at!.ArtifactDataType)
            .Include(a => a.Document)
            .Where(a => a.EntityType == entityType 
                && a.EntityId == entityId 
                && !a.IsDeleted
                && a.Status == EntityStatus.Active
                && (a.EffectiveDate == null || a.EffectiveDate <= now))
            .OrderBy(a => a.ArtifactType!.Order)
            .ToList();

        // Map to EntityArtifactModel using shared static method
        return MapToModels(artifacts);
    }

    private string GetEntityType(object source)
    {
        return source.GetType().Name switch
        {
            nameof(OrganizationHierarchy) => "OrganizationHierarchy",
            nameof(Country) => "Country",
            nameof(Partner) => "Partner",
            //TO-DO: // Add other entity types as needed
            _ => source.GetType().Name
        };
    }

    private int GetEntityId(object source)
    {
        var idProperty = source.GetType().GetProperty("Id");
        if (idProperty != null)
        {
            var value = idProperty.GetValue(source);
            if (value is int intValue)
                return intValue;
        }
        return 0;
    }

    /// <summary>
    /// Gets the appropriate value from an EntityArtifact based on its data type
    /// </summary>
    public static object? GetArtifactValue(EntityArtifact artifact)
    {
        var dataTypeName = artifact.ArtifactType?.ArtifactDataType?.Name?.ToLower();

        return dataTypeName switch
        {
            "string" or "text" => artifact.ValueText,
            "number" or "decimal" or "integer" => artifact.ValueNumber,
            "boolean" or "bool" => artifact.ValueBoolean,
            "date" or "datetime" => artifact.ValueDate,
            "json" or "array" or "object" => artifact.ValueJson,
            "document" => artifact.ValueText,
            _ => artifact.ValueText // Default to text
        };
    }

    /// <summary>
    /// Maps a collection of EntityArtifact entities to EntityArtifactModel objects
    /// Can be used when the resolver doesn't work (e.g., nested entity mappings)
    /// </summary>
    public static List<EntityArtifactModel> MapToModels(IEnumerable<EntityArtifact> artifacts)
    {
        return artifacts.Select(artifact => new EntityArtifactModel
        {
            ArtifactTypeCode = artifact.ArtifactType?.ArtifactTypeCode ?? string.Empty,
            ArtifactTypeName = artifact.ArtifactType?.Name,
            Category = artifact.ArtifactType?.Category,
            DataType = artifact.ArtifactType?.ArtifactDataType?.Name,
            Value = GetArtifactValue(artifact),
            DocumentId = artifact.DocumentId,
            Metadata = artifact.Metadata,
            EffectiveDate = artifact.EffectiveDate,
            ExpiryDate = artifact.ExpiryDate,
            Source = artifact.Source,
            IsExtracted = artifact.IsExtracted,
            ConfidenceScore = artifact.ConfidenceScore
        }).ToList();
    }
}

