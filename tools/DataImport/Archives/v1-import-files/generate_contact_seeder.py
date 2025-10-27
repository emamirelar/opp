#!/usr/bin/env python3
"""
Python script to generate ContactSeeder.cs from CSV data.
This script reads the Contacts CSV file and generates a C# seeder class
with individual contact objects, similar to PartnerSeeder.cs format.

The generated seeder includes logic to check for existing contacts in the database
by matching the ContactNumber field with the Id from the CSV file. If found, it updates
the existing contact; otherwise, it creates a new one.

File Structure:
- Script location: tools/DataImport/Archives/v1-import-files/
- CSV input: tools/DataImport/Archives/v1-import-files/Contacts_SF_Export_20251008 - Sheet1.csv
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

def generate_contact_object(row: Dict[str, str], index: int) -> Optional[Dict[str, str]]:
    """Generate contact data from CSV row"""
    
    # Get account number and account name for partner lookup
    account_number = clean_field(row.get('Account.AccountNumber', ''))
    account_name = clean_field(row.get('Account.Name', ''))
    
    # We need at least account number OR account name to look up a partner
    if (not account_number or not account_number.isdigit()) and not account_name:
        return None  # Skip if no valid account identifier
    
    # Handle multi-line descriptions by removing newlines
    description = clean_field(row.get('Description', ''))
    if description:
        description = re.sub(r'\n+', ' ', description)
    
    # Build required fields with defaults
    first_name = clean_field(row.get('FirstName', ''))
    middle_name = clean_field(row.get('MiddleName', ''))
    last_name = clean_field(row.get('LastName', ''))
    title = clean_field(row.get('Title', ''))
    email = clean_field(row.get('Email', ''))
    
    if not last_name:
        last_name = "Unknown"
    if not title:
        title = "Contact"
    if not email:
        email = f"contact{row.get('Id', 'unknown')}@example.com"
    
    # Build Name from FirstName, MiddleName, and LastName
    name_parts = []
    if first_name:
        name_parts.append(first_name)
    if middle_name:
        name_parts.append(middle_name)
    name_parts.append(last_name)  # LastName is always included (has default)
    full_name = ' '.join(name_parts)
    
    # Get CreatedBy email and Owner Department
    created_by_email = clean_field(row.get('CreatedBy.Email', ''))
    owner_department = clean_field(row.get('Owner.Department', ''))
    
    # Return dictionary with contact data
    return {
        'contact_number': row.get('Id', ''),
        'name': full_name,
        'salutation': row.get('Salutation', ''),
        'first_name': first_name if first_name else '',
        'middle_name': middle_name if middle_name else '',
        'last_name': last_name,
        'suffix': row.get('Suffix', ''),
        'title': title,
        'department': row.get('Department', ''),
        'description': description,
        'email': email,
        'phone': row.get('Phone', ''),
        'mobile': row.get('MobilePhone', ''),
        'assistant': row.get('AssistantName', ''),
        'assistant_phone': row.get('AssistantPhone', ''),
        'assistant_email': row.get('SF_PRM_AssistantEmail__c', ''),
        'mailing_street': row.get('MailingStreet', ''),
        'mailing_city': row.get('MailingCity', ''),
        'mailing_state': row.get('MailingState', ''),
        'mailing_country': row.get('MailingCountry', ''),
        'mailing_postal_code': row.get('MailingPostalCode', ''),
        'account_number': account_number if account_number else '',
        'account_name': account_name if account_name else '',
        'created_by_email': created_by_email if created_by_email else '',
        'owner_department': owner_department if owner_department else ''
    }

def generate_contact_seeder(csv_file_path: str, output_file_path: str) -> None:
    """Generate the complete ContactSeeder.cs file"""
    
    contacts: List[Dict[str, str]] = []
    skipped_count = 0
    
    # Read CSV and generate contact objects
    try:
        with open(csv_file_path, 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            
            for index, row in enumerate(reader, 1):
                contact_data = generate_contact_object(row, index)
                if contact_data is not None:
                    contacts.append(contact_data)
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
    
    # Generate contact tuples for C# code
    contact_tuples = []
    for contact in contacts:
        # Build partner lookup logic using -1 as sentinel value for "not found"
        if contact['account_number']:
            # Has account number - try ErpDimValue first, then fallback to Name
            partner_lookup = f"partnerErpMapping.ContainsKey({contact['account_number']}) ? partnerErpMapping[{contact['account_number']}] : (partnerNameMapping.ContainsKey({escape_csharp_string(contact['account_name'])}) ? partnerNameMapping[{escape_csharp_string(contact['account_name'])}] : -1)"
        else:
            # No account number - only use Name
            partner_lookup = f"partnerNameMapping.ContainsKey({escape_csharp_string(contact['account_name'])}) ? partnerNameMapping[{escape_csharp_string(contact['account_name'])}] : -1"
        
        contact_tuple = f"""                new ({escape_csharp_string(contact['contact_number'])}, new UNOPSContact
                {{
                    Name = {escape_csharp_string(contact['name'])},
                    ContactNumber = {escape_csharp_string(contact['contact_number'])},
                    Salutation = {escape_csharp_string(contact['salutation'])},
                    FirstName = {escape_csharp_string(contact['first_name'])},
                    MiddleName = {escape_csharp_string(contact['middle_name'])},
                    LastName = {escape_csharp_string(contact['last_name'])},
                    Suffix = {escape_csharp_string(contact['suffix'])},
                    Title = {escape_csharp_string(contact['title'])},
                    Department = {escape_csharp_string(contact['department'])},
                    Description = {escape_csharp_string(contact['description'])},
                    Email = {escape_csharp_string(contact['email'])},
                    Phone = {escape_csharp_string(contact['phone'])},
                    Mobile = {escape_csharp_string(contact['mobile'])},
                    Assistant = {escape_csharp_string(contact['assistant'])},
                    AssistantPhone = {escape_csharp_string(contact['assistant_phone'])},
                    AssistantEmail = {escape_csharp_string(contact['assistant_email'])},
                    MailingStreet = {escape_csharp_string(contact['mailing_street'])},
                    MailingCity = {escape_csharp_string(contact['mailing_city'])},
                    MailingStateProvince = {escape_csharp_string(contact['mailing_state'])},
                    MailingCountry = {escape_csharp_string(contact['mailing_country'])},
                    MailingPostalCode = {escape_csharp_string(contact['mailing_postal_code'])},
                    PartnerId = {partner_lookup},
                    Status = EntityStatus.Active,
                    CreatedBy = paoUserEmailMapping.ContainsKey({escape_csharp_string(contact['created_by_email'])}.ToLower()) ? paoUserEmailMapping[{escape_csharp_string(contact['created_by_email'])}.ToLower()] : 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }}, {escape_csharp_string(contact['owner_department'])}, {escape_csharp_string(contact['account_number']) if contact['account_number'] else 'null'}, {escape_csharp_string(contact['account_name'])}),"""
        contact_tuples.append(contact_tuple)
    
    # Remove comma from the last contact tuple
    if contact_tuples:
        contact_tuples[-1] = contact_tuples[-1].rstrip(',')
    
    # Generate the complete C# file
    csharp_content = f"""using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            // Create mapping from Partner ErpDimValue to PartnerId (handle duplicates by taking first)
            var partnersWithErp = await context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .ToListAsync();
            var partnerErpMapping = partnersWithErp
                .GroupBy(p => p.ErpDimValue.Value)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Create mapping from Partner Name to PartnerId (handle duplicates by taking first)
            var partnersWithName = await context.Partners
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .ToListAsync();
            var partnerNameMapping = partnersWithName
                .GroupBy(p => p.Name)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Create mapping from PAOUser Email to Id (handle duplicates by taking first, filter out null emails)
            var paoUsers = await context.PAOUsers
                .Select(u => new {{ u.Id, u.Email }})
                .ToListAsync();
            var paoUserEmailMapping = paoUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .GroupBy(u => u.Email.ToLower())
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Create mapping from OrganizationHierarchy Description to Id (handle duplicates by taking first, filter out null descriptions)
            var orgUnits = await context.OrganizationHierarchies
                .Where(oh => oh.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                .ToListAsync();
            var orgUnitMapping = orgUnits
                .Where(oh => !string.IsNullOrEmpty(oh.Description))
                .GroupBy(oh => oh.Description)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Process contacts
            var contactsToProcess = new List<(string ContactNumber, UNOPSContact Contact, string? OrgUnit, string? AccountNumber, string? AccountName)>
            {{
{chr(10).join(contact_tuples)}
            }};

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {{
                // Step 1: Process all contacts (create or update), skipping those without valid PartnerId
                foreach (var (contactNumber, contactData, _, accountNumber, accountName) in contactsToProcess)
                {{
                    // Skip if PartnerId is -1 (partner not found by either ErpDimValue or Name)
                    if (contactData.PartnerId == -1)
                    {{
                        Console.WriteLine($"Skipping contact {{contactNumber}} - Partner not found (AccountNumber: {{accountNumber ?? "null"}}, AccountName: {{accountName ?? "null"}})");
                        continue;
                    }}

                    // Check if contact already exists based on ContactNumber
                    var existingContact = await context.Contacts
                        .FirstOrDefaultAsync(c => c.ContactNumber == contactNumber);

                    if (existingContact != null)
                    {{
                        // Update existing contact
                        existingContact.Name = contactData.Name;
                        existingContact.Salutation = contactData.Salutation;
                        existingContact.FirstName = contactData.FirstName;
                        existingContact.MiddleName = contactData.MiddleName;
                        existingContact.LastName = contactData.LastName;
                        existingContact.Suffix = contactData.Suffix;
                        existingContact.Title = contactData.Title;
                        existingContact.Department = contactData.Department;
                        existingContact.Description = contactData.Description;
                        existingContact.Email = contactData.Email;
                        existingContact.Phone = contactData.Phone;
                        existingContact.Mobile = contactData.Mobile;
                        existingContact.Assistant = contactData.Assistant;
                        existingContact.AssistantPhone = contactData.AssistantPhone;
                        existingContact.AssistantEmail = contactData.AssistantEmail;
                        existingContact.MailingStreet = contactData.MailingStreet;
                        existingContact.MailingCity = contactData.MailingCity;
                        existingContact.MailingStateProvince = contactData.MailingStateProvince;
                        existingContact.MailingCountry = contactData.MailingCountry;
                        existingContact.MailingPostalCode = contactData.MailingPostalCode;
                        existingContact.PartnerId = contactData.PartnerId;
                        existingContact.Status = contactData.Status;
                        existingContact.LastModifiedBy = 0;
                        existingContact.LastModifiedDate = DateTime.UtcNow;
                    }}
                    else
                    {{
                        // Add new contact to context
                        context.Contacts.Add(contactData);
                    }}
                }}

                // Save all contacts at once
                await context.SaveChangesAsync();

                // Step 2: Process all organization unit relationships in batch
                foreach (var (contactNumber, contactData, orgUnit, _, _) in contactsToProcess)
                {{
                    // Skip if contact doesn't have a valid PartnerId (was skipped in step 1)
                    if (contactData.PartnerId == -1)
                        continue;

                    // Skip if no org unit specified
                    if (string.IsNullOrWhiteSpace(orgUnit) || !orgUnitMapping.ContainsKey(orgUnit))
                        continue;

                    // Get the contact (now guaranteed to exist with an ID)
                    var contact = await context.Contacts
                        .FirstOrDefaultAsync(c => c.ContactNumber == contactNumber);

                    if (contact == null)
                        continue;

                    var orgHierarchyId = orgUnitMapping[orgUnit];

                    // Check if relationship already exists
                    var existingRelationship = await context.OrganizationUnitRelationships
                        .FirstOrDefaultAsync(r => r.EntityType == nameof(Contact) && 
                                                  r.EntityId == contact.Id && 
                                                  r.OrganizationHierarchyId == orgHierarchyId);

                    if (existingRelationship == null)
                    {{
                        // Create new relationship
                        var newRelationship = new OrganizationUnitRelationship
                        {{
                            OrganizationHierarchyId = orgHierarchyId,
                            EntityId = contact.Id,
                            EntityType = nameof(Contact),
                            Name = $"Contact-{{contact.Id}}-{{orgHierarchyId}}",
                            Status = EntityStatus.Active,
                            CreatedBy = 0,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = 0,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false
                        }};
                        context.OrganizationUnitRelationships.Add(newRelationship);
                    }}
                }}

                // Save all organization unit relationships at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"Successfully seeded contacts");
            }}
            catch (Exception ex)
            {{
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error seeding contacts: {{ex.Message}}");
                throw;
            }}
        }}
    }}
}}"""
    
    # Write the generated file
    try:
        with open(output_file_path, 'w', encoding='utf-8') as file:
            file.write(csharp_content)
        print(f"Successfully generated ContactSeeder.cs with {len(contacts)} contacts")
        print(f"Skipped {skipped_count} contacts due to missing or invalid PartnerId")
        print(f"Note: The seeder will check for existing contacts by ContactNumber and update or create as needed")
        print(f"Output file: {output_file_path}")
    except Exception as e:
        print(f"Error writing output file: {e}")

def main():
    """Main function to run the generator"""
    
    # Get the directory where this script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # File paths (relative to script location)
    csv_file = os.path.join(script_dir, "Contacts_SF_Export_20251008 - Sheet1.csv")
    output_file = os.path.join(script_dir, "..", "..", "..", "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", "ContactSeeder.cs")
    
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
