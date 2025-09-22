namespace UNOPS.PAO.ExternalDataService.Models.Sync;

public class SyncProcessedRecord
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public string PrimaryKey { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
    public string RecordAction { get; set; } = string.Empty; // 'inserted', 'updated', 'failed'
    public string? ErrorMessage { get; set; }
}

public enum RecordAction
{
    Inserted,
    Updated,
    Failed
}
