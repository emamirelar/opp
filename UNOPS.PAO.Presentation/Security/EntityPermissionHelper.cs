using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.Presentation.Security
{
    /// <summary>
    /// Helper class for entity permission operations
    /// </summary>
    public class EntityPermissionHelper
    {
        private readonly ILogger<EntityPermissionHelper> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EntityPermissionHelper(
            ILogger<EntityPermissionHelper> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Checks if the current user has a specified role
        /// </summary>
        /// <param name="roleName">Role to check</param>
        /// <returns>True if the user has the role, false otherwise</returns>
        public bool CurrentUserHasRole(string roleName)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                _logger.LogWarning("No user context available when checking role {RoleName}", roleName);
                return false;
            }

            // Check role claim
            return user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Any(c => string.Equals(c.Value, roleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if the current user can perform an action on an entity based on EntityPermissions table
        /// </summary>
        /// <param name="entityName">The entity name (e.g., "Partner", "Contact")</param>
        /// <param name="action">The action (e.g., "Read", "Create", "Update", "Delete")</param>
        /// <returns>True if permission exists, false otherwise</returns>
        public bool CanPerformAction(string entityName, string action)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                _logger.LogWarning("No user context available when checking permission for {EntityName}/{Action}", 
                    entityName, action);
                return false;
            }

            // Administrator role always has access
            if (CurrentUserHasRole("Administrator"))
            {
                return true;
            }

            // Get user roles
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // TODO: This would be better using an actual query to check against EntityPermissions
            // For now, this just implements basic rules based on the seed-entity-permissions.sql
            
            // Internal role has broad access to most entities
            if (roles.Contains("Internal"))
            {
                if (entityName is "Partner" or "Contact" or "Document" or "Project")
                {
                    // Internal can't delete
                    if (action == "Delete")
                    {
                        return false;
                    }
                    
                    return true;
                }
            }

            // Partner role has limited access
            if (roles.Contains("Partner"))
            {
                if (entityName is "Partner" or "Contact" or "Document" or "Project")
                {
                    // Partner can only read
                    if (action == "Read")
                    {
                        return true;
                    }
                    
                    // Partner can create and update contacts and documents
                    if ((entityName is "Contact" or "Document") && (action is "Create" or "Update"))
                    {
                        return true;
                    }
                }
            }

            // External and User roles have very limited access
            if (roles.Contains("External") || roles.Contains("User"))
            {
                if (entityName is "Partner" or "Contact" or "Document" or "Project")
                {
                    // Can only read
                    if (action == "Read")
                    {
                        return true;
                    }
                }
            }

            _logger.LogInformation("Permission denied for user with roles {Roles} to perform {Action} on {EntityName}",
                string.Join(", ", roles), action, entityName);
            
            return false;
        }

        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        /// <returns>User ID or null if not found</returns>
        public string? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }

            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
} 