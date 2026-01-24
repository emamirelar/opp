using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds SDG (Sustainable Development Goals) with proper insert/update logic
/// </summary>
public static class SDGSeeder
{
    public static async Task SeedSDGsAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding SDGs...");

        var sdgsToSeed = GetSDGsToSeed();

        // Get existing SDGs from database
        var existingSDGs = await context.SDGs.ToListAsync();

        var sdgIdsToKeep = sdgsToSeed.Select(s => s.SDGId).ToHashSet();

        // Insert or Update SDGs
        foreach (var sdgData in sdgsToSeed)
        {
            var existingSDG = existingSDGs.FirstOrDefault(s => s.SDGId == sdgData.SDGId);

            if (existingSDG == null)
            {
                // Insert new SDG
                context.SDGs.Add(sdgData);
                Console.WriteLine($"  ✅ Inserted SDG: {sdgData.SDGId} - {sdgData.Name}");
            }
            else
            {
                // Update if any properties changed
                bool hasChanges = false;

                if (existingSDG.Name != sdgData.Name)
                {
                    existingSDG.Name = sdgData.Name;
                    hasChanges = true;
                }

                if (existingSDG.SDGNumber != sdgData.SDGNumber)
                {
                    existingSDG.SDGNumber = sdgData.SDGNumber;
                    hasChanges = true;
                }

                if (existingSDG.SDGDescription != sdgData.SDGDescription)
                {
                    existingSDG.SDGDescription = sdgData.SDGDescription;
                    hasChanges = true;
                }

                if (existingSDG.SDGLogo != sdgData.SDGLogo)
                {
                    existingSDG.SDGLogo = sdgData.SDGLogo;
                    hasChanges = true;
                }

                if (existingSDG.SDGLongDescription != sdgData.SDGLongDescription)
                {
                    existingSDG.SDGLongDescription = sdgData.SDGLongDescription;
                    hasChanges = true;
                }

                if (existingSDG.Status != sdgData.Status)
                {
                    existingSDG.Status = sdgData.Status;
                    hasChanges = true;
                }

                if (existingSDG.IsDeleted)
                {
                    existingSDG.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated SDG: {sdgData.SDGId} - {sdgData.Name}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped SDG (unchanged): {sdgData.SDGId} - {sdgData.Name}");
                }
            }
        }

        // Delete SDGs that are no longer in the seed list
        var sdgsToDelete = existingSDGs
            .Where(s => !sdgIdsToKeep.Contains(s.SDGId))
            .ToList();

        foreach (var sdgToDelete in sdgsToDelete)
        {
            context.SDGs.Remove(sdgToDelete);
            Console.WriteLine($"  🗑️  Deleted SDG: {sdgToDelete.SDGId} - {sdgToDelete.Name}");
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ SDGs seeding completed\n");
    }

