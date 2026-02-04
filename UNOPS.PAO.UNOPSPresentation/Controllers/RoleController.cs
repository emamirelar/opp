using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace UNOPS.PAO.UNOPSPresentation.Controllers
{
    /// <summary>
    /// Request model for assigning DOA roles
    /// </summary>
    public class DoaRoleAssignmentRequest
    {
        public int EntityId { get; set; }      // Organization hierarchy ID
        public int UserId { get; set; }        // User ID
        public string RoleName { get; set; } = string.Empty;  // DOA Role Name ('DoA2' or 'DoA3')
        public string EntityType { get; set; } = "OrganizationHierarchy";
    }

    /// <summary>
    /// Response model for DOA role assignment
    /// </summary>
    public class DoaRoleAssignmentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
    }

    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<PAOIdentityUser> _userManager;
        private readonly RoleManager<PAOIdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly UserResolverService<int> _userResolverService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(
            UserManager<PAOIdentityUser> userManager,
            RoleManager<PAOIdentityRole> roleManager,
            AppDbContext context,
            UserResolverService<int> userResolverService,
            ILogger<RoleController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _userResolverService = userResolverService;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new { id = r.Id, name = r.Name })
                .ToListAsync();
            
            return Ok(roles);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { 
                email = user.Email,
                roles = roles 
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserRoles([FromBody] string[] roles)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Ensure SecurityStamp is set (required for role operations)
                if (string.IsNullOrEmpty(user.SecurityStamp))
                {
                    _logger.LogInformation($"SecurityStamp is null for user {user.Email}, updating it");
                    var updateStampResult = await _userManager.UpdateSecurityStampAsync(user);
                    if (!updateStampResult.Succeeded)
                    {
                        var errors = string.Join(", ", updateStampResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to update security stamp: {errors}");
                        return BadRequest(new { 
                            message = "Failed to update user security stamp", 
                            errors = updateStampResult.Errors.Select(e => e.Description) 
                        });
                    }
                    _logger.LogInformation($"SecurityStamp updated successfully for user {user.Email}");
                }

                // Validate input roles
                if (roles == null)
                {
                    roles = new string[0];
                }

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation($"Current roles for user {user.Email}: {string.Join(", ", currentRoles)}");
                _logger.LogInformation($"New roles to assign: {string.Join(", ", roles)}");

                // Remove current roles only if there are any
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to remove current roles: {errors}");
                        return BadRequest(new { 
                            message = "Failed to remove current roles", 
                            errors = removeResult.Errors.Select(e => e.Description) 
                        });
                    }
                    _logger.LogInformation($"Successfully removed roles: {string.Join(", ", currentRoles)}");
                }

                // Add new roles only if there are any
                if (roles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, roles);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to add new roles: {errors}");
                        return BadRequest(new { 
                            message = "Failed to add new roles", 
                            errors = addResult.Errors.Select(e => e.Description) 
                        });
                    }
                    _logger.LogInformation($"Successfully added roles: {string.Join(", ", roles)}");
                }

                _logger.LogInformation($"Roles updated successfully for user {user.Email}");
                return Ok(new { message = "Roles updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user roles");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Assigns DOA roles (DOA2 or DOA3) to users for specific organization hierarchies.
        /// Inserts records into EntityUserRoles table.
        /// </summary>
        /// <param name="assignments">List of DOA role assignments to create</param>
        /// <returns>Result of the assignment operation</returns>
        [HttpPost("assign-doa-roles")]
        public async Task<IActionResult> AssignDoaRoles([FromBody] List<DoaRoleAssignmentRequest> assignments)
        {
            try
            {
                if (assignments == null || !assignments.Any())
                {
                    return BadRequest(new DoaRoleAssignmentResponse
                    {
                        Success = false,
                        Message = "No assignments provided",
                        AssignedCount = 0
                    });
                }

                var currentUserId = _userResolverService.GetCurrentUserId();
                var assignedCount = 0;
                var skippedCount = 0;

                foreach (var assignment in assignments)
                {
                    // Validate RoleName is DoA2 or DoA3
                    if (string.IsNullOrEmpty(assignment.RoleName) || 
                        (assignment.RoleName != "DoA2" && assignment.RoleName != "DoA3"))
                    {
                        _logger.LogWarning($"Invalid RoleName '{assignment.RoleName}'. Only 'DoA2' and 'DoA3' are allowed.");
                        skippedCount++;
                        continue;
                    }

                    // Look up EntityRoleId from EntityRoles table
                    var entityRole = await _context.EntityRoles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(er => 
                            er.EntityType == "OrganizationHierarchy" && 
                            er.Name == assignment.RoleName &&
                            !er.IsDeleted);

                    if (entityRole == null)
                    {
                        _logger.LogWarning($"EntityRole not found for EntityType='OrganizationHierarchy' and Name='{assignment.RoleName}'");
                        skippedCount++;
                        continue;
                    }

                    var entityRoleId = entityRole.Id;

                    // Check if the assignment already exists
                    var existingAssignment = await _context.EntityUserRoles
                        .FirstOrDefaultAsync(e => 
                            e.EntityId == assignment.EntityId &&
                            e.UserId == assignment.UserId &&
                            e.EntityRoleId == entityRoleId &&
                            e.EntityType == "OrganizationHierarchy" &&
                            !e.IsDeleted);

                    if (existingAssignment != null)
                    {
                        _logger.LogInformation($"DOA role assignment already exists for EntityId={assignment.EntityId}, UserId={assignment.UserId}, EntityRoleId={entityRoleId}");
                        skippedCount++;
                        continue;
                    }

                    // Generate a unique ID using hash of composite key (similar to BigQuery sync)
                    var compositeKey = $"{assignment.EntityId}-{entityRoleId}-{assignment.UserId}";
                    var hashId = Math.Abs(compositeKey.GetHashCode()) % int.MaxValue;
                    
                    // Check if this ID already exists, if so generate a new one
                    while (await _context.EntityUserRoles.AnyAsync(e => e.Id == hashId))
                    {
                        hashId = (hashId + 1) % int.MaxValue;
                    }

                    // Look up org unit code for better Name
                    var orgUnit = await _context.OrganizationHierarchies
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id == assignment.EntityId);
                    var orgUnitCode = orgUnit?.Code ?? assignment.EntityId.ToString();
                    
                    // Look up user name for better Name
                    var user = await _context.PAOUsers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == assignment.UserId);
                    var userName = user?.Name ?? $"User {assignment.UserId}";
                    
                    // Format: DoA2 - B0048 - 123456 (John Doe) - similar to BigQuery sync
                    var assignmentName = $"{assignment.RoleName} - {orgUnitCode} - {assignment.UserId} ({userName})";

                    // Create new EntityUserRole
                    var entityUserRole = new EntityUserRole
                    {
                        Id = hashId,
                        Name = assignmentName,
                        EntityId = assignment.EntityId,
                        UserId = assignment.UserId,
                        EntityRoleId = entityRoleId,
                        EntityType = "OrganizationHierarchy",
                        Status = EntityStatus.Active,
                        CreatedBy = currentUserId,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = currentUserId,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        WorkflowStatus = WorkflowStatus.None
                    };

                    _context.EntityUserRoles.Add(entityUserRole);
                    assignedCount++;
                    _logger.LogInformation($"Created DOA role assignment: EntityId={assignment.EntityId}, UserId={assignment.UserId}, RoleName={assignment.RoleName}, EntityRoleId={entityRoleId}");
                }

                await _context.SaveChangesAsync();

                var message = assignedCount > 0 
                    ? $"Successfully assigned {assignedCount} DOA role(s)."
                    : "No new assignments were created.";
                
                if (skippedCount > 0)
                {
                    message += $" {skippedCount} assignment(s) were skipped (already exist or invalid).";
                }

                return Ok(new DoaRoleAssignmentResponse
                {
                    Success = true,
                    Message = message,
                    AssignedCount = assignedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning DOA roles");
                return StatusCode(500, new DoaRoleAssignmentResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    AssignedCount = 0
                });
            }
        }

        /// <summary>
        /// Gets all existing DOA role assignments (DOA2 and DOA3) from EntityUserRoles table.
        /// </summary>
        /// <returns>List of existing DOA role assignments with org unit and user details</returns>
        [HttpGet("doa-roles")]
        public async Task<IActionResult> GetDoaRoles()
        {
            try
            {
                // Get DOA role codes
                var doaRoleCodes = new[] { "DoA2_OrganizationHierarchy", "DoA3_OrganizationHierarchy" };
                
                // Get EntityRoleIds for DOA2 and DOA3
                var doaRoleIds = await _context.EntityRoles
                    .AsNoTracking()
                    .Where(er => er.EntityType == "OrganizationHierarchy" && 
                                doaRoleCodes.Contains(er.Code) &&
                                !er.IsDeleted)
                    .Select(er => er.Id)
                    .ToListAsync();

                if (!doaRoleIds.Any())
                {
                    return Ok(new List<object>());
                }

                // Get all DOA role assignments
                var doaRoles = await _context.EntityUserRoles
                    .AsNoTracking()
                    .Include(eur => eur.EntityRole)
                    .Include(eur => eur.User)
                        .ThenInclude(u => u!.UserProfile)
                    .Where(eur => eur.EntityType == "OrganizationHierarchy" &&
                                 eur.EntityRoleId.HasValue &&
                                 doaRoleIds.Contains(eur.EntityRoleId.Value) &&
                                 !eur.IsDeleted)
                    .ToListAsync();

                // Get org unit details
                var orgUnitIds = doaRoles.Select(r => r.EntityId).Distinct().ToList();
                var orgUnits = await _context.OrganizationHierarchies
                    .AsNoTracking()
                    .Where(o => orgUnitIds.Contains(o.Id))
                    .ToDictionaryAsync(o => o.Id, o => new { o.Code, o.Name });

                // Map to response
                var result = doaRoles.Select(r => new
                {
                    id = r.Id,
                    entityId = r.EntityId,
                    orgUnitCode = orgUnits.ContainsKey(r.EntityId) ? orgUnits[r.EntityId].Code : r.EntityId.ToString(),
                    orgUnitName = orgUnits.ContainsKey(r.EntityId) ? orgUnits[r.EntityId].Name : "Unknown",
                    userId = r.UserId,
                    userName = r.User?.UserProfile?.Name ?? r.User?.Email ?? $"User {r.UserId}",
                    userEmail = r.User?.Email ?? "",
                    entityRoleId = r.EntityRoleId ?? 0,
                    roleName = r.EntityRole?.Name ?? "Unknown",
                    roleCode = r.EntityRole?.Code ?? "Unknown",
                    createdDate = r.CreatedDate
                })
                .OrderBy(r => r.orgUnitCode)
                .ThenBy(r => r.roleName)
                .ThenBy(r => r.userName)
                .ToList();

                _logger.LogInformation($"Retrieved {result.Count} DOA role assignments");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DOA roles");
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Deletes a DOA role assignment by ID (soft delete).
        /// </summary>
        /// <param name="id">The EntityUserRole ID to delete</param>
        /// <returns>Success or error response</returns>
        [HttpDelete("doa-roles/{id}")]
        public async Task<IActionResult> DeleteDoaRole(int id)
        {
            try
            {
                var entityUserRole = await _context.EntityUserRoles
                    .Include(eur => eur.EntityRole)
                    .FirstOrDefaultAsync(eur => eur.Id == id && !eur.IsDeleted);

                if (entityUserRole == null)
                {
                    return NotFound(new { success = false, message = "DOA role assignment not found" });
                }

                // Verify it's a DOA role (DOA2 or DOA3)
                var doaRoleCodes = new[] { "DoA2_OrganizationHierarchy", "DoA3_OrganizationHierarchy" };
                if (entityUserRole.EntityRole == null || !doaRoleCodes.Contains(entityUserRole.EntityRole.Code))
                {
                    return BadRequest(new { success = false, message = "This is not a DOA role assignment" });
                }

                // Soft delete
                var currentUserId = _userResolverService.GetCurrentUserId();
                entityUserRole.IsDeleted = true;
                entityUserRole.LastModifiedBy = currentUserId;
                entityUserRole.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted DOA role assignment: Id={id}, EntityId={entityUserRole.EntityId}, UserId={entityUserRole.UserId}, RoleCode={entityUserRole.EntityRole?.Code}");

                return Ok(new { success = true, message = "DOA role assignment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting DOA role {id}");
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
} 