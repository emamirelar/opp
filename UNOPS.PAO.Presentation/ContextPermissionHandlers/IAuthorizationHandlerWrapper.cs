namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.Models;

public interface IAuthorizationHandlerWrapper
{
    AuthorizationHandler<OperationAuthorizationRequirement, ProfileModel> ProfileAuthorizationHandler { get; }
    AuthorizationHandler<OperationAuthorizationRequirement, FundingOpportunityModel> FundingOpportunityAuthorizationHandler { get; }
    AuthorizationHandler<OperationAuthorizationRequirement, ContactModel> ContactAuthorizationHandler { get; }
}
