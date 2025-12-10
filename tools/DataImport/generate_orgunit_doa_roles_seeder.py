#!/usr/bin/env python3
"""
Script to generate OrgUnitDOARolesSeeder.cs from CSV data.
Reads the OrgUnit_DoA - ImportFile.csv and generates
a C# seeder file for EntityUserRoles linking OrganizationHierarchy entities
to Users via DoA (Delegation of Authority) EntityRoles.
"""

import csv
import os
from collections import defaultdict

# Path configuration
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CSV_FILE = os.path.join(SCRIPT_DIR, "Archives/opportunity-feature-import-files/OrgUnit_DoA - ImportFile.csv")
OUTPUT_FILE = os.path.join(SCRIPT_DIR, "../../UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/OrgUnitDOARolesSeeder.cs")

# Mapping from DoA level to EntityRole name and code
# Codes from EntityRoleSeeder.cs for OrganizationHierarchy entity type
DOA_LEVEL_TO_ROLE = {
    "1": {"name": "DoA1", "code": "DoA1_OrganizationHierarchy"},
    "2": {"name": "DoA2", "code": "DoA2_OrganizationHierarchy"},
    "3": {"name": "DoA3", "code": "DoA3_OrganizationHierarchy"},
    "4": {"name": "DoA4", "code": "DoA4_OrganizationHierarchy"},
}


def read_csv_data(csv_path):
    """Read the CSV file and extract org unit to DoA role/email mappings."""
    # Use a dictionary to collect unique (org_unit, doa_level, email) combinations
    unique_entries = {}
    
    with open(csv_path, 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f)
        for row in reader:
            org_unit_code = row.get('Org_Unit', '').strip()
            doa_level = row.get('Delegation_Of_Authority_Level', '').strip()
            email = row.get('Resource_Email', '').strip().lower()
            
            if not org_unit_code or not doa_level or not email:
                continue
            
            if doa_level not in DOA_LEVEL_TO_ROLE:
                print(f"Warning: Unknown DoA level '{doa_level}' for org unit {org_unit_code}")
                continue
            
            # Create unique key to avoid duplicates
            key = (org_unit_code, doa_level, email)
            if key not in unique_entries:
                role_info = DOA_LEVEL_TO_ROLE[doa_level]
                unique_entries[key] = {
                    'org_unit_code': org_unit_code,
                    'doa_level': doa_level,
                    'role_name': role_info['name'],
                    'role_code': role_info['code'],
                    'email': email
                }
    
    return list(unique_entries.values())


def collect_unique_emails(data):
    """Collect all unique emails from the data."""
    return sorted(set(entry['email'] for entry in data))


def collect_unique_org_units(data):
    """Collect all unique org unit codes from the data."""
    return sorted(set(entry['org_unit_code'] for entry in data))


