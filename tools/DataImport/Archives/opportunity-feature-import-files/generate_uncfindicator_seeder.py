#!/usr/bin/env python3
"""
Generate C# Seeder for UNCFIndicator Entity from CSV Data
"""

import csv
import os
from datetime import datetime


def escape_csharp_string(value):
    """Escape special characters for C# string literals"""
    if value is None or value == '' or value == '-':
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
    if not date_str or date_str.strip() == '' or date_str.strip() == '-':
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


def safe_print(text):
    """Print text with fallback for Unicode encoding issues"""
    try:
        print(text)
    except UnicodeEncodeError:
        # Fall back to ASCII-safe output
        print(text.encode('ascii', 'replace').decode('ascii'))


def generate_seeder_code(csv_file_path, output_file_path):
    """Generate C# seeder code from CSV file"""
    
    safe_print(f"Reading CSV file: {csv_file_path}")
    
    # Read CSV data
    indicators = []
    with open(csv_file_path, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            indicator_id = row['indicator_id'].strip()
            unit = row.get('unit', '').strip() if row.get('unit') else None
            description = row.get('description', '').strip() if row.get('description') else None
            start_date = parse_datetime(row.get('start_date'))
            end_date = parse_datetime(row.get('end_date'))
            indicators_text = row.get('indicators', '').strip() if row.get('indicators') else None
            baseline = row.get('baseline', '').strip() if row.get('baseline') else None
            narrative = row.get('narrative', '').strip() if row.get('narrative') else None
            version_no = row['version_no'].strip() if row.get('version_no') else None
            country = row['country'].strip()
            last_update = parse_datetime(row['last_update'])
            outcome_id = row['outcome_id'].strip()
            
            # Determine the name - use indicators text if available, otherwise description, otherwise indicator_id
            if indicators_text and indicators_text != '-':
                name = indicators_text
            elif description and description != '-':
                name = description
            else:
                name = f"Indicator {indicator_id}"
            
            # Truncate name if too long (max 1000 chars)
            if len(name) > 1000:
                name = name[:997] + "..."
            
            indicators.append({
                'indicator_id': indicator_id,
                'name': name,
                'unit': unit,
                'description': description,
                'start_date': start_date,
                'end_date': end_date,
                'indicators': indicators_text,
                'baseline': baseline,
                'narrative': narrative,
                'version_no': version_no,
                'country': country,
                'last_update': last_update,
                'outcome_id': outcome_id
            })
    
    safe_print(f"Loaded {len(indicators)} indicators from CSV")
    
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
    cs_code.append("/// Seeds UNCF Indicators (UN Cooperation Framework Indicators) with proper insert/update logic")
    cs_code.append("/// Data synced from External Data Service (ERP Database)")
    cs_code.append("/// </summary>")
    cs_code.append("public static class UNCFIndicatorSeeder")
    cs_code.append("{")
    cs_code.append("    public static async Task SeedUNCFIndicatorsAsync(UNOPSAppDbContext context)")
    cs_code.append("    {")
    cs_code.append('        Console.WriteLine("🔄 Seeding UNCF Indicators...");')
    cs_code.append("")
    cs_code.append("        var indicatorsToSeed = GetUNCFIndicatorsToSeed();")
    cs_code.append("")
    cs_code.append("        // Get existing UNCF Indicators from database")
    cs_code.append("        var existingIndicators = await context.Set<UNCFIndicator>().ToListAsync();")
    cs_code.append("")
    cs_code.append("        // Track indicator identifiers to keep")
    cs_code.append("        var indicatorKeysToKeep = indicatorsToSeed")
    cs_code.append("            .Select(i => i.UNCFIndicatorId)")
    cs_code.append("            .ToHashSet();")
    cs_code.append("")
    cs_code.append("        // Insert or Update UNCF Indicators")
    cs_code.append("        foreach (var indicatorData in indicatorsToSeed)")
    cs_code.append("        {")
    cs_code.append("            var existingIndicator = existingIndicators.FirstOrDefault(i =>")
    cs_code.append("                i.UNCFIndicatorId == indicatorData.UNCFIndicatorId);")
    cs_code.append("")
    cs_code.append("            if (existingIndicator == null)")
    cs_code.append("            {")
    cs_code.append("                // Insert new UNCF Indicator")
    cs_code.append("                context.Set<UNCFIndicator>().Add(indicatorData);")
    cs_code.append('                Console.WriteLine($"  ✅ Inserted UNCF Indicator: {indicatorData.UNCFIndicatorId} - {indicatorData.Country}");')
    cs_code.append("            }")
    cs_code.append("            else")
    cs_code.append("            {")
    cs_code.append("                // Update if any properties changed")
    cs_code.append("                bool hasChanges = false;")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Name != indicatorData.Name)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Name = indicatorData.Name;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Unit != indicatorData.Unit)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Unit = indicatorData.Unit;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Description != indicatorData.Description)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Description = indicatorData.Description;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Indicators != indicatorData.Indicators)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Indicators = indicatorData.Indicators;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Baseline != indicatorData.Baseline)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Baseline = indicatorData.Baseline;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Narrative != indicatorData.Narrative)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Narrative = indicatorData.Narrative;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Country != indicatorData.Country)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Country = indicatorData.Country;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.UNCFOutcomeExternalId != indicatorData.UNCFOutcomeExternalId)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.UNCFOutcomeExternalId = indicatorData.UNCFOutcomeExternalId;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.UNCFIndicatorStartDate != indicatorData.UNCFIndicatorStartDate)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.UNCFIndicatorStartDate = indicatorData.UNCFIndicatorStartDate;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.UNCFIndicatorEndDate != indicatorData.UNCFIndicatorEndDate)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.UNCFIndicatorEndDate = indicatorData.UNCFIndicatorEndDate;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.UNCFIndicatorLastUpdatedDate != indicatorData.UNCFIndicatorLastUpdatedDate)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.UNCFIndicatorLastUpdatedDate = indicatorData.UNCFIndicatorLastUpdatedDate;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.Status != indicatorData.Status)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.Status = indicatorData.Status;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingIndicator.IsDeleted)")
    cs_code.append("                {")
    cs_code.append("                    existingIndicator.IsDeleted = false;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (hasChanges)")
    cs_code.append("                {")
    cs_code.append('                    Console.WriteLine($"  🔄 Updated UNCF Indicator: {indicatorData.UNCFIndicatorId} - {indicatorData.Country}");')
    cs_code.append("                }")
    cs_code.append("                else")
    cs_code.append("                {")
    cs_code.append('                    Console.WriteLine($"  ⏭️  Skipped UNCF Indicator (unchanged): {indicatorData.UNCFIndicatorId} - {indicatorData.Country}");')
    cs_code.append("                }")
    cs_code.append("            }")
    cs_code.append("        }")
    cs_code.append("")
    cs_code.append("        await context.SaveChangesAsync();")
    cs_code.append('        Console.WriteLine($"✅ UNCF Indicators seeding completed - Total: {indicatorsToSeed.Count}\\n");')
    cs_code.append("    }")
    cs_code.append("")
    cs_code.append("    private static List<UNCFIndicator> GetUNCFIndicatorsToSeed()")
    cs_code.append("    {")
    cs_code.append("        return new List<UNCFIndicator>")
    cs_code.append("        {")
    
    # Generate indicator entries
    for idx, indicator in enumerate(indicators):
        is_last = (idx == len(indicators) - 1)
        
        cs_code.append("            new UNCFIndicator")
        cs_code.append("            {")
        cs_code.append(f"                Name = {escape_csharp_string(indicator['name'])},")
        cs_code.append("                Status = EntityStatus.Active,")
        cs_code.append("                IsDeleted = false,")
        cs_code.append(f"                UNCFIndicatorId = {escape_csharp_string(indicator['indicator_id'])},")
        cs_code.append(f"                Unit = {escape_csharp_string(indicator['unit'])},")
        cs_code.append(f"                Description = {escape_csharp_string(indicator['description'])},")
        cs_code.append(f"                UNCFIndicatorStartDate = {format_csharp_datetime(indicator['start_date'])},")
        cs_code.append(f"                UNCFIndicatorEndDate = {format_csharp_datetime(indicator['end_date'])},")
        cs_code.append(f"                Indicators = {escape_csharp_string(indicator['indicators'])},")
        cs_code.append(f"                Baseline = {escape_csharp_string(indicator['baseline'])},")
        cs_code.append(f"                Narrative = {escape_csharp_string(indicator['narrative'])},")
        
        # Parse version_no as integer
        try:
            version_no = int(indicator['version_no']) if indicator['version_no'] else None
            if version_no:
                cs_code.append(f"                UNCooperationFrameworkVersionNo = {version_no},")
            else:
                cs_code.append(f"                UNCooperationFrameworkVersionNo = null,")
        except (ValueError, TypeError):
            cs_code.append(f"                UNCooperationFrameworkVersionNo = null,")
        
        cs_code.append(f"                UNCFOutcomeExternalId = {escape_csharp_string(indicator['outcome_id'])},")
        cs_code.append(f"                Country = {escape_csharp_string(indicator['country'])},")
        cs_code.append(f"                UNCFIndicatorLastUpdatedDate = {format_csharp_datetime(indicator['last_update'])}")
        
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
    safe_print(f"Writing C# seeder to: {output_file_path}")
    with open(output_file_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(cs_code))
    
    safe_print(f"[OK] Successfully generated seeder with {len(indicators)} UNCF Indicators")
    return len(indicators)


def main():
    """Main execution function"""
    
    # Get paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_file = os.path.join(script_dir, "UNSDCF Indicators - OppPlusImport.csv")
    
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
    output_file = os.path.join(seeders_folder, "UNCFIndicatorSeeder.cs")
    
    # Validate CSV file exists
    if not os.path.exists(csv_file):
        safe_print(f"[X] Error: CSV file not found: {csv_file}")
        return 1
    
    # Ensure output directory exists
    os.makedirs(seeders_folder, exist_ok=True)
    
    # Generate seeder
    try:
        indicator_count = generate_seeder_code(csv_file, output_file)
        safe_print("\n" + "="*60)
        safe_print("[OK] Seeder generation completed successfully!")
        safe_print(f"   Total indicators: {indicator_count}")
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

