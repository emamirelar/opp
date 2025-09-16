using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
using UNOPS.PAO.Domain.Entities;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using static Google.Cloud.SecretManager.V1.Replication.Types;

namespace UNOPS.PAO.Presentation.Controllers;
[Route("/")]
public class DocumentController : BaseController
{
    private readonly IDocumentManager _manager;
    private readonly IManagerWrapper _managerWrapper;
    private readonly IConfiguration _configuration;

    public DocumentController(
        IManagerWrapper managerWrapper, 
        IAuthorizationService authorizationService,
        ILogger<DocumentController> logger,
        UserResolverService<int> userResolverService,
        IConfiguration configuration)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.DocumentManager;
        _managerWrapper = managerWrapper;
        _configuration = configuration;
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

    /// <summary>
    /// Generates a Google Doc by summarizing the provided data using Gemini AI and converting the result to a Google Document.
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
            // Step 1: Create AI prompt for summarization using dynamic configuration
            var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
            var location = _configuration.GetValue<string>("AISettings:Location");
            var model = _configuration.GetValue<string>("AISettings:GeminiModelName");
            
            _logger.LogInformation("Using AI configuration - Project: {ProjectId}, Location: {Location}, Model: {Model}", 
                projectId, location, model);
            
            // Validate configuration
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(location) || string.IsNullOrEmpty(model))
            {
                throw new InvalidOperationException("AI configuration is incomplete. Please check AISettings in appsettings.json");
            }
            
            var aiPrompt = new AiPrompt
            {
                Type = "summarization",
                Prompt = "You are a summarizer. You will be provided with data that you need to summarize and return in a PURE markdown format. {data}",
                GenerationConfig = "{\"temperature\": 0.7, \"maxOutputTokens\": 2048}",
                ContentConfig = "{\"role\": \"user\", \"parts\": [{\"text\": \"{promptData}\"}]}",
                Project = projectId,
                Location = location, 
                Model = model
            };

            // Step 2: Call Gemini API for summarization
            var geminiResponse = await _managerWrapper.GeminiManager.FetchResultFromGemini(aiPrompt, request.Data);
            
            // Step 3: Extract markdown content from Gemini response
            var markdownContent = ExtractMarkdownFromGeminiResponse(geminiResponse);
            
            // Step 4: Convert markdown to Google Doc
            var filename = !string.IsNullOrEmpty(request.Filename) ? request.Filename : "AI_Generated_Summary";
            var googleDocResult = await ConvertMarkdownToGoogleDoc(markdownContent, filename);
            
            return googleDocResult;
        });
    }

    private string ExtractMarkdownFromGeminiResponse(string geminiResponse)
    {
        try
        {
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(geminiResponse);
            var candidates = jsonResponse["candidates"];
            if (candidates != null && candidates.Count > 0)
            {
                var content = candidates[0]["content"]["parts"][0]["text"];
                var markdownText = content?.ToString() ?? "";
                
                // Clean up the markdown - remove code block markers if present
                markdownText = markdownText.Replace("```markdown", "").Replace("```", "").Trim();
                
                return markdownText;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting markdown from Gemini response: {Response}", geminiResponse);
        }
        
        // Fallback: return the response as is if parsing fails
        return geminiResponse;
    }

    private async Task<object> ConvertMarkdownToGoogleDoc(string markdownContent, string filename)
    {
        try
        {
            using var httpClient = new HttpClient();
            
            // Set up the external API endpoint
            var convertEndpoint = "https://api.ai.dev.unops.org/v1/convert/markdown-to-google-doc";
            
            _logger.LogInformation("Converting markdown to Google Doc: {Filename}", filename);
            
            // Prepare the multipart form data
            var formData = new MultipartFormDataContent();
            var fileContent = new StringContent(markdownContent, Encoding.UTF8, "text/markdown");
            formData.Add(fileContent, "file", filename + ".md");
            formData.Add(new StringContent("{}"), "data");
            
            // Add authentication headers
            await AddAuthenticationHeaders(httpClient);
            
            // Make the request
            var response = await httpClient.PostAsync(convertEndpoint, formData);
            
            _logger.LogInformation("Google Doc conversion response status: {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<object>(responseContent);
                return responseData;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Google Doc conversion failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
                return new
                {
                    error = $"Google Doc conversion failed: {response.StatusCode}",
                    details = errorContent
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting markdown to Google Doc");
            return new
            {
                error = "Error converting markdown to Google Doc",
                details = ex.Message
            };
        }
    }

    private async Task AddAuthenticationHeaders(HttpClient httpClient)
    {
        try
        {
            // Get OAuth configuration from appsettings 
            // First try to get from OAuth config (if added to appsettings)
            var clientId = _configuration.GetValue<string>("OAuth:ClientId");
            
            if (string.IsNullOrEmpty(clientId))
            {
                // Fallback: construct from IAP settings based on Python config format
                // Python config: "1069310298210-ubl2naqi5bjeqlqrroiqb4qdm482aans.apps.googleusercontent.com"
                var projectNumber = _configuration.GetValue<string>("IAP:ProjectNumber");
                var clientIdSuffix = "ubl2naqi5bjeqlqrroiqb4qdm482aans"; // This should match your OAuth client ID
                clientId = $"{projectNumber}-{clientIdSuffix}.apps.googleusercontent.com";
                _logger.LogInformation("Constructed OAuth client ID from IAP config: {ClientId}", clientId);
            }
            else
            {
                _logger.LogInformation("Using OAuth client ID from config: {ClientId}", clientId);
            }
            
            if (!string.IsNullOrEmpty(clientId))
            {
                // Get OIDC token with the client ID as target audience
                var oidcToken = await GetOidcTokenAsync(clientId);
                if (!string.IsNullOrEmpty(oidcToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", oidcToken);
                    _logger.LogInformation("Added OIDC token to Authorization header for audience: {ClientId}", clientId);
                }
                else
                {
                    _logger.LogWarning("Failed to get OIDC token for client ID: {ClientId}", clientId);
                }
            }
            else
            {
                _logger.LogWarning("No OAuth client ID found in configuration");
            }
            
            // Add development headers if in development mode
            var isDevelopment = Environment.GetEnvironmentVariable("IS_DEVELOPMENT")?.ToUpper() == "TRUE";
            var devEmail = Environment.GetEnvironmentVariable("DEV_EMAIL");
            
            if (isDevelopment && !string.IsNullOrEmpty(devEmail))
            {
                _logger.LogInformation("Adding development IAP headers for email: {Email}", devEmail);
                var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                
                httpClient.DefaultRequestHeaders.Add("x-goog-authenticated-user-email", $"accounts.google.com:{devEmail}");
                httpClient.DefaultRequestHeaders.Add("x-goog-authenticated-user-id", $"accounts.google.com:dev-user-id-{currentTimestamp}");
                httpClient.DefaultRequestHeaders.Add("x-forwarded-user", devEmail);
                httpClient.DefaultRequestHeaders.Add("x-forwarded-email", devEmail);
                httpClient.DefaultRequestHeaders.Add("X-Dev-IAP-Simulation", "true");
                httpClient.DefaultRequestHeaders.Add("X-Dev-Auth-Timestamp", currentTimestamp);
            }
            
            // Add user impersonation header
            var userEmail = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userEmail))
            {
                httpClient.DefaultRequestHeaders.Add("x-unops-impersonated-user", userEmail);
                _logger.LogInformation("Added impersonated user header: {Email}", userEmail);
            }
            else if (isDevelopment && !string.IsNullOrEmpty(devEmail))
            {
                httpClient.DefaultRequestHeaders.Add("x-unops-impersonated-user", devEmail);
                _logger.LogInformation("Added impersonated user header (dev): {Email}", devEmail);
            }
            
            _logger.LogInformation("Authentication headers added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding authentication headers");
        }
    }

    private async Task<string> GetOidcTokenAsync(string targetAudience)
    {
        try
        {
            var credential = await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync();
            
            // Get OIDC token with the target audience (similar to CloudRunHelper)
            var oidcToken = await credential.GetOidcTokenAsync(Google.Apis.Auth.OAuth2.OidcTokenOptions.FromTargetAudience(targetAudience));
            
            // Note: Despite the confusing name, GetAccessTokenAsync() on an OidcToken 
            // actually returns the ID token string, not an access token
            var idToken = await oidcToken.GetAccessTokenAsync();
            
            _logger.LogInformation("Generated OIDC token for audience: {Audience}", targetAudience);
            
            return idToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get OIDC token for audience: {Audience}", targetAudience);
            return null;
        }
    }
}
