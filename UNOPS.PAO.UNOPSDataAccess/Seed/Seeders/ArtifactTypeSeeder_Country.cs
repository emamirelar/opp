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
        var booleanDataType = dataTypes.FirstOrDefault(dt => dt.Name == "boolean");
        var documentDataType = dataTypes.FirstOrDefault(dt => dt.Name == "document");

        if (stringDataType == null || numberDataType == null || dateDataType == null || booleanDataType == null || documentDataType == null)
        {
            Console.WriteLine("  ❌ Error: Required ArtifactDataTypes not found. Please seed ArtifactDataTypes first.");
            Console.WriteLine($"     Found - string: {stringDataType != null}, number: {numberDataType != null}, date: {dateDataType != null}, boolean: {booleanDataType != null}, document: {documentDataType != null}");
            return;
        }

        var stringDataTypeId = stringDataType.Id;
        var numberDataTypeId = numberDataType.Id;
        var dateDataTypeId = dateDataType.Id;
        var booleanDataTypeId = booleanDataType.Id;
        var documentDataTypeId = documentDataType.Id;

        var artifactTypesToSeed = GetCountryArtifactTypesToSeed(stringDataTypeId, numberDataTypeId, dateDataTypeId, booleanDataTypeId, documentDataTypeId);
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

    private static List<ArtifactType> GetCountryArtifactTypesToSeed(int stringDataTypeId, int numberDataTypeId, int dateDataTypeId, int booleanDataTypeId, int documentDataTypeId)
    {
        return new List<ArtifactType>
        {
            new ArtifactType
            {
                Name = "Country Code",
                ArtifactTypeCode = "Country_Code",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Standard Country or Area Codes for Statistical Use\" (published by the United Nations Statistics Division)",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "UNSD",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1000,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UN Region",
                ArtifactTypeCode = "UN_Region",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "UN-defined regional classification based on the M49 standard",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "UNSD",
                IsSearchable = true,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1001,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UN Sub Region",
                ArtifactTypeCode = "UN_Sub_Region",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "UN-defined regional classification based on the M49 standard",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "UNSD",
                IsSearchable = true,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1002,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UNOPS Region",
                ArtifactTypeCode = "UNOPS_Region",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Global business units and entities within UNOPS",
                Category = "UNOPS Internal (centrally managed)",
                ApplicableEntityTypes = "Country",
                Source = "Annex 2 UNOPS Global Structure",
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1003,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LDC",
                ArtifactTypeCode = "LDC",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = "Least Developed Countries",
                Category = "External Global Index",
                ApplicableEntityTypes = "Country",
                Source = "OHRLLS LDCS",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1004,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "LLDC",
                ArtifactTypeCode = "LLDC",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = "Land Locked Developing Countries",
                Category = "External Global Index",
                ApplicableEntityTypes = "Country",
                Source = "OHRLLS LLDCs",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1005,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SIDS",
                ArtifactTypeCode = "SIDS",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = null,
                Category = "External Global Index",
                ApplicableEntityTypes = "Country",
                Source = "OHRLLS SIDS",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1006,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "MVI Score",
                ArtifactTypeCode = "MVI_Score",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Structural vulnerability and lack of resilience of countries to external shocks across three dimensions: Environmental vulnerability, Economic vulnerability, Social vulnerability",
                Category = "External Global Index",
                ApplicableEntityTypes = "Country",
                Source = "OHRLLS MVI",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1007,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Structural Vulnerability Index",
                ArtifactTypeCode = "Structural_Vulnerability_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1008,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Lack of Structural Resilience Index",
                ArtifactTypeCode = "Lack_of_Structural_Resilience_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1009,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "World Bank Fragile Situation",
                ArtifactTypeCode = "World_Bank_Fragile_Situation",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = "Countries and territories identified as experiencing conflict and institutional and social fragility",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "World Bank List of Fragile and Conflict-affected Situations",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1010,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UN Programme Country",
                ArtifactTypeCode = "UN_Programme_Country",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "UNSDG Countries listing",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1011,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "OECD Member",
                ArtifactTypeCode = "OECD_Member",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "OECD Members",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1012,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "DAC Member",
                ArtifactTypeCode = "DAC_Member",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "DAC ODA recipients",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1013,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "UNOPS Country Typology",
                ArtifactTypeCode = "UNOPS_Country_Typology",
                ArtifactDataTypeId = stringDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1014,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "OECD List High Extreme Fragility",
                ArtifactTypeCode = "OECD_List_High_Extreme_Fragility",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = "Evidence-based assessment of fragility trends across countries, to understand where, how, and why fragility is evolving",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1015,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "States of Fragility OECD",
                ArtifactTypeCode = "States_of_Fragility_OECD",
                ArtifactDataTypeId = stringDataTypeId,
                Description = "Evidence-based assessment of fragility trends across countries, to understand where, how, and why fragility is evolving",
                Category = "External Global Index",
                ApplicableEntityTypes = "Country",
                Source = "OECD States of Fragility",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1016,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Fragility Score OECD",
                ArtifactTypeCode = "Fragility_Score_OECD",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1017,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SDG Index",
                ArtifactTypeCode = "SDG_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Progress towards each of the 17 SDGs, per country",
                Category = "External Global Index",
                ApplicableEntityTypes = "Country",
                Source = "Sustainable Development Report",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1018,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SDG Index Rank",
                ArtifactTypeCode = "SDG_Index_Rank",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1019,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "SDGI Year",
                ArtifactTypeCode = "SDGI_Year",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1020,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "HDI Index",
                ArtifactTypeCode = "HDI_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Achievements in human development",
                Category = "External Global Index",
                ApplicableEntityTypes = "Country",
                Source = "UNDP HDR HDI",
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1021,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "HDI Fiscal Year",
                ArtifactTypeCode = "HDI_Fiscal_Year",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1022,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "HDI Group",
                ArtifactTypeCode = "HDI_Group",
                ArtifactDataTypeId = stringDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1023,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Risk Index",
                ArtifactTypeCode = "Inform_Risk_Index",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Risk from humanitarian crisis and disasters that could overwhelm national response capacity",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = true,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1024,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Risk Class",
                ArtifactTypeCode = "Inform_Risk_Class",
                ArtifactDataTypeId = stringDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = true,
                Order = 1025,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Rank",
                ArtifactTypeCode = "Inform_Rank",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1026,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Version",
                ArtifactTypeCode = "Inform_Version",
                ArtifactDataTypeId = stringDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1027,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Inform Download Date",
                ArtifactTypeCode = "Inform_Download_Date",
                ArtifactDataTypeId = dateDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1028,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "GNI Per Capita USD 2024",
                ArtifactTypeCode = "GNI_Per_Capita_USD_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = false,
                Order = 1029,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "GNI Download At",
                ArtifactTypeCode = "GNI_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1030,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Population 2024",
                ArtifactTypeCode = "Population_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = false,
                Order = 1031,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Population Download At",
                ArtifactTypeCode = "Population_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1032,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Urban Population 2024",
                ArtifactTypeCode = "Urban_Population_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = false,
                Order = 1033,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Urban Population Download At",
                ArtifactTypeCode = "Urban_Population_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1034,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Gini Index 2024",
                ArtifactTypeCode = "Gini_Index_2024",
                ArtifactDataTypeId = numberDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = true,
                IsUsedForAI = true,
                Order = 1035,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Gini Index Download At",
                ArtifactTypeCode = "Gini_Index_Download_At",
                ArtifactDataTypeId = dateDataTypeId,
                Description = null,
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1036,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "Special Situation Countries",
                ArtifactTypeCode = "Special_Situation_Countries",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = "QCPR (combines LDC, LLDC, SIDS). Report focuses on tracking demographic trends in LDCs, LLDCs, and SIDS",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1037,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            
            new ArtifactType
            {
                Name = "FSI",
                ArtifactTypeCode = "FSI",
                ArtifactDataTypeId = numberDataTypeId,
                Description = "Fragile State Index",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = "Fragile States Index",
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1038,
                Status = EntityStatus.Active,
                IsDeleted = false
            },

            new ArtifactType
            {
                Name = "Has Active UNSDCF",
                ArtifactTypeCode = "Has_Active_UNSDCF",
                ArtifactDataTypeId = booleanDataTypeId,
                Description = "Indicates whether an active UNSDCF exists",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1039,
                Status = EntityStatus.Active,
                IsDeleted = false
            },

            new ArtifactType
            {
                Name = "Host Agreement",
                ArtifactTypeCode = "Host_Agreement",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Host Agreement Document",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1040,
                Status = EntityStatus.Active,
                IsDeleted = false
            },

            new ArtifactType
            {
                Name = "Humanitarian Peace Security Framework",
                ArtifactTypeCode = "Humanitarian_Peace_Security_Framework",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Humanitarian Peace Security Framework",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1041,
                Status = EntityStatus.Active,
                IsDeleted = false
            },

            new ArtifactType
            {
                Name = "Nationally Determined Contributions (NDCs)",
                ArtifactTypeCode = "NDC",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "Nationally Determined Contributions (NDCs)",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1042,
                Status = EntityStatus.Active,
                IsDeleted = false
            },

            new ArtifactType
            {
                Name = "National Adaptation Plan (NAP)",
                ArtifactTypeCode = "NAP",
                ArtifactDataTypeId = documentDataTypeId,
                Description = "National Adaptation Plan (NAP)",
                Category = null,
                ApplicableEntityTypes = "Country",
                Source = null,
                IsSearchable = false,
                AllowBulkUpdate = false,
                IsUsedForCalculations = false,
                IsUsedForAI = false,
                Order = 1043,
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}
