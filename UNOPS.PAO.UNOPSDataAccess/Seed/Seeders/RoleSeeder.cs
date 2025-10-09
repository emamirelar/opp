using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Seeds AspNetRoles and user-role assignments using UserManager and RoleManager
    /// </summary>
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(UNOPSAppDbContext context, IServiceProvider serviceProvider)
        {
            Console.WriteLine("🔄 Seeding Roles and User Assignments...");

            // Get UserManager and RoleManager from DI
            var roleManager = serviceProvider.GetRequiredService<RoleManager<PAOIdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<PAOIdentityUser>>();

            // Define roles to seed
            var rolesToSeed = new List<(string Name, string Description)>
            {
                ("UNOPS_GEN_USER", "General Opp+ User"),
                ("PARTNER_GLOB_ADMIN", "Partnership Global Admin"),
                ("PARTNER_USER", "Partnership User"),
                ("ORG_UNIT_ADMIN", "Org Unit Admin"),
                ("SYSTEM_ADMIN", "System Administrator")
            };

            var roleNamesToKeep = rolesToSeed.Select(r => r.Name).ToHashSet();

            // Insert or Update roles
            foreach (var (name, description) in rolesToSeed)
            {
                var existingRole = await roleManager.FindByNameAsync(name);

                if (existingRole == null)
                {
                    // Create new role
                    var role = new PAOIdentityRole
                    {
                        Name = name,
                        NormalizedName = name,
                        Description = description
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"  ✅ Inserted role: {name}");
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ Failed to create role {name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    // Update if description changed
                    if (existingRole.Description != description)
                    {
                        existingRole.Description = description;
                        var result = await roleManager.UpdateAsync(existingRole);
                        if (result.Succeeded)
                        {
                            Console.WriteLine($"  🔄 Updated role: {name}");
                        }
                        else
                        {
                            Console.WriteLine($"  ❌ Failed to update role {name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  ⏭️  Skipped role (unchanged): {name}");
                    }
                }
            }

            // Delete roles that are no longer in the seed list
            var allRoles = roleManager.Roles.ToList();
            var rolesToDelete = allRoles.Where(r => !roleNamesToKeep.Contains(r.Name ?? string.Empty)).ToList();

            foreach (var roleToDelete in rolesToDelete)
            {
                var result = await roleManager.DeleteAsync(roleToDelete);
                if (result.Succeeded)
                {
                    Console.WriteLine($"  🗑️  Deleted role: {roleToDelete.Name}");
                }
                else
                {
                    Console.WriteLine($"  ❌ Failed to delete role {roleToDelete.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Assign users to roles
            await AssignUsersToRolesAsync(userManager, roleManager);

            Console.WriteLine("✅ Roles and user assignments seeding completed\n");
        }

        private static async Task AssignUsersToRolesAsync(UserManager<PAOIdentityUser> userManager, RoleManager<PAOIdentityRole> roleManager)
        {
            Console.WriteLine("🔄 Assigning users to roles...");

            // Define user-role mappings
            var userRoleAssignments = new List<(string Email, string RoleName)>
            {
                // PARTNER_GLOB_ADMIN assignments
                ("michaelri@unops.org", "PARTNER_GLOB_ADMIN"),
                ("isabelaf@unops.org", "PARTNER_GLOB_ADMIN"),
                
                // SYSTEM_ADMIN assignments
                ("larsj@unops.org", "SYSTEM_ADMIN"),
                ("tushard@unops.org", "SYSTEM_ADMIN")
            };

            foreach (var (email, roleName) in userRoleAssignments)
            {
                try
                {
                    Console.WriteLine($"  📧 Processing: {email} -> {roleName}");
                    
                    // Find user by email
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        Console.WriteLine($"  ⚠️  User not found: {email}");
                        Console.WriteLine($"     Checking all users in database...");
                        var allUsers = userManager.Users.Select(u => u.Email).ToList();
                        Console.WriteLine($"     Found {allUsers.Count} users. First 10: {string.Join(", ", allUsers.Take(10))}");
                        continue;
                    }

                    Console.WriteLine($"     ✓ Found user: {user.Email} (ID: {user.Id})");

                    // Check if role exists
                    var roleExists = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        Console.WriteLine($"  ⚠️  Role not found: {roleName}");
                        var allRoles = roleManager.Roles.Select(r => r.Name).ToList();
                        Console.WriteLine($"     Available roles: {string.Join(", ", allRoles)}");
                        continue;
                    }

                    Console.WriteLine($"     ✓ Role exists: {roleName}");

                    // Check if user already has the role
                    var isInRole = await userManager.IsInRoleAsync(user, roleName);
                    Console.WriteLine($"     Current role status: IsInRole = {isInRole}");
                    
                    if (!isInRole)
                    {
                        Console.WriteLine($"     🔄 Attempting to assign role...");
                        // Assign role to user
                        var result = await userManager.AddToRoleAsync(user, roleName);
                        if (result.Succeeded)
                        {
                            Console.WriteLine($"  ✅ Successfully assigned {email} to role {roleName}");
                            
                            // Verify the assignment
                            var verifyInRole = await userManager.IsInRoleAsync(user, roleName);
                            Console.WriteLine($"     Verification: IsInRole = {verifyInRole}");
                        }
                        else
                        {
                            Console.WriteLine($"  ❌ Failed to assign {email} to role {roleName}");
                            foreach (var error in result.Errors)
                            {
                                Console.WriteLine($"     Error: {error.Code} - {error.Description}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  ⏭️  {email} already has role {roleName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ Exception while assigning {email} to {roleName}:");
                    Console.WriteLine($"     {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine($"     Stack: {ex.StackTrace}");
                }
            }
        }
    }
}

