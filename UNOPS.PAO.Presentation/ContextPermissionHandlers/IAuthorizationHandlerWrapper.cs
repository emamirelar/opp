namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Models.Contacts;

public interface IAuthorizationHandlerWrapper
{
    AuthorizationHandler<OperationAuthorizationRequirement, ProfileModel> ProfileAuthorizationHandler { get; }
    AuthorizationHandler<OperationAuthorizationRequirement, ContactModel> ContactAuthorizationHandler { get; }

    AuthorizationHandler<OperationAuthorizationRequirement, PartnerTreeModel> PartnerTreeAuthorizationHandler { get; }
}
