using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public static class UnitSeeder
{
    public static async Task SeedUnitsAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Units...");

        var unitsToSeed = GetUnitsToSeed();
        var existingUnits = await context.Set<Unit>().ToListAsync();
        var unitCodesToKeep = unitsToSeed.Select(u => u.Code).ToHashSet();

        foreach (var unitData in unitsToSeed)
        {
            var existingUnit = existingUnits.FirstOrDefault(u => u.Code == unitData.Code);

            if (existingUnit == null)
            {
                context.Set<Unit>().Add(unitData);
                Console.WriteLine($"  ✅ Inserted Unit: {unitData.Code} - {unitData.Name}");
            }
            else
            {
                bool hasChanges = false;

                if (existingUnit.Name != unitData.Name)
                {
                    existingUnit.Name = unitData.Name;
                    hasChanges = true;
                }

                if (existingUnit.Description != unitData.Description)
                {
                    existingUnit.Description = unitData.Description;
                    hasChanges = true;
                }

                if (existingUnit.Status != unitData.Status)
                {
                    existingUnit.Status = unitData.Status;
                    hasChanges = true;
                }

                if (existingUnit.IsDeleted)
                {
                    existingUnit.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated Unit: {unitData.Code} - {unitData.Name}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped Unit (unchanged): {unitData.Code} - {unitData.Name}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Units seeding completed\n");
    }

    private static List<Unit> GetUnitsToSeed()
    {
        return new List<Unit>
        {
            new Unit
            {
                Code = "DAYS",
                Name = "Days",
                Description = "Days",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new Unit
            {
                Code = "NO",
                Name = "Number",
                Description = "Number",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new Unit
            {
                Code = "LTR",
                Name = "Litres",
                Description = "Litres",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new Unit
            {
                Code = "Hectares",
                Name = "Hectares",
                Description = "Hectares",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new Unit
            {
                Code = "KG",
                Name = "Kilograms",
                Description = "Kilograms",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new Unit
            {
                Code = "KM",
                Name = "Kilometers",
                Description = "Kilometers",
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new Unit
            {
                Code = "KM2",
                Name = "Square Kilometers",
                Description = "Square Kilometers",
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}

