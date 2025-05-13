using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace UNOPS.PAO.UNOPSPresentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class PermissionsController : ControllerBase
    {
        private readonly SharedPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            SharedPermissionService permissionService,
            ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        /// <summary>
        /// Get the entire permission configuration for the frontend
        /// </summary>
        [HttpGet]
        public IActionResult GetPermissionConfiguration()
        {
            return Ok(_permissionService.GetPermissionConfiguration());
        }

        /// <summary>
        /// Get allowed roles for a specific route (used by frontend)
        /// </summary>
        [HttpGet("route/{*route}")]
        public IActionResult GetAllowedRolesForRoute(string route)
        {
            _logger.LogDebug("Getting allowed roles for route {Route}", route);
            var normalizedRoute = NormalizeRoutePath(route);
            var roles = _permissionService.GetAllowedRolesForRoute(normalizedRoute);
            
            return Ok(new { route = normalizedRoute, allowedRoles = roles });
        }

        /// <summary>
        /// Check if the current user has access to a specific route
        /// </summary>
        [HttpGet("check/{*route}")]
        public IActionResult CheckRouteAccess(string route)
        {
            _logger.LogDebug("Checking access for route {Route}", route);
            var normalizedRoute = NormalizeRoutePath(route);
            
            var allowedRoles = _permissionService.GetAllowedRolesForRoute(normalizedRoute);
            
            // Dump all user claims for debugging
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            _logger.LogInformation("All user claims for route access check: {Claims}", 
                System.Text.Json.JsonSerializer.Serialize(allClaims));
            
            // Check for ALL role
            if (allowedRoles.Contains("ALL"))
            {
                _logger.LogInformation("Route {Route} allows ALL users, granting access", normalizedRoute);
                return Ok(new { route = normalizedRoute, hasAccess = true });
            }
            
            // Check user roles
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
                
            _logger.LogInformation("User roles from claims for route check: {UserRoles}", 
                string.Join(", ", userRoles));
                
            bool hasAccess = false;
            
            // Check Administrator role first
            if (userRoles.Contains("Administrator"))
            {
                _logger.LogInformation("User has Administrator role, granting access to {Route}", normalizedRoute);
                hasAccess = true;
            }
            else
            {
                // Check if user has any of the allowed roles
                foreach (var role in userRoles)
                {
                    if (allowedRoles.Contains(role))
                    {
                        _logger.LogInformation("User has required role {Role} for route {Route}, granting access", 
                            role, normalizedRoute);
                        hasAccess = true;
                        break;
                    }
                }
                
                if (!hasAccess)
                {
                    _logger.LogWarning("User doesn't have any required roles. User roles: {UserRoles}, Required roles: {RequiredRoles}",
                        string.Join(", ", userRoles),
                        string.Join(", ", allowedRoles));
                }
            }
            
            _logger.LogDebug("Access for route {Route}: {HasAccess}. User roles: {UserRoles}, Allowed roles: {AllowedRoles}", 
                normalizedRoute, 
                hasAccess,
                string.Join(", ", userRoles),
                string.Join(", ", allowedRoles));
                
            return Ok(new { route = normalizedRoute, hasAccess });
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
                    long.TryParse(segment, out _) ||
                    Guid.TryParse(segment, out _))
                {
                    // Replace with a parameter indicator - the exact value doesn't matter
                    // since we just need to match route patterns in the config
                    segments[i] = ":id";
                }
            }
            
            // Rejoin the segments
            return string.Join("/", segments);
        }

        /// <summary>
        /// Debug endpoint to check permissions for a given route
        /// </summary>
        [HttpGet("diagnose/{*route}")]
        public IActionResult DiagnoseRoutePermissions(string route)
        {
            _logger.LogInformation("Diagnosing permissions for route: {Route}", route);
            
            // Normalize the route
            if (!route.StartsWith("/"))
            {
                route = "/" + route;
            }
            
            var fixedRoute = route.ToLowerInvariant();
            
            // Get allowed roles for the route
            var allowedRoles = _permissionService.GetAllowedRolesForRoute(fixedRoute);
            
            // Build path variations to check all possibilities
            var variations = new List<string>
            {
                fixedRoute,
                fixedRoute.TrimStart('/'),
                "partnerships/" + fixedRoute.TrimStart('/').TrimStart("partnerships/".ToCharArray())
            };
            
            var variationResults = new Dictionary<string, IEnumerable<string>>();
            
            foreach (var variation in variations)
            {
                if (!string.IsNullOrEmpty(variation))
                {
                    variationResults[variation] = _permissionService.GetAllowedRolesForRoute(variation);
                }
            }
            
            // Check user's roles
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            
            // Get all claims for debugging
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            var isAdmin = userRoles.Contains("Administrator");
            var hasAllowedRole = allowedRoles.Any(r => userRoles.Contains(r) || r == "ALL");
            var accessGranted = isAdmin || hasAllowedRole;
            
            return Ok(new
            {
                requestedRoute = route,
                fixedRoute,
                allowedRoles,
                pathVariations = variationResults,
                userInfo = new
                {
                    isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    userName = User.Identity?.Name,
                    authType = User.Identity?.AuthenticationType,
                    userRoles,
                    allClaims,
                    isAdmin,
                    hasAllowedRole
                },
                result = new
                {
                    accessGranted,
                    reason = isAdmin ? "User is Administrator" :
                             hasAllowedRole ? "User has required role" :
                             "User lacks required role"
                }
            });
        }

        /// <summary>
        /// Debug endpoint to check current user roles
        /// </summary>
        [HttpGet("user-roles")]
        public IActionResult GetUserRoles()
        {
            // Get all user claims
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            // Get role claims specifically
            var roleClaims = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
                
            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                userName = User.Identity?.Name,
                email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                authType = User.Identity?.AuthenticationType,
                roles = roleClaims,
                allClaims = allClaims,
                isAdmin = roleClaims.Contains("Administrator"),
                isInternal = roleClaims.Contains("Internal"),
                isPartner = roleClaims.Contains("Partner")
            });
        }
    }
} 