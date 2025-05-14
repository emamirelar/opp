using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers;
[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class DocumentTypeController : ControllerBase
{
    private IDocumentTypeManager manager;

    public DocumentTypeController(IManagerWrapper manager)
    {
        this.manager = manager.DocumentTypeManager;
    }

    [HttpGet(APIDictionary.DocumentType + "/{entityName}")]
    public ActionResult GetAll(string entityName,
        int pageIndex = -1, int pageSize = 10,
        string? orderBy = null, bool? ascending = true)
    {

        var parameters = new DocumentTypeRequestParameters()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            OrderBy = orderBy,
            Ascending = ascending,
            EntityType = EntityNames.ByName(entityName)
        };

        return Ok(manager.GetDocumentTypesAsync(parameters));
    }
}
