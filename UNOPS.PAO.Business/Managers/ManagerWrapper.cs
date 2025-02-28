namespace UNOPS.PAO.Business.Managers;

using System;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public class ManagerWrapper : IManagerWrapper
{
    private IFundingOpportunityManager fundingOpportunityManager;
    private IProposalManager proposalManager;
    private IDocumentManager documentManager;
    private ISystemAdminManager systemAdminManager;
    private IWorkflowManager workflowManager;
    private IContactManager contactManager;
    private IPartnerManager partnerManager;

    public ManagerWrapper(IMapper mapper, AppDbContext context)
    {
        workflowManager = new WorkflowManager(context);

        fundingOpportunityManager = new FundingOpportunityManager(mapper, context);
        proposalManager = new ProposalManager(mapper, context, workflowManager);

        documentManager = new DocumentManager(mapper, context);
        systemAdminManager = new SystemAdminManager(context);

        contactManager = new ContactManager(mapper, context);
        partnerManager = new PartnerManager(mapper, context);
    }

    public virtual IFundingOpportunityManager FundingOpportunityManager => fundingOpportunityManager;

    public virtual IProposalManager ProposalManager => proposalManager;
    public virtual IDocumentManager DocumentManager => documentManager;
    public virtual ISystemAdminManager SystemAdminManager => systemAdminManager;

    public virtual IWorkflowManager WorkflowManager => workflowManager;

    public virtual IContactManager ContactManager => contactManager;

    public virtual IPartnerManager PartnerManager => partnerManager;
}
