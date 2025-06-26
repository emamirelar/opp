using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

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
                    IsDeleted = temp.userInfo.IsDeleted
                })
            .FirstOrDefaultAsync();

        return result;
    }
} 