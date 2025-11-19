using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using System.Text.Json;
using UNOPS.PAO.Models.AuditLogs;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class OpportunityController : BaseController
{
    private readonly IOpportunityManager _manager;
    private readonly IAuditLogManager _auditLogManager;
    private readonly IGeminiManager _geminiManager;
    private readonly IRiskManager _riskManager;
    private readonly int _currentUserId;

    public OpportunityController(
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<OpportunityController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.OpportunityManager;
        _auditLogManager = manager.AuditLogManager;
        _geminiManager = manager.GeminiManager;
        _riskManager = manager.RiskManager;
        _currentUserId = userResolverService.GetCurrentUserId();
    }

    /// <summary>
    /// Creates a new opportunity
    /// </summary>
    [HttpPost(APIDictionary.Opportunity)]
    [AccessControlled(EntityTypes.Opportunity, "create")]
    public async Task<ActionResult> Create([FromBody] OpportunityRequest req)
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(req.Name))
        {
            validationErrors.Add("Name is required for opportunity creation");
        }
        
        if (string.IsNullOrWhiteSpace(req.Description))
        {
            validationErrors.Add("Description is required for opportunity creation");
        }

        if (validationErrors.Any())
        {
            var errorMessage = $"Missing required fields for opportunity creation: {string.Join(", ", validationErrors)}";
            return BadRequest(new
            {
                error = errorMessage,
                missingFields = validationErrors
            });
        }

        var result = await _manager.CreateOpportunityAsync(req);
        
        // Assign the current user as Opportunity Manager
        try
        {
            await _manager.AssignCreatorAsOpportunityManagerAsync(result.Id, _currentUserId);
            _logger.LogInformation("✅ Assigned user {UserId} as Opportunity Manager for opportunity {OpportunityId}", _currentUserId, result.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to assign creator as Opportunity Manager for opportunity {OpportunityId}", result.Id);
            // Don't fail the request if role assignment fails
        }
        
        // Create audit log for the new opportunity
        await CreateAuditLogAsync(result.Id, "create", result);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific opportunity by ID
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> Get(int id)
    {
        var result = await _manager.GetOpportunityAsync(id);

        if (result == null)
        {
            return NotFound(new { error = $"Opportunity with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all opportunities with pagination support
    /// </summary>
    [HttpGet(APIDictionary.Opportunity)]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetAllOpportunities(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = "name",
        [FromQuery] bool ascending = true,
        [FromQuery] bool filterActive = true)
    {
        try
        {
            _logger.LogInformation("=== GET ALL OPPORTUNITIES ENDPOINT ===");
            _logger.LogInformation("Page: {PageIndex}, Size: {PageSize}, OrderBy: {OrderBy}", pageIndex, pageSize, orderBy);

            // Get all opportunities from manager
            var opportunities = await _manager.GetAllOpportunitiesAsync();

            // Apply ordering
            var orderedOpportunities = orderBy?.ToLower() switch
            {
                "name" => ascending ? opportunities.OrderBy(o => o.Name) : opportunities.OrderByDescending(o => o.Name),
                "createddate" => ascending ? opportunities.OrderBy(o => o.CreatedDate) : opportunities.OrderByDescending(o => o.CreatedDate),
                "lastmodifieddate" => ascending ? opportunities.OrderBy(o => o.LastModifiedDate) : opportunities.OrderByDescending(o => o.LastModifiedDate),
                _ => ascending ? opportunities.OrderBy(o => o.Name) : opportunities.OrderByDescending(o => o.Name)
            };

            // Apply pagination
            var totalCount = orderedOpportunities.Count();
            var paginatedOpportunities = orderedOpportunities
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginationResponse<OpportunityModel>
            {
                Records = paginatedOpportunities,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            _logger.LogInformation("Returned {Count} opportunities out of {TotalCount}", paginatedOpportunities.Count, totalCount);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opportunities");
            return StatusCode(500, new { error = "Internal server error while fetching opportunities", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing opportunity
    /// </summary>
    [HttpPut(APIDictionary.Opportunity + "/{id}")]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateOpportunityRequest req)
    {
        if (id != req.Id)
        {
            return BadRequest(new { error = "ID mismatch between route and request body" });
        }

        var result = await _manager.UpdateOpportunityAsync(req);

        if (result == null)
        {
            return NotFound(new { error = $"Opportunity with ID {id} not found" });
        }

        // Create audit log
        await CreateAuditLogAsync(id, "update", result);

        return Ok(result);
    }

    /// <summary>
    /// Helper method to create audit log entry with complete opportunity data
    /// </summary>
    private async Task CreateAuditLogAsync(int opportunityId, string action, OpportunityModel? opportunityData = null)
    {
        try
        {
            // Get the current opportunity data if not provided
            if (opportunityData == null)
            {
                opportunityData = await _manager.GetOpportunityAsync(opportunityId);
            }

            if (opportunityData != null)
            {
                var jsonData = JsonSerializer.Serialize(opportunityData, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await _auditLogManager.CreateAuditLogAsync(new AuditLogCreateRequest
                {
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    Action = action,
                    UserId = _currentUserId,
                    JsonData = jsonData,
                    Description = $"Opportunity {action} - {opportunityData.Name}"
                });
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request
            _logger.LogError(ex, "Error creating audit log for opportunity {OpportunityId}", opportunityId);
        }
    }

    /// <summary>
    /// Updates the WHAT section of an opportunity (description, org unit, initiative type, deliverables)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhat)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhatSection(int id, [FromBody] WhatSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhatSectionAsync(id, req);
            
            // Create audit log
            await CreateAuditLogAsync(id, "update_what_section", result);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHAT section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHAT section", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHY section of an opportunity (strategic alignment, beneficiaries, outcomes, SDGs)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhy)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhySection(int id, [FromBody] WhySectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhySectionAsync(id, req);
            
            // Create audit log
            await CreateAuditLogAsync(id, "update_why_section", result);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHY section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHY section", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHO section of an opportunity (funding partners, client partners)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWho)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhoSection(int id, [FromBody] WhoSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhoSectionAsync(id, req);
            
            // Create audit log
            await CreateAuditLogAsync(id, "update_who_section", result);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHO section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHO section", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHERE section of an opportunity (implementation countries)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhere)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhereSection(int id, [FromBody] WhereSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhereSectionAsync(id, req);
            
            // Create audit log
            await CreateAuditLogAsync(id, "update_where_section", result);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHERE section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHERE section", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets related items (contacts, partners, interactions) for an opportunity
    /// </summary>
    [HttpGet(APIDictionary.OpportunityRelated)]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetRelatedItems(int id)
    {
        try
        {
            var result = await _manager.GetRelatedItemsAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related items for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while getting related items", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the WHEN section of an opportunity (timeline dates)
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityWhen)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> UpdateWhenSection(int id, [FromBody] WhenSectionRequest req)
    {
        try
        {
            var result = await _manager.UpdateWhenSectionAsync(id, req);
            
            // Create audit log
            await CreateAuditLogAsync(id, "update_when_section", result);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WHEN section for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while updating WHEN section", details = ex.Message });
        }
    }

    /// <summary>
    /// Applies AI-extracted changes to an opportunity across multiple sections
    /// </summary>
    [HttpPatch(APIDictionary.OpportunityApplyAiChanges)]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> ApplyAiChanges(int id, [FromBody] ApplyOpportunityAiChangesRequest req)
    {
        try
        {
            var result = await _manager.ApplyAiChangesAsync(id, req);
            
            // Create audit log
            await CreateAuditLogAsync(id, "apply_ai_changes", result);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying AI changes to opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while applying AI changes", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets similar opportunities using semantic search based on embeddings
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}/similar-opportunities")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetSimilarOpportunities(int id, [FromQuery] int maxResults = 6)
    {
        try
        {
            _logger.LogInformation("Getting similar opportunities for opportunity {OpportunityId} with maxResults={MaxResults}", 
                id, maxResults);

            // Validate maxResults
            if (maxResults < 1 || maxResults > 50)
            {
                return BadRequest(new { error = "maxResults must be between 1 and 50" });
            }

            // Get the opportunity to verify it exists
            var opportunity = await _manager.GetOpportunityAsync(id);
            if (opportunity == null)
            {
                _logger.LogWarning("Opportunity {OpportunityId} not found for similar opportunities search", id);
                return NotFound(new { error = $"Opportunity with ID {id} not found" });
            }

            // Get current user from claims
            var user = User;

            // Call the manager to get similar opportunities
            var response = await _manager.GetSimilarOpportunitiesAsync(id, maxResults, user);

            _logger.LogInformation("Found {Count} similar opportunities for opportunity {OpportunityId}", 
                response.SimilarOpportunities?.Count ?? 0, id);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Opportunity {OpportunityId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar opportunities for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while getting similar opportunities", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets similar projects for an opportunity using AI-powered semantic search
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}/similar-projects")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetSimilarProjects(int id, [FromQuery] int maxResults = 6)
    {
        try
        {
            _logger.LogInformation("Getting similar projects for opportunity {OpportunityId} with maxResults={MaxResults}", 
                id, maxResults);

            // Validate maxResults
            if (maxResults < 1 || maxResults > 50)
            {
                return BadRequest(new { error = "maxResults must be between 1 and 50" });
            }

            // Get the opportunity to verify it exists
            var opportunity = await _manager.GetOpportunityAsync(id);
            if (opportunity == null)
            {
                _logger.LogWarning("Opportunity {OpportunityId} not found for similar projects search", id);
                return NotFound(new { error = $"Opportunity with ID {id} not found" });
            }

            // Get current user from claims
            var user = User;

            // Call the GeminiManager to get similar projects
            var response = await _geminiManager.GetSimilarProjectsAsync(id, maxResults, user);

            _logger.LogInformation("Found {Count} similar projects for opportunity {OpportunityId}", 
                response.SimilarProjects?.Count ?? 0, id);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Opportunity {OpportunityId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar projects for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while getting similar projects", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets relevant people from corporate directory for an opportunity using AI-powered semantic search
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}/relevant-people")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetRelevantPeople(int id, [FromQuery] int maxResults = 6)
    {
        try
        {
            _logger.LogInformation("Getting relevant people for opportunity {OpportunityId} with maxResults={MaxResults}", 
                id, maxResults);

            // Validate maxResults
            if (maxResults < 1 || maxResults > 50)
            {
                return BadRequest(new { error = "maxResults must be between 1 and 50" });
            }

            // Get the opportunity to verify it exists
            var opportunity = await _manager.GetOpportunityAsync(id);
            if (opportunity == null)
            {
                _logger.LogWarning("Opportunity {OpportunityId} not found for relevant people search", id);
                return NotFound(new { error = $"Opportunity with ID {id} not found" });
            }

            // Get current user from claims
            var user = User;

            // Call the GeminiManager to get relevant people
            var response = await _geminiManager.GetRelevantPeopleAsync(id, maxResults, user);

            _logger.LogInformation("Found {Count} relevant people for opportunity {OpportunityId}", 
                response.RelevantPeople?.Count ?? 0, id);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Opportunity {OpportunityId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relevant people for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while getting relevant people", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets AI-powered DST risk recommendations for an opportunity
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}/dst-recommendations")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult<DSTRecommendationsResponse>> GetDSTRecommendations(int id, [FromQuery] int maxResults = 10)
    {
        try
        {
            _logger.LogInformation("🎯 [API] Getting DST recommendations for opportunity {OpportunityId}", id);

            var response = await _geminiManager.GetDSTRecommendationsAsync(id, User, maxResults);

            _logger.LogInformation("✅ [API] Successfully retrieved {Count} DST recommendations for opportunity {OpportunityId}",
                response.Recommendations?.Count ?? 0, id);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Opportunity {OpportunityId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DST recommendations for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while getting DST recommendations", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets existing risks from the risk register for an opportunity
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}/dst-risks")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult<DSTRisksResponse>> GetDSTRisks(int id)
    {
        try
        {
            _logger.LogInformation("📋 [API] Getting DST risks for opportunity {OpportunityId}", id);

            var response = await _riskManager.GetRisksByEntityAsync("Opportunity", id, User);

            _logger.LogInformation("✅ [API] Successfully retrieved {Count} DST risks for opportunity {OpportunityId}",
                response.Risks?.Count ?? 0, id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DST risks for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while getting DST risks", details = ex.Message });
        }
    }

    /// <summary>
    /// Adds a new risk to the risk register for an opportunity
    /// </summary>
    [HttpPost(APIDictionary.Opportunity + "/{id}/dst-risks")]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult<RiskModel>> AddDSTRisk(int id, RiskCreateRequest request)
    {
        try
        {
            _logger.LogInformation("[API] Adding DST risk for opportunity {OpportunityId}", id);

            // Ensure the entity ID matches the route parameter
            request.EntityId = id;

            var risk = await _riskManager.CreateRiskAsync(request, User);

            _logger.LogInformation("✅ [API] Successfully added DST risk {RiskId} for opportunity {OpportunityId}", risk.Id, id);

            return CreatedAtAction(nameof(GetDSTRisks), new { id }, risk);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding DST risk for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Internal server error while adding DST risk", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing risk in the risk register
    /// </summary>
    [HttpPut(APIDictionary.Opportunity + "/{id}/dst-risks/{riskId}")]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult<RiskModel>> UpdateDSTRisk(int id, int riskId, RiskCreateRequest request)
    {
        try
        {
            _logger.LogInformation("📝 [API] Updating DST risk {RiskId} for opportunity {OpportunityId}", riskId, id);

            // Ensure the entity ID matches the route parameter
            request.EntityId = id;

            var risk = await _riskManager.UpdateRiskAsync(riskId, request, User);

            _logger.LogInformation("✅ [API] Successfully updated DST risk {RiskId} for opportunity {OpportunityId}", riskId, id);

            return Ok(risk);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("DST risk {RiskId} not found for opportunity {OpportunityId}", riskId, id);
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid data for DST risk {RiskId}: {Message}", riskId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DST risk {RiskId} for opportunity {OpportunityId}", riskId, id);
            return StatusCode(500, new { error = "Internal server error while updating DST risk", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets AI-generated insights and suggestions for an opportunity
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}/insights")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult<OpportunityInsightsResponse>> GetInsights(int id)
    {
        try
        {
            _logger.LogInformation("💡 [API] Getting AI insights for opportunity {OpportunityId}", id);

            // Verify opportunity exists
            var opportunity = await _manager.GetOpportunityAsync(id);
            if (opportunity == null)
            {
                _logger.LogWarning("Opportunity {OpportunityId} not found", id);
                return NotFound(new { error = $"Opportunity with ID {id} not found" });
            }

            var response = await _geminiManager.GenerateOpportunityInsightsAsync(id, User);

            _logger.LogInformation("✅ [API] Successfully generated {InsightCount} insights and {SuggestionCount} suggestions for opportunity {OpportunityId}", 
                response.Insights?.Count ?? 0, 
                response.Suggestions?.Count ?? 0, 
                id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Failed to generate insights", details = ex.Message });
        }
    }

    /// <summary>
    /// Generates AI-powered opportunity proposal from multiple sources
    /// Analyzes interactions, documents (new uploads or existing), or combination to create comprehensive proposal
    /// Can be called from partner tabs, interaction lists, opportunity lists, etc.
    /// </summary>
    /// <param name="request">Proposal request with source data and basic info</param>
    /// <returns>AI-proposed opportunity data for user review</returns>
    [HttpPost(APIDictionary.Opportunity + "/generate-proposal")]
    [AccessControlled(EntityTypes.Opportunity, "create")]
    public async Task<ActionResult<UNOPS.PAO.Models.Opportunities.OpportunityProposalResponse>> GenerateOpportunityProposal(
        [FromBody] UNOPS.PAO.Models.Opportunities.OpportunityProposalRequest request)
    {
        try
        {
            _logger.LogInformation("🔍 [API] Generating opportunity proposal: Name='{Name}', PartnerId={PartnerId}, Interactions={InteractionCount}, NewDocs={NewDocCount}, ExistingDocs={ExistingDocCount}", 
                request.OpportunityName, 
                request.PartnerId ?? 0,
                request.InteractionIds?.Count ?? 0,
                request.NewDocumentStoragePaths?.Count ?? 0,
                request.ExistingDocumentIds?.Count ?? 0);

            // Validate request - at least one source is required
            if ((request.InteractionIds == null || !request.InteractionIds.Any()) &&
                (request.NewDocumentStoragePaths == null || !request.NewDocumentStoragePaths.Any()) &&
                (request.ExistingDocumentIds == null || !request.ExistingDocumentIds.Any()))
            {
                return BadRequest(new { error = "At least one source is required: interactions, new documents, or existing documents" });
            }

            if (string.IsNullOrWhiteSpace(request.OpportunityName))
            {
                return BadRequest(new { error = "Opportunity name is required" });
            }

            if (string.IsNullOrWhiteSpace(request.OpportunityDescription))
            {
                return BadRequest(new { error = "Opportunity description is required" });
            }

            // Partner validation: if partnerId provided, require role selection
            if (request.PartnerId.HasValue && request.PartnerId > 0)
            {
                if (!request.IsFundingPartner && !request.IsClientPartner)
                {
                    return BadRequest(new { error = "Partner must be marked as funding partner, client partner, or both" });
                }
            }

            // Validate that NewDocumentStoragePaths and NewDocumentMimeTypes have matching counts
            if (request.NewDocumentStoragePaths != null && request.NewDocumentStoragePaths.Any())
            {
                if (request.NewDocumentMimeTypes == null || 
                    request.NewDocumentStoragePaths.Count != request.NewDocumentMimeTypes.Count)
                {
                    return BadRequest(new { error = "NewDocumentStoragePaths and NewDocumentMimeTypes must have the same number of elements" });
                }
                
                // Validate all paths are GCS URIs
                foreach (var path in request.NewDocumentStoragePaths)
                {
                    if (string.IsNullOrEmpty(path) || !path.StartsWith("gs://"))
                    {
                        return BadRequest(new { error = "All NewDocumentStoragePaths must be valid GCS URIs (gs://...)" });
                    }
                }
            }

            // Call Gemini Manager to generate proposal
            var proposal = await _geminiManager.GenerateOpportunityProposalAsync(request, User);

            _logger.LogInformation("✅ [API] Successfully generated opportunity proposal");

            return Ok(proposal);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Source data not found for proposal generation");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating opportunity proposal");
            return StatusCode(500, new { error = "Internal server error while generating proposal", details = ex.Message });
        }
    }

    /// <summary>
    /// Creates an opportunity from AI-generated proposal with user-accepted fields
    /// Takes the reviewed and accepted proposal data to create the actual opportunity record
    /// </summary>
    /// <param name="request">Create request with accepted fields and resolved IDs</param>
    /// <returns>Created opportunity model</returns>
    [HttpPost(APIDictionary.Opportunity + "/create-from-proposal")]
    [AccessControlled(EntityTypes.Opportunity, "create")]
    public async Task<ActionResult<OpportunityModel>> CreateOpportunityFromProposal(
        [FromBody] UNOPS.PAO.Models.Opportunities.CreateOpportunityFromInteractionsRequest request)
    {
        try
        {
            _logger.LogInformation("🎯 [API] Creating opportunity '{Name}' from {Count} interactions for partner {PartnerId}", 
                request.Name, request.SourceInteractionIds.Count, request.PartnerId);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { error = "Opportunity name is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "Opportunity description is required" });
            }

            if (!request.IsFundingPartner && !request.IsClientPartner)
            {
                return BadRequest(new { error = "Partner must be marked as funding partner, client partner, or both" });
            }

            // Build opportunity request from accepted proposal
            var opportunityRequest = new OpportunityRequest
            {
                Name = request.Name,
                Description = request.Description,
                PartnerReference = request.PartnerReference,
                ResponsibleOrgUnitId = request.ResponsibleOrgUnitId,
                ProposedInitiativeTypeId = request.ProposedInitiativeTypeId,
                InitiativeBudgetUSD = request.InitiativeBudgetUSD,
                PartnershipAgreementReference = request.PartnershipAgreementReference,
                TargetSigningDate = request.TargetSigningDate,
                TargetDeliveryDate = request.TargetDeliveryDate,
                SDGs = request.SdGs?.Select(sdgId => new OpportunitySDGRequest { SDGId = sdgId }).ToList() ?? new List<OpportunitySDGRequest>(),
                Countries = request.Countries?.Select(countryId => new OpportunityCountryRequest { CountryId = countryId }).ToList() ?? new List<OpportunityCountryRequest>(),
                Deliverables = request.Deliverables ?? new List<OpportunityDeliverableRequest>(),
                Stakeholders = request.Stakeholders ?? new List<OpportunityStakeholderRequest>(),
                FundingPartners = new List<OpportunityFundingPartnerRequest>(),
                ClientPartners = new List<OpportunityClientPartnerRequest>()
            };

            // Add the primary partner as funding/client based on user selection
            if (request.IsFundingPartner)
            {
                opportunityRequest.FundingPartners.Add(new OpportunityFundingPartnerRequest
                {
                    PartnerId = request.PartnerId
                });
            }

            if (request.IsClientPartner)
            {
                opportunityRequest.ClientPartners.Add(new OpportunityClientPartnerRequest
                {
                    PartnerId = request.PartnerId
                });
            }

            // Add any additional funding/client partners from the proposal
            if (request.FundingPartners != null)
            {
                foreach (var fundingPartnerId in request.FundingPartners)
                {
                    if (fundingPartnerId != request.PartnerId) // Avoid duplicates
                    {
                        opportunityRequest.FundingPartners.Add(new OpportunityFundingPartnerRequest
                        {
                            PartnerId = fundingPartnerId
                        });
                    }
                }
            }

            if (request.ClientPartners != null)
            {
                foreach (var clientPartnerId in request.ClientPartners)
                {
                    if (clientPartnerId != request.PartnerId) // Avoid duplicates
                    {
                        opportunityRequest.ClientPartners.Add(new OpportunityClientPartnerRequest
                        {
                            PartnerId = clientPartnerId
                        });
                    }
                }
            }

            // Create the opportunity
            var result = await _manager.CreateOpportunityAsync(opportunityRequest);

            // Assign the current user as Opportunity Manager
            try
            {
                await _manager.AssignCreatorAsOpportunityManagerAsync(result.Id, _currentUserId);
                _logger.LogInformation("✅ Assigned user {UserId} as Opportunity Manager for opportunity {OpportunityId}", 
                    _currentUserId, result.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to assign creator as Opportunity Manager for opportunity {OpportunityId}", result.Id);
            }

            // Create audit log noting this was AI-assisted
            await CreateAuditLogAsync(result.Id, "create", result);

            _logger.LogInformation("✅ [API] Successfully created opportunity {OpportunityId} from interactions", result.Id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opportunity from interactions");
            return StatusCode(500, new { error = "Internal server error while creating opportunity", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an opportunity
    /// </summary>
    [HttpDelete(APIDictionary.Opportunity + "/{id}")]
    [AccessControlled(EntityTypes.Opportunity, "delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _manager.DeleteOpportunityAsync(id);

        if (!result)
        {
            return NotFound(new { error = $"Opportunity with ID {id} not found" });
        }

        return Ok(new { message = "Opportunity deleted successfully", id });
    }
}

