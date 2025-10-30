using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Partner_LiaisonOffice_Fixes_v3
    {
        public static async Task UpdatePartnerLiaisonOfficesAsync(UNOPSAppDbContext context)
        {
            // Create mapping from LiaisonOffice Name to Id (handle duplicates by taking first, filter out null names)
            var liaisonOffices = await context.LiaisonOffices.ToListAsync();
            var liaisonOfficeMapping = liaisonOffices
                .Where(lo => !string.IsNullOrEmpty(lo.Name))
                .GroupBy(lo => lo.Name)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Define ErpDimValue to LiaisonOffice name mapping
            var erpDimValueToLiaisonOffice = new Dictionary<int, string>
            {
                { 1142, "Other PLG Managed Partners" },
                { 1109, "Other PLG Managed Partners" },
                { 1445, "Other PLG Managed Partners" },
                { 1448, "Other PLG Managed Partners" },
                { 1680, "Other PLG Managed Partners" },
                { 1681, "Other PLG Managed Partners" },
                { 1679, "Other PLG Managed Partners" },
                { 1193, "Other PLG Managed Partners" },
                { 1192, "Other PLG Managed Partners" },
                { 1222, "Other PLG Managed Partners" },
                { 1183, "Other PLG Managed Partners" },
                { 1261, "Other PLG Managed Partners" }
            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Process each ErpDimValue
                foreach (var (erpDimValue, liaisonOfficeName) in erpDimValueToLiaisonOffice)
                {
                    // Check if the liaison office exists in the mapping
                    if (!liaisonOfficeMapping.ContainsKey(liaisonOfficeName))
                    {
                        Console.WriteLine($"Warning: LiaisonOffice '{liaisonOfficeName}' not found in database for ErpDimValue {erpDimValue}");
                        continue;
                    }

                    var liaisonOfficeId = liaisonOfficeMapping[liaisonOfficeName];

                    // Find partner by ErpDimValue where LiaisonOfficeId is null
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == erpDimValue && p.LiaisonOfficeId == null);

                    if (partner != null)
                    {
                        // Update only the LiaisonOfficeId field
                        partner.LiaisonOfficeId = liaisonOfficeId;
                        partner.LastModifiedBy = -1; // Opportunity+ system user
                        partner.LastModifiedDate = DateTime.UtcNow;

                        Console.WriteLine($"Updated Partner ErpDimValue {erpDimValue} - '{partner.Name}' with LiaisonOfficeId: {liaisonOfficeId} ({liaisonOfficeName})");
                    }
                    else
                    {
                        var existingPartner = await context.Partners
                            .FirstOrDefaultAsync(p => p.ErpDimValue == erpDimValue);
                        
                        if (existingPartner != null && existingPartner.LiaisonOfficeId != null)
                        {
                            Console.WriteLine($"Skipped Partner ErpDimValue {erpDimValue} - '{existingPartner.Name}' (LiaisonOfficeId already set: {existingPartner.LiaisonOfficeId})");
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

                Console.WriteLine("Partner LiaisonOffice updates completed successfully.");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating Partner LiaisonOffices: {ex.Message}");
                throw;
            }
        }
    }
}

