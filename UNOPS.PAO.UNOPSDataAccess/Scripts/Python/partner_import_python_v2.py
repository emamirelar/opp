import csv

def generate_partner_inserts(csv_file_path, output_sql_path):
    """Generate SQL INSERT statements from CSV file with PartnerCategoryId lookups"""
    
    # Read the CSV file
    with open(csv_file_path, mode='r', encoding='utf-8') as csv_file:
        reader = csv.DictReader(csv_file)
        
        # Prepare SQL output
        sql_output = [
            "-- Partner data import script",
            "-- Generated from: " + csv_file_path,
            "",
            "BEGIN;",
            "",
            "-- Create temporary table for category mapping",
            "CREATE TEMP TABLE temp_partner_categories AS",
            "SELECT ",
            "    p.\"Name\" AS category_name,",
            "    p.\"Id\" AS category_id",
            "FROM \"public\".\"PartnerCategories\" p",
            "WHERE p.\"Name\" IN (",
        ]
        
        # First pass: Collect all unique category names
        category_names = set()
        for row in reader:
            category_name = row.get('PartnerCategory', '').strip()
            if category_name:
                category_names.add(category_name.replace("'", "''"))
        
        # Add the category names to the SQL
        sql_output.append("    '" + "',\n    '".join(category_names) + "'")
        sql_output.append(");\n")
        
        # Second pass: Generate INSERT statements
        csv_file.seek(0)  # Rewind to read CSV again
        next(reader)  # Skip header row
        
        sql_output.append("-- Insert partner records")
        for row in reader:
            # Prepare values for each field
            values = []
            for field in [
                'PartnerCode', 'Name', 'Status', 'NewEngagement', 'Phone', 'Website',
                'Address1Street', 'Address1City', 'Address1StateProvince', 'Address1PostalCode', 'Address1Country',
                'ShortName', 'PooledFund', 'DDRequired', 'DDEACDone',
                'EACReference', 'GlobalKeyAccount', 'UNSecretariatEntity', 'LevyPotentiallyApplies', 'ReasonForLevyNotApplying',
                'LevyTreatment', 'Discriminator', 'CreatedBy', 'CreatedDate', 'LastModifiedBy', 'IsDeleted', 'DeletedBy'
            ]:
                value = row.get(field, '').strip()
                if not value:
                    values.append('NULL')
                else:
                    escaped_value = value.replace("'", "''")
                    values.append(f"'{escaped_value}'")
            
            # Get PartnerCategoryId from temp table
            partner_category_name = row.get('PartnerCategory', '').strip().replace("'", "''")
            
            sql_output.append(f"""
INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    {', '.join(values)},
    (SELECT category_id FROM temp_partner_categories WHERE category_name = '{partner_category_name}')
);""")
        
        # Clean up
        sql_output.extend([
            "",
            "-- Clean up temporary tables",
            "DROP TABLE temp_partner_categories;",
            "",
            "COMMIT;"
        ])
        
        # Write to SQL file
        with open(output_sql_path, 'w', encoding='utf-8') as sql_file:
            sql_file.write('\n'.join(sql_output))
        
        print(f"Successfully generated {len(category_names)} category mappings and {reader.line_num - 2} partner records")
        print(f"SQL script saved to: {output_sql_path}")

# Example usage
if __name__ == "__main__":
    input_csv = "partner_data_export_SF_UAT_v2 - Sheet1.csv"  # Your CSV file path
    output_sql = "partners_insert_script_v2.sql"  # Output SQL file path
    generate_partner_inserts(input_csv, output_sql)