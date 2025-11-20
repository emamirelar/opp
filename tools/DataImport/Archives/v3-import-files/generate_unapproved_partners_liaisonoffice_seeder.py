#!/usr/bin/env python3
"""
Script to generate Unapproved_Partners_LiaisonOffice_Fixes_v3.cs seeder
"""

def escape_csharp_string(value: str) -> str:
    """Escape special characters for C# string literals"""
    if value is None or value.strip() == "":
        return ""
    return value.replace('\\', '\\\\').replace('"', '\\"')

def generate_seeder_code():
    """Generate the C# seeder code"""
    
    # Partner Name to LiaisonOffice mapping
    partner_liaison_data = [
        ("AAIC Japan Co., Ltd.", "Tokyo Liaison Office"),
        ("AEF Africa-Europe Foundation", "Brussels Liaison Office"),
        ("Allm Inc.", "Tokyo Liaison Office"),
        ("British Virgin Islands", "Other Partners"),
        ("Camara de Comercio de Cortes", "Other Partners"),
        ("Carlsberg Group A/S", "Northern Europe Liaison Office"),
        ("Comunità Sant'Egidio", "Rome Liaison Office"),
        ("FPI - European Commission", "Brussels Liaison Office"),
        ("Hotel New Otani Tokyo", "Tokyo Liaison Office"),
        ("Human Practice Foundation", "Northern Europe Liaison Office"),
        ("IFU - Impact Fund Denmark", "Northern Europe Liaison Office"),
        ("Japan Embassy Conakry", "Tokyo Liaison Office"),
        ("Japan Embassy Guinea", "Tokyo Liaison Office"),
        ("Ministry of Climate, Energy and Utilities of Denmark", "Northern Europe Liaison Office"),
        ("Ministry of Economy, Trade and Industry (METI) Japan", "Tokyo Liaison Office"),
        ("Ministry of Foreign Affairs of Italy", "Brussels Liaison Office"),
        ("Ministry of Health, Labour and Welfare (MHLW) Japan", "Tokyo Liaison Office"),
        ("NEC Corporation", "Tokyo Liaison Office"),
        ("Nomura Research Institute (NRI)", "Tokyo Liaison Office"),
        ("RCO Mali", "Other Partners"),
        ("Secretaría de Relaciones Exteriores y Cooperación Internacional", "Other Partners"),
        ("Twinbird Corporation", "Tokyo Liaison Office"),
        ("UN in Rome", "Rome Liaison Office"),
        ("UN Integrated Strategy for the Sahel (UNISS)", "Other Partners"),
        ("Yamaha Motor Co., Ltd.", "Tokyo Liaison Office"),
    ]
    
    # Start building the C# file
    code = """using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Unapproved_Partners_LiaisonOffice_Fixes_v3
    {
        public static async Task UpdateUnapprovedPartnersLiaisonOfficeAsync(UNOPSAppDbContext context)
        {
            // Create mapping from LiaisonOffice Name to Id (handle duplicates by taking first, filter out null names)
            var liaisonOffices = await context.LiaisonOffices
                .Select(lo => new { lo.Id, lo.Name })
                .ToListAsync();
            var liaisonOfficeMapping = liaisonOffices
                .Where(lo => !string.IsNullOrEmpty(lo.Name))
                .GroupBy(lo => lo.Name)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Define partner to liaison office mapping
            var partnerLiaisonMappings = new Dictionary<string, string>
            {
"""
    
    # Add all mappings
    for idx, (partner_name, liaison_office) in enumerate(partner_liaison_data):
        escaped_name = escape_csharp_string(partner_name)
        escaped_liaison = escape_csharp_string(liaison_office)
        
        comma = "," if idx < len(partner_liaison_data) - 1 else ""
        code += f'                {{ "{escaped_name}", "{escaped_liaison}" }}{comma}\n'
    
    code += """            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                int partnersUpdated = 0;
                int partnersSkipped = 0;
                int partnersNotFound = 0;
                int liaisonOfficesNotFound = 0;

                foreach (var mapping in partnerLiaisonMappings)
                {
                    var partnerName = mapping.Key;
                    var liaisonOfficeName = mapping.Value;

                    // Check if liaison office exists
                    if (!liaisonOfficeMapping.ContainsKey(liaisonOfficeName))
                    {
                        Console.WriteLine($"Warning: LiaisonOffice '{liaisonOfficeName}' not found in database for Partner '{partnerName}'");
                        liaisonOfficesNotFound++;
                        continue;
                    }

                    var liaisonOfficeId = liaisonOfficeMapping[liaisonOfficeName];

                    // Find partner by Name where ErpDimValue is null
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == partnerName && p.ErpDimValue == null);

                    if (partner == null)
                    {
                        Console.WriteLine($"Warning: Partner '{partnerName}' (with ErpDimValue = null) not found in database");
                        partnersNotFound++;
                        continue;
                    }

                    // Only update if LiaisonOfficeId is not already set
                    if (partner.LiaisonOfficeId == null)
                    {
                        partner.LiaisonOfficeId = liaisonOfficeId;
                        partner.LastModifiedBy = -1; // System user
                        partner.LastModifiedDate = DateTime.UtcNow;
                        partnersUpdated++;
                        Console.WriteLine($"Updated Partner '{partnerName}' with LiaisonOffice '{liaisonOfficeName}' (ID: {liaisonOfficeId})");
                    }
                    else
                    {
                        partnersSkipped++;
                        Console.WriteLine($"Skipped Partner '{partnerName}' - LiaisonOfficeId already set (ID: {partner.LiaisonOfficeId})");
                    }
                }

                // Save all changes at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"\\nUnapproved Partners LiaisonOffice updates completed successfully.");
                Console.WriteLine($"Partners updated: {partnersUpdated}");
                Console.WriteLine($"Partners skipped (already set): {partnersSkipped}");
                Console.WriteLine($"Partners not found: {partnersNotFound}");
                Console.WriteLine($"Liaison offices not found: {liaisonOfficesNotFound}");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating unapproved partners liaison offices: {ex.Message}");
                throw;
            }
        }
    }
}
"""
    
    return code

def main():
    import os
    
    # Define output path
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_path = os.path.join(
        script_dir, '..', '..', '..', '..', 
        'UNOPS.PAO.UNOPSDataAccess', 'Seed', 'Seeders', 
        'Unapproved_Partners_LiaisonOffice_Fixes_v3.cs'
    )
    
    print("Generating C# seeder code...")
    seeder_code = generate_seeder_code()
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    
    print(f"Writing seeder to: {output_path}")
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(seeder_code)
    
    print("[SUCCESS] Seeder file generated successfully!")
    print(f"   Output: {output_path}")

if __name__ == '__main__':
    main()

