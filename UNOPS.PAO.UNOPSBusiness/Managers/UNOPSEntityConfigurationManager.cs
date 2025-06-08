using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSEntityConfigurationManager : BaseUNOPSManager, IUNOPSEntityConfigurationManager
{
    private readonly IPermissionService _permissionService;

    public UNOPSEntityConfigurationManager(
        IMapper mapper, 
        UNOPSAppDbContext context, 
        IConfiguration configuration,
        IPermissionService permissionService) 
        : base(mapper, context, configuration)
    {
        _permissionService = permissionService;
    }

    public async Task<IEnumerable<Entities>> GetAllEntitiesAsync()
    {
        return await _context.Entities
            .Where(e => e.IsActive && !e.IsDeleted)
            .OrderBy(e => e.EntityName)
            .ToListAsync();
    }

    public async Task<IEnumerable<EntityManager>> GetAllEntityConfigurationsAsync(ClaimsPrincipal user)
    {
        await EnsurePermissionAsync(user, "EntityManager", "read");
        
        return await _context.EntityManagers
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .Where(em => !em.IsDeleted)
            .OrderBy(em => em.EntityName)
            .ToListAsync();
    }

    public async Task<EntityManager?> GetEntityConfigurationAsync(ClaimsPrincipal user, int id)
    {
        await EnsurePermissionAsync(user, "EntityManager", "read");
        
        return await _context.EntityManagers
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.Id == id && !em.IsDeleted);
    }

    public async Task<EntityManager?> GetEntityConfigurationByNameAsync(ClaimsPrincipal user, string entityName)
    {
        await EnsurePermissionAsync(user, "EntityManager", "read");
        
        return await _context.EntityManagers
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.EntityName == entityName && !em.IsDeleted);
    }

    public async Task<EntityManager> CreateEntityConfigurationAsync(ClaimsPrincipal user, CreateEntityConfigurationRequest request)
    {
        await EnsurePermissionAsync(user, "EntityManager", "create");
        
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
        await EnsurePermissionAsync(user, "EntityManager", "update");
        
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
        entity.Name = request.EntityName;

        // Set audit data
        var userId = GetCurrentUserId(user);
        entity.SetUpdateAuditData(userId);

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteEntityConfigurationAsync(ClaimsPrincipal user, int id)
    {
        await EnsurePermissionAsync(user, "EntityManager", "delete");
        
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
        await EnsurePermissionAsync(user, "EntityFieldManager", "read");
        
        return await _context.EntityFieldManagers
            .Where(ef => ef.EntityManagerId == entityManagerId && !ef.IsDeleted)
            .OrderBy(ef => ef.DisplayOrder)
            .ThenBy(ef => ef.FieldName)
            .ToListAsync();
    }

    public async Task<EntityFieldManager?> GetEntityFieldAsync(ClaimsPrincipal user, int fieldId)
    {
        await EnsurePermissionAsync(user, "EntityFieldManager", "read");
        
        return await _context.EntityFieldManagers
            .Include(ef => ef.EntityManager)
            .FirstOrDefaultAsync(ef => ef.Id == fieldId && !ef.IsDeleted);
    }

    public async Task<EntityFieldManager> CreateEntityFieldAsync(ClaimsPrincipal user, CreateEntityFieldRequest request)
    {
        await EnsurePermissionAsync(user, "EntityFieldManager", "create");
        
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
            Name = request.FieldName,
            Status = Domain.Entities.EntityStatus.Active
        };

        // Set audit data
        var userId = GetCurrentUserId(user);
        field.SetCreateAuditData(userId);

        _context.EntityFieldManagers.Add(field);
        await _context.SaveChangesAsync();

        return field;
    }

    public async Task<EntityFieldManager> UpdateEntityFieldAsync(ClaimsPrincipal user, UpdateEntityFieldRequest request)
    {
        await EnsurePermissionAsync(user, "EntityFieldManager", "update");
        
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
        field.Name = request.FieldName;

        // Set audit data
        var userId = GetCurrentUserId(user);
        field.SetUpdateAuditData(userId);

        await _context.SaveChangesAsync();
        return field;
    }

    public async Task DeleteEntityFieldAsync(ClaimsPrincipal user, int fieldId)
    {
        await EnsurePermissionAsync(user, "EntityFieldManager", "delete");
        
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
        await EnsurePermissionAsync(user, "EntityManager", "read");
        
        var entityConfig = await _context.EntityManagers
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.EntityName == entityName && !em.IsDeleted);

        if (entityConfig == null)
        {
            // Return empty configuration for new entity
            return new EntityConfigurationDetailsResponse
            {
                EntityName = entityName,
                IsActive = true,
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
                    FirstLetterFallbackField = f.FirstLetterFallbackField
                })
                .ToList()
        };
    }

    public async Task<EntityConfigurationDetailsResponse> SaveEntityConfigurationDetailsAsync(ClaimsPrincipal user, SaveEntityConfigurationRequest request)
    {
        await EnsurePermissionAsync(user, "EntityManager", "update");
        
        var userId = GetCurrentUserId(user);
        
        // Get or create entity configuration
        var entityConfig = await _context.EntityManagers
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(em => em.EntityName == request.EntityName && !em.IsDeleted);

        if (entityConfig == null)
        {
            // Create new entity configuration
            await EnsurePermissionAsync(user, "EntityManager", "create");
            
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
                    existingField.Name = fieldDto.FieldName;
                    existingField.SetUpdateAuditData(userId);
                }
            }
            else
            {
                // Create new field
                await EnsurePermissionAsync(user, "EntityFieldManager", "create");
                
                var newField = new EntityFieldManager
                {
                    EntityManagerId = entityConfig.Id,
                    FieldName = fieldDto.FieldName,
                    DataType = fieldDto.DataType,
                    Description = fieldDto.Description,
                    IsRequired = fieldDto.IsRequired,
                    IsActive = fieldDto.IsActive,
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
                    Name = fieldDto.FieldName,
                    Status = Domain.Entities.EntityStatus.Active
                };
                
                newField.SetCreateAuditData(userId);
                _context.EntityFieldManagers.Add(newField);
            }
        }

        await _context.SaveChangesAsync();

        // Return updated configuration
        return await GetEntityConfigurationDetailsAsync(user, request.EntityName);
    }

    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        return await GetEntityConfigurationAsync(user!, entityId);
    }

    public async Task<IEnumerable<RelatedFieldOptionDto>> GetRelatedEntityFieldsAsync(ClaimsPrincipal user, string entityType)
    {
        await EnsurePermissionAsync(user, "EntityManager", "read");
        
        var entityConfig = await _context.EntityManagers
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
                new RelatedFieldOptionDto { Value = "name,code", Label = "Name (Code)", IsTemplate = true, TemplatePattern = "{name} ({code})", FieldPath = "organizationHierarchy.name,organizationHierarchy.code" }
            },
            "interaction" => new[]
            {
                new RelatedFieldOptionDto { Value = "subject,type", Label = "Subject (Type)", IsTemplate = true, TemplatePattern = "{subject} ({type})", FieldPath = "interaction.subject,interaction.type" },
                new RelatedFieldOptionDto { Value = "type,subject", Label = "Type - Subject", IsTemplate = true, TemplatePattern = "{type} - {subject}", FieldPath = "interaction.type,interaction.subject" }
            },
            _ => Array.Empty<RelatedFieldOptionDto>()
        };
    }

    private async Task EnsurePermissionAsync(ClaimsPrincipal user, string entityName, string action)
    {
        var hasPermission = await _permissionService.CanPerformActionAsync(entityName, action, user);
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException($"Access denied for {action} operation on {entityName}");
        }
    }

    private int GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 1; // Default to 1 if not found
    }

    public async Task<IEnumerable<ListViewColumnDto>> GetEntityListViewConfigurationAsync(ClaimsPrincipal user, string entityName)
    {
        await EnsurePermissionAsync(user, "EntityManager", "read");
        
        var entityConfig = await _context.EntityManagers
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
                FirstLetterFallbackField = f.FirstLetterFallbackField
            })
            .ToList();
    }
} 