using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSUserManagementManager : BaseUNOPSManager, IUserManagementManager
{
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;
    private readonly IPermissionService _permissionService;
    private readonly IGeminiManager _geminiManager;
    private readonly ILogger<UNOPSUserManagementManager> _logger;

    public UNOPSUserManagementManager(
        IMapper mapper,
        UNOPSAppDbContext context,
        IConfiguration configuration,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        IPermissionService permissionService,
        IGeminiManager geminiManager,
        ILogger<UNOPSUserManagementManager> logger)
        : base(mapper, context, configuration, userManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionService = permissionService;
        _geminiManager = geminiManager;
        _logger = logger;
    }

    public async Task<PaginationResponse<UserManagementModel>> GetUsersAsync(ClaimsPrincipal user, UserManagementRequest request)
    {
        // Start with UserProfile query
        var userProfileQuery = _context.UserProfile.Where(u => !u.IsDeleted);

        // Apply "Show My Org Unit Only" filter if requested
        if (request.ShowMyOrgUnitOnly)
        {
            var currentUserOrgUnit = await _permissionService.GetUserOrgUnitAsync(user);
            if (!string.IsNullOrEmpty(currentUserOrgUnit))
            {
                userProfileQuery = userProfileQuery.Where(x => x.OrgUnit == currentUserOrgUnit);
            }
        }

        // Apply org unit filter if specified
        if (!string.IsNullOrEmpty(request.OrgUnitFilter))
        {
            userProfileQuery = userProfileQuery.Where(x => x.OrgUnit != null && x.OrgUnit.Contains(request.OrgUnitFilter));
        }

        // Apply search term filter - use actual database fields instead of computed Name property
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            userProfileQuery = userProfileQuery.Where(x => 
                (x.FirstName != null && x.FirstName.ToLower().Contains(searchLower)) ||
                (x.LastName != null && x.LastName.ToLower().Contains(searchLower)) ||
                (x.UserEmail != null && x.UserEmail.ToLower().Contains(searchLower)));
        }

        // Apply sorting - use actual database fields instead of computed Name property
        userProfileQuery = request.SortBy?.ToLower() switch
        {
            "email" => request.SortDirection?.ToLower() == "desc" 
                ? userProfileQuery.OrderByDescending(x => x.UserEmail)
                : userProfileQuery.OrderBy(x => x.UserEmail),
            "orgunit" => request.SortDirection?.ToLower() == "desc"
                ? userProfileQuery.OrderByDescending(x => x.OrgUnit)
                : userProfileQuery.OrderBy(x => x.OrgUnit),
            "lastmodified" => request.SortDirection?.ToLower() == "desc"
                ? userProfileQuery.OrderByDescending(x => x.LastModifiedDate)
                : userProfileQuery.OrderBy(x => x.LastModifiedDate),
            _ => request.SortDirection?.ToLower() == "desc"
                ? userProfileQuery.OrderByDescending(x => x.FirstName ?? x.LastName ?? x.UserEmail)
                : userProfileQuery.OrderBy(x => x.FirstName ?? x.LastName ?? x.UserEmail)
        };

        // Get total count before pagination
        var totalCount = await userProfileQuery.CountAsync();

        // Apply pagination
        var pagedUserProfiles = await userProfileQuery
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Get user roles for each user
        var userModels = new List<UserManagementModel>();
        foreach (var userProfile in pagedUserProfiles)
        {
            if (string.IsNullOrEmpty(userProfile.UserEmail)) continue;

            var aspNetUser = await _userManager.FindByEmailAsync(userProfile.UserEmail);
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
                UserId = userProfile.UserId.ToString(),
                Name = userProfile.Name ?? "N/A",
                Email = userProfile.UserEmail ?? "N/A",
                OrgUnit = userProfile.OrgUnit ?? "N/A",
                OrgUnitCode = userProfile.OrgUnit,
                Roles = roles,
                LastModifiedDate = userProfile.LastModifiedDate,
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
            PageSize = request.PageSize
        };
    }

    public async Task<UserManagementModel?> GetUserByIdAsync(ClaimsPrincipal user, string userId)
    {
        // RBAC interceptor handles security enforcement
        if (!int.TryParse(userId, out int userIdInt))
        {
            return null; // Invalid userId format
        }
        
        var userProfile = await _context.UserProfile
            .Where(u => u.UserId == userIdInt && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (userProfile == null) return null;

        var aspNetUser = await _userManager.FindByEmailAsync(userProfile.UserEmail);
        if (aspNetUser == null) return null;

        // Additional org unit check for ORG_UNIT_ADMIN (business logic)
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            var currentUserOrgUnit = await _permissionService.GetUserOrgUnitAsync(user);
            if (userProfile.OrgUnit != currentUserOrgUnit)
            {
                throw new UnauthorizedAccessException("Access denied. You can only view users from your organization unit.");
            }
        }

        var roles = await _userManager.GetRolesAsync(aspNetUser);

        return new UserManagementModel
        {
            UserId = userProfile.UserId.ToString(),
            Name = userProfile.Name ?? "N/A",
            Email = userProfile.UserEmail ?? "N/A",
            OrgUnit = userProfile.OrgUnit ?? "N/A",
            OrgUnitCode = userProfile.OrgUnit,
            Roles = roles.ToList(),
            LastModifiedDate = DateTime.UtcNow, // Use current time since we don't track this in UserProfile
            IsActive = !aspNetUser.LockoutEnabled || 
                      (aspNetUser.LockoutEnd == null || aspNetUser.LockoutEnd <= DateTimeOffset.UtcNow)
        };
    }

    public async Task<UserManagementModel?> UpdateUserRolesAsync(ClaimsPrincipal user, string userId, UpdateUserRolesRequest request)
    {
        // RBAC interceptor handles security enforcement
        if (!int.TryParse(userId, out int userIdInt))
        {
            throw new ArgumentException("Invalid userId format. UserId must be a valid integer.", nameof(userId));
        }
        
        var userProfile = await _context.UserProfile
            .Where(u => u.UserId == userIdInt && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (userProfile == null)
        {
            throw new ArgumentException("User not found.");
        }

        var aspNetUser = await _userManager.FindByEmailAsync(userProfile.UserEmail);
        
        // If user doesn't exist in AspNetUsers, create them
        if (aspNetUser == null)
        {
            aspNetUser = new PAOIdentityUser
            {
                UserName = userProfile.UserEmail,
                Email = userProfile.UserEmail,
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            var createResult = await _userManager.CreateAsync(aspNetUser);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user account: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        // Additional org unit and role validation for ORG_UNIT_ADMIN (business logic)
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            // var currentUserOrgUnit = await _securityService.GetUserOrgUnitAsync(user);
            // if (userProfile.OrgUnit != currentUserOrgUnit)
            // {
            //     throw new UnauthorizedAccessException("Access denied. You can only update users from your organization unit.");
            // }

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

        // Update the UserProfile last modified date
        userProfile.LastModifiedDate = DateTime.UtcNow;
        userProfile.LastModifiedBy = int.Parse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _context.SaveChangesAsync();

        // Return updated user model
        return await GetUserByIdAsync(user, userId);
    }

    public async Task<IEnumerable<RoleModel>> GetAvailableRolesAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        var roles = await _roleManager.Roles.ToListAsync();

        // Filter roles based on user permissions (business logic)
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
        // RBAC interceptor handles security enforcement
        // Find the organization unit
        var orgUnit = await _context.OrganizationHierarchies
            .Where(o => o.Code == orgUnitCode && !o.IsDeleted && o.Type == OrganizationUnitType.OrgUnit)
            .FirstOrDefaultAsync();

        if (orgUnit == null)
        {
            throw new ArgumentException($"Organization unit with code '{orgUnitCode}' not found.");
        }

        // Additional org unit check for ORG_UNIT_ADMIN (business logic)
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            var currentUserOrgUnit = await _permissionService.GetUserOrgUnitAsync(user);
            if (orgUnit.Code != currentUserOrgUnit)
            {
                throw new UnauthorizedAccessException("Access denied. You can only view settings for your organization unit.");
            }
        }

        return orgUnit.IsSelfManagementEnabled;
    }

    public async Task UpdateOrgUnitSelfManagementAsync(ClaimsPrincipal user, string orgUnitCode, UpdateOrgUnitSelfManagementRequest request)
    {
        // RBAC interceptor handles security enforcement
        // Find the organization unit
        var orgUnit = await _context.OrganizationHierarchies
            .Where(o => o.Code == orgUnitCode && !o.IsDeleted && o.Type == OrganizationUnitType.OrgUnit)
            .FirstOrDefaultAsync();

        if (orgUnit == null)
        {
            throw new ArgumentException($"Organization unit with code '{orgUnitCode}' not found.");
        }

        // Additional org unit check for ORG_UNIT_ADMIN (business logic)
        if (user.IsInRole("ORG_UNIT_ADMIN") && !user.IsInRole("PARTNER_GLOB_ADMIN"))
        {
            var currentUserOrgUnit = await _permissionService.GetUserOrgUnitAsync(user);
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

    /// <summary>
    /// Gets basic entity data for AI prompts and generic operations
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        // For user management, we need to convert int to string
        // This is a temporary compatibility layer
        var userId = entityId.ToString();
        
        if (user != null)
        {
            return await GetUserByIdAsync(user, userId);
        }
        
        // Fallback for cases without user context
        if (!int.TryParse(userId, out int userIdInt))
        {
            return null; // Invalid userId format
        }
        
        var userProfile = await _context.UserProfile
            .Where(u => u.UserId == userIdInt && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (userProfile == null) return null;

        return new UserManagementModel
        {
            UserId = userProfile.UserId.ToString(),
            Name = userProfile.Name ?? "N/A",
            Email = userProfile.UserEmail ?? "N/A",
            OrgUnit = userProfile.OrgUnit ?? "N/A",
            OrgUnitCode = userProfile.OrgUnit,
            Roles = new List<string>(),
            LastModifiedDate = DateTime.UtcNow, // Use current time since we don't track this in UserProfile
            IsActive = true
        };
    }

    public async Task<object> AnalyzeUserRoleFileAsync(ClaimsPrincipal user, AnalyseFileRequest request)
    {
        try
        {
            // Get current user ID
            var currentUserId = int.Parse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Use the GeminiManager to analyze the file
            var analysisResult = await _geminiManager.ExtractDataAfterAnalysis(request, currentUserId);
            
            return analysisResult;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to analyze user role file: {ex.Message}", ex);
        }
    }

    public async Task<object> BulkUploadUserRolesAsync(ClaimsPrincipal user, BulkUploadRequest request)
    {
        try
        {
            if (request.Records == null || !request.Records.Any())
            {
                throw new ArgumentException("No records provided for import");
            }

            var successCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            foreach (var record in request.Records)
            {
                try
                {
                    var recordJson = JsonConvert.SerializeObject(record);
                    var userRoleData = JsonConvert.DeserializeObject<dynamic>(recordJson);
                    
                    // Extract user ID and role IDs from the processed data
                    var userId = userRoleData.userId?.ToObject<int?>();
                    var roleIds = userRoleData.roleIds?.ToObject<List<string>>();
                    
                    if (userId == null)
                    {
                        errors.Add("No valid user ID found in record");
                        errorCount++;
                        continue;
                    }
                    
                    if (roleIds == null || !roleIds.Any())
                    {
                        errors.Add("No valid role IDs found in record");
                        errorCount++;
                        continue;
                    }

                    // Process the user-role assignment
                    var updateRequest = new UpdateUserRolesRequest
                    {
                        Roles = roleIds.ToArray()
                    };
                    
                    await UpdateUserRolesAsync(user, userId.Value.ToString(), updateRequest);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errors.Add($"Error processing record: {ex.Message}");
                }
            }

            var result = new
            {
                IsSuccess = errorCount == 0,
                SuccessCount = successCount,
                ErrorCount = errorCount,
                Errors = errors,
                Message = errorCount == 0 ? 
                    $"Successfully imported {successCount} user role assignments" :
                    $"Imported {successCount} user role assignments with {errorCount} errors"
            };

            return new { message = JsonConvert.SerializeObject(result) };
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                IsSuccess = false,
                SuccessCount = 0,
                ErrorCount = 1,
                Errors = new[] { ex.Message },
                Message = $"Bulk upload failed: {ex.Message}"
            };

            return new { message = JsonConvert.SerializeObject(errorResult) };
        }
    }

    public async Task<Dictionary<int, object>> ResolveUsersAsync(ClaimsPrincipal user, ResolveUsersRequest request)
    {
        var result = new Dictionary<int, object>();
        
        foreach (var userId in request.UserIds)
        {
            try
            {
                var userProfile = await _context.UserProfile
                    .Where(u => u.UserId == userId)
                    .FirstOrDefaultAsync();
                
                if (userProfile != null)
                {
                    // Use the computed Name property from the entity
                    var displayName = !string.IsNullOrEmpty(userProfile.Name) ? userProfile.Name : userProfile.UserEmail;
                    
                    result[userId] = new { 
                        name = !string.IsNullOrEmpty(displayName) ? displayName : $"User {userId}", 
                        email = userProfile.UserEmail ?? ""
                    };
                }
                else
                {
                    result[userId] = new { 
                        name = $"User {userId}", 
                        email = "Unknown" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving user ID {UserId}", userId);
                result[userId] = new { 
                    name = $"User {userId}", 
                    email = "Error" 
                };
            }
        }
        
        return result;
    }

    public async Task<Dictionary<int, object>> ResolveRolesAsync(ClaimsPrincipal user, ResolveRolesRequest request)
    {
        var result = new Dictionary<int, object>();
        
        foreach (var roleId in request.RoleIds)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                
                if (role != null)
                {
                    result[roleId] = new { 
                        name = role.Name, 
                        description = role.Description ?? role.Name 
                    };
                }
                else
                {
                    result[roleId] = new { 
                        name = $"Role {roleId}", 
                        description = "Unknown" 
                    };
                }
            }
            catch (Exception ex)
            {
                result[roleId] = new { 
                    name = $"Role {roleId}", 
                    description = "Error" 
                };
            }
        }
        
        return result;
    }
} 