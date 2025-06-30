namespace UNOPS.PAO.Presentation.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Attributes;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class AiPromptController : BaseController
{
    private readonly IManagerWrapper _managerWrapper;

    public AiPromptController(
        IManagerWrapper managerWrapper,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<AiPromptController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _managerWrapper = managerWrapper;
    }

    /// <summary>
    /// Gets all AI prompts with pagination and search
    /// </summary>
    [HttpPost(APIDictionary.AiPromptsList)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptsAsync([FromBody] AiPromptFilterRequest request)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptsAsync(User, request);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific AI prompt by ID
    /// </summary>
    [HttpGet(APIDictionary.AiPrompts + "/{id}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptByIdAsync(int id)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptByIdAsync(User, id);
        if (result == null)
        {
            return NotFound($"AI Prompt with ID {id} not found.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Creates a new AI prompt
    /// </summary>
    [HttpPost(APIDictionary.AiPrompts)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> CreatePromptAsync([FromBody] AiPromptModel model)
    {
        // RBAC interceptor handles permission checking
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _managerWrapper.AiPromptManager.CreatePromptAsync(User, model);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Updates an existing AI prompt
    /// </summary>
    [HttpPut(APIDictionary.AiPrompts + "/{id}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "update")]
    public async Task<ActionResult> UpdatePromptAsync(int id, [FromBody] AiPromptModel model)
    {
        // RBAC interceptor handles permission checking
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _managerWrapper.AiPromptManager.UpdatePromptAsync(User, id, model);
        if (result == null)
        {
            return NotFound($"AI Prompt with ID {id} not found.");
        }
        return Ok(result);
    }

    /// <summary>
    /// Deletes an AI prompt
    /// </summary>
    [HttpDelete(APIDictionary.AiPrompts + "/{id}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "delete")]
    public async Task<ActionResult> DeletePromptAsync(int id)
    {
        // RBAC interceptor handles permission checking
        var success = await _managerWrapper.AiPromptManager.DeletePromptAsync(User, id);
        if (!success)
        {
            return NotFound($"AI Prompt with ID {id} not found.");
        }
        return NoContent();
    }

    /// <summary>
    /// Gets prompts by type
    /// </summary>
    [HttpGet(APIDictionary.AiPromptsByType + "/{type}")]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptsByTypeAsync(string type)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptsByTypeAsync(User, type);
        return Ok(result);
    }

    /// <summary>
    /// Gets unique prompt types for dropdown/filter
    /// </summary>
    [HttpGet(APIDictionary.AiPromptsTypes)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetPromptTypesAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetPromptTypesAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Gets unique models for dropdown/filter
    /// </summary>
    [HttpGet(APIDictionary.AiPromptsModels)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetModelsAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetModelsAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Gets unique projects for dropdown/filter
    /// </summary>
    [HttpGet(APIDictionary.AiPromptsProjects)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetProjectsAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetProjectsAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Gets unique locations for dropdown/filter
    /// </summary>
    [HttpGet(APIDictionary.AiPromptsLocations)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> GetLocationsAsync()
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.GetLocationsAsync(User);
        return Ok(result);
    }

    /// <summary>
    /// Tests an AI prompt with provided test data
    /// </summary>
    [HttpPost(APIDictionary.AiPromptsTest)]
    [AccessControlled(EntityTypes.AiPromptManagement, "read")]
    public async Task<ActionResult> TestPromptAsync([FromBody] TestPromptRequest request)
    {
        // RBAC interceptor handles permission checking
        var result = await _managerWrapper.AiPromptManager.TestPromptAsync(User, request);
        return Ok(result);
    }
} 