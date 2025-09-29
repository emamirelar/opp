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

        // Fallback: get from UserProfile using email (proper way)
        return await GetDefaultOrgUnitIdFromUserProfileAsync();
    }

    /// <summary>
    /// Gets the default org unit ID from UserProfile table using the current user's email
    /// </summary>
    private async Task<int?> GetDefaultOrgUnitIdFromUserProfileAsync()
    {
        var userEmail = _userResolver.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
            return null;
            
        var userProfile = await _context.UserProfile
            .FirstOrDefaultAsync(up => up.UserEmail.ToLower() == userEmail.ToLower());
        
        if (userProfile?.OrgUnit != null)
        {
            var orgUnit = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(oh => oh.Code == userProfile.OrgUnit && oh.Type == OrganizationUnitType.OrgUnit);
            return orgUnit?.Id;
        }

        return null;
    }

    public async Task UpdateDefaultOrgUnitAsync(int userId, int? orgUnitId)
    {
        // If orgUnitId is not provided, get default from UserProfile using email
        if (orgUnitId == null)
        {
            orgUnitId = await GetDefaultOrgUnitIdFromUserProfileAsync();
        }
        
        // Now handle UserPreference
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userId);
        
        if (preference == null)
        {
            // Ensure UserProfile exists before creating UserPreference
            var userProfile = await _context.Set<UserProfile>().FirstOrDefaultAsync(up => up.UserId == userId);
            if (userProfile == null)
            {
                // Auto-create UserProfile if it doesn't exist
                await CreateUserProfileAsync(userId);
            }
            
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
            
        // Allow OrgUnitId to be null in GlobalFilters - users can choose to see everything
        
        var existingPreference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userIdInt);
        
        if (existingPreference == null)
        {
            // Ensure UserProfile exists before creating UserPreference
            var userProfile = await _context.Set<UserProfile>().FirstOrDefaultAsync(up => up.UserId == userIdInt);
            if (userProfile == null)
            {
                // Auto-create UserProfile if it doesn't exist
                await CreateUserProfileAsync(userIdInt);
            }
            
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
        
        var globalFilters = userPreferences?.GlobalFilters ?? new GlobalFilters();
        
        // Populate org unit name if orgUnitId exists
        if (globalFilters.OrgUnitId.HasValue)
        {
            var orgUnit = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(oh => oh.Id == globalFilters.OrgUnitId.Value);
            globalFilters.OrgUnitName = orgUnit?.Name;
        }
        
        return globalFilters;
    }

    public async Task UpdateGlobalFiltersAsync(string userId, GlobalFilters globalFilters)
    {
        if (!int.TryParse(userId, out int userIdInt))
            return;
            
        // Allow OrgUnitId to be null - users can choose to see everything
        
        var existingPreference = await _context.UserPreferences
            .FirstOrDefaultAsync(up => up.UserId == userIdInt);
        
        if (existingPreference == null)
        {
            // Ensure UserProfile exists before creating UserPreference
            var userProfile = await _context.Set<UserProfile>().FirstOrDefaultAsync(up => up.UserId == userIdInt);
            if (userProfile == null)
            {
                // Auto-create UserProfile if it doesn't exist
                await CreateUserProfileAsync(userIdInt);
            }
            
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
            // Reset to defaults with no org unit filter (show everything)
            existingPreference.GlobalFilters = new GlobalFilters
            {
                OrgUnitId = null  // Don't default to user's org unit - show everything
            };
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Creates a UserProfile for the specified user ID with default values
    /// </summary>
    private async Task CreateUserProfileAsync(int userId)
    {
        // Get user information to create a meaningful profile
        var userEmail = _userResolver.GetUserEmail();
        var firstName = "Unknown User";
        
        if (!string.IsNullOrEmpty(userEmail))
        {
            // Extract first name from email prefix (e.g., john.doe@example.com -> john)
            var emailPrefix = userEmail.Split('@')[0];
            var nameParts = emailPrefix.Split('.', '_', '-');
            if (nameParts.Length > 0 && !string.IsNullOrEmpty(nameParts[0]))
            {
                firstName = char.ToUpper(nameParts[0][0]) + nameParts[0].Substring(1).ToLower();
            }
        }

        var userProfile = new UserProfile
        {
            UserId = userId,
            FirstName = firstName,
            LastName = "",
            Status = EntityStatus.Active,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedBy = userId,
            IsDeleted = false,
            DeletedBy = 0
        };

        _context.Set<UserProfile>().Add(userProfile);
        await _context.SaveChangesAsync();
    }
}