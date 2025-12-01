#!/usr/bin/env python3
"""
Generate C# Seeder for UNCFMetadata Entity from CSV Data
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


def map_status(status_code):
    """Map status code to EntityStatus enum"""
    if status_code and status_code.strip().upper() == 'C':
        return 'EntityStatus.Inactive'
    else:
        return 'EntityStatus.Active'


def generate_seeder_code(csv_file_path, output_file_path):
    """Generate C# seeder code from CSV file"""
    
    print(f"Reading CSV file: {csv_file_path}")
    
    # Read CSV data
    metadata_records = []
    with open(csv_file_path, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            country = row['country'].strip()
            file_url = row['file_url'].strip()
            version_no = row['version_no'].strip()
            last_update = parse_datetime(row['last_update'])
            file_name = row['file_name'].strip()
            status = row['status'].strip()
            agrtid = row['agrtid'].strip()
            
            metadata_records.append({
                'country': country,
                'file_url': file_url,
                'version_no': version_no,
                'last_update': last_update,
                'file_name': file_name,
                'status': status,
                'agrtid': agrtid
            })
    
    print(f"Loaded {len(metadata_records)} metadata records from CSV")
    
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
    cs_code.append("/// Seeds UNCF Metadata (UN Cooperation Framework Metadata) with proper insert/update logic")
    cs_code.append("/// Data synced from External Data Service (ERP Database)")
    cs_code.append("/// </summary>")
    cs_code.append("public static class UNCFMetadataSeeder")
    cs_code.append("{")
    cs_code.append("    public static async Task SeedUNCFMetadataAsync(UNOPSAppDbContext context)")
    cs_code.append("    {")
    cs_code.append('        Console.WriteLine("🔄 Seeding UNCF Metadata...");')
    cs_code.append("")
    cs_code.append("        var metadataToSeed = GetUNCFMetadataToSeed();")
    cs_code.append("")
    cs_code.append("        // Get existing UNCF Metadata from database")
    cs_code.append("        var existingMetadata = await context.Set<UNCFMetadata>().ToListAsync();")
    cs_code.append("")
    cs_code.append("        // Track metadata identifiers to keep")
    cs_code.append("        var metadataKeysToKeep = metadataToSeed")
    cs_code.append("            .Select(m => new { m.Country, m.UNCooperationFrameworkVersionNo })")
    cs_code.append("            .ToHashSet();")
    cs_code.append("")
    cs_code.append("        // Insert or Update UNCF Metadata")
    cs_code.append("        foreach (var metadataData in metadataToSeed)")
    cs_code.append("        {")
    cs_code.append("            var existingRecord = existingMetadata.FirstOrDefault(m =>")
    cs_code.append("                m.Country == metadataData.Country &&")
    cs_code.append("                m.UNCooperationFrameworkVersionNo == metadataData.UNCooperationFrameworkVersionNo);")
    cs_code.append("")
    cs_code.append("            if (existingRecord == null)")
    cs_code.append("            {")
    cs_code.append("                // Insert new UNCF Metadata")
    cs_code.append("                context.Set<UNCFMetadata>().Add(metadataData);")
    cs_code.append('                Console.WriteLine($"  ✅ Inserted UNCF Metadata: {metadataData.Country} v{metadataData.UNCooperationFrameworkVersionNo}");')
    cs_code.append("            }")
    cs_code.append("            else")
    cs_code.append("            {")
    cs_code.append("                // Update if any properties changed")
    cs_code.append("                bool hasChanges = false;")
    cs_code.append("")
    cs_code.append("                if (existingRecord.Name != metadataData.Name)")
    cs_code.append("                {")
    cs_code.append("                    existingRecord.Name = metadataData.Name;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingRecord.UNCFMetadataId != metadataData.UNCFMetadataId)")
    cs_code.append("                {")
    cs_code.append("                    existingRecord.UNCFMetadataId = metadataData.UNCFMetadataId;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingRecord.UNCFFileURL != metadataData.UNCFFileURL)")
    cs_code.append("                {")
    cs_code.append("                    existingRecord.UNCFFileURL = metadataData.UNCFFileURL;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingRecord.UNCFFileName != metadataData.UNCFFileName)")
    cs_code.append("                {")
    cs_code.append("                    existingRecord.UNCFFileName = metadataData.UNCFFileName;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingRecord.UNCFLastUpdatedDate != metadataData.UNCFLastUpdatedDate)")
    cs_code.append("                {")
    cs_code.append("                    existingRecord.UNCFLastUpdatedDate = metadataData.UNCFLastUpdatedDate;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingRecord.Status != metadataData.Status)")
    cs_code.append("                {")
    cs_code.append("                    existingRecord.Status = metadataData.Status;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (existingRecord.IsDeleted)")
    cs_code.append("                {")
    cs_code.append("                    existingRecord.IsDeleted = false;")
    cs_code.append("                    hasChanges = true;")
    cs_code.append("                }")
    cs_code.append("")
    cs_code.append("                if (hasChanges)")
    cs_code.append("                {")
    cs_code.append('                    Console.WriteLine($"  🔄 Updated UNCF Metadata: {metadataData.Country} v{metadataData.UNCooperationFrameworkVersionNo}");')
    cs_code.append("                }")
    cs_code.append("                else")
    cs_code.append("                {")
    cs_code.append('                    Console.WriteLine($"  ⏭️  Skipped UNCF Metadata (unchanged): {metadataData.Country} v{metadataData.UNCooperationFrameworkVersionNo}");')
    cs_code.append("                }")
    cs_code.append("            }")
    cs_code.append("        }")
    cs_code.append("")
    cs_code.append("        await context.SaveChangesAsync();")
    cs_code.append('        Console.WriteLine($"✅ UNCF Metadata seeding completed - Total: {metadataToSeed.Count}\\n");')
    cs_code.append("    }")
    cs_code.append("")
    cs_code.append("    private static List<UNCFMetadata> GetUNCFMetadataToSeed()")
    cs_code.append("    {")
    cs_code.append("        return new List<UNCFMetadata>")
    cs_code.append("        {")
    
    # Generate metadata entries
    for idx, record in enumerate(metadata_records):
        is_last = (idx == len(metadata_records) - 1)
        
        # Create name from country and version
        name = f"{record['country']} v{record['version_no']}"
        
        cs_code.append("            new UNCFMetadata")
        cs_code.append("            {")
        cs_code.append(f"                Name = {escape_csharp_string(name)},")
        cs_code.append(f"                Status = {map_status(record['status'])},")
        cs_code.append("                IsDeleted = false,")
        
        # Parse agrtid as integer
        try:
            agrtid = int(record['agrtid'])
            cs_code.append(f"                UNCFMetadataId = {agrtid},")
        except ValueError:
            cs_code.append(f"                UNCFMetadataId = null,")
        
        cs_code.append(f"                Country = {escape_csharp_string(record['country'])},")
        cs_code.append(f"                UNCFFileURL = {escape_csharp_string(record['file_url'])},")
        
        # Parse version_no as integer
        try:
            version_no = int(record['version_no'])
            cs_code.append(f"                UNCooperationFrameworkVersionNo = {version_no},")
        except ValueError:
            cs_code.append(f"                UNCooperationFrameworkVersionNo = null,")
        
        cs_code.append(f"                UNCFLastUpdatedDate = {format_csharp_datetime(record['last_update'])},")
        cs_code.append(f"                UNCFFileName = {escape_csharp_string(record['file_name'])}")
        
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
    
    print(f"[OK] Successfully generated seeder with {len(metadata_records)} UNCF Metadata records")
    return len(metadata_records)


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
    csv_file = os.path.join(script_dir, "UNSDCF Metadata  - OppPlusImport.csv")
    
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
    output_file = os.path.join(seeders_folder, "UNCFMetadataSeeder.cs")
    
    # Validate CSV file exists
    if not os.path.exists(csv_file):
        safe_print(f"[X] Error: CSV file not found: {csv_file}")
        return 1
    
    # Ensure output directory exists
    os.makedirs(seeders_folder, exist_ok=True)
    
    # Generate seeder
    try:
        record_count = generate_seeder_code(csv_file, output_file)
        safe_print("\n" + "="*60)
        safe_print("[OK] Seeder generation completed successfully!")
        safe_print(f"   Total metadata records: {record_count}")
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


