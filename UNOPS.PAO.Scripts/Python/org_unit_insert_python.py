import csv

# Read the CSV file
with open('Org_Unit_Import - Sheet1.csv', mode='r', encoding='utf-8') as file:
    csv_reader = csv.DictReader(file)
    rows = list(csv_reader)

# Generate the INSERT statement
insert_statement = """INSERT INTO
  "public"."OrganizationUnits" ("Code", "Name", "Discriminator", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy")
VALUES
"""

# Process each row and add to the VALUES clause
value_rows = []
for row in rows:
    code = row['Code'].strip()
    name = row['Name'].replace("'", "''")  # Escape single quotes for SQL
    value_rows.append(f"  ('{code}','{name}', 'UNOPSOrganizationUnit', 1, 0, NOW(), 0, false, 0)")

# Combine all parts
full_sql = insert_statement + ",\n".join(value_rows) + ";"

# Print or save to file
print(full_sql)

# Optionally save to a file
with open('organization_units_insert.sql', 'w', encoding='utf-8') as sql_file:
    sql_file.write(full_sql)