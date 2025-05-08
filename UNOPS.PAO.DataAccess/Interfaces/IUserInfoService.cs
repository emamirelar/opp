using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.DataAccess.Interfaces;

public interface IUserInfoService
{
    Task<UserInfo?> GetUserInfoByEmailAsync(string email);
} 