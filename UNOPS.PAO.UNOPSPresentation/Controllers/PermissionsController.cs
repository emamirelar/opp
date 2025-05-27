using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Services;
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
        private readonly IBusinessSecurityService _businessSecurityService;

        public PermissionsController(
            ILogger<PermissionsController> logger,
            UserManager<PAOIdentityUser> userManager,
            RoleManager<PAOIdentityRole> roleManager,
            IPermissionService permissionService,
            IBusinessSecurityService businessSecurityService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
            _businessSecurityService = businessSecurityService;
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
            return Ok(new
            {
                Roles = roles
            });
        }

        /// <summary>
        /// Check if the current user has access to a specific route
        /// </summary>
        [HttpGet("check/{*route}")]
        public async Task<IActionResult> CheckRouteAccess(string route)
        {
            _logger.LogDebug("Checking access for route {Route}", route);
            var normalizedRoute = NormalizeRoutePath(route);

            // DEBUG: Log user information
            var userId = _userManager.GetUserId(User);
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
            var userNameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            _logger.LogInformation("DEBUG - User Info: UserId={UserId}, Email={Email}, NameIdentifier={NameIdentifier}", 
                userId, userEmail, userNameIdentifier);
            _logger.LogInformation("DEBUG - All User Claims: {@Claims}", allClaims);

            // DEBUG: Check if user exists in database
            PAOIdentityUser dbUser = null;
            if (!string.IsNullOrEmpty(userId))
            {
                dbUser = await _userManager.FindByIdAsync(userId);
                _logger.LogInformation("DEBUG - User found in database: {UserFound}, UserName: {UserName}", 
                    dbUser != null, dbUser?.UserName);
                
                if (dbUser != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(dbUser);
                    _logger.LogInformation("DEBUG - User roles from database: {Roles}", string.Join(", ", userRoles));
                }
            }
            else
            {
                _logger.LogWarning("DEBUG - No UserId found in claims");
            }

            // Extract entity name and ID from route
            var (entityName, entityId) = ExtractEntityInfoFromRoute(route);
            
            _logger.LogInformation("DEBUG - Extracted from route: EntityName={EntityName}, EntityId={EntityId}", 
                entityName, entityId);
            
            if (!string.IsNullOrEmpty(entityName))
            {
                _logger.LogInformation("Checking {Entity} entity permissions for route: {Route}", entityName, normalizedRoute);

                try
                {
                    // DEBUG: Log before calling BusinessSecurityService
                    _logger.LogInformation("DEBUG - Calling GetEntityPermissionsAsync for entity: {EntityName}", entityName);
                    
                    // Get entity-level permissions using BusinessSecurityService
                    var entityPermissions = await _businessSecurityService.GetEntityPermissionsAsync(User, entityName);
                    
                    _logger.LogInformation("DEBUG - EntityPermissions result: CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}", 
                        entityPermissions.CanRead, entityPermissions.CanCreate, entityPermissions.CanUpdate, entityPermissions.CanDelete);

                    // If checking a specific instance, apply row-level filtering
                    bool hasInstanceAccess = true;
                    if (!string.IsNullOrEmpty(entityId) && int.TryParse(entityId, out int id))
                    {
                        _logger.LogInformation("DEBUG - Checking instance access for {EntityName} ID {Id}", entityName, id);
                        hasInstanceAccess = await _businessSecurityService.CanUserAccessEntityAsync(User, entityName, id);
                        _logger.LogInformation("Instance access check for {Entity} ID {Id}: {HasAccess}", 
                            entityName, id, hasInstanceAccess);
                    }
                    else
                    {
                        _logger.LogInformation("DEBUG - No specific entity ID, skipping instance access check");
                    }

                    // Combine entity permissions with instance access
                    var canRead = entityPermissions.CanRead && hasInstanceAccess;
                    var canCreate = entityPermissions.CanCreate;
                    var canUpdate = entityPermissions.CanUpdate && hasInstanceAccess;
                    var canDelete = entityPermissions.CanDelete && hasInstanceAccess;

                    // Log the results
                    _logger.LogInformation("{Entity} permissions: CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}",
                        entityName, canRead, canCreate, canUpdate, canDelete);
                    
                    _logger.LogInformation("DEBUG - Final hasAccess value: {HasAccess} (based on canRead)", canRead);

                    return Ok(new
                    {
                        route = normalizedRoute,
                        hasAccess = canRead,
                        entity = entityName,
                        permissions = new
                        {
                            canRead,
                            canCreate,
                            canUpdate,
                            canDelete
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DEBUG - Error checking entity permissions for {EntityName}: {ErrorMessage}", 
                        entityName, ex.Message);
                    
                    // Return false access on error
                    return Ok(new
                    {
                        route = normalizedRoute,
                        hasAccess = false,
                        entity = entityName,
                        error = ex.Message,
                        permissions = new
                        {
                            canRead = false,
                            canCreate = false,
                            canUpdate = false,
                            canDelete = false
                        }
                    });
                }
            }

            // For other routes, we check basic access
            _logger.LogInformation("DEBUG - No entity found in route, checking basic route access");
            bool hasAccess = await CheckBasicRouteAccess(normalizedRoute);
            _logger.LogInformation("DEBUG - Basic route access result: {HasAccess}", hasAccess);

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

            if (route.Contains("/profile"))
            {
                // Everyone can access their own profile
                return true;
            }

            if (route.Contains("/admin/user-management") || route.Contains("user-management"))
            {
                // User management requires specific admin roles
                return User.IsInRole("PARTNER_GLOB_ADMIN") || User.IsInRole("ORG_UNIT_ADMIN");
            }

            if (route.Contains("/admin"))
            {
                // Other admin routes require admin roles
                return User.IsInRole("PARTNER_GLOB_ADMIN") || User.IsInRole("ORG_UNIT_ADMIN") || User.IsInRole("Administrator");
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

            return Ok(new
            {
                userId,
                userName = user.UserName,
                roles
            });
        }

        /// <summary>
        /// Get roles for a specific user (admin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRolesById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                userId,
                userName = user.UserName,
                roles
            });
        }

        /// <summary>
        /// Extract entity name and ID from route using generic approach
        /// </summary>
        private (string EntityName, string EntityId) ExtractEntityInfoFromRoute(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                return (string.Empty, null);
            }

            // Remove query parameters and fragments
            var queryParamIndex = route.IndexOf('?');
            if (queryParamIndex > -1)
            {
                route = route.Substring(0, queryParamIndex);
            }

            var hashIndex = route.IndexOf('#');
            if (hashIndex > -1)
            {
                route = route.Substring(0, hashIndex);
            }

            // Split the route into segments and remove empty ones
            var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return (string.Empty, null);
            }

            string entityName = string.Empty;
            string entityId = null;

            // Check if last segment is numeric (ID)
            var lastSegment = segments[segments.Length - 1];
            if (int.TryParse(lastSegment, out _))
            {
                // Last segment is an ID, entity name is second-to-last
                entityId = lastSegment;
                if (segments.Length >= 2)
                {
                    var entityPlural = segments[segments.Length - 2];
                    entityName = ConvertPluralToSingular(entityPlural);
                }
            }
            else
            {
                // Last segment is the entity name (no ID)
                var entityPlural = lastSegment;
                entityName = ConvertPluralToSingular(entityPlural);
            }

            _logger.LogDebug("Extracted from route '{Route}': EntityName='{EntityName}', EntityId='{EntityId}'",
                route, entityName, entityId);

            return (entityName, entityId);
        }

        /// <summary>
        /// Convert plural entity name to singular and apply proper casing
        /// </summary>
        private string ConvertPluralToSingular(string entityPlural)
        {
            if (string.IsNullOrEmpty(entityPlural))
            {
                return string.Empty;
            }

            // Special case handling for compound names with dashes
            if (entityPlural.Contains("-"))
            {
                if (entityPlural.ToLower() == "partnership-agreements")
                {
                    return "Agreement";
                }
                if (entityPlural.ToLower() == "partner-tree")
                {
                    return "PartnerTree";
                }
                // For other hyphenated entities, convert to PascalCase
                // Split by dash, singularize each part, then join in PascalCase
                var parts = entityPlural.Split('-');
                var pascalParts = parts.Select(part => part.Singularize(inputIsKnownToBePlural: true).Pascalize());
                return string.Join("", pascalParts);
            }

            // Use Humanizer to convert plural to singular
            string entityName = entityPlural.Singularize(inputIsKnownToBePlural: true);

            // Apply Pascal case to ensure proper entity name format
            entityName = entityName.Pascalize();

            return entityName;
        }

        /// <summary>
        /// Extract the entity name from a route (legacy method - kept for compatibility)
        /// </summary>
        private string ExtractEntityNameFromRoute(string route)
        {
            var (entityName, _) = ExtractEntityInfoFromRoute(route);
            return entityName;
        }

        /// <summary>
        /// Extract entity ID from route (legacy method - kept for compatibility)
        /// </summary>
        private string ExtractEntityIdFromRoute(string route)
        {
            var (_, entityId) = ExtractEntityInfoFromRoute(route);
            return entityId;
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
    }
} 