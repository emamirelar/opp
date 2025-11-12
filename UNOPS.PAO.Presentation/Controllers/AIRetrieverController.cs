/**
 * @fileoverview AI Retriever Controller for all external AI API endpoints
 * @author UNOPS Opportunity+ System Development Team
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers;

/// <summary>
/// Controller for all AI retriever API endpoints
/// Handles vector store, document conversion, and other AI retriever operations
/// </summary>
[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class AIRetrieverController : ControllerBase
{
    private readonly IAiRetrieverManager _aiRetrieverManager;
    private readonly UserResolverService<int> _userResolverService;
    private readonly ILogger<AIRetrieverController> _logger;

    public AIRetrieverController(
        IAiRetrieverManager aiRetrieverManager,
        UserResolverService<int> userResolverService,
        ILogger<AIRetrieverController> logger)
    {
        _aiRetrieverManager = aiRetrieverManager;
        _userResolverService = userResolverService;
        _logger = logger;
    }

    #region Vector Store Endpoints

    /// <summary>
    /// Search corporate vector store
    /// </summary>
    /// <param name="request">Search request with query and filters</param>
    /// <returns>Search results from vector store</returns>
    [HttpPost(APIDictionary.AIRetriever.VectorStoreSearch)]
    public async Task<IActionResult> SearchVectorStore([FromBody] VectorStoreSearchRequest request)
    {
        try
        {
            _logger.LogInformation("🔍 ============ VECTOR STORE SEARCH REQUEST START ============");
            _logger.LogInformation("📋 Request Payload:");
            _logger.LogInformation("   Query: {Query}", request.Query);
            _logger.LogInformation("   MaxResults: {MaxResults}", request.MaxResults);
            _logger.LogInformation("   EntityTypeId: {EntityTypeId}", request.EntityTypeId ?? "(empty)");
            _logger.LogInformation("   EntityId: {EntityId}", request.EntityId ?? "(empty)");
            _logger.LogInformation("   ApplicationId: {ApplicationId}", request.ApplicationId ?? "(empty)");
            _logger.LogInformation("   DatasourceId: {DatasourceId}", request.DatasourceId ?? "(empty)");
            _logger.LogInformation("   PrimaryRelatedToEntityTypeId: {PrimaryRelatedToEntityTypeId}", request.PrimaryRelatedToEntityTypeId ?? "(empty)");
            _logger.LogInformation("   PrimaryRelatedToEntityId: {PrimaryRelatedToEntityId}", request.PrimaryRelatedToEntityId ?? "(empty)");
            _logger.LogInformation("   Debug: {Debug}", request.Debug);
            _logger.LogInformation("   Filters Count: {FiltersCount}", request.Filters?.Count ?? 0);

            var userEmail = _userResolverService.GetUserEmail();
            _logger.LogInformation("👤 Current User Email: {UserEmail}", userEmail ?? "⚠️ NO USER EMAIL FOUND");
            
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("⚠️ WARNING: No user email found! This may cause authorization issues.");
            }

            var result = await _aiRetrieverManager.SearchVectorStoreAsync(request, userEmail);

            _logger.LogInformation("✅ Vector store search completed successfully");
            _logger.LogInformation("📊 Response Summary:");
            _logger.LogInformation("   Status: {Status}", result.Status ?? "(empty)");
            _logger.LogInformation("   Documents Count: {Count}", result.Documents?.Count ?? 0);
            _logger.LogInformation("   Query Echo: {Query}", result.Query ?? "(empty)");
            _logger.LogInformation("   Error: {Error}", result.Error ?? "(none)");
            
            if (result.Documents != null && result.Documents.Count > 0)
            {
                _logger.LogInformation("📄 First Document Preview:");
                var firstDoc = result.Documents[0];
                _logger.LogInformation("   Distance: {Distance}", firstDoc.Distance);
                _logger.LogInformation("   Score: {Score}", firstDoc.Score);
                _logger.LogInformation("   Title: {Title}", firstDoc.Title);
                _logger.LogInformation("   Entity Type: {EntityType}", firstDoc.EntityTypeId);
                _logger.LogInformation("   Entity ID: {EntityId}", firstDoc.EntityId);
                _logger.LogInformation("   Content Length: {Length}", firstDoc.Content?.Length ?? 0);
                _logger.LogInformation("   Document ID: {DocumentId}", firstDoc.DocumentId);
            }
            else
            {
                _logger.LogWarning("⚠️ ZERO RESULTS RETURNED - This may indicate:");
                _logger.LogWarning("   1. No matching data in vector store");
                _logger.LogWarning("   2. User does not have access to data");
                _logger.LogWarning("   3. Authentication/authorization issue");
                _logger.LogWarning("   4. Incorrect entity filters");
            }
            
            _logger.LogInformation("🔍 ============ VECTOR STORE SEARCH REQUEST END ============");
            return Ok(result);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error during vector store search: {Message}", httpEx.Message);
            return StatusCode(502, new
            {
                error = "External service error",
                message = httpEx.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vector store: {Message}", ex.Message);
            return StatusCode(500, new
            {
                error = "Internal server error",
                message = "An error occurred while searching the vector store"
            });
        }
    }

    #endregion

    #region Document Conversion Endpoints

    /// <summary>
    /// Convert URL to document
    /// </summary>
    /// <param name="request">URL conversion request</param>
    /// <returns>Converted document</returns>
    [HttpPost(APIDictionary.AIRetriever.ConvertUrl)]
    public async Task<IActionResult> ConvertUrl([FromBody] ConvertUrlRequest request)
    {
        try
        {
            _logger.LogInformation("URL conversion requested for: {Url}", request.Url);

            var userEmail = _userResolverService.GetUserEmail();
            var result = await _aiRetrieverManager.ConvertUrlAsync(request.Url, userEmail);

            _logger.LogInformation("URL conversion completed");
            return Ok(result);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error during URL conversion: {Message}", httpEx.Message);
            return StatusCode(502, new
            {
                error = "External service error",
                message = httpEx.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting URL: {Message}", ex.Message);
            return StatusCode(500, new
            {
                error = "Internal server error",
                message = "An error occurred while converting the URL"
            });
        }
    }

    /// <summary>
    /// Convert markdown to Google Doc
    /// </summary>
    /// <param name="request">Markdown conversion request</param>
    /// <returns>Google Doc response</returns>
    [HttpPost(APIDictionary.AIRetriever.ConvertMarkdownToGoogleDoc)]
    public async Task<IActionResult> ConvertMarkdownToGoogleDoc([FromBody] ConvertMarkdownRequest request)
    {
        try
        {
            _logger.LogInformation("Markdown to Google Doc conversion requested");

            var userEmail = _userResolverService.GetUserEmail();
            var result = await _aiRetrieverManager.ConvertMarkdownToGoogleDocAsync(request.Markdown, userEmail);

            _logger.LogInformation("Markdown conversion completed");
            return Ok(result);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error during markdown conversion: {Message}", httpEx.Message);
            return StatusCode(502, new
            {
                error = "External service error",
                message = httpEx.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting markdown: {Message}", ex.Message);
            return StatusCode(500, new
            {
                error = "Internal server error",
                message = "An error occurred while converting the markdown"
            });
        }
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Health check endpoint for AI retriever service
    /// </summary>
    [HttpGet(APIDictionary.AIRetriever.Health)]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "ai-retriever",
            timestamp = DateTime.UtcNow
        });
    }

    #endregion

    // Add more endpoint methods here as needed (97+ more endpoints)
    // Each method is a simple one-liner calling the appropriate AIRetrieverManager method
}

/// <summary>
/// Request model for URL conversion
/// </summary>
public class ConvertUrlRequest
{
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Request model for markdown conversion
/// </summary>
public class ConvertMarkdownRequest
{
    public string Markdown { get; set; } = string.Empty;
}

