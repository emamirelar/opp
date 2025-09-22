using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.ExternalDataService.Models.Configuration;

public class DestinationConfiguration
{
    [Required]
    public string TableName { get; set; } = string.Empty;
    public string? Schema { get; set; }
    [Required, MinLength(1)]
    public List<FieldMapping> FieldMappings { get; set; } = new();
    public List<ForeignKeyMapping> ForeignKeyMappings { get; set; } = new();
    public AuditFieldsConfiguration AuditFields { get; set; } = new();
    public SchemaAlterationOptions SchemaAlteration { get; set; } = new();
}

public class FieldMapping
{
    [Required]
    public string SourceField { get; set; } = string.Empty;
    [Required]
    public string DestinationField { get; set; } = string.Empty;
    [Required]
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public bool IsUnique { get; set; } = false;
    public string? DefaultValue { get; set; }
    public List<FieldTransformation> Transformations { get; set; } = new();
}

public class FieldTransformation
{
    [Required]
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ForeignKeyMapping
{
    [Required]
    public string SourceField { get; set; } = string.Empty;
    [Required]
    public string LookupTable { get; set; } = string.Empty;
    [Required]
    public string LookupField { get; set; } = string.Empty;
    [Required]
    public string DestinationField { get; set; } = string.Empty;
    public string LookupReturnField { get; set; } = "Id";
    public bool IsRequired { get; set; } = false;
    public LookupFailAction OnLookupFail { get; set; } = LookupFailAction.LogWarning;
}

public class AuditFieldsConfiguration
{
    public string CreatedBy { get; set; } = "CreatedBy";
    public string CreatedDate { get; set; } = "CreatedDate";
    public string LastModifiedBy { get; set; } = "LastModifiedBy";
    public string LastModifiedDate { get; set; } = "LastModifiedDate";
    public string SyncDate { get; set; } = "SyncDate";
    public string SyncBatchId { get; set; } = "SyncBatchId";
    public string SourceSystem { get; set; } = "SourceSystem";
    public string IsDeleted { get; set; } = "IsDeleted";
}

public class TableSchema
{
    public string Name { get; set; } = string.Empty;
    public string? Schema { get; set; }
    public Dictionary<string, ColumnSchema> Columns { get; set; } = new();
}

public class ColumnSchema
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; } = true;
    public bool IsPrimaryKey { get; set; } = false;
    public bool IsUnique { get; set; } = false;
    public string? DefaultValue { get; set; }
    public int? MaxLength { get; set; }
}

public enum LookupFailAction
{
    LogWarning = 0,
    SetNull = 1,
    FailRecord = 2
}

public class SchemaAlterationOptions
{
    /// <summary>
    /// Whether to automatically add new columns when they appear in the configuration
    /// </summary>
    public bool AutoAddColumns { get; set; } = true;
    
    /// <summary>
    /// Whether to automatically modify existing columns when their definition changes
    /// </summary>
    public bool AutoModifyColumns { get; set; } = true;
    
    /// <summary>
    /// Whether to automatically drop columns that are no longer in the configuration
    /// </summary>
    public bool AutoDropColumns { get; set; } = false;
    
    /// <summary>
    /// Whether to drop columns even if they contain data
    /// </summary>
    public bool AllowDropColumnsWithData { get; set; } = false;
    
    /// <summary>
    /// Whether to automatically update constraints (indexes, foreign keys)
    /// </summary>
    public bool AutoUpdateConstraints { get; set; } = true;
    
    /// <summary>
    /// Maximum number of rows to check when determining if a column has data
    /// </summary>
    public int MaxRowsToCheckForData { get; set; } = 1000;
}
