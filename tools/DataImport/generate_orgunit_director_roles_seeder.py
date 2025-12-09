#!/usr/bin/env python3
"""
Script to generate OrgUnitDirectorRolesSeeder.cs from CSV data.
Reads the OrgUnit_Director_DeputyDirector_etc - ImportFile.csv and generates
a C# seeder file for EntityUserRoles linking OrganizationHierarchy entities
to Users via EntityRoles.
"""

import csv
import os
from collections import defaultdict

# Path configuration
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CSV_FILE = os.path.join(SCRIPT_DIR, "Archives/opportunity-feature-import-files/OrgUnit_Director_DeputyDirector_etc - ImportFile.csv")
OUTPUT_FILE = os.path.join(SCRIPT_DIR, "../../UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/OrgUnitDirectorRolesSeeder.cs")

# Mapping from CSV column suffix to EntityRole name
# Note: Ignoring HSSE columns as requested
ROLE_MAPPINGS = {
    "Region_Director_Resource_Email": "Region Director",
    "Region_Deputy_Director_Resource_Email": "Region Deputy Director",
    "Hub_Director_Resource_Email": "Hub Director",
    "Hub_Deputy_Director_Resource_Email": "Hub Deputy Director",
    "Org_Unit_Director_Resource_Email": "OrgUnit Director",
    "Org_Unit_Deputy_Director_Resource_Email": "OrgUnit Deputy Director",
}


def read_csv_data(csv_path):
    """Read the CSV file and extract org unit to role/email mappings."""
    data = []
    
    with open(csv_path, 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f)
        for row in reader:
            org_unit_code = row.get('Org_Unit', '').strip()
            if not org_unit_code:
                continue
            
            # Extract all role emails for this org unit
            role_emails = {}
            for csv_column, role_name in ROLE_MAPPINGS.items():
                email = row.get(csv_column, '').strip()
                if email:
                    role_emails[role_name] = email.lower()  # Normalize email to lowercase
            
            if role_emails:
                data.append({
                    'org_unit_code': org_unit_code,
                    'role_emails': role_emails
                })
    
    return data


def collect_unique_emails(data):
    """Collect all unique emails from the data."""
    emails = set()
    for entry in data:
        for email in entry['role_emails'].values():
            emails.add(email)
    return sorted(emails)


def collect_unique_org_units(data):
    """Collect all unique org unit codes from the data."""
    return sorted(set(entry['org_unit_code'] for entry in data))


