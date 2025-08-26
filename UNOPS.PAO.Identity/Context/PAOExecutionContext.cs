namespace UNOPS.PAO.Identity.Context;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Claims;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Identity.Security;

public class PAOExecutionContext : IPAOExecutionContext
{
    private readonly UserManager<PAOIdentityUser> userManager;
    private readonly RoleManager<PAOIdentityRole> roleManager;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<PAOExecutionContext> logger;

    private IEnumerable<Permission>? userPermissions;

    public IEnumerable<Permission> UserPermissions
    {
        get
        {
            if (userPermissions == null)
            {
                userPermissions = new List<Permission>();

                var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                logger.LogInformation("PAOExecutionContext - Found NameIdentifier claim: {UserId}", userId);

                if (userId != null)
                {
                    try
                    {
                        var user = userManager.FindByIdAsync(userId).Result;
                        logger.LogInformation("PAOExecutionContext - User lookup result: {UserFound}", user != null);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            var roles = userManager.GetRolesAsync(user).Result;
                            logger.LogInformation("PAOExecutionContext - Found roles for user: {Roles}", string.Join(", ", roles));

                            foreach (var roleName in roles)
                            {
                                var role = roleManager.FindByNameAsync(roleName).Result;
                                if (role != null)
                                {
                                    var claims = roleManager.GetClaimsAsync(role).Result;
                                    var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
                                    logger.LogInformation("PAOExecutionContext - Found permissions for role {Role}: {Permissions}", 
                                        roleName, string.Join(", ", permissions));

                                    foreach (var permissionName in permissions)
                                    {
                                        var permissionField = typeof(Permission).GetField(permissionName, BindingFlags.Static | BindingFlags.Public);
                                        if (permissionField != null)
                                        {
                                            var permission = permissionField.GetValue(null) as Permission;
                                            if (permission != null)
                                            {
                                                userPermissions = userPermissions.Concat(new[] { permission }).ToList();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "PAOExecutionContext - Error getting user permissions for userId: {UserId}", userId);
                        throw;
                    }
                }
                else
                {
                    logger.LogWarning("PAOExecutionContext - No NameIdentifier claim found in user context");
                }
            }

            return userPermissions;
        }
    }

    public PAOExecutionContext(
        UserManager<PAOIdentityUser> userManager, 
        RoleManager<PAOIdentityRole> roleManager, 
        IHttpContextAccessor httpContextAccessor,
        ILogger<PAOExecutionContext> logger)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }
}
