using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds UNCF Metadata (UN Cooperation Framework Metadata) with proper insert/update logic
/// Data synced from External Data Service (ERP Database)
/// </summary>
public static class UNCFMetadataSeeder
{
    public static async Task SeedUNCFMetadataAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding UNCF Metadata...");

        var metadataToSeed = GetUNCFMetadataToSeed();

        // Get existing UNCF Metadata from database
        var existingMetadata = await context.Set<UNCFMetadata>().ToListAsync();

        // Track metadata identifiers to keep
        var metadataKeysToKeep = metadataToSeed
            .Select(m => new { m.Country, m.UNCooperationFrameworkVersionNo })
            .ToHashSet();

        // Insert or Update UNCF Metadata
        foreach (var metadataData in metadataToSeed)
        {
            var existingRecord = existingMetadata.FirstOrDefault(m =>
                m.Country == metadataData.Country &&
                m.UNCooperationFrameworkVersionNo == metadataData.UNCooperationFrameworkVersionNo);

            if (existingRecord == null)
            {
                // Insert new UNCF Metadata
                context.Set<UNCFMetadata>().Add(metadataData);
                Console.WriteLine($"  ✅ Inserted UNCF Metadata: {metadataData.Country} v{metadataData.UNCooperationFrameworkVersionNo}");
            }
            else
            {
                // Update if any properties changed
                bool hasChanges = false;

                if (existingRecord.Name != metadataData.Name)
                {
                    existingRecord.Name = metadataData.Name;
                    hasChanges = true;
                }

                if (existingRecord.UNCFMetadataId != metadataData.UNCFMetadataId)
                {
                    existingRecord.UNCFMetadataId = metadataData.UNCFMetadataId;
                    hasChanges = true;
                }

                if (existingRecord.UNCFFileURL != metadataData.UNCFFileURL)
                {
                    existingRecord.UNCFFileURL = metadataData.UNCFFileURL;
                    hasChanges = true;
                }

                if (existingRecord.UNCFFileName != metadataData.UNCFFileName)
                {
                    existingRecord.UNCFFileName = metadataData.UNCFFileName;
                    hasChanges = true;
                }

                if (existingRecord.UNCFLastUpdatedDate != metadataData.UNCFLastUpdatedDate)
                {
                    existingRecord.UNCFLastUpdatedDate = metadataData.UNCFLastUpdatedDate;
                    hasChanges = true;
                }

                if (existingRecord.Status != metadataData.Status)
                {
                    existingRecord.Status = metadataData.Status;
                    hasChanges = true;
                }

                if (existingRecord.IsDeleted)
                {
                    existingRecord.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated UNCF Metadata: {metadataData.Country} v{metadataData.UNCooperationFrameworkVersionNo}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped UNCF Metadata (unchanged): {metadataData.Country} v{metadataData.UNCooperationFrameworkVersionNo}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"✅ UNCF Metadata seeding completed - Total: {metadataToSeed.Count}\n");
    }

    private static List<UNCFMetadata> GetUNCFMetadataToSeed()
    {
        return new List<UNCFMetadata>
        {
            new UNCFMetadata
            {
                Name = "AL v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 1,
                Country = "AL",
                UNCFFileURL = "https://drive.google.com/file/d/1EFPmJFQfZrvvcQyogs8oBkai_53FLb4f/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 32, 39, 7, DateTimeKind.Utc),
                UNCFFileName = "AL_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "AM v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 2,
                Country = "AM",
                UNCFFileURL = "https://drive.google.com/file/d/1rzq2QcoBTE8qV0hLJHwXzKM51ZSocWEg/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 503, DateTimeKind.Utc),
                UNCFFileName = "AM_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "AZ v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 3,
                Country = "AZ",
                UNCFFileURL = "https://drive.google.com/file/d/1kfhIn9tTw0aA0medZQUqycMAFas_nXt1/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 35, 277, DateTimeKind.Utc),
                UNCFFileName = "AZ_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BA v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 4,
                Country = "BA",
                UNCFFileURL = "https://drive.google.com/file/d/1uAsa7E61MmswQxJ-q9KgQhq6tWD0tUaL/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 3, 623, DateTimeKind.Utc),
                UNCFFileName = "BA_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "BD v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 5,
                Country = "BD",
                UNCFFileURL = "https://drive.google.com/file/d/13CAlOS0oFi8gGXyCtOIljYjf6jCoaZ21/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 41, 510, DateTimeKind.Utc),
                UNCFFileName = "BD_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "BT v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 6,
                Country = "BT",
                UNCFFileURL = "https://drive.google.com/file/d/1rLjZZgRenPdl4yXMRyKGe7BnVFuG_8UE/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 15, 910, DateTimeKind.Utc),
                UNCFFileName = "BT_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "BW v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 7,
                Country = "BW",
                UNCFFileURL = "https://drive.google.com/file/d/1pMKn8BLbo6SjuhXRazE6ejKXeYIcHvgH/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 39, 930, DateTimeKind.Utc),
                UNCFFileName = "BW_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "BY v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 8,
                Country = "BY",
                UNCFFileURL = "https://drive.google.com/file/d/10so4-AcFyV96r10XtZpGH5JymRflZm5y/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 12, 42, 35, 510, DateTimeKind.Utc),
                UNCFFileName = "BY_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "CD v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 9,
                Country = "CD",
                UNCFFileURL = "https://drive.google.com/file/d/1OF7yxE9dpmMhNcqjp-mMOpjJaIjYq3Bn/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 200, DateTimeKind.Utc),
                UNCFFileName = "CD_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "CF v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 10,
                Country = "CF",
                UNCFFileURL = "https://drive.google.com/file/d/1KHgfcpzeWqYdaIi9RClKjkbD3q32jUQK/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 253, DateTimeKind.Utc),
                UNCFFileName = "CF_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CG v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 11,
                Country = "CG",
                UNCFFileURL = "https://drive.google.com/file/d/1LqMhsqwkCgOBllB6X_LuOEm3WsFJkrEu/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 58, 593, DateTimeKind.Utc),
                UNCFFileName = "CG_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "CI v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 12,
                Country = "CI",
                UNCFFileURL = "https://drive.google.com/file/d/1OIonT2ETB4hBqe62JgLarn6g0mXbLbOX/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 77, DateTimeKind.Utc),
                UNCFFileName = "CI_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "CM v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 13,
                Country = "CM",
                UNCFFileURL = "https://drive.google.com/file/d/1ceSnGP3uxPCI55zuGeE791Ji1viZxxRC/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 49, 963, DateTimeKind.Utc),
                UNCFFileName = "CM_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CN v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 14,
                Country = "CN",
                UNCFFileURL = "https://drive.google.com/file/d/1OsC9TfZ9SKNwKlAiUk5QlmTNQ-UeuAOf/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 18, 917, DateTimeKind.Utc),
                UNCFFileName = "CN_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "CR v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 15,
                Country = "CR",
                UNCFFileURL = "https://drive.google.com/file/d/1Fp6riVcsFPF1H1UH5_mQUHZRA_1vJlM8/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 107, DateTimeKind.Utc),
                UNCFFileName = "CR_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CU v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 16,
                Country = "CU",
                UNCFFileURL = "https://drive.google.com/file/d/1uwKMgLd1RTUL0B4UnHTB4GjCw-E4sir_/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 41, 10, 100, DateTimeKind.Utc),
                UNCFFileName = "CU_2023-02-10.csv"
            },
            new UNCFMetadata
            {
                Name = "EC v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 17,
                Country = "EC",
                UNCFFileURL = "https://drive.google.com/file/d/1zc6w3TWWjN38_qme8RL64jOtIq7IAD3I/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 19, 180, DateTimeKind.Utc),
                UNCFFileName = "EC_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ER v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 18,
                Country = "ER",
                UNCFFileURL = "https://drive.google.com/file/d/11wXEBU9iAezujgdR2RyeFNB0h8Mz9mpU/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 48, 330, DateTimeKind.Utc),
                UNCFFileName = "ER_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ET v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 19,
                Country = "ET",
                UNCFFileURL = "https://drive.google.com/file/d/1ulr3BWbVNtBkjc2PtPK-hqYTRIHuLeC-/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 43, 14, 57, DateTimeKind.Utc),
                UNCFFileName = "ET_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "GA v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 20,
                Country = "GA",
                UNCFFileURL = "https://drive.google.com/file/d/1FcACSQbqQ7cV64WswbeD_o7wFX7ZQT2z/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 46, 30, 943, DateTimeKind.Utc),
                UNCFFileName = "GA_2023-03-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "GE v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 21,
                Country = "GE",
                UNCFFileURL = "https://drive.google.com/file/d/1BqvNCf_wF2WtlVzqO0XzNDpfvfE91kUA/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 3, 620, DateTimeKind.Utc),
                UNCFFileName = "GE_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "GT v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 22,
                Country = "GT",
                UNCFFileURL = "https://drive.google.com/file/d/1LuNmfdsrcDw7sHF4JF1Ay7_71xKBNFMa/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 627, DateTimeKind.Utc),
                UNCFFileName = "GT_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "GW v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 23,
                Country = "GW",
                UNCFFileURL = "https://drive.google.com/file/d/1q3rLSAcqgJ-RXYOdVC1MezhubpgfFc07/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 59, 163, DateTimeKind.Utc),
                UNCFFileName = "GW_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "GY v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 24,
                Country = "GY",
                UNCFFileURL = "https://drive.google.com/file/d/1nxK9UZ2i8H3G43a1DR6TV-LJ0tEnN23s/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 300, DateTimeKind.Utc),
                UNCFFileName = "GY_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "HN v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 25,
                Country = "HN",
                UNCFFileURL = "https://drive.google.com/file/d/1TZjv4rBnpR-uFFwXCF1okUZsO-CAKBho/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 710, DateTimeKind.Utc),
                UNCFFileName = "HN_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "ID v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 26,
                Country = "ID",
                UNCFFileURL = "https://drive.google.com/file/d/16erteqAuoRfagxgC6LPpclGKOfDKyyrY/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 31, 543, DateTimeKind.Utc),
                UNCFFileName = "ID_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "IQ v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 27,
                Country = "IQ",
                UNCFFileURL = "https://drive.google.com/file/d/122RTz6jUWuom-M3o7tzZ1uMPbOUD4Irx/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 660, DateTimeKind.Utc),
                UNCFFileName = "IQ_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "IR v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 28,
                Country = "IR",
                UNCFFileURL = "https://drive.google.com/file/d/1Q4dHLRgzhfP8tX1-WqklSh2hN1HJzVTc/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 20, 880, DateTimeKind.Utc),
                UNCFFileName = "IR_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "KE v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 29,
                Country = "KE",
                UNCFFileURL = "https://drive.google.com/file/d/1l-Nx27XPL79wyKm6Cn1QdFiosLM87MAY/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 49, 543, DateTimeKind.Utc),
                UNCFFileName = "KE_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "KG v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 30,
                Country = "KG",
                UNCFFileURL = "https://drive.google.com/file/d/1nLVgth3u2b-FvstkA4YklPEv5T3HtHbb/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 9, 723, DateTimeKind.Utc),
                UNCFFileName = "KG_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "KM v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 31,
                Country = "KM",
                UNCFFileURL = "https://drive.google.com/file/d/1lQnzSvLeOmTtvFUTUKcT4MiPHF2WwHac/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 34, 420, DateTimeKind.Utc),
                UNCFFileName = "KM_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "KZ v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 32,
                Country = "KZ",
                UNCFFileURL = "https://drive.google.com/file/d/1cHkTXXLipLCVmX2Di4-E1Ma4s_hRN_jM/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 12, 223, DateTimeKind.Utc),
                UNCFFileName = "KZ_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "LA v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 33,
                Country = "LA",
                UNCFFileURL = "https://drive.google.com/file/d/1s7wGR3O1SCrsG9I7fR_O8HtXehH5c2rB/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 36, 413, DateTimeKind.Utc),
                UNCFFileName = "LA_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "LB v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 34,
                Country = "LB",
                UNCFFileURL = "https://drive.google.com/file/d/1rCAbLCbnn_7kYJmyXT4uAjgxgfBjY31W/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 123, DateTimeKind.Utc),
                UNCFFileName = "LB_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "LK v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 35,
                Country = "LK",
                UNCFFileURL = "https://drive.google.com/file/d/1TMYYvtqwXVBy-rPeAr5Kxyax1VI8X6TC/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 21, 533, DateTimeKind.Utc),
                UNCFFileName = "LK_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "LR v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 36,
                Country = "LR",
                UNCFFileURL = "https://drive.google.com/file/d/1PuCCAJJIrDbyyPMMMBhQalKnFU8-PRMp/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 46, 537, DateTimeKind.Utc),
                UNCFFileName = "LR_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "LS v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 37,
                Country = "LS",
                UNCFFileURL = "https://drive.google.com/file/d/19fG5-zJvAWT9KSFoTkotQO73pcxouLo8/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 15, 110, DateTimeKind.Utc),
                UNCFFileName = "LS_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "LY v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 38,
                Country = "LY",
                UNCFFileURL = "https://drive.google.com/file/d/1b0n3fhFeObC776bTVA7XvRJj-HljV5vK/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 623, DateTimeKind.Utc),
                UNCFFileName = "LY_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MD v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 39,
                Country = "MD",
                UNCFFileURL = "https://drive.google.com/file/d/19z4VuKjlCzrsIw7G7YRys_xSrD_cE59r/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 9, 847, DateTimeKind.Utc),
                UNCFFileName = "MD_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ME v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 40,
                Country = "ME",
                UNCFFileURL = "https://drive.google.com/file/d/1MTpqd4cIeH7qpJIUpsjnJN4fHV448kb5/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 32, 170, DateTimeKind.Utc),
                UNCFFileName = "ME_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MG v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 41,
                Country = "MG",
                UNCFFileURL = "https://drive.google.com/file/d/1ZYPOP9_gbDPBEbO2BiZ0CFWq-IOMnhDo/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 340, DateTimeKind.Utc),
                UNCFFileName = "MG_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "MK v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 42,
                Country = "MK",
                UNCFFileURL = "https://drive.google.com/file/d/1AuqaB1QqdIfRh_6u8yekN0RGtjdvxqRM/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 41, 680, DateTimeKind.Utc),
                UNCFFileName = "MK_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "ML v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 43,
                Country = "ML",
                UNCFFileURL = "https://drive.google.com/file/d/1hn3jNgN1ch-XF1u7rPuvBp5SJpNoX4dz/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 1, 560, DateTimeKind.Utc),
                UNCFFileName = "ML_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "MM v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 44,
                Country = "MM",
                UNCFFileURL = "https://drive.google.com/file/d/16HFgIs0MOKDGEnzE2vpCTmwC2g97h8t3/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 53, 6, 917, DateTimeKind.Utc),
                UNCFFileName = "MM_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "MN v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 45,
                Country = "MN",
                UNCFFileURL = "https://drive.google.com/file/d/1MPpK0k2xW0lMEtTgqepvQPMekpRJsfFz/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 42, 523, DateTimeKind.Utc),
                UNCFFileName = "MN_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MV v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 46,
                Country = "MV",
                UNCFFileURL = "https://drive.google.com/file/d/1cnyTqVrWMj7_NhZsR1TdximPm0h0PBCV/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 18, 800, DateTimeKind.Utc),
                UNCFFileName = "MV_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "MX v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 47,
                Country = "MX",
                UNCFFileURL = "https://drive.google.com/file/d/1g3xp06Th8ygQyyuv4BkMQMeJbJRHMSfm/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 337, DateTimeKind.Utc),
                UNCFFileName = "MX_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "MZ v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 48,
                Country = "MZ",
                UNCFFileURL = "https://drive.google.com/file/d/1Mbzb4kU1TsN7xhJz8eO73vfEWaglzcs3/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 1, 3, 473, DateTimeKind.Utc),
                UNCFFileName = "MZ_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "NG v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 49,
                Country = "NG",
                UNCFFileURL = "https://drive.google.com/file/d/1GfTkYI2rQEq8Q_YPXZ4asMiZyBL6K5rJ/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 360, DateTimeKind.Utc),
                UNCFFileName = "NG_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PA v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 50,
                Country = "PA",
                UNCFFileURL = "https://drive.google.com/file/d/1Y97sYh5R_HDzHU9-jWf0PpShfpPgh_85/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 21, 617, DateTimeKind.Utc),
                UNCFFileName = "PA_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "PE v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 51,
                Country = "PE",
                UNCFFileURL = "https://drive.google.com/file/d/1en-4KlKkl-qWKrZkcA2Si6jJ6LL4pTE0/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 46, 800, DateTimeKind.Utc),
                UNCFFileName = "PE_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PK v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 52,
                Country = "PK",
                UNCFFileURL = "https://drive.google.com/file/d/1w0lg2hSH97DeRhHWn4PKxVtJ1ozzeA58/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 7, 6, 403, DateTimeKind.Utc),
                UNCFFileName = "PK_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PS v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 53,
                Country = "PS",
                UNCFFileURL = "https://drive.google.com/file/d/1cycnqiuReC1rOZ2B5MMCsGZ9KB-mtBMx/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 8, 50, 987, DateTimeKind.Utc),
                UNCFFileName = "PS_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PY v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 54,
                Country = "PY",
                UNCFFileURL = "https://drive.google.com/file/d/1FgAMLc0f52b1jUK19pV0s_zSKWdWfopc/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 447, DateTimeKind.Utc),
                UNCFFileName = "PY_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "RS v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 55,
                Country = "RS",
                UNCFFileURL = "https://drive.google.com/file/d/1v86ahV93eWwtDZ1lJnKmyqTmgm8DaHkk/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 623, DateTimeKind.Utc),
                UNCFFileName = "RS_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "RW v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 56,
                Country = "RW",
                UNCFFileURL = "https://drive.google.com/file/d/15G_jYktWdaj4yPAeZCCqfOcx2YFyw0LB/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 18, 60, DateTimeKind.Utc),
                UNCFFileName = "RW_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "SA v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 57,
                Country = "SA",
                UNCFFileURL = "https://drive.google.com/file/d/1YR9A5XWsBJ5F7Rai-6PVnKu9dRMF5Rk7/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 35, 800, DateTimeKind.Utc),
                UNCFFileName = "SA_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SL v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 58,
                Country = "SL",
                UNCFFileURL = "https://drive.google.com/file/d/1zmFhSYA1jOdfpWFoOdLWTXNZxt_udmK3/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 11, 4, 37, DateTimeKind.Utc),
                UNCFFileName = "SL_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "SN v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 59,
                Country = "SN",
                UNCFFileURL = "https://drive.google.com/file/d/1uh1Nq4jif1mTHpe77CZqXObg7vWxlxZq/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 11, 24, 687, DateTimeKind.Utc),
                UNCFFileName = "SN_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "SS v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 60,
                Country = "SS",
                UNCFFileURL = "https://drive.google.com/file/d/1b-BteZlzWeLIh1bsPFpR2AgnTwcMTAlj/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 12, 5, 70, DateTimeKind.Utc),
                UNCFFileName = "SS_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ST v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 61,
                Country = "ST",
                UNCFFileURL = "https://drive.google.com/file/d/1hZStjF9pltKVej5QWks99R0TDB_WEXZ7/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 13, 40, 547, DateTimeKind.Utc),
                UNCFFileName = "ST_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SV v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 62,
                Country = "SV",
                UNCFFileURL = "https://drive.google.com/file/d/1aHLZLfXiZJJ2QSPiHYM4nvfVruMA3xN8/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 30, DateTimeKind.Utc),
                UNCFFileName = "SV_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "SZ v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 63,
                Country = "SZ",
                UNCFFileURL = "https://drive.google.com/file/d/1c3r_n7ZbOh2Djom66veZzUVvSlaM6j_D/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 23, 537, DateTimeKind.Utc),
                UNCFFileName = "SZ_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "TH v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 64,
                Country = "TH",
                UNCFFileURL = "https://drive.google.com/file/d/1rCzy60zqV4gq6cTg_cFD7AuGxezV1YHp/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 46, 170, DateTimeKind.Utc),
                UNCFFileName = "TH_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "TJ v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 65,
                Country = "TJ",
                UNCFFileURL = "https://drive.google.com/file/d/1_xCoG8xpaWM0KvJ3gBpV7W0E7L_j3ezZ/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 3, 520, DateTimeKind.Utc),
                UNCFFileName = "TJ_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TL v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 66,
                Country = "TL",
                UNCFFileURL = "https://drive.google.com/file/d/1lhQ1dbPghO-0-2gxtXDXRtYCRi5k1u3T/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 24, 750, DateTimeKind.Utc),
                UNCFFileName = "TL_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "TM v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 67,
                Country = "TM",
                UNCFFileURL = "https://drive.google.com/file/d/1oIA16pcWe71zRSiT4DdTtRRJZDdtPuDm/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 43, 467, DateTimeKind.Utc),
                UNCFFileName = "TM_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "TN v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 68,
                Country = "TN",
                UNCFFileURL = "https://drive.google.com/file/d/1GbIoQjt1IbD1noM_P8UNDWU0L7lNAo7a/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 4, 700, DateTimeKind.Utc),
                UNCFFileName = "TN_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "TR v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 69,
                Country = "TR",
                UNCFFileURL = "https://drive.google.com/file/d/1vjaSkWFlxGiSRtqyUt5ZtBefSdeZi6A8/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 423, DateTimeKind.Utc),
                UNCFFileName = "TR_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "UG v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 70,
                Country = "UG",
                UNCFFileURL = "https://drive.google.com/file/d/1eHrpjeMWCX3NaKDQ0a5tKWctNm2YmJ01/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 52, 317, DateTimeKind.Utc),
                UNCFFileName = "UG_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "UY v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 71,
                Country = "UY",
                UNCFFileURL = "https://drive.google.com/file/d/1c92rLfg7Vs49Hlmvuhx5Pq0k9e_DfNC9/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 650, DateTimeKind.Utc),
                UNCFFileName = "UY_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "UZ v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 72,
                Country = "UZ",
                UNCFFileURL = "https://drive.google.com/file/d/1--28bAOsESOUySspfLLJGwB8yUavwSay/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 39, 213, DateTimeKind.Utc),
                UNCFFileName = "UZ_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "VE v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 73,
                Country = "VE",
                UNCFFileURL = "https://drive.google.com/file/d/179h6VgiuG2xKOe5TpC-LXPdbksrEdRD1/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 337, DateTimeKind.Utc),
                UNCFFileName = "VE_2023-02-27_ND .csv"
            },
            new UNCFMetadata
            {
                Name = "VN v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 74,
                Country = "VN",
                UNCFFileURL = "https://drive.google.com/file/d/1L8vWOjChF8kd6qKSigjRIGFF-BZyV_3X/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 29, 473, DateTimeKind.Utc),
                UNCFFileName = "VN_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "XK v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 75,
                Country = "XK",
                UNCFFileURL = "https://drive.google.com/file/d/1nkHb5-THYXqgs6tRqEIg5qgokKxnhpDX/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 58, 980, DateTimeKind.Utc),
                UNCFFileName = "XK_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "ZA v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 76,
                Country = "ZA",
                UNCFFileURL = "https://drive.google.com/file/d/1L1H9j9GpLoFiw9DTkwMCiR_UBzl87ODl/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 200, DateTimeKind.Utc),
                UNCFFileName = "ZA_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "ZW v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 77,
                Country = "ZW",
                UNCFFileURL = "https://drive.google.com/file/d/1whDpzYyuidsj9n4p0974UarlmsId-JWY/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 37, 300, DateTimeKind.Utc),
                UNCFFileName = "ZW_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "SO v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 78,
                Country = "SO",
                UNCFFileURL = "https://drive.google.com/file/d/13M2ZYAuRVfcMhjaVSBeM_1i3syJiudzP/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 610, DateTimeKind.Utc),
                UNCFFileName = "SO_2023-02-23.csv"
            },
            new UNCFMetadata
            {
                Name = "JO v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 79,
                Country = "JO",
                UNCFFileURL = "https://drive.google.com/file/d/1Pw9gieHGjK5OTnth9G1R58k2fryL7xTE/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 13, 9, 13, 24, 723, DateTimeKind.Utc),
                UNCFFileName = "JO_2023-03-09_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "YE v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 80,
                Country = "YE",
                UNCFFileURL = "https://drive.google.com/file/d/1soUsgeKViOpRHOnYZ5HBAV3HWwXjzObm/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 3, 13, 10, 51, 9, 547, DateTimeKind.Utc),
                UNCFFileName = "YE_2023-03-12_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SY v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 81,
                Country = "SY",
                UNCFFileURL = "https://drive.google.com/file/d/1ZNXWpUrNfIdA35boceQOLtEBlLNyZ27F/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 4, 13, 13, 42, 3, 743, DateTimeKind.Utc),
                UNCFFileName = "SY_2023-04-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "DE v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 82,
                Country = "DE",
                UNCFFileURL = "https://drive.google.com/file/d/1ZNXWpUrNfIdA35boceQOLtEBlLNyZ27F/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 4, 20, 14, 17, 51, 647, DateTimeKind.Utc),
                UNCFFileName = "SY_2023-04-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "NE v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 83,
                Country = "NE",
                UNCFFileURL = "https://drive.google.com/file/d/1r51e5szs83Dd_E1WhwMKbKYcQn46H8t2/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 4, 25, 12, 37, 58, 360, DateTimeKind.Utc),
                UNCFFileName = "NE_2023-04-25_ND .csv"
            },
            new UNCFMetadata
            {
                Name = "HT v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 84,
                Country = "HT",
                UNCFFileURL = "https://drive.google.com/file/d/1t_M3BFHwtO6nRffyVzUtn-EjE_irpnRl/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 4, 26, 14, 16, 58, 347, DateTimeKind.Utc),
                UNCFFileName = "HT_2023-04-26_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "DE v2",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 85,
                Country = "DE",
                UNCFFileURL = "https://drive.google.com/file/d/1t_M3BFHwtO6nRffyVzUtn-EjE_irpnRl/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 5, 1, 10, 47, 59, 980, DateTimeKind.Utc),
                UNCFFileName = "HT_2023-04-26_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "NP v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 86,
                Country = "NP",
                UNCFFileURL = "https://drive.google.com/file/d/1-0CCOuaELE1d3XoK1j2LiC6YodvKdMcS/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 5, 3, 10, 31, 42, 920, DateTimeKind.Utc),
                UNCFFileName = "NP_2023-05-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BJ v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 87,
                Country = "BJ",
                UNCFFileURL = "https://drive.google.com/file/d/11boy9lO29oV0wpoLls9g-3-Z8pgaSrfF/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 6, 2, 14, 32, 19, 933, DateTimeKind.Utc),
                UNCFFileName = "BJ_2023-06-02_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MR v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 88,
                Country = "MR",
                UNCFFileURL = "https://drive.google.com/file/d/1mxIMshwWDA4u7rl6ta0m0rcLe64V7f51/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 6, 2, 14, 39, 20, 147, DateTimeKind.Utc),
                UNCFFileName = "MR_2023-06-02_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BO v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 90,
                Country = "BO",
                UNCFFileURL = "https://drive.google.com/file/d/1sLEQbhtatu04mVtqGC0IifHwbn5Bgiei/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 613, DateTimeKind.Utc),
                UNCFFileName = "BO_2023-06-14_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "FM v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 91,
                Country = "FM",
                UNCFFileURL = "https://drive.google.com/file/d/1W323xRH_pZDH2rR0HK-5kmdZWMZO9MhA/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 14, 33, 43, 730, DateTimeKind.Utc),
                UNCFFileName = "FM_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CK v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 92,
                Country = "CK",
                UNCFFileURL = "https://drive.google.com/file/d/1I15bKuIiM1RoYJkRMc1AChCD3MWygqWV/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 14, 37, 17, 20, DateTimeKind.Utc),
                UNCFFileName = "CK_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MH v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 93,
                Country = "MH",
                UNCFFileURL = "https://drive.google.com/file/d/1AeTp0CchJSwJKQCHxcMJ3LkwSeGERY9R/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 14, 40, 3, 80, DateTimeKind.Utc),
                UNCFFileName = "MH_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "FJ v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 94,
                Country = "FJ",
                UNCFFileURL = "https://drive.google.com/file/d/1LSXB49Akjxc5I9fxnwxUrkykonYQmu5f/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 14, 41, 36, 793, DateTimeKind.Utc),
                UNCFFileName = "FJ_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "WS v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 95,
                Country = "WS",
                UNCFFileURL = "https://drive.google.com/file/d/1yqU0v1xPxqsif99TLDNxXEGoVZw5av2T/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 14, 43, 6, 250, DateTimeKind.Utc),
                UNCFFileName = "WS_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SB v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 96,
                Country = "SB",
                UNCFFileURL = "https://drive.google.com/file/d/15NKSgqRSUupPzHeyql3nUgAL2yLzgDoO/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 0, 24, 870, DateTimeKind.Utc),
                UNCFFileName = "SB_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "KI v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 97,
                Country = "KI",
                UNCFFileURL = "https://drive.google.com/file/d/1i4xKx3xCXTjVIYseWaFCGamoO-o2ioQo/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 3, 13, 913, DateTimeKind.Utc),
                UNCFFileName = "KI_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TK v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 98,
                Country = "TK",
                UNCFFileURL = "https://drive.google.com/file/d/12tu-QBEKcPYu1U7tBV3Um95HbDi_XO6H/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 25, 56, 237, DateTimeKind.Utc),
                UNCFFileName = "TK_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "NR v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 99,
                Country = "NR",
                UNCFFileURL = "https://drive.google.com/file/d/1CeS0L1dpewmkllih1RiS9Ih7IPtXHuJN/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 29, 40, 593, DateTimeKind.Utc),
                UNCFFileName = "NR_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TO v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 100,
                Country = "TO",
                UNCFFileURL = "https://drive.google.com/file/d/1wDpDFkBxmBSD8qyLeqkO7F_SnuqxxdUl/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 30, 48, 333, DateTimeKind.Utc),
                UNCFFileName = "TO_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "NU v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 101,
                Country = "NU",
                UNCFFileURL = "https://drive.google.com/file/d/1u00zdnpufymDhO-FExQRoM3avrcUZNrB/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 33, 11, 913, DateTimeKind.Utc),
                UNCFFileName = "NU_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TV v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 102,
                Country = "TV",
                UNCFFileURL = "https://drive.google.com/file/d/1kKkcNviB-bhVBhd_3YtnglnJj30TW0nn/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 34, 45, 530, DateTimeKind.Utc),
                UNCFFileName = "TV_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PW v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 103,
                Country = "PW",
                UNCFFileURL = "https://drive.google.com/file/d/1wyUlscxoQ32DBk3QOhvHPom2zq8wAWMF/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 36, 13, 260, DateTimeKind.Utc),
                UNCFFileName = "PW_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "VU v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 104,
                Country = "VU",
                UNCFFileURL = "https://drive.google.com/file/d/18OHl2y6V7mDblHMIrXvXVauJd_mbeckm/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 3, 15, 37, 18, 260, DateTimeKind.Utc),
                UNCFFileName = "VU_2023-07-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "GQ v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 105,
                Country = "GQ",
                UNCFFileURL = "https://drive.google.com/file/d/1hK1VIN3Xv1hZiZaVv8XF8sqexyLckvHg/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 4, 10, 56, 19, 543, DateTimeKind.Utc),
                UNCFFileName = "GQ_2023-07-04_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TG v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 106,
                Country = "TG",
                UNCFFileURL = "https://drive.google.com/file/d/1EA52B56X_8vRsVc1fuRnVD1MhlS0DjCu/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 7, 4, 11, 4, 41, 927, DateTimeKind.Utc),
                UNCFFileName = "TG_2023-07-04_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CL v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 107,
                Country = "CL",
                UNCFFileURL = "https://drive.google.com/file/d/1h-tkA6zr5Utp4o8XMdt5aPFXa0rLzhw7/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 970, DateTimeKind.Utc),
                UNCFFileName = "CL_2023-08-01_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "AF v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 108,
                Country = "AF",
                UNCFFileURL = "https://drive.google.com/file/d/11KquZyN3stevlQIA9xuhoFDN1la9Vn8a/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 8, 10, 46, 34, 473, DateTimeKind.Utc),
                UNCFFileName = "AF_2023-08-08_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "DZ v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 109,
                Country = "DZ",
                UNCFFileURL = "https://drive.google.com/file/d/1rFjxl9JPEA-O8Vvz24zT7HMOZ-f00bop/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 8, 10, 53, 23, 577, DateTimeKind.Utc),
                UNCFFileName = "DZ_2023-08-08_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BI v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 110,
                Country = "BI",
                UNCFFileURL = "https://drive.google.com/file/d/1s_dXvqdn6hutUX_l8A3MB6KVK7265Ro-/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 8, 10, 59, 30, 550, DateTimeKind.Utc),
                UNCFFileName = "BI_2023-08-08_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CV v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 111,
                Country = "CV",
                UNCFFileURL = "https://drive.google.com/file/d/1MVxCvnuD74JQuBg2SkTLFLiKeOL0y_8v/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 8, 11, 4, 42, 980, DateTimeKind.Utc),
                UNCFFileName = "CV_2023-08-08_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "EG v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 112,
                Country = "EG",
                UNCFFileURL = "https://drive.google.com/file/d/19FKaYUzT9Fr4tLtO4TMqbN77H1aL34tv/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 8, 11, 10, 56, 413, DateTimeKind.Utc),
                UNCFFileName = "EG_2023-08-08_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ZM v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 113,
                Country = "ZM",
                UNCFFileURL = "https://drive.google.com/file/d/1sSU2IuoxaxtY_MlHSkaQXZAYwetWcOxj/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 8, 11, 21, 53, 650, DateTimeKind.Utc),
                UNCFFileName = "ZM_2023-08-08_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "AR v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 114,
                Country = "AR",
                UNCFFileURL = "https://drive.google.com/file/d/1W0kHNOgza2j0WbL5fEotoCtzYnd5QtBj/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 617, DateTimeKind.Utc),
                UNCFFileName = "AR_2023-08-08_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "IN v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 115,
                Country = "IN",
                UNCFFileURL = "https://drive.google.com/file/d/1BTRLVI_SSOdqXxrfMZKU6HbCHG54UH7s/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 8, 16, 16, 4, 43, 7, DateTimeKind.Utc),
                UNCFFileName = "IN_2023-08-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PG v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 116,
                Country = "PG",
                UNCFFileURL = "https://drive.google.com/file/d/12pW_bbNojijqwgtK72Pal7UpaBV-twZV/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 9, 14, 11, 50, 15, 343, DateTimeKind.Utc),
                UNCFFileName = "PG_2023-09-14_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "DO v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 117,
                Country = "DO",
                UNCFFileURL = "https://drive.google.com/file/d/1TQRGef5ZZrmR7X1fh0Vcwaf1jKkHIJQ0/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 9, 50, 37, 167, DateTimeKind.Utc),
                UNCFFileName = "DO_2023-13-09_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "AL v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 118,
                Country = "AL",
                UNCFFileURL = "https://drive.google.com/file/d/1UaVCJtZk_jHQEmJNl0rLGfflvzkVJ6gP/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 9, 57, 14, 160, DateTimeKind.Utc),
                UNCFFileName = "AL_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BA v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 119,
                Country = "BA",
                UNCFFileURL = "https://drive.google.com/file/d/10xLzEcYrmb0oNfd0PDWCMpCyEtD6DM90/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 10, 2, 49, 640, DateTimeKind.Utc),
                UNCFFileName = "BA_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BD v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 120,
                Country = "BD",
                UNCFFileURL = "https://drive.google.com/file/d/1igPxHRGY8eHZehl7JP7Vh-VIB0qoaWfz/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 10, 7, 5, 180, DateTimeKind.Utc),
                UNCFFileName = "BD_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BT v2",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 121,
                Country = "BT",
                UNCFFileURL = "https://drive.google.com/file/d/1w_3BtIHy0tc6ErKmzDGa7eyMk5jAU8Hm/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 10, 11, 30, 910, DateTimeKind.Utc),
                UNCFFileName = "BT_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BW v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 122,
                Country = "BW",
                UNCFFileURL = "https://drive.google.com/file/d/1X2YJ8BA2u09kBEvGGr1uIh59TXJlq4GI/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 10, 22, 15, 870, DateTimeKind.Utc),
                UNCFFileName = "BW_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CD v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 123,
                Country = "CD",
                UNCFFileURL = "https://drive.google.com/file/d/1pPVGsET8c9Kjkh-c3A64mU_cI_kB45yl/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 503, DateTimeKind.Utc),
                UNCFFileName = "CD_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CG v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 124,
                Country = "CG",
                UNCFFileURL = "https://drive.google.com/file/d/1Cw5Xsn726Wv43oQpB2mfIhI8Uab0SrJz/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 12, 27, 4, 553, DateTimeKind.Utc),
                UNCFFileName = "CG_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CI v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 125,
                Country = "CI",
                UNCFFileURL = "https://drive.google.com/file/d/1l7AAdSbKPEWfZbfM0xJawfqWnYzR7M3u/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 49, 993, DateTimeKind.Utc),
                UNCFFileName = "CI_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CN v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 126,
                Country = "CN",
                UNCFFileURL = "https://drive.google.com/file/d/1MUNPF-7nYh3vg0UAkWcvV7T-0MIn371E/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 12, 45, 39, 887, DateTimeKind.Utc),
                UNCFFileName = "CN_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CU v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 127,
                Country = "CU",
                UNCFFileURL = "https://drive.google.com/file/d/1NuGqHKzDVzCMgtuQKlBoDdrTlB2HWN1G/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 12, 54, 47, 573, DateTimeKind.Utc),
                UNCFFileName = "CU_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SV v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 128,
                Country = "SV",
                UNCFFileURL = "https://drive.google.com/file/d/12Zc8HNdXSh7VtuF3OeTOA29wRjjmi-dT/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 600, DateTimeKind.Utc),
                UNCFFileName = "SV_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SZ v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 129,
                Country = "SZ",
                UNCFFileURL = "https://drive.google.com/file/d/1xHc4dU_b4YyTJpEpAM6uRo5SQfmaaGIK/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 16, 53, 783, DateTimeKind.Utc),
                UNCFFileName = "SZ_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ET v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 130,
                Country = "ET",
                UNCFFileURL = "https://drive.google.com/file/d/1D962dT4hn1jNBBFN4BCE7LWpd4qDzszA/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 21, 18, 863, DateTimeKind.Utc),
                UNCFFileName = "ET_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "GE v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 131,
                Country = "GE",
                UNCFFileURL = "https://drive.google.com/file/d/1pOqLee3q3o5fMLwEmOxRJFV6qN9hLEwX/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 28, 45, 833, DateTimeKind.Utc),
                UNCFFileName = "GE_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "GT v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 132,
                Country = "GT",
                UNCFFileURL = "https://drive.google.com/file/d/1MeLUsVjUz-5TNzVufHRVeq7EjMh0uVu-/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 927, DateTimeKind.Utc),
                UNCFFileName = "GT_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "GW v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 133,
                Country = "GW",
                UNCFFileURL = "https://drive.google.com/file/d/1leI16m3YHAr6aKnqcT9XsMgIKlSmRX_G/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 51, 28, 450, DateTimeKind.Utc),
                UNCFFileName = "GW_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "HN v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 134,
                Country = "HN",
                UNCFFileURL = "https://drive.google.com/file/d/13vni9I_U21bI9BoZ5-f1ynXQkn6Gyglq/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 287, DateTimeKind.Utc),
                UNCFFileName = "HN_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ID v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 135,
                Country = "ID",
                UNCFFileURL = "https://drive.google.com/file/d/1ieqLOHBk7chUkZ9h1cZS1rPsmZn09jxI/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 13, 57, 14, 283, DateTimeKind.Utc),
                UNCFFileName = "ID_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "IQ v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 136,
                Country = "IQ",
                UNCFFileURL = "https://drive.google.com/file/d/11N-ux7FILbO1GAVI5b_8p1UhuiOQdX2x/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 393, DateTimeKind.Utc),
                UNCFFileName = "IQ_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "KZ v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 137,
                Country = "KZ",
                UNCFFileURL = "https://drive.google.com/file/d/1wxtylN3FNQUiwbuMH7MgHASG1WWvaEPH/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 14, 31, 12, 987, DateTimeKind.Utc),
                UNCFFileName = "KZ_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "KE v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 138,
                Country = "KE",
                UNCFFileURL = "https://drive.google.com/file/d/1i44DlRZ7ynWVPiqpvkAM7_ziIrgLRUlg/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 14, 34, 21, 253, DateTimeKind.Utc),
                UNCFFileName = "KE_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "XK v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 139,
                Country = "XK",
                UNCFFileURL = "https://drive.google.com/file/d/1nU2ndsvzWrKOml8s3LYJwsef7D6k2s5m/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 14, 37, 4, 617, DateTimeKind.Utc),
                UNCFFileName = "XK_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "LA v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 140,
                Country = "LA",
                UNCFFileURL = "https://drive.google.com/file/d/1TEQSvhw3erkbm3ZWZXjIOqr61o_8EwSS/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 1, 19, 383, DateTimeKind.Utc),
                UNCFFileName = "LA_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "LS v2",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 141,
                Country = "LS",
                UNCFFileURL = "https://drive.google.com/file/d/1N0iCqWyIS69zwXN_Fku0oROgP1CIhgK1/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 4, 5, 433, DateTimeKind.Utc),
                UNCFFileName = "LS_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "LR v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 142,
                Country = "LR",
                UNCFFileURL = "https://drive.google.com/file/d/1orEea_OD6E5d1l1mAfhChPELMT6C0h9X/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 6, 23, 740, DateTimeKind.Utc),
                UNCFFileName = "LR_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MG v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 143,
                Country = "MG",
                UNCFFileURL = "https://drive.google.com/file/d/1dv5kIuOZ3KwtiL10LrByMTJ2puD_kUzq/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 10, DateTimeKind.Utc),
                UNCFFileName = "MG_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MV v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 144,
                Country = "MV",
                UNCFFileURL = "https://drive.google.com/file/d/1U3qyzWfw8h8nWJbNdyrf5MiiADyj_lh2/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 25, 2, 830, DateTimeKind.Utc),
                UNCFFileName = "MV_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MX v2",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 145,
                Country = "MX",
                UNCFFileURL = "https://drive.google.com/file/d/1FjMexOV6eYVbFCBbbaRBjmq1ng73nKqa/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 38, 41, 713, DateTimeKind.Utc),
                UNCFFileName = "MX_2023-09-13_ND - AM_2023-02-28_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MX v3",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 146,
                Country = "MX",
                UNCFFileURL = "https://drive.google.com/file/d/1FjMexOV6eYVbFCBbbaRBjmq1ng73nKqa/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 3,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 12, 410, DateTimeKind.Utc),
                UNCFFileName = "MX_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MZ v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 147,
                Country = "MZ",
                UNCFFileURL = "https://drive.google.com/file/d/1_Od8kEcPqNdl3DA8ptvbHXAtR3TWBuJa/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 40, 763, DateTimeKind.Utc),
                UNCFFileName = "MZ_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MK v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 148,
                Country = "MK",
                UNCFFileURL = "https://drive.google.com/file/d/1SUCglsQLbnc8l_N44yIp_mknfpb6nz4N/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 15, 55, 44, 423, DateTimeKind.Utc),
                UNCFFileName = "MK_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PA v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 149,
                Country = "PA",
                UNCFFileURL = "https://drive.google.com/file/d/1fiMyRt815cFMmUj2cyW-BFD59lyKGRpW/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 13, 16, 2, 6, 797, DateTimeKind.Utc),
                UNCFFileName = "PA_2023-09-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PY v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 150,
                Country = "PY",
                UNCFFileURL = "https://drive.google.com/file/d/1Cb-t4f54-sn2iSAOCY6WR9KkhhA6SY2u/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 787, DateTimeKind.Utc),
                UNCFFileName = "PY_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "RW v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 151,
                Country = "RW",
                UNCFFileURL = "https://drive.google.com/file/d/1oT9isplLejrlBxMTtQDNaS8EDNO2WqIp/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 11, 27, 40, 287, DateTimeKind.Utc),
                UNCFFileName = "RW_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SN v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 152,
                Country = "SN",
                UNCFFileURL = "https://drive.google.com/file/d/1_SCx7bHe8e82zHokUEYd_17SixAzgylX/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 11, 34, 43, 117, DateTimeKind.Utc),
                UNCFFileName = "SN_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "RS v2",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 153,
                Country = "RS",
                UNCFFileURL = "https://drive.google.com/file/d/1kEdlOkHMp3buuBjuGdAWY9IMv-Eya-eM/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 13, 43, 30, 103, DateTimeKind.Utc),
                UNCFFileName = "RS_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SL v2",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 154,
                Country = "SL",
                UNCFFileURL = "https://drive.google.com/file/d/1FU4HaVYpfzui51xNMyOJdLeAZU5Fwm6y/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 11, 47, 51, 633, DateTimeKind.Utc),
                UNCFFileName = "SL_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SO v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 155,
                Country = "SO",
                UNCFFileURL = "https://drive.google.com/file/d/1L2JDyQjIfmvdrPciWzD3o0qfvWh_0S0Q/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 227, DateTimeKind.Utc),
                UNCFFileName = "SO_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ZA v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 156,
                Country = "ZA",
                UNCFFileURL = "https://drive.google.com/file/d/1u0RxuGUhM2BSYhWRbf5adTAdiIRw6zaz/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 247, DateTimeKind.Utc),
                UNCFFileName = "ZA_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TH v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 157,
                Country = "TH",
                UNCFFileURL = "https://drive.google.com/file/d/1ap1rtM2fMDrHsd8CKDyddgIXOkQZT9eC/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 12, 31, 47, 923, DateTimeKind.Utc),
                UNCFFileName = "TH_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TL v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 158,
                Country = "TL",
                UNCFFileURL = "https://drive.google.com/file/d/1Gy3o1ZWgTukjFS_zTie2i2I3R-CDN87R/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 14, 18, 24, 720, DateTimeKind.Utc),
                UNCFFileName = "TL_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TN v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 159,
                Country = "TN",
                UNCFFileURL = "https://drive.google.com/file/d/1-6RXmWZy7d7RKY8kGlSH0Azz4KzeHiu5/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 14, 27, 40, 133, DateTimeKind.Utc),
                UNCFFileName = "TN_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TR v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 160,
                Country = "TR",
                UNCFFileURL = "https://drive.google.com/file/d/1CJtfx4ZWg_WR_mV8m6KCk8A1ZV_64mDO/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 950, DateTimeKind.Utc),
                UNCFFileName = "TR_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TM v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 161,
                Country = "TM",
                UNCFFileURL = "https://drive.google.com/file/d/1QoXvAM2M5ZZnmIiD-tZEhG6qRD1IlCwA/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 15, 29, 26, 137, DateTimeKind.Utc),
                UNCFFileName = "TM_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "UG v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 162,
                Country = "UG",
                UNCFFileURL = "https://drive.google.com/file/d/1zd27movhjQBRijTolKsp65oMcfXcJI7u/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 15, 32, 5, 767, DateTimeKind.Utc),
                UNCFFileName = "UG_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "UY v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 163,
                Country = "UY",
                UNCFFileURL = "https://drive.google.com/file/d/1CXPqy1vaGNBZNm3vc_kYBVpXnx4bb1jS/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 100, DateTimeKind.Utc),
                UNCFFileName = "UY_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "UZ v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 164,
                Country = "UZ",
                UNCFFileURL = "https://drive.google.com/file/d/1iGIV1wK0eD-a2EvbgLd00PsMWkBCQFV2/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 16, 21, 4, 320, DateTimeKind.Utc),
                UNCFFileName = "UZ_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "VN v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 165,
                Country = "VN",
                UNCFFileURL = "https://drive.google.com/file/d/1nCQits0-aOijgUQXFIKElkK9PaGoghDE/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 17, 24, 54, 633, DateTimeKind.Utc),
                UNCFFileName = "VN_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ZW v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 166,
                Country = "ZW",
                UNCFFileURL = "https://drive.google.com/file/d/1LvzAseHNx1xBAbTnP_aWzUdh7qA9BOKR/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 16, 17, 28, 36, 37, DateTimeKind.Utc),
                UNCFFileName = "ZW_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ML v2",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 167,
                Country = "ML",
                UNCFFileURL = "https://drive.google.com/file/d/15AKG2jpCux2oDeR0omm0D_77C2QJZvam/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 13, 43, 30, 123, DateTimeKind.Utc),
                UNCFFileName = "ML_2023-09-17_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "GH v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 168,
                Country = "GH",
                UNCFFileURL = "https://drive.google.com/file/d/1HXcAaVEKGh5X8pQoS_WiSguplDy9tt1t/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 25, 15, 50, 2, 607, DateTimeKind.Utc),
                UNCFFileName = "GH_2023-10-25_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BF v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 169,
                Country = "BF",
                UNCFFileURL = "https://drive.google.com/file/d/1w-eRGwYuQEFHNPwGwW4oJI3wRLDrRUsg/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 403, DateTimeKind.Utc),
                UNCFFileName = "BF_2023-10-26_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "PH v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 170,
                Country = "PH",
                UNCFFileURL = "https://drive.google.com/file/d/1SEF4m2TLhis57sNaWpZyy85Ue_9f6JYb/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 10, 26, 15, 17, 31, 300, DateTimeKind.Utc),
                UNCFFileName = "PH_2023-10-26_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "RS v3",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 171,
                Country = "RS",
                UNCFFileURL = "https://drive.google.com/file/d/1kEdlOkHMp3buuBjuGdAWY9IMv-Eya-eM/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 3,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 37, 477, DateTimeKind.Utc),
                UNCFFileName = "RS_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "ML v3",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 172,
                Country = "ML",
                UNCFFileURL = "https://drive.google.com/file/d/15AKG2jpCux2oDeR0omm0D_77C2QJZvam/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 3,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 15, 43, 0, 133, DateTimeKind.Utc),
                UNCFFileName = "ML_2023-09-17_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TD v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 173,
                Country = "TD",
                UNCFFileURL = "https://drive.google.com/file/d/1R4BMbhtH3KCFCW7xJCsas-wC1_i51P7v/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 16, 9, 33, 790, DateTimeKind.Utc),
                UNCFFileName = "TD_2023-11-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MU v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 174,
                Country = "MU",
                UNCFFileURL = "https://drive.google.com/file/d/1oZGNVRzCSktgUunUdkKE92hUIBRlZvNK/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 16, 14, 30, 547, DateTimeKind.Utc),
                UNCFFileName = "MU_2023-11-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "GM v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 175,
                Country = "GM",
                UNCFFileURL = "https://drive.google.com/file/d/17vStoZUHtbFStKYiynWRr2MqV3IApntY/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 16, 26, 47, 890, DateTimeKind.Utc),
                UNCFFileName = "GM_2023-11-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "KH v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 176,
                Country = "KH",
                UNCFFileURL = "https://drive.google.com/file/d/1UftJYw32jpt7vtAnwrCeXqlR_P73VceL/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 13, 16, 32, 39, 27, DateTimeKind.Utc),
                UNCFFileName = "KH_2023-11-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "AF v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 177,
                Country = "AF",
                UNCFFileURL = "https://drive.google.com/file/d/1wCQoXAbCpSLuCTdjjlCyOdexCUSbvRpA/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 21, 11, 29, 24, 910, DateTimeKind.Utc),
                UNCFFileName = "AF_2023-11-21_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SC v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 178,
                Country = "SC",
                UNCFFileURL = "https://drive.google.com/file/d/1GH5nQB9h8DZhkXxInaopORqWdeznOQ_p/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 21, 11, 35, 29, 660, DateTimeKind.Utc),
                UNCFFileName = "SC_2023-11-21_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MA v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 179,
                Country = "MA",
                UNCFFileURL = "https://drive.google.com/file/d/1VRPXx6LmFDEeJXCGQZJ6iuWFp8pZjEmx/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 30, 15, 57, 7, 933, DateTimeKind.Utc),
                UNCFFileName = "MA_2023-11-30_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "LS v3",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 180,
                Country = "LS",
                UNCFFileURL = "https://drive.google.com/file/d/1VUo-z7IZIgkukjJdUQp0O1M77W71rJ-8/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 3,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 30, 16, 31, 47, 747, DateTimeKind.Utc),
                UNCFFileName = "LS_2023-11-30_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BT v3",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 181,
                Country = "BT",
                UNCFFileURL = "https://drive.google.com/file/d/1Qh4DM6cL1GLfl3aPsAe4V7ThJyzyjV6F/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 3,
                UNCFLastUpdatedDate = new DateTime(2023, 11, 30, 16, 51, 53, 193, DateTimeKind.Utc),
                UNCFFileName = "BT_2023-11-30_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SD v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 182,
                Country = "SD",
                UNCFFileURL = "https://drive.google.com/file/d/1yDoarGgM1BEP0JqcaLg3KQ403mgTFW3O/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 12, 12, 13, 44, 32, 963, DateTimeKind.Utc),
                UNCFFileName = "SD_2023-12-12_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "UA v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 183,
                Country = "UA",
                UNCFFileURL = "https://drive.google.com/file/d/1us711b0bX0D_xQMUy_QEh_wrXv3GcW76/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 12, 13, 13, 45, 9, 743, DateTimeKind.Utc),
                UNCFFileName = "UA_2023-12-13_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "TZ v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 184,
                Country = "TZ",
                UNCFFileURL = "https://drive.google.com/file/d/1DZiV8lKS7kyyXljOAz5Zbvb6yDUSnwI7/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2023, 12, 15, 14, 24, 7, 413, DateTimeKind.Utc),
                UNCFFileName = "TZ_2023-12-15_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BR v1",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 185,
                Country = "BR",
                UNCFFileURL = "https://drive.google.com/file/d/1LYKej7a1xD9H-ShTvwpgUtauaK7SrWFU/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2024, 6, 3, 13, 48, 15, 950, DateTimeKind.Utc),
                UNCFFileName = "BR_2024-06-03_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "BR v2",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 186,
                Country = "BR",
                UNCFFileURL = "https://drive.google.com/file/d/1IGPEqyh3oWQRppSre3U5FQYMj1EMjFW-/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 2,
                UNCFLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 570, DateTimeKind.Utc),
                UNCFFileName = "BR_2024-06-03_ND2.csv"
            },
            new UNCFMetadata
            {
                Name = "GN v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 187,
                Country = "GN",
                UNCFFileURL = "https://drive.google.com/file/d/1Ei9xsQ_W1DeLg-VbRnyhO33yDV3ChSo2/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2024, 6, 4, 16, 19, 23, 770, DateTimeKind.Utc),
                UNCFFileName = "GN_2024-06-04_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "CO v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 188,
                Country = "CO",
                UNCFFileURL = "https://drive.google.com/file/d/1HCFB7EkiwKqzEqNh3ALZkvn6hhZi6FH3/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2024, 8, 5, 12, 24, 25, 263, DateTimeKind.Utc),
                UNCFFileName = "CO_2024-08-05_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SL v3",
                Status = EntityStatus.Inactive,
                IsDeleted = false,
                UNCFMetadataId = 189,
                Country = "SL",
                UNCFFileURL = "https://drive.google.com/file/d/1BI9XcKrEAnQ3t2Bp6v7sQUN7Tw754pS9/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 3,
                UNCFLastUpdatedDate = new DateTime(2024, 8, 19, 16, 43, 54, 197, DateTimeKind.Utc),
                UNCFFileName = "SL_2023-09-16_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "SL v4",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 190,
                Country = "SL",
                UNCFFileURL = "https://drive.google.com/file/d/1ogfB_BVR2_P81r-AMQ-anG94vsJOHAUA/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 4,
                UNCFLastUpdatedDate = new DateTime(2024, 8, 19, 16, 44, 23, 130, DateTimeKind.Utc),
                UNCFFileName = "SL_2024-08-19_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "MW v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 191,
                Country = "MW",
                UNCFFileURL = "https://drive.google.com/file/d/1QkwjmRQhxUuPnL6DCYyoKw9ywVO7jzkM/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2024, 9, 2, 15, 1, 24, 180, DateTimeKind.Utc),
                UNCFFileName = "MW_2024-09-02_ND.csv"
            },
            new UNCFMetadata
            {
                Name = "AO v1",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFMetadataId = 192,
                Country = "AO",
                UNCFFileURL = "https://drive.google.com/file/d/187DEV1uC54DI6MgnWAsJPQlFCJcQ07Wd/view?usp=drive_web",
                UNCooperationFrameworkVersionNo = 1,
                UNCFLastUpdatedDate = new DateTime(2025, 1, 14, 14, 43, 59, 727, DateTimeKind.Utc),
                UNCFFileName = "AO_2025-01-14_GM.csv"
            }
        };
    }
}
