using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
//using UNOPS.PAO.ContextPermissions.Handlers;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSPresentation.Helpers;
using System.Text;
using System.Text.Json;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;
[Route("/")]
public class DocumentController : BaseController
{
    private readonly UNOPSDocumentManager _manager;
    private readonly IManagerWrapper _managerWrapper;
    private readonly IConfiguration _configuration;
    private readonly GoogleCloudStorageService _gcsService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IMapper mapper, 
        IGoogleDriveDocumentManager driveManager, 
        IConfiguration configuration, 
        UNOPSAppDbContext context, 
        UserManager<PAOIdentityUser> userManager, 
        IManagerWrapper managerWrapper, 
        IAuthorizationService authorizationService,
        ILogger<DocumentController> logger,
        UserResolverService<int> userResolverService,
        IServiceProvider serviceProvider)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = new UNOPSDocumentManager(driveManager, configuration, mapper, context, userManager, serviceProvider);
        _managerWrapper = managerWrapper;
        _configuration = configuration;
        _gcsService = new GoogleCloudStorageService(configuration);
        _logger = logger;
    }

    [HttpGet(APIDictionary.Document + "/entity/{entityType}/{entityId}")]
    public async Task<ActionResult> GetDocumentsByEntity(string entityType, int entityId)
    {
        return await HandleOperationAsync(async () =>
        {
            var documents = await _manager.GetDocumentsByEntityAsync(entityType, entityId);
            return documents;
        });
    }

    [HttpPost(APIDictionary.DocumentUpload)]
    public async Task<ActionResult> Create([FromForm] DocumentUploadModel model)
    {
        return await HandleOperationAsync(async () =>
        {
            // Log the UploadToGCS flag for debugging
            _logger.LogInformation("DocumentUpload: UploadToGCS={UploadToGCS}, SkipDatabaseSave={SkipDatabaseSave}, FileName={FileName}", 
                model.UploadToGCS, model.SkipDatabaseSave, model.File?.FileName);

            /*var isInternalUser = await this.IsInternalUser();
            var canCreateResult = await HasPermission(model.ParentEntityType.ToString(), model.ParentEntityId, this.GetRequirement(isInternalUser, model.ParentEntityType.ToString(), "Create"));

            if (!canCreateResult)
            {
                throw new UnauthorizedAccessException("You don't have permission to create this document");
            }*/

            // Check if UploadToGCS is specified (for client-side PDF uploads)
            if (model.UploadToGCS && model.File != null)
            {
                _logger.LogInformation("Uploading to GCS: {FileName}", model.File.FileName);
                
                // Validate PDF
                if (model.File.ContentType != "application/pdf" && !model.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessException("Only PDF files are supported for GCS upload");
                }

                // Upload to GCS
                var gsUri = await _gcsService.UploadPdfAsync(model.File, model.ParentEntityType.ToString().ToLower(), model.ParentEntityId);
                
                _logger.LogInformation("GCS Upload successful: {GsUri}", gsUri);
                
                // If SkipDatabaseSave is true, return only the GCS path without persisting to database
                if (model.SkipDatabaseSave)
                {
                    _logger.LogInformation("SkipDatabaseSave=true, returning GCS path only without database persistence");
                    return (object)new
                    {
                        storagePath = gsUri,
                        mimeType = model.File.ContentType,
                        fileName = model.File.FileName,
                        message = "File uploaded to GCS successfully (not persisted to database)"
                    };
                }
                
                // Update model to use GCS storage path instead of blob
                model.StoragePath = gsUri;
                model.Blob = null; // Don't store blob when using GCS
            }
            else
            {
                _logger.LogWarning("NOT uploading to GCS. UploadToGCS={UploadToGCS}, HasFile={HasFile}", 
                    model.UploadToGCS, model.File != null);
            }

            // Persist to database (only if SkipDatabaseSave is false)
            var result = await _manager.CreateDocumentAsync(model);

            if (result == null)
            {
                throw new BusinessException("Failed to create document");
            }

            return (object)result;
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

    /*[HttpPost(APIDictionary.Document + "/convert-url")]
    public async Task<ActionResult> ConvertUrl([FromBody] ConvertUrlRequest request)
    {
        return await HandleOperationAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                throw new BusinessException("URL is required");
            }

            var endpoint = _configuration["ExternalApiSettings:ConvertUrlEndpoint"];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new BusinessException("Convert URL endpoint is not configured");
            }

            _logger.LogInformation("Starting ConvertUrl request for URL: {Url}", request.Url);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("ExternalApiSettings:Timeout", 60));

            // Get current user email for impersonation
            var userEmail = User.Identity?.Name;
            _logger.LogInformation("Current user identity: {UserEmail}", userEmail ?? "null");

            // Build authenticated headers using simplified IAP helper
            _logger.LogInformation("Creating IapAuthenticationHelper...");
            var loggerFactory = new LoggerFactory();
            var iapHelper = new IapAuthenticationHelper(
                loggerFactory.CreateLogger<IapAuthenticationHelper>(),
                _configuration);
            
            var environment = _configuration["AppConfig:Environment"];
            var isLocalDevelopment = environment == "Development" || environment == "Local";
            var skipAuth = _configuration.GetValue<bool>("ExternalApiSettings:SkipAuthenticationInDevelopment", false);
            
            _logger.LogInformation("Calling BuildIapHeadersAsync... Environment: {Env}, IsLocal: {IsLocal}, SkipAuth: {Skip}",
                environment, isLocalDevelopment, skipAuth);
            
            Dictionary<string, string> headers;
            try
            {
                headers = await iapHelper.BuildIapHeadersAsync(
                    userEmail, 
                    isLocalDevelopment && skipAuth);
                _logger.LogInformation("Received {Count} headers from BuildIapHeadersAsync", headers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build IAP headers: {Message}", ex.Message);
                throw new BusinessException($"IAP Authentication failed: {ex.Message}");
            }

            // Add headers to HttpClient
            _logger.LogInformation("Adding headers to HttpClient...");
            foreach (var header in headers)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Skipping Content-Type header (will be set by StringContent)");
                    continue; // Will be set by StringContent
                }

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                _logger.LogInformation("Added header: {Key} = {Value}", 
                    header.Key, 
                    header.Key.Contains("Authorization") ? "[REDACTED]" : header.Value);
            }

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                }),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation("Calling convert URL endpoint: {Endpoint}", endpoint);

            var response = await httpClient.PostAsync(endpoint, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Received response with status code: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Convert URL API returned error: {StatusCode} - {Content}", 
                    response.StatusCode, responseContent);
                throw new BusinessException($"External API error: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<ConvertUrlResponse>(responseContent, 
                new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

            _logger.LogInformation("Successfully converted URL");
            return result;
        });
    }*/

    [HttpGet(APIDictionary.Document + "/Download/{id}")]
    public async Task<ActionResult> Download(int id)
    {
        // File downloads must handle responses differently than normal API operations
        try
        {
            var document = await _manager.GetDocumentByIdAsync(id);

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

            // Get file content - handles both blob and Google Drive scenarios
            byte[] contents;
            
            if (document.Blob != null && document.Blob.Length > 0)
            {
                // File is stored as blob in database
                contents = document.Blob;
            }
            else if (!string.IsNullOrWhiteSpace(document.GoogleId))
            {
                // File is stored in Google Drive
                var userToImpersonate = await _manager.GetCreatorEmailAsync(id);
                contents = await _manager.GetFileContentAsync(document.GoogleId, userToImpersonate);
            }
            else
            {
                return NotFound("Document content not found");
            }

            return File(contents, document.Type ?? "application/octet-stream", document.Name);
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
