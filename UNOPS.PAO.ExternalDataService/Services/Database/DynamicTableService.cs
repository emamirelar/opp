using System.Text;
using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Infrastructure.Database;
using UNOPS.PAO.ExternalDataService.Infrastructure.Utilities;

namespace UNOPS.PAO.ExternalDataService.Services.Database;

public class DynamicTableService : IDynamicTableService
{
    private readonly IApplicationDatabaseService _databaseService;
    private readonly ILogger<DynamicTableService> _logger;
    private readonly DataTypeMapper _dataTypeMapper;

    public DynamicTableService(
        IApplicationDatabaseService databaseService,
        ILogger<DynamicTableService> logger,
        DataTypeMapper dataTypeMapper)
    {
        _databaseService = databaseService;
        _logger = logger;
        _dataTypeMapper = dataTypeMapper;
    }

    public async Task<bool> TableExistsAsync(string tableName, string? schema = null)
    {
        var schemaName = schema ?? "public";
        
        try
        {
            // Log connection details to verify we're checking the right database
            using var testConnection = await _databaseService.GetConnectionAsync();
            _logger.LogDebug("Checking table existence for {TableName}.{SchemaName}", tableName, schemaName);
            
            // PostgreSQL information_schema stores identifiers in lowercase for unquoted identifiers
            // For case-sensitive (quoted) identifiers, we need to check both the exact case and lowercase
            // First try exact case match
            var sql = @"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_name = @tableName
                AND table_schema = @schemaName";

            var result = await _databaseService.ExecuteScalarAsync<long>(sql, new { tableName, schemaName });
            
            // If not found with exact case, try lowercase (for unquoted identifiers)
            if (result == 0 && tableName != tableName.ToLowerInvariant())
            {
                var lowerTableName = tableName.ToLowerInvariant();
                result = await _databaseService.ExecuteScalarAsync<long>(sql, new { tableName = lowerTableName, schemaName });
                _logger.LogDebug("Table exists check result: {Result} (lowercase search)", result);
            }
            else
            {
                _logger.LogDebug("Table exists check result: {Result}", result);
            }
            
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if table {TableName} exists in schema {SchemaName}", tableName, schemaName);
            // Return false if we can't determine existence - this will trigger table creation attempt
            return false;
        }
    }

