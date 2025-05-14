namespace UNOPS.PAO.Presentation.Controllers;

using System;
using Google.Cloud.BigQuery.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
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
            _ => [],
        };
    }

    [HttpGet(APIDictionary.Workflow + "/{entityName}/{id}")]
    public async Task<WorkflowStateModel?> GetStateMachine(string entityName, int id)
    {
        string? stage;

        switch (entityName)
        {
            default:
                return null;
        }
    }

    [Authorize(AuthenticationSchemes = "IAP")]
    [HttpPost(APIDictionary.Workflow)]
    public async Task<WorkflowStateModel?> DoWorkflowAction([FromBody] WorkflowActionModel model)
    {
        string? stage = string.Empty;

        switch (model.EntityName)
        {
        }

        await this.manager.WorkflowManager.AddLog(model.EntityName, model.Id.ToString(), stage, model.NewStage, model.Comment);

        return await GetStateMachine(model.EntityName, model.Id);
    }
}