using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Search;
using System.Security.Claims;

namespace UNOPS.PAO.Business.Interfaces;

public interface IOpportunityManager
{
    Task<OpportunityModel> CreateOpportunityAsync(OpportunityRequest model);
    Task<OpportunityModel?> GetOpportunityAsync(int id);
    Task<OpportunityModel?> GetOpportunityAsync(ClaimsPrincipal user, int id);
    Task<IEnumerable<OpportunityModel>> GetAllOpportunitiesAsync();
    Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model);
    Task<OpportunityModel> UpdateOverviewSectionAsync(int id, OverviewSectionRequest request);
    Task<OpportunityModel> UpdateWhatSectionAsync(int id, WhatSectionRequest request);
    Task<OpportunityModel> UpdateWhySectionAsync(int id, WhySectionRequest request);
    Task<OpportunityModel> UpdateWhoSectionAsync(int id, WhoSectionRequest request);
    Task<OpportunityModel> UpdateTeamSectionAsync(int id, TeamSectionRequest request);
    Task<OpportunityModel> UpdateWhereSectionAsync(int id, WhereSectionRequest request);
    Task<OpportunityModel> UpdateWhenSectionAsync(int id, WhenSectionRequest request);
    Task<OpportunityModel> ApplyAiChangesAsync(int id, ApplyOpportunityAiChangesRequest request);
    Task<OpportunityModel> CreateOpportunityFromProposalAsync(CreateOpportunityFromInteractionsRequest request, int currentUserId);
    Task<RelatedItemsModel> GetRelatedItemsAsync(int id);
    Task<bool> DeleteOpportunityAsync(int id);
    Task<SimilarOpportunitiesResponse> GetSimilarOpportunitiesAsync(int id, int maxResults = 6, ClaimsPrincipal? user = null);
    Task AssignCreatorAsOpportunityManagerAsync(int opportunityId, int userId);
    Task<IEnumerable<OpportunityModel>> GetOpportunitiesByPartnerIdAsync(int partnerId);
    List<SearchFieldInfo> GetOpportunitySearchFields();
    
    /// <summary>
    /// Updates the high risk acknowledgement status for an opportunity
    /// AC1: User must acknowledge they've reviewed all applicable organizational high risks
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="acknowledged">Whether the high risks have been acknowledged</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateHighRiskAcknowledgementAsync(int opportunityId, bool acknowledged);

    /// <summary>
    /// Assigns an Executive to an opportunity during Go decision approval.
    /// The Executive is typically the Director/Manager/OiC of the responsible org unit.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="executiveId">The user ID of the assigned Executive</param>
    Task AssignExecutiveAsync(int opportunityId, int executiveId);

    /// <summary>
    /// Gets personnel for an opportunity's responsible org unit.
    /// Used to populate the Executive dropdown in the Go Decision approval dialog.
    /// Returns all personnel with roles on the org unit, with Directors/Deputy Directors marked as "Suggested".
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <returns>List of personnel with display label and user ID</returns>
    Task<IEnumerable<TypeaheadInput>> GetExecutivesForOpportunityAsync(int opportunityId);
}


