using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public class ProposedInitiativeTypeSeeder
{
    public static async Task SeedProposedInitiativeTypesAsync(UNOPSAppDbContext context)
    {
        // Check if any ProposedInitiativeTypes exist
        var existingTypes = await context.ProposedInitiativeTypes.AnyAsync();

        if (existingTypes)
        {
            Console.WriteLine("ProposedInitiativeTypes already exist. Skipping seed.");
            return;
        }

        var proposedInitiativeTypes = new List<ProposedInitiativeType>
        {
            new ProposedInitiativeType
            {
                Name = "Project",
                Description = "Single initiative with defined scope, timeline, and deliverables",
                Order = 1,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new ProposedInitiativeType
            {
                Name = "Programme",
                Description = "Collection of related projects managed in a coordinated way",
                Order = 2,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new ProposedInitiativeType
            {
                Name = "Portfolio",
                Description = "Collection of programmes and projects grouped together for strategic management",
                Order = 3,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            }
        };

        await context.ProposedInitiativeTypes.AddRangeAsync(proposedInitiativeTypes);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {proposedInitiativeTypes.Count} ProposedInitiativeTypes.");
    }
}

