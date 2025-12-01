using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds UNOPS Strategic Missions with proper insert/update logic
/// </summary>
public static class UNOPSMissionSeeder
{
    public static async Task SeedUNOPSMissionsAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding UNOPS Missions...");

        var missionsToSeed = GetUNOPSMissionsToSeed();

        // Get existing UNOPS Missions from database
        var existingMissions = await context.Set<UNOPSMission>().ToListAsync();

        var missionCodesToKeep = missionsToSeed.Select(m => m.Code).ToHashSet();

        // Insert or Update UNOPS Missions
        foreach (var missionData in missionsToSeed)
        {
            var existingMission = existingMissions.FirstOrDefault(m => m.Code == missionData.Code);

            if (existingMission == null)
            {
                // Insert new UNOPS Mission
                context.Set<UNOPSMission>().Add(missionData);
                Console.WriteLine($"  ✅ Inserted UNOPS Mission: {missionData.Code} - {missionData.Name}");
            }
            else
            {
                // Update if any properties changed
                bool hasChanges = false;

                if (existingMission.Name != missionData.Name)
                {
                    existingMission.Name = missionData.Name;
                    hasChanges = true;
                }

                if (existingMission.Description != missionData.Description)
                {
                    existingMission.Description = missionData.Description;
                    hasChanges = true;
                }

                if (existingMission.DisplayOrder != missionData.DisplayOrder)
                {
                    existingMission.DisplayOrder = missionData.DisplayOrder;
                    hasChanges = true;
                }

                if (existingMission.IconClass != missionData.IconClass)
                {
                    existingMission.IconClass = missionData.IconClass;
                    hasChanges = true;
                }

                if (existingMission.Status != missionData.Status)
                {
                    existingMission.Status = missionData.Status;
                    hasChanges = true;
                }

                if (existingMission.IsDeleted)
                {
                    existingMission.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated UNOPS Mission: {missionData.Code} - {missionData.Name}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped UNOPS Mission (unchanged): {missionData.Code} - {missionData.Name}");
                }
            }
        }

        // Delete UNOPS Missions that are no longer in the seed list
        var missionsToDelete = existingMissions
            .Where(m => !missionCodesToKeep.Contains(m.Code))
            .ToList();

        foreach (var missionToDelete in missionsToDelete)
        {
            context.Set<UNOPSMission>().Remove(missionToDelete);
            Console.WriteLine($"  🗑️  Deleted UNOPS Mission: {missionToDelete.Code} - {missionToDelete.Name}");
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ UNOPS Missions seeding completed\n");
    }

    private static List<UNOPSMission> GetUNOPSMissionsToSeed()
    {
        return new List<UNOPSMission>
        {
            new UNOPSMission
            {
                Code = "CLIMATE_BIODIVERSITY",
                Name = "Climate, Biodiversity, and Pollution",
                Description = "Address the interconnected challenges of climate change, biodiversity loss, and pollution",
                DisplayOrder = 1,
                IconClass = "pi pi-globe",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "ENERGY_TRANSITION",
                Name = "Energy Access and Transition",
                Description = "Increase energy access and accelerate the transition away from fossil fuels, promoting renewable energy and energy efficiency",
                DisplayOrder = 2,
                IconClass = "pi pi-bolt",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "DIGITAL_TRANSFORMATION",
                Name = "Just Digital Transformation",
                Description = "Advance just digital transformation, promoting developing countries' access to and use of digital infrastructure, technology and data",
                DisplayOrder = 3,
                IconClass = "pi pi-tablet",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "SUSTAINABLE_FOOD",
                Name = "Sustainable Food Systems",
                Description = "Support the transition to sustainable food systems",
                DisplayOrder = 4,
                IconClass = "pi pi-leaf",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "SIDS_RESILIENCE",
                Name = "SIDS Resilience and Ocean Economy",
                Description = "Support small island developing States in increasing resilience to environmental and economic shocks and harness the benefits of a sustainable ocean economy",
                DisplayOrder = 5,
                IconClass = "pi pi-flag",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "FRAGILITY_EQUITY",
                Name = "Fragility, Equity, and Community Resilience",
                Description = "Address root causes of fragility, advance equity, and strengthen the resilience of communities affected by conflict and disaster",
                DisplayOrder = 6,
                IconClass = "pi pi-heart",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "HEALTH_CARE",
                Name = "Quality Health Care",
                Description = "Enhance the availability of essential supplies, equipment and facilities for quality health care and services",
                DisplayOrder = 7,
                IconClass = "pi pi-heart-fill",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "SOCIAL_DEVELOPMENT",
                Name = "Inclusive Social Development",
                Description = "Provide essential and sustainable goods, services and infrastructure, rooted in the social development values of the United Nations Charter and the 2030 Agenda, to reduce inequalities and achieve inclusive social development",
                DisplayOrder = 8,
                IconClass = "pi pi-users",
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}

