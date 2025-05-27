namespace UNOPS.PAO.UNOPSPresentation.Authorization;

using Microsoft.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class EntityPermissionAttribute : AuthorizeAttribute
{
    public EntityPermissionAttribute(string entityName, string action)
    {
        EntityName = entityName;
        Action = action;
        Policy = $"EntityPermission:{entityName}:{action}";
    }
    
    public string EntityName { get; }
    public string Action { get; }
} 