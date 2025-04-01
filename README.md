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
	•	--startup-project: Specifies the startup project that should be used to build and run the migrations (often the main application’s *.csproj).

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