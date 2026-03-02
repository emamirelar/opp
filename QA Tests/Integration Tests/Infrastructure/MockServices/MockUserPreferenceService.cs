using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IUserPreferenceService for integration tests.
/// Returns sensible defaults so endpoints that depend on user preferences
/// do not throw 500 InternalServerError in the InMemory test environment.
/// </summary>
public class MockUserPreferenceService : IUserPreferenceService
{
    public Task<int?> GetDefaultOrgUnitIdAsync(int userId)
        => Task.FromResult<int?>(null);

    public Task UpdateDefaultOrgUnitAsync(int userId, int? orgUnitId)
        => Task.CompletedTask;

    public Task<UserPreference?> GetUserPreferencesAsync(string userId)
    {
        if (int.TryParse(userId, out var uid))
        {
            return Task.FromResult<UserPreference?>(new UserPreference
            {
                Id = 1,
                Name = $"Preferences-{userId}",
                UserId = uid
            });
        }

        return Task.FromResult<UserPreference?>(null);
    }

    public Task UpdateUserPreferencesAsync(string userId, UserPreference userPreferences)
        => Task.CompletedTask;

    public Task<GlobalFilters> GetGlobalFiltersAsync(string userId)
        => Task.FromResult(new GlobalFilters());

    public Task UpdateGlobalFiltersAsync(string userId, GlobalFilters globalFilters)
        => Task.CompletedTask;

    public Task ResetGlobalFiltersAsync(string userId)
        => Task.CompletedTask;
}