def generate_seeder_code(data):
    """Generate the C# seeder code."""
    unique_emails = collect_unique_emails(data)
    unique_org_units = collect_unique_org_units(data)
    unique_roles = sorted(set(ROLE_MAPPINGS.values()))
    
    code = '''using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds EntityUserRole records linking OrganizationHierarchy entities to Users via EntityRoles.
/// Generated from OrgUnit_Director_DeputyDirector_etc - ImportFile.csv
/// </summary>
public class OrgUnitDirectorRolesSeeder
{
    public static async Task SeedOrgUnitDirectorRolesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("Starting OrgUnitDirectorRolesSeeder...");
        
        // Get all EntityRoles for OrganizationHierarchy
        var entityRoles = await context.EntityRoles
            .Where(er => er.EntityType == "OrganizationHierarchy" && !er.IsDeleted)
            .ToListAsync();
        
        var roleNameToId = entityRoles.ToDictionary(r => r.Name, r => r.Id);
        
        // Get all users by email (case-insensitive)
        var allUsers = await context.PAOUsers
            .Where(u => u.Email != null)
            .Select(u => new { u.Id, Email = u.Email!.ToLower() })
            .ToListAsync();
        
        var emailToUserId = allUsers
            .GroupBy(u => u.Email)
            .ToDictionary(g => g.Key, g => g.First().Id);
        
        // Get all OrgUnit type OrganizationHierarchy records by Code
        var orgUnits = await context.OrganizationHierarchies
            .Where(oh => oh.Type == OrganizationUnitType.OrgUnit && !oh.IsDeleted)
            .Select(oh => new { oh.Id, oh.Code })
            .ToListAsync();
        
        var codeToOrgUnitId = orgUnits.ToDictionary(o => o.Code, o => o.Id);
        
        // Get existing EntityUserRoles to avoid duplicates
        var existingRoles = await context.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && !eur.IsDeleted)
            .Select(eur => new { eur.EntityId, eur.EntityRoleId, eur.UserId })
            .ToListAsync();
        
        var existingRoleKeys = existingRoles
            .Select(r => $"{r.EntityId}_{r.EntityRoleId}_{r.UserId}")
            .ToHashSet();
        
        var rolesToAdd = new List<EntityUserRole>();
        var skippedCount = 0;
        var missingOrgUnits = new HashSet<string>();
        var missingUsers = new HashSet<string>();
        var missingRoles = new HashSet<string>();
        
'''
    
    # Generate the role assignments
    code += '''        // Process each org unit and its role assignments
'''
    
    for entry in data:
        org_unit_code = entry['org_unit_code']
        code += f'''
        // {org_unit_code}
        if (codeToOrgUnitId.TryGetValue("{org_unit_code}", out var orgUnit_{org_unit_code.replace('-', '_')}Id))
        {{
'''
        for role_name, email in entry['role_emails'].items():
            safe_var_name = org_unit_code.replace('-', '_')
            code += f'''            // {role_name}: {email}
            if (roleNameToId.TryGetValue("{role_name}", out var role_{safe_var_name}_{role_name.replace(' ', '_')}Id) &&
                emailToUserId.TryGetValue("{email}", out var user_{safe_var_name}_{role_name.replace(' ', '_')}Id))
            {{
                var key_{safe_var_name}_{role_name.replace(' ', '_')} = $"{{orgUnit_{safe_var_name}Id}}_{{role_{safe_var_name}_{role_name.replace(' ', '_')}Id}}_{{user_{safe_var_name}_{role_name.replace(' ', '_')}Id}}";
                if (!existingRoleKeys.Contains(key_{safe_var_name}_{role_name.replace(' ', '_')}))
                {{
                    rolesToAdd.Add(new EntityUserRole
                    {{
                        Name = $"{role_name} - OrganizationHierarchy - {{orgUnit_{safe_var_name}Id}} - {{user_{safe_var_name}_{role_name.replace(' ', '_')}Id}}",
                        EntityId = orgUnit_{safe_var_name}Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_{safe_var_name}_{role_name.replace(' ', '_')}Id,
                        UserId = user_{safe_var_name}_{role_name.replace(' ', '_')}Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    }});
                }}
                else
                {{
                    skippedCount++;
                }}
            }}
            else
            {{
                if (!roleNameToId.ContainsKey("{role_name}")) missingRoles.Add("{role_name}");
                if (!emailToUserId.ContainsKey("{email}")) missingUsers.Add("{email}");
            }}
'''
        
        code += '''        }
        else
        {
            missingOrgUnits.Add("''' + org_unit_code + '''");
        }
'''
    
    code += '''
        // Add all new roles
        if (rolesToAdd.Any())
        {
            await context.EntityUserRoles.AddRangeAsync(rolesToAdd);
            await context.SaveChangesAsync();
            Console.WriteLine($"Added {rolesToAdd.Count} new EntityUserRole records for OrganizationHierarchy.");
        }
        else
        {
            Console.WriteLine("No new EntityUserRole records to add.");
        }
        
        if (skippedCount > 0)
        {
            Console.WriteLine($"Skipped {skippedCount} existing EntityUserRole records.");
        }
        
        if (missingOrgUnits.Any())
        {
            Console.WriteLine($"Warning: Could not find OrgUnit codes: {string.Join(", ", missingOrgUnits)}");
        }
        
        if (missingUsers.Any())
        {
            Console.WriteLine($"Warning: Could not find users with emails: {string.Join(", ", missingUsers)}");
        }
        
        if (missingRoles.Any())
        {
            Console.WriteLine($"Warning: Could not find EntityRoles: {string.Join(", ", missingRoles)}");
        }
        
        Console.WriteLine("OrgUnitDirectorRolesSeeder completed.");
    }
}
'''
    
    return code


def main():
    print(f"Reading CSV from: {CSV_FILE}")
    
    if not os.path.exists(CSV_FILE):
        print(f"Error: CSV file not found at {CSV_FILE}")
        return
    
    data = read_csv_data(CSV_FILE)
    print(f"Found {len(data)} org units with role assignments")
    
    # Generate the seeder code
    seeder_code = generate_seeder_code(data)
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(OUTPUT_FILE), exist_ok=True)
    
    # Write the output file
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        f.write(seeder_code)
    
    print(f"Generated seeder file: {OUTPUT_FILE}")
    
    # Print summary
    unique_emails = collect_unique_emails(data)
    unique_org_units = collect_unique_org_units(data)
    print(f"Summary:")
    print(f"  - Unique org units: {len(unique_org_units)}")
    print(f"  - Unique emails: {len(unique_emails)}")
    print(f"  - Role types: {len(ROLE_MAPPINGS)}")


if __name__ == "__main__":
    main()

