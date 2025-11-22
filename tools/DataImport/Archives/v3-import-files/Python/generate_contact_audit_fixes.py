#!/usr/bin/env python3
"""
Python script to generate Contact_Audit_Data_Fixes_v3.cs from CSV data.
This script reads the SF_Contacts_Audit_Data_Fixes CSV file and generates a C# seeder class
that updates CreatedBy, CreatedDate, and conditionally updates LastModifiedBy and LastModifiedDate.

File Structure:
- Script location: tools/DataImport/Archives/v3-import-files/
- CSV input: tools/DataImport/Archives/v3-import-files/SF_Contacts_Audit_Data_Fixes - Sheet1.csv
- C# output: UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/Contact_Audit_Data_Fixes_v3.cs
"""

import csv
import os
from datetime import datetime
from typing import Dict, List, Optional

def clean_field(field: str) -> Optional[str]:
    """Clean and format field values"""
    if not field or field.strip() == '':
        return None
    return field.strip()

def escape_csharp_string(value: str) -> str:
    """Escape special characters for C# string literals"""
    if not value:
        return 'null'
    
    # Escape backslashes, quotes, and newlines
    escaped = value.replace('\\', '\\\\')
    escaped = escaped.replace('"', '\\"')
    escaped = escaped.replace('\n', '\\n')
    escaped = escaped.replace('\r', '\\r')
    escaped = escaped.replace('\t', '\\t')
    
    return f'"{escaped}"'

def parse_salesforce_datetime(sf_datetime: str) -> Optional[str]:
    """Parse Salesforce datetime format and convert to C# DateTime.Parse format"""
    if not sf_datetime:
        return None
    
    try:
        # Salesforce format: 2025-04-22T13:00:23.000+0000
        # Remove the timezone offset and milliseconds for simpler parsing
        # Split on '+' or '-' for timezone
        if '+' in sf_datetime:
            dt_part = sf_datetime.split('+')[0]
        elif sf_datetime.count('-') > 2:  # Has negative timezone
            # Find last occurrence of '-'
            last_dash = sf_datetime.rfind('-')
            dt_part = sf_datetime[:last_dash]
        else:
            dt_part = sf_datetime
        
        # Parse the datetime
        dt = datetime.strptime(dt_part, '%Y-%m-%dT%H:%M:%S.%f')
        
        # Format for C# DateTime.Parse (ISO 8601)
        return dt.strftime('%Y-%m-%dT%H:%M:%S.%f')[:-3] + 'Z'
    except Exception as e:
        print(f"Warning: Could not parse datetime '{sf_datetime}': {e}")
        return None

def generate_contact_update(row: Dict[str, str]) -> Optional[Dict[str, str]]:
    """Generate contact audit update data from CSV row"""
    
    contact_id = clean_field(row.get('Id', ''))
    if not contact_id:
        return None
    
    created_by_email = clean_field(row.get('CreatedBy.Email', ''))
    created_date = clean_field(row.get('CreatedDate', ''))
    last_modified_by_email = clean_field(row.get('LastModifiedBy.Email', ''))
    last_modified_date = clean_field(row.get('LastModifiedDate', ''))
    
    # Parse datetimes
    created_date_parsed = parse_salesforce_datetime(created_date) if created_date else None
    last_modified_date_parsed = parse_salesforce_datetime(last_modified_date) if last_modified_date else None
    
    return {
        'contact_id': contact_id,
        'created_by_email': created_by_email if created_by_email else '',
        'created_date': created_date_parsed if created_date_parsed else '',
        'last_modified_by_email': last_modified_by_email if last_modified_by_email else '',
        'last_modified_date': last_modified_date_parsed if last_modified_date_parsed else ''
    }

