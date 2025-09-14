import csv
import html

# Read the CSV file
csv_file_path = 'sf_prod_liaison_office_export - Sheet1.csv'
with open(csv_file_path, mode='r', encoding='utf-8') as file:
    csv_reader = csv.DictReader(file)
    rows = list(csv_reader)

# Generate the C# seeder class
seeder_code = '''using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class LiaisonOfficeSeeder
    {
        public static async Task SeedLiaisonOfficesAsync(UNOPSAppDbContext context)
        {
            if (await context.LiaisonOffices.AnyAsync())
            {
                return;
            }

            var liaisonOffices = new List<LiaisonOffice>
            {
'''

# Process each row and generate LiaisonOffice objects
for i, row in enumerate(rows):
    code = row['Id'].strip()
    name = row['Name'].replace('"', '\\"').strip()  # Escape double quotes for C#
    
    seeder_code += f'''                new LiaisonOffice
                {{
                    Code = "{code}",
                    Name = "{name}",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }}'''
    
    # Add comma if not the last item
    if i < len(rows) - 1:
        seeder_code += ','
    
    seeder_code += '\n'

# Close the seeder class
seeder_code += '''            };

            await context.LiaisonOffices.AddRangeAsync(liaisonOffices);
            await context.SaveChangesAsync();
        }
    }
}'''

# Print the generated code
print(seeder_code)

# Save to file
output_file = 'LiaisonOfficeSeeder.cs'
with open(output_file, 'w', encoding='utf-8') as cs_file:
    cs_file.write(seeder_code)

print(f"\nSeeder code has been generated and saved to {output_file}")
print(f"Processed {len(rows)} liaison offices from the CSV file.")

# Also print the ID mapping for reference
print("\nSalesforce ID to Database ID mapping (1-based):")
for i, row in enumerate(rows):
    print(f"{row['Id'].strip()} -> {i + 1} ({row['Name'].strip()})")