#!/usr/bin/env python3
"""
Script to generate ArtifactTypeSeeder_Country.cs from CSV file
Reads Country_Artifact_Type_Seeder - Sheet1.csv and generates C# seeder code
"""

import csv
import os
from pathlib import Path

# Configuration
CSV_FILE = "Country_Artifact_Type_Seeder - Sheet1.csv"
OUTPUT_FILE = "ArtifactTypeSeeder_Country.cs"
OUTPUT_DIR = "../../../../UNOPS.PAO.UNOPSDataAccess/Seed/Seeders"

# Data type mapping
DATA_TYPE_MAP = {
    "String": "stringDataTypeId",
    "Number": "numberDataTypeId",
    "Date": "dateDataTypeId",
    "Boolean": "booleanDataTypeId",
    "Document": "documentDataTypeId"
}

# Categories that should use IsUsedForCalculations
CALCULATION_CATEGORIES = ["Index", "Score", "Rank", "Per_Capita", "Population", "Gini"]

# Categories that should use IsUsedForAI (key indicators)
AI_CATEGORIES = ["Region", "Index", "Score", "Typology", "Situation", "Classification", "Group", "Class"]


def should_use_for_calculations(name, data_type, description):
    """Determine if artifact should be used for calculations"""
    if data_type != "Number":
        return False
    
    # Check if any calculation keyword is in name
    for keyword in CALCULATION_CATEGORIES:
        if keyword.lower() in name.lower():
            return True
    
    return False


def should_use_for_ai(name, data_type, description):
    """Determine if artifact should be used for AI"""
    # String types with meaningful classification data
    if data_type == "String":
        for keyword in AI_CATEGORIES:
            if keyword.lower() in name.lower():
                return True
    
    # Number types that are key indicators
    if data_type == "Number":
        for keyword in ["Index", "Score", "Rank"]:
            if keyword.lower() in name.lower():
                return True
    
    return False


def escape_csharp_string(text):
    """Escape special characters for C# string literals"""
    if not text:
        return ""
    return text.replace('\\', '\\\\').replace('"', '\\"').replace('\n', ' ').replace('\r', '')


def determine_category(name, description):
    """Determine appropriate category based on name and description"""
    name_lower = name.lower()
    desc_lower = (description or "").lower()
    
    # Check for various category indicators
    if any(word in name_lower for word in ["region", "un_", "unops_"]):
        return "Classification"
    elif any(word in name_lower for word in ["index", "score", "rank"]):
        return "Assessment"
    elif any(word in name_lower for word in ["population", "gni", "gdp"]):
        return "Demographics"
    elif any(word in name_lower for word in ["date", "year", "updated", "download"]):
        return "Metadata"
    elif any(word in name_lower for word in ["source", "version"]):
        return "Metadata"
    elif any(word in name_lower for word in ["member", "country", "situation"]):
        return "Classification"
    else:
        return "General"


def convert_yes_no_to_bool(value):
    """Convert YES/NO to C# boolean"""
    if not value or not value.strip():
        return "false"
    value_upper = value.strip().upper()
    return "true" if value_upper == "YES" else "false"


def generate_artifact_type_code(row, order):
    """Generate a single ArtifactType object in C# code"""
    name = row["Name"].replace("_", " ").strip()
    code = row["ArtifactTypeCode"].strip()
    data_type = row["ArtifactDataType"].strip()
    
    # Handle Description - set to null if empty
    description_value = row.get("Description", "").strip()
    if description_value:
        description = f'"{escape_csharp_string(description_value)}"'
    else:
        description = "null"
    
    # Handle Category - set to null if empty
    category_value = row.get("Category", "").strip()
    if category_value:
        category = f'"{category_value}"'
    else:
        category = "null"
    
    # Handle Source - set to null if empty
    source_value = row.get("Source", "").strip()
    if source_value:
        source = f'"{escape_csharp_string(source_value)}"'
    else:
        source = "null"
    
    entity_types = row["ApplicableEntityTypes"].strip()
    
    # Handle AllowBulkUpdate (YES/NO to true/false)
    allow_bulk_update = convert_yes_no_to_bool(row.get("AllowBulkUpdate", "NO"))
    
    # Handle IsSearchable (YES/NO to true/false)
    is_searchable = convert_yes_no_to_bool(row.get("IsSearchable", "NO"))
    
    data_type_var = DATA_TYPE_MAP.get(data_type, "stringDataTypeId")
    is_for_calculations = "true" if should_use_for_calculations(code, data_type, description_value) else "false"
    is_for_ai = "true" if should_use_for_ai(code, data_type, description_value) else "false"
    
    code_template = f"""            new ArtifactType
            {{
                Name = "{name}",
                ArtifactTypeCode = "{code}",
                ArtifactDataTypeId = {data_type_var},
                Description = {description},
                Category = {category},
                ApplicableEntityTypes = "{entity_types}",
                Source = {source},
                IsSearchable = {is_searchable},
                AllowBulkUpdate = {allow_bulk_update},
                IsUsedForCalculations = {is_for_calculations},
                IsUsedForAI = {is_for_ai},
                Order = {order},
                Status = EntityStatus.Active,
                IsDeleted = false
            }}"""
    
    return code_template


