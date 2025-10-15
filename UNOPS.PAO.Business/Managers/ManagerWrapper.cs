namespace UNOPS.PAO.Business.Managers;

using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;

public class ManagerWrapper : IManagerWrapper
{
    public UserManager<PAOIdentityUser> UserManager { get; }
    private ISystemAdminManager systemAdminManager;
    private IWorkflowManager workflowManager;
    private IContactManager contactManager;
    private IInteractionManager interactionManager;
    private IPartnerTreeManager partnerTreeManager;
    private IPartnerManager partnerManager;
    private IDocumentManager documentManager;
    private IDocumentTypeManager documentTypeManager;

    private IGeminiManager geminiManager;
    private ILinkManager linkManager;
    private IUserDataManager userDataManager;
    private IUserManagementManager userManagementManager;
    private IAiPromptManager aiPromptManager;
    private IGmailAddonManager gmailAddonManager;
    
    public ManagerWrapper(IMapper mapper, AppDbContext context,
                          UserManager<PAOIdentityUser> userManager, 
                          IHttpContextAccessor httpContextAccessor,
                          IConfiguration configuration,
                          IServiceProvider serviceProvider)
    {
        this.UserManager = userManager;
        workflowManager = new WorkflowManager(context);

        systemAdminManager = new SystemAdminManager(context, configuration, serviceProvider);

        contactManager = new ContactManager(mapper, context);
        interactionManager = new InteractionManager(mapper, context);
        partnerTreeManager = new PartnerTreeManager(mapper, context);
        partnerManager = new PartnerManager(mapper, context);
        documentManager = new DocumentManager(mapper, context);
        documentTypeManager = new DocumentTypeManager(mapper, context);

        geminiManager = new GeminiManager(mapper, context);

        linkManager = new LinkManager(mapper, context);
        userDataManager = new UserDataManager(mapper, context, httpContextAccessor);
        
        gmailAddonManager = new GmailAddonManager(mapper, context);
        
        // Default implementation - will be overridden in UNOPSManagerWrapper
        userManagementManager = null;
        aiPromptManager = null; // Will be overridden in UNOPSManagerWrapper
    }

    public virtual ISystemAdminManager SystemAdminManager => systemAdminManager;

    public virtual IWorkflowManager WorkflowManager => workflowManager;

    public virtual IContactManager ContactManager => contactManager;

    public virtual IInteractionManager InteractionManager => interactionManager;

    public virtual IPartnerTreeManager PartnerTreeManager => partnerTreeManager;

    public virtual IPartnerManager PartnerManager => partnerManager;

    public virtual IGeminiManager GeminiManager => geminiManager;
    public virtual IDocumentManager DocumentManager => documentManager;
    public virtual IDocumentTypeManager DocumentTypeManager => documentTypeManager;

    public virtual ILinkManager LinkManager => linkManager;
    public virtual IUserDataManager UserDataManager => userDataManager;
    public virtual IUserManagementManager UserManagementManager => userManagementManager;
    public virtual IAiPromptManager AiPromptManager => aiPromptManager;
    public virtual IGmailAddonManager GmailAddonManager => gmailAddonManager;
}
