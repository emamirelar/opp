#!/usr/bin/env python3
"""
Python script to generate CountryAndOrgUnitRelationshipSeeder.cs
Reads CountryAndOrgUnitRelationshipSeeder.csv and creates a C# seeder file
that establishes relationships between Country entities and OrganizationHierarchy entities.
"""

import csv
import os
from datetime import datetime

def escape_csharp_string(text):
    """Escape special characters for C# string literals"""
    if text is None:
        return ""
    return text.replace("\\", "\\\\").replace("\"", "\\\"")

def generate_seeder_file():
    # Get the directory where this script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_file_path = os.path.join(script_dir, "CountryAndOrgUnitRelationshipSeeder.csv")
    
    # Output file path (in the Seeders folder)
    output_file_path = os.path.join(
        script_dir, 
        "..", "..", "..", "..", 
        "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", 
        "CountryAndOrgUnitRelationshipSeeder.cs"
    )
    
    # Normalize the path
    output_file_path = os.path.normpath(output_file_path)
    
    # Read the CSV file
    relationships = []
    with open(csv_file_path, 'r', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            iso3 = row['ISO3'].strip()
            country_name = row['Country'].strip()
            org_unit = row['Org Unit Responsible'].strip()
            
            # Skip rows without required data
            if not iso3 or not org_unit or iso3 == '-' or org_unit == '-':
                continue
                
            relationships.append({
                'iso3': iso3,
                'country': country_name,
                'org_unit': org_unit
            })
    
    print(f"[INFO] Parsed {len(relationships)} country-orgunit relationships from CSV")
    
    # Generate the C# seeder file
    seeder_content = generate_csharp_code(relationships)
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_file_path), exist_ok=True)
    
    # Write the seeder file
    with open(output_file_path, 'w', encoding='utf-8') as f:
        f.write(seeder_content)
    
    print(f"[SUCCESS] Generated seeder file: {output_file_path}")
    print(f"[INFO] Total relationships to seed: {len(relationships)}")

def generate_csharp_code(relationships):
    """Generate the C# seeder class code"""
    
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    # Build the list of tuples for the relationships
    relationship_tuples = []
    for rel in relationships:
        iso3 = escape_csharp_string(rel['iso3'])
        org_unit = escape_csharp_string(rel['org_unit'])
        relationship_tuples.append(f'                ("{iso3}", "{org_unit}")')
    
    relationships_code = ",\n".join(relationship_tuples)
    
    code = f'''using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds OrganizationUnitRelationships between Countries and OrganizationHierarchy units
/// Generated from CountryAndOrgUnitRelationshipSeeder.csv
/// Generated on: {timestamp}
/// </summary>
public static class CountryAndOrgUnitRelationshipSeeder
{{
    public static async Task SeedCountryOrgUnitRelationshipsAsync(UNOPSAppDbContext context)
    {{
        Console.WriteLine("🔄 Seeding Country-OrganizationUnit Relationships...");

        var relationshipsToSeed = GetRelationshipsToSeed();

        // Load all countries with Iso3Code
        var countries = await context.Set<Country>()
            .Where(c => !c.IsDeleted && c.Iso3Code != null)
            .ToDictionaryAsync(c => c.Iso3Code!, c => c);

        // Load all organization hierarchy units by Code
        var orgUnits = await context.Set<OrganizationHierarchy>()
            .Where(oh => !oh.IsDeleted && oh.Type == OrganizationUnitType.OrgUnit)
            .ToDictionaryAsync(oh => oh.Code, oh => oh);

        // Load all existing relationships for Countries with EntityType "OrgUnit"
        var existingRelationships = await context.Set<OrganizationUnitRelationship>()
            .Where(r => r.EntityType == "Country" && !r.IsDeleted)
            .ToListAsync();

        int inserted = 0;
        int updated = 0;
        int skipped = 0;
        int notFoundCountry = 0;
        int notFoundOrgUnit = 0;

        foreach (var (iso3Code, orgUnitCode) in relationshipsToSeed)
        {{
            // Find the country
            if (!countries.TryGetValue(iso3Code, out var country))
            {{
                Console.WriteLine($"  ⚠️  Country not found for ISO3: {{iso3Code}}");
                notFoundCountry++;
                continue;
            }}

            // Find the organization unit
            if (!orgUnits.TryGetValue(orgUnitCode, out var orgUnit))
            {{
                Console.WriteLine($"  ⚠️  Organization Unit not found for Code: {{orgUnitCode}}");
                notFoundOrgUnit++;
                continue;
            }}

            // Check if relationship already exists
            var existingRel = existingRelationships.FirstOrDefault(r =>
                r.OrganizationHierarchyId == orgUnit.Id &&
                r.EntityId == country.Id &&
                r.EntityType == "Country");

            if (existingRel == null)
            {{
                // Create new relationship
                var relationshipName = $"Country-{{country.Id}}-{{orgUnit.Code}}";
                var newRelationship = new OrganizationUnitRelationship
                {{
                    Name = relationshipName,
                    OrganizationHierarchyId = orgUnit.Id,
                    EntityId = country.Id,
                    EntityType = "Country",
                    Status = EntityStatus.Active,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    DeletedBy = 0,
                    IsDeleted = false,
                }};

                context.Set<OrganizationUnitRelationship>().Add(newRelationship);
                Console.WriteLine($"  ✅ Inserted: {{country.Name}} ({{iso3Code}}) → {{orgUnit.Name}} ({{orgUnitCode}})");
                inserted++;
            }}
            else
            {{
                // Check if update is needed
                bool hasChanges = false;
                var expectedName = $"Country-{{country.Id}}-{{orgUnit.Code}}";

                if (existingRel.Name != expectedName)
                {{
                    existingRel.Name = expectedName;
                    hasChanges = true;
                }}

                if (existingRel.Status != EntityStatus.Active)
                {{
                    existingRel.Status = EntityStatus.Active;
                    hasChanges = true;
                }}

                if (existingRel.IsDeleted)
                {{
                    existingRel.IsDeleted = false;
                    hasChanges = true;
                }}

                if (hasChanges)
                {{
                    existingRel.LastModifiedBy = 1;
                    existingRel.LastModifiedDate = DateTime.UtcNow;
                    Console.WriteLine($"  🔄 Updated: {{country.Name}} ({{iso3Code}}) → {{orgUnit.Name}} ({{orgUnitCode}})");
                    updated++;
                }}
                else
                {{
                    skipped++;
                }}
            }}
        }}

        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Country-OrganizationUnit Relationships seeding completed");
        Console.WriteLine($"   📊 Statistics:");
        Console.WriteLine($"      ✅ Inserted: {{inserted}}");
        Console.WriteLine($"      🔄 Updated: {{updated}}");
        Console.WriteLine($"      ⏭️  Skipped (unchanged): {{skipped}}");
        if (notFoundCountry > 0)
            Console.WriteLine($"      ⚠️  Countries not found: {{notFoundCountry}}");
        if (notFoundOrgUnit > 0)
            Console.WriteLine($"      ⚠️  Organization Units not found: {{notFoundOrgUnit}}");
        Console.WriteLine();
    }}

    private static List<(string Iso3Code, string OrgUnitCode)> GetRelationshipsToSeed()
    {{
        return new List<(string, string)>
        {{
{relationships_code}
        }};
    }}
}}
'''
    
    return code

if __name__ == "__main__":
    try:
        generate_seeder_file()
        print("\n[SUCCESS] Seeder generation completed successfully!")
    except Exception as e:
        print(f"\n[ERROR] Error generating seeder: {e}")
        import traceback
        traceback.print_exc()
        exit(1)

