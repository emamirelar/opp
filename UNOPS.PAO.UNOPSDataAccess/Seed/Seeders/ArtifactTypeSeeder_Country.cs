using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds ArtifactTypes for Country entity
/// Generated from Country_Artifact_Type_Seeder - Sheet1.csv
/// </summary>
public static class ArtifactTypeSeeder_Country
{
    public static async Task SeedCountryArtifactTypesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Country Artifact Types...");

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

        var stringDataTypeId = stringDataType.Id;
        var numberDataTypeId = numberDataType.Id;
        var dateDataTypeId = dateDataType.Id;
        var documentDataTypeId = documentDataType.Id;

        var artifactTypesToSeed = GetCountryArtifactTypesToSeed(stringDataTypeId, numberDataTypeId, dateDataTypeId, documentDataTypeId);
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
                Console.WriteLine($"  ✅ Inserted Country Artifact Type: {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
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
                    updatedCount++;
                    Console.WriteLine($"  🔄 Updated Country Artifact Type: {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
                }
                else
                {
                    skippedCount++;
                    Console.WriteLine($"  ⏭️  Skipped Country Artifact Type (unchanged): {artifactTypeData.ArtifactTypeCode} - {artifactTypeData.Name}");
                }
            }
        }

        if (insertedCount > 0 || updatedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Country Artifact Types seeding completed: {insertedCount} inserted, {updatedCount} updated, {skippedCount} skipped\n");
        }
        else
        {
            Console.WriteLine($"✅ Country Artifact Types seeding completed: No changes needed ({skippedCount} already up-to-date)\n");
        }
    }

    private static List<ArtifactType> GetCountryArtifactTypesToSeed(int stringDataTypeId, int numberDataTypeId, int dateDataTypeId, int documentDataTypeId)
    {
        return new List<ArtifactType>
        {
            new ArtifactType
            {
                Name = "UN Region",
                ArtifactTypeCode = "UN_Region",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "UN-defined regional classification based on the M49 standard",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1000,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UN Sub Region",
                ArtifactTypeCode = "UN_Sub_Region",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "UN-defined regional classification based on the M49 standard",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1001,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UNOPS Region",
                ArtifactTypeCode = "UNOPS_Region",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Global business units and entities within UNOPS",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1002,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LDC",
                ArtifactTypeCode = "LDC",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "LDC",
                Category = "General",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1003,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LDC Source",
                ArtifactTypeCode = "LDC_Source",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "LDC Source",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1004,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LDC Updated Date",
                ArtifactTypeCode = "LDC_Updated_Date",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "LDC Updated Date",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1005,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LLDC",
                ArtifactTypeCode = "LLDC",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "LLDC",
                Category = "General",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1006,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LLDC Source",
                ArtifactTypeCode = "LLDC_Source",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "LLDC Source",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1007,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LLDC Updated Date",
                ArtifactTypeCode = "LLDC_Updated_Date",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "LLDC Updated Date",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1008,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SIDS",
                ArtifactTypeCode = "SIDS",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "SIDS",
                Category = "General",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1009,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SIDS Source",
                ArtifactTypeCode = "SIDS_Source",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "SIDS Source",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1010,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SIDS Updated Date",
                ArtifactTypeCode = "SIDS_Updated_Date",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "SIDS Updated Date",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1011,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "MVI Score",
                ArtifactTypeCode = "MVI_Score",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Structural vulnerability and lack of resilience of countries to external shocks across three dimensions: Environmental vulnerability, Economic vulnerability, Social vulnerability",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1012,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Structural Vulnerability Index",
                ArtifactTypeCode = "Structural_Vulnerability_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Structural Vulnerability Index",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1013,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Lack of Structural Resilience Index",
                ArtifactTypeCode = "Lack_of_Structural_Resilience_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Lack of Structural Resilience Index",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1014,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "MVI Effective Date",
                ArtifactTypeCode = "MVI_Effective_Date",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "MVI Effective Date",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1015,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "World Bank Fragile Situation",
                ArtifactTypeCode = "World_Bank_Fragile_Situation",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Countries and territories identified as experiencing conflict and institutional and social fragility",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1016,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UN Programme Country",
                ArtifactTypeCode = "UN_Programme_Country",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "UN Programme Country",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1017,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "OECD Member",
                ArtifactTypeCode = "OECD_Member",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "OECD Member",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1018,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "DAC Member",
                ArtifactTypeCode = "DAC_Member",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "DAC Member",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1019,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UNOPS Country Typology",
                ArtifactTypeCode = "UNOPS_Country_Typology",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "UNOPS Country Typology",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1020,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "OECD List High Extreme Fragility",
                ArtifactTypeCode = "OECD_List_High_Extreme_Fragility",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Evidence-based assessment of fragility trends across countries, to understand where, how, and why fragility is evolving",
                Category = "General",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1021,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "States of Fragility OECD",
                ArtifactTypeCode = "States_of_Fragility_OECD",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Evidence-based assessment of fragility trends across countries, to understand where, how, and why fragility is evolving",
                Category = "General",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1022,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Fragility Score OECD",
                ArtifactTypeCode = "Fragility_Score_OECD",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Fragility Score OECD",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1023,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SDG Index",
                ArtifactTypeCode = "SDG_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Progress towards each of the 17 SDGs, per country",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1024,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SDG Index Rank",
                ArtifactTypeCode = "SDG_Index_Rank",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "SDG Index Rank",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1025,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SDGI Year",
                ArtifactTypeCode = "SDGI_Year",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "SDGI Year",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1026,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "HDI Index",
                ArtifactTypeCode = "HDI_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Achievements in human development",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1027,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "HDI Fiscal Year",
                ArtifactTypeCode = "HDI_Fiscal_Year",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "HDI Fiscal Year",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1028,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "HDI Group",
                ArtifactTypeCode = "HDI_Group",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "HDI Group",
                Category = "General",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1029,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "HDI Download Date",
                ArtifactTypeCode = "HDI_Download_Date",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "HDI Download Date",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1030,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Risk Index",
                ArtifactTypeCode = "Inform_Risk_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Risk from humanitarian crisis and disasters that could overwhelm national response capacity",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1031,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Risk Class",
                ArtifactTypeCode = "Inform_Risk_Class",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Inform Risk Class",
                Category = "General",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1032,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Rank",
                ArtifactTypeCode = "Inform_Rank",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Inform Rank",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1033,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Version",
                ArtifactTypeCode = "Inform_Version",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Inform Version",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1034,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Download Date",
                ArtifactTypeCode = "Inform_Download_Date",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "Inform Download Date",
                Category = "Metadata",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1035,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "GNI Per Capita USD 2024",
                ArtifactTypeCode = "GNI_Per_Capita_USD_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "GNI Per Capita USD 2024",
                Category = "Demographics",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = false,
                Order = 1036,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "GNI Download At",
                ArtifactTypeCode = "GNI_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "GNI Download At",
                Category = "Demographics",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1037,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Population 2024",
                ArtifactTypeCode = "Population_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Population 2024",
                Category = "Demographics",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = false,
                Order = 1038,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Population Download At",
                ArtifactTypeCode = "Population_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "Population Download At",
                Category = "Demographics",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1039,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Urban Population 2024",
                ArtifactTypeCode = "Urban_Population_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Urban Population 2024",
                Category = "Demographics",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = false,
                Order = 1040,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Urban Population Download At",
                ArtifactTypeCode = "Urban_Population_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "Urban Population Download At",
                Category = "Demographics",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1041,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Gini Index 2024",
                ArtifactTypeCode = "Gini_Index_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Gini Index 2024",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1042,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Gini Index Download At",
                ArtifactTypeCode = "Gini_Index_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = "Gini Index Download At",
                Category = "Assessment",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1043,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Special Situation Countries",
                ArtifactTypeCode = "Special_Situation_Countries",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "QCPR (combines LDC, LLDC, SIDS). Report focuses on tracking demographic trends in LDCs, LLDCs, and SIDS",
                Category = "Classification",
                ApplicableEntityTypes = "Country",
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1044,
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}
