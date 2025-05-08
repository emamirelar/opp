using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace UNOPS.PAO.DataAccess.Services;

public class UserResolverService<TUserId>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _userEmail;
    private readonly int _userId;

    public UserResolverService(IHttpContextAccessor context)
    {
        _httpContextAccessor = context;
    }

    public UserResolverService(string userEmail)
    {
        _userEmail = userEmail;
    }

    public string? GetUserEmail()
    {
        return _userEmail ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public string? GetUserName()
    {
        return _userEmail ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public bool IsImpersonator()
    {
        var isImpersonation = _httpContextAccessor?.HttpContext?.User.HasClaim(a => a.Type == "Impersonator");
        return isImpersonation != null && isImpersonation.Value;
    }

    public TUserId GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null) return default;

        return (TUserId)Convert.ChangeType(userIdClaim, typeof(TUserId));
    }
}