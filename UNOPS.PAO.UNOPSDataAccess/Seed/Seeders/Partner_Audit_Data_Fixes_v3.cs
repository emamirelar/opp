using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Partner_Audit_Data_Fixes_v3
    {
        public static async Task UpdatePartnerAuditDataAsync(UNOPSAppDbContext context)
        {
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Find all partners where CreatedBy or LastModifiedBy is 0
                var partnersToUpdate = await context.Partners
                    .Where(p => p.CreatedBy == 0 || p.LastModifiedBy == 0)
                    .ToListAsync();

                if (partnersToUpdate.Count == 0)
                {
                    Console.WriteLine("No partners found with CreatedBy or LastModifiedBy set to 0.");
                    await transaction.CommitAsync();
                    return;
                }

                Console.WriteLine($"Found {partnersToUpdate.Count} partners to update.");

                int createdByUpdates = 0;
                int lastModifiedByUpdates = 0;

                foreach (var partner in partnersToUpdate)
                {
                    bool updated = false;

                    // Update CreatedBy if it's 0
                    if (partner.CreatedBy == 0)
                    {
                        partner.CreatedBy = -1; // Opportunity+ system user
                        createdByUpdates++;
                        updated = true;
                    }

                    // Update LastModifiedBy if it's 0
                    if (partner.LastModifiedBy == 0)
                    {
                        partner.LastModifiedBy = -1; // Opportunity+ system user
                        partner.LastModifiedDate = DateTime.UtcNow;
                        lastModifiedByUpdates++;
                        updated = true;
                    }

                    if (updated)
                    {
                        Console.WriteLine($"Updated Partner ErpDimValue {partner.ErpDimValue} - '{partner.Name}'");
                    }
                }

                // Save all changes at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"Partner system user updates completed successfully.");
                Console.WriteLine($"Total CreatedBy updates: {createdByUpdates}");
                Console.WriteLine($"Total LastModifiedBy updates: {lastModifiedByUpdates}");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating Partner system users: {ex.Message}");
                throw;
            }
        }
    }
}

