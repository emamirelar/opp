using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class EngagementController : BaseController
{
    private readonly UNOPSAppDbContext _context;
    private readonly AiContextualService _aiContextualService;

    public EngagementController(
        UNOPSAppDbContext context,
        AiContextualService aiContextualService,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<EngagementController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _context = context;
        _aiContextualService = aiContextualService;
    }

    /// <summary>
    /// Retrieves all engagements for a specific partner with complete details.
    /// </summary>
    /// <param name="partnerId">Partner ID to filter engagements by</param>
    /// <param name="pageIndex">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order results by (optional)</param>
    /// <param name="ascending">Sort direction - true for ascending, false for descending (default: true)</param>
    /// <example_uses>
    /// Show all engagements for partner 123
    /// List engagements for specific partner organization
    /// Get partner engagement history
    /// Display partner's project engagements
    /// Show engagement records for a partner
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see all engagements associated with a specific partner.</when_to_use>
    /// <returns>Paginated list of engagements for the specified partner</returns>
    [HttpGet(APIDictionary.Engagement + "/partner/{partnerId}")]
    public async Task<ActionResult<PaginationResponse<Engagement>>> GetEngagementsByPartner(
        int partnerId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool ascending = true)
    {
        return await HandleOperationAsync(async () =>
        {
            // Validate pagination parameters
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // First get the partner's ErpDimValue since Engagement.PartnerId references Partner.ErpDimValue, not Partner.Id
            var partner = await _context.Partners
                .Where(p => p.Id == partnerId && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (partner == null || !partner.ErpDimValue.HasValue)
            {
                return new PaginationResponse<Engagement>
                {
                    Records = new List<Engagement>(),
                    TotalCount = 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = 0
                };
            }

            var query = _context.Engagements
                .Where(e => e.PartnerId == partner.ErpDimValue.Value && !e.IsDeleted)
                .Include(e => e.Partner);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var engagements = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResponse<Engagement>
            {
                Records = engagements,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        });
    }

    /// <summary>
    /// Retrieves all engagements for a partner using AI-powered smart search by partner name with complete details.
    /// Uses semantic similarity search to find the best matching partner, supporting partial names, abbreviations, and synonyms.
    /// </summary>
    /// <param name="partnerName">Partner name or partial name to search for (supports fuzzy matching, abbreviations, synonyms)</param>
    /// <param name="pageIndex">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order results by (default: 'createdDate')</param>
    /// <param name="ascending">Sort direction - true for ascending, false for descending (default: false for newest first)</param>
    /// <example_uses>
    /// Show all engagements for partner named "UNICEF"
    /// List engagements for "World Bank" organization (works with "WB", "World Bank", "bank")
    /// Get engagement history for partner "Red Cross" (works with "ICRC", "Red Cross", "Cross")
    /// Display engagements for "WHO" partner (works with "World Health", "WHO", "Health Organization")
    /// Show engagement records using partial names like "UN" or "Development"
    /// Find engagements by organization type like "Government" or "NGO"
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see all engagements for a partner using any variation of the partner name, abbreviation, or description. The AI search will find the best match even with incomplete or alternative names.</when_to_use>
    /// <returns>Paginated list of engagements for the best matching partner</returns>
    [HttpGet(APIDictionary.Engagement + "/partner/by-name/{partnerName}")]
        public async Task<ActionResult<PaginationResponse<Engagement>>> GetEngagementsByPartnerName(
            string partnerName,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? orderBy = "CreatedDate",
            [FromQuery] bool ascending = false)
    {
        return await HandleOperationAsync(async () =>
        {
            // Validate pagination parameters
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            if (string.IsNullOrWhiteSpace(partnerName))
            {
                throw new ArgumentException("Partner name is required");
            }

            // Use AI similarity search to find the best matching partner
            var searchResult = await _aiContextualService.RetrieveEntityId(
                entityName: "Partners", 
                vectorEmbedding: null,
                searchText: partnerName,
                similarityThreshold: 0.3f, // Lower threshold for more flexible matching
                embeddingThreshold: 0.7f
            );

            if (searchResult == null || searchResult is DBNull)
            {
                return new PaginationResponse<Engagement>
                {
                    Records = new List<Engagement>(),
                    TotalCount = 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = 0
                };
            }

            // Extract partner ID from the search result (ExecuteScalarAsync returns the entityId directly)
            int partnerId;
            try
            {
                partnerId = Convert.ToInt32(searchResult);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid partner ID returned from search: {searchResult?.ToString()}", ex);
            }

            var partner = await _context.Partners
                .Where(p => p.Id == partnerId)
                .FirstOrDefaultAsync();

            if (partner == null || !partner.ErpDimValue.HasValue)
            {
                return new PaginationResponse<Engagement>
                {
                    Records = new List<Engagement>(),
                    TotalCount = 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = 0
                };
            }

            // Query engagements using the partner's ErpDimValue
            var baseQuery = _context.Engagements
                .Where(e => e.PartnerId == partner.ErpDimValue.Value && !e.IsDeleted)
                .Include(e => e.Partner);

            IQueryable<Engagement> query;
            
            // Apply ordering
            if (!string.IsNullOrEmpty(orderBy))
            {
                try
                {
                    query = ascending 
                        ? baseQuery.OrderBy(e => Microsoft.EntityFrameworkCore.EF.Property<object>(e, orderBy))
                        : baseQuery.OrderByDescending(e => Microsoft.EntityFrameworkCore.EF.Property<object>(e, orderBy));
                }
                catch
                {
                    // If orderBy field is invalid, fall back to default ordering
                    query = baseQuery.OrderByDescending(e => e.CreatedDate);
                }
            }
            else
            {
                // Default ordering by creation date (newest first)
                query = baseQuery.OrderByDescending(e => e.CreatedDate);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var engagements = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResponse<Engagement>
            {
                Records = engagements,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        });
    }
}
