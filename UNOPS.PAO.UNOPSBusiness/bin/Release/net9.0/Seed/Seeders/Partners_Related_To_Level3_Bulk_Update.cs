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
    public static class Partners_Related_To_Level3_Bulk_Update
    {
        public static async Task UpdatePartnersForIntegrationAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting Partners Related To Level 3 bulk update - Setting LastModifiedBy and LastModifiedDate...");
            
            int updatedCount = 0;
            int notFoundCount = 0;
            var updatedRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: ErpDimValue = 1949
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
                
                // Record 2: ErpDimValue = 1807
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
                
                // Record 3: ErpDimValue = 1938
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
                
                // Record 4: ErpDimValue = 1943
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
                
                // Record 5: ErpDimValue = 1944
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
                
                // Record 6: ErpDimValue = 1029
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
                
                // Record 7: ErpDimValue = 1274
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1274);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1274' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1274' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 8: ErpDimValue = 1275
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1275);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1275' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1275' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 9: ErpDimValue = 1276
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1276);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1276' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1276' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 10: ErpDimValue = 1277
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1277);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1277' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1277' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 11: ErpDimValue = 1280
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1280);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1280' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1280' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 12: ErpDimValue = 1281
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1281);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1281' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1281' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 13: ErpDimValue = 1282
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1282);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1282' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1282' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 14: ErpDimValue = 1283
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1283);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1283' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1283' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 15: ErpDimValue = 1285
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1285);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1285' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1285' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 16: ErpDimValue = 1286
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1286);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1286' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1286' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 17: ErpDimValue = 1287
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1287);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1287' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1287' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 18: ErpDimValue = 1289
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1289);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1289' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1289' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 19: ErpDimValue = 1292
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1292);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1292' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1292' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 20: ErpDimValue = 1293
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1293);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1293' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1293' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 21: ErpDimValue = 1294
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1294);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1294' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1294' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 22: ErpDimValue = 1295
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1295);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1295' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1295' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 23: ErpDimValue = 1296
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1296);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1296' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1296' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 24: ErpDimValue = 1297
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1297);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1297' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1297' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 25: ErpDimValue = 1298
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1298);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1298' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1298' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 26: ErpDimValue = 1291
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1291);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1291' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1291' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 27: ErpDimValue = 1330
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1330);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1330' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1330' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 28: ErpDimValue = 1300
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1300);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1300' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1300' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 29: ErpDimValue = 1302
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1302);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1302' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1302' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 30: ErpDimValue = 1329
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1329);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1329' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1329' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 31: ErpDimValue = 1303
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1303);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1303' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1303' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 32: ErpDimValue = 1304
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1304);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1304' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1304' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 33: ErpDimValue = 1331
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1331);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1331' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1331' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 34: ErpDimValue = 1309
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1309);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1309' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1309' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 35: ErpDimValue = 1308
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1308);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1308' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1308' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 36: ErpDimValue = 1310
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1310);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1310' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1310' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 37: ErpDimValue = 1311
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1311);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1311' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1311' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 38: ErpDimValue = 1314
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1314);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1314' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1314' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 39: ErpDimValue = 1315
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1315);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1315' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1315' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 40: ErpDimValue = 1316
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1316);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1316' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1316' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 41: ErpDimValue = 1318
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1318);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1318' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1318' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 42: ErpDimValue = 1320
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1320);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1320' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1320' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 43: ErpDimValue = 1321
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1321);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1321' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1321' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 44: ErpDimValue = 1323
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1323);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1323' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1323' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 45: ErpDimValue = 1305
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1305);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1305' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1305' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 46: ErpDimValue = 1335
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1335);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1335' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1335' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 47: ErpDimValue = 1336
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1336);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1336' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1336' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 48: ErpDimValue = 1338
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1338);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1338' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1338' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 49: ErpDimValue = 1340
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1340);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1340' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1340' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 50: ErpDimValue = 1342
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1342);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1342' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1342' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 51: ErpDimValue = 1346
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1346);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1346' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1346' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 52: ErpDimValue = 1350
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1350);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1350' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1350' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 53: ErpDimValue = 1351
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1351);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1351' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1351' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 54: ErpDimValue = 1356
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1356);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1356' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1356' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 55: ErpDimValue = 1355
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1355);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1355' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1355' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 56: ErpDimValue = 1345
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1345);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1345' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1345' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 57: ErpDimValue = 1353
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1353);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1353' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1353' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 58: ErpDimValue = 1354
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1354);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1354' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1354' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 59: ErpDimValue = 1360
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1360);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1360' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1360' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 60: ErpDimValue = 1361
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1361);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1361' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1361' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 61: ErpDimValue = 1362
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1362);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1362' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1362' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 62: ErpDimValue = 1358
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1358);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1358' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1358' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 63: ErpDimValue = 1366
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1366);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1366' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1366' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 64: ErpDimValue = 1367
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1367);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1367' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1367' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 65: ErpDimValue = 1368
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1368);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1368' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1368' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 66: ErpDimValue = 1385
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1385);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1385' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1385' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 67: ErpDimValue = 1373
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1373);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1373' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1373' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 68: ErpDimValue = 1381
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1381);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1381' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1381' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 69: ErpDimValue = 1376
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1376);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1376' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1376' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 70: ErpDimValue = 1382
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1382);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1382' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1382' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 71: ErpDimValue = 1377
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1377);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1377' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1377' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 72: ErpDimValue = 1378
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1378);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1378' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1378' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 73: ErpDimValue = 1380
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1380);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1380' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1380' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 74: ErpDimValue = 1379
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1379);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1379' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1379' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 75: ErpDimValue = 1384
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1384);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1384' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1384' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 76: ErpDimValue = 1386
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1386);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1386' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1386' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 77: ErpDimValue = 1388
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1388);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1388' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1388' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 78: ErpDimValue = 1389
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1389);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1389' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1389' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 79: ErpDimValue = 1390
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1390);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1390' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1390' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 80: ErpDimValue = 1392
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1392);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1392' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1392' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 81: ErpDimValue = 1372
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1372);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1372' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1372' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 82: ErpDimValue = 1391
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1391);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1391' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1391' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 83: ErpDimValue = 1420
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1420);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1420' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1420' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 84: ErpDimValue = 1413
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1413);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1413' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1413' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 85: ErpDimValue = 1394
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1394);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1394' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1394' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 86: ErpDimValue = 1396
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1396);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1396' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1396' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 87: ErpDimValue = 1400
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1400);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1400' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1400' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 88: ErpDimValue = 1402
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1402);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1402' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1402' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 89: ErpDimValue = 1405
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1405);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1405' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1405' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 90: ErpDimValue = 1407
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1407);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1407' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1407' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 91: ErpDimValue = 1397
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1397);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1397' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1397' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 92: ErpDimValue = 1410
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1410);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1410' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1410' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 93: ErpDimValue = 1419
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1419);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1419' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1419' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 94: ErpDimValue = 1412
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1412);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1412' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1412' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 95: ErpDimValue = 1414
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1414);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1414' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1414' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 96: ErpDimValue = 1415
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1415);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1415' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1415' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 97: ErpDimValue = 1417
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1417);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1417' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1417' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 98: ErpDimValue = 1418
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1418);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1418' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1418' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 99: ErpDimValue = 1421
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1421);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1421' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1421' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 100: ErpDimValue = 1422
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1422);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1422' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1422' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 101: ErpDimValue = 1403
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1403);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1403' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1403' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 102: ErpDimValue = 1404
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1404);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1404' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1404' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 103: ErpDimValue = 1617
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1617);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1617' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1617' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 104: ErpDimValue = 1620
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1620);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1620' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1620' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 105: ErpDimValue = 1624
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1624);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1624' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1624' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 106: ErpDimValue = 1426
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1426);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1426' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1426' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 107: ErpDimValue = 1430
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1430);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1430' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1430' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 108: ErpDimValue = 1612
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1612);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1612' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1612' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 109: ErpDimValue = 1431
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1431);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1431' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1431' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 110: ErpDimValue = 1568
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1568);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1568' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1568' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 111: ErpDimValue = 1442
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1442);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1442' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1442' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 112: ErpDimValue = 1307
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1307);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1307' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1307' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 113: ErpDimValue = 1327
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1327);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1327' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1327' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 114: ErpDimValue = 1268
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1268);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1268' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1268' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 115: ErpDimValue = 1269
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1269);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1269' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1269' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 116: ErpDimValue = 1270
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1270);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1270' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1270' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 117: ErpDimValue = 1271
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1271);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1271' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1271' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 118: ErpDimValue = 1272
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1272);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1272' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1272' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 119: ErpDimValue = 1284
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1284);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1284' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1284' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 120: ErpDimValue = 1288
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1288);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1288' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1288' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 121: ErpDimValue = 1290
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1290);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1290' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1290' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 122: ErpDimValue = 1328
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1328);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1328' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1328' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 123: ErpDimValue = 1299
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1299);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1299' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1299' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 124: ErpDimValue = 1715
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1715);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1715' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1715' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 125: ErpDimValue = 1332
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1332);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1332' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1332' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 126: ErpDimValue = 1333
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1333);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1333' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1333' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 127: ErpDimValue = 1337
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1337);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1337' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1337' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 128: ErpDimValue = 1409
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1409);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1409' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1409' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 129: ErpDimValue = 1348
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1348);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1348' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1348' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 130: ErpDimValue = 1306
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1306);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1306' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1306' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 131: ErpDimValue = 1343
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1343);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1343' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1343' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 132: ErpDimValue = 1359
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1359);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1359' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1359' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 133: ErpDimValue = 1313
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1313);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1313' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1313' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 134: ErpDimValue = 1317
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1317);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1317' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1317' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 135: ErpDimValue = 1406
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1406);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1406' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1406' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 136: ErpDimValue = 1363
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1363);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1363' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1363' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 137: ErpDimValue = 1364
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1364);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1364' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1364' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 138: ErpDimValue = 1365
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1365);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1365' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1365' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 139: ErpDimValue = 1370
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1370);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1370' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1370' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 140: ErpDimValue = 1374
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1374);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1374' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1374' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 141: ErpDimValue = 1383
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1383);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1383' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1383' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 142: ErpDimValue = 1326
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1326);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1326' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1326' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 143: ErpDimValue = 1387
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1387);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1387' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1387' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 144: ErpDimValue = 1393
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1393);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1393' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1393' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 145: ErpDimValue = 1399
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1399);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1399' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1399' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 146: ErpDimValue = 1716
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1716);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1716' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1716' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 147: ErpDimValue = 1408
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1408);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1408' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1408' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 148: ErpDimValue = 1411
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1411);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1411' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1411' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 149: ErpDimValue = 1416
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1416);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1416' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1416' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 150: ErpDimValue = 1423
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1423);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1423' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1423' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 151: ErpDimValue = 1429
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1429);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1429' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1429' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 152: ErpDimValue = 1433
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1433);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1433' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1433' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 153: ErpDimValue = 1826
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1826);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1826' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1826' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 154: ErpDimValue = 1428
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1428);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1428' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1428' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 155: ErpDimValue = 1619
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1619);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1619' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1619' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 156: ErpDimValue = 1575
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1575);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1575' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1575' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 157: ErpDimValue = 1614
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1614);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1614' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1614' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 158: ErpDimValue = 1615
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1615);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1615' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1615' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 159: ErpDimValue = 1621
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1621);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1621' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1621' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 160: ErpDimValue = 1625
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1625);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1625' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1625' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 161: ErpDimValue = 1677
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1677);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1677' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1677' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 162: ErpDimValue = 1854
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1854);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1854' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1854' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 163: ErpDimValue = 1018
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1018);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1018' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1018' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 164: ErpDimValue = 1041
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1041);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1041' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1041' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 165: ErpDimValue = 1043
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1043);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1043' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1043' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 166: ErpDimValue = 1045
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1045);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1045' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1045' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 167: ErpDimValue = 1054
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1054);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1054' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1054' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 168: ErpDimValue = 1060
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1060);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1060' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1060' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 169: ErpDimValue = 1068
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1068);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1068' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1068' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 170: ErpDimValue = 1150
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1150);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1150' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1150' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 171: ErpDimValue = 1156
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1156);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1156' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1156' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 172: ErpDimValue = 1172
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1172);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1172' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1172' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 173: ErpDimValue = 1242
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1242);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1242' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1242' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 174: ErpDimValue = 1447
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1447);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1447' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1447' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 175: ErpDimValue = 1458
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1458);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1458' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1458' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 176: ErpDimValue = 1460
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1460);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1460' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1460' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 177: ErpDimValue = 1462
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1462);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1462' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1462' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 178: ErpDimValue = 1592
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1592);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1592' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1592' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 179: ErpDimValue = 1606
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1606);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1606' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1606' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 180: ErpDimValue = 1607
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1607);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1607' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1607' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 181: ErpDimValue = 1650
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1650);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1650' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1650' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 182: ErpDimValue = 1683
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1683);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1683' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1683' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 183: ErpDimValue = 1698
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1698);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1698' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1698' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 184: ErpDimValue = 1717
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1717);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1717' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1717' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 185: ErpDimValue = 1729
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1729);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1729' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1729' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 186: ErpDimValue = 1730
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1730);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1730' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1730' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 187: ErpDimValue = 1756
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1756);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1756' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1756' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 188: ErpDimValue = 1763
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1763);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1763' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1763' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 189: ErpDimValue = 1774
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1774);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1774' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1774' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 190: ErpDimValue = 1778
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1778);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1778' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1778' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 191: ErpDimValue = 1782
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1782);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1782' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1782' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 192: ErpDimValue = 1790
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1790);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1790' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1790' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 193: ErpDimValue = 1920
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1920);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1920' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1920' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 194: ErpDimValue = 1459
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1459);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1459' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1459' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 195: ErpDimValue = 1654
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1654);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1654' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1654' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 196: ErpDimValue = 1851
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1851);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1851' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1851' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 197: ErpDimValue = 1069
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1069);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1069' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1069' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 198: ErpDimValue = 1070
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1070);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1070' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1070' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 199: ErpDimValue = 1161
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1161);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1161' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1161' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 200: ErpDimValue = 1004
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1004);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1004' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1004' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 201: ErpDimValue = 1010
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1010);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1010' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1010' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 202: ErpDimValue = 1017
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1017);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1017' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1017' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 203: ErpDimValue = 1028
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1028);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1028' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1028' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 204: ErpDimValue = 1030
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1030);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1030' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1030' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 205: ErpDimValue = 1039
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1039);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1039' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1039' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 206: ErpDimValue = 1042
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1042);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1042' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1042' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 207: ErpDimValue = 1044
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1044);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1044' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1044' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 208: ErpDimValue = 1051
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1051);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1051' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1051' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 209: ErpDimValue = 1059
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1059);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1059' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1059' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 210: ErpDimValue = 1231
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1231);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1231' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1231' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 211: ErpDimValue = 1253
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1253);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1253' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1253' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 212: ErpDimValue = 1435
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1435);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1435' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1435' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 213: ErpDimValue = 1436
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1436);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1436' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1436' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 214: ErpDimValue = 1449
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1449);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1449' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1449' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 215: ErpDimValue = 1577
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1577);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1577' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1577' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 216: ErpDimValue = 1578
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1578);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1578' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1578' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 217: ErpDimValue = 1591
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1591);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1591' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1591' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 218: ErpDimValue = 1603
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1603);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1603' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1603' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 219: ErpDimValue = 1604
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1604);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1604' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1604' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 220: ErpDimValue = 1653
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1653);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1653' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1653' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 221: ErpDimValue = 1660
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1660);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1660' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1660' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 222: ErpDimValue = 1674
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1674);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1674' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1674' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 223: ErpDimValue = 1675
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1675);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1675' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1675' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 224: ErpDimValue = 1689
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1689);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1689' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1689' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 225: ErpDimValue = 1704
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1704);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1704' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1704' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 226: ErpDimValue = 1792
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1792);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1792' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1792' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 227: ErpDimValue = 1802
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1802);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1802' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1802' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 228: ErpDimValue = 1806
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1806);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1806' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1806' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 229: ErpDimValue = 1811
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1811);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1811' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1811' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 230: ErpDimValue = 1648
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1648);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1648' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1648' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 231: ErpDimValue = 1006
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1006);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1006' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1006' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 232: ErpDimValue = 1434
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1434);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1434' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1434' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 233: ErpDimValue = 1026
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
                
                // Record 234: ErpDimValue = 1031
                {
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1031);
                    
                    if (existingPartner != null)
                    {
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '1031' - {existingPartner.Name}");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1031' does not exist.");
                        notFoundCount++;
                    }
                }
                
                // Record 235: ErpDimValue = 1739
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
                
                // Record 236: ErpDimValue = 1649
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
                
                // Record 237: ErpDimValue = 1032
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
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nPartners Related To Level 3 bulk update completed successfully.");
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
                Console.WriteLine($"Error during Partners Related To Level 3 bulk update: {ex.Message}");
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
