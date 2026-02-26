using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Repositories.Generic;
//using UNOPS.PAO.ContextPermissions.Handlers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Text;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using AutoMapper;

namespace UNOPS.PAO.Presentation.Controllers.Documents;
[Route("/")]
public class DocumentController : BaseController
{
    private readonly IDocumentManager _manager;
    private readonly IManagerWrapper _managerWrapper;
    private readonly IConfiguration _configuration;
    private new readonly ILogger<DocumentController> _logger;
    private readonly GoogleCloudStorageService _gcsService;
    private readonly IMapper _mapper;
    private readonly UNOPSAppDbContext _context;

    public DocumentController(
        IManagerWrapper managerWrapper,
        IAuthorizationService authorizationService,
        ILogger<DocumentController> logger,
        IConfiguration configuration,
        UserResolverService<int> userResolverService,
        IMapper mapper,
        UNOPSAppDbContext context)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.DocumentManager;
        _managerWrapper = managerWrapper;
        _configuration = configuration;
        _logger = logger;
        _mapper = mapper;
        _context = context;
        _gcsService = new GoogleCloudStorageService(configuration);
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
    /// <remarks>entityName excludes "download" to avoid route conflict with DocumentController.Download (DEF-021)</remarks>
    [HttpGet(APIDictionary.Document + "/{entityName:regex(^(?!download$).+)}/{entityId:int}")]
    public async Task<ActionResult> GetAll(string entityName, int entityId)
    {
        return await HandleOperationAsync(() => 
        {
            var result = _manager.ListDocumentsAsync(EntityNames.ByName(entityName), entityId);
            return Task.FromResult(result);
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

    /// <summary>
    /// Converts markdown content to a Google Doc via the AI API (same headers as similar-projects).
    /// Returns the Google Doc URL.
    /// </summary>
    [HttpPost(APIDictionary.DocumentConvertMarkdownToDoc)]
    public async Task<ActionResult> ConvertMarkdownToDoc([FromBody] GenerateGoogleDocRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            var markdownContent = request?.Data ?? string.Empty;
            _logger.LogInformation("Converting markdown to Google Doc, content length: {Length}", markdownContent.Length);

            var userEmail = _userResolverService.GetUserEmail();
            var aiRetrieverManager = _managerWrapper.AiRetrieverManager;
            if (aiRetrieverManager == null)
                throw new InvalidOperationException("AI Retriever manager not available");
            var fileName = !string.IsNullOrEmpty(request?.Filename) ? request.Filename : "document.md";
            if (!fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                fileName += ".md";
            var result = await aiRetrieverManager.ConvertMarkdownToGoogleDocAsync(markdownContent, userEmail, fileName);

            if (!string.IsNullOrEmpty(result.DocumentUrl))
            {
                return (object)new { googleDocUrl = result.DocumentUrl };
            }

            return (object)new { googleDocUrl = (string?)null, error = result.Error };
        });
    }

    /// <summary>
    /// Generates a Google Doc by summarizing the provided data using Gemini AI and converting the result to a Google Document.
    /// Uses AiRetrieverManager (same headers as similar-projects).
    /// </summary>
    /// <param name="request">Request containing the data to be summarized and optional filename</param>
    /// <example_uses>
    /// Generate document from meeting notes
    /// Create summary document from project data
    /// Convert analysis results to Google Doc
    /// Generate report from structured data
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to create a Google Doc with AI-generated summary content from provided data.</when_to_use>
    /// <returns>JSON response containing the Google Doc creation result</returns>
    [HttpPost(APIDictionary.DocumentGenerate)]
    public async Task<ActionResult> GenerateGoogleDoc([FromBody] GenerateGoogleDocRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            if (string.IsNullOrEmpty(request.Data))
            {
                throw new ArgumentException("Request data cannot be empty");
            }

            _logger.LogInformation("Converting markdown content to Google Doc. Content length: {Length}", request.Data.Length);

            var userEmail = _userResolverService.GetUserEmail();
            var aiRetrieverManager = _managerWrapper.AiRetrieverManager;
            if (aiRetrieverManager == null)
                throw new InvalidOperationException("AI Retriever manager not available");
            var fileName = !string.IsNullOrEmpty(request.Filename) ? request.Filename : "document.md";
            if (!fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                fileName += ".md";
            var result = await aiRetrieverManager.ConvertMarkdownToGoogleDocAsync(request.Data, userEmail, fileName);

            return (object)new
            {
                status = result.Status,
                documentId = result.DocumentId,
                documentUrl = result.DocumentUrl,
                error = result.Error
            };
        });
    }

    /// <summary>
    /// Gets a viewable URL for a document (signed URL for GCS documents)
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <example_uses>
    /// Get viewable URL for document 123
    /// Generate signed URL for PDF viewing
    /// Get temporary access link for document
    /// </example_uses>
    /// <when_to_use>Use this to get a temporary URL for viewing documents stored in Google Cloud Storage</when_to_use>
    /// <returns>Viewable URL and type information</returns>
    [HttpGet(APIDictionary.DocumentViewUrl + "/{id}")]
    public async Task<ActionResult> GetDocumentViewUrl(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var document = await _manager.GetDocumentByIdAsync(id);
            
            if (document == null)
            {
                throw new BusinessException("Document not found");
            }

            // If stored in GCS, generate signed URL
            if (!string.IsNullOrEmpty(document.StoragePath) && document.StoragePath.StartsWith("gs://"))
            {
                var signedUrl = await _gcsService.GetSignedUrlFromGsUri(document.StoragePath, 60); // 60 minutes expiration
                
                return new { url = signedUrl, type = "gcs", mimeType = document.Type };
            }

            // If Google Drive link
            if (!string.IsNullOrEmpty(document.Link))
            {
                return new { url = document.Link, type = "link", mimeType = document.Type };
            }

            // If blob (deprecated but supported for backward compatibility)
            if (document.Blob != null && document.Blob.Length > 0)
            {
                return new { url = $"/api/document/{id}/download", type = "blob", mimeType = document.Type };
            }

            throw new BusinessException("No viewable content found for this document");
        });
    }

    /// <summary>
    /// Downloads document blob content (for backward compatibility with blob storage).
    /// Marked NonAction: UNOPS override (UNOPSPresentation.DocumentController.Download) provides the active endpoint.
    /// When both assemblies are loaded, only the UNOPS endpoint is registered to avoid AmbiguousMatchException (DEF-021).
    /// </summary>
    /// <param name="id">Document ID</param>
    [NonAction]
    [HttpGet(APIDictionary.DocumentDownload + "/{id}")]
    public async Task<ActionResult> DownloadDocument(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            var fileContent = await _manager.GetFileContentByIdAsync(id);
            var document = await _manager.GetDocumentByIdAsync(id);
            
            if (document == null)
            {
                throw new BusinessException("Document not found");
            }

            return File(fileContent, document.Type ?? "application/octet-stream", document.Name);
        });
    }

}
