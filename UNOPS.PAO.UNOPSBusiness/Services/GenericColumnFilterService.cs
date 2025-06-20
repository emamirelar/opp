using System.Text.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Column-level filtering service using a DENYLIST approach.
/// PropertyFilter JSON specifies columns to RESTRICT, all others are allowed by default.
/// This makes configuration more maintainable as you only specify sensitive/restricted fields.
/// </summary>
public interface IGenericColumnFilterService
{
    /// <summary>
    /// Gets columns that user is allowed to access (all columns except those in the denylist)
    /// </summary>
    Task<IEnumerable<string>> GetAllowedColumnsAsync<T>(ClaimsPrincipal user, string action = "read") where T : class;
    
    /// <summary>
    /// Gets columns that user is allowed to access (all columns except those in the denylist)
    /// </summary>
    Task<IEnumerable<string>> GetAllowedColumnsAsync(ClaimsPrincipal user, string entityName, string action = "read");
    
    /// <summary>
    /// Checks if user can access a specific column (true if not in the denylist)
    /// </summary>
    Task<bool> CanUserAccessColumnAsync<T>(ClaimsPrincipal user, string columnName, string action = "read") where T : class;
    
    /// <summary>
    /// Checks if user can access a specific column (true if not in the denylist)
    /// </summary>
    Task<bool> CanUserAccessColumnAsync(ClaimsPrincipal user, string entityName, string columnName, string action = "read");
    
    /// <summary>
    /// Filters entity properties to only include allowed columns
    /// </summary>
    Task<T> FilterEntityColumns<T>(T entity, ClaimsPrincipal user, string action = "read") where T : class;
    
    /// <summary>
    /// Filters entity properties for a list to only include allowed columns
    /// </summary>
    Task<IEnumerable<T>> FilterEntityColumnsForList<T>(IEnumerable<T> entities, ClaimsPrincipal user, string action = "read") where T : class;
}

public class GenericColumnFilterService : IGenericColumnFilterService
{
    private readonly UNOPSAppDbContext _context;
    private readonly ILogger<GenericColumnFilterService> _logger;

