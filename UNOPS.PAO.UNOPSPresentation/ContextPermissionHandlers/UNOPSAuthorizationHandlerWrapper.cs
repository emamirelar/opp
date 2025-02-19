namespace UNOPS.PAO.UNOPSPresentation.ContextPermissionHandlers;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.ContextPermissionHandlers;

public class UNOPSAuthorizationHandlerWrapper : AuthorizationHandlerWrapper
{
    private UNOPSProfileAuthorizationHandler profileAuthorizationHandler;

    public UNOPSAuthorizationHandlerWrapper() : base()
    {
        profileAuthorizationHandler = new UNOPSProfileAuthorizationHandler();
    }

    public override AuthorizationHandler<OperationAuthorizationRequirement, ProfileModel> ProfileAuthorizationHandler => profileAuthorizationHandler;
}
