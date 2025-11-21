import csv
import os
from datetime import datetime

def process_csv_and_generate_seeder(csv_file_path, output_cs_file_path):
    """
    Process the partner category/group CSV file and generate C# seeder code
    to update existing Partners with their PartnerCategory and PartnerGroup references.
    """
    
    partner_updates = []
    
    # Read CSV file
    with open(csv_file_path, 'r', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        
        for row in reader:
            partner_erp_dim_value = row.get('Partner', '').strip()
            partner_category = row.get('Partner_Category', '').strip()
            partner_group_short = row.get('Partner Group Short', '').strip()
            
            # Skip if no partner identifier
            if not partner_erp_dim_value:
                continue
            
            # Create update record dictionary
            update_record = {
                'ErpDimValue': partner_erp_dim_value,
                'PartnerCategory': partner_category if partner_category else 'null',
                'PartnerGroupShort': partner_group_short if partner_group_short else 'null'
            }
            
            # Check if we already have this partner (deduplicate)
            if not any(r['ErpDimValue'] == update_record['ErpDimValue'] for r in partner_updates):
                partner_updates.append(update_record)
    
    # Generate C# seeder file
    generate_csharp_seeder(partner_updates, output_cs_file_path)
    
    print(f"\nProcessing complete!")
    print(f"Total unique partner update records to process: {len(partner_updates)}")
    print(f"C# seeder file generated: {output_cs_file_path}")


def generate_csharp_seeder(records, output_file_path):
    """
    Generate the C# seeder file from the processed records.
    """
    
    # C# code template
    cs_code = """using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Partner_Update_With_CategoryGroup_Seeder_v3
    {
        public static async Task UpdatePartnersWithCategoryGroupAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting Partner Category/Group update process (v3)...");
            
            int skippedCount = 0;
            int categoryUpdatedCount = 0;
            int groupUpdatedCount = 0;
            int notFoundCount = 0;
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
"""
    
    # Add update logic for each record
    for idx, record in enumerate(records):
        erp_dim_value = record['ErpDimValue'].replace("'", "\\'")
        partner_category = record['PartnerCategory'] if record['PartnerCategory'] != 'null' else None
        partner_group_short = record['PartnerGroupShort'] if record['PartnerGroupShort'] != 'null' else None
        
        # Build category update code
        category_code_block = ""
        if partner_category:
            category_code_block = f"""
                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {{
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "{partner_category}");
                            
                            if (categoryTree != null)
                            {{
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }}
                            else
                            {{
                                Console.WriteLine($"Warning: PartnerTree with Code '{partner_category}' not found for Partner ErpDimValue '{erp_dim_value}'");
                            }}
                        }}
"""
        
        # Build group update code
        group_code_block = ""
        if partner_group_short:
            group_code_block = f"""
                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {{
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "{partner_group_short}");
                            
                            if (groupTree != null)
                            {{
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }}
                            else
                            {{
                                Console.WriteLine($"Warning: PartnerTree with Code '{partner_group_short}' not found for Partner ErpDimValue '{erp_dim_value}'");
                            }}
                        }}
"""
        
        # Build logging code
        category_log = ""
        if partner_category:
            category_log = f"""
                            if (categoryUpdated) 
                            {{
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '{erp_dim_value}' - PartnerCategoryId set to PartnerTree Code '{partner_category}'");
                            }}"""
        
        group_log = ""
        if partner_group_short:
            group_log = f"""
                            if (groupUpdated)
                            {{
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '{erp_dim_value}' - PartnerGroupId set to PartnerTree Code '{partner_group_short}'");
                            }}"""
        
        cs_code += f"""                // Record {idx + 1}: Partner ErpDimValue={erp_dim_value}
                {{
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == {erp_dim_value});
                    
                    if (partner == null)
                    {{
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '{erp_dim_value}' does not exist.");
                        notFoundCount++;
                    }}
                    else
                    {{
                        bool categoryUpdated = false;
                        bool groupUpdated = false;
{category_code_block}{group_code_block}                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {{
                            await context.SaveChangesAsync();
{category_log}{group_log}
                        }}
                        else
                        {{
                            Console.WriteLine($"Skipped: Partner '{erp_dim_value}' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }}
                    }}
                }}
                
"""
    
    # Add closing code
    cs_code += """                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\\nPartner Category/Group update completed successfully.");
                Console.WriteLine($"Total partners processed: {notFoundCount + skippedCount + Math.Max(categoryUpdatedCount, groupUpdatedCount)}");
                Console.WriteLine($"Partners not found: {notFoundCount}");
                Console.WriteLine($"Partners skipped (already populated): {skippedCount}");
                Console.WriteLine($"Partners with PartnerCategoryId updated: {categoryUpdatedCount}");
                Console.WriteLine($"Partners with PartnerGroupId updated: {groupUpdatedCount}");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during Partner Category/Group update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
    csv_file = os.path.join(script_dir, "Partner_Category_Group_Import_File_v3 - Sheet4.csv")
    output_file = os.path.join(script_dir, "..", "..", "SeederFiles", "Partner_Update_With_CategoryGroup_Seeder_v3.cs")
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    # Process CSV and generate seeder
    process_csv_and_generate_seeder(csv_file, output_file)

