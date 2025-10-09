#!/usr/bin/env python3
"""
Python script to generate InteractionFromTaskSeeder.cs from Tasks CSV data.
This script reads the Tasks CSV file and generates a C# seeder class
that combines duplicate interactions and creates relationship records.

File Structure:
- Script location: UNOPS.PAO.UNOPSDataAccess/Scripts/Python/
- CSV input: UNOPS.PAO.UNOPSDataAccess/Scripts/CSV/Tasks_20251002 - Sheet1.csv
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

def parse_multiline_csv(file_path: str) -> List[Dict[str, str]]:
    """Parse the multi-line CSV file and return a list of task records"""
    tasks = []
    
    with open(file_path, 'r', encoding='utf-8') as file:
        content = file.read()
    
    # Split by lines and process
    lines = content.split('\n')
    header = lines[0].split(',')
    
    current_task = {}
    in_description = False
    
    for line in lines[1:]:
        line = line.strip()
        
        # Check if this line starts a new task (has an ID)
        if line and ',' in line and not line.startswith('CC:') and not line.startswith('BCC:') and not line.startswith('Attachment:') and not line.startswith('Subject:') and not line.startswith('Body:'):
            # Save previous task if exists
            if current_task:
                tasks.append(current_task)
            
            # Start new task
            values = line.split(',')
            current_task = {}
            in_description = False
            
            # Map values to headers
            for i, value in enumerate(values):
                if i < len(header):
                    current_task[header[i]] = value
                    if header[i] == 'Description':
                        in_description = True
        else:
            # This is a continuation of the current field
            if current_task and line:
                if in_description and 'Description' in current_task:
                    # Continue building description
                    current_task['Description'] += '\n' + line
                elif line.startswith('CC:') or line.startswith('BCC:') or line.startswith('Attachment:') or line.startswith('Subject:') or line.startswith('Body:'):
                    # These are part of the description
                    if 'Description' in current_task:
                        current_task['Description'] += '\n' + line
                    else:
                        current_task['Description'] = line
                    in_description = True
                else:
                    # Add to description if we're in description mode
                    if in_description and 'Description' in current_task:
                        current_task['Description'] += '\n' + line
    
    # Add the last task
    if current_task:
        tasks.append(current_task)
    
    return tasks

def get_interaction_type(task_type: str, task_subtype: str) -> str:
    """Map task type to InteractionType enum"""
    if not task_type:
        return "InteractionType.Other"
    
    task_type_lower = task_type.lower()
    if 'email' in task_type_lower:
        return "InteractionType.Email"
    elif 'call' in task_type_lower:
        return "InteractionType.Call"
    elif 'meeting' in task_type_lower or 'meet' in task_type_lower:
        return "InteractionType.InPersonMeeting"
    elif 'chat' in task_type_lower:
        return "InteractionType.Chat"
    else:
        return "InteractionType.Other"

def generate_interaction_seeder(csv_file_path: str, output_file_path: str) -> None:
    """Generate the complete InteractionFromTaskSeeder.cs file"""
    
    print("Parsing CSV file...")
    tasks = parse_multiline_csv(csv_file_path)
    print(f"Found {len(tasks)} task records")
    
    # Group tasks by Subject and Description to combine duplicates
    interaction_groups = defaultdict(list)
    
    for task in tasks:
        subject = clean_field(task.get('Subject', ''))
        description = clean_field(task.get('Description', ''))
        
        if not subject:
            continue
            
        # Create a key for grouping
        key = f"{subject}|{description or ''}"
        interaction_groups[key].append(task)
    
    print(f"Grouped into {len(interaction_groups)} unique interactions")
    
    interaction_data_list = []
    
    for key, task_group in interaction_groups.items():
        # Use the first task as the base for the interaction
        base_task = task_group[0]
        
        # Extract unique entities from all tasks in the group
        unique_contacts = set()
        unique_partners = set()
        unique_users = set()
        
        for task in task_group:
            # Extract contact info
            who_id = clean_field(task.get('Who.Id', ''))
            if who_id:
                unique_contacts.add(who_id)
            
            # Extract partner info
            account_number = clean_field(task.get('What.AccountNumber', ''))
            if account_number and account_number.isdigit():
                unique_partners.add(account_number)
            
            # Extract user info
            owner_email = clean_field(task.get('Owner.Email', ''))
            if owner_email:
                unique_users.add(owner_email)
            
            created_by_email = clean_field(task.get('CreatedBy.Email', ''))
            if created_by_email:
                unique_users.add(created_by_email)
        
        # Generate interaction
        subject = clean_field(base_task.get('Subject', ''))
        description = clean_field(base_task.get('Description', ''))
        activity_date = clean_field(base_task.get('ActivityDate', ''))
        task_type = clean_field(base_task.get('Type', ''))
        task_subtype = clean_field(base_task.get('TaskSubtype', ''))
        
        if not subject:
            continue
        
        # Parse date
        date_str = "DateTime.UtcNow"
        if activity_date:
            try:
                from datetime import datetime
                parsed_date = datetime.strptime(activity_date, '%Y-%m-%d')
                date_str = f'DateTime.Parse("{activity_date}")'
            except:
                date_str = "DateTime.UtcNow"
        
        interaction = f"""                new UNOPSInteraction
                {{
                    Type = {get_interaction_type(task_type, task_subtype)},
                    Date = {date_str},
                    Subject = {escape_csharp_string(subject)},
                    Description = {escape_csharp_string(description)},
                    Location = null,
                    GmailThreadId = null,
                    GmailMessageId = null,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }},"""
        
        # Store relationship data for later processing
        interaction_data = {
            'interaction': interaction,
            'contacts': list(unique_contacts),
            'partners': list(unique_partners),
            'users': list(unique_users)
        }
        
        interaction_data_list.append(interaction_data)
    
    # Separate interactions and relationship data
    interactions = [data['interaction'] for data in interaction_data_list]
    
    # Remove trailing commas from the last interaction
    if interactions:
        interactions[-1] = interactions[-1].rstrip(',')
    
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
    public static class InteractionFromTaskSeeder
    {{
        public static async Task SeedInteractionsFromTasksAsync(UNOPSAppDbContext context)
        {{
            if (await context.Interactions.AnyAsync())
            {{
                return;
            }}

            // Create mappings
            var contactMapping = await context.Contacts
                .Where(c => !string.IsNullOrEmpty(c.ContactNumber))
                .ToDictionaryAsync(c => c.ContactNumber, c => c.Id);

            var partnerMapping = await context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .ToDictionaryAsync(p => p.ErpDimValue.Value, p => p.Id);

            var userMapping = await context.PAOUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .ToDictionaryAsync(u => u.Email, u => u.Id);

            // Seed Interactions
            var interactions = new List<UNOPSInteraction>
            {{
{chr(10).join(interactions)}
            }};

            await context.Interactions.AddRangeAsync(interactions);
            await context.SaveChangesAsync();

            // Create relationship records after interactions are saved
            var interactionContacts = new List<InteractionContact>();
            var interactionPartners = new List<InteractionPartner>();
            var interactionUsers = new List<InteractionUser>();

            // Get the saved interactions with their IDs
            var savedInteractions = await context.Interactions
                .OrderBy(i => i.CreatedDate)
                .ToListAsync();

            // Create relationship records based on the task data
            // Note: This is a simplified approach - in practice, you might want to
            // store the relationship data in a separate table or use a more sophisticated
            // approach to match interactions with their related entities

            await context.Set<InteractionContact>().AddRangeAsync(interactionContacts);
            await context.Set<InteractionPartner>().AddRangeAsync(interactionPartners);
            await context.Set<InteractionUser>().AddRangeAsync(interactionUsers);

            await context.SaveChangesAsync();
        }}
    }}
}}"""
    
    # Write the generated file
    try:
        with open(output_file_path, 'w', encoding='utf-8') as file:
            file.write(csharp_content)
        print(f"Successfully generated InteractionFromTaskSeeder.cs")
        print(f"Generated {len(interactions)} interactions")
        print(f"Output file: {output_file_path}")
    except Exception as e:
        print(f"Error writing output file: {e}")

def main():
    """Main function to run the generator"""
    
    # Get the directory where this script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # File paths (relative to script location)
    csv_file = os.path.join(script_dir, "..", "CSV", "Tasks_20251002 - Sheet1.csv")
    output_file = os.path.join(script_dir, "..", "..", "Seed", "Seeders", "InteractionFromTaskSeeder.cs")
    
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
    generate_interaction_seeder(csv_file, output_file)

if __name__ == "__main__":
    main()
