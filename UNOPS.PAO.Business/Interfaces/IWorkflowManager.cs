using UNOPS.PAO.Models.Workflow;

namespace UNOPS.PAO.Business.Interfaces;
public interface IWorkflowManager
{
    static abstract List<WorkflowStageModel> GetWorkflowPath(StateMachine stateMachine, Facing facing);
    public abstract static List<WorkflowStageModel> GetWorkflowPath(StateMachine stateMachine);
    WorkflowStateModel? GetWorkflowState(StateMachine stateMachine, string stage, Facing facing);
    WorkflowStateModel? GetWorkflowState(StateMachine stateMachine, string stage) => GetWorkflowState(stateMachine, stage, Facing.TwoFace);
    Task AddLog(string entityName, string entityId, string? stage, string newStage, string comment);
}