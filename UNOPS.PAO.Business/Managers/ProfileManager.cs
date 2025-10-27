namespace UNOPS.PAO.Business.Managers;

using System.Threading.Tasks;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Utilities.Interfaces;


public class ProfileManager : IApplicationService
{
    private readonly AppDbContext appDbContext;

    public ProfileManager(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public ProfileModel Get(string? email)
    {
        var user = appDbContext.PAOUsers.FirstOrDefault(x => x.Email == email);
        if (user == null)
        {
            throw new BusinessException("User profile not found");
        }

        return new ProfileModel()
        {
            Email = email,
            FirstName = user.UserProfile?.FirstName ?? string.Empty,
            LastName = user.UserProfile?.LastName ?? string.Empty
        };
    }

    public async Task Update(ProfileModel profile)
    {
        var user = appDbContext.PAOUsers.FirstOrDefault(x => x.Email == profile.Email);

        if (user == null)
        {
            throw new BusinessException("User profile not found");
        }

        if (user.UserProfile == null)
        {
            user.UserProfile = new Domain.Entities.UserProfile();
        }

        user.UserProfile.FirstName = profile.FirstName;
        user.UserProfile.LastName = profile.LastName;

        await appDbContext.SaveChangesAsync();
    }
}