    private static List<SDG> GetSDGsToSeed()
    {
        return new List<SDG>
        {
            new SDG
            {
                Name = "GOAL 1: No Poverty",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-01",
                SDGNumber = "Goal 1",
                SDGDescription = "GOAL 1: No Poverty",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-01.png",
                SDGLongDescription = "End poverty in all its forms everywhere"
            },
            new SDG
            {
                Name = "GOAL 2: Zero Hunger",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-02",
                SDGNumber = "Goal 2",
                SDGDescription = "GOAL 2: Zero Hunger",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-02.png",
                SDGLongDescription = "End hunger, achieve food security and improved nutrition and promote sustainable agriculture"
            },
            new SDG
            {
                Name = "GOAL 3: Good Health and Well-being",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-03",
                SDGNumber = "Goal 3",
                SDGDescription = "GOAL 3: Good Health and Well-being",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-03.png",
                SDGLongDescription = "Ensure healthy lives and promote well-being for all at all ages"
            },
            new SDG
            {
                Name = "GOAL 4: Quality Education",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-04",
                SDGNumber = "Goal 4",
                SDGDescription = "GOAL 4: Quality Education",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-04.png",
                SDGLongDescription = "Ensure inclusive and equitable quality education and promote lifelong learning opportunities for all"
            },
            new SDG
            {
                Name = "GOAL 5: Gender Equality",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-05",
                SDGNumber = "Goal 5",
                SDGDescription = "GOAL 5: Gender Equality",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-05.png",
                SDGLongDescription = "Achieve gender equality and empower all women and girls"
            },
            new SDG
            {
                Name = "GOAL 6: Clean Water and Sanitation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-06",
                SDGNumber = "Goal 6",
                SDGDescription = "GOAL 6: Clean Water and Sanitation",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-06.png",
                SDGLongDescription = "Ensure availability and sustainable management of water and sanitation for all"
            },
            new SDG
            {
                Name = "GOAL 7: Affordable and Clean Energy",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-07",
                SDGNumber = "Goal 7",
                SDGDescription = "GOAL 7: Affordable and Clean Energy",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-07.png",
                SDGLongDescription = "Ensure access to affordable, reliable, sustainable and modern energy for all"
            },
            new SDG
            {
                Name = "Goal 8: Decent Work and Economic Growth",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-08",
                SDGNumber = "Goal 8",
                SDGDescription = "GOAL 8: Decent Work and Economic Growth",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-08.png",
                SDGLongDescription = "Promote sustained, inclusive and sustainable economic growth, full and productive employment and decent work for all"
            },
            new SDG
            {
                Name = "GOAL 9: Industry, Innovation and Infrastructure",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-09",
                SDGNumber = "Goal 9",
                SDGDescription = "GOAL 9: Industry, Innovation and Infrastructure",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-09.png",
                SDGLongDescription = "Build resilient infrastructure, promote inclusive and sustainable industrialization and foster innovation"
            },
            new SDG
            {
                Name = "GOAL 10: Reduced Inequality",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-10",
                SDGNumber = "Goal 10",
                SDGDescription = "GOAL 10: Reduced Inequality",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-10.png",
                SDGLongDescription = "Reduce inequality within and among countries"
            },
            new SDG
            {
                Name = "GOAL 11: Sustainable Cities and Communities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-11",
                SDGNumber = "Goal 11",
                SDGDescription = "GOAL 11: Sustainable Cities and Communities",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-11.png",
                SDGLongDescription = "Make cities and human settlements inclusive, safe, resilient and sustainable"
            },
            new SDG
            {
                Name = "GOAL 12: Responsible Consumption and Production",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-12",
                SDGNumber = "Goal 12",
                SDGDescription = "GOAL 12: Responsible Consumption and Production",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-12.png",
                SDGLongDescription = "Ensure sustainable consumption and production patterns"
            },
            new SDG
            {
                Name = "GOAL 13: Climate Action",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-13",
                SDGNumber = "Goal 13",
                SDGDescription = "GOAL 13: Climate Action",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-13.png",
                SDGLongDescription = "Take urgent action to combat climate change and its impacts"
            },
            new SDG
            {
                Name = "GOAL 14: Life Below Water",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-14",
                SDGNumber = "Goal 14",
                SDGDescription = "GOAL 14: Life Below Water",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-14.png",
                SDGLongDescription = "Conserve and sustainably use the oceans, seas and marine resources for sustainable development"
            },
            new SDG
            {
                Name = "GOAL 15: Life on Land",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-15",
                SDGNumber = "Goal 15",
                SDGDescription = "GOAL 15: Life on Land",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-15.png",
                SDGLongDescription = "Protect, restore and promote sustainable use of terrestrial ecosystems, sustainably manage forests, combat desertification, and halt and reverse land degradation and halt biodiversity loss"
            },
            new SDG
            {
                Name = "GOAL 16: Peace, Justice and Strong Institutions",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-16",
                SDGNumber = "Goal 16",
                SDGDescription = "GOAL 16: Peace, Justice and Strong Institutions",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-16.png",
                SDGLongDescription = "Promote peaceful and inclusive societies for sustainable development, provide access to justice for all and build effective, accountable and inclusive institutions at all levels"
            },
            new SDG
            {
                Name = "GOAL 17: Partnerships for the Goals",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "SDG-17",
                SDGNumber = "Goal 17",
                SDGDescription = "GOAL 17: Partnerships for the Goals",
                SDGLogo = "https://storage.googleapis.com/unops_sdg/SDG-17.png",
                SDGLongDescription = "Strengthen the means of implementation and revitalize the Global Partnership for Sustainable Development Finance"
            },
            new SDG
            {
                Name = "No contribution to the SDGs",
                Status = EntityStatus.Active,
                IsDeleted = false,
                SDGId = "N/A",
                SDGNumber = "N/A",
                SDGDescription = "No contribution to the SDGs ",
                SDGLogo = "SDG_Images/N-A.SDG_Logo.141231.png",
                SDGLongDescription = "No contribution to the SDGs"
            }
        };
    }
}

