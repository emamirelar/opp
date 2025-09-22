using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.External;
using UNOPS.PAO.ExternalDataService.Models.Sync;

namespace UNOPS.PAO.ExternalDataService.Services.Sync;

public interface ISyncProcessor
{
    Task<SyncBatchResult> ProcessBatchAsync(
        IEnumerable<ExternalDataRecord> records, 
        SyncConfiguration configuration, 
        string batchId);
        
    Task<ExternalDataRecord> TransformRecordAsync(
        ExternalDataRecord record, 
        DestinationConfiguration destinationConfig);
        
    Task<SyncBatchResult> UpsertRecordsAsync(
        IEnumerable<ExternalDataRecord> records, 
        DestinationConfiguration destinationConfig, 
        string batchId);
}
