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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using System.Text.Json;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/permissions")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class PermissionController : ControllerBase
    {
        private readonly ILogger<PermissionController> _logger;
        private readonly UserManager<PAOIdentityUser> _userManager;
        private readonly RoleManager<PAOIdentityRole> _roleManager;
        private readonly IPermissionService _permissionService;

        // Route-to-entity mapping for special cases where route entity differs from data entity
        // This allows PartnerTree to have read access for dropdowns, but PartnerTreeManagement for page access
        private static readonly Dictionary<string, string> RouteToEntityMapping = new Dictionary<string, string>
        {
            { "/admin/partner-tree", "PartnerTreeManagement" },
            { "/admin/entity-artifacts", "EntityManager" },
            { "/admin/bulk-entity-artifacts", "EntityManager" },
        };

        public PermissionController(
            ILogger<PermissionController> logger,
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
        /// Get the entire system permission configuration including all roles and their capabilities
        /// </summary>
        /// <returns>System-wide permission configuration for frontend initialization</returns>
        [HttpGet]
        public async Task<IActionResult> GetSystemPermissionConfiguration()
        {
            try
            {
                // Get all roles from the system
                var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

                return Ok(new
                {
                    Roles = roles,
                    GeneratedAt = DateTime.UtcNow,
                    TotalRoles = roles.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system permission configuration");
                return StatusCode(500, new { Error = "Failed to retrieve system permission configuration", Details = ex.Message });
            }
        }

        /// <summary>
        /// Check if the current user has permission to access a specific route or page
        /// </summary>
        /// <param name="route">The route/URL path to check permissions for (e.g., '/partnerships/contacts/123')</param>
        /// <returns>Permission details including access status and specific permissions for the route</returns>
        [HttpGet("check/{*route}")]
        public async Task<IActionResult> CheckUserRoutePermission(string route)
        {
            try
            {
                _logger.LogDebug("Checking user route permission for route {Route}", route);
                var normalizedRoute = NormalizeRoutePath(route);

                // Get user information
                var userId = _userManager.GetUserId(User);
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
                
                _logger.LogInformation("Checking route access for User: {UserId}, Email: {Email}, Route: {Route}", 
                    userId, userEmail, normalizedRoute);

                if (normalizedRoute.Contains("ai-prompt-management", StringComparison.OrdinalIgnoreCase))
                {
                    var aiPromptAccess = await CheckBasicRouteAccess(normalizedRoute);
                    return Ok(new { Route = normalizedRoute, HasAccess = aiPromptAccess, Entity = (string?)null, CheckedAt = DateTime.UtcNow });
                }

                // Extract entity name and ID from route
                var (entityName, entityId) = ExtractEntityInfoFromRoute(route);
                
                _logger.LogInformation("Extracted from route: EntityName={EntityName}, EntityId={EntityId}", 
                    entityName, entityId);
                
                if (!string.IsNullOrEmpty(entityName))
                {
                    _logger.LogInformation("Checking {Entity} entity permissions for route: {Route}", entityName, normalizedRoute);

                    object entityPermissionsObj;
                    bool isTeamMember = false;
                    
                    // For specific entity instances (when entityId is provided), use instance-level permissions
                    // This enables team-based access for Opportunity entities
                    if (!string.IsNullOrEmpty(entityId) && int.TryParse(entityId, out int parsedEntityId) && parsedEntityId > 0)
                    {
                        _logger.LogInformation("Checking instance-level permissions for {Entity} ID={EntityId}", entityName, parsedEntityId);
                        entityPermissionsObj = await _permissionService.GetEntityInstancePermissionsAsync(entityName, parsedEntityId);
                        
                        // Check if response includes IsTeamMember flag (for Opportunity)
                        var instanceJsonString = JsonSerializer.Serialize(entityPermissionsObj);
                        var instanceDoc = JsonDocument.Parse(instanceJsonString);
                        if (instanceDoc.RootElement.TryGetProperty("IsTeamMember", out var teamMemberProp))
                        {
                            isTeamMember = teamMemberProp.GetBoolean();
                        }
                    }
                    else
                    {
                        // For list views (no specific ID), use entity-level permissions
                        entityPermissionsObj = await _permissionService.GetEntityPermissionsAsync(entityName);
                    }
                    
                    // Convert to JSON and deserialize to our response class
                    var jsonString = JsonSerializer.Serialize(entityPermissionsObj);
                    var permissions = JsonSerializer.Deserialize<EntityPermissionsModel>(jsonString);
                    
                    _logger.LogInformation("EntityPermissions result: CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}, CanExport={CanExport}, CanImport={CanImport}, IsTeamMember={IsTeamMember}", 
                        permissions?.CanRead, permissions?.CanCreate, permissions?.CanUpdate, permissions?.CanDelete, permissions?.CanExport, permissions?.CanImport, isTeamMember);

                    return Ok(new
                    {
                        Route = normalizedRoute,
                        HasAccess = permissions?.CanRead ?? false,
                        Entity = entityName,
                        EntityId = entityId,
                        Permissions = new
                        {
                            CanRead = permissions?.CanRead ?? false,
                            CanCreate = permissions?.CanCreate ?? false,
                            CanUpdate = permissions?.CanUpdate ?? false,
                            CanDelete = permissions?.CanDelete ?? false,
                            CanExport = permissions?.CanExport ?? false,
                            CanImport = permissions?.CanImport ?? false
                        },
                        IsTeamMember = isTeamMember,
                        CheckedAt = DateTime.UtcNow
                    });
                }

                // For other routes, check basic access
                _logger.LogInformation("No entity found in route, checking basic route access");
                bool hasAccess = await CheckBasicRouteAccess(normalizedRoute);
                _logger.LogInformation("Basic route access result: {HasAccess}", hasAccess);

                return Ok(new { 
                    Route = normalizedRoute, 
                    HasAccess = hasAccess,
                    Entity = (string?)null,
                    CheckedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user route permission for route: {Route}", route);
                return Ok(new
                {
                    Route = route,
                    HasAccess = false,
                    Error = ex.Message,
                    CheckedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get complete permission details for a specific entity, including all possible operations and role requirements
        /// </summary>
        /// <param name="entityName">Name of the entity (e.g., 'Contact', 'Partner', 'Interaction')</param>
        /// <returns>Comprehensive entity permission configuration</returns>
        [HttpGet("entity-permissions/{entityName}")]
        public async Task<IActionResult> GetEntityPermissionDetails(string entityName)
        {
            try
            {
                _logger.LogInformation("Getting entity permission details for: {EntityName}", entityName);

                if (string.IsNullOrEmpty(entityName))
                {
                    return BadRequest(new { Error = "Entity name is required" });
                }

                // Get current user's permissions for this entity
                var userPermissionsObj = await _permissionService.GetEntityPermissionsAsync(entityName);
                var jsonString = JsonSerializer.Serialize(userPermissionsObj);
                var userPermissions = JsonSerializer.Deserialize<EntityPermissionsModel>(jsonString);

                // Get all roles that can access this entity (this would need to be implemented in the service)
                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

                var result = new
                {
                    EntityName = entityName,
                    UserPermissions = new
                    {
                        CanRead = userPermissions?.CanRead ?? false,
                        CanCreate = userPermissions?.CanCreate ?? false,
                        CanUpdate = userPermissions?.CanUpdate ?? false,
                        CanDelete = userPermissions?.CanDelete ?? false,
                        CanExport = userPermissions?.CanExport ?? false,
                        CanImport = userPermissions?.CanImport ?? false,
                        HasAccess = userPermissions?.CanRead ?? false
                    },
                    SystemConfiguration = new
                    {
                        AvailableRoles = allRoles,
                        TotalRoles = allRoles.Count
                    },
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedFor = _userManager.GetUserId(User)
                };

                _logger.LogInformation("Successfully retrieved entity permission details for {EntityName}", entityName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity permission details for: {EntityName}", entityName);
                return StatusCode(500, new 
                { 
                    Error = "Failed to retrieve entity permission details", 
                    EntityName = entityName,
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get the current authenticated user's roles and permissions
        /// </summary>
        /// <returns>Current user's role assignments and basic information</returns>
        [HttpGet("user-roles")]
        public async Task<IActionResult> GetCurrentUserRoles()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized(new { Error = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Error = "User not found", UserId = userId });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    UserId = userId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles,
                    RoleCount = roles.Count,
                    RetrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user roles");
                return StatusCode(500, new { Error = "Failed to retrieve user roles", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get roles and basic information for a specific user by ID (admin access required)
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve role information for</param>
        /// <returns>Specified user's role assignments and basic information</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRolesByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { Error = "User ID is required" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Error = "User not found", UserId = userId });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    UserId = userId,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles,
                    RoleCount = roles.Count,
                    RetrievedAt = DateTime.UtcNow,
                    RetrievedBy = _userManager.GetUserId(User)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for user ID: {UserId}", userId);
                return StatusCode(500, new { Error = "Failed to retrieve user roles", UserId = userId, Details = ex.Message });
            }
        }

        /// <summary>
        /// Get all available system roles and their descriptions
        /// </summary>
        /// <returns>List of all system roles with metadata</returns>
        [HttpGet("available-roles")]
        public async Task<IActionResult> GetAvailableSystemRoles()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Select(r => new { 
                        Id = r.Id,
                        Name = r.Name,
                        NormalizedName = r.NormalizedName 
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Roles = roles,
                    TotalRoles = roles.Count,
                    RetrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available system roles");
                return StatusCode(500, new { Error = "Failed to retrieve available roles", Details = ex.Message });
            }
        }

        /// <summary>
        /// Basic route access check for non-entity routes
        /// </summary>
        private Task<bool> CheckBasicRouteAccess(string route)
        {
            // Map common routes to entities for permission checking
            if (route.Contains("/dashboard"))
            {
                return Task.FromResult(true); // Everyone can access dashboard
            }

            if (route.Contains("/profile"))
            {
                return Task.FromResult(true); // Everyone can access their own profile
            }

            if (route.Contains("/admin/user-management") || route.Contains("user-management"))
            {
                return Task.FromResult(User.IsInRole("PARTNER_GLOB_ADMIN") || User.IsInRole("ORG_UNIT_ADMIN"));
            }

            if (route.Contains("/admin"))
            {
                return Task.FromResult(User.IsInRole("PARTNER_GLOB_ADMIN") || User.IsInRole("ORG_UNIT_ADMIN") || User.IsInRole("Administrator"));
            }

            return Task.FromResult(true); // Default to allowing access for authenticated users
        }

        /// <summary>
        /// Extract entity name and ID from route using generic approach
        /// </summary>
        private (string EntityName, string? EntityId) ExtractEntityInfoFromRoute(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                return (string.Empty, (string?)null);
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

            // Normalize the route path
            var normalizedRoute = NormalizeRoutePath(route);
            
            // Check if this route has a specific entity mapping (for pages that differ from data entities)
            foreach (var mapping in RouteToEntityMapping)
            {
                if (normalizedRoute.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Route '{Route}' mapped to entity '{Entity}' via RouteToEntityMapping", 
                        route, mapping.Value);
                    return (mapping.Value, (string?)null);
                }
            }

            // Split the route into segments and remove empty ones
            var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return (string.Empty, (string?)null);
            }

            // Define known section parameters that should be ignored in entity detection
            var knownSectionParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "analysis", "what", "who", "team", "why", "where", "when", "related", "collaboration", "overview", "risks"
            };

            string entityName = string.Empty;
            string? entityId = null;

            // Check if last segment is a known section parameter and remove it
            var lastSegment = segments[segments.Length - 1];
            if (knownSectionParameters.Contains(lastSegment))
            {
                _logger.LogDebug("Detected section parameter '{Section}' in route '{Route}', skipping it for entity extraction",
                    lastSegment, route);
                
                // Remove the section parameter and process remaining segments
                if (segments.Length > 1)
                {
                    var remainingSegments = segments.Take(segments.Length - 1).ToArray();
                    lastSegment = remainingSegments[remainingSegments.Length - 1];
                    
                    // Check if the new last segment is numeric (ID)
                    if (int.TryParse(lastSegment, out _))
                    {
                        entityId = lastSegment;
                        if (remainingSegments.Length >= 2)
                        {
                            var entityPlural = remainingSegments[remainingSegments.Length - 2];
                            entityName = ConvertPluralToSingular(entityPlural);
                        }
                    }
                    else
                    {
                        var entityPlural = lastSegment;
                        entityName = ConvertPluralToSingular(entityPlural);
                    }
                }
            }
            else
            {
                // Original logic for routes without section parameters
                // Check if last segment is numeric (ID)
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
