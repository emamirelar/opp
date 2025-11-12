using System.Security.Claims;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces
{
    /// <summary>
    /// Interface for Risk management operations
    /// </summary>
    public interface IRiskManager
    {
        /// <summary>
        /// Gets all risks for a specific entity
        /// </summary>
        /// <param name="entityType">Type of entity (e.g., "Opportunity", "Project")</param>
        /// <param name="entityId">ID of the entity</param>
        /// <param name="user">Current user context</param>
        /// <returns>Response containing list of risks</returns>
        Task<DSTRisksResponse> GetRisksByEntityAsync(string entityType, int entityId, ClaimsPrincipal? user = null);

        /// <summary>
        /// Creates a new risk
        /// </summary>
        /// <param name="request">Risk creation request</param>
        /// <param name="user">Current user context</param>
        /// <returns>Created risk model</returns>
        Task<RiskModel> CreateRiskAsync(RiskCreateRequest request, ClaimsPrincipal? user = null);

        /// <summary>
        /// Updates an existing risk
        /// </summary>
        /// <param name="id">Risk ID</param>
        /// <param name="request">Risk update request</param>
        /// <param name="user">Current user context</param>
        /// <returns>Updated risk model</returns>
        Task<RiskModel> UpdateRiskAsync(int id, RiskCreateRequest request, ClaimsPrincipal? user = null);

        /// <summary>
        /// Deletes a risk
        /// </summary>
        /// <param name="id">Risk ID</param>
        /// <param name="user">Current user context</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteRiskAsync(int id, ClaimsPrincipal? user = null);
    }
}

