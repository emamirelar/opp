using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;

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
} 