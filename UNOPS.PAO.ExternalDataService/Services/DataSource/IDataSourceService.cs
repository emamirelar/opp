using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.External;

namespace UNOPS.PAO.ExternalDataService.Services.DataSource;

public interface IDataSourceService
{
    Task<IEnumerable<ExternalDataRecord>> ExtractDataAsync(SourceConfiguration sourceConfig, DateTime? lastSyncDate = null);
    Task<bool> TestConnectionAsync(SourceConfiguration sourceConfig);
    Task<DataSchema> GetSourceSchemaAsync(SourceConfiguration sourceConfig);
}
