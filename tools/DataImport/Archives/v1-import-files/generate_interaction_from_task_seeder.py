#!/usr/bin/env python3
"""
Python script to generate InteractionFromTaskSeeder.cs from Tasks CSV data.
This script reads the Tasks CSV file and generates a C# seeder class
that combines duplicate interactions and creates relationship records.

File Structure:
- Script location: tools/DataImport/Archives/v1-import-files
- CSV input: tools/DataImport/Archives/v1-import-files/Tasks_20251009 - Sheet1.csv
- C# output: UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/InteractionFromTaskSeeder.cs
"""

import csv
import re
import os
from typing import Dict, List, Optional, Set, Tuple
from collections import defaultdict

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

def extract_email_addresses(text: str) -> List[str]:
    """Extract all email addresses from text"""
    if not text:
        return []
    
    # Regular expression to find email addresses
    email_pattern = r'[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}'
    emails = re.findall(email_pattern, text)
    
    # Remove duplicates and return as list
    return list(set([email.lower() for email in emails]))

def map_interaction_type(activity_subtype: str) -> str:
    """Map SF_PRM_ActivitySubtype__c to InteractionType enum"""
    if not activity_subtype:
        return "InteractionType.Other"
    
    activity_subtype_lower = activity_subtype.lower()
    
    if 'email' in activity_subtype_lower:
        return "InteractionType.Email"
    elif 'call' in activity_subtype_lower or 'phone' in activity_subtype_lower:
        return "InteractionType.Call"
    elif 'meeting' in activity_subtype_lower:
        if 'virtual' in activity_subtype_lower or 'online' in activity_subtype_lower or 'teams' in activity_subtype_lower or 'zoom' in activity_subtype_lower:
            return "InteractionType.VirtualMeeting"
        else:
            return "InteractionType.InPersonMeeting"
    elif 'chat' in activity_subtype_lower or 'message' in activity_subtype_lower:
        return "InteractionType.Chat"
    else:
        return "InteractionType.Other"

def parse_csv_with_multiline(file_path: str) -> List[Dict[str, str]]:
    """Parse CSV file handling multi-line descriptions properly"""
    records = []
    
    with open(file_path, 'r', encoding='utf-8', newline='') as file:
        # Use csv.reader to properly handle quoted fields with newlines
        reader = csv.DictReader(file)
        
        for row in reader:
            records.append(row)
    
    return records

