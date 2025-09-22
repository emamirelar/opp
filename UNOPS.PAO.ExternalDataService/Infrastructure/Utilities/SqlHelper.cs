using System.Text;

namespace UNOPS.PAO.ExternalDataService.Infrastructure.Utilities;

public class SqlHelper
{
    private readonly ILogger<SqlHelper> _logger;

    public SqlHelper(ILogger<SqlHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Builds a parameterized INSERT query
    /// </summary>
    public string BuildInsertQuery(string tableName, Dictionary<string, object?> fields, string? schema = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var columns = fields.Keys.Select(k => $"\"{k}\"").ToList();
        var parameters = fields.Keys.Select(k => $"@{k}").ToList();

        var sql = new StringBuilder();
        sql.AppendLine($"INSERT INTO {fullTableName} ({string.Join(", ", columns)})");
        sql.AppendLine($"VALUES ({string.Join(", ", parameters)})");
        sql.AppendLine("RETURNING Id;");

        return sql.ToString();
    }

    /// <summary>
    /// Builds a parameterized UPDATE query
    /// </summary>
    public string BuildUpdateQuery(string tableName, Dictionary<string, object?> fields, string whereClause, string? schema = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var setClauses = fields.Keys.Select(k => $"\"{k}\" = @{k}").ToList();

        var sql = new StringBuilder();
        sql.AppendLine($"UPDATE {fullTableName}");
        sql.AppendLine($"SET {string.Join(", ", setClauses)}");
        sql.AppendLine($"WHERE {whereClause}");

        return sql.ToString();
    }

    /// <summary>
    /// Builds a parameterized UPSERT (INSERT ... ON CONFLICT) query for PostgreSQL
    /// </summary>
    public string BuildUpsertQuery(
        string tableName, 
        Dictionary<string, object?> fields, 
        string conflictColumn, 
        List<string>? excludeFromUpdate = null, 
        string? schema = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var columns = fields.Keys.Select(k => $"\"{k}\"").ToList();
        var parameters = fields.Keys.Select(k => $"@{k}").ToList();

        var updateFields = excludeFromUpdate != null 
            ? fields.Keys.Where(k => !excludeFromUpdate.Contains(k)).ToList()
            : fields.Keys.ToList();

        var updateClauses = updateFields.Select(k => $"\"{k}\" = EXCLUDED.\"{k}\"").ToList();

        var sql = new StringBuilder();
        sql.AppendLine($"INSERT INTO {fullTableName} ({string.Join(", ", columns)})");
        sql.AppendLine($"VALUES ({string.Join(", ", parameters)})");
        sql.AppendLine($"ON CONFLICT (\"{conflictColumn}\") DO UPDATE SET");
        sql.AppendLine($"    {string.Join(",\n    ", updateClauses)}");
        sql.AppendLine("RETURNING (xmax = 0) AS inserted;");

        return sql.ToString();
    }

    /// <summary>
    /// Builds a SELECT query with optional WHERE clause
    /// </summary>
    public string BuildSelectQuery(
        string tableName, 
        List<string>? columns = null, 
        string? whereClause = null, 
        string? orderBy = null, 
        int? limit = null,
        string? schema = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var selectColumns = columns?.Count > 0 
            ? string.Join(", ", columns.Select(c => $"\"{c}\""))
            : "*";

        var sql = new StringBuilder();
        sql.AppendLine($"SELECT {selectColumns}");
        sql.AppendLine($"FROM {fullTableName}");

        if (!string.IsNullOrEmpty(whereClause))
        {
            sql.AppendLine($"WHERE {whereClause}");
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            sql.AppendLine($"ORDER BY {orderBy}");
        }

        if (limit.HasValue)
        {
            sql.AppendLine($"LIMIT {limit}");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Builds a table existence check query
    /// </summary>
    public string BuildTableExistsQuery()
    {
        return @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_name = @tableName 
            AND table_schema = @schemaName";
    }

    /// <summary>
    /// Builds a column existence check query
    /// </summary>
    public string BuildColumnExistsQuery()
    {
        return @"
            SELECT COUNT(*) 
            FROM information_schema.columns 
            WHERE table_name = @tableName 
            AND table_schema = @schemaName
            AND column_name = @columnName";
    }

    /// <summary>
    /// Builds a table schema query
    /// </summary>
    public string BuildTableSchemaQuery()
    {
        return @"
            SELECT 
                column_name,
                data_type,
                is_nullable,
                column_default,
                character_maximum_length,
                numeric_precision,
                numeric_scale,
                ordinal_position
            FROM information_schema.columns 
            WHERE table_name = @tableName 
            AND table_schema = @schemaName
            ORDER BY ordinal_position";
    }

    /// <summary>
    /// Builds a foreign key lookup query
    /// </summary>
    public string BuildForeignKeyLookupQuery(string lookupTable, string lookupField, string returnField)
    {
        return $@"
            SELECT ""{lookupField}"" as lookup_value, ""{returnField}"" as id 
            FROM ""{lookupTable}"" 
            WHERE ""{lookupField}"" = ANY(@values)";
    }

    /// <summary>
    /// Builds a CREATE TABLE query
    /// </summary>
    public string BuildCreateTableQuery(
        string tableName, 
        Dictionary<string, string> columns, 
        string? primaryKey = null,
        List<string>? indexes = null,
        string? schema = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var sql = new StringBuilder();

        // Create schema if specified
        if (!string.IsNullOrEmpty(schema))
        {
            sql.AppendLine($"CREATE SCHEMA IF NOT EXISTS \"{schema}\";");
        }

        // Create table
        sql.AppendLine($"CREATE TABLE {fullTableName} (");

        // Add Id column as primary key if not specified
        if (string.IsNullOrEmpty(primaryKey))
        {
            sql.AppendLine("    Id SERIAL PRIMARY KEY,");
        }

        // Add columns
        var columnDefinitions = columns.Select(kvp => $"    \"{kvp.Key}\" {kvp.Value}").ToList();
        sql.AppendLine(string.Join(",\n", columnDefinitions));

        // Add custom primary key if specified
        if (!string.IsNullOrEmpty(primaryKey) && primaryKey != "Id")
        {
            sql.AppendLine($",\n    PRIMARY KEY (\"{primaryKey}\")");
        }

        sql.AppendLine(");");

        // Add indexes
        if (indexes?.Count > 0)
        {
            foreach (var index in indexes)
            {
                sql.AppendLine($"CREATE INDEX IX_{tableName}_{index} ON {fullTableName} (\"{index}\");");
            }
        }

        return sql.ToString();
    }

    /// <summary>
    /// Builds an ADD COLUMN query
    /// </summary>
    public string BuildAddColumnQuery(string tableName, string columnName, string columnDefinition, string? schema = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        return $"ALTER TABLE {fullTableName} ADD COLUMN \"{columnName}\" {columnDefinition};";
    }

    /// <summary>
    /// Builds a DROP TABLE query
    /// </summary>
    public string BuildDropTableQuery(string tableName, string? schema = null, bool ifExists = true)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var ifExistsClause = ifExists ? "IF EXISTS " : "";
        return $"DROP TABLE {ifExistsClause}{fullTableName};";
    }

    /// <summary>
    /// Builds a COUNT query
    /// </summary>
    public string BuildCountQuery(string tableName, string? whereClause = null, string? schema = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var sql = new StringBuilder();
        sql.AppendLine($"SELECT COUNT(*) FROM {fullTableName}");
        
        if (!string.IsNullOrEmpty(whereClause))
        {
            sql.AppendLine($"WHERE {whereClause}");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Gets the full table name with schema
    /// </summary>
    public string GetFullTableName(string tableName, string? schema = null)
    {
        return string.IsNullOrEmpty(schema) 
            ? $"\"{tableName}\"" 
            : $"\"{schema}\".\"{tableName}\"";
    }

    /// <summary>
    /// Escapes a SQL identifier (table name, column name, etc.)
    /// </summary>
    public string EscapeIdentifier(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }

    /// <summary>
    /// Sanitizes a string value for SQL (basic protection against SQL injection)
    /// </summary>
    public string SanitizeStringValue(string value)
    {
        return value?.Replace("'", "''") ?? string.Empty;
    }

    /// <summary>
    /// Converts a .NET Type to PostgreSQL column type
    /// </summary>
    public string GetPostgreSqlType(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => "BOOLEAN",
            TypeCode.Byte => "SMALLINT",
            TypeCode.Int16 => "SMALLINT",
            TypeCode.Int32 => "INTEGER",
            TypeCode.Int64 => "BIGINT",
            TypeCode.Single => "REAL",
            TypeCode.Double => "DOUBLE PRECISION",
            TypeCode.Decimal => "DECIMAL",
            TypeCode.DateTime => "TIMESTAMP",
            TypeCode.String => "TEXT",
            _ when type == typeof(Guid) => "UUID",
            _ when type == typeof(DateOnly) => "DATE",
            _ when type == typeof(TimeOnly) => "TIME",
            _ when type == typeof(byte[]) => "BYTEA",
            _ => "TEXT"
        };
    }

    /// <summary>
    /// Builds a batch insert query using VALUES
    /// </summary>
    public string BuildBatchInsertQuery(
        string tableName, 
        List<string> columns, 
        int batchSize,
        string? schema = null,
        string? conflictColumn = null,
        List<string>? updateColumns = null)
    {
        var fullTableName = GetFullTableName(tableName, schema);
        var columnNames = string.Join(", ", columns.Select(c => $"\"{c}\""));
        
        var sql = new StringBuilder();
        sql.AppendLine($"INSERT INTO {fullTableName} ({columnNames})");
        sql.AppendLine("VALUES");
        
        // Create parameter placeholders for batch
        var valueRows = new List<string>();
        for (int i = 0; i < batchSize; i++)
        {
            var parameterNames = columns.Select(c => $"@{c}{i}");
            valueRows.Add($"({string.Join(", ", parameterNames)})");
        }
        
        sql.AppendLine(string.Join(",\n", valueRows));

        // Add ON CONFLICT clause if specified
        if (!string.IsNullOrEmpty(conflictColumn) && updateColumns?.Count > 0)
        {
            sql.AppendLine($"ON CONFLICT (\"{conflictColumn}\") DO UPDATE SET");
            var updateClauses = updateColumns.Select(c => $"\"{c}\" = EXCLUDED.\"{c}\"");
            sql.AppendLine(string.Join(",\n    ", updateClauses));
        }
        else if (!string.IsNullOrEmpty(conflictColumn))
        {
            sql.AppendLine($"ON CONFLICT (\"{conflictColumn}\") DO NOTHING");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Validates SQL identifier (table name, column name)
    /// </summary>
    public bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return false;

        // Basic validation: alphanumeric and underscore, starts with letter or underscore
        return System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
    }

    /// <summary>
    /// Logs SQL query summary for debugging (reduced verbosity)
    /// </summary>
    public void LogQuery(string operation, string sql, Dictionary<string, object?>? parameters = null)
    {
        // Only log operation type and parameter count to reduce log volume
        _logger.LogDebug("SQL {Operation} executed with {ParameterCount} parameters", 
            operation, parameters?.Count ?? 0);
        
        // Only log detailed SQL and parameters at Trace level for deep debugging
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("SQL {Operation}: {SQL}", operation, sql);
            if (parameters?.Count > 0)
            {
                var paramLog = string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value ?? "NULL"}"));
                _logger.LogTrace("SQL Parameters: {Parameters}", paramLog);
            }
        }
    }
}
