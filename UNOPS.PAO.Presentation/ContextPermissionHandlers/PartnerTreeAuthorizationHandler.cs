namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Presentation.Security;

public class PartnerTreeAuthorizationHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, PartnerTreeModel>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   OperationAuthorizationRequirement requirement,
                                                   PartnerTreeModel PartnerTree)
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