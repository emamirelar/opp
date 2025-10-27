#!/usr/bin/env python3
"""
Python script to generate PartnerTreeSeeder_v2.cs from CSV data.
This script reads the Partner tree export CSV file and generates a C# seeder class
with individual partner tree objects.

The script processes the hierarchical partner tree structure and creates entries
for each level, avoiding duplicates and correctly setting parent relationships,
PartnerGroupCode, and PartnerCategoryCode.

File Structure:
- Script location: tools/DataImport/Archives/v1-import-files
- CSV input: tools/DataImport/Archives/v1-import-files/Partner tree export 02-Oct-2025 - Sheet1.csv
- C# output: UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/PartnerTreeSeeder_v2.cs
"""

import csv
import re
import os
from typing import Dict, List, Optional, Set

def clean_field(field: str) -> Optional[str]:
    """Clean and format field values"""
    if not field or field.strip() == '':
        return None
    return field.strip()

def escape_csharp_string(value: str) -> str:
    """Escape special characters for C# string literals"""
    if not value:
        return '""'
    
    # Escape backslashes, quotes, and newlines
    escaped = value.replace('\\', '\\\\')
    escaped = escaped.replace('"', '\\"')
    escaped = escaped.replace('\n', '\\n')
    escaped = escaped.replace('\r', '\\r')
    escaped = escaped.replace('\t', '\\t')
    
    return f'"{escaped}"'

def generate_code_from_description(description_short: str) -> str:
    """Generate code from description short by removing spaces and capitalizing"""
    if not description_short:
        return ""
    # Remove spaces and convert to uppercase
    code = description_short.replace(' ', '_').replace('-', '_').upper()
    # Remove any special characters except underscores
    code = re.sub(r'[^A-Z0-9_]', '', code)
    return code

def is_number(value: str) -> bool:
    """Check if value is a number"""
    if not value:
        return False
    return value.strip().isdigit()

def process_csv_to_partner_trees(csv_file_path: str) -> List[Dict[str, str]]:
    """Process CSV file and extract unique partner tree entries"""
    
    # Dictionary to store unique partner tree entries (key = Code)
    partner_trees: Dict[str, Dict[str, str]] = {}
    
    with open(csv_file_path, 'r', encoding='utf-8-sig') as csvfile:
        reader = csv.DictReader(csvfile)
        
        for row in reader:
            # Process each level (1-6)
            for level in range(1, 7):
                level_col = f"Partner_Level{level}"
                desc_col = f"Partner_Level{level}_Description"
                desc_short_col = f"Partner_Level{level}_Description_Short"
                
                level_value = clean_field(row.get(level_col, ''))
                desc_value = clean_field(row.get(desc_col, ''))
                desc_short_value = clean_field(row.get(desc_short_col, ''))
                
                # Skip if this level is a number
                if is_number(level_value):
                    continue
                
                # Skip if no description short (means this level is empty)
                if not desc_short_value:
                    continue
                
                # Generate code
                if level_value:
                    code = level_value
                else:
                    code = generate_code_from_description(desc_short_value)
                
                # Skip if code is empty or already processed
                if not code or code in partner_trees:
                    continue
                
                # Determine parent (previous level's code)
                parent_code = ""
                if level > 1:
                    for parent_level in range(level - 1, 0, -1):
                        parent_level_col = f"Partner_Level{parent_level}"
                        parent_desc_short_col = f"Partner_Level{parent_level}_Description_Short"
                        parent_level_value = clean_field(row.get(parent_level_col, ''))
                        parent_desc_short_value = clean_field(row.get(parent_desc_short_col, ''))
                        
                        if is_number(parent_level_value):
                            continue
                        
                        if parent_desc_short_value:
                            if parent_level_value:
                                parent_code = parent_level_value
                            else:
                                parent_code = generate_code_from_description(parent_desc_short_value)
                            break
                
                # Determine PartnerGroupCode and PartnerCategoryCode
                partner_group_code = None
                partner_category_code = None
                
                # Check next level
                if level < 6:
                    next_level_col = f"Partner_Level{level + 1}"
                    next_desc_short_col = f"Partner_Level{level + 1}_Description_Short"
                    next_level_value = clean_field(row.get(next_level_col, ''))
                    next_desc_short_value = clean_field(row.get(next_desc_short_col, ''))
                    
                    if is_number(next_level_value):
                        # Next level is a number, set PartnerGroupCode
                        partner_group_code = code
                    elif next_level_value or (not next_level_value and next_desc_short_value):
                        # Next level is a string OR (empty but has description short)
                        partner_category_code = code
                else:
                    # Level 6 has no next level, check if it has a partner number
                    partner_col = row.get('Partner', '')
                    if is_number(partner_col):
                        partner_group_code = code
                
                # Create partner tree entry
                partner_trees[code] = {
                    'Code': code,
                    'Name': desc_short_value,
                    'Description': desc_value if desc_value else desc_short_value,
                    'Type': f"Level_{level}",
                    'Parent': parent_code,
                    'PartnerGroupCode': partner_group_code,
                    'PartnerCategoryCode': partner_category_code
                }
    
    # Convert dictionary to list
    return list(partner_trees.values())

