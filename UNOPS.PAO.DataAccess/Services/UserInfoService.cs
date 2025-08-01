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

    public async Task<UserInfo?> GetUserInfoByEmailAsync(string email)
    {
        // Convert both the input email and database email to lowercase for case-insensitive comparison
        return await _context.UserInfos
            .FirstOrDefaultAsync(u => u.UserEmail.ToLower() == email.ToLower());
    }

    public async Task<object?> GetUserInfoWithOrgSettingsAsync(string email)
    {
        // Convert both the input email and database email to lowercase for case-insensitive comparison
        var result = await _context.UserInfos
            .Where(u => u.UserEmail.ToLower() == email.ToLower())
            .GroupJoin(_context.OrganizationHierarchies.Where(oh => oh.Type == OrganizationUnitType.OrgUnit),
                userInfo => userInfo.OrgUnit,
                orgHierarchy => orgHierarchy.Code,
                (userInfo, orgHierarchies) => new { userInfo, orgHierarchies })
            .SelectMany(
                temp => temp.orgHierarchies.DefaultIfEmpty(),
                (temp, orgHierarchy) => new { temp.userInfo, orgHierarchy })
            .GroupJoin(_context.UserInfos,
                combined => combined.userInfo.SupervisorId,
                supervisor => supervisor.UserId,
                (combined, supervisors) => new { combined.userInfo, combined.orgHierarchy, supervisors })
            .SelectMany(
                temp => temp.supervisors.DefaultIfEmpty(),
                (temp, supervisor) => new
                {
                    UserId = temp.userInfo.UserId,
                    Name = temp.userInfo.Name,
                    UserEmail = temp.userInfo.UserEmail,
                    OrgUnit = temp.userInfo.OrgUnit,
                    OrgUnitDescription = temp.orgHierarchy != null ? temp.orgHierarchy.Description : null,
                    SupervisorId = temp.userInfo.SupervisorId,
                    SupervisorName = supervisor != null ? supervisor.Name : null,
                    SupervisorEmail = supervisor != null ? supervisor.UserEmail : null,
                    IsSelfManagementEnabled = temp.orgHierarchy != null ? temp.orgHierarchy.IsSelfManagementEnabled : false,
                    CreatedDate = temp.userInfo.CreatedDate,
                    LastModifiedDate = temp.userInfo.LastModifiedDate,
                    CreatedBy = temp.userInfo.CreatedBy,
                    LastModifiedBy = temp.userInfo.LastModifiedBy,
                    IsDeleted = temp.userInfo.IsDeleted,
                    TextToSpeech = temp.userInfo.TextToSpeech,
                    Language = temp.userInfo.Language
                })
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<UserInfo?> UpdateUserInfoAsync(UserInfo userInfo)
    {
        var existingUserInfo = await _context.UserInfos.FindAsync(userInfo.UserId);
        if (existingUserInfo == null)
        {
            throw new BusinessException("UserInfo not found");
        }
        PatchNonNullProperties(userInfo, existingUserInfo);
        _context.UserInfos.Update(existingUserInfo);
        await _context.SaveChangesAsync();
        return existingUserInfo;
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

    public async Task<List<UserInfo>> GetUserInfosByEmailsAsync(IEnumerable<string> emails)
    {
        if (emails == null || !emails.Any())
        {
            return new List<UserInfo>();
        }

        var emailList = emails.Select(e => e.ToLower()).ToList();
        return await _context.UserInfos
            .Where(u => emailList.Contains(u.UserEmail.ToLower()))
            .ToListAsync();
    }
} 