    public async Task CreateTableAsync(DestinationConfiguration destinationConfig)
    {
        var tableName = destinationConfig.TableName;
        var schema = destinationConfig.Schema ?? "public";
        var fullTableName = $"{schema}.{tableName}";

        try
        {
            // Log connection details to verify we're connecting to the right database
            using var testConnection = await _databaseService.GetConnectionAsync();
            _logger.LogInformation("Creating table {TableName} in database: {Database} on server: {Server}", 
                fullTableName, testConnection.Database, testConnection.DataSource);
            
            var sqlBuilder = new StringBuilder();
            
            // Ensure schema exists - quote schema name to handle special cases
            sqlBuilder.AppendLine($"CREATE SCHEMA IF NOT EXISTS \"{schema}\";");
            
            // Start table creation with quoted schema and table name to preserve case
            var quotedFullTableName = $"\"{schema}\".\"{tableName}\"";
            sqlBuilder.AppendLine($"CREATE TABLE {quotedFullTableName} (");
            
            // Always add the Id column as primary key - quoted to preserve case
            sqlBuilder.AppendLine("    \"Id\" SERIAL PRIMARY KEY,");

            // Add mapped fields with quoted column names to preserve case
            foreach (var mapping in destinationConfig.FieldMappings)
            {
                var columnDefinition = BuildColumnDefinition(mapping);
                sqlBuilder.AppendLine($"    {columnDefinition},");
            }

            // Add foreign key columns with quoted names and dynamic data types
            foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
            {
                var nullable = fkMapping.IsRequired ? "NOT NULL" : "NULL";
                var dataType = await GetLookupFieldDataTypeAsync(fkMapping.LookupTable, fkMapping.LookupReturnField);
                sqlBuilder.AppendLine($"    \"{fkMapping.DestinationField}\" {dataType} {nullable},");
            }

            // Add audit fields with quoted names to preserve case
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.CreatedBy}\" INTEGER NOT NULL DEFAULT 0,");
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.CreatedDate}\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,");
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.LastModifiedBy}\" INTEGER NOT NULL DEFAULT 0,");
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.LastModifiedDate}\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,");
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.SyncDate}\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,");
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.SyncBatchId}\" VARCHAR(50) NOT NULL,");
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.SourceSystem}\" VARCHAR(100) NOT NULL,");
            sqlBuilder.AppendLine($"    \"{destinationConfig.AuditFields.IsDeleted}\" BOOLEAN NOT NULL DEFAULT FALSE");

            sqlBuilder.AppendLine(");");

            // Add indexes with quoted names
            var uniqueFields = destinationConfig.FieldMappings.Where(f => f.IsUnique).ToList();
            foreach (var field in uniqueFields)
            {
                sqlBuilder.AppendLine($"CREATE UNIQUE INDEX \"IX_{tableName}_{field.DestinationField}\" ON {quotedFullTableName} (\"{field.DestinationField}\");");
            }

            // Add foreign key constraints (commented out for now to avoid dependency issues)
            // We'll add these manually in production or when the main app context is available
            /*
            foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
            {
                sqlBuilder.AppendLine($@"
                    ALTER TABLE {quotedFullTableName} 
                    ADD CONSTRAINT \"FK_{tableName}_{fkMapping.DestinationField}\"
                    FOREIGN KEY (\"{fkMapping.DestinationField}\") 
                    REFERENCES \"{fkMapping.LookupTable}\"(\"{fkMapping.LookupReturnField}\");");
            }
            */

            var sql = sqlBuilder.ToString();
            
            _logger.LogInformation("Creating table {TableName}", fullTableName);
            
            // Only log full SQL at Debug level to reduce log volume
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Table creation SQL for {TableName}:\n{SQL}", fullTableName, sql);
            }
            
            await _databaseService.ExecuteCommandAsync(sql);
            
            // Verify the table was actually created by checking its existence
            // Use a small retry loop in case of timing issues
            var maxAttempts = 3;
            var attempt = 0;
            bool tableExists = false;
            
            while (attempt < maxAttempts && !tableExists)
            {
                attempt++;
                await Task.Delay(100); // Small delay to allow database to commit
                tableExists = await TableExistsAsync(tableName, schema);
                
                if (!tableExists && attempt < maxAttempts)
                {
                    _logger.LogWarning("Table {TableName} not found after creation attempt {Attempt}, retrying...", fullTableName, attempt);
                }
            }
            
            if (!tableExists)
            {
                throw new InvalidOperationException($"Table {fullTableName} was not created successfully - table existence check failed after {maxAttempts} attempts");
            }
            
            _logger.LogInformation("Successfully created and verified table {TableName}", fullTableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create table {TableName}", $"{schema}.\"{tableName}\"");
            throw;
        }
    }

    public async Task UpdateTableSchemaAsync(DestinationConfiguration destinationConfig)
    {
        var tableName = destinationConfig.TableName;
        var schema = destinationConfig.Schema ?? "public";
        var quotedFullTableName = $"\"{schema}\".\"{tableName}\"";

        try
        {
            _logger.LogInformation("Starting comprehensive schema update for table {TableName}", quotedFullTableName);
            
            var currentSchema = await GetTableSchemaAsync(tableName, schema);
            var requiredSchema = await GetRequiredTableSchema(destinationConfig);
            var schemaDiff = AnalyzeSchemaChanges(currentSchema, requiredSchema, destinationConfig);

            if (!schemaDiff.HasChanges)
            {
                _logger.LogDebug("No schema changes required for table {TableName}", quotedFullTableName);
                return;
            }

            _logger.LogInformation("Schema changes detected for table {TableName}: {NewColumns} new, {ModifiedColumns} modified, {DroppedColumns} dropped",
                quotedFullTableName, schemaDiff.NewColumns.Count, schemaDiff.ModifiedColumns.Count, schemaDiff.DroppedColumns.Count);

            // Execute schema changes in order
            await ExecuteSchemaChanges(schemaDiff, quotedFullTableName, destinationConfig);
            
            _logger.LogInformation("Successfully updated schema for table {TableName}", quotedFullTableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update table schema for {TableName}", quotedFullTableName);
            throw;
        }
    }

    #region Schema Analysis and Alteration Methods

    private async Task<TableSchema> GetRequiredTableSchema(DestinationConfiguration destinationConfig)
    {
        var requiredSchema = new TableSchema
        {
            Name = destinationConfig.TableName,
            Schema = destinationConfig.Schema
        };

        // Add field mappings
        foreach (var mapping in destinationConfig.FieldMappings)
        {
            var column = CreateColumnSchemaFromMapping(mapping);
            requiredSchema.Columns[mapping.DestinationField] = column;
        }

        // Add foreign key columns
        foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
        {
            var column = await CreateColumnSchemaFromForeignKey(fkMapping);
            requiredSchema.Columns[fkMapping.DestinationField] = column;
        }

        // Add audit fields
        var auditColumns = GetAuditFieldsSchema(destinationConfig.AuditFields);
        foreach (var auditColumn in auditColumns)
        {
            requiredSchema.Columns[auditColumn.Key] = auditColumn.Value;
        }

        return requiredSchema;
    }

    private ColumnSchema CreateColumnSchemaFromMapping(FieldMapping mapping)
    {
        var postgresType = _dataTypeMapper.MapToPostgreSqlType(mapping.DataType);
        return new ColumnSchema
        {
            Name = mapping.DestinationField,
            DataType = ExtractDataTypeFromPostgres(postgresType),
            IsNullable = !mapping.IsRequired,
            DefaultValue = mapping.DefaultValue,
            IsUnique = mapping.IsUnique,
            MaxLength = ExtractMaxLengthFromType(postgresType)
        };
    }

    private async Task<ColumnSchema> CreateColumnSchemaFromForeignKey(ForeignKeyMapping fkMapping)
    {
        var dataType = await GetLookupFieldDataTypeAsync(fkMapping.LookupTable, fkMapping.LookupReturnField);
        return new ColumnSchema
        {
            Name = fkMapping.DestinationField,
            DataType = ExtractDataTypeFromPostgres(dataType),
            IsNullable = !fkMapping.IsRequired,
            MaxLength = ExtractMaxLengthFromType(dataType)
        };
    }

    private Dictionary<string, ColumnSchema> GetAuditFieldsSchema(AuditFieldsConfiguration auditFields)
    {
        return new Dictionary<string, ColumnSchema>
        {
            [auditFields.CreatedBy] = new() { Name = auditFields.CreatedBy, DataType = "integer", IsNullable = false, DefaultValue = "0" },
            [auditFields.CreatedDate] = new() { Name = auditFields.CreatedDate, DataType = "timestamp without time zone", IsNullable = false, DefaultValue = "CURRENT_TIMESTAMP" },
            [auditFields.LastModifiedBy] = new() { Name = auditFields.LastModifiedBy, DataType = "integer", IsNullable = false, DefaultValue = "0" },
            [auditFields.LastModifiedDate] = new() { Name = auditFields.LastModifiedDate, DataType = "timestamp without time zone", IsNullable = false, DefaultValue = "CURRENT_TIMESTAMP" },
            [auditFields.SyncDate] = new() { Name = auditFields.SyncDate, DataType = "timestamp without time zone", IsNullable = false, DefaultValue = "CURRENT_TIMESTAMP" },
            [auditFields.SyncBatchId] = new() { Name = auditFields.SyncBatchId, DataType = "character varying", IsNullable = false, MaxLength = 50 },
            [auditFields.SourceSystem] = new() { Name = auditFields.SourceSystem, DataType = "character varying", IsNullable = false, MaxLength = 100 },
            [auditFields.IsDeleted] = new() { Name = auditFields.IsDeleted, DataType = "boolean", IsNullable = false, DefaultValue = "false" }
        };
    }

    private SchemaDifference AnalyzeSchemaChanges(TableSchema currentSchema, TableSchema requiredSchema, DestinationConfiguration destinationConfig)
    {
        var schemaDiff = new SchemaDifference();

        // Find new columns
        foreach (var requiredColumn in requiredSchema.Columns)
        {
            if (!currentSchema.Columns.ContainsKey(requiredColumn.Key))
            {
                schemaDiff.NewColumns.Add(requiredColumn.Key, requiredColumn.Value);
            }
        }

        // Find modified columns
        foreach (var requiredColumn in requiredSchema.Columns)
        {
            if (currentSchema.Columns.TryGetValue(requiredColumn.Key, out var existingColumn))
            {
                var changes = DetectColumnChanges(existingColumn, requiredColumn.Value);
                if (changes.HasChanges)
                {
                    schemaDiff.ModifiedColumns.Add(requiredColumn.Key, changes);
                }
            }
        }

        // Find dropped columns (but exclude protected system columns)
        var protectedColumns = new HashSet<string> { "id", "Id" };
        foreach (var currentColumn in currentSchema.Columns)
        {
            if (!requiredSchema.Columns.ContainsKey(currentColumn.Key) && 
                !protectedColumns.Contains(currentColumn.Key))
            {
                schemaDiff.DroppedColumns.Add(currentColumn.Key, currentColumn.Value);
            }
        }

        return schemaDiff;
    }

    private ColumnChange DetectColumnChanges(ColumnSchema existing, ColumnSchema required)
    {
        var change = new ColumnChange
        {
            ColumnName = existing.Name,
            ExistingColumn = existing,
            RequiredColumn = required
        };

        var existingTypeNormalized = NormalizePostgreSqlType(existing.DataType);
        var requiredTypeNormalized = NormalizePostgreSqlType(required.DataType);

        if (existingTypeNormalized != requiredTypeNormalized)
        {
            change.DataTypeChanged = true;
        }

        if (existing.IsNullable != required.IsNullable)
        {
            change.NullabilityChanged = true;
        }

        var existingDefault = NormalizeDefaultValue(existing.DefaultValue);
        var requiredDefault = NormalizeDefaultValue(required.DefaultValue);
        
        if (existingDefault != requiredDefault)
        {
            change.DefaultValueChanged = true;
        }

        if (existing.MaxLength != required.MaxLength)
        {
            change.MaxLengthChanged = true;
        }

        return change;
    }

    private async Task ExecuteSchemaChanges(SchemaDifference schemaDiff, string quotedFullTableName, DestinationConfiguration destinationConfig)
    {
        var options = destinationConfig.SchemaAlteration;

        // 1. Add new columns first (if enabled)
        if (options.AutoAddColumns)
        {
            foreach (var newColumn in schemaDiff.NewColumns)
            {
                await AddColumnAsync(quotedFullTableName, newColumn.Key, newColumn.Value);
            }
        }
        else if (schemaDiff.NewColumns.Any())
        {
            _logger.LogWarning("Schema alteration detected {NewColumnCount} new columns but AutoAddColumns is disabled. Columns: {ColumnNames}",
                schemaDiff.NewColumns.Count, string.Join(", ", schemaDiff.NewColumns.Keys));
        }

        // 2. Modify existing columns (if enabled)
        if (options.AutoModifyColumns)
        {
            foreach (var modifiedColumn in schemaDiff.ModifiedColumns)
            {
                await ModifyColumnAsync(quotedFullTableName, modifiedColumn.Value);
            }
        }
        else if (schemaDiff.ModifiedColumns.Any())
        {
            _logger.LogWarning("Schema alteration detected {ModifiedColumnCount} modified columns but AutoModifyColumns is disabled. Columns: {ColumnNames}",
                schemaDiff.ModifiedColumns.Count, string.Join(", ", schemaDiff.ModifiedColumns.Keys));
        }

        // 3. Drop removed columns (if enabled and with safety checks)
        if (options.AutoDropColumns)
        {
            foreach (var droppedColumn in schemaDiff.DroppedColumns)
            {
                await DropColumnAsync(quotedFullTableName, droppedColumn.Key, droppedColumn.Value, options);
            }
        }
        else if (schemaDiff.DroppedColumns.Any())
        {
            _logger.LogWarning("Schema alteration detected {DroppedColumnCount} dropped columns but AutoDropColumns is disabled. Columns: {ColumnNames}",
                schemaDiff.DroppedColumns.Count, string.Join(", ", schemaDiff.DroppedColumns.Keys));
        }

        // 4. Update indexes and constraints (if enabled)
        if (options.AutoUpdateConstraints)
        {
            await UpdateConstraintsAsync(quotedFullTableName, destinationConfig);
        }
    }

    private async Task AddColumnAsync(string quotedFullTableName, string columnName, ColumnSchema columnSchema)
    {
        try
        {
            var columnDefinition = BuildColumnDefinitionFromSchema(columnSchema);
            var sql = $"ALTER TABLE {quotedFullTableName} ADD COLUMN {columnDefinition}";
            
            await _databaseService.ExecuteCommandAsync(sql);
            _logger.LogInformation("Added column {ColumnName} to table {TableName}: {Definition}", 
                columnName, quotedFullTableName, columnDefinition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add column {ColumnName} to table {TableName}", 
                columnName, quotedFullTableName);
            throw;
        }
    }

    private async Task ModifyColumnAsync(string quotedFullTableName, ColumnChange change)
    {
        try
        {
            var commands = new List<string>();

            // Data type change
            if (change.DataTypeChanged)
            {
                var newType = ConvertToCreateTableDataType(change.RequiredColumn.DataType, 
                    change.RequiredColumn.MaxLength, null);
                
                // For safety, try to cast the data type
                commands.Add($"ALTER TABLE {quotedFullTableName} ALTER COLUMN \"{change.ColumnName}\" TYPE {newType} USING \"{change.ColumnName}\"::{newType}");
                
                _logger.LogInformation("Will change data type for column {ColumnName} from {OldType} to {NewType}",
                    change.ColumnName, change.ExistingColumn.DataType, newType);
            }

            // Nullability change
            if (change.NullabilityChanged)
            {
                var nullConstraint = change.RequiredColumn.IsNullable ? "DROP NOT NULL" : "SET NOT NULL";
                commands.Add($"ALTER TABLE {quotedFullTableName} ALTER COLUMN \"{change.ColumnName}\" {nullConstraint}");
                
                _logger.LogInformation("Will change nullability for column {ColumnName} to {Nullable}",
                    change.ColumnName, change.RequiredColumn.IsNullable ? "NULL" : "NOT NULL");
            }

            // Max length change (for VARCHAR, CHAR, etc.)
            if (change.MaxLengthChanged && !change.DataTypeChanged)
            {
                var newType = ConvertToCreateTableDataType(change.RequiredColumn.DataType, 
                    change.RequiredColumn.MaxLength, null);
                
                // For safety, try to cast the data type with new length
                commands.Add($"ALTER TABLE {quotedFullTableName} ALTER COLUMN \"{change.ColumnName}\" TYPE {newType} USING \"{change.ColumnName}\"::{newType}");
                
                _logger.LogInformation("Will change max length for column {ColumnName} from {OldLength} to {NewLength}",
                    change.ColumnName, change.ExistingColumn.MaxLength, change.RequiredColumn.MaxLength);
            }

            // Default value change
            if (change.DefaultValueChanged)
            {
                if (string.IsNullOrEmpty(change.RequiredColumn.DefaultValue))
                {
                    commands.Add($"ALTER TABLE {quotedFullTableName} ALTER COLUMN \"{change.ColumnName}\" DROP DEFAULT");
                }
                else
                {
                    commands.Add($"ALTER TABLE {quotedFullTableName} ALTER COLUMN \"{change.ColumnName}\" SET DEFAULT {change.RequiredColumn.DefaultValue}");
                }
                
                _logger.LogInformation("Will change default value for column {ColumnName} to {DefaultValue}",
                    change.ColumnName, change.RequiredColumn.DefaultValue ?? "(no default)");
            }

            // Execute all column modifications
            foreach (var command in commands)
            {
                await _databaseService.ExecuteCommandAsync(command);
            }

            if (commands.Any())
            {
                _logger.LogInformation("Successfully modified column {ColumnName} in table {TableName}", 
                    change.ColumnName, quotedFullTableName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to modify column {ColumnName} in table {TableName}", 
                change.ColumnName, quotedFullTableName);
            throw;
        }
    }

    private async Task DropColumnAsync(string quotedFullTableName, string columnName, ColumnSchema columnSchema, SchemaAlterationOptions options)
    {
        try
        {
            // Safety check - don't drop columns with data unless explicitly configured
            if (!options.AllowDropColumnsWithData)
            {
                var hasData = await ColumnHasDataAsync(quotedFullTableName, columnName, options.MaxRowsToCheckForData);
                if (hasData)
                {
                    _logger.LogWarning("Column {ColumnName} in table {TableName} contains data and AllowDropColumnsWithData is false. Column will not be dropped.",
                        columnName, quotedFullTableName);
                    return;
                }
            }

            var sql = $"ALTER TABLE {quotedFullTableName} DROP COLUMN IF EXISTS \"{columnName}\"";
            await _databaseService.ExecuteCommandAsync(sql);
            
            _logger.LogInformation("Dropped column {ColumnName} from table {TableName}", 
                columnName, quotedFullTableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to drop column {ColumnName} from table {TableName}", 
                columnName, quotedFullTableName);
            throw;
        }
    }

    private async Task<bool> ColumnHasDataAsync(string quotedFullTableName, string columnName, int maxRowsToCheck = 1000)
    {
        try
        {
            var sql = $"SELECT COUNT(*) FROM (SELECT 1 FROM {quotedFullTableName} WHERE \"{columnName}\" IS NOT NULL LIMIT {maxRowsToCheck}) AS limited";
            var count = await _databaseService.ExecuteScalarAsync<long>(sql);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check if column {ColumnName} has data, assuming it does for safety", columnName);
            return true; // Assume it has data for safety
        }
    }

    private async Task UpdateConstraintsAsync(string quotedFullTableName, DestinationConfiguration destinationConfig)
    {
        try
        {
            var tableName = destinationConfig.TableName;
            
            // Update unique indexes
            foreach (var fieldMapping in destinationConfig.FieldMappings.Where(f => f.IsUnique))
            {
                var indexName = $"IX_{tableName}_{fieldMapping.DestinationField}";
                var checkIndexSql = $"SELECT COUNT(*) FROM pg_indexes WHERE indexname = '{indexName}'";
                var indexExists = await _databaseService.ExecuteScalarAsync<long>(checkIndexSql) > 0;
                
                if (!indexExists)
                {
                    var createIndexSql = $"CREATE UNIQUE INDEX \"{indexName}\" ON {quotedFullTableName} (\"{fieldMapping.DestinationField}\")";
                    await _databaseService.ExecuteCommandAsync(createIndexSql);
                    _logger.LogInformation("Created unique index {IndexName} for column {ColumnName}", 
                        indexName, fieldMapping.DestinationField);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update constraints for table {TableName}", quotedFullTableName);
            // Don't throw - constraint failures shouldn't stop the sync
        }
    }

    #endregion

    #region Helper Classes and Methods

    private class SchemaDifference
    {
        public Dictionary<string, ColumnSchema> NewColumns { get; set; } = new();
        public Dictionary<string, ColumnChange> ModifiedColumns { get; set; } = new();
        public Dictionary<string, ColumnSchema> DroppedColumns { get; set; } = new();
        
        public bool HasChanges => NewColumns.Any() || ModifiedColumns.Any() || DroppedColumns.Any();
    }

    private class ColumnChange
    {
        public string ColumnName { get; set; } = string.Empty;
        public ColumnSchema ExistingColumn { get; set; } = new();
        public ColumnSchema RequiredColumn { get; set; } = new();
        
        public bool DataTypeChanged { get; set; }
        public bool NullabilityChanged { get; set; }
        public bool DefaultValueChanged { get; set; }
        public bool MaxLengthChanged { get; set; }
        
        public bool HasChanges => DataTypeChanged || NullabilityChanged || DefaultValueChanged || MaxLengthChanged;
    }

    private string BuildColumnDefinitionFromSchema(ColumnSchema columnSchema)
    {
        var dataType = ConvertToCreateTableDataType(columnSchema.DataType, columnSchema.MaxLength, null);
        var nullable = columnSchema.IsNullable ? "NULL" : "NOT NULL";
        var defaultValue = !string.IsNullOrEmpty(columnSchema.DefaultValue) ? $"DEFAULT {columnSchema.DefaultValue}" : "";
        
        return $"\"{columnSchema.Name}\" {dataType} {nullable} {defaultValue}".Trim();
    }

    private string ExtractDataTypeFromPostgres(string postgresType)
    {
        // Convert PostgreSQL CREATE TABLE syntax back to information_schema format
        var type = postgresType.ToLowerInvariant();
        
        if (type.StartsWith("varchar"))
            return "character varying";
        if (type.StartsWith("char"))
            return "character";
        if (type == "timestamp")
            return "timestamp without time zone";
        if (type == "timestamptz")
            return "timestamp with time zone";
        if (type == "float8" || type == "double precision")
            return "double precision";
        if (type == "float4" || type == "real")
            return "real";
        if (type == "int8" || type == "bigint")
            return "bigint";
        if (type == "int2" || type == "smallint")
            return "smallint";
        if (type == "int4" || type == "integer")
            return "integer";
            
        return type;
    }

    private int? ExtractMaxLengthFromType(string postgresType)
    {
        var match = System.Text.RegularExpressions.Regex.Match(postgresType, @"\((\d+)\)");
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    private string NormalizeDefaultValue(string? defaultValue)
    {
        if (string.IsNullOrEmpty(defaultValue))
            return string.Empty;
            
        return defaultValue.Trim().ToLowerInvariant()
            .Replace("'", "")  // Remove quotes
            .Replace("::text", "")  // Remove PostgreSQL type casts
            .Replace("::integer", "")
            .Replace("now()", "current_timestamp");
    }

    #endregion
    
    private bool DoesColumnNeedModification(ColumnSchema existing, ParsedColumnDefinition required)
    {
        // Simple check for major differences - you might want to make this more sophisticated
        var existingTypeNormalized = NormalizePostgreSqlType(existing.DataType);
        var requiredTypeNormalized = NormalizePostgreSqlType(required.DataType);
        
        return existingTypeNormalized != requiredTypeNormalized || 
               existing.IsNullable != required.IsNullable;
    }
    
    private string NormalizePostgreSqlType(string dataType)
    {
        // Normalize common PostgreSQL type variations
        return dataType?.ToLowerInvariant() switch
        {
            "character varying" => "varchar",
            "character" => "char", 
            "timestamp without time zone" => "timestamp",
            "timestamp with time zone" => "timestamptz",
            "double precision" => "float8",
            "real" => "float4",
            "bigint" => "int8",
            "smallint" => "int2",
            "integer" => "int4",
            var type => type ?? "unknown"
        };
    }
    
    private ParsedColumnDefinition ParseColumnDefinition(string columnDefinition)
    {
        // Simple parser for column definitions like: "ColumnName" TYPE [NOT] NULL [DEFAULT value]
        var parts = columnDefinition.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var parsed = new ParsedColumnDefinition();
        
        if (parts.Length >= 2)
        {
            // Skip quoted column name (first part) and get the data type
            parsed.DataType = parts[1];
            
            // Check for nullability
            var definitionUpper = columnDefinition.ToUpperInvariant();
            parsed.IsNullable = !definitionUpper.Contains("NOT NULL");
        }
        
        return parsed;
    }
    
    private class ParsedColumnDefinition
    {
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; } = true;
    }

    public async Task<TableSchema> GetTableSchemaAsync(string tableName, string? schema = null)
    {
        var schemaName = schema ?? "public";
        
        var sql = @"
            SELECT 
                column_name,
                data_type,
                is_nullable,
                column_default,
                character_maximum_length
            FROM information_schema.columns 
            WHERE table_name = @tableName
            AND table_schema = @schemaName
            ORDER BY ordinal_position";

        var tableSchema = new TableSchema 
        { 
            Name = tableName, 
            Schema = schema 
        };

        try
        {
            using var connection = await _databaseService.GetConnectionAsync();
            
            // Try with exact case first
            var columns = await ExecuteSchemaQuery(connection, sql, tableName, schemaName);
            
            // If no columns found and table name isn't already lowercase, try lowercase
            if (!columns.Any() && tableName != tableName.ToLowerInvariant())
            {
                var lowerTableName = tableName.ToLowerInvariant();
                _logger.LogDebug("No columns found for {TableName}, trying lowercase {LowerTableName}", tableName, lowerTableName);
                columns = await ExecuteSchemaQuery(connection, sql, lowerTableName, schemaName);
            }
            
            foreach (var column in columns)
            {
                tableSchema.Columns[column.Name] = column;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get table schema for {TableName}.{Schema}", tableName, schemaName);
            throw;
        }

        return tableSchema;
    }
    
    private async Task<List<ColumnSchema>> ExecuteSchemaQuery(System.Data.Common.DbConnection connection, string sql, string tableName, string schemaName)
    {
        var columns = new List<ColumnSchema>();
        
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var tableNameParam = command.CreateParameter();
        tableNameParam.ParameterName = "@tableName";
        tableNameParam.Value = tableName;
        command.Parameters.Add(tableNameParam);
        
        var schemaNameParam = command.CreateParameter();
        schemaNameParam.ParameterName = "@schemaName";
        schemaNameParam.Value = schemaName;
        command.Parameters.Add(schemaNameParam);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var columnName = reader["column_name"].ToString() ?? string.Empty;
            var dataType = reader["data_type"].ToString() ?? string.Empty;
            var isNullable = reader["is_nullable"]?.ToString() == "YES";
            var defaultValue = reader["column_default"] == DBNull.Value ? null : reader["column_default"]?.ToString();
            var maxLength = reader["character_maximum_length"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["character_maximum_length"]);

            columns.Add(new ColumnSchema
            {
                Name = columnName,
                DataType = dataType,
                IsNullable = isNullable,
                DefaultValue = defaultValue,
                MaxLength = maxLength
            });
        }
        
        return columns;
    }

    public async Task<IEnumerable<string>> GetExistingRecordKeysAsync(string tableName, string keyField, IEnumerable<string> sourceKeys)
    {
        if (!sourceKeys.Any()) return Enumerable.Empty<string>();

        try
        {
            var keysArray = sourceKeys.ToArray();
            // Quote both table name and key field to preserve case
            var sql = $"SELECT DISTINCT \"{keyField}\" FROM \"{tableName}\" WHERE \"{keyField}\" = ANY(@keys)";
            
            var existingKeys = new List<string>();
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@keys";
            parameter.Value = keysArray;
            command.Parameters.Add(parameter);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                existingKeys.Add(reader.GetString(0));
            }

            return existingKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get existing record keys from {TableName}", tableName);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Gets the PostgreSQL data type of a specific field in a lookup table
    /// </summary>
    private async Task<string> GetLookupFieldDataTypeAsync(string tableName, string fieldName, string? schema = null)
    {
        var schemaName = schema ?? "public";
        
        try
        {
            var sql = @"
                SELECT data_type, 
                       COALESCE(character_maximum_length, numeric_precision) as max_length,
                       numeric_scale
                FROM information_schema.columns 
                WHERE table_name = @tableName
                AND table_schema = @schemaName
                AND column_name = @fieldName";

            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var tableNameParam = command.CreateParameter();
            tableNameParam.ParameterName = "@tableName";
            tableNameParam.Value = tableName;
            command.Parameters.Add(tableNameParam);
            
            var schemaNameParam = command.CreateParameter();
            schemaNameParam.ParameterName = "@schemaName";
            schemaNameParam.Value = schemaName;
            command.Parameters.Add(schemaNameParam);
            
            var fieldNameParam = command.CreateParameter();
            fieldNameParam.ParameterName = "@fieldName";
            fieldNameParam.Value = fieldName;
            command.Parameters.Add(fieldNameParam);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var dataType = reader["data_type"]?.ToString() ?? "integer";
                var maxLength = reader["max_length"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["max_length"]);
                var numericScale = reader["numeric_scale"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["numeric_scale"]);
                
                // Convert PostgreSQL information_schema types to CREATE TABLE syntax
                return ConvertToCreateTableDataType(dataType, maxLength, numericScale);
            }
            
            // If field not found with exact case, try lowercase (for unquoted identifiers)
            var lowerFieldName = fieldName.ToLowerInvariant();
            if (fieldName != lowerFieldName)
            {
                fieldNameParam.Value = lowerFieldName;
                using var reader2 = await command.ExecuteReaderAsync();
                
                if (await reader2.ReadAsync())
                {
                    var dataType = reader2["data_type"]?.ToString() ?? "integer";
                    var maxLength = reader2["max_length"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader2["max_length"]);
                    var numericScale = reader2["numeric_scale"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader2["numeric_scale"]);
                    
                    return ConvertToCreateTableDataType(dataType, maxLength, numericScale);
                }
            }
            
            _logger.LogWarning("Field {FieldName} not found in table {TableName}.{Schema}, defaulting to INTEGER", 
                fieldName, tableName, schemaName);
            return "INTEGER";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data type for field {FieldName} in table {TableName}.{Schema}, defaulting to INTEGER", 
                fieldName, tableName, schemaName);
            return "INTEGER";
        }
    }
    
    /// <summary>
    /// Converts PostgreSQL information_schema data types to CREATE TABLE syntax
    /// </summary>
    private string ConvertToCreateTableDataType(string dataType, int? maxLength, int? numericScale)
    {
        return dataType.ToLowerInvariant() switch
        {
            "character varying" => maxLength.HasValue ? $"VARCHAR({maxLength})" : "VARCHAR",
            "character" => maxLength.HasValue ? $"CHAR({maxLength})" : "CHAR",
            "text" => "TEXT",
            "integer" => "INTEGER",
            "bigint" => "BIGINT",
            "smallint" => "SMALLINT",
            "numeric" => numericScale.HasValue && maxLength.HasValue ? $"NUMERIC({maxLength},{numericScale})" : 
                        maxLength.HasValue ? $"NUMERIC({maxLength})" : "NUMERIC",
            "decimal" => numericScale.HasValue && maxLength.HasValue ? $"DECIMAL({maxLength},{numericScale})" : 
                        maxLength.HasValue ? $"DECIMAL({maxLength})" : "DECIMAL",
            "real" => "REAL",
            "double precision" => "DOUBLE PRECISION",
            "boolean" => "BOOLEAN",
            "timestamp without time zone" => "TIMESTAMP",
            "timestamp with time zone" => "TIMESTAMPTZ",
            "date" => "DATE",
            "time without time zone" => "TIME",
            "time with time zone" => "TIMETZ",
            "uuid" => "UUID",
            "json" => "JSON",
            "jsonb" => "JSONB",
            _ => dataType.ToUpperInvariant() // Fallback to original type in uppercase
        };
    }

    private string BuildColumnDefinition(FieldMapping mapping)
    {
        var postgresType = _dataTypeMapper.MapToPostgreSqlType(mapping.DataType);
        var nullable = mapping.IsRequired ? "NOT NULL" : "NULL";
        var defaultValue = !string.IsNullOrEmpty(mapping.DefaultValue) ? $"DEFAULT '{mapping.DefaultValue}'" : "";
        
        // Quote the column name to preserve exact case from configuration
        return $"\"{mapping.DestinationField}\" {postgresType} {nullable} {defaultValue}".Trim();
    }

    private async Task<Dictionary<string, string>> GetRequiredColumns(DestinationConfiguration destinationConfig)
    {
        var columns = new Dictionary<string, string>();

        // Add mapped fields with quoted names
        foreach (var mapping in destinationConfig.FieldMappings)
        {
            columns[mapping.DestinationField] = BuildColumnDefinition(mapping);
        }

        // Add FK columns with quoted names and dynamic data types
        foreach (var fkMapping in destinationConfig.ForeignKeyMappings)
        {
            var nullable = fkMapping.IsRequired ? "NOT NULL" : "NULL";
            var dataType = await GetLookupFieldDataTypeAsync(fkMapping.LookupTable, fkMapping.LookupReturnField);
            columns[fkMapping.DestinationField] = $"\"{fkMapping.DestinationField}\" {dataType} {nullable}";
        }

        // Add audit fields with quoted names to preserve case
        columns[destinationConfig.AuditFields.CreatedBy] = $"\"{destinationConfig.AuditFields.CreatedBy}\" INTEGER NOT NULL DEFAULT 0";
        columns[destinationConfig.AuditFields.CreatedDate] = $"\"{destinationConfig.AuditFields.CreatedDate}\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP";
        columns[destinationConfig.AuditFields.LastModifiedBy] = $"\"{destinationConfig.AuditFields.LastModifiedBy}\" INTEGER NOT NULL DEFAULT 0";
        columns[destinationConfig.AuditFields.LastModifiedDate] = $"\"{destinationConfig.AuditFields.LastModifiedDate}\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP";
        columns[destinationConfig.AuditFields.SyncDate] = $"\"{destinationConfig.AuditFields.SyncDate}\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP";
        columns[destinationConfig.AuditFields.SyncBatchId] = $"\"{destinationConfig.AuditFields.SyncBatchId}\" VARCHAR(50) NOT NULL";
        columns[destinationConfig.AuditFields.SourceSystem] = $"\"{destinationConfig.AuditFields.SourceSystem}\" VARCHAR(100) NOT NULL";
        columns[destinationConfig.AuditFields.IsDeleted] = $"\"{destinationConfig.AuditFields.IsDeleted}\" BOOLEAN NOT NULL DEFAULT FALSE";

        return columns;
    }

    private object CreateParameter(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        return parameter;
    }
}
