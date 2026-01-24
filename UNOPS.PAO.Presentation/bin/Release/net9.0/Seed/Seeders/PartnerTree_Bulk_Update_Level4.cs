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
    public static class PartnerTree_Bulk_Update_Level4
    {
        public static async Task UpdatePartnerTreesForIntegrationAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting PartnerTree bulk update (Level 4) - Setting LastModifiedBy and LastModifiedDate...");
            
            int updatedCount = 0;
            int notFoundCount = 0;
            var updatedRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: Code = JP_BANGLADESH_LGSP–LIC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_BANGLADESH_LGSP–LIC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_BANGLADESH_LGSP–LIC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_BANGLADESH_LGSP–LIC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 2: Code = CAF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 3: Code = IMF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IMF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IMF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IMF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 4: Code = AFDB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AFDB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AFDB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AFDB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 5: Code = ADB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ADB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ADB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ADB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 6: Code = CDB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CDB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CDB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CDB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 7: Code = CFC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CFC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CFC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CFC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 8: Code = EBRD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EBRD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EBRD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EBRD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 9: Code = IsDB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IsDB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IsDB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IsDB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 10: Code = AFESD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AFESD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AFESD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AFESD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 11: Code = AIIB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "AIIB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'AIIB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'AIIB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 12: Code = OFID
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OFID");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OFID' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OFID' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 13: Code = BOAD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BOAD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BOAD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BOAD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 14: Code = EC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 15: Code = UNAMID
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNAMID");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNAMID' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNAMID' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 16: Code = EU_DG_MENA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EU_DG_MENA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EU_DG_MENA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EU_DG_MENA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 17: Code = EU_DG_CLIMA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EU_DG_CLIMA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EU_DG_CLIMA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EU_DG_CLIMA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 18: Code = SSHF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SSHF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SSHF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SSHF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 19: Code = EBOLA_RESPONSE_MPTF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "EBOLA_RESPONSE_MPTF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'EBOLA_RESPONSE_MPTF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'EBOLA_RESPONSE_MPTF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 20: Code = SYRIA_EMERGENCY_RESPONSE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SYRIA_EMERGENCY_RESPONSE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SYRIA_EMERGENCY_RESPONSE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SYRIA_EMERGENCY_RESPONSE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 21: Code = SOMALIA_UN_MPTF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SOMALIA_UN_MPTF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SOMALIA_UN_MPTF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SOMALIA_UN_MPTF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 22: Code = UNDF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 23: Code = UN_GENERAL_TRUST_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_GENERAL_TRUST_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_GENERAL_TRUST_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_GENERAL_TRUST_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 24: Code = CERF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CERF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CERF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CERF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 25: Code = UNPBF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNPBF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNPBF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNPBF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 26: Code = UNVFTC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNVFTC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNVFTC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNVFTC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 27: Code = UNVFVT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNVFVT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNVFVT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNVFVT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 28: Code = UNVFD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNVFD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNVFD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNVFD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 29: Code = UNDEF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDEF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDEF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDEF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 30: Code = UNFIP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNFIP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNFIP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNFIP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 31: Code = UN-WATER
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN-WATER");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN-WATER' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN-WATER' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 32: Code = ALBANIA_ONE_UNCF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ALBANIA_ONE_UNCF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ALBANIA_ONE_UNCF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ALBANIA_ONE_UNCF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 33: Code = BHUTAN_UNCF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BHUTAN_UNCF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BHUTAN_UNCF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BHUTAN_UNCF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 34: Code = BOTSWANA_UNCF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BOTSWANA_UNCF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BOTSWANA_UNCF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BOTSWANA_UNCF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 35: Code = CAPE_VERDE_TRANSITION_FU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAPE_VERDE_TRANSITION_FU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAPE_VERDE_TRANSITION_FU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAPE_VERDE_TRANSITION_FU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 36: Code = CAR_HF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CAR_HF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CAR_HF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CAR_HF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 37: Code = CFIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CFIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CFIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CFIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 38: Code = CBA_CC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CBA_CC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CBA_CC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CBA_CC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 39: Code = COMOROS_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "COMOROS_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'COMOROS_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'COMOROS_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 40: Code = DCPSF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DCPSF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DCPSF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DCPSF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 41: Code = DRC_POOLED_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DRC_POOLED_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DRC_POOLED_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DRC_POOLED_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 42: Code = DRC_STABILIZATION_AND_RE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "DRC_STABILIZATION_AND_RE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'DRC_STABILIZATION_AND_RE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'DRC_STABILIZATION_AND_RE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 43: Code = ETHIOPIA_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ETHIOPIA_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ETHIOPIA_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ETHIOPIA_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 44: Code = HRM_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "HRM_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'HRM_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'HRM_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 45: Code = INDONESIA_DR_TF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "INDONESIA_DR_TF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'INDONESIA_DR_TF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'INDONESIA_DR_TF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 46: Code = IRAQ_UNDAF_TRUST_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IRAQ_UNDAF_TRUST_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IRAQ_UNDAF_TRUST_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IRAQ_UNDAF_TRUST_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 47: Code = JP_ARMED_VIOLENCE_PREVEN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_ARMED_VIOLENCE_PREVEN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_ARMED_VIOLENCE_PREVEN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_ARMED_VIOLENCE_PREVEN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 48: Code = JP_CHAD_DIS_SECURITY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_CHAD_DIS_SECURITY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_CHAD_DIS_SECURITY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_CHAD_DIS_SECURITY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 49: Code = JP_DRC_MICROFINANCE_II
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_DRC_MICROFINANCE_II");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_DRC_MICROFINANCE_II' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_DRC_MICROFINANCE_II' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 50: Code = JP_DRC_SECURITY_SECT_REF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_DRC_SECURITY_SECT_REF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_DRC_SECURITY_SECT_REF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_DRC_SECURITY_SECT_REF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 51: Code = JP_GUATEMALA_MAYA_PROGRA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_GUATEMALA_MAYA_PROGRA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_GUATEMALA_MAYA_PROGRA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_GUATEMALA_MAYA_PROGRA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 52: Code = JP_GUATEMALA_RURAL_DEV
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_GUATEMALA_RURAL_DEV");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_GUATEMALA_RURAL_DEV' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_GUATEMALA_RURAL_DEV' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 53: Code = JP_KAZAKHSTAN_INNOV_APRC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_KAZAKHSTAN_INNOV_APRC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_KAZAKHSTAN_INNOV_APRC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_KAZAKHSTAN_INNOV_APRC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 54: Code = JP_KENYA_HIV_AND_AIDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_KENYA_HIV_AND_AIDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_KENYA_HIV_AND_AIDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_KENYA_HIV_AND_AIDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 55: Code = JP_KOSOVO_DOMESTIC_VIOLE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_KOSOVO_DOMESTIC_VIOLE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_KOSOVO_DOMESTIC_VIOLE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_KOSOVO_DOMESTIC_VIOLE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 56: Code = JP_LAO_GOVERN_PUBLIC_ADM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_LAO_GOVERN_PUBLIC_ADM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_LAO_GOVERN_PUBLIC_ADM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_LAO_GOVERN_PUBLIC_ADM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 57: Code = JP_LIBERIA_FOOD_SECURITY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_LIBERIA_FOOD_SECURITY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_LIBERIA_FOOD_SECURITY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_LIBERIA_FOOD_SECURITY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 58: Code = JP_LIBERIA_GENDER_EQUALI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_LIBERIA_GENDER_EQUALI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_LIBERIA_GENDER_EQUALI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_LIBERIA_GENDER_EQUALI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 59: Code = JP_MALI_AGRO_PASTORAL_PR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_MALI_AGRO_PASTORAL_PR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_MALI_AGRO_PASTORAL_PR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_MALI_AGRO_PASTORAL_PR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 60: Code = JP_MOLDOVA_JILDP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_MOLDOVA_JILDP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_MOLDOVA_JILDP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_MOLDOVA_JILDP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 61: Code = JP_NEPAL_LGCDP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_NEPAL_LGCDP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_NEPAL_LGCDP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_NEPAL_LGCDP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 62: Code = JP_SERBIA_SCILD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_SERBIA_SCILD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_SERBIA_SCILD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_SERBIA_SCILD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 63: Code = JP_SOLOMON_ISLANDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_SOLOMON_ISLANDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_SOLOMON_ISLANDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_SOLOMON_ISLANDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 64: Code = JP_SOMALIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_SOMALIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_SOMALIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_SOMALIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 65: Code = JP_MACEDONIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_MACEDONIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_MACEDONIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_MACEDONIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 66: Code = JP_TIMOR-LESTE_INFUSE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_TIMOR-LESTE_INFUSE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_TIMOR-LESTE_INFUSE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_TIMOR-LESTE_INFUSE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 67: Code = JP_TIMOR-LESTE_LGSP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_TIMOR-LESTE_LGSP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_TIMOR-LESTE_LGSP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_TIMOR-LESTE_LGSP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 68: Code = JP_UGANDA_GENDER_EQUALIT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_UGANDA_GENDER_EQUALIT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_UGANDA_GENDER_EQUALIT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_UGANDA_GENDER_EQUALIT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 69: Code = JP_UGANDA_SUPPORT_FOR_AI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "JP_UGANDA_SUPPORT_FOR_AI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'JP_UGANDA_SUPPORT_FOR_AI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'JP_UGANDA_SUPPORT_FOR_AI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 70: Code = KIRIBATI_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KIRIBATI_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KIRIBATI_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KIRIBATI_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 71: Code = KYRGYZSTAN_ONE_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "KYRGYZSTAN_ONE_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'KYRGYZSTAN_ONE_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'KYRGYZSTAN_ONE_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 72: Code = LEBANON_RECOVERY_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LEBANON_RECOVERY_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LEBANON_RECOVERY_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LEBANON_RECOVERY_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 73: Code = LESOTHO_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "LESOTHO_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'LESOTHO_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'LESOTHO_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 74: Code = MALAWI_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALAWI_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALAWI_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALAWI_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 75: Code = MALDIVES_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MALDIVES_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MALDIVES_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MALDIVES_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 76: Code = MDG_ACHIEVEMENT_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MDG_ACHIEVEMENT_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MDG_ACHIEVEMENT_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MDG_ACHIEVEMENT_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 77: Code = MONTENEGRO_UN_COUNTRY_FU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MONTENEGRO_UN_COUNTRY_FU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MONTENEGRO_UN_COUNTRY_FU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MONTENEGRO_UN_COUNTRY_FU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 78: Code = MOZAMBIQUE_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MOZAMBIQUE_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MOZAMBIQUE_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MOZAMBIQUE_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 79: Code = NEPAL_-_UN_PEACE_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "NEPAL_-_UN_PEACE_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'NEPAL_-_UN_PEACE_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'NEPAL_-_UN_PEACE_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 80: Code = PAKISTAN_ONE_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PAKISTAN_ONE_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PAKISTAN_ONE_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PAKISTAN_ONE_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 81: Code = PBF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PBF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PBF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PBF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 82: Code = PNG_UN_COUNTRY_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "PNG_UN_COUNTRY_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'PNG_UN_COUNTRY_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'PNG_UN_COUNTRY_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 83: Code = REDD+_JP_PARTNERSHIP_SUP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "REDD+_JP_PARTNERSHIP_SUP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'REDD+_JP_PARTNERSHIP_SUP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'REDD+_JP_PARTNERSHIP_SUP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 84: Code = RWANDA_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "RWANDA_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'RWANDA_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'RWANDA_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 85: Code = SIERRA_LEONE_MDTF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SIERRA_LEONE_MDTF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SIERRA_LEONE_MDTF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SIERRA_LEONE_MDTF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 86: Code = SOMALIA_COMMON_HUMANITAR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SOMALIA_COMMON_HUMANITAR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SOMALIA_COMMON_HUMANITAR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SOMALIA_COMMON_HUMANITAR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 87: Code = SSRF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SSRF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SSRF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SSRF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 88: Code = SUDAN_COMMON_HUMANITARIA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SUDAN_COMMON_HUMANITARIA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SUDAN_COMMON_HUMANITARIA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SUDAN_COMMON_HUMANITARIA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 89: Code = TANZANIA_ONE_UN_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "TANZANIA_ONE_UN_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'TANZANIA_ONE_UN_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'TANZANIA_ONE_UN_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 90: Code = UN_ACTION
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ACTION");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ACTION' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ACTION' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 91: Code = UN_CIVIL_SOCIETY_TRUST_F
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_CIVIL_SOCIETY_TRUST_F");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_CIVIL_SOCIETY_TRUST_F' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_CIVIL_SOCIETY_TRUST_F' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 92: Code = UNIPP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIPP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIPP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIPP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 93: Code = UNTFHS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNTFHS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNTFHS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNTFHS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 94: Code = UN_TRUST_FUND
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_TRUST_FUND");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_TRUST_FUND' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_TRUST_FUND' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 95: Code = UNDG_HRF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDG_HRF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDG_HRF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDG_HRF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 96: Code = UNDG_ITF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDG_ITF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDG_ITF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDG_ITF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 97: Code = UN-REDD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN-REDD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN-REDD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN-REDD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 98: Code = URUGUAY_ONE_UN_COHERENCE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "URUGUAY_ONE_UN_COHERENCE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'URUGUAY_ONE_UN_COHERENCE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'URUGUAY_ONE_UN_COHERENCE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 99: Code = VIET_NAM_ONE_FUND_I
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "VIET_NAM_ONE_FUND_I");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'VIET_NAM_ONE_FUND_I' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'VIET_NAM_ONE_FUND_I' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 100: Code = VIET_NAM_ONE_FUND_II
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "VIET_NAM_ONE_FUND_II");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'VIET_NAM_ONE_FUND_II' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'VIET_NAM_ONE_FUND_II' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 101: Code = OTHER_UNDP_MDTF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTHER_UNDP_MDTF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTHER_UNDP_MDTF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTHER_UNDP_MDTF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 102: Code = OTHER_UNDP_JP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OTHER_UNDP_JP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OTHER_UNDP_JP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OTHER_UNDP_JP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 103: Code = UNSO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNSO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNSO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNSO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 104: Code = UN_VTF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_VTF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_VTF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_VTF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 105: Code = UN_HAITI_CHOLERA_MPTF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_HAITI_CHOLERA_MPTF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_HAITI_CHOLERA_MPTF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_HAITI_CHOLERA_MPTF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 106: Code = UNITLIFE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNITLIFE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNITLIFE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNITLIFE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 107: Code = UN_MPTF_OFFICE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_MPTF_OFFICE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_MPTF_OFFICE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_MPTF_OFFICE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 108: Code = UN_SRI_LANKA_SDG_MPTF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_SRI_LANKA_SDG_MPTF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_SRI_LANKA_SDG_MPTF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_SRI_LANKA_SDG_MPTF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 109: Code = BINUCA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "BINUCA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'BINUCA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'BINUCA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 110: Code = CEB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CEB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CEB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CEB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 111: Code = MENUB
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MENUB");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MENUB' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MENUB' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 112: Code = MINURSO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MINURSO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MINURSO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MINURSO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 113: Code = MINUSCA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MINUSCA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MINUSCA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MINUSCA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 114: Code = MINUSMA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MINUSMA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MINUSMA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MINUSMA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 115: Code = MINUSTAH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MINUSTAH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MINUSTAH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MINUSTAH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 116: Code = MONUSCO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MONUSCO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MONUSCO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MONUSCO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 117: Code = UNAKRT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNAKRT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNAKRT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNAKRT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 118: Code = UNAMA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNAMA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNAMA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNAMA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 119: Code = UNAMI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNAMI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNAMI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNAMI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 120: Code = UNFICYP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNFICYP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNFICYP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNFICYP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 121: Code = UNIFIL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIFIL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIFIL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIFIL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 122: Code = UNIPSIL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIPSIL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIPSIL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIPSIL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 123: Code = UNISFA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNISFA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNISFA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNISFA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 124: Code = UNMIL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNMIL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNMIL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNMIL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 125: Code = UNMISS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNMISS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNMISS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNMISS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 126: Code = UNMIT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNMIT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNMIT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNMIT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 127: Code = UNMOGIP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNMOGIP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNMOGIP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNMOGIP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 128: Code = UNOAU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOAU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOAU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOAU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 129: Code = UNOCA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOCA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOCA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOCA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 130: Code = UNOCI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOCI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOCI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOCI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 131: Code = ITC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ITC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ITC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ITC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 132: Code = UNHCR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNHCR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNHCR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNHCR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 133: Code = UNCDF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNCDF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNCDF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNCDF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 134: Code = UNICEF
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNICEF");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNICEF' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNICEF' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 135: Code = UNCTAD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNCTAD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNCTAD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNCTAD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 136: Code = UNEP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNEP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNEP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNEP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 137: Code = UN-HABITAT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN-HABITAT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN-HABITAT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN-HABITAT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 138: Code = UNODC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNODC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNODC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNODC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 139: Code = UNFPA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNFPA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNFPA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNFPA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 140: Code = UNRWA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNRWA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNRWA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNRWA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 141: Code = UNV
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNV");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNV' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNV' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 142: Code = WFP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WFP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WFP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WFP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 143: Code = UN_DESA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_DESA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_DESA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_DESA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 144: Code = UN_DGACM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_DGACM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_DGACM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_DGACM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 145: Code = UN_DMSPC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_DMSPC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_DMSPC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_DMSPC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 146: Code = UN_DGC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_DGC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_DGC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_DGC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 147: Code = UNDSS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDSS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDSS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDSS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 148: Code = UN_OCHA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_OCHA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_OCHA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_OCHA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 149: Code = UN_OHCHR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_OHCHR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_OHCHR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_OHCHR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 150: Code = OIOS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OIOS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OIOS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OIOS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 151: Code = UN_OLA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_OLA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_OLA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_OLA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 152: Code = OSAA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OSAA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OSAA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OSAA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 153: Code = SRSG_CAAC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "SRSG_CAAC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'SRSG_CAAC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'SRSG_CAAC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 154: Code = UNODA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNODA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNODA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNODA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 155: Code = UNOG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 156: Code = UN-OHRLLS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN-OHRLLS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN-OHRLLS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN-OHRLLS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 157: Code = UNON
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNON");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNON' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNON' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 158: Code = UNOV
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOV");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOV' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOV' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 159: Code = UN_ICC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ICC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ICC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ICC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 160: Code = UNAIDS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNAIDS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNAIDS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNAIDS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 161: Code = UN_WOMEN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_WOMEN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_WOMEN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_WOMEN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 162: Code = UNDRR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDRR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDRR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDRR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 163: Code = UNSSC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNSSC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNSSC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNSSC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 164: Code = UNU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 165: Code = UN_ESCAP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ESCAP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ESCAP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ESCAP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 166: Code = UN_ESCWA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ESCWA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ESCWA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ESCWA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 167: Code = UN_ECA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ECA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ECA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ECA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 168: Code = UN_ECLAC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ECLAC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ECLAC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ECLAC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 169: Code = UNDG
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDG");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDG' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDG' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 170: Code = UN_ECE
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_ECE");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_ECE' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_ECE' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 171: Code = UNIOGBIS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIOGBIS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIOGBIS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIOGBIS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 172: Code = UNSCN
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNSCN");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNSCN' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNSCN' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 173: Code = CRPD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "CRPD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'CRPD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'CRPD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 174: Code = FAO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "FAO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'FAO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'FAO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 175: Code = IAEA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IAEA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IAEA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IAEA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 176: Code = ICAO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ICAO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ICAO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ICAO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 177: Code = IFAD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IFAD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IFAD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IFAD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 178: Code = ILO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ILO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ILO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ILO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 179: Code = IMO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IMO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IMO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IMO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 180: Code = ITU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "ITU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'ITU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'ITU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 181: Code = OPCW
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OPCW");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OPCW' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OPCW' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 182: Code = UNCCD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNCCD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNCCD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNCCD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 183: Code = UNESCO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNESCO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNESCO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNESCO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 184: Code = UNFCCC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNFCCC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNFCCC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNFCCC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 185: Code = UNIDO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIDO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIDO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIDO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 186: Code = UPU
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UPU");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UPU' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UPU' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 187: Code = WIPO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WIPO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WIPO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WIPO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 188: Code = WMO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WMO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WMO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WMO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 189: Code = UNWTO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNWTO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNWTO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNWTO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 190: Code = WTO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "WTO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'WTO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'WTO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 191: Code = UNIDIR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIDIR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIDIR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIDIR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 192: Code = UNITAR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNITAR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNITAR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNITAR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 193: Code = UNICRI
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNICRI");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNICRI' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNICRI' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 194: Code = UNRISD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNRISD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNRISD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNRISD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 195: Code = UNOIP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOIP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOIP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOIP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 196: Code = UNROD
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNROD");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNROD' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNROD' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 197: Code = UNMIS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNMIS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNMIS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNMIS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 198: Code = IOM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IOM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IOM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IOM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 199: Code = UNMIK
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNMIK");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNMIK' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNMIK' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 200: Code = UN_UNITED_NATIONS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_UNITED_NATIONS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_UNITED_NATIONS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_UNITED_NATIONS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 201: Code = UNIFEM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIFEM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIFEM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIFEM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 202: Code = UNORCID
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNORCID");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNORCID' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNORCID' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 203: Code = UNOWA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOWA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOWA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOWA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 204: Code = UNSCEAR
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNSCEAR");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNSCEAR' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNSCEAR' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 205: Code = UNSMIL
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNSMIL");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNSMIL' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNSMIL' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 206: Code = UNSOS
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNSOS");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNSOS' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNSOS' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 207: Code = UNSOM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNSOM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNSOM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNSOM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 208: Code = UNTSO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNTSO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNTSO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNTSO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 209: Code = MINUJUSTH
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "MINUJUSTH");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'MINUJUSTH' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'MINUJUSTH' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 210: Code = UN_DCO
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_DCO");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_DCO' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_DCO' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 211: Code = UNGM
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNGM");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNGM' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNGM' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 212: Code = UN_TBLDC
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UN_TBLDC");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UN_TBLDC' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UN_TBLDC' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 213: Code = UNRCO_-_SRI_LANKA
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNRCO_-_SRI_LANKA");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNRCO_-_SRI_LANKA' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNRCO_-_SRI_LANKA' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 214: Code = UNOCT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNOCT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNOCT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNOCT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 215: Code = OSGEY
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "OSGEY");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'OSGEY' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'OSGEY' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 216: Code = UNIRMCT
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNIRMCT");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNIRMCT' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNIRMCT' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 217: Code = UNDP
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "UNDP");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'UNDP' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'UNDP' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 218: Code = IPSAS_ACCOUNTING
                {
                    var existingPartnerTree = await context.PartnerTrees
                        .FirstOrDefaultAsync(p => p.Code == "IPSAS_ACCOUNTING");
                    
                    if (existingPartnerTree != null)
                    {
                        updatedRecordIds.Add(existingPartnerTree.Id);
                        Console.WriteLine($"Found: PartnerTree with Code 'IPSAS_ACCOUNTING' - {existingPartnerTree.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: PartnerTree with Code 'IPSAS_ACCOUNTING' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartnerTree bulk update (Level 4) completed successfully.");
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
                Console.WriteLine($"Error during PartnerTree bulk update (Level 4): {ex.Message}");
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
