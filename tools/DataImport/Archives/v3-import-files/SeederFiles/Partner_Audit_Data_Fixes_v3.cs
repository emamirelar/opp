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
            // Find the user ID for larsj@unops.org
            var larsjUser = await context.PAOUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == "larsj@unops.org");

            if (larsjUser == null)
            {
                Console.WriteLine("Warning: User with email 'larsj@unops.org' not found in database. No updates will be performed.");
                return;
            }

            int larsjUserId = larsjUser.Id;
            Console.WriteLine($"Found user 'larsj@unops.org' with ID: {larsjUserId}");

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Use ExecuteUpdateAsync to bypass audit interceptor
                // Update CreatedBy for partners where it matches larsj user ID
                int createdByUpdates = await context.Partners
                    .Where(p => p.CreatedBy == larsjUserId || p.CreatedBy == 0)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.CreatedBy, -1));

                Console.WriteLine($"Updated CreatedBy for {createdByUpdates} partners from user ID {larsjUserId} to -1 (system user)");

                // Update LastModifiedBy for partners where it matches larsj user ID
                int lastModifiedByUpdates = await context.Partners
                    .Where(p => p.LastModifiedBy == larsjUserId || p.LastModifiedBy == 0)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.LastModifiedBy, -1));

                Console.WriteLine($"Updated LastModifiedBy for {lastModifiedByUpdates} partners from user ID {larsjUserId} to -1 (system user)");

                if (createdByUpdates == 0 && lastModifiedByUpdates == 0)
                {
                    Console.WriteLine($"No partners found with CreatedBy or LastModifiedBy set to user ID {larsjUserId} (larsj@unops.org).");
                }

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"\nPartner audit data updates completed successfully.");
                Console.WriteLine($"Updated partners previously attributed to user 'larsj@unops.org' (ID: {larsjUserId})");
                Console.WriteLine($"Total partners with CreatedBy updated: {createdByUpdates}");
                Console.WriteLine($"Total partners with LastModifiedBy updated: {lastModifiedByUpdates}");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating Partner audit data: {ex.Message}");
                throw;
            }
        }
    }
}

