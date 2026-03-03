using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.DataAccess.Services;

public class UserInfoService : IUserInfoService
{
    private readonly AppDbContext _context;

    public UserInfoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetUserInfoByEmailAsync(string email)
    {
        // Convert both the input email and database email to lowercase for case-insensitive comparison
        return await _context.UserProfile
            .FirstOrDefaultAsync(u => (u.UserEmail ?? "").ToLower() == email.ToLower());
    }

    public async Task<object?> GetUserInfoWithOrgSettingsAsync(string email)
    {
        // Convert both the input email and database email to lowercase for case-insensitive comparison
        var result = await _context.UserProfile
            .Where(u => (u.UserEmail ?? "").ToLower() == email.ToLower())
            .GroupJoin(_context.OrganizationHierarchies.Where(oh => oh.Type == OrganizationUnitType.OrgUnit),
                userProfile => userProfile.OrgUnit,
                orgHierarchy => orgHierarchy.Code,
                (userProfile, orgHierarchies) => new { userProfile, orgHierarchies })
            .SelectMany(
                temp => temp.orgHierarchies.DefaultIfEmpty(),
                (temp, orgHierarchy) => new { temp.userProfile, orgHierarchy })
            .GroupJoin(_context.UserProfile,
                combined => combined.userProfile.SupervisorId,
                supervisor => supervisor.UserId,
                (combined, supervisors) => new { combined.userProfile, combined.orgHierarchy, supervisors })
            .SelectMany(
                temp => temp.supervisors.DefaultIfEmpty(),
                (temp, supervisor) => new
                {
                    UserId = temp.userProfile.UserId,
                    Name = temp.userProfile.Name,
                    FirstName = temp.userProfile.FirstName,
                    LastName = temp.userProfile.LastName,
                    UserEmail = temp.userProfile.UserEmail,
                    OrgUnit = temp.userProfile.OrgUnit,
                    OrgUnitDescription = temp.orgHierarchy != null ? temp.orgHierarchy.Description : null,
                    SupervisorId = temp.userProfile.SupervisorId,
                    SupervisorName = supervisor != null ? supervisor.Name : null,
                    SupervisorEmail = supervisor != null ? supervisor.UserEmail : null,
                    IsSelfManagementEnabled = temp.orgHierarchy != null ? temp.orgHierarchy.IsSelfManagementEnabled : false,
                    DutyStation = temp.userProfile.DutyStation,
                    Position = temp.userProfile.Position
                })
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<UserProfile?> UpdateUserInfoAsync(UserProfile userProfile)
    {
        var existingUserProfile = await _context.UserProfile.FindAsync(userProfile.UserId);
        if (existingUserProfile == null)
        {
            throw new BusinessException("UserProfile not found");
        }
        PatchNonNullProperties(userProfile, existingUserProfile);
        _context.UserProfile.Update(existingUserProfile);
        await _context.SaveChangesAsync();
        return existingUserProfile;
    }

    public void PatchNonNullProperties<TSource, TTarget>(TSource source, TTarget target)
    {
        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // Handle duplicate property names by grouping and taking the first one
        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                                              .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var sourceProp in sourceProperties)
        {
            if (!targetProperties.TryGetValue(sourceProp.Name, out var targetProp)) continue;
            if (!targetProp.CanWrite || !sourceProp.CanRead) continue;

            var value = sourceProp.GetValue(source);

            // Only set if value is not null (or not empty string for strings)
            if (value != null && (!(value is string str) || !string.IsNullOrWhiteSpace(str)))
            {
                // Special handling for ID columns: don't update if source is 0 and target already has a value
                if (sourceProp.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && 
                    value.Equals(0))
                {
                    var existingValue = targetProp.GetValue(target);
                    if (existingValue != null && !existingValue.Equals(0))
                    {
                        continue; // Skip updating ID if target already has a non-zero value
                    }
                }

                targetProp.SetValue(target, value);
            }
        }
    }

    public async Task<List<UserProfile>> GetUserInfosByEmailsAsync(IEnumerable<string> emails)
    {
        if (emails == null || !emails.Any())
        {
            return new List<UserProfile>();
        }

        var emailList = emails.Select(e => e.ToLower()).ToList();
        return await _context.UserProfile
            .Where(u => emailList.Contains((u.UserEmail ?? "").ToLower()))
            .ToListAsync();
    }
} 