#!/usr/bin/env python3
"""
Generate C# Seeder for UNCFOutcome Entity from CSV Data
"""

import csv
import os
from datetime import datetime


def escape_csharp_string(value):
    """Escape special characters for C# string literals"""
    if value is None:
        return "null"
    
    # Replace backslash first, then quotes
    value = str(value).replace('\\', '\\\\')
    value = value.replace('"', '\\"')
    value = value.replace('\n', '\\n')
    value = value.replace('\r', '\\r')
    value = value.replace('\t', '\\t')
    
    return f'"{value}"'


def parse_datetime(date_str):
    """Parse datetime string from CSV"""
    if not date_str or date_str.strip() == '':
        return None
    
    try:
        # Parse format: "2023-03-03 09:32:39.01 UTC"
        date_str = date_str.strip()
        if ' UTC' in date_str:
            date_str = date_str.replace(' UTC', '')
        
        # Try parsing with microseconds
        try:
            dt = datetime.strptime(date_str, "%Y-%m-%d %H:%M:%S.%f")
        except ValueError:
            dt = datetime.strptime(date_str, "%Y-%m-%d %H:%M:%S")
        
        return dt
    except Exception as e:
        print(f"Warning: Could not parse date '{date_str}': {e}")
        return None


def format_csharp_datetime(dt):
    """Format datetime for C# DateTime constructor"""
    if dt is None:
        return "null"
    
    # Format: new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc)
    ms = dt.microsecond // 1000
    return f"new DateTime({dt.year}, {dt.month}, {dt.day}, {dt.hour}, {dt.minute}, {dt.second}, {ms}, DateTimeKind.Utc)"


