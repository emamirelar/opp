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
    public static class UnapprovedPartnerTreeSeeder_DummyName_v3
    {
        public static async Task SeedUnapprovedPartnerTreeDummyNameAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting Unapproved PartnerTree DummyName seeding process (v3)...");
            
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
                
                // Record 2: AAIC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AAIC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "AAIC.";
                        existingRecord.Description = "AAIC Japan Co., Ltd.";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "AAIC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'AAIC' - AAIC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AAIC",
                            Name = "AAIC.",
                            Description = "AAIC Japan Co., Ltd.",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "AAIC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'AAIC' - AAIC.");
                        createdCount++;
                    }
                }
                
                // Record 3: FOUNDATION
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "FOUNDATION");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Foundation.";
                        existingRecord.Description = "Foundation";
                        existingRecord.Type = "Level_1";
                        existingRecord.Parent = null;
                        existingRecord.PartnerCategoryCode = "FOUNDATION";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'FOUNDATION' - Foundation.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "FOUNDATION",
                            Name = "Foundation.",
                            Description = "Foundation",
                            Type = "Level_1",
                            Parent = null,
                            PartnerCategoryCode = "FOUNDATION",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'FOUNDATION' - Foundation.");
                        createdCount++;
                    }
                }
                
                // Record 4: AEF
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "AEF");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "AEF.";
                        existingRecord.Description = "AEF Africa-Europe Foundation";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "FOUNDATION";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "AEF";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'AEF' - AEF.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "AEF",
                            Name = "AEF.",
                            Description = "AEF Africa-Europe Foundation",
                            Type = "Level_2",
                            Parent = "FOUNDATION",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "AEF",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'AEF' - AEF.");
                        createdCount++;
                    }
                }
                
                // Record 5: ALLM
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ALLM");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Allm.";
                        existingRecord.Description = "Allm Inc.";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ALLM";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ALLM' - Allm.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ALLM",
                            Name = "Allm.",
                            Description = "Allm Inc.",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ALLM",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ALLM' - Allm.");
                        createdCount++;
                    }
                }
                
                // Record 6: GOVERNMENT
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
                
                // Record 7: OECD_DAC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "OECD_DAC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Gov: OECD/DAC.";
                        existingRecord.Description = "OECD/DAC Government";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "GOVERNMENT";
                        existingRecord.PartnerCategoryCode = "OECD_DAC";
                        existingRecord.PartnerGroupCode = null;
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'OECD_DAC' - Gov: OECD/DAC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "OECD_DAC",
                            Name = "Gov: OECD/DAC.",
                            Description = "OECD/DAC Government",
                            Type = "Level_2",
                            Parent = "GOVERNMENT",
                            PartnerCategoryCode = "OECD_DAC",
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
                        Console.WriteLine($"Created: PartnerTree with Code 'OECD_DAC' - Gov: OECD/DAC.");
                        createdCount++;
                    }
                }
                
                // Record 8: UK
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UK");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UK.";
                        existingRecord.Description = "UK United Kingdom";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "OECD_DAC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UK";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UK' - UK.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UK",
                            Name = "UK.",
                            Description = "UK United Kingdom",
                            Type = "Level_3",
                            Parent = "OECD_DAC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UK",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UK' - UK.");
                        createdCount++;
                    }
                }
                
                // Record 9: CAMARA_DE_COMERCIO_DE_CO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CAMARA_DE_COMERCIO_DE_CO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Camara de Comercio de Cortes.";
                        existingRecord.Description = "Camara de Comercio de Cortes";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CAMARA_DE_COMERCIO_DE_CO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CAMARA_DE_COMERCIO_DE_CO' - Camara de Comercio de Cortes.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CAMARA_DE_COMERCIO_DE_CO",
                            Name = "Camara de Comercio de Cortes.",
                            Description = "Camara de Comercio de Cortes",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CAMARA_DE_COMERCIO_DE_CO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CAMARA_DE_COMERCIO_DE_CO' - Camara de Comercio de Cortes.");
                        createdCount++;
                    }
                }
                
                // Record 10: CARLSBERG
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "CARLSBERG");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Carlsberg.";
                        existingRecord.Description = "Carlsberg Group A/S";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "CARLSBERG";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'CARLSBERG' - Carlsberg.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "CARLSBERG",
                            Name = "Carlsberg.",
                            Description = "Carlsberg Group A/S",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "CARLSBERG",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'CARLSBERG' - Carlsberg.");
                        createdCount++;
                    }
                }
                
                // Record 11: OTHER
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
                
                // Record 12: MULTILATERAL
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
                
                // Record 13: REG_OTH_INGO
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
                
                // Record 14: EU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "EU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "EU.";
                        existingRecord.Description = "EU European Union";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "REG_OTH_INGO";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "EU";
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
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "EU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
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
                
                // Record 15: HOTEL_NEW_OTANI_TOKYO
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "HOTEL_NEW_OTANI_TOKYO");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Hotel New Otani Tokyo.";
                        existingRecord.Description = "Hotel New Otani Tokyo";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "HOTEL_NEW_OTANI_TOKYO";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'HOTEL_NEW_OTANI_TOKYO' - Hotel New Otani Tokyo.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "HOTEL_NEW_OTANI_TOKYO",
                            Name = "Hotel New Otani Tokyo.",
                            Description = "Hotel New Otani Tokyo",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "HOTEL_NEW_OTANI_TOKYO",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'HOTEL_NEW_OTANI_TOKYO' - Hotel New Otani Tokyo.");
                        createdCount++;
                    }
                }
                
                // Record 16: HUMAN_PRACTICE_FDN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "HUMAN_PRACTICE_FDN");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Human Practice Fdn.";
                        existingRecord.Description = "Human Practice Foundation";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "FOUNDATION";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "HUMAN_PRACTICE_FDN";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'HUMAN_PRACTICE_FDN' - Human Practice Fdn.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "HUMAN_PRACTICE_FDN",
                            Name = "Human Practice Fdn.",
                            Description = "Human Practice Foundation",
                            Type = "Level_2",
                            Parent = "FOUNDATION",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "HUMAN_PRACTICE_FDN",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'HUMAN_PRACTICE_FDN' - Human Practice Fdn.");
                        createdCount++;
                    }
                }
                
                // Record 17: IFU
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "IFU");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "IFU.";
                        existingRecord.Description = "IFU - Impact Fund Denmark";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "IFU";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'IFU' - IFU.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "IFU",
                            Name = "IFU.",
                            Description = "IFU - Impact Fund Denmark",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "IFU",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'IFU' - IFU.");
                        createdCount++;
                    }
                }
                
                // Record 18: JAPAN
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "JAPAN");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Japan.";
                        existingRecord.Description = "Japan";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "OECD_DAC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "JAPAN";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'JAPAN' - Japan.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "JAPAN",
                            Name = "Japan.",
                            Description = "Japan",
                            Type = "Level_3",
                            Parent = "OECD_DAC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "JAPAN",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'JAPAN' - Japan.");
                        createdCount++;
                    }
                }
                
                // Record 19: DENMARK
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "DENMARK");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Denmark.";
                        existingRecord.Description = "Denmark";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "OECD_DAC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "DENMARK";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'DENMARK' - Denmark.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "DENMARK",
                            Name = "Denmark.",
                            Description = "Denmark",
                            Type = "Level_3",
                            Parent = "OECD_DAC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "DENMARK",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'DENMARK' - Denmark.");
                        createdCount++;
                    }
                }
                
                // Record 20: ITA001
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "ITA001");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Italiy.";
                        existingRecord.Description = "Italy";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "OECD_DAC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "ITA001";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'ITA001' - Italiy.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "ITA001",
                            Name = "Italiy.",
                            Description = "Italy",
                            Type = "Level_3",
                            Parent = "OECD_DAC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "ITA001",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'ITA001' - Italiy.");
                        createdCount++;
                    }
                }
                
                // Record 21: NEC
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NEC");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "NEC.";
                        existingRecord.Description = "NEC Corporation";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "NEC";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'NEC' - NEC.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NEC",
                            Name = "NEC.",
                            Description = "NEC Corporation",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "NEC",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'NEC' - NEC.");
                        createdCount++;
                    }
                }
                
                // Record 22: ACADEMIC_TRAINING_RESEARC
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
                
                // Record 23: NRI
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "NRI");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "NRI.";
                        existingRecord.Description = "Nomura Research Institute (NRI)";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "ACADEMIC_TRAINING_RESEARC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "NRI";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'NRI' - NRI.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "NRI",
                            Name = "NRI.",
                            Description = "Nomura Research Institute (NRI)",
                            Type = "Level_2",
                            Parent = "ACADEMIC_TRAINING_RESEARC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "NRI",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'NRI' - NRI.");
                        createdCount++;
                    }
                }
                
                // Record 24: UNITED_NATIONS
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
                
                // Record 25: DEPARTMENT_OFFICE
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
                
                // Record 26: UN_DCO
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
                
                // Record 27: NON_OECD_DAC
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
                
                // Record 28: HONDURAS
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "HONDURAS");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Honduras.";
                        existingRecord.Description = "Honduras";
                        existingRecord.Type = "Level_3";
                        existingRecord.Parent = "NON_OECD_DAC";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "HONDURAS";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'HONDURAS' - Honduras.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "HONDURAS",
                            Name = "Honduras.",
                            Description = "Honduras",
                            Type = "Level_3",
                            Parent = "NON_OECD_DAC",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "HONDURAS",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'HONDURAS' - Honduras.");
                        createdCount++;
                    }
                }
                
                // Record 29: TWINBIRD
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "TWINBIRD");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Twinbird.";
                        existingRecord.Description = "Twinbird Corporation";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "TWINBIRD";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'TWINBIRD' - Twinbird.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "TWINBIRD",
                            Name = "Twinbird.",
                            Description = "Twinbird Corporation",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "TWINBIRD",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'TWINBIRD' - Twinbird.");
                        createdCount++;
                    }
                }
                
                // Record 30: UN_-_ROME
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "UN_-_ROME");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "UN - Rome.";
                        existingRecord.Description = "UN in Rome";
                        existingRecord.Type = "Level_4";
                        existingRecord.Parent = "DEPARTMENT_OFFICE";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "UN_-_ROME";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'UN_-_ROME' - UN - Rome.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "UN_-_ROME",
                            Name = "UN - Rome.",
                            Description = "UN in Rome",
                            Type = "Level_4",
                            Parent = "DEPARTMENT_OFFICE",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "UN_-_ROME",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'UN_-_ROME' - UN - Rome.");
                        createdCount++;
                    }
                }
                
                // Record 31: YAMAHA
                {
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "YAMAHA");
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Name = "Yamaha.";
                        existingRecord.Description = "Yamaha Motor Co., Ltd.";
                        existingRecord.Type = "Level_2";
                        existingRecord.Parent = "PRIVATE_SECTOR";
                        existingRecord.PartnerCategoryCode = null;
                        existingRecord.PartnerGroupCode = "YAMAHA";
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code 'YAMAHA' - Yamaha.");
                        updatedCount++;
                    }
                    else
                    {
                        var newRecord = new UNOPSPartnerTree
                        {
                            Code = "YAMAHA",
                            Name = "Yamaha.",
                            Description = "Yamaha Motor Co., Ltd.",
                            Type = "Level_2",
                            Parent = "PRIVATE_SECTOR",
                            PartnerCategoryCode = null,
                            PartnerGroupCode = "YAMAHA",
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        };
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code 'YAMAHA' - Yamaha.");
                        createdCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nUnapproved PartnerTree DummyName seeding completed successfully.");
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
                Console.WriteLine($"Error during Unapproved PartnerTree DummyName seeding: {ex.Message}");
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
