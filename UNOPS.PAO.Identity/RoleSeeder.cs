namespace UNOPS.PAO.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Identity.Security;

public static class RoleSeeder
{
    public static async void SeedAsync(this IServiceCollection services)
    {
        var roles = BaseRole.GetAllRoles();
        if (roles.Any())
        {
            var roleManager = services.BuildServiceProvider()
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<RoleManager<PAOIdentityRole>>();

            foreach (var (name, description, permissions) in roles)
            {
                var existingRole = await roleManager.FindByNameAsync(name);

                if (existingRole == null)
                {
                    var role = new PAOIdentityRole
                    {
                        Name = name,
                        NormalizedName = name,
                        Description = description
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        await AddPermissionsAsClaims(roleManager, role, permissions);
                    }
                }
                else
                {
                    if (existingRole.Description != description)
                    {
                        existingRole.Description = description;
                        await roleManager.UpdateAsync(existingRole);
                    }

                    await AddPermissionsAsClaims(roleManager, existingRole, permissions);
                }
            }

            await RemoveUnusedRolesAsync(roleManager, roles.Select(r => r.Name).ToList());
        }
    }

    private static async Task AddPermissionsAsClaims(RoleManager<PAOIdentityRole> roleManager, PAOIdentityRole role, IEnumerable<Permission> permissions)
    {
        var currentClaims = await roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

        var newPermissions = permissions.Select(p => p.Name).Except(currentPermissions).ToList();
        var removedPermissions = currentPermissions.Except(permissions.Select(p => p.Name)).ToList();

        foreach (var permission in newPermissions)
        {
            await roleManager.AddClaimAsync(role, new Claim("permission", permission));
        }

        foreach (var permission in removedPermissions)
        {
            var claim = currentClaims.FirstOrDefault(c => c.Type == "permission" && c.Value == permission);
            if (claim != null)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }
        }
    }

    private static async Task RemoveUnusedRolesAsync(RoleManager<PAOIdentityRole> roleManager, List<string> roles)
    {
        foreach (var role in roleManager.Roles.ToList())
        {
            var roleStillExists = roles.Any(a => a == role.Name);
            if (!roleStillExists)
                await roleManager.DeleteAsync(role);
        }
    }
}
