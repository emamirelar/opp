namespace UNOPS.PAO.Presentation.Security;

using Microsoft.AspNetCore.Authorization;
using System;

public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    const string POLICY_PREFIX = "Permission";

    public PermissionAuthorizeAttribute(params string[] permissions)
    {
        Permissions = permissions;
    }

    public string[] Permissions
    {
        get
        {
            if (Policy != null && Policy.StartsWith(POLICY_PREFIX))
            {
                return Policy.Substring(POLICY_PREFIX.Length).Split(',');
            }
            return Array.Empty<string>();
        }
        set
        {
            Policy = $"{POLICY_PREFIX}{string.Join(",", value)}";
        }
    }
}
