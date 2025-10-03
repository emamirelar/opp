#!/usr/bin/env python3
"""
Python script to generate ContactSeeder.cs from CSV data.
This script reads the Contacts CSV file and generates a C# seeder class
with individual contact objects, similar to PartnerSeeder.cs format.

File Structure:
- Script location: UNOPS.PAO.UNOPSDataAccess/Scripts/Python/
- CSV input: UNOPS.PAO.UNOPSDataAccess/Scripts/CSV/Contacts_20251002 - Sheet2.csv
- C# output: UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/ContactSeeder.cs
"""

import csv
import re
import os
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

def generate_contact_object(row: Dict[str, str], index: int) -> Optional[str]:
    """Generate a single UNOPSContact object from CSV row"""
    
    # Get account number for partner lookup first
    account_number = clean_field(row.get('Account.AccountNumber', ''))
    if not account_number or not account_number.isdigit():
        return None  # Skip if no valid account number
    
    # Handle multi-line descriptions by removing newlines
    description = clean_field(row.get('Description', ''))
    if description:
        description = re.sub(r'\n+', ' ', description)
    
    # Build required fields with defaults
    first_name = clean_field(row.get('FirstName', ''))
    last_name = clean_field(row.get('LastName', ''))
    title = clean_field(row.get('Title', ''))
    email = clean_field(row.get('Email', ''))
    
    if not last_name:
        last_name = "Unknown"
    if not title:
        title = "Contact"
    if not email:
        email = f"contact{row.get('Id', 'unknown')}@example.com"
    
    # Build the contact object
    contact_obj = f"""                new UNOPSContact
                {{
                    ContactNumber = {escape_csharp_string(row.get('Id', ''))},
                    Salutation = {escape_csharp_string(row.get('Salutation', ''))},
                    FirstName = {escape_csharp_string(first_name)},
                    MiddleName = {escape_csharp_string(row.get('MiddleName', ''))},
                    LastName = {escape_csharp_string(last_name)},
                    Suffix = {escape_csharp_string(row.get('Suffix', ''))},
                    Title = {escape_csharp_string(title)},
                    Department = {escape_csharp_string(row.get('Department', ''))},
                    Description = {escape_csharp_string(description)},
                    Email = {escape_csharp_string(email)},
                    Phone = {escape_csharp_string(row.get('Phone', ''))},
                    Mobile = {escape_csharp_string(row.get('MobilePhone', ''))},
                    Assistant = {escape_csharp_string(row.get('AssistantName', ''))},
                    AssistantPhone = {escape_csharp_string(row.get('AssistantPhone', ''))},
                    AssistantEmail = {escape_csharp_string(row.get('SF_PRM_AssistantEmail__c', ''))},
                    MailingStreet = {escape_csharp_string(row.get('MailingStreet', ''))},
                    MailingCity = {escape_csharp_string(row.get('MailingCity', ''))},
                    MailingStateProvince = {escape_csharp_string(row.get('MailingState', ''))},
                    MailingCountry = {escape_csharp_string(row.get('MailingCountry', ''))},
                    MailingPostalCode = {escape_csharp_string(row.get('MailingPostalCode', ''))},
                    PartnerId = partnerMapping[{account_number}],
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }},"""
    
    return contact_obj

def generate_contact_seeder(csv_file_path: str, output_file_path: str) -> None:
    """Generate the complete ContactSeeder.cs file"""
    
    contacts: List[str] = []
    skipped_count = 0
    
    # Read CSV and generate contact objects
    try:
        with open(csv_file_path, 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            
            for index, row in enumerate(reader, 1):
                contact_obj = generate_contact_object(row, index)
                if contact_obj is not None:
                    contacts.append(contact_obj)
                else:
                    skipped_count += 1
                
                # Progress indicator
                if index % 100 == 0:
                    print(f"Processed {index} contacts... (Skipped: {skipped_count})")
            
            # Remove comma from the last contact object
            if contacts:
                contacts[-1] = contacts[-1].rstrip(',')
    
    except FileNotFoundError:
        print(f"Error: CSV file not found at {csv_file_path}")
        return
    except Exception as e:
        print(f"Error reading CSV file: {e}")
        return
    
    # Generate the complete C# file
    csharp_content = f"""using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{{
    public static class ContactSeeder
    {{
        public static async Task SeedContactsAsync(UNOPSAppDbContext context)
        {{
            if (await context.Contacts.AnyAsync())
            {{
                return;
            }}

            // Create mapping from Partner ErpDimValue to PartnerId
            var partnerMapping = await context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .ToDictionaryAsync(p => p.ErpDimValue.Value, p => p.Id);

            var contacts = new List<UNOPSContact>
            {{
{chr(10).join(contacts)}
            }};

            await context.Contacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
        }}
    }}
}}"""
    
    # Write the generated file
    try:
        with open(output_file_path, 'w', encoding='utf-8') as file:
            file.write(csharp_content)
        print(f"Successfully generated ContactSeeder.cs with {len(contacts)} contacts")
        print(f"Skipped {skipped_count} contacts due to missing or invalid PartnerId")
        print(f"Output file: {output_file_path}")
    except Exception as e:
        print(f"Error writing output file: {e}")

def main():
    """Main function to run the generator"""
    
    # Get the directory where this script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # File paths (relative to script location)
    csv_file = os.path.join(script_dir, "..", "CSV", "Contacts_20251002 - Sheet2.csv")
    output_file = os.path.join(script_dir, "..", "..", "Seed", "Seeders", "ContactSeeder.cs")
    
    print("ContactSeeder Generator")
    print("=" * 50)
    print(f"Input CSV: {csv_file}")
    print(f"Output C#: {output_file}")
    print()
    
    # Check if CSV file exists
    if not os.path.exists(csv_file):
        print(f"Error: CSV file not found at {csv_file}")
        print("Please ensure the CSV file is in the correct location.")
        return
    
    # Generate the seeder
    generate_contact_seeder(csv_file, output_file)

if __name__ == "__main__":
    main()
