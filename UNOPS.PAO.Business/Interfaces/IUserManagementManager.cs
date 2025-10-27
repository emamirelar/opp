using System.Security.Claims;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Users;

namespace UNOPS.PAO.Business.Interfaces;

public interface IUserManagementManager
{
    Task<PaginationResponse<UserManagementModel>> GetUsersAsync(ClaimsPrincipal user, UserManagementRequest request);
    
    Task<UserManagementModel?> GetUserByIdAsync(ClaimsPrincipal user, string userId);
    
    Task<UserManagementModel?> UpdateUserRolesAsync(ClaimsPrincipal user, string userId, UpdateUserRolesRequest request);
    
    Task<IEnumerable<RoleModel>> GetAvailableRolesAsync(ClaimsPrincipal user);
    
    Task<IEnumerable<OrgUnitModel>> GetAvailableOrgUnitsAsync(ClaimsPrincipal user);
    
    Task<bool> GetOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode);
    
    Task UpdateOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode, UpdateOrgUnitSelfManagementRequest request);
    
    Task<object> AnalyzeUserRoleFileAsync(ClaimsPrincipal user, AnalyseFileRequest request);
    
    Task<object> BulkUploadUserRolesAsync(ClaimsPrincipal user, BulkUploadRequest request);
    
    Task<Dictionary<int, object>> ResolveUsersAsync(ClaimsPrincipal user, ResolveUsersRequest request);
    
    Task<Dictionary<int, object>> ResolveRolesAsync(ClaimsPrincipal user, ResolveRolesRequest request);
} 