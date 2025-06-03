using System.Security.Claims;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;

public interface IUserManagementManager
{
    Task<PaginationResponse<UserManagementModel>> GetUsersAsync(ClaimsPrincipal user, UserManagementRequest request);
    Task<UserManagementModel?> GetUserByIdAsync(ClaimsPrincipal user, int userId);
    Task<UserManagementModel?> UpdateUserRolesAsync(ClaimsPrincipal user, int userId, UpdateUserRolesRequest request);
    Task<IEnumerable<RoleModel>> GetAvailableRolesAsync(ClaimsPrincipal user);
    Task<bool> GetOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode);
    Task UpdateOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode, UpdateOrgUnitSelfManagementRequest request);
} 