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
    public static class PartnerTree_Bulk_Update_Level2
    {
        public static async Task UpdatePartnerTreesForIntegrationAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting PartnerTree bulk update (Level 2) - Setting LastModifiedBy and LastModifiedDate...");
            
            int updatedCount = 0;
            int notFoundCount = 0;
            var updatedRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: Code = ABCR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ABCR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ABCR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ABCR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 2: Code = ABDIB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ABDIB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ABDIB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ABDIB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 3: Code = ABT_ASSOCIATES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ABT_ASSOCIATES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ABT_ASSOCIATES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ABT_ASSOCIATES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 4: Code = ACCENTURE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ACCENTURE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ACCENTURE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ACCENTURE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 5: Code = ACFE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ACFE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ACFE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ACFE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 6: Code = AFC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AFC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AFC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AFC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 7: Code = AKF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AKF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AKF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AKF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 8: Code = ALTER_VIDA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ALTER_VIDA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ALTER_VIDA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ALTER_VIDA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 9: Code = AMERICAN_RED_CROSS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AMERICAN_RED_CROSS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AMERICAN_RED_CROSS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AMERICAN_RED_CROSS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 10: Code = AMERICARES_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AMERICARES_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AMERICARES_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AMERICARES_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 11: Code = AMREF_HEALTH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AMREF_HEALTH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AMREF_HEALTH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AMREF_HEALTH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 12: Code = APH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "APH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'APH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'APH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 13: Code = ASLM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ASLM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ASLM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ASLM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 14: Code = ASSIST
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ASSIST");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ASSIST' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ASSIST' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 15: Code = ASSOCIATION_IPE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ASSOCIATION_IPE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ASSOCIATION_IPE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ASSOCIATION_IPE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 16: Code = BCBRP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BCBRP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BCBRP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BCBRP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 17: Code = BEGECA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BEGECA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BEGECA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BEGECA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 18: Code = BOSTON_CONSULTING_GROUP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BOSTON_CONSULTING_GROUP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BOSTON_CONSULTING_GROUP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BOSTON_CONSULTING_GROUP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 19: Code = CAMEG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAMEG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAMEG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAMEG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 20: Code = CARITAS_INTERNATIONALIS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CARITAS_INTERNATIONALIS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CARITAS_INTERNATIONALIS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CARITAS_INTERNATIONALIS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 21: Code = CATHOLIC_RELIEF_SERVICES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CATHOLIC_RELIEF_SERVICES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CATHOLIC_RELIEF_SERVICES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CATHOLIC_RELIEF_SERVICES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 22: Code = CBHF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CBHF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CBHF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CBHF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 23: Code = CHAG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CHAG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CHAG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CHAG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 24: Code = CHECCI_AND_COMPANY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CHECCI_AND_COMPANY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CHECCI_AND_COMPANY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CHECCI_AND_COMPANY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 25: Code = CIFF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CIFF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CIFF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CIFF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 26: Code = CISCO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CISCO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CISCO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CISCO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 27: Code = CLAEH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CLAEH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CLAEH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CLAEH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 28: Code = CLEAN_COOKSTOVES
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CLEAN_COOKSTOVES");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CLEAN_COOKSTOVES' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CLEAN_COOKSTOVES' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 29: Code = CLIMATEWORKS_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CLIMATEWORKS_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CLIMATEWORKS_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CLIMATEWORKS_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 30: Code = CLINTON_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CLINTON_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CLINTON_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CLINTON_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 31: Code = COCA_COLA_COMPANY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COCA_COLA_COMPANY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COCA_COLA_COMPANY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COCA_COLA_COMPANY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 32: Code = CODEMGE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CODEMGE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CODEMGE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CODEMGE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 33: Code = COLUMBIA_UNIVERSITY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COLUMBIA_UNIVERSITY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COLUMBIA_UNIVERSITY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COLUMBIA_UNIVERSITY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 34: Code = COMIC_RELIEF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COMIC_RELIEF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COMIC_RELIEF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COMIC_RELIEF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 35: Code = CONCERN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CONCERN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CONCERN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CONCERN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 36: Code = CONISMA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CONISMA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CONISMA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CONISMA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 37: Code = CORDAID
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CORDAID");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CORDAID' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CORDAID' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 38: Code = CPI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CPI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CPI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CPI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 39: Code = CRCF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CRCF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CRCF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CRCF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 40: Code = CRDF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CRDF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CRDF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CRDF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 41: Code = CRF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CRF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CRF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CRF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 42: Code = CROWN_AGENTS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CROWN_AGENTS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CROWN_AGENTS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CROWN_AGENTS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 43: Code = DEFEAT_NCD_PARTNERSHIP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DEFEAT_NCD_PARTNERSHIP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DEFEAT_NCD_PARTNERSHIP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DEFEAT_NCD_PARTNERSHIP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 44: Code = DEVNET_INTERNATIONAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DEVNET_INTERNATIONAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DEVNET_INTERNATIONAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DEVNET_INTERNATIONAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 45: Code = DGAPP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DGAPP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DGAPP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DGAPP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 46: Code = DIGITAL_GOOD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DIGITAL_GOOD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DIGITAL_GOOD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DIGITAL_GOOD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 47: Code = DIIS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DIIS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DIIS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DIIS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 48: Code = DNA_GENOTEK
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DNA_GENOTEK");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DNA_GENOTEK' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DNA_GENOTEK' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 49: Code = DOEN_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DOEN_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DOEN_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DOEN_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 50: Code = ECEAP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ECEAP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ECEAP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ECEAP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 51: Code = EGPAF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EGPAF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EGPAF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EGPAF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 52: Code = EID_CHARITY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EID_CHARITY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EID_CHARITY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EID_CHARITY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 53: Code = ELI_LILLY_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ELI_LILLY_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ELI_LILLY_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ELI_LILLY_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 54: Code = ESTEE_LAUDER
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ESTEE_LAUDER");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ESTEE_LAUDER' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ESTEE_LAUDER' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 55: Code = FAMINE_RELIEF_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FAMINE_RELIEF_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FAMINE_RELIEF_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FAMINE_RELIEF_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 56: Code = FHF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FHF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FHF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FHF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 57: Code = FHI_360
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FHI_360");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FHI_360' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FHI_360' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 58: Code = FIND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FIND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FIND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FIND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 59: Code = FORD_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FORD_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FORD_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FORD_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 60: Code = FOREIGN_TRADE_BANK_OF_CAMBODIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FOREIGN_TRADE_BANK_OF_CAMBODIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FOREIGN_TRADE_BANK_OF_CAMBODIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FOREIGN_TRADE_BANK_OF_CAMBODIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 61: Code = FPN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FPN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FPN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FPN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 62: Code = FUNAG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FUNAG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FUNAG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FUNAG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 63: Code = FUNZILIFE_OY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FUNZILIFE_OY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FUNZILIFE_OY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FUNZILIFE_OY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 64: Code = GAIN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GAIN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GAIN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GAIN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 65: Code = GAP_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GAP_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GAP_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GAP_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 66: Code = GATES_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GATES_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GATES_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GATES_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 67: Code = GCA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GCA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GCA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GCA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 68: Code = GCDP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GCDP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GCDP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GCDP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 69: Code = GELI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GELI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GELI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GELI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 70: Code = GHIT_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GHIT_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GHIT_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GHIT_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 71: Code = GHL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GHL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GHL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GHL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 72: Code = GOOD_NEIGHBORS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "GOOD_NEIGHBORS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'GOOD_NEIGHBORS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'GOOD_NEIGHBORS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 73: Code = HAMMER_FORUM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HAMMER_FORUM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HAMMER_FORUM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HAMMER_FORUM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 74: Code = HEALTH_THROUGH_WALLS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HEALTH_THROUGH_WALLS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HEALTH_THROUGH_WALLS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HEALTH_THROUGH_WALLS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 75: Code = HEMAS_PLC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HEMAS_PLC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HEMAS_PLC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HEMAS_PLC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 76: Code = IATI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IATI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IATI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IATI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 77: Code = IATI_TRUST_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IATI_TRUST_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IATI_TRUST_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IATI_TRUST_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 78: Code = ICARDA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ICARDA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ICARDA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ICARDA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 79: Code = IDOR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IDOR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IDOR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IDOR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 80: Code = IFA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IFA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IFA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IFA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 81: Code = IFPRI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IFPRI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IFPRI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IFPRI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 82: Code = IFRC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IFRC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IFRC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IFRC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 83: Code = IKEA_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IKEA_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IKEA_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IKEA_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 84: Code = INCAP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "INCAP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'INCAP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'INCAP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 85: Code = INPREMA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "INPREMA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'INPREMA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'INPREMA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 86: Code = INS_NGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "INS_NGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'INS_NGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'INS_NGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 87: Code = INT_NGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "INT_NGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'INT_NGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'INT_NGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 88: Code = INTERPEACE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "INTERPEACE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'INTERPEACE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'INTERPEACE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 89: Code = IPEA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IPEA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IPEA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IPEA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 90: Code = IRC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IRC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IRC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IRC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 91: Code = IRW
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IRW");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IRW' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IRW' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 92: Code = ISRAAID
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ISRAAID");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ISRAAID' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ISRAAID' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 93: Code = ITRC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ITRC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ITRC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ITRC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 94: Code = IUCN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IUCN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IUCN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IUCN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 95: Code = JORDAN_RIVER_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JORDAN_RIVER_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JORDAN_RIVER_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JORDAN_RIVER_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 96: Code = JSI_INSTITUTE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JSI_INSTITUTE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JSI_INSTITUTE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JSI_INSTITUTE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 97: Code = KARCPP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KARCPP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KARCPP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KARCPP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 98: Code = KAS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KAS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KAS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KAS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 99: Code = KNCV
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KNCV");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KNCV' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KNCV' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 100: Code = KOCHON_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KOCHON_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KOCHON_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KOCHON_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 101: Code = LA_BENEVOLENCIJA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LA_BENEVOLENCIJA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LA_BENEVOLENCIJA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LA_BENEVOLENCIJA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 102: Code = LABOMERSA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LABOMERSA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LABOMERSA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LABOMERSA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 103: Code = LDSC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LDSC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LDSC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LDSC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 104: Code = LSTM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LSTM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LSTM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LSTM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 105: Code = MAC_ARTHUR_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MAC_ARTHUR_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MAC_ARTHUR_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MAC_ARTHUR_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 106: Code = MACFADDEN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MACFADDEN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MACFADDEN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MACFADDEN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 107: Code = MALARIA_NO_MORE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALARIA_NO_MORE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALARIA_NO_MORE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALARIA_NO_MORE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 108: Code = MARINE_INFORMATION_SERVICE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MARINE_INFORMATION_SERVICE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MARINE_INFORMATION_SERVICE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MARINE_INFORMATION_SERVICE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 109: Code = MAVA_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MAVA_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MAVA_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MAVA_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 110: Code = METRO_DE_QUITO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "METRO_DE_QUITO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'METRO_DE_QUITO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'METRO_DE_QUITO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 111: Code = MFM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MFM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MFM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MFM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 112: Code = MICROSOFT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MICROSOFT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MICROSOFT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MICROSOFT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 113: Code = MILLENNIUM_PROMISE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MILLENNIUM_PROMISE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MILLENNIUM_PROMISE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MILLENNIUM_PROMISE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 114: Code = MITSUBISHI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MITSUBISHI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MITSUBISHI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MITSUBISHI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 115: Code = MIYAMOTO_INTERNATIONAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MIYAMOTO_INTERNATIONAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MIYAMOTO_INTERNATIONAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MIYAMOTO_INTERNATIONAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 116: Code = MOTT_MACDONALD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MOTT_MACDONALD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MOTT_MACDONALD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MOTT_MACDONALD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 117: Code = MSFL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MSFL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MSFL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MSFL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 118: Code = NAT_NGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NAT_NGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NAT_NGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NAT_NGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 119: Code = NATIONAL_GEOGRAPHIC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NATIONAL_GEOGRAPHIC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NATIONAL_GEOGRAPHIC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NATIONAL_GEOGRAPHIC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 120: Code = NIC_UNION_EUROPEA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NIC_UNION_EUROPEA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NIC_UNION_EUROPEA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NIC_UNION_EUROPEA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 121: Code = NIPPON_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NIPPON_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NIPPON_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NIPPON_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 122: Code = OBR_NGO_INTERNATIONAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OBR_NGO_INTERNATIONAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OBR_NGO_INTERNATIONAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OBR_NGO_INTERNATIONAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 123: Code = OBR_NGO_NATIONAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OBR_NGO_NATIONAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OBR_NGO_NATIONAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OBR_NGO_NATIONAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 124: Code = OMIDYAR_NETWORK
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OMIDYAR_NETWORK");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OMIDYAR_NETWORK' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OMIDYAR_NETWORK' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 125: Code = ONE_EARTH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ONE_EARTH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ONE_EARTH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ONE_EARTH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 126: Code = OPEN_SOCIETY_AFGHANISTAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OPEN_SOCIETY_AFGHANISTAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OPEN_SOCIETY_AFGHANISTAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OPEN_SOCIETY_AFGHANISTAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 127: Code = OSF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OSF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OSF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OSF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 128: Code = OSISA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OSISA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OSISA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OSISA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 129: Code = OTB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 130: Code = OTHER_DONORS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTHER_DONORS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTHER_DONORS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTHER_DONORS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 131: Code = OTHER_SPONSORS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTHER_SPONSORS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTHER_SPONSORS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTHER_SPONSORS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 132: Code = OXFAM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OXFAM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OXFAM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OXFAM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 133: Code = PAS_CENTER
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PAS_CENTER");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PAS_CENTER' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PAS_CENTER' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 134: Code = PATH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PATH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PATH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PATH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 135: Code = PAUL_G_ALLEN_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PAUL_G_ALLEN_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PAUL_G_ALLEN_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PAUL_G_ALLEN_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 136: Code = PBSP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PBSP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PBSP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PBSP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 137: Code = PEACENEXUS_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PEACENEXUS_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PEACENEXUS_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PEACENEXUS_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 138: Code = PETUNIA_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PETUNIA_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PETUNIA_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PETUNIA_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 139: Code = PHILIPS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PHILIPS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PHILIPS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PHILIPS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 140: Code = PSI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PSI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PSI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PSI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 141: Code = PURPOSE_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PURPOSE_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PURPOSE_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PURPOSE_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 142: Code = QCF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "QCF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'QCF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'QCF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 143: Code = R20
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "R20");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'R20' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'R20' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 144: Code = RAP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RAP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RAP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RAP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 145: Code = RBF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RBF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RBF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RBF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 146: Code = RBM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RBM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RBM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RBM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 147: Code = REALL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "REALL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'REALL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'REALL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 148: Code = RED_SEA_TRADING_CORPORATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RED_SEA_TRADING_CORPORATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RED_SEA_TRADING_CORPORATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RED_SEA_TRADING_CORPORATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 149: Code = ROCHE_DIAGNOSTICS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ROCHE_DIAGNOSTICS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ROCHE_DIAGNOSTICS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ROCHE_DIAGNOSTICS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 150: Code = ROCKEFELLER_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ROCKEFELLER_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ROCKEFELLER_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ROCKEFELLER_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 151: Code = ROMANIAN_ANGEL_APPEAL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ROMANIAN_ANGEL_APPEAL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ROMANIAN_ANGEL_APPEAL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ROMANIAN_ANGEL_APPEAL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 152: Code = ROVET_SCIENTIFICS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ROVET_SCIENTIFICS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ROVET_SCIENTIFICS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ROVET_SCIENTIFICS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 153: Code = RPA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RPA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RPA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RPA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 154: Code = SANRU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SANRU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SANRU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SANRU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 155: Code = SAVE_THE_CHILDREN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SAVE_THE_CHILDREN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SAVE_THE_CHILDREN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SAVE_THE_CHILDREN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 156: Code = SED_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SED_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SED_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SED_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 157: Code = SEQUOIA_CLIMATE_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SEQUOIA_CLIMATE_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SEQUOIA_CLIMATE_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SEQUOIA_CLIMATE_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 158: Code = SES_PERU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SES_PERU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SES_PERU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SES_PERU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 159: Code = SILATECH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SILATECH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SILATECH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SILATECH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 160: Code = SKYOCEAN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SKYOCEAN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SKYOCEAN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SKYOCEAN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 161: Code = SONY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SONY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SONY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SONY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 162: Code = SSACONG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SSACONG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SSACONG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SSACONG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 163: Code = STANBIC_BANK_GHANA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "STANBIC_BANK_GHANA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'STANBIC_BANK_GHANA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'STANBIC_BANK_GHANA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 164: Code = STOP_TB_PARTNERSHIP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "STOP_TB_PARTNERSHIP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'STOP_TB_PARTNERSHIP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'STOP_TB_PARTNERSHIP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 165: Code = SUN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SUN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SUN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SUN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 166: Code = SUNRISE_PROJECT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SUNRISE_PROJECT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SUNRISE_PROJECT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SUNRISE_PROJECT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 167: Code = SUSTAINABLE_MARKETS_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SUSTAINABLE_MARKETS_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SUSTAINABLE_MARKETS_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SUSTAINABLE_MARKETS_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 168: Code = SWEDBIO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SWEDBIO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SWEDBIO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SWEDBIO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 169: Code = TAKEDA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TAKEDA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TAKEDA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TAKEDA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 170: Code = TASMIM_LIBYA_CONSULTING
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TASMIM_LIBYA_CONSULTING");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TASMIM_LIBYA_CONSULTING' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TASMIM_LIBYA_CONSULTING' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 171: Code = TDF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TDF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TDF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TDF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 172: Code = TDH_ITALY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TDH_ITALY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TDH_ITALY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TDH_ITALY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 173: Code = TEARFUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TEARFUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TEARFUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TEARFUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 174: Code = TEMASEK_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TEMASEK_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TEMASEK_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TEMASEK_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 175: Code = THE_ENERGY_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "THE_ENERGY_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'THE_ENERGY_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'THE_ENERGY_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 176: Code = THPS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "THPS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'THPS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'THPS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 177: Code = TMEA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TMEA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TMEA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TMEA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 178: Code = UABC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UABC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UABC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UABC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 179: Code = UMCOR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UMCOR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UMCOR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UMCOR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 180: Code = UN_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 181: Code = UN_LIVE_MUSEUM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_LIVE_MUSEUM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_LIVE_MUSEUM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_LIVE_MUSEUM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 182: Code = UNA_USA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNA_USA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNA_USA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNA_USA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 183: Code = UNIVERSITY_OF_GENOVA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIVERSITY_OF_GENOVA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIVERSITY_OF_GENOVA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIVERSITY_OF_GENOVA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 184: Code = UNIVERSITY_OF_NOTRE_DAME
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIVERSITY_OF_NOTRE_DAME");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIVERSITY_OF_NOTRE_DAME' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIVERSITY_OF_NOTRE_DAME' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 185: Code = UNIVERSITY_OF_OXFORD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIVERSITY_OF_OXFORD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIVERSITY_OF_OXFORD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIVERSITY_OF_OXFORD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 186: Code = UPNFM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UPNFM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UPNFM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UPNFM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 187: Code = WALIC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WALIC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WALIC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WALIC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 188: Code = WALMART_FOUNDATION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WALMART_FOUNDATION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WALMART_FOUNDATION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WALMART_FOUNDATION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 189: Code = WAPCAS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WAPCAS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WAPCAS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WAPCAS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 190: Code = WELLSPRING
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WELLSPRING");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WELLSPRING' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WELLSPRING' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 191: Code = WEM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WEM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WEM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WEM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 192: Code = WHH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WHH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WHH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WHH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 193: Code = WINDWARD_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WINDWARD_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WINDWARD_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WINDWARD_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 194: Code = WOORD_EN_DAAD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WOORD_EN_DAAD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WOORD_EN_DAAD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WOORD_EN_DAAD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 195: Code = WORLD_VISION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WORLD_VISION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WORLD_VISION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WORLD_VISION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 196: Code = YAJILARRA_TRUST
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "YAJILARRA_TRUST");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'YAJILARRA_TRUST' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'YAJILARRA_TRUST' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 197: Code = CC001
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CC001");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CC001' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CC001' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 198: Code = COG01
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COG01");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COG01' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COG01' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 199: Code = IFI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IFI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IFI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IFI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 200: Code = MAI001
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MAI001");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MAI001' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MAI001' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 201: Code = MPI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MPI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MPI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MPI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 202: Code = NEH001
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NEH001");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NEH001' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NEH001' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 203: Code = NON_OECD_DAC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NON_OECD_DAC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NON_OECD_DAC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NON_OECD_DAC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 204: Code = PAR001
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PAR001");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PAR001' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PAR001' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 205: Code = REG_OTH_INGO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "REG_OTH_INGO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'REG_OTH_INGO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'REG_OTH_INGO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 206: Code = UCD001
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UCD001");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UCD001' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UCD001' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 207: Code = UNITED_NATIONS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNITED_NATIONS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNITED_NATIONS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNITED_NATIONS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 208: Code = UNISID1
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNISID1");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNISID1' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNISID1' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 209: Code = OTHER_PRIVATE_SECTOR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTHER_PRIVATE_SECTOR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTHER_PRIVATE_SECTOR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTHER_PRIVATE_SECTOR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartnerTree bulk update (Level 2) completed successfully.");
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
                Console.WriteLine($"Error during PartnerTree bulk update (Level 2): {ex.Message}");
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
