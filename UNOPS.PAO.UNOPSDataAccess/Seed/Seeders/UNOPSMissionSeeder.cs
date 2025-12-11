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

        // Mark UNOPS Missions that are no longer in the seed list as Inactive (instead of deleting)
        var missionsToDeactivate = existingMissions
            .Where(m => !missionCodesToKeep.Contains(m.Code) && m.Status != EntityStatus.Inactive)
            .ToList();

        foreach (var missionToDeactivate in missionsToDeactivate)
        {
            missionToDeactivate.Status = EntityStatus.Inactive;
            Console.WriteLine($"  ⚠️  Marked UNOPS Mission as Inactive: {missionToDeactivate.Code} - {missionToDeactivate.Name}");
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
                Code = "TRIPLE_PLANETARY_CRISIS",
                Name = "Triple Planetary Crisis",
                Description = "Address the interconnected challenges of climate change, biodiversity loss, and environmental degradation",
                DisplayOrder = 1,
                IconClass = "pi pi-globe",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "ENERGY_TRANSITION",
                Name = "Energy Transition",
                Description = "Increase energy access and accelerate net-zero transition, promoting renewable energy and energy efficiency",
                DisplayOrder = 2,
                IconClass = "pi pi-bolt",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "SIDS_RESILIENCE_SUSTAINABILITY",
                Name = "SIDS Resilience and Sustainability",
                Description = "Support small island developing States in increasing resilience to environmental and economic shocks, and harnessing the benefits of a sustainable ocean economy",
                DisplayOrder = 3,
                IconClass = "pi pi-flag",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "QUALITY_HEALTHCARE",
                Name = "Quality Healthcare",
                Description = "Strengthen the availability of essential health-related supplies, equipment and facilities necessary to deliver quality healthcare and services",
                DisplayOrder = 4,
                IconClass = "pi pi-heart-fill",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "JUST_DIGITAL_TRANSFORMATION",
                Name = "Just Digital Transformation",
                Description = "Shape a just digital transformation, promoting developing countries' access and use of digital infrastructures, technologies and data",
                DisplayOrder = 5,
                IconClass = "pi pi-tablet",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "SOCIAL_PROTECTION_EQUALITY_EDUCATION_JOBS",
                Name = "Social Protection, Equality, Education and Jobs",
                Description = "Provide essential and sustainable services and infrastructure to communities and promote education and decent job creation to overcome inequalities and create prosperity",
                DisplayOrder = 6,
                IconClass = "pi pi-users",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "HUMANITARIAN_DEVELOPMENT_PEACE_NEXUS",
                Name = "Humanitarian, Development and Peace Nexus",
                Description = "Support the holistic efforts to address the root causes of fragility and strengthen the resilience of communities affected by conflict and disasters",
                DisplayOrder = 7,
                IconClass = "pi pi-heart",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new UNOPSMission
            {
                Code = "FOOD_SYSTEMS_TRANSFORMATION",
                Name = "Food Systems Transformation",
                Description = "Contribute to accelerating actions to support food security and healthy diets for all",
                DisplayOrder = 8,
                IconClass = "pi pi-sun",
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}

