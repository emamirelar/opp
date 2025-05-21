using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace UNOPS.PAO.UNOPSBusiness.Authorization
{
    /// <summary>
    /// Implementation of the permission service that uses the shared permission configuration from JSON
    /// </summary>
    public class SharedPermissionService : IPermissionService
    {
        private readonly PermissionConfiguration _config;
        private readonly ILogger<SharedPermissionService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public SharedPermissionService(
            PermissionConfiguration config,
            ILogger<SharedPermissionService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Check if the user can perform the specified action on the entity
        /// </summary>
        public async Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object? entity = null)
        {
            // Check for the Administrator role first (has all permissions)
            if (user.IsInRole("Administrator"))
            {
                return true;
            }

            // Get allowed roles for this entity action
            var allowedRoles = _config.GetAllowedRolesForEntityAction(entityName, action);
            
            // Check for "ALL" role which means anyone can access
            if (allowedRoles.Contains("ALL"))
            {
                return true;
            }
            
            // Check if user has any of the allowed roles
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
                
            foreach (var role in userRoles)
            {
                if (allowedRoles.Contains(role))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Apply security filters to a query based on user permissions
        /// </summary>
        public async Task<IQueryable<T>> ApplySecurityFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user) where T : class
        {
            // If user is Administrator, return all data
            if (user.IsInRole("Administrator"))
            {
                return query;
            }
            
            // Get entity name from type
            string entityName = typeof(T).Name;
            
            // Check if user has read permission for this entity
            bool canRead = await CanPerformActionAsync(entityName, "Read", user);
            if (!canRead)
            {
                // If user cannot read the entity at all, return empty query
                _logger.LogDebug("User {UserName} does not have Read permission for {EntityName}, returning empty result", 
                    user.Identity?.Name, entityName);
                return query.Take(0);
            }
            
            // Get user roles from claims
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            
            // For simplicity in this implementation, we'll apply some common filter patterns
            // based on entity properties. In a real application, you would implement more
            // sophisticated filtering based on your JSON configuration.
            
            // Check for CreatedBy property to filter by user's own data
            var hasCreatedByProperty = typeof(T).GetProperty("CreatedBy") != null;
            var hasIsPublicProperty = typeof(T).GetProperty("IsPublic") != null;
            
            // If user has Internal or Partner role, show additional data
            bool isInternal = userRoles.Contains("Internal");
            bool isPartner = userRoles.Contains("Partner");
            
            // Apply common filters:
            // 1. User can see their own data (CreatedBy == current user)
            // 2. User can see public data (IsPublic == true)
            // 3. Internal users can see all data
            
            IQueryable<T> filteredQuery = query;
            
            if (!isInternal) // Internal users see all data
            {
                if (hasCreatedByProperty && hasIsPublicProperty)
                {
                    // Filter: CreatedBy == currentUser OR IsPublic == true
                    var parameter = Expression.Parameter(typeof(T), "e");
                    
                    // e => e.CreatedBy == userId
                    var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name;
                    var createdByProperty = Expression.Property(parameter, "CreatedBy");
                    var userIdConstant = Expression.Constant(userIdValue);
                    var createdByExpression = Expression.Equal(createdByProperty, userIdConstant);
                    
                    // e => e.IsPublic == true
                    var isPublicProperty = Expression.Property(parameter, "IsPublic");
                    var trueConstant = Expression.Constant(true);
                    var isPublicExpression = Expression.Equal(isPublicProperty, trueConstant);
                    
                    // e => e.CreatedBy == userId || e.IsPublic == true
                    var orExpression = Expression.OrElse(createdByExpression, isPublicExpression);
                    
                    // Build lambda
                    var lambda = Expression.Lambda<Func<T, bool>>(orExpression, parameter);
                    
                    // Apply filter
                    filteredQuery = query.Where(lambda);
                }
                else if (hasCreatedByProperty)
                {
                    // Only filter by CreatedBy
                    var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name;
                    filteredQuery = query.Where(e => 
                        EF.Property<string>(e, "CreatedBy") == userIdValue);
                }
                else if (hasIsPublicProperty)
                {
                    // Only filter by IsPublic
                    filteredQuery = query.Where(e => 
                        EF.Property<bool>(e, "IsPublic") == true);
                }
            }
            
            return filteredQuery;
        }
        
        /// <summary>
        /// Get readable properties for an entity based on user permissions
        /// </summary>
        public async Task<IEnumerable<string>> GetReadablePropertiesAsync(string entityName, ClaimsPrincipal user)
        {
            // Check if user can read the entity
            bool canRead = await CanPerformActionAsync(entityName, "Read", user);
            if (!canRead)
            {
                return Enumerable.Empty<string>();
            }
            
            // For this implementation, we allow all properties if the user can read the entity
            // In a real implementation, you would define property-level permissions in your JSON config
            
            // Get all entity properties
            Type? entityType = Type.GetType(entityName);
            if (entityType == null)
            {
                // Try common namespaces
                entityType = Type.GetType($"UNOPS.PAO.UNOPSDomain.{entityName}");
                if (entityType == null)
                {
                    entityType = Type.GetType($"UNOPS.PAO.UNOPSDomain.Entities.{entityName}");
                }
            }
            
            if (entityType == null)
            {
                _logger.LogWarning("Could not find type for entity {EntityName}", entityName);
                return Enumerable.Empty<string>();
            }
            
            // Return all property names
            return entityType.GetProperties().Select(p => p.Name);
        }
        
        /// <summary>
        /// Check if the current request's path and method are authorized
        /// </summary>
        public bool IsAuthorizedEndpoint()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogWarning("Cannot check endpoint authorization: HttpContext is null");
                return false;
            }
            
            var path = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;
            var method = context.Request.Method;
            
            // If we're checking the root path, allow access to all
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return true;
            }
            
            // Convert path to lowercase for case-insensitive matching
            path = path.ToLowerInvariant();
            
            // Quick check for Administrator role
            if (context.User.IsInRole("Administrator"))
            {
                return true;
            }
            
            // Try to match the path directly first
            var allowedRoles = _config.GetAllowedRolesForApiEndpoint(path, method);
            
            // If no match, try to match with the /api/ prefix
            if (!allowedRoles.Any() && !path.StartsWith("api/", StringComparison.OrdinalIgnoreCase) && !path.StartsWith("api\\", StringComparison.OrdinalIgnoreCase))
            {
                var apiPath = $"api/{path}";
                allowedRoles = _config.GetAllowedRolesForApiEndpoint(apiPath, method);
            }
            
            // If no match, try with just controller and method
            if (!allowedRoles.Any() && path.Contains("/"))
            {
                // Extract just the controller name (first segment after api/)
                var pathParts = path.Split('/', '\\');
                if (pathParts.Length > 1)
                {
                    var controllerName = pathParts.Length > 0 ? pathParts[0] : string.Empty;
                    if (string.IsNullOrEmpty(controllerName) && pathParts.Length > 1)
                    {
                        controllerName = pathParts[1]; // Skip empty first segment
                    }
                    
                    if (!string.IsNullOrEmpty(controllerName))
                    {
                        var apiControllerPath = $"api/{controllerName}";
                        allowedRoles = _config.GetAllowedRolesForApiEndpoint(apiControllerPath, method);
                    }
                }
            }
            
            // If still no match, try the default permission
            if (!allowedRoles.Any())
            {
                _logger.LogWarning("No permission configuration found for {Path} with method {Method}", path, method);
                
                // By default, if no matching permission is found, deny access
                return false;
            }
            
            // Check for "ALL" role which means anyone can access
            if (allowedRoles.Contains("ALL"))
            {
                return true;
            }
            
            // Check if user has any of the allowed roles
            var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
                
            foreach (var role in userRoles)
            {
                if (allowedRoles.Contains(role))
                {
                    return true;
                }
            }
            
            _logger.LogWarning("User {User} with roles {Roles} does not have any of the required roles {RequiredRoles} for path {Path}",
                context.User.Identity?.Name,
                string.Join(", ", userRoles),
                string.Join(", ", allowedRoles),
                path);
            
            return false;
        }
        
        /// <summary>
        /// Get allowed roles for a specific route (used by frontend)
        /// </summary>
        public List<string> GetAllowedRolesForRoute(string route)
        {
            return _config.GetAllowedRolesForRoute(route).ToList();
        }
        
        /// <summary>
        /// Get the entire permission configuration (used by frontend)
        /// </summary>
        public object GetPermissionConfiguration()
        {
            return new 
            {
                Routes = _config.Routes,
                Entities = _config.Entities
            };
        }
    }
} 