using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
//using UNOPS.PAO.ContextPermissions.Handlers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using static Google.Cloud.SecretManager.V1.Replication.Types;

namespace UNOPS.PAO.Presentation.Controllers;
[Route("/")]
public class DocumentController : BaseController
{
    private readonly IDocumentManager _manager;
    private readonly IManagerWrapper _managerWrapper;

    public DocumentController(
        IManagerWrapper managerWrapper, 
        IAuthorizationService authorizationService,
        ILogger<DocumentController> logger,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.DocumentManager;
        _managerWrapper = managerWrapper;
    }

    [HttpGet(APIDictionary.Document + "/{entityName}/{entityId}")]
    public async Task<ActionResult> GetAll(string entityName, int entityId)
    {
        return await HandleOperationAsync(async () => 
        {
            /*var canListResult = await this.HasPermission(EntityNames.ByName(entityName).ToString(), entityId, this.GetRequirement(EntityNames.ByName(entityName).ToString(), "List"));

            if (!canListResult)
            {
                throw new UnauthorizedAccessException("You don't have permission to view these documents");
            }*/

            return _manager.ListDocumentsAsync(EntityNames.ByName(entityName), entityId);
        });
    }

    [HttpGet(APIDictionary.Document + "/{id}")]
    public async Task<ActionResult> Get(int id)
    {
        return await HandleOperationAsync(async () => 
        {
            var document = await _manager.GetDocumentByIdAsync(id);

            if (document == null)
            {
                throw new BusinessException($"Document with ID {id} not found");
            }

            var parentEntity = await _manager.GetDocumentParentEntityByIdAsync(id);

            /*if (parentEntity != null)
            {
                var canReadResult = await this.HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, this.GetRequirement(parentEntity.Value.EntityType, "Read"));

                if (!canReadResult)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view this document");
                }
            }*/

            return document;
        });
    }

    [HttpPut(APIDictionary.Document)]
    public async Task<ActionResult> Update([FromBody] UpdateDocumentRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            var parentEntity = await _manager.GetDocumentParentEntityByIdAsync(req.Id);

            /*if (parentEntity != null)
            {
                var canEditResult = await HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, GetRequirement(parentEntity.Value.EntityType, "Edit"));

                if (!canEditResult)
                {
                    throw new UnauthorizedAccessException("You don't have permission to edit this document");
                }
            }*/

            await _manager.UpdateDocumentAsync(req);
        });
    }

    private async Task<bool> HasPermission(string documentParentEntityType, int documentParentEntityId, IAuthorizationRequirement? authorizationRequirement)
    {
        if (authorizationRequirement != null)
        {
            if (documentParentEntityType == nameof(DocumentParentEntityType.Contact))
            {
                var contact = await _managerWrapper.ContactManager.GetContactAsync(documentParentEntityId);
                var canResult = await _authorizationService.AuthorizeAsync(User, contact, authorizationRequirement);

                return canResult.Succeeded;
            }
            else if (documentParentEntityType == nameof(DocumentParentEntityType.Partner))
            {
                var partner = await _managerWrapper.PartnerManager.GetPartnerAsync(documentParentEntityId);
                var canResult = await _authorizationService.AuthorizeAsync(User, partner, authorizationRequirement);

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
