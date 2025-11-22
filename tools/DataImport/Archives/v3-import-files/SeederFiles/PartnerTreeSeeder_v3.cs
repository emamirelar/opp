using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class PartnerTreeSeeder_v3
    {
        public static async Task SeedPartnerTreeAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting PartnerTree seeding process (v3)...");
            
            int skippedCount = 0;
            int createdCount = 0;
            var createdRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: PRIVATE_SECTOR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'PRIVATE_SECTOR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PRIVATE_SECTOR",
                            Name = "Private Sector",
                            Description = "Private Sector",
                            Type = "Level_1",
                            Parent = null,
                            PartnerCategoryCode = "PRIVATE_SECTOR",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'PRIVATE_SECTOR' - Private Sector");
                        createdCount++;
                    }
                }
                
                // Record 2: CC001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CC001");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CC001' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CC001",
                            Name = "Cygnum Capital",
                            Description = "Cygnum Capital Asset Management",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CC001",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CC001' - Cygnum Capital");
                        createdCount++;
                    }
                }
                
                // Record 3: OTHER
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OTHER' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER",
                            Name = "Other",
                            Description = "Other",
                            Type = "Level_1",
                            Parent = null,
                            PartnerCategoryCode = "OTHER",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER' - Other");
                        createdCount++;
                    }
                }
                
                // Record 4: COG01
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "COG01");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'COG01' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "COG01",
                            Name = "Comité Olimpico Guatemalteco",
                            Description = "Comité Olimpico Guatemalteco (COG)",
                            Type = "Level_2",
                            Parent = "OTHER",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "COG01",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'COG01' - Comité Olimpico Guatemalteco");
                        createdCount++;
                    }
                }
                
                // Record 5: MULTILATERAL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MULTILATERAL");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MULTILATERAL' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MULTILATERAL",
                            Name = "Multilateral",
                            Description = "Multilateral",
                            Type = "Level_1",
                            Parent = null,
                            PartnerCategoryCode = "MULTILATERAL",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MULTILATERAL' - Multilateral");
                        createdCount++;
                    }
                }
                
                // Record 6: IFI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IFI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IFI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IFI",
                            Name = "IFI",
                            Description = "IFI International Financial Institutions",
                            Type = "Level_2",
                            Parent = "MULTILATERAL",
                            PartnerCategoryCode = "IFI",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IFI' - IFI");
                        createdCount++;
                    }
                }
                
                // Record 7: REG_OTH_FI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'REG_OTH_FI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REG_OTH_FI",
                            Name = "Reg & other Financial Insitutions",
                            Description = "Regional and other Financial Insitutions",
                            Type = "Level_3",
                            Parent = "IFI",
                            PartnerCategoryCode = "REG_OTH_FI",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'REG_OTH_FI' - Reg & other Financial Insitutions");
                        createdCount++;
                    }
                }
                
                // Record 8: CAF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CAF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CAF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CAF",
                            Name = "CAF",
                            Description = "CAF Development Bank of Latin America",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CAF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CAF' - CAF");
                        createdCount++;
                    }
                }
                
                // Record 9: IMF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IMF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IMF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IMF",
                            Name = "IMF",
                            Description = "IMF International Monetary Fund",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IMF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IMF' - IMF");
                        createdCount++;
                    }
                }
                
                // Record 10: AFDB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AFDB");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'AFDB' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AFDB",
                            Name = "AFDB",
                            Description = "AfDB African Development Bank",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "AFDB",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'AFDB' - AFDB");
                        createdCount++;
                    }
                }
                
                // Record 11: ADB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ADB");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ADB' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ADB",
                            Name = "ADB",
                            Description = "ADB Asian Development Bank",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ADB",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ADB' - ADB");
                        createdCount++;
                    }
                }
                
                // Record 12: CDB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CDB");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CDB' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CDB",
                            Name = "CDB",
                            Description = "CDB Caribbean Development Bank",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CDB",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CDB' - CDB");
                        createdCount++;
                    }
                }
                
                // Record 13: CFC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CFC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CFC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CFC",
                            Name = "CFC",
                            Description = "CFC Common Fund for Commodities",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CFC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CFC' - CFC");
                        createdCount++;
                    }
                }
                
                // Record 14: EBRD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EBRD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EBRD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EBRD",
                            Name = "EBRD",
                            Description = "EBRD European Bank for Reconstruction and Development",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EBRD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EBRD' - EBRD");
                        createdCount++;
                    }
                }
                
                // Record 15: IsDB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IsDB");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IsDB' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IsDB",
                            Name = "IsDB",
                            Description = "IsDB Islamic Development Bank",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IsDB",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IsDB' - IsDB");
                        createdCount++;
                    }
                }
                
                // Record 16: AFESD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AFESD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'AFESD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AFESD",
                            Name = "AFESD",
                            Description = "AFESD Arab Fund for Economic and Social Development",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "AFESD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'AFESD' - AFESD");
                        createdCount++;
                    }
                }
                
                // Record 17: AIIB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AIIB");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'AIIB' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AIIB",
                            Name = "AIIB",
                            Description = "AIIB Asian Infrastructure Investment Bank",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "AIIB",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'AIIB' - AIIB");
                        createdCount++;
                    }
                }
                
                // Record 18: OFID
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OFID");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OFID' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OFID",
                            Name = "OFID",
                            Description = "OFID OPEC Fund for International Development",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OFID",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OFID' - OFID");
                        createdCount++;
                    }
                }
                
                // Record 19: BOAD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BOAD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'BOAD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BOAD",
                            Name = "BOAD",
                            Description = "West African Development Bank",
                            Type = "Level_4",
                            Parent = "REG_OTH_FI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "BOAD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'BOAD' - BOAD");
                        createdCount++;
                    }
                }
                
                // Record 20: MAI001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MAI001");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MAI001' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MAI001",
                            Name = "Maisha",
                            Description = "Maisha",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MAI001",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MAI001' - Maisha");
                        createdCount++;
                    }
                }
                
                // Record 22: MPI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MPI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MPI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MPI",
                            Name = "MPI",
                            Description = "Multi-partner initiatives",
                            Type = "Level_2",
                            Parent = "MULTILATERAL",
                            PartnerCategoryCode = "MPI",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MPI' - MPI");
                        createdCount++;
                    }
                }
                
                // Record 23: EIF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EIF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EIF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EIF",
                            Name = "EIF",
                            Description = "EIF Enhanced Integrated Framework",
                            Type = "Level_3",
                            Parent = "MPI",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EIF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EIF' - EIF");
                        createdCount++;
                    }
                }
                
                // Record 24: NGO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NGO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'NGO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NGO",
                            Name = "NGO",
                            Description = "Non-governmental Organizations",
                            Type = "Level_1",
                            Parent = null,
                            PartnerCategoryCode = "NGO",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'NGO' - NGO");
                        createdCount++;
                    }
                }
                
                // Record 25: NEH001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NEH001");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'NEH001' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NEH001",
                            Name = "Nehemia",
                            Description = "Nehemia",
                            Type = "Level_2",
                            Parent = "NGO",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "NEH001",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'NEH001' - Nehemia");
                        createdCount++;
                    }
                }
                
                // Record 26: GOVERNMENT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'GOVERNMENT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "GOVERNMENT",
                            Name = "Government",
                            Description = "Government",
                            Type = "Level_1",
                            Parent = null,
                            PartnerCategoryCode = "GOVERNMENT",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'GOVERNMENT' - Government");
                        createdCount++;
                    }
                }
                
                // Record 27: NON_OECD_DAC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NON_OECD_DAC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'NON_OECD_DAC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NON_OECD_DAC",
                            Name = "Gov: Non-OECD/DAC",
                            Description = "Non-OECD/DAC Government",
                            Type = "Level_2",
                            Parent = "GOVERNMENT",
                            PartnerCategoryCode = "NON_OECD_DAC",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'NON_OECD_DAC' - Gov: Non-OECD/DAC");
                        createdCount++;
                    }
                }
                
                // Record 28: PNG001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PNG001");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'PNG001' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PNG001",
                            Name = "Papua New Guinea",
                            Description = "Papua New Guinea",
                            Type = "Level_3",
                            Parent = "NON_OECD_DAC",
                            PartnerCategoryCode = "PNG001",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'PNG001' - Papua New Guinea");
                        createdCount++;
                    }
                }
                
                // Record 29: PAR001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PAR001");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'PAR001' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PAR001",
                            Name = "Parexel",
                            Description = "Parexel",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "PAR001",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'PAR001' - Parexel");
                        createdCount++;
                    }
                }
                
                // Record 30: REG_OTH_INGO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_INGO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'REG_OTH_INGO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REG_OTH_INGO",
                            Name = "Regional & Other IGO",
                            Description = "Regional and other Intergovernmental Organizations",
                            Type = "Level_2",
                            Parent = "MULTILATERAL",
                            PartnerCategoryCode = "REG_OTH_INGO",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'REG_OTH_INGO' - Regional & Other IGO");
                        createdCount++;
                    }
                }
                
                // Record 31: EU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EU",
                            Name = "EU",
                            Description = "EU European Union",
                            Type = "Level_3",
                            Parent = "REG_OTH_INGO",
                            PartnerCategoryCode = "EU",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EU' - EU");
                        createdCount++;
                    }
                }
                
                // Record 32: EC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EC",
                            Name = "EC",
                            Description = "EC European Commission",
                            Type = "Level_4",
                            Parent = "EU",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EC' - EC");
                        createdCount++;
                    }
                }
                
                // Record 33: AU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'AU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AU",
                            Name = "AU",
                            Description = "AU African Union",
                            Type = "Level_3",
                            Parent = "REG_OTH_INGO",
                            PartnerCategoryCode = "AU",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'AU' - AU");
                        createdCount++;
                    }
                }
                
                // Record 34: UNAMID
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAMID");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNAMID' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAMID",
                            Name = "UNAMID",
                            Description = "UNAMID African Union-United Nations Hybrid Operation in Darfur",
                            Type = "Level_4",
                            Parent = "AU",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNAMID",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAMID' - UNAMID");
                        createdCount++;
                    }
                }
                
                // Record 35: EBY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EBY");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EBY' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EBY",
                            Name = "EBY",
                            Description = "EBY Entidad Binacional Yacyretá",
                            Type = "Level_3",
                            Parent = "REG_OTH_INGO",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EBY",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EBY' - EBY");
                        createdCount++;
                    }
                }
                
                // Record 36: EU_DG_MENA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EU_DG_MENA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EU_DG_MENA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EU_DG_MENA",
                            Name = "EU DG MENA",
                            Description = "EU DG MENA, Directorate-General for the Middle East, North Africa and the Gulf",
                            Type = "Level_4",
                            Parent = "EU",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EU_DG_MENA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EU_DG_MENA' - EU DG MENA");
                        createdCount++;
                    }
                }
                
                // Record 37: EU_DG_CLIMA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EU_DG_CLIMA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EU_DG_CLIMA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EU_DG_CLIMA",
                            Name = "EU DG CLIMA",
                            Description = "EU DG CLIMA, Directorate-General for Climate Action",
                            Type = "Level_4",
                            Parent = "EU",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EU_DG_CLIMA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EU_DG_CLIMA' - EU DG CLIMA");
                        createdCount++;
                    }
                }
                
                // Record 38: ACADEMIC_TRAINING_RESEARC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ACADEMIC_TRAINING_RESEARC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ACADEMIC_TRAINING_RESEARC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ACADEMIC_TRAINING_RESEARC",
                            Name = "Academic, Training and Research",
                            Description = "Academic, Training and Research",
                            Type = "Level_1",
                            Parent = null,
                            PartnerCategoryCode = "ACADEMIC_TRAINING_RESEARC",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ACADEMIC_TRAINING_RESEARC' - Academic, Training and Research");
                        createdCount++;
                    }
                }
                
                // Record 39: UCD001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UCD001");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UCD001' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UCD001",
                            Name = "UC Davis",
                            Description = "UC Davis",
                            Type = "Level_2",
                            Parent = "ACADEMIC_TRAINING_RESEARC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UCD001",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UCD001' - UC Davis");
                        createdCount++;
                    }
                }
                
                // Record 40: UNITED_NATIONS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNITED_NATIONS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNITED_NATIONS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNITED_NATIONS",
                            Name = "UN",
                            Description = "United Nations",
                            Type = "Level_2",
                            Parent = "MULTILATERAL",
                            PartnerCategoryCode = "UNITED_NATIONS",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNITED_NATIONS' - UN");
                        createdCount++;
                    }
                }
                
                // Record 41: UN_INTER_POOLED_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_INTER_POOLED_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_INTER_POOLED_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_INTER_POOLED_FUND",
                            Name = "UN inter-agency pooled funds incl. JPs",
                            Description = "United Nations inter-agency pooled funds incl. Joint Programmes",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "UN_INTER_POOLED_FUND",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_INTER_POOLED_FUND' - UN inter-agency pooled funds incl. JPs");
                        createdCount++;
                    }
                }
                
                // Record 42: SSHF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SSHF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SSHF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SSHF",
                            Name = "SSHF",
                            Description = "SSHF South Sudan Common Humanitarian Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SSHF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SSHF' - SSHF");
                        createdCount++;
                    }
                }
                
                // Record 43: EBOLA_RESPONSE_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EBOLA_RESPONSE_MPTF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'EBOLA_RESPONSE_MPTF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EBOLA_RESPONSE_MPTF",
                            Name = "Ebola Response MPTF",
                            Description = "Ebola Response MPTF",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EBOLA_RESPONSE_MPTF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'EBOLA_RESPONSE_MPTF' - Ebola Response MPTF");
                        createdCount++;
                    }
                }
                
                // Record 44: SYRIA_EMERGENCY_RESPONSE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SYRIA_EMERGENCY_RESPONSE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SYRIA_EMERGENCY_RESPONSE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SYRIA_EMERGENCY_RESPONSE",
                            Name = "Syria Emergency Response Fund",
                            Description = "Syria Emergency Response Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SYRIA_EMERGENCY_RESPONSE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SYRIA_EMERGENCY_RESPONSE' - Syria Emergency Response Fund");
                        createdCount++;
                    }
                }
                
                // Record 45: SOMALIA_UN_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SOMALIA_UN_MPTF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SOMALIA_UN_MPTF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SOMALIA_UN_MPTF",
                            Name = "Somalia UN MPTF",
                            Description = "UN Multi-Partner Trust Fund for Somalia (Somalia UN MPTF)",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SOMALIA_UN_MPTF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SOMALIA_UN_MPTF' - Somalia UN MPTF");
                        createdCount++;
                    }
                }
                
                // Record 46: UNDF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDF",
                            Name = "UNDF",
                            Description = "UNDF United Nations Fund for Recovery Reconstruction and Development in Darfur",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDF' - UNDF");
                        createdCount++;
                    }
                }
                
                // Record 47: UN_GENERAL_TRUST_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_GENERAL_TRUST_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_GENERAL_TRUST_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_GENERAL_TRUST_FUND",
                            Name = "UN General Trust Fund",
                            Description = "UN General Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_GENERAL_TRUST_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_GENERAL_TRUST_FUND' - UN General Trust Fund");
                        createdCount++;
                    }
                }
                
                // Record 48: CERF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CERF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CERF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CERF",
                            Name = "CERF",
                            Description = "CERF Central Emergency Response Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CERF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CERF' - CERF");
                        createdCount++;
                    }
                }
                
                // Record 49: UNPBF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNPBF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNPBF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNPBF",
                            Name = "UNPBF",
                            Description = "UNPBF United Nations Peacebuilding Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNPBF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNPBF' - UNPBF");
                        createdCount++;
                    }
                }
                
                // Record 50: UNVFTC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNVFTC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNVFTC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNVFTC",
                            Name = "UNVFTC",
                            Description = "UNVFTC United Nations Voluntary Fund for Technical Co-operation in the Field of Human Rights",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNVFTC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNVFTC' - UNVFTC");
                        createdCount++;
                    }
                }
                
                // Record 51: UNVFVT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNVFVT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNVFVT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNVFVT",
                            Name = "UNVFVT",
                            Description = "UNVFVT United Nations Voluntary Fund for Victims of Torture",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNVFVT",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNVFVT' - UNVFVT");
                        createdCount++;
                    }
                }
                
                // Record 52: UNVFD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNVFD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNVFD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNVFD",
                            Name = "UNVFD",
                            Description = "UNVFD United Nations Voluntary Fund on Disability",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNVFD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNVFD' - UNVFD");
                        createdCount++;
                    }
                }
                
                // Record 53: UNDEF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDEF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDEF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDEF",
                            Name = "UNDEF",
                            Description = "UNDEF United Nations Democracy Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDEF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDEF' - UNDEF");
                        createdCount++;
                    }
                }
                
                // Record 54: UNFIP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFIP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNFIP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFIP",
                            Name = "UNFIP",
                            Description = "UNFIP United Nations Fund for International Partnerships",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNFIP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFIP' - UNFIP");
                        createdCount++;
                    }
                }
                
                // Record 55: UN-WATER
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-WATER");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN-WATER' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-WATER",
                            Name = "UN-Water",
                            Description = "UN-Water Inter-agency Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN-WATER",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-WATER' - UN-Water");
                        createdCount++;
                    }
                }
                
                // Record 56: ALBANIA_ONE_UNCF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ALBANIA_ONE_UNCF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ALBANIA_ONE_UNCF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ALBANIA_ONE_UNCF",
                            Name = "Albania One UNCF",
                            Description = "Albania One UN Coherence Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ALBANIA_ONE_UNCF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ALBANIA_ONE_UNCF' - Albania One UNCF");
                        createdCount++;
                    }
                }
                
                // Record 57: BHUTAN_UNCF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BHUTAN_UNCF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'BHUTAN_UNCF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BHUTAN_UNCF",
                            Name = "Bhutan UNCF",
                            Description = "Bhutan UN Country Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "BHUTAN_UNCF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'BHUTAN_UNCF' - Bhutan UNCF");
                        createdCount++;
                    }
                }
                
                // Record 58: BOTSWANA_UNCF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BOTSWANA_UNCF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'BOTSWANA_UNCF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BOTSWANA_UNCF",
                            Name = "Botswana UNCF",
                            Description = "Botswana UN Country Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "BOTSWANA_UNCF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'BOTSWANA_UNCF' - Botswana UNCF");
                        createdCount++;
                    }
                }
                
                // Record 59: CAPE_VERDE_TRANSITION_FU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CAPE_VERDE_TRANSITION_FU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CAPE_VERDE_TRANSITION_FU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CAPE_VERDE_TRANSITION_FU",
                            Name = "Cape Verde Transition Fund",
                            Description = "Cape Verde Transition Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CAPE_VERDE_TRANSITION_FU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CAPE_VERDE_TRANSITION_FU' - Cape Verde Transition Fund");
                        createdCount++;
                    }
                }
                
                // Record 60: CAR_HF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CAR_HF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CAR_HF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CAR_HF",
                            Name = "CAR HF",
                            Description = "Central African Republic Common Humanitarian Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CAR_HF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CAR_HF' - CAR HF");
                        createdCount++;
                    }
                }
                
                // Record 61: CFIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CFIA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CFIA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CFIA",
                            Name = "CFIA",
                            Description = "CFIA United Nations Central Fund for Influenza Action",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CFIA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CFIA' - CFIA");
                        createdCount++;
                    }
                }
                
                // Record 62: CBA_CC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CBA_CC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CBA_CC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CBA_CC",
                            Name = "CBA CC",
                            Description = "Community-based Based Adaptation to Climate Change",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CBA_CC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CBA_CC' - CBA CC");
                        createdCount++;
                    }
                }
                
                // Record 63: COMOROS_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "COMOROS_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'COMOROS_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "COMOROS_ONE_UN_FUND",
                            Name = "Comoros One UN Fund",
                            Description = "Comoros One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "COMOROS_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'COMOROS_ONE_UN_FUND' - Comoros One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 64: DCPSF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DCPSF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'DCPSF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DCPSF",
                            Name = "DCPSF",
                            Description = "DCPSF Darfur Community Peace and Stability Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "DCPSF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'DCPSF' - DCPSF");
                        createdCount++;
                    }
                }
                
                // Record 65: DRC_POOLED_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DRC_POOLED_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'DRC_POOLED_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DRC_POOLED_FUND",
                            Name = "DRC Pooled Fund",
                            Description = "DRC Pooled Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "DRC_POOLED_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'DRC_POOLED_FUND' - DRC Pooled Fund");
                        createdCount++;
                    }
                }
                
                // Record 66: DRC_STABILIZATION_AND_RE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DRC_STABILIZATION_AND_RE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'DRC_STABILIZATION_AND_RE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DRC_STABILIZATION_AND_RE",
                            Name = "DRC Stabilization and Recovery",
                            Description = "DRC Stabilization and Recovery",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "DRC_STABILIZATION_AND_RE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'DRC_STABILIZATION_AND_RE' - DRC Stabilization and Recovery");
                        createdCount++;
                    }
                }
                
                // Record 67: ETHIOPIA_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ETHIOPIA_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ETHIOPIA_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ETHIOPIA_ONE_UN_FUND",
                            Name = "Ethiopia One UN Fund",
                            Description = "Ethiopia One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ETHIOPIA_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ETHIOPIA_ONE_UN_FUND' - Ethiopia One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 68: HRM_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "HRM_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'HRM_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "HRM_FUND",
                            Name = "HRM Fund",
                            Description = "Human Rights Mainstreaming Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "HRM_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'HRM_FUND' - HRM Fund");
                        createdCount++;
                    }
                }
                
                // Record 69: INDONESIA_DR_TF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "INDONESIA_DR_TF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'INDONESIA_DR_TF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "INDONESIA_DR_TF",
                            Name = "Indonesia DR TF",
                            Description = "Indonesia Disaster Recovery Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "INDONESIA_DR_TF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'INDONESIA_DR_TF' - Indonesia DR TF");
                        createdCount++;
                    }
                }
                
                // Record 70: IRAQ_UNDAF_TRUST_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IRAQ_UNDAF_TRUST_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IRAQ_UNDAF_TRUST_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IRAQ_UNDAF_TRUST_FUND",
                            Name = "Iraq UNDAF Trust Fund",
                            Description = "Iraq UNDAF Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IRAQ_UNDAF_TRUST_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IRAQ_UNDAF_TRUST_FUND' - Iraq UNDAF Trust Fund");
                        createdCount++;
                    }
                }
                
                // Record 71: JP_ARMED_VIOLENCE_PREVEN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_ARMED_VIOLENCE_PREVEN");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_ARMED_VIOLENCE_PREVEN' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_ARMED_VIOLENCE_PREVEN",
                            Name = "JP Armed Violence Prevention",
                            Description = "JP Armed Violence Prevention",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_ARMED_VIOLENCE_PREVEN",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_ARMED_VIOLENCE_PREVEN' - JP Armed Violence Prevention");
                        createdCount++;
                    }
                }
                
                // Record 72: JP_BANGLADESH_LGSP–LIC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_BANGLADESH_LGSP–LIC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_BANGLADESH_LGSP–LIC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_BANGLADESH_LGSP–LIC",
                            Name = "JP Bangladesh LGSP–LIC",
                            Description = "JP LGSP-LIC Bangladesh Local Governance Support Project – Learning and Innovation Component",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_BANGLADESH_LGSP–LIC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_BANGLADESH_LGSP–LIC' - JP Bangladesh LGSP–LIC");
                        createdCount++;
                    }
                }
                
                // Record 73: JP_CHAD_DIS_SECURITY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_CHAD_DIS_SECURITY");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_CHAD_DIS_SECURITY' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_CHAD_DIS_SECURITY",
                            Name = "JP Chad DIS Security",
                            Description = "JP Chad DIS Security",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_CHAD_DIS_SECURITY",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_CHAD_DIS_SECURITY' - JP Chad DIS Security");
                        createdCount++;
                    }
                }
                
                // Record 74: JP_DRC_MICROFINANCE_II
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_DRC_MICROFINANCE_II");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_DRC_MICROFINANCE_II' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_DRC_MICROFINANCE_II",
                            Name = "JP DRC Microfinance II",
                            Description = "JP DRC Microfinance II",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_DRC_MICROFINANCE_II",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_DRC_MICROFINANCE_II' - JP DRC Microfinance II");
                        createdCount++;
                    }
                }
                
                // Record 75: JP_DRC_SECURITY_SECT_REF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_DRC_SECURITY_SECT_REF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_DRC_SECURITY_SECT_REF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_DRC_SECURITY_SECT_REF",
                            Name = "JP DRC Security Sect Reform",
                            Description = "JP DRC Security Sect Reform",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_DRC_SECURITY_SECT_REF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_DRC_SECURITY_SECT_REF' - JP DRC Security Sect Reform");
                        createdCount++;
                    }
                }
                
                // Record 76: JP_GUATEMALA_MAYA_PROGRA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_GUATEMALA_MAYA_PROGRA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_GUATEMALA_MAYA_PROGRA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_GUATEMALA_MAYA_PROGRA",
                            Name = "JP Guatemala Maya Programme",
                            Description = "JP Guatemala Maya Programme",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_GUATEMALA_MAYA_PROGRA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_GUATEMALA_MAYA_PROGRA' - JP Guatemala Maya Programme");
                        createdCount++;
                    }
                }
                
                // Record 77: JP_GUATEMALA_RURAL_DEV
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_GUATEMALA_RURAL_DEV");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_GUATEMALA_RURAL_DEV' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_GUATEMALA_RURAL_DEV",
                            Name = "JP Guatemala Rural Dev",
                            Description = "JP Guatemala Rural Dev",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_GUATEMALA_RURAL_DEV",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_GUATEMALA_RURAL_DEV' - JP Guatemala Rural Dev");
                        createdCount++;
                    }
                }
                
                // Record 78: JP_KAZAKHSTAN_INNOV_APRC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_KAZAKHSTAN_INNOV_APRC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_KAZAKHSTAN_INNOV_APRC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_KAZAKHSTAN_INNOV_APRC",
                            Name = "JP Kazakhstan Innov Aprch RPSS",
                            Description = "JP Kazakhstan Innov Aprch RPSS",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_KAZAKHSTAN_INNOV_APRC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_KAZAKHSTAN_INNOV_APRC' - JP Kazakhstan Innov Aprch RPSS");
                        createdCount++;
                    }
                }
                
                // Record 79: JP_KENYA_HIV_AND_AIDS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_KENYA_HIV_AND_AIDS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_KENYA_HIV_AND_AIDS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_KENYA_HIV_AND_AIDS",
                            Name = "JP Kenya HIV and AIDS",
                            Description = "JP Kenya HIV and AIDS",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_KENYA_HIV_AND_AIDS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_KENYA_HIV_AND_AIDS' - JP Kenya HIV and AIDS");
                        createdCount++;
                    }
                }
                
                // Record 80: JP_KOSOVO_DOMESTIC_VIOLE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_KOSOVO_DOMESTIC_VIOLE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_KOSOVO_DOMESTIC_VIOLE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_KOSOVO_DOMESTIC_VIOLE",
                            Name = "JP Kosovo Domestic Violence",
                            Description = "JP Kosovo Domestic Violence",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_KOSOVO_DOMESTIC_VIOLE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_KOSOVO_DOMESTIC_VIOLE' - JP Kosovo Domestic Violence");
                        createdCount++;
                    }
                }
                
                // Record 81: JP_LAO_GOVERN/PUBLIC_ADM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_LAO_GOVERN/PUBLIC_ADM");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_LAO_GOVERN/PUBLIC_ADM' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_LAO_GOVERN/PUBLIC_ADM",
                            Name = "JP Lao Govern/Public Admin",
                            Description = "JP Lao Governance and Public Administration Reform",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_LAO_GOVERN/PUBLIC_ADM",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_LAO_GOVERN/PUBLIC_ADM' - JP Lao Govern/Public Admin");
                        createdCount++;
                    }
                }
                
                // Record 82: JP_LIBERIA_FOOD_SECURITY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_LIBERIA_FOOD_SECURITY");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_LIBERIA_FOOD_SECURITY' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_LIBERIA_FOOD_SECURITY",
                            Name = "JP Liberia Food Security",
                            Description = "JP Liberia Food Security",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_LIBERIA_FOOD_SECURITY",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_LIBERIA_FOOD_SECURITY' - JP Liberia Food Security");
                        createdCount++;
                    }
                }
                
                // Record 83: JP_LIBERIA_GENDER_EQUALI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_LIBERIA_GENDER_EQUALI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_LIBERIA_GENDER_EQUALI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_LIBERIA_GENDER_EQUALI",
                            Name = "JP Liberia Gender Equality",
                            Description = "JP Liberia Gender Equality",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_LIBERIA_GENDER_EQUALI",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_LIBERIA_GENDER_EQUALI' - JP Liberia Gender Equality");
                        createdCount++;
                    }
                }
                
                // Record 84: JP_MALI_AGRO_PASTORAL_PR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_MALI_AGRO_PASTORAL_PR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_MALI_AGRO_PASTORAL_PR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_MALI_AGRO_PASTORAL_PR",
                            Name = "JP Mali Agro Pastoral Products",
                            Description = "JP Mali Agro Pastoral Products",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_MALI_AGRO_PASTORAL_PR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_MALI_AGRO_PASTORAL_PR' - JP Mali Agro Pastoral Products");
                        createdCount++;
                    }
                }
                
                // Record 85: JP_MOLDOVA_JILDP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_MOLDOVA_JILDP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_MOLDOVA_JILDP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_MOLDOVA_JILDP",
                            Name = "JP Moldova JILDP",
                            Description = "JP Moldova Integrated Local Development Programme",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_MOLDOVA_JILDP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_MOLDOVA_JILDP' - JP Moldova JILDP");
                        createdCount++;
                    }
                }
                
                // Record 86: JP_NEPAL_LGCDP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_NEPAL_LGCDP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_NEPAL_LGCDP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_NEPAL_LGCDP",
                            Name = "JP Nepal LGCDP",
                            Description = "JP Nepal LGCDP Local Governance and Community Development Programme",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_NEPAL_LGCDP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_NEPAL_LGCDP' - JP Nepal LGCDP");
                        createdCount++;
                    }
                }
                
                // Record 87: JP_SERBIA_SCILD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_SERBIA_SCILD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_SERBIA_SCILD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_SERBIA_SCILD",
                            Name = "JP Serbia SCILD",
                            Description = "JP Serbia SCILD Strengthening Capacity for Inclusive Local Development",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_SERBIA_SCILD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_SERBIA_SCILD' - JP Serbia SCILD");
                        createdCount++;
                    }
                }
                
                // Record 88: JP_SOLOMON_ISLANDS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_SOLOMON_ISLANDS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_SOLOMON_ISLANDS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_SOLOMON_ISLANDS",
                            Name = "JP Solomon Islands",
                            Description = "JP Solomon Islands PGSP Provincial Governance Strengthening Programme",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_SOLOMON_ISLANDS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_SOLOMON_ISLANDS' - JP Solomon Islands");
                        createdCount++;
                    }
                }
                
                // Record 89: JP_SOMALIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_SOMALIA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_SOMALIA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_SOMALIA",
                            Name = "JP Somalia",
                            Description = "JP Somalia Local Governance and Decentralized Service Delivery",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_SOMALIA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_SOMALIA' - JP Somalia");
                        createdCount++;
                    }
                }
                
                // Record 90: JP_MACEDONIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_MACEDONIA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_MACEDONIA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_MACEDONIA",
                            Name = "JP Macedonia",
                            Description = "JP TFYR SNC PDV Macedonia Strengthening National Capacities to Prevent Domestic Violence",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_MACEDONIA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_MACEDONIA' - JP Macedonia");
                        createdCount++;
                    }
                }
                
                // Record 91: JP_TIMOR-LESTE_INFUSE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_TIMOR-LESTE_INFUSE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_TIMOR-LESTE_INFUSE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_TIMOR-LESTE_INFUSE",
                            Name = "JP Timor-Leste INFUSE",
                            Description = "JP Timor-Leste INFUSE Inclusive Finance for Under-Served Economy",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_TIMOR-LESTE_INFUSE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_TIMOR-LESTE_INFUSE' - JP Timor-Leste INFUSE");
                        createdCount++;
                    }
                }
                
                // Record 92: JP_TIMOR-LESTE_LGSP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_TIMOR-LESTE_LGSP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_TIMOR-LESTE_LGSP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_TIMOR-LESTE_LGSP",
                            Name = "JP Timor-Leste LGSP",
                            Description = "JP Timor-Leste LGSP Local Governance Support Programme",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_TIMOR-LESTE_LGSP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_TIMOR-LESTE_LGSP' - JP Timor-Leste LGSP");
                        createdCount++;
                    }
                }
                
                // Record 93: JP_UGANDA_GENDER_EQUALIT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_UGANDA_GENDER_EQUALIT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_UGANDA_GENDER_EQUALIT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_UGANDA_GENDER_EQUALIT",
                            Name = "JP Uganda Gender Equality",
                            Description = "JP Uganda Gender Equality",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_UGANDA_GENDER_EQUALIT",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_UGANDA_GENDER_EQUALIT' - JP Uganda Gender Equality");
                        createdCount++;
                    }
                }
                
                // Record 94: JP_UGANDA_SUPPORT_FOR_AI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_UGANDA_SUPPORT_FOR_AI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'JP_UGANDA_SUPPORT_FOR_AI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_UGANDA_SUPPORT_FOR_AI",
                            Name = "JP Uganda Support for AIDS",
                            Description = "JP Uganda Support for AIDS",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JP_UGANDA_SUPPORT_FOR_AI",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_UGANDA_SUPPORT_FOR_AI' - JP Uganda Support for AIDS");
                        createdCount++;
                    }
                }
                
                // Record 95: KIRIBATI_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "KIRIBATI_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'KIRIBATI_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "KIRIBATI_ONE_UN_FUND",
                            Name = "Kiribati One UN Fund",
                            Description = "Kiribati One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "KIRIBATI_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'KIRIBATI_ONE_UN_FUND' - Kiribati One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 96: KYRGYZSTAN_ONE_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "KYRGYZSTAN_ONE_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'KYRGYZSTAN_ONE_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "KYRGYZSTAN_ONE_FUND",
                            Name = "Kyrgyzstan One Fund",
                            Description = "Kyrgyzstan One Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "KYRGYZSTAN_ONE_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'KYRGYZSTAN_ONE_FUND' - Kyrgyzstan One Fund");
                        createdCount++;
                    }
                }
                
                // Record 97: LEBANON_RECOVERY_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "LEBANON_RECOVERY_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'LEBANON_RECOVERY_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "LEBANON_RECOVERY_FUND",
                            Name = "Lebanon Recovery Fund",
                            Description = "Lebanon Recovery Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "LEBANON_RECOVERY_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'LEBANON_RECOVERY_FUND' - Lebanon Recovery Fund");
                        createdCount++;
                    }
                }
                
                // Record 98: LESOTHO_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "LESOTHO_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'LESOTHO_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "LESOTHO_ONE_UN_FUND",
                            Name = "Lesotho One UN Fund",
                            Description = "Lesotho One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "LESOTHO_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'LESOTHO_ONE_UN_FUND' - Lesotho One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 99: MALAWI_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MALAWI_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MALAWI_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MALAWI_ONE_UN_FUND",
                            Name = "Malawi One UN Fund",
                            Description = "Malawi One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MALAWI_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MALAWI_ONE_UN_FUND' - Malawi One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 100: MALDIVES_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MALDIVES_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MALDIVES_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MALDIVES_ONE_UN_FUND",
                            Name = "Maldives One UN Fund",
                            Description = "Maldives One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MALDIVES_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MALDIVES_ONE_UN_FUND' - Maldives One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 101: MDG_ACHIEVEMENT_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MDG_ACHIEVEMENT_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MDG_ACHIEVEMENT_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MDG_ACHIEVEMENT_FUND",
                            Name = "MDG Achievement Fund",
                            Description = "MDG Achievement Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MDG_ACHIEVEMENT_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MDG_ACHIEVEMENT_FUND' - MDG Achievement Fund");
                        createdCount++;
                    }
                }
                
                // Record 102: MONTENEGRO_UN_COUNTRY_FU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MONTENEGRO_UN_COUNTRY_FU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MONTENEGRO_UN_COUNTRY_FU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MONTENEGRO_UN_COUNTRY_FU",
                            Name = "Montenegro UN Country Fund",
                            Description = "Montenegro UN Country Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MONTENEGRO_UN_COUNTRY_FU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MONTENEGRO_UN_COUNTRY_FU' - Montenegro UN Country Fund");
                        createdCount++;
                    }
                }
                
                // Record 103: MOZAMBIQUE_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MOZAMBIQUE_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MOZAMBIQUE_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MOZAMBIQUE_ONE_UN_FUND",
                            Name = "Mozambique One UN Fund",
                            Description = "Mozambique One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MOZAMBIQUE_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MOZAMBIQUE_ONE_UN_FUND' - Mozambique One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 104: NEPAL_-_UN_PEACE_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NEPAL_-_UN_PEACE_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'NEPAL_-_UN_PEACE_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NEPAL_-_UN_PEACE_FUND",
                            Name = "Nepal - UN Peace Fund",
                            Description = "Nepal - UN Peace Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "NEPAL_-_UN_PEACE_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'NEPAL_-_UN_PEACE_FUND' - Nepal - UN Peace Fund");
                        createdCount++;
                    }
                }
                
                // Record 105: PAKISTAN_ONE_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PAKISTAN_ONE_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'PAKISTAN_ONE_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PAKISTAN_ONE_FUND",
                            Name = "Pakistan One Fund",
                            Description = "Pakistan One Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "PAKISTAN_ONE_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'PAKISTAN_ONE_FUND' - Pakistan One Fund");
                        createdCount++;
                    }
                }
                
                // Record 106: PBF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PBF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'PBF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PBF",
                            Name = "PBF",
                            Description = "PBF Peacebuilding Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "PBF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'PBF' - PBF");
                        createdCount++;
                    }
                }
                
                // Record 107: PNG_UN_COUNTRY_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PNG_UN_COUNTRY_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'PNG_UN_COUNTRY_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PNG_UN_COUNTRY_FUND",
                            Name = "PNG UN Country Fund",
                            Description = "PNG UN Country Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "PNG_UN_COUNTRY_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'PNG_UN_COUNTRY_FUND' - PNG UN Country Fund");
                        createdCount++;
                    }
                }
                
                // Record 108: REDD+_JP_PARTNERSHIP_SUP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REDD+_JP_PARTNERSHIP_SUP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'REDD+_JP_PARTNERSHIP_SUP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REDD+_JP_PARTNERSHIP_SUP",
                            Name = "REDD+ JP Partnership Support",
                            Description = "REDD+ JP Partnership Support",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "REDD+_JP_PARTNERSHIP_SUP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'REDD+_JP_PARTNERSHIP_SUP' - REDD+ JP Partnership Support");
                        createdCount++;
                    }
                }
                
                // Record 109: RWANDA_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "RWANDA_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'RWANDA_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "RWANDA_ONE_UN_FUND",
                            Name = "Rwanda One UN Fund",
                            Description = "Rwanda One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "RWANDA_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'RWANDA_ONE_UN_FUND' - Rwanda One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 110: SIERRA_LEONE_MDTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SIERRA_LEONE_MDTF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SIERRA_LEONE_MDTF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SIERRA_LEONE_MDTF",
                            Name = "Sierra Leone MDTF",
                            Description = "Sierra Leone MDTF",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SIERRA_LEONE_MDTF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SIERRA_LEONE_MDTF' - Sierra Leone MDTF");
                        createdCount++;
                    }
                }
                
                // Record 111: SOMALIA_COMMON_HUMANITAR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SOMALIA_COMMON_HUMANITAR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SOMALIA_COMMON_HUMANITAR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SOMALIA_COMMON_HUMANITAR",
                            Name = "Somalia Common Humanitarian Fd",
                            Description = "Somalia Common Humanitarian Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SOMALIA_COMMON_HUMANITAR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SOMALIA_COMMON_HUMANITAR' - Somalia Common Humanitarian Fd");
                        createdCount++;
                    }
                }
                
                // Record 112: SSRF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SSRF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SSRF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SSRF",
                            Name = "SSRF",
                            Description = "SSRF South Sudan Recovery Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SSRF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SSRF' - SSRF");
                        createdCount++;
                    }
                }
                
                // Record 113: SUDAN_COMMON_HUMANITARIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SUDAN_COMMON_HUMANITARIA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SUDAN_COMMON_HUMANITARIA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SUDAN_COMMON_HUMANITARIA",
                            Name = "Sudan Common Humanitarian Fund",
                            Description = "Sudan Common Humanitarian Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SUDAN_COMMON_HUMANITARIA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SUDAN_COMMON_HUMANITARIA' - Sudan Common Humanitarian Fund");
                        createdCount++;
                    }
                }
                
                // Record 114: TANZANIA_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "TANZANIA_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'TANZANIA_ONE_UN_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "TANZANIA_ONE_UN_FUND",
                            Name = "Tanzania One UN Fund",
                            Description = "Tanzania One UN Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "TANZANIA_ONE_UN_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'TANZANIA_ONE_UN_FUND' - Tanzania One UN Fund");
                        createdCount++;
                    }
                }
                
                // Record 115: UN_ACTION
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ACTION");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_ACTION' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ACTION",
                            Name = "UN Action",
                            Description = "UN Action Against Sexual Violence in Conflict",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_ACTION",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ACTION' - UN Action");
                        createdCount++;
                    }
                }
                
                // Record 116: UN_CIVIL_SOCIETY_TRUST_F
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_CIVIL_SOCIETY_TRUST_F");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_CIVIL_SOCIETY_TRUST_F' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_CIVIL_SOCIETY_TRUST_F",
                            Name = "UN Civil Society Trust Fund",
                            Description = "UN Civil Society Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_CIVIL_SOCIETY_TRUST_F",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_CIVIL_SOCIETY_TRUST_F' - UN Civil Society Trust Fund");
                        createdCount++;
                    }
                }
                
                // Record 117: UNIPP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIPP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIPP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIPP",
                            Name = "UNIPP",
                            Description = "UNIPP United Nations Indigenous Peoples’ Partnership",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIPP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIPP' - UNIPP");
                        createdCount++;
                    }
                }
                
                // Record 118: UNTFHS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNTFHS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNTFHS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNTFHS",
                            Name = "UNTFHS",
                            Description = "UN Trust Fund for Human Security",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNTFHS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNTFHS' - UNTFHS");
                        createdCount++;
                    }
                }
                
                // Record 119: UN_TRUST_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_TRUST_FUND");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_TRUST_FUND' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_TRUST_FUND",
                            Name = "UN Trust Fund",
                            Description = "UN Trust Fund to End Volence Against Women",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_TRUST_FUND",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_TRUST_FUND' - UN Trust Fund");
                        createdCount++;
                    }
                }
                
                // Record 120: UNDG_HRF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDG_HRF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDG_HRF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDG_HRF",
                            Name = "UNDG HRF",
                            Description = "Haiti Reconstruction Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDG_HRF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDG_HRF' - UNDG HRF");
                        createdCount++;
                    }
                }
                
                // Record 121: UNDG_ITF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDG_ITF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDG_ITF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDG_ITF",
                            Name = "UNDG ITF",
                            Description = "UNDG Iraq Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDG_ITF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDG_ITF' - UNDG ITF");
                        createdCount++;
                    }
                }
                
                // Record 122: UN-REDD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-REDD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN-REDD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-REDD",
                            Name = "UN-REDD",
                            Description = "UN-REDD Programme Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN-REDD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-REDD' - UN-REDD");
                        createdCount++;
                    }
                }
                
                // Record 123: URUGUAY_ONE_UN_COHERENCE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "URUGUAY_ONE_UN_COHERENCE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'URUGUAY_ONE_UN_COHERENCE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "URUGUAY_ONE_UN_COHERENCE",
                            Name = "Uruguay One UN Coherence Fund",
                            Description = "Uruguay One UN Coherence Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "URUGUAY_ONE_UN_COHERENCE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'URUGUAY_ONE_UN_COHERENCE' - Uruguay One UN Coherence Fund");
                        createdCount++;
                    }
                }
                
                // Record 124: VIET_NAM_ONE_FUND_I
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "VIET_NAM_ONE_FUND_I");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'VIET_NAM_ONE_FUND_I' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "VIET_NAM_ONE_FUND_I",
                            Name = "Viet Nam One Plan Fund I",
                            Description = "Viet Nam One Plan Fund I",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "VIET_NAM_ONE_FUND_I",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'VIET_NAM_ONE_FUND_I' - Viet Nam One Plan Fund I");
                        createdCount++;
                    }
                }
                
                // Record 125: VIET_NAM_ONE_FUND_II
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "VIET_NAM_ONE_FUND_II");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'VIET_NAM_ONE_FUND_II' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "VIET_NAM_ONE_FUND_II",
                            Name = "Viet Nam One Plan Fund II",
                            Description = "Viet Nam One Plan Fund II",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "VIET_NAM_ONE_FUND_II",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'VIET_NAM_ONE_FUND_II' - Viet Nam One Plan Fund II");
                        createdCount++;
                    }
                }
                
                // Record 126: OTHER_UNDP_MDTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_UNDP_MDTF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OTHER_UNDP_MDTF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_UNDP_MDTF",
                            Name = "Other UNDP MDTF",
                            Description = "Other UNDP MDTF",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OTHER_UNDP_MDTF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_UNDP_MDTF' - Other UNDP MDTF");
                        createdCount++;
                    }
                }
                
                // Record 127: OTHER_UNDP_JP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_UNDP_JP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OTHER_UNDP_JP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_UNDP_JP",
                            Name = "Other UNDP JP",
                            Description = "Other UNDP JP",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OTHER_UNDP_JP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_UNDP_JP' - Other UNDP JP");
                        createdCount++;
                    }
                }
                
                // Record 128: UNSO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNSO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSO",
                            Name = "UNSO",
                            Description = "UN Fund for Sudano-Sahelian Activities",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNSO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSO' - UNSO");
                        createdCount++;
                    }
                }
                
                // Record 129: UN_VTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_VTF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_VTF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_VTF",
                            Name = "UN VTF",
                            Description = "VTF UN Voluntary Trust Fund for Assistance in Mine Action",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_VTF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_VTF' - UN VTF");
                        createdCount++;
                    }
                }
                
                // Record 130: UN_HAITI_CHOLERA_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_HAITI_CHOLERA_MPTF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_HAITI_CHOLERA_MPTF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_HAITI_CHOLERA_MPTF",
                            Name = "UN Haiti Cholera MPTF",
                            Description = "UN Haiti Cholera Response Multi-Partner Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_HAITI_CHOLERA_MPTF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_HAITI_CHOLERA_MPTF' - UN Haiti Cholera MPTF");
                        createdCount++;
                    }
                }
                
                // Record 131: UNITLIFE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNITLIFE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNITLIFE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNITLIFE",
                            Name = "UNITLIFE",
                            Description = "UNITLIFE United Nations Initiative Fighting Chronic Malnutrition Through Innovation",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNITLIFE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNITLIFE' - UNITLIFE");
                        createdCount++;
                    }
                }
                
                // Record 132: UN_MPTF_OFFICE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_MPTF_OFFICE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_MPTF_OFFICE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_MPTF_OFFICE",
                            Name = "UN MPTF Office",
                            Description = "United Nations Multi-Partner Trust Fund Office",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_MPTF_OFFICE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_MPTF_OFFICE' - UN MPTF Office");
                        createdCount++;
                    }
                }
                
                // Record 133: UN_SRI_LANKA_SDG_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_SRI_LANKA_SDG_MPTF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_SRI_LANKA_SDG_MPTF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_SRI_LANKA_SDG_MPTF",
                            Name = "UN Sri Lanka SDG MPTF",
                            Description = "United Nations Sri Lanka SDG Multi-Partner Trust Fund",
                            Type = "Level_4",
                            Parent = "UN_INTER_POOLED_FUND",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_SRI_LANKA_SDG_MPTF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_SRI_LANKA_SDG_MPTF' - UN Sri Lanka SDG MPTF");
                        createdCount++;
                    }
                }
                
                // Record 134: UNISID1
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNISID1");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNISID1' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNISID1",
                            Name = "Sidney University",
                            Description = "The University of Sidney",
                            Type = "Level_2",
                            Parent = "ACADEMIC_TRAINING_RESEARC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNISID1",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNISID1' - Sidney University");
                        createdCount++;
                    }
                }
                
                // Record 135: SUBSIDIARY_ORG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SUBSIDIARY_ORG");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SUBSIDIARY_ORG' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SUBSIDIARY_ORG",
                            Name = "UN Subsidiary Organs",
                            Description = "United Nations Subsidiary Organs",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "SUBSIDIARY_ORG",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SUBSIDIARY_ORG' - UN Subsidiary Organs");
                        createdCount++;
                    }
                }
                
                // Record 136: BINUCA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BINUCA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'BINUCA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BINUCA",
                            Name = "BINUCA",
                            Description = "BINUCA United Nations Integrated Peacebuilding Office in the Central African Republic",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "BINUCA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'BINUCA' - BINUCA");
                        createdCount++;
                    }
                }
                
                // Record 137: UN_COORD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_COORD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_COORD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_COORD",
                            Name = "UN Coordination Mechanisms",
                            Description = "United Nations Coordination Mechanisms",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "UN_COORD",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_COORD' - UN Coordination Mechanisms");
                        createdCount++;
                    }
                }
                
                // Record 138: CEB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CEB");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CEB' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CEB",
                            Name = "CEB",
                            Description = "CEB United Nations System Chief Executives Board for Coordination",
                            Type = "Level_4",
                            Parent = "UN_COORD",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CEB",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CEB' - CEB");
                        createdCount++;
                    }
                }
                
                // Record 139: MENUB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MENUB");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MENUB' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MENUB",
                            Name = "MENUB",
                            Description = "MENUB United Nations Electoral Observation Mission in Burundi",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MENUB",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MENUB' - MENUB");
                        createdCount++;
                    }
                }
                
                // Record 140: MINURSO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINURSO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MINURSO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINURSO",
                            Name = "MINURSO",
                            Description = "MINURSO United Nations Mission for the Referendum in Western Sahara",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MINURSO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MINURSO' - MINURSO");
                        createdCount++;
                    }
                }
                
                // Record 141: MINUSCA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUSCA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MINUSCA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUSCA",
                            Name = "MINUSCA",
                            Description = "MINUSCA United Nations Multidimensional Integrated Stabilization Mission in the Central African Republic",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MINUSCA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUSCA' - MINUSCA");
                        createdCount++;
                    }
                }
                
                // Record 142: MINUSMA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUSMA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MINUSMA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUSMA",
                            Name = "MINUSMA",
                            Description = "MINUSMA United Nations Multidimensional Integrated Stabilization Mission in Mali",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MINUSMA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUSMA' - MINUSMA");
                        createdCount++;
                    }
                }
                
                // Record 143: MINUSTAH
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUSTAH");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MINUSTAH' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUSTAH",
                            Name = "MINUSTAH",
                            Description = "MINUSTAH United Nations Stabilization Mission in Haiti",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MINUSTAH",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUSTAH' - MINUSTAH");
                        createdCount++;
                    }
                }
                
                // Record 144: MONUSCO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MONUSCO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MONUSCO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MONUSCO",
                            Name = "MONUSCO",
                            Description = "MONUSCO United Nations Organization Stabilization Mission in the Democratic Republic of the Congo",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MONUSCO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MONUSCO' - MONUSCO");
                        createdCount++;
                    }
                }
                
                // Record 145: UNAKRT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAKRT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNAKRT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAKRT",
                            Name = "UNAKRT",
                            Description = "UNAKRT United Nations Assistance to the Khmer Rouge Trials",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNAKRT",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAKRT' - UNAKRT");
                        createdCount++;
                    }
                }
                
                // Record 146: UNAMA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAMA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNAMA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAMA",
                            Name = "UNAMA",
                            Description = "UNAMA United Nations Assistance Mission in Afghanistan",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNAMA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAMA' - UNAMA");
                        createdCount++;
                    }
                }
                
                // Record 147: UNAMI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAMI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNAMI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAMI",
                            Name = "UNAMI",
                            Description = "UNAMI United Nations Assistance Mission for Iraq",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNAMI",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAMI' - UNAMI");
                        createdCount++;
                    }
                }
                
                // Record 148: UNFICYP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFICYP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNFICYP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFICYP",
                            Name = "UNFICYP",
                            Description = "UNFICYP United Nations Peacekeeping Force in Cyprus",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNFICYP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFICYP' - UNFICYP");
                        createdCount++;
                    }
                }
                
                // Record 149: UNIFIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIFIL");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIFIL' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIFIL",
                            Name = "UNIFIL",
                            Description = "UNIFIL United Nations Interim Force in Lebanon",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIFIL",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIFIL' - UNIFIL");
                        createdCount++;
                    }
                }
                
                // Record 150: UNIPSIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIPSIL");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIPSIL' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIPSIL",
                            Name = "UNIPSIL",
                            Description = "UNIPSIL United Nations Integrated Peacebuilding Office in Sierra Leone",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIPSIL",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIPSIL' - UNIPSIL");
                        createdCount++;
                    }
                }
                
                // Record 151: UNISFA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNISFA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNISFA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNISFA",
                            Name = "UNISFA",
                            Description = "UNISFA United Nations Interim Security Force in Abyei",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNISFA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNISFA' - UNISFA");
                        createdCount++;
                    }
                }
                
                // Record 152: UNMIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIL");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNMIL' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIL",
                            Name = "UNMIL",
                            Description = "UNMIL United Nations Mission in Liberia",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNMIL",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIL' - UNMIL");
                        createdCount++;
                    }
                }
                
                // Record 153: UNMISS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMISS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNMISS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMISS",
                            Name = "UNMISS",
                            Description = "UNMISS United Nations Mission in the Republic of South Sudan",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNMISS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMISS' - UNMISS");
                        createdCount++;
                    }
                }
                
                // Record 154: UNMIT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNMIT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIT",
                            Name = "UNMIT",
                            Description = "UNMIT United Nations Integrated Mission in Timor-Leste",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNMIT",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIT' - UNMIT");
                        createdCount++;
                    }
                }
                
                // Record 155: UNMOGIP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMOGIP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNMOGIP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMOGIP",
                            Name = "UNMOGIP",
                            Description = "UNMOGIP United Nations Military Observer Group in India and Pakistan",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNMOGIP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMOGIP' - UNMOGIP");
                        createdCount++;
                    }
                }
                
                // Record 156: DEPARTMENT_OFFICE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DEPARTMENT_OFFICE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'DEPARTMENT_OFFICE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DEPARTMENT_OFFICE",
                            Name = "UN Departments and Offices",
                            Description = "United Nations Departments and Offices",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "DEPARTMENT_OFFICE",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'DEPARTMENT_OFFICE' - UN Departments and Offices");
                        createdCount++;
                    }
                }
                
                // Record 157: UNOAU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOAU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOAU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOAU",
                            Name = "UNOAU",
                            Description = "UNOAU United Nations Office to the African Union",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOAU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOAU' - UNOAU");
                        createdCount++;
                    }
                }
                
                // Record 158: UNOCA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOCA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOCA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOCA",
                            Name = "UNOCA",
                            Description = "UNOCA United Nations Regional Office for Central Africa",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOCA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOCA' - UNOCA");
                        createdCount++;
                    }
                }
                
                // Record 159: UNOCI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOCI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOCI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOCI",
                            Name = "UNOCI",
                            Description = "UNOCI United Nations Operation in Côte d\'Ivoire",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOCI",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOCI' - UNOCI");
                        createdCount++;
                    }
                }
                
                // Record 160: OTHER_ENTITIES
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_ENTITIES");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OTHER_ENTITIES' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_ENTITIES",
                            Name = "UN Other Entities",
                            Description = "United Nations Other Entities",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "OTHER_ENTITIES",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_ENTITIES' - UN Other Entities");
                        createdCount++;
                    }
                }
                
                // Record 161: ITC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ITC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ITC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ITC",
                            Name = "ITC",
                            Description = "ITC International Trade Centre",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ITC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ITC' - ITC");
                        createdCount++;
                    }
                }
                
                // Record 162: UNHCR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNHCR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNHCR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNHCR",
                            Name = "UNHCR",
                            Description = "UNHCR Office of the United Nations High Commissioner for Refugees",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNHCR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNHCR' - UNHCR");
                        createdCount++;
                    }
                }
                
                // Record 163: FUND_PROGRAMME
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "FUND_PROGRAMME");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'FUND_PROGRAMME' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "FUND_PROGRAMME",
                            Name = "UN Funds and Programmes",
                            Description = "United Nations Funds and Programmes",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "FUND_PROGRAMME",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'FUND_PROGRAMME' - UN Funds and Programmes");
                        createdCount++;
                    }
                }
                
                // Record 164: UNCDF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNCDF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNCDF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNCDF",
                            Name = "UNCDF",
                            Description = "UNCDF United Nations Capital Development Fund",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNCDF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNCDF' - UNCDF");
                        createdCount++;
                    }
                }
                
                // Record 165: UNICEF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNICEF");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNICEF' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNICEF",
                            Name = "UNICEF",
                            Description = "UNICEF United Nations Children\'s Fund",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNICEF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNICEF' - UNICEF");
                        createdCount++;
                    }
                }
                
                // Record 166: UNCTAD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNCTAD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNCTAD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNCTAD",
                            Name = "UNCTAD",
                            Description = "UNCTAD United Nations Conference on Trade and Development",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNCTAD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNCTAD' - UNCTAD");
                        createdCount++;
                    }
                }
                
                // Record 167: UNEP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNEP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNEP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNEP",
                            Name = "UNEP",
                            Description = "UNEP United Nations Environment Programme",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNEP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNEP' - UNEP");
                        createdCount++;
                    }
                }
                
                // Record 168: UN-HABITAT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-HABITAT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN-HABITAT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-HABITAT",
                            Name = "UN-HABITAT",
                            Description = "UN-HABITAT United Nations Human Settlements Programme",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN-HABITAT",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-HABITAT' - UN-HABITAT");
                        createdCount++;
                    }
                }
                
                // Record 169: UNODC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNODC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNODC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNODC",
                            Name = "UNODC",
                            Description = "UNODC United Nations Office on Drugs and Crime",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNODC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNODC' - UNODC");
                        createdCount++;
                    }
                }
                
                // Record 170: UNFPA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFPA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNFPA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFPA",
                            Name = "UNFPA",
                            Description = "UNFPA United Nations Population Fund",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNFPA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFPA' - UNFPA");
                        createdCount++;
                    }
                }
                
                // Record 171: UNRWA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNRWA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNRWA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNRWA",
                            Name = "UNRWA",
                            Description = "UNRWA United Nations Relief and Works Agency for Palestine Refugees in the Near East",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNRWA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNRWA' - UNRWA");
                        createdCount++;
                    }
                }
                
                // Record 172: UNV
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNV");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNV' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNV",
                            Name = "UNV",
                            Description = "UNV United Nations Volunteers",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNV",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNV' - UNV");
                        createdCount++;
                    }
                }
                
                // Record 173: WFP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WFP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'WFP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WFP",
                            Name = "WFP",
                            Description = "WFP United Nations World Food Programme",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "WFP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'WFP' - WFP");
                        createdCount++;
                    }
                }
                
                // Record 174: UN_DESA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DESA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_DESA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DESA",
                            Name = "UN DESA",
                            Description = "UN DESA Department of Economic and Social Affairs",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_DESA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DESA' - UN DESA");
                        createdCount++;
                    }
                }
                
                // Record 175: UN_DGACM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DGACM");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_DGACM' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DGACM",
                            Name = "UN DGACM",
                            Description = "UN DGACM Department for General Assembly and Conference Management",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_DGACM",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DGACM' - UN DGACM");
                        createdCount++;
                    }
                }
                
                // Record 176: UN_DMSPC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DMSPC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_DMSPC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DMSPC",
                            Name = "UN DMSPC",
                            Description = "UN DMSPC Department of Management Strategy, Policy and Compliance",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_DMSPC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DMSPC' - UN DMSPC");
                        createdCount++;
                    }
                }
                
                // Record 177: UN_DGC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DGC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_DGC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DGC",
                            Name = "UN DGC",
                            Description = "UN DGC Department of Global Communications",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_DGC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DGC' - UN DGC");
                        createdCount++;
                    }
                }
                
                // Record 178: UNDSS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDSS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDSS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDSS",
                            Name = "UNDSS",
                            Description = "UNDSS Department of Safety and Security",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDSS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDSS' - UNDSS");
                        createdCount++;
                    }
                }
                
                // Record 179: UN_OCHA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_OCHA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_OCHA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_OCHA",
                            Name = "UN OCHA",
                            Description = "UN OCHA Office for the Coordination of Humanitarian Affairs",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_OCHA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_OCHA' - UN OCHA");
                        createdCount++;
                    }
                }
                
                // Record 180: UN_OHCHR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_OHCHR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_OHCHR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_OHCHR",
                            Name = "UN OHCHR",
                            Description = "UN OHCHR Office of the United Nations High Commissioner for Human Rights",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_OHCHR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_OHCHR' - UN OHCHR");
                        createdCount++;
                    }
                }
                
                // Record 181: OIOS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OIOS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OIOS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OIOS",
                            Name = "OIOS",
                            Description = "OIOS Office of Internal Oversight Services",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OIOS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OIOS' - OIOS");
                        createdCount++;
                    }
                }
                
                // Record 182: UN_OLA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_OLA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_OLA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_OLA",
                            Name = "UN OLA",
                            Description = "UN OLA Office of Legal Affairs",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_OLA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_OLA' - UN OLA");
                        createdCount++;
                    }
                }
                
                // Record 183: OSAA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OSAA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OSAA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OSAA",
                            Name = "OSAA",
                            Description = "OSAA Office of the Special Adviser on Africa",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OSAA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OSAA' - OSAA");
                        createdCount++;
                    }
                }
                
                // Record 184: SRSG_CAAC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SRSG_CAAC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SRSG_CAAC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SRSG_CAAC",
                            Name = "SRSG CAAC",
                            Description = "SRSG CAAC Office of the Special Representative of the Secretary-General for Children and Armed Conflict",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "SRSG_CAAC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SRSG_CAAC' - SRSG CAAC");
                        createdCount++;
                    }
                }
                
                // Record 185: UNODA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNODA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNODA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNODA",
                            Name = "UNODA",
                            Description = "UNODA Office for Disarmament Affairs",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNODA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNODA' - UNODA");
                        createdCount++;
                    }
                }
                
                // Record 186: UNOG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOG");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOG' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOG",
                            Name = "UNOG",
                            Description = "UNOG United Nations Office at Geneva",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOG",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOG' - UNOG");
                        createdCount++;
                    }
                }
                
                // Record 187: UN-OHRLLS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-OHRLLS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN-OHRLLS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-OHRLLS",
                            Name = "UN-OHRLLS",
                            Description = "UN-OHRLLS Office of the High Representative for the Least Developed Countries, Landlocked Developing Countries and Small Island Developing States",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN-OHRLLS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-OHRLLS' - UN-OHRLLS");
                        createdCount++;
                    }
                }
                
                // Record 188: UNON
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNON");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNON' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNON",
                            Name = "UNON",
                            Description = "UNON United Nations Office at Nairobi",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNON",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNON' - UNON");
                        createdCount++;
                    }
                }
                
                // Record 189: UNOV
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOV");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOV' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOV",
                            Name = "UNOV",
                            Description = "UNOV United Nations Office at Vienna",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOV",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOV' - UNOV");
                        createdCount++;
                    }
                }
                
                // Record 190: UN_ICC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ICC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_ICC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ICC",
                            Name = "UN ICC",
                            Description = "ICC International Computing Centre",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_ICC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ICC' - UN ICC");
                        createdCount++;
                    }
                }
                
                // Record 191: OTHER_BODIES
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_BODIES");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OTHER_BODIES' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_BODIES",
                            Name = "UN Other Bodies",
                            Description = "United Nations Other Bodies",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "OTHER_BODIES",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_BODIES' - UN Other Bodies");
                        createdCount++;
                    }
                }
                
                // Record 192: UNAIDS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAIDS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNAIDS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAIDS",
                            Name = "UNAIDS",
                            Description = "UNAIDS Joint United Nations Programme on HIV/AIDS",
                            Type = "Level_4",
                            Parent = "OTHER_BODIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNAIDS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAIDS' - UNAIDS");
                        createdCount++;
                    }
                }
                
                // Record 193: UN_WOMEN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_WOMEN");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_WOMEN' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_WOMEN",
                            Name = "UN WOMEN",
                            Description = "UN WOMEN United Nations Entity for Gender Equality and the Empowerment of Women",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_WOMEN",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_WOMEN' - UN WOMEN");
                        createdCount++;
                    }
                }
                
                // Record 194: UNDRR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDRR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDRR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDRR",
                            Name = "UNDRR",
                            Description = "UNDRR United Nations Office for Disaster Risk Reduction",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDRR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDRR' - UNDRR");
                        createdCount++;
                    }
                }
                
                // Record 195: RESEARCH_TRAINING
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "RESEARCH_TRAINING");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'RESEARCH_TRAINING' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "RESEARCH_TRAINING",
                            Name = "UN Research and Training",
                            Description = "United Nations Research and Training",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "RESEARCH_TRAINING",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'RESEARCH_TRAINING' - UN Research and Training");
                        createdCount++;
                    }
                }
                
                // Record 196: UNSSC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSSC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNSSC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSSC",
                            Name = "UNSSC",
                            Description = "UNSSC United Nations System Staff College",
                            Type = "Level_4",
                            Parent = "RESEARCH_TRAINING",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNSSC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSSC' - UNSSC");
                        createdCount++;
                    }
                }
                
                // Record 197: UNU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNU",
                            Name = "UNU",
                            Description = "UNU United Nations University",
                            Type = "Level_4",
                            Parent = "RESEARCH_TRAINING",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNU' - UNU");
                        createdCount++;
                    }
                }
                
                // Record 198: REG_COMMISSION
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REG_COMMISSION");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'REG_COMMISSION' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REG_COMMISSION",
                            Name = "UN Regional Commissions",
                            Description = "United Nations Regional Commissions",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "REG_COMMISSION",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'REG_COMMISSION' - UN Regional Commissions");
                        createdCount++;
                    }
                }
                
                // Record 199: UN_ESCAP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ESCAP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_ESCAP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ESCAP",
                            Name = "UN ESCAP",
                            Description = "UN ESCAP Economic and Social Commission for Asia and the Pacific",
                            Type = "Level_4",
                            Parent = "REG_COMMISSION",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_ESCAP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ESCAP' - UN ESCAP");
                        createdCount++;
                    }
                }
                
                // Record 200: UN_ESCWA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ESCWA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_ESCWA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ESCWA",
                            Name = "UN ESCWA",
                            Description = "UN ESCWA Economic and Social Commission for Western Asia",
                            Type = "Level_4",
                            Parent = "REG_COMMISSION",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_ESCWA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ESCWA' - UN ESCWA");
                        createdCount++;
                    }
                }
                
                // Record 201: UN_ECA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ECA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_ECA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ECA",
                            Name = "UN ECA",
                            Description = "UN ECA Economic Commission for Africa",
                            Type = "Level_4",
                            Parent = "REG_COMMISSION",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_ECA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ECA' - UN ECA");
                        createdCount++;
                    }
                }
                
                // Record 202: UN_ECLAC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ECLAC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_ECLAC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ECLAC",
                            Name = "UN ECLAC",
                            Description = "UN ECLAC Economic Commission for Latin America and the Caribbean",
                            Type = "Level_4",
                            Parent = "REG_COMMISSION",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_ECLAC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ECLAC' - UN ECLAC");
                        createdCount++;
                    }
                }
                
                // Record 203: UNDG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDG");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDG' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDG",
                            Name = "UNDG",
                            Description = "UNSDG United Nations Sustainable Development Group (formerly UNDG)",
                            Type = "Level_4",
                            Parent = "UN_COORD",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDG",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDG' - UNDG");
                        createdCount++;
                    }
                }
                
                // Record 204: UN_ECE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ECE");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_ECE' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ECE",
                            Name = "UN ECE",
                            Description = "UN ECE Economic Commission for Europe",
                            Type = "Level_4",
                            Parent = "REG_COMMISSION",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_ECE",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ECE' - UN ECE");
                        createdCount++;
                    }
                }
                
                // Record 205: UNIOGBIS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIOGBIS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIOGBIS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIOGBIS",
                            Name = "UNIOGBIS",
                            Description = "UNIOGBIS United Nations Integrated Peacebuilding Office in Guinea-Bissau",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIOGBIS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIOGBIS' - UNIOGBIS");
                        createdCount++;
                    }
                }
                
                // Record 206: UNSCN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSCN");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNSCN' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSCN",
                            Name = "UNSCN",
                            Description = "UNSCN United Nations System Standing Committee on Nutrition",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNSCN",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSCN' - UNSCN");
                        createdCount++;
                    }
                }
                
                // Record 207: CONVENTION_FRAMEWORK
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CONVENTION_FRAMEWORK");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CONVENTION_FRAMEWORK' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CONVENTION_FRAMEWORK",
                            Name = "UN Conventions and Frameworks",
                            Description = "United Nations Conventions and Frameworks",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "CONVENTION_FRAMEWORK",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CONVENTION_FRAMEWORK' - UN Conventions and Frameworks");
                        createdCount++;
                    }
                }
                
                // Record 208: CRPD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CRPD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'CRPD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CRPD",
                            Name = "CRPD",
                            Description = "CRPD Convention on the Rights of Persons with Disabilities",
                            Type = "Level_4",
                            Parent = "CONVENTION_FRAMEWORK",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CRPD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CRPD' - CRPD");
                        createdCount++;
                    }
                }
                
                // Record 209: SPECIALIZED_AGENCIES
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SPECIALIZED_AGENCIES");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'SPECIALIZED_AGENCIES' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SPECIALIZED_AGENCIES",
                            Name = "UN Specialized Agencies",
                            Description = "United Nations Specialized Agencies",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "SPECIALIZED_AGENCIES",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'SPECIALIZED_AGENCIES' - UN Specialized Agencies");
                        createdCount++;
                    }
                }
                
                // Record 210: FAO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "FAO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'FAO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "FAO",
                            Name = "FAO",
                            Description = "FAO Food and Agriculture Organization of the United Nations",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "FAO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'FAO' - FAO");
                        createdCount++;
                    }
                }
                
                // Record 211: RELATED_ORG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "RELATED_ORG");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'RELATED_ORG' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "RELATED_ORG",
                            Name = "UN Related Organizations",
                            Description = "United Nations Related Organizations",
                            Type = "Level_3",
                            Parent = "UNITED_NATIONS",
                            PartnerCategoryCode = "RELATED_ORG",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'RELATED_ORG' - UN Related Organizations");
                        createdCount++;
                    }
                }
                
                // Record 212: IAEA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IAEA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IAEA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IAEA",
                            Name = "IAEA",
                            Description = "IAEA International Atomic Energy Agency",
                            Type = "Level_4",
                            Parent = "RELATED_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IAEA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IAEA' - IAEA");
                        createdCount++;
                    }
                }
                
                // Record 213: ICAO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ICAO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ICAO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ICAO",
                            Name = "ICAO",
                            Description = "ICAO International Civil Aviation Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ICAO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ICAO' - ICAO");
                        createdCount++;
                    }
                }
                
                // Record 214: IFAD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IFAD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IFAD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IFAD",
                            Name = "IFAD",
                            Description = "IFAD International Fund for Agricultural Development",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IFAD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IFAD' - IFAD");
                        createdCount++;
                    }
                }
                
                // Record 215: ILO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ILO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ILO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ILO",
                            Name = "ILO",
                            Description = "ILO International Labour Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ILO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ILO' - ILO");
                        createdCount++;
                    }
                }
                
                // Record 216: IMO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IMO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IMO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IMO",
                            Name = "IMO",
                            Description = "IMO International Maritime Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IMO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IMO' - IMO");
                        createdCount++;
                    }
                }
                
                // Record 217: ITU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ITU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'ITU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ITU",
                            Name = "ITU",
                            Description = "ITU International Telecommunication Union",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ITU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ITU' - ITU");
                        createdCount++;
                    }
                }
                
                // Record 218: OPCW
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OPCW");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OPCW' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OPCW",
                            Name = "OPCW",
                            Description = "OPCW Organisation for the Prohibition of Chemical Weapons",
                            Type = "Level_4",
                            Parent = "RELATED_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OPCW",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OPCW' - OPCW");
                        createdCount++;
                    }
                }
                
                // Record 219: UNCCD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNCCD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNCCD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNCCD",
                            Name = "UNCCD",
                            Description = "UNCCD United Nations Convention to Combat Desertification",
                            Type = "Level_4",
                            Parent = "CONVENTION_FRAMEWORK",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNCCD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNCCD' - UNCCD");
                        createdCount++;
                    }
                }
                
                // Record 220: UNESCO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNESCO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNESCO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNESCO",
                            Name = "UNESCO",
                            Description = "UNESCO United Nations Educational, Scientific and Cultural Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNESCO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNESCO' - UNESCO");
                        createdCount++;
                    }
                }
                
                // Record 221: UNFCCC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFCCC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNFCCC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFCCC",
                            Name = "UNFCCC",
                            Description = "UNFCCC United Nations Framework Convention on Climate Change",
                            Type = "Level_4",
                            Parent = "CONVENTION_FRAMEWORK",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNFCCC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFCCC' - UNFCCC");
                        createdCount++;
                    }
                }
                
                // Record 222: UNIDO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIDO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIDO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIDO",
                            Name = "UNIDO",
                            Description = "UNIDO United Nations Industrial Development Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIDO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIDO' - UNIDO");
                        createdCount++;
                    }
                }
                
                // Record 223: UPU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UPU");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UPU' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UPU",
                            Name = "UPU",
                            Description = "UPU Universal Postal Union",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UPU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UPU' - UPU");
                        createdCount++;
                    }
                }
                
                // Record 224: WIPO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WIPO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'WIPO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WIPO",
                            Name = "WIPO",
                            Description = "WIPO World Intellectual Property Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "WIPO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'WIPO' - WIPO");
                        createdCount++;
                    }
                }
                
                // Record 225: WMO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WMO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'WMO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WMO",
                            Name = "WMO",
                            Description = "WMO World Meteorological Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "WMO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'WMO' - WMO");
                        createdCount++;
                    }
                }
                
                // Record 226: UNWTO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNWTO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNWTO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNWTO",
                            Name = "UNWTO",
                            Description = "UNWTO World Tourism Organization",
                            Type = "Level_4",
                            Parent = "SPECIALIZED_AGENCIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNWTO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNWTO' - UNWTO");
                        createdCount++;
                    }
                }
                
                // Record 227: WTO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WTO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'WTO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WTO",
                            Name = "WTO",
                            Description = "WTO World Trade Organization",
                            Type = "Level_4",
                            Parent = "RELATED_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "WTO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'WTO' - WTO");
                        createdCount++;
                    }
                }
                
                // Record 228: UNIDIR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIDIR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIDIR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIDIR",
                            Name = "UNIDIR",
                            Description = "UNIDIR United Nations Institute for Disarmament Research",
                            Type = "Level_4",
                            Parent = "RESEARCH_TRAINING",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIDIR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIDIR' - UNIDIR");
                        createdCount++;
                    }
                }
                
                // Record 229: UNITAR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNITAR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNITAR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNITAR",
                            Name = "UNITAR",
                            Description = "UNITAR United Nations Institute for Training and Research",
                            Type = "Level_4",
                            Parent = "RESEARCH_TRAINING",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNITAR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNITAR' - UNITAR");
                        createdCount++;
                    }
                }
                
                // Record 230: UNICRI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNICRI");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNICRI' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNICRI",
                            Name = "UNICRI",
                            Description = "UNICRI United Nations Interregional Crime and Justice Research Institute",
                            Type = "Level_4",
                            Parent = "RESEARCH_TRAINING",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNICRI",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNICRI' - UNICRI");
                        createdCount++;
                    }
                }
                
                // Record 231: UNRISD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNRISD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNRISD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNRISD",
                            Name = "UNRISD",
                            Description = "UNRISD United Nations Research Institute for Social Development",
                            Type = "Level_4",
                            Parent = "RESEARCH_TRAINING",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNRISD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNRISD' - UNRISD");
                        createdCount++;
                    }
                }
                
                // Record 232: UNOIP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOIP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOIP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOIP",
                            Name = "UNOIP",
                            Description = "UNOIP United Nations Office of the Iraq Programme",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOIP",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOIP' - UNOIP");
                        createdCount++;
                    }
                }
                
                // Record 233: UNROD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNROD");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNROD' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNROD",
                            Name = "UNROD",
                            Description = "UNROD United Nations Register of Damage",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNROD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNROD' - UNROD");
                        createdCount++;
                    }
                }
                
                // Record 234: UNMIS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNMIS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIS",
                            Name = "UNMIS",
                            Description = "UNMIS United Nations Mission in Sudan",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNMIS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIS' - UNMIS");
                        createdCount++;
                    }
                }
                
                // Record 235: IOM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IOM");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IOM' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IOM",
                            Name = "IOM",
                            Description = "IOM International Organization for Migration",
                            Type = "Level_4",
                            Parent = "RELATED_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IOM",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IOM' - IOM");
                        createdCount++;
                    }
                }
                
                // Record 236: UNMIK
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIK");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNMIK' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIK",
                            Name = "UNMIK",
                            Description = "UNMIK United Nations Interim Administration Mission in Kosovo",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNMIK",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIK' - UNMIK");
                        createdCount++;
                    }
                }
                
                // Record 237: UN_UNITED_NATIONS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_UNITED_NATIONS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_UNITED_NATIONS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_UNITED_NATIONS",
                            Name = "United Nations",
                            Description = "UN United Nations",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_UNITED_NATIONS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_UNITED_NATIONS' - United Nations");
                        createdCount++;
                    }
                }
                
                // Record 238: UNIFEM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIFEM");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIFEM' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIFEM",
                            Name = "UNIFEM",
                            Description = "UNIFEM United Nations Development Fund for Women",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIFEM",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIFEM' - UNIFEM");
                        createdCount++;
                    }
                }
                
                // Record 239: UNORCID
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNORCID");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNORCID' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNORCID",
                            Name = "UNORCID",
                            Description = "UNORCID United Nations Office for REDD+ Coordination in Indonesia",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNORCID",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNORCID' - UNORCID");
                        createdCount++;
                    }
                }
                
                // Record 240: UNOWA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOWA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOWA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOWA",
                            Name = "UNOWA",
                            Description = "UNOWA United Nations Office for West Africa",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOWA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOWA' - UNOWA");
                        createdCount++;
                    }
                }
                
                // Record 241: UNSCEAR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSCEAR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNSCEAR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSCEAR",
                            Name = "UNSCEAR",
                            Description = "UNSCEAR United Nations Scientific Committee on the Effects of Atomic Radiation",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNSCEAR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSCEAR' - UNSCEAR");
                        createdCount++;
                    }
                }
                
                // Record 242: UNSMIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSMIL");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNSMIL' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSMIL",
                            Name = "UNSMIL",
                            Description = "UNSMIL United Nations Support Mission in Libya",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNSMIL",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSMIL' - UNSMIL");
                        createdCount++;
                    }
                }
                
                // Record 243: UNSOS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSOS");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNSOS' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSOS",
                            Name = "UNSOS",
                            Description = "UNSOS United Nations Support Office in Somalia",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNSOS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSOS' - UNSOS");
                        createdCount++;
                    }
                }
                
                // Record 244: UNSOM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSOM");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNSOM' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSOM",
                            Name = "UNSOM",
                            Description = "UNSOM United Nations Assistance Mission in Somalia",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNSOM",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSOM' - UNSOM");
                        createdCount++;
                    }
                }
                
                // Record 245: UNTSO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNTSO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNTSO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNTSO",
                            Name = "UNTSO",
                            Description = "UNTSO United Nations Truce Supervision",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNTSO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNTSO' - UNTSO");
                        createdCount++;
                    }
                }
                
                // Record 246: MINUJUSTH
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUJUSTH");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'MINUJUSTH' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUJUSTH",
                            Name = "MINUJUSTH",
                            Description = "MINUJUSTH United Nations Mission for Justice Support in Haiti",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "MINUJUSTH",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUJUSTH' - MINUJUSTH");
                        createdCount++;
                    }
                }
                
                // Record 247: UN_DCO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DCO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_DCO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DCO",
                            Name = "UN DCO",
                            Description = "UN DCO United Nations Development Coordination Office",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_DCO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DCO' - UN DCO");
                        createdCount++;
                    }
                }
                
                // Record 248: UNGM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNGM");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNGM' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNGM",
                            Name = "UNGM",
                            Description = "UNGM United Nations Global Marketplace",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNGM",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNGM' - UNGM");
                        createdCount++;
                    }
                }
                
                // Record 249: UN_TBLDC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_TBLDC");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UN_TBLDC' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_TBLDC",
                            Name = "UN TBLDC",
                            Description = "UN Technology Bank for LDC",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_TBLDC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_TBLDC' - UN TBLDC");
                        createdCount++;
                    }
                }
                
                // Record 250: UNRCO_-_SRI_LANKA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNRCO_-_SRI_LANKA");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNRCO_-_SRI_LANKA' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNRCO_-_SRI_LANKA",
                            Name = "UNRCo - Sri Lanka",
                            Description = "United Nations Resident Coordinator Office - Sri Lanka",
                            Type = "Level_4",
                            Parent = "UN_COORD",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNRCO_-_SRI_LANKA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNRCO_-_SRI_LANKA' - UNRCo - Sri Lanka");
                        createdCount++;
                    }
                }
                
                // Record 251: UNOCT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOCT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNOCT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOCT",
                            Name = "UNOCT",
                            Description = "UNOCT United Nations Office of Counter-Terrorism",
                            Type = "Level_4",
                            Parent = "SUBSIDIARY_ORG",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNOCT",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOCT' - UNOCT");
                        createdCount++;
                    }
                }
                
                // Record 252: OSGEY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OSGEY");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OSGEY' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OSGEY",
                            Name = "OSGEY",
                            Description = "Office of the Secretary-General’s Envoy on Youth",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OSGEY",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OSGEY' - OSGEY");
                        createdCount++;
                    }
                }
                
                // Record 253: UNIRMCT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIRMCT");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNIRMCT' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIRMCT",
                            Name = "UNIRMCT",
                            Description = "UNIRMCT United Nations International Residual Mechanism for Criminal Tribunals",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNIRMCT",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIRMCT' - UNIRMCT");
                        createdCount++;
                    }
                }
                
                // Record 254: UNDP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDP");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDP' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDP",
                            Name = "UNDP",
                            Description = "UNDP United Nations Development Programme",
                            Type = "Level_4",
                            Parent = "FUND_PROGRAMME",
                            PartnerCategoryCode = "UNDP",
                            PartnerGroupCode = null,
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDP' - UNDP");
                        createdCount++;
                    }
                }
                
                // Record 255: UNDP_MPTFO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDP_MPTFO");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'UNDP_MPTFO' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDP_MPTFO",
                            Name = "UNDP MPTFO",
                            Description = "UNDP Multi-Partner Trust Fund Office",
                            Type = "Level_5",
                            Parent = "UNDP",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UNDP_MPTFO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDP_MPTFO' - UNDP MPTFO");
                        createdCount++;
                    }
                }
                
                // Record 256: IPSAS_ACCOUNTING
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IPSAS_ACCOUNTING");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'IPSAS_ACCOUNTING' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IPSAS_ACCOUNTING",
                            Name = "IPSAS Accounting",
                            Description = "IPSAS Accounting",
                            Type = "Level_4",
                            Parent = "OTHER_ENTITIES",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IPSAS_ACCOUNTING",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IPSAS_ACCOUNTING' - IPSAS Accounting");
                        createdCount++;
                    }
                }
                
                // Record 257: OTHER_PRIVATE_SECTOR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_PRIVATE_SECTOR");
                    
                    if (existingRecord != null)
                    {
                        Console.WriteLine($"Skipped: PartnerTree with Code 'OTHER_PRIVATE_SECTOR' already exists.");
                        skippedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_PRIVATE_SECTOR",
                            Name = "Other Private Sector",
                            Description = "Other Private Sector",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "OTHER_PRIVATE_SECTOR",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_PRIVATE_SECTOR' - Other Private Sector");
                        createdCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartnerTree seeding completed successfully.");
                Console.WriteLine($"Total records processed: {skippedCount + createdCount}");
                Console.WriteLine($"Records skipped (already exist): {skippedCount}");
                Console.WriteLine($"Records created: {createdCount}");
                
                // Fix audit data for newly created records
                // Note: SaveChangesAsync triggers audit interceptor which overwrites CreatedBy/LastModifiedBy
                // We need to fix these values after the transaction commits
                if (createdCount > 0)
                {
                    await FixAuditDataAsync(context, createdRecordIds);
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during PartnerTree seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        private static async Task FixAuditDataAsync(UNOPSAppDbContext context, List<int> recordIds)
        {
            Console.WriteLine("\nApplying audit data fixes to prevent LastModifiedBy overwrite...");
            
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Use ExecuteUpdateAsync to bypass audit interceptor
                // Update CreatedBy for newly created partner trees
                int createdByUpdates = await context.PartnerTrees
                    .Where(pt => recordIds.Contains(pt.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(pt => pt.CreatedBy, -1));
                
                Console.WriteLine($"Updated CreatedBy to -1 for {createdByUpdates} partner tree records");
                
                // Update LastModifiedBy for newly created partner trees
                int lastModifiedByUpdates = await context.PartnerTrees
                    .Where(pt => recordIds.Contains(pt.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(pt => pt.LastModifiedBy, -1));
                
                Console.WriteLine($"Updated LastModifiedBy to -1 for {lastModifiedByUpdates} partner tree records");
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine("Audit data fixes applied successfully.\n");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error applying audit data fixes: {ex.Message}");
                throw;
            }
        }
    }
}
