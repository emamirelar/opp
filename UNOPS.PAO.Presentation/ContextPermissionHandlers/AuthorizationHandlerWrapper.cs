namespace UNOPS.PAO.Presentation.ContextPermissionHandlers;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Models.Contacts;

public class AuthorizationHandlerWrapper : IAuthorizationHandlerWrapper
{
    private ProfileAuthorizationHandler profileAuthorizationHandler;
    private ContactAuthorizationHandler contactAuthorizationHandler;

    private PartnerTreeAuthorizationHandler partnerTreeAuthorizationHandler;

    public AuthorizationHandlerWrapper()
    {
        profileAuthorizationHandler = new ProfileAuthorizationHandler();
        contactAuthorizationHandler = new ContactAuthorizationHandler();
        partnerTreeAuthorizationHandler = new PartnerTreeAuthorizationHandler();
    }

    public virtual AuthorizationHandler<OperationAuthorizationRequirement, ProfileModel> ProfileAuthorizationHandler => profileAuthorizationHandler;
    public virtual AuthorizationHandler<OperationAuthorizationRequirement, ContactModel> ContactAuthorizationHandler => contactAuthorizationHandler;

    public virtual AuthorizationHandler<OperationAuthorizationRequirement, PartnerTreeModel> PartnerTreeAuthorizationHandler => partnerTreeAuthorizationHandler;
}
