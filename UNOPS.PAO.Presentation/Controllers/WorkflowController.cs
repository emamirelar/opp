namespace UNOPS.PAO.Presentation.Controllers;

using System;
using Google.Cloud.BigQuery.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;

[Route("/")]
public class WorkflowController : BaseController
{
    private readonly IManagerWrapper _manager;

    public WorkflowController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService,
        ILogger<WorkflowController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager;
    }

    [HttpGet(APIDictionary.Workflow + "/{entityName}")]
    public async Task<ActionResult> GetWorkflowPath(string entityName)
    {
        return await HandleOperationAsync(async () => 
        {
            var result = entityName switch
            {
                _ => new List<WorkflowStageModel>(),
            };
            return await Task.FromResult(result);
        });
    }

    [HttpGet(APIDictionary.Workflow + "/{entityName}/{id}")]
    public async Task<ActionResult> GetStateMachine(string entityName, int id)
    {
        return await HandleOperationAsync(async () => 
        {
            WorkflowStateModel? result = null;
            
            switch (entityName)
            {
                default:
                    return result;
            }
        });
    }

    [HttpPost(APIDictionary.Workflow)]
    public async Task<ActionResult> DoWorkflowAction([FromBody] WorkflowActionModel model)
    {
        return await HandleOperationAsync(async () => 
        {
            string? stage = string.Empty;

            switch (model.EntityName)
            {
                // Add cases if needed
            }

            await _manager.WorkflowManager.AddLog(model.EntityName, model.Id.ToString(), stage, model.NewStage, model.Comment);

            // We can't directly return the result of GetStateMachine as it returns an ActionResult
            // Instead, we need to call the actual service method that returns the state machine data
            WorkflowStateModel? stateMachine = null;
            
            // Here you would typically call the workflow manager to get the state machine
            // For example: stateMachine = await _manager.WorkflowManager.GetStateMachine(model.EntityName, model.Id);
            
            return stateMachine;
        });
    }
}