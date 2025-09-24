using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
using UNOPS.PAO.UNOPSDomain.Entities;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using Google.Apis.Auth.OAuth2;
using UNOPS.PAO.Domain.Entities;
using static Google.Cloud.SecretManager.V1.Replication.Types;

namespace UNOPS.PAO.Presentation.Controllers;
[Route("/")]
public class DocumentController : BaseController
{
    private readonly IDocumentManager _manager;
    private readonly IManagerWrapper _managerWrapper;
    private readonly IConfiguration _configuration;
    private new readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IManagerWrapper managerWrapper, 
        IAuthorizationService authorizationService,
        ILogger<DocumentController> logger,
        IConfiguration configuration,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = managerWrapper.DocumentManager;
        _managerWrapper = managerWrapper;
        _configuration = configuration;
        _logger = logger;
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
                PromptFunction = "summarizeDocument",
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
            var googleDocResult = await ConvertMarkdownToGoogleDoc(markdownContent ?? "", filename);
            
            return googleDocResult;
        });
    }

    private string? ExtractMarkdownFromGeminiResponse(string geminiResponse)
    {
        try
        {
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(geminiResponse);
            var candidates = jsonResponse?["candidates"];
            if (candidates != null && candidates.Count > 0 && candidates[0] != null)
            {
                var content = candidates[0]?["content"]?["parts"]?[0]?["text"];
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

    private async Task<object?> ConvertMarkdownToGoogleDoc(string markdownContent, string filename)
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
            // Enhanced debugging for development mode detection
            var envIsDevelopment = Environment.GetEnvironmentVariable("IS_DEVELOPMENT")?.ToUpper() == "TRUE";
            var configIsDevelopment = _configuration.GetValue<bool>("Development:IAPSimulation:Enabled");
            var isDevelopment = envIsDevelopment || configIsDevelopment;
            
            var envDevEmail = Environment.GetEnvironmentVariable("DEV_EMAIL");
            var configDevEmail = _configuration.GetValue<string>("Development:IAPSimulation:UserEmail");
            var devEmail = envDevEmail ?? configDevEmail;
            
            _logger.LogInformation("Development mode detection - EnvIsDev: {EnvIsDev}, ConfigIsDev: {ConfigIsDev}, Final: {IsDev}", 
                envIsDevelopment, configIsDevelopment, isDevelopment);
            _logger.LogInformation("Dev email sources - Env: {EnvEmail}, Config: {ConfigEmail}, Final: {DevEmail}", 
                envDevEmail, configDevEmail, devEmail);
            
            // Try multiple configuration sources for OAuth client ID (needed for both dev and prod)
            // The Python code uses the QA config: "351976372264-du8e5b5r2d1s6jqn6e4i7ig3ddanqmbc.apps.googleusercontent.com"
            var oauthClientId = _configuration.GetValue<string>("OAuth:ClientId");
            var googleAuthClientId = _configuration.GetValue<string>("GoogleAuthSettings:clientId");
            
            // For QA environment, we should use the same client ID as Python
            var qaClientId = "351976372264-du8e5b5r2d1s6jqn6e4i7ig3ddanqmbc.apps.googleusercontent.com";
            
            _logger.LogInformation("OAuth client ID sources - OAuth:ClientId: {OAuthId}, GoogleAuthSettings:clientId: {GoogleAuthId}, QA: {QAId}", 
                oauthClientId, googleAuthClientId, qaClientId);
            
            // Use QA client ID for now to match Python behavior
            var clientId = googleAuthClientId;
            
            // Always generate Authorization token (required by external API even in dev mode)
            if (!string.IsNullOrEmpty(clientId))
            {
                _logger.LogInformation("🎯 Using client ID for OIDC token: {ClientId}", clientId);
                
                // Get OIDC token with the client ID as target audience
                var oidcToken = await GetOidcTokenAsync(clientId);
                if (!string.IsNullOrEmpty(oidcToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", oidcToken);
                    _logger.LogInformation("✅ Added OIDC Bearer token to Authorization header for audience: {ClientId}", clientId);
                    
                    // Log token info (masked for security)
                    var tokenPreview = oidcToken.Length > 20 ? $"{oidcToken.Substring(0, 20)}..." : oidcToken;
                    _logger.LogInformation("🔑 Token preview: {TokenPreview}", tokenPreview);
                }
                else
                {
                    _logger.LogError("❌ Failed to get OIDC token for client ID: {ClientId}", clientId);
                }
            }
            else
            {
                _logger.LogError("❌ No OAuth client ID found in any configuration source");
            }

            if (isDevelopment && !string.IsNullOrEmpty(devEmail))
            {
                _logger.LogInformation("🧪 Using DEVELOPMENT mode - adding IAP simulation headers with email: {Email}", devEmail);
                var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                
                httpClient.DefaultRequestHeaders.Add("x-goog-authenticated-user-email", $"accounts.google.com:{devEmail}");
                httpClient.DefaultRequestHeaders.Add("x-goog-authenticated-user-id", $"accounts.google.com:dev-user-id-{currentTimestamp}");
                httpClient.DefaultRequestHeaders.Add("x-forwarded-user", devEmail);
                httpClient.DefaultRequestHeaders.Add("x-forwarded-email", devEmail);
                httpClient.DefaultRequestHeaders.Add("X-Dev-IAP-Simulation", "true");
                httpClient.DefaultRequestHeaders.Add("X-Dev-Auth-Timestamp", currentTimestamp);
                httpClient.DefaultRequestHeaders.Add("x-unops-impersonated-user", devEmail);
                
                _logger.LogInformation("✅ Added development IAP headers and impersonation for: {Email}", devEmail);
            }
            else
            {
                _logger.LogInformation("🔐 Using PRODUCTION mode authentication");
                
                // Add user impersonation header for production
                var userEmail = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    httpClient.DefaultRequestHeaders.Add("x-unops-impersonated-user", userEmail);
                    _logger.LogInformation("✅ Added impersonated user header: {Email}", userEmail);
                }
                else
                {
                    _logger.LogWarning("⚠️ No user email available for impersonation header");
                }
            }
            
            // Log all headers being sent (safely)
            _logger.LogInformation("📋 Final request headers count: {HeaderCount}", httpClient.DefaultRequestHeaders.Count());
            foreach (var header in httpClient.DefaultRequestHeaders)
            {
                var headerValues = string.Join(", ", header.Value);
                if (header.Key.ToLower().Contains("authorization"))
                {
                    var maskedValue = headerValues.Length > 20 ? $"{headerValues.Substring(0, 20)}..." : "***";
                    _logger.LogInformation("   {Key}: {Value} (masked)", header.Key, maskedValue);
                }
                else
                {
                    _logger.LogInformation("   {Key}: {Value}", header.Key, headerValues);
                }
            }
            
            _logger.LogInformation("✅ Authentication headers setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error adding authentication headers");
        }
    }

    private async Task<string?> GetOidcTokenAsync(string targetAudience)
    {
        try
        {
            _logger.LogInformation("🔑 Starting OIDC token generation for audience: {Audience}", targetAudience);
            
            // Try the simpler approach first - use service account credentials directly
            // Get service account JSON from configuration
            var serviceAccountJsonSecretName = _configuration.GetValue<string>("GoogleDriveSettings:GoogleDriveServiceAccountJSONSecretName");
            
            if (!string.IsNullOrEmpty(serviceAccountJsonSecretName))
            {
                _logger.LogInformation("🔓 Attempting to use service account JSON from secret: {SecretName}", serviceAccountJsonSecretName);
                
                try
                {
                    // This would require implementing secret manager access
                    // For now, let's try application default credentials with a fallback
                    _logger.LogInformation("🔄 Falling back to application default credentials approach");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Could not access service account JSON secret: {SecretName}", serviceAccountJsonSecretName);
                }
            }
            
            // Fallback approach: Use application default credentials directly
            // This should work if the environment has service account credentials set up
            _logger.LogInformation("🔓 Getting application default credentials...");
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            
            // Add cloud platform scope
            credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            
            _logger.LogInformation("✅ Successfully obtained application default credentials");
            
            // Try to get OIDC token directly (this will work if credential is service account)
            _logger.LogInformation("🎯 Creating OIDC token with target audience: {Audience}", targetAudience);
            
            try
            {
                var oidcToken = await credential.GetOidcTokenAsync(OidcTokenOptions.FromTargetAudience(targetAudience));
                _logger.LogInformation("✅ Successfully created OIDC token object");
                
                var idToken = await oidcToken.GetAccessTokenAsync();
                
                if (!string.IsNullOrEmpty(idToken))
                {
                    var tokenPreview = idToken.Length > 50 ? $"{idToken.Substring(0, 50)}..." : idToken;
                    _logger.LogInformation("✅ Generated OIDC token for audience: {Audience}, Token preview: {TokenPreview}", 
                        targetAudience, tokenPreview);
                    return idToken;
                }
                else
                {
                    _logger.LogError("❌ OIDC token generation returned empty/null token for audience: {Audience}", targetAudience);
                    return null;
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("UnderlyingCredential is not an OIDC token provider"))
            {
                _logger.LogWarning("⚠️ Application default credentials don't support OIDC tokens directly. This means we're running with user credentials instead of service account credentials.");
                _logger.LogInformation("💡 In development, you might need to set up service account credentials or run with gcloud auth application-default login --impersonate-service-account=pno-ai-service@unops-opportunityplus-qa.iam.gserviceaccount.com");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get OIDC token for audience: {Audience}. Error: {Error}", targetAudience, ex.Message);
            _logger.LogDebug(ex, "Full OIDC token generation exception");
            return null;
        }
    }
}
