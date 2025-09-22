using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.Sync;

namespace UNOPS.PAO.ExternalDataService.Services.Sync;

public interface ISyncOrchestrator
{
    Task<SyncResult> ExecuteSyncAsync(SyncConfiguration configuration, CancellationToken cancellationToken = default);
    Task<bool> ValidateConfigurationAsync(SyncConfiguration configuration);
}
