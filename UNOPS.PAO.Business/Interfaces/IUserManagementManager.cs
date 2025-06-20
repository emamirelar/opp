using System.Security.Claims;
using UNOPS.PAO.Models;
using UNOPS.PAO.RBAC.Attributes;

namespace UNOPS.PAO.Business.Interfaces;

public interface IUserManagementManager
{
    [RBAC("read", Entity = "UserManagement")]
    Task<PaginationResponse<UserManagementModel>> GetUsersAsync(ClaimsPrincipal user, UserManagementRequest request);
    
    [RBAC("read", Entity = "UserManagement", RequireEntityAccess = true, EntityIdParameterName = "userId")]
    Task<UserManagementModel?> GetUserByIdAsync(ClaimsPrincipal user, int userId);
    
    [RBAC("update", Entity = "UserManagement", RequireEntityAccess = true, EntityIdParameterName = "userId")]
    Task<UserManagementModel?> UpdateUserRolesAsync(ClaimsPrincipal user, int userId, UpdateUserRolesRequest request);
    
    [RBAC("read", Entity = "UserManagement")]
    Task<IEnumerable<RoleModel>> GetAvailableRolesAsync(ClaimsPrincipal user);
    
    [RBAC("read", Entity = "UserManagement")]
    Task<bool> GetOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode);
    
    [RBAC("update", Entity = "UserManagement")]
    Task UpdateOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode, UpdateOrgUnitSelfManagementRequest request);
} 