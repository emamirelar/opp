using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models.EntityConfiguration;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSEntityConfigurationManager : BaseUNOPSManager, IUNOPSEntityConfigurationManager
{

    public UNOPSEntityConfigurationManager(
        IMapper mapper, 
        UNOPSAppDbContext context, 
        IConfiguration configuration,
        IPermissionService permissionService) 
        : base(mapper, context, configuration, null, "EntityConfiguration", permissionService)
    {
    }

    public async Task<IEnumerable<Entities>> GetAllEntitiesAsync()
    {
        return await _context.Entities
            .Where(e => e.IsActive && !e.IsDeleted && e.CanManage)
            .OrderBy(e => e.EntityName)
            .ToListAsync();
    }

    public async Task<IEnumerable<EntityManager>> GetAllEntityConfigurationsAsync(ClaimsPrincipal user)
    {
        
        return await _context.EntityManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .Where(em => !em.IsDeleted)
            .OrderBy(em => em.EntityName)
            .ToListAsync();
    }

    public async Task<EntityManager?> GetEntityConfigurationAsync(ClaimsPrincipal user, int id)
    {
        
        return await _context.EntityManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.Id == id && !em.IsDeleted);
    }

    public async Task<EntityManager?> GetEntityConfigurationByNameAsync(ClaimsPrincipal user, string entityName)
    {
        
        return await _context.EntityManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.EntityName == entityName && !em.IsDeleted);
    }

    public async Task<EntityManager> CreateEntityConfigurationAsync(ClaimsPrincipal user, CreateEntityConfigurationRequest request)
    {
        
        // Check if entity name already exists
        var existing = await _context.EntityManagers
            .FirstOrDefaultAsync(em => em.EntityName == request.EntityName && !em.IsDeleted);
        
        if (existing != null)
        {
            throw new BusinessException($"Entity configuration for '{request.EntityName}' already exists");
        }

        var entity = new EntityManager
        {
            EntityName = request.EntityName,
            TableName = request.TableName,
            Description = request.Description,
            IsActive = request.IsActive,
            EnableChangeLog = request.EnableChangeLog,
            Name = request.EntityName,
            Status = Domain.Entities.EntityStatus.Active
        };

        // Set audit data
        var userId = GetCurrentUserId(user);
        entity.SetCreateAuditData(userId);

        _context.EntityManagers.Add(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<EntityManager> UpdateEntityConfigurationAsync(ClaimsPrincipal user, UpdateEntityConfigurationRequest request)
    {
        
        var entity = await _context.EntityManagers
            .FirstOrDefaultAsync(em => em.Id == request.Id && !em.IsDeleted);
        
        if (entity == null)
        {
            throw new BusinessException($"Entity configuration with ID {request.Id} not found");
        }

        // Check if entity name already exists (excluding current entity)
        var existing = await _context.EntityManagers
            .FirstOrDefaultAsync(em => em.EntityName == request.EntityName && em.Id != request.Id && !em.IsDeleted);
        
        if (existing != null)
        {
            throw new BusinessException($"Entity configuration for '{request.EntityName}' already exists");
        }

        entity.EntityName = request.EntityName;
        entity.TableName = request.TableName;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.EnableChangeLog = request.EnableChangeLog;
        entity.Name = request.EntityName;

        // Set audit data
        var userId = GetCurrentUserId(user);
        entity.SetUpdateAuditData(userId);

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteEntityConfigurationAsync(ClaimsPrincipal user, int id)
    {
        
        var entity = await _context.EntityManagers
            .Include(em => em.EntityFields)
            .FirstOrDefaultAsync(em => em.Id == id && !em.IsDeleted);
        
        if (entity == null)
        {
            throw new BusinessException($"Entity configuration with ID {id} not found");
        }

        // Set audit data
        var userId = GetCurrentUserId(user);
        entity.SetDeleteAuditData(userId);
        
        // Soft delete all associated fields
        foreach (var field in entity.EntityFields.Where(f => !f.IsDeleted))
        {
            field.SetDeleteAuditData(userId);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<EntityFieldManager>> GetEntityFieldsAsync(ClaimsPrincipal user, int entityManagerId)
    {
        
        return await _context.EntityFieldManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Where(ef => ef.EntityManagerId == entityManagerId && !ef.IsDeleted)
            .OrderBy(ef => ef.DisplayOrder)
            .ThenBy(ef => ef.FieldName)
            .ToListAsync();
    }

    public async Task<EntityFieldManager?> GetEntityFieldAsync(ClaimsPrincipal user, int fieldId)
    {
        
        return await _context.EntityFieldManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Include(ef => ef.EntityManager)
            .FirstOrDefaultAsync(ef => ef.Id == fieldId && !ef.IsDeleted);
    }

    public async Task<EntityFieldManager> CreateEntityFieldAsync(ClaimsPrincipal user, CreateEntityFieldRequest request)
    {
        
        // Verify the parent entity exists
        var parentEntity = await _context.EntityManagers
            .FirstOrDefaultAsync(em => em.Id == request.EntityManagerId && !em.IsDeleted);
        
        if (parentEntity == null)
        {
            throw new BusinessException($"Entity configuration with ID {request.EntityManagerId} not found");
        }

        // Check if field name already exists for this entity
        var existing = await _context.EntityFieldManagers
            .FirstOrDefaultAsync(ef => ef.EntityManagerId == request.EntityManagerId && 
                                     ef.FieldName == request.FieldName && !ef.IsDeleted);
        
        if (existing != null)
        {
            throw new BusinessException($"Field '{request.FieldName}' already exists for entity '{parentEntity.EntityName}'");
        }

        var field = new EntityFieldManager
        {
            EntityManagerId = request.EntityManagerId,
            FieldName = request.FieldName,
            DataType = request.DataType,
            Description = request.Description,
            IsRequired = request.IsRequired,
            IsActive = request.IsActive,
            EnableChangeLog = request.EnableChangeLog,
            DefaultValue = request.DefaultValue,
            MaxLength = request.MaxLength,
            DisplayOrder = request.DisplayOrder,
            ShowInListView = request.ShowInListView,
            ListViewOrder = request.ShowInListView ? request.ListViewOrder : null,
            RelatedDisplayProperty = request.RelatedDisplayProperty,
            DisplayFieldPath = request.DisplayFieldPath,
            DisplayTemplate = request.DisplayTemplate,
            ListViewLabel = request.ListViewLabel,
            ListViewType = request.ListViewType ?? "text",
            ListViewWidth = request.ListViewWidth,
            ListViewEllipsis = request.ListViewEllipsis ?? false,
            ListViewSortable = request.ListViewSortable ?? true,
            FirstLetterFallbackField = request.FirstLetterFallbackField,
            HelperText = request.HelperText,
            Name = request.FieldName,
            Status = Domain.Entities.EntityStatus.Active
        };

        // Set audit data
        var userId = GetCurrentUserId(user);
        field.SetCreateAuditData(userId);

        // Auto-enable entity change log if field change log is enabled
        if (request.EnableChangeLog && !parentEntity.EnableChangeLog)
        {
            parentEntity.EnableChangeLog = true;
            parentEntity.SetUpdateAuditData(userId);
        }

        _context.EntityFieldManagers.Add(field);
        await _context.SaveChangesAsync();

        return field;
    }

    public async Task<EntityFieldManager> UpdateEntityFieldAsync(ClaimsPrincipal user, UpdateEntityFieldRequest request)
    {
        
        var field = await _context.EntityFieldManagers
            .Include(ef => ef.EntityManager)
            .FirstOrDefaultAsync(ef => ef.Id == request.Id && !ef.IsDeleted);
        
        if (field == null)
        {
            throw new BusinessException($"Entity field with ID {request.Id} not found");
        }

        // Check if field name already exists for this entity (excluding current field)
        var existing = await _context.EntityFieldManagers
            .FirstOrDefaultAsync(ef => ef.EntityManagerId == request.EntityManagerId && 
                                     ef.FieldName == request.FieldName && 
                                     ef.Id != request.Id && !ef.IsDeleted);
        
        if (existing != null)
        {
            throw new BusinessException($"Field '{request.FieldName}' already exists for entity '{field.EntityManager.EntityName}'");
        }

        field.FieldName = request.FieldName;
        field.DataType = request.DataType;
        field.Description = request.Description;
        field.IsRequired = request.IsRequired;
        field.IsActive = request.IsActive;
        field.EnableChangeLog = request.EnableChangeLog;
        field.DefaultValue = request.DefaultValue;
        field.MaxLength = request.MaxLength;
        field.DisplayOrder = request.DisplayOrder;
        field.ShowInListView = request.ShowInListView;
        field.ListViewOrder = request.ShowInListView ? request.ListViewOrder : null;
        field.RelatedDisplayProperty = request.RelatedDisplayProperty;
        field.DisplayFieldPath = request.DisplayFieldPath;
        field.DisplayTemplate = request.DisplayTemplate;
        field.ListViewLabel = request.ListViewLabel;
        field.ListViewType = request.ListViewType ?? "text";
        field.ListViewWidth = request.ListViewWidth;
        field.ListViewEllipsis = request.ListViewEllipsis ?? false;
        field.ListViewSortable = request.ListViewSortable ?? true;
        field.FirstLetterFallbackField = request.FirstLetterFallbackField;
        field.HelperText = request.HelperText;
        field.Name = request.FieldName;

        // Set audit data
        var userId = GetCurrentUserId(user);
        field.SetUpdateAuditData(userId);

        // Auto-enable entity change log if field change log is enabled
        if (request.EnableChangeLog && !field.EntityManager.EnableChangeLog)
        {
            field.EntityManager.EnableChangeLog = true;
            field.EntityManager.SetUpdateAuditData(userId);
        }

        await _context.SaveChangesAsync();
        return field;
    }

    public async Task DeleteEntityFieldAsync(ClaimsPrincipal user, int fieldId)
    {
        
        var field = await _context.EntityFieldManagers
            .FirstOrDefaultAsync(ef => ef.Id == fieldId && !ef.IsDeleted);
        
        if (field == null)
        {
            throw new BusinessException($"Entity field with ID {fieldId} not found");
        }

        // Set audit data
        var userId = GetCurrentUserId(user);
        field.SetDeleteAuditData(userId);

        await _context.SaveChangesAsync();
    }

    public async Task<EntityConfigurationDetailsResponse> GetEntityConfigurationDetailsAsync(ClaimsPrincipal user, string entityName)
    {
        
        var entityConfig = await _context.EntityManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.EntityName == entityName && !em.IsDeleted);

        if (entityConfig == null)
        {
            // Return empty configuration for new entity
            return new EntityConfigurationDetailsResponse
            {
                EntityName = entityName,
                IsActive = true,
                EnableChangeLog = false,
                Fields = new List<EntityFieldConfigurationDto>()
            };
        }

        return new EntityConfigurationDetailsResponse
        {
            Id = entityConfig.Id,
            EntityName = entityConfig.EntityName,
            TableName = entityConfig.TableName,
            Description = entityConfig.Description,
            IsActive = entityConfig.IsActive,
            EnableChangeLog = entityConfig.EnableChangeLog,
            Fields = entityConfig.EntityFields
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.FieldName)
                .Select(f => new EntityFieldConfigurationDto
                {
                    Id = f.Id,
                    FieldName = f.FieldName,
                    DataType = f.DataType,
                    Description = f.Description,
                    IsRequired = f.IsRequired,
                    IsActive = f.IsActive,
                    EnableChangeLog = f.EnableChangeLog,
                    DefaultValue = f.DefaultValue,
                    MaxLength = f.MaxLength,
                    DisplayOrder = f.DisplayOrder,
                    ShowInListView = f.ShowInListView,
                    ListViewOrder = f.ListViewOrder,
                    RelatedDisplayProperty = f.RelatedDisplayProperty,
                    DisplayFieldPath = f.DisplayFieldPath,
                    DisplayTemplate = f.DisplayTemplate,
                    ListViewLabel = f.ListViewLabel,
                    ListViewType = f.ListViewType,
                    ListViewWidth = f.ListViewWidth,
                    ListViewEllipsis = f.ListViewEllipsis,
                    ListViewSortable = f.ListViewSortable,
                    FirstLetterFallbackField = f.FirstLetterFallbackField,
                    HelperText = f.HelperText
                })
                .ToList()
        };
    }

    public async Task<EntityConfigurationDetailsResponse> SaveEntityConfigurationDetailsAsync(ClaimsPrincipal user, SaveEntityConfigurationRequest request)
    {
        
        var userId = GetCurrentUserId(user);
        
        // Get or create entity configuration
        var entityConfig = await _context.EntityManagers
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.EntityName == request.EntityName && !em.IsDeleted);

        if (entityConfig == null)
        {
            // Create new entity configuration
            
            entityConfig = new EntityManager
            {
                EntityName = request.EntityName,
                TableName = request.EntityName, // Default table name to entity name
                Description = request.Description,
                IsActive = true,
                Name = request.EntityName,
                Status = Domain.Entities.EntityStatus.Active
            };
            
            entityConfig.SetCreateAuditData(userId);
            _context.EntityManagers.Add(entityConfig);
            await _context.SaveChangesAsync(); // Save to get the ID
        }
        else
        {
            // Update existing entity configuration
            entityConfig.Description = request.Description;
            entityConfig.SetUpdateAuditData(userId);
        }

        // Handle field operations
        var existingFieldIds = entityConfig.EntityFields.Select(f => f.Id).ToHashSet();
        var requestFieldIds = request.Fields.Where(f => f.Id.HasValue).Select(f => f.Id!.Value).ToHashSet();
        
        // Delete fields that are no longer in the request
        var fieldsToDelete = entityConfig.EntityFields.Where(f => !requestFieldIds.Contains(f.Id));
        foreach (var field in fieldsToDelete)
        {
            field.SetDeleteAuditData(userId);
        }

        // Update or create fields
        foreach (var fieldDto in request.Fields)
        {
            if (fieldDto.Id.HasValue)
            {
                // Update existing field
                var existingField = entityConfig.EntityFields.FirstOrDefault(f => f.Id == fieldDto.Id.Value);
                if (existingField != null)
                {
                    existingField.FieldName = fieldDto.FieldName;
                    existingField.DataType = fieldDto.DataType;
                    existingField.Description = fieldDto.Description;
                    existingField.IsRequired = fieldDto.IsRequired;
                    existingField.IsActive = fieldDto.IsActive;
                    existingField.EnableChangeLog = fieldDto.EnableChangeLog;
                    existingField.DefaultValue = fieldDto.DefaultValue;
                    existingField.MaxLength = fieldDto.MaxLength;
                    existingField.DisplayOrder = fieldDto.DisplayOrder;
                    existingField.ShowInListView = fieldDto.ShowInListView;
                    existingField.ListViewOrder = fieldDto.ShowInListView ? fieldDto.ListViewOrder : null;
                    existingField.RelatedDisplayProperty = fieldDto.RelatedDisplayProperty;
                    existingField.DisplayFieldPath = fieldDto.DisplayFieldPath;
                    existingField.DisplayTemplate = fieldDto.DisplayTemplate;
                    existingField.ListViewLabel = fieldDto.ListViewLabel;
                    existingField.ListViewType = fieldDto.ListViewType ?? "text";
                    existingField.ListViewWidth = fieldDto.ListViewWidth;
                    existingField.ListViewEllipsis = fieldDto.ListViewEllipsis ?? false;
                    existingField.ListViewSortable = fieldDto.ListViewSortable ?? true;
                    existingField.FirstLetterFallbackField = fieldDto.FirstLetterFallbackField;
                    existingField.HelperText = fieldDto.HelperText;
                    existingField.Name = fieldDto.FieldName;
                    existingField.SetUpdateAuditData(userId);
                }
            }
            else
            {
                // Create new field
                
                var newField = new EntityFieldManager
                {
                    EntityManagerId = entityConfig.Id,
                    FieldName = fieldDto.FieldName,
                    DataType = fieldDto.DataType,
                    Description = fieldDto.Description,
                    IsRequired = fieldDto.IsRequired,
                    IsActive = fieldDto.IsActive,
                    EnableChangeLog = fieldDto.EnableChangeLog,
                    DefaultValue = fieldDto.DefaultValue,
                    MaxLength = fieldDto.MaxLength,
                    DisplayOrder = fieldDto.DisplayOrder,
                    ShowInListView = fieldDto.ShowInListView,
                    ListViewOrder = fieldDto.ShowInListView ? fieldDto.ListViewOrder : null,
                    RelatedDisplayProperty = fieldDto.RelatedDisplayProperty,
                    DisplayFieldPath = fieldDto.DisplayFieldPath,
                    DisplayTemplate = fieldDto.DisplayTemplate,
                    ListViewLabel = fieldDto.ListViewLabel,
                    ListViewType = fieldDto.ListViewType ?? "text",
                    ListViewWidth = fieldDto.ListViewWidth,
                    ListViewEllipsis = fieldDto.ListViewEllipsis ?? false,
                    ListViewSortable = fieldDto.ListViewSortable ?? true,
                    FirstLetterFallbackField = fieldDto.FirstLetterFallbackField,
                    HelperText = fieldDto.HelperText,
                    Name = fieldDto.FieldName,
                    Status = Domain.Entities.EntityStatus.Active
                };
                
                newField.SetCreateAuditData(userId);
                _context.EntityFieldManagers.Add(newField);
            }
        }

        // Ensure ListViewOrder values are sequential for list view fields
        var listViewFields = request.Fields
            .Where(f => f.ShowInListView)
            .OrderBy(f => f.ListViewOrder ?? 0)
            .ToList();

        for (int i = 0; i < listViewFields.Count; i++)
        {
            var fieldDto = listViewFields[i];
            var field = fieldDto.Id.HasValue 
                ? entityConfig.EntityFields.FirstOrDefault(f => f.Id == fieldDto.Id.Value)
                : _context.EntityFieldManagers.Local.FirstOrDefault(f => f.FieldName == fieldDto.FieldName && f.EntityManagerId == entityConfig.Id);
            
            if (field != null)
            {
                field.ListViewOrder = i + 1; // Ensure sequential ordering starting from 1
            }
        }

        // Auto-enable entity change log if any field has change log enabled
        var hasFieldChangeLogEnabled = request.Fields.Any(f => f.EnableChangeLog);
        if (hasFieldChangeLogEnabled && !entityConfig.EnableChangeLog)
        {
            entityConfig.EnableChangeLog = true;
            entityConfig.SetUpdateAuditData(userId);
        }

        await _context.SaveChangesAsync();

        // Return updated configuration
        return await GetEntityConfigurationDetailsAsync(user, request.EntityName);
    }

    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal? user = null)
    {
        return await GetEntityConfigurationAsync(user!, entityId);
    }

    public async Task<IEnumerable<RelatedFieldOptionDto>> GetRelatedEntityFieldsAsync(ClaimsPrincipal user, string entityType)
    {   
        var entityConfig = await _context.EntityManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted && f.IsActive))
            .FirstOrDefaultAsync(em => em.EntityName == entityType && !em.IsDeleted);

        if (entityConfig == null)
        {
            return new List<RelatedFieldOptionDto>();
        }

        var basicFields = entityConfig.EntityFields
            .Where(f => f.IsActive && IsDisplayableDataType(f.DataType))
            .Select(f => new RelatedFieldOptionDto
            {
                Value = f.FieldName.ToLowerInvariant(),
                Label = f.FieldName,
                IsTemplate = false,
                FieldPath = $"{entityType.ToLowerInvariant()}.{f.FieldName.ToLowerInvariant()}"
            })
            .ToList();

        // Add common template combinations based on entity type
        var templateFields = GetCommonTemplateFields(entityType);
        
        return basicFields.Concat(templateFields);
    }

    /// <summary>
    /// Get field options for a data type, considering the actual property name in the context entity
    /// </summary>
    public async Task<IEnumerable<RelatedFieldOptionDto>> GetFieldOptionsForDataTypeAsync(ClaimsPrincipal user, string dataType, string contextEntityName)
    {
        
        // Map data type to actual entity property name based on context
        var propertyName = GetPropertyNameForDataType(dataType, contextEntityName);
        
        // Get the target entity type (remove array notation if present)
        var targetEntityType = dataType.Replace("[]", "");
        
        // Get fields for the target entity type
        var fieldOptions = await GetRelatedEntityFieldsAsync(user, targetEntityType);
        
        // Update field paths to use the actual property name
        return fieldOptions.Select(option => new RelatedFieldOptionDto
        {
            Value = option.Value,
            Label = option.Label,
            IsTemplate = option.IsTemplate,
            TemplatePattern = option.TemplatePattern,
            FieldPath = propertyName != null 
                ? option.FieldPath?.Replace($"{targetEntityType.ToLowerInvariant()}.", $"{propertyName}.") 
                : option.FieldPath
        });
    }

    /// <summary>
    /// Map data type to actual property name in the context entity
    /// </summary>
    private string GetPropertyNameForDataType(string dataType, string contextEntityName)
    {
        var baseDataType = dataType.Replace("[]", "");
        
        return (contextEntityName.ToLowerInvariant(), baseDataType.ToLowerInvariant()) switch
        {
            ("partner", "organizationhierarchy") => "partnerOffice",
            ("partner", "partnertree") => "partnerGroup",
            ("contact", "partner") => "partner",
            ("interaction", "contact") => "contact",
            ("interaction", "partner") => "partner",
            _ => baseDataType.ToLowerInvariant()
        };
    }

    private bool IsDisplayableDataType(string dataType)
    {
        var displayableTypes = new[] { "string", "int", "datetime", "boolean", "enum" };
        return displayableTypes.Contains(dataType.ToLowerInvariant());
    }

    private IEnumerable<RelatedFieldOptionDto> GetCommonTemplateFields(string entityType)
    {
        return entityType.ToLowerInvariant() switch
        {
            "partner" => new[]
            {
                new RelatedFieldOptionDto { Value = "name,shortname", Label = "Name (Short Name)", IsTemplate = true, TemplatePattern = "{name} ({shortName})", FieldPath = "partner.name,partner.shortName" },
                new RelatedFieldOptionDto { Value = "shortname,status", Label = "Short Name - Status", IsTemplate = true, TemplatePattern = "{shortName} - {status}", FieldPath = "partner.shortName,partner.status" }
            },
            "contact" => new[]
            {
                new RelatedFieldOptionDto { Value = "firstname,lastname", Label = "First Last", IsTemplate = true, TemplatePattern = "{firstName} {lastName}", FieldPath = "contact.firstName,contact.lastName" },
                new RelatedFieldOptionDto { Value = "lastname,firstname", Label = "Last, First", IsTemplate = true, TemplatePattern = "{lastName}, {firstName}", FieldPath = "contact.lastName,contact.firstName" },
                new RelatedFieldOptionDto { Value = "email,firstname,lastname", Label = "Email (First Last)", IsTemplate = true, TemplatePattern = "{email} ({firstName} {lastName})", FieldPath = "contact.email,contact.firstName,contact.lastName" }
            },
            "partnertree" => new[]
            {
                new RelatedFieldOptionDto { Value = "code,description", Label = "Code - Description", IsTemplate = true, TemplatePattern = "{code} - {description}", FieldPath = "partnerTree.code,partnerTree.description" },
                new RelatedFieldOptionDto { Value = "description,type", Label = "Description (Type)", IsTemplate = true, TemplatePattern = "{description} ({type})", FieldPath = "partnerTree.description,partnerTree.type" }
            },
            "organizationhierarchy" => new[]
            {
                new RelatedFieldOptionDto { Value = "name,code", Label = "Name (Code)", IsTemplate = true, TemplatePattern = "{name} ({code})", FieldPath = "organizationHierarchy.name,organizationHierarchy.code" },
                new RelatedFieldOptionDto { Value = "code,description", Label = "Code - Description", IsTemplate = true, TemplatePattern = "{code} - {description}", FieldPath = "organizationHierarchy.code,organizationHierarchy.description" }
            },
            "interaction" => new[]
            {
                new RelatedFieldOptionDto { Value = "subject,type", Label = "Subject (Type)", IsTemplate = true, TemplatePattern = "{subject} ({type})", FieldPath = "interaction.subject,interaction.type" },
                new RelatedFieldOptionDto { Value = "type,subject", Label = "Type - Subject", IsTemplate = true, TemplatePattern = "{type} - {subject}", FieldPath = "interaction.type,interaction.subject" }
            },
            _ => Array.Empty<RelatedFieldOptionDto>()
        };
    }

    private int GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 1; // Default to 1 if not found
    }

    public async Task<IEnumerable<ListViewColumnDto>> GetEntityListViewConfigurationAsync(ClaimsPrincipal user, string entityName)
    {
        
        var entityConfig = await _context.EntityManagers
            .AsNoTracking() // ✅ Read-only query - no updates will be made
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted && f.IsActive && f.ShowInListView))
            .FirstOrDefaultAsync(em => em.EntityName == entityName && !em.IsDeleted);

        if (entityConfig == null)
        {
            return new List<ListViewColumnDto>();
        }

        return entityConfig.EntityFields
            .Where(f => f.ShowInListView)
            .OrderBy(f => f.ListViewOrder ?? 0)
            .Select(f => new ListViewColumnDto
            {
                Field = f.DisplayFieldPath ?? f.FieldName.ToLowerInvariant(),
                Label = f.ListViewLabel ?? f.FieldName,
                Type = f.ListViewType ?? "text",
                Sortable = f.ListViewSortable ?? true,
                Width = f.ListViewWidth,
                Ellipsis = f.ListViewEllipsis ?? false,
                TemplatePattern = f.DisplayTemplate,
                DisplayFieldPath = f.DisplayFieldPath,
                FirstLetterFallbackField = f.FirstLetterFallbackField,
                HelperText = f.HelperText,
                ThumbnailSize = f.ThumbnailSize,
                ThumbnailShape = f.ThumbnailShape,
                ThumbnailBorder = f.ThumbnailBorder,
                ThumbnailFallback = f.ThumbnailFallback
            })
            .ToList();
    }

    /// <summary>
    /// Exports all entity configurations as a single SQL script
    /// </summary>
    public async Task<string> ExportEntityConfigurationAsSqlAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        var allEntityManagers = await _context.EntityManagers
            .AsNoTracking() // ✅ Read-only query - generating SQL export
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .Where(em => !em.IsDeleted)
            .OrderBy(em => em.EntityName)
            .ToListAsync();

        var allEntityFields = await _context.EntityFieldManagers
            .AsNoTracking() // ✅ Read-only query - generating SQL export
            .Include(efm => efm.EntityManager)
            .Where(efm => !efm.IsDeleted && !efm.EntityManager.IsDeleted)
            .OrderBy(efm => efm.EntityManager.EntityName)
            .ThenBy(efm => efm.DisplayOrder)
            .ToListAsync();

        // Debug logging
        Console.WriteLine($"Found {allEntityManagers.Count} entity managers and {allEntityFields.Count} entity fields for export");

        // Generate combined SQL script
        var combinedSql = GenerateCombinedEntityManagerSql(allEntityManagers, allEntityFields);

        Console.WriteLine($"Generated combined SQL length: {combinedSql.Length} characters");
        return combinedSql;
    }

    /// <summary>
    /// Generates combined SQL script for both EntityManagers and EntityFieldManagers tables
    /// </summary>
    private string GenerateCombinedEntityManagerSql(List<EntityManager> entityManagers, List<EntityFieldManager> entityFields)
    {
        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine("-- Entity Managers and Field Managers Complete Configuration");
        sqlBuilder.AppendLine("-- This script manages both entity manager definitions and their field configurations");
        sqlBuilder.AppendLine();
        sqlBuilder.AppendLine("-- Clear existing data and reset");
        sqlBuilder.AppendLine("TRUNCATE TABLE public.\"EntityManagers\" RESTART IDENTITY CASCADE;");
        sqlBuilder.AppendLine("TRUNCATE TABLE public.\"EntityFieldManagers\" RESTART IDENTITY CASCADE;");
        sqlBuilder.AppendLine();

        // Generate EntityManagers section
        if (entityManagers != null && entityManagers.Count > 0)
        {
            sqlBuilder.AppendLine("-- Insert Entity Managers");
            sqlBuilder.AppendLine("INSERT INTO public.\"EntityManagers\" (");
            sqlBuilder.AppendLine("    \"Id\", \"EntityName\", \"TableName\", \"Description\", \"IsActive\", \"EnableChangeLog\", ");
            sqlBuilder.AppendLine("    \"Name\", \"Status\", \"CreatedBy\", \"CreatedDate\", \"LastModifiedBy\", ");
            sqlBuilder.AppendLine("    \"LastModifiedDate\", \"IsDeleted\", \"DeletedBy\", \"DeletedDate\"");
            sqlBuilder.AppendLine(")");
            sqlBuilder.AppendLine("VALUES ");

            for (int i = 0; i < entityManagers.Count; i++)
            {
                var entity = entityManagers[i];
                var comma = i < entityManagers.Count - 1 ? "," : ";";
                
                sqlBuilder.AppendLine($"    ({i + 1}, '{EscapeSqlString(entity.EntityName)}', '{EscapeSqlString(entity.TableName ?? "")}', '{EscapeSqlString(entity.Description ?? "")}', {entity.IsActive.ToString().ToLower()}, {entity.EnableChangeLog.ToString().ToLower()}, '{EscapeSqlString(entity.Name)}', {(int)entity.Status}, 1, NOW(), 0, NULL, {entity.IsDeleted.ToString().ToLower()}, 0, NULL){comma}");
            }
            sqlBuilder.AppendLine();
        }
        else
        {
            sqlBuilder.AppendLine("-- No entity managers found to export");
            sqlBuilder.AppendLine();
        }

        // Generate EntityFieldManagers section
        sqlBuilder.AppendLine("-- EntityFieldManagers configuration seeding");
        sqlBuilder.AppendLine("-- This section seeds the UI field configuration for all entities");
        sqlBuilder.AppendLine();
        sqlBuilder.AppendLine("DO $$");
        sqlBuilder.AppendLine("DECLARE");
        sqlBuilder.AppendLine("    field_managers_count INTEGER;");
        sqlBuilder.AppendLine("BEGIN");
        sqlBuilder.AppendLine("    RAISE NOTICE 'EntityFieldManagers table cleared, inserting UI configuration...';");
        sqlBuilder.AppendLine();

        if (entityFields != null && entityFields.Count > 0)
        {
            // Group fields by entity
            var fieldsByEntity = entityFields.GroupBy(f => f.EntityManager.EntityName).OrderBy(g => g.Key);

            foreach (var entityGroup in fieldsByEntity)
            {
                var entityName = entityGroup.Key;
                var fields = entityGroup.OrderBy(f => f.DisplayOrder).ToList();
                
                sqlBuilder.AppendLine($"    -- ================================================================");
                sqlBuilder.AppendLine($"    -- {entityName.ToUpper()} ENTITY FIELDS ({fields.Count} fields)");
                sqlBuilder.AppendLine($"    -- ================================================================");
                sqlBuilder.AppendLine("    INSERT INTO public.\"EntityFieldManagers\" (");
                sqlBuilder.AppendLine("        \"EntityManagerId\", \"FieldName\", \"DataType\", \"Description\", \"IsRequired\", \"IsActive\", \"DefaultValue\", \"MaxLength\", \"DisplayOrder\", \"ShowInListView\", \"ListViewOrder\", \"RelatedDisplayProperty\", \"DisplayFieldPath\", \"DisplayTemplate\", \"ListViewLabel\", \"ListViewType\", \"ListViewWidth\", \"ListViewEllipsis\", \"ListViewSortable\", \"FirstLetterFallbackField\", \"HelperText\",");
                sqlBuilder.AppendLine("        \"Name\", \"Status\", \"CreatedBy\", \"CreatedDate\", \"LastModifiedBy\", \"LastModifiedDate\", \"IsDeleted\", \"DeletedBy\", \"DeletedDate\"");
                sqlBuilder.AppendLine("    ) VALUES ");

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];
                    var comma = i < fields.Count - 1 ? "," : ";";
                    
                    // Generate single line format like in EntityManagers_Complete.sql
                    sqlBuilder.AppendLine($"        ({field.EntityManagerId}, '{EscapeSqlString(field.FieldName)}', '{EscapeSqlString(field.DataType)}', '{EscapeSqlString(field.Description ?? "")}', {field.IsRequired.ToString().ToLower()}, {field.IsActive.ToString().ToLower()}, {(field.DefaultValue != null ? $"'{EscapeSqlString(field.DefaultValue)}'" : "NULL")}, {field.MaxLength?.ToString() ?? "NULL"}, {field.DisplayOrder}, {field.ShowInListView.ToString().ToLower()}, {field.ListViewOrder?.ToString() ?? "NULL"}, {(field.RelatedDisplayProperty != null ? $"'{EscapeSqlString(field.RelatedDisplayProperty)}'" : "NULL")}, '{EscapeSqlString(field.DisplayFieldPath ?? "")}', {(field.DisplayTemplate != null ? $"'{EscapeSqlString(field.DisplayTemplate)}'" : "NULL")}, '{EscapeSqlString(field.ListViewLabel ?? "")}', '{EscapeSqlString(field.ListViewType ?? "")}', '{EscapeSqlString(field.ListViewWidth ?? "")}', {(field.ListViewEllipsis?.ToString().ToLower() ?? "false")}, {(field.ListViewSortable?.ToString().ToLower() ?? "false")}, {(field.FirstLetterFallbackField != null ? $"'{EscapeSqlString(field.FirstLetterFallbackField)}'" : "NULL")}, '{EscapeSqlString(field.HelperText ?? "")}', '{EscapeSqlString(field.Name)}', {(int)field.Status}, 1, NOW(), 0, NULL, {field.IsDeleted.ToString().ToLower()}, 0, NULL){comma}");
                }

                sqlBuilder.AppendLine();
            }

            sqlBuilder.AppendLine("    SELECT COUNT(*) INTO field_managers_count FROM public.\"EntityFieldManagers\";");
            sqlBuilder.AppendLine("    RAISE NOTICE 'All EntityFieldManagers setup complete with % total records', field_managers_count;");
        }
        else
        {
            sqlBuilder.AppendLine("    RAISE NOTICE 'No entity field managers found to export';");
        }

        sqlBuilder.AppendLine();
        sqlBuilder.AppendLine("END $$;");

        return sqlBuilder.ToString();
    }


    /// <summary>
    /// Escapes strings for SQL script generation
    /// </summary>
    private string EscapeSqlString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        return input.Replace("'", "''")  // Escape single quotes for SQL
                   .Replace("\\", "\\\\"); // Escape backslashes
    }
} 