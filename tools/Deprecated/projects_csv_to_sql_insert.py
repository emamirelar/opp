import csv

def generate_sql_insert(csv_file, table_name):
    # Open the CSV file
    with open(csv_file, mode='r', encoding='utf-8') as file:
        reader = csv.reader(file)
        headers = next(reader)  # Read the header row

        # Prepare the VALUES part of the SQL statement
        values_list = []
        for row in reader:
            # Process each value in the row
            processed_row = []
            for i, value in enumerate(row):
                column_name = headers[i].strip()  # Get the column name
                if column_name == "Name":  # Handle Name column specifically
                    if value.strip() == "":  # If Name is empty or NULL, replace with "No Name"
                        processed_row.append("'No Name'")
                    else:
                        processed_row.append(f"'{value.replace("'", "''")}'")  # Escape single quotes
                elif column_name == "Status":  # Handle Status column specifically
                    if value.strip().lower() == "inactive":
                        processed_row.append('0')
                    elif value.strip().lower() == "active":
                        processed_row.append('1')
                    elif value.strip().lower() == "closed":
                        processed_row.append('2')
                    elif value.strip().lower() == "draft":
                        processed_row.append('3')
                    else:
                        processed_row.append('NULL')  # Default to NULL for unknown values
                elif value.strip() == "":  # Handle empty values as NULL
                    processed_row.append('NULL')
                elif column_name == "ProjectNumber":  # Always treat ProjectNumber as a string
                    processed_row.append(f"'{value}'")
                elif value.upper() in ("FALSE", "TRUE"):  # Do not add quotes for FALSE or TRUE
                    processed_row.append(value.upper())
                elif value and value.replace('.', '', 1).isdigit():  # Check if it's a numeric value
                    processed_row.append(value)  # Add as-is (no quotes)
                elif value and value.replace('-', '', 2).replace(':', '', 2).replace(' ', '', 1).isdigit():  # Check if it's a datetime value
                    processed_row.append(f"'{value}'")  # Enclose in single quotes
                else:  # Handle strings
                    processed_row.append(f"'{value.replace("'", "''")}'")  # Escape single quotes
            values = ', '.join(processed_row)
            values_list.append(f"({values})")  # Add a newline after each row

        # Add double quotes around column names
        quoted_headers = [f'"{header}"' for header in headers]

        # Combine all rows into a single INSERT statement with newlines
        sql = f"INSERT INTO {table_name} ({', '.join(quoted_headers)})\nVALUES\n{',\n'.join(values_list)};"
        return sql

# Example usage
csv_file = 'project_export_erp.csv'  # Path to your CSV file
table_name = 'public."Projects"'  # Name of the table in the database

# Generate the SQL INSERT statement
sql_statement = generate_sql_insert(csv_file, table_name)

# Write the SQL statement to a file or print it
with open('project_insert.sql', 'w', encoding='utf-8') as output_file:
    output_file.write(sql_statement + '\n')

# Optionally, print the SQL statement to the console
print(sql_statement)