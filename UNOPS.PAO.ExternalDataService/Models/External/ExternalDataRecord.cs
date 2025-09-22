namespace UNOPS.PAO.ExternalDataService.Models.External;

public class ExternalDataRecord
{
    public string PrimaryKey { get; set; } = string.Empty;
    public Dictionary<string, object?> Data { get; set; } = new();
    public Dictionary<string, int?> ResolvedForeignKeys { get; set; } = new();
    public DateTime? LastModified { get; set; }
    public string? SourceSystem { get; set; }
}

public class ConfigurationChangedEventArgs : EventArgs
{
    public ConfigurationChangedEventArgs(object configuration, ChangeType changeType)
    {
        Configuration = configuration;
        ChangeType = changeType;
    }

    public object Configuration { get; }
    public ChangeType ChangeType { get; }
}

public enum ChangeType
{
    Created = 0,
    Updated = 1,
    Deleted = 2
}
