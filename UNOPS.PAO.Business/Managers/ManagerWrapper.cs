namespace UNOPS.PAO.Business.Managers;

using System;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public class ManagerWrapper : IManagerWrapper
{
    private ISystemAdminManager systemAdminManager;
    private IWorkflowManager workflowManager;
    private IContactManager contactManager;
    private IInteractionManager interactionManager;
    private IPartnerTreeManager partnerTreeManager;
    private IPartnerManager partnerManager;

    private IGeminiManager geminiManager;

    public ManagerWrapper(IMapper mapper, AppDbContext context)
    {
        workflowManager = new WorkflowManager(context);

        systemAdminManager = new SystemAdminManager(context);

        contactManager = new ContactManager(mapper, context);
        interactionManager = new InteractionManager(mapper, context);
        partnerTreeManager = new PartnerTreeManager(mapper, context);
        partnerManager = new PartnerManager(mapper, context);

        geminiManager = new GeminiManager(mapper, context);
    }

    public virtual ISystemAdminManager SystemAdminManager => systemAdminManager;

    public virtual IWorkflowManager WorkflowManager => workflowManager;

    public virtual IContactManager ContactManager => contactManager;

    public virtual IInteractionManager InteractionManager => interactionManager;

    public virtual IPartnerTreeManager PartnerTreeManager => partnerTreeManager;

    public virtual IPartnerManager PartnerManager => partnerManager;

    public virtual IGeminiManager GeminiManager => geminiManager;
}
