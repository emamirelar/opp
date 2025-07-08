using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly UNOPSAppDbContext _context;
    private readonly UserResolverService<int> _userResolver;

    public UserPreferenceService(UNOPSAppDbContext context, UserResolverService<int> userResolver)
    {
        _context = context;
        _userResolver = userResolver;
    }

    public async Task<int?> GetDefaultOrgUnitIdAsync(int userId)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);
        
        if (preference != null)
        {
            return preference.DefaultOrgUnitId;
        }

        // If no preference exists, try to get from UserInfo
        var userInfo = await _context.UserInfos
            .FirstOrDefaultAsync(ui => ui.UserId == userId);
        
        if (userInfo?.OrgUnit != null)
        {
            var orgUnit = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(oh => oh.Code == userInfo.OrgUnit);
            return orgUnit?.Id;
        }

        return null;
    }

    public async Task UpdateDefaultOrgUnitAsync(int userId, int? orgUnitId)
    {
        // First check if UserInfo exists for this user
        var userInfo = await _context.UserInfos
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        // If UserInfo doesn't exist, create it
        if (userInfo == null)
        {
            var userEmail = _userResolver.GetUserEmail();
            userInfo = new UserInfo
            {
                UserId = userId,
                UserEmail = userEmail,
                Name = userEmail?.Split('@').FirstOrDefault() ?? $"User_{userId}"
            };
            _context.UserInfos.Add(userInfo);
            await _context.SaveChangesAsync();
        }
        
        // Now handle UserPreference
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);
        
        if (preference == null)
        {
            preference = new UserPreference
            {
                UserId = userId,
                DefaultOrgUnitId = orgUnitId,
                Name = $"UserPreferences_{userId}"
            };
            _context.UserPreferences.Add(preference);
        }
        else
        {
            preference.DefaultOrgUnitId = orgUnitId;
        }

        await _context.SaveChangesAsync();
    }
}