using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow.Interfaces;

/// <summary>
/// PAO-specific workflow approver provider interface.
/// Extends base interface for future PAO-specific methods.
/// </summary>
public interface IPaoWorkflowApproverProvider : IWorkflowApproverProvider
{
    // Placeholder for future PAO-specific approval methods
    // e.g., Task<bool> CanUserApproveOpportunityAsync(int opportunityId, int userId);
}
