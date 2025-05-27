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
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "Administrator",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "Administrator",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Document", 
                    Role = "Administrator",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Project", 
                    Role = "Administrator",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },

                // Internal role permissions
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "Internal",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "Internal",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Document", 
                    Role = "Internal",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Project", 
                    Role = "Internal",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false
                },

                // Partner role permissions
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "Partner",
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = true,
                    CanDelete = false,
                    RowFilter = "CreatedBy == CurrentUser"
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "Partner",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false,
                    RowFilter = "CreatedBy == CurrentUser"
                },
                new EntityPermission 
                { 
                    Entity = "Document", 
                    Role = "Partner",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false,
                    RowFilter = "CreatedBy == CurrentUser"
                },
                new EntityPermission 
                { 
                    Entity = "Project", 
                    Role = "Partner",
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    RowFilter = "CreatedBy == CurrentUser"
                },

                // External role permissions
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "External",
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    RowFilter = "IsPublic == true"
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "External",
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    RowFilter = "IsPublic == true"
                },
                new EntityPermission 
                { 
                    Entity = "Document", 
                    Role = "External",
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    RowFilter = "IsPublic == true"
                },
                new EntityPermission 
                { 
                    Entity = "Project", 
                    Role = "External",
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false,
                    RowFilter = "IsPublic == true"
                }
            };

            await context.EntityPermissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
} 