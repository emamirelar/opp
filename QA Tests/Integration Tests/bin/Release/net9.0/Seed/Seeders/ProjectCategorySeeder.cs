using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public static class ProjectCategorySeeder
{
    public static async Task SeedProjectCategoriesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Project Categories...");

        var categoriesToSeed = GetProjectCategoriesToSeed();
        var existingCategories = await context.Set<ProjectCategory>().ToListAsync();
        var categoryCodesToKeep = categoriesToSeed.Select(c => c.Code).ToHashSet();

        foreach (var categoryData in categoriesToSeed)
        {
            var existingCategory = existingCategories.FirstOrDefault(c => c.Code == categoryData.Code);

            if (existingCategory == null)
            {
                context.Set<ProjectCategory>().Add(categoryData);
                Console.WriteLine($"  ✅ Inserted Project Category: {categoryData.Code} - {categoryData.Name}");
            }
            else
            {
                bool hasChanges = false;

                if (existingCategory.Name != categoryData.Name)
                {
                    existingCategory.Name = categoryData.Name;
                    hasChanges = true;
                }

                if (existingCategory.Description != categoryData.Description)
                {
                    existingCategory.Description = categoryData.Description;
                    hasChanges = true;
                }

                if (existingCategory.Status != categoryData.Status)
                {
                    existingCategory.Status = categoryData.Status;
                    hasChanges = true;
                }

                if (existingCategory.IsDeleted)
                {
                    existingCategory.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated Project Category: {categoryData.Code} - {categoryData.Name}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped Project Category (unchanged): {categoryData.Code} - {categoryData.Name}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Project Categories seeding completed\n");
    }

    private static List<ProjectCategory> GetProjectCategoriesToSeed()
    {
        return new List<ProjectCategory>
        {
            new ProjectCategory
            {
                Code = "Category 1",
                Name = "Category 1",
                Description = "Category 1",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ProjectCategory
            {
                Code = "Category 2",
                Name = "Category 2",
                Description = "Category 2",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ProjectCategory
            {
                Code = "Category 3",
                Name = "Category 3",
                Description = "Category 3",
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}

