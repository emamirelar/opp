using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Managers;

public class UserDataManager : IUserDataManager
{
    private IMapper mapper;
    AppDbContext context;
    private readonly IHttpContextAccessor httpContextAccessor;

    public UserDataManager(IMapper mapper, AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.mapper = mapper;
        this.context = context;
        this.httpContextAccessor = httpContextAccessor;
    }

    public Task<PAOUserModel?> GetUserByIdAsync(int id)
    {
        var user = context.GrantUsers.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return Task.FromResult<PAOUserModel?>(null);
        return Task.FromResult(mapper.Map<PAOUserModel>(user));
    }
    public Task<PAOUserModel?> GetCurrentUserAsync()
    {
        var user = httpContextAccessor?.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<PAOUserModel?>(null);
        }

        var userId = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userId == null)
        {
            return Task.FromResult<PAOUserModel?>(null);
        }
        return GetUserByIdAsync(int.Parse(userId));
    }
}
