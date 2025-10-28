using UNOPS.PAO.Models.Users;

namespace UNOPS.PAO.Business.Interfaces;
public interface IUserDataManager
{
    Task<PAOUserModel?> GetUserByIdAsync(int id);
    Task<PAOUserModel?> GetCurrentUserAsync();
    Task<PAOUserModel?> GetUserByEmailAsync(string email);
    Task<List<PAOUserModel>> GetUsersByEmailsAsync(IEnumerable<string> emails);
}
