using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Utilities.Helpers;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSUserManagementManager : IUserManagementManager
{
    private readonly UNOPSAppDbContext _context;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;
    private readonly IBusinessSecurityService _securityService;

    public UNOPSUserManagementManager(
        UNOPSAppDbContext context,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        IBusinessSecurityService securityService)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _securityService = securityService;
    }

    public async Task<PaginationResponse<UserManagementModel>> GetUsersAsync(ClaimsPrincipal user, UserManagementRequest request)
    {
        // Security check - only PARTNER_GLOBAL_ADMIN and ORG_ADMIN can access
        if (!user.IsInRole("PARTNER_GLOB_ADMIN") && !user.IsInRole("ORG_UNIT_ADMIN"))
        {
            throw new UnauthorizedAccessException("Access denied. Only Partnership Global Admins and Org Unit Admins can manage users.");
        }

        // Start with UserInfos query
        var userInfoQuery = _context.UserInfos.Where(u => !u.IsDeleted);

        // Apply "Show My Org Unit Only" filter if requested
        if (request.ShowMyOrgUnitOnly)
        {
            var currentUserOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            if (!string.IsNullOrEmpty(currentUserOrgUnit))
            {
                userInfoQuery = userInfoQuery.Where(x => x.OrgUnit == currentUserOrgUnit);
            }
        }

        // Apply org unit filter if specified
        if (!string.IsNullOrEmpty(request.OrgUnitFilter))
        {
            userInfoQuery = userInfoQuery.Where(x => x.OrgUnit != null && x.OrgUnit.Contains(request.OrgUnitFilter));
        }

        // Apply search term filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            userInfoQuery = userInfoQuery.Where(x => 
                (x.Name != null && x.Name.ToLower().Contains(searchLower)) ||
                (x.UserEmail != null && x.UserEmail.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        userInfoQuery = request.SortBy?.ToLower() switch
        {
            "email" => request.SortDirection?.ToLower() == "desc" 
                ? userInfoQuery.OrderByDescending(x => x.UserEmail)
                : userInfoQuery.OrderBy(x => x.UserEmail),
            "orgunit" => request.SortDirection?.ToLower() == "desc"
                ? userInfoQuery.OrderByDescending(x => x.OrgUnit)
                : userInfoQuery.OrderBy(x => x.OrgUnit),
            "lastmodified" => request.SortDirection?.ToLower() == "desc"
                ? userInfoQuery.OrderByDescending(x => x.LastModifiedDate)
                : userInfoQuery.OrderBy(x => x.LastModifiedDate),
            _ => userInfoQuery.OrderBy(x => x.Name ?? x.UserEmail)
        };

        // Get total count before pagination
        var totalCount = await userInfoQuery.CountAsync();

        // Apply pagination
        var pagedUserInfos = await userInfoQuery
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Get user roles for each user
        var userModels = new List<UserManagementModel>();
        foreach (var userInfo in pagedUserInfos)
        {
            if (string.IsNullOrEmpty(userInfo.UserEmail)) continue;

            var aspNetUser = await _userManager.FindByEmailAsync(userInfo.UserEmail);
            var roles = new List<string>();
            var isActive = true; // Default to active if not found in AspNetUsers

            if (aspNetUser != null)
            {
                var userRoles = await _userManager.GetRolesAsync(aspNetUser);
                roles = userRoles.ToList();
                isActive = !aspNetUser.LockoutEnabled || 
                          (aspNetUser.LockoutEnd == null || aspNetUser.LockoutEnd <= DateTimeOffset.UtcNow);
            }
            
            // Apply role filter if specified
            if (!string.IsNullOrEmpty(request.RoleFilter) && 
                !roles.Any(r => r.Contains(request.RoleFilter, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            userModels.Add(new UserManagementModel
            {
                UserId = userInfo.UserId,
                Name = userInfo.Name ?? "N/A",
                Email = userInfo.UserEmail ?? "N/A",
                OrgUnit = userInfo.OrgUnit ?? "N/A",
                OrgUnitCode = userInfo.OrgUnit,
                Roles = roles,
                LastModifiedDate = userInfo.LastModifiedDate,
                IsActive = isActive
            });
        }

        // If role filter was applied, we need to adjust the total count
        if (!string.IsNullOrEmpty(request.RoleFilter))
        {
            totalCount = userModels.Count;
        }

        return new PaginationResponse<UserManagementModel>
        {
            Records = userModels,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task<UserManagementModel?> GetUserByIdAsync(ClaimsPrincipal user, int userId)
    {
        // Security check
        if (!user.IsInRole("PARTNER_GLOB_ADMIN") && !user.IsInRole("ORG_UNIT_ADMIN"))
        {
            throw new UnauthorizedAccessException("Access denied. Only Partnership Global Admins and Org Unit Admins can view user details.");
        }

        var userInfo = await _context.UserInfos
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (userInfo == null) return null;

        var aspNetUser = await _userManager.FindByEmailAsync(userInfo.UserEmail);
        if (aspNetUser == null) return null;

        // Check org unit access for ORG_UNIT_ADMIN
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            var currentUserOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            if (userInfo.OrgUnit != currentUserOrgUnit)
            {
                throw new UnauthorizedAccessException("Access denied. You can only view users from your organization unit.");
            }
        }

        var roles = await _userManager.GetRolesAsync(aspNetUser);

        return new UserManagementModel
        {
            UserId = userInfo.UserId,
            Name = userInfo.Name ?? "N/A",
            Email = userInfo.UserEmail ?? "N/A",
            OrgUnit = userInfo.OrgUnit ?? "N/A",
            OrgUnitCode = userInfo.OrgUnit,
            Roles = roles.ToList(),
            LastModifiedDate = userInfo.LastModifiedDate,
            IsActive = !aspNetUser.LockoutEnabled || 
                      (aspNetUser.LockoutEnd == null || aspNetUser.LockoutEnd <= DateTimeOffset.UtcNow)
        };
    }

    public async Task<UserManagementModel?> UpdateUserRolesAsync(ClaimsPrincipal user, int userId, UpdateUserRolesRequest request)
    {
        // Security check
        if (!user.IsInRole("PARTNER_GLOB_ADMIN") && !user.IsInRole("ORG_UNIT_ADMIN"))
        {
            throw new UnauthorizedAccessException("Access denied. Only Partnership Global Admins and Org Unit Admins can update user roles.");
        }

        var userInfo = await _context.UserInfos
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (userInfo == null)
        {
            throw new ArgumentException("User not found.");
        }

        var aspNetUser = await _userManager.FindByEmailAsync(userInfo.UserEmail);
        
        // If user doesn't exist in AspNetUsers, create them
        if (aspNetUser == null)
        {
            aspNetUser = new PAOIdentityUser
            {
                UserName = userInfo.UserEmail,
                Email = userInfo.UserEmail,
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            var createResult = await _userManager.CreateAsync(aspNetUser);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user account: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        // Check org unit access for ORG_UNIT_ADMIN
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            var currentUserOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            if (userInfo.OrgUnit != currentUserOrgUnit)
            {
                throw new UnauthorizedAccessException("Access denied. You can only update users from your organization unit.");
            }

            // ORG_UNIT_ADMIN can only assign certain roles
            var allowedRoles = new[] { "UNOPS_GEN_USER", "PARTNER_USER", "ORG_UNIT_ADMIN" };
            var invalidRoles = request.Roles.Except(allowedRoles).ToList();
            if (invalidRoles.Any())
            {
                throw new UnauthorizedAccessException($"Access denied. You cannot assign the following roles: {string.Join(", ", invalidRoles)}");
            }
        }

        // Ensure UNOPS_GEN_USER is always included
        var rolesToAssign = request.Roles.ToList();
        if (!rolesToAssign.Contains("UNOPS_GEN_USER"))
        {
            rolesToAssign.Add("UNOPS_GEN_USER");
        }

        // Validate that all requested roles exist
        var availableRoles = await GetAvailableRolesAsync(user);
        var availableRoleNames = availableRoles.Select(r => r.Name).ToList();
        var invalidRequestedRoles = rolesToAssign.Except(availableRoleNames).ToList();
        
        if (invalidRequestedRoles.Any())
        {
            throw new ArgumentException($"Invalid roles specified: {string.Join(", ", invalidRequestedRoles)}");
        }

        // Get current roles
        var currentRoles = await _userManager.GetRolesAsync(aspNetUser);

        // Remove roles that are no longer needed
        var rolesToRemove = currentRoles.Except(rolesToAssign).ToList();
        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(aspNetUser, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to remove roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
            }
        }

        // Add new roles
        var rolesToAdd = rolesToAssign.Except(currentRoles).ToList();
        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(aspNetUser, rolesToAdd);
            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add roles: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
            }
        }

        // Update the UserInfo last modified date
        userInfo.LastModifiedDate = DateTime.UtcNow;
        userInfo.LastModifiedBy = int.Parse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _context.SaveChangesAsync();

        // Return updated user model
        return await GetUserByIdAsync(user, userId);
    }

    public async Task<IEnumerable<RoleModel>> GetAvailableRolesAsync(ClaimsPrincipal user)
    {
        // Security check
        if (!user.IsInRole("PARTNER_GLOB_ADMIN") && !user.IsInRole("ORG_UNIT_ADMIN"))
        {
            throw new UnauthorizedAccessException("Access denied. Only Partnership Global Admins and Org Unit Admins can view available roles.");
        }

        var roles = await _roleManager.Roles.ToListAsync();

        // Filter roles based on user permissions
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            // ORG_UNIT_ADMIN can only see and assign certain roles
            var allowedRoleNames = new[] { "UNOPS_GEN_USER", "PARTNER_USER", "ORG_UNIT_ADMIN" };
            roles = roles.Where(r => allowedRoleNames.Contains(r.Name)).ToList();
        }

        return roles.Select(r => new RoleModel
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = r.Description ?? string.Empty
        }).OrderBy(r => r.Name);
    }

    public async Task<bool> GetOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode)
    {
        // Security check
        if (!user.IsInRole("PARTNER_GLOB_ADMIN") && !user.IsInRole("ORG_UNIT_ADMIN"))
        {
            throw new UnauthorizedAccessException("Access denied. Only Partnership Global Admins and Org Unit Admins can view organization settings.");
        }

        // Find the organization unit
        var orgUnit = await _context.OrganizationHierarchies
            .Where(o => o.Code == orgUnitCode && !o.IsDeleted)
            .FirstOrDefaultAsync();

        if (orgUnit == null)
        {
            throw new ArgumentException($"Organization unit with code '{orgUnitCode}' not found.");
        }

        // Check org unit access for ORG_UNIT_ADMIN
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            var currentUserOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            if (orgUnit.Code != currentUserOrgUnit)
            {
                throw new UnauthorizedAccessException("Access denied. You can only view settings for your organization unit.");
            }
        }

        return orgUnit.IsSelfManagementEnabled;
    }

    public async Task UpdateOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode, UpdateOrgUnitSelfManagementRequest request)
    {
        // Security check
        if (!user.IsInRole("PARTNER_GLOB_ADMIN") && !user.IsInRole("ORG_UNIT_ADMIN"))
        {
            throw new UnauthorizedAccessException("Access denied. Only Partnership Global Admins and Org Unit Admins can update organization settings.");
        }

        // Find the organization unit
        var orgUnit = await _context.OrganizationHierarchies
            .Where(o => o.Code == orgUnitCode && !o.IsDeleted)
            .FirstOrDefaultAsync();

        if (orgUnit == null)
        {
            throw new ArgumentException($"Organization unit with code '{orgUnitCode}' not found.");
        }

        // Check org unit access for ORG_UNIT_ADMIN
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            var currentUserOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            if (orgUnit.Code != currentUserOrgUnit)
            {
                throw new UnauthorizedAccessException("Access denied. You can only update settings for your organization unit.");
            }
        }

        // Update the self-management setting
        orgUnit.IsSelfManagementEnabled = request.IsSelfManagementEnabled;
        orgUnit.LastModifiedDate = DateTime.UtcNow;
        orgUnit.LastModifiedBy = int.Parse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        await _context.SaveChangesAsync();
    }
} 