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

namespace UNOPS.PAO.Presentation.Controllers;

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class EngagementController : BaseController
{
    private readonly UNOPSAppDbContext _context;

    public EngagementController(
        UNOPSAppDbContext context,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<EngagementController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _context = context;
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

            var query = _context.Engagements
                .Where(e => e.PartnerId == partnerId && !e.IsDeleted)
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
}
