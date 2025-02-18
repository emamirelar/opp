namespace UNOPS.PAO.Identity.Context;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using System.Security.Claims;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Identity.Security;

public class PAOExecutionContext : IPAOExecutionContext
{
    private readonly UserManager<PAOIdentityUser> userManager;
    private readonly RoleManager<PAOIdentityRole> roleManager;
    private readonly IHttpContextAccessor httpContextAccessor;

    private IEnumerable<Permission>? userPermissions;

    public IEnumerable<Permission> UserPermissions
    {
        get
        {
            if (userPermissions == null)
            {
                userPermissions = new List<Permission>();

                var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    var user = userManager.FindByIdAsync(userId).Result;

                    if (user != null)
                    {
                        var roles = userManager.GetRolesAsync(user).Result;
                        foreach (var roleName in roles)
                        {
                            var role = roleManager.FindByNameAsync(roleName).Result;
                            if (role != null)
                            {
                                var claims = roleManager.GetClaimsAsync(role).Result;
                                var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

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
            }

            return userPermissions;
        }
    }

    public PAOExecutionContext(UserManager<PAOIdentityUser> userManager, RoleManager<PAOIdentityRole> roleManager, IHttpContextAccessor httpContextAccessor)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.httpContextAccessor = httpContextAccessor;
    }
}