def generate_interaction_from_task_seeder(csv_file_path: str, output_file_path: str) -> None:
    """Generate the complete InteractionFromTaskSeeder.cs file"""
    
    print("Parsing CSV file...")
    tasks = parse_csv_with_multiline(csv_file_path)
    print(f"Found {len(tasks)} task records")
    
    # Track duplicates by Subject + Description
    seen_interactions = {}  # Key: (subject, description) -> interaction data
    interaction_data_list = []
    skipped_count = 0
    
    for task in tasks:
        subject = clean_field(task.get('Subject', ''))
        description = clean_field(task.get('Description', ''))
        
        if not subject:
            skipped_count += 1
            continue
        
        # Check for duplicate (exact match on Subject and Description)
        dup_key = (subject, description or '')
        
        if dup_key in seen_interactions:
            # Duplicate found - add to the existing interaction's relationships
            existing = seen_interactions[dup_key]
            
            # Add Who.Id (contact)
            who_id = clean_field(task.get('Who.Id', ''))
            if who_id:
                existing['contact_ids'].add(who_id)
            
            # Add What.AccountNumber (partner)
            account_number = clean_field(task.get('What.AccountNumber', ''))
            if account_number and account_number.isdigit():
                existing['partner_erp_values'].add(int(account_number))
            
            # Add Owner.Email
            owner_email = clean_field(task.get('Owner.Email', ''))
            if owner_email:
                existing['owner_emails'].add(owner_email.lower())
            
            # Add SF_Organisation__r.SF_EntityCode__c
            org_code = clean_field(task.get('SF_Organisation__r.SF_EntityCode__c', ''))
            if org_code:
                existing['org_codes'].add(org_code)
            
            continue
        
        # New interaction
        gmail_message_id = clean_field(task.get('Id', ''))
        activity_date = clean_field(task.get('ActivityDate', ''))
        activity_subtype = clean_field(task.get('SF_PRM_ActivitySubtype__c', ''))
        who_id = clean_field(task.get('Who.Id', ''))
        account_number = clean_field(task.get('What.AccountNumber', ''))
        owner_email = clean_field(task.get('Owner.Email', ''))
        org_code = clean_field(task.get('SF_Organisation__r.SF_EntityCode__c', ''))
        
        # Parse date
        date_str = "DateTime.UtcNow"
        if activity_date:
            try:
                from datetime import datetime
                parsed_date = datetime.strptime(activity_date, '%Y-%m-%d')
                date_str = f'DateTime.Parse("{activity_date}").ToUniversalTime()'
            except:
                date_str = "DateTime.UtcNow"
        
        # Extract email addresses from description
        email_addresses = extract_email_addresses(description) if description else []
        
        # Build the email addresses list for C#
        if email_addresses:
            email_list_str = "new List<string> { " + ", ".join([escape_csharp_string(email) for email in email_addresses]) + " }"
        else:
            email_list_str = "new List<string>()"
        
        # Store interaction data
        interaction_data = {
            'gmail_message_id': gmail_message_id,
            'type': map_interaction_type(activity_subtype),
            'date': date_str,
            'subject': subject,
            'description': description,
            'email_addresses': email_list_str,
            'contact_ids': {who_id} if who_id else set(),
            'partner_erp_values': {int(account_number)} if account_number and account_number.isdigit() else set(),
            'owner_emails': {owner_email.lower()} if owner_email else set(),
            'org_codes': {org_code} if org_code else set()
        }
        
        seen_interactions[dup_key] = interaction_data
        interaction_data_list.append(interaction_data)
    
    print(f"Generated {len(interaction_data_list)} unique interactions")
    print(f"Skipped {skipped_count} records due to missing Subject")
    
    # Generate C# tuples for interactions
    interaction_tuples = []
    for idx, data in enumerate(interaction_data_list):
        # Convert sets to comma-separated strings for the tuple
        contact_ids_str = ", ".join([escape_csharp_string(cid) for cid in data['contact_ids']]) if data['contact_ids'] else ""
        partner_erp_str = ", ".join([str(p) for p in data['partner_erp_values']]) if data['partner_erp_values'] else ""
        owner_emails_str = ", ".join([escape_csharp_string(email) for email in data['owner_emails']]) if data['owner_emails'] else ""
        org_codes_str = ", ".join([escape_csharp_string(code) for code in data['org_codes']]) if data['org_codes'] else ""
        
        # Determine CreatedBy lookup
        if owner_emails_str:
            created_by_emails = list(data['owner_emails'])
            first_email = escape_csharp_string(created_by_emails[0])
            created_by_lookup = f"paoUserEmailMapping.ContainsKey({first_email}.ToLower()) ? paoUserEmailMapping[{first_email}.ToLower()] : 0"
        else:
            created_by_lookup = "0"
        
        interaction_tuple = f"""                new (
                    {escape_csharp_string(data['gmail_message_id'])},
                    new UNOPSInteraction
                    {{
                        Name = {escape_csharp_string(data['subject'])},
                        Type = {data['type']},
                        Date = {data['date']},
                        Subject = {escape_csharp_string(data['subject'])},
                        Description = {escape_csharp_string(data['description'])},
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = {escape_csharp_string(data['gmail_message_id'])},
                        EmailAddresses = {data['email_addresses']},
                        Status = (EntityStatus)1,
                        CreatedBy = {created_by_lookup},
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    }},
                    new List<string> {{ {contact_ids_str} }},
                    new List<int> {{ {partner_erp_str} }},
                    new List<string> {{ {owner_emails_str} }},
                    new List<string> {{ {org_codes_str} }}
                ),"""
        
        interaction_tuples.append(interaction_tuple)
    
    # Remove comma from last tuple
    if interaction_tuples:
        interaction_tuples[-1] = interaction_tuples[-1].rstrip(',')
    
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
    public static class InteractionFromTaskSeeder
    {{
        public static async Task SeedInteractionsFromTasksAsync(UNOPSAppDbContext context)
        {{
            // Create mapping from Contact.ContactNumber to ContactId
            var contactMapping = await context.Contacts
                .Where(c => !string.IsNullOrEmpty(c.ContactNumber))
                .ToDictionaryAsync(c => c.ContactNumber, c => c.Id);

            // Create mapping from Partner.ErpDimValue to PartnerId
            var partnerMapping = await context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .ToDictionaryAsync(p => p.ErpDimValue.Value, p => p.Id);

            // Create mapping from PAOUser.Email to UserId
            var paoUserEmailMapping = await context.PAOUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .ToDictionaryAsync(u => u.Email.ToLower(), u => u.Id);

            // Create mapping from OrganizationHierarchy.Code to OrganizationHierarchyId
            var orgHierarchyMapping = await context.OrganizationHierarchies
                .Where(oh => !string.IsNullOrEmpty(oh.Code) && oh.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                .ToDictionaryAsync(oh => oh.Code, oh => oh.Id);

            // Create mapping from Contact.Email to ContactId (for email-based lookups)
            var contactEmailMapping = await context.Contacts
                .Where(c => !string.IsNullOrEmpty(c.Email))
                .GroupBy(c => c.Email.ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.First().Id);

            // Get all Partner IDs with their Contact relationships for parent partner lookup
            var contactPartnerMapping = await context.Contacts
                .ToDictionaryAsync(c => c.Id, c => c.PartnerId);

            // Process interactions
            var interactionsToProcess = new List<(string GmailMessageId, UNOPSInteraction Interaction, List<string> ContactIds, List<int> PartnerErpValues, List<string> OwnerEmails, List<string> OrgCodes)>
            {{
{chr(10).join(interaction_tuples)}
            }};

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {{
                // Step 1: Process all interactions (create or update)
                foreach (var (gmailMessageId, interactionData, _, _, _, _) in interactionsToProcess)
                {{
                    if (string.IsNullOrEmpty(gmailMessageId))
                        continue;

                    // Check if interaction already exists based on GmailMessageId
                    var existingInteraction = await context.Interactions
                        .FirstOrDefaultAsync(i => i.GmailMessageId == gmailMessageId);

                    if (existingInteraction != null)
                    {{
                        // Update existing interaction
                        existingInteraction.Name = interactionData.Name;
                        existingInteraction.Type = interactionData.Type;
                        existingInteraction.Date = interactionData.Date;
                        existingInteraction.Subject = interactionData.Subject;
                        existingInteraction.Description = interactionData.Description;
                        existingInteraction.Location = interactionData.Location;
                        existingInteraction.GmailThreadId = interactionData.GmailThreadId;
                        existingInteraction.EmailAddresses = interactionData.EmailAddresses;
                        existingInteraction.Status = interactionData.Status;
                        existingInteraction.LastModifiedBy = 0;
                        existingInteraction.LastModifiedDate = DateTime.UtcNow;
                    }}
                    else
                    {{
                        // Add new interaction to context
                        context.Interactions.Add(interactionData);
                    }}
                }}

                // Save all interactions at once
                await context.SaveChangesAsync();

                // Step 2: Process all junction table records in batch
                var interactionContactsToAdd = new List<InteractionContact>();
                var interactionPartnersToAdd = new List<InteractionPartner>();
                var interactionUsersToAdd = new List<InteractionUser>();
                var orgUnitRelationshipsToAdd = new List<OrganizationUnitRelationship>();

                foreach (var (gmailMessageId, _, contactIds, partnerErpValues, ownerEmails, orgCodes) in interactionsToProcess)
                {{
                    if (string.IsNullOrEmpty(gmailMessageId))
                        continue;

                    // Get the interaction (now guaranteed to exist with an ID)
                    var interaction = await context.Interactions
                        .FirstOrDefaultAsync(i => i.GmailMessageId == gmailMessageId);

                    if (interaction == null)
                        continue;

                    // Track unique relationships to avoid duplicates
                    var uniqueContactIds = new HashSet<int>();
                    var uniquePartnerIds = new HashSet<int>();
                    var uniqueUserIds = new HashSet<int>();

                    // Process Contact relationships from Who.Id (ContactNumber)
                    foreach (var contactId in contactIds)
                    {{
                        if (contactMapping.ContainsKey(contactId))
                        {{
                            var dbContactId = contactMapping[contactId];
                            uniqueContactIds.Add(dbContactId);

                            // Also get the parent Partner for this Contact
                            if (contactPartnerMapping.ContainsKey(dbContactId))
                            {{
                                var parentPartnerId = contactPartnerMapping[dbContactId];
                                uniquePartnerIds.Add(parentPartnerId);
                            }}
                        }}
                    }}

                    // Process Partner relationships from What.AccountNumber (ErpDimValue)
                    foreach (var erpValue in partnerErpValues)
                    {{
                        if (partnerMapping.ContainsKey(erpValue))
                        {{
                            uniquePartnerIds.Add(partnerMapping[erpValue]);
                        }}
                    }}

                    // Process email addresses from interaction.EmailAddresses
                    if (interaction.EmailAddresses != null && interaction.EmailAddresses.Any())
                    {{
                        foreach (var email in interaction.EmailAddresses)
                        {{
                            var emailLower = email.ToLower();

                            // Find contacts by email
                            if (contactEmailMapping.ContainsKey(emailLower))
                            {{
                                var dbContactId = contactEmailMapping[emailLower];
                                uniqueContactIds.Add(dbContactId);

                                // Also get the parent Partner for this Contact
                                if (contactPartnerMapping.ContainsKey(dbContactId))
                                {{
                                    var parentPartnerId = contactPartnerMapping[dbContactId];
                                    uniquePartnerIds.Add(parentPartnerId);
                                }}
                            }}

                            // Find users by email
                            if (paoUserEmailMapping.ContainsKey(emailLower))
                            {{
                                uniqueUserIds.Add(paoUserEmailMapping[emailLower]);
                            }}
                        }}
                    }}

                    // Process Owner.Email for User relationships
                    foreach (var ownerEmail in ownerEmails)
                    {{
                        var emailLower = ownerEmail.ToLower();
                        if (paoUserEmailMapping.ContainsKey(emailLower))
                        {{
                            uniqueUserIds.Add(paoUserEmailMapping[emailLower]);
                        }}
                    }}

                    // Create InteractionContact records
                    foreach (var contactId in uniqueContactIds)
                    {{
                        // Check if relationship already exists
                        var existingRelationship = await context.Set<InteractionContact>()
                            .FirstOrDefaultAsync(ic => ic.InteractionId == interaction.Id && ic.ContactId == contactId);

                        if (existingRelationship == null)
                        {{
                            interactionContactsToAdd.Add(new InteractionContact
                            {{
                                InteractionId = interaction.Id,
                                ContactId = contactId
                            }});
                        }}
                    }}

                    // Create InteractionPartner records
                    foreach (var partnerId in uniquePartnerIds)
                    {{
                        // Check if relationship already exists
                        var existingRelationship = await context.Set<InteractionPartner>()
                            .FirstOrDefaultAsync(ip => ip.InteractionId == interaction.Id && ip.PartnerId == partnerId);

                        if (existingRelationship == null)
                        {{
                            interactionPartnersToAdd.Add(new InteractionPartner
                            {{
                                InteractionId = interaction.Id,
                                PartnerId = partnerId
                            }});
                        }}
                    }}

                    // Create InteractionUser records
                    foreach (var userId in uniqueUserIds)
                    {{
                        // Check if relationship already exists
                        var existingRelationship = await context.Set<InteractionUser>()
                            .FirstOrDefaultAsync(iu => iu.InteractionId == interaction.Id && iu.UserId == userId);

                        if (existingRelationship == null)
                        {{
                            interactionUsersToAdd.Add(new InteractionUser
                            {{
                                InteractionId = interaction.Id,
                                UserId = userId
                            }});
                        }}
                    }}

                    // Process OrganizationUnitRelationship from SF_Organisation__r.SF_EntityCode__c
                    foreach (var orgCode in orgCodes)
                    {{
                        if (orgHierarchyMapping.ContainsKey(orgCode))
                        {{
                            var orgHierarchyId = orgHierarchyMapping[orgCode];

                            // Check if relationship already exists
                            var existingRelationship = await context.OrganizationUnitRelationships
                                .FirstOrDefaultAsync(r => r.EntityType == nameof(Interaction) && 
                                                          r.EntityId == interaction.Id && 
                                                          r.OrganizationHierarchyId == orgHierarchyId);

                            if (existingRelationship == null)
                            {{
                                orgUnitRelationshipsToAdd.Add(new OrganizationUnitRelationship
                                {{
                                    OrganizationHierarchyId = orgHierarchyId,
                                    EntityId = interaction.Id,
                                    EntityType = nameof(Interaction),
                                    Name = $"Interaction-{{interaction.Id}}-{{orgHierarchyId}}",
                                    Status = EntityStatus.Active,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.UtcNow,
                                    LastModifiedBy = 0,
                                    LastModifiedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                }});
                            }}
                        }}
                    }}
                }}

                // Add all junction table records at once
                if (interactionContactsToAdd.Any())
                    await context.Set<InteractionContact>().AddRangeAsync(interactionContactsToAdd);
                
                if (interactionPartnersToAdd.Any())
                    await context.Set<InteractionPartner>().AddRangeAsync(interactionPartnersToAdd);
                
                if (interactionUsersToAdd.Any())
                    await context.Set<InteractionUser>().AddRangeAsync(interactionUsersToAdd);
                
                if (orgUnitRelationshipsToAdd.Any())
                    await context.OrganizationUnitRelationships.AddRangeAsync(orgUnitRelationshipsToAdd);

                // Save all junction table records at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"Successfully seeded {{interactionsToProcess.Count}} interactions");
                Console.WriteLine($"Created {{interactionContactsToAdd.Count}} InteractionContact relationships");
                Console.WriteLine($"Created {{interactionPartnersToAdd.Count}} InteractionPartner relationships");
                Console.WriteLine($"Created {{interactionUsersToAdd.Count}} InteractionUser relationships");
                Console.WriteLine($"Created {{orgUnitRelationshipsToAdd.Count}} OrganizationUnitRelationship records");
            }}
            catch (Exception ex)
            {{
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error seeding interactions: {{ex.Message}}");
                throw;
            }}
        }}
    }}
}}"""
    
    # Write the generated file
    try:
        with open(output_file_path, 'w', encoding='utf-8') as file:
            file.write(csharp_content)
        print(f"Successfully generated InteractionFromTaskSeeder.cs")
        print(f"Generated {len(interaction_data_list)} interactions")
        print(f"Output file: {output_file_path}")
    except Exception as e:
        print(f"Error writing output file: {e}")

def main():
    """Main function to run the generator"""
    
    # Get the directory where this script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # File paths (relative to script location)
    csv_file = os.path.join(script_dir, "Tasks_20251009 - Sheet1.csv")
    output_file = os.path.join(script_dir, "..", "..", "..", "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", "InteractionFromTaskSeeder.cs")
    
    print("InteractionFromTaskSeeder Generator")
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
    generate_interaction_from_task_seeder(csv_file, output_file)

if __name__ == "__main__":
    main()
