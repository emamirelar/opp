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
            // Create mapping from PAOUser Name to Id (handle duplicates by taking first, filter out null names)
            var paoUsers = await context.PAOUsers
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();
            var paoUserMapping = paoUsers
                .Where(u => !string.IsNullOrEmpty(u.Name))
                .GroupBy(u => u.Name)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Define ErpDimValue to Focal Point name mapping
            var erpDimValueToFocalPoint = new Dictionary<int, string>
            {
                { 1902, "Martin Carlos Eduardo AREVALO DE LEON" },
                { 1738, "Laetitia Kraus" },
                { 1864, "Laetitia Kraus" },
                { 1089, "Laetitia Kraus" },
                { 1610, "Asbjorn Brink" },
                { 1613, "Asbjorn Brink" },
                { 1618, "Asbjorn Brink" },
                { 1024, "Michael Patrick Ellsworth" },
                { 1121, "Michael Patrick Ellsworth" },
                { 1702, "Michael Patrick Ellsworth" },
                { 1082, "Michael Patrick Ellsworth" },
                { 1123, "Asbjorn Brink" },
                { 1910, "Asbjorn Brink" },
                { 1083, "Michael Patrick Ellsworth" },
                { 1111, "Asbjorn Brink" },
                { 1025, "Mariacarmen   COLITTI" },
                { 1917, "Michael Patrick Ellsworth" },
                { 1649, "Mariacarmen   COLITTI" },
                { 1031, "Mariacarmen   COLITTI" },
                { 1032, "Mariacarmen   COLITTI" },
                { 1944, "Mariacarmen   COLITTI" },
                { 1029, "Mariacarmen   COLITTI" },
                { 1026, "Mariacarmen   COLITTI" },
                { 1943, "Mariacarmen   COLITTI" },
                { 1739, "Mariacarmen   COLITTI" },
                { 1752, "Asbjorn Brink" },
                { 1124, "Asbjorn Brink" },
                { 1711, "Asbjorn Brink" },
                { 1903, "Laetitia Kraus" },
                { 1622, "Mariacarmen   COLITTI" },
                { 1445, "Daniel Nicolas Elliott" },
                { 1126, "Laetitia Kraus" },
                { 1448, "Daniel Nicolas Elliott" },
                { 1679, "Daniel Nicolas Elliott" },
                { 1681, "Daniel Nicolas Elliott" },
                { 1680, "Daniel Nicolas Elliott" },
                { 1737, "Laetitia Kraus" },
                { 1589, "Laetitia Kraus" },
                { 1443, "Christine Leslie BOWERS" },
                { 1049, "Asbjorn Brink" },
                { 1128, "Asbjorn Brink" },
                { 1628, "Christine Leslie BOWERS" },
                { 1444, "Christine Leslie BOWERS" },
                { 1084, "Michael Patrick Ellsworth" },
                { 1247, "Martin Carlos Eduardo AREVALO DE LEON" },
                { 1547, "Christine Leslie BOWERS" },
                { 1788, "Michael Patrick Ellsworth" },
                { 1571, "Hala R Alsharifi" },
                { 1266, "Martin Carlos Eduardo AREVALO DE LEON" },
                { 1905, "Martin Carlos Eduardo AREVALO DE LEON" },
                { 1131, "Yuko MAEKAWA" },
                { 1906, "Yuko MAEKAWA" },
                { 1907, "Yuko MAEKAWA" },
                { 1096, "Yuko MAEKAWA" },
                { 1095, "Yuko MAEKAWA" },
                { 1868, "Yuko MAEKAWA" },
                { 1915, "Hala R Alsharifi" },
                { 1669, "Laetitia Kraus" },
                { 1105, "Arnaud Sgambato" },
                { 1761, "Hala R Alsharifi" },
                { 1312, "Hala R Alsharifi" },
                { 1914, "Hala R Alsharifi" },
                { 1904, "Martin Carlos Eduardo AREVALO DE LEON" },
                { 1114, "Michael Patrick Ellsworth" },
                { 1546, "Christine Leslie BOWERS" },
                { 1087, "Asbjorn Brink" },
                { 1959, "Mariacarmen   COLITTI" },
                { 1086, "Asbjorn Brink" },
                { 1091, "Asbjorn Brink" },
                { 1102, "Asbjorn Brink" },
                { 1753, "Asbjorn Brink" },
                { 1456, "Hala R Alsharifi" },
                { 1101, "Asbjorn Brink" },
                { 1136, "Asbjorn Brink" },
                { 1837, "Asbjorn Brink" },
                { 1688, "Asbjorn Brink" },
                { 1319, "Hala R Alsharifi" },
                { 1916, "Hala R Alsharifi" },
                { 1371, "Hala R Alsharifi" },
                { 1919, "Hala R Alsharifi" },
                { 1912, "Hala R Alsharifi" },
                { 1818, "Hala R Alsharifi" },
                { 1714, "Hala R Alsharifi" },
                { 1139, "Arnaud Sgambato" },
                { 1395, "Hala R Alsharifi" },
                { 1911, "Hala R Alsharifi" },
                { 1918, "Hala R Alsharifi" },
                { 1754, "Asbjorn Brink" },
                { 1723, "Hala R Alsharifi" },
                { 1108, "Asbjorn Brink" },
                { 1908, "Arnaud Sgambato" },
                { 1267, "Asbjorn Brink" },
                { 1909, "Asbjorn Brink" },
                { 1646, "Christine Leslie BOWERS" },
                { 1222, "Mikaela Solfrid Gerkman" },
                { 1193, "Noriko   Kominami" },
                { 1192, "Noriko   Kominami" },
                { 1183, "Laurentiu Mastacan" },
                { 1425, "Hala R Alsharifi" },
                { 1913, "Hala R Alsharifi" },
                { 1144, "Asbjorn Brink" },
                { 1145, "Michael Patrick Ellsworth" },
                { 1641, "Michael Patrick Ellsworth" },
                { 1116, "Michael Patrick Ellsworth" },
                { 1112, "Michael Patrick Ellsworth" },
                { 1115, "Michael Patrick Ellsworth" },
                { 1642, "Michael Patrick Ellsworth" },
                { 1113, "Michael Patrick Ellsworth" },
                { 1898, "Michael Patrick Ellsworth" },
                { 1940, "Mariacarmen   COLITTI" },
                { 1261, "Noriko   Kominami" }
            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Process each ErpDimValue
                foreach (var (erpDimValue, focalPointName) in erpDimValueToFocalPoint)
                {
                    // Check if the focal point user exists in the mapping
                    if (!paoUserMapping.ContainsKey(focalPointName))
                    {
                        Console.WriteLine($"Warning: Focal Point User '{focalPointName}' not found in database for ErpDimValue {erpDimValue}");
                        continue;
                    }

                    var focalPointUserId = paoUserMapping[focalPointName];

                    // Find partner by ErpDimValue where PartnerFocalPointUserId is null
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == erpDimValue && p.PartnerFocalPointUserId == null);

                    if (partner != null)
                    {
                        // Update only the PartnerFocalPointUserId field
                        partner.PartnerFocalPointUserId = focalPointUserId;
                        partner.LastModifiedBy = -1; // Opportunity+ system user
                        partner.LastModifiedDate = DateTime.UtcNow;

                        Console.WriteLine($"Updated Partner ErpDimValue {erpDimValue} - '{partner.Name}' with PartnerFocalPointUserId: {focalPointUserId} ({focalPointName})");
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