def generate_seeder_code(data):
    """Generate the C# seeder code."""
    unique_emails = collect_unique_emails(data)
    unique_org_units = collect_unique_org_units(data)
    
    # Group data by org unit for cleaner output
    by_org_unit = defaultdict(list)
    for entry in data:
        by_org_unit[entry['org_unit_code']].append(entry)
    
    code = '''using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds EntityUserRole records for DoA (Delegation of Authority) roles linking 
/// OrganizationHierarchy entities to Users.
/// Generated from OrgUnit_DoA - ImportFile.csv
/// </summary>
public class OrgUnitDOARolesSeeder
{
    public static async Task SeedOrgUnitDOARolesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("Starting OrgUnitDOARolesSeeder...");
        
        // Get all DoA EntityRoles for OrganizationHierarchy by Code
        var entityRoles = await context.EntityRoles
            .Where(er => er.EntityType == "OrganizationHierarchy" 
                && (er.Code == "DoA1_OrganizationHierarchy" || er.Code == "DoA2_OrganizationHierarchy" 
                    || er.Code == "DoA3_OrganizationHierarchy" || er.Code == "DoA4_OrganizationHierarchy")
                && !er.IsDeleted)
            .ToListAsync();
        
        var roleCodeToId = entityRoles.ToDictionary(r => r.Code!, r => r.Id);
        
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
        
        // Get existing EntityUserRoles to update or add
        var existingRoles = await context.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && !eur.IsDeleted)
            .ToListAsync();
        
        var existingRoleDict = existingRoles
            .GroupBy(r => (r.EntityId, r.EntityRoleId, r.UserId))
            .ToDictionary(g => g.Key, g => g.First());
        
        var rolesToAdd = new List<EntityUserRole>();
        var rolesToUpdate = new List<EntityUserRole>();
        var updatedCount = 0;
        var missingOrgUnits = new HashSet<string>();
        var missingUsers = new HashSet<string>();
        var missingRoles = new HashSet<string>();
        
        // Process DoA role assignments
'''
    
    # Generate role assignments grouped by org unit
    for org_unit_code in sorted(by_org_unit.keys()):
        entries = by_org_unit[org_unit_code]
        safe_var = org_unit_code.replace('-', '_')
        
        code += f'''
        // {org_unit_code}
        if (codeToOrgUnitId.TryGetValue("{org_unit_code}", out var orgUnit_{safe_var}Id))
        {{
'''
        for entry in entries:
            role_name = entry['role_name']
            role_code = entry['role_code']
            email = entry['email']
            safe_role = role_name.replace(' ', '_')
            # Create a unique variable suffix using org unit, role, and a hash of email
            var_suffix = f"{safe_var}_{safe_role}_{abs(hash(email)) % 10000}"
            
            code += f'''            // {role_name}: {email}
            if (roleCodeToId.TryGetValue("{role_code}", out var role_{var_suffix}Id) &&
                emailToUserId.TryGetValue("{email}", out var user_{var_suffix}Id))
            {{
                var keyTuple_{var_suffix} = (EntityId: orgUnit_{safe_var}Id, EntityRoleId: role_{var_suffix}Id, UserId: user_{var_suffix}Id);
                if (existingRoleDict.TryGetValue(keyTuple_{var_suffix}, out var existingRole_{var_suffix}))
                {{
                    // Update existing record
                    existingRole_{var_suffix}.Name = $"{role_code} - {{orgUnit_{safe_var}Id}} - {{user_{var_suffix}Id}}";
                    existingRole_{var_suffix}.Status = EntityStatus.Active;
                    existingRole_{var_suffix}.LastModifiedDate = DateTime.UtcNow;
                    existingRole_{var_suffix}.LastModifiedBy = 1;
                    existingRole_{var_suffix}.IsDeleted = false;
                    existingRole_{var_suffix}.DeletedDate = null;
                    existingRole_{var_suffix}.DeletedBy = 0;
                    rolesToUpdate.Add(existingRole_{var_suffix});
                    updatedCount++;
                }}
                else
                {{
                    // Add new record
                    rolesToAdd.Add(new EntityUserRole
                    {{
                        Name = $"{role_name} - OrganizationHierarchy - {{orgUnit_{safe_var}Id}} - {{user_{var_suffix}Id}}",
                        EntityId = orgUnit_{safe_var}Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_{var_suffix}Id,
                        UserId = user_{var_suffix}Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    }});
                }}
            }}
            else
            {{
                if (!roleCodeToId.ContainsKey("{role_code}")) missingRoles.Add("{role_code}");
                if (!emailToUserId.ContainsKey("{email}")) missingUsers.Add("{email}");
            }}
'''
        
        code += f'''        }}
        else
        {{
            missingOrgUnits.Add("{org_unit_code}");
        }}
'''
    
    code += '''
        // Add all new roles
        if (rolesToAdd.Any())
        {
            await context.EntityUserRoles.AddRangeAsync(rolesToAdd);
            Console.WriteLine($"Added {rolesToAdd.Count} new DoA EntityUserRole records for OrganizationHierarchy.");
        }
        
        // Update existing roles
        if (rolesToUpdate.Any())
        {
            context.EntityUserRoles.UpdateRange(rolesToUpdate);
            Console.WriteLine($"Updated {updatedCount} existing DoA EntityUserRole records for OrganizationHierarchy.");
        }
        
        if (rolesToAdd.Any() || rolesToUpdate.Any())
        {
            await context.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("No new or updated DoA EntityUserRole records.");
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
        
        Console.WriteLine("OrgUnitDOARolesSeeder completed.");
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
    print(f"Found {len(data)} unique DoA role assignments")
    
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
    
    # Count by DoA level
    doa_counts = defaultdict(int)
    for entry in data:
        doa_counts[entry['role_name']] += 1
    
    print(f"\nSummary:")
    print(f"  - Unique org units: {len(unique_org_units)}")
    print(f"  - Unique emails: {len(unique_emails)}")
    print(f"  - Role breakdown:")
    for role_name in sorted(doa_counts.keys()):
        print(f"      {role_name}: {doa_counts[role_name]}")


if __name__ == "__main__":
    main()

