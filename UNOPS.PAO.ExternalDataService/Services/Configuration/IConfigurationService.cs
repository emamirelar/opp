using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.External;

namespace UNOPS.PAO.ExternalDataService.Services.Configuration;

public interface IConfigurationService
{
    Task<IEnumerable<SyncConfiguration>> LoadAllConfigurationsAsync();
    Task<SyncConfiguration?> LoadConfigurationAsync(string name);
    Task<bool> ValidateConfigurationAsync(SyncConfiguration configuration);
    Task RefreshConfigurationsAsync();
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
}
