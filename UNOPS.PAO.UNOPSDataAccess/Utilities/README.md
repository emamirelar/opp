# MigrationSqlScriptExecutor Utility

A utility class for executing SQL scripts from Entity Framework migrations, designed to work with the `UNOPS.PAO.Scripts` directory.

## Features

- **Simple API**: Execute single or multiple SQL scripts with minimal code
- **Flexible Path Resolution**: Automatically finds scripts regardless of build/deployment environment
- **Error Handling**: Clear error messages and robust exception handling
- **Subdirectory Support**: Organize scripts in subdirectories within `UNOPS.PAO.Scripts`
- **Script Validation**: Check if scripts exist before attempting execution

## Usage Examples

### 1. Execute a Single Script

```csharp
using UNOPS.PAO.UNOPSDataAccess.Utilities;

public partial class MyMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create tables first...
        
        // Execute a single SQL script
        MigrationSqlScriptExecutor.ExecuteSqlScript(migrationBuilder, "seed-data.sql");
    }
}
```

### 2. Execute Multiple Scripts in Order

```csharp
using UNOPS.PAO.UNOPSDataAccess.Utilities;

public partial class MyMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create tables first...
        
        // Execute multiple scripts in order
        MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
        {
            "seed-entities.sql",
            "seed-entity-field-managers.sql",
            "seed-permissions.sql"
        });
    }
}
```

### 3. Execute Scripts from Subdirectories

```csharp
using UNOPS.PAO.UNOPSDataAccess.Utilities;

public partial class MyMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Execute script from UNOPS.PAO.Scripts/InitialData/ subdirectory
        MigrationSqlScriptExecutor.ExecuteSqlScript(
            migrationBuilder, 
            "partner-data.sql", 
            "InitialData"
        );
    }
}
```

### 4. Check if Script Exists Before Execution

```csharp
using UNOPS.PAO.UNOPSDataAccess.Utilities;

public partial class MyMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Check if optional script exists before executing
        if (MigrationSqlScriptExecutor.ScriptExists("optional-seed-data.sql"))
        {
            MigrationSqlScriptExecutor.ExecuteSqlScript(migrationBuilder, "optional-seed-data.sql");
        }
    }
}
```

### 5. Read Script Content Without Execution

```csharp
using UNOPS.PAO.UNOPSDataAccess.Utilities;

public partial class MyMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Read script content for custom processing
        var scriptContent = MigrationSqlScriptExecutor.ReadSqlScript("template.sql");
        
        // Modify content if needed
        var modifiedScript = scriptContent.Replace("{SCHEMA}", "public");
        
        // Execute modified script
        migrationBuilder.Sql(modifiedScript);
    }
}
```

## API Reference

### ExecuteSqlScript

Executes a single SQL script from the UNOPS.PAO.Scripts directory.

```csharp
public static void ExecuteSqlScript(
    MigrationBuilder migrationBuilder, 
    string scriptFileName, 
    string scriptsSubdirectory = null)
```

**Parameters:**
- `migrationBuilder`: The EF migration builder instance
- `scriptFileName`: Name of the SQL file (e.g., "seed-data.sql")
- `scriptsSubdirectory`: Optional subdirectory within UNOPS.PAO.Scripts

### ExecuteSqlScripts

Executes multiple SQL scripts in the specified order.

```csharp
public static void ExecuteSqlScripts(
    MigrationBuilder migrationBuilder, 
    string[] scriptFileNames, 
    string scriptsSubdirectory = null)
```

### ReadSqlScript

Reads a SQL script file and returns its content as a string.

```csharp
public static string ReadSqlScript(
    string scriptFileName, 
    string scriptsSubdirectory = null)
```

### ScriptExists

Checks if a SQL script file exists.

```csharp
public static bool ScriptExists(
    string scriptFileName, 
    string scriptsSubdirectory = null)
```

## Directory Structure

The utility expects SQL scripts to be organized as follows:

```
Solution Root/
├── UNOPS.PAO.Scripts/
│   ├── seed-entities.sql
│   ├── seed-entity-field-managers.sql
│   ├── InitialData/
│   │   ├── partner-data.sql
│   │   └── contact-data.sql
│   └── Maintenance/
│       ├── cleanup.sql
│       └── reindex.sql
└── Other Projects...
```

## Error Handling

The utility provides clear error messages:

- **FileNotFoundException**: When a script file cannot be found
- **InvalidOperationException**: When script execution fails
- **ArgumentException**: When invalid parameters are provided

All methods include comprehensive error handling and will provide detailed information about what went wrong and where the utility searched for files.

## Best Practices

1. **Order Matters**: When executing multiple scripts, ensure they're in the correct dependency order
2. **Idempotent Scripts**: Write scripts that can be safely re-run (use IF NOT EXISTS, etc.)
3. **Error Handling**: Consider using transactions in your SQL scripts for atomicity
4. **Script Organization**: Use subdirectories to organize scripts by purpose or release
5. **Documentation**: Include comments in your SQL scripts explaining their purpose 