namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Security;

public class FundingOpportunityAuthorizationHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, FundingOpportunityModel>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   OperationAuthorizationRequirement requirement,
                                                   FundingOpportunityModel fundingOpportunity)
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