def generate_seeder_code(csv_file_path, output_file_path):
    """Generate C# seeder code from CSV file"""
    
    print(f"Reading CSV file: {csv_file_path}")
    
    # Read CSV data
    outcomes = []
    with open(csv_file_path, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            outcome_id = row['outcome_id'].strip()
            outcome_name = row['outcome'].strip()
            version_no = row['version_no'].strip()
            country = row['country'].strip()
            last_update = parse_datetime(row['last_update'])
            
            outcomes.append({
                'outcome_id': outcome_id,
                'name': outcome_name,
                'version_no': version_no,
                'country': country,
                'last_update': last_update
            })
    
    print(f"Loaded {len(outcomes)} outcomes from CSV")
    
    # Generate C# code
    cs_code = []
    
    # Header
    cs_code.append("using Microsoft.EntityFrameworkCore;")
    cs_code.append("using UNOPS.PAO.Domain.Entities;")
    cs_code.append("using UNOPS.PAO.Domain.Enums;")
    cs_code.append("using UNOPS.PAO.UNOPSDataAccess.Context;")
    cs_code.append("")
    cs_code.append("namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;")
    cs_code.append("")
    cs_code.append("/// <summary>")
    cs_code.append("/// Seeds UNCF Outcomes (UN Cooperation Framework Outcomes) with proper insert/update logic")
    cs_code.append("/// Data synced from External Data Service (ERP Database)")
    cs_code.append("/// </summary>")
    cs_code.append("public static class UNCFOutcomeSeeder")
    cs_code.append("{")
    cs_code.append("    public static async Task SeedUNCFOutcomesAsync(UNOPSAppDbContext context)")
    cs_code.append("    {")
    cs_code.append('        Console.WriteLine("🔄 Seeding UNCF Outcomes...");')
    cs_code.append("")
    cs_code.append("        var outcomesToSeed = GetUNCFOutcomesToSeed();")
    cs_code.append("")
    cs_code.append("        // Get existing UNCF Outcomes from database")
    cs_code.append("        var existingOutcomes = await context.Set<UNCFOutcome>().ToListAsync();")
    cs_code.append("")
    cs_code.append("        // Track outcome identifiers to keep")
    cs_code.append("        var outcomeKeysToKeep = outcomesToSeed")
    cs_code.append("            .Select(o => new { o.UNCFOutcomeId, o.UNCooperationFrameworkVersionNo })")
    cs_code.append("            .ToHashSet();")
    cs_code.append("")
    cs_code.append("        // Insert or Update UNCF Outcomes")
    cs_code.append("        foreach (var outcomeData in outcomesToSeed)")
    cs_code.append("        {")
    cs_code.append("            var existingOutcome = existingOutcomes.FirstOrDefault(o =>")
    cs_code.append("                o.UNCFOutcomeId == outcomeData.UNCFOutcomeId &&")
    cs_code.append("                o.UNCooperationFrameworkVersionNo == outcomeData.UNCooperationFrameworkVersionNo);")
    cs_code.append("")
    cs_code.append("            if (existingOutcome == null)")
    cs_code.append("            {")
    cs_code.append("                // Insert new UNCF Outcome")
    cs_code.append("                context.Set<UNCFOutcome>().Add(outcomeData);")
    cs_code.append('                Console.WriteLine($"  ✅ Inserted UNCF Outcome: {outcomeData.Country} v{outcomeData.UNCooperationFrameworkVersionNo} - {outcomeData.UNCFOutcomeId}");')
    cs_code.append("            }")
    cs_code.append("            else")
    cs_code.append("            {")
    cs_code.append("                // Update if any properties changed")
    cs_code.append("                bool hasChanges = false;")
    cs_code.append("")
    cs_code.append("                if (existingOutcome.Name != outcomeData.Name)")
    cs_code.append("                {")
    cs_code.append("                    existingOutcome.Name = outcomeData.Name;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingOutcome.Country != outcomeData.Country)")
    cs_code.append("                {")
    cs_code.append("                    existingOutcome.Country = outcomeData.Country;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingOutcome.UNCFOutcomeLastUpdatedDate != outcomeData.UNCFOutcomeLastUpdatedDate)")
    cs_code.append("                {")
    cs_code.append("                    existingOutcome.UNCFOutcomeLastUpdatedDate = outcomeData.UNCFOutcomeLastUpdatedDate;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingOutcome.Status != outcomeData.Status)")
    cs_code.append("                {")
    cs_code.append("                    existingOutcome.Status = outcomeData.Status;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingOutcome.IsDeleted)")
    cs_code.append("                {")
    cs_code.append("                    existingOutcome.IsDeleted = false;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (hasChanges)")
    cs_code.append("                {")
    cs_code.append('                    Console.WriteLine($"  🔄 Updated UNCF Outcome: {outcomeData.Country} v{outcomeData.UNCooperationFrameworkVersionNo} - {outcomeData.UNCFOutcomeId}");')
    cs_code.append("                }")
    cs_code.append("                else")
    cs_code.append("                {")
    cs_code.append('                    Console.WriteLine($"  ⏭️  Skipped UNCF Outcome (unchanged): {outcomeData.Country} v{outcomeData.UNCooperationFrameworkVersionNo} - {outcomeData.UNCFOutcomeId}");')
    cs_code.append("                }")
    cs_code.append("            }")
    cs_code.append("        }")
    cs_code.append("")
    cs_code.append("        await context.SaveChangesAsync();")
    cs_code.append('        Console.WriteLine($"✅ UNCF Outcomes seeding completed - Total: {outcomesToSeed.Count}\\n");')
    cs_code.append("    }")
    cs_code.append("")
    cs_code.append("    private static List<UNCFOutcome> GetUNCFOutcomesToSeed()")
    cs_code.append("    {")
    cs_code.append("        return new List<UNCFOutcome>")
    cs_code.append("        {")
    
    # Generate outcome entries
    for idx, outcome in enumerate(outcomes):
        is_last = (idx == len(outcomes) - 1)
        
        cs_code.append("            new UNCFOutcome")
        cs_code.append("            {")
        cs_code.append(f"                Name = {escape_csharp_string(outcome['name'])},")
        cs_code.append("                Status = EntityStatus.Active,")
        cs_code.append("                IsDeleted = false,")
        cs_code.append(f"                UNCFOutcomeId = {escape_csharp_string(outcome['outcome_id'])},")
        
        # Parse version_no as integer
        try:
            version_no = int(outcome['version_no'])
            cs_code.append(f"                UNCooperationFrameworkVersionNo = {version_no},")
        except ValueError:
            cs_code.append(f"                UNCooperationFrameworkVersionNo = null,")
        
        cs_code.append(f"                Country = {escape_csharp_string(outcome['country'])},")
        cs_code.append(f"                UNCFOutcomeLastUpdatedDate = {format_csharp_datetime(outcome['last_update'])}")
        
        if is_last:
            cs_code.append("            }")
        else:
            cs_code.append("            },")
    
    # Footer
    cs_code.append("        };")
    cs_code.append("    }")
    cs_code.append("}")
    cs_code.append("")
    
    # Write to output file
    print(f"Writing C# seeder to: {output_file_path}")
    with open(output_file_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(cs_code))
    
    print(f"[OK] Successfully generated seeder with {len(outcomes)} UNCF Outcomes")
    return len(outcomes)


def safe_print(text):
    """Print text with fallback for Unicode encoding issues"""
    try:
        print(text)
    except UnicodeEncodeError:
        # Fall back to ASCII-safe output
        print(text.encode('ascii', 'replace').decode('ascii'))


def main():
    """Main execution function"""
    
    # Get paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_file = os.path.join(script_dir, "UNSDCF Outcomes - OppPlusImport.csv")
    
    # Output to Seeders folder
    # script_dir is: .../tools/DataImport/Archives/opportunity-feature-import-files
    # Need to go up 4 levels to get to project root
    project_root = os.path.abspath(os.path.join(script_dir, '..', '..', '..', '..'))
    seeders_folder = os.path.join(
        project_root,
        'UNOPS.PAO.UNOPSDataAccess',
        'Seed',
        'Seeders'
    )
    output_file = os.path.join(seeders_folder, "UNCFOutcomeSeeder.cs")
    
    # Validate CSV file exists
    if not os.path.exists(csv_file):
        safe_print(f"[X] Error: CSV file not found: {csv_file}")
        return 1
    
    # Ensure output directory exists
    os.makedirs(seeders_folder, exist_ok=True)
    
    # Generate seeder
    try:
        outcome_count = generate_seeder_code(csv_file, output_file)
        safe_print("\n" + "="*60)
        safe_print("[OK] Seeder generation completed successfully!")
        safe_print(f"   Total outcomes: {outcome_count}")
        safe_print(f"   Output file: {output_file}")
        safe_print("="*60)
        return 0
    except Exception as e:
        safe_print(f"\n[ERROR] Error generating seeder: {e}")
        import traceback
        traceback.print_exc()
        return 1


if __name__ == "__main__":
    exit(main())

