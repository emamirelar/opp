namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Claims;
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
            var userInfo = await _context.UserInfos
                .Where(u => u.UserEmail.ToLower() == userEmail.ToLower() && !u.IsDeleted)
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
    protected async Task<T> MapEntityToModelWithPermissionsAsync<T>(T result, ClaimsPrincipal user) where T : class
    {  
        // Add permissions using the helper method from BaseUNOPSManager
        // Only add permissions if user is provided and result has a Permissions property
        if (user != null)
        {
            try
            {
                var entityPermissions = await GetEntityPermissionsAsync(user, _entityName);
                
                // Consolidate permissions from multiple roles into a single permissions object
                // If ANY role grants a permission, it should be true
                var consolidatedPermissions = new EntityPermissionsModel
                {
                    CanRead = entityPermissions.Any(p => p.CanRead),
                    CanCreate = entityPermissions.Any(p => p.CanCreate),
                    CanUpdate = entityPermissions.Any(p => p.CanUpdate),
                    CanDelete = entityPermissions.Any(p => p.CanDelete)
                };

                // Check instance-level access if PermissionService is available and entity has data
                if (_permissionService != null && result != null)
                {
                    try
                    {
                        // Check instance access for each permission type
                        var hasReadInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, result, user, "read");
                        var hasCreateInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, result, user, "create");
                        var hasUpdateInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, result, user, "update");
                        var hasDeleteInstanceAccess = await _permissionService.HasInstanceAccessAsync(_entityName, result, user, "delete");

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
    protected async Task<object> ApplyAccessControlFilters<T>(IQueryable<T> query, ClaimsPrincipal user, string action) where T : class
    {
        if (_permissionService == null)
        {
            // Fallback: return empty list if no permission service available
            return new List<T>();
        }

        return await _permissionService.ApplyAccessControlFiltersAsync(query, user, action, _entityName);
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
} 