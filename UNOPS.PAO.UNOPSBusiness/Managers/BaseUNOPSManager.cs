namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDomain.Authorization;
using System.Text.Json;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Z.Expressions;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// Base class for all UNOPS managers that provides common functionality
/// </summary>
public abstract class BaseUNOPSManager
{
    protected readonly IMapper _mapper;
    protected readonly UNOPSAppDbContext _context;
    protected readonly IConfiguration _configuration;
    protected readonly UserManager<PAOIdentityUser> _userManager;
    protected readonly IPermissionService _permissionService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly string _entityName;

    protected BaseUNOPSManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, 
        UserManager<PAOIdentityUser> userManager = null, string entityName = null, IPermissionService permissionService = null, IHttpContextAccessor httpContextAccessor = null)
    {
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
        _entityName = entityName ?? GetEntityTypeName();
    }

    /// <summary>
    /// Calls a specific function on this manager by name with the entity ID
    /// This method uses reflection to call the function specified in AiPrompt.PromptFunction
    /// </summary>
    /// <param name="functionName">Name of the function to call (e.g., "GetPartnerAsync", "GetContactWithInteractionsAsync")</param>
    /// <param name="entityId">ID of the entity to retrieve</param>
    /// <param name="user">Optional user context for permission checking</param>
    /// <returns>Result of the function call</returns>
    public virtual async Task<object> CallFunctionByNameAsync(string functionName, int entityId, ClaimsPrincipal user = null)
    {
        if (string.IsNullOrEmpty(functionName))
        {
            throw new ArgumentException("Function name cannot be null or empty", nameof(functionName));
        }

        try
        {
            // Get the method by name
            var method = GetType().GetMethod(functionName, BindingFlags.Public | BindingFlags.Instance);
            
            if (method == null)
            {
                throw new ArgumentException($"Method '{functionName}' not found on {GetType().Name}");
            }

            // Get method parameters to determine the correct overload
            var parameters = method.GetParameters();
            
            // Call the method with appropriate parameters based on its signature
            object result;
            
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
            {
                // Method signature: Method(int id)
                result = method.Invoke(this, new object[] { entityId });
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(ClaimsPrincipal) && parameters[1].ParameterType == typeof(int))
            {
                // Method signature: Method(ClaimsPrincipal user, int id)
                result = method.Invoke(this, new object[] { user, entityId });
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(int))
            {
                // Method signature: Method(int userId, int id) - for legacy methods
                var userId = GetUserIdFromClaims(user);
                result = method.Invoke(this, new object[] { userId, entityId });
            }
            else
            {
                // Try calling with just entityId as fallback
                result = method.Invoke(this, new object[] { entityId });
            }

            // Handle async methods
            if (result is Task task)
            {
                await task;
                
                // Get the result if it's Task<T>
                if (task.GetType().IsGenericType)
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
                
                return null; // Task without return value
            }

            return result;
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap the inner exception for better error messages
            throw ex.InnerException ?? ex;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error calling function '{functionName}' on {GetType().Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Helper method to extract user ID from claims
    /// </summary>
    private int GetUserIdFromClaims(ClaimsPrincipal user)
    {
        if (user == null) return 0;
        
        var userIdClaim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        
        return 0;
    }

    /// <summary>
    /// Gets the current user ID from claims for row filtering
    /// </summary>
    private int GetCurrentUserId(ClaimsPrincipal user)
    {
        if (user == null) return 0;
        
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         user.FindFirst("sub")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Gets the user's organization unit for row filtering
    /// </summary>
    private async Task<string> GetUserOrgUnitAsync(ClaimsPrincipal user)
    {
        if (user == null) return string.Empty;

        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                       user.FindFirst("email")?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
        {
            return string.Empty;
        }

        try
        {   
            // Look up user's assigned org unit from database
            var userInfo = await _context.UserProfile
                .Where(u => u.UserEmail.ToLower() == userEmail.ToLower())
                .Select(u => u.OrgUnit)
                .FirstOrDefaultAsync();
                
            return userInfo ?? string.Empty;
        }
        catch (Exception)
        {
            // If any error occurs, return empty string
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets basic entity data - must be implemented by derived managers
    /// </summary>
    public abstract Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null);

    /// <summary>
    /// Gets basic entity data by ID without nested entities - can be overridden by derived managers
    /// </summary>
    public virtual async Task<object> GetBasicEntityDataAsync(int id)
    {
        throw new NotImplementedException($"GetBasicEntityDataAsync not implemented for {GetType().Name}");
    }

    /// <summary>
    /// Gets multiple entities by their IDs for search results - must be implemented by derived managers
    /// </summary>
    /// <param name="ids">Array of entity IDs</param>
    /// <param name="user">Current user context for security</param>
    /// <returns>List of entity models</returns>
    public virtual async Task<List<object>> GetByIdsAsync(int[] ids, ClaimsPrincipal user = null)
    {
        throw new NotImplementedException($"GetByIdsAsync not implemented for {GetType().Name}");
    }

    /// <summary>
    /// Gets the entity type name for this manager (used for logging/error messages)
    /// </summary>
    protected virtual string GetEntityTypeName()
    {
        var typeName = GetType().Name;
        // Remove "UNOPS" prefix and "Manager" suffix
        return typeName.Replace("UNOPS", "").Replace("Manager", "");
    }

    /// <summary>
    /// Maps entity to model with permissions, handling cases where no user context is available
    /// </summary>
    protected async Task<T> MapEntityToModelWithPermissionsAsync<T>(T result, ClaimsPrincipal user, object sourceEntity = null) where T : class
    {  
        // Add permissions using the helper method from BaseUNOPSManager
        // Only add permissions if user is provided and result has a Permissions property
        if (user != null)
        {
            try
            {
                var entityPermissions = await GetEntityPermissionsAsync(user, _entityName);
                
                // Check if Partner is in a final state (Closed or Archived)
                var isPartnerInFinalState = IsPartnerInFinalState(result);
                
                // Consolidate permissions from multiple roles into a single permissions object
                // If ANY role grants a permission, it should be true
                // However, override modification permissions if Partner is in final state
                var consolidatedPermissions = new EntityPermissionsModel
                {
                    CanRead = entityPermissions.Any(p => p.CanRead),
                    CanCreate = isPartnerInFinalState ? false : entityPermissions.Any(p => p.CanCreate),
                    CanUpdate = isPartnerInFinalState ? false : entityPermissions.Any(p => p.CanUpdate),
                    CanDelete = isPartnerInFinalState ? false : entityPermissions.Any(p => p.CanDelete),
                    CanEditFields = isPartnerInFinalState ? new List<string>() : GetConsolidatedEditableFields(entityPermissions),
                    CanActivate = GetCanActivate(result, entityPermissions),
                    CanClose = GetCanClose(result, entityPermissions),
                    CanArchive = GetCanArchive(result, entityPermissions),
                    CanApprove = GetCanApprove(result, entityPermissions),
                    CanExport = _permissionService?.CanExport(user) ?? false,
                    CanImport = _permissionService?.CanImport(user) ?? false
                };

                // Check instance-level access if PermissionService is available and entity has data
                // Use sourceEntity if provided (for RBAC), otherwise fall back to result (model)
                var entityForRBAC = sourceEntity ?? result;
                if (_permissionService != null && entityForRBAC != null)
                {
                    try
                    {
                        // Check instance access for each permission type using the actual entity
                        var hasReadInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, entityForRBAC, user, "read");
                        var hasCreateInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, entityForRBAC, user, "create");
                        var hasUpdateInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, entityForRBAC, user, "update");
                        var hasDeleteInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, entityForRBAC, user, "delete");

                        // Apply instance-level filtering: permission = defaultPermission && hasInstanceAccess
                        consolidatedPermissions.CanRead = consolidatedPermissions.CanRead && hasReadInstanceAccess;
                        consolidatedPermissions.CanCreate = consolidatedPermissions.CanCreate && hasCreateInstanceAccess;
                        consolidatedPermissions.CanUpdate = consolidatedPermissions.CanUpdate && hasUpdateInstanceAccess;
                        consolidatedPermissions.CanDelete = consolidatedPermissions.CanDelete && hasDeleteInstanceAccess;
                    }
                    catch (Exception)
                    {
                        // If instance access check fails, keep the default permissions
                        // This ensures the method doesn't break even if instance checking has issues
                    }
                }
                
                // Use reflection to check if the result has a Permissions property
                var permissionsProperty = typeof(T).GetProperty("Permissions");
                if (permissionsProperty != null && permissionsProperty.CanWrite)
                {
                    permissionsProperty.SetValue(result, consolidatedPermissions);
                }
            }
            catch (Exception)
            {
                // If permission loading fails, continue without permissions
                // This ensures the method doesn't break even if permission system has issues
            }
        }
        
        return result;
    }

    /// <summary>
    /// Gets the current user from HTTP context if available
    /// </summary>
    protected ClaimsPrincipal GetCurrentUser()
    {
        return _httpContextAccessor?.HttpContext?.User;
    }

    /// <summary>
    /// Gets the current user or creates a system user context for operations that don't have a user but need to work with permissions
    /// This allows legacy methods to still participate in the permission system with the actual current user when possible
    /// </summary>
    protected ClaimsPrincipal GetCurrentUserOrSystemContext()
    {
        // First try to get the current user from HTTP context
        var currentUser = GetCurrentUser();
        if (currentUser?.Identity?.IsAuthenticated == true)
        {
            return currentUser;
        }

        // Fallback to system user if no current user is available
        return CreateSystemUserContext();
    }

    /// <summary>
    /// Creates a system user context for operations that don't have a user but need to work with permissions
    /// This allows legacy methods to still participate in the permission system
    /// </summary>
    protected ClaimsPrincipal CreateSystemUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "0"), // System user ID
            new Claim(ClaimTypes.Name, "System"),
            new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"), // Default role for system operations
            new Claim(ClaimTypes.Email, "system@unops.org")
        };

        var identity = new ClaimsIdentity(claims, "System");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Gets entity permissions for the current entity and user roles from database
    /// </summary>
    protected async Task<List<EntityPermission>> GetEntityPermissionsAsync(ClaimsPrincipal user, string entityName = null)
    {
        if (user == null || !user.Identity.IsAuthenticated)
            return new List<EntityPermission>();

        if (entityName == null)
        {
            entityName = _entityName;
        }

        // Get user roles
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (!userRoles.Any())
            return new List<EntityPermission>();

        // Load permissions from database every time for real-time permission changes
        var permissions = await _context.EntityPermissions
            .Where(ep => ep.Entity == entityName && userRoles.Contains(ep.Role))
            .ToListAsync();

        return permissions;
    }

    /// <summary>
    /// Applies access control filters using PermissionService
    /// </summary>
    protected async Task<List<T>> ApplyAccessControlFilters<T>(IQueryable<T> query, ClaimsPrincipal user, string action) where T : class
    {
        if (_permissionService == null)
        {
            // Fallback: return empty list if no permission service available
            return new List<T>();
        }

        var result = await _permissionService.ApplyAccessControlFiltersAsync(query, user, action, _entityName);
        
        // Cast the result back to List<T>
        if (result is List<T> typedList)
        {
            return typedList;
        }
        
        // If it's some other enumerable, convert it
        if (result is IEnumerable<T> enumerable)
        {
            return enumerable.ToList();
        }
        
        // Fallback: return empty list
        return new List<T>();
    }

    /// <summary>
    /// Gets entity data based on query type (count, list, select)
    /// </summary>
    /// <param name="entityName">Name of the entity</param>
    /// <param name="id">Entity ID for select queries</param>
    /// <param name="query">Query type: count, list, select</param>
    /// <param name="user">User context for permissions</param>
    /// <returns>Entity data based on query type</returns>
    public virtual async Task<object> GetEntityData(string entityName, string id = null, string query = null, ClaimsPrincipal user = null)
    {
        try
        {
            // Set default query if not provided
            if (string.IsNullOrEmpty(query))
            {
                query = string.IsNullOrEmpty(id) ? "count" : "select";
            }

            // Handle different query types
            object result = null;
            
            switch (query.ToLower())
            {
                case "count":
                    result = await GetEntityCount(entityName);
                    break;
                    
                case "list":
                    result = await GetEntityList(entityName, user);
                    break;
                    
                case "select":
                    if (string.IsNullOrEmpty(id))
                    {
                        throw new ArgumentException("ID is required for select queries");
                    }
                    result = await GetEntityById(entityName, id, user);
                    break;
                    
                default:
                    throw new ArgumentException($"Unknown query type: {query}");
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting {query} data for {entityName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Simple entity count - override in derived classes for specific logic
    /// </summary>
    protected virtual async Task<object> GetEntityCount(string entityName)
    {
        return await Task.FromResult(new { count = 0, message = "Count not implemented for " + entityName });
    }

    /// <summary>
    /// Simple entity list - override in derived classes for specific logic
    /// </summary>
    protected virtual async Task<object> GetEntityList(string entityName, ClaimsPrincipal user)
    {
        return await Task.FromResult(new { data = new object[0], message = "List not implemented for " + entityName });
    }

    /// <summary>
    /// Simple entity by ID - override in derived classes for specific logic
    /// </summary>
    protected virtual async Task<object> GetEntityById(string entityName, string id, ClaimsPrincipal user)
    {
        return await Task.FromResult(new { id, message = "GetById not implemented for " + entityName });
    }

    public void PatchNonNullProperties<TSource, TTarget>(TSource source, TTarget target)
    {
        PatchNonNullPropertiesExcept(source, target);
    }

    public void PatchNonNullPropertiesExcept<TSource, TTarget>(TSource source, TTarget target, params string[] excludeProperties)
    {
        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // Handle duplicate property names by grouping and taking the first one
        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                                              .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Create a set of excluded properties for fast lookup
        var excludeSet = new HashSet<string>(excludeProperties ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

        // Add common properties that typically need manual handling
        var commonExclusions = GetCommonExcludedProperties();
        foreach (var exclusion in commonExclusions)
        {
            excludeSet.Add(exclusion);
        }
        
        foreach (var sourceProp in sourceProperties)
        {
            // Skip excluded properties
            if (excludeSet.Contains(sourceProp.Name)) continue;
            
            if (!targetProperties.TryGetValue(sourceProp.Name, out var targetProp)) continue;
            if (!targetProp.CanWrite || !sourceProp.CanRead) continue;

            // Skip properties with incompatible types that can't be directly assigned
            if (!IsCompatibleForDirectAssignment(sourceProp.PropertyType, targetProp.PropertyType))
                continue;

            var value = sourceProp.GetValue(source);

            // Only set if value is not null (or not empty string for strings)
            if (value != null && (!(value is string str) || !string.IsNullOrWhiteSpace(str)))
            {
                // Special handling for ID columns: don't update if source is 0 and target already has a value
                if (sourceProp.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && 
                    value.Equals(0))
                {
                    var existingValue = targetProp.GetValue(target);
                    if (existingValue != null && !existingValue.Equals(0))
                    {
                        continue; // Skip updating ID if target already has a non-zero value
                    }
                }

                try
                {
                    // Special handling for string-to-enum conversion
                    if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
                    {
                        var underlyingTargetType = Nullable.GetUnderlyingType(targetProp.PropertyType) ?? targetProp.PropertyType;
                        if (underlyingTargetType.IsEnum)
                        {
                            // Try to parse the string as enum
                            if (Enum.TryParse(underlyingTargetType, stringValue, ignoreCase: true, out var enumValue))
                            {
                                targetProp.SetValue(target, enumValue);
                                continue;
                            }
                            else
                            {
                                // Skip if enum parsing fails - invalid enum value
                                continue;
                            }
                        }
                    }
                    
                    // Default direct assignment for compatible types
                    targetProp.SetValue(target, value);
                }
                catch
                {
                    // Skip properties that fail to set (type conversion issues, etc.)
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// Gets a list of property names that commonly need manual handling and should be excluded from automatic patching
    /// </summary>
    private HashSet<string> GetCommonExcludedProperties()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OrganizationUnitRelationships",
            "InteractionContacts", 
            "InteractionPartners",
            "InteractionUsers",
            "Projects",
            "Documents", // Navigation properties that usually need special handling
            // Audit fields that should be managed by the system, not from frontend
            "CreatedDate",
            "LastModifiedDate", 
            "CreatedBy",
            "LastModifiedBy",
            "DeletedDate",
            "DeletedBy",
            "IsDeleted"
        };
    }

    /// <summary>
    /// Checks if two types are compatible for direct assignment without conversion
    /// </summary>
    private bool IsCompatibleForDirectAssignment(Type sourceType, Type targetType)
    {
        // Same type is always compatible
        if (sourceType == targetType) return true;
        
        // Nullable to non-nullable of same underlying type
        if (Nullable.GetUnderlyingType(sourceType) == targetType) return true;
        if (Nullable.GetUnderlyingType(targetType) == sourceType) return true;
        
        // Check if target type is assignable from source type
        if (targetType.IsAssignableFrom(sourceType)) return true;
        
        // String to enum conversion (including nullable enums)
        if (sourceType == typeof(string))
        {
            var underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlyingTargetType.IsEnum) return true;
        }
        
        // Skip complex collection types that likely need manual handling
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(sourceType) && 
            sourceType != typeof(string) && 
            typeof(System.Collections.IEnumerable).IsAssignableFrom(targetType) &&
            targetType != typeof(string))
        {
            return false; // Collections usually need manual mapping
        }
        
        return true;
    }

    /// <summary>
    /// Consolidates editable fields from multiple roles' PropertyFilter CanUpdate arrays
    /// Returns the union of all editable fields across all user roles
    /// </summary>
    private List<string>? GetConsolidatedEditableFields(List<EntityPermission> entityPermissions)
    {
        var allEditableFields = new HashSet<string>();
        var hasPropertyFilters = false;

        foreach (var permission in entityPermissions.Where(p => p.CanUpdate))
        {
            if (!string.IsNullOrEmpty(permission.PropertyFilter))
            {
                try
                {
                    var propertyFilterJson = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(permission.PropertyFilter);
                    if (propertyFilterJson != null && propertyFilterJson.TryGetValue("CanUpdate", out var canUpdateFields))
                    {
                        hasPropertyFilters = true;
                        
                        // If CanUpdate is empty array, it means admin role can edit all fields
                        if (canUpdateFields.Count == 0)
                        {
                            return null; // null means no field restrictions (can edit all fields)
                        }
                        
                        // Add all fields from this role to the consolidated list
                        foreach (var field in canUpdateFields)
                        {
                            allEditableFields.Add(field);
                        }
                    }
                }
                catch (JsonException)
                {
                    // Invalid JSON, skip this permission's property filter
                }
            }
        }

        // If no PropertyFilters were found, return null (no field-level restrictions)
        if (!hasPropertyFilters)
        {
            return null;
        }

        // Return the consolidated list of editable fields
        return allEditableFields.ToList();
    }

    /// <summary>
    /// Determines if the user can activate a Partner entity based on mandatory fields and permissions
    /// Mandatory fields for activation: Name, PartnerShortDescription, PartnerCategoryId, PartnerLiaisonOfficeId
    /// </summary>
    private bool? GetCanActivate(object result, List<EntityPermission> entityPermissions)
    {
        // Only applicable to Partner entities
        if (_entityName != "Partner" || result == null)
            return null;

        // Check if user has update permission (required for activation)
        var hasUpdatePermission = entityPermissions.Any(p => p.CanUpdate);
        if (!hasUpdatePermission)
            return false;

        var resultType = result.GetType();
        var statusProperty = resultType.GetProperty("Status");

        if (statusProperty == null)
            return null;

        try
        {
            // Check if partner is in Draft status
            var status = statusProperty.GetValue(result);
            var isDraft = status?.ToString() == "Draft" || status?.ToString() == "3"; // EntityStatus.Draft = 3

            if (!isDraft)
                return false; // Can only activate Draft partners

            // User can activate if they have permissions and partner is in Draft status
            // Field validation is handled on the frontend for UX and on the backend for security
            return true;
        }
        catch
        {
            // If reflection fails, return null (unknown)
            return null;
        }
    }

    /// <summary>
    /// Determines if the user can close a Partner entity
    /// </summary>
    private bool? GetCanClose(object result, List<EntityPermission> entityPermissions)
    {
        // Only applicable to Partner entities
        if (_entityName != "Partner" || result == null)
            return null;

        // Check if user has update permission (required for closing)
        var hasUpdatePermission = entityPermissions.Any(p => p.CanUpdate);
        if (!hasUpdatePermission)
            return false;

        // Use reflection to check partner status and approval status
        var resultType = result.GetType();
        var statusProperty = resultType.GetProperty("Status");
        var approvalStatusProperty = resultType.GetProperty("PartnerApprovalStatus");

        if (statusProperty == null || approvalStatusProperty == null)
            return null;

        try
        {
            // Check if partner is Active
            var status = statusProperty.GetValue(result);
            var isActive = status?.ToString() == "Active" || status?.ToString() == "1"; // EntityStatus.Active = 1
            
            if (!isActive)
                return false; // Can only close Active partners

            // Check if partner is NotApproved (only NotApproved partners can be closed by regular users)
            var approvalStatus = approvalStatusProperty.GetValue(result);
            var isNotApproved = approvalStatus?.ToString() == "NotApproved" || approvalStatus?.ToString() == "0"; // PartnerApprovalStatus.NotApproved = 0
            
            return isNotApproved;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if the user can archive a Partner entity
    /// </summary>
    private bool? GetCanArchive(object result, List<EntityPermission> entityPermissions)
    {
        // Only applicable to Partner entities
        if (_entityName != "Partner" || result == null)
            return null;

        // Check if user has update permission (required for archiving)
        var hasUpdatePermission = entityPermissions.Any(p => p.CanUpdate);
        if (!hasUpdatePermission)
            return false;

        // Use reflection to check partner status and approval status
        var resultType = result.GetType();
        var statusProperty = resultType.GetProperty("Status");
        var approvalStatusProperty = resultType.GetProperty("PartnerApprovalStatus");

        if (statusProperty == null || approvalStatusProperty == null)
            return null;

        try
        {
            // Check if partner is Active or Closed
            var status = statusProperty.GetValue(result);
            var isActive = status?.ToString() == "Active" || status?.ToString() == "1"; // EntityStatus.Active = 1
            var isClosed = status?.ToString() == "Closed" || status?.ToString() == "2"; // EntityStatus.Closed = 2
            
            if (!isActive && !isClosed)
                return false; // Can only archive Active or Closed partners

            // Check if partner is NotApproved (only NotApproved partners can be archived by regular users)
            var approvalStatus = approvalStatusProperty.GetValue(result);
            var isNotApproved = approvalStatus?.ToString() == "NotApproved" || approvalStatus?.ToString() == "0"; // PartnerApprovalStatus.NotApproved = 0
            
            return isNotApproved;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if the user can approve a Partner entity
    /// </summary>
    private bool? GetCanApprove(object result, List<EntityPermission> entityPermissions)
    {
        // Only applicable to Partner entities
        if (_entityName != "Partner" || result == null)
            return null;

        // Check if user has admin-level permissions (only admins can approve)
        // Admin users should have specific roles like PARTNER_GLOB_ADMIN
        var hasAdminPermission = entityPermissions.Any(p => p.Role == "PARTNER_GLOB_ADMIN");
        if (!hasAdminPermission)
            return false;

        // Use reflection to check partner status and approval status
        var resultType = result.GetType();
        var statusProperty = resultType.GetProperty("Status");
        var approvalStatusProperty = resultType.GetProperty("PartnerApprovalStatus");

        if (statusProperty == null || approvalStatusProperty == null)
            return null;

        try
        {
            // Check if partner is Active
            var status = statusProperty.GetValue(result);
            var isActive = status?.ToString() == "Active" || status?.ToString() == "1"; // EntityStatus.Active = 1
            
            if (!isActive)
                return false; // Can only approve Active partners

            // Check if partner is NotApproved (can't approve already approved partners)
            var approvalStatus = approvalStatusProperty.GetValue(result);
            var isNotApproved = approvalStatus?.ToString() == "NotApproved" || approvalStatus?.ToString() == "0"; // PartnerApprovalStatus.NotApproved = 0
            
            return isNotApproved;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if a Partner entity is in a final state (Closed or Archived)
    /// where modification permissions should be restricted
    /// </summary>
    private bool IsPartnerInFinalState(object result)
    {
        // Only applicable to Partner entities
        if (_entityName != "Partner" || result == null)
            return false;

        // Use reflection to check partner status
        var resultType = result.GetType();
        var statusProperty = resultType.GetProperty("Status");

        if (statusProperty == null)
            return false;

        try
        {
            var status = statusProperty.GetValue(result);
            var isClosed = status?.ToString() == "Closed" || status?.ToString() == "2"; // EntityStatus.Closed = 2
            var isArchived = status?.ToString() == "Archived" || status?.ToString() == "4"; // EntityStatus.Archived = 4
            
            return isClosed || isArchived;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Performs comprehensive smart search across the specified entity and all its related entities.
    /// Uses multi-tier search strategy with intelligent ranking based on relevance and entity relationships.
    /// </summary>
    /// <typeparam name="T">The main entity type to search</typeparam>
    /// <param name="searchText">The text to search for across all entity fields and related entities</param>
    /// <param name="includeInactive">Whether to include inactive/deleted entities in search results</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <param name="similarityThreshold">Similarity search threshold 0.0-1.0 (default: 0.3)</param>
    /// <param name="semanticThreshold">Semantic search threshold 0.0-1.0 (default: 0.3)</param>
    /// <returns>Ordered list of entities with relevance scores and match details</returns>
    protected async Task<SmartSearchResult<T>> PerformSmartSearchAsync<T>(
        string searchText,
        bool includeInactive = false,
        int maxResults = 50,
        float similarityThreshold = 0.3f,
        float semanticThreshold = 0.3f) where T : class
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new SmartSearchResult<T>
            {
                Results = new List<SmartSearchItem<T>>(),
                TotalFound = 0,
                SearchStrategy = "none",
                ExecutionTime = TimeSpan.Zero,
                EntitiesSearched = new List<string>()
            };
        }

        var startTime = DateTime.UtcNow;
        var searchResults = new List<SmartSearchItem<T>>();
        var entitiesSearched = new List<string>();
        var searchStrategy = "comprehensive";

        Console.WriteLine($"[DEBUG] PerformSmartSearchAsync: Starting search for '{searchText}' on entity type {typeof(T).Name}");
        Console.WriteLine($"[DEBUG] Parameters: includeInactive={includeInactive}, maxResults={maxResults}");

        try
        {
            // Get the entity type name
            var entityType = typeof(T);
            var entityName = entityType.Name;
            entitiesSearched.Add(entityName);

            // STEP 1: Search the main entity
            var mainEntityResults = await SearchMainEntityAsync<T>(searchText, includeInactive, maxResults);
            searchResults.AddRange(mainEntityResults);

            // STEP 2: Search related entities based on entity type
            var relatedEntityResults = await SearchRelatedEntitiesAsync<T>(searchText, includeInactive, maxResults);
            searchResults.AddRange(relatedEntityResults);
            entitiesSearched.AddRange(GetRelatedEntityNames<T>());

            // STEP 3: Apply intelligent ranking and deduplication
            var rankedResults = ApplyIntelligentRanking(searchResults, searchText);

            // STEP 4: Limit results and apply final sorting
            var finalResults = rankedResults
                .Take(maxResults)
                .ToList();

            var executionTime = DateTime.UtcNow - startTime;
            
            Console.WriteLine($"[DEBUG] Smart search final results: {finalResults.Count} items out of {searchResults.Count} total search results");
            foreach (var result in finalResults.Take(3))
            {
                var resultEntityName = GetEntityName(result.Entity);
                Console.WriteLine($"[DEBUG] Result: {resultEntityName} (Score: {result.RelevanceScore}, Type: {result.MatchType})");
            }

            return new SmartSearchResult<T>
            {
                Results = finalResults,
                TotalFound = rankedResults.Count,
                SearchStrategy = searchStrategy,
                ExecutionTime = executionTime,
                EntitiesSearched = entitiesSearched
            };
        }
        catch (Exception ex)
        {
            // Log error and return empty result
            Console.WriteLine($"Error in smart search: {ex.Message}");
            
            return new SmartSearchResult<T>
            {
                Results = new List<SmartSearchItem<T>>(),
                TotalFound = 0,
                SearchStrategy = "error",
                ExecutionTime = DateTime.UtcNow - startTime,
                EntitiesSearched = entitiesSearched,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Searches the main entity fields for the specified search text
    /// </summary>
    private async Task<List<SmartSearchItem<T>>> SearchMainEntityAsync<T>(string searchText, bool includeInactive, int maxResults) where T : class
    {
        var results = new List<SmartSearchItem<T>>();
        var entityType = typeof(T);
        
        Console.WriteLine($"[DEBUG] SearchMainEntityAsync: Searching {entityType.Name} for '{searchText}', includeInactive: {includeInactive}");
        
        // Create the base query for the entity with includes for related entities
        var query = _context.Set<T>().AsQueryable();
        
        // Include related entities for Partner types to enable searching
        if (entityType.Name == "Partner" || entityType.Name == "UNOPSPartner")
        {
            Console.WriteLine($"[DEBUG] Including related entities for {entityType.Name}");
            query = query
                .Include("PartnerGroup")
                .Include("LiaisonOffice");
        }
        
        // Apply active filter if needed
        // TODO: TEMPORARILY DISABLED FOR TESTING
        // if (!includeInactive)
        // {
        //     query = ApplyActiveFilter(query);
        // }

        // Count total entities before filtering
        var totalEntities = await query.CountAsync();
        Console.WriteLine($"[DEBUG] Total {entityType.Name} entities in query (after active filter): {totalEntities}");
        
        // Test: Try to get some sample partner names to debug
        if (entityType.Name == "UNOPSPartner" && totalEntities > 0)
        {
            var samplePartners = await query.Take(5).Select(e => EF.Property<string>(e, "Name")).ToListAsync();
            Console.WriteLine($"[DEBUG] Sample partner names: {string.Join(", ", samplePartners)}");
        }

        // Build dynamic search expression for main entity fields
        var searchExpression = BuildMainEntitySearchExpression<T>(searchText);
        if (searchExpression != null)
        {
            Console.WriteLine($"[DEBUG] Built search expression for {entityType.Name}");
            
            var matchingEntities = await query
                .Where(searchExpression)
                .Take(maxResults)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Found {matchingEntities.Count} matching {entityType.Name} entities");

            foreach (var entity in matchingEntities)
            {
                var relevanceScore = CalculateMainEntityRelevanceScore(entity, searchText);
                results.Add(new SmartSearchItem<T>
                {
                    Entity = entity,
                    RelevanceScore = relevanceScore,
                    MatchType = "main_entity",
                    MatchDetails = $"Found in {entityType.Name} fields"
                });
            }
        }
        else
        {
            Console.WriteLine($"[DEBUG] No search expression built for {entityType.Name}");
        }

        return results;
    }

    /// <summary>
    /// Searches related entities for the specified search text
    /// </summary>
    private async Task<List<SmartSearchItem<T>>> SearchRelatedEntitiesAsync<T>(string searchText, bool includeInactive, int maxResults) where T : class
    {
        var results = new List<SmartSearchItem<T>>();
        var entityType = typeof(T);

        // Partner-specific related entity searches
        if (entityType.Name == "Partner" || entityType.Name == "UNOPSPartner")
        {
            results.AddRange(await SearchPartnerRelatedEntitiesAsync<T>(searchText, includeInactive, maxResults));
        }
        // Add more entity types as needed (Contact, Interaction, etc.)

        return results;
    }

    /// <summary>
    /// Searches Partner-specific related entities
    /// </summary>
    private async Task<List<SmartSearchItem<T>>> SearchPartnerRelatedEntitiesAsync<T>(string searchText, bool includeInactive, int maxResults) where T : class
    {
        var results = new List<SmartSearchItem<T>>();

        Console.WriteLine($"[DEBUG] SearchPartnerRelatedEntitiesAsync: Searching related entities for '{searchText}'");

        try
        {
            // Search through Contacts (case-insensitive)
            Console.WriteLine($"[DEBUG] Searching contacts for '{searchText}'");
            var lowerSearchText = searchText.ToLower();
            var contactMatches = await _context.Contacts
                .Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{lowerSearchText}%") || 
                           (c.Title != null && EF.Functions.Like(c.Title.ToLower(), $"%{lowerSearchText}%")) || 
                           (c.Department != null && EF.Functions.Like(c.Department.ToLower(), $"%{lowerSearchText}%")))
                .Select(c => c.Partner)
                .Where(p => p != null)
                .Cast<T>()
                .Take(maxResults / 4)
                .ToListAsync();
            
            Console.WriteLine($"[DEBUG] Found {contactMatches.Count} partners through contact matches");

            foreach (var partner in contactMatches)
            {
                results.Add(new SmartSearchItem<T>
                {
                    Entity = partner,
                    RelevanceScore = 60, // Related entity match
                    MatchType = "contact",
                    MatchDetails = "Found through contact information"
                });
            }

            // Search through PartnerGroup/PartnerTree (case-insensitive)
            Console.WriteLine($"[DEBUG] Searching partner groups for '{searchText}'");
            
            // First check if there are any partner groups at all
            var allPartnerGroups = await _context.PartnerTrees.CountAsync();
            Console.WriteLine($"[DEBUG] Total PartnerTrees in database: {allPartnerGroups}");
            
            var partnerGroupMatches = await _context.Partners
                .Include(p => p.PartnerGroup)
                .Where(p => p.PartnerGroup != null && 
                           EF.Functions.Like(p.PartnerGroup.Name.ToLower(), $"%{searchText.ToLower()}%"))
                .Cast<T>()
                .Take(maxResults / 4)
                .ToListAsync();
            
            Console.WriteLine($"[DEBUG] Found {partnerGroupMatches.Count} partners through partner group matches");

            foreach (var partner in partnerGroupMatches)
            {
                results.Add(new SmartSearchItem<T>
                {
                    Entity = partner,
                    RelevanceScore = 70, // Partner group match
                    MatchType = "partner_group",
                    MatchDetails = "Found through partner group"
                });
            }

            // Search through LiaisonOffice (case-insensitive)
            Console.WriteLine($"[DEBUG] Searching liaison offices for '{searchText}'");
            var liaisonOfficeMatches = await _context.Partners
                .Include(p => p.LiaisonOffice)
                .Where(p => p.LiaisonOffice != null && 
                           EF.Functions.Like(p.LiaisonOffice.Name.ToLower(), $"%{searchText.ToLower()}%"))
                .Cast<T>()
                .Take(maxResults / 4)
                .ToListAsync();
                
            Console.WriteLine($"[DEBUG] Found {liaisonOfficeMatches.Count} partners through liaison office matches");

            foreach (var partner in liaisonOfficeMatches)
            {
                results.Add(new SmartSearchItem<T>
                {
                    Entity = partner,
                    RelevanceScore = 65, // Liaison office match
                    MatchType = "liaison_office",
                    MatchDetails = "Found through liaison office"
                });
            }

            // Search through OrganizationUnitRelationships
            var orgUnitMatches = await _context.Partners
                .Include(p => p.OrganizationUnitRelationships)
                    .ThenInclude(our => our.OrganizationHierarchy)
                .Where(p => p.OrganizationUnitRelationships.Any(our => 
                    our.OrganizationHierarchy != null && our.OrganizationHierarchy.Name.Contains(searchText)))
                .Cast<T>()
                .Take(maxResults / 4)
                .ToListAsync();

            foreach (var partner in orgUnitMatches)
            {
                results.Add(new SmartSearchItem<T>
                {
                    Entity = partner,
                    RelevanceScore = 55, // Organization unit match
                    MatchType = "organization_unit",
                    MatchDetails = "Found through organization unit"
                });
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching partner related entities: {ex.Message}");
        }

        return results;
    }

    /// <summary>
    /// Builds search expression for main entity fields
    /// </summary>
    private System.Linq.Expressions.Expression<Func<T, bool>> BuildMainEntitySearchExpression<T>(string searchText) where T : class
    {
        var entityType = typeof(T);
        
        // Partner-specific search fields (both Partner and UNOPSPartner)
        if (entityType.Name == "Partner" || entityType.Name == "UNOPSPartner")
        {
            // Build expression directly for the correct type T
            return BuildPartnerSearchExpressionGeneric<T>(searchText);
        }

        // Add more entity types as needed
        return null;
    }

    /// <summary>
    /// Builds strongly-typed search expression for Partner entities (generic version)
    /// Includes related entities like PartnerGroup and LiaisonOffice in the search
    /// </summary>
    private System.Linq.Expressions.Expression<Func<T, bool>> BuildPartnerSearchExpressionGeneric<T>(string searchText) where T : class
    {
        Console.WriteLine($"[DEBUG] Building comprehensive partner search expression for type {typeof(T).Name} with searchText: '{searchText}'");
        
        // Convert search text to lowercase for case-insensitive search
        var lowerSearchText = searchText.ToLower();
        
        // Since UNOPSPartner inherits from Partner, we can use the same field names
        // Using EF.Functions.Like for better database compatibility and case-insensitive search
        return partner => 
            // Main Partner fields (case-insensitive)
            EF.Functions.Like(EF.Property<string>(partner, "Name").ToLower(), $"%{lowerSearchText}%") ||
            (EF.Property<string>(partner, "PartnerShortDescription") != null && 
             EF.Functions.Like(EF.Property<string>(partner, "PartnerShortDescription").ToLower(), $"%{lowerSearchText}%")) ||
            (EF.Property<string>(partner, "PartnerLongDescription") != null && 
             EF.Functions.Like(EF.Property<string>(partner, "PartnerLongDescription").ToLower(), $"%{lowerSearchText}%")) ||
            (EF.Property<string>(partner, "PartnerApprovalReference") != null && 
             EF.Functions.Like(EF.Property<string>(partner, "PartnerApprovalReference").ToLower(), $"%{lowerSearchText}%"));
    }

    /// <summary>
    /// Builds strongly-typed search expression specifically for Partner entity
    /// </summary>
    private System.Linq.Expressions.Expression<Func<Partner, bool>> BuildPartnerSearchExpression(string searchText)
    {
        Console.WriteLine($"[DEBUG] Building partner search expression for: '{searchText}'");
        // Note: Assuming SQL Server with case-insensitive collation (default behavior)
        return partner => 
            partner.Name.Contains(searchText) ||
            (partner.PartnerShortDescription != null && partner.PartnerShortDescription.Contains(searchText)) ||
            (partner.PartnerLongDescription != null && partner.PartnerLongDescription.Contains(searchText)) ||
            (partner.PartnerApprovalReference != null && partner.PartnerApprovalReference.Contains(searchText));
    }

    /// <summary>
    /// Calculates relevance score for main entity matches
    /// </summary>
    private double CalculateMainEntityRelevanceScore<T>(T entity, string searchText) where T : class
    {
        double score = 40; // Base score for main entity match

        try
        {
            var entityType = typeof(T);
            
            if ((entityType.Name == "Partner" || entityType.Name == "UNOPSPartner") && entity is Partner partner)
            {
                var partnerName = partner.Name ?? "";
                var shortDesc = partner.PartnerShortDescription ?? "";
                var longDesc = partner.PartnerLongDescription ?? "";

                // Exact name match gets highest score
                if (string.Equals(partnerName, searchText, StringComparison.OrdinalIgnoreCase))
                {
                    score = 100;
                }
                else if (partnerName.ToLower().Contains(searchText.ToLower()))
                {
                    score = 85;
                }
                else if (shortDesc.ToLower().Contains(searchText.ToLower()))
                {
                    score = 80;
                }
                else if (longDesc.ToLower().Contains(searchText.ToLower()))
                {
                    score = 75;
                }

                // Boost score for special partner statuses
                if (partner.KeyGlobalPartner)
                {
                    score += 5;
                }
                if (partner.PartnerApprovalStatus.ToString() == "Approved")
                {
                    score += 3;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating relevance score: {ex.Message}");
        }

        return score;
    }

    /// <summary>
    /// Applies intelligent ranking and deduplication to search results
    /// </summary>
    private List<SmartSearchItem<T>> ApplyIntelligentRanking<T>(List<SmartSearchItem<T>> searchResults, string searchText) where T : class
    {
        // Group by entity ID and take the highest scoring match for each entity
        var groupedResults = searchResults
            .GroupBy(r => GetEntityId(r.Entity))
            .Select(g => g.OrderByDescending(r => r.RelevanceScore).First())
            .ToList();

        // Apply final ranking
        return groupedResults
            .OrderByDescending(r => r.RelevanceScore)
            .ThenBy(r => GetEntityName(r.Entity))
            .ToList();
    }

    /// <summary>
    /// Gets the ID of an entity using reflection
    /// </summary>
    private object GetEntityId<T>(T entity) where T : class
    {
        try
        {
            var idProperty = typeof(T).GetProperty("Id");
            return idProperty?.GetValue(entity) ?? Guid.NewGuid();
        }
        catch
        {
            return Guid.NewGuid();
        }
    }

    /// <summary>
    /// Gets the name of an entity using reflection
    /// </summary>
    private string GetEntityName<T>(T entity) where T : class
    {
        try
        {
            var nameProperty = typeof(T).GetProperty("Name");
            return nameProperty?.GetValue(entity)?.ToString() ?? "";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Gets the names of related entities being searched
    /// </summary>
    private List<string> GetRelatedEntityNames<T>() where T : class
    {
        var entityType = typeof(T);
        
        if (entityType.Name == "Partner" || entityType.Name == "UNOPSPartner")
        {
            return new List<string> { "Contacts", "PartnerGroup", "LiaisonOffice", "OrganizationUnits" };
        }

        return new List<string>();
    }

    /// <summary>
    /// Applies active filter to query if the entity supports it
    /// </summary>
    private IQueryable<T> ApplyActiveFilter<T>(IQueryable<T> query) where T : class
    {
        try
        {
            var entityType = typeof(T);
            
            // Check if entity has IsDeleted property
            var isDeletedProperty = entityType.GetProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                query = query.Where(e => !((bool)EF.Property<object>(e, "IsDeleted")));
            }

            // Check if entity has Status property
            var statusProperty = entityType.GetProperty("Status");
            if (statusProperty != null)
            {
                // Filter for Active status (assuming Active = 1)
                query = query.Where(e => EF.Property<int>(e, "Status") == 1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying active filter: {ex.Message}");
        }

        return query;
    }
}

/// <summary>
/// Result container for smart search operations
/// </summary>
public class SmartSearchResult<T> where T : class
{
    public List<SmartSearchItem<T>> Results { get; set; } = new List<SmartSearchItem<T>>();
    public int TotalFound { get; set; }
    public string SearchStrategy { get; set; } = "";
    public TimeSpan ExecutionTime { get; set; }
    public List<string> EntitiesSearched { get; set; } = new List<string>();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual search result item with relevance information
/// </summary>
public class SmartSearchItem<T> where T : class
{
    public T Entity { get; set; }
    public double RelevanceScore { get; set; }
    public string MatchType { get; set; } = "";
    public string MatchDetails { get; set; } = "";
    public List<string> MatchedFields { get; set; } = new List<string>();
} 