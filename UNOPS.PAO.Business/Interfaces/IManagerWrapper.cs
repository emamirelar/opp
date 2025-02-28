namespace UNOPS.PAO.Business.Interfaces;

public interface IManagerWrapper
{
    IFundingOpportunityManager FundingOpportunityManager { get; }
    IProposalManager ProposalManager { get; }
    IDocumentManager DocumentManager { get; }
    ISystemAdminManager SystemAdminManager { get; }

    IWorkflowManager WorkflowManager { get; }

    IContactManager ContactManager { get; }
    IPartnerManager PartnerManager { get; }
}