def generate_seeder_class(artifact_types_code):
    """Generate the complete C# seeder class"""
    
    class_template = f"""using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds ArtifactTypes for Country entity
/// Generated from Country_Artifact_Type_Seeder - Sheet1.csv
/// </summary>
public static class ArtifactTypeSeeder_Country
{{
    public static async Task SeedCountryArtifactTypesAsync(UNOPSAppDbContext context)
    {{
        Console.WriteLine("🔄 Seeding Country Artifact Types...");

        // Get all data type IDs
        var dataTypes = await context.Set<ArtifactDataType>().ToListAsync();
        var stringDataType = dataTypes.FirstOrDefault(dt => dt.Name == "string");
        var numberDataType = dataTypes.FirstOrDefault(dt => dt.Name == "number");
        var dateDataType = dataTypes.FirstOrDefault(dt => dt.Name == "date");
        var booleanDataType = dataTypes.FirstOrDefault(dt => dt.Name == "boolean");
        var documentDataType = dataTypes.FirstOrDefault(dt => dt.Name == "document");

        if (stringDataType == null || numberDataType == null || dateDataType == null || booleanDataType == null || documentDataType == null)
        {{
            Console.WriteLine("  ❌ Error: Required ArtifactDataTypes not found. Please seed ArtifactDataTypes first.");
            Console.WriteLine($"     Found - string: {{stringDataType != null}}, number: {{numberDataType != null}}, date: {{dateDataType != null}}, boolean: {{booleanDataType != null}}, document: {{documentDataType != null}}");
            return;
        }}

        var stringDataTypeId = stringDataType.Id;
        var numberDataTypeId = numberDataType.Id;
        var dateDataTypeId = dateDataType.Id;
        var booleanDataTypeId = booleanDataType.Id;
        var documentDataTypeId = documentDataType.Id;

        var artifactTypesToSeed = GetCountryArtifactTypesToSeed(stringDataTypeId, numberDataTypeId, dateDataTypeId, booleanDataTypeId, documentDataTypeId);
        var existingArtifactTypes = await context.Set<ArtifactType>().ToListAsync();

        int insertedCount = 0;
        int updatedCount = 0;
        int skippedCount = 0;

        foreach (var artifactTypeData in artifactTypesToSeed)
        {{
            var existingArtifactType = existingArtifactTypes
                .FirstOrDefault(at => at.ArtifactTypeCode == artifactTypeData.ArtifactTypeCode);

            if (existingArtifactType == null)
            {{
                context.Set<ArtifactType>().Add(artifactTypeData);
                insertedCount++;
                Console.WriteLine($"  ✅ Inserted Country Artifact Type: {{artifactTypeData.ArtifactTypeCode}} - {{artifactTypeData.Name}}");
            }}
            else
            {{
                bool hasChanges = false;

                if (existingArtifactType.Name != artifactTypeData.Name)
                {{
                    existingArtifactType.Name = artifactTypeData.Name;
                    hasChanges = true;
                }}

                if (existingArtifactType.Description != artifactTypeData.Description)
                {{
                    existingArtifactType.Description = artifactTypeData.Description;
                    hasChanges = true;
                }}

                if (existingArtifactType.Category != artifactTypeData.Category)
                {{
                    existingArtifactType.Category = artifactTypeData.Category;
                    hasChanges = true;
                }}

                if (existingArtifactType.ApplicableEntityTypes != artifactTypeData.ApplicableEntityTypes)
                {{
                    existingArtifactType.ApplicableEntityTypes = artifactTypeData.ApplicableEntityTypes;
                    hasChanges = true;
                }}

                if (existingArtifactType.IsUsedForCalculations != artifactTypeData.IsUsedForCalculations)
                {{
                    existingArtifactType.IsUsedForCalculations = artifactTypeData.IsUsedForCalculations;
                    hasChanges = true;
                }}

                if (existingArtifactType.IsUsedForAI != artifactTypeData.IsUsedForAI)
                {{
                    existingArtifactType.IsUsedForAI = artifactTypeData.IsUsedForAI;
                    hasChanges = true;
                }}

                if (existingArtifactType.Order != artifactTypeData.Order)
                {{
                    existingArtifactType.Order = artifactTypeData.Order;
                    hasChanges = true;
                }}

                if (existingArtifactType.Source != artifactTypeData.Source)
                {{
                    existingArtifactType.Source = artifactTypeData.Source;
                    hasChanges = true;
                }}

                if (existingArtifactType.IsSearchable != artifactTypeData.IsSearchable)
                {{
                    existingArtifactType.IsSearchable = artifactTypeData.IsSearchable;
                    hasChanges = true;
                }}

                if (existingArtifactType.AllowBulkUpdate != artifactTypeData.AllowBulkUpdate)
                {{
                    existingArtifactType.AllowBulkUpdate = artifactTypeData.AllowBulkUpdate;
                    hasChanges = true;
                }}

                if (existingArtifactType.Status != artifactTypeData.Status)
                {{
                    existingArtifactType.Status = artifactTypeData.Status;
                    hasChanges = true;
                }}

                if (existingArtifactType.IsDeleted)
                {{
                    existingArtifactType.IsDeleted = false;
                    hasChanges = true;
                }}

                if (hasChanges)
                {{
                    updatedCount++;
                    Console.WriteLine($"  🔄 Updated Country Artifact Type: {{artifactTypeData.ArtifactTypeCode}} - {{artifactTypeData.Name}}");
                }}
                else
                {{
                    skippedCount++;
                    Console.WriteLine($"  ⏭️  Skipped Country Artifact Type (unchanged): {{artifactTypeData.ArtifactTypeCode}} - {{artifactTypeData.Name}}");
                }}
            }}
        }}

        if (insertedCount > 0 || updatedCount > 0)
        {{
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Country Artifact Types seeding completed: {{insertedCount}} inserted, {{updatedCount}} updated, {{skippedCount}} skipped\\n");
        }}
        else
        {{
            Console.WriteLine($"✅ Country Artifact Types seeding completed: No changes needed ({{skippedCount}} already up-to-date)\\n");
        }}
    }}

    private static List<ArtifactType> GetCountryArtifactTypesToSeed(int stringDataTypeId, int numberDataTypeId, int dateDataTypeId, int booleanDataTypeId, int documentDataTypeId)
    {{
        return new List<ArtifactType>
        {{
{artifact_types_code}
        }};
    }}
}}
"""
    
    return class_template


