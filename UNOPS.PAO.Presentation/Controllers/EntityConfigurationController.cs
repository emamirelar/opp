using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class EntityConfigurationController : BaseController
{
    private readonly IUNOPSEntityConfigurationManager _manager;

    public EntityConfigurationController(
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<EntityConfigurationController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
    }

    /// <summary>
    /// Get all available entities for dropdown selection
    /// </summary>
    [HttpGet(APIDictionary.EntityList)]
    public async Task<ActionResult> GetEntities()
    {
        try
        {
            var entities = await _manager.GetAllEntitiesAsync();
            return Ok(entities.Select(e => new { e.Id, e.EntityName, e.IsActive }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entities list");
            return StatusCode(500, new { error = "Failed to retrieve entities" });
        }
    }

    /// <summary>
    /// Get entity configuration details by entity name for the frontend screen
    /// </summary>
    [HttpGet(APIDictionary.EntityConfiguration + "/{entityName}")]
    public async Task<ActionResult> GetEntityConfiguration(string entityName)
    {
        try
        {
            var result = await _manager.GetEntityConfigurationDetailsAsync(User, entityName);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for entity configuration: {EntityName}", entityName);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity configuration for: {EntityName}", entityName);
            return StatusCode(500, new { error = "Failed to retrieve entity configuration" });
        }
    }

    /// <summary>
    /// Save entity configuration (entity description and field configurations)
    /// </summary>
    [HttpPost(APIDictionary.EntityConfiguration + "/{entityName}/save")]
    public async Task<ActionResult> SaveEntityConfiguration(string entityName, [FromBody] SaveEntityConfigurationRequest request)
    {
        try
        {
            var result = await _manager.SaveEntityConfigurationDetailsAsync(User, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for saving entity configuration: {EntityName}", request.EntityName);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving entity configuration for: {EntityName}", request.EntityName);
            return StatusCode(500, new { error = "Failed to save entity configuration" });
        }
    }

    /// <summary>
    /// Get all entity configurations (for admin purposes)
    /// </summary>
    [HttpGet(APIDictionary.EntityConfiguration)]
    public async Task<ActionResult> GetAllEntityConfigurations()
    {
        try
        {
            var configurations = await _manager.GetAllEntityConfigurationsAsync(User);
            return Ok(configurations);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for retrieving all entity configurations");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all entity configurations");
            return StatusCode(500, new { error = "Failed to retrieve entity configurations" });
        }
    }

    /// <summary>
    /// Create a new entity configuration
    /// </summary>
    [HttpPost(APIDictionary.EntityConfigurationCreate)]
    [AccessControlled(EntityTypes.EntityConfiguration, "create")]
    public async Task<ActionResult> CreateEntityConfiguration([FromBody] CreateEntityConfigurationRequest request)
    {
        try
        {
            var result = await _manager.CreateEntityConfigurationAsync(User, request);
            return StatusCode(201, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for creating entity configuration: {EntityName}", request.EntityName);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity configuration: {EntityName}", request.EntityName);
            return StatusCode(500, new { error = "Failed to create entity configuration" });
        }
    }

    /// <summary>
    /// Update an entity configuration
    /// </summary>
    [HttpPut(APIDictionary.EntityConfiguration + "/{id}")]
    [AccessControlled(EntityTypes.EntityConfiguration, "update")]
    public async Task<ActionResult> UpdateEntityConfiguration(int id, [FromBody] UpdateEntityConfigurationRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(new { error = "ID mismatch" });
        }
        try
        {
            var result = await _manager.UpdateEntityConfigurationAsync(User, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for updating entity configuration: {Id}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity configuration: {Id}", id);
            return StatusCode(500, new { error = "Failed to update entity configuration" });
        }
    }

    /// <summary>
    /// Delete an entity configuration
    /// </summary>
    [HttpDelete(APIDictionary.EntityConfiguration + "/{id}")]
    [AccessControlled(EntityTypes.EntityConfiguration, "delete")]
    public async Task<ActionResult> DeleteEntityConfiguration(int id)
    {
        try
        {
            await _manager.DeleteEntityConfigurationAsync(User, id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for deleting entity configuration: {Id}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity configuration: {Id}", id);
            return StatusCode(500, new { error = "Failed to delete entity configuration" });
        }
    }

    /// <summary>
    /// Get fields for a specific entity configuration
    /// </summary>
    [HttpGet(APIDictionary.EntityConfiguration + "/{entityManagerId}/fields")]
    [AccessControlled(EntityTypes.EntityConfiguration, "read")]
    public async Task<ActionResult> GetEntityFields(int entityManagerId)
    {
        try
        {
            var fields = await _manager.GetEntityFieldsAsync(User, entityManagerId);
            return Ok(fields);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for retrieving entity fields: {EntityManagerId}", entityManagerId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity fields for: {EntityManagerId}", entityManagerId);
            return StatusCode(500, new { error = "Failed to retrieve entity fields" });
        }
    }

    /// <summary>
    /// Create a new entity field
    /// </summary>
    [HttpPost(APIDictionary.EntityFieldCreate)]
    [AccessControlled(EntityTypes.EntityConfiguration, "create")]
    public async Task<ActionResult> CreateEntityField([FromBody] CreateEntityFieldRequest request)
    {
        try
        {
            var result = await _manager.CreateEntityFieldAsync(User, request);
            return StatusCode(201, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for creating entity field: {FieldName}", request.FieldName);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity field: {FieldName}", request.FieldName);
            return StatusCode(500, new { error = "Failed to create entity field" });
        }
    }

    /// <summary>
    /// Update an entity field
    /// </summary>
    [HttpPut(APIDictionary.EntityField + "/{id}")]
    [AccessControlled(EntityTypes.EntityConfiguration, "update")]
    public async Task<ActionResult> UpdateEntityField(int id, [FromBody] UpdateEntityFieldRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(new { error = "ID mismatch" });
        }
        try
        {
            var result = await _manager.UpdateEntityFieldAsync(User, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for updating entity field: {Id}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity field: {Id}", id);
            return StatusCode(500, new { error = "Failed to update entity field" });
        }
    }

    /// <summary>
    /// Delete an entity field
    /// </summary>
    [HttpDelete(APIDictionary.EntityField + "/{id}")]
    [AccessControlled(EntityTypes.EntityConfiguration, "delete")]
    public async Task<ActionResult> DeleteEntityField(int id)
    {
        try
        {
            await _manager.DeleteEntityFieldAsync(User, id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for deleting entity field: {Id}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity field: {Id}", id);
            return StatusCode(500, new { error = "Failed to delete entity field" });
        }
    }

    /// <summary>
    /// Get available fields for a related entity type (for relationship field configuration)
    /// </summary>
    [HttpGet(APIDictionary.EntityConfiguration + "/related-fields/{entityType}")]
    [AccessControlled(EntityTypes.EntityConfiguration, "read")]
    public async Task<ActionResult> GetRelatedEntityFields(string entityType)
    {
        try
        {
            var fields = await _manager.GetRelatedEntityFieldsAsync(User, entityType);
            return Ok(fields);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for retrieving related entity fields: {EntityType}", entityType);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving related entity fields for: {EntityType}", entityType);
            return StatusCode(500, new { error = "Failed to retrieve related entity fields" });
        }
    }

    /// <summary>
    /// Get field options for a specific data type in the context of an entity
    /// </summary>
    [HttpGet(APIDictionary.EntityConfiguration + "/field-options/{dataType}/{contextEntityName}")]
    [AccessControlled(EntityTypes.EntityConfiguration, "read")]
    public async Task<ActionResult> GetFieldOptionsForDataType(string dataType, string contextEntityName)
    {
        try
        {
            var fields = await _manager.GetFieldOptionsForDataTypeAsync(User, dataType, contextEntityName);
            return Ok(fields);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for retrieving field options: {DataType} in {ContextEntity}", dataType, contextEntityName);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving field options for data type: {DataType} in {ContextEntity}", dataType, contextEntityName);
            return StatusCode(500, new { error = "Failed to retrieve field options" });
        }
    }

    /// <summary>
    /// Get list view configuration for an entity (for dynamic column generation)
    /// </summary>
    [HttpGet(APIDictionary.EntityConfiguration + "/{entityName}/list-view")]
    public async Task<ActionResult> GetEntityListViewConfiguration(string entityName)
    {
        try
        {
            var listViewConfig = await _manager.GetEntityListViewConfigurationAsync(User, entityName);
            return Ok(listViewConfig);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied for retrieving list view configuration: {EntityName}", entityName);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving list view configuration for: {EntityName}", entityName);
            return StatusCode(500, new { error = "Failed to retrieve list view configuration" });
        }
    }
} 