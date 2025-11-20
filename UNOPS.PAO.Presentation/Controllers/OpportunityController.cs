using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Documents;
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
    private readonly AppDbContext _context;
    private readonly UNOPSDocumentManager _documentManager;

    public OpportunityController(
        IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<OpportunityController> logger,
        IAuthorizationService authorizationService,
        AppDbContext context,
        UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext unopsContext,
        AutoMapper.IMapper mapper,
        IGoogleDriveDocumentManager driveManager,
        IConfiguration configuration,
        UserManager<PAOIdentityUser> userManager,
        IServiceProvider serviceProvider)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.OpportunityManager;
        _auditLogManager = manager.AuditLogManager;
        _geminiManager = manager.GeminiManager;
        _riskManager = manager.RiskManager;
        _currentUserId = userResolverService.GetCurrentUserId();
        _context = context;
        _documentManager = new UNOPSDocumentManager(driveManager, configuration, mapper, unopsContext, userManager, serviceProvider);
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
    /// Retrieves partner-document associations for a specific document
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/retrieve-partner-document-association/{documentId}")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> RetrievePartnerDocumentAssociation(int documentId)
    {
        try
        {
            // Find all funding partners associated with this document
            var fundingPartners = await _context.OpportunityFundingPartners
                .Where(fp => fp.DocumentId == documentId)
                .Select(fp => new
                {
                    partnerId = fp.PartnerId,
                    partnerType = "funding"
                })
                .ToListAsync();
            
            // Find all client partners associated with this document
            var clientPartners = await _context.OpportunityClientPartners
                .Where(cp => cp.DocumentId == documentId)
                .Select(cp => new
                {
                    partnerId = cp.PartnerId,
                    partnerType = "client"
                })
                .ToListAsync();
            
            // Combine both lists
            var allPartners = fundingPartners.Concat(clientPartners).ToList();
            
            return Ok(new
            {
                documentId = documentId,
                partners = allPartners
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving partner-document associations for document {DocumentId}", documentId);
            return StatusCode(500, new { error = "Internal server error while retrieving partner-document associations", details = ex.Message });
        }
    }

    /// <summary>
    /// Tags a document as Partner Results Framework for specific funding/client partners
    /// Updates OpportunityFundingPartner and OpportunityClientPartner records with the document ID
    /// </summary>
    [HttpPost(APIDictionary.Opportunity + "/{opportunityId}/tag-related-partner-to-doc")]
    [AccessControlled(EntityTypes.Opportunity, "update")]
    public async Task<ActionResult> TagDocumentToPartners(int opportunityId, [FromBody] TagDocumentToPartnersRequest request)
    {
        try
        {
            _logger.LogInformation("📎 [API] Tagging document {DocumentId} to partners for opportunity {OpportunityId}", 
                request.DocumentId, opportunityId);

            // Validate request
            if ((request.FundingPartnerIds == null || !request.FundingPartnerIds.Any()) &&
                (request.ClientPartnerIds == null || !request.ClientPartnerIds.Any()))
            {
                return BadRequest(new { error = "At least one funding or client partner must be selected" });
            }

            // Get the document to verify it exists and get its name
            var document = await _context.Documents.FindAsync(request.DocumentId);
            if (document == null)
            {
                return NotFound(new { error = $"Document with ID {request.DocumentId} not found" });
            }

            // Update funding partners with document ID
            if (request.FundingPartnerIds != null && request.FundingPartnerIds.Any())
            {
                var fundingPartnersToUpdate = await _context.OpportunityFundingPartners
                    .Where(fp => fp.OpportunityId == opportunityId && request.FundingPartnerIds.Contains(fp.PartnerId))
                    .ToListAsync();

                foreach (var fundingPartner in fundingPartnersToUpdate)
                {
                    fundingPartner.DocumentId = request.DocumentId;
                    _logger.LogInformation("✅ [API] Tagged document {DocumentId} to funding partner {PartnerId}", 
                        request.DocumentId, fundingPartner.PartnerId);
                }
            }

            // Update client partners with document ID
            if (request.ClientPartnerIds != null && request.ClientPartnerIds.Any())
            {
                var clientPartnersToUpdate = await _context.OpportunityClientPartners
                    .Where(cp => cp.OpportunityId == opportunityId && request.ClientPartnerIds.Contains(cp.PartnerId))
                    .ToListAsync();

                foreach (var clientPartner in clientPartnersToUpdate)
                {
                    clientPartner.DocumentId = request.DocumentId;
                    _logger.LogInformation("✅ [API] Tagged document {DocumentId} to client partner {PartnerId}", 
                        request.DocumentId, clientPartner.PartnerId);
                }
            }

            // Save all changes
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ [API] Successfully tagged document {DocumentId} to partners for opportunity {OpportunityId}", 
                request.DocumentId, opportunityId);

            return Ok(new
            {
                message = "Document successfully tagged to partners",
                documentId = request.DocumentId,
                fundingPartnersUpdated = request.FundingPartnerIds?.Count ?? 0,
                clientPartnersUpdated = request.ClientPartnerIds?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tagging document {DocumentId} to partners for opportunity {OpportunityId}", 
                request.DocumentId, opportunityId);
            return StatusCode(500, new { error = "Internal server error while tagging document to partners", details = ex.Message });
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
    /// Gets source interactions that led to opportunity creation from OpportunityInteractions table
    /// </summary>
    [HttpGet(APIDictionary.Opportunity + "/{id}/source-interactions")]
    [AccessControlled(EntityTypes.Opportunity, "read")]
    public async Task<ActionResult> GetSourceInteractions(int id)
    {
        try
        {
            _logger.LogInformation("🔗 [API] Getting source interactions for opportunity {OpportunityId}", id);

            // Verify opportunity exists
            var opportunity = await _manager.GetOpportunityAsync(id);
            if (opportunity == null)
            {
                _logger.LogWarning("Opportunity {OpportunityId} not found", id);
                return NotFound(new { error = $"Opportunity with ID {id} not found" });
            }

            // Get interaction IDs from OpportunityInteractions table
            var interactionIds = await _context.OpportunityInteractions
                .Where(oi => oi.OpportunityId == id)
                .Select(oi => oi.InteractionId)
                .ToListAsync();

            if (!interactionIds.Any())
            {
                return Ok(new List<object>()); // Return empty array if no interactions found
            }

            // Get full interaction details with partner info via InteractionPartners
            var interactions = await _context.Interactions
                .Where(i => interactionIds.Contains(i.Id))
                .Include(i => i.InteractionPartners)
                    .ThenInclude(ip => ip.Partner)
                .Select(i => new
                {
                    id = i.Id,
                    subject = i.Subject,
                    interactionType = i.Type.ToString(),
                    interactionDate = i.Date,
                    partnerName = i.InteractionPartners != null && i.InteractionPartners.Any()
                        ? i.InteractionPartners.First().Partner != null 
                            ? i.InteractionPartners.First().Partner.Name 
                            : "Unknown Partner"
                        : "Unknown Partner",
                    summary = i.Description
                })
                .ToListAsync();

            _logger.LogInformation("✅ [API] Found {Count} source interactions for opportunity {OpportunityId}", interactions.Count, id);

            return Ok(interactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting source interactions for opportunity {OpportunityId}", id);
            return StatusCode(500, new { error = "Failed to get source interactions", details = ex.Message });
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

            // Partner validation: only required if partnerId is provided (creating from partner context)
            if (request.PartnerId.HasValue && request.PartnerId > 0)
            {
                if (!request.IsFundingPartner && !request.IsClientPartner)
                {
                    return BadRequest(new { error = "When creating from a partner context, the partner must be marked as funding partner, client partner, or both" });
                }
                
                _logger.LogInformation("📊 [API] Context partner {PartnerId} will be added as {Role}", 
                    request.PartnerId, 
                    request.IsFundingPartner && request.IsClientPartner ? "both funding and client" :
                    request.IsFundingPartner ? "funding partner" : "client partner");
            }
            else
            {
                _logger.LogInformation("📊 [API] No context partner - will use AI-proposed partners from interactions");
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

            // Add the context partner as funding/client based on user selection (only if partnerId provided)
            // This ensures the context partner is included even if not in the AI-proposed arrays
            if (request.PartnerId.HasValue && request.PartnerId > 0)
            {
                // Check if context partner is already in the AI-proposed arrays
                var contextPartnerInFunding = request.FundingPartners?.Any(fp => fp.PartnerId == request.PartnerId.Value) ?? false;
                var contextPartnerInClient = request.ClientPartners?.Any(cp => cp.PartnerId == request.PartnerId.Value) ?? false;
                
                // Add to funding partners if user selected funding role and not already in array
                if (request.IsFundingPartner && !contextPartnerInFunding)
                {
                    _logger.LogInformation("➕ [API] Adding context partner {PartnerId} to funding partners", request.PartnerId.Value);
                    opportunityRequest.FundingPartners.Add(new OpportunityFundingPartnerRequest
                    {
                        PartnerId = request.PartnerId.Value,
                        Amount = null // User can set later
                    });
                }
                
                // Add to client partners if user selected client role and not already in array
                if (request.IsClientPartner && !contextPartnerInClient)
                {
                    _logger.LogInformation("➕ [API] Adding context partner {PartnerId} to client partners", request.PartnerId.Value);
                    opportunityRequest.ClientPartners.Add(new OpportunityClientPartnerRequest
                    {
                        PartnerId = request.PartnerId.Value
                    });
                }
            }

            // Add all AI-proposed funding partners
            if (request.FundingPartners != null && request.FundingPartners.Any())
            {
                _logger.LogInformation("➕ [API] Adding {Count} AI-proposed funding partners", request.FundingPartners.Count);
                opportunityRequest.FundingPartners.AddRange(request.FundingPartners);
            }

            // Add all AI-proposed client partners  
            if (request.ClientPartners != null && request.ClientPartners.Any())
            {
                _logger.LogInformation("➕ [API] Adding {Count} AI-proposed client partners", request.ClientPartners.Count);
                opportunityRequest.ClientPartners.AddRange(request.ClientPartners);
            }

            // Create the opportunity
            var result = await _manager.CreateOpportunityAsync(opportunityRequest);

            // Persist uploaded documents to database if any (from GCS temporary uploads)
            if (request.Documents != null && request.Documents.Any())
            {
                _logger.LogInformation("📄 [API] Persisting {Count} uploaded documents to database for opportunity {OpportunityId}", 
                    request.Documents.Count, result.Id);
                
                foreach (var doc in request.Documents)
                {
                    try
                    {
                        // Extract file name from GCS path (gs://bucket/folder/file.ext)
                        var fileName = System.IO.Path.GetFileName(doc.GcsPath);
                        
                        // Create DocumentUploadModel for the document manager (without IFormFile since already uploaded to GCS)
                        var documentModel = new DocumentUploadModel
                        {
                            Name = fileName,
                            StoragePath = doc.GcsPath,
                            Type = doc.MimeType,
                            DocumentTypeId = doc.DocumentTypeId,
                            ParentEntityName = "Opportunity",
                            ParentEntityId = result.Id,
                            AITranscribed = true,
                            UploadToGCS = false, // Already uploaded to GCS
                            SkipDatabaseSave = false, // We want to save to database
                            File = null // No file since already in GCS
                        };
                        
                        // Use the document manager to create the document (handles UNOPSDocument creation correctly)
                        var createdDoc = await _documentManager.CreateDocumentAsync(documentModel);
                        
                        _logger.LogInformation("✅ [API] Persisted document {FileName} (ID: {DocumentId}) for opportunity {OpportunityId}", 
                            fileName, createdDoc.Id, result.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to persist document {FileName} for opportunity {OpportunityId}", 
                            doc.GcsPath, result.Id);
                    }
                }
            }
            
            // Save interaction relationships to OpportunityInteractions table
            if (request.SourceInteractionIds != null && request.SourceInteractionIds.Any())
            {
                _logger.LogInformation("🔗 [API] Saving {Count} interaction relationships for opportunity {OpportunityId}", 
                    request.SourceInteractionIds.Count, result.Id);
                
                foreach (var interactionId in request.SourceInteractionIds)
                {
                    try
                    {
                        var opportunityInteraction = new OpportunityInteraction
                        {
                            OpportunityId = result.Id,
                            InteractionId = interactionId
                        };
                        
                        _context.OpportunityInteractions.Add(opportunityInteraction);
                        _logger.LogInformation("✅ [API] Linked interaction {InteractionId} to opportunity {OpportunityId}", 
                            interactionId, result.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to link interaction {InteractionId} to opportunity {OpportunityId}", 
                            interactionId, result.Id);
                    }
                }
                
                await _context.SaveChangesAsync();
            }

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

