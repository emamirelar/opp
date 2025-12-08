using UNOPS.PAO.Models;
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
}


