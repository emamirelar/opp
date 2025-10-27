using System.Security.Claims;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

public interface IBaseEngagementManager
{
    Task<IEnumerable<BaseEngagementModel>> GetAllAsync(ClaimsPrincipal user);
    Task<BaseEngagementModel?> GetByIdAsync(ClaimsPrincipal user, int id);
    Task<IEnumerable<BaseEngagementModel>> GetByPartnerIdAsync(ClaimsPrincipal user, int partnerId);
    Task<IEnumerable<BaseEngagementPartnerModel>> GetEngagementPartnersAsync(ClaimsPrincipal user, int engagementId);
}
