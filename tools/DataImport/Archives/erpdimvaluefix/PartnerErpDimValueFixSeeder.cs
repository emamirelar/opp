using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Fixes ErpDimValue for partners that have values > 9999 by recalculating
    /// based on the highest ErpDimValue excluding the 8000-9999 range.
    /// This seeder addresses the issue where partners were assigned ErpDimValues > 9999
    /// even though the actual highest value (excluding 8000-9999) was much lower.
    /// Considers all partners (including soft-deleted) when calculating to ensure unique values.
    /// </summary>
    public static class PartnerErpDimValueFixSeeder
    {
        public static async Task FixPartnerErpDimValuesAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("🔄 Fixing Partner ErpDimValues (excluding 8000-9999 range)...");

            // Get partners with ErpDimValue > 9999 that need to be fixed
            var partnersToFix = await context.Partners
                .Where(p => p.ErpDimValue.HasValue 
                    && p.ErpDimValue.Value > 9999)
                .OrderBy(p => p.ErpDimValue)
                .ToListAsync();

            if (partnersToFix.Count == 0)
            {
                Console.WriteLine("  ℹ️  No partners found with ErpDimValue > 9999");
                Console.WriteLine("✅ Partner ErpDimValue fix completed\n");
                return;
            }

            Console.WriteLine($"  📊 Found {partnersToFix.Count} partner(s) with ErpDimValue > 9999");

            // Calculate the highest ErpDimValue excluding 8000-9999 range
            // Note: Considers all partners regardless of deletion status to ensure unique values
            // Assuming the ErpDimValue should be below 8000 for the fixes and ignoring other 9999+ records
            var highestValidErpDimValue = await context.Partners
                .Where(p => p.ErpDimValue.HasValue 
                    && (p.ErpDimValue.Value < 8000))
                .MaxAsync(p => (int?)p.ErpDimValue) ?? 0;

            Console.WriteLine($"  📊 Highest valid ErpDimValue (excluding 8000-9999 and all partners): {highestValidErpDimValue}");

            // Start assigning from the next available value
            int nextErpDimValue = highestValidErpDimValue + 1;

            // Track the ErpDimValues we're going to use to avoid conflicts
            // Note: Includes all partners (even soft-deleted) to prevent duplicate values
            var usedErpDimValues = new HashSet<int>(
                await context.Partners
                    .Where(p => p.ErpDimValue.HasValue)
                    .Select(p => p.ErpDimValue.Value)
                    .ToListAsync()
            );

            // Reassign ErpDimValues to partners with values > 9999
            foreach (var partner in partnersToFix)
            {
                var oldValue = partner.ErpDimValue;

                // Find the next available ErpDimValue that:
                // 1. Is not in the used set
                // 2. Is not in the 8000-9999 range
                while (usedErpDimValues.Contains(nextErpDimValue) || 
                       (nextErpDimValue >= 8000 && nextErpDimValue <= 9999))
                {
                    nextErpDimValue++;
                }

                // Assign the new ErpDimValue
                partner.ErpDimValue = nextErpDimValue;
                partner.LastModifiedBy = 0; // System user
                partner.LastModifiedDate = DateTime.UtcNow;

                // Add to used set and increment for next iteration
                usedErpDimValues.Add(nextErpDimValue);
                nextErpDimValue++;

                Console.WriteLine($"  🔄 Partner ID {partner.Id} ('{partner.Name}'): {oldValue} → {partner.ErpDimValue}");
            }

            // Save all changes
            await context.SaveChangesAsync();

            Console.WriteLine($"  ✅ Successfully reassigned ErpDimValue for {partnersToFix.Count} partner(s)");
            Console.WriteLine("✅ Partner ErpDimValue fix completed\n");
        }
    }
}

