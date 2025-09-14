import csv

def generate_partner_projects_insert(csv_file):
    # Open the CSV file
    with open(csv_file, mode='r', encoding='utf-8') as file:
        reader = csv.reader(file)
        headers = next(reader)  # Read the header row

        # Prepare the VALUES part of the SQL statement
        values_list = []
        for row in reader:
            partner_code = row[0]  # PartnerCode
            project_number = row[1]  # ProjectNumber

            # Add the PartnerCode and ProjectNumber to the VALUES list
            values_list.append(f"('{partner_code}', '{project_number}')")

        # Create a temporary table and insert the CSV data
        temp_table_sql = """
        CREATE TEMP TABLE "temp_partner_projects" (
            "PartnerCode" VARCHAR(50),
            "ProjectNumber" VARCHAR(50)
        );
        """

        # Insert the CSV data into the temporary table
        insert_temp_table_sql = f"""
        INSERT INTO "temp_partner_projects" ("PartnerCode", "ProjectNumber")
        VALUES
        {',\n'.join(values_list)};
        """

        # Generate the final INSERT statement using a JOIN
        final_insert_sql = """
        INSERT INTO public."PartnerProjects" ("PartnersId", "ProjectsId")
        SELECT p."Id", pr."Id"
        FROM "temp_partner_projects" t
        JOIN public."Partners" p ON t."PartnerCode" = p."PartnerCode"
        JOIN public."Projects" pr ON t."ProjectNumber" = pr."ProjectNumber";

        DROP TABLE "temp_partner_projects";
        """

        # Combine all SQL statements into one script
        full_sql_script = f"""
        {temp_table_sql}
        {insert_temp_table_sql}
        {final_insert_sql}
        """
        return full_sql_script

# Example usage
csv_file = 'partner-projects-export-erp.csv'  # Path to your CSV file

# Generate the SQL script
sql_script = generate_partner_projects_insert(csv_file)

# Write the SQL script to a file or print it
with open('partner_projects_insert_optimized.sql', 'w', encoding='utf-8') as output_file:
    output_file.write(sql_script + '\n')

# Optionally, print the SQL script to the console
print(sql_script)