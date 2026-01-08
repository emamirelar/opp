using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Dashboard;

/// <summary>
/// Dedicated controller for dashboard data with user-specific filtering
/// This keeps dashboard logic separate from core entity APIs
/// </summary>
[Authorize(AuthenticationSchemes = "IAP")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        IDashboardService dashboardService,
        UserResolverService<int> userResolverService,
        ILogger<DashboardController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Gets partners for the current user's dashboard (created by or modified by current user, excluding drafts)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Partners related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyPartners)]
    public async Task<ActionResult> GetMyPartners([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyPartnersAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets contacts for the current user's dashboard (created by or modified by current user, excluding drafts)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Contacts related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyContacts)]
    public async Task<ActionResult> GetMyContacts([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyContactsAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets draft partners for the current user's dashboard (created by or modified by current user, draft status only)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Draft partners related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyDraftPartners)]
    public async Task<ActionResult> GetMyDraftPartners([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyDraftPartnersAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets draft contacts for the current user's dashboard (created by or modified by current user, draft status only)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Draft contacts related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyDraftContacts)]
    public async Task<ActionResult> GetMyDraftContacts([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyDraftContactsAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets interactions for the current user's dashboard (created by or modified by current user, excluding drafts)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Interactions related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyInteractions)]
    public async Task<ActionResult> GetMyInteractions([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyInteractionsAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets draft interactions for the current user's dashboard (created by or modified by current user, draft status only)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Draft interactions related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyDraftInteractions)]
    public async Task<ActionResult> GetMyDraftInteractions([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyDraftInteractionsAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets opportunities for the current user's dashboard (created by or modified by current user, excluding drafts)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Opportunities related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyOpportunities)]
    public async Task<ActionResult> GetMyOpportunities([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyOpportunitiesAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets draft opportunities for the current user's dashboard (created by or modified by current user, draft status only)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 1000)</param>
    /// <returns>Draft opportunities related to the current user</returns>
    [HttpGet(APIDictionary.DashboardMyDraftOpportunities)]
    public async Task<ActionResult> GetMyDraftOpportunities([FromQuery] int pageSize = 1000)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetMyDraftOpportunitiesAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets recent updates from all entity types in the org unit (Partners, Contacts, Interactions)
    /// </summary>
    /// <param name="pageSize">Number of records to return (default: 10)</param>
    /// <returns>Recent updates across all entity types</returns>
    [HttpGet(APIDictionary.DashboardOrgUnitRecentUpdates)]
    public async Task<ActionResult> GetOrgUnitRecentUpdates([FromQuery] int pageSize = 10)
    {
        return await HandleOperationAsync(async () =>
        {
            var result = await _dashboardService.GetOrgUnitRecentUpdatesAsync(User, pageSize);
            return result;
        });
    }

    /// <summary>
    /// Gets all dashboard content in a single optimized request.
    /// Returns: MyPartners, MyContacts, MyInteractions, MyOpportunities, DraftPartners, DraftContacts,
    /// DraftInteractions, DraftOpportunities, and OrgUnitRecentUpdates in a single response.
    /// 
    /// Uses lightweight projection models and optimized queries for high performance.
    /// </summary>
    /// <param name="pageSize">Number of records per entity type (default: 50)</param>
    /// <param name="recentUpdatesPageSize">Number of recent updates to return (default: 10)</param>
    /// <returns>Dashboard content data</returns>
    [HttpGet(APIDictionary.DashboardContent)]
    public async Task<ActionResult> GetDashboardContent([FromQuery] int pageSize = 50, [FromQuery] int recentUpdatesPageSize = 10)
    {
        return await HandleOperationAsync(async () =>
        {
            // Cap page sizes at reasonable limits for dashboard display
            var effectivePageSize = Math.Min(pageSize, 100); // Dashboard only shows ~3-5 items per section
            var effectiveRecentUpdatesPageSize = Math.Min(recentUpdatesPageSize, 20);
            
            var result = await _dashboardService.GetAllDashboardDataAsync(
                User, 
                effectivePageSize, 
                effectiveRecentUpdatesPageSize);
            return result;
        });
    }
}
