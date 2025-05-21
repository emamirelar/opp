namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;

[Route("/")]
public class PartnerTreeController : BaseController
{
    private readonly IPartnerTreeManager _manager;

    public PartnerTreeController(
        IManagerWrapper managerWrapper, 
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<PartnerTreeController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.PartnerTreeManager;
    }

    [HttpPost(APIDictionary.PartnerTree)]
    // Internal call: Create a Partner Tree
    public async Task<ActionResult> Create([FromBody] PartnerTreeDataModel req)
    { 
        return await HandleOperationAsync(async () =>
        {
            var result = await _manager.CreatePartnerTreeAsync(req);
            if (result == null)
            {
                throw new BusinessException("Failed to create partner tree");
            }
            return result;
        }, 201);
    }

    [HttpGet(APIDictionary.PartnerTree)]
    // Internal call: get partner tree created by logged-in user
    public ActionResult GetAll([FromQuery] string sortBy = "Name", [FromQuery] bool ascending = true)
    {
        try
        {
            return Ok(_manager.GetPartnerTrees(CurrentUserId, sortBy, ascending));
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business exception occurred: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    [HttpGet(APIDictionary.PartnerTree + "/{id}")]
    // Internal call: Partner Tree details
    public async Task<ActionResult> Get(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var partnerTree = await _manager.GetPartnerTree(CurrentUserId, id);
            if (partnerTree == null)
            {
                throw new BusinessException($"Partner Tree with ID {id} not found");
            }
            return partnerTree;
        });
    }

    [HttpPut(APIDictionary.PartnerTree)]
    // Internal call: update Partner Tree
    public async Task<ActionResult> Update([FromBody] PartnerTreeDataModel[] req)
    {
        return await HandleOperationAsync(async () =>
        {
            List<PartnerTreeModel> updatedTrees = new List<PartnerTreeModel>();
            
            foreach (var item in req)
            {
                var updatedTree = await _manager.UpdatePartnerTreeAsync(CurrentUserId, item);
                if (updatedTree != null)
                {
                    updatedTrees.Add(updatedTree);
                }
            }

            return updatedTrees;
        });
    }

    [HttpDelete(APIDictionary.PartnerTree + "/{id}")]
    // Internal call: delete partner tree
    public async Task<ActionResult> Delete(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            await _manager.DeletePartnerTreeAsync(CurrentUserId, id);
        });
    }

    [HttpGet(APIDictionary.PartnerTree + "/{id}/permissions")]
    public async Task<ActionResult> PermissionsGet(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var partnerTree = await _manager.GetPartnerTree(CurrentUserId, id);
            if (partnerTree == null)
            {
                throw new BusinessException($"Partner Tree with ID {id} not found");
            }
            return await GetEntityPermissionsAsync(partnerTree);
        });
    }

    [HttpGet(APIDictionary.PartnerTree + "-structure")]
    public ActionResult GetCategoryAndGroupStructure()
    {
        return Ok(_manager.GetCategoryAndGroupStructure(CurrentUserId));
    }
}