def generate_contact_audit_seeder(csv_file_path: str, output_file_path: str) -> None:
    """Generate the complete Contact_Audit_Data_Fixes_v3.cs file"""
    
    contact_updates: List[Dict[str, str]] = []
    skipped_count = 0
    
    # Read CSV and generate contact update objects
    try:
        with open(csv_file_path, 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            
            for index, row in enumerate(reader, 1):
                contact_data = generate_contact_update(row)
                if contact_data is not None:
                    contact_updates.append(contact_data)
                else:
                    skipped_count += 1
                
                # Progress indicator
                if index % 100 == 0:
                    print(f"Processed {index} contacts... (Skipped: {skipped_count})")
    
    except FileNotFoundError:
        print(f"Error: CSV file not found at {csv_file_path}")
        return
    except Exception as e:
        print(f"Error reading CSV file: {e}")
        return
    
    # Generate update entries for C# code
    update_entries = []
    for contact in contact_updates:
        update_entry = f"""                new ContactAuditUpdate
                {{
                    ContactId = {escape_csharp_string(contact['contact_id'])},
                    CreatedByEmail = {escape_csharp_string(contact['created_by_email'])},
                    CreatedDate = {escape_csharp_string(contact['created_date'])},
                    LastModifiedByEmail = {escape_csharp_string(contact['last_modified_by_email'])},
                    LastModifiedDate = {escape_csharp_string(contact['last_modified_date'])}
                }},"""
        update_entries.append(update_entry)
    
    # Remove comma from the last entry
    if update_entries:
        update_entries[-1] = update_entries[-1].rstrip(',')
    
    # Generate the complete C# file
    csharp_content = f"""using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{{
    public static class Contact_Audit_Data_Fixes_v3
    {{
        private class ContactAuditUpdate
        {{
            public string ContactId {{ get; set; }} = string.Empty;
            public string CreatedByEmail {{ get; set; }} = string.Empty;
            public string CreatedDate {{ get; set; }} = string.Empty;
            public string LastModifiedByEmail {{ get; set; }} = string.Empty;
            public string LastModifiedDate {{ get; set; }} = string.Empty;
        }}

        public static async Task UpdateContactAuditDataAsync(UNOPSAppDbContext context)
        {{
            // Create mapping from PAOUser Email to Id (handle duplicates by taking first, filter out null emails)
            var paoUsers = await context.PAOUsers
                .Select(u => new {{ u.Id, u.Email }})
                .ToListAsync();
            var paoUserEmailMapping = paoUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .GroupBy(u => u.Email.ToLower())
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Define contact audit updates
            var contactAuditUpdates = new List<ContactAuditUpdate>
            {{
{chr(10).join(update_entries)}
            }};

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {{
                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var updateData in contactAuditUpdates)
                {{
                    // Find contact by ContactNumber (which corresponds to Salesforce Id)
                    var contact = await context.Contacts
                        .FirstOrDefaultAsync(c => c.ContactNumber == updateData.ContactId);

                    if (contact == null)
                    {{
                        Console.WriteLine($"Warning: Contact with ContactNumber {{updateData.ContactId}} not found in database");
                        skippedCount++;
                        continue;
                    }}

                    bool updated = false;

                    // Update CreatedBy
                    if (!string.IsNullOrEmpty(updateData.CreatedByEmail))
                    {{
                        var createdByEmail = updateData.CreatedByEmail.ToLower();
                        contact.CreatedBy = paoUserEmailMapping.ContainsKey(createdByEmail) 
                            ? paoUserEmailMapping[createdByEmail] 
                            : -1; // Opportunity+ system user if not found
                        updated = true;
                    }}

                    // Update CreatedDate
                    if (!string.IsNullOrEmpty(updateData.CreatedDate))
                    {{
                        if (DateTime.TryParse(updateData.CreatedDate, out DateTime parsedCreatedDate))
                        {{
                            contact.CreatedDate = parsedCreatedDate;
                            updated = true;
                        }}
                        else
                        {{
                            Console.WriteLine($"Warning: Could not parse CreatedDate '{{updateData.CreatedDate}}' for Contact {{updateData.ContactId}}");
                        }}
                    }}

                    // Only update LastModifiedBy and LastModifiedDate if LastModifiedBy is currently 0
                    if (contact.LastModifiedBy == 0)
                    {{
                        // Update LastModifiedBy
                        if (!string.IsNullOrEmpty(updateData.LastModifiedByEmail))
                        {{
                            var lastModifiedByEmail = updateData.LastModifiedByEmail.ToLower();
                            contact.LastModifiedBy = paoUserEmailMapping.ContainsKey(lastModifiedByEmail) 
                                ? paoUserEmailMapping[lastModifiedByEmail] 
                                : -1; // Opportunity+ system user if not found
                            updated = true;
                        }}

                        // Update LastModifiedDate
                        if (!string.IsNullOrEmpty(updateData.LastModifiedDate))
                        {{
                            if (DateTime.TryParse(updateData.LastModifiedDate, out DateTime parsedLastModifiedDate))
                            {{
                                contact.LastModifiedDate = parsedLastModifiedDate;
                                updated = true;
                            }}
                            else
                            {{
                                Console.WriteLine($"Warning: Could not parse LastModifiedDate '{{updateData.LastModifiedDate}}' for Contact {{updateData.ContactId}}");
                            }}
                        }}
                    }}
                    else
                    {{
                        Console.WriteLine($"Skipped LastModified updates for Contact {{updateData.ContactId}} - LastModifiedBy already set ({{contact.LastModifiedBy}})");
                    }}

                    if (updated)
                    {{
                        updatedCount++;
                        Console.WriteLine($"Updated audit data for Contact {{updateData.ContactId}} - '{{contact.Name}}'");
                    }}
                }}

                // Save all changes at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"Contact audit data updates completed successfully.");
                Console.WriteLine($"Total contacts updated: {{updatedCount}}");
                Console.WriteLine($"Total contacts skipped: {{skippedCount}}");
            }}
            catch (Exception ex)
            {{
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating Contact audit data: {{ex.Message}}");
                throw;
            }}
        }}
    }}
}}"""
    
    # Write the generated file
    try:
        # Ensure output directory exists
        os.makedirs(os.path.dirname(output_file_path), exist_ok=True)
        
        with open(output_file_path, 'w', encoding='utf-8') as file:
            file.write(csharp_content)
        print(f"Successfully generated Contact_Audit_Data_Fixes_v3.cs with {len(contact_updates)} contact updates")
        print(f"Skipped {skipped_count} rows due to missing or invalid data")
        print(f"Output file: {output_file_path}")
    except Exception as e:
        print(f"Error writing output file: {e}")

def main():
    """Main function to run the generator"""
    
    # Get the directory where this script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # File paths (relative to script location)
    csv_file = os.path.join(script_dir, "SF_Contacts_Audit_Data_Fixes - Sheet1.csv")
    output_file = os.path.join(script_dir, "..", "..", "..", "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", "Contact_Audit_Data_Fixes_v3.cs")
    
    print("Contact Audit Data Fixes Seeder Generator")
    print("=" * 60)
    print(f"Input CSV: {csv_file}")
    print(f"Output C#: {output_file}")
    print()
    
    # Check if CSV file exists
    if not os.path.exists(csv_file):
        print(f"Error: CSV file not found at {csv_file}")
        print("Please ensure the CSV file is in the correct location.")
        return
    
    # Generate the seeder
    generate_contact_audit_seeder(csv_file, output_file)

if __name__ == "__main__":
    main()

