using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Test implementation of IPermissionService that returns all data without filtering
    /// </summary>
    public class TestPermissionService : IPermissionService
    {
        public Task<bool> HasPermissionAsync(ClaimsPrincipal user, string entity, string action)
        {
            // For testing, always return true
            return Task.FromResult(true);
        }

        public Task<object> ApplyAccessControlFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action, string entityName) where T : class
        {
            // For testing, return all items without filtering
            // Force evaluation of the query first
            var list = query.ToList();
            Console.WriteLine($"TestPermissionService.ApplyAccessControlFiltersAsync: Returning {list.Count} items of type {typeof(T).Name}");
            return Task.FromResult<object>(list);
        }

        public Task<string> GetUserOrgUnitAsync(ClaimsPrincipal user)
        {
            // For testing, return a default org unit
            return Task.FromResult("HQ");
        }

        public Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object entity = null)
        {
            // For testing, always return true
            return Task.FromResult(true);
        }

        public Task<object> GetEntityPermissionsAsync(string entityName, object entity = null)
        {
            // For testing, return all permissions as true
            var permissions = new Dictionary<string, bool>
            {
                { "read", true },
                { "create", true },
                { "update", true },
                { "delete", true }
            };
            return Task.FromResult<object>(permissions);
        }

        public Task<bool> HasInstanceAccessAsync(string entityName, object entity, ClaimsPrincipal user, string action)
        {
            // For testing, always return true
            return Task.FromResult(true);
        }

        public string GetEffectiveRole(ClaimsPrincipal user)
        {
            // For testing, return the first role or a default test role
            if (user?.Identity?.IsAuthenticated == true)
            {
                var roles = user.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (roles.Any())
                {
                    // Return the first role for testing, or prioritize known test roles
                    if (roles.Contains("PARTNER_GLOB_ADMIN")) return "PARTNER_GLOB_ADMIN";
                    if (roles.Contains("ORG_UNIT_ADMIN")) return "ORG_UNIT_ADMIN";
                    if (roles.Contains("PARTNER_USER")) return "PARTNER_USER";
                    if (roles.Contains("UNOPS_GEN_USER")) return "UNOPS_GEN_USER";
                    
                    return roles.First(); // Return first role if none match known roles
                }
            }
            
            // Default role
            return "UNOPS_GEN_USER";
        }

        public bool CanExport(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated == true)
            {
                var roles = user.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                return roles.Contains("PARTNER_GLOB_ADMIN");
            }
            
            return false; // Default for tests - no export unless explicitly granted
        }

        public bool CanImport(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated == true)
            {
                var roles = user.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                return roles.Contains("PARTNER_GLOB_ADMIN");
            }
            
            return false; // Default for tests - no import unless explicitly granted
        }

        public Task<object> GetEntityInstancePermissionsAsync(string entityName, int entityId)
        {
            // For testing, return all permissions as true
            var permissions = new Dictionary<string, bool>
            {
                { "read", true },
                { "create", true },
                { "update", true },
                { "delete", true }
            };
            return Task.FromResult<object>(permissions);
        }

        public Task<bool> IsOpportunityTeamMemberAsync(int opportunityId)
        {
            // For testing, always return true
            return Task.FromResult(true);
        }
    }
}