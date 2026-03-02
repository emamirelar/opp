using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IUserInfoService for testing
/// </summary>
public class MockUserInfoService : IUserInfoService
{
    public Task<UserProfile?> GetUserInfoByEmailAsync(string email)
    {
        if (email == "testuser@unops.org")
        {
            return Task.FromResult<UserProfile?>(new UserProfile
            {
                UserId = 123,
                UserEmail = "testuser@unops.org",
                FirstName = "Test User",
                OrgUnit = "HQ"
            });
        }
        return Task.FromResult<UserProfile?>(null);
    }

    public Task<object?> GetUserInfoWithOrgSettingsAsync(string email)
    {
        if (string.Equals(email, "testuser@unops.org", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<object?>(new
            {
                UserId = 123,
                UserEmail = "testuser@unops.org",
                FirstName = "Test",
                LastName = "User",
                OrgUnit = "HQ"
            });
        }
        return Task.FromResult<object?>(null);
    }

    public Task<UserProfile?> UpdateUserInfoAsync(UserProfile userProfile)
    {
        return Task.FromResult<UserProfile?>(userProfile);
    }

    public Task<List<UserProfile>> GetUserInfosByEmailsAsync(IEnumerable<string> emails)
    {
        return Task.FromResult(new List<UserProfile>());
    }
}

