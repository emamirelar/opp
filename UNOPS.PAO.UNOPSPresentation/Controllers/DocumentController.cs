using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
//using UNOPS.PAO.ContextPermissions.Handlers;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSPresentation.Helpers;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;
[Route("/")]
public class DocumentController : BaseController
{
    private readonly UNOPSDocumentManager _manager;
    private readonly IManagerWrapper _managerWrapper;

    public DocumentController(
        IMapper mapper, 
        IGoogleDriveDocumentManager driveManager, 
        IConfiguration configuration, 
        UNOPSAppDbContext context, 
        UserManager<PAOIdentityUser> userManager, 
        IManagerWrapper managerWrapper, 
        IAuthorizationService authorizationService,
        ILogger<DocumentController> logger,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = new UNOPSDocumentManager(driveManager, configuration, mapper, context, userManager);
        _managerWrapper = managerWrapper;
    }

    [HttpPost(APIDictionary.DocumentUpload)]
    public async Task<ActionResult> Create([FromForm] DocumentUploadModel model)
    {
        return await HandleOperationAsync(async () =>
        {
            /*var isInternalUser = await this.IsInternalUser();
            var canCreateResult = await HasPermission(model.ParentEntityType.ToString(), model.ParentEntityId, this.GetRequirement(isInternalUser, model.ParentEntityType.ToString(), "Create"));

            if (!canCreateResult)
            {
                throw new UnauthorizedAccessException("You don't have permission to create this document");
            }*/

            var result = await _manager.CreateDocumentAsync(model);

            if (result == null)
            {
                throw new BusinessException("Failed to create document");
            }

            return result;
        }, 201);
    }

    [HttpPost(APIDictionary.DocumentLink)]
    public async Task<ActionResult> Link([FromBody] DocumentLinkModel model)
    {
        return await HandleOperationAsync(async () =>
        {
            /*var isInternalUser = await this.IsInternalUser();
            var canLinkResult = await HasPermission(model.ParentEntityType.ToString(), model.ParentEntityId, this.GetRequirement(isInternalUser, model.ParentEntityType.ToString(), "Link"));

            if (!canLinkResult)
            {
                throw new UnauthorizedAccessException("You don't have permission to link this document");
            }*/

            var result = await _manager.LinkDocumentAsync(model);

            if (result == null)
            {
                throw new BusinessException("Failed to link document");
            }

            return result;
        }, 201);
    }

    [HttpDelete(APIDictionary.Document + "/{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var parentEntity = await _manager.GetDocumentParentEntityByIdAsync(id);

            if (parentEntity != null)
            {
                /*var isInternalUser = await this.IsInternalUser();
                var canDeleteResult = await HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, GetRequirement(isInternalUser, parentEntity.Value.EntityType, "Delete"));

                if (!canDeleteResult)
                {
                    throw new UnauthorizedAccessException("You don't have permission to delete this document");
                }*/
            }

            await _manager.DeleteDocumentAsync(id);
        });
    }

    [HttpGet(APIDictionary.Document + "/Download/{id}")]
    public async Task<ActionResult> Download(int id)
    {
        // File downloads must handle responses differently than normal API operations
        try
        {
            var document = await _manager.GetDocumentByIdAsync(id);
            var userToImpersonate = await _manager.GetCreatorEmailAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            var parentEntity = await _manager.GetDocumentParentEntityByIdAsync(id);

            if (parentEntity != null)
            {
                /*var isInternalUser = await this.IsInternalUser();
                var canDownloadResult = await HasPermission(parentEntity.Value.EntityType, parentEntity.Value.EntityId, GetRequirement(isInternalUser, parentEntity.Value.EntityType, "Download"));

                if (!canDownloadResult)
                {
                    return Forbid();
                }*/
            }

            var contents = await _manager.GetFileContentAsync(document.GoogleId, userToImpersonate);

            return File(contents, document.Type ?? string.Empty, document.Name);
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

        var user = await _managerWrapper.UserManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);

        if (user == null)
        {
            return false;
        }

        return user.IsInternal;
    }*/
}