    public GenericColumnFilterService(UNOPSAppDbContext context, ILogger<GenericColumnFilterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetAllowedColumnsAsync<T>(ClaimsPrincipal user, string action = "read") where T : class
    {
        var entityName = GetEntityName<T>();
        return await GetAllowedColumnsAsync(user, entityName, action);
    }

    public async Task<IEnumerable<string>> GetAllowedColumnsAsync(ClaimsPrincipal user, string entityName, string action = "read")
    {
        _logger.LogDebug("Getting allowed columns for entity {EntityName}, action {Action}", entityName, action);

        // For administrators, return all columns
        if (user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            return await GetAllEntityPropertiesAsync(entityName);
        }

        // Get user roles
        var userRoles = GetUserRoles(user);
        
        if (!userRoles.Any())
        {
            _logger.LogWarning("No roles found for user, denying all column access");
            return new List<string>();
        }

        // Get permissions for this entity and user's roles
        var permissions = await _context.EntityPermissions
            .Where(ep => ep.Entity == entityName && userRoles.Contains(ep.Role))
            .ToListAsync();

        if (!permissions.Any())
        {
            _logger.LogDebug("No permissions found for entity {EntityName} and roles {Roles}", entityName, string.Join(", ", userRoles));
            return new List<string>();
        }

        // Check if user has basic permission for this action
        var hasBasicPermission = permissions.Any(p => action.ToLower() switch
        {
            "read" => p.CanRead,
            "create" => p.CanCreate,
            "update" => p.CanUpdate,
            "delete" => p.CanDelete,
            _ => false
        });

        if (!hasBasicPermission)
        {
            _logger.LogDebug("User does not have basic {Action} permission for entity {EntityName}", action, entityName);
            return new List<string>();
        }

        // Get permissions that have basic permission for this action
        var relevantPermissions = permissions.Where(p => action.ToLower() switch
        {
            "read" => p.CanRead,
            "create" => p.CanCreate,
            "update" => p.CanUpdate,
            "delete" => p.CanDelete,
            _ => false
        }).ToList();

        // Collect permitted columns from PropertyFilter (denylist approach)
        var permittedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasColumnFilters = false;

        foreach (var permission in relevantPermissions)
        {
            if (string.IsNullOrEmpty(permission.PropertyFilter)) continue;
            
            try
            {
                var columnFilterConditions = JsonSerializer.Deserialize<ColumnFilterConditions>(permission.PropertyFilter);
                if (columnFilterConditions == null) continue;

                hasColumnFilters = true;

                var actionColumns = action.ToLower() switch
                {
                    "read" => columnFilterConditions.CanRead,
                    "create" => columnFilterConditions.CanCreate,
                    "update" => columnFilterConditions.CanUpdate,
                    "delete" => columnFilterConditions.CanDelete,
                    _ => null
                };

                if (actionColumns != null && actionColumns.Any())
                {
                    foreach (var column in actionColumns)
                    {
                        if (!string.IsNullOrWhiteSpace(column))
                        {
                            permittedColumns.Add(column);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in PropertyFilter for permission {PermissionId}: {PropertyFilter}", permission.Id, permission.PropertyFilter);
            }
        }

        // Get all entity properties
        var allColumns = await GetAllEntityPropertiesAsync(entityName);
        
        // If no column filters defined, return all columns (backward compatibility)
        if (!hasColumnFilters)
        {
            _logger.LogDebug("No column filters defined for entity {EntityName} and action {Action}, allowing all columns", entityName, action);
            return allColumns;
        }

        // Remove restricted columns from all columns (denylist approach)
        var allowedColumns = allColumns.Where(col => permittedColumns.Contains(col)).ToList();

        _logger.LogDebug("Permitted columns for entity {EntityName}, action {Action}: {PermittedColumns}", entityName, action, string.Join(", ", permittedColumns));
        _logger.LogDebug("Allowed columns for entity {EntityName}, action {Action}: {AllowedColumns}", entityName, action, string.Join(", ", allowedColumns));
        
        return allowedColumns;
    }

    public async Task<bool> CanUserAccessColumnAsync<T>(ClaimsPrincipal user, string columnName, string action = "read") where T : class
    {
        var entityName = GetEntityName<T>();
        return await CanUserAccessColumnAsync(user, entityName, columnName, action);
    }

    public async Task<bool> CanUserAccessColumnAsync(ClaimsPrincipal user, string entityName, string columnName, string action = "read")
    {
        var allowedColumns = await GetAllowedColumnsAsync(user, entityName, action);
        return allowedColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<T> FilterEntityColumns<T>(T entity, ClaimsPrincipal user, string action = "read") where T : class
    {
        if (entity == null) return entity;

        var allowedColumns = await GetAllowedColumnsAsync<T>(user, action);
        return FilterEntityByColumns(entity, allowedColumns);
    }

    public async Task<IEnumerable<T>> FilterEntityColumnsForList<T>(IEnumerable<T> entities, ClaimsPrincipal user, string action = "read") where T : class
    {
        if (!entities.Any()) return entities;

        var allowedColumns = await GetAllowedColumnsAsync<T>(user, action);
        return entities.Select(entity => FilterEntityByColumns(entity, allowedColumns));
    }

    #region Private Helper Methods

    private T FilterEntityByColumns<T>(T entity, IEnumerable<string> allowedColumns) where T : class
    {
        if (entity == null) return entity;

        var allowedColumnsSet = new HashSet<string>(allowedColumns, StringComparer.OrdinalIgnoreCase);
        var entityType = typeof(T);
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Create a new instance or clone the entity
        var filteredEntity = CreateFilteredEntity<T>();

        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                // Check if this property is allowed
                if (allowedColumnsSet.Contains(property.Name))
                {
                    try
                    {
                        var value = property.GetValue(entity);
                        property.SetValue(filteredEntity, value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error copying property {PropertyName} for entity {EntityType}", property.Name, entityType.Name);
                    }
                }
                else
                {
                    // Set restricted properties to default values or null
                    SetPropertyToDefault(filteredEntity, property);
                }
            }
        }

        return filteredEntity;
    }

    private T CreateFilteredEntity<T>() where T : class
    {
        try
        {
            return Activator.CreateInstance<T>();
        }
        catch
        {
            // If default constructor is not available, return null
            // In real scenarios, you might want to use a more sophisticated approach
            throw new InvalidOperationException($"Cannot create instance of {typeof(T).Name}. Ensure it has a parameterless constructor.");
        }
    }

    private void SetPropertyToDefault<T>(T entity, PropertyInfo property) where T : class
    {
        try
        {
            if (property.PropertyType.IsValueType)
            {
                // Set value types to their default value
                var defaultValue = Activator.CreateInstance(property.PropertyType);
                property.SetValue(entity, defaultValue);
            }
            else
            {
                // Set reference types to null
                property.SetValue(entity, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting default value for property {PropertyName}", property.Name);
        }
    }

    private async Task<IEnumerable<string>> GetAllEntityPropertiesAsync(string entityName)
    {
        // This is a simplified approach. In a more sophisticated implementation,
        // you might want to get this from database schema or entity metadata
        return entityName.ToLower() switch
        {
            "contact" or "unopscontact" => new[]
            {
                "Id", "FirstName", "LastName", "Email", "PhoneNumber", "Title", "Department",
                "PartnerId", "Partner", "CreatedDate", "LastModifiedDate", "CreatedBy", "LastModifiedBy"
            },
            "partner" or "unopspartner" => new[]
            {
                "Id", "Name", "Code", "Description", "PartnerType", "PartnerOfficeId", "PartnerOffice",
                "CreatedDate", "LastModifiedDate", "CreatedBy", "LastModifiedBy"
            },
            "interaction" or "unopsinteraction" => new[]
            {
                "Id", "Title", "Description", "InteractionType", "InteractionDate", "OrgUnitId", "OrgUnit",
                "CreatedDate", "LastModifiedDate", "CreatedBy", "LastModifiedBy"
            },
            _ => await GetEntityPropertiesFromReflection(entityName)
        };
    }

    private async Task<IEnumerable<string>> GetEntityPropertiesFromReflection(string entityName)
    {
        // As a fallback, try to get properties from loaded assemblies
        // This is a basic implementation - you might want to enhance this
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var type = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase) ||
                                        t.Name.Equals($"UNOPS{entityName}", StringComparison.OrdinalIgnoreCase));

                if (type != null)
                {
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead)
                        .Select(p => p.Name);
                    
                    return properties;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error while reflecting on assembly {AssemblyName}", assembly.FullName);
            }
        }

        // Return empty if no entity type found
        _logger.LogWarning("Could not determine properties for entity {EntityName}", entityName);
        return new List<string>();
    }

    private List<string> GetUserRoles(ClaimsPrincipal user)
    {
        return user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    private string GetEntityName<T>()
    {
        var entityName = typeof(T).Name;
        
        // Clean up common entity prefixes
        if (entityName.StartsWith("UNOPS"))
        {
            entityName = entityName.Substring(5); // Remove "UNOPS" prefix
        }
        
        return entityName;
    }

    #endregion
}

/// <summary>
/// Column filter conditions using a DENYLIST approach.
/// Lists columns that should be RESTRICTED/DENIED for each action.
/// If a list is empty or null, no restrictions apply for that action.
/// All other columns not listed are automatically accessible.
/// </summary>
public class ColumnFilterConditions
{
    /// <summary>
    /// Columns that cannot be read/viewed by the user
    /// </summary>
    public List<string>? CanRead { get; set; }
    
    /// <summary>
    /// Columns that cannot be set when creating new entities
    /// </summary>
    public List<string>? CanCreate { get; set; }
    
    /// <summary>
    /// Columns that cannot be modified when updating entities
    /// </summary>
    public List<string>? CanUpdate { get; set; }
    
    /// <summary>
    /// Columns/fields that cannot be deleted (typically used for soft delete scenarios)
    /// </summary>
    public List<string>? CanDelete { get; set; }
} 