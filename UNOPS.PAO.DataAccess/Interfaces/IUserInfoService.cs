using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.DataAccess.Interfaces;

public interface IUserInfoService
{
    Task<UserProfile?> GetUserInfoByEmailAsync(string email);
    Task<object?> GetUserInfoWithOrgSettingsAsync(string email);
    Task<UserProfile?> UpdateUserInfoAsync(UserProfile userProfile);
    Task<List<UserProfile>> GetUserInfosByEmailsAsync(IEnumerable<string> emails);
} 