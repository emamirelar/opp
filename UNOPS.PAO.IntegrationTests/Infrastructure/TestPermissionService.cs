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
    }
}