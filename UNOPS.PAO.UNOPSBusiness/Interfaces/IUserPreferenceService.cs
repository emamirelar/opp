using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

public interface IUserPreferenceService
{
    Task<int?> GetDefaultOrgUnitIdAsync(int userId);
    Task UpdateDefaultOrgUnitAsync(int userId, int? orgUnitId);
    Task<UserPreference?> GetUserPreferencesAsync(string userId);
    Task UpdateUserPreferencesAsync(string userId, UserPreference userPreferences);
    Task<GlobalFilters> GetGlobalFiltersAsync(string userId);
    Task UpdateGlobalFiltersAsync(string userId, GlobalFilters globalFilters);
    Task ResetGlobalFiltersAsync(string userId);
}