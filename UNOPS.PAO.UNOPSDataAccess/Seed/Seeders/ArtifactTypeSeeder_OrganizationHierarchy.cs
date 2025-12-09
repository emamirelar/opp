using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds ArtifactTypes for OrganizationHierarchy entity
/// </summary>
public static class ArtifactTypeSeeder_OrganizationHierarchy
{
    public static async Task SeedOrganizationHierarchyArtifactTypesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding OrganizationHierarchy Artifact Types...");

        // Get all data type IDs
        var dataTypes = await context.Set<ArtifactDataType>().ToListAsync();
        var documentDataType = dataTypes.FirstOrDefault(dt => dt.Name == "document");

        if (documentDataType == null)
        {
            Console.WriteLine("  ❌ Error: Required ArtifactDataType 'document' not found. Please seed ArtifactDataTypes first.");
            return;
        }

        var documentDataTypeId = documentDataType.Id;

        var artifactTypesToSeed = GetOrganizationHierarchyArtifactTypesToSeed(documentDataTypeId);
        var existingArtifactTypes = await context.Set<ArtifactType>().ToListAsync();

        int insertedCount = 0;
        int updatedCount = 0;
        int skippedCount = 0;

        foreach (var artifactTypeData in artifactTypesToSeed)
        {
            var existingArtifactType = existingArtifactTypes
                .FirstOrDefault(at => at.ArtifactTypeCode == artifactTypeData.ArtifactTypeCode);

            if (existingArtifactType == null)
            {
                context.Set<ArtifactType>().Add(artifactTypeData);
                insertedCount++;
                Console.WriteLine($"  ✅ Inserted OrganizationHierarchy Artifact Type: {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
            }
            else
            {
                bool hasChanges = false;

                if (existingArtifactType.Name != artifactTypeData.Name)
                {
                    existingArtifactType.Name = artifactTypeData.Name;
                    hasChanges = true;
                }

                if (existingArtifactType.ArtifactDataTypeId != artifactTypeData.ArtifactDataTypeId)
                {
                    existingArtifactType.ArtifactDataTypeId = artifactTypeData.ArtifactDataTypeId;
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

                if (existingArtifactType.Source != artifactTypeData.Source)
                {
                    existingArtifactType.Source = artifactTypeData.Source;
                    hasChanges = true;
                }

                if (existingArtifactType.IsSearchable != artifactTypeData.IsSearchable)
                {
                    existingArtifactType.IsSearchable = artifactTypeData.IsSearchable;
                    hasChanges = true;
                }

                if (existingArtifactType.AllowBulkUpdate != artifactTypeData.AllowBulkUpdate)
                {
                    existingArtifactType.AllowBulkUpdate = artifactTypeData.AllowBulkUpdate;
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
                    updatedCount++;
                    Console.WriteLine($"  🔄 Updated OrganizationHierarchy Artifact Type: {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
                }
                else
                {
                    skippedCount++;
                    Console.WriteLine($"  ⏭️  Skipped OrganizationHierarchy Artifact Type (unchanged): {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
                }
            }
        }

        if (insertedCount > 0 || updatedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ OrganizationHierarchy Artifact Types seeding completed: {insertedCount} inserted, {updatedCount} updated, {skippedCount} skipped\n");
        }
        else
        {
            Console.WriteLine($"✅ OrganizationHierarchy Artifact Types seeding completed: No changes needed ({skippedCount} already up-to-date)\n");
        }
    }

    private static List<ArtifactType> GetOrganizationHierarchyArtifactTypesToSeed(int documentDataTypeId)
    {
        return new List<ArtifactType>
        {
            new ArtifactType
            {
                Name = "Strategy",
                ArtifactTypeCode = "Strategy",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Entity Strategy Document",
                Category = null,
                ApplicableEntityTypes = "OrganizationHierarchy",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 2000,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "High Risk Guidance",
                ArtifactTypeCode = "High_Risk_Guidance",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Entity High Risk Guidance Document",
                Category = null,
                ApplicableEntityTypes = "OrganizationHierarchy",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 2001,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new ArtifactType
            {
                Name = "Products & Services Guidance",
                ArtifactTypeCode = "Products_Services_Guidance",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Entity Products & Services Guidance Document",
                Category = null,
                ApplicableEntityTypes = "OrganizationHierarchy",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 2002,
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}