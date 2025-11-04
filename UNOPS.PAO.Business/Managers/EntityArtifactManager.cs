using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Artifacts;

namespace UNOPS.PAO.Business.Managers;

public class EntityArtifactManager : IEntityArtifactManager
{
    private readonly IMapper mapper;
    private readonly AppDbContext context;
    private readonly DataRepository<EntityArtifact> entityArtifactRepository;
    private readonly DataRepository<ArtifactType> artifactTypeRepository;
    private readonly DataRepository<ArtifactDataType> artifactDataTypeRepository;

    public EntityArtifactManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.context = context;
        this.entityArtifactRepository = new DataRepository<EntityArtifact>(context);
        this.artifactTypeRepository = new DataRepository<ArtifactType>(context);
        this.artifactDataTypeRepository = new DataRepository<ArtifactDataType>(context);
    }

    public async Task<IEnumerable<EntityTypeOption>> GetAvailableEntityTypesAsync()
    {
        // Get all artifact types with ApplicableEntityTypes
        var artifactTypes = await artifactTypeRepository
            .GetAll()
            .Where(at => !string.IsNullOrEmpty(at.ApplicableEntityTypes))
            .Select(at => at.ApplicableEntityTypes)
            .ToListAsync();

        // Extract unique entity types from comma-separated lists
        var entityTypes = new HashSet<string>();
        foreach (var applicableTypes in artifactTypes)
        {
            if (!string.IsNullOrEmpty(applicableTypes))
            {
                var types = applicableTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var type in types)
                {
                    entityTypes.Add(type.Trim());
                }
            }
        }

        // Return as EntityTypeOption with both technical name and display name
        return entityTypes
            .OrderBy(et => et)
            .Select(et => new EntityTypeOption
            {
                EntityType = et,
                DisplayName = et // Can be enhanced with translations
            })
            .ToList();
    }

    public async Task<IEnumerable<ArtifactTypeResponse>> GetArtifactTypesByEntityTypeAsync(string entityType)
    {
        var artifactTypes = await artifactTypeRepository
            .GetAll()
            .Include(at => at.ArtifactDataType)
            .Where(at => at.ApplicableEntityTypes != null && 
                        at.ApplicableEntityTypes.Contains(entityType))
            .OrderBy(at => at.Order)
            .ThenBy(at => at.Name)
            .ToListAsync();

        return artifactTypes.Select(at => new ArtifactTypeResponse
        {
            Id = at.Id,
            Name = at.Name,
            ArtifactTypeCode = at.ArtifactTypeCode,
            ArtifactDataTypeId = at.ArtifactDataTypeId,
            ArtifactDataTypeName = at.ArtifactDataType?.Name,
            Description = at.Description,
            Category = at.Category,
            ApplicableEntityTypes = at.ApplicableEntityTypes,
            IsUsedForCalculations = at.IsUsedForCalculations,
            IsUsedForAI = at.IsUsedForAI,
            Order = at.Order
        }).ToList();
    }

    public async Task<IEnumerable<EntityRecordOption>> GetEntityRecordsAsync(string entityType, string? searchTerm = null)
    {
        // Dynamically query the appropriate table based on entity type
        switch (entityType.ToLower())
        {
            case "country":
                var countries = context.Set<Country>().AsQueryable();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countries = countries.Where(c => c.Name.Contains(searchTerm));
                }
                return await countries
                    .OrderBy(c => c.Name)
                    .Select(c => new EntityRecordOption
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = null
                    })
                    .ToListAsync();

            case "partner":
            case "organization":
                var partners = context.Set<Partner>().AsQueryable();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    partners = partners.Where(p => p.Name.Contains(searchTerm));
                }
                return await partners
                    .OrderBy(p => p.Name)
                    .Select(p => new EntityRecordOption
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.PartnerShortDescription
                    })
                    .ToListAsync();

            case "contact":
                var contacts = context.Set<Contact>().AsQueryable();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    contacts = contacts.Where(c => 
                        c.FirstName.Contains(searchTerm) || 
                        c.LastName.Contains(searchTerm) ||
                        c.Email.Contains(searchTerm));
                }
                return await contacts
                    .OrderBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                    .Select(c => new EntityRecordOption
                    {
                        Id = c.Id,
                        Name = c.FirstName + " " + c.LastName,
                        Description = c.Email
                    })
                    .ToListAsync();

            case "orgunit":
            case "organizationhierarchy":
                var orgUnits = context.Set<OrganizationHierarchy>().AsQueryable();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    orgUnits = orgUnits.Where(o => o.Name.Contains(searchTerm) || o.Code.Contains(searchTerm));
                }
                return await orgUnits
                    .OrderBy(o => o.Name)
                    .Select(o => new EntityRecordOption
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Description = o.Code + " - " + o.Type.ToString()
                    })
                    .ToListAsync();

            case "opportunity":
                var opportunities = context.Set<Opportunity>().AsQueryable();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    opportunities = opportunities.Where(o => o.Name.Contains(searchTerm));
                }
                return await opportunities
                    .OrderBy(o => o.Name)
                    .Select(o => new EntityRecordOption
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Description = o.Description
                    })
                    .ToListAsync();

            default:
                return new List<EntityRecordOption>();
        }
    }

    public async Task<EntityArtifactResponse?> GetEntityArtifactAsync(string entityType, int entityId, int artifactTypeId)
    {
        var artifact = await entityArtifactRepository
            .GetAll()
            .Include(ea => ea.ArtifactType)
            .ThenInclude(at => at!.ArtifactDataType)
            .Include(ea => ea.Document)
            .Where(ea => ea.EntityType == entityType && 
                        ea.EntityId == entityId && 
                        ea.ArtifactTypeId == artifactTypeId &&
                        !ea.IsDeleted)
            .OrderByDescending(ea => ea.CreatedDate)
            .FirstOrDefaultAsync();

        if (artifact == null)
        {
            return null;
        }

        return new EntityArtifactResponse
        {
            Id = artifact.Id,
            EntityType = artifact.EntityType,
            EntityId = artifact.EntityId,
            ArtifactTypeId = artifact.ArtifactTypeId,
            ArtifactTypeName = artifact.ArtifactType?.Name,
            ArtifactTypeCode = artifact.ArtifactType?.ArtifactTypeCode,
            DataTypeName = artifact.ArtifactType?.ArtifactDataType?.Name,
            Name = artifact.Name,
            ValueText = artifact.ValueText,
            ValueNumber = artifact.ValueNumber,
            ValueDate = artifact.ValueDate,
            ValueJson = artifact.ValueJson,
            DocumentId = artifact.DocumentId,
            DocumentName = artifact.Document?.Name,
            EffectiveDate = artifact.EffectiveDate,
            ExpiryDate = artifact.ExpiryDate,
            Source = artifact.Source,
            IsExtracted = artifact.IsExtracted,
            SourceArtifactId = artifact.SourceArtifactId,
            Metadata = artifact.Metadata,
            ConfidenceScore = artifact.ConfidenceScore,
            CreatedDate = artifact.CreatedDate,
            CreatedBy = artifact.CreatedBy,
            CreatedByName = null, // Can be enhanced with user lookup
            LastModifiedDate = artifact.LastModifiedDate,
            LastModifiedBy = artifact.LastModifiedBy,
            LastModifiedByName = null // Can be enhanced with user lookup
        };
    }

    public async Task<EntityArtifactResponse> UpsertEntityArtifactAsync(EntityArtifactRequest request)
    {
        // Check if artifact already exists
        var existingArtifact = await entityArtifactRepository
            .GetAll()
            .Where(ea => ea.EntityType == request.EntityType && 
                        ea.EntityId == request.EntityId && 
                        ea.ArtifactTypeId == request.ArtifactTypeId &&
                        !ea.IsDeleted)
            .FirstOrDefaultAsync();

        EntityArtifact artifact;

        if (existingArtifact != null)
        {
            // Update existing artifact
            existingArtifact.Name = request.Name;
            existingArtifact.ValueText = request.ValueText;
            existingArtifact.ValueNumber = request.ValueNumber;
            existingArtifact.ValueDate = request.ValueDate;
            existingArtifact.ValueJson = request.ValueJson;
            existingArtifact.DocumentId = request.DocumentId;
            existingArtifact.EffectiveDate = request.EffectiveDate;
            existingArtifact.ExpiryDate = request.ExpiryDate;
            existingArtifact.Source = request.Source ?? "User Input";
            existingArtifact.Metadata = request.Metadata;

            await entityArtifactRepository.UpdateAsync(existingArtifact);
            artifact = existingArtifact;
        }
        else
        {
            // Create new artifact
            artifact = new EntityArtifact
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ArtifactTypeId = request.ArtifactTypeId,
                Name = request.Name,
                ValueText = request.ValueText,
                ValueNumber = request.ValueNumber,
                ValueDate = request.ValueDate,
                ValueJson = request.ValueJson,
                DocumentId = request.DocumentId,
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate,
                Source = request.Source ?? "User Input",
                Metadata = request.Metadata,
                IsExtracted = false
            };

            await entityArtifactRepository.AddAsync(artifact);
        }

        // Reload with includes for response
        var savedArtifact = await entityArtifactRepository
            .GetAll()
            .Include(ea => ea.ArtifactType)
            .ThenInclude(at => at!.ArtifactDataType)
            .Include(ea => ea.Document)
            .FirstOrDefaultAsync(ea => ea.Id == artifact.Id);

        if (savedArtifact == null)
        {
            throw new Exception("Failed to save artifact");
        }

        return new EntityArtifactResponse
        {
            Id = savedArtifact.Id,
            EntityType = savedArtifact.EntityType,
            EntityId = savedArtifact.EntityId,
            ArtifactTypeId = savedArtifact.ArtifactTypeId,
            ArtifactTypeName = savedArtifact.ArtifactType?.Name,
            ArtifactTypeCode = savedArtifact.ArtifactType?.ArtifactTypeCode,
            DataTypeName = savedArtifact.ArtifactType?.ArtifactDataType?.Name,
            Name = savedArtifact.Name,
            ValueText = savedArtifact.ValueText,
            ValueNumber = savedArtifact.ValueNumber,
            ValueDate = savedArtifact.ValueDate,
            ValueJson = savedArtifact.ValueJson,
            DocumentId = savedArtifact.DocumentId,
            DocumentName = savedArtifact.Document?.Name,
            EffectiveDate = savedArtifact.EffectiveDate,
            ExpiryDate = savedArtifact.ExpiryDate,
            Source = savedArtifact.Source,
            IsExtracted = savedArtifact.IsExtracted,
            SourceArtifactId = savedArtifact.SourceArtifactId,
            Metadata = savedArtifact.Metadata,
            ConfidenceScore = savedArtifact.ConfidenceScore,
            CreatedDate = savedArtifact.CreatedDate,
            CreatedBy = savedArtifact.CreatedBy,
            CreatedByName = null,
            LastModifiedDate = savedArtifact.LastModifiedDate,
            LastModifiedBy = savedArtifact.LastModifiedBy,
            LastModifiedByName = null
        };
    }

    public async Task<IEnumerable<EntityArtifactResponse>> GetEntityArtifactsAsync(string entityType, int entityId)
    {
        var artifacts = await entityArtifactRepository
            .GetAll()
            .Include(ea => ea.ArtifactType)
            .ThenInclude(at => at!.ArtifactDataType)
            .Include(ea => ea.Document)
            .Where(ea => ea.EntityType == entityType && 
                        ea.EntityId == entityId &&
                        !ea.IsDeleted)
            .OrderBy(ea => ea.ArtifactType!.Order)
            .ThenBy(ea => ea.CreatedDate)
            .ToListAsync();

        return artifacts.Select(artifact => new EntityArtifactResponse
        {
            Id = artifact.Id,
            EntityType = artifact.EntityType,
            EntityId = artifact.EntityId,
            ArtifactTypeId = artifact.ArtifactTypeId,
            ArtifactTypeName = artifact.ArtifactType?.Name,
            ArtifactTypeCode = artifact.ArtifactType?.ArtifactTypeCode,
            DataTypeName = artifact.ArtifactType?.ArtifactDataType?.Name,
            Name = artifact.Name,
            ValueText = artifact.ValueText,
            ValueNumber = artifact.ValueNumber,
            ValueDate = artifact.ValueDate,
            ValueJson = artifact.ValueJson,
            DocumentId = artifact.DocumentId,
            DocumentName = artifact.Document?.Name,
            EffectiveDate = artifact.EffectiveDate,
            ExpiryDate = artifact.ExpiryDate,
            Source = artifact.Source,
            IsExtracted = artifact.IsExtracted,
            SourceArtifactId = artifact.SourceArtifactId,
            Metadata = artifact.Metadata,
            ConfidenceScore = artifact.ConfidenceScore,
            CreatedDate = artifact.CreatedDate,
            CreatedBy = artifact.CreatedBy,
            CreatedByName = null,
            LastModifiedDate = artifact.LastModifiedDate,
            LastModifiedBy = artifact.LastModifiedBy,
            LastModifiedByName = null
        }).ToList();
    }
}

