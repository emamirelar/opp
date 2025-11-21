#!/usr/bin/env python3
"""
Script to generate Partner_FocalPoint_OrgUnit_Fixes_v3.cs seeder from CSV data
"""

import csv
import os
from typing import List, Dict, Optional

def escape_csharp_string(value: Optional[str]) -> str:
    """Escape special characters for C# string literals"""
    if value is None or value.strip() == "":
        return ""
    # Replace backslashes first, then quotes
    return value.replace('\\', '\\\\').replace('"', '\\"')

def parse_csv_file(csv_path: str) -> List[Dict[str, str]]:
    """Parse the CSV file and return a list of dictionaries"""
    rows = []
    with open(csv_path, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            rows.append(row)
    return rows

def generate_seeder_code(csv_data: List[Dict[str, str]]) -> str:
    """Generate the C# seeder code from CSV data"""
    
    # Start building the C# file
    code = """using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Partner_FocalPoint_OrgUnit_Fixes_v3
    {
        public static async Task UpdatePartnerFocalPointAndOrgUnitAsync(UNOPSAppDbContext context)
        {
            // Create mapping from PAOUser Email to Id (handle duplicates by taking first, filter out null emails)
            // Convert emails to lowercase for case-insensitive matching
            var paoUsers = await context.PAOUsers
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();
            var paoUserMapping = paoUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .GroupBy(u => u.Email!.ToLower())
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Create mapping from OrganizationHierarchy Description to Id (only OrgUnit type)
            var orgUnits = await context.OrganizationHierarchies
                .Where(o => o.Type == OrganizationUnitType.OrgUnit)
                .Select(o => new { o.Id, o.Description })
                .ToListAsync();
            var orgUnitMapping = orgUnits
                .Where(o => !string.IsNullOrEmpty(o.Description))
                .GroupBy(o => o.Description)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Define partner updates data structure
            var partnerUpdates = new List<PartnerUpdateData>
            {
"""

    # Process each row from CSV
    for idx, row in enumerate(csv_data):
        account_number = row.get('AccountNumber', '').strip()
        name = escape_csharp_string(row.get('Name', '').strip())
        # Convert emails to lowercase for case-insensitive matching
        legacy_focal_point = escape_csharp_string(row.get('LegacyFocalPointUser', '').strip().lower())
        suggested_focal_point = escape_csharp_string(row.get('SuggestedFocalPoint', '').strip().lower())
        legacy_org_unit = escape_csharp_string(row.get('LegacyOrgUnit', '').strip())
        suggested_org_unit = escape_csharp_string(row.get('SuggestedOrgUnit', '').strip())
        
        # Skip rows with no meaningful data
        if not name and not account_number:
            continue
        
        # Determine if we have ErpDimValue or need to use Name
        has_erp = account_number != ""
        erp_value = f'{account_number}' if has_erp else 'null'
        
        # Format string values for C#
        name_value = f'"{name}"' if name else "null"
        legacy_fp_value = f'"{legacy_focal_point}"' if legacy_focal_point else "null"
        suggested_fp_value = f'"{suggested_focal_point}"' if suggested_focal_point else "null"
        legacy_ou_value = f'"{legacy_org_unit}"' if legacy_org_unit else "null"
        suggested_ou_value = f'"{suggested_org_unit}"' if suggested_org_unit else "null"
        
        code += f'                new PartnerUpdateData\n'
        code += f'                {{\n'
        code += f'                    ErpDimValue = {erp_value},\n'
        code += f'                    Name = {name_value},\n'
        code += f'                    LegacyFocalPointUser = {legacy_fp_value},\n'
        code += f'                    SuggestedFocalPoint = {suggested_fp_value},\n'
        code += f'                    LegacyOrgUnit = {legacy_ou_value},\n'
        code += f'                    SuggestedOrgUnit = {suggested_ou_value}\n'
        code += f'                }},\n'

    code += """            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                int partnersProcessed = 0;
                int focalPointUpdates = 0;
                int orgUnitUpdates = 0;
                int orgUnitDeletions = 0;

                foreach (var data in partnerUpdates)
                {
                    // Find the partner
                    Partner? partner = null;
                    
                    if (data.ErpDimValue.HasValue)
                    {
                        // Lookup by ErpDimValue
                        partner = await context.Partners
                            .Include(p => p.PartnerFocalPointUser)
                            .FirstOrDefaultAsync(p => p.ErpDimValue == data.ErpDimValue);
                    }
                    else if (!string.IsNullOrEmpty(data.Name))
                    {
                        // Lookup by Name where ErpDimValue is null
                        partner = await context.Partners
                            .Include(p => p.PartnerFocalPointUser)
                            .FirstOrDefaultAsync(p => p.Name == data.Name && p.ErpDimValue == null);
                    }

                    // Load organization unit relationships separately (polymorphic relationship)
                    if (partner != null)
                    {
                        var orgUnitRelationships = await context.OrganizationUnitRelationships
                            .Include(our => our.OrganizationHierarchy)
                            .Where(our => our.EntityId == partner.Id && our.EntityType == nameof(Partner) && !our.IsDeleted)
                            .ToListAsync();
                        
                        partner.OrganizationUnitRelationships = orgUnitRelationships;
                    }

                    if (partner == null)
                    {
                        Console.WriteLine($"Warning: Partner not found - ErpDimValue: {data.ErpDimValue}, Name: {data.Name}");
                        continue;
                    }

                    bool partnerModified = false;
                    string partnerIdentifier = $"{partner.Name} (ErpDimValue: {partner.ErpDimValue ?? 0})";

                    // ========== FOCAL POINT LOGIC ==========
                    
                    // If SuggestedFocalPoint is null, set FocalPointUserId to null
                    if (string.IsNullOrEmpty(data.SuggestedFocalPoint))
                    {
                        if (partner.PartnerFocalPointUserId != null)
                        {
                            partner.PartnerFocalPointUserId = null;
                            partner.LastModifiedBy = -1; // System user
                            partner.LastModifiedDate = DateTime.UtcNow;
                            partnerModified = true;
                            focalPointUpdates++;
                            Console.WriteLine($"Cleared FocalPoint for Partner: {partnerIdentifier}");
                        }
                    }
                    // If SuggestedFocalPoint is not null, update only if the FocalPoint in database is null OR matches LegacyFocalPoint
                    else
                    {
                        // Check if current focal point matches legacy
                        int? legacyUserId = data.LegacyFocalPointUser != null ? paoUserMapping.ContainsKey(data.LegacyFocalPointUser) 
                            ? paoUserMapping[data.LegacyFocalPointUser] 
                            : (int?)null : (int?)null;

                        if (partner.PartnerFocalPointUserId == null || partner.PartnerFocalPointUserId == legacyUserId)
                        {
                            // Get the suggested focal point user ID
                            if (paoUserMapping.ContainsKey(data.SuggestedFocalPoint))
                            {
                                int suggestedUserId = paoUserMapping[data.SuggestedFocalPoint];
                                partner.PartnerFocalPointUserId = suggestedUserId;
                                partner.LastModifiedBy = -1; // System user
                                partner.LastModifiedDate = DateTime.UtcNow;
                                partnerModified = true;
                                focalPointUpdates++;
                                Console.WriteLine($"Updated FocalPoint for Partner: {partnerIdentifier} from '{data.LegacyFocalPointUser}' to '{data.SuggestedFocalPoint}'");
                            }
                            else
                            {
                                Console.WriteLine($"Warning: SuggestedFocalPoint user '{data.SuggestedFocalPoint}' not found for Partner: {partnerIdentifier}");
                            }
                        }
                    }

                    // ========== ORGANIZATION UNIT LOGIC ==========

                    // If SuggestedOrgUnit is null, delete the relationship
                    if (string.IsNullOrEmpty(data.SuggestedOrgUnit))
                    {
                        if (!string.IsNullOrEmpty(data.LegacyOrgUnit) && orgUnitMapping.ContainsKey(data.LegacyOrgUnit))
                        {
                            int legacyOrgUnitId = orgUnitMapping[data.LegacyOrgUnit];
                            var relationshipsToRemove = partner.OrganizationUnitRelationships
                                .Where(r => r.OrganizationHierarchyId == legacyOrgUnitId && !r.IsDeleted)
                                .ToList();

                            foreach (var relationship in relationshipsToRemove)
                            {
                                relationship.IsDeleted = true;
                                relationship.DeletedBy = -1;
                                relationship.DeletedDate = DateTime.UtcNow;
                                orgUnitDeletions++;
                                Console.WriteLine($"Deleted OrgUnit relationship '{data.LegacyOrgUnit}' for Partner: {partnerIdentifier}");
                            }

                            if (relationshipsToRemove.Any())
                            {
                                partnerModified = true;
                            }
                        }
                    }
                    // If SuggestedOrgUnit is not null update or create OrganizationUnitRelationship
                    else
                    // && data.SuggestedOrgUnit != data.LegacyOrgUnit)
                    {
                        if (!orgUnitMapping.ContainsKey(data.SuggestedOrgUnit))
                        {
                            Console.WriteLine($"Warning: SuggestedOrgUnit '{data.SuggestedOrgUnit}' not found for Partner: {partnerIdentifier}");
                            continue;
                        }

                        int suggestedOrgUnitId = orgUnitMapping[data.SuggestedOrgUnit];

                        // Check if there's an existing relationship to update
                        OrganizationUnitRelationship? existingRelationship = null;
                        
                        if (!string.IsNullOrEmpty(data.LegacyOrgUnit) && orgUnitMapping.ContainsKey(data.LegacyOrgUnit))
                        {
                            int legacyOrgUnitId = orgUnitMapping[data.LegacyOrgUnit];
                            existingRelationship = partner.OrganizationUnitRelationships
                                .FirstOrDefault(r => r.OrganizationHierarchyId == legacyOrgUnitId && !r.IsDeleted);
                        }

                        if (existingRelationship != null)
                        {
                            // Update existing relationship
                            existingRelationship.OrganizationHierarchyId = suggestedOrgUnitId;
                            existingRelationship.Name = $"Partner-{partner.Id}-{suggestedOrgUnitId}";
                            existingRelationship.LastModifiedBy = -1;
                            existingRelationship.LastModifiedDate = DateTime.UtcNow;
                            orgUnitUpdates++;
                            Console.WriteLine($"Updated OrgUnit for Partner: {partnerIdentifier} from '{data.LegacyOrgUnit}' to '{data.SuggestedOrgUnit}'");
                        }
                        else
                        {
                            // Check if relationship already exists for the suggested org unit
                            var alreadyExists = partner.OrganizationUnitRelationships
                                .Any(r => r.OrganizationHierarchyId == suggestedOrgUnitId && !r.IsDeleted);

                            if (!alreadyExists)
                            {
                                // Create new relationship
                                var newRelationship = new OrganizationUnitRelationship
                                {
                                    OrganizationHierarchyId = suggestedOrgUnitId,
                                    EntityId = partner.Id,
                                    EntityType = nameof(Partner),
                                    Name = $"Partner-{partner.Id}-{suggestedOrgUnitId}",
                                    Status = EntityStatus.Active,
                                    CreatedBy = -1,
                                    CreatedDate = DateTime.UtcNow,
                                    LastModifiedBy = -1,
                                    LastModifiedDate = DateTime.UtcNow
                                };
                                context.OrganizationUnitRelationships.Add(newRelationship);
                                orgUnitUpdates++;
                                Console.WriteLine($"Created new OrgUnit relationship '{data.SuggestedOrgUnit}' for Partner: {partnerIdentifier}");
                            }
                        }
                        
                        partnerModified = true;
                    }

                    if (partnerModified)
                    {
                        partnersProcessed++;
                    }
                }

                // Save all changes at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"\\nPartner FocalPoint and OrgUnit updates completed successfully.");
                Console.WriteLine($"Partners processed: {partnersProcessed}");
                Console.WriteLine($"FocalPoint updates: {focalPointUpdates}");
                Console.WriteLine($"OrgUnit updates/creations: {orgUnitUpdates}");
                Console.WriteLine($"OrgUnit deletions: {orgUnitDeletions}");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating Partner FocalPoints and OrgUnits: {ex.Message}");
                throw;
            }
        }

        private class PartnerUpdateData
        {
            public int? ErpDimValue { get; set; }
            public string? Name { get; set; }
            public string? LegacyFocalPointUser { get; set; }
            public string? SuggestedFocalPoint { get; set; }
            public string? LegacyOrgUnit { get; set; }
            public string? SuggestedOrgUnit { get; set; }
        }
    }
}
"""

    return code

def main():
    # Define paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_path = os.path.join(script_dir, 'SF_Partner_AccountOwnerDepartment - Sheet4.csv')
    output_path = os.path.join(
        script_dir, '..', '..', '..', '..', 
        'UNOPS.PAO.UNOPSDataAccess', 'Seed', 'Seeders', 
        'Partner_FocalPoint_OrgUnit_Fixes_v3.cs'
    )
    
    # Check if CSV exists
    if not os.path.exists(csv_path):
        print(f"Error: CSV file not found at {csv_path}")
        return
    
    print(f"Reading CSV from: {csv_path}")
    csv_data = parse_csv_file(csv_path)
    print(f"Parsed {len(csv_data)} rows from CSV")
    
    print("Generating C# seeder code...")
    seeder_code = generate_seeder_code(csv_data)
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    
    print(f"Writing seeder to: {output_path}")
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(seeder_code)
    
    print("[SUCCESS] Seeder file generated successfully!")
    print(f"   Output: {output_path}")

if __name__ == '__main__':
    main()

