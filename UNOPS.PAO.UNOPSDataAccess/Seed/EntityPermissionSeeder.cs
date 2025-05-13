using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class EntityPermissionSeeder
    {
        public static async Task SeedEntityPermissionsAsync(UNOPSAppDbContext context)
        {
            // Check if permissions already exist
            if (await context.EntityPermissions.AnyAsync())
            {
                // Permissions already seeded, no need to run again
                return;
            }

            // Create list of entity permissions to add
            var permissions = new List<EntityPermission>
            {
                // Administrator role permissions (full access to all entities)
                new EntityPermission { EntityName = "Partner", Action = "Read", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Partner", Action = "Create", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Partner", Action = "Update", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Partner", Action = "Delete", RoleName = "Administrator" },

                new EntityPermission { EntityName = "Contact", Action = "Read", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Contact", Action = "Create", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Contact", Action = "Update", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Contact", Action = "Delete", RoleName = "Administrator" },

                new EntityPermission { EntityName = "Document", Action = "Read", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Document", Action = "Create", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Document", Action = "Update", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Document", Action = "Delete", RoleName = "Administrator" },

                new EntityPermission { EntityName = "Project", Action = "Read", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Project", Action = "Create", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Project", Action = "Update", RoleName = "Administrator" },
                new EntityPermission { EntityName = "Project", Action = "Delete", RoleName = "Administrator" },

                // Internal role permissions
                new EntityPermission { EntityName = "Partner", Action = "Read", RoleName = "Internal" },
                new EntityPermission { EntityName = "Partner", Action = "Create", RoleName = "Internal" },
                new EntityPermission { EntityName = "Partner", Action = "Update", RoleName = "Internal" },

                new EntityPermission { EntityName = "Contact", Action = "Read", RoleName = "Internal" },
                new EntityPermission { EntityName = "Contact", Action = "Create", RoleName = "Internal" },
                new EntityPermission { EntityName = "Contact", Action = "Update", RoleName = "Internal" },

                new EntityPermission { EntityName = "Document", Action = "Read", RoleName = "Internal" },
                new EntityPermission { EntityName = "Document", Action = "Create", RoleName = "Internal" },
                new EntityPermission { EntityName = "Document", Action = "Update", RoleName = "Internal" },

                new EntityPermission { EntityName = "Project", Action = "Read", RoleName = "Internal" },
                new EntityPermission { EntityName = "Project", Action = "Create", RoleName = "Internal" },
                new EntityPermission { EntityName = "Project", Action = "Update", RoleName = "Internal" },

                // Partner role permissions (can only read most things, manage their own content)
                new EntityPermission { EntityName = "Partner", Action = "Read", RoleName = "Partner", FilterExpression = "CreatedBy == CurrentUser" },
                new EntityPermission { EntityName = "Contact", Action = "Read", RoleName = "Partner", FilterExpression = "CreatedBy == CurrentUser" },
                new EntityPermission { EntityName = "Contact", Action = "Create", RoleName = "Partner" },
                new EntityPermission { EntityName = "Contact", Action = "Update", RoleName = "Partner", FilterExpression = "CreatedBy == CurrentUser" },

                new EntityPermission { EntityName = "Document", Action = "Read", RoleName = "Partner", FilterExpression = "CreatedBy == CurrentUser" },
                new EntityPermission { EntityName = "Document", Action = "Create", RoleName = "Partner" },
                new EntityPermission { EntityName = "Document", Action = "Update", RoleName = "Partner", FilterExpression = "CreatedBy == CurrentUser" },

                new EntityPermission { EntityName = "Project", Action = "Read", RoleName = "Partner", FilterExpression = "CreatedBy == CurrentUser" },

                // External role permissions (limited access)
                new EntityPermission { EntityName = "Partner", Action = "Read", RoleName = "External", FilterExpression = "IsPublic == true" },
                new EntityPermission { EntityName = "Contact", Action = "Read", RoleName = "External", FilterExpression = "IsPublic == true" },
                new EntityPermission { EntityName = "Document", Action = "Read", RoleName = "External", FilterExpression = "IsPublic == true" },
                new EntityPermission { EntityName = "Project", Action = "Read", RoleName = "External", FilterExpression = "IsPublic == true" },

                // User role permissions (basic role assigned to all authenticated users)
                new EntityPermission { EntityName = "Partner", Action = "Read", RoleName = "User", FilterExpression = "IsPublic == true" },
                new EntityPermission { EntityName = "Contact", Action = "Read", RoleName = "User", FilterExpression = "IsPublic == true" },
                new EntityPermission { EntityName = "Document", Action = "Read", RoleName = "User", FilterExpression = "IsPublic == true" },
                new EntityPermission { EntityName = "Project", Action = "Read", RoleName = "User", FilterExpression = "IsPublic == true" }
            };

            // Add all permissions
            await context.EntityPermissions.AddRangeAsync(permissions);

            // Save changes
            await context.SaveChangesAsync();
        }
    }
} 