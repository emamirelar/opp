namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.Models;

public class AuthorizationHandlerWrapper : IAuthorizationHandlerWrapper
{
    private ProfileAuthorizationHandler profileAuthorizationHandler;
    private FundingOpportunityAuthorizationHandler fundingOpportunityAuthorizationHandler;

    public AuthorizationHandlerWrapper()
    {
        profileAuthorizationHandler = new ProfileAuthorizationHandler();
        fundingOpportunityAuthorizationHandler = new FundingOpportunityAuthorizationHandler();
    }

    public virtual AuthorizationHandler<OperationAuthorizationRequirement, ProfileModel> ProfileAuthorizationHandler => profileAuthorizationHandler;
    public virtual AuthorizationHandler<OperationAuthorizationRequirement, FundingOpportunityModel> FundingOpportunityAuthorizationHandler => fundingOpportunityAuthorizationHandler;
}
