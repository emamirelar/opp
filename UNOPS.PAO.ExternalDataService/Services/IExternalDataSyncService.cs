using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.Sync;

namespace UNOPS.PAO.ExternalDataService.Services;

public interface IExternalDataSyncService
{
    Task<SyncResult> ExecuteSyncAsync(SyncConfiguration configuration, CancellationToken cancellationToken = default);
    Task<IEnumerable<SyncResult>> ExecuteAllConfigurationsAsync(CancellationToken cancellationToken = default);
    Task<SyncResult?> ExecuteConfigurationByNameAsync(string configurationName, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAvailableConfigurationsAsync();
    Task<Dictionary<string, object?>> GetSyncStatusAsync(string? configurationName = null);
    Task<Dictionary<string, object?>> GetSystemHealthAsync();
    Task<bool> ValidateConfigurationAsync(string configurationName);
}
