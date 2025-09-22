using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.ExternalDataService.Models.Configuration;

public class SourceConfiguration
{
    [Required]
    public string Type { get; set; } = "bigquery";
    public BigQueryConnection Connection { get; set; } = new();
    [Required]
    public string Query { get; set; } = string.Empty;
    [Required]
    public string PrimaryKeyField { get; set; } = string.Empty;
    public string? IncrementalField { get; set; }
    public int BatchSize { get; set; } = 1000;
}

public class BigQueryConnection
{
    public string? ProjectId { get; set; }  // Optional - falls back to appsettings.BigQuery.ProjectId
    public string? DatasetId { get; set; }
    public string? CredentialsFile { get; set; }
    public bool UseEnvironmentAuth { get; set; } = true;
}

public class DataSchema
{
    public Dictionary<string, FieldSchema> Fields { get; set; } = new();
}

public class FieldSchema
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public bool IsNullable { get; set; } = true;
    public int? MaxLength { get; set; }
}
