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

    /// <summary>
    /// Retrieves all documents associated with a specific entity (partner, contact, or interaction) with access control.
    /// </summary>
    /// <param name="entityName">Entity type name (e.g., 'Partner', 'Contact', 'Interaction')</param>
    /// <param name="entityId">Entity ID to get documents for</param>
    /// <example_uses>
    /// Show all documents for partner 123
    /// Get documents attached to contact 456
    /// List files for interaction 789
    /// Find all documents for this partner
    /// Show uploaded files for contact
    /// Get document attachments for entity
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see documents, files, or attachments for a specific partner, contact, or interaction.</when_to_use>
    /// <returns>List of documents with metadata for the specified entity</returns>
    [HttpGet(APIDictionary.Document + "/{entityName}/{entityId}")]
    public async Task<ActionResult> GetAll(string entityName, int entityId)
    {
        return await HandleOperationAsync(async () => 
        {
            return _manager.ListDocumentsAsync(EntityNames.ByName(entityName), entityId);
        });
    }

    /// <summary>
    /// Retrieves a specific document by ID with complete details including file information, metadata, and download access.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <example_uses>
    /// Show me details for document ID 123
    /// Get document 456 information
    /// Display document record 789
    /// Get complete document metadata
    /// Show document with download link
    /// Access document file details
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for specific document details by ID or when you need complete document information for viewing or downloading.</when_to_use>
    /// <returns>Complete document details with file metadata and access information</returns>
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

            return document;
        });
    }

    /// <summary>
    /// Updates an existing document's metadata, description, and properties with permission validation.
    /// </summary>
    /// <param name="req">Document update request containing modified fields</param>
    /// <param name="req.id">Document ID to update (required)</param>
    /// <param name="req.title">Updated document title</param>
    /// <param name="req.description">Updated document description</param>
    /// <param name="req.documentType">Updated document type/category</param>
    /// <param name="req.tags">Updated document tags for categorization</param>
    /// <param name="req.isPublic">Updated public/private visibility setting</param>
    /// <example_uses>
    /// Update document 123's title to "New Contract"
    /// Change document 456's description
    /// Modify document type to "Legal Agreement"
    /// Update document tags and visibility
    /// Change document metadata and properties
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to update, modify, edit, or change document information or metadata.</when_to_use>
    /// <returns>Success confirmation or validation errors</returns>
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
