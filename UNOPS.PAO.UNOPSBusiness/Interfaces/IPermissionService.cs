using System.Security.Claims;

namespace UNOPS.PAO.UNOPSBusiness.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string entity, string action);
        
        Task<object> ApplyAccessControlFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action, string entityName) where T : class;
        
        Task<string> GetUserOrgUnitAsync(ClaimsPrincipal user);
        
        Task<bool> CanPerformActionAsync(string entityName, string action, ClaimsPrincipal user, object? entity = null);
        
        Task<object> GetEntityPermissionsAsync(string entityName, object? entity = null);
        
        /// <summary>
        /// Gets entity permissions for a specific instance, including team-based permissions for Opportunity
        /// </summary>
        /// <param name="entityName">Name of the entity</param>
        /// <param name="entityId">ID of the specific entity instance</param>
        /// <returns>Permission object with canRead, canCreate, canUpdate, canDelete flags</returns>
        Task<object> GetEntityInstancePermissionsAsync(string entityName, int entityId);
        
        Task<bool> HasInstanceAccessAsync(string entityName, object entity, ClaimsPrincipal user, string action);
        
        /// <summary>
        /// Checks if the current user is a stakeholder (team member) of an Opportunity
        /// </summary>
        /// <param name="opportunityId">The opportunity ID to check</param>
        /// <returns>True if the user is a team member</returns>
        Task<bool> IsOpportunityTeamMemberAsync(int opportunityId);
        
        string GetEffectiveRole(ClaimsPrincipal user);
        
        bool CanExport(ClaimsPrincipal user);
        
        bool CanImport(ClaimsPrincipal user);
    }
} 