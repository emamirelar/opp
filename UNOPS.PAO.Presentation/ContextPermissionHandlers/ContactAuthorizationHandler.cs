namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Presentation.Security;

public class ContactAuthorizationHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, ContactModel>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   OperationAuthorizationRequirement requirement,
                                                   ContactModel Contact)
    {
        if (requirement == Operations.Create ||
            requirement == Operations.Read ||
            requirement == Operations.Update ||
            requirement == Operations.Delete)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}