using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public static class ArtifactDataTypeSeeder
{
    public static async Task SeedArtifactDataTypesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Artifact Data Types...");

        var dataTypesToSeed = GetArtifactDataTypesToSeed();
        var existingDataTypes = await context.Set<ArtifactDataType>().ToListAsync();

        foreach (var dataTypeData in dataTypesToSeed)
        {
            var existingDataType = existingDataTypes.FirstOrDefault(d => d.Name == dataTypeData.Name);

            if (existingDataType == null)
            {
                context.Set<ArtifactDataType>().Add(dataTypeData);
                Console.WriteLine($"  ✅ Inserted Artifact Data Type: {dataTypeData.Name}");
            }
            else
            {
                bool hasChanges = false;

                if (existingDataType.Description != dataTypeData.Description)
                {
                    existingDataType.Description = dataTypeData.Description;
                    hasChanges = true;
                }

                if (existingDataType.Order != dataTypeData.Order)
                {
                    existingDataType.Order = dataTypeData.Order;
                    hasChanges = true;
                }

                if (existingDataType.Status != dataTypeData.Status)
                {
                    existingDataType.Status = dataTypeData.Status;
                    hasChanges = true;
                }

                if (existingDataType.IsDeleted)
                {
                    existingDataType.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated Artifact Data Type: {dataTypeData.Name}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped Artifact Data Type (unchanged): {dataTypeData.Name}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Artifact Data Types seeding completed\n");
    }

    private static List<ArtifactDataType> GetArtifactDataTypesToSeed()
    {
        return new List<ArtifactDataType>
        {
            new ArtifactDataType
            {
                Name = "string",
                Description = "Text data type for storing string values",
                Order = 1,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactDataType
            {
                Name = "number",
                Description = "Numeric data type for storing decimal values",
                Order = 2,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactDataType
            {
                Name = "date",
                Description = "Date data type for storing date-only values",
                Order = 3,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactDataType
            {
                Name = "boolean",
                Description = "Boolean data type for storing boolean values",
                Order = 4,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactDataType
            {
                Name = "document",
                Description = "Document reference data type for linking to Document entities",
                Order = 5,
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}

