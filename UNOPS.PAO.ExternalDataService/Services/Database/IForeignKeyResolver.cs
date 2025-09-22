using UNOPS.PAO.ExternalDataService.Models.Configuration;

namespace UNOPS.PAO.ExternalDataService.Services.Database;

public interface IForeignKeyResolver
{
    Task<Dictionary<string, int?>> ResolveForeignKeysAsync(
        ForeignKeyMapping mapping, 
        IEnumerable<string> sourceValues);
    
    Task<int?> ResolveSingleForeignKeyAsync(
        ForeignKeyMapping mapping, 
        string sourceValue);
        
    Task<bool> ValidateForeignKeyMappingAsync(ForeignKeyMapping mapping);
}
