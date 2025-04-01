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

    ILinkManager LinkManager { get; }
}