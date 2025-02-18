using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Helpers;

[Route("/")]
[ApiController]
[Authorize]
public class DocumentController : ControllerBase
{
    private IDocumentManager manager;
    private UserResolverService<int> userResolverService;

    private int currentUserId => userResolverService.GetCurrentUserId();

    public DocumentController(IManagerWrapper manager, UserResolverService<int> userResolverService)
    {
        this.manager = manager.DocumentManager;
        this.userResolverService = userResolverService;
    }

    [HttpPost(APIDictionary.DocumentUpload)]
    public async Task<IActionResult> Create([FromForm] DocumentUploadModel model)
    {
        var result = await manager.CreateDocumentAsync(model);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }
    
    [HttpPost(APIDictionary.DocumentLink)]
    public async Task<IActionResult> Link([FromBody] DocumentLinkModel model)
    {
        var result = await manager.LinkDocumentAsync(model);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpGet(APIDictionary.Document)]
    public ActionResult GetAll()
    {
        return Ok(manager.ListDocumentsAsync());
    }

    [HttpGet(APIDictionary.Document + "/{id}")]
    public async Task<ActionResult> Get(int id)
    {
        var x = await manager.GetDocumentByIdAsync(currentUserId, id);

        if (x == null)
        {
            return NotFound();
        }

        return Ok(x);
    }

    [HttpDelete(APIDictionary.Document + "/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeleteDocumentAsync(currentUserId, id);
        return NoContent();
    }
}
