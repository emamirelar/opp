using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;

namespace UNOPS.PAO.Business.Interfaces;

public interface IManagerWrapper
{
    ISystemAdminManager SystemAdminManager { get; }

    IContactManager ContactManager { get; }

    IInteractionManager InteractionManager { get; }

    IPartnerTreeManager PartnerTreeManager { get; }
    IPartnerManager PartnerManager { get; }

    IGeminiManager GeminiManager { get; }
    IImageGenerationManager ImageGenerationManager { get; }
    IDocumentManager DocumentManager { get; }
    IDocumentTypeManager DocumentTypeManager { get; }
    UserManager<PAOIdentityUser> UserManager { get; }

    ILinkManager LinkManager { get; }
    IUserDataManager UserDataManager { get; }
    IUserManagementManager UserManagementManager { get; }
    IAiPromptManager AiPromptManager { get; }
    IGmailAddonManager GmailAddonManager { get; }
    IOpportunityManager OpportunityManager { get; }
    ICommentManager CommentManager { get; }
    IEntityArtifactManager EntityArtifactManager { get; }
    IAuditLogManager AuditLogManager { get; }
    IAiRetrieverManager AiRetrieverManager { get; }
    IRiskManager RiskManager { get; }
}