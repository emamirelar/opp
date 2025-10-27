namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Presentation.Security;

public class ProfileAuthorizationHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, ProfileModel>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   OperationAuthorizationRequirement requirement,
                                                   ProfileModel profile)
    {
        if (requirement == Operations.Read)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}