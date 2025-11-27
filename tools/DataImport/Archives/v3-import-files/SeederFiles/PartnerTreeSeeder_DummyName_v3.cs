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
    public static class PartnerTreeSeeder_DummyName_v3
    {
        public static async Task SeedPartnerTreeDummyNameAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting PartnerTree DummyName seeding process (v3)...");
            
            int updatedCount = 0;
            int createdCount = 0;
            var updatedRecordIds = new List<int>();
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
                        existingRecord.Name = "Private Sector.";
                        existingRecord.Description = "Private Sector";
                        existingRecord.Type = "Level_1";
                        existingRecord.Parent = null;
                        existingRecord.PartnerCategoryCode = "PRIVATE_SECTOR";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'PRIVATE_SECTOR' - Private Sector.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PRIVATE_SECTOR",
                            Name = "Private Sector.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'PRIVATE_SECTOR' - Private Sector.");
                        createdCount++;
                    }
                }
                
                // Record 2: CC001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CC001");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Cygnum Capital.";
                        existingRecord.Description = "Cygnum Capital Asset Management";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CC001";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CC001' - Cygnum Capital.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CC001",
                            Name = "Cygnum Capital.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CC001' - Cygnum Capital.");
                        createdCount++;
                    }
                }
                
                // Record 3: OTHER
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Other.";
                        existingRecord.Description = "Other";
                        existingRecord.Type = "Level_1";
                        existingRecord.Parent = null;
                        existingRecord.PartnerCategoryCode = "OTHER";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OTHER' - Other.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER",
                            Name = "Other.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER' - Other.");
                        createdCount++;
                    }
                }
                
                // Record 4: COG01
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "COG01");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Comité Olimpico Guatemalteco.";
                        existingRecord.Description = "Comité Olimpico Guatemalteco (COG)";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "OTHER";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "COG01";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'COG01' - Comité Olimpico Guatemalteco.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "COG01",
                            Name = "Comité Olimpico Guatemalteco.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'COG01' - Comité Olimpico Guatemalteco.");
                        createdCount++;
                    }
                }
                
                // Record 5: MULTILATERAL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MULTILATERAL");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Multilateral.";
                        existingRecord.Description = "Multilateral";
                        existingRecord.Type = "Level_1";
                        existingRecord.Parent = null;
                        existingRecord.PartnerCategoryCode = "MULTILATERAL";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MULTILATERAL' - Multilateral.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MULTILATERAL",
                            Name = "Multilateral.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MULTILATERAL' - Multilateral.");
                        createdCount++;
                    }
                }
                
                // Record 6: IFI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IFI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IFI.";
                        existingRecord.Description = "IFI International Financial Institutions";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "MULTILATERAL";
                        existingRecord.PartnerCategoryCode = "IFI";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IFI' - IFI.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IFI",
                            Name = "IFI.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IFI' - IFI.");
                        createdCount++;
                    }
                }
                
                // Record 7: REG_OTH_FI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Reg & other Financial Insitutions.";
                        existingRecord.Description = "Regional and other Financial Insitutions";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "IFI";
                        existingRecord.PartnerCategoryCode = "REG_OTH_FI";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'REG_OTH_FI' - Reg & other Financial Insitutions.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REG_OTH_FI",
                            Name = "Reg & other Financial Insitutions.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'REG_OTH_FI' - Reg & other Financial Insitutions.");
                        createdCount++;
                    }
                }
                
                // Record 8: CAF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CAF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CAF.";
                        existingRecord.Description = "CAF Development Bank of Latin America";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CAF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CAF' - CAF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CAF",
                            Name = "CAF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CAF' - CAF.");
                        createdCount++;
                    }
                }
                
                // Record 9: IMF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IMF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IMF.";
                        existingRecord.Description = "IMF International Monetary Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IMF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IMF' - IMF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IMF",
                            Name = "IMF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IMF' - IMF.");
                        createdCount++;
                    }
                }
                
                // Record 10: AFDB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AFDB");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "AFDB.";
                        existingRecord.Description = "AfDB African Development Bank";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "AFDB";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'AFDB' - AFDB.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AFDB",
                            Name = "AFDB.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'AFDB' - AFDB.");
                        createdCount++;
                    }
                }
                
                // Record 11: ADB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ADB");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "ADB.";
                        existingRecord.Description = "ADB Asian Development Bank";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ADB";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ADB' - ADB.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ADB",
                            Name = "ADB.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ADB' - ADB.");
                        createdCount++;
                    }
                }
                
                // Record 12: CDB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CDB");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CDB.";
                        existingRecord.Description = "CDB Caribbean Development Bank";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CDB";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CDB' - CDB.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CDB",
                            Name = "CDB.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CDB' - CDB.");
                        createdCount++;
                    }
                }
                
                // Record 13: CFC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CFC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CFC.";
                        existingRecord.Description = "CFC Common Fund for Commodities";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CFC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CFC' - CFC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CFC",
                            Name = "CFC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CFC' - CFC.");
                        createdCount++;
                    }
                }
                
                // Record 14: EBRD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EBRD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EBRD.";
                        existingRecord.Description = "EBRD European Bank for Reconstruction and Development";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EBRD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EBRD' - EBRD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EBRD",
                            Name = "EBRD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EBRD' - EBRD.");
                        createdCount++;
                    }
                }
                
                // Record 15: IsDB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IsDB");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IsDB.";
                        existingRecord.Description = "IsDB Islamic Development Bank";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IsDB";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IsDB' - IsDB.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IsDB",
                            Name = "IsDB.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IsDB' - IsDB.");
                        createdCount++;
                    }
                }
                
                // Record 16: AFESD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AFESD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "AFESD.";
                        existingRecord.Description = "AFESD Arab Fund for Economic and Social Development";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "AFESD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'AFESD' - AFESD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AFESD",
                            Name = "AFESD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'AFESD' - AFESD.");
                        createdCount++;
                    }
                }
                
                // Record 17: AIIB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AIIB");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "AIIB.";
                        existingRecord.Description = "AIIB Asian Infrastructure Investment Bank";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "AIIB";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'AIIB' - AIIB.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AIIB",
                            Name = "AIIB.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'AIIB' - AIIB.");
                        createdCount++;
                    }
                }
                
                // Record 18: OFID
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OFID");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "OFID.";
                        existingRecord.Description = "OFID OPEC Fund for International Development";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OFID";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OFID' - OFID.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OFID",
                            Name = "OFID.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OFID' - OFID.");
                        createdCount++;
                    }
                }
                
                // Record 19: BOAD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BOAD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "BOAD.";
                        existingRecord.Description = "West African Development Bank";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_OTH_FI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "BOAD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'BOAD' - BOAD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BOAD",
                            Name = "BOAD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'BOAD' - BOAD.");
                        createdCount++;
                    }
                }
                
                // Record 20: MAI001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MAI001");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Maisha.";
                        existingRecord.Description = "Maisha";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MAI001";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MAI001' - Maisha.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MAI001",
                            Name = "Maisha.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MAI001' - Maisha.");
                        createdCount++;
                    }
                }
                
                // Record 21: MPI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MPI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MPI.";
                        existingRecord.Description = "Multi-partner initiatives";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "MULTILATERAL";
                        existingRecord.PartnerCategoryCode = "MPI";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MPI' - MPI.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MPI",
                            Name = "MPI.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MPI' - MPI.");
                        createdCount++;
                    }
                }
                
                // Record 22: EIF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EIF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EIF.";
                        existingRecord.Description = "EIF Enhanced Integrated Framework";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "MPI";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EIF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EIF' - EIF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EIF",
                            Name = "EIF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EIF' - EIF.");
                        createdCount++;
                    }
                }
                
                // Record 23: NGO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NGO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "NGO.";
                        existingRecord.Description = "Non-governmental Organizations";
                        existingRecord.Type = "Level_1";
                        existingRecord.Parent = null;
                        existingRecord.PartnerCategoryCode = "NGO";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'NGO' - NGO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NGO",
                            Name = "NGO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'NGO' - NGO.");
                        createdCount++;
                    }
                }
                
                // Record 24: NEH001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NEH001");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Nehemia.";
                        existingRecord.Description = "Nehemia";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "NGO";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "NEH001";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'NEH001' - Nehemia.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NEH001",
                            Name = "Nehemia.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'NEH001' - Nehemia.");
                        createdCount++;
                    }
                }
                
                // Record 25: GOVERNMENT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Government.";
                        existingRecord.Description = "Government";
                        existingRecord.Type = "Level_1";
                        existingRecord.Parent = null;
                        existingRecord.PartnerCategoryCode = "GOVERNMENT";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'GOVERNMENT' - Government.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "GOVERNMENT",
                            Name = "Government.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'GOVERNMENT' - Government.");
                        createdCount++;
                    }
                }
                
                // Record 26: NON_OECD_DAC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NON_OECD_DAC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Gov: Non-OECD/DAC.";
                        existingRecord.Description = "Non-OECD/DAC Government";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "GOVERNMENT";
                        existingRecord.PartnerCategoryCode = "NON_OECD_DAC";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'NON_OECD_DAC' - Gov: Non-OECD/DAC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NON_OECD_DAC",
                            Name = "Gov: Non-OECD/DAC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'NON_OECD_DAC' - Gov: Non-OECD/DAC.");
                        createdCount++;
                    }
                }
                
                // Record 27: PNG001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PNG001");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Papua New Guinea.";
                        existingRecord.Description = "Papua New Guinea";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "NON_OECD_DAC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "PNG001";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'PNG001' - Papua New Guinea.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PNG001",
                            Name = "Papua New Guinea.",
                            Description = "Papua New Guinea",
                            Type = "Level_3",
                            Parent = "NON_OECD_DAC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "PNG001",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'PNG001' - Papua New Guinea.");
                        createdCount++;
                    }
                }
                
                // Record 28: PAR001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PAR001");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Parexel.";
                        existingRecord.Description = "Parexel";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "PAR001";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'PAR001' - Parexel.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PAR001",
                            Name = "Parexel.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'PAR001' - Parexel.");
                        createdCount++;
                    }
                }
                
                // Record 29: REG_OTH_INGO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_INGO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Regional & Other IGO.";
                        existingRecord.Description = "Regional and other Intergovernmental Organizations";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "MULTILATERAL";
                        existingRecord.PartnerCategoryCode = "REG_OTH_INGO";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'REG_OTH_INGO' - Regional & Other IGO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REG_OTH_INGO",
                            Name = "Regional & Other IGO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'REG_OTH_INGO' - Regional & Other IGO.");
                        createdCount++;
                    }
                }
                
                // Record 30: EU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EU.";
                        existingRecord.Description = "EU European Union";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "REG_OTH_INGO";
                        existingRecord.PartnerCategoryCode = "EU";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EU' - EU.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EU",
                            Name = "EU.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EU' - EU.");
                        createdCount++;
                    }
                }
                
                // Record 31: EC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EC.";
                        existingRecord.Description = "EC European Commission";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "EU";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EC' - EC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EC",
                            Name = "EC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EC' - EC.");
                        createdCount++;
                    }
                }
                
                // Record 32: AU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "AU.";
                        existingRecord.Description = "AU African Union";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "REG_OTH_INGO";
                        existingRecord.PartnerCategoryCode = "AU";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'AU' - AU.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AU",
                            Name = "AU.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'AU' - AU.");
                        createdCount++;
                    }
                }
                
                // Record 33: UNAMID
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAMID");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNAMID.";
                        existingRecord.Description = "UNAMID African Union-United Nations Hybrid Operation in Darfur";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "AU";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNAMID";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNAMID' - UNAMID.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAMID",
                            Name = "UNAMID.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAMID' - UNAMID.");
                        createdCount++;
                    }
                }
                
                // Record 34: EBY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EBY");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EBY.";
                        existingRecord.Description = "EBY Entidad Binacional Yacyretá";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "REG_OTH_INGO";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EBY";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EBY' - EBY.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EBY",
                            Name = "EBY.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EBY' - EBY.");
                        createdCount++;
                    }
                }
                
                // Record 35: EU_DG_MENA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EU_DG_MENA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EU DG MENA.";
                        existingRecord.Description = "EU DG MENA, Directorate-General for the Middle East, North Africa and the Gulf";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "EU";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EU_DG_MENA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EU_DG_MENA' - EU DG MENA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EU_DG_MENA",
                            Name = "EU DG MENA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EU_DG_MENA' - EU DG MENA.");
                        createdCount++;
                    }
                }
                
                // Record 36: EU_DG_CLIMA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EU_DG_CLIMA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EU DG CLIMA.";
                        existingRecord.Description = "EU DG CLIMA, Directorate-General for Climate Action";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "EU";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EU_DG_CLIMA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EU_DG_CLIMA' - EU DG CLIMA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EU_DG_CLIMA",
                            Name = "EU DG CLIMA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EU_DG_CLIMA' - EU DG CLIMA.");
                        createdCount++;
                    }
                }
                
                // Record 37: ACADEMIC_TRAINING_RESEARC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ACADEMIC_TRAINING_RESEARC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Academic, Training and Research.";
                        existingRecord.Description = "Academic, Training and Research";
                        existingRecord.Type = "Level_1";
                        existingRecord.Parent = null;
                        existingRecord.PartnerCategoryCode = "ACADEMIC_TRAINING_RESEARC";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ACADEMIC_TRAINING_RESEARC' - Academic, Training and Research.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ACADEMIC_TRAINING_RESEARC",
                            Name = "Academic, Training and Research.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ACADEMIC_TRAINING_RESEARC' - Academic, Training and Research.");
                        createdCount++;
                    }
                }
                
                // Record 38: UCD001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UCD001");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UC Davis.";
                        existingRecord.Description = "UC Davis";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "ACADEMIC_TRAINING_RESEARC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UCD001";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UCD001' - UC Davis.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UCD001",
                            Name = "UC Davis.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UCD001' - UC Davis.");
                        createdCount++;
                    }
                }
                
                // Record 39: UNITED_NATIONS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNITED_NATIONS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN.";
                        existingRecord.Description = "United Nations";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "MULTILATERAL";
                        existingRecord.PartnerCategoryCode = "UNITED_NATIONS";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNITED_NATIONS' - UN.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNITED_NATIONS",
                            Name = "UN.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNITED_NATIONS' - UN.");
                        createdCount++;
                    }
                }
                
                // Record 40: UN_INTER_POOLED_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_INTER_POOLED_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN inter-agency pooled funds incl. JPs.";
                        existingRecord.Description = "United Nations inter-agency pooled funds incl. Joint Programmes";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_INTER_POOLED_FUND' - UN inter-agency pooled funds incl. JPs.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_INTER_POOLED_FUND",
                            Name = "UN inter-agency pooled funds incl. JPs.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_INTER_POOLED_FUND' - UN inter-agency pooled funds incl. JPs.");
                        createdCount++;
                    }
                }
                
                // Record 41: SSHF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SSHF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "SSHF.";
                        existingRecord.Description = "SSHF South Sudan Common Humanitarian Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SSHF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SSHF' - SSHF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SSHF",
                            Name = "SSHF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SSHF' - SSHF.");
                        createdCount++;
                    }
                }
                
                // Record 42: EBOLA_RESPONSE_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EBOLA_RESPONSE_MPTF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Ebola Response MPTF.";
                        existingRecord.Description = "Ebola Response MPTF";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EBOLA_RESPONSE_MPTF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'EBOLA_RESPONSE_MPTF' - Ebola Response MPTF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "EBOLA_RESPONSE_MPTF",
                            Name = "Ebola Response MPTF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'EBOLA_RESPONSE_MPTF' - Ebola Response MPTF.");
                        createdCount++;
                    }
                }
                
                // Record 43: SYRIA_EMERGENCY_RESPONSE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SYRIA_EMERGENCY_RESPONSE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Syria Emergency Response Fund.";
                        existingRecord.Description = "Syria Emergency Response Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SYRIA_EMERGENCY_RESPONSE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SYRIA_EMERGENCY_RESPONSE' - Syria Emergency Response Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SYRIA_EMERGENCY_RESPONSE",
                            Name = "Syria Emergency Response Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SYRIA_EMERGENCY_RESPONSE' - Syria Emergency Response Fund.");
                        createdCount++;
                    }
                }
                
                // Record 44: SOMALIA_UN_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SOMALIA_UN_MPTF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Somalia UN MPTF.";
                        existingRecord.Description = "UN Multi-Partner Trust Fund for Somalia (Somalia UN MPTF)";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SOMALIA_UN_MPTF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SOMALIA_UN_MPTF' - Somalia UN MPTF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SOMALIA_UN_MPTF",
                            Name = "Somalia UN MPTF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SOMALIA_UN_MPTF' - Somalia UN MPTF.");
                        createdCount++;
                    }
                }
                
                // Record 45: UNDF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDF.";
                        existingRecord.Description = "UNDF United Nations Fund for Recovery Reconstruction and Development in Darfur";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDF' - UNDF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDF",
                            Name = "UNDF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDF' - UNDF.");
                        createdCount++;
                    }
                }
                
                // Record 46: UN_GENERAL_TRUST_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_GENERAL_TRUST_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN General Trust Fund.";
                        existingRecord.Description = "UN General Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_GENERAL_TRUST_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_GENERAL_TRUST_FUND' - UN General Trust Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_GENERAL_TRUST_FUND",
                            Name = "UN General Trust Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_GENERAL_TRUST_FUND' - UN General Trust Fund.");
                        createdCount++;
                    }
                }
                
                // Record 47: CERF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CERF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CERF.";
                        existingRecord.Description = "CERF Central Emergency Response Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CERF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CERF' - CERF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CERF",
                            Name = "CERF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CERF' - CERF.");
                        createdCount++;
                    }
                }
                
                // Record 48: UNPBF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNPBF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNPBF.";
                        existingRecord.Description = "UNPBF United Nations Peacebuilding Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNPBF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNPBF' - UNPBF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNPBF",
                            Name = "UNPBF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNPBF' - UNPBF.");
                        createdCount++;
                    }
                }
                
                // Record 49: UNVFTC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNVFTC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNVFTC.";
                        existingRecord.Description = "UNVFTC United Nations Voluntary Fund for Technical Co-operation in the Field of Human Rights";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNVFTC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNVFTC' - UNVFTC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNVFTC",
                            Name = "UNVFTC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNVFTC' - UNVFTC.");
                        createdCount++;
                    }
                }
                
                // Record 50: UNVFVT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNVFVT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNVFVT.";
                        existingRecord.Description = "UNVFVT United Nations Voluntary Fund for Victims of Torture";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNVFVT";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNVFVT' - UNVFVT.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNVFVT",
                            Name = "UNVFVT.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNVFVT' - UNVFVT.");
                        createdCount++;
                    }
                }
                
                // Record 51: UNVFD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNVFD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNVFD.";
                        existingRecord.Description = "UNVFD United Nations Voluntary Fund on Disability";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNVFD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNVFD' - UNVFD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNVFD",
                            Name = "UNVFD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNVFD' - UNVFD.");
                        createdCount++;
                    }
                }
                
                // Record 52: UNDEF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDEF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDEF.";
                        existingRecord.Description = "UNDEF United Nations Democracy Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDEF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDEF' - UNDEF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDEF",
                            Name = "UNDEF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDEF' - UNDEF.");
                        createdCount++;
                    }
                }
                
                // Record 53: UNFIP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFIP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNFIP.";
                        existingRecord.Description = "UNFIP United Nations Fund for International Partnerships";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNFIP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNFIP' - UNFIP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFIP",
                            Name = "UNFIP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFIP' - UNFIP.");
                        createdCount++;
                    }
                }
                
                // Record 54: UN-WATER
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-WATER");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN-Water.";
                        existingRecord.Description = "UN-Water Inter-agency Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN-WATER";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN-WATER' - UN-Water.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-WATER",
                            Name = "UN-Water.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-WATER' - UN-Water.");
                        createdCount++;
                    }
                }
                
                // Record 55: ALBANIA_ONE_UNCF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ALBANIA_ONE_UNCF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Albania One UNCF.";
                        existingRecord.Description = "Albania One UN Coherence Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ALBANIA_ONE_UNCF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ALBANIA_ONE_UNCF' - Albania One UNCF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ALBANIA_ONE_UNCF",
                            Name = "Albania One UNCF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ALBANIA_ONE_UNCF' - Albania One UNCF.");
                        createdCount++;
                    }
                }
                
                // Record 56: BHUTAN_UNCF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BHUTAN_UNCF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Bhutan UNCF.";
                        existingRecord.Description = "Bhutan UN Country Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "BHUTAN_UNCF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'BHUTAN_UNCF' - Bhutan UNCF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BHUTAN_UNCF",
                            Name = "Bhutan UNCF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'BHUTAN_UNCF' - Bhutan UNCF.");
                        createdCount++;
                    }
                }
                
                // Record 57: BOTSWANA_UNCF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BOTSWANA_UNCF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Botswana UNCF.";
                        existingRecord.Description = "Botswana UN Country Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "BOTSWANA_UNCF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'BOTSWANA_UNCF' - Botswana UNCF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BOTSWANA_UNCF",
                            Name = "Botswana UNCF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'BOTSWANA_UNCF' - Botswana UNCF.");
                        createdCount++;
                    }
                }
                
                // Record 58: CAPE_VERDE_TRANSITION_FU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CAPE_VERDE_TRANSITION_FU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Cape Verde Transition Fund.";
                        existingRecord.Description = "Cape Verde Transition Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CAPE_VERDE_TRANSITION_FU";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CAPE_VERDE_TRANSITION_FU' - Cape Verde Transition Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CAPE_VERDE_TRANSITION_FU",
                            Name = "Cape Verde Transition Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CAPE_VERDE_TRANSITION_FU' - Cape Verde Transition Fund.");
                        createdCount++;
                    }
                }
                
                // Record 59: CAR_HF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CAR_HF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CAR HF.";
                        existingRecord.Description = "Central African Republic Common Humanitarian Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CAR_HF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CAR_HF' - CAR HF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CAR_HF",
                            Name = "CAR HF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CAR_HF' - CAR HF.");
                        createdCount++;
                    }
                }
                
                // Record 60: CFIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CFIA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CFIA.";
                        existingRecord.Description = "CFIA United Nations Central Fund for Influenza Action";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CFIA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CFIA' - CFIA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CFIA",
                            Name = "CFIA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CFIA' - CFIA.");
                        createdCount++;
                    }
                }
                
                // Record 61: CBA_CC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CBA_CC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CBA CC.";
                        existingRecord.Description = "Community-based Based Adaptation to Climate Change";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CBA_CC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CBA_CC' - CBA CC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CBA_CC",
                            Name = "CBA CC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CBA_CC' - CBA CC.");
                        createdCount++;
                    }
                }
                
                // Record 62: COMOROS_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "COMOROS_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Comoros One UN Fund.";
                        existingRecord.Description = "Comoros One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "COMOROS_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'COMOROS_ONE_UN_FUND' - Comoros One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "COMOROS_ONE_UN_FUND",
                            Name = "Comoros One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'COMOROS_ONE_UN_FUND' - Comoros One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 63: DCPSF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DCPSF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "DCPSF.";
                        existingRecord.Description = "DCPSF Darfur Community Peace and Stability Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "DCPSF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'DCPSF' - DCPSF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DCPSF",
                            Name = "DCPSF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'DCPSF' - DCPSF.");
                        createdCount++;
                    }
                }
                
                // Record 64: DRC_POOLED_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DRC_POOLED_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "DRC Pooled Fund.";
                        existingRecord.Description = "DRC Pooled Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "DRC_POOLED_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'DRC_POOLED_FUND' - DRC Pooled Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DRC_POOLED_FUND",
                            Name = "DRC Pooled Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'DRC_POOLED_FUND' - DRC Pooled Fund.");
                        createdCount++;
                    }
                }
                
                // Record 65: DRC_STABILIZATION_AND_RE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DRC_STABILIZATION_AND_RE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "DRC Stabilization and Recovery.";
                        existingRecord.Description = "DRC Stabilization and Recovery";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "DRC_STABILIZATION_AND_RE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'DRC_STABILIZATION_AND_RE' - DRC Stabilization and Recovery.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DRC_STABILIZATION_AND_RE",
                            Name = "DRC Stabilization and Recovery.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'DRC_STABILIZATION_AND_RE' - DRC Stabilization and Recovery.");
                        createdCount++;
                    }
                }
                
                // Record 66: ETHIOPIA_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ETHIOPIA_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Ethiopia One UN Fund.";
                        existingRecord.Description = "Ethiopia One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ETHIOPIA_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ETHIOPIA_ONE_UN_FUND' - Ethiopia One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ETHIOPIA_ONE_UN_FUND",
                            Name = "Ethiopia One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ETHIOPIA_ONE_UN_FUND' - Ethiopia One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 67: HRM_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "HRM_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "HRM Fund.";
                        existingRecord.Description = "Human Rights Mainstreaming Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "HRM_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'HRM_FUND' - HRM Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "HRM_FUND",
                            Name = "HRM Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'HRM_FUND' - HRM Fund.");
                        createdCount++;
                    }
                }
                
                // Record 68: INDONESIA_DR_TF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "INDONESIA_DR_TF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Indonesia DR TF.";
                        existingRecord.Description = "Indonesia Disaster Recovery Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "INDONESIA_DR_TF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'INDONESIA_DR_TF' - Indonesia DR TF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "INDONESIA_DR_TF",
                            Name = "Indonesia DR TF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'INDONESIA_DR_TF' - Indonesia DR TF.");
                        createdCount++;
                    }
                }
                
                // Record 69: IRAQ_UNDAF_TRUST_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IRAQ_UNDAF_TRUST_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Iraq UNDAF Trust Fund.";
                        existingRecord.Description = "Iraq UNDAF Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IRAQ_UNDAF_TRUST_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IRAQ_UNDAF_TRUST_FUND' - Iraq UNDAF Trust Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IRAQ_UNDAF_TRUST_FUND",
                            Name = "Iraq UNDAF Trust Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IRAQ_UNDAF_TRUST_FUND' - Iraq UNDAF Trust Fund.");
                        createdCount++;
                    }
                }
                
                // Record 70: JP_ARMED_VIOLENCE_PREVEN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_ARMED_VIOLENCE_PREVEN");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Armed Violence Prevention.";
                        existingRecord.Description = "JP Armed Violence Prevention";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_ARMED_VIOLENCE_PREVEN";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_ARMED_VIOLENCE_PREVEN' - JP Armed Violence Prevention.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_ARMED_VIOLENCE_PREVEN",
                            Name = "JP Armed Violence Prevention.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_ARMED_VIOLENCE_PREVEN' - JP Armed Violence Prevention.");
                        createdCount++;
                    }
                }
                
                // Record 71: JP_BANGLADESH_LGSP–LIC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_BANGLADESH_LGSP–LIC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Bangladesh LGSP–LIC.";
                        existingRecord.Description = "JP LGSP-LIC Bangladesh Local Governance Support Project – Learning and Innovation Component";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_BANGLADESH_LGSP–LIC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_BANGLADESH_LGSP–LIC' - JP Bangladesh LGSP–LIC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_BANGLADESH_LGSP–LIC",
                            Name = "JP Bangladesh LGSP–LIC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_BANGLADESH_LGSP–LIC' - JP Bangladesh LGSP–LIC.");
                        createdCount++;
                    }
                }
                
                // Record 72: JP_CHAD_DIS_SECURITY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_CHAD_DIS_SECURITY");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Chad DIS Security.";
                        existingRecord.Description = "JP Chad DIS Security";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_CHAD_DIS_SECURITY";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_CHAD_DIS_SECURITY' - JP Chad DIS Security.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_CHAD_DIS_SECURITY",
                            Name = "JP Chad DIS Security.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_CHAD_DIS_SECURITY' - JP Chad DIS Security.");
                        createdCount++;
                    }
                }
                
                // Record 73: JP_DRC_MICROFINANCE_II
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_DRC_MICROFINANCE_II");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP DRC Microfinance II.";
                        existingRecord.Description = "JP DRC Microfinance II";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_DRC_MICROFINANCE_II";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_DRC_MICROFINANCE_II' - JP DRC Microfinance II.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_DRC_MICROFINANCE_II",
                            Name = "JP DRC Microfinance II.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_DRC_MICROFINANCE_II' - JP DRC Microfinance II.");
                        createdCount++;
                    }
                }
                
                // Record 74: JP_DRC_SECURITY_SECT_REF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_DRC_SECURITY_SECT_REF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP DRC Security Sect Reform.";
                        existingRecord.Description = "JP DRC Security Sect Reform";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_DRC_SECURITY_SECT_REF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_DRC_SECURITY_SECT_REF' - JP DRC Security Sect Reform.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_DRC_SECURITY_SECT_REF",
                            Name = "JP DRC Security Sect Reform.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_DRC_SECURITY_SECT_REF' - JP DRC Security Sect Reform.");
                        createdCount++;
                    }
                }
                
                // Record 75: JP_GUATEMALA_MAYA_PROGRA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_GUATEMALA_MAYA_PROGRA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Guatemala Maya Programme.";
                        existingRecord.Description = "JP Guatemala Maya Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_GUATEMALA_MAYA_PROGRA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_GUATEMALA_MAYA_PROGRA' - JP Guatemala Maya Programme.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_GUATEMALA_MAYA_PROGRA",
                            Name = "JP Guatemala Maya Programme.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_GUATEMALA_MAYA_PROGRA' - JP Guatemala Maya Programme.");
                        createdCount++;
                    }
                }
                
                // Record 76: JP_GUATEMALA_RURAL_DEV
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_GUATEMALA_RURAL_DEV");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Guatemala Rural Dev.";
                        existingRecord.Description = "JP Guatemala Rural Dev";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_GUATEMALA_RURAL_DEV";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_GUATEMALA_RURAL_DEV' - JP Guatemala Rural Dev.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_GUATEMALA_RURAL_DEV",
                            Name = "JP Guatemala Rural Dev.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_GUATEMALA_RURAL_DEV' - JP Guatemala Rural Dev.");
                        createdCount++;
                    }
                }
                
                // Record 77: JP_KAZAKHSTAN_INNOV_APRC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_KAZAKHSTAN_INNOV_APRC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Kazakhstan Innov Aprch RPSS.";
                        existingRecord.Description = "JP Kazakhstan Innov Aprch RPSS";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_KAZAKHSTAN_INNOV_APRC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_KAZAKHSTAN_INNOV_APRC' - JP Kazakhstan Innov Aprch RPSS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_KAZAKHSTAN_INNOV_APRC",
                            Name = "JP Kazakhstan Innov Aprch RPSS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_KAZAKHSTAN_INNOV_APRC' - JP Kazakhstan Innov Aprch RPSS.");
                        createdCount++;
                    }
                }
                
                // Record 78: JP_KENYA_HIV_AND_AIDS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_KENYA_HIV_AND_AIDS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Kenya HIV and AIDS.";
                        existingRecord.Description = "JP Kenya HIV and AIDS";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_KENYA_HIV_AND_AIDS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_KENYA_HIV_AND_AIDS' - JP Kenya HIV and AIDS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_KENYA_HIV_AND_AIDS",
                            Name = "JP Kenya HIV and AIDS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_KENYA_HIV_AND_AIDS' - JP Kenya HIV and AIDS.");
                        createdCount++;
                    }
                }
                
                // Record 79: JP_KOSOVO_DOMESTIC_VIOLE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_KOSOVO_DOMESTIC_VIOLE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Kosovo Domestic Violence.";
                        existingRecord.Description = "JP Kosovo Domestic Violence";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_KOSOVO_DOMESTIC_VIOLE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_KOSOVO_DOMESTIC_VIOLE' - JP Kosovo Domestic Violence.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_KOSOVO_DOMESTIC_VIOLE",
                            Name = "JP Kosovo Domestic Violence.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_KOSOVO_DOMESTIC_VIOLE' - JP Kosovo Domestic Violence.");
                        createdCount++;
                    }
                }
                
                // Record 80: JP_LAO_GOVERN/PUBLIC_ADM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_LAO_GOVERN/PUBLIC_ADM" || pt.Code == "JP_LAO_GOVERN_PUBLIC_ADM");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Code = "JP_LAO_GOVERN_PUBLIC_ADM";
                        existingRecord.Name = "JP Lao Govern/Public Admin.";
                        existingRecord.Description = "JP Lao Governance and Public Administration Reform";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_LAO_GOVERN_PUBLIC_ADM";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_LAO_GOVERN_PUBLIC_ADM' - JP Lao Govern/Public Admin.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_LAO_GOVERN/PUBLIC_ADM",
                            Name = "JP Lao Govern/Public Admin.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_LAO_GOVERN/PUBLIC_ADM' - JP Lao Govern/Public Admin.");
                        createdCount++;
                    }
                }
                
                // Record 81: JP_LIBERIA_FOOD_SECURITY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_LIBERIA_FOOD_SECURITY");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Liberia Food Security.";
                        existingRecord.Description = "JP Liberia Food Security";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_LIBERIA_FOOD_SECURITY";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_LIBERIA_FOOD_SECURITY' - JP Liberia Food Security.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_LIBERIA_FOOD_SECURITY",
                            Name = "JP Liberia Food Security.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_LIBERIA_FOOD_SECURITY' - JP Liberia Food Security.");
                        createdCount++;
                    }
                }
                
                // Record 82: JP_LIBERIA_GENDER_EQUALI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_LIBERIA_GENDER_EQUALI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Liberia Gender Equality.";
                        existingRecord.Description = "JP Liberia Gender Equality";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_LIBERIA_GENDER_EQUALI";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_LIBERIA_GENDER_EQUALI' - JP Liberia Gender Equality.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_LIBERIA_GENDER_EQUALI",
                            Name = "JP Liberia Gender Equality.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_LIBERIA_GENDER_EQUALI' - JP Liberia Gender Equality.");
                        createdCount++;
                    }
                }
                
                // Record 83: JP_MALI_AGRO_PASTORAL_PR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_MALI_AGRO_PASTORAL_PR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Mali Agro Pastoral Products.";
                        existingRecord.Description = "JP Mali Agro Pastoral Products";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_MALI_AGRO_PASTORAL_PR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_MALI_AGRO_PASTORAL_PR' - JP Mali Agro Pastoral Products.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_MALI_AGRO_PASTORAL_PR",
                            Name = "JP Mali Agro Pastoral Products.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_MALI_AGRO_PASTORAL_PR' - JP Mali Agro Pastoral Products.");
                        createdCount++;
                    }
                }
                
                // Record 84: JP_MOLDOVA_JILDP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_MOLDOVA_JILDP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Moldova JILDP.";
                        existingRecord.Description = "JP Moldova Integrated Local Development Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_MOLDOVA_JILDP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_MOLDOVA_JILDP' - JP Moldova JILDP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_MOLDOVA_JILDP",
                            Name = "JP Moldova JILDP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_MOLDOVA_JILDP' - JP Moldova JILDP.");
                        createdCount++;
                    }
                }
                
                // Record 85: JP_NEPAL_LGCDP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_NEPAL_LGCDP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Nepal LGCDP.";
                        existingRecord.Description = "JP Nepal LGCDP Local Governance and Community Development Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_NEPAL_LGCDP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_NEPAL_LGCDP' - JP Nepal LGCDP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_NEPAL_LGCDP",
                            Name = "JP Nepal LGCDP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_NEPAL_LGCDP' - JP Nepal LGCDP.");
                        createdCount++;
                    }
                }
                
                // Record 86: JP_SERBIA_SCILD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_SERBIA_SCILD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Serbia SCILD.";
                        existingRecord.Description = "JP Serbia SCILD Strengthening Capacity for Inclusive Local Development";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_SERBIA_SCILD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_SERBIA_SCILD' - JP Serbia SCILD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_SERBIA_SCILD",
                            Name = "JP Serbia SCILD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_SERBIA_SCILD' - JP Serbia SCILD.");
                        createdCount++;
                    }
                }
                
                // Record 87: JP_SOLOMON_ISLANDS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_SOLOMON_ISLANDS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Solomon Islands.";
                        existingRecord.Description = "JP Solomon Islands PGSP Provincial Governance Strengthening Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_SOLOMON_ISLANDS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_SOLOMON_ISLANDS' - JP Solomon Islands.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_SOLOMON_ISLANDS",
                            Name = "JP Solomon Islands.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_SOLOMON_ISLANDS' - JP Solomon Islands.");
                        createdCount++;
                    }
                }
                
                // Record 88: JP_SOMALIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_SOMALIA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Somalia.";
                        existingRecord.Description = "JP Somalia Local Governance and Decentralized Service Delivery";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_SOMALIA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_SOMALIA' - JP Somalia.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_SOMALIA",
                            Name = "JP Somalia.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_SOMALIA' - JP Somalia.");
                        createdCount++;
                    }
                }
                
                // Record 89: JP_MACEDONIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_MACEDONIA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Macedonia.";
                        existingRecord.Description = "JP TFYR SNC PDV Macedonia Strengthening National Capacities to Prevent Domestic Violence";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_MACEDONIA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_MACEDONIA' - JP Macedonia.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_MACEDONIA",
                            Name = "JP Macedonia.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_MACEDONIA' - JP Macedonia.");
                        createdCount++;
                    }
                }
                
                // Record 90: JP_TIMOR-LESTE_INFUSE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_TIMOR-LESTE_INFUSE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Timor-Leste INFUSE.";
                        existingRecord.Description = "JP Timor-Leste INFUSE Inclusive Finance for Under-Served Economy";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_TIMOR-LESTE_INFUSE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_TIMOR-LESTE_INFUSE' - JP Timor-Leste INFUSE.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_TIMOR-LESTE_INFUSE",
                            Name = "JP Timor-Leste INFUSE.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_TIMOR-LESTE_INFUSE' - JP Timor-Leste INFUSE.");
                        createdCount++;
                    }
                }
                
                // Record 91: JP_TIMOR-LESTE_LGSP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_TIMOR-LESTE_LGSP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Timor-Leste LGSP.";
                        existingRecord.Description = "JP Timor-Leste LGSP Local Governance Support Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_TIMOR-LESTE_LGSP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_TIMOR-LESTE_LGSP' - JP Timor-Leste LGSP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_TIMOR-LESTE_LGSP",
                            Name = "JP Timor-Leste LGSP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_TIMOR-LESTE_LGSP' - JP Timor-Leste LGSP.");
                        createdCount++;
                    }
                }
                
                // Record 92: JP_UGANDA_GENDER_EQUALIT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_UGANDA_GENDER_EQUALIT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Uganda Gender Equality.";
                        existingRecord.Description = "JP Uganda Gender Equality";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_UGANDA_GENDER_EQUALIT";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_UGANDA_GENDER_EQUALIT' - JP Uganda Gender Equality.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_UGANDA_GENDER_EQUALIT",
                            Name = "JP Uganda Gender Equality.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_UGANDA_GENDER_EQUALIT' - JP Uganda Gender Equality.");
                        createdCount++;
                    }
                }
                
                // Record 93: JP_UGANDA_SUPPORT_FOR_AI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JP_UGANDA_SUPPORT_FOR_AI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "JP Uganda Support for AIDS.";
                        existingRecord.Description = "JP Uganda Support for AIDS";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JP_UGANDA_SUPPORT_FOR_AI";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JP_UGANDA_SUPPORT_FOR_AI' - JP Uganda Support for AIDS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JP_UGANDA_SUPPORT_FOR_AI",
                            Name = "JP Uganda Support for AIDS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'JP_UGANDA_SUPPORT_FOR_AI' - JP Uganda Support for AIDS.");
                        createdCount++;
                    }
                }
                
                // Record 94: KIRIBATI_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "KIRIBATI_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Kiribati One UN Fund.";
                        existingRecord.Description = "Kiribati One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "KIRIBATI_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'KIRIBATI_ONE_UN_FUND' - Kiribati One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "KIRIBATI_ONE_UN_FUND",
                            Name = "Kiribati One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'KIRIBATI_ONE_UN_FUND' - Kiribati One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 95: KYRGYZSTAN_ONE_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "KYRGYZSTAN_ONE_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Kyrgyzstan One Fund.";
                        existingRecord.Description = "Kyrgyzstan One Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "KYRGYZSTAN_ONE_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'KYRGYZSTAN_ONE_FUND' - Kyrgyzstan One Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "KYRGYZSTAN_ONE_FUND",
                            Name = "Kyrgyzstan One Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'KYRGYZSTAN_ONE_FUND' - Kyrgyzstan One Fund.");
                        createdCount++;
                    }
                }
                
                // Record 96: LEBANON_RECOVERY_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "LEBANON_RECOVERY_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Lebanon Recovery Fund.";
                        existingRecord.Description = "Lebanon Recovery Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "LEBANON_RECOVERY_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'LEBANON_RECOVERY_FUND' - Lebanon Recovery Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "LEBANON_RECOVERY_FUND",
                            Name = "Lebanon Recovery Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'LEBANON_RECOVERY_FUND' - Lebanon Recovery Fund.");
                        createdCount++;
                    }
                }
                
                // Record 97: LESOTHO_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "LESOTHO_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Lesotho One UN Fund.";
                        existingRecord.Description = "Lesotho One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "LESOTHO_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'LESOTHO_ONE_UN_FUND' - Lesotho One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "LESOTHO_ONE_UN_FUND",
                            Name = "Lesotho One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'LESOTHO_ONE_UN_FUND' - Lesotho One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 98: MALAWI_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MALAWI_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Malawi One UN Fund.";
                        existingRecord.Description = "Malawi One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MALAWI_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MALAWI_ONE_UN_FUND' - Malawi One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MALAWI_ONE_UN_FUND",
                            Name = "Malawi One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MALAWI_ONE_UN_FUND' - Malawi One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 99: MALDIVES_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MALDIVES_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Maldives One UN Fund.";
                        existingRecord.Description = "Maldives One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MALDIVES_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MALDIVES_ONE_UN_FUND' - Maldives One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MALDIVES_ONE_UN_FUND",
                            Name = "Maldives One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MALDIVES_ONE_UN_FUND' - Maldives One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 100: MDG_ACHIEVEMENT_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MDG_ACHIEVEMENT_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MDG Achievement Fund.";
                        existingRecord.Description = "MDG Achievement Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MDG_ACHIEVEMENT_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MDG_ACHIEVEMENT_FUND' - MDG Achievement Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MDG_ACHIEVEMENT_FUND",
                            Name = "MDG Achievement Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MDG_ACHIEVEMENT_FUND' - MDG Achievement Fund.");
                        createdCount++;
                    }
                }
                
                // Record 101: MONTENEGRO_UN_COUNTRY_FU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MONTENEGRO_UN_COUNTRY_FU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Montenegro UN Country Fund.";
                        existingRecord.Description = "Montenegro UN Country Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MONTENEGRO_UN_COUNTRY_FU";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MONTENEGRO_UN_COUNTRY_FU' - Montenegro UN Country Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MONTENEGRO_UN_COUNTRY_FU",
                            Name = "Montenegro UN Country Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MONTENEGRO_UN_COUNTRY_FU' - Montenegro UN Country Fund.");
                        createdCount++;
                    }
                }
                
                // Record 102: MOZAMBIQUE_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MOZAMBIQUE_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Mozambique One UN Fund.";
                        existingRecord.Description = "Mozambique One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MOZAMBIQUE_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MOZAMBIQUE_ONE_UN_FUND' - Mozambique One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MOZAMBIQUE_ONE_UN_FUND",
                            Name = "Mozambique One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MOZAMBIQUE_ONE_UN_FUND' - Mozambique One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 103: NEPAL_-_UN_PEACE_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NEPAL_-_UN_PEACE_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Nepal - UN Peace Fund.";
                        existingRecord.Description = "Nepal - UN Peace Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "NEPAL_-_UN_PEACE_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'NEPAL_-_UN_PEACE_FUND' - Nepal - UN Peace Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NEPAL_-_UN_PEACE_FUND",
                            Name = "Nepal - UN Peace Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'NEPAL_-_UN_PEACE_FUND' - Nepal - UN Peace Fund.");
                        createdCount++;
                    }
                }
                
                // Record 104: PAKISTAN_ONE_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PAKISTAN_ONE_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Pakistan One Fund.";
                        existingRecord.Description = "Pakistan One Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "PAKISTAN_ONE_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'PAKISTAN_ONE_FUND' - Pakistan One Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PAKISTAN_ONE_FUND",
                            Name = "Pakistan One Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'PAKISTAN_ONE_FUND' - Pakistan One Fund.");
                        createdCount++;
                    }
                }
                
                // Record 105: PBF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PBF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "PBF.";
                        existingRecord.Description = "PBF Peacebuilding Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "PBF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'PBF' - PBF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PBF",
                            Name = "PBF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'PBF' - PBF.");
                        createdCount++;
                    }
                }
                
                // Record 106: PNG_UN_COUNTRY_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "PNG_UN_COUNTRY_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "PNG UN Country Fund.";
                        existingRecord.Description = "PNG UN Country Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "PNG_UN_COUNTRY_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'PNG_UN_COUNTRY_FUND' - PNG UN Country Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "PNG_UN_COUNTRY_FUND",
                            Name = "PNG UN Country Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'PNG_UN_COUNTRY_FUND' - PNG UN Country Fund.");
                        createdCount++;
                    }
                }
                
                // Record 107: REDD+_JP_PARTNERSHIP_SUP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REDD+_JP_PARTNERSHIP_SUP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "REDD+ JP Partnership Support.";
                        existingRecord.Description = "REDD+ JP Partnership Support";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "REDD+_JP_PARTNERSHIP_SUP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'REDD+_JP_PARTNERSHIP_SUP' - REDD+ JP Partnership Support.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REDD+_JP_PARTNERSHIP_SUP",
                            Name = "REDD+ JP Partnership Support.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'REDD+_JP_PARTNERSHIP_SUP' - REDD+ JP Partnership Support.");
                        createdCount++;
                    }
                }
                
                // Record 108: RWANDA_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "RWANDA_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Rwanda One UN Fund.";
                        existingRecord.Description = "Rwanda One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "RWANDA_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'RWANDA_ONE_UN_FUND' - Rwanda One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "RWANDA_ONE_UN_FUND",
                            Name = "Rwanda One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'RWANDA_ONE_UN_FUND' - Rwanda One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 109: SIERRA_LEONE_MDTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SIERRA_LEONE_MDTF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Sierra Leone MDTF.";
                        existingRecord.Description = "Sierra Leone MDTF";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SIERRA_LEONE_MDTF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SIERRA_LEONE_MDTF' - Sierra Leone MDTF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SIERRA_LEONE_MDTF",
                            Name = "Sierra Leone MDTF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SIERRA_LEONE_MDTF' - Sierra Leone MDTF.");
                        createdCount++;
                    }
                }
                
                // Record 110: SOMALIA_COMMON_HUMANITAR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SOMALIA_COMMON_HUMANITAR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Somalia Common Humanitarian Fd.";
                        existingRecord.Description = "Somalia Common Humanitarian Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SOMALIA_COMMON_HUMANITAR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SOMALIA_COMMON_HUMANITAR' - Somalia Common Humanitarian Fd.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SOMALIA_COMMON_HUMANITAR",
                            Name = "Somalia Common Humanitarian Fd.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SOMALIA_COMMON_HUMANITAR' - Somalia Common Humanitarian Fd.");
                        createdCount++;
                    }
                }
                
                // Record 111: SSRF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SSRF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "SSRF.";
                        existingRecord.Description = "SSRF South Sudan Recovery Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SSRF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SSRF' - SSRF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SSRF",
                            Name = "SSRF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SSRF' - SSRF.");
                        createdCount++;
                    }
                }
                
                // Record 112: SUDAN_COMMON_HUMANITARIA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SUDAN_COMMON_HUMANITARIA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Sudan Common Humanitarian Fund.";
                        existingRecord.Description = "Sudan Common Humanitarian Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SUDAN_COMMON_HUMANITARIA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SUDAN_COMMON_HUMANITARIA' - Sudan Common Humanitarian Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SUDAN_COMMON_HUMANITARIA",
                            Name = "Sudan Common Humanitarian Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SUDAN_COMMON_HUMANITARIA' - Sudan Common Humanitarian Fund.");
                        createdCount++;
                    }
                }
                
                // Record 113: TANZANIA_ONE_UN_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "TANZANIA_ONE_UN_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Tanzania One UN Fund.";
                        existingRecord.Description = "Tanzania One UN Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "TANZANIA_ONE_UN_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'TANZANIA_ONE_UN_FUND' - Tanzania One UN Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "TANZANIA_ONE_UN_FUND",
                            Name = "Tanzania One UN Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'TANZANIA_ONE_UN_FUND' - Tanzania One UN Fund.");
                        createdCount++;
                    }
                }
                
                // Record 114: UN_ACTION
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ACTION");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Action.";
                        existingRecord.Description = "UN Action Against Sexual Violence in Conflict";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_ACTION";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_ACTION' - UN Action.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ACTION",
                            Name = "UN Action.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ACTION' - UN Action.");
                        createdCount++;
                    }
                }
                
                // Record 115: UN_CIVIL_SOCIETY_TRUST_F
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_CIVIL_SOCIETY_TRUST_F");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Civil Society Trust Fund.";
                        existingRecord.Description = "UN Civil Society Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_CIVIL_SOCIETY_TRUST_F";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_CIVIL_SOCIETY_TRUST_F' - UN Civil Society Trust Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_CIVIL_SOCIETY_TRUST_F",
                            Name = "UN Civil Society Trust Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_CIVIL_SOCIETY_TRUST_F' - UN Civil Society Trust Fund.");
                        createdCount++;
                    }
                }
                
                // Record 116: UNIPP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIPP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIPP.";
                        existingRecord.Description = "UNIPP United Nations Indigenous Peoples’ Partnership";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIPP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIPP' - UNIPP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIPP",
                            Name = "UNIPP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIPP' - UNIPP.");
                        createdCount++;
                    }
                }
                
                // Record 117: UNTFHS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNTFHS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNTFHS.";
                        existingRecord.Description = "UN Trust Fund for Human Security";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNTFHS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNTFHS' - UNTFHS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNTFHS",
                            Name = "UNTFHS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNTFHS' - UNTFHS.");
                        createdCount++;
                    }
                }
                
                // Record 118: UN_TRUST_FUND
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_TRUST_FUND");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Trust Fund.";
                        existingRecord.Description = "UN Trust Fund to End Volence Against Women";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_TRUST_FUND";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_TRUST_FUND' - UN Trust Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_TRUST_FUND",
                            Name = "UN Trust Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_TRUST_FUND' - UN Trust Fund.");
                        createdCount++;
                    }
                }
                
                // Record 119: UNDG_HRF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDG_HRF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDG HRF.";
                        existingRecord.Description = "Haiti Reconstruction Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDG_HRF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDG_HRF' - UNDG HRF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDG_HRF",
                            Name = "UNDG HRF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDG_HRF' - UNDG HRF.");
                        createdCount++;
                    }
                }
                
                // Record 120: UNDG_ITF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDG_ITF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDG ITF.";
                        existingRecord.Description = "UNDG Iraq Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDG_ITF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDG_ITF' - UNDG ITF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDG_ITF",
                            Name = "UNDG ITF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDG_ITF' - UNDG ITF.");
                        createdCount++;
                    }
                }
                
                // Record 121: UN-REDD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-REDD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN-REDD.";
                        existingRecord.Description = "UN-REDD Programme Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN-REDD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN-REDD' - UN-REDD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-REDD",
                            Name = "UN-REDD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-REDD' - UN-REDD.");
                        createdCount++;
                    }
                }
                
                // Record 122: URUGUAY_ONE_UN_COHERENCE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "URUGUAY_ONE_UN_COHERENCE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Uruguay One UN Coherence Fund.";
                        existingRecord.Description = "Uruguay One UN Coherence Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "URUGUAY_ONE_UN_COHERENCE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'URUGUAY_ONE_UN_COHERENCE' - Uruguay One UN Coherence Fund.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "URUGUAY_ONE_UN_COHERENCE",
                            Name = "Uruguay One UN Coherence Fund.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'URUGUAY_ONE_UN_COHERENCE' - Uruguay One UN Coherence Fund.");
                        createdCount++;
                    }
                }
                
                // Record 123: VIET_NAM_ONE_FUND_I
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "VIET_NAM_ONE_FUND_I");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Viet Nam One Plan Fund I.";
                        existingRecord.Description = "Viet Nam One Plan Fund I";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "VIET_NAM_ONE_FUND_I";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'VIET_NAM_ONE_FUND_I' - Viet Nam One Plan Fund I.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "VIET_NAM_ONE_FUND_I",
                            Name = "Viet Nam One Plan Fund I.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'VIET_NAM_ONE_FUND_I' - Viet Nam One Plan Fund I.");
                        createdCount++;
                    }
                }
                
                // Record 124: VIET_NAM_ONE_FUND_II
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "VIET_NAM_ONE_FUND_II");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Viet Nam One Plan Fund II.";
                        existingRecord.Description = "Viet Nam One Plan Fund II";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "VIET_NAM_ONE_FUND_II";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'VIET_NAM_ONE_FUND_II' - Viet Nam One Plan Fund II.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "VIET_NAM_ONE_FUND_II",
                            Name = "Viet Nam One Plan Fund II.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'VIET_NAM_ONE_FUND_II' - Viet Nam One Plan Fund II.");
                        createdCount++;
                    }
                }
                
                // Record 125: OTHER_UNDP_MDTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_UNDP_MDTF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Other UNDP MDTF.";
                        existingRecord.Description = "Other UNDP MDTF";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OTHER_UNDP_MDTF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OTHER_UNDP_MDTF' - Other UNDP MDTF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_UNDP_MDTF",
                            Name = "Other UNDP MDTF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_UNDP_MDTF' - Other UNDP MDTF.");
                        createdCount++;
                    }
                }
                
                // Record 126: OTHER_UNDP_JP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_UNDP_JP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Other UNDP JP.";
                        existingRecord.Description = "Other UNDP JP";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OTHER_UNDP_JP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OTHER_UNDP_JP' - Other UNDP JP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_UNDP_JP",
                            Name = "Other UNDP JP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_UNDP_JP' - Other UNDP JP.");
                        createdCount++;
                    }
                }
                
                // Record 127: UNSO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNSO.";
                        existingRecord.Description = "UN Fund for Sudano-Sahelian Activities";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNSO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNSO' - UNSO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSO",
                            Name = "UNSO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSO' - UNSO.");
                        createdCount++;
                    }
                }
                
                // Record 128: UN_VTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_VTF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN VTF.";
                        existingRecord.Description = "VTF UN Voluntary Trust Fund for Assistance in Mine Action";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_VTF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_VTF' - UN VTF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_VTF",
                            Name = "UN VTF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_VTF' - UN VTF.");
                        createdCount++;
                    }
                }
                
                // Record 129: UN_HAITI_CHOLERA_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_HAITI_CHOLERA_MPTF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Haiti Cholera MPTF.";
                        existingRecord.Description = "UN Haiti Cholera Response Multi-Partner Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_HAITI_CHOLERA_MPTF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_HAITI_CHOLERA_MPTF' - UN Haiti Cholera MPTF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_HAITI_CHOLERA_MPTF",
                            Name = "UN Haiti Cholera MPTF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_HAITI_CHOLERA_MPTF' - UN Haiti Cholera MPTF.");
                        createdCount++;
                    }
                }
                
                // Record 130: UNITLIFE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNITLIFE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNITLIFE.";
                        existingRecord.Description = "UNITLIFE United Nations Initiative Fighting Chronic Malnutrition Through Innovation";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNITLIFE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNITLIFE' - UNITLIFE.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNITLIFE",
                            Name = "UNITLIFE.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNITLIFE' - UNITLIFE.");
                        createdCount++;
                    }
                }
                
                // Record 131: UN_MPTF_OFFICE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_MPTF_OFFICE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN MPTF Office.";
                        existingRecord.Description = "United Nations Multi-Partner Trust Fund Office";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_MPTF_OFFICE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_MPTF_OFFICE' - UN MPTF Office.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_MPTF_OFFICE",
                            Name = "UN MPTF Office.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_MPTF_OFFICE' - UN MPTF Office.");
                        createdCount++;
                    }
                }
                
                // Record 132: UN_SRI_LANKA_SDG_MPTF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_SRI_LANKA_SDG_MPTF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Sri Lanka SDG MPTF.";
                        existingRecord.Description = "United Nations Sri Lanka SDG Multi-Partner Trust Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_INTER_POOLED_FUND";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_SRI_LANKA_SDG_MPTF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_SRI_LANKA_SDG_MPTF' - UN Sri Lanka SDG MPTF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_SRI_LANKA_SDG_MPTF",
                            Name = "UN Sri Lanka SDG MPTF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_SRI_LANKA_SDG_MPTF' - UN Sri Lanka SDG MPTF.");
                        createdCount++;
                    }
                }
                
                // Record 133: UNISID1
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNISID1");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Sidney University.";
                        existingRecord.Description = "The University of Sidney";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "ACADEMIC_TRAINING_RESEARC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNISID1";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNISID1' - Sidney University.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNISID1",
                            Name = "Sidney University.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNISID1' - Sidney University.");
                        createdCount++;
                    }
                }
                
                // Record 134: SUBSIDIARY_ORG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SUBSIDIARY_ORG");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Subsidiary Organs.";
                        existingRecord.Description = "United Nations Subsidiary Organs";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "SUBSIDIARY_ORG";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SUBSIDIARY_ORG' - UN Subsidiary Organs.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SUBSIDIARY_ORG",
                            Name = "UN Subsidiary Organs.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SUBSIDIARY_ORG' - UN Subsidiary Organs.");
                        createdCount++;
                    }
                }
                
                // Record 135: BINUCA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "BINUCA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "BINUCA.";
                        existingRecord.Description = "BINUCA United Nations Integrated Peacebuilding Office in the Central African Republic";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "BINUCA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'BINUCA' - BINUCA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "BINUCA",
                            Name = "BINUCA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'BINUCA' - BINUCA.");
                        createdCount++;
                    }
                }
                
                // Record 136: UN_COORD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_COORD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Coordination Mechanisms.";
                        existingRecord.Description = "United Nations Coordination Mechanisms";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "UN_COORD";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_COORD' - UN Coordination Mechanisms.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_COORD",
                            Name = "UN Coordination Mechanisms.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_COORD' - UN Coordination Mechanisms.");
                        createdCount++;
                    }
                }
                
                // Record 137: CEB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CEB");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CEB.";
                        existingRecord.Description = "CEB United Nations System Chief Executives Board for Coordination";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_COORD";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CEB";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CEB' - CEB.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CEB",
                            Name = "CEB.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CEB' - CEB.");
                        createdCount++;
                    }
                }
                
                // Record 138: MENUB
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MENUB");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MENUB.";
                        existingRecord.Description = "MENUB United Nations Electoral Observation Mission in Burundi";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MENUB";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MENUB' - MENUB.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MENUB",
                            Name = "MENUB.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MENUB' - MENUB.");
                        createdCount++;
                    }
                }
                
                // Record 139: MINURSO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINURSO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MINURSO.";
                        existingRecord.Description = "MINURSO United Nations Mission for the Referendum in Western Sahara";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MINURSO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MINURSO' - MINURSO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINURSO",
                            Name = "MINURSO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MINURSO' - MINURSO.");
                        createdCount++;
                    }
                }
                
                // Record 140: MINUSCA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUSCA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MINUSCA.";
                        existingRecord.Description = "MINUSCA United Nations Multidimensional Integrated Stabilization Mission in the Central African Republic";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MINUSCA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MINUSCA' - MINUSCA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUSCA",
                            Name = "MINUSCA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUSCA' - MINUSCA.");
                        createdCount++;
                    }
                }
                
                // Record 141: MINUSMA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUSMA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MINUSMA.";
                        existingRecord.Description = "MINUSMA United Nations Multidimensional Integrated Stabilization Mission in Mali";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MINUSMA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MINUSMA' - MINUSMA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUSMA",
                            Name = "MINUSMA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUSMA' - MINUSMA.");
                        createdCount++;
                    }
                }
                
                // Record 142: MINUSTAH
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUSTAH");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MINUSTAH.";
                        existingRecord.Description = "MINUSTAH United Nations Stabilization Mission in Haiti";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MINUSTAH";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MINUSTAH' - MINUSTAH.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUSTAH",
                            Name = "MINUSTAH.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUSTAH' - MINUSTAH.");
                        createdCount++;
                    }
                }
                
                // Record 143: MONUSCO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MONUSCO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MONUSCO.";
                        existingRecord.Description = "MONUSCO United Nations Organization Stabilization Mission in the Democratic Republic of the Congo";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MONUSCO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MONUSCO' - MONUSCO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MONUSCO",
                            Name = "MONUSCO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MONUSCO' - MONUSCO.");
                        createdCount++;
                    }
                }
                
                // Record 144: UNAKRT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAKRT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNAKRT.";
                        existingRecord.Description = "UNAKRT United Nations Assistance to the Khmer Rouge Trials";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNAKRT";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNAKRT' - UNAKRT.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAKRT",
                            Name = "UNAKRT.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAKRT' - UNAKRT.");
                        createdCount++;
                    }
                }
                
                // Record 145: UNAMA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAMA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNAMA.";
                        existingRecord.Description = "UNAMA United Nations Assistance Mission in Afghanistan";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNAMA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNAMA' - UNAMA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAMA",
                            Name = "UNAMA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAMA' - UNAMA.");
                        createdCount++;
                    }
                }
                
                // Record 146: UNAMI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAMI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNAMI.";
                        existingRecord.Description = "UNAMI United Nations Assistance Mission for Iraq";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNAMI";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNAMI' - UNAMI.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAMI",
                            Name = "UNAMI.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAMI' - UNAMI.");
                        createdCount++;
                    }
                }
                
                // Record 147: UNFICYP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFICYP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNFICYP.";
                        existingRecord.Description = "UNFICYP United Nations Peacekeeping Force in Cyprus";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNFICYP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNFICYP' - UNFICYP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFICYP",
                            Name = "UNFICYP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFICYP' - UNFICYP.");
                        createdCount++;
                    }
                }
                
                // Record 148: UNIFIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIFIL");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIFIL.";
                        existingRecord.Description = "UNIFIL United Nations Interim Force in Lebanon";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIFIL";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIFIL' - UNIFIL.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIFIL",
                            Name = "UNIFIL.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIFIL' - UNIFIL.");
                        createdCount++;
                    }
                }
                
                // Record 149: UNIPSIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIPSIL");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIPSIL.";
                        existingRecord.Description = "UNIPSIL United Nations Integrated Peacebuilding Office in Sierra Leone";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIPSIL";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIPSIL' - UNIPSIL.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIPSIL",
                            Name = "UNIPSIL.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIPSIL' - UNIPSIL.");
                        createdCount++;
                    }
                }
                
                // Record 150: UNISFA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNISFA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNISFA.";
                        existingRecord.Description = "UNISFA United Nations Interim Security Force in Abyei";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNISFA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNISFA' - UNISFA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNISFA",
                            Name = "UNISFA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNISFA' - UNISFA.");
                        createdCount++;
                    }
                }
                
                // Record 151: UNMIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIL");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNMIL.";
                        existingRecord.Description = "UNMIL United Nations Mission in Liberia";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNMIL";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNMIL' - UNMIL.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIL",
                            Name = "UNMIL.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIL' - UNMIL.");
                        createdCount++;
                    }
                }
                
                // Record 152: UNMISS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMISS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNMISS.";
                        existingRecord.Description = "UNMISS United Nations Mission in the Republic of South Sudan";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNMISS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNMISS' - UNMISS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMISS",
                            Name = "UNMISS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMISS' - UNMISS.");
                        createdCount++;
                    }
                }
                
                // Record 153: UNMIT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNMIT.";
                        existingRecord.Description = "UNMIT United Nations Integrated Mission in Timor-Leste";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNMIT";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNMIT' - UNMIT.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIT",
                            Name = "UNMIT.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIT' - UNMIT.");
                        createdCount++;
                    }
                }
                
                // Record 154: UNMOGIP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMOGIP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNMOGIP.";
                        existingRecord.Description = "UNMOGIP United Nations Military Observer Group in India and Pakistan";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNMOGIP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNMOGIP' - UNMOGIP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMOGIP",
                            Name = "UNMOGIP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMOGIP' - UNMOGIP.");
                        createdCount++;
                    }
                }
                
                // Record 155: DEPARTMENT_OFFICE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DEPARTMENT_OFFICE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Departments and Offices.";
                        existingRecord.Description = "United Nations Departments and Offices";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'DEPARTMENT_OFFICE' - UN Departments and Offices.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DEPARTMENT_OFFICE",
                            Name = "UN Departments and Offices.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'DEPARTMENT_OFFICE' - UN Departments and Offices.");
                        createdCount++;
                    }
                }
                
                // Record 156: UNOAU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOAU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOAU.";
                        existingRecord.Description = "UNOAU United Nations Office to the African Union";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOAU";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOAU' - UNOAU.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOAU",
                            Name = "UNOAU.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOAU' - UNOAU.");
                        createdCount++;
                    }
                }
                
                // Record 157: UNOCA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOCA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOCA.";
                        existingRecord.Description = "UNOCA United Nations Regional Office for Central Africa";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOCA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOCA' - UNOCA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOCA",
                            Name = "UNOCA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOCA' - UNOCA.");
                        createdCount++;
                    }
                }
                
                // Record 158: UNOCI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOCI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOCI.";
                        existingRecord.Description = "UNOCI United Nations Operation in Côte d\'Ivoire";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOCI";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOCI' - UNOCI.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOCI",
                            Name = "UNOCI.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOCI' - UNOCI.");
                        createdCount++;
                    }
                }
                
                // Record 159: OTHER_ENTITIES
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_ENTITIES");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Other Entities.";
                        existingRecord.Description = "United Nations Other Entities";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "OTHER_ENTITIES";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OTHER_ENTITIES' - UN Other Entities.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_ENTITIES",
                            Name = "UN Other Entities.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_ENTITIES' - UN Other Entities.");
                        createdCount++;
                    }
                }
                
                // Record 160: ITC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ITC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "ITC.";
                        existingRecord.Description = "ITC International Trade Centre";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ITC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ITC' - ITC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ITC",
                            Name = "ITC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ITC' - ITC.");
                        createdCount++;
                    }
                }
                
                // Record 161: UNHCR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNHCR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNHCR.";
                        existingRecord.Description = "UNHCR Office of the United Nations High Commissioner for Refugees";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNHCR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNHCR' - UNHCR.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNHCR",
                            Name = "UNHCR.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNHCR' - UNHCR.");
                        createdCount++;
                    }
                }
                
                // Record 162: FUND_PROGRAMME
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "FUND_PROGRAMME");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Funds and Programmes.";
                        existingRecord.Description = "United Nations Funds and Programmes";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "FUND_PROGRAMME";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'FUND_PROGRAMME' - UN Funds and Programmes.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "FUND_PROGRAMME",
                            Name = "UN Funds and Programmes.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'FUND_PROGRAMME' - UN Funds and Programmes.");
                        createdCount++;
                    }
                }
                
                // Record 163: UNCDF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNCDF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNCDF.";
                        existingRecord.Description = "UNCDF United Nations Capital Development Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNCDF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNCDF' - UNCDF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNCDF",
                            Name = "UNCDF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNCDF' - UNCDF.");
                        createdCount++;
                    }
                }
                
                // Record 164: UNICEF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNICEF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNICEF.";
                        existingRecord.Description = "UNICEF United Nations Children\'s Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNICEF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNICEF' - UNICEF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNICEF",
                            Name = "UNICEF.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNICEF' - UNICEF.");
                        createdCount++;
                    }
                }
                
                // Record 165: UNCTAD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNCTAD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNCTAD.";
                        existingRecord.Description = "UNCTAD United Nations Conference on Trade and Development";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNCTAD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNCTAD' - UNCTAD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNCTAD",
                            Name = "UNCTAD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNCTAD' - UNCTAD.");
                        createdCount++;
                    }
                }
                
                // Record 166: UNEP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNEP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNEP.";
                        existingRecord.Description = "UNEP United Nations Environment Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNEP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNEP' - UNEP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNEP",
                            Name = "UNEP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNEP' - UNEP.");
                        createdCount++;
                    }
                }
                
                // Record 167: UN-HABITAT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-HABITAT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN-HABITAT.";
                        existingRecord.Description = "UN-HABITAT United Nations Human Settlements Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN-HABITAT";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN-HABITAT' - UN-HABITAT.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-HABITAT",
                            Name = "UN-HABITAT.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-HABITAT' - UN-HABITAT.");
                        createdCount++;
                    }
                }
                
                // Record 168: UNODC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNODC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNODC.";
                        existingRecord.Description = "UNODC United Nations Office on Drugs and Crime";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNODC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNODC' - UNODC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNODC",
                            Name = "UNODC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNODC' - UNODC.");
                        createdCount++;
                    }
                }
                
                // Record 169: UNFPA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFPA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNFPA.";
                        existingRecord.Description = "UNFPA United Nations Population Fund";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNFPA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNFPA' - UNFPA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFPA",
                            Name = "UNFPA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFPA' - UNFPA.");
                        createdCount++;
                    }
                }
                
                // Record 170: UNRWA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNRWA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNRWA.";
                        existingRecord.Description = "UNRWA United Nations Relief and Works Agency for Palestine Refugees in the Near East";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNRWA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNRWA' - UNRWA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNRWA",
                            Name = "UNRWA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNRWA' - UNRWA.");
                        createdCount++;
                    }
                }
                
                // Record 171: UNV
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNV");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNV.";
                        existingRecord.Description = "UNV United Nations Volunteers";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNV";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNV' - UNV.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNV",
                            Name = "UNV.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNV' - UNV.");
                        createdCount++;
                    }
                }
                
                // Record 172: WFP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WFP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "WFP.";
                        existingRecord.Description = "WFP United Nations World Food Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "WFP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'WFP' - WFP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WFP",
                            Name = "WFP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'WFP' - WFP.");
                        createdCount++;
                    }
                }
                
                // Record 173: UN_DESA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DESA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN DESA.";
                        existingRecord.Description = "UN DESA Department of Economic and Social Affairs";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_DESA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_DESA' - UN DESA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DESA",
                            Name = "UN DESA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DESA' - UN DESA.");
                        createdCount++;
                    }
                }
                
                // Record 174: UN_DGACM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DGACM");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN DGACM.";
                        existingRecord.Description = "UN DGACM Department for General Assembly and Conference Management";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_DGACM";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_DGACM' - UN DGACM.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DGACM",
                            Name = "UN DGACM.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DGACM' - UN DGACM.");
                        createdCount++;
                    }
                }
                
                // Record 175: UN_DMSPC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DMSPC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN DMSPC.";
                        existingRecord.Description = "UN DMSPC Department of Management Strategy, Policy and Compliance";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_DMSPC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_DMSPC' - UN DMSPC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DMSPC",
                            Name = "UN DMSPC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DMSPC' - UN DMSPC.");
                        createdCount++;
                    }
                }
                
                // Record 176: UN_DGC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DGC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN DGC.";
                        existingRecord.Description = "UN DGC Department of Global Communications";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_DGC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_DGC' - UN DGC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DGC",
                            Name = "UN DGC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DGC' - UN DGC.");
                        createdCount++;
                    }
                }
                
                // Record 177: UNDSS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDSS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDSS.";
                        existingRecord.Description = "UNDSS Department of Safety and Security";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDSS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDSS' - UNDSS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDSS",
                            Name = "UNDSS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDSS' - UNDSS.");
                        createdCount++;
                    }
                }
                
                // Record 178: UN_OCHA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_OCHA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN OCHA.";
                        existingRecord.Description = "UN OCHA Office for the Coordination of Humanitarian Affairs";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_OCHA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_OCHA' - UN OCHA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_OCHA",
                            Name = "UN OCHA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_OCHA' - UN OCHA.");
                        createdCount++;
                    }
                }
                
                // Record 179: UN_OHCHR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_OHCHR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN OHCHR.";
                        existingRecord.Description = "UN OHCHR Office of the United Nations High Commissioner for Human Rights";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_OHCHR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_OHCHR' - UN OHCHR.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_OHCHR",
                            Name = "UN OHCHR.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_OHCHR' - UN OHCHR.");
                        createdCount++;
                    }
                }
                
                // Record 180: OIOS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OIOS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "OIOS.";
                        existingRecord.Description = "OIOS Office of Internal Oversight Services";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OIOS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OIOS' - OIOS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OIOS",
                            Name = "OIOS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OIOS' - OIOS.");
                        createdCount++;
                    }
                }
                
                // Record 181: UN_OLA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_OLA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN OLA.";
                        existingRecord.Description = "UN OLA Office of Legal Affairs";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_OLA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_OLA' - UN OLA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_OLA",
                            Name = "UN OLA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_OLA' - UN OLA.");
                        createdCount++;
                    }
                }
                
                // Record 182: OSAA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OSAA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "OSAA.";
                        existingRecord.Description = "OSAA Office of the Special Adviser on Africa";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OSAA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OSAA' - OSAA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OSAA",
                            Name = "OSAA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OSAA' - OSAA.");
                        createdCount++;
                    }
                }
                
                // Record 183: SRSG_CAAC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SRSG_CAAC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "SRSG CAAC.";
                        existingRecord.Description = "SRSG CAAC Office of the Special Representative of the Secretary-General for Children and Armed Conflict";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "SRSG_CAAC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SRSG_CAAC' - SRSG CAAC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SRSG_CAAC",
                            Name = "SRSG CAAC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SRSG_CAAC' - SRSG CAAC.");
                        createdCount++;
                    }
                }
                
                // Record 184: UNODA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNODA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNODA.";
                        existingRecord.Description = "UNODA Office for Disarmament Affairs";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNODA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNODA' - UNODA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNODA",
                            Name = "UNODA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNODA' - UNODA.");
                        createdCount++;
                    }
                }
                
                // Record 185: UNOG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOG");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOG.";
                        existingRecord.Description = "UNOG United Nations Office at Geneva";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOG";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOG' - UNOG.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOG",
                            Name = "UNOG.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOG' - UNOG.");
                        createdCount++;
                    }
                }
                
                // Record 186: UN-OHRLLS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN-OHRLLS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN-OHRLLS.";
                        existingRecord.Description = "UN-OHRLLS Office of the High Representative for the Least Developed Countries, Landlocked Developing Countries and Small Island Developing States";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN-OHRLLS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN-OHRLLS' - UN-OHRLLS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN-OHRLLS",
                            Name = "UN-OHRLLS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN-OHRLLS' - UN-OHRLLS.");
                        createdCount++;
                    }
                }
                
                // Record 187: UNON
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNON");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNON.";
                        existingRecord.Description = "UNON United Nations Office at Nairobi";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNON";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNON' - UNON.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNON",
                            Name = "UNON.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNON' - UNON.");
                        createdCount++;
                    }
                }
                
                // Record 188: UNOV
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOV");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOV.";
                        existingRecord.Description = "UNOV United Nations Office at Vienna";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOV";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOV' - UNOV.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOV",
                            Name = "UNOV.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOV' - UNOV.");
                        createdCount++;
                    }
                }
                
                // Record 189: UN_ICC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ICC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN ICC.";
                        existingRecord.Description = "ICC International Computing Centre";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_ICC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_ICC' - UN ICC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ICC",
                            Name = "UN ICC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ICC' - UN ICC.");
                        createdCount++;
                    }
                }
                
                // Record 190: OTHER_BODIES
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_BODIES");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Other Bodies.";
                        existingRecord.Description = "United Nations Other Bodies";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "OTHER_BODIES";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OTHER_BODIES' - UN Other Bodies.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_BODIES",
                            Name = "UN Other Bodies.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_BODIES' - UN Other Bodies.");
                        createdCount++;
                    }
                }
                
                // Record 191: UNAIDS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNAIDS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNAIDS.";
                        existingRecord.Description = "UNAIDS Joint United Nations Programme on HIV/AIDS";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_BODIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNAIDS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNAIDS' - UNAIDS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNAIDS",
                            Name = "UNAIDS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNAIDS' - UNAIDS.");
                        createdCount++;
                    }
                }
                
                // Record 192: UN_WOMEN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_WOMEN");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN WOMEN.";
                        existingRecord.Description = "UN WOMEN United Nations Entity for Gender Equality and the Empowerment of Women";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_WOMEN";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_WOMEN' - UN WOMEN.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_WOMEN",
                            Name = "UN WOMEN.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_WOMEN' - UN WOMEN.");
                        createdCount++;
                    }
                }
                
                // Record 193: UNDRR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDRR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDRR.";
                        existingRecord.Description = "UNDRR United Nations Office for Disaster Risk Reduction";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDRR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDRR' - UNDRR.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDRR",
                            Name = "UNDRR.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDRR' - UNDRR.");
                        createdCount++;
                    }
                }
                
                // Record 194: RESEARCH_TRAINING
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "RESEARCH_TRAINING");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Research and Training.";
                        existingRecord.Description = "United Nations Research and Training";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "RESEARCH_TRAINING";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'RESEARCH_TRAINING' - UN Research and Training.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "RESEARCH_TRAINING",
                            Name = "UN Research and Training.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'RESEARCH_TRAINING' - UN Research and Training.");
                        createdCount++;
                    }
                }
                
                // Record 195: UNSSC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSSC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNSSC.";
                        existingRecord.Description = "UNSSC United Nations System Staff College";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RESEARCH_TRAINING";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNSSC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNSSC' - UNSSC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSSC",
                            Name = "UNSSC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSSC' - UNSSC.");
                        createdCount++;
                    }
                }
                
                // Record 196: UNU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNU.";
                        existingRecord.Description = "UNU United Nations University";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RESEARCH_TRAINING";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNU";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNU' - UNU.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNU",
                            Name = "UNU.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNU' - UNU.");
                        createdCount++;
                    }
                }
                
                // Record 197: REG_COMMISSION
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "REG_COMMISSION");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Regional Commissions.";
                        existingRecord.Description = "United Nations Regional Commissions";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "REG_COMMISSION";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'REG_COMMISSION' - UN Regional Commissions.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "REG_COMMISSION",
                            Name = "UN Regional Commissions.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'REG_COMMISSION' - UN Regional Commissions.");
                        createdCount++;
                    }
                }
                
                // Record 198: UN_ESCAP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ESCAP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN ESCAP.";
                        existingRecord.Description = "UN ESCAP Economic and Social Commission for Asia and the Pacific";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_COMMISSION";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_ESCAP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_ESCAP' - UN ESCAP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ESCAP",
                            Name = "UN ESCAP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ESCAP' - UN ESCAP.");
                        createdCount++;
                    }
                }
                
                // Record 199: UN_ESCWA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ESCWA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN ESCWA.";
                        existingRecord.Description = "UN ESCWA Economic and Social Commission for Western Asia";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_COMMISSION";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_ESCWA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_ESCWA' - UN ESCWA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ESCWA",
                            Name = "UN ESCWA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ESCWA' - UN ESCWA.");
                        createdCount++;
                    }
                }
                
                // Record 200: UN_ECA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ECA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN ECA.";
                        existingRecord.Description = "UN ECA Economic Commission for Africa";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_COMMISSION";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_ECA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_ECA' - UN ECA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ECA",
                            Name = "UN ECA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ECA' - UN ECA.");
                        createdCount++;
                    }
                }
                
                // Record 201: UN_ECLAC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ECLAC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN ECLAC.";
                        existingRecord.Description = "UN ECLAC Economic Commission for Latin America and the Caribbean";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_COMMISSION";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_ECLAC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_ECLAC' - UN ECLAC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ECLAC",
                            Name = "UN ECLAC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ECLAC' - UN ECLAC.");
                        createdCount++;
                    }
                }
                
                // Record 202: UNDG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDG");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDG.";
                        existingRecord.Description = "UNSDG United Nations Sustainable Development Group (formerly UNDG)";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_COORD";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDG";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDG' - UNDG.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDG",
                            Name = "UNDG.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDG' - UNDG.");
                        createdCount++;
                    }
                }
                
                // Record 203: UN_ECE
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_ECE");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN ECE.";
                        existingRecord.Description = "UN ECE Economic Commission for Europe";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "REG_COMMISSION";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_ECE";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_ECE' - UN ECE.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_ECE",
                            Name = "UN ECE.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_ECE' - UN ECE.");
                        createdCount++;
                    }
                }
                
                // Record 204: UNIOGBIS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIOGBIS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIOGBIS.";
                        existingRecord.Description = "UNIOGBIS United Nations Integrated Peacebuilding Office in Guinea-Bissau";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIOGBIS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIOGBIS' - UNIOGBIS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIOGBIS",
                            Name = "UNIOGBIS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIOGBIS' - UNIOGBIS.");
                        createdCount++;
                    }
                }
                
                // Record 205: UNSCN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSCN");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNSCN.";
                        existingRecord.Description = "UNSCN United Nations System Standing Committee on Nutrition";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNSCN";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNSCN' - UNSCN.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSCN",
                            Name = "UNSCN.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSCN' - UNSCN.");
                        createdCount++;
                    }
                }
                
                // Record 206: CONVENTION_FRAMEWORK
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CONVENTION_FRAMEWORK");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Conventions and Frameworks.";
                        existingRecord.Description = "United Nations Conventions and Frameworks";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "CONVENTION_FRAMEWORK";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CONVENTION_FRAMEWORK' - UN Conventions and Frameworks.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CONVENTION_FRAMEWORK",
                            Name = "UN Conventions and Frameworks.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CONVENTION_FRAMEWORK' - UN Conventions and Frameworks.");
                        createdCount++;
                    }
                }
                
                // Record 207: CRPD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CRPD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "CRPD.";
                        existingRecord.Description = "CRPD Convention on the Rights of Persons with Disabilities";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "CONVENTION_FRAMEWORK";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CRPD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CRPD' - CRPD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CRPD",
                            Name = "CRPD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'CRPD' - CRPD.");
                        createdCount++;
                    }
                }
                
                // Record 208: SPECIALIZED_AGENCIES
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "SPECIALIZED_AGENCIES");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Specialized Agencies.";
                        existingRecord.Description = "United Nations Specialized Agencies";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'SPECIALIZED_AGENCIES' - UN Specialized Agencies.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "SPECIALIZED_AGENCIES",
                            Name = "UN Specialized Agencies.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'SPECIALIZED_AGENCIES' - UN Specialized Agencies.");
                        createdCount++;
                    }
                }
                
                // Record 209: FAO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "FAO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "FAO.";
                        existingRecord.Description = "FAO Food and Agriculture Organization of the United Nations";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "FAO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'FAO' - FAO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "FAO",
                            Name = "FAO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'FAO' - FAO.");
                        createdCount++;
                    }
                }
                
                // Record 210: RELATED_ORG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "RELATED_ORG");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN Related Organizations.";
                        existingRecord.Description = "United Nations Related Organizations";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "UNITED_NATIONS";
                        existingRecord.PartnerCategoryCode = "RELATED_ORG";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'RELATED_ORG' - UN Related Organizations.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "RELATED_ORG",
                            Name = "UN Related Organizations.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'RELATED_ORG' - UN Related Organizations.");
                        createdCount++;
                    }
                }
                
                // Record 211: IAEA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IAEA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IAEA.";
                        existingRecord.Description = "IAEA International Atomic Energy Agency";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RELATED_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IAEA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IAEA' - IAEA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IAEA",
                            Name = "IAEA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IAEA' - IAEA.");
                        createdCount++;
                    }
                }
                
                // Record 212: ICAO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ICAO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "ICAO.";
                        existingRecord.Description = "ICAO International Civil Aviation Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ICAO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ICAO' - ICAO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ICAO",
                            Name = "ICAO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ICAO' - ICAO.");
                        createdCount++;
                    }
                }
                
                // Record 213: IFAD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IFAD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IFAD.";
                        existingRecord.Description = "IFAD International Fund for Agricultural Development";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IFAD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IFAD' - IFAD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IFAD",
                            Name = "IFAD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IFAD' - IFAD.");
                        createdCount++;
                    }
                }
                
                // Record 214: ILO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ILO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "ILO.";
                        existingRecord.Description = "ILO International Labour Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ILO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ILO' - ILO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ILO",
                            Name = "ILO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ILO' - ILO.");
                        createdCount++;
                    }
                }
                
                // Record 215: IMO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IMO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IMO.";
                        existingRecord.Description = "IMO International Maritime Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IMO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IMO' - IMO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IMO",
                            Name = "IMO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IMO' - IMO.");
                        createdCount++;
                    }
                }
                
                // Record 216: ITU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ITU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "ITU.";
                        existingRecord.Description = "ITU International Telecommunication Union";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ITU";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ITU' - ITU.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ITU",
                            Name = "ITU.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'ITU' - ITU.");
                        createdCount++;
                    }
                }
                
                // Record 217: OPCW
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OPCW");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "OPCW.";
                        existingRecord.Description = "OPCW Organisation for the Prohibition of Chemical Weapons";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RELATED_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OPCW";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OPCW' - OPCW.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OPCW",
                            Name = "OPCW.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OPCW' - OPCW.");
                        createdCount++;
                    }
                }
                
                // Record 218: UNCCD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNCCD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNCCD.";
                        existingRecord.Description = "UNCCD United Nations Convention to Combat Desertification";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "CONVENTION_FRAMEWORK";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNCCD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNCCD' - UNCCD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNCCD",
                            Name = "UNCCD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNCCD' - UNCCD.");
                        createdCount++;
                    }
                }
                
                // Record 219: UNESCO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNESCO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNESCO.";
                        existingRecord.Description = "UNESCO United Nations Educational, Scientific and Cultural Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNESCO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNESCO' - UNESCO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNESCO",
                            Name = "UNESCO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNESCO' - UNESCO.");
                        createdCount++;
                    }
                }
                
                // Record 220: UNFCCC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNFCCC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNFCCC.";
                        existingRecord.Description = "UNFCCC United Nations Framework Convention on Climate Change";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "CONVENTION_FRAMEWORK";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNFCCC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNFCCC' - UNFCCC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNFCCC",
                            Name = "UNFCCC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNFCCC' - UNFCCC.");
                        createdCount++;
                    }
                }
                
                // Record 221: UNIDO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIDO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIDO.";
                        existingRecord.Description = "UNIDO United Nations Industrial Development Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIDO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIDO' - UNIDO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIDO",
                            Name = "UNIDO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIDO' - UNIDO.");
                        createdCount++;
                    }
                }
                
                // Record 222: UPU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UPU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UPU.";
                        existingRecord.Description = "UPU Universal Postal Union";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UPU";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UPU' - UPU.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UPU",
                            Name = "UPU.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UPU' - UPU.");
                        createdCount++;
                    }
                }
                
                // Record 223: WIPO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WIPO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "WIPO.";
                        existingRecord.Description = "WIPO World Intellectual Property Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "WIPO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'WIPO' - WIPO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WIPO",
                            Name = "WIPO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'WIPO' - WIPO.");
                        createdCount++;
                    }
                }
                
                // Record 224: WMO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WMO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "WMO.";
                        existingRecord.Description = "WMO World Meteorological Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "WMO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'WMO' - WMO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WMO",
                            Name = "WMO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'WMO' - WMO.");
                        createdCount++;
                    }
                }
                
                // Record 225: UNWTO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNWTO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNWTO.";
                        existingRecord.Description = "UNWTO World Tourism Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SPECIALIZED_AGENCIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNWTO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNWTO' - UNWTO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNWTO",
                            Name = "UNWTO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNWTO' - UNWTO.");
                        createdCount++;
                    }
                }
                
                // Record 226: WTO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "WTO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "WTO.";
                        existingRecord.Description = "WTO World Trade Organization";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RELATED_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "WTO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'WTO' - WTO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "WTO",
                            Name = "WTO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'WTO' - WTO.");
                        createdCount++;
                    }
                }
                
                // Record 227: UNIDIR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIDIR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIDIR.";
                        existingRecord.Description = "UNIDIR United Nations Institute for Disarmament Research";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RESEARCH_TRAINING";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIDIR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIDIR' - UNIDIR.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIDIR",
                            Name = "UNIDIR.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIDIR' - UNIDIR.");
                        createdCount++;
                    }
                }
                
                // Record 228: UNITAR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNITAR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNITAR.";
                        existingRecord.Description = "UNITAR United Nations Institute for Training and Research";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RESEARCH_TRAINING";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNITAR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNITAR' - UNITAR.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNITAR",
                            Name = "UNITAR.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNITAR' - UNITAR.");
                        createdCount++;
                    }
                }
                
                // Record 229: UNICRI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNICRI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNICRI.";
                        existingRecord.Description = "UNICRI United Nations Interregional Crime and Justice Research Institute";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RESEARCH_TRAINING";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNICRI";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNICRI' - UNICRI.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNICRI",
                            Name = "UNICRI.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNICRI' - UNICRI.");
                        createdCount++;
                    }
                }
                
                // Record 230: UNRISD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNRISD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNRISD.";
                        existingRecord.Description = "UNRISD United Nations Research Institute for Social Development";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RESEARCH_TRAINING";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNRISD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNRISD' - UNRISD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNRISD",
                            Name = "UNRISD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNRISD' - UNRISD.");
                        createdCount++;
                    }
                }
                
                // Record 231: UNOIP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOIP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOIP.";
                        existingRecord.Description = "UNOIP United Nations Office of the Iraq Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOIP";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOIP' - UNOIP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOIP",
                            Name = "UNOIP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOIP' - UNOIP.");
                        createdCount++;
                    }
                }
                
                // Record 232: UNROD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNROD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNROD.";
                        existingRecord.Description = "UNROD United Nations Register of Damage";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNROD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNROD' - UNROD.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNROD",
                            Name = "UNROD.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNROD' - UNROD.");
                        createdCount++;
                    }
                }
                
                // Record 233: UNMIS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNMIS.";
                        existingRecord.Description = "UNMIS United Nations Mission in Sudan";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNMIS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNMIS' - UNMIS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIS",
                            Name = "UNMIS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIS' - UNMIS.");
                        createdCount++;
                    }
                }
                
                // Record 234: IOM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IOM");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IOM.";
                        existingRecord.Description = "IOM International Organization for Migration";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "RELATED_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IOM";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IOM' - IOM.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IOM",
                            Name = "IOM.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IOM' - IOM.");
                        createdCount++;
                    }
                }
                
                // Record 235: UNMIK
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNMIK");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNMIK.";
                        existingRecord.Description = "UNMIK United Nations Interim Administration Mission in Kosovo";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNMIK";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNMIK' - UNMIK.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNMIK",
                            Name = "UNMIK.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNMIK' - UNMIK.");
                        createdCount++;
                    }
                }
                
                // Record 236: UN_UNITED_NATIONS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_UNITED_NATIONS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "United Nations.";
                        existingRecord.Description = "UN United Nations";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_UNITED_NATIONS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_UNITED_NATIONS' - United Nations.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_UNITED_NATIONS",
                            Name = "United Nations.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_UNITED_NATIONS' - United Nations.");
                        createdCount++;
                    }
                }
                
                // Record 237: UNIFEM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIFEM");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIFEM.";
                        existingRecord.Description = "UNIFEM United Nations Development Fund for Women";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIFEM";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIFEM' - UNIFEM.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIFEM",
                            Name = "UNIFEM.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIFEM' - UNIFEM.");
                        createdCount++;
                    }
                }
                
                // Record 238: UNORCID
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNORCID");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNORCID.";
                        existingRecord.Description = "UNORCID United Nations Office for REDD+ Coordination in Indonesia";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNORCID";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNORCID' - UNORCID.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNORCID",
                            Name = "UNORCID.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNORCID' - UNORCID.");
                        createdCount++;
                    }
                }
                
                // Record 239: UNOWA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOWA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOWA.";
                        existingRecord.Description = "UNOWA United Nations Office for West Africa";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOWA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOWA' - UNOWA.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOWA",
                            Name = "UNOWA.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOWA' - UNOWA.");
                        createdCount++;
                    }
                }
                
                // Record 240: UNSCEAR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSCEAR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNSCEAR.";
                        existingRecord.Description = "UNSCEAR United Nations Scientific Committee on the Effects of Atomic Radiation";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNSCEAR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNSCEAR' - UNSCEAR.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSCEAR",
                            Name = "UNSCEAR.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSCEAR' - UNSCEAR.");
                        createdCount++;
                    }
                }
                
                // Record 241: UNSMIL
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSMIL");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNSMIL.";
                        existingRecord.Description = "UNSMIL United Nations Support Mission in Libya";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNSMIL";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNSMIL' - UNSMIL.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSMIL",
                            Name = "UNSMIL.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSMIL' - UNSMIL.");
                        createdCount++;
                    }
                }
                
                // Record 242: UNSOS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSOS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNSOS.";
                        existingRecord.Description = "UNSOS United Nations Support Office in Somalia";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNSOS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNSOS' - UNSOS.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSOS",
                            Name = "UNSOS.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSOS' - UNSOS.");
                        createdCount++;
                    }
                }
                
                // Record 243: UNSOM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNSOM");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNSOM.";
                        existingRecord.Description = "UNSOM United Nations Assistance Mission in Somalia";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNSOM";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNSOM' - UNSOM.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNSOM",
                            Name = "UNSOM.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNSOM' - UNSOM.");
                        createdCount++;
                    }
                }
                
                // Record 244: UNTSO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNTSO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNTSO.";
                        existingRecord.Description = "UNTSO United Nations Truce Supervision";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNTSO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNTSO' - UNTSO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNTSO",
                            Name = "UNTSO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNTSO' - UNTSO.");
                        createdCount++;
                    }
                }
                
                // Record 245: MINUJUSTH
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "MINUJUSTH");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "MINUJUSTH.";
                        existingRecord.Description = "MINUJUSTH United Nations Mission for Justice Support in Haiti";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "MINUJUSTH";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'MINUJUSTH' - MINUJUSTH.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "MINUJUSTH",
                            Name = "MINUJUSTH.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'MINUJUSTH' - MINUJUSTH.");
                        createdCount++;
                    }
                }
                
                // Record 246: UN_DCO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_DCO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN DCO.";
                        existingRecord.Description = "UN DCO United Nations Development Coordination Office";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_DCO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_DCO' - UN DCO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_DCO",
                            Name = "UN DCO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_DCO' - UN DCO.");
                        createdCount++;
                    }
                }
                
                // Record 247: UNGM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNGM");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNGM.";
                        existingRecord.Description = "UNGM United Nations Global Marketplace";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNGM";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNGM' - UNGM.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNGM",
                            Name = "UNGM.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNGM' - UNGM.");
                        createdCount++;
                    }
                }
                
                // Record 248: UN_TBLDC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_TBLDC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN TBLDC.";
                        existingRecord.Description = "UN Technology Bank for LDC";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_TBLDC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_TBLDC' - UN TBLDC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_TBLDC",
                            Name = "UN TBLDC.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_TBLDC' - UN TBLDC.");
                        createdCount++;
                    }
                }
                
                // Record 249: UNRCO_-_SRI_LANKA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNRCO_-_SRI_LANKA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNRCo - Sri Lanka.";
                        existingRecord.Description = "United Nations Resident Coordinator Office - Sri Lanka";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "UN_COORD";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNRCO_-_SRI_LANKA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNRCO_-_SRI_LANKA' - UNRCo - Sri Lanka.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNRCO_-_SRI_LANKA",
                            Name = "UNRCo - Sri Lanka.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNRCO_-_SRI_LANKA' - UNRCo - Sri Lanka.");
                        createdCount++;
                    }
                }
                
                // Record 250: UNOCT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNOCT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNOCT.";
                        existingRecord.Description = "UNOCT United Nations Office of Counter-Terrorism";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "SUBSIDIARY_ORG";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNOCT";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNOCT' - UNOCT.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNOCT",
                            Name = "UNOCT.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNOCT' - UNOCT.");
                        createdCount++;
                    }
                }
                
                // Record 251: OSGEY
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OSGEY");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "OSGEY.";
                        existingRecord.Description = "Office of the Secretary-General’s Envoy on Youth";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OSGEY";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OSGEY' - OSGEY.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OSGEY",
                            Name = "OSGEY.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OSGEY' - OSGEY.");
                        createdCount++;
                    }
                }
                
                // Record 252: UNIRMCT
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNIRMCT");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNIRMCT.";
                        existingRecord.Description = "UNIRMCT United Nations International Residual Mechanism for Criminal Tribunals";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNIRMCT";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNIRMCT' - UNIRMCT.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNIRMCT",
                            Name = "UNIRMCT.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNIRMCT' - UNIRMCT.");
                        createdCount++;
                    }
                }
                
                // Record 253: UNDP
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDP");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDP.";
                        existingRecord.Description = "UNDP United Nations Development Programme";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "FUND_PROGRAMME";
                        existingRecord.PartnerCategoryCode = "UNDP";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDP' - UNDP.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDP",
                            Name = "UNDP.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDP' - UNDP.");
                        createdCount++;
                    }
                }
                
                // Record 254: UNDP_MPTFO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UNDP_MPTFO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UNDP MPTFO.";
                        existingRecord.Description = "UNDP Multi-Partner Trust Fund Office";
                        existingRecord.Type = "Level_5";
                        existingRecord.Parent = "UNDP";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UNDP_MPTFO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UNDP_MPTFO' - UNDP MPTFO.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UNDP_MPTFO",
                            Name = "UNDP MPTFO.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'UNDP_MPTFO' - UNDP MPTFO.");
                        createdCount++;
                    }
                }
                
                // Record 255: IPSAS_ACCOUNTING
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IPSAS_ACCOUNTING");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IPSAS Accounting.";
                        existingRecord.Description = "IPSAS Accounting";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "OTHER_ENTITIES";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IPSAS_ACCOUNTING";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IPSAS_ACCOUNTING' - IPSAS Accounting.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IPSAS_ACCOUNTING",
                            Name = "IPSAS Accounting.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'IPSAS_ACCOUNTING' - IPSAS Accounting.");
                        createdCount++;
                    }
                }
                
                // Record 256: OTHER_PRIVATE_SECTOR
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OTHER_PRIVATE_SECTOR");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Other Private Sector.";
                        existingRecord.Description = "Other Private Sector";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "OTHER_PRIVATE_SECTOR";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OTHER_PRIVATE_SECTOR' - Other Private Sector.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OTHER_PRIVATE_SECTOR",
                            Name = "Other Private Sector.",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OTHER_PRIVATE_SECTOR' - Other Private Sector.");
                        createdCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartnerTree DummyName seeding completed successfully.");
                Console.WriteLine($"Total records processed: {updatedCount + createdCount}");
                Console.WriteLine($"Records updated: {updatedCount}");
                Console.WriteLine($"Records created: {createdCount}");
                
                // Fix audit data for updated and newly created records
                // Note: SaveChangesAsync triggers audit interceptor which overwrites CreatedBy/LastModifiedBy
                // We need to fix these values after the transaction commits
                var allRecordIds = createdRecordIds.Concat(updatedRecordIds).ToList();
                if (allRecordIds.Count > 0)
                {
                    await FixAuditDataAsync(context, allRecordIds);
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during PartnerTree DummyName seeding: {ex.Message}");
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
