using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public static class ArtifactTypeSeeder
{
    public static async Task SeedArtifactTypesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Artifact Types...");

        // Get all data type IDs
        var dataTypes = await context.Set<ArtifactDataType>().ToListAsync();
        var stringDataType = dataTypes.FirstOrDefault(dt => dt.Name == "string");
        var numberDataType = dataTypes.FirstOrDefault(dt => dt.Name == "number");
        var dateDataType = dataTypes.FirstOrDefault(dt => dt.Name == "date");
        var documentDataType = dataTypes.FirstOrDefault(dt => dt.Name == "document");

        if (stringDataType == null || numberDataType == null || dateDataType == null || documentDataType == null)
        {
            Console.WriteLine("  ❌ Error: Required ArtifactDataTypes not found. Please seed ArtifactDataTypes first.");
            Console.WriteLine($"     Found - string: {stringDataType != null}, number: {numberDataType != null}, date: {dateDataType != null}, document: {documentDataType != null}");
            return;
        }

        var artifactTypesToSeed = GetArtifactTypesToSeed(stringDataType.Id, numberDataType.Id, dateDataType.Id, documentDataType.Id);
        var existingArtifactTypes = await context.Set<ArtifactType>().ToListAsync();

        foreach (var artifactTypeData in artifactTypesToSeed)
        {
            var existingArtifactType = existingArtifactTypes
                .FirstOrDefault(at => at.ArtifactTypeCode == artifactTypeData.ArtifactTypeCode);

            if (existingArtifactType == null)
            {
                context.Set<ArtifactType>().Add(artifactTypeData);
                Console.WriteLine($"  ✅ Inserted Artifact Type: {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
            }
            else
            {
                bool hasChanges = false;

                if (existingArtifactType.Name != artifactTypeData.Name)
                {
                    existingArtifactType.Name = artifactTypeData.Name;
                    hasChanges = true;
                }

                if (existingArtifactType.Description != artifactTypeData.Description)
                {
                    existingArtifactType.Description = artifactTypeData.Description;
                    hasChanges = true;
                }

                if (existingArtifactType.Category != artifactTypeData.Category)
                {
                    existingArtifactType.Category = artifactTypeData.Category;
                    hasChanges = true;
                }

                if (existingArtifactType.ApplicableEntityTypes != artifactTypeData.ApplicableEntityTypes)
                {
                    existingArtifactType.ApplicableEntityTypes = artifactTypeData.ApplicableEntityTypes;
                    hasChanges = true;
                }

                if (existingArtifactType.IsUsedForCalculations != artifactTypeData.IsUsedForCalculations)
                {
                    existingArtifactType.IsUsedForCalculations = artifactTypeData.IsUsedForCalculations;
                    hasChanges = true;
                }

                if (existingArtifactType.IsUsedForAI != artifactTypeData.IsUsedForAI)
                {
                    existingArtifactType.IsUsedForAI = artifactTypeData.IsUsedForAI;
                    hasChanges = true;
                }

                if (existingArtifactType.Order != artifactTypeData.Order)
                {
                    existingArtifactType.Order = artifactTypeData.Order;
                    hasChanges = true;
                }

                if (existingArtifactType.Status != artifactTypeData.Status)
                {
                    existingArtifactType.Status = artifactTypeData.Status;
                    hasChanges = true;
                }

                if (existingArtifactType.IsDeleted)
                {
                    existingArtifactType.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated Artifact Type: {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped Artifact Type (unchanged): {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Artifact Types seeding completed\n");
    }

    private static List<ArtifactType> GetArtifactTypesToSeed(int stringDataTypeId, int numberDataTypeId, int dateDataTypeId, int documentDataTypeId)
    {
        return new List<ArtifactType>
        {
            new ArtifactType
            {
                Name = "Fragile Category",
                ArtifactTypeCode = "FragileCategory",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Fragile Category",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            // ========================================
            // TEST ARTIFACT TYPES FOR COUNTRY
            // ========================================
            
            new ArtifactType
            {
                Name = "Test Country String",
                ArtifactTypeCode = "TestCountryString",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Test artifact for Country entity with string data type",
                Category = "Test",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 100,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "Test Country Number",
                ArtifactTypeCode = "TestCountryNumber",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Test artifact for Country entity with number data type",
                Category = "Test",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 101,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "Test Country Date",
                ArtifactTypeCode = "TestCountryDate",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "Test artifact for Country entity with date data type",
                Category = "Test",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 102,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "Test Country Document",
                ArtifactTypeCode = "TestCountryDocument",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Test artifact for Country entity with document data type",
                Category = "Test",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 103,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            // ========================================
            // TEST ARTIFACT TYPES FOR ORGANIZATIONHIERARCHY
            // ========================================
            
            new ArtifactType
            {
                Name = "Test Organization Hierarchy String",
                ArtifactTypeCode = "TestOrganizationHierarchyString",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Test artifact for OrganizationHierarchy entity with string data type",
                Category = "Test",
                ApplicableEntityTypes = "OrganizationHierarchy",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 200,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "Test Organization Hierarchy Number",
                ArtifactTypeCode = "TestOrganizationHierarchyNumber",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Test artifact for OrganizationHierarchy entity with number data type",
                Category = "Test",
                ApplicableEntityTypes = "OrganizationHierarchy",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 201,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "Test Organization Hierarchy Date",
                ArtifactTypeCode = "TestOrganizationHierarchyDate",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "Test artifact for OrganizationHierarchy entity with date data type",
                Category = "Test",
                ApplicableEntityTypes = "OrganizationHierarchy",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 202,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "Test Organization Hierarchy Document",
                ArtifactTypeCode = "TestOrganizationHierarchyDocument",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Test artifact for OrganizationHierarchy entity with document data type",
                Category = "Test",
                ApplicableEntityTypes = "OrganizationHierarchy",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 203,
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}

