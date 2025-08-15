using System.Security.Claims;
using UNOPS.PAO.Models;

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
    Task<OrgUnitRecentUpdatesResponse> GetOrgUnitRecentUpdatesAsync(ClaimsPrincipal user, int pageSize = 10);
}
