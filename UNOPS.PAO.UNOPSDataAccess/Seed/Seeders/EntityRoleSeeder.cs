using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public class EntityRoleSeeder
{
    public static async Task SeedEntityRolesAsync(UNOPSAppDbContext context)
    {
        await SeedOpportunityRolesAsync(context);
        await SeedOrganizationHierarchyRolesAsync(context);
    }

    /// <summary>
    /// Adds a role only if it doesn't already exist (by EntityType and Name)
    /// </summary>
    private static async Task<bool> AddRoleIfNotExistsAsync(UNOPSAppDbContext context, EntityRole role)
    {
        var exists = await context.EntityRoles
            .AnyAsync(er => er.EntityType == role.EntityType && er.Name == role.Name);

        if (exists)
        {
            return false;
        }

        await context.EntityRoles.AddAsync(role);
        return true;
    }

    private static async Task SeedOpportunityRolesAsync(UNOPSAppDbContext context)
    {
        var rolesToSeed = new List<EntityRole>
        {
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Opportunity Manager",
                Description = "Primary manager responsible for overall opportunity strategy, stakeholder engagement, and successful delivery",
                IsInternal = true,
                AllowsMultiple = false,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Partnership Lead",
                Description = "Lead responsible for partnership development, relationship management, and collaboration with partners",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Reviewer",
                Description = "Reviewer responsible for quality assurance, compliance checks, and approval workflows",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Internal Stakeholder",
                Description = "Internal UNOPS stakeholder involved in the opportunity",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "External Stakeholder",
                Description = "External stakeholder or partner contact involved in the opportunity",
                IsInternal = false,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            }
        };

        var addedCount = 0;
        foreach (var role in rolesToSeed)
        {
            if (await AddRoleIfNotExistsAsync(context, role))
            {
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {addedCount} new EntityRoles for Opportunity entity.");
        }
        else
        {
            Console.WriteLine("All EntityRoles for Opportunity already exist. Skipping seed.");
        }
    }

    private static async Task SeedOrganizationHierarchyRolesAsync(UNOPSAppDbContext context)
    {
        var rolesToSeed = new List<EntityRole>
        {
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Region Director",
                Description = "Director responsible for overseeing the entire region",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Region Deputy Director",
                Description = "Deputy Director supporting the Region Director",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Hub Director",
                Description = "Director responsible for overseeing a hub",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Hub Deputy Director",
                Description = "Deputy Director supporting the Hub Director",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "OrgUnit Director",
                Description = "Director responsible for overseeing an organizational unit",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "OrgUnit Deputy Director",
                Description = "Deputy Director supporting the OrgUnit Director",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA1",
                Description = "Delegation of Authority Level 1",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA2",
                Description = "Delegation of Authority Level 2",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA3",
                Description = "Delegation of Authority Level 3",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA4",
                Description = "Delegation of Authority Level 4",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            }
        };

        var addedCount = 0;
        foreach (var role in rolesToSeed)
        {
            if (await AddRoleIfNotExistsAsync(context, role))
            {
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {addedCount} new EntityRoles for OrganizationHierarchy entity.");
        }
        else
        {
            Console.WriteLine("All EntityRoles for OrganizationHierarchy already exist. Skipping seed.");
        }
    }
}

