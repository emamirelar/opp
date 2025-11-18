using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Partner_FocalPoint_Fixes_v3
    {
        public static async Task UpdatePartnerFocalPointsAsync(UNOPSAppDbContext context)
        {
            // Create mapping from PAOUser Email to Id (handle duplicates by taking first, filter out null emails)
            // Convert emails to lowercase for case-insensitive matching
            var paoUsers = await context.PAOUsers
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();
            var paoUserMapping = paoUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .GroupBy(u => u.Email!.ToLower())
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Define ErpDimValue to Focal Point email mapping (all emails in lowercase)
            var erpDimValueToFocalPoint = new Dictionary<int, string>
            {
                { 1902, "martina@unops.org" },
                { 1738, "laetitiak@unops.org" },
                { 1864, "laetitiak@unops.org" },
                { 1089, "laetitiak@unops.org" },
                { 1610, "asbjornb@unops.org" },
                { 1613, "asbjornb@unops.org" },
                { 1618, "asbjornb@unops.org" },
                { 1024, "patrickel@unops.org" },
                { 1121, "patrickel@unops.org" },
                { 1702, "patrickel@unops.org" },
                { 1082, "patrickel@unops.org" },
                { 1123, "asbjornb@unops.org" },
                { 1910, "asbjornb@unops.org" },
                { 1083, "patrickel@unops.org" },
                { 1111, "asbjornb@unops.org" },
                { 1025, "mariacarmenco@unops.org" },
                { 1917, "patrickel@unops.org" },
                { 1649, "mariacarmenco@unops.org" },
                { 1031, "mariacarmenco@unops.org" },
                { 1032, "mariacarmenco@unops.org" },
                { 1944, "mariacarmenco@unops.org" },
                { 1029, "mariacarmenco@unops.org" },
                { 1026, "mariacarmenco@unops.org" },
                { 1943, "mariacarmenco@unops.org" },
                { 1739, "mariacarmenco@unops.org" },
                { 1752, "asbjornb@unops.org" },
                { 1124, "asbjornb@unops.org" },
                { 1711, "asbjornb@unops.org" },
                { 1903, "laetitiak@unops.org" },
                { 1622, "mariacarmenco@unops.org" },
                { 1445, "daniele@unops.org" },
                { 1126, "laetitiak@unops.org" },
                { 1448, "daniele@unops.org" },
                { 1679, "daniele@unops.org" },
                { 1681, "daniele@unops.org" },
                { 1680, "daniele@unops.org" },
                { 1737, "laetitiak@unops.org" },
                { 1589, "laetitiak@unops.org" },
                { 1443, "christinebo@unops.org" },
                { 1049, "asbjornb@unops.org" },
                { 1128, "asbjornb@unops.org" },
                { 1628, "christinebo@unops.org" },
                { 1444, "christinebo@unops.org" },
                { 1084, "patrickel@unops.org" },
                { 1247, "martina@unops.org" },
                { 1547, "christinebo@unops.org" },
                { 1788, "patrickel@unops.org" },
                { 1571, "halas@unops.org" },
                { 1266, "martina@unops.org" },
                { 1905, "martina@unops.org" },
                { 1131, "yukom@unops.org" },
                { 1906, "yukom@unops.org" },
                { 1907, "yukom@unops.org" },
                { 1096, "yukom@unops.org" },
                { 1095, "yukom@unops.org" },
                { 1868, "yukom@unops.org" },
                { 1915, "halas@unops.org" },
                { 1669, "laetitiak@unops.org" },
                { 1105, "arnauds@unops.org" },
                { 1761, "halas@unops.org" },
                { 1312, "halas@unops.org" },
                { 1914, "halas@unops.org" },
                { 1904, "martina@unops.org" },
                { 1114, "patrickel@unops.org" },
                { 1546, "christinebo@unops.org" },
                { 1087, "asbjornb@unops.org" },
                { 1959, "mariacarmenco@unops.org" },
                { 1086, "asbjornb@unops.org" },
                { 1091, "asbjornb@unops.org" },
                { 1102, "asbjornb@unops.org" },
                { 1753, "asbjornb@unops.org" },
                { 1456, "halas@unops.org" },
                { 1101, "asbjornb@unops.org" },
                { 1136, "asbjornb@unops.org" },
                { 1837, "asbjornb@unops.org" },
                { 1688, "asbjornb@unops.org" },
                { 1319, "halas@unops.org" },
                { 1916, "halas@unops.org" },
                { 1371, "halas@unops.org" },
                { 1919, "halas@unops.org" },
                { 1912, "halas@unops.org" },
                { 1818, "halas@unops.org" },
                { 1714, "halas@unops.org" },
                { 1139, "arnauds@unops.org" },
                { 1395, "halas@unops.org" },
                { 1911, "halas@unops.org" },
                { 1918, "halas@unops.org" },
                { 1754, "asbjornb@unops.org" },
                { 1723, "halas@unops.org" },
                { 1108, "asbjornb@unops.org" },
                { 1908, "arnauds@unops.org" },
                { 1267, "asbjornb@unops.org" },
                { 1909, "asbjornb@unops.org" },
                { 1646, "christinebo@unops.org" },
                { 1222, "mikaelag@unops.org" },
                { 1193, "norikok@unops.org" },
                { 1192, "norikok@unops.org" },
                { 1183, "laurentium@unops.org" },
                { 1425, "halas@unops.org" },
                { 1913, "halas@unops.org" },
                { 1144, "asbjornb@unops.org" },
                { 1145, "patrickel@unops.org" },
                { 1641, "patrickel@unops.org" },
                { 1116, "patrickel@unops.org" },
                { 1112, "patrickel@unops.org" },
                { 1115, "patrickel@unops.org" },
                { 1642, "patrickel@unops.org" },
                { 1113, "patrickel@unops.org" },
                { 1898, "patrickel@unops.org" },
                { 1940, "mariacarmenco@unops.org" },
                { 1261, "norikok@unops.org" }
            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Process each ErpDimValue
                foreach (var (erpDimValue, focalPointEmail) in erpDimValueToFocalPoint)
                {
                    // Check if the focal point user exists in the mapping
                    if (!paoUserMapping.ContainsKey(focalPointEmail))
                    {
                        Console.WriteLine($"Warning: Focal Point User with email '{focalPointEmail}' not found in database for ErpDimValue {erpDimValue}");
                        continue;
                    }

                    var focalPointUserId = paoUserMapping[focalPointEmail];

                    // Find partner by ErpDimValue where PartnerFocalPointUserId is null
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == erpDimValue && p.PartnerFocalPointUserId == null);

                    if (partner != null)
                    {
                        // Update only the PartnerFocalPointUserId field
                        partner.PartnerFocalPointUserId = focalPointUserId;
                        partner.LastModifiedBy = -1; // Opportunity+ system user
                        partner.LastModifiedDate = DateTime.UtcNow;

                        Console.WriteLine($"Updated Partner ErpDimValue {erpDimValue} - '{partner.Name}' with PartnerFocalPointUserId: {focalPointUserId} ({focalPointEmail})");
                    }
                    else
                    {
                        var existingPartner = await context.Partners
                            .FirstOrDefaultAsync(p => p.ErpDimValue == erpDimValue);
                        
                        if (existingPartner != null && existingPartner.PartnerFocalPointUserId != null)
                        {
                            Console.WriteLine($"Skipped Partner ErpDimValue {erpDimValue} - '{existingPartner.Name}' (PartnerFocalPointUserId already set: {existingPartner.PartnerFocalPointUserId})");
                        }
                        else if (existingPartner == null)
                        {
                            Console.WriteLine($"Warning: Partner with ErpDimValue {erpDimValue} not found in database");
                        }
                    }
                }

                // Save all changes at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine("Partner FocalPoint updates completed successfully.");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating Partner FocalPoints: {ex.Message}");
                throw;
            }
        }
    }
}

