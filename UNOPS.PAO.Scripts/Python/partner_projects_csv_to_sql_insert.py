import csv

def generate_partner_projects_insert(csv_file):
    # Open the CSV file
    with open(csv_file, mode='r', encoding='utf-8') as file:
        reader = csv.reader(file)
        headers = next(reader)  # Read the header row

        # Prepare the VALUES part of the SQL statement
        values_list = []
        for row in reader:
            partner_number = row[0]  # PartnerNumber
            project_number = row[1]  # ProjectNumber

            # Generate the VALUES clause with subqueries to look up PartnerId and ProjectId
            values = f"""(
                (SELECT "Id" FROM public."Partners" WHERE "PartnerNumber" = '{partner_number}' LIMIT 1),
                (SELECT "Id" FROM public."Projects" WHERE "ProjectNumber" = '{project_number}' LIMIT 1)
            )"""
            values_list.append(values)

        # Combine all rows into a single INSERT statement
        sql = f"""
        INSERT INTO public."PartnerProjects" ("PartnerId", "ProjectId")
        VALUES
        {',\n'.join(values_list)};
        """
        return sql

# Example usage
csv_file = 'partner-projects-export-erp.csv'  # Path to your CSV file

# Generate the SQL INSERT statement
sql_statement = generate_partner_projects_insert(csv_file)

# Write the SQL statement to a file or print it
with open('partner_projects_insert.sql', 'w', encoding='utf-8') as output_file:
    output_file.write(sql_statement + '\n')

# Optionally, print the SQL statement to the console
print(sql_statement)