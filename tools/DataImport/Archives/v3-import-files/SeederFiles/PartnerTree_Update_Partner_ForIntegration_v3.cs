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
    public static class PartnerTree_Update_Partner_ForIntegration_v3
    {
        public static async Task UpdatePartnersForIntegrationAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting Partner update for integration (v3) - Setting LastModifiedBy and LastModifiedDate...");
            
            int updatedCount = 0;
            int notFoundCount = 0;
            var updatedRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: ErpDimValue = 1945
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1945);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1945' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1945' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 2: ErpDimValue = 1942
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1942);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1942' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1942' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 3: ErpDimValue = 1011
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1011);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1011' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1011' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 4: ErpDimValue = 1250
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1250);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1250' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1250' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 5: ErpDimValue = 1437
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1437);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1437' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1437' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 6: ErpDimValue = 1438
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1438);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1438' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1438' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 7: ErpDimValue = 1439
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1439);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1439' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1439' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 8: ErpDimValue = 1440
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1440);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1440' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1440' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 9: ErpDimValue = 1441
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1441);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1441' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1441' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 10: ErpDimValue = 1571
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1571);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1571' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1571' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 11: ErpDimValue = 1572
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1572);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1572' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1572' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 12: ErpDimValue = 1793
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1793);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1793' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1793' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 13: ErpDimValue = 1817
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1817);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1817' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1817' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 14: ErpDimValue = 1925
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1925);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1925' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1925' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 15: ErpDimValue = 1948
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1948);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1948' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1948' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 16: ErpDimValue = 1938
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1938);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1938' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1938' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 17: ErpDimValue = 1933
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1933);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1933' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1933' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 18: ErpDimValue = 1949
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1949);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1949' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1949' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 19: ErpDimValue = 1947
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1947);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1947' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1947' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 20: ErpDimValue = 1025
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1025);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1025' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1025' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 21: ErpDimValue = 1026
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1026);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1026' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1026' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 22: ErpDimValue = 1029
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1029);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1029' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1029' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 23: ErpDimValue = 1032
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1032);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1032' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1032' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 24: ErpDimValue = 1165
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1165);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1165' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1165' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 25: ErpDimValue = 1649
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1649);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1649' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1649' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 26: ErpDimValue = 1739
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1739);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1739' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1739' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 27: ErpDimValue = 1807
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1807);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1807' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1807' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 28: ErpDimValue = 1943
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1943);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1943' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1943' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 29: ErpDimValue = 1944
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1944);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1944' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1944' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 30: ErpDimValue = 1934
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1934);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1934' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1934' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 31: ErpDimValue = 1015
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1015);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1015' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1015' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 32: ErpDimValue = 1027
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1027);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1027' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1027' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 33: ErpDimValue = 1151
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1151);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1151' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1151' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 34: ErpDimValue = 1154
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1154);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1154' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1154' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 35: ErpDimValue = 1166
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1166);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1166' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1166' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 36: ErpDimValue = 1168
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1168);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1168' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1168' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 37: ErpDimValue = 1226
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1226);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1226' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1226' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 38: ErpDimValue = 1237
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1237);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1237' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1237' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 39: ErpDimValue = 1239
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1239);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1239' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1239' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 40: ErpDimValue = 1240
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1240);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1240' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1240' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 41: ErpDimValue = 1241
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1241);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1241' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1241' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 42: ErpDimValue = 1255
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1255);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1255' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1255' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 43: ErpDimValue = 1258
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1258);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1258' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1258' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 44: ErpDimValue = 1463
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1463);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1463' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1463' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 45: ErpDimValue = 1464
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1464);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1464' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1464' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 46: ErpDimValue = 1465
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1465);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1465' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1465' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 47: ErpDimValue = 1466
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1466);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1466' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1466' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 48: ErpDimValue = 1467
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1467);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1467' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1467' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 49: ErpDimValue = 1468
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1468);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1468' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1468' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 50: ErpDimValue = 1469
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1469);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1469' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1469' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 51: ErpDimValue = 1470
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1470);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1470' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1470' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 52: ErpDimValue = 1471
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1471);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1471' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1471' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 53: ErpDimValue = 1472
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1472);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1472' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1472' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 54: ErpDimValue = 1473
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1473);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1473' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1473' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 55: ErpDimValue = 1474
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1474);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1474' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1474' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 56: ErpDimValue = 1475
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1475);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1475' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1475' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 57: ErpDimValue = 1476
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1476);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1476' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1476' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 58: ErpDimValue = 1477
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1477);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1477' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1477' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 59: ErpDimValue = 1478
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1478);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1478' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1478' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 60: ErpDimValue = 1479
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1479);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1479' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1479' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 61: ErpDimValue = 1480
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1480);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1480' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1480' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 62: ErpDimValue = 1481
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1481);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1481' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1481' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 63: ErpDimValue = 1482
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1482);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1482' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1482' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 64: ErpDimValue = 1483
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1483);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1483' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1483' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 65: ErpDimValue = 1484
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1484);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1484' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1484' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 66: ErpDimValue = 1485
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1485);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1485' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1485' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 67: ErpDimValue = 1486
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1486);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1486' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1486' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 68: ErpDimValue = 1487
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1487);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1487' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1487' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 69: ErpDimValue = 1488
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1488);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1488' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1488' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 70: ErpDimValue = 1489
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1489);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1489' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1489' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 71: ErpDimValue = 1490
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1490);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1490' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1490' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 72: ErpDimValue = 1491
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1491);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1491' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1491' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 73: ErpDimValue = 1492
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1492);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1492' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1492' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 74: ErpDimValue = 1493
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1493);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1493' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1493' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 75: ErpDimValue = 1494
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1494);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1494' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1494' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 76: ErpDimValue = 1495
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1495);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1495' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1495' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 77: ErpDimValue = 1496
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1496);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1496' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1496' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 78: ErpDimValue = 1497
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1497);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1497' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1497' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 79: ErpDimValue = 1498
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1498);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1498' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1498' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 80: ErpDimValue = 1499
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1499);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1499' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1499' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 81: ErpDimValue = 1500
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1500);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1500' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1500' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 82: ErpDimValue = 1501
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1501);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1501' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1501' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 83: ErpDimValue = 1502
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1502);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1502' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1502' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 84: ErpDimValue = 1503
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1503);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1503' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1503' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 85: ErpDimValue = 1504
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1504);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1504' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1504' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 86: ErpDimValue = 1505
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1505);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1505' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1505' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 87: ErpDimValue = 1506
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1506);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1506' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1506' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 88: ErpDimValue = 1507
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1507);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1507' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1507' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 89: ErpDimValue = 1508
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1508);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1508' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1508' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 90: ErpDimValue = 1509
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1509);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1509' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1509' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 91: ErpDimValue = 1510
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1510);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1510' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1510' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 92: ErpDimValue = 1511
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1511);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1511' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1511' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 93: ErpDimValue = 1512
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1512);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1512' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1512' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 94: ErpDimValue = 1513
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1513);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1513' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1513' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 95: ErpDimValue = 1514
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1514);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1514' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1514' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 96: ErpDimValue = 1515
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1515);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1515' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1515' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 97: ErpDimValue = 1516
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1516);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1516' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1516' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 98: ErpDimValue = 1517
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1517);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1517' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1517' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 99: ErpDimValue = 1518
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1518);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1518' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1518' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 100: ErpDimValue = 1519
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1519);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1519' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1519' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 101: ErpDimValue = 1520
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1520);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1520' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1520' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 102: ErpDimValue = 1521
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1521);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1521' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1521' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 103: ErpDimValue = 1522
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1522);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1522' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1522' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 104: ErpDimValue = 1523
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1523);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1523' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1523' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 105: ErpDimValue = 1524
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1524);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1524' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1524' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 106: ErpDimValue = 1525
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1525);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1525' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1525' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 107: ErpDimValue = 1526
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1526);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1526' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1526' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 108: ErpDimValue = 1527
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1527);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1527' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1527' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 109: ErpDimValue = 1528
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1528);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1528' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1528' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 110: ErpDimValue = 1529
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1529);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1529' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1529' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 111: ErpDimValue = 1530
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1530);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1530' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1530' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 112: ErpDimValue = 1531
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1531);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1531' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1531' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 113: ErpDimValue = 1532
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1532);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1532' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1532' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 114: ErpDimValue = 1533
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1533);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1533' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1533' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 115: ErpDimValue = 1538
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1538);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1538' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1538' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 116: ErpDimValue = 1539
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1539);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1539' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1539' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 117: ErpDimValue = 1545
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1545);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1545' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1545' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 118: ErpDimValue = 1643
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1643);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1643' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1643' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 119: ErpDimValue = 1705
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1705);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1705' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1705' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 120: ErpDimValue = 1718
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1718);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1718' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1718' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 121: ErpDimValue = 1760
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1760);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1760' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1760' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 122: ErpDimValue = 1765
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1765);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1765' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1765' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 123: ErpDimValue = 1779
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1779);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1779' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1779' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 124: ErpDimValue = 1941
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1941);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1941' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1941' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 125: ErpDimValue = 1009
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1009);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1009' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1009' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 126: ErpDimValue = 1014
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1014);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1014' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1014' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 127: ErpDimValue = 1058
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1058);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1058' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1058' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 128: ErpDimValue = 1061
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1061);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1061' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1061' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 129: ErpDimValue = 1062
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1062);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1062' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1062' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 130: ErpDimValue = 1063
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1063);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1063' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1063' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 131: ErpDimValue = 1064
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1064);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1064' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1064' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 132: ErpDimValue = 1066
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1066);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1066' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1066' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 133: ErpDimValue = 1162
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1162);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1162' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1162' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 134: ErpDimValue = 1163
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1163);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1163' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1163' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 135: ErpDimValue = 1164
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1164);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1164' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1164' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 136: ErpDimValue = 1167
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1167);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1167' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1167' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 137: ErpDimValue = 1169
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1169);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1169' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1169' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 138: ErpDimValue = 1170
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1170);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1170' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1170' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 139: ErpDimValue = 1171
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1171);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1171' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1171' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 140: ErpDimValue = 1175
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1175);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1175' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1175' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 141: ErpDimValue = 1176
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1176);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1176' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1176' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 142: ErpDimValue = 1177
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1177);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1177' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1177' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 143: ErpDimValue = 1178
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1178);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1178' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1178' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 144: ErpDimValue = 1179
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1179);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1179' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1179' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 145: ErpDimValue = 1180
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1180);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1180' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1180' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 146: ErpDimValue = 1181
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1181);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1181' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1181' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 147: ErpDimValue = 1182
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1182);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1182' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1182' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 148: ErpDimValue = 1183
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1183);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1183' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1183' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 149: ErpDimValue = 1184
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1184);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1184' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1184' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 150: ErpDimValue = 1185
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1185);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1185' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1185' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 151: ErpDimValue = 1186
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1186);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1186' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1186' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 152: ErpDimValue = 1192
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1192);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1192' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1192' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 153: ErpDimValue = 1193
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1193);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1193' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1193' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 154: ErpDimValue = 1194
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1194);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1194' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1194' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 155: ErpDimValue = 1195
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1195);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1195' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1195' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 156: ErpDimValue = 1196
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1196);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1196' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1196' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 157: ErpDimValue = 1197
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1197);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1197' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1197' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 158: ErpDimValue = 1198
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1198);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1198' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1198' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 159: ErpDimValue = 1200
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1200);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1200' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1200' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 160: ErpDimValue = 1202
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1202);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1202' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1202' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 161: ErpDimValue = 1203
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1203);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1203' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1203' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 162: ErpDimValue = 1205
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1205);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1205' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1205' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 163: ErpDimValue = 1206
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1206);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1206' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1206' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 164: ErpDimValue = 1207
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1207);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1207' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1207' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 165: ErpDimValue = 1208
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1208);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1208' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1208' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 166: ErpDimValue = 1209
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1209);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1209' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1209' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 167: ErpDimValue = 1210
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1210);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1210' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1210' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 168: ErpDimValue = 1211
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1211);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1211' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1211' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 169: ErpDimValue = 1212
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1212);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1212' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1212' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 170: ErpDimValue = 1213
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1213);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1213' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1213' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 171: ErpDimValue = 1214
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1214);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1214' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1214' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 172: ErpDimValue = 1215
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1215);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1215' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1215' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 173: ErpDimValue = 1216
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1216);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1216' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1216' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 174: ErpDimValue = 1217
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1217);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1217' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1217' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 175: ErpDimValue = 1220
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1220);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1220' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1220' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 176: ErpDimValue = 1221
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1221);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1221' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1221' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 177: ErpDimValue = 1222
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1222);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1222' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1222' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 178: ErpDimValue = 1223
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1223);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1223' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1223' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 179: ErpDimValue = 1224
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1224);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1224' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1224' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 180: ErpDimValue = 1225
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1225);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1225' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1225' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 181: ErpDimValue = 1227
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1227);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1227' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1227' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 182: ErpDimValue = 1228
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1228);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1228' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1228' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 183: ErpDimValue = 1229
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1229);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1229' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1229' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 184: ErpDimValue = 1230
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1230);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1230' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1230' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 185: ErpDimValue = 1234
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1234);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1234' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1234' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 186: ErpDimValue = 1235
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1235);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1235' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1235' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 187: ErpDimValue = 1236
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1236);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1236' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1236' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 188: ErpDimValue = 1238
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1238);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1238' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1238' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 189: ErpDimValue = 1243
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1243);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1243' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1243' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 190: ErpDimValue = 1244
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1244);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1244' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1244' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 191: ErpDimValue = 1245
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1245);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1245' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1245' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 192: ErpDimValue = 1246
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1246);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1246' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1246' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 193: ErpDimValue = 1247
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1247);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1247' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1247' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 194: ErpDimValue = 1248
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1248);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1248' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1248' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 195: ErpDimValue = 1249
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1249);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1249' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1249' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 196: ErpDimValue = 1251
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1251);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1251' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1251' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 197: ErpDimValue = 1252
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1252);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1252' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1252' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 198: ErpDimValue = 1254
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1254);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1254' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1254' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 199: ErpDimValue = 1256
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1256);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1256' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1256' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 200: ErpDimValue = 1257
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1257);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1257' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1257' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 201: ErpDimValue = 1259
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1259);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1259' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1259' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 202: ErpDimValue = 1260
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1260);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1260' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1260' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 203: ErpDimValue = 1262
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1262);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1262' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1262' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 204: ErpDimValue = 1263
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1263);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1263' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1263' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 205: ErpDimValue = 1264
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1264);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1264' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1264' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 206: ErpDimValue = 1265
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1265);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1265' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1265' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 207: ErpDimValue = 1534
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1534);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1534' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1534' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 208: ErpDimValue = 1535
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1535);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1535' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1535' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 209: ErpDimValue = 1536
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1536);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1536' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1536' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 210: ErpDimValue = 1537
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1537);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1537' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1537' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 211: ErpDimValue = 1542
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1542);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1542' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1542' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 212: ErpDimValue = 1543
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1543);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1543' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1543' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 213: ErpDimValue = 1567
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1567);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1567' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1567' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 214: ErpDimValue = 1576
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1576);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1576' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1576' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 215: ErpDimValue = 1590
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1590);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1590' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1590' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 216: ErpDimValue = 1593
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1593);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1593' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1593' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 217: ErpDimValue = 1608
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1608);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1608' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1608' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 218: ErpDimValue = 1629
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1629);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1629' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1629' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 219: ErpDimValue = 1630
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1630);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1630' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1630' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 220: ErpDimValue = 1631
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1631);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1631' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1631' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 221: ErpDimValue = 1633
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1633);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1633' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1633' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 222: ErpDimValue = 1636
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1636);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1636' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1636' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 223: ErpDimValue = 1637
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1637);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1637' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1637' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 224: ErpDimValue = 1638
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1638);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1638' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1638' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 225: ErpDimValue = 1639
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1639);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1639' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1639' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 226: ErpDimValue = 1685
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1685);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1685' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1685' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 227: ErpDimValue = 1725
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1725);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1725' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1725' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 228: ErpDimValue = 1758
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1758);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1758' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1758' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 229: ErpDimValue = 1762
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1762);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1762' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1762' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 230: ErpDimValue = 1764
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1764);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1764' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1764' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 231: ErpDimValue = 1769
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1769);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1769' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1769' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 232: ErpDimValue = 1848
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1848);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1848' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1848' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 233: ErpDimValue = 1866
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1866);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1866' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1866' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 234: ErpDimValue = 1935
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1935);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1935' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1935' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 235: ErpDimValue = 9012
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 9012);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '9012' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '9012' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 236: ErpDimValue = 1581
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1581);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1581' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1581' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartner update for integration completed successfully.");
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
                Console.WriteLine($"Error during Partner update for integration: {ex.Message}");
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
                // Update LastModifiedBy and LastModifiedDate for updated partners
                int updates = await context.Partners
                    .Where(p => recordIds.Contains(p.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.LastModifiedBy, -1)
                        .SetProperty(p => p.LastModifiedDate, DateTime.UtcNow));
                
                Console.WriteLine($"Updated LastModifiedBy to -1 and LastModifiedDate for {updates} partner records");
                
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
