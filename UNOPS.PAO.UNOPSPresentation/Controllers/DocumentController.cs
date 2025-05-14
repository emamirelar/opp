using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
//using UNOPS.PAO.ContextPermissions.Handlers;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSPresentation.Helpers;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;
[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class DocumentController : ControllerBase
{
    private UNOPSDocumentManager manager;
    private IManagerWrapper managerWrapper;
    private IAuthorizationService authorizationService;


    public DocumentController(IMapper mapper, IGoogleDriveDocumentManager driveManager, IConfiguration configuration, UNOPSAppDbContext context, UserManager<PAOIdentityUser> userManager, IManagerWrapper managerWrapper, IAuthorizationService authorizationService)
    {
        this.manager = new UNOPSDocumentManager(driveManager, configuration, mapper, context, userManager);
        this.managerWrapper = managerWrapper;
        this.authorizationService = authorizationService;
    }

    [HttpPost(APIDictionary.DocumentUpload)]
    public async Task<IActionResult> Create([FromForm] DocumentUploadModel model)
    {
        /*var isInternalUser = await this.IsInternalUser();
        var canCreateResult = await HasPermission(model.ParentEntityType.ToString(), model.ParentEntityId, this.GetRequirement(isInternalUser, model.ParentEntityType.ToString(), "Create"));

        if (!canCreateResult)
        {
            return Forbid();
        }*/

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
        /*var isInternalUser = await this.IsInternalUser();
        var canLinkResult = await HasPermission(model.ParentEntityType.ToString(), model.ParentEntityId, this.GetRequirement(isInternalUser, model.ParentEntityType.ToString(), "Link"));

        if (!canLinkResult)
        {
            return Forbid();
        }*/

        var result = await manager.LinkDocumentAsync(model);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(Create), result.Id, result);
    }

    [HttpDelete(APIDictionary.Document + "/{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        var parentEntity = await manager.GetDocumentParentEntityByIdAsync(id);

        if (parentEntity != null)
        {
            /*var isInternalUser = await this.IsInternalUser();
            var canDeleteResult = await this.HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, this.GetRequirement(isInternalUser, parentEntity.Value.EntityType, "Delete"));

            if (!canDeleteResult)
            {
                return Forbid();
            }*/
        }

        await manager.DeleteDocumentAsync(id);
        return NoContent();
    }


    [HttpGet(APIDictionary.Document + "/Download/{id}")]
    public async Task<ActionResult> Download(int id)
    {
        var document = await manager.GetDocumentByIdAsync(id);
        var userToImpersonate = await manager.GetCreatorEmailAsync(id);

        if (document == null)
        {
            return NotFound();
        }

        var parentEntity = await manager.GetDocumentParentEntityByIdAsync(id);

        if (parentEntity != null)
        {
            /*var isInternalUser = await this.IsInternalUser();
            var canDownloadResult = await this.HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, this.GetRequirement(isInternalUser, parentEntity.Value.EntityType, "Download"));

            if (!canDownloadResult)
            {
                return Forbid();
            }*/
        }

        var contents = await manager.GetFileContentAsync(document.GoogleId, userToImpersonate);

        return File(contents, document.Type ?? string.Empty, document.Name);
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

    private IAuthorizationRequirement? GetRequirement(bool isInternalUser, string documentParentEntityType, string documentAction)
    {
        if (documentParentEntityType == nameof(DocumentParentEntityType.Contact))
        {
            if (documentAction == "Create" || documentAction == "Delete" || documentAction == "Link")
            {
                //return ContactActions.Edit;
            }
            else if (documentAction == "Download")
            {
                //return ContactActions.View;
            }
        }
        else if (documentParentEntityType == nameof(DocumentParentEntityType.Partner))
        {
            if (documentAction == "Create" || documentAction == "Delete" || documentAction == "Link")
            {
                //return PartnerActions.Edit;
            }
            else if (documentAction == "Download")
            {
                //return PartnerActions.View;
            }
        }
        return null;
    }

    /*private async Task<bool> IsInternalUser()
    {
        if (HttpContext.User.Identity == null)
        {
            return false;
        }

        var user = await managerWrapper.UserManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);

        if (user == null)
        {
            return false;
        }

        return user.IsInternal;
    }*/
}
