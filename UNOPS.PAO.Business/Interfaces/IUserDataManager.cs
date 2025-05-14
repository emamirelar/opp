 using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;
public interface IUserDataManager
{
    Task<PAOUserModel?> GetUserByIdAsync(int id);
    Task<PAOUserModel?> GetCurrentUserAsync();
    Task<PAOUserModel?> GetUserByEmailAsync(string email);
}
