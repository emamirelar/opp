namespace UNOPS.PAO.Server.Infrastructure.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.Identity.Entities;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly UserManager<PAOIdentityUser> userManager;
    private readonly IPAOExecutionContext executionContext;

    public PermissionHandler(UserManager<PAOIdentityUser> userManager, IPAOExecutionContext executionContext)
    {
        this.userManager = userManager;
        this.executionContext = executionContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return;
        }

        if (executionContext.UserPermissions.Any(c => requirement.Permissions.Contains(c.Name)))
        {
            context.Succeed(requirement);
            return;
        }
        else
        {
            context.Fail();
        }
    }
}
