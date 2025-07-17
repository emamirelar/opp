using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
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
        
        if (preference?.GlobalFilters?.OrgUnitId != null)
        {
            return preference.GlobalFilters.OrgUnitId;
        }

        // Fallback: get from UserInfo using email (proper way)
        return await GetDefaultOrgUnitIdFromUserInfoAsync();
    }

    /// <summary>
    /// Gets the default org unit ID from UserInfo table using the current user's email
    /// </summary>
    private async Task<int?> GetDefaultOrgUnitIdFromUserInfoAsync()
    {
        var userEmail = _userResolver.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
            return null;
            
        var userInfo = await _context.UserInfos
            .FirstOrDefaultAsync(ui => ui.UserEmail.ToLower() == userEmail.ToLower());
        
        if (userInfo?.OrgUnit != null)
        {
            var orgUnit = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(oh => oh.Code == userInfo.OrgUnit && oh.Type == OrganizationUnitType.OrgUnit);
            return orgUnit?.Id;
        }

        return null;
    }

    public async Task UpdateDefaultOrgUnitAsync(int userId, int? orgUnitId)
    {
        // If orgUnitId is not provided, get default from UserInfo using email
        if (orgUnitId == null)
        {
            orgUnitId = await GetDefaultOrgUnitIdFromUserInfoAsync();
        }
        
        // Now handle UserPreference
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);
        
        if (preference == null)
        {
            var globalFilters = new GlobalFilters
            {
                OrgUnitId = orgUnitId
            };
            
            preference = new UserPreference
            {
                UserId = userId,
                Name = $"UserPreferences_{userId}",
                GlobalFilters = globalFilters
            };
            _context.UserPreferences.Add(preference);
        }
        else
        {
            var globalFilters = preference.GlobalFilters ?? new GlobalFilters();
            globalFilters.OrgUnitId = orgUnitId;
            preference.GlobalFilters = globalFilters;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserPreference?> GetUserPreferencesAsync(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
            return null;
            
        return await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userIdInt);
    }

    public async Task UpdateUserPreferencesAsync(string userId, UserPreference userPreferences)
    {
        if (!int.TryParse(userId, out int userIdInt))
            return;
            
        // Ensure OrgUnitId always has a value in GlobalFilters - fall back to user's default if null
        if (userPreferences.GlobalFilters?.OrgUnitId == null)
        {
            var defaultOrgUnitId = await GetDefaultOrgUnitIdFromUserInfoAsync();
            
            if (defaultOrgUnitId != null)
            {
                if (userPreferences.GlobalFilters == null)
                {
                    userPreferences.GlobalFilters = new GlobalFilters();
                }
                userPreferences.GlobalFilters.OrgUnitId = defaultOrgUnitId;
            }
        }
        
        var existingPreference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userIdInt);
        
        if (existingPreference == null)
        {
            userPreferences.UserId = userIdInt;
            userPreferences.Name = $"UserPreferences_{userIdInt}";
            _context.UserPreferences.Add(userPreferences);
        }
        else
        {
            // Update existing preference
            existingPreference.GlobalFilters = userPreferences.GlobalFilters;
            existingPreference.AdditionalSettingsJson = userPreferences.AdditionalSettingsJson;
            
            // Explicitly mark the GlobalFilterJson property as modified to ensure EF detects the change
            _context.Entry(existingPreference).Property(p => p.GlobalFilterJson).IsModified = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<GlobalFilters> GetGlobalFiltersAsync(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
            return new GlobalFilters();
            
        var userPreferences = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userIdInt);
        
        return userPreferences?.GlobalFilters ?? new GlobalFilters();
    }

    public async Task UpdateGlobalFiltersAsync(string userId, GlobalFilters globalFilters)
    {
        if (!int.TryParse(userId, out int userIdInt))
            return;
            
        // Ensure OrgUnitId always has a value - fall back to user's default if null
        if (globalFilters.OrgUnitId == null)
        {
            var defaultOrgUnitId = await GetDefaultOrgUnitIdFromUserInfoAsync();
            globalFilters.OrgUnitId = defaultOrgUnitId;
        }
        
        var existingPreference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userIdInt);
        
        if (existingPreference == null)
        {
            var userPreference = new UserPreference
            {
                UserId = userIdInt,
                Name = $"UserPreferences_{userIdInt}",
                GlobalFilters = globalFilters
            };
            _context.UserPreferences.Add(userPreference);
        }
        else
        {
            existingPreference.GlobalFilters = globalFilters;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ResetGlobalFiltersAsync(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
            return;
            
        var existingPreference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userIdInt);
        
        if (existingPreference != null)
        {
            // Get user's default org unit from UserInfo using email
            var defaultOrgUnitId = await GetDefaultOrgUnitIdFromUserInfoAsync();
            
            // Reset to defaults but set user's default org unit
            existingPreference.GlobalFilters = new GlobalFilters
            {
                OrgUnitId = defaultOrgUnitId
            };
            await _context.SaveChangesAsync();
        }
    }
}