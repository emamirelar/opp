# business-projects-and-opportunities
Github repo for Partner and Opportunity project




## Database Migrations 

1. List Migrations
```bash
dotnet ef migrations list --context UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj --startup-project UNOPS.PAO.Server
```

	•	dotnet ef migrations list: Displays all migrations that have been defined in your application so far.
	•	--context: Specifies which DbContext to use. This is helpful if you have multiple DbContexts in your project.
	•	--project: Points to the *.csproj file where the migrations are stored (i.e., where your EF code and Migrations folder live).
	•	--startup-project: Specifies the startup project that should be used to build and run the migrations (often the main application's *.csproj).

Why use it?
To verify which migrations you already have, check their naming, or confirm the current state of your migrations before adding new ones.

⸻

2. Add a New Migration
```bash
dotnet ef migrations add [migration-name] --context UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj --startup-project UNOPS.PAO.Server
```
	•	dotnet ef migrations add <MigrationName>: Creates a new migration file named [migration-name].
	•	EF Core analyzes the DbContext and your entity classes to see what changes are needed compared to the last migration, then generates migration files containing the necessary SQL or schema changes.
	•	The same flags as above (--context, --project, --startup-project) ensure EF Core knows which context and projects to work with.

Why use it?
To capture new schema changes (like added tables, columns, constraints, etc.) in versioned, trackable migration files.

⸻
3. Update the Database
```bash
dotnet ef database update --context UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj --startup-project UNOPS.PAO.Server
```
	•	dotnet ef database update: Applies any pending migrations to the actual database, updating its schema to match your current EF model.
	•	Again, --context, --project, and --startup-project specify which context to use and how EF Core should locate and apply migrations.

Why use it?
To bring your database schema in sync with your latest migrations, ensuring that code changes are reflected in the DB.

⸻


Need to remove the last migration?
```bash
dotnet ef migrations remove --context UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj --startup-project UNOPS.PAO.Server
```

# Import Functionality for Contacts and Partners

This document provides an overview of the import functionality for contacts and partners in the UNOPS PAO application.

## Overview

The import functionality allows users to import contact and partner data from Google Sheets. The system processes the data, validates it, and provides a user interface for reviewing and editing records before importing them.

## Import Flow

1. **Initiate Import**: 
   - From the Contact list or Partner list screen, click on the "Import" button.
   - The system will open a Google Sheets picker where you can select a spreadsheet.

2. **Review Data**: 
   - The system processes the spreadsheet and displays a dialog with the data.
   - You can see all records, paginate through them, and select which ones to import.
   - Records with missing required fields are highlighted in red.

3. **Edit Records**: 
   - You can edit individual records by clicking the edit button on each row.
   - The appropriate edit dialog (Contact or Partner) will open to allow you to modify the data.

4. **Select Records**: 
   - You can select all records or individual records for import.
   - The system shows a warning if you select records with missing required fields.

5. **Import**: 
   - Click the "Import" button to import the selected records.
   - The system applies default values to any fields that are not explicitly set.
   - After import, the list view is refreshed to show the new records.

## Required Fields

### Contacts
- Last Name
- Partner ID
- Email

### Partners
- Name
- Short Name
- New Engagement
- Pooled Fund
- DD Required
- DDEAC Done
- Levy Potentially Applies

## Using the Import Dialog

- **Pagination**: Use the paginator at the bottom to navigate through the records.
- **Select All**: The checkbox in the header selects all records across all pages.
- **Edit**: Click the edit button on a row to edit that record.
- **Search/Filter**: Use the search box to filter records.
- **Selected Count**: The number of selected records is displayed at the top.

## Default Values

The system applies default values to any fields that are not explicitly set:

### Contacts
- Default status: "Active"
- Empty strings for text fields
- Null values for objects and dates

### Partners
- Default status: "Active"
- Default globalKeyAccount: false
- Default unSecretariatEntity: false
- Empty strings for text fields
- Null values for objects and dates

## Troubleshooting

- If records fail to import, check the error messages displayed by the system.
- Ensure that all required fields are properly filled in.
- If you encounter issues with the Google Sheets picker, ensure you have the necessary permissions to access the spreadsheet.