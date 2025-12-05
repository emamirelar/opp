using System.Globalization;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Artifacts;
using UNOPS.PAO.Domain.Enums;

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
            Order = at.Order,
            Source = at.Source,
            IsSearchable = at.IsSearchable,
            AllowBulkUpdate = at.AllowBulkUpdate
        }).ToList();
    }

    public async Task<IEnumerable<ArtifactTypeResponse>> GetBulkUpdateArtifactTypesByEntityTypeAsync(string entityType)
    {
        var artifactTypes = await artifactTypeRepository
            .GetAll()
            .Include(at => at.ArtifactDataType)
            .Where(at => at.ApplicableEntityTypes != null && 
                        at.ApplicableEntityTypes.Contains(entityType) &&
                        at.AllowBulkUpdate == true)
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
            Order = at.Order,
            Source = at.Source,
            IsSearchable = at.IsSearchable,
            AllowBulkUpdate = at.AllowBulkUpdate
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
                        !ea.IsDeleted &&
                        ea.Status == EntityStatus.Active)
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
            ValueBoolean = artifact.ValueBoolean,
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
            existingArtifact.ValueBoolean = request.ValueBoolean;
            existingArtifact.ValueDate = request.ValueDate;
            existingArtifact.ValueJson = request.ValueJson;
            existingArtifact.DocumentId = request.DocumentId;
            existingArtifact.EffectiveDate = request.EffectiveDate;
            existingArtifact.ExpiryDate = request.ExpiryDate;
            existingArtifact.Source = request.Source ?? "User Input";
            existingArtifact.Metadata = request.Metadata;
            existingArtifact.Status = EntityStatus.Active;

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
                ValueBoolean = request.ValueBoolean,
                ValueDate = request.ValueDate,
                ValueJson = request.ValueJson,
                DocumentId = request.DocumentId,
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate,
                Source = request.Source ?? "User Input",
                Metadata = request.Metadata,
                IsExtracted = false,
                Status = EntityStatus.Active
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
            ValueBoolean = savedArtifact.ValueBoolean,
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

    public async Task<EntityArtifactResponse> UpsertDocumentArtifactAsync(
        EntityArtifactRequest request, 
        string documentUrl, 
        string fileName, 
        string mimeType, 
        long fileSize)
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

        // Create metadata JSON with file info
        var documentMetadata = System.Text.Json.JsonSerializer.Serialize(new
        {
            fileName = fileName,
            mimeType = mimeType,
            fileSize = fileSize,
            uploadedAt = DateTime.UtcNow.ToString("o")
        });

        if (existingArtifact != null)
        {
            // Update existing artifact - store URL in ValueText
            existingArtifact.Name = request.Name ?? fileName;
            existingArtifact.ValueText = documentUrl; // GCS URL stored in ValueText
            existingArtifact.ValueJson = documentMetadata; // File metadata in ValueJson
            existingArtifact.ValueNumber = null;
            existingArtifact.ValueBoolean = null;
            existingArtifact.ValueDate = null;
            existingArtifact.DocumentId = request.DocumentId;
            existingArtifact.EffectiveDate = request.EffectiveDate;
            existingArtifact.ExpiryDate = request.ExpiryDate;
            existingArtifact.Source = request.Source ?? "User Input";
            existingArtifact.Metadata = request.Metadata;
            existingArtifact.Status = EntityStatus.Active;

            await entityArtifactRepository.UpdateAsync(existingArtifact);
            artifact = existingArtifact;
        }
        else
        {
            // Create new artifact - store URL in ValueText
            artifact = new EntityArtifact
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ArtifactTypeId = request.ArtifactTypeId,
                Name = request.Name ?? fileName,
                ValueText = documentUrl, // GCS URL stored in ValueText
                ValueJson = documentMetadata, // File metadata in ValueJson
                ValueNumber = null,
                ValueBoolean = null,
                ValueDate = null,
                DocumentId = request.DocumentId,
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate,
                Source = request.Source ?? "User Input",
                Metadata = request.Metadata,
                IsExtracted = false,
                Status = EntityStatus.Active
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
            ValueBoolean = savedArtifact.ValueBoolean,
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

    public async Task<string?> GetArtifactTypeCodeAsync(int artifactTypeId)
    {
        var artifactType = await artifactTypeRepository
            .GetAll()
            .Where(at => at.Id == artifactTypeId)
            .FirstOrDefaultAsync();

        return artifactType?.ArtifactTypeCode;
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
                        !ea.IsDeleted &&
                        ea.Status == EntityStatus.Active)
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
            ValueBoolean = artifact.ValueBoolean,
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

    public async Task<EntityUniqueIdExampleResponse> GetUniqueIdExampleAsync(string entityType)
    {
        switch (entityType.ToLower())
        {
            case "country":
                var country = await context.Set<Country>()
                    .OrderBy(c => c.Name)
                    .FirstOrDefaultAsync();
                    
                if (country == null)
                {
                    throw new Exception("No country records found to generate example");
                }
                
                return new EntityUniqueIdExampleResponse
                {
                    EntityType = entityType,
                    UniqueIdFieldName = "Iso2Code",
                    UniqueIdFieldLabel = "ISO 2-Letter Country Code",
                    Description = "The 2-letter ISO country code (ISO 3166-1 alpha-2)",
                    ExampleValue = country.Iso2Code,
                    ExampleEntityName = country.Name
                };

            case "partner":
            case "organization":
                var partner = await context.Set<Partner>()
                    .Where(p => p.ErpDimValue.HasValue)
                    .OrderBy(p => p.Name)
                    .FirstOrDefaultAsync();
                    
                if (partner == null)
                {
                    throw new Exception("No partner records with ERP dimension value found to generate example");
                }
                
                return new EntityUniqueIdExampleResponse
                {
                    EntityType = entityType,
                    UniqueIdFieldName = "ErpDimValue",
                    UniqueIdFieldLabel = "ERP Dimension Value",
                    Description = "The numeric ERP dimension value assigned to the partner",
                    ExampleValue = partner.ErpDimValue!.Value.ToString(),
                    ExampleEntityName = partner.Name
                };

            case "orgunit":
            case "organizationhierarchy":
                var orgUnit = await context.Set<OrganizationHierarchy>()
                    .OrderBy(o => o.Name)
                    .FirstOrDefaultAsync();
                    
                if (orgUnit == null)
                {
                    throw new Exception("No organization hierarchy records found to generate example");
                }
                
                return new EntityUniqueIdExampleResponse
                {
                    EntityType = entityType,
                    UniqueIdFieldName = "Code",
                    UniqueIdFieldLabel = "Organization Unit Code",
                    Description = "The unique organization unit code (OrgUnitCode)",
                    ExampleValue = orgUnit.Code,
                    ExampleEntityName = orgUnit.Name
                };

            case "contact":
                var contact = await context.Set<Contact>()
                    .OrderBy(c => c.LastName)
                    .FirstOrDefaultAsync();
                    
                if (contact == null)
                {
                    throw new Exception("No contact records found to generate example");
                }
                
                return new EntityUniqueIdExampleResponse
                {
                    EntityType = entityType,
                    UniqueIdFieldName = "Email",
                    UniqueIdFieldLabel = "Email Address",
                    Description = "The contact's email address as unique identifier",
                    ExampleValue = contact.Email,
                    ExampleEntityName = $"{contact.FirstName} {contact.LastName}"
                };

            default:
                throw new ArgumentException($"Unsupported entity type: {entityType}");
        }
    }

    public async Task<byte[]> GenerateBulkTemplateAsync(BulkTemplateDownloadRequest request)
    {
        // Get artifact types to include in template
        var artifactTypes = await artifactTypeRepository
            .GetAll()
            .Include(at => at.ArtifactDataType)
            .Where(at => request.ArtifactTypeIds.Contains(at.Id))
            .OrderBy(at => request.ArtifactTypeIds.IndexOf(at.Id))
            .ToListAsync();

        // Get unique ID info for entity type
        var uniqueIdInfo = await GetUniqueIdExampleAsync(request.EntityType);

        // Build CSV content
        var csv = new StringBuilder();

        // Header row
        var headers = new List<string> { uniqueIdInfo.UniqueIdFieldLabel };
        headers.AddRange(artifactTypes.Select(at => at.Name));
        csv.AppendLine(string.Join(",", headers.Select(h => EscapeCsvValue(h))));

        // Info row (data type hints)
        var dataTypeHints = new List<string> { $"Example: {uniqueIdInfo.ExampleValue}" };
        dataTypeHints.AddRange(artifactTypes.Select(at => $"({at.ArtifactDataType?.Name ?? "text"})"));
        csv.AppendLine(string.Join(",", dataTypeHints.Select(h => EscapeCsvValue(h))));

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<BulkEntityArtifactResponse> BulkUpsertEntityArtifactsAsync(BulkEntityArtifactRequest request)
    {
        var response = new BulkEntityArtifactResponse
        {
            EntityType = request.EntityType,
            TotalRows = request.Rows.Count,
            SuccessfulRows = 0,
            FailedRows = 0,
            RowResults = new List<BulkEntityArtifactRowResult>()
        };

        // Get all artifact types for this batch
        var artifactTypeIds = request.ColumnToArtifactTypeMapping.Values.ToList();
        var artifactTypes = await artifactTypeRepository
            .GetAll()
            .Include(at => at.ArtifactDataType)
            .Where(at => artifactTypeIds.Contains(at.Id))
            .ToDictionaryAsync(at => at.Id);

        // Filter out document type artifacts
        var filteredColumnMapping = request.ColumnToArtifactTypeMapping
            .Where(kvp => 
            {
                var artifactType = artifactTypes.GetValueOrDefault(kvp.Value);
                return artifactType?.ArtifactDataType?.Name?.ToLower() != "document";
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Process each row
        foreach (var row in request.Rows)
        {
            var rowResult = new BulkEntityArtifactRowResult
            {
                RowNumber = row.RowNumber,
                UniqueId = row.UniqueId,
                Success = true,
                CellResults = new List<BulkEntityArtifactCellResult>()
            };

            try
            {
                // Resolve entity ID from unique identifier
                var entityId = await ResolveEntityIdFromUniqueIdAsync(request.EntityType, row.UniqueId);
                
                if (!entityId.HasValue)
                {
                    rowResult.Success = false;
                    rowResult.ErrorMessage = $"Entity not found with unique identifier: {row.UniqueId}";
                    response.FailedRows++;
                    response.RowResults.Add(rowResult);
                    continue;
                }

                rowResult.EntityId = entityId.Value;
                rowResult.EntityName = await GetEntityNameAsync(request.EntityType, entityId.Value);

                // Process each cell/column
                bool hasAnyCellError = false;
                foreach (var columnMapping in filteredColumnMapping)
                {
                    var columnIndex = columnMapping.Key;
                    var artifactTypeId = columnMapping.Value;
                    var artifactType = artifactTypes[artifactTypeId];

                    var cellResult = new BulkEntityArtifactCellResult
                    {
                        ColumnIndex = columnIndex,
                        ArtifactTypeId = artifactTypeId,
                        ArtifactTypeName = artifactType.Name,
                        Success = true,
                        Skipped = false
                    };

                    try
                    {
                        // Get cell value
                        var cellValue = row.CellValues.GetValueOrDefault(columnIndex, string.Empty).Trim();

                        // Get existing artifact to check previous value
                        var existingArtifact = await GetEntityArtifactAsync(request.EntityType, entityId.Value, artifactTypeId);
                        
                        if (existingArtifact != null)
                        {
                            cellResult.IsNew = false;
                            cellResult.PreviousValue = GetDisplayValue(existingArtifact);
                        }
                        else
                        {
                            cellResult.IsNew = true;
                            cellResult.PreviousValue = null;
                        }

                        // Skip if cell is empty and would overwrite existing data
                        if (string.IsNullOrWhiteSpace(cellValue) && existingArtifact != null)
                        {
                            cellResult.Skipped = true;
                            cellResult.CurrentValue = cellResult.PreviousValue;
                            rowResult.CellResults.Add(cellResult);
                            continue;
                        }

                        // Skip if cell is empty and no existing data (nothing to create)
                        if (string.IsNullOrWhiteSpace(cellValue))
                        {
                            cellResult.Skipped = true;
                            cellResult.CurrentValue = null;
                            rowResult.CellResults.Add(cellResult);
                            continue;
                        }

                        // Check if the new value is the same as the existing value
                        // If so, skip the update to avoid unnecessary database operations
                        var dataTypeName = artifactType.ArtifactDataType?.Name?.ToLower();
                        bool valueUnchanged = false;
                        
                        if (existingArtifact != null)
                        {
                            switch (dataTypeName)
                            {
                                case "string":
                                case "text":
                                    valueUnchanged = cellValue == existingArtifact.ValueText;
                                    break;

                                case "number":
                                case "numeric":
                                case "decimal":
                                    if (decimal.TryParse(cellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var numValue))
                                    {
                                        valueUnchanged = existingArtifact.ValueNumber.HasValue && 
                                                        existingArtifact.ValueNumber.Value == numValue;
                                    }
                                    break;

                                case "boolean":
                                case "bool":
                                    if (TryParseFlexibleBoolean(cellValue, out var boolValue))
                                    {
                                        valueUnchanged = existingArtifact.ValueBoolean.HasValue && 
                                                        existingArtifact.ValueBoolean.Value == boolValue;
                                    }
                                    break;

                                case "date":
                                case "datetime":
                                    if (DateTime.TryParse(cellValue, out var dateValue))
                                    {
                                        var utcDateValue = dateValue.Kind == DateTimeKind.Utc 
                                            ? dateValue 
                                            : DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);
                                        
                                        valueUnchanged = existingArtifact.ValueDate.HasValue && 
                                                        existingArtifact.ValueDate.Value.Date == utcDateValue.Date;
                                    }
                                    break;

                                case "json":
                                    valueUnchanged = cellValue == existingArtifact.ValueJson;
                                    break;

                                default:
                                    valueUnchanged = cellValue == existingArtifact.ValueText;
                                    break;
                            }
                        }

                        // Skip if value hasn't changed
                        if (valueUnchanged)
                        {
                            cellResult.Skipped = true;
                            cellResult.CurrentValue = cellResult.PreviousValue;
                            cellResult.ErrorMessage = $"Value unchanged: {cellResult.PreviousValue}";
                            rowResult.CellResults.Add(cellResult);
                            continue;
                        }

                        // Create upsert request
                        var upsertRequest = new EntityArtifactRequest
                        {
                            EntityType = request.EntityType,
                            EntityId = entityId.Value,
                            ArtifactTypeId = artifactTypeId,
                            Source = "Bulk Import"
                        };

                        // Set value based on data type
                        switch (dataTypeName)
                        {
                            case "string":
                            case "text":
                                upsertRequest.ValueText = cellValue;
                                break;

                            case "number":
                            case "numeric":
                            case "decimal":
                                if (decimal.TryParse(cellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var numValue))
                                {
                                    upsertRequest.ValueNumber = numValue;
                                }
                                else
                                {
                                    throw new FormatException($"Invalid number format: {cellValue}");
                                }
                                break;

                            case "boolean":
                            case "bool":
                                if (TryParseFlexibleBoolean(cellValue, out var boolValue))
                                {
                                    upsertRequest.ValueBoolean = boolValue;
                                }
                                else
                                {
                                    throw new FormatException($"Invalid boolean format: {cellValue}. Expected 'true', 'false', '1', or '0'.");
                                }
                                break;

                            case "date":
                            case "datetime":
                                if (DateTime.TryParse(cellValue, out var dateValue))
                                {
                                    // Convert to UTC to satisfy PostgreSQL timestamp with time zone requirements
                                    // PostgreSQL requires DateTime.Kind = UTC, not Unspecified
                                    upsertRequest.ValueDate = dateValue.Kind == DateTimeKind.Utc 
                                        ? dateValue 
                                        : DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);
                                }
                                else
                                {
                                    throw new FormatException($"Invalid date format: {cellValue}");
                                }
                                break;

                            case "json":
                                upsertRequest.ValueJson = cellValue;
                                break;

                            default:
                                upsertRequest.ValueText = cellValue;
                                break;
                        }

                        // Upsert the artifact
                        var result = await UpsertEntityArtifactAsync(upsertRequest);
                        
                        if (result == null)
                        {
                            throw new Exception("Upsert returned null result");
                        }
                        
                        cellResult.CurrentValue = GetDisplayValue(result);
                        cellResult.Success = true;
                    }
                    catch (Exception ex)
                    {
                        cellResult.Success = false;
                        cellResult.ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
                        if (ex.InnerException != null)
                        {
                            cellResult.ErrorMessage += $" | Inner: {ex.InnerException.Message}";
                        }
                        hasAnyCellError = true;
                        
                        // Clear any tracked entities that failed to save to prevent them from affecting subsequent operations
                        // This ensures each cell operation is truly independent
                        var failedEntries = context.ChangeTracker.Entries()
                            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                            .ToList();
                        
                        foreach (var entry in failedEntries)
                        {
                            entry.State = EntityState.Detached;
                        }
                        
                        // Log the full exception for debugging
                        Console.WriteLine($"[BulkUpsert Error] Row {row.RowNumber}, Column {columnMapping.Key}, ArtifactType {artifactType.Name}: {ex}");
                    }

                    rowResult.CellResults.Add(cellResult);
                }

                if (hasAnyCellError)
                {
                    rowResult.Success = false;
                    response.FailedRows++;
                }
                else
                {
                    response.SuccessfulRows++;
                }
            }
            catch (Exception ex)
            {
                rowResult.Success = false;
                rowResult.ErrorMessage = ex.Message;
                response.FailedRows++;
            }

            response.RowResults.Add(rowResult);
        }

        return response;
    }

    // Helper method to resolve entity ID from unique identifier
    private async Task<int?> ResolveEntityIdFromUniqueIdAsync(string entityType, string uniqueId)
    {
        switch (entityType.ToLower())
        {
            case "country":
                var country = await context.Set<Country>()
                    .Where(c => c.Iso2Code == uniqueId)
                    .FirstOrDefaultAsync();
                return country?.Id;

            case "partner":
            case "organization":
                if (int.TryParse(uniqueId, out var erpDimValue))
                {
                    var partner = await context.Set<Partner>()
                        .Where(p => p.ErpDimValue == erpDimValue)
                        .FirstOrDefaultAsync();
                    return partner?.Id;
                }
                return null;

            case "orgunit":
            case "organizationhierarchy":
                var orgUnit = await context.Set<OrganizationHierarchy>()
                    .Where(o => o.Code == uniqueId)
                    .FirstOrDefaultAsync();
                return orgUnit?.Id;

            case "contact":
                var contact = await context.Set<Contact>()
                    .Where(c => c.Email == uniqueId)
                    .FirstOrDefaultAsync();
                return contact?.Id;

            default:
                return null;
        }
    }

    // Helper method to get entity name for display
    private async Task<string?> GetEntityNameAsync(string entityType, int entityId)
    {
        switch (entityType.ToLower())
        {
            case "country":
                var country = await context.Set<Country>()
                    .Where(c => c.Id == entityId)
                    .FirstOrDefaultAsync();
                return country?.Name;

            case "partner":
            case "organization":
                var partner = await context.Set<Partner>()
                    .Where(p => p.Id == entityId)
                    .FirstOrDefaultAsync();
                return partner?.Name;

            case "orgunit":
            case "organizationhierarchy":
                var orgUnit = await context.Set<OrganizationHierarchy>()
                    .Where(o => o.Id == entityId)
                    .FirstOrDefaultAsync();
                return orgUnit?.Name;

            case "contact":
                var contact = await context.Set<Contact>()
                    .Where(c => c.Id == entityId)
                    .FirstOrDefaultAsync();
                return contact != null ? $"{contact.FirstName} {contact.LastName}" : null;

            default:
                return null;
        }
    }

    // Helper method to get display value from artifact response
    private string? GetDisplayValue(EntityArtifactResponse artifact)
    {
        var dataType = artifact.DataTypeName?.ToLower();
        
        switch (dataType)
        {
            case "string":
            case "text":
                return artifact.ValueText;
            
            case "number":
            case "numeric":
            case "decimal":
                return artifact.ValueNumber?.ToString();
            
            case "boolean":
            case "bool":
                return artifact.ValueBoolean?.ToString();
            
            case "date":
            case "datetime":
                return artifact.ValueDate?.ToString("yyyy-MM-dd");
            
            case "json":
                return artifact.ValueJson;
            
            default:
                return artifact.ValueText;
        }
    }

    // Helper method to parse boolean values with flexible formats
    // Accepts: true, false, TRUE, FALSE, 1, 0
    private bool TryParseFlexibleBoolean(string value, out bool result)
    {
        result = false;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;
        
        var trimmedValue = value.Trim();
        
        // Handle standard boolean strings (case-insensitive)
        if (bool.TryParse(trimmedValue, out result))
            return true;
        
        // Handle numeric representations
        if (trimmedValue == "1")
        {
            result = true;
            return true;
        }
        
        if (trimmedValue == "0")
        {
            result = false;
            return true;
        }
        
        return false;
    }

    // Helper method to escape CSV values
    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape quotes and wrap in quotes if needed
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

