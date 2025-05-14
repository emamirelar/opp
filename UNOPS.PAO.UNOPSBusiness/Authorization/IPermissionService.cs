namespace UNOPS.PAO.UNOPSBusiness.Authorization;

using System.Linq.Expressions;
using System.Security.Claims;

public interface IPermissionService
{
    Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object? entity = null);
    Task<IQueryable<T>> ApplySecurityFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user) where T : class;
    Task<IEnumerable<string>> GetReadablePropertiesAsync(string entityName, ClaimsPrincipal user);
} 