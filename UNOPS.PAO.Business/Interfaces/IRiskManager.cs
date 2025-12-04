using System.Security.Claims;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces
{
    /// <summary>
    /// Interface for Risk management operations (aligned with oUP)
    /// </summary>
    public interface IRiskManager
    {
        #region Risk CRUD Operations

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

        #endregion

        #region Risk Lookups

        /// <summary>
        /// Gets all risk lookup data (types, probabilities, proximities, impact levels, response types)
        /// </summary>
        /// <returns>All lookup data for risk forms</returns>
        Task<RiskLookupsResponse> GetRiskLookupsAsync();

        /// <summary>
        /// Gets risk categories in hierarchical format (3 levels)
        /// </summary>
        /// <returns>Category hierarchy with selectable leaf nodes</returns>
        Task<RiskCategoryHierarchyResponse> GetRiskCategoriesAsync();

        /// <summary>
        /// Gets all predefined high risks (EAC checklist items)
        /// </summary>
        /// <returns>List of predefined high risk items</returns>
        Task<List<PreDefinedHighRiskModel>> GetPreDefinedHighRisksAsync();

        #endregion

        #region High Risk Analysis

        /// <summary>
        /// Analyzes an opportunity and returns high risk recommendations
        /// </summary>
        /// <param name="opportunityId">ID of the opportunity to analyze</param>
        /// <param name="user">Current user context</param>
        /// <returns>High risk analysis with auto-detected recommendations</returns>
        Task<HighRiskAnalysisResponse> GetHighRiskAnalysisAsync(int opportunityId, ClaimsPrincipal? user = null);

        #endregion
    }
}

