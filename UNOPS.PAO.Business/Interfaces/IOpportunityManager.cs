using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using System.Security.Claims;

namespace UNOPS.PAO.Business.Interfaces;

public interface IOpportunityManager
{
    Task<OpportunityModel> CreateOpportunityAsync(OpportunityRequest model);
    Task<OpportunityModel?> GetOpportunityAsync(int id);
    Task<IEnumerable<OpportunityModel>> GetAllOpportunitiesAsync();
    Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model);
    Task<OpportunityModel> UpdateWhatSectionAsync(int id, WhatSectionRequest request);
    Task<OpportunityModel> UpdateWhySectionAsync(int id, WhySectionRequest request);
    Task<OpportunityModel> UpdateWhoSectionAsync(int id, WhoSectionRequest request);
    Task<OpportunityModel> UpdateWhereSectionAsync(int id, WhereSectionRequest request);
    Task<OpportunityModel> UpdateWhenSectionAsync(int id, WhenSectionRequest request);
    Task<OpportunityModel> ApplyAiChangesAsync(int id, ApplyOpportunityAiChangesRequest request);
    Task<RelatedItemsModel> GetRelatedItemsAsync(int id);
    Task<bool> DeleteOpportunityAsync(int id);
    Task<SimilarOpportunitiesResponse> GetSimilarOpportunitiesAsync(int id, int maxResults = 6, ClaimsPrincipal? user = null);
    Task AssignCreatorAsOpportunityManagerAsync(int opportunityId, int userId);
}

