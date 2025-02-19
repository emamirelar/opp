namespace UNOPS.PAO.Presentation.Controllers;

using System;
using Google.Cloud.BigQuery.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;

[Route("/")]
[ApiController]
public class WorkflowController : ControllerBase
{
    private IManagerWrapper manager;
    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public WorkflowController(IManagerWrapper manager, UserResolverService<int> userResolverService)
    {
        this.manager = manager;
        this.userResolverService = userResolverService;
    }

    [HttpGet(APIDictionary.Workflow + "/{entityName}")]
    public List<WorkflowStageModel> GetWorkflowPath(string entityName)
    {
        return entityName switch
        {
            "funding-opportunity" => WorkflowManager.GetWorkflowPath(FundingOpportunityWorkflow.StateMachine, Facing.Internal),
            "external-funding-opportunity" => WorkflowManager.GetWorkflowPath(FundingOpportunityWorkflow.StateMachine, Facing.External),
            "proposal" => WorkflowManager.GetWorkflowPath(ProposalWorkflow.StateMachine, Facing.External),
            "internal-proposal" => WorkflowManager.GetWorkflowPath(ProposalWorkflow.StateMachine, Facing.Internal),
            _ => [],
        };
    }

    [HttpGet(APIDictionary.Workflow + "/{entityName}/{id}")]
    public async Task<WorkflowStateModel?> GetStateMachine(string entityName, int id)
    {
        string? stage;

        switch (entityName)
        {
            case "funding-opportunity":
                stage = await manager.FundingOpportunityManager.GetFundingOpportunityStage(id);
                return manager.WorkflowManager.GetWorkflowState(FundingOpportunityWorkflow.StateMachine, stage ?? string.Empty, Facing.Internal);
            case "proposal":
                stage = await manager.ProposalManager.GetProposalStage(id);
                return manager.WorkflowManager.GetWorkflowState(ProposalWorkflow.StateMachine, stage ?? string.Empty, Facing.External);
            case "internal-proposal":
                stage = await manager.ProposalManager.GetProposalStage(id);
                return manager.WorkflowManager.GetWorkflowState(ProposalWorkflow.StateMachine, stage ?? string.Empty, Facing.Internal);
            default:
                return null;
        }
    }

    [Authorize]
    [HttpPost(APIDictionary.Workflow)]
    public async Task<WorkflowStateModel?> DoWorkflowAction([FromBody] WorkflowActionModel model)
    {
        string? stage = string.Empty;

        switch (model.EntityName)
        {
            case "funding-opportunity":
                stage = await manager.FundingOpportunityManager.GetFundingOpportunityStage(model.Id);
                await this.manager.FundingOpportunityManager.UpdateStage(currentUserId, model.Id, model.NewStage);
                break;
            case "proposal":
                stage = await manager.ProposalManager.GetProposalStage(model.Id);
                await this.manager.ProposalManager.UpdateStage(currentUserId, model.Id, model.NewStage);
                break;
            case "internal-proposal":
                stage = await manager.ProposalManager.GetProposalStage(model.Id);
                await this.manager.ProposalManager.UpdateStage(currentUserId, model.Id, model.NewStage);
                break;
        }

        await this.manager.WorkflowManager.AddLog(model.EntityName, model.Id.ToString(), stage, model.NewStage, model.Comment);

        return await GetStateMachine(model.EntityName, model.Id);
    }
}