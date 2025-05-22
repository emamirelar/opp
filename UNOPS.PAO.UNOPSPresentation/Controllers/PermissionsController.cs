using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Humanizer;

namespace UNOPS.PAO.UNOPSPresentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class PermissionsController : ControllerBase
    {
        private readonly ILogger<PermissionsController> _logger;
        private readonly UserManager<PAOIdentityUser> _userManager;
        private readonly RoleManager<PAOIdentityRole> _roleManager;
        private readonly IPermissionService _permissionService;

        public PermissionsController(
            ILogger<PermissionsController> logger,
            UserManager<PAOIdentityUser> userManager,
            RoleManager<PAOIdentityRole> roleManager,
            IPermissionService permissionService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }

        /// <summary>
        /// Get the entire permission configuration for the frontend
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPermissionConfiguration()
        {
            // Get all roles from the system
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            
            // Return a simplified view of roles for now
            return Ok(new { 
                Roles = roles
            });
        }

        /// <summary>
        /// Get allowed roles for a specific route (used by frontend)
        /// </summary>
        [HttpGet("route/{*route}")]
        public IActionResult GetAllowedRolesForRoute(string route)
        {
            _logger.LogDebug("Getting allowed roles for route {Route}", route);
            var normalizedRoute = NormalizeRoutePath(route);
            
            // For backward compatibility, return Administrator role for all routes
            var roles = new[] { "Administrator" };
            
            return Ok(new { route = normalizedRoute, allowedRoles = roles });
        }

        /// <summary>
        /// Check if the current user has access to a specific route
        /// </summary>
        [HttpGet("check/{*route}")]
        public async Task<IActionResult> CheckRouteAccess(string route)
        {
            _logger.LogDebug("Checking access for route {Route}", route);
            var normalizedRoute = NormalizeRoutePath(route);
            
            // Check if this is an entity route (format: /partnerships/[entity-name])
            string entityName = ExtractEntityNameFromRoute(normalizedRoute);
            if (!string.IsNullOrEmpty(entityName))
            {
                _logger.LogInformation("Checking {Entity} entity permissions for route: {Route}", entityName, normalizedRoute);
                
                // Get all permissions for this entity
                var canRead = await _permissionService.CanPerformActionAsync(entityName, "read", User);
                var canCreate = await _permissionService.CanPerformActionAsync(entityName, "create", User);
                var canUpdate = await _permissionService.CanPerformActionAsync(entityName, "update", User);
                var canDelete = await _permissionService.CanPerformActionAsync(entityName, "delete", User);
                
                // Log the results
                _logger.LogInformation("{Entity} permissions: CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}",
                    entityName, canRead, canCreate, canUpdate, canDelete);
                
                return Ok(new { 
                    route = normalizedRoute, 
                    hasAccess = canRead,
                    entity = entityName,
                    canRead,
                    canCreate,
                    canUpdate,
                    canDelete
                });
            }
            
            // For non-entity routes, use a simple authorization check
            // Administrator can access everything
            if (User.IsInRole("Administrator"))
            {
                return Ok(new { route = normalizedRoute, hasAccess = true });
            }
            
            // For other roles, we check basic access
            // This is a simplified approach compared to the previous implementation
            bool hasAccess = await CheckBasicRouteAccess(normalizedRoute);
            
            return Ok(new { route = normalizedRoute, hasAccess });
        }

        /// <summary>
        /// Basic route access check for non-entity routes
        /// </summary>
        private async Task<bool> CheckBasicRouteAccess(string route)
        {
            // Map common routes to entities for permission checking
            if (route.Contains("/dashboard"))
            {
                // Everyone can access dashboard
                return true;
            }
            
            if (route.Contains("/admin"))
            {
                // Only administrator can access admin routes
                return User.IsInRole("Administrator");
            }
            
            if (route.Contains("/profile"))
            {
                // Everyone can access their own profile
                return true;
            }
            
            // Default to allowing access for authenticated users
            // This is a simplification - you may want a more restrictive default
            return true;
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        [HttpGet("user-roles")]
        public async Task<IActionResult> GetUserRoles()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            
            return Ok(new { 
                userId, 
                userName = user.UserName,
                roles 
            });
        }

        /// <summary>
        /// Get roles for a specific user (admin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserRolesById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            
            return Ok(new { 
                userId, 
                userName = user.UserName,
                roles 
            });
        }

        /// <summary>
        /// Extract the entity name from a route
        /// </summary>
        private string ExtractEntityNameFromRoute(string route)
        {
            // First, check if this is a partnerships route
            if (!route.Contains("/partnerships/"))
            {
                return string.Empty;
            }
            
            // Split the route into segments
            var segments = route.Split('/');
            
            // Find the segment after "partnerships"
            string entityPlural = null;
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].ToLower() == "partnerships" && i + 1 < segments.Length)
                {
                    entityPlural = segments[i + 1];
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(entityPlural))
            {
                return string.Empty;
            }
            
            // Remove any ID or parameters after the entity name
            // Example: "contacts/123" becomes "contacts"
            entityPlural = entityPlural.Split('/')[0];
            
            // Special case handling for compound names with dashes
            if (entityPlural.Contains("-"))
            {
                if (entityPlural.ToLower() == "partnership-agreements")
                {
                    return "Agreement";
                }
            }
            
            // Use Humanizer to convert plural to singular
            string entityName = entityPlural.Singularize(inputIsKnownToBePlural: true);
            
            // Apply Pascal case to ensure proper entity name format
            entityName = entityName.Pascalize();
            
            _logger.LogDebug("Extracted entity name: {EntityName} from route segment: {EntityPlural}", 
                entityName, entityPlural);
                
            return entityName;
        }
        
        /// <summary>
        /// Normalizes a route path for permission checking, removing parameter values
        /// </summary>
        private string NormalizeRoutePath(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                return string.Empty;
            }

            // Ensure route starts with /
            if (!route.StartsWith("/"))
            {
                route = "/" + route;
            }
            
            // Split the route into segments
            var segments = route.Split('/');
            
            // Normalize each segment - for ones that look like parameters (numbers, guids, etc.),
            // replace with a generic parameter marker
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                
                // Skip empty segments
                if (string.IsNullOrEmpty(segment))
                {
                    continue;
                }
                
                // Check if segment is a number
                if (int.TryParse(segment, out _) || 
                    Guid.TryParse(segment, out _))
                {
                    segments[i] = "{id}";
                }
            }
            
            // Join the segments back together
            return string.Join("/", segments);
        }

        /// <summary>
        /// Debug endpoint to check permissions for a given route
        /// </summary>
        [HttpGet("diagnose/{*route}")]
        public async Task<IActionResult> DiagnoseRoutePermissions(string route)
        {
            _logger.LogInformation("Diagnosing permissions for route: {Route}", route);
            
            // Normalize the route
            if (!route.StartsWith("/"))
            {
                route = "/" + route;
            }
            
            var fixedRoute = route.ToLowerInvariant();
            
            // Extract entity from route, if possible
            string entityName = ExtractEntityNameFromRoute(fixedRoute);
            bool isEntityRoute = !string.IsNullOrEmpty(entityName);
            
            // Get permissions for entity if this is an entity route
            var entityPermissions = new Dictionary<string, bool>();
            if (isEntityRoute)
            {
                entityPermissions["canRead"] = await _permissionService.CanPerformActionAsync(entityName, "read", User);
                entityPermissions["canCreate"] = await _permissionService.CanPerformActionAsync(entityName, "create", User);
                entityPermissions["canUpdate"] = await _permissionService.CanPerformActionAsync(entityName, "update", User);
                entityPermissions["canDelete"] = await _permissionService.CanPerformActionAsync(entityName, "delete", User);
            }
            
            // Get user's roles
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            
            // Get all claims for debugging
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            var isAdmin = userRoles.Contains("Administrator");
            var accessGranted = isAdmin || (isEntityRoute && entityPermissions["canRead"]);
            
            return Ok(new
            {
                requestedRoute = route,
                fixedRoute,
                isEntityRoute,
                entityName = isEntityRoute ? entityName : null,
                entityPermissions = isEntityRoute ? entityPermissions : null,
                userInfo = new
                {
                    isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    userName = User.Identity?.Name,
                    authType = User.Identity?.AuthenticationType,
                    userRoles,
                    allClaims,
                    isAdmin
                },
                result = new
                {
                    accessGranted,
                    reason = isAdmin ? "User is Administrator" :
                             (isEntityRoute && entityPermissions["canRead"]) ? "User has permission to read entity" :
                             "User lacks required permission"
                }
            });
        }
    }
} 