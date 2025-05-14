using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class LinkController : ControllerBase
{
    private readonly ILinkManager manager;

    public LinkController(IManagerWrapper manager)
    {
        this.manager = manager.LinkManager;
    }

    [HttpGet(APIDictionary.Link)]
    public async Task<ActionResult> GetLinks(
        [FromQuery] LinkEntityType entity, 
        [FromQuery] int entityId,
        [FromQuery] PaginationRequest parameters)
    {
        var links = await manager.GetEntityLinks(entity, entityId, parameters);
        return Ok(links);
    }

    [HttpPost(APIDictionary.Link)]
    public async Task<IActionResult> Create([FromBody] LinkRequest req)
    {
        var result = await manager.CreateLinkAsync(req);
        if (result == null)
        {
            return BadRequest();
        }
        return CreatedAtAction(nameof(GetLinks), new { entity = req.Entity, entityId = req.EntityId }, result);
    }

    [HttpPut(APIDictionary.Link)]
    public async Task<IActionResult> Update([FromBody] UpdateLinkRequest req)
    {
        await manager.UpdateLinkAsync(req);
        return NoContent();
    }

    [HttpDelete(APIDictionary.Link)]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        await manager.DeleteLinkAsync(id);
        return NoContent();
    }
} 