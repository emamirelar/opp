using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

// DTO for SQL query
public class UserRoleDto
{
    public string Email { get; set; }
    public string RoleName { get; set; }
}

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
        // OPTIMIZED APPROACH: Use SQL to get filtered UserIds, then get full data for those users
        
        // Step 1: Build SQL query to get filtered UserIds with role filtering at database level
        var sqlParams = new List<object>();
        var paramIndex = 0;

        // Build WHERE conditions
        var whereConditions = new List<string> { @"up.""IsDeleted"" = false" };

        // Add "Show My Org Unit Only" filter if requested
        if (request.ShowMyOrgUnitOnly)
        {
            var currentUserOrgUnit = await _permissionService.GetUserOrgUnitAsync(user);
            if (!string.IsNullOrEmpty(currentUserOrgUnit))
            {
                // For a single value, we still use ANY but wrap it in an array
                whereConditions.Add($@"up.""OrgUnit"" = ANY(@p{paramIndex})");
                sqlParams.Add(new[] { currentUserOrgUnit });
                paramIndex++;
            }
        }

        // Add org unit filter if provided (and not already filtered by ShowMyOrgUnitOnly)
        if (!request.ShowMyOrgUnitOnly && request.OrgUnitFilter != null && request.OrgUnitFilter.Any())
        {
            var filteredOrgUnitCodes = await _context.OrganizationHierarchies
                .Where(o => request.OrgUnitFilter.Contains(o.Id))
                .Select(o => o.Code)
                .ToListAsync();
            
            if (filteredOrgUnitCodes.Any())
            {
                whereConditions.Add($@"up.""OrgUnit"" = ANY(@p{paramIndex})");
                sqlParams.Add(filteredOrgUnitCodes.ToArray());
                paramIndex++;
            }
        }

        // Add search filter if provided
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            whereConditions.Add($@"(
                LOWER(up.""FirstName"") LIKE @p{paramIndex} OR 
                LOWER(up.""LastName"") LIKE @p{paramIndex} OR 
                LOWER(up.""UserEmail"") LIKE @p{paramIndex}
            )");
            sqlParams.Add($"%{request.SearchTerm.ToLower()}%");
            paramIndex++;
        }

        var whereClause = string.Join(" AND ", whereConditions);

        // Build HAVING clause for role filter
        var havingClause = "";
        if (request.RoleFilter != null && request.RoleFilter.Any())
        {
            var roleConditions = request.RoleFilter.Select((role, idx) => 
            {
                var paramName = $"@p{paramIndex + idx}";
                sqlParams.Add($"%{role}%");
                return $@"STRING_AGG(r.""Name"", ',') LIKE {paramName}";
            });
            havingClause = "HAVING " + string.Join(" OR ", roleConditions);
            paramIndex += request.RoleFilter.Count();
        }

        // Build ORDER BY clause
        var orderByColumn = request.SortBy?.ToLower() switch
        {
            "email" => @"up.""UserEmail""",
            "orgunit" => @"up.""OrgUnit""",
            "lastmodified" => @"up.""LastModifiedDate""",
            _ => @"up.""FirstName"""
        };
        var orderByDirection = request.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";
        var orderBy = $"{orderByColumn} {orderByDirection}";

        // Execute SQL to get filtered UserIds
        var sql = $@"
            SELECT up.""UserId""
            FROM public.""UserProfile"" up
            LEFT JOIN public.""AspNetUsers"" u ON up.""UserEmail"" = u.""Email""
            LEFT JOIN public.""AspNetUserRoles"" ur ON u.""Id"" = ur.""UserId""
            LEFT JOIN public.""AspNetRoles"" r ON ur.""RoleId"" = r.""Id""
            WHERE {whereClause}
            GROUP BY up.""UserId""
            {havingClause}
            ORDER BY {orderBy}";

        var filteredUserIds = await _context.Database
            .SqlQueryRaw<int>(sql, sqlParams.ToArray())
            .ToListAsync();

        // Get total count
        var totalCount = filteredUserIds.Count;

        // Apply pagination to UserIds
        var pagedUserIds = filteredUserIds
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Step 2: Get full UserProfile data for the filtered and paginated UserIds
        var pagedUserProfiles = await _context.UserProfile
            .Where(u => pagedUserIds.Contains(u.UserId))
            .ToListAsync();

        // Restore original sorting on the in-memory list
        pagedUserProfiles = request.SortBy?.ToLower() switch
        {
            "email" => request.SortDirection?.ToLower() == "desc" 
                ? pagedUserProfiles.OrderByDescending(x => x.UserEmail).ToList()
                : pagedUserProfiles.OrderBy(x => x.UserEmail).ToList(),
            "orgunit" => request.SortDirection?.ToLower() == "desc"
                ? pagedUserProfiles.OrderByDescending(x => x.OrgUnit).ToList()
                : pagedUserProfiles.OrderBy(x => x.OrgUnit).ToList(),
            "lastmodified" => request.SortDirection?.ToLower() == "desc"
                ? pagedUserProfiles.OrderByDescending(x => x.LastModifiedDate).ToList()
                : pagedUserProfiles.OrderBy(x => x.LastModifiedDate).ToList(),
            _ => request.SortDirection?.ToLower() == "desc"
                ? pagedUserProfiles.OrderByDescending(x => x.FirstName ?? x.LastName ?? x.UserEmail).ToList()
                : pagedUserProfiles.OrderBy(x => x.FirstName ?? x.LastName ?? x.UserEmail).ToList()
        };

        // Get organization hierarchy data
        var orgUnitCodes = pagedUserProfiles.Select(x => x.OrgUnit).Distinct().ToList();
        var orgHierarchies = await _context.OrganizationHierarchies
            .Where(o => orgUnitCodes.Contains(o.Code) && o.Type == OrganizationUnitType.OrgUnit)
            .GroupBy(o => o.Code)
            .ToDictionaryAsync(g => g.Key, g => g.First());

        // Get all user emails
        var userEmails = pagedUserProfiles.Select(x => x.UserEmail).Where(e => !string.IsNullOrEmpty(e)).ToList();

        Dictionary<string, PAOIdentityUser> aspNetUsers;
        Dictionary<string, List<string>> userRolesDict;

        if (userEmails.Any())
        {
            // Get AspNetUsers
            aspNetUsers = await _userManager.Users
                .Where(u => userEmails.Contains(u.Email))
                .ToDictionaryAsync(u => u.Email, u => u);

            // Get all roles in a single query using proper array parameter
            var emailParam = new Npgsql.NpgsqlParameter("@p0", NpgsqlDbType.Array | NpgsqlDbType.Text)
            {
                Value = userEmails.ToArray()
            };
            
            var userRolesQuery = await _context.Database.SqlQueryRaw<UserRoleDto>(@"
                SELECT u.""Email"", r.""Name"" as RoleName
                FROM ""AspNetUsers"" u
                INNER JOIN ""AspNetUserRoles"" ur ON u.""Id"" = ur.""UserId""
                INNER JOIN ""AspNetRoles"" r ON ur.""RoleId"" = r.""Id""
                WHERE u.""Email"" = ANY(@p0)
            ", emailParam).ToListAsync();

            userRolesDict = userRolesQuery
                .GroupBy(ur => ur.Email)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());
        }
        else
        {
            aspNetUsers = new Dictionary<string, PAOIdentityUser>();
            userRolesDict = new Dictionary<string, List<string>>();
        }

        // Build final result
        var userModels = new List<UserManagementModel>();
        foreach (var userProfile in pagedUserProfiles)
        {
            if (string.IsNullOrEmpty(userProfile.UserEmail)) continue;

            var aspNetUser = aspNetUsers.GetValueOrDefault(userProfile.UserEmail);
            var roles = userRolesDict.GetValueOrDefault(userProfile.UserEmail, new List<string>());
            var isActive = aspNetUser != null 
                ? !aspNetUser.LockoutEnabled || (aspNetUser.LockoutEnd == null || aspNetUser.LockoutEnd <= DateTimeOffset.UtcNow)
                : true;

            var orgHierarchy = orgHierarchies.GetValueOrDefault(userProfile.OrgUnit);

            userModels.Add(new UserManagementModel
            {
                UserId = userProfile.UserId.ToString(),
                Name = userProfile.Name ?? "N/A",
                Email = userProfile.UserEmail ?? "N/A",
                OrgUnit = orgHierarchy?.Description ?? userProfile.OrgUnit ?? "N/A",
                OrgUnitCode = userProfile.OrgUnit,
                OrgUnitDescription = orgHierarchy?.Name,
                Roles = roles,
                LastModifiedDate = userProfile.LastModifiedDate,
                IsActive = isActive
            });
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
        
        var userProfileWithOrg = await (from up in _context.UserProfile.Where(u => u.UserId == userIdInt && !u.IsDeleted)
                                        join oh in _context.OrganizationHierarchies on up.OrgUnit equals oh.Code into orgJoin
                                        from org in orgJoin.DefaultIfEmpty()
                                        select new { UserProfile = up, OrgHierarchy = org })
                                        .FirstOrDefaultAsync();

        if (userProfileWithOrg?.UserProfile == null) return null;
        
        var userProfile = userProfileWithOrg.UserProfile;
        var orgHierarchy = userProfileWithOrg.OrgHierarchy;

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
            OrgUnit = orgHierarchy?.Description ?? userProfile.OrgUnit ?? "N/A",
            OrgUnitCode = userProfile.OrgUnit,
            OrgUnitDescription = orgHierarchy?.Name,
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
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createResult = await _userManager.CreateAsync(aspNetUser);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user account: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        // Ensure SecurityStamp is set for existing users (required for role operations)
        if (string.IsNullOrEmpty(aspNetUser.SecurityStamp))
        {
            var updateStampResult = await _userManager.UpdateSecurityStampAsync(aspNetUser);
            if (!updateStampResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update user security stamp: {string.Join(", ", updateStampResult.Errors.Select(e => e.Description))}");
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

    public async Task<IEnumerable<OrgUnitModel>> GetAvailableOrgUnitsAsync(ClaimsPrincipal user)
    {
        // RBAC interceptor handles security enforcement
        var orgUnits = await _context.OrganizationHierarchies
            .Where(o => !o.IsDeleted && o.Status == EntityStatus.Active && o.Type == OrganizationUnitType.OrgUnit)
            .OrderBy(o => o.Name)
            .ToListAsync();

        return orgUnits.Select(o => new OrgUnitModel
        {
            Id = o.Id,
            Name = o.Name,
            Code = o.Code,
            Description = o.Description
        });
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