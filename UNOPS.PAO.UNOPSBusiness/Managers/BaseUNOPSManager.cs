namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.Models;

/// <summary>
/// Base class for all UNOPS managers that provides common AI prompt functionality
/// </summary>
public abstract class BaseUNOPSManager
{
    protected readonly IMapper _mapper;
    protected readonly UNOPSAppDbContext _context;
    protected readonly IConfiguration _configuration;
    protected readonly UserManager<PAOIdentityUser> _userManager;
    protected readonly IBusinessSecurityService _securityService;

    protected BaseUNOPSManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, 
        UserManager<PAOIdentityUser> userManager = null, IBusinessSecurityService securityService = null)
    {
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
        _securityService = securityService;
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
    /// Helper method to get entity permissions for frontend UI
    /// This method provides a single call to get all permissions for an entity
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entity">The entity to check permissions for</param>
    /// <param name="user">User context</param>
    /// <returns>EntityPermissionsModel with all permissions</returns>
    protected async Task<EntityPermissionsModel> GetEntityPermissionsAsync<T>(T entity, ClaimsPrincipal user) where T : class
    {
        if (_securityService == null)
        {
            // Return default permissions if no security service available
            return new EntityPermissionsModel
            {
                CanRead = true,
                CanCreate = false,
                CanUpdate = false,
                CanDelete = false
            };
        }

        return await _securityService.GetEntityPermissionsForUIAsync(entity, user);
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
                        return new { error = "ID is required for select query" };
                    }
                    result = await GetEntityById(entityName, id, user);
                    break;
                    
                default:
                    return new { error = $"Invalid query type: {query}. Supported types are: count, list, select" };
            }

            return result;
        }
        catch (Exception ex)
        {
            return new { error = ex.Message };
        }
    }

    /// <summary>
    /// Creates a ClaimsPrincipal from user ID for permission checking
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>ClaimsPrincipal for the user</returns>
    protected virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalFromUserId(int userId)
    {
        if (_userManager == null)
        {
            // Return a minimal ClaimsPrincipal if UserManager is not available
            var basicClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            return new ClaimsPrincipal(new ClaimsIdentity(basicClaims, "Internal"));
        }

        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                Console.WriteLine($"🐛 User not found with ID: {userId}");
                var fallbackClaims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, "UNOPS_GEN_USER")
                };
                return new ClaimsPrincipal(new ClaimsIdentity(fallbackClaims, "Internal"));
            }

            // Get user's roles and claims from the Identity system
            var roles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            // Create a list of claims
            var claims = new List<Claim>(userClaims)
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsInternal", user.IsInternal.ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create and return the ClaimsPrincipal
            var identity = new ClaimsIdentity(claims, "Internal", ClaimTypes.Name, ClaimTypes.Role);
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🐛 Error creating ClaimsPrincipal for user {userId}: {ex.Message}");
            var fallbackClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "UNOPS_GEN_USER")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(fallbackClaims, "Internal"));
        }
    }

    /// <summary>
    /// Checks if user has permission to access entity
    /// </summary>
    /// <param name="entityName">Name of the entity</param>
    /// <param name="user">User claims principal</param>
    /// <param name="action">Action to check (default: read)</param>
    /// <returns>True if user has permission</returns>
    protected virtual async Task<bool> CheckEntityPermission(string entityName, ClaimsPrincipal user, string action = "read")
    {
        if (_securityService == null)
        {
            // If no security service is available, allow access for backward compatibility
            return true;
        }

        try
        {
            // Use BusinessSecurityService to check permissions
            // Note: This is a general permission check, specific entity access would be checked later
            var permissions = await _securityService.GetEntityPermissionsAsync(user, entityName);
            
            return action.ToLower() switch
            {
                "read" => permissions.CanRead,
                "create" => permissions.CanCreate,
                "update" => permissions.CanUpdate,
                "delete" => permissions.CanDelete,
                _ => false
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🐛 Error checking entity permission: {ex.Message}");
            // Default to allowing access if permission check fails (for backward compatibility)
                         return true;
         }
     }

    /// <summary>
    /// Gets entity data based on query type (count, list, select) - overload that accepts userId
    /// </summary>
    /// <param name="entityName">Name of the entity</param>
    /// <param name="userId">User ID for permission checking</param>
    /// <param name="id">Entity ID for select queries</param>
    /// <param name="query">Query type: count, list, select</param>
    /// <returns>Entity data based on query type</returns>
    public virtual async Task<object> GetEntityData(string entityName, int userId, string id = null, string query = null)
    {
        try
        {
            Console.WriteLine($"🐛 BaseUNOPSManager.GetEntityData called with entityName: {entityName}, userId: {userId}, id: {id}, query: {query}");

            // Create ClaimsPrincipal from userId
            var user = await CreateClaimsPrincipalFromUserId(userId);

            // Check permissions
            bool hasReadPermission = await CheckEntityPermission(entityName, user, "read");
            
            if (!hasReadPermission)
            {
                Console.WriteLine($"🐛 User {userId} does not have read permission for entity {entityName}");
                return new { message = "Not authorized" };
            }

            Console.WriteLine($"🐛 User {userId} has read permission for entity {entityName}");

            // Call the main GetEntityData method with the ClaimsPrincipal
            return await GetEntityData(entityName, id, query, user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🐛 Error in BaseUNOPSManager.GetEntityData: {ex.Message}");
            return new { error = ex.Message };
        }
    }

    /// <summary>
    /// Gets the total count of entities
    /// </summary>
    protected virtual async Task<object> GetEntityCount(string entityName)
    {
        try
        {
            // Get the DbSet property for this entity type
            var dbSetProperty = _context.GetType()
                .GetProperty($"{entityName}s", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            
            if (dbSetProperty != null)
            {
                var dbSet = dbSetProperty.GetValue(_context) as IQueryable<object>;
                if (dbSet != null)
                {
                    var count = await dbSet.CountAsync();
                    return new { entityName, query = "count", totalCount = count };
                }
            }
            
            return new { entityName, query = "count", totalCount = 0, message = "DbSet not found" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🐛 Error getting count for {entityName}: {ex.Message}");
            return new { error = $"Error getting count: {ex.Message}" };
        }
    }

    /// <summary>
    /// Gets a list of entities with pagination
    /// </summary>
    protected virtual async Task<object> GetEntityList(string entityName, ClaimsPrincipal user)
    {
        try
        {
            // Look for a method that returns a list/pagination of entities
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            
            // Try to find Get{EntityName}sAsync method first
            var asyncListMethod = methods.FirstOrDefault(m => 
                m.Name == $"Get{entityName}sAsync" && 
                m.ReturnType.IsGenericType && 
                m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
            
            if (asyncListMethod != null)
            {
                var parameters = asyncListMethod.GetParameters();
                object result = null;
                
                if (parameters.Length >= 2 && parameters[1].ParameterType.Name.Contains("PaginationRequest"))
                {
                    // Method with pagination - create an instance of PaginationRequest
                    var paginationRequestType = parameters[1].ParameterType;
                    var paginationRequest = Activator.CreateInstance(paginationRequestType);
                    
                    // Set properties if they exist
                    var pageIndexProp = paginationRequestType.GetProperty("PageIndex");
                    var pageSizeProp = paginationRequestType.GetProperty("PageSize");
                    
                    pageIndexProp?.SetValue(paginationRequest, 1);
                    pageSizeProp?.SetValue(paginationRequest, 10);
                    
                    result = asyncListMethod.Invoke(this, new object[] { user, paginationRequest });
                }
                else if (parameters.Length == 1)
                {
                    result = asyncListMethod.Invoke(this, new object[] { user });
                }
                else if (parameters.Length == 0)
                {
                    result = asyncListMethod.Invoke(this, new object[0]);
                }
                
                if (result is Task task)
                {
                    await task;
                    var resultProperty = task.GetType().GetProperty("Result");
                    var taskResult = resultProperty?.GetValue(task);
                    return new { data = taskResult };
                }
            }
            
            return new { data = new List<object>(), message = "No suitable list method found" };
        }
        catch (Exception ex)
        {
            return new { error = $"Error getting list: {ex.Message}" };
        }
    }

    /// <summary>
    /// Gets entity by ID or list of IDs
    /// </summary>
    protected virtual async Task<object> GetEntityById(string entityName, string id, ClaimsPrincipal user)
    {
        try
        {
            // Parse the ID - could be single ID or comma-separated list
            var ids = id.Split(',').Select(i => i.Trim()).ToList();
            
            if (ids.Count == 1 && int.TryParse(ids[0], out int singleId))
            {
                // Single ID - use Get{EntityName}Async method
                var method = GetType().GetMethod($"Get{entityName}Async", 
                    new[] { typeof(int) });
                
                // Try alternative method signatures if the first one doesn't exist
                if (method == null)
                {
                    method = GetType().GetMethod($"Get{entityName}Async", 
                        new[] { typeof(ClaimsPrincipal), typeof(int) });
                }
                
                if (method != null)
                {
                    object result = null;
                    var parameters = method.GetParameters();
                    
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(ClaimsPrincipal))
                    {
                        result = method.Invoke(this, new object[] { user, singleId });
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                    {
                        result = method.Invoke(this, new object[] { singleId });
                    }
                    
                    if (result is Task task)
                    {
                        await task;
                        var resultProperty = task.GetType().GetProperty("Result");
                        var taskResult = resultProperty?.GetValue(task);
                        return new { entityName, query = "select", id = singleId, data = taskResult };
                    }
                }
            }
            else
            {
                // Multiple IDs - would need custom implementation
                var validIds = ids.Where(i => int.TryParse(i, out _)).Select(i => int.Parse(i)).ToList();
                return new { entityName, query = "select", ids = validIds, message = "Multiple ID selection not implemented yet" };
            }
            
            return new { entityName, query = "select", id, data = (object)null, message = "Entity not found or method not available" };
        }
        catch (Exception ex)
        {
            return new { error = $"Error getting entity by ID: {ex.Message}" };
        }
    }
} 