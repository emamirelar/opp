import csv
import os

# Get the directory where this script is located
script_dir = os.path.dirname(os.path.abspath(__file__))
csv_file_path = os.path.join(script_dir, 'Orgunit data.csv')
output_sql_path = os.path.join(script_dir, '../../Seed/Scripts/OrganizationUnits.sql')

# Read the CSV file
with open(csv_file_path, mode='r', encoding='utf-8') as file:
    csv_reader = csv.DictReader(file)
    rows = list(csv_reader)

# Generate the INSERT statement
insert_statement = """INSERT INTO
  "public"."OrganizationHierarchies" ("Id", "Code", "Name", "Type", "Description", "ParentId", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate")
VALUES
"""

# Process each row and add to the VALUES clause
value_rows = []
for row in rows:
    # Extract and clean values from CSV
    id_val = row['Id'].strip()
    code = row['Code'].strip()
    name = row['Name'].replace("'", "''")  # Escape single quotes for SQL
    type_val = row['Type'].strip()
    description = row['Description'].replace("'", "''") if row['Description'].strip() else ''
    parent_id = row['ParentId'].strip() if row['ParentId'].strip() else 'NULL'
    status = row['Status'].strip()
    created_by = row['CreatedBy'].strip()
    created_date = row['CreatedDate'].strip()
    last_modified_by = row['LastModifiedBy'].strip()
    last_modified_date = row['LastModifiedDate'].strip()
    is_deleted = row['IsDeleted'].strip().lower()
    deleted_by = row['DeletedBy'].strip()
    deleted_date = row['DeletedDate'].strip()
    
    # Format the VALUES clause
    if parent_id == 'NULL':
        parent_id_sql = 'NULL'
    else:
        parent_id_sql = f"'{parent_id}'"
    
    value_rows.append(f"  ({id_val}, '{code}', '{name}', '{type_val}', '{description}', {parent_id_sql}, {status}, {created_by}, {created_date}, {last_modified_by}, {last_modified_date}, {is_deleted}, {deleted_by}, {deleted_date})")

# Combine all parts
full_sql = insert_statement + ",\n".join(value_rows) + ";\n"

# Print the SQL (optional)
print("Generated SQL file with", len(value_rows), "organization units")

# Save to the Scripts folder
os.makedirs(os.path.dirname(output_sql_path), exist_ok=True)
with open(output_sql_path, 'w', encoding='utf-8') as sql_file:
    sql_file.write(full_sql)

print(f"SQL file saved to: {output_sql_path}")