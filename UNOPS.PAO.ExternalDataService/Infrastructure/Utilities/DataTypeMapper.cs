namespace UNOPS.PAO.ExternalDataService.Infrastructure.Utilities;

public class DataTypeMapper
{
    private readonly ILogger<DataTypeMapper> _logger;

    public DataTypeMapper(ILogger<DataTypeMapper> logger)
    {
        _logger = logger;
    }

    public string MapToPostgreSqlType(string sourceType)
    {
        if (string.IsNullOrEmpty(sourceType))
        {
            _logger.LogWarning("Empty source type provided, defaulting to TEXT");
            return "TEXT";
        }

        var normalizedType = sourceType.ToLower().Trim();
        
        // Handle parameterized types (e.g., varchar(255))
        if (normalizedType.Contains('('))
        {
            var baseType = normalizedType.Substring(0, normalizedType.IndexOf('('));
            var parameters = normalizedType.Substring(normalizedType.IndexOf('('));
            
            var mappedBaseType = MapBaseType(baseType);
            
            // Only append parameters if the mapped type supports them
            if (TypeSupportsParameters(mappedBaseType))
            {
                return mappedBaseType + parameters;
            }
            
            return mappedBaseType;
        }
        
        return MapBaseType(normalizedType);
    }

    private string MapBaseType(string baseType)
    {
        return baseType switch
        {
            // String types
            "varchar" or "nvarchar" or "string" => "VARCHAR",
            "char" or "nchar" => "CHAR",
            "text" or "ntext" or "longtext" => "TEXT",
            
            // Numeric types
            "int" or "integer" or "int32" => "INTEGER",
            "bigint" or "long" or "int64" => "BIGINT",
            "smallint" or "short" or "int16" => "SMALLINT",
            "tinyint" or "byte" => "SMALLINT", // PostgreSQL doesn't have TINYINT
            
            // Decimal types
            "decimal" or "numeric" => "DECIMAL",
            "money" => "MONEY",
            "float" or "real" or "single" => "REAL",
            "double" or "double precision" => "DOUBLE PRECISION",
            
            // Boolean
            "boolean" or "bool" or "bit" => "BOOLEAN",
            
            // Date/Time types
            "date" => "DATE",
            "time" => "TIME",
            "datetime" or "datetime2" or "timestamp" => "TIMESTAMP",
            "datetimeoffset" => "TIMESTAMP WITH TIME ZONE",
            "interval" or "timespan" => "INTERVAL",
            
            // Binary types
            "binary" or "varbinary" or "image" or "blob" => "BYTEA",
            
            // Other types
            "uuid" or "guid" or "uniqueidentifier" => "UUID",
            "json" => "JSON",
            "jsonb" => "JSONB",
            "xml" => "XML",
            
            // Default fallback
            _ => MapUnknownType(baseType)
        };
    }

    private string MapUnknownType(string unknownType)
    {
        _logger.LogWarning("Unknown data type '{UnknownType}', mapping to TEXT", unknownType);
        
        // Try to infer from common patterns
        if (unknownType.Contains("int"))
            return "INTEGER";
        if (unknownType.Contains("char") || unknownType.Contains("string"))
            return "VARCHAR(255)";
        if (unknownType.Contains("date") || unknownType.Contains("time"))
            return "TIMESTAMP";
        if (unknownType.Contains("bool"))
            return "BOOLEAN";
        if (unknownType.Contains("float") || unknownType.Contains("double") || unknownType.Contains("decimal"))
            return "DECIMAL";
            
        return "TEXT";
    }

    private bool TypeSupportsParameters(string postgresType)
    {
        var typesWithParameters = new[]
        {
            "VARCHAR", "CHAR", "DECIMAL", "NUMERIC", "TIME", "TIMESTAMP", "INTERVAL"
        };
        
        return typesWithParameters.Any(t => postgresType.StartsWith(t, StringComparison.OrdinalIgnoreCase));
    }

    public string GetDefaultConstraint(string dataType, string? defaultValue)
    {
        if (string.IsNullOrEmpty(defaultValue))
            return "";

        var normalizedType = dataType.ToLower();
        
        return normalizedType switch
        {
            var t when t.Contains("varchar") || t.Contains("char") || t.Contains("text") => $"DEFAULT '{defaultValue}'",
            var t when t.Contains("int") || t.Contains("decimal") || t.Contains("numeric") || t.Contains("real") || t.Contains("double") => $"DEFAULT {defaultValue}",
            var t when t.Contains("boolean") => $"DEFAULT {defaultValue.ToUpper()}",
            var t when t.Contains("timestamp") || t.Contains("date") => 
                defaultValue.ToUpper() switch
                {
                    "NOW" or "CURRENT_TIMESTAMP" => "DEFAULT CURRENT_TIMESTAMP",
                    "CURRENT_DATE" => "DEFAULT CURRENT_DATE",
                    _ => $"DEFAULT '{defaultValue}'"
                },
            var t when t.Contains("uuid") => 
                defaultValue.ToUpper() == "NEWID" || defaultValue.ToUpper() == "UUID" 
                    ? "DEFAULT gen_random_uuid()" 
                    : $"DEFAULT '{defaultValue}'",
            _ => $"DEFAULT '{defaultValue}'"
        };
    }

    public bool IsNumericType(string dataType)
    {
        var normalizedType = dataType.ToLower();
        var numericTypes = new[] { "int", "bigint", "smallint", "decimal", "numeric", "real", "double", "money" };
        
        return numericTypes.Any(t => normalizedType.Contains(t));
    }

    public bool IsStringType(string dataType)
    {
        var normalizedType = dataType.ToLower();
        var stringTypes = new[] { "varchar", "char", "text", "json", "xml" };
        
        return stringTypes.Any(t => normalizedType.Contains(t));
    }

    public bool IsDateTimeType(string dataType)
    {
        var normalizedType = dataType.ToLower();
        var dateTimeTypes = new[] { "date", "time", "timestamp", "interval" };
        
        return dateTimeTypes.Any(t => normalizedType.Contains(t));
    }
}