def generate_csharp_seeder(partner_trees: List[Dict[str, str]], output_file_path: str):
    """Generate C# seeder file from partner tree data"""
    
    # Sort by Type then Code for better organization
    partner_trees_sorted = sorted(partner_trees, key=lambda x: (x['Type'], x['Code']))
    
    # Generate C# objects
    csharp_objects = []
    for tree in partner_trees_sorted:
        partner_group_code_line = f"                    PartnerGroupCode = {escape_csharp_string(tree['PartnerGroupCode'])}," if tree['PartnerGroupCode'] else "                    PartnerGroupCode = null,"
        partner_category_code_line = f"                    PartnerCategoryCode = {escape_csharp_string(tree['PartnerCategoryCode'])}," if tree['PartnerCategoryCode'] else "                    PartnerCategoryCode = null,"
        parent_line = f"                    Parent = {escape_csharp_string(tree['Parent'])}," if tree['Parent'] else "                    Parent = null,"
        
        obj = f"""                new UNOPSPartnerTree
                {{
                    Code = {escape_csharp_string(tree['Code'])},
                    Name = {escape_csharp_string(tree['Name'])},
                    Description = {escape_csharp_string(tree['Description'])},
                    Type = {escape_csharp_string(tree['Type'])},
{parent_line}
{partner_group_code_line}
{partner_category_code_line}
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }}"""
        csharp_objects.append(obj)
    
    # Join objects with commas
    objects_string = ',\n'.join(csharp_objects)
    
    # Generate full C# file content
    csharp_content = f"""using Microsoft.EntityFrameworkCore;
using System.Linq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{{
    public static class PartnerTreeSeeder_v2
    {{
        public static async Task SeedPartnerTreesAsync(UNOPSAppDbContext context)
        {{
            // Get existing partner trees from database
            var existingPartnerTrees = await context.PartnerTrees
                .Where(pt => !string.IsNullOrEmpty(pt.Code))
                .ToDictionaryAsync(pt => pt.Code, pt => pt);

            var partnerTrees = new List<UNOPSPartnerTree>
            {{
{objects_string}
            }};

            var newPartnerTrees = new List<UNOPSPartnerTree>();
            var updatedCount = 0;

            foreach (var partnerTree in partnerTrees)
            {{
                if (existingPartnerTrees.TryGetValue(partnerTree.Code, out var existingPartnerTree))
                {{
                    // Update existing record
                    existingPartnerTree.Name = partnerTree.Name;
                    existingPartnerTree.Description = partnerTree.Description;
                    existingPartnerTree.Type = partnerTree.Type;
                    existingPartnerTree.Parent = partnerTree.Parent;
                    existingPartnerTree.PartnerGroupCode = partnerTree.PartnerGroupCode;
                    existingPartnerTree.PartnerCategoryCode = partnerTree.PartnerCategoryCode;
                    existingPartnerTree.LastModifiedBy = 0;
                    existingPartnerTree.LastModifiedDate = DateTime.UtcNow;
                    updatedCount++;
                }}
                else
                {{
                    // Add new record
                    newPartnerTrees.Add(partnerTree);
                }}
            }}

            if (newPartnerTrees.Any())
            {{
                await context.PartnerTrees.AddRangeAsync(newPartnerTrees);
            }}

            if (newPartnerTrees.Any() || updatedCount > 0)
            {{
                await context.SaveChangesAsync();
            }}
        }}
    }}
}}
"""
    
    # Write to output file
    with open(output_file_path, 'w', encoding='utf-8') as f:
        f.write(csharp_content)
    
    print(f"Generated {len(partner_trees_sorted)} partner tree entries")
    print(f"Output written to: {output_file_path}")

def main():
    """Main execution function"""
    # Get script directory
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # Define file paths
    csv_file = os.path.join(script_dir, './Partner tree export 02-Oct-2025 - Sheet1.csv')
    output_file = os.path.join(script_dir, '../../../UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/PartnerTreeSeeder_v2.cs')
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    print("Processing CSV file...")
    partner_trees = process_csv_to_partner_trees(csv_file)
    
    print(f"Found {len(partner_trees)} unique partner tree entries")
    
    print("Generating C# seeder file...")
    generate_csharp_seeder(partner_trees, output_file)
    
    print("Done!")

if __name__ == "__main__":
    main()

