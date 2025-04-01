using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;

namespace UNOPS.PAO.Business.Interfaces;

public interface IManagerWrapper
{
    ISystemAdminManager SystemAdminManager { get; }

    IWorkflowManager WorkflowManager { get; }

    IContactManager ContactManager { get; }

    IInteractionManager InteractionManager { get; }

    IPartnerTreeManager PartnerTreeManager { get; }
    IPartnerManager PartnerManager { get; }

    IGeminiManager GeminiManager { get; }
    IDocumentManager DocumentManager { get; }
    IDocumentTypeManager DocumentTypeManager { get; }
    UserManager<PAOIdentityUser> UserManager { get; }

    ILinkManager LinkManager { get; }
}