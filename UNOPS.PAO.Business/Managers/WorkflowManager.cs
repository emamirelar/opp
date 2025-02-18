using Microsoft.EntityFrameworkCore.Metadata.Internal;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models.Workflow;

namespace UNOPS.PAO.Business.Managers;
public class WorkflowManager : IWorkflowManager
{
    private DataRepository<WorkflowLog> workflowLogRepository;

    public WorkflowManager(AppDbContext context)
    {
        workflowLogRepository = new DataRepository<WorkflowLog>(context);
    }

    public static List<WorkflowStageModel> GetWorkflowPath(StateMachine stateMachine, Facing facing)
    {
        return stateMachine
            .States
            .Select(x => facing switch
            { 
                Facing.Internal => x.InternalState ?? x,
                Facing.External => x.ExternalState ?? x,
                _ => x
            })
            .Where(x => x.Sequence >= 0 && (x.Facing == Facing.TwoFace || x.Facing == facing))
            .Select(x => new 
            {
                Stage = x.StageCode,
                DisplayName = x.Name,
                x.Sequence,
            })
            .Distinct()
            .Select(x => new WorkflowStageModel()
            {
                Stage = x.Stage,
                DisplayName = x.DisplayName,
                Sequence = x.Sequence,
            })
            .OrderBy(x => x.Sequence)
            .ToList();
    }

    public static List<WorkflowStageModel> GetWorkflowPath(StateMachine stateMachine) => GetWorkflowPath(stateMachine, Facing.TwoFace);

    public WorkflowStateModel GetWorkflowState(StateMachine stateMachine, string stage, Facing facing)
    {
        stateMachine.Stage = stage;

        var state = stateMachine.StateAction;
        if (state == null)
        {
            return new WorkflowStateModel()
            {
                Stage = string.Empty,
                DisplayName = string.Empty,
                Comment = string.Empty,
                NextActions = []
            };
        }

        var facingStage = facing switch
        {
            Facing.Internal => state.InternalState ?? state,
            Facing.External => state.ExternalState ?? state,
            _ => state
        };

        var lastEntry = workflowLogRepository
            .GetAll()
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefault(x => x.NewStage == state.StageCode);

        string comment = lastEntry != null ? lastEntry.Comment ?? string.Empty : string.Empty;

        return new WorkflowStateModel()
        {
            Stage = facingStage.StageCode,
            DisplayName = facingStage.Name,
            Comment = comment,
            NextActions = [.. facingStage.Actions.Where(x => (x.Facing == Facing.TwoFace || x.Facing == facing)).OrderBy(x => x.Sequence)]
        };
    }
    public async Task AddLog(string entityName, string entityId, string? stage, string newStage, string comment)
    {
        await workflowLogRepository.AddAsync(new WorkflowLog()
        {
            Name = string.Empty,
            EntityName = entityName,
            EntityId = entityId,
            Stage = stage,
            NewStage = newStage,
            Comment = comment
        });
    }
}