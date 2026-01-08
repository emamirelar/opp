using System.Security.Claims;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Dashboard;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

/// <summary>
/// Dedicated service interface for dashboard data retrieval with user-specific filtering
/// This service keeps dashboard logic separate from core entity APIs
/// </summary>
public interface IDashboardService
{
    Task<PaginationResponse<PartnerModel>> GetMyPartnersAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<PaginationResponse<ContactModel>> GetMyContactsAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<PaginationResponse<PartnerModel>> GetMyDraftPartnersAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<PaginationResponse<ContactModel>> GetMyDraftContactsAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<PaginationResponse<InteractionModel>> GetMyInteractionsAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<PaginationResponse<InteractionModel>> GetMyDraftInteractionsAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<PaginationResponse<OpportunityModel>> GetMyOpportunitiesAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<PaginationResponse<OpportunityModel>> GetMyDraftOpportunitiesAsync(ClaimsPrincipal user, int pageSize = 1000);
    Task<OrgUnitRecentUpdatesResponse> GetOrgUnitRecentUpdatesAsync(ClaimsPrincipal user, int pageSize = 10);
    
    /// <summary>
    /// Gets all dashboard data in a single request to avoid DbContext threading issues
    /// from concurrent API calls. This combines: MyPartners, MyContacts, MyInteractions,
    /// MyOpportunities, DraftPartners, DraftContacts, DraftInteractions, DraftOpportunities,
    /// and OrgUnitRecentUpdates.
    /// </summary>
    /// <param name="user">The current user's claims principal</param>
    /// <param name="pageSize">Number of records per entity type (default: 1000)</param>
    /// <param name="recentUpdatesPageSize">Number of recent updates to return (default: 10)</param>
    /// <returns>Combined dashboard data response</returns>
    Task<DashboardCombinedResponse> GetAllDashboardDataAsync(ClaimsPrincipal user, int pageSize = 1000, int recentUpdatesPageSize = 10);
}
