
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
//using UNOPS.PAO.ContextPermissions.Handlers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using static Google.Cloud.SecretManager.V1.Replication.Types;

namespace UNOPS.PAO.Presentation.Controllers;
[Route("/")]
[ApiController]
public class DocumentController : ControllerBase
{
    private IDocumentManager manager;
    private IManagerWrapper managerWrapper;
    private IAuthorizationService authorizationService;

    public DocumentController(IManagerWrapper managerWrapper, IAuthorizationService authorizationService)
    {
        this.manager = managerWrapper.DocumentManager;
        this.managerWrapper = managerWrapper;
        this.authorizationService = authorizationService;
    }

    [HttpGet(APIDictionary.Document + "/{entityName}/{entityId}")]
    public async Task<ActionResult> GetAll(string entityName, int entityId)
    {
        /*var canListResult = await this.HasPermission(EntityNames.ByName(entityName).ToString(), entityId, this.GetRequirement(EntityNames.ByName(entityName).ToString(), "List"));

        if (!canListResult)
        {
            return Forbid();
        }*/

        return Ok(manager.ListDocumentsAsync(EntityNames.ByName(entityName), entityId));
    }

    [HttpGet(APIDictionary.Document + "/{id}")]
    public async Task<ActionResult> Get(int id)
    {
        var x = await manager.GetDocumentByIdAsync(id);

        if (x == null)
        {
            return NotFound();
        }

        var parentEntity = await manager.GetDocumentParentEntityByIdAsync(id);

        /*if (parentEntity != null)
        {
            var canReadResult = await this.HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, this.GetRequirement(parentEntity.Value.EntityType, "Read"));

            if (!canReadResult)
            {
                return Forbid();
            }
        }*/

        return Ok(x);
    }

    [HttpPut(APIDictionary.Document)]
    public async Task<IActionResult> Update([FromBody] UpdateDocumentRequest req)
    {
        var parentEntity = await manager.GetDocumentParentEntityByIdAsync(req.Id);

        /*if (parentEntity != null)
        {
            var canEditResult = await this.HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, this.GetRequirement(parentEntity.Value.EntityType, "Edit"));

            if (!canEditResult)
            {
                return Forbid();
            }
        }*/

        await manager.UpdateDocumentAsync(req);

        return NoContent();
    }

    private async Task<bool> HasPermission(string documentParentEntityType, int documentParentEntityId, IAuthorizationRequirement? authorizationRequirement)
    {
        if (authorizationRequirement != null)
        {
            if (documentParentEntityType == nameof(DocumentParentEntityType.Contact))
            {
                var contact = await managerWrapper.ContactManager.GetContactAsync(documentParentEntityId);
                var canResult = await authorizationService.AuthorizeAsync(User, contact, authorizationRequirement);

                return canResult.Succeeded;
            }
            else if (documentParentEntityType == nameof(DocumentParentEntityType.Partner))
            {
                var partner = await managerWrapper.PartnerManager.GetPartnerAsync(documentParentEntityId);
                var canResult = await authorizationService.AuthorizeAsync(User, partner, authorizationRequirement);

                return canResult.Succeeded;
            }
            else
            {
                return true;
            }
        }

        return true;
    }

    private IAuthorizationRequirement? GetRequirement(string documentParentEntityType, string documentAction)
    {
        if (documentParentEntityType == nameof(DocumentParentEntityType.Contact))
        {
            if (documentAction == "Edit")
            {
                //return ContactActions.Edit;
            }
            else if (documentAction == "Read" || documentAction == "List")
            {
                //return ContactActions.View;
            }
        }
        else if (documentParentEntityType == nameof(DocumentParentEntityType.Partner))
        {
            if (documentAction == "Edit")
            {
                //return PartnerActions.Edit;
            }
            else if (documentAction == "Read" || documentAction == "List")
            {
                //return PartnerActions.View;
            }
        }
        return null;
    }
}
