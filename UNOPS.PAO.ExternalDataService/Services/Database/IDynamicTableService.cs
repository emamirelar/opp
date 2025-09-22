using UNOPS.PAO.ExternalDataService.Models.Configuration;

namespace UNOPS.PAO.ExternalDataService.Services.Database;

public interface IDynamicTableService
{
    Task<bool> TableExistsAsync(string tableName, string? schema = null);
    Task CreateTableAsync(DestinationConfiguration destinationConfig);
    Task UpdateTableSchemaAsync(DestinationConfiguration destinationConfig);
    Task<TableSchema> GetTableSchemaAsync(string tableName, string? schema = null);
    Task<IEnumerable<string>> GetExistingRecordKeysAsync(string tableName, string keyField, IEnumerable<string> sourceKeys);
}
