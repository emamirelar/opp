using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using System.Linq;
using System.Collections.Generic;

namespace UNOPS.PAO.UNOPSPresentation.Controllers
{
    [Route("api/debug")]
    [ApiController]
    [AllowAnonymous] // Allow anonymous access for debugging purposes
    public class DebugController : ControllerBase
    {
        private readonly SharedPermissionService _permissionService;
        private readonly ILogger<DebugController> _logger;

        public DebugController(
            SharedPermissionService permissionService,
            ILogger<DebugController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpGet("auth")]
        public IActionResult GetAuthInfo()
        {
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            // Get roles from claims
            var userRoles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            
            // Create response object
            var authInfo = new
            {
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                userName = User.Identity?.Name,
                authenticationType = User.Identity?.AuthenticationType,
                userRoles,
                claims = userClaims,
                requestInfo = new
                {
                    path = Request.Path.Value,
                    method = Request.Method,
                    scheme = Request.Scheme,
                    host = Request.Host.Value
                }
            };
            
            return Ok(authInfo);
        }

        [HttpGet("validate-route")]
        public IActionResult ValidateRoute([FromQuery] string route)
        {
            // Get allowed roles for the route
            var allowedRoles = _permissionService.GetAllowedRolesForRoute(route);
            
            // Check if route exists in config
            bool routeConfigured = allowedRoles.Any();
            
            // Check for ALL role
            bool allowsAll = allowedRoles.Contains("ALL");
            
            // Check user roles
            var userRoles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
                
            // Check if user has required role
            bool hasRequiredRole = false;
            if (allowsAll)
            {
                hasRequiredRole = true;
            }
            else if (userRoles.Contains("Administrator"))
            {
                hasRequiredRole = true;
            }
            else
            {
                foreach (var role in userRoles)
                {
                    if (allowedRoles.Contains(role))
                    {
                        hasRequiredRole = true;
                        break;
                    }
                }
            }
            
            return Ok(new
            {
                route = route,
                routeConfigured = routeConfigured,
                allowedRoles = allowedRoles,
                allowsAll = allowsAll,
                userRoles = userRoles,
                hasRequiredRole = hasRequiredRole,
                result = hasRequiredRole ? "Access granted" : "Access denied",
                allRoleInfo = "The ALL role is a special marker that grants access to any authenticated user"
            });
        }

        [HttpGet("check-paths")]
        public IActionResult CheckPathPermissions()
        {
            var testPaths = new[]
            {
                "/",
                "/partnerships",
                "/partnerships/contacts",
                "/partnerships/partners",
                "/partnerships/interactions",
                "/partnerships/partnership-agreements",
                "/leads",
                "/initiatives",
                "/admin"
            };
            
            var results = new List<object>();
            
            foreach (var path in testPaths)
            {
                // Get allowed roles for the route
                var allowedRoles = _permissionService.GetAllowedRolesForRoute(path);
                
                // Check if route exists in config
                bool routeConfigured = allowedRoles.Any();
                
                // Check for ALL role
                bool allowsAll = allowedRoles.Contains("ALL");
                
                // Check user roles
                var userRoles = User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();
                    
                // Check if user has required role
                bool hasRequiredRole = false;
                if (allowsAll)
                {
                    hasRequiredRole = true;
                }
                else if (userRoles.Contains("Administrator"))
                {
                    hasRequiredRole = true;
                }
                else
                {
                    foreach (var role in userRoles)
                    {
                        if (allowedRoles.Contains(role))
                        {
                            hasRequiredRole = true;
                            break;
                        }
                    }
                }
                
                results.Add(new
                {
                    path,
                    routeConfigured,
                    allowedRoles,
                    allowsAll,
                    userRoles,
                    hasRequiredRole,
                    result = hasRequiredRole ? "Access granted" : "Access denied"
                });
            }
            
            return Ok(results);
        }
    }
} 