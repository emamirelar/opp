using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.DataAccess.Services;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    // Keep the original interface for backward compatibility
    public interface IUserLookupService : IEmailToUserIdResolver
    {
    }

    public class UserLookupService : IUserLookupService
    {
        private readonly UserManager<PAOIdentityUser> _userManager;

        public UserLookupService(UserManager<PAOIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<int> GetUserIdByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return 0;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && user.ActiveUser)
            {
                return user.Id;
            }

            // Return 0 if user not found or inactive
            return 0;
        }
    }
} 