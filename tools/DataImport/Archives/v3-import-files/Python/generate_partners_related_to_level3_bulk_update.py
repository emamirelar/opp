import csv
import os
from datetime import datetime

def process_csv_and_generate_seeder(csv_file_path, output_cs_file_path):
    """
    Process the Partners CSV file and generate C# seeder code to update Partner records
    by ErpDimValue, setting LastModifiedBy to -1 and LastModifiedDate to DateTime.UtcNow.
    """
    
    partner_records = []
    
    # Read CSV file
    with open(csv_file_path, 'r', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        
        for row in reader:
            erp_dim_value = row.get('ErpDimValue', '').strip()
            
            # Skip if ErpDimValue is empty
            if not erp_dim_value:
                continue
            
            # Check if this ErpDimValue already exists in our collection (deduplicate)
            if not any(r['ErpDimValue'] == erp_dim_value for r in partner_records):
                partner_records.append({
                    'ErpDimValue': erp_dim_value
                })
    
    # Generate C# seeder file
    generate_csharp_seeder(partner_records, output_cs_file_path)
    
    print(f"\nProcessing complete!")
    print(f"Total unique Partner records to process: {len(partner_records)}")
    print(f"C# seeder file generated: {output_cs_file_path}")


def generate_csharp_seeder(records, output_file_path):
    """
    Generate the C# seeder file from the processed records.
    """
    
    # C# code template
    cs_code = """using Microsoft.EntityFrameworkCore;
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
"""
    
    # Add update logic for each record
    for idx, record in enumerate(records):
        erp_dim_value = record['ErpDimValue']
        
        cs_code += f"""                // Record {idx + 1}: ErpDimValue = {erp_dim_value}
                {{
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == {erp_dim_value});
                    
                    if (existingPartner != null)
                    {{
                        updatedRecordIds.Add(existingPartner.Id);
                        Console.WriteLine($"Found: Partner with ErpDimValue '{erp_dim_value}' - {{existingPartner.Name}}");
                        updatedCount++;
                    }}
                    else
                    {{
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '{erp_dim_value}' does not exist.");
                        notFoundCount++;
                    }}
                }}
                
"""
    
    # Add the closing code
    cs_code += """                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\\nPartners Related To Level 3 bulk update completed successfully.");
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
            Console.WriteLine("\\nApplying audit data fixes to prevent LastModifiedBy and LastModifiedDate overwrite...");
            
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
                
                Console.WriteLine("Audit data fixes applied successfully.\\n");
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
"""
    
    # Write to file
    with open(output_file_path, 'w', encoding='utf-8') as f:
        f.write(cs_code)


if __name__ == "__main__":
    # Define file paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    # CSV file is in the CSV subdirectory of the parent directory
    csv_file = os.path.join(script_dir, "..", "CSV", "Partners_Related_To_Level3 - Sheet1.csv")
    
    # Navigate to project root and then to the seeder file location
    # Script is at: tools/DataImport/Archives/v3-import-files/Python/
    # Need to go 5 levels up to reach project root
    project_root = os.path.abspath(os.path.join(script_dir, "..", "..", "..", "..", ".."))
    output_file = os.path.join(project_root, "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", "Partners_Related_To_Level3_Bulk_Update.cs")
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    # Process CSV and generate seeder
    process_csv_and_generate_seeder(csv_file, output_file)



