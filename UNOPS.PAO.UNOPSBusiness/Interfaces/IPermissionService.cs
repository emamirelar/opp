using System.Security.Claims;

namespace UNOPS.PAO.UNOPSBusiness.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string entity, string action);
        
        Task<object> ApplyAccessControlFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action, string entityName) where T : class;
        
        Task<string> GetUserOrgUnitAsync(ClaimsPrincipal user);
        
        Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object entity = null);
        
        Task<object> GetEntityPermissionsAsync(string entityName, object entity = null);
        
        Task<bool> HasInstanceAccessAsync(string entityName, object entity, ClaimsPrincipal user, string action);
    }
} 