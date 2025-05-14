namespace UNOPS.PAO.UNOPSPresentation.Authorization;

using Microsoft.AspNetCore.Authorization;

public class EntityPermissionRequirement : IAuthorizationRequirement
{
    public string EntityName { get; }
    public string Action { get; }

    public EntityPermissionRequirement(string entityName, string action)
    {
        EntityName = entityName;
        Action = action;
    }
} 