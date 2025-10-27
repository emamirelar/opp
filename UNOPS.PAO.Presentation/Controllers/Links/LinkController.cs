using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Links;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Links;

[Route("/")]
public class LinkController : BaseController
{
    private readonly ILinkManager _manager;

    public LinkController(
        IManagerWrapper manager,
        ILogger<LinkController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.LinkManager;
    }

    [HttpGet(APIDictionary.Link)]
    public async Task<ActionResult> GetLinks(
        [FromQuery] LinkEntityType entity, 
        [FromQuery] int entityId,
        [FromQuery] PaginationRequest parameters)
    {
        return await HandleOperationAsync(async () =>
        {
            return await _manager.GetEntityLinks(entity, entityId, parameters);
        });
    }

    [HttpPost(APIDictionary.Link)]
    public async Task<ActionResult> Create([FromBody] LinkRequest req)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _manager.CreateLinkAsync(req);
            if (result == null)
            {
                throw new BusinessException("Failed to create link");
            }
            return result;
        }, 201);
    }

    [HttpPut(APIDictionary.Link)]
    public async Task<ActionResult> Update([FromBody] UpdateLinkRequest req)
    {
        return await HandleOperationAsync(async () =>
        {
            await _manager.UpdateLinkAsync(req);
        });
    }

    [HttpDelete(APIDictionary.Link)]
    public async Task<ActionResult> Delete([FromQuery] int id)
    {
        return await HandleOperationAsync(async () =>
        {
            await _manager.DeleteLinkAsync(id);
        });
    }
} 