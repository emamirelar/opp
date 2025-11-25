import csv
import os
from datetime import datetime

def process_csv_and_generate_seeder(csv_file_path, output_cs_file_path):
    """
    Process the partner category/group CSV file and generate C# seeder code.
    Adds a '.' at the end of the Name field for dummy name testing.
    """
    
    partner_tree_records = []
    
    # Read CSV file
    with open(csv_file_path, 'r', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        
        for row in reader:
            # Process each level (1 through 6)
            for level_num in range(1, 7):
                level_code = row.get(f'Partner_Level{level_num}', '').strip()
                level_desc = row.get(f'Partner_Level{level_num}_Description', '').strip()
                level_desc_short = row.get(f'Partner_Level{level_num}_Description_Short', '').strip()
                
                # Skip if current level is empty
                if not level_code:
                    break
                
                # Determine if this is the last level (next level is empty)
                next_level_code = row.get(f'Partner_Level{level_num + 1}', '').strip() if level_num < 6 else ''
                is_last_level = not next_level_code
                
                # Determine parent (previous level code)
                parent_code = row.get(f'Partner_Level{level_num - 1}', '').strip() if level_num > 1 else ''
                
                # Create record dictionary with '.' added to Name
                record = {
                    'Code': level_code,
                    'Name': level_desc_short + '.',  # Add '.' to the end of Name
                    'Description': level_desc,
                    'Type': f'Level_{level_num}',
                    'Parent': parent_code if parent_code else 'null',
                    'PartnerCategoryCode': level_code if not is_last_level else 'null',
                    'PartnerGroupCode': level_code if is_last_level else 'null'
                }
                
                # Check if record already exists in our collection (deduplicate)
                if not any(r['Code'] == record['Code'] for r in partner_tree_records):
                    partner_tree_records.append(record)
    
    # Generate C# seeder file
    generate_csharp_seeder(partner_tree_records, output_cs_file_path)
    
    print(f"\nProcessing complete!")
    print(f"Total unique partner tree records to process: {len(partner_tree_records)}")
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
    public static class PartnerTreeSeeder_DummyName_v3
    {
        public static async Task SeedPartnerTreeDummyNameAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting PartnerTree DummyName seeding process (v3)...");
            
            int updatedCount = 0;
            int createdCount = 0;
            var updatedRecordIds = new List<int>();
            var createdRecordIds = new List<int>();
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
"""
    
    # Add seeding logic for each record
    for idx, record in enumerate(records):
        code = record['Code'].replace("'", "\\'")  # Escape single quotes
        name = record['Name'].replace("'", "\\'")  # Name already has '.' added
        description = record['Description'].replace("'", "\\'")
        type_value = record['Type']
        parent = f'"{record["Parent"].replace("'", "\\'")}\"' if record['Parent'] != 'null' else 'null'
        category_code = f'"{record["PartnerCategoryCode"].replace("'", "\\'")}\"' if record['PartnerCategoryCode'] != 'null' else 'null'
        group_code = f'"{record["PartnerGroupCode"].replace("'", "\\'")}\"' if record['PartnerGroupCode'] != 'null' else 'null'
        
        cs_code += f"""                // Record {idx + 1}: {code}
                {{
                    var existingRecord = await context.PartnerTrees
                        .FirstOrDefaultAsync(pt => pt.Code == "{code}");
                    
                    if (existingRecord != null)
                    {{
                        existingRecord.Name = "{name}";
                        existingRecord.Description = "{description}";
                        existingRecord.Type = "{type_value}";
                        existingRecord.Parent = {parent};
                        existingRecord.PartnerCategoryCode = {category_code};
                        existingRecord.PartnerGroupCode = {group_code};
                        existingRecord.Status = (EntityStatus)1;
                        existingRecord.LastModifiedBy = -1;
                        existingRecord.LastModifiedDate = DateTime.UtcNow;
                        existingRecord.IsDeleted = false;
                        
                        context.PartnerTrees.Update(existingRecord);
                        await context.SaveChangesAsync();
                        updatedRecordIds.Add(existingRecord.Id);
                        Console.WriteLine($"Updated: PartnerTree with Code '{code}' - {name}");
                        updatedCount++;
                    }}
                    else
                    {{
                        var newRecord = new UNOPSPartnerTree
                        {{
                            Code = "{code}",
                            Name = "{name}",
                            Description = "{description}",
                            Type = "{type_value}",
                            Parent = {parent},
                            PartnerCategoryCode = {category_code},
                            PartnerGroupCode = {group_code},
                            Status = (EntityStatus)1,
                            CreatedBy = -1,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = -1,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedBy = 0
                        }};
                        
                        context.PartnerTrees.Add(newRecord);
                        await context.SaveChangesAsync();
                        createdRecordIds.Add(newRecord.Id);
                        Console.WriteLine($"Created: PartnerTree with Code '{code}' - {name}");
                        createdCount++;
                    }}
                }}
                
"""
    
    # Add the audit data fix logic and closing code
    cs_code += """                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\\nPartnerTree DummyName seeding completed successfully.");
                Console.WriteLine($"Total records processed: {updatedCount + createdCount}");
                Console.WriteLine($"Records updated: {updatedCount}");
                Console.WriteLine($"Records created: {createdCount}");
                
                // Fix audit data for updated and newly created records
                // Note: SaveChangesAsync triggers audit interceptor which overwrites CreatedBy/LastModifiedBy
                // We need to fix these values after the transaction commits
                var allRecordIds = createdRecordIds.Concat(updatedRecordIds).ToList();
                if (allRecordIds.Count > 0)
                {
                    await FixAuditDataAsync(context, allRecordIds);
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during PartnerTree DummyName seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        private static async Task FixAuditDataAsync(UNOPSAppDbContext context, List<int> recordIds)
        {
            Console.WriteLine("\\nApplying audit data fixes to prevent LastModifiedBy overwrite...");
            
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Use ExecuteUpdateAsync to bypass audit interceptor
                // Update CreatedBy for newly created partner trees
                int createdByUpdates = await context.PartnerTrees
                    .Where(pt => recordIds.Contains(pt.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(pt => pt.CreatedBy, -1));
                
                Console.WriteLine($"Updated CreatedBy to -1 for {createdByUpdates} partner tree records");
                
                // Update LastModifiedBy for newly created partner trees
                int lastModifiedByUpdates = await context.PartnerTrees
                    .Where(pt => recordIds.Contains(pt.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(pt => pt.LastModifiedBy, -1));
                
                Console.WriteLine($"Updated LastModifiedBy to -1 for {lastModifiedByUpdates} partner tree records");
                
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
    csv_file = os.path.join(script_dir, "..", "CSV", "Partner_Category_Group_Import_File_v3 - Sheet4.csv")
    
    # Navigate to project root and then to the seeder file location
    # From Python subdirectory, go up 4 levels to reach project root
    project_root = os.path.abspath(os.path.join(script_dir, "..", "..", "..", ".."))
    output_file = os.path.join(project_root, "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", "PartnerTreeSeeder_DummyName_v3.cs")
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    # Process CSV and generate seeder
    process_csv_and_generate_seeder(csv_file, output_file)