def main():
    """Main execution function"""
    print(">> Starting Country Artifact Type Seeder generation...")
    
    # Get the script directory
    script_dir = Path(__file__).parent
    csv_path = script_dir / CSV_FILE
    
    # Check if CSV file exists
    if not csv_path.exists():
        print(f"ERROR: CSV file not found at {csv_path}")
        return
    
    print(f">> Reading CSV file: {csv_path}")
    
    # Read CSV and generate artifact types
    artifact_types = []
    order_counter = 1000  # Start from 1000 to avoid conflicts with existing test data
    
    with open(csv_path, 'r', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            artifact_code = generate_artifact_type_code(row, order_counter)
            artifact_types.append(artifact_code)
            order_counter += 1
    
    print(f">> Processed {len(artifact_types)} artifact types")
    
    # Join all artifact types with commas
    artifact_types_code = ",\n            \n".join(artifact_types)
    
    # Generate complete seeder class
    seeder_code = generate_seeder_class(artifact_types_code)
    
    # Write output file
    output_dir = script_dir / OUTPUT_DIR
    output_dir.mkdir(parents=True, exist_ok=True)
    output_path = output_dir / OUTPUT_FILE
    
    print(f">> Writing seeder file: {output_path}")
    
    with open(output_path, 'w', encoding='utf-8') as outfile:
        outfile.write(seeder_code)
    
    print(f"SUCCESS: Generated {OUTPUT_FILE}")
    print(f"Location: {output_path}")
    print("\nNext steps:")
    print("   1. Review the generated file")
    print("   2. Add it to your project if not already included")
    print("   3. Call SeedCountryArtifactTypesAsync() in your seed configuration")


if __name__ == "__main__":
    main()

