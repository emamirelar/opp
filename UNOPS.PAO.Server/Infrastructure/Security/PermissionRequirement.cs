namespace UNOPS.PAO.Server.Infrastructure.Security;

using Microsoft.AspNetCore.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(params string[] permissions)
    {
        Permissions = permissions;
    }

    public string[] Permissions { get; }
}