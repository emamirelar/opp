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
    public static class PartnerTree_Bulk_Update_Level3
    {
        public static async Task UpdatePartnerTreesForIntegrationAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting PartnerTree bulk update (Level 3) - Setting LastModifiedBy and LastModifiedDate...");
            
            int updatedCount = 0;
            int notFoundCount = 0;
            var updatedRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: Code = 3DF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "3DF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code '3DF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code '3DF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 2: Code = ADAPTATION_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ADAPTATION_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ADAPTATION_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ADAPTATION_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 3: Code = AFGHANISTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AFGHANISTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AFGHANISTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AFGHANISTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 4: Code = AFRICAN_RISK_CAPACITY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AFRICAN_RISK_CAPACITY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AFRICAN_RISK_CAPACITY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AFRICAN_RISK_CAPACITY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 5: Code = AGFUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AGFUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AGFUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AGFUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 6: Code = ALBANIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ALBANIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ALBANIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ALBANIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 7: Code = ALGERIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ALGERIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ALGERIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ALGERIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 8: Code = ANDORRA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ANDORRA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ANDORRA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ANDORRA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 9: Code = ANGOLA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ANGOLA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ANGOLA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ANGOLA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 10: Code = ANTIGUA_AND_BARBUDA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ANTIGUA_AND_BARBUDA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ANTIGUA_AND_BARBUDA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ANTIGUA_AND_BARBUDA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 11: Code = ANTILLES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ANTILLES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ANTILLES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ANTILLES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 12: Code = APRA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "APRA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'APRA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'APRA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 13: Code = ARISE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ARISE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ARISE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ARISE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 14: Code = ARMENIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ARMENIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ARMENIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ARMENIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 15: Code = ASEAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ASEAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ASEAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ASEAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 16: Code = ATSCALE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ATSCALE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ATSCALE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ATSCALE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 17: Code = AZERBAIJAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AZERBAIJAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AZERBAIJAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AZERBAIJAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 18: Code = BAHAMAS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BAHAMAS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BAHAMAS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BAHAMAS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 19: Code = BAHRAIN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BAHRAIN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BAHRAIN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BAHRAIN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 20: Code = BELARUS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BELARUS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BELARUS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BELARUS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 21: Code = BELIZE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BELIZE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BELIZE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BELIZE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 22: Code = BENIN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BENIN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BENIN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BENIN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 23: Code = BHUTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BHUTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BHUTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BHUTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 24: Code = BOLIVIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BOLIVIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BOLIVIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BOLIVIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 25: Code = BOSNIA_AND_HERZEGOVINA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BOSNIA_AND_HERZEGOVINA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BOSNIA_AND_HERZEGOVINA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BOSNIA_AND_HERZEGOVINA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 26: Code = BOTSWANA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BOTSWANA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BOTSWANA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BOTSWANA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 27: Code = BRUNEI_DARUSSALAM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BRUNEI_DARUSSALAM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BRUNEI_DARUSSALAM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BRUNEI_DARUSSALAM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 28: Code = BULGARIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BULGARIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BULGARIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BULGARIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 29: Code = BURKINA_FASO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BURKINA_FASO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BURKINA_FASO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BURKINA_FASO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 30: Code = BURUNDI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BURUNDI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BURUNDI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BURUNDI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 31: Code = C40
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "C40");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'C40' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'C40' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 32: Code = CAC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 33: Code = CAMBODIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAMBODIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAMBODIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAMBODIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 34: Code = CAMEROON
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAMEROON");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAMEROON' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAMEROON' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 35: Code = CAPE_VERDE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAPE_VERDE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAPE_VERDE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAPE_VERDE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 36: Code = CENTRAL_AFRICAN_REPUBLIC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CENTRAL_AFRICAN_REPUBLIC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CENTRAL_AFRICAN_REPUBLIC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CENTRAL_AFRICAN_REPUBLIC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 37: Code = CHAD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CHAD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CHAD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CHAD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 38: Code = CHILE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CHILE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CHILE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CHILE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 39: Code = CILSS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CILSS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CILSS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CILSS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 40: Code = CITIES_ALLIANCE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CITIES_ALLIANCE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CITIES_ALLIANCE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CITIES_ALLIANCE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 41: Code = CMI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CMI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CMI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CMI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 42: Code = COLOMBIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COLOMBIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COLOMBIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COLOMBIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 43: Code = COMOROS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COMOROS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COMOROS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COMOROS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 44: Code = CONGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CONGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CONGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CONGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 45: Code = COOK_ISLANDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COOK_ISLANDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COOK_ISLANDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COOK_ISLANDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 46: Code = CROATIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CROATIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CROATIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CROATIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 47: Code = CTBTO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CTBTO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CTBTO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CTBTO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 48: Code = CTE_DIVOIRE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CTE_DIVOIRE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CTE_DIVOIRE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CTE_DIVOIRE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 49: Code = CUBA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CUBA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CUBA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CUBA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 50: Code = CURAAO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CURAAO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CURAAO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CURAAO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 51: Code = CVFV20
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CVFV20");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CVFV20' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CVFV20' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 52: Code = CYPRUS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CYPRUS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CYPRUS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CYPRUS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 53: Code = CZECH_REPUBLIC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CZECH_REPUBLIC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CZECH_REPUBLIC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CZECH_REPUBLIC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 54: Code = CZECHOSLOVAKIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CZECHOSLOVAKIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CZECHOSLOVAKIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CZECHOSLOVAKIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 55: Code = DAG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DAG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DAG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DAG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 56: Code = DJIBOUTI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DJIBOUTI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DJIBOUTI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DJIBOUTI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 57: Code = DOMINICA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DOMINICA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DOMINICA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DOMINICA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 58: Code = DOMINICAN_REPUBLIC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DOMINICAN_REPUBLIC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DOMINICAN_REPUBLIC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DOMINICAN_REPUBLIC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 59: Code = DPR_KOREA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DPR_KOREA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DPR_KOREA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DPR_KOREA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 60: Code = DR_CONGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DR_CONGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DR_CONGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DR_CONGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 61: Code = ECOWAS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ECOWAS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ECOWAS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ECOWAS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 62: Code = ECSAHC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ECSAHC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ECSAHC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ECSAHC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 63: Code = EGYPT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EGYPT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EGYPT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EGYPT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 64: Code = EL_SALVADOR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EL_SALVADOR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EL_SALVADOR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EL_SALVADOR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 65: Code = EQUATORIAL_GUINEA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EQUATORIAL_GUINEA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EQUATORIAL_GUINEA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EQUATORIAL_GUINEA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 66: Code = ERITREA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ERITREA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ERITREA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ERITREA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 67: Code = ESTONIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ESTONIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ESTONIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ESTONIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 68: Code = ETP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ETP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ETP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ETP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 69: Code = FIJI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FIJI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FIJI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FIJI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 70: Code = G5_SAHEL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "G5_SAHEL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'G5_SAHEL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'G5_SAHEL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 71: Code = G77
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "G77");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'G77' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'G77' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 72: Code = GABON
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GABON");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GABON' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GABON' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 73: Code = GCF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GCF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GCF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GCF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 74: Code = GEF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GEF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GEF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GEF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 75: Code = GEORGIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GEORGIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GEORGIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GEORGIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 76: Code = GFDRR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GFDRR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GFDRR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GFDRR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 77: Code = GGGI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GGGI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GGGI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GGGI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 78: Code = GHANA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GHANA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GHANA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GHANA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 79: Code = GLOBE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GLOBE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GLOBE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GLOBE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 80: Code = GPE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GPE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GPE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GPE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 81: Code = GRENADA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GRENADA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GRENADA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GRENADA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 82: Code = GUINEA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GUINEA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GUINEA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GUINEA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 83: Code = GUINEA_BISSAU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GUINEA_BISSAU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GUINEA_BISSAU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GUINEA_BISSAU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 84: Code = GUYANA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GUYANA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GUYANA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GUYANA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 85: Code = HAITI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HAITI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HAITI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HAITI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 86: Code = HOLY_SEE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HOLY_SEE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HOLY_SEE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HOLY_SEE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 87: Code = HONG_KONG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HONG_KONG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HONG_KONG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HONG_KONG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 88: Code = HUNGARY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HUNGARY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HUNGARY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HUNGARY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 89: Code = ICAT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ICAT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ICAT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ICAT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 90: Code = ICC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ICC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ICC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ICC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 91: Code = ICMPD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ICMPD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ICMPD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ICMPD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 92: Code = IGAD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IGAD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IGAD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IGAD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 93: Code = IRAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IRAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IRAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IRAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 94: Code = IRAQ
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IRAQ");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IRAQ' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IRAQ' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 95: Code = IRENA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IRENA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IRENA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IRENA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 96: Code = ISA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ISA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ISA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ISA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 97: Code = ITAIPU_BINACIONAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ITAIPU_BINACIONAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ITAIPU_BINACIONAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ITAIPU_BINACIONAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 98: Code = JAMAICA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JAMAICA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JAMAICA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JAMAICA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 99: Code = JORDAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JORDAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JORDAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JORDAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 100: Code = JPF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JPF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JPF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JPF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 101: Code = JSS_NEC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JSS_NEC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JSS_NEC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JSS_NEC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 102: Code = KAZAKHSTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KAZAKHSTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KAZAKHSTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KAZAKHSTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 103: Code = KENYA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KENYA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KENYA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KENYA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 104: Code = KIRIBATI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KIRIBATI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KIRIBATI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KIRIBATI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 105: Code = KOSOVO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KOSOVO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KOSOVO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KOSOVO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 106: Code = KYRGYZSTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KYRGYZSTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KYRGYZSTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KYRGYZSTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 107: Code = LAO_PDR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LAO_PDR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LAO_PDR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LAO_PDR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 108: Code = LATVIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LATVIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LATVIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LATVIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 109: Code = LEBANON
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LEBANON");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LEBANON' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LEBANON' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 110: Code = LESOTHO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LESOTHO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LESOTHO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LESOTHO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 111: Code = LIBERIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LIBERIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LIBERIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LIBERIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 112: Code = LIFT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LIFT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LIFT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LIFT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 113: Code = LITHUANIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LITHUANIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LITHUANIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LITHUANIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 114: Code = MADAGASCAR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MADAGASCAR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MADAGASCAR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MADAGASCAR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 115: Code = MALAWI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALAWI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALAWI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALAWI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 116: Code = MALAYSIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALAYSIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALAYSIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALAYSIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 117: Code = MALDIVES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALDIVES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALDIVES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALDIVES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 118: Code = MALI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 119: Code = MALTA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALTA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALTA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALTA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 120: Code = MARSHALL_ISLANDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MARSHALL_ISLANDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MARSHALL_ISLANDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MARSHALL_ISLANDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 121: Code = MAURITANIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MAURITANIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MAURITANIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MAURITANIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 122: Code = MAURITIUS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MAURITIUS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MAURITIUS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MAURITIUS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 123: Code = MERCOSUR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MERCOSUR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MERCOSUR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MERCOSUR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 124: Code = MICRONESIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MICRONESIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MICRONESIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MICRONESIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 125: Code = MOLDOVA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MOLDOVA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MOLDOVA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MOLDOVA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 126: Code = MONACO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MONACO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MONACO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MONACO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 127: Code = MONGOLIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MONGOLIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MONGOLIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MONGOLIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 128: Code = MONTENEGRO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MONTENEGRO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MONTENEGRO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MONTENEGRO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 129: Code = MONTREAL_PROTOCOL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MONTREAL_PROTOCOL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MONTREAL_PROTOCOL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MONTREAL_PROTOCOL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 130: Code = MOZAMBIQUE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MOZAMBIQUE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MOZAMBIQUE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MOZAMBIQUE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 131: Code = MYANMAR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MYANMAR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MYANMAR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MYANMAR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 132: Code = NAMIBIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NAMIBIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NAMIBIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NAMIBIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 133: Code = NANSEN_INITIATIVE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NANSEN_INITIATIVE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NANSEN_INITIATIVE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NANSEN_INITIATIVE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 134: Code = NAURU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NAURU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NAURU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NAURU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 135: Code = NBI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NBI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NBI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NBI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 136: Code = NDCP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NDCP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NDCP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NDCP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 137: Code = NEPAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NEPAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NEPAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NEPAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 138: Code = NEW_HEBRIDES_CONDOMINIUM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NEW_HEBRIDES_CONDOMINIUM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NEW_HEBRIDES_CONDOMINIUM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NEW_HEBRIDES_CONDOMINIUM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 139: Code = NICARAGUA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NICARAGUA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NICARAGUA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NICARAGUA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 140: Code = NIGER
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NIGER");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NIGER' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NIGER' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 141: Code = NIGERIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NIGERIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NIGERIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NIGERIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 142: Code = NIUE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NIUE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NIUE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NIUE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 143: Code = NORDIC_DEVELOPMENT_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NORDIC_DEVELOPMENT_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NORDIC_DEVELOPMENT_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NORDIC_DEVELOPMENT_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 144: Code = NORTH_MACEDONIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NORTH_MACEDONIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NORTH_MACEDONIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NORTH_MACEDONIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 145: Code = NUTRITION_INTERNATIONAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NUTRITION_INTERNATIONAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NUTRITION_INTERNATIONAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NUTRITION_INTERNATIONAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 146: Code = OECD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OECD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OECD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OECD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 147: Code = OECS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OECS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OECS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OECS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 148: Code = OFFICE_OF_THE_QUARTET
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OFFICE_OF_THE_QUARTET");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OFFICE_OF_THE_QUARTET' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OFFICE_OF_THE_QUARTET' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 149: Code = OIF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OIF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OIF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OIF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 150: Code = OIRSA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OIRSA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OIRSA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OIRSA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 151: Code = OPEC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OPEC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OPEC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OPEC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 152: Code = OSCE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OSCE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OSCE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OSCE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 153: Code = PACIFIC_MULTI_ISLANDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PACIFIC_MULTI_ISLANDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PACIFIC_MULTI_ISLANDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PACIFIC_MULTI_ISLANDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 154: Code = PAKISTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PAKISTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PAKISTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PAKISTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 155: Code = PALAU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PALAU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PALAU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PALAU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 156: Code = PHILIPPINES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PHILIPPINES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PHILIPPINES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PHILIPPINES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 157: Code = PIFS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PIFS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PIFS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PIFS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 158: Code = PONREPP_TF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PONREPP_TF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PONREPP_TF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PONREPP_TF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 159: Code = PPS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PPS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PPS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PPS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 160: Code = ROMANIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ROMANIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ROMANIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ROMANIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 161: Code = RSHQ
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RSHQ");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RSHQ' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RSHQ' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 162: Code = RUSSIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RUSSIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RUSSIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RUSSIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 163: Code = RWANDA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RWANDA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RWANDA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RWANDA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 164: Code = SACEP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SACEP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SACEP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SACEP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 165: Code = SADC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SADC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SADC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SADC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 166: Code = SAINT_KITTS_AND_NEVIS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SAINT_KITTS_AND_NEVIS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SAINT_KITTS_AND_NEVIS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SAINT_KITTS_AND_NEVIS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 167: Code = SAINT_LUCIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SAINT_LUCIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SAINT_LUCIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SAINT_LUCIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 168: Code = SAMOA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SAMOA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SAMOA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SAMOA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 169: Code = SAN_MARINO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SAN_MARINO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SAN_MARINO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SAN_MARINO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 170: Code = SAO_TOME_AND_PRINCIPE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SAO_TOME_AND_PRINCIPE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SAO_TOME_AND_PRINCIPE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SAO_TOME_AND_PRINCIPE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 171: Code = SEFORALL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SEFORALL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SEFORALL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SEFORALL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 172: Code = SENEGAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SENEGAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SENEGAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SENEGAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 173: Code = SERBIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SERBIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SERBIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SERBIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 174: Code = SEYCHELLES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SEYCHELLES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SEYCHELLES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SEYCHELLES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 175: Code = SHF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SHF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SHF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SHF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 176: Code = SINGAPORE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SINGAPORE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SINGAPORE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SINGAPORE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 177: Code = SINT_MAARTEN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SINT_MAARTEN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SINT_MAARTEN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SINT_MAARTEN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 178: Code = SLOVAKIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SLOVAKIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SLOVAKIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SLOVAKIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 179: Code = SLOVENIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SLOVENIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SLOVENIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SLOVENIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 180: Code = SOLOMON_ISLANDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SOLOMON_ISLANDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SOLOMON_ISLANDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SOLOMON_ISLANDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 181: Code = SOMALIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SOMALIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SOMALIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SOMALIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 182: Code = SOMALIA_JPP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SOMALIA_JPP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SOMALIA_JPP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SOMALIA_JPP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 183: Code = SOUTH_SUDAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SOUTH_SUDAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SOUTH_SUDAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SOUTH_SUDAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 184: Code = SRI_LANKA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SRI_LANKA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SRI_LANKA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SRI_LANKA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 185: Code = SSF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SSF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SSF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SSF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 186: Code = ST_VINCENT_AND_THE_GRENADINES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ST_VINCENT_AND_THE_GRENADINES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ST_VINCENT_AND_THE_GRENADINES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ST_VINCENT_AND_THE_GRENADINES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 187: Code = STATE_OF_PALESTINE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "STATE_OF_PALESTINE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'STATE_OF_PALESTINE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'STATE_OF_PALESTINE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 188: Code = SUDAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SUDAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SUDAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SUDAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 189: Code = SURINAME
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SURINAME");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SURINAME' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SURINAME' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 190: Code = SYRIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SYRIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SYRIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SYRIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 191: Code = TAJIKISTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TAJIKISTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TAJIKISTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TAJIKISTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 192: Code = TANZANIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TANZANIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TANZANIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TANZANIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 193: Code = THAILAND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "THAILAND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'THAILAND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'THAILAND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 194: Code = TIMOR_LESTE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TIMOR_LESTE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TIMOR_LESTE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TIMOR_LESTE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 195: Code = TOGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TOGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TOGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TOGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 196: Code = TOKELAU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TOKELAU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TOKELAU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TOKELAU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 197: Code = TONGA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TONGA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TONGA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TONGA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 198: Code = TRINIDAD_AND_TOBAGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TRINIDAD_AND_TOBAGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TRINIDAD_AND_TOBAGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TRINIDAD_AND_TOBAGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 199: Code = TRUST_TERRITORY_PACIFIC_IS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TRUST_TERRITORY_PACIFIC_IS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TRUST_TERRITORY_PACIFIC_IS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TRUST_TERRITORY_PACIFIC_IS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 200: Code = TUNISIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TUNISIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TUNISIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TUNISIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 201: Code = TURKMENISTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TURKMENISTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TURKMENISTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TURKMENISTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 202: Code = TUVALU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TUVALU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TUVALU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TUVALU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 203: Code = UEMOA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UEMOA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UEMOA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UEMOA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 204: Code = UGANDA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UGANDA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UGANDA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UGANDA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 205: Code = UN_ECCAS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ECCAS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ECCAS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ECCAS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 206: Code = UNITAID
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNITAID");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNITAID' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNITAID' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 207: Code = URUGUAY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "URUGUAY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'URUGUAY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'URUGUAY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 208: Code = VANUATU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "VANUATU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'VANUATU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'VANUATU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 209: Code = VENEZUELA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "VENEZUELA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'VENEZUELA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'VENEZUELA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 210: Code = VIET_NAM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "VIET_NAM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'VIET_NAM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'VIET_NAM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 211: Code = VIRGIN_ISLANDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "VIRGIN_ISLANDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'VIRGIN_ISLANDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'VIRGIN_ISLANDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 212: Code = WSSCC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WSSCC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WSSCC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WSSCC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 213: Code = YEMEN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "YEMEN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'YEMEN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'YEMEN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 214: Code = YUGOSLAVIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "YUGOSLAVIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'YUGOSLAVIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'YUGOSLAVIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 215: Code = ZIMBABWE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ZIMBABWE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ZIMBABWE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ZIMBABWE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 216: Code = REG_OTH_FI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "REG_OTH_FI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'REG_OTH_FI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'REG_OTH_FI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 217: Code = EIF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EIF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EIF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EIF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 218: Code = PNG001
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PNG001");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PNG001' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PNG001' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 219: Code = EU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 220: Code = AU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 221: Code = EBY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EBY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EBY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EBY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 222: Code = UN_INTER_POOLED_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_INTER_POOLED_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_INTER_POOLED_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_INTER_POOLED_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 223: Code = SUBSIDIARY_ORG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SUBSIDIARY_ORG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SUBSIDIARY_ORG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SUBSIDIARY_ORG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 224: Code = UN_COORD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_COORD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_COORD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_COORD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 225: Code = DEPARTMENT_OFFICE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DEPARTMENT_OFFICE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DEPARTMENT_OFFICE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DEPARTMENT_OFFICE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 226: Code = OTHER_ENTITIES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTHER_ENTITIES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTHER_ENTITIES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTHER_ENTITIES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 227: Code = FUND_PROGRAMME
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FUND_PROGRAMME");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FUND_PROGRAMME' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FUND_PROGRAMME' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 228: Code = OTHER_BODIES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTHER_BODIES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTHER_BODIES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTHER_BODIES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 229: Code = RESEARCH_TRAINING
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RESEARCH_TRAINING");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RESEARCH_TRAINING' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RESEARCH_TRAINING' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 230: Code = REG_COMMISSION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "REG_COMMISSION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'REG_COMMISSION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'REG_COMMISSION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 231: Code = CONVENTION_FRAMEWORK
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CONVENTION_FRAMEWORK");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CONVENTION_FRAMEWORK' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CONVENTION_FRAMEWORK' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 232: Code = SPECIALIZED_AGENCIES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SPECIALIZED_AGENCIES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SPECIALIZED_AGENCIES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SPECIALIZED_AGENCIES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 233: Code = RELATED_ORG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RELATED_ORG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RELATED_ORG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RELATED_ORG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartnerTree bulk update (Level 3) completed successfully.");
                Console.WriteLine($"Total records processed: {updatedCount + notFoundCount}");
                Console.WriteLine($"Records updated: {updatedCount}");
                Console.WriteLine($"Records not found: {notFoundCount}");
                
                // Fix audit data for updated records
                // Note: SaveChangesAsync triggers audit interceptor which overwrites LastModifiedBy
                // We need to fix these values after the transaction commits
                if (updatedCount > 0)
                {
                    await FixAuditDataAsync(context, updatedRecordIds);
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during PartnerTree bulk update (Level 3): {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        private static async Task FixAuditDataAsync(UNOPSAppDbContext context, List<int> recordIds)
        {
            Console.WriteLine("\nApplying audit data fixes to prevent LastModifiedBy and LastModifiedDate overwrite...");
            
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Use ExecuteUpdateAsync to bypass audit interceptor
                // Update LastModifiedBy and LastModifiedDate for updated PartnerTrees
                int updates = await context.PartnerTrees
                    .Where(p => recordIds.Contains(p.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.LastModifiedBy, -1)
                        .SetProperty(p => p.LastModifiedDate, DateTime.UtcNow));
                
                Console.WriteLine($"Updated LastModifiedBy to -1 and LastModifiedDate for {updates} PartnerTree records");
                
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
