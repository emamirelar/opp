using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public class EntityRoleSeeder
{
    public static async Task SeedEntityRolesAsync(UNOPSAppDbContext context)
    {
        // Check if any EntityRoles exist for Opportunity entity
        var existingRoles = await context.EntityRoles
            .Where(er => er.EntityType == "Opportunity")
            .AnyAsync();

        if (existingRoles)
        {
            Console.WriteLine("EntityRoles for Opportunity already exist. Skipping seed.");
            return;
        }

        var entityRoles = new List<EntityRole>
        {
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Opportunity Manager",
                Description = "Primary manager responsible for the opportunity",
                IsInternal = true,
                AllowsMultiple = false,
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

        await context.EntityRoles.AddRangeAsync(entityRoles);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {entityRoles.Count} EntityRoles for Opportunity entity.");
    }
}

