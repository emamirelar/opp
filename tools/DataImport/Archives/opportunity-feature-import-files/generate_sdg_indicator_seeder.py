"""
Python script to generate SDGIndicatorSeeder.cs from CSV data
Reads: SDG Targets and Indicators.xlsx - Indicator.csv
Outputs: SDGIndicatorSeeder.cs
"""

import csv
import os

def extract_target_id(indicator_id):
    """
    Extract SDG Target ID from Indicator ID
    Example: "1.1.1" -> "1.1", "3.3.2" -> "3.3"
    """
    parts = indicator_id.split('.')
    if len(parts) >= 2:
        return f"{parts[0]}.{parts[1]}"
    return indicator_id

def escape_csharp_string(text):
    """
    Escape special characters for C# string literals
    """
    if not text:
        return ""
    # Escape backslashes first
    text = text.replace('\\', '\\\\')
    # Escape double quotes
    text = text.replace('"', '\\"')
    return text

def generate_seeder():
    """
    Generate SDGIndicatorSeeder.cs from CSV data
    """
    
    # File paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_file = os.path.join(script_dir, "SDG Targets and Indicators.xlsx - Indicator.csv")
    output_file = os.path.join(script_dir, "..", "..", "..", "..", 
                                "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", "SDGIndicatorSeeder.cs")
    
    # Read CSV data
    indicators = []
    with open(csv_file, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            indicator_id = row['sdg_indicator_id'].strip()
            long_description = row['sdg_indicator_long_description'].strip()
            target_id = extract_target_id(indicator_id)
            
            indicators.append({
                'indicator_id': indicator_id,
                'target_id': target_id,
                'long_description': long_description
            })
    
    print(f"[OK] Read {len(indicators)} indicators from CSV")
    
    # Generate C# code
    lines = []
    
    # Add file header
    lines.append("using Microsoft.EntityFrameworkCore;")
    lines.append("using UNOPS.PAO.Domain.Entities;")
    lines.append("using UNOPS.PAO.Domain.Enums;")
    lines.append("using UNOPS.PAO.UNOPSDataAccess.Context;")
    lines.append("")
    lines.append("namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;")
    lines.append("")
    lines.append("/// <summary>")
    lines.append("/// Seeds SDG Indicators with proper insert/update logic")
    lines.append("/// </summary>")
    lines.append("public static class SDGIndicatorSeeder")
    lines.append("{")
    
    # Add SeedSDGIndicatorsAsync method
    lines.append("    public static async Task SeedSDGIndicatorsAsync(UNOPSAppDbContext context)")
    lines.append("    {")
    lines.append('        Console.WriteLine("🔄 Seeding SDG Indicators...");')
    lines.append("")
    lines.append("        var indicatorsToSeed = GetSDGIndicatorsToSeed();")
    lines.append("")
    lines.append("        // Get existing SDG Indicators from database")
    lines.append("        var existingIndicators = await context.SDGIndicators.ToListAsync();")
    lines.append("")
    lines.append("        var indicatorIdsToKeep = indicatorsToSeed.Select(i => i.SDGIndicatorId).ToHashSet();")
    lines.append("")
    lines.append("        // Insert or Update SDG Indicators")
    lines.append("        foreach (var indicatorData in indicatorsToSeed)")
    lines.append("        {")
    lines.append("            var existingIndicator = existingIndicators.FirstOrDefault(i => i.SDGIndicatorId == indicatorData.SDGIndicatorId);")
    lines.append("")
    lines.append("            if (existingIndicator == null)")
    lines.append("            {")
    lines.append("                // Insert new SDG Indicator")
    lines.append("                context.SDGIndicators.Add(indicatorData);")
    lines.append('                Console.WriteLine($"  ✅ Inserted SDG Indicator: {indicatorData.SDGIndicatorId}");')
    lines.append("            }")
    lines.append("            else")
    lines.append("            {")
    lines.append("                // Update if any properties changed")
    lines.append("                bool hasChanges = false;")
    lines.append("")
    lines.append("                if (existingIndicator.Name != indicatorData.Name)")
    lines.append("                {")
    lines.append("                    existingIndicator.Name = indicatorData.Name;")
    lines.append("                    hasChanges = true;")
    lines.append("                }")
    lines.append("")
    lines.append("                if (existingIndicator.SDGTargetId != indicatorData.SDGTargetId)")
    lines.append("                {")
    lines.append("                    existingIndicator.SDGTargetId = indicatorData.SDGTargetId;")
    lines.append("                    hasChanges = true;")
    lines.append("                }")
    lines.append("")
    lines.append("                if (existingIndicator.SDGIndicatorLongDescription != indicatorData.SDGIndicatorLongDescription)")
    lines.append("                {")
    lines.append("                    existingIndicator.SDGIndicatorLongDescription = indicatorData.SDGIndicatorLongDescription;")
    lines.append("                    hasChanges = true;")
    lines.append("                }")
    lines.append("")
    lines.append("                if (existingIndicator.Status != indicatorData.Status)")
    lines.append("                {")
    lines.append("                    existingIndicator.Status = indicatorData.Status;")
    lines.append("                    hasChanges = true;")
    lines.append("                }")
    lines.append("")
    lines.append("                if (existingIndicator.IsDeleted)")
    lines.append("                {")
    lines.append("                    existingIndicator.IsDeleted = false;")
    lines.append("                    hasChanges = true;")
    lines.append("                }")
    lines.append("")
    lines.append("                if (hasChanges)")
    lines.append("                {")
    lines.append('                    Console.WriteLine($"  🔄 Updated SDG Indicator: {indicatorData.SDGIndicatorId}");')
    lines.append("                }")
    lines.append("                else")
    lines.append("                {")
    lines.append('                    Console.WriteLine($"  ⏭️  Skipped SDG Indicator (unchanged): {indicatorData.SDGIndicatorId}");')
    lines.append("                }")
    lines.append("            }")
    lines.append("        }")
    lines.append("")
    lines.append("        // Delete SDG Indicators that are no longer in the seed list")
    lines.append("        var indicatorsToDelete = existingIndicators")
    lines.append("            .Where(i => !indicatorIdsToKeep.Contains(i.SDGIndicatorId))")
    lines.append("            .ToList();")
    lines.append("")
    lines.append("        foreach (var indicatorToDelete in indicatorsToDelete)")
    lines.append("        {")
    lines.append("            context.SDGIndicators.Remove(indicatorToDelete);")
    lines.append('            Console.WriteLine($"  🗑️  Deleted SDG Indicator: {indicatorToDelete.SDGIndicatorId}");')
    lines.append("        }")
    lines.append("")
    lines.append("        await context.SaveChangesAsync();")
    lines.append('        Console.WriteLine("✅ SDG Indicators seeding completed\\n");')
    lines.append("    }")
    lines.append("")
    
    # Add GetSDGIndicatorsToSeed method
    lines.append("    private static List<SDGIndicator> GetSDGIndicatorsToSeed()")
    lines.append("    {")
    lines.append("        return new List<SDGIndicator>")
    lines.append("        {")
    
    # Add indicator entities
    for i, indicator in enumerate(indicators):
        lines.append("            new SDGIndicator")
        lines.append("            {")
        lines.append(f'                Name = "{escape_csharp_string(indicator["indicator_id"])}",')
        lines.append("                Status = EntityStatus.Active,")
        lines.append("                IsDeleted = false,")
        lines.append(f'                SDGIndicatorId = "{escape_csharp_string(indicator["indicator_id"])}",')
        lines.append(f'                SDGTargetId = "{escape_csharp_string(indicator["target_id"])}",')
        lines.append(f'                SDGIndicatorLongDescription = "{escape_csharp_string(indicator["long_description"])}"')
        
        # Add closing brace with or without comma
        if i < len(indicators) - 1:
            lines.append("            },")
        else:
            lines.append("            }")
    
    lines.append("        };")
    lines.append("    }")
    lines.append("}")
    
    # Write output file
    output_content = '\n'.join(lines)
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(output_content)
    
    print(f"[OK] Generated seeder file: {output_file}")
    print(f"[INFO] Total indicators: {len(indicators)}")
    
    # Print sample of generated data
    print("\n[INFO] Sample indicators:")
    for i in range(min(5, len(indicators))):
        ind = indicators[i]
        print(f"  - {ind['indicator_id']} (Target: {ind['target_id']})")

if __name__ == "__main__":
    print("[START] Starting SDG Indicator Seeder Generator...")
    print("=" * 60)
    generate_seeder()
    print("=" * 60)
    print("[OK] Generation complete!")

