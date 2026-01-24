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
    public static class Partners_Related_To_Level2_Bulk_Update
    {
        public static async Task UpdatePartnersForIntegrationAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting Partners Related To Level 2 bulk update - Setting LastModifiedBy and LastModifiedDate...");
            
            int updatedCount = 0;
            int notFoundCount = 0;
            var updatedRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: ErpDimValue = 1948
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
                
                // Record 2: ErpDimValue = 1933
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
                
                // Record 3: ErpDimValue = 1947
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
                
                // Record 4: ErpDimValue = 1945
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
                
                // Record 5: ErpDimValue = 1942
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
                
                // Record 6: ErpDimValue = 1934
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
                
                // Record 7: ErpDimValue = 1941
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
                
                // Record 8: ErpDimValue = 1540
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1540);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1540' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1540' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 9: ErpDimValue = 1581
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
                
                // Record 10: ErpDimValue = 1690
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1690);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1690' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1690' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 11: ErpDimValue = 1821
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1821);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1821' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1821' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 12: ErpDimValue = 1709
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1709);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1709' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1709' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 13: ErpDimValue = 1597
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1597);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1597' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1597' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 14: ErpDimValue = 1719
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1719);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1719' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1719' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 15: ErpDimValue = 1048
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1048);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1048' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1048' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 16: ErpDimValue = 1804
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1804);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1804' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1804' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 17: ErpDimValue = 1755
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1755);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1755' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1755' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 18: ErpDimValue = 1820
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1820);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1820' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1820' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 19: ErpDimValue = 1676
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1676);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1676' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1676' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 20: ErpDimValue = 1695
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1695);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1695' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1695' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 21: ErpDimValue = 1692
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1692);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1692' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1692' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 22: ErpDimValue = 1694
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1694);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1694' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1694' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 23: ErpDimValue = 1830
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1830);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1830' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1830' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 24: ErpDimValue = 1019
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1019);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1019' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1019' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 25: ErpDimValue = 1640
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1640);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1640' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1640' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 26: ErpDimValue = 1003
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1003);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1003' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1003' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 27: ErpDimValue = 1872
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1872);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1872' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1872' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 28: ErpDimValue = 1749
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1749);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1749' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1749' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 29: ErpDimValue = 1743
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1743);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1743' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1743' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 30: ErpDimValue = 1446
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1446);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1446' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1446' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 31: ErpDimValue = 1020
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1020);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1020' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1020' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 32: ErpDimValue = 1074
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1074);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1074' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1074' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 33: ErpDimValue = 1772
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1772);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1772' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1772' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 34: ErpDimValue = 1146
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1146);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1146' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1146' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 35: ErpDimValue = 1696
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1696);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1696' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1696' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 36: ErpDimValue = 1645
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1645);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1645' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1645' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 37: ErpDimValue = 1768
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1768);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1768' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1768' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 38: ErpDimValue = 1664
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1664);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1664' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1664' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 39: ErpDimValue = 1795
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1795);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1795' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1795' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 40: ErpDimValue = 1595
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1595);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1595' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1595' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 41: ErpDimValue = 1585
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1585);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1585' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1585' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 42: ErpDimValue = 1801
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1801);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1801' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1801' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 43: ErpDimValue = 1751
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1751);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1751' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1751' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 44: ErpDimValue = 1722
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1722);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1722' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1722' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 45: ErpDimValue = 1816
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1816);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1816' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1816' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 46: ErpDimValue = 1588
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1588);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1588' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1588' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 47: ErpDimValue = 1579
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1579);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1579' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1579' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 48: ErpDimValue = 1875
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1875);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1875' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1875' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 49: ErpDimValue = 1671
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1671);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1671' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1671' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 50: ErpDimValue = 1057
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1057);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1057' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1057' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 51: ErpDimValue = 1748
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1748);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1748' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1748' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 52: ErpDimValue = 1071
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1071);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1071' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1071' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 53: ErpDimValue = 1580
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1580);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1580' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1580' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 54: ErpDimValue = 1770
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1770);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1770' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1770' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 55: ErpDimValue = 1594
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1594);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1594' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1594' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 56: ErpDimValue = 1149
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1149);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1149' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1149' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 57: ErpDimValue = 1750
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1750);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1750' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1750' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 58: ErpDimValue = 1759
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1759);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1759' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1759' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 59: ErpDimValue = 1862
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1862);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1862' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1862' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 60: ErpDimValue = 1931
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1931);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1931' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1931' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 61: ErpDimValue = 1841
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1841);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1841' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1841' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 62: ErpDimValue = 1829
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1829);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1829' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1829' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 63: ErpDimValue = 1598
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1598);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1598' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1598' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 64: ErpDimValue = 1034
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1034);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1034' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1034' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 65: ErpDimValue = 1735
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1735);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1735' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1735' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 66: ErpDimValue = 1147
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1147);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1147' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1147' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 67: ErpDimValue = 1845
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1845);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1845' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1845' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 68: ErpDimValue = 1036
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1036);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1036' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1036' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 69: ErpDimValue = 1767
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1767);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1767' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1767' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 70: ErpDimValue = 1684
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1684);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1684' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1684' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 71: ErpDimValue = 1727
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1727);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1727' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1727' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 72: ErpDimValue = 1016
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1016);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1016' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1016' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 73: ErpDimValue = 1455
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1455);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1455' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1455' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 74: ErpDimValue = 1155
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1155);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1155' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1155' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 75: ErpDimValue = 1659
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1659);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1659' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1659' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 76: ErpDimValue = 1839
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1839);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1839' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1839' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 77: ErpDimValue = 1072
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1072);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1072' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1072' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 78: ErpDimValue = 1814
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1814);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1814' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1814' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 79: ErpDimValue = 1584
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1584);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1584' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1584' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 80: ErpDimValue = 1843
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1843);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1843' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1843' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 81: ErpDimValue = 1960
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1960);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1960' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1960' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 82: ErpDimValue = 1713
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1713);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1713' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1713' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 83: ErpDimValue = 1682
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1682);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1682' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1682' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 84: ErpDimValue = 1824
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1824);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1824' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1824' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 85: ErpDimValue = 1789
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1789);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1789' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1789' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 86: ErpDimValue = 1450
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1450);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1450' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1450' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 87: ErpDimValue = 1644
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1644);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1644' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1644' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 88: ErpDimValue = 1700
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1700);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1700' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1700' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 89: ErpDimValue = 1873
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1873);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1873' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1873' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 90: ErpDimValue = 1747
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1747);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1747' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1747' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 91: ErpDimValue = 1744
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1744);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1744' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1744' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 92: ErpDimValue = 1867
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1867);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1867' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1867' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 93: ErpDimValue = 1844
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1844);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1844' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1844' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 94: ErpDimValue = 1173
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1173);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1173' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1173' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 95: ErpDimValue = 1033
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1033);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1033' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1033' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 96: ErpDimValue = 1663
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1663);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1663' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1663' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 97: ErpDimValue = 1035
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1035);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1035' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1035' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 98: ErpDimValue = 1158
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1158);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1158' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1158' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 99: ErpDimValue = 1797
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1797);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1797' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1797' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 100: ErpDimValue = 1452
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1452);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1452' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1452' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 101: ErpDimValue = 1453
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1453);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1453' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1453' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 102: ErpDimValue = 1776
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1776);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1776' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1776' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 103: ErpDimValue = 1037
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1037);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1037' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1037' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 104: ErpDimValue = 1454
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1454);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1454' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1454' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 105: ErpDimValue = 1582
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1582);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1582' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1582' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 106: ErpDimValue = 1665
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1665);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1665' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1665' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 107: ErpDimValue = 1736
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1736);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1736' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1736' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 108: ErpDimValue = 1846
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1846);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1846' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1846' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 109: ErpDimValue = 1647
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1647);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1647' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1647' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 110: ErpDimValue = 1451
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1451);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1451' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1451' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 111: ErpDimValue = 1005
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1005);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1005' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1005' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 112: ErpDimValue = 1809
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1809);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1809' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1809' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 113: ErpDimValue = 1574
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1574);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1574' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1574' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 114: ErpDimValue = 1658
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1658);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1658' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1658' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 115: ErpDimValue = 1808
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1808);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1808' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1808' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 116: ErpDimValue = 1825
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1825);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1825' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1825' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 117: ErpDimValue = 1050
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1050);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1050' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1050' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 118: ErpDimValue = 1771
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1771);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1771' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1771' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 119: ErpDimValue = 1796
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1796);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1796' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1796' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 120: ErpDimValue = 1661
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1661);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1661' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1661' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 121: ErpDimValue = 1740
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1740);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1740' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1740' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 122: ErpDimValue = 1159
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1159);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1159' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1159' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 123: ErpDimValue = 1731
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1731);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1731' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1731' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 124: ErpDimValue = 1928
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1928);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1928' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1928' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 125: ErpDimValue = 1569
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1569);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1569' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1569' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 126: ErpDimValue = 1587
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1587);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1587' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1587' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 127: ErpDimValue = 1701
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1701);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1701' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1701' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 128: ErpDimValue = 1583
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1583);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1583' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1583' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 129: ErpDimValue = 1053
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1053);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1053' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1053' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 130: ErpDimValue = 1055
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1055);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1055' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1055' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 131: ErpDimValue = 1874
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1874);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1874' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1874' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 132: ErpDimValue = 1152
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1152);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1152' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1152' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 133: ErpDimValue = 1673
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1673);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1673' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1673' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 134: ErpDimValue = 1007
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1007);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1007' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1007' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 135: ErpDimValue = 1668
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1668);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1668' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1668' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 136: ErpDimValue = 1794
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1794);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1794' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1794' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 137: ErpDimValue = 1626
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1626);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1626' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1626' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 138: ErpDimValue = 1655
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1655);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1655' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1655' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 139: ErpDimValue = 1721
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1721);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1721' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1721' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 140: ErpDimValue = 1160
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1160);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1160' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1160' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 141: ErpDimValue = 1573
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1573);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1573' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1573' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 142: ErpDimValue = 1766
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1766);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1766' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1766' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 143: ErpDimValue = 1697
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1697);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1697' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1697' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 144: ErpDimValue = 1013
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1013);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1013' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1013' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 145: ErpDimValue = 1052
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1052);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1052' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1052' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 146: ErpDimValue = 1627
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1627);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1627' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1627' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 147: ErpDimValue = 1012
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1012);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1012' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1012' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 148: ErpDimValue = 1094
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1094);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1094' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1094' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 149: ErpDimValue = 1699
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1699);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1699' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1699' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 150: ErpDimValue = 1800
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1800);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1800' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1800' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 151: ErpDimValue = 1835
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1835);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1835' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1835' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 152: ErpDimValue = 1021
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1021);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1021' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1021' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 153: ErpDimValue = 1666
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1666);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1666' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1666' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 154: ErpDimValue = 1791
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1791);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1791' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1791' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 155: ErpDimValue = 1686
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1686);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1686' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1686' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 156: ErpDimValue = 1601
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1601);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1601' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1601' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 157: ErpDimValue = 1784
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1784);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1784' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1784' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 158: ErpDimValue = 1819
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1819);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1819' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1819' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 159: ErpDimValue = 1672
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1672);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1672' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1672' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 160: ErpDimValue = 1662
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1662);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1662' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1662' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 161: ErpDimValue = 1858
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1858);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1858' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1858' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 162: ErpDimValue = 1813
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1813);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1813' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1813' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 163: ErpDimValue = 1586
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1586);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1586' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1586' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 164: ErpDimValue = 1852
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1852);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1852' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1852' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 165: ErpDimValue = 1541
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1541);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1541' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1541' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 166: ErpDimValue = 1859
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1859);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1859' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1859' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 167: ErpDimValue = 1815
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1815);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1815' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1815' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 168: ErpDimValue = 1847
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1847);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1847' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1847' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 169: ErpDimValue = 1157
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1157);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1157' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1157' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 170: ErpDimValue = 1040
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1040);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1040' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1040' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 171: ErpDimValue = 1869
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1869);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1869' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1869' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 172: ErpDimValue = 1047
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1047);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1047' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1047' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 173: ErpDimValue = 1667
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1667);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1667' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1667' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 174: ErpDimValue = 1046
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1046);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1046' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1046' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 175: ErpDimValue = 1153
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1153);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1153' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1153' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 176: ErpDimValue = 1746
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1746);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1746' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1746' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 177: ErpDimValue = 1065
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1065);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1065' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1065' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 178: ErpDimValue = 1810
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1810);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1810' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1810' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 179: ErpDimValue = 1457
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1457);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1457' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1457' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 180: ErpDimValue = 1599
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1599);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1599' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1599' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 181: ErpDimValue = 1602
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1602);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1602' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1602' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 182: ErpDimValue = 1703
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1703);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1703' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1703' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 183: ErpDimValue = 1745
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1745);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1745' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1745' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 184: ErpDimValue = 1038
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1038);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1038' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1038' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 185: ErpDimValue = 1691
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1691);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1691' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1691' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 186: ErpDimValue = 1670
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1670);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1670' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1670' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 187: ErpDimValue = 1712
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1712);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1712' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1712' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 188: ErpDimValue = 1657
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1657);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1657' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1657' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 189: ErpDimValue = 1710
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1710);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1710' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1710' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 190: ErpDimValue = 1148
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1148);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1148' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1148' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 191: ErpDimValue = 1073
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1073);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1073' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1073' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 192: ErpDimValue = 1600
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1600);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1600' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1600' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 193: ErpDimValue = 1742
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1742);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1742' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1742' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 194: ErpDimValue = 1002
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1002);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1002' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1002' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 195: ErpDimValue = 1720
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1720);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1720' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1720' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 196: ErpDimValue = 1734
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1734);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1734' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1734' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 197: ErpDimValue = 1067
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1067);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1067' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1067' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 198: ErpDimValue = 1741
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1741);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1741' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1741' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 199: ErpDimValue = 1008
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1008);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1008' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1008' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 200: ErpDimValue = 1605
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1605);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1605' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1605' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 201: ErpDimValue = 1693
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1693);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1693' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1693' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 202: ErpDimValue = 1777
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1777);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1777' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1777' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 203: ErpDimValue = 1656
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1656);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1656' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1656' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 204: ErpDimValue = 1836
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1836);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1836' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1836' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 205: ErpDimValue = 1596
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1596);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1596' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1596' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 206: ErpDimValue = 1022
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1022);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1022' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1022' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartners Related To Level 2 bulk update completed successfully.");
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
                Console.WriteLine($"Error during Partners Related To Level 2 bulk update: {ex.Message}");
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
                // Update LastModifiedBy and LastModifiedDate for updated Partners
                int updates = await context.Partners
                    .Where(p => recordIds.Contains(p.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.LastModifiedBy, -1)
                        .SetProperty(p => p.LastModifiedDate, DateTime.UtcNow));
                
                Console.WriteLine($"Updated LastModifiedBy to -1 and LastModifiedDate for {updates} Partner records");
